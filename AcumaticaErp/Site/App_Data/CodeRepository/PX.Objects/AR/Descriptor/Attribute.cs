using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.TX;
using PX.Objects.CM;
using PX.Objects.BQLConstants;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.CR;
using ARCashSale = PX.Objects.AR.Standalone.ARCashSale;
using PX.Objects.EP;
using System.Collections;

namespace PX.Objects.AR
{
	public class ARCommissionPeriodIDAttribute : PeriodIDAttribute, IPXFieldVerifyingSubscriber 
	{
		protected int _SelAttrIndex = -1;

		public ARCommissionPeriodIDAttribute()
			:base(null, typeof(Search<ARSPCommissionPeriod.commnPeriodID>))
		{
			_Attributes.Add(new PXSelectorAttribute(typeof(Search<ARSPCommissionPeriod.commnPeriodID>)));
			_SelAttrIndex = _Attributes.Count - 1;
		}

		#region Initialization
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber))
			{
				subscribers.Add(this as ISubscriber);
			}
			else
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			try
			{
				if (_SelAttrIndex != -1)
					((IPXFieldVerifyingSubscriber)_Attributes[_SelAttrIndex]).FieldVerifying(sender, e);
			}
			catch (PXSetPropertyException)
			{
				e.NewValue = FormatPeriod((string)e.NewValue);
				throw;
			}
		}

		public static string FormatPeriod(string period)
		{
			return FinPeriodIDFormattingAttribute.FormatForDisplay(period);
		}

		public static string UnFormatPeriod(string period)
		{
			return FinPeriodIDFormattingAttribute.FormatForStoring(period);
		}
		#endregion
	}

    /// <summary>
    /// Specialized for ARInvoice version of the InvoiceNbrAttribute.<br/>
    /// The main purpose of the attribute is poviding of the uniqueness of the RefNbr <br/>
    /// amoung  a set of  documents of the specifyed types (for example, each RefNbr of the ARInvoice <br/>
    /// the ARInvoices must be unique across all ARInvoices and AR Debit memos)<br/>
    /// This may be useful, if user has configured a manual numberin for ARInvoices  <br/>
    /// or needs  to create ARInvoice from another document (like SOOrder) allowing to type RefNbr <br/>
    /// for the to-be-created Invoice manually. To store the numbers, system using ARInvoiceNbr table, <br/>
    /// keyed uniquelly by DocType and RefNbr. A source document is linked to a number by NoteID.<br/>
    /// Attributes checks a number for uniqueness on FieldVerifying and RowPersisting events.<br/>
    /// </summary>
	public class ARInvoiceNbrAttribute : InvoiceNbrAttribute
	{
		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		protected override long? GetNoteID(PXCache sender, PXRowPersistedEventArgs e)
		{
			long? val = (long?)sender.GetValue<ARInvoice.refNoteID>(e.Row);
			return (val != null) ? val : base.GetNoteID(sender, e);
		}

		protected override bool DeleteOnUpdate(PXCache sender, PXRowPersistedEventArgs e)
		{
			long? val = (long?)sender.GetValue<ARInvoice.refNoteID>(e.Row);

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete && val != null)
			{
				return false;
			}
			else
			{
				return base.DeleteOnUpdate(sender, e);
			}
		}

		public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			try
			{
				base.RowPersisted(sender, e);
			}
			catch (PXRowPersistedException)
			{
				if (e.Operation != PXDBOperation.Update || e.TranStatus != PXTranStatus.Open)
				{
					throw;
				}
			}
		}

		public ARInvoiceNbrAttribute()
			: base(typeof(ARInvoice.docType), typeof(ARInvoice.noteID))
		{
		}
	}

	public class InvoiceNbrAttribute : PXEventSubscriberAttribute, IPXRowInsertedSubscriber, IPXRowUpdatedSubscriber, IPXRowPersistedSubscriber, IPXFieldVerifyingSubscriber
	{
		protected Type _DocType;
		protected Type _NoteID;
		protected string[] _DocTypes;

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PXNoteAttribute.GetNoteID(sender, e.Row, _NoteID.Name);
			sender.Graph.Caches[typeof(Note)].IsDirty = false;
		}

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PXNoteAttribute.GetNoteID(sender, e.Row, _NoteID.Name);
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			long? RefNoteID = (long?)sender.GetValue(e.Row, _NoteID.Name);
			string DocType = GetDocType(sender, e.Row);

			if (string.IsNullOrEmpty((string)e.NewValue) == false)
			{
				ARInvoiceNbr dup = PXSelectReadonly<ARInvoiceNbr, Where<ARInvoiceNbr.docType, Equal<Required<ARInvoiceNbr.docType>>, And<ARInvoiceNbr.refNbr, Equal<Required<ARInvoiceNbr.refNbr>>, And<ARInvoiceNbr.refNoteID, NotEqual<Required<ARInvoiceNbr.refNoteID>>>>>>.Select(sender.Graph, DocType, e.NewValue, RefNoteID);
				if (dup != null)
				{
					throw new PXSetPropertyException(Messages.DuplicateInvoiceNbr);
				}
			}
		}

		protected virtual bool DeleteNumber(string DocType, string RefNbr, long? RefNoteID)
		{
			return PXDatabase.Delete<ARInvoiceNbr>(
				new PXDataFieldRestrict("DocType", DocType),
				new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, RefNbr, PXComp.NE),
				new PXDataFieldRestrict("RefNoteID", RefNoteID));
		}

		protected virtual bool SelectNumber(string DocType, string RefNbr, long? RefNoteID)
		{
			using (PXDataRecord record = PXDatabase.SelectSingle<ARInvoiceNbr>(
				new PXDataField("RefNoteID"),
				new PXDataFieldValue("DocType", DocType),
				new PXDataFieldValue("RefNbr", RefNbr),
				new PXDataFieldValue("RefNoteID", RefNoteID)))
			{
				return record != null;
			}
		}

		protected virtual void InsertNumber(PXCache sender, string DocType, string RefNbr, long? RefNoteID)
		{
			ARInvoiceNbr record = new ARInvoiceNbr();
			PXCache cache = sender.Graph.Caches[typeof(ARInvoiceNbr)];

			List<PXDataFieldAssign> fields = new List<PXDataFieldAssign>();

			foreach (string field in cache.Fields)
			{
				object NewValue = null;
				switch (field)
				{
					case "DocType":
						NewValue = DocType;
						break;
					case "RefNbr":
						NewValue = RefNbr;
						break;
					case "RefNoteID":
						NewValue = RefNoteID;
						break;
					case "tstamp":
						continue;
					default:
						cache.RaiseFieldDefaulting(field, record, out NewValue);
						if (NewValue == null)
						{
							cache.RaiseRowInserting(record);
							NewValue = cache.GetValue(record, field);
						}
						break;
				}
				fields.Add(new PXDataFieldAssign(field, NewValue));
			}
			PXDatabase.Insert<ARInvoiceNbr>(fields.ToArray());
		}

		protected virtual bool DeleteOnUpdate(PXCache sender, PXRowPersistedEventArgs e)
		{
			return (e.Operation & PXDBOperation.Command) == PXDBOperation.Delete;
		}

		protected virtual long? GetNoteID(PXCache sender, PXRowPersistedEventArgs e)
		{
			return (long?)sender.GetValue(e.Row, _NoteID.Name);
		}

		protected virtual string GetNumber(PXCache sender, PXRowPersistedEventArgs e)
		{
			return (string)sender.GetValue(e.Row, _FieldName);
		}

		protected virtual string GetDocType(PXCache sender, object data)
		{
			string DocType = (string)sender.GetValue(data, _DocType.Name);

			switch (DocType)
			{
				case ARDocType.Invoice:
				case ARDocType.CashSale:
					return ARDocType.Invoice;
				case ARDocType.CreditMemo:
				case ARDocType.DebitMemo:
					return DocType;
				default:
					return null;
			}
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Open)
			{
				long? RefNoteID = GetNoteID(sender, e);
				string DocType = GetDocType(sender, e.Row);
				string SetNumber = GetNumber(sender, e);
				bool Deleted = true;

				if (string.IsNullOrEmpty(DocType))
				{
					return;
				}

				if (RefNoteID < 0L)
				{
					throw new PXRowPersistedException(_FieldName, SetNumber, Messages.CannotSaveNotes);
				}


				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update || (e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
				{
					if (DeleteOnUpdate(sender, e))
					{
						DeleteNumber(DocType, string.Empty, RefNoteID);
						SetNumber = string.Empty;
					}
					else
					{
						Deleted = DeleteNumber(DocType, SetNumber ?? string.Empty, RefNoteID);
					}
				}
				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update || (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
				{
					if (string.IsNullOrEmpty((string)SetNumber) == false && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update && Deleted || SelectNumber(DocType, SetNumber, RefNoteID) == false))
					{
						try
						{
							InsertNumber(sender, DocType, SetNumber, RefNoteID);
						}
						catch (PXDatabaseException)
						{
							throw new PXRowPersistedException(_FieldName, SetNumber, Messages.DuplicateInvoiceNbr);
						}
					}
				}
			}
		}

		public InvoiceNbrAttribute(Type DocType, Type NoteID)
		{
			_DocType = DocType;
			_NoteID = NoteID;
			_DocTypes = new ARDocType.SOEntryListAttribute().AllowedValues;
		}
	}

    /// <summary>
    /// Specialized for AR version of the Address attribute.<br/>
    /// Uses ARAddress tables for Address versions storage <br/>
    /// Prameters AddressID, IsDefault address are assigned to the <br/>
    /// corresponded fields in the POAddress table. <br/>
    /// Cache for ARAddress must be present in the graph <br/>    
    /// Depends upon row instance. <br/>
    /// <example>
    ///[ARAddress(typeof(Select2<Customer,
    ///        InnerJoin<Location, On<Location.bAccountID, Equal<Customer.bAccountID>, 
    ///         And<Location.locationID, Equal<Customer.defLocationID>>>,
    ///        InnerJoin<Address, On<Address.bAccountID, Equal<Customer.bAccountID>, 
    ///         And<Address.addressID, Equal<Customer.defBillAddressID>>>,
    ///        LeftJoin<ARAddress, On<ARAddress.customerID, Equal<Address.bAccountID>, 
    ///         And<ARAddress.customerAddressID, Equal<Address.addressID>, 
    ///         And<ARAddress.revisionID, Equal<Address.revisionID>, 
    ///         And<ARAddress.isDefaultBillAddress, Equal<boolTrue>>>>>>>>,
    ///        Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>>>))]
    /// </example>
    /// </summary>
	public class ARAddressAttribute : AddressAttribute
	{
        /// <summary>
        /// Internaly, it expects ARAddress as a IAddress type. 
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/> 
        /// a source Address record from which AR address is defaulted and for selecting default version of POAddress, <br/>
        /// created  from source Address (having  matching ContactID, revision and IsDefaultContact = true) <br/>
        /// if it exists - so it must include both records. See example above. <br/>
        /// </param>
        public ARAddressAttribute(Type SelectType)
			: base(typeof(ARAddress.addressID), typeof(ARAddress.isDefaultBillAddress), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<ARAddress.overrideAddress>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<ARAddress, ARAddress.addressID>(sender, DocumentRow, Row);
		}

		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<ARAddress, ARAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}

		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<ARAddress>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<ARAddress.overrideAddress>(sender, e.Row, sender.AllowUpdate);
				PXUIFieldAttribute.SetEnabled<ARAddress.isValidated>(sender, e.Row, false);
			}
		}
	}

    /// <summary>
    /// Specialized for AR version of the Contact attribute.<br/>
    /// Uses ARContact tables for Contact versions storage <br/>
    /// Parameters ContactID, IsDefaultContact are assigned to the <br/>
    /// corresponded fields in the ARContact table. <br/>
    /// Cache for ARContact must be present in the graph <br/>
    /// Depends upon row instance.    
    ///<example>
    ///[ARContact(typeof(Select2<Customer,
	///		InnerJoin<Location, On<Location.bAccountID, Equal<Customer.bAccountID>, 
    ///		    And<Location.locationID, Equal<Customer.defLocationID>>>,
	///		InnerJoin<Contact, On<Contact.bAccountID, Equal<Customer.bAccountID>, 
    ///		    And<Contact.contactID, Equal<Customer.defBillContactID>>>,
	///		LeftJoin<ARContact, On<ARContact.customerID, Equal<Contact.bAccountID>, 
    ///		    And<ARContact.customerContactID, Equal<Contact.contactID>, 
    ///		    And<ARContact.revisionID, Equal<Contact.revisionID>, 
    ///		    And<ARContact.isDefaultContact, Equal<boolTrue>>>>>>>>,
	///		Where<Customer.bAccountID, Equal<Current<ARCashSale.customerID>>>>))]
    ///</example>    
    ///</summary>			
	public class ARContactAttribute : ContactAttribute
	{
        /// <summary>
        /// Ctor. Internaly, it expects ARContact as a IContact type
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/> 
        /// a source Contact record from which AR Contact is defaulted and for selecting version of ARContact, <br/>
        /// created from source Contact (having  matching ContactID, revision and IsDefaultContact = true).<br/>
        /// - so it must include both records. See example above. <br/>
        /// </param>				
		public ARContactAttribute(Type SelectType)
			: base(typeof(ARContact.contactID), typeof(ARContact.isDefaultContact), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<ARContact.overrideContact>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<ARContact, ARContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<ARContact, ARContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<ARContact>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<ARContact.overrideContact>(sender, e.Row, sender.AllowUpdate);
			}
		}
	}

	/// <summary>
	/// FinPeriod selector that extends <see cref="AnyPeriodFilterableAttribute"/>. 
	/// Displays any periods (active, closed, etc), maybe filtered. 
	/// When Date is supplied through aSourceType parameter FinPeriod is defaulted with the FinPeriod for the given date.
	/// Default columns list includes 'Active' and  'Closed in GL' and 'Closed in AP'  columns
	/// </summary>
	public class ARAnyPeriodFilterableAttribute : AnyPeriodFilterableAttribute
	{

		public ARAnyPeriodFilterableAttribute(Type aSearchType, Type aSourceType)
			: base(aSearchType, aSourceType, typeof(FinPeriod.finPeriodID), typeof(FinPeriod.active), typeof(FinPeriod.closed), typeof(FinPeriod.aRClosed))
		{

		}

		public ARAnyPeriodFilterableAttribute(Type aSourceType)
			: this(null, aSourceType)
		{

		}

		public ARAnyPeriodFilterableAttribute()
			: this(null)
		{

		}
	}

	public class ARAcctSubDefault
	{
		public class CustomListAttribute : PXStringListAttribute
		{
			public string[] AllowedValues
			{
				get
				{
					return _AllowedValues;
				}
			}

			public string[] AllowedLabels
			{
				get
				{
					return _AllowedLabels;
				}
			}

			public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels)
				: base(AllowedValues, AllowedLabels)
			{
			}
		}
        /// <summary>
        /// Defines a list of the possible sources for the AR Documents sub-account defaulting: <br/>
        /// Namely: MaskLocation, MaskItem, MaskEmployee, MaskCompany, MaskSalesPerson <br/>
        /// Mostly, this attribute serves as a container <br/>
        /// </summary>
		public class ClassListAttribute : CustomListAttribute
		{
			public ClassListAttribute()
				: base(new string[] { MaskLocation, MaskItem, MaskEmployee, MaskCompany, MaskSalesPerson }, new string[] { Messages.MaskLocation, Messages.MaskItem, Messages.MaskEmployee, Messages.MaskCompany, Messages.MaskSalesPerson })
			{
			}
		}
		public const string MaskLocation = "L";
		public const string MaskItem = "I";
		public const string MaskEmployee = "E";
		public const string MaskCompany = "C";
		public const string MaskSalesPerson = "S";
	}

	[PXDBString(30, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = SubAccountAttribute.DimensionName)]
	public sealed class SubAccountMaskAttribute : AcctSubAttribute
	{
		private string _DimensionName = "SUBACCOUNT";
		private string _MaskName = "ARSETUP";
		public SubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, ARAcctSubDefault.MaskLocation, new ARAcctSubDefault.ClassListAttribute().AllowedValues, new ARAcctSubDefault.ClassListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new ARAcctSubDefault.ClassListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new ARAcctSubDefault.ClassListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

    /// <summary>
    /// Provides a UI Selector for Customers AcctCD as a string. <br/>
    /// Should be used where the definition of the AccountCD is needed - mostly, in a Customer DAC class.<br/>
    /// Properties of the selector - mask, length of the key, etc.<br/>
    /// are defined in the Dimension with predefined name "CUSTOMER".<br/>
    /// By default, search uses the following tables (linked) BAccount, Customer (strict), Contact, Address (optional).<br/> 
    /// List of the Customers is filtered based on the user's access rights.<br/>
    /// Default column's list in the Selector - Customer.acctCD, Customer.acctName,<br/>
    ///	Customer.customerClassID, Customer.status, Contact.phone1, Address.city, Address.countryID, Contact.EMail<br/>    
    ///	List of properties - inherited from AcctSubAttribute <br/>
    ///	<example> 
    /// [CustomerRaw(IsKey = true)]
    ///	</example>    
    /// </summary>
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.Visible)]
	public sealed class CustomerRawAttribute : AcctSubAttribute
	{
		public const string DimensionName = "CUSTOMER";
		public CustomerRawAttribute()
			: base()
		{

			Type SearchType = typeof(Search2<Customer.acctCD,
				LeftJoin<Contact, On<Contact.bAccountID, Equal<Customer.bAccountID>, And<Contact.contactID, Equal<Customer.defContactID>>>,
				LeftJoin<Address, On<Address.bAccountID, Equal<Customer.bAccountID>, And<Address.addressID, Equal<Customer.defAddressID>>>>>,
				Where<Match<Current<AccessInfo.userName>>>>);
			_Attributes.Add(new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(Customer.acctCD),
					typeof(Customer.acctCD), typeof(Customer.acctName), typeof(Customer.customerClassID), typeof(Customer.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID), typeof(Contact.eMail)
				));
			_SelAttrIndex = _Attributes.Count - 1;

			((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).CacheGlobal = true;
			this.Filterable = true;
		}
	}


    /// <summary>
    /// This is a specialized version of the Customer attribute.<br/>
    /// Displays only Active or OneTime customers<br/>
    /// It also provides CreditCheck functionality, preventing <br/>
    /// the unreleased documents from been taken from 'Hold' state if the Customer <br/>
    /// do not pass credit verification criteria and displays the result of the verification <br/>
    /// as en error/waring on the Hold field <br/>
    /// Graph must contain a APSetup cache. <br/>
    /// See CustomerAttribute for detailed description. <br/>
    /// <example>
    /// [CustomerCredit(typeof(ARInvoice.hold), typeof(ARInvoice.released), Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Filterable = true, TabOrder = 2)]
    /// </example>
    /// </summary>	
	[PXDBInt()]
	[PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.Visible)]
	public class CustomerCreditAttribute : CustomerActiveAttribute, IPXRowSelectedSubscriber, IPXRowPersistingSubscriber, IPXRowUpdatedSubscriber 
	{
		#region State
		protected string _HoldFieldName = null;
		protected string _ReleasedFieldName = null;
		protected bool _InternalCall = false;
		#endregion

		#region Ctor
        /// <summary>
        /// Default Ctor. 
        /// </summary>
        public CustomerCreditAttribute()
			:this(null, null)
        {
        }

        /// Extended Ctor.
		public CustomerCreditAttribute(Type HoldType, Type ReleasedType)
			:base()
		{
			_HoldFieldName = HoldType.Name;
			_ReleasedFieldName = ReleasedType.Name;
		}

		public CustomerCreditAttribute(Type search, Type HoldType, Type ReleasedType)
			:base(search)
		{
			_HoldFieldName = HoldType.Name;
			_ReleasedFieldName = ReleasedType.Name;
		}
		#endregion

		#region Implementation

	    
		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (sender.Graph.GetType() != typeof(PXGraph) && sender.Graph.GetType() != typeof(PXGenericInqGrph))
			{
				Type itemType = sender.GetItemType();
				//will be called after graph event
				PXRowUpdated del = null;
				del = delegate(PXCache cache, PXRowUpdatedEventArgs args)
					{
						Document_RowUpdated(cache, args);
						cache.Graph.RowUpdated.RemoveHandler(itemType, del);
					}; 

				
				sender.Graph.RowUpdated.AddHandler(itemType, del);
				//sender.Graph.RowUpdated.AddHandler(itemType, (cache, args) =>
				//{
				//	cache.Graph.RowUpdated.RemoveHandler(itemType, del);
				//});
			}
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			Verify(sender, e.Row, e);
		}

		public virtual void Document_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (_InternalCall)
			{
				return;
			}
			try
			{
				_InternalCall = true;

				if ((int?)sender.GetValue(e.Row, _FieldName) != (int?)sender.GetValue(e.OldRow, _FieldName))
				{
					sender.RaiseExceptionHandling(_FieldName, e.Row, null, null);
					sender.RaiseExceptionHandling(_HoldFieldName, e.Row, null, null);
				}

				Verify(sender, e.Row, e);
			}
			finally
			{
				_InternalCall = false;
			}

		}

		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Verify(sender, e.Row, e);
		}

		protected virtual decimal? GetDocumentBalance(PXCache cache, object Row)
		{
			ARBalances accumbal = cache.Current as ARBalances;
			if (accumbal != null && cache.GetStatus(accumbal) == PXEntryStatus.Inserted)
			{
				//get balance only from PXAccumulator
				return accumbal.CurrentBal + accumbal.UnreleasedBal + accumbal.TotalOpenOrders;
			}
			return 0m;
		}

		protected virtual void GetCustomerBalance(PXCache cache, object Row, out decimal? CustomerBal, out DateTime? OldInvoiceDate)
		{
			ARBalances summarybal = PXSelectGroupBy<ARBalances,
													Where<ARBalances.customerID, Equal<Current<Customer.bAccountID>>>,
														Aggregate<
														Sum<ARBalances.currentBal,
														Sum<ARBalances.totalOpenOrders,
														Sum<ARBalances.totalPrepayments,
														Sum<ARBalances.totalShipped,
														Sum<ARBalances.unreleasedBal,
														Min<ARBalances.oldInvoiceDate>>>>>>>>.Select(cache.Graph);
			/*
			ARTempCreditLimit templimit = PXSelect<ARTempCreditLimit,
											Where<ARTempCreditLimit.customerID, Equal<Required<ARTempCreditLimit.CustomerID>>,
												And<ARTempCreditLimit.startDate, LessEqual<Current<ARTempCreditLimit.startDate>>,
												And<ARTempCreditLimit.endDate, Greater<Current<ARTempCreditLimit.endDate>>>>>>.
												Select(sender.Graph, doc.CustomerID, doc.DocDate, doc.DocDate);
			*/

			CustomerBal = 0m;
			ARBalances accumbal = cache.Current as ARBalances;
			if (accumbal != null && cache.GetStatus(accumbal) == PXEntryStatus.Inserted)
			{
				//get balance only from PXAccumulator
				CustomerBal = accumbal.CurrentBal + accumbal.UnreleasedBal + accumbal.TotalOpenOrders;
			}

			OldInvoiceDate = null;
			if (summarybal != null)
			{
				CustomerBal += (summarybal.CurrentBal ?? 0m);
				CustomerBal += (summarybal.UnreleasedBal ?? 0m);
				CustomerBal += (summarybal.TotalOpenOrders ?? 0m);
				CustomerBal += (summarybal.TotalShipped ?? 0m);
				CustomerBal -= (summarybal.TotalPrepayments ?? 0m);

				OldInvoiceDate = summarybal.OldInvoiceDate;
			}
		}

		protected virtual bool? GetHoldValue(PXCache sender, object Row)
		{
			return (bool?)sender.GetValue(Row, _HoldFieldName);
		}

		protected virtual bool? GetReleasedValue(PXCache sender, object Row)
		{
			return (bool?)sender.GetValue(Row, _ReleasedFieldName);
		}

		protected virtual bool? GetCreditCheckError(PXCache sender)
		{
			PXCache arsetupcache = sender.Graph.Caches[typeof(ARSetup)];
			return ((ARSetup)arsetupcache.Current).CreditCheckError;
		}

		protected virtual void PlaceOnHold(PXCache sender, object Row, bool OnAdminHold)
		{
			sender.RaiseExceptionHandling(_HoldFieldName, Row, true, new PXSetPropertyException(Messages.CreditHoldEntry, PXErrorLevel.Warning));
		}

		public virtual void Verify(PXCache sender, object Row, EventArgs e)
		{
			PXCache customercache = sender.Graph.Caches[typeof(Customer)];
			PXCache customerclasscache = sender.Graph.Caches[typeof(CustomerClass)];
			PXCache arbalancescache = sender.Graph.Caches[typeof(ARBalances)];

			int? CustomerID = (int?)sender.GetValue(Row, _FieldOrdinal);
			bool? HoldValue = GetHoldValue(sender, Row);
			bool? ReleasedValue = GetReleasedValue(sender, Row);
			string errmsg = null;

		
			if ((bool)(ReleasedValue ?? false))
			{
				HoldValue = true;
			}

			if (customercache.Current != null && object.Equals(((Customer)customercache.Current).BAccountID, CustomerID) == false)
			{
				customercache.Current = null;
			}
			
			if (customercache.Current != null && ((Customer)customercache.Current).CreditRule != "N")
			{
				if (arbalancescache.Current != null && ((Customer)customercache.Current).BAccountID != ((ARBalances)arbalancescache.Current).CustomerID)
				{
					arbalancescache.Current = null;
				}

				if (customerclasscache.Current != null && object.Equals(((CustomerClass)customerclasscache.Current).CustomerClassID, ((Customer)customercache.Current).CustomerClassID) == false)
				{
					customerclasscache.Current = null;
				}

				decimal? DocumentBal = GetDocumentBalance(arbalancescache, Row);

				{
					decimal? CustomerBal;
					DateTime? OldInvoiceDate;
					GetCustomerBalance(arbalancescache, Row, out CustomerBal, out OldInvoiceDate);

					TimeSpan overdue = (DateTime)sender.Graph.Accessinfo.BusinessDate - (DateTime)(OldInvoiceDate ?? sender.Graph.Accessinfo.BusinessDate);

					bool failed = (ReleasedValue ?? false);
					bool enforce = false;

					//On graph load Current for setups can be null
					if (customercache.Current == null || customerclasscache.Current == null) return;
					if (failed == false && (((Customer)customercache.Current).CreditRule == "B" || ((Customer)customercache.Current).CreditRule == "D"))
					{
						if (overdue.Days > ((Customer)customercache.Current).CreditDaysPastDue)
						{
							errmsg = Messages.CreditDaysPastDueWereExceeded;
						}

						if (DocumentBal > 0 && overdue.Days > ((Customer)customercache.Current).CreditDaysPastDue)
						{
							failed = true;
						}
					}

					if (failed == false && (((Customer)customercache.Current).CreditRule == "B" || ((Customer)customercache.Current).CreditRule == "C"))
					{
						if (CustomerBal >= ((Customer)customercache.Current).CreditLimit)
						{
							errmsg = Messages.CreditLimitWasExceeded;
						}

						if (DocumentBal > 0 && CustomerBal >= ((Customer)customercache.Current).CreditLimit + ((CustomerClass)customerclasscache.Current).OverLimitAmount)
						{
							failed = true;
						}
					}

					if (failed == false && (((Customer)customercache.Current).Status == Customer.status.CreditHold))
					{
						errmsg = Messages.CustomerIsOnCreditHold;

						if (DocumentBal > 0m)
						{
							enforce = true;
							failed = true;
						}
					}

					if (failed == false && (((Customer)customercache.Current).Status == Customer.status.Hold))
					{
						errmsg = Messages.CustomerIsOnHold;
						failed = true;
						enforce = true;
					}

					if (failed == false && (((Customer)customercache.Current).Status == Customer.status.Inactive))
					{
						errmsg = Messages.CustomerIsInactive;
						failed = true;
						enforce = true;
					}

					if (!string.IsNullOrEmpty(errmsg))
					{
						sender.RaiseExceptionHandling(_FieldName, Row, ((Customer)customercache.Current).AcctCD, new PXSetPropertyException(errmsg, PXErrorLevel.Warning));
					}						

					if (failed && HoldValue == false)
					{
						if (e is PXRowUpdatedEventArgs && (enforce || GetCreditCheckError(sender) == true))
						{
							object OldRow = sender.CreateCopy(Row);
							sender.SetValueExt(Row, _HoldFieldName, true);
							sender.RaiseRowUpdated(Row, OldRow);

							DocumentBal = GetDocumentBalance(arbalancescache, Row);

							//this is a Credit Memo
							if (DocumentBal > 0m)
							{
								OldRow = sender.CreateCopy(Row);
								sender.SetValueExt(Row, _HoldFieldName, false);
								sender.RaiseRowUpdated(Row, OldRow);
							}
							else
							{
								PlaceOnHold(sender, Row, enforce);
							}
						}
						else if (e is PXRowPersistingEventArgs && (enforce || GetCreditCheckError(sender) == true))
						{
							if (string.IsNullOrEmpty(errmsg) == false)
							{
								object OldRow = sender.CreateCopy(Row);
								sender.SetValueExt(Row, _HoldFieldName, true);
								sender.RaiseRowUpdated(Row, OldRow);

								DocumentBal = GetDocumentBalance(arbalancescache, Row);

								OldRow = sender.CreateCopy(Row);
								sender.SetValueExt(Row, _HoldFieldName, false);
								sender.RaiseRowUpdated(Row, OldRow);

								//this is not a Credit Memo
								if (DocumentBal <= 0m)
								{
									throw new PXException(errmsg);
								}
							}
						}
					}
				}
			}
		}
		#endregion
	}

    /// <summary>
    /// This is a specialized version of the Customer attribute.<br/>
    /// Displays only Active or OneTime customers<br/>
    /// See CustomerAttribute for detailed description. <br/>
    /// </summary>	
	[PXDBInt()]
	[PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<Customer.status, Equal<BAccount.status.active>, 
					Or<Customer.status, Equal<BAccount.status.oneTime>>>), Messages.CustomerIsInStatus, typeof(Customer.status))]
	public class CustomerActiveAttribute : CustomerAttribute
	{
		public CustomerActiveAttribute(Type search)
			:base(search)
		{
		}

		public CustomerActiveAttribute()
			: base ()
		{
		}
	}


    /// <summary>
    /// Provides a UI Selector for Customers.<br/>
    /// Properties of the selector - mask, length of the key, etc.<br/>
    /// are defined in the Dimension with predefined name "BIZACCT".<br/>
    /// By default, search uses the following tables (linked) BAccount, Customer (strict), Contact, Address (optional).<br/> 
    /// List of the Vendors is filtered based on the user's access rights.<br/>
    /// Default column's list in the Selector - Customer.acctCD, Customer.acctName,<br/>
    ///	Customer.customerClassID, Customer.status, Contact.phone1, Address.city, Address.countryID<br/>
    ///	As most Dimention Selector - substitutes BAccountID by AcctCD.<br/>
    ///	List of properties - inherited from AcctSubAttribute <br/>
    ///	<example> 
    ///[Customer(typeof(Search<BAccountR.bAccountID, 
    /// Where<Customer.smallBalanceAllow, Equal<boolTrue>, 
    /// And<Where<Customer.status, Equal<BAccount.status.active>, 
    /// Or<Customer.status, Equal<BAccount.status.oneTime>>>>>>),
    /// Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Customer.acctName))]
    ///	</example>
    /// </summary>	
	[PXDBInt()]
	[PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.Visible)]
    [Serializable]
	public class CustomerAttribute : AcctSubAttribute
	{
		#region Override DACs

		[Serializable]
		public partial class Location : IBqlTable
		{
			#region BAccountID
			public abstract class bAccountID : PX.Data.IBqlField
			{
			}
			protected Int32? _BAccountID;
			[PXDBInt(IsKey = true)]
			public virtual Int32? BAccountID
			{
				get
				{
					return this._BAccountID;
				}
				set
				{
					this._BAccountID = value;
				}
			}
			#endregion
			#region LocationID
			public abstract class locationID : PX.Data.IBqlField
			{
			}
			protected Int32? _LocationID;
			[PXDBIdentity()]
			public virtual Int32? LocationID
			{
				get
				{
					return this._LocationID;
				}
				set
				{
					this._LocationID = value;
				}
			}
			#endregion
			#region LocationCD
			public abstract class locationCD : PX.Data.IBqlField
			{
			}
			protected String _LocationCD;
			[PXDBString(IsKey = true, IsUnicode = true)]
			public virtual String LocationCD
			{
				get
				{
					return this._LocationCD;
				}
				set
				{
					this._LocationCD = value;
				}
			}
			#endregion
			#region TaxRegistrationID
			public abstract class taxRegistrationID : PX.Data.IBqlField
			{
			}
			protected String _TaxRegistrationID;
			[PXDBString(50, IsUnicode = true)]
			[PXUIField(DisplayName = "Tax Registration ID")]
			public virtual String TaxRegistrationID
			{
				get
				{
					return this._TaxRegistrationID;
				}
				set
				{
					this._TaxRegistrationID = value;
				}
			}
			#endregion
            #region VLeadTime
            public abstract class vLeadTime : PX.Data.IBqlField
            {
            }
            protected Int16? _VLeadTime;
            [PXDBShort(MinValue = 0, MaxValue = 100000)]
            [PXUIField(DisplayName = "Lead Time (days)")]
            public virtual Int16? VLeadTime
            {
                get
                {
                    return this._VLeadTime;
                }
                set
                {
                    this._VLeadTime = value;
                }
            }
            #endregion
            #region VCarrierID
            public abstract class vCarrierID : PX.Data.IBqlField
            {
            }
            protected String _VCarrierID;
            [PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
            [PXUIField(DisplayName = "Ship Via")]
            public virtual String VCarrierID
            {
                get
                {
                    return this._VCarrierID;
                }
                set
                {
                    this._VCarrierID = value;
                }
            }
            #endregion
		}

        [Serializable]
		public class Contact : IBqlTable
		{
			#region BAccountID
			public abstract class bAccountID : PX.Data.IBqlField { }

			[PXDBInt]
			[CRContactBAccountDefault]
			public virtual Int32? BAccountID { get; set; }
			#endregion

			#region ContactID
			public abstract class contactID : IBqlField { }

			[PXDBIdentity(IsKey = true)]
			[PXUIField(DisplayName = "Contact ID", Visibility = PXUIVisibility.Invisible)]
			public virtual Int32? ContactID { get; set; }
			#endregion

			#region RevisionID
			public abstract class revisionID : PX.Data.IBqlField { }

			[PXDBInt]
			[PXDefault(0)]
			[AddressRevisionID()]
			public virtual Int32? RevisionID { get; set; }
			#endregion

			#region Title
			public abstract class title : PX.Data.IBqlField { }

			[PXDBString(50, IsUnicode = true)]
			[Titles]
			[PXUIField(DisplayName = "Title")]
			public virtual String Title { get; set; }
			#endregion

			#region FirstName
			public abstract class firstName : PX.Data.IBqlField { }

			[PXDBString(50, IsUnicode = true)]
			[PXUIField(DisplayName = "First Name")]
			public virtual String FirstName { get; set; }
			#endregion

			#region MidName
			public abstract class midName : PX.Data.IBqlField { }

			[PXDBString(50, IsUnicode = true)]
			[PXUIField(DisplayName = "Middle Name")]
			public virtual String MidName { get; set; }
			#endregion

			#region LastName
			public abstract class lastName : PX.Data.IBqlField { }

			[PXDBString(100, IsUnicode = true)]
			[PXUIField(DisplayName = "Last Name")]
			[CRLastNameDefault]
			public virtual String LastName { get; set; }
			#endregion

			#region Salutation

			public abstract class salutation : PX.Data.IBqlField { }

			[PXDBString(255, IsUnicode = true)]
			[PXUIField(DisplayName = "Attention", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String Salutation { get; set; }
			#endregion

			#region Phone1
			public abstract class phone1 : PX.Data.IBqlField { }

			[PXDBString(50)]
			[PXUIField(DisplayName = "Phone 1", Visibility = PXUIVisibility.SelectorVisible)]
			[PhoneValidation()]
			public virtual String Phone1 { get; set; }
			#endregion

			#region Phone1Type
			public abstract class phone1Type : PX.Data.IBqlField { }

			[PXDBString(3)]
			[PXDefault(PhoneTypesAttribute.Business1, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Phone 1")]
			[PhoneTypes]
			public virtual String Phone1Type { get; set; }
			#endregion

			#region Phone2
			public abstract class phone2 : PX.Data.IBqlField { }

			[PXDBString(50)]
			[PXUIField(DisplayName = "Phone 2")]
			[PhoneValidation()]
			public virtual String Phone2 { get; set; }
			#endregion

			#region Phone2Type
			public abstract class phone2Type : PX.Data.IBqlField { }

			[PXDBString(3)]
			[PXDefault(PhoneTypesAttribute.Business2, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Phone 2")]
			[PhoneTypes]
			public virtual String Phone2Type { get; set; }
			#endregion

			#region Phone3
			public abstract class phone3 : PX.Data.IBqlField { }

			[PXDBString(50)]
			[PXUIField(DisplayName = "Phone 3")]
			[PhoneValidation()]
			public virtual String Phone3 { get; set; }
			#endregion

			#region Phone3Type
			public abstract class phone3Type : PX.Data.IBqlField { }

			[PXDBString(3)]
			[PXDefault(PhoneTypesAttribute.Home, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Phone 3")]
			[PhoneTypes]
			public virtual String Phone3Type { get; set; }
			#endregion

			#region WebSite
			public abstract class webSite : PX.Data.IBqlField { }

			[PXDBWeblink]
			[PXUIField(DisplayName = "Web")]
			public virtual String WebSite { get; set; }
			#endregion

			#region Fax
			public abstract class fax : PX.Data.IBqlField { }

			[PXDBString(50)]
			[PXUIField(DisplayName = "Fax")]
			[PhoneValidation()]
			public virtual String Fax { get; set; }
			#endregion

			#region EMail
			public abstract class eMail : PX.Data.IBqlField { }

			[PXDBEmail]
			[PXUIField(DisplayName = "Email", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String EMail { get; set; }
			#endregion
		}
		#endregion

		public const string DimensionName = "BIZACCT";
        /// <summary>
        /// Default Ctor
        /// </summary>
		public CustomerAttribute()
			: this(typeof(Search<BAccountR.bAccountID>))
		{
		}
        /// <summary>
        /// Extended Ctor. User may define customised search.
        /// </summary>
        /// <param name="search">Must Be IBqlSearch, which returns BAccountID. Tables Customer,Contact, Address will be added automatically
        /// </param>
		public CustomerAttribute(Type search)
			: this(search,
					typeof(Customer.acctCD), typeof(Customer.acctName),
					typeof(Address.addressLine1), typeof(Address.addressLine2), typeof(Address.postalCode),					
					typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID),
					typeof(Location.taxRegistrationID), typeof(Customer.curyID),
					typeof(Contact.salutation),
					typeof(Customer.customerClassID), typeof(Customer.status))
		{			
		}

        /// <summary>
        /// Extended ctor - full. User may define a select and the list of the fields.
        /// </summary>
        /// </summary>
        /// <param name="search">Must be IBqlSearch, which returns BAccountID. Tables Customer,Contact, Address will be added automatically </param>
	    /// <param name="fields">Msut be a list of IBqlFields from the tables, used in the search</param>
		public CustomerAttribute(Type search, params Type[] fields)
			: base()
		{
			Type searchType = search.GetGenericTypeDefinition();
			Type[] searchArgs = search.GetGenericArguments();

			Type cmd;
			if (searchType == typeof(Search<>))
			{
				cmd = BqlCommand.Compose(
								typeof(Search2<,,>),
								typeof(BAccountR.bAccountID),
								typeof(LeftJoin<,,>),
								typeof(Customer),
								typeof(On<Customer.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Customer, Current<AccessInfo.userName>>>>),
								typeof(LeftJoin<,,>),
								typeof(Contact),
								typeof(On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>),
								typeof(LeftJoin<,,>),
								typeof(Address),
								typeof(On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>),
								typeof(LeftJoin<,>),
								typeof(Location),
								typeof(On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>),
								typeof(Where<Customer.bAccountID, IsNotNull>));
			}
			else if (searchType == typeof(Search<,>))
			{
				cmd = BqlCommand.Compose(
								typeof(Search2<,,>),
								typeof(BAccountR.bAccountID),
								typeof(LeftJoin<,,>),
								typeof(Customer),
								typeof(On<Customer.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Customer, Current<AccessInfo.userName>>>>),
								typeof(LeftJoin<,,>),
								typeof(Contact),
								typeof(On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>),
								typeof(LeftJoin<,,>),
								typeof(Address),
								typeof(On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>),
								typeof(LeftJoin<,>),
								typeof(Location),
								typeof(On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>),
								searchArgs[1]);
			}
			else if (searchType == typeof(Search<,,>))
			{
				cmd = BqlCommand.Compose(
								typeof(Search2<,,>),
								typeof(BAccountR.bAccountID),
								typeof(LeftJoin<,,>),
								typeof(Customer),
								typeof(On<Customer.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Customer, Current<AccessInfo.userName>>>>),
								typeof(LeftJoin<,,>),
								typeof(Contact),
								typeof(On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>),
								typeof(LeftJoin<,,>),
								typeof(Address),
								typeof(On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>),
								typeof(LeftJoin<,>),
								typeof(Location),
								typeof(On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>),
								searchArgs[1],
								searchArgs[2]);
			}
			else if (searchType == typeof(Search2<,>))
			{
				cmd = BqlCommand.Compose(
								typeof(Search2<,,>),
								typeof(BAccountR.bAccountID),
								typeof(LeftJoin<,,>),
								typeof(Customer),
								typeof(On<Customer.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Customer, Current<AccessInfo.userName>>>>),
								typeof(LeftJoin<,,>),
								typeof(Contact),
								typeof(On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>),
								typeof(LeftJoin<,,>),
								typeof(Address),
								typeof(On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>),
								typeof(LeftJoin<,,>),
								typeof(Location),
								typeof(On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>),
								searchArgs[1],
								typeof(Where<Customer.bAccountID, IsNotNull>));
			}
			else if (searchType == typeof(Search2<,,>))
			{
				cmd = BqlCommand.Compose(
								typeof(Search2<,,>),
								typeof(BAccountR.bAccountID),
								typeof(LeftJoin<,,>),
								typeof(Customer),
								typeof(On<Customer.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Customer, Current<AccessInfo.userName>>>>),
								typeof(LeftJoin<,,>),
								typeof(Contact),
								typeof(On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>),
								typeof(LeftJoin<,,>),
								typeof(Address),
								typeof(On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>),
								typeof(LeftJoin<,,>),
								typeof(Location),
								typeof(On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>),
								searchArgs[1],
								searchArgs[2]);
			}
			else if (searchType == typeof(Search2<,,,>))
			{
				cmd = BqlCommand.Compose(
								typeof(Search2<,,>),
								typeof(BAccountR.bAccountID),
								typeof(LeftJoin<,,>),
								typeof(Customer),
								typeof(On<Customer.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Customer, Current<AccessInfo.userName>>>>),
								typeof(LeftJoin<,,>),
								typeof(Contact),
								typeof(On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>),
								typeof(LeftJoin<,,>),
								typeof(Address),
								typeof(On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>),
								typeof(LeftJoin<,,>),
								typeof(Location),
								typeof(On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>),
								searchArgs[1],
								searchArgs[2],
								searchArgs[3]);
			}
			else
			{
				throw new PXArgumentException("search", ErrorMessages.ArgumentException);
			}

			PXDimensionSelectorAttribute attr;
			_Attributes.Add(attr = new PXDimensionSelectorAttribute(DimensionName, cmd, typeof(BAccountR.acctCD),
				typeof(BAccountR.acctCD), typeof(BAccountR.acctName), typeof(Customer.customerClassID), typeof(Customer.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID)
			));
			attr.DescriptionField = typeof(Customer.acctName);
			_SelAttrIndex = _Attributes.Count - 1;
			this.Filterable = true;
			_fields = fields;
		}
											
		private readonly Type[] _fields;

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			string[] selFields = new string[this._fields.Length];
			string[] selHeaders = new string[this._fields.Length];
			for (int i = 0; i < this._fields.Length; i++)
			{
				Type cacheType = BqlCommand.GetItemType(_fields[i]);
				PXCache cache = sender.Graph.Caches[cacheType];
				if (cacheType.IsAssignableFrom(typeof(BAccountR)) || 
					_fields[i].Name == typeof(BAccountR.acctCD).Name ||
					_fields[i].Name == typeof(BAccountR.acctName).Name)
				{
					selFields[i] = _fields[i].Name;
				}
				else
				{
					selFields[i] = cacheType.Name + "__" + _fields[i].Name;
				}
				selHeaders[i] = PXUIFieldAttribute.GetDisplayName(cache, _fields[i].Name);
			}
			PXSelectorAttribute.SetColumns(sender, _FieldName, selFields, selHeaders);
			EmitColumnForCustomerField(sender);
		}

		protected void EmitColumnForCustomerField(PXCache sender)
		{
			if (this.DescriptionField  == null) 
				return;			

			{
				string alias = _FieldName + "_" + typeof(Customer).Name + "_" + DescriptionField.Name;
				if (!sender.Fields.Contains(alias))
				{
					sender.Fields.Add(alias);
					sender.Graph.FieldSelecting.AddHandler
						(sender.GetItemType(), alias,
						 (s, e) => GetAttribute<PXDimensionSelectorAttribute>().GetAttribute<PXSelectorAttribute>().DescriptionFieldSelecting(s, e, alias));
				}
			}			
			{
				string alias = _FieldName + "_description";
				if (!sender.Fields.Contains(alias))
				{
					sender.Fields.Add(alias);
					sender.Graph.FieldSelecting.AddHandler
						(sender.GetItemType(), alias,
						 (s, e) => GetAttribute<PXDimensionSelectorAttribute>().GetAttribute<PXSelectorAttribute>().DescriptionFieldSelecting(s, e, alias));					
				}
			}
		}
	}


    [PXDBString(15, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "SalesPerson ID", Visibility = PXUIVisibility.Visible)]
	public sealed class SalesPersonRawAttribute : AcctSubAttribute
	{
		public string DimensionName = "SALESPER";
		public SalesPersonRawAttribute()
			: base()
		{

			Type SearchType = typeof(Search<SalesPerson.salesPersonCD, Where<boolTrue, Equal<boolTrue>>>);
			PXDimensionSelectorAttribute attr;
			_Attributes.Add(attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(SalesPerson.salesPersonCD),
					typeof(SalesPerson.salesPersonCD), typeof(SalesPerson.descr)
				));
			attr.DescriptionField = typeof(SalesPerson.descr);
			_SelAttrIndex = _Attributes.Count - 1;

			((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).CacheGlobal = true;
		}
	}

    /// <summary>
    /// Provides UI selector of the Salespersons. <br/>
    /// Properties of the selector - mask, length of the key, etc.<br/>
    /// are defined in the Dimension with predefined name "SALESPER".<br/>    
    ///	As most Dimention Selector - substitutes SalesPersonID by SalesPersonCD.<br/>
    ///	List of properties - inherited from AcctSubAttribute <br/>    
    /// </summary>	
	[PXDBInt()]
	[PXUIField(DisplayName = "Salesperson ID", Visibility = PXUIVisibility.Visible)]
    [PXRestrictor(typeof(Where<SalesPerson.isActive, Equal<True>>), Messages.SalesPersonIsInactive)]
	public class SalesPersonAttribute : AcctSubAttribute
	{
		public const string DimensionName = "SALESPER";

        /// <summary>
        /// Default ctor. Shows all the salespersons
        /// </summary>
		public SalesPersonAttribute()
            : this(typeof(Where<True, Equal<True>>))
		{
		}
        /// <summary>
        /// Extended ctor. User can provide addtional where clause
        /// </summary>
        /// <param name="WhereType">Must be IBqlWhere type. Additional Where Clause</param>
		public SalesPersonAttribute(Type WhereType)
			: base()
		{
			Type SearchType =
				BqlCommand.Compose(
				typeof(Search<,>),
				typeof(SalesPerson.salesPersonID),
				/*typeof(LeftJoin<Contact, On<Contact.bAccountID, Equal<SalesPerson.bAccountID>, And<Contact.contactID, Equal<SalesPerson.contactID>>>, 
							LeftJoin<Address, On<Address.bAccountID, Equal<SalesPerson.bAccountID>, And<Address.addressID, Equal<Contact.defAddressID>>>>>),*/
				WhereType);

			PXDimensionSelectorAttribute attr;
			_Attributes.Add(attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(SalesPerson.salesPersonCD),
				typeof(SalesPerson.salesPersonCD), typeof(SalesPerson.descr)
			));
			attr.DescriptionField = typeof(SalesPerson.descr);
			_SelAttrIndex = _Attributes.Count - 1;
            attr.CacheGlobal = true;
		}

        /// <summary>
        /// Extended ctor, full form. User can provide addtional Where and Join clause. 
        /// </summary>
        /// <param name="WhereType">Must be IBqlWhere type. Additional Where Clause</param>
        /// <param name="JoinType">Must be IBqlJoin type. Defines Join Clause</param>
		public SalesPersonAttribute(Type WhereType, Type JoinType)
			: base()
		{
			/* if (!JoinType.IsGenericType)
		 {
			 throw new PXException(PX.Data.ErrorMessages.InvalidJoinCriteria);
		 }
		 Type[] args = JoinType.GetGenericArguments();
		 JoinType = JoinType.GetGenericTypeDefinition();
		 if (JoinType == typeof(CrossJoin<>))
		 {
			 JoinType = typeof(CrossJoin<,>);
		 }
		 else if (JoinType == typeof(InnerJoin<,>))
		 {
			 JoinType = typeof(InnerJoin<,,>);
		 }
		 else if (JoinType == typeof(LeftJoin<,>))
		 {
			 JoinType = typeof(LeftJoin<,,>);
		 }
		 else if (JoinType == typeof(RightJoin<,>))
		 {
			 JoinType = typeof(RightJoin<,,>);
		 }
		 else if (JoinType == typeof(FullJoin<,>))
		 {
			 JoinType = typeof(FullJoin<,,>);
		 }
		 else
		 {
			 throw new PXException(PX.Data.ErrorMessages.InvalidJoinCriteria);
		 }
		 Array.Resize<Type>(ref args, args.Length + 5);
		 for (int i = args.Length - 6; i >= 0; i--)
		 {
			 args[i + 3] = args[i];
		 }
		 args[0] = typeof(Search2<,,>);
		 args[1] = typeof(SalesPerson.salesPersonID);
		 args[args.Length - 2] = typeof(LeftJoin<Contact,
			 On<Contact.bAccountID, Equal<SalesPerson.bAccountID>, And<Contact.contactID, Equal<SalesPerson.contactID>>>,
			 LeftJoin<Address, 
			 On<Address.bAccountID, Equal<SalesPerson.bAccountID>, And<Address.addressID, Equal<Contact.defAddressID>>>>>);
		 args[args.Length - 1] = WhereType;
		 Type SearchType = BqlCommand.Compose(args);*/
			Type SearchType =
				BqlCommand.Compose(
				typeof(Search2<,,>),
				typeof(SalesPerson.salesPersonID),
				JoinType,
				WhereType);

			PXDimensionSelectorAttribute attr;
			_Attributes.Add(attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(SalesPerson.salesPersonCD),
				typeof(SalesPerson.salesPersonCD), typeof(SalesPerson.descr)
			));
			attr.DescriptionField = typeof(SalesPerson.descr);
			_SelAttrIndex = _Attributes.Count - 1;
            attr.CacheGlobal = true;
        }

		public override bool DirtyRead
		{
			get
			{
				return (_SelAttrIndex == -1) ? false : ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).DirtyRead;
			}
			set
			{
				if (_SelAttrIndex != -1)
					((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).DirtyRead = value;
			}
		}
	}

	public class ARCashSaleTaxAttribute : ARTaxAttribute
	{
		public ARCashSaleTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
            DocDate = typeof(ARCashSale.adjDate);
            FinPeriodID = typeof(ARCashSale.adjFinPeriodID); 
            CuryLineTotal = typeof(ARCashSale.curyLineTotal);
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<ARTran.lineType, NotEqual<SO.SOLineType.discount>>, ARTran.curyTranAmt>, Minus<ARTran.curyTranAmt>>), typeof(SumCalc<ARCashSale.curyLineTotal>)));
		}

		protected override void ParentFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARCashSale doc;
			if (e.Row is ARCashSale && ((ARCashSale)e.Row).DocType != ARDocType.CashReturn)
			{
				base.ParentFieldUpdated(sender, e);
			}
			else if (e.Row is CurrencyInfo && (doc = PXSelect<ARCashSale, Where<ARCashSale.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(sender.Graph, ((CurrencyInfo)e.Row).CuryInfoID)) != null && doc.DocType != ARDocType.CashReturn)
			{
				base.ParentFieldUpdated(sender, e);
			}
		}

		protected override void ZoneUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (((ARCashSale)e.Row).DocType != ARDocType.CashReturn)
			{
				base.ZoneUpdated(sender, e);
			}
		}

		protected override void DateUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (((ARCashSale)e.Row).DocType != ARDocType.CashReturn)
			{
				base.DateUpdated(sender, e);
			}
		}
	}

	public class ARTaxAttribute : TaxAttribute
	{
		protected abstract class signedCuryTranAmt : IBqlField { }

		public ARTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
            CuryTranAmt = typeof(ARTran.curyTranAmt);
            GroupDiscountRate = typeof(ARTran.groupDiscountRate);
			CuryLineTotal = typeof(ARInvoice.curyLineTotal);
            this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<ARTran.lineType, NotEqual<SO.SOLineType.discount>>, ARTran.curyTranAmt>, decimal0>), typeof(SumCalc<ARInvoice.curyLineTotal>)));
            this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<
            Case<Where<ARTran.commissionable, Equal<True>, And<Where<ARTran.lineType, NotEqual<SO.SOLineType.discount>,
            And<Where<ARTran.tranType, Equal<ARDocType.creditMemo>, Or<ARTran.tranType, Equal<ARDocType.cashReturn>>>>>>>,
                Minus<Mult<ARTran.curyTranAmt, ARTran.groupDiscountRate>>,
            Case<Where<ARTran.commissionable, Equal<True>, And<Where<ARTran.lineType, NotEqual<SO.SOLineType.discount>>>>,
                Mult<ARTran.curyTranAmt, ARTran.groupDiscountRate>>>,
                decimal0>),
            typeof(SumCalc<ARSalesPerTran.curyCommnblAmt>)));
		}

        protected override decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType)
        {
            if (TaxCalcType == "I")
                return base.GetCuryTranAmt(sender, row) * (decimal?)sender.GetValue(row, _GroupDiscountRate);
            else
                return base.GetCuryTranAmt(sender, row);
        }

        protected override void SetTaxableAmt(PXCache sender, object row, decimal? value)
        {
            sender.SetValue<ARTran.curyTaxableAmt>(row, value);
        }

        protected override void SetTaxAmt(PXCache sender, object row, decimal? value)
        {
            sender.SetValue<ARTran.curyTaxAmt>(row, value);
        }

        protected override void ParentSetValue(PXGraph graph, string fieldname, object value)
        {
            PXCache cache = graph.Caches[typeof(ARSetup)];
            if (fieldname == _CuryDocBal && cache.Current != null && ((ARSetup)cache.Current).InvoiceRounding != RoundingType.Currency)
            {
                decimal? oldval = (decimal?)value;
                value = ARReleaseProcess.RoundAmount((decimal?)value, ((ARSetup)cache.Current).InvoiceRounding, ((ARSetup)cache.Current).InvoicePrecision);

                decimal oldbaseval;
                decimal baseval;
                PXDBCurrencyAttribute.CuryConvBase(ParentCache(graph), ParentRow(graph), (decimal)oldval, out oldbaseval);
                PXDBCurrencyAttribute.CuryConvBase(ParentCache(graph), ParentRow(graph), (decimal)value, out baseval);

                oldbaseval -= baseval;
                oldval -= (decimal?)value;

                base.ParentSetValue(graph, typeof(ARRegister.curyRoundDiff).Name, oldval);
                base.ParentSetValue(graph, typeof(ARRegister.roundDiff).Name, oldbaseval);
            }
            else
            {
                base.ParentSetValue(graph, typeof(ARRegister.curyRoundDiff).Name, 0m);
                base.ParentSetValue(graph, typeof(ARRegister.roundDiff).Name, 0m);
            }

            base.ParentSetValue(graph, fieldname, value);
        }

        protected override void AdjustTaxableAmount(PXCache sender, object row, List<object> taxitems, ref decimal CuryTaxableAmt)
        {
            decimal CuryLineTotal = (decimal?)ParentGetValue<ARInvoice.curyLineTotal>(sender.Graph) ?? 0m;
            decimal CuryDiscountTotal = (decimal)(ParentGetValue<ARInvoice.curyDiscTot>(sender.Graph) ?? 0m);

            if (CuryLineTotal != 0m && CuryTaxableAmt != 0m)
            {
                if (Math.Abs(CuryTaxableAmt - CuryLineTotal) < 0.00005m)
                {
                    CuryTaxableAmt -= CuryDiscountTotal;
                }
                else
                {
                    CuryTaxableAmt = PXCurrencyAttribute.RoundCury(ParentCache(sender.Graph), ParentRow(sender.Graph), CuryTaxableAmt * (1 - CuryDiscountTotal / CuryLineTotal));
                }
            }
        }

		protected override decimal CalcLineTotal(PXCache sender, object row)
		{
			return (decimal?)ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m;
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
			foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
				LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
					And<TaxRev.outdated, Equal<boolFalse>, 
					And<TaxRev.taxType, Equal<TaxType.sales>, 
					And<Tax.taxType, NotEqual<CSTaxType.withholding>, 
					And<Tax.taxType, NotEqual<CSTaxType.use>, 
					And<Tax.reverseTax, Equal<boolFalse>, 
					And<Current<ARRegister.docDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>>,
				Where>
				.SelectMultiBound(graph, new object[] { row }, parameters))
			{
				tail[((Tax)record).TaxID] = record;
			}
			List<object> ret = new List<object>();
			switch (taxchk)
			{
				case PXTaxCheck.Line:
					foreach (ARTax record in PXSelect<ARTax,
						Where<ARTax.tranType, Equal<Current<ARTran.tranType>>,
							And<ARTax.refNbr, Equal<Current<ARTran.refNbr>>,
							And<ARTax.lineNbr, Equal<Current<ARTran.lineNbr>>>>>>
						.SelectMultiBound(graph, new object[] { row }))
					{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& String.Compare(((Tax)(PXResult<ARTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<ARTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				case PXTaxCheck.RecalcLine:
					foreach (ARTax record in PXSelect<ARTax,
						Where<ARTax.tranType, Equal<Current<ARRegister.docType>>,
							And<ARTax.refNbr, Equal<Current<ARRegister.refNbr>>>>>
						.SelectMultiBound(graph, new object[] { row }))
					{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& ((ARTax)(PXResult<ARTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
								&& String.Compare(((Tax)(PXResult<ARTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<ARTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				case PXTaxCheck.RecalcTotals:
					foreach (ARTaxTran record in PXSelect<ARTaxTran,
						Where<ARTaxTran.module, Equal<BatchModule.moduleAR>,
							And<ARTaxTran.tranType, Equal<Current<ARRegister.docType>>,
							And<ARTaxTran.refNbr, Equal<Current<ARRegister.refNbr>>>>>>
						.SelectMultiBound(graph, new object[] { row }))
					{
						PXResult<Tax, TaxRev> line;
						if (record.TaxID != null && tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& String.Compare(((Tax)(PXResult<ARTaxTran, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<ARTaxTran, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				default:
					return ret;
			}
		}

        protected override void _CalcDocTotals(PXCache sender, object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
        {
            decimal CuryDiscountTotal = (decimal)(ParentGetValue<ARRegister.curyDiscTot>(sender.Graph) ?? 0m);

            decimal CuryLineTotal = CalcLineTotal(sender, row);

            decimal CuryDocTotal = CuryLineTotal + CuryTaxTotal - CuryInclTaxTotal - CuryDiscountTotal;

            decimal doc_CuryLineTotal = (decimal)(ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m);
            decimal doc_CuryTaxTotal = (decimal)(ParentGetValue(sender.Graph, _CuryTaxTotal) ?? 0m);

            if (object.Equals(CuryLineTotal, doc_CuryLineTotal) == false ||
                object.Equals(CuryTaxTotal, doc_CuryTaxTotal) == false)
            {
                ParentSetValue(sender.Graph, _CuryLineTotal, CuryLineTotal);
                ParentSetValue(sender.Graph, _CuryTaxTotal, CuryTaxTotal);
                if (!string.IsNullOrEmpty(_CuryDocBal))
                {
                    ParentSetValue(sender.Graph, _CuryDocBal, CuryDocTotal);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(_CuryDocBal))
            {
                decimal doc_CuryDocBal = (decimal)(ParentGetValue(sender.Graph, _CuryDocBal) ?? 0m);

                if (object.Equals(CuryDocTotal, doc_CuryDocBal) == false)
                {
                    ParentSetValue(sender.Graph, _CuryDocBal, CuryDocTotal);
                }
            }
        }
	}
    /// <summary>
    /// Specialized for the ARPayment  version of the <see cref="CashTranIDAttribute"/><br/>
    /// Since CATran created from the source row, it may be used only the fields <br/>
    /// of ARPayment compatible DAC. <br/>
    /// The main purpuse of the attribute - to create CATran <br/>
    /// for the source row and provide CATran and source synchronization on persisting.<br/>
    /// CATran cache must exists in the calling Graph.<br/>
    /// </summary>
	public class ARCashTranIDAttribute : CashTranIDAttribute
	{
		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			return ARCashTranIDAttribute.DefaultValues(sender, catran_Row, (ARPayment)orig_Row);
		}

		public static CATran DefaultValues(PXCache sender, CATran catran_Row, ARPayment orig_Row)
		{
			ARPayment parentDoc = (ARPayment)orig_Row;
			if (parentDoc.DocType == ARDocType.CreditMemo ||
				parentDoc.DocType == ARDocType.SmallBalanceWO ||
				(parentDoc.Released == true) && (catran_Row.TranID != null) ||
				 parentDoc.CuryOrigDocAmt == null ||
				 parentDoc.CuryOrigDocAmt == 0m || (parentDoc.Released == false && parentDoc.Voided == true))
			{
				return null;
			}

			catran_Row.OrigModule = BatchModule.AR;
			catran_Row.OrigTranType = parentDoc.DocType;
			catran_Row.OrigRefNbr = parentDoc.RefNbr;
			catran_Row.ExtRefNbr = parentDoc.ExtRefNbr;
			catran_Row.CashAccountID = parentDoc.CashAccountID;
			catran_Row.CuryInfoID = parentDoc.CuryInfoID;
			catran_Row.CuryID = parentDoc.CuryID;
            string voidedType = string.Empty;
			switch (parentDoc.DocType)
			{
				case ARDocType.Payment:
				case ARDocType.CashSale:
				case ARDocType.Prepayment:
					catran_Row.CuryTranAmt = parentDoc.CuryOrigDocAmt;
					catran_Row.DrCr = "D";
					break;
				case ARDocType.VoidPayment:
					catran_Row.CuryTranAmt = parentDoc.CuryOrigDocAmt;
					catran_Row.DrCr = "C";
                    voidedType = ARDocType.Payment;
					break;
				case ARDocType.Refund:
				case ARDocType.CashReturn:
					catran_Row.CuryTranAmt = -parentDoc.CuryOrigDocAmt;
					catran_Row.DrCr = "C"; 
                    //Cash Return is not considered as a void for CashSale;
					break;
				default:
					throw new PXException();
			}

			catran_Row.TranDate = parentDoc.DocDate;
			catran_Row.TranDesc = parentDoc.DocDesc;
			catran_Row.FinPeriodID = parentDoc.FinPeriodID;
			catran_Row.ReferenceID = parentDoc.CustomerID;
			catran_Row.Released = parentDoc.Released;
			catran_Row.Hold = parentDoc.Hold;
			catran_Row.Cleared = parentDoc.Cleared;
			catran_Row.ClearDate = parentDoc.ClearDate;
			//This coping is required in one specfic case - when payment reclassification is made
			if (parentDoc.CARefTranID.HasValue)
			{
				catran_Row.RefTranAccountID = parentDoc.CARefTranAccountID;
				catran_Row.RefTranID = parentDoc.CARefTranID;
                catran_Row.RefSplitLineNbr = parentDoc.CARefSplitLineNbr;
			}

            if (!string.IsNullOrEmpty(voidedType))
            {
                string refNbr = parentDoc.RefNbr;                
                ARPayment voidedDoc = PXSelectReadonly<ARPayment, Where<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>,
                                                And<ARPayment.docType, Equal<Required<ARPayment.docType>>>>>.Select(sender.Graph, refNbr, voidedType);
                    
                if (voidedDoc != null)
                {
                    catran_Row.VoidedTranID = voidedDoc.CATranID;
                }
            }

			PXSelectBase<CashAccount> selectStatement = new PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>(sender.Graph);
			CashAccount cashacc = (CashAccount)selectStatement.View.SelectSingle(catran_Row.CashAccountID);
			if (cashacc != null && cashacc.Reconcile == false && (catran_Row.Cleared != true || catran_Row.TranDate == null))
			{
				catran_Row.Cleared = true;
				catran_Row.ClearDate = catran_Row.TranDate;
			}

			if (catran_Row.ExtRefNbr == null)
			{
				throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<ARPayment.extRefNbr>(sender));
			}

			return catran_Row;
		}
	}

    /// <summary>
    /// Specialized for the ARCashSale  version of the <see cref="CashTranIDAttribute"/><br/>
    /// Since CATran created from the source row, it may be used only the fields <br/>
    /// of ARPayment compatible DAC. <br/>
    /// The main purpuse of the attribute - to create CATran <br/>
    /// for the source row and provide CATran and source synchronization on persisting.<br/>
    /// CATran cache must exists in the calling Graph.<br/>
    /// </summary>
	public class ARCashSaleCashTranIDAttribute : CashTranIDAttribute
	{
		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			ARCashSale parentDoc = (ARCashSale)orig_Row;
			if (parentDoc.Released == true || parentDoc.CuryOrigDocAmt == null || parentDoc.CuryOrigDocAmt == 0m)
			{
				return null;
			}

			catran_Row.OrigModule = BatchModule.AR;
			catran_Row.OrigTranType = parentDoc.DocType;
			catran_Row.OrigRefNbr = parentDoc.RefNbr;
			catran_Row.ExtRefNbr = parentDoc.ExtRefNbr;
			catran_Row.CashAccountID = parentDoc.CashAccountID;
			catran_Row.CuryInfoID = parentDoc.CuryInfoID;
			catran_Row.CuryID = parentDoc.CuryID;
            
			switch (parentDoc.DocType)
			{
				case ARDocType.CashSale:
					catran_Row.CuryTranAmt = parentDoc.CuryOrigDocAmt;
					catran_Row.DrCr = "D";
					break;
				case ARDocType.CashReturn:
					catran_Row.CuryTranAmt = -parentDoc.CuryOrigDocAmt;
					catran_Row.DrCr = "C";                    
					break;
				default:
					throw new PXException();
			}

			catran_Row.TranDate = parentDoc.DocDate;
			catran_Row.TranDesc = parentDoc.DocDesc;
			catran_Row.FinPeriodID = parentDoc.FinPeriodID;
			catran_Row.ReferenceID = parentDoc.CustomerID;
			catran_Row.Released = parentDoc.Released;
			catran_Row.Hold = parentDoc.Hold;
			catran_Row.Cleared = parentDoc.Cleared;
			catran_Row.ClearDate = parentDoc.ClearDate;            
            
			return catran_Row;
		}
	}

    /// <summary>
    /// Specialized for AR version of the <see cref="OpenPeriodAttribut"/><br/>
    /// Selector. Provides  a list  of the active Fin. Periods, having ARClosed flag = false <br/>
    /// <example>
    /// [AROpenPeriod(typeof(ARRegister.docDate))]
    /// </example>
    /// </summary>
	public class AROpenPeriodAttribute : OpenPeriodAttribute
	{
		#region Ctor

        /// <summary>
        /// Extended Ctor. 
        /// </summary>
        /// <param name="SourceType">Must be IBqlField. Refers a date, based on which "current" period will be defined</param>
		public AROpenPeriodAttribute(Type SourceType)
			: base(typeof(Search<FinPeriod.finPeriodID, Where<FinPeriod.aRClosed, Equal<False>, And<FinPeriod.active, Equal<True>>>>), SourceType)
		{
		}

		public AROpenPeriodAttribute()
			: this(null)
		{
		}
		#endregion

		#region Implementation

		public static void DefaultFirstOpenPeriod(PXCache sender, string FieldName)
		{
			foreach (PeriodIDAttribute attr in sender.GetAttributesReadonly(FieldName).OfType<PeriodIDAttribute>())
			{
				attr.SearchType = typeof(Search2<FinPeriod.finPeriodID, CrossJoin<GLSetup>, Where<FinPeriod.endDate, Greater<Required<FinPeriod.endDate>>, And<FinPeriod.active, Equal<True>, And<Where<GLSetup.postClosedPeriods, Equal<True>, Or<FinPeriod.aRClosed, Equal<False>>>>>>, OrderBy<Asc<FinPeriod.finPeriodID>>>);
			}
		}

		public static void DefaultFirstOpenPeriod<Field>(PXCache sender)
			where Field : IBqlField
		{
			DefaultFirstOpenPeriod(sender, typeof(Field).Name);
		}

		public override void IsValidPeriod(PXCache sender, object Row, object NewValue)
		{
			string OldValue = (string)sender.GetValue(Row, _FieldName);
			base.IsValidPeriod(sender, Row, NewValue);

			if (NewValue != null && _ValidatePeriod != PeriodValidation.Nothing)
			{
				FinPeriod p = (FinPeriod)PXSelect<FinPeriod, Where<FinPeriod.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>>>.Select(sender.Graph, (string)NewValue);
				if (p.ARClosed == true)
				{
                    if (PostClosedPeriods(sender.Graph))
                    {
                        sender.RaiseExceptionHandling(_FieldName, Row, null, new FiscalPeriodClosedException(p.FinPeriodID, PXErrorLevel.Warning));
                        return;
                    }
                    else
                    {
                        throw new FiscalPeriodClosedException(p.FinPeriodID);
                    }
                }
			}
			return;
		}
		#endregion
	}

	/// <summary>
	/// FinPeriod selector that extends <see cref="FinPeriodSelectorAttribute"/>. 
	/// Displays and accepts only Closed Fin Periods. 
	/// When Date is supplied through aSourceType parameter FinPeriod is defaulted with the FinPeriod for the given date.
	/// </summary>
	public class ARClosedPeriodAttribute : FinPeriodSelectorAttribute
	{
		public ARClosedPeriodAttribute(Type aSourceType)
			: base(typeof(Search<FinPeriod.finPeriodID, Where<FinPeriod.closed, Equal<True>, Or<FinPeriod.aRClosed, Equal<True>, Or<FinPeriod.active, Equal<True>>>>, OrderBy<Desc<FinPeriod.finPeriodID>>>), aSourceType)
		{
		}

		public ARClosedPeriodAttribute()
			: this(null)
		{

		}
	}

	#region accountsReceivableModule

	public sealed class accountsReceivableModule : Constant<string>
	{
		public accountsReceivableModule() : base(typeof(accountsReceivableModule).Namespace) { }
	}

	#endregion
	#region customerType
	public sealed class customerType : Constant<string>
	{
		public customerType()
			: base(typeof(PX.Objects.AR.Customer).FullName)
		{
		}
	}
	#endregion

	#region ARRegisterCacheNameAttribute

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class ARRegisterCacheNameAttribute : PX.Data.PXCacheNameAttribute
	{
		public ARRegisterCacheNameAttribute(string name)
			: base(name)
		{
		}

		public override string GetName(object row)
		{
			var register = row as ARRegister;
			if (register == null) return base.GetName();

			switch (register.DocType)
			{
				case ARDocType.CashReturn:
					return Messages.CashReturn;
				case ARDocType.CashSale:
					return Messages.CashSale;
				case ARDocType.CreditMemo:
					return Messages.CreditMemo;
				case ARDocType.DebitMemo:
					return Messages.DebitMemo;
				case ARDocType.FinCharge:
					return Messages.FinCharge;
				case ARDocType.Invoice:
					return Messages.Invoice;
				case ARDocType.Payment:
					return Messages.Payment;
				case ARDocType.Prepayment:
					return Messages.Prepayment;
				case ARDocType.Refund:
					return Messages.Refund;
				case ARDocType.SmallBalanceWO:
					return Messages.SmallBalanceWO;
				case ARDocType.SmallCreditWO:
					return Messages.SmallCreditWO;
				case ARDocType.VoidPayment:
					return Messages.VoidPayment;
			}
			return Messages.Register;
		}
	}

	#endregion


	/// <summary>
	/// This attribute implements auto-generation of the next check sequential number for ARPayment Document<br/>
	/// according to the settings in the CashAccount and PaymentMethod. <br/>
	/// </summary>
	public class PaymentRefAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber, IPXFieldDefaultingSubscriber, IPXFieldVerifyingSubscriber
	{
		protected Type _CashAccountID;
		protected Type _PaymentTypeID;
		protected Type _UpdateNextNumber;
		protected Type _ClassType;
		protected string _TargetDisplayName;

		protected bool _UpdateCashManager = true;

		/// <summary>
		/// This flag defines wether the field is defaulted from the PaymentMethodAccount record by the next check number<br/>
		/// If it set to false - the field on which attribute is set will not be initialized by the next value.<br/>
		/// This flag doesn't affect persisting behavior - if user enter next number manually, it will be saved.<br/>
		/// </summary>
		public bool UpdateCashManager
		{
			get
			{
				return this._UpdateCashManager;
			}
			set
			{
				this._UpdateCashManager = value;
			}
		}

		private PaymentMethodAccount GetCashAccountDetail(PXCache sender, object row)
		{
			object CashAccountID = sender.GetValue(row, _CashAccountID.Name);
			object PaymentTypeID = sender.GetValue(row, _PaymentTypeID.Name);
			object Hold = false;

			if (_UpdateCashManager && CashAccountID != null && PaymentTypeID != null)
			{
				PXSelectBase<PaymentMethodAccount> cmd = new PXSelectReadonly<PaymentMethodAccount,
																Where<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>,
																	And<PaymentMethodAccount.paymentMethodID, Equal<Required<PaymentMethodAccount.paymentMethodID>>,
																	And<PaymentMethodAccount.useForAR, Equal<True>>>>>(sender.Graph);
				PaymentMethodAccount det = cmd.Select(CashAccountID, PaymentTypeID);
				cmd.View.Clear();

				if (det != null && det.ARLastRefNbr == null)
				{
					det.ARLastRefNbr = string.Empty;
					det.ARLastRefNbrIsNull = true;
				}
				return det;
			}
			return null;
		}

		private void GetPaymentMethodSettings(PXCache sender, object row, out PaymentMethod aPaymentMethod, out PaymentMethodAccount aPMAccount)
		{
			aPaymentMethod = null;
			aPMAccount = null;
			object CashAccountID = sender.GetValue(row, _CashAccountID.Name);
			object PaymentTypeID = sender.GetValue(row, _PaymentTypeID.Name);
			object Hold = false;
			if (_UpdateCashManager && CashAccountID != null && PaymentTypeID != null)
			{
				PXSelectBase<PaymentMethodAccount> cmd = new PXSelectReadonly2<PaymentMethodAccount,
																	InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<PaymentMethodAccount.paymentMethodID>>>,
																Where<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>,
																	And<PaymentMethodAccount.paymentMethodID, Equal<Required<PaymentMethodAccount.paymentMethodID>>,
																	And<PaymentMethodAccount.useForAR, Equal<True>>>>>(sender.Graph);
				PXResult<PaymentMethodAccount, PaymentMethod> res = (PXResult<PaymentMethodAccount, PaymentMethod>)cmd.Select(CashAccountID, PaymentTypeID);
				aPaymentMethod = res;
				cmd.View.Clear();
				PaymentMethodAccount det = res;
				if (det != null && det.ARLastRefNbr == null)
				{
					det.ARLastRefNbr = string.Empty;
					det.ARLastRefNbrIsNull = true;
				}
				aPMAccount = det;
			}

		}

		void IPXFieldVerifyingSubscriber.FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PaymentMethodAccount det = GetCashAccountDetail(sender, e.Row);
			if (det != null && det.ARAutoNextNbr == true)
			{
				string OldValue = (string)sender.GetValue(e.Row, _FieldOrdinal);

				if (string.IsNullOrEmpty(OldValue) == false && string.IsNullOrEmpty((string)e.NewValue) == false && object.Equals(OldValue, e.NewValue) == false)
				{
					try
					{
						if (sender.Graph.Views.ContainsKey("Document") && sender.Graph.Views["Document"].Cache.GetItemType() == sender.GetItemType())
						{
							WebDialogResult result = sender.Graph.Views["Document"].Ask(e.Row, Messages.AskConfirmation, Messages.AskUpdateLastRefNbr, MessageButtons.YesNo, MessageIcon.Question);
							if (result == WebDialogResult.Yes)
							{
								//will be persisted via Graph
								sender.SetValue(e.Row, this._UpdateNextNumber.Name, true);
							}
						}
					}
					catch (PXException ex)
					{
						if (ex is PXDialogRequiredException)
						{
							throw;
						}
					}
				}
			}

		}

		protected Type _Table = null;

		/// <summary>
		/// Defines a table, from where oldValue of the field is taken from.<br/>
		/// If not set - the table associated with the sender will be used<br/>
		/// </summary>
		public Type Table
		{
			get
			{
				return this._Table;
			}
			set
			{
				this._Table = value;
			}
		}

		protected virtual string GetOldField(PXCache sender, object Row)
		{
			List<PXDataField> fields = new List<PXDataField>();

			fields.Add(new PXDataField(_FieldName));

			foreach (string key in sender.Keys)
			{
				fields.Add(new PXDataFieldValue(key, sender.GetValue(Row, key)));
			}

			using (PXDataRecord OldRow = PXDatabase.SelectSingle(_Table, fields.ToArray()))
			{
				if (OldRow != null)
				{
					return OldRow.GetString(0);
				}
			}
			return null;
		}

		void IPXRowPersistingSubscriber.RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object SetNumber = sender.GetValue(e.Row, _FieldOrdinal);
			bool updateNextNumber = ((Boolean?)sender.GetValue(e.Row, this._UpdateNextNumber.Name)) ?? false;

			PaymentMethodAccount det;
			PaymentMethod pm;
			GetPaymentMethodSettings(sender, e.Row, out pm, out det);

			if (det == null || pm == null)
			{
				return;
			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				string NewNumber = AutoNumberAttribute.NextNumber(det.ARLastRefNbr);

				if (SetNumber != null)
				{
					if (det.ARAutoNextNbr == true && (object.Equals((string)SetNumber, NewNumber) || updateNextNumber))
					{
						try
						{
							PXDatabase.Update<PaymentMethodAccount>(
								new PXDataFieldAssign("ARLastRefNbr", SetNumber),
								new PXDataFieldRestrict("CashAccountID", det.CashAccountID),
								new PXDataFieldRestrict("PaymentMethodID", det.PaymentMethodID),
								new PXDataFieldRestrict("ARLastRefNbr", det.ARLastRefNbr),
								PXDataFieldRestrict.OperationSwitchAllowed);
						}
						catch (PXDatabaseException ex)
						{
							if (ex.ErrorCode == PXDbExceptions.OperationSwitchRequired)
							{
								PXDatabase.Insert<PaymentMethodAccount>(
									new PXDataFieldAssign("CashAccountID", det.CashAccountID),
									new PXDataFieldAssign("PaymentMethodID", det.PaymentMethodID),
									new PXDataFieldAssign("UseForAR", det.UseForAR),
									new PXDataFieldAssign("ARLastRefNbr", NewNumber),
									new PXDataFieldAssign("ARAutoNextNbr", det.ARAutoNextNbr),
									new PXDataFieldAssign("ARIsDefault", det.ARIsDefault),
									new PXDataFieldAssign("UseForAP", det.UseForAP),
									new PXDataFieldAssign("APIsDefault", det.APIsDefault),
									new PXDataFieldAssign("APAutoNextNbr", det.APIsDefault));
							}
							else
							{
								throw;
							}
						}
					}
				}
			}
		}

		void IPXFieldDefaultingSubscriber.FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PaymentMethod pm;
			PaymentMethodAccount det;
			GetPaymentMethodSettings(sender, e.Row, out pm, out det);
			e.NewValue = null;
			if (pm != null && det != null)
			{
				if (det.ARAutoNextNbr == true)
				{
					int i = 0;
					do
					{
						try
						{
							e.NewValue = AutoNumberAttribute.NextNumber(det.ARLastRefNbr, ++i);
						}
						catch (Exception)
						{
							sender.RaiseExceptionHandling(_FieldName, e.Row, null, new AutoNumberException(_TargetDisplayName));
						}

						if (i > 100)
						{
							e.NewValue = null;
							new AutoNumberException(_TargetDisplayName);
						}
					}
					while (PXSelect<CashAccountCheck, Where<CashAccountCheck.accountID, Equal<Current<PaymentMethodAccount.cashAccountID>>, And<CashAccountCheck.paymentMethodID, Equal<Current<PaymentMethodAccount.paymentMethodID>>, And<CashAccountCheck.checkNbr, Equal<Required<CashAccountCheck.checkNbr>>>>>>.SelectSingleBound(sender.Graph, new object[] { det }, new object[] { e.NewValue }).Count == 1);

					if (i > 1)
					{
						//will be persisted via Graph
						//det.APLastRefNbr = (string)e.NewValue;
						//sender.Graph.Caches[typeof(PaymentMethodAccount)].Update(det);
						sender.SetValue(e.Row, this._UpdateNextNumber.Name, true);
					}
				}
			}
		}


		public PaymentRefAttribute(Type CashAccountID, Type PaymentTypeID, Type UpdateNextNumber)
		{
			_CashAccountID = CashAccountID;
			_PaymentTypeID = PaymentTypeID;
			this._UpdateNextNumber = UpdateNextNumber;
		}


		private void DefaultRef(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object oldValue = sender.GetValue(e.Row, _FieldName);
			sender.SetValue(e.Row, _FieldName, null);
			sender.SetDefaultExt(e.Row, _FieldName);
			if (sender.GetValue(e.Row, _FieldName) == null)
			{
				sender.SetValue(e.Row, _FieldName, oldValue);
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_ClassType = sender.GetItemType();
			if (_Table == null)
			{
				_Table = sender.BqlTable;
			}

			sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_CashAccountID), _CashAccountID.Name, DefaultRef);
			sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_PaymentTypeID), _PaymentTypeID.Name, DefaultRef);

			_TargetDisplayName = PXUIFieldAttribute.GetDisplayName<PaymentMethodAccount.aRLastRefNbr>(sender.Graph.Caches[typeof(PaymentMethodAccount)]);
		}

		/// <summary>
		/// Sets IsUpdateCashManager flag for each instances of the Attibute specifed on the on the cache.
		/// </summary>
		/// <typeparam name="Field"> field, on which attribute is set</typeparam>
		/// <param name="cache"></param>
		/// <param name="data">Row. If omited, Field is set as altered for the cache</param>
		/// <param name="isUpdateCashManager">Value of the flag</param>
		public static void SetUpdateCashManager<Field>(PXCache cache, object data, bool isUpdateCashManager)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PaymentRefAttribute)
				{
					((PaymentRefAttribute)attr).UpdateCashManager = isUpdateCashManager;
				}
			}
		}
	}

    public class ARPaymentChargeCashTranIDAttribute : CashTranIDAttribute
    {
        protected bool _IsIntegrityCheck = false;

        public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
        {
            ARPaymentChargeTran parentDoc = (ARPaymentChargeTran)orig_Row;
            if (parentDoc.Released == true || parentDoc.CuryTranAmt == null || parentDoc.CuryTranAmt == 0m || parentDoc.Consolidate==true)
            {
                return null;
            }
            catran_Row.OrigModule = BatchModule.AR;
            catran_Row.OrigTranType = parentDoc.DocType;
            catran_Row.OrigRefNbr = parentDoc.RefNbr;
            catran_Row.CashAccountID = parentDoc.CashAccountID;
            catran_Row.CuryInfoID = parentDoc.CuryInfoID;
            catran_Row.CuryTranAmt = parentDoc.CuryTranAmt;
            catran_Row.ExtRefNbr = parentDoc.ExtRefNbr;
            catran_Row.DrCr = parentDoc.DrCr;
            catran_Row.FinPeriodID = parentDoc.FinPeriodID;
            ARPayment payment = PXSelect<ARPayment, Where<ARPayment.docType, Equal<Required<ARPaymentChargeTran.docType>>,
                                                                            And<ARPayment.refNbr, Equal<Required<ARPaymentChargeTran.refNbr>>>>>.Select(sender.Graph, parentDoc.DocType, parentDoc.RefNbr);
            catran_Row.ReferenceID = payment.CustomerID;
            catran_Row.TranPeriodID = parentDoc.TranPeriodID;
            catran_Row.TranDate = parentDoc.TranDate;
            catran_Row.TranDesc = parentDoc.TranDesc;
            catran_Row.Released = parentDoc.Released;

            PXSelectBase<CashAccount> selectStatement = new PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>(sender.Graph);
            CashAccount cashacc = (CashAccount)selectStatement.View.SelectSingle(catran_Row.CashAccountID);
            if (cashacc != null && cashacc.Reconcile == false && (catran_Row.Cleared != true || catran_Row.TranDate == null))
            {
                catran_Row.Cleared = true;
                catran_Row.ClearDate = catran_Row.TranDate;
            }

            return catran_Row;
        }

        public static CATran DefaultValues<Field>(PXCache sender, object data)
            where Field : IBqlField
        {
            foreach (PXEventSubscriberAttribute attr in sender.GetAttributes<Field>(data))
            {
                if (attr is ARPaymentChargeCashTranIDAttribute)
                {
                    ((ARPaymentChargeCashTranIDAttribute)attr)._IsIntegrityCheck = true;
                    return ((ARPaymentChargeCashTranIDAttribute)attr).DefaultValues(sender, new CATran(), data);
                }
            }
            return null;
        }

        public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (_IsIntegrityCheck == false)
            {
                base.RowPersisting(sender, e);
            }
        }

        public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if (_IsIntegrityCheck == false)
            {
                base.RowPersisted(sender, e);
            }
        }
    }

    public class ARSetupSelect : PXSetupSelect<ARSetup>
    {
        public ARSetupSelect(PXGraph graph)
            : base(graph)
        {
        }

        protected override void FillDefaultValues(ARSetup record)
        {
            record.LineDiscountTarget = LineDiscountTargetType.ExtendedPrice;
        }
    }
}

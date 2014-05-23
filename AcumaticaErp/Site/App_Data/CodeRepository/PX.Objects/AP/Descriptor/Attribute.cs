using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.TX;
using PX.Objects.CM;
using PX.Objects.BQLConstants;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.CR;
using APQuickCheck = PX.Objects.AP.Standalone.APQuickCheck;

namespace PX.Objects.AP
{
    /// <summary>
    /// Specialized for AP version of the Address attribute.<br/>
    /// Uses APAddress tables for Address versions storage <br/>
    /// Prameters AddressID, IsDefault address are assigned to the <br/>
    /// corresponded fields in the APAddress table. <br/>
    /// Cache for APAddress must be present in the graph <br/>
    /// Depends upon row instance.
    /// <example>
    /// [APAddress(typeof(Select2<Location,
	///	    InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Location.bAccountID>>,
	///		InnerJoin<Address, On<Address.addressID, Equal<Location.remitAddressID>, 
    ///		    And<Where<Address.bAccountID, Equal<Location.bAccountID>, 
    ///		    Or<Address.bAccountID, Equal<BAccountR.parentBAccountID>>>>>,
	///		LeftJoin<APAddress, On<APAddress.vendorID, Equal<Address.bAccountID>, 
    ///		    And<APAddress.vendorAddressID, Equal<Address.addressID>, 
    ///		    And<APAddress.revisionID, Equal<Address.revisionID>, 
    ///		    And<APAddress.isDefaultAddress, Equal<boolTrue>>>>>>>>,
	///		Where<Location.bAccountID, Equal<Current<APPayment.vendorID>>, 
    ///		    And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>>>>))]
    /// </example>
    /// </summary>
	public class APAddressAttribute : AddressAttribute
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/> 
        /// a source Address record from which AP address is defaulted and for selecting default version of APAddress, <br/>
        /// created  from source Address (having  matching ContactID, revision and IsDefaultContact = true) <br/>
        /// if it exists - so it must include both records. See example above. <br/>
        /// </param>
		public APAddressAttribute(Type SelectType)
			: base(typeof(APAddress.addressID), typeof(APAddress.isDefaultAddress), SelectType)
		{
		}
        
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<APAddress.overrideAddress>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<APAddress, APAddress.addressID>(sender, DocumentRow, Row);
		}

		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<APAddress, APAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}

		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<APAddress>(sender, e);
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
				PXUIFieldAttribute.SetEnabled<APAddress.overrideAddress>(sender, e.Row, sender.AllowUpdate);
				PXUIFieldAttribute.SetEnabled<APAddress.isValidated>(sender, e.Row, false);
			}			
		}
	}

    /// <summary>
    /// Specialized for AP version of the Contact attribute.<br/>
    /// Uses APContact tables for Contact versions storage <br/>
    /// Parameters ContactID, IsDefaultContact are assigned to the <br/>
    /// corresponded fields in the APContact table. <br/>
    /// Cache for APContact must be present in the graph <br/>
    /// Depends upon row instance.
    /// <example>
    /// [APContact(typeof(Select2<Location,
    ///		InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Location.bAccountID>>,
	///		InnerJoin<Contact, On<Contact.contactID, Equal<Location.remitContactID>, 
    ///		    And<Where<Contact.bAccountID, Equal<Location.bAccountID>, 
    ///		    Or<Contact.bAccountID, Equal<BAccountR.parentBAccountID>>>>>,
	///		LeftJoin<APContact, On<APContact.vendorID, Equal<Contact.bAccountID>, 
    ///		    And<APContact.vendorContactID, Equal<Contact.contactID>, 
    ///		    And<APContact.revisionID, Equal<Contact.revisionID>, 
    ///		    And<APContact.isDefaultContact, Equal<boolTrue>>>>>>>>,
	///		Where<Location.bAccountID, Equal<Current<APPayment.vendorID>>, 
    ///		And<Location.locationID, Equal<Current<APPayment.vendorLocationID>>>>>))]
    /// </example>
    /// </summary>
	public class APContactAttribute : ContactAttribute
	{
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/> 
        /// a source Contact record from which AP address is defaulted and for selecting version of APContact, <br/>
        /// created from source Contact (having  matching ContactID, revision and IsDefaultContact = true).<br/>
        /// - so it must include both records. See example above. <br/>
        /// </param>
		public APContactAttribute(Type SelectType)
			: base(typeof(APContact.contactID), typeof(APContact.isDefaultContact), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<APContact.overrideContact>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<APContact, APContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<APContact, APContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}

		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<APContact>(sender, e);
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
				PXUIFieldAttribute.SetEnabled<APContact.overrideContact>(sender, e.Row, sender.AllowUpdate);
			}
		}
	}

	/// <summary>
	/// FinPeriod selector that extends <see cref="AnyPeriodFilterableAttribute"/>. 
	/// Displays any periods (active, closed, etc), maybe filtered. 
	/// When Date is supplied through aSourceType parameter FinPeriod is defaulted with the FinPeriod for the given date.
	/// Default columns list includes 'Active' and  'Closed in GL' and 'Closed in AP'  columns
	/// </summary>
	public class APAnyPeriodFilterableAttribute : AnyPeriodFilterableAttribute
	{
		public APAnyPeriodFilterableAttribute(Type aSearchType, Type aSourceType)
			: base(aSearchType, aSourceType, typeof(FinPeriod.finPeriodID), typeof(FinPeriod.active), typeof(FinPeriod.closed), typeof(FinPeriod.aPClosed))
		{

		}

		public APAnyPeriodFilterableAttribute(Type aSourceType)
			: this(null, aSourceType)
		{

		}
		public APAnyPeriodFilterableAttribute()
			: this(null)
		{

		}
	}

	public class APAcctSubDefault
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
        /// Specialized version of the string list attribute which represents <br/>
        /// the list of the possible sources of the segments for the sub-account <br/>
        /// defaulting in the AP transactions. <br/>
        /// </summary>
		public class ClassListAttribute : CustomListAttribute
		{
			public ClassListAttribute()
				: base(new string[] { MaskLocation, MaskItem, MaskEmployee, MaskCompany, MaskProject }, new string[] { Messages.MaskLocation, Messages.MaskItem, Messages.MaskEmployee, Messages.MaskCompany, Messages.MaskProject })
			{
			}
		}
		public const string MaskLocation = "L";
		public const string MaskItem = "I";
		public const string MaskEmployee = "E";
		public const string MaskCompany = "C";
		public const string MaskProject = "P";
	}

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class SubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "APSETUP";
		public SubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, APAcctSubDefault.MaskLocation, new APAcctSubDefault.ClassListAttribute().AllowedValues, new APAcctSubDefault.ClassListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField 
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new APAcctSubDefault.ClassListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException)
			{
                // default source subID is null
                return null;

                //PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
                //string fieldName = fields[ex.SourceIdx].Name;
                //throw new PXMaskArgumentException(new APAcctSubDefault.ClassListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

    /// <summary>
    /// This attribute implements auto-generation of the next check sequential number for APPayment Document<br/>
    /// according to the settings in the CashAccount and PaymentMethod. <br/>
    /// It's also creates and inserts a related record into the CashAccountCheck table <br/>
    /// and keeps it in synch. with AP Payment (delets it if the the payment is deleted.<br/>
    /// Depends upon CashAccountID, PaymentMethodID, StubCntr fields of the row.<br/>
    /// Cache(s) for the CashAccountCheck must be present in the graph <br/>
    /// </summary>
	public class PaymentRefAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber, IPXFieldDefaultingSubscriber, IPXFieldVerifyingSubscriber 
	{
		protected Type _CashAccountID;
		protected Type _PaymentTypeID;
		protected Type _StubCntr;
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
                                                                    And<PaymentMethodAccount.useForAP,Equal<True>>>>>(sender.Graph);
                PaymentMethodAccount det = cmd.Select(CashAccountID, PaymentTypeID);
				cmd.View.Clear();

				if (det != null && det.APLastRefNbr == null)
				{
					det.APLastRefNbr = string.Empty;
					det.APLastRefNbrIsNull = true;
				}
				return det;
			}
			return null;
		}

		private void GetPaymentMethodSettings(PXCache sender, object row, out PaymentMethod aPaymentMethod, out PaymentMethodAccount aPMAccount ) 
		{
			aPaymentMethod = null;
			aPMAccount = null; 
			object CashAccountID = sender.GetValue(row, _CashAccountID.Name);
			object PaymentTypeID = sender.GetValue(row, _PaymentTypeID.Name);
			object Hold = false;
			if (_UpdateCashManager && CashAccountID != null && PaymentTypeID != null)
			{
                PXSelectBase<PaymentMethodAccount> cmd = new PXSelectReadonly2<PaymentMethodAccount, 
																	InnerJoin<PaymentMethod,On<PaymentMethod.paymentMethodID,Equal<PaymentMethodAccount.paymentMethodID>>>,
                                                                Where<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>,
                                                                    And<PaymentMethodAccount.paymentMethodID, Equal<Required<PaymentMethodAccount.paymentMethodID>>,
                                                                    And<PaymentMethodAccount.useForAP,Equal<True>>>>>(sender.Graph);
                PXResult<PaymentMethodAccount,PaymentMethod> res = (PXResult<PaymentMethodAccount,PaymentMethod>) cmd.Select(CashAccountID, PaymentTypeID);
				aPaymentMethod = res;
				cmd.View.Clear();
				PaymentMethodAccount det = res;
				if (det != null && det.APLastRefNbr == null)
				{
					det.APLastRefNbr = string.Empty;
					det.APLastRefNbrIsNull = true;
				}
				aPMAccount = det;
			}
			
		}

		void IPXFieldVerifyingSubscriber.FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
            PaymentMethodAccount det = GetCashAccountDetail(sender, e.Row);
			if (det != null && (bool)det.APAutoNextNbr)
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
								//det.APLastRefNbr = (string)e.NewValue;
                                //sender.Graph.Caches[typeof(PaymentMethodAccount)].Update(det);
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

        protected virtual void DeleteCheck(PaymentMethodAccount det, string CheckNbr)
		{
			PXDatabase.Delete<CashAccountCheck>(
				new PXDataFieldRestrict("AccountID", det.CashAccountID),
				new PXDataFieldRestrict("PaymentMethodID", det.PaymentMethodID),
				new PXDataFieldRestrict("CheckNbr", CheckNbr));
		}

        protected virtual void InsertCheck(PXCache sender, PaymentMethodAccount det, APRegister payment, string CheckNbr)
		{
			CashAccountCheck check = new CashAccountCheck();
			PXCache cache = sender.Graph.Caches[typeof(CashAccountCheck)];

			List<PXDataFieldAssign> fields = new List<PXDataFieldAssign>();

			foreach (string field in cache.Fields)
			{
				object NewValue = null;
				switch (field)
				{
					case "AccountID":
						NewValue = det.CashAccountID;
						break;
					case "PaymentMethodID":
						NewValue = det.PaymentMethodID;
						break;
					case "CheckNbr":
						NewValue = CheckNbr;
						break;
                    case "DocType":
                        NewValue = payment.DocType;
                        break;
                    case "RefNbr":
                        NewValue = payment.RefNbr;
                        break;
                    case "FinPeriodID":
                        NewValue = payment.FinPeriodID;
                        break;
                    case "DocDate":
                        NewValue = payment.DocDate;
                        break;
                    case "VendorID":
                        NewValue = payment.VendorID;
                        break;
					case "tstamp":
						continue;
					default:
						cache.RaiseFieldDefaulting(field, check, out NewValue);
						if (NewValue == null)
						{
							cache.RaiseRowInserting(check);
							NewValue = cache.GetValue(check, field);
						}
						break;
				}
				PXCommandPreparingEventArgs.FieldDescription descr;
				cache.RaiseCommandPreparing(field, check, NewValue, PXDBOperation.Insert, typeof(CashAccountCheck), out descr);

				if (descr != null && string.IsNullOrEmpty(descr.FieldName) == false)
				{
				fields.Add(new PXDataFieldAssign(descr.FieldName, descr.DataType, descr.DataLength, descr.DataValue));
			}
			}
			PXDatabase.Insert<CashAccountCheck>(fields.ToArray());
		}

		protected virtual object GetStubCntr(PXCache sender, object Row)
		{
			object StubCntr = null;

			if (_StubCntr != null)
			{
				StubCntr = sender.GetValue(Row, _StubCntr.Name);
			}

			if (StubCntr == null || (int)StubCntr == 0)
			{
				StubCntr = (int)1;
			}
			return StubCntr;
		}

		void IPXRowPersistingSubscriber.RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object SetNumber = sender.GetValue(e.Row, _FieldOrdinal);
			object StubCntr = GetStubCntr(sender, e.Row);
			bool updateNextNumber = ((Boolean?)sender.GetValue(e.Row,this._UpdateNextNumber.Name))?? false;

            PaymentMethodAccount det;
			PaymentMethod pm;
			GetPaymentMethodSettings(sender, e.Row, out pm, out det);

            if (det == null || pm == null)
            {
                return;
            }

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
                string NewNumber;
                NewNumber = AutoNumberAttribute.NextNumber(det.APLastRefNbr);

				if ((int)StubCntr < 0)
				{
					return;
				}
				if (SetNumber != null)
				{
					bool autoNumberOnPrint = (pm.APPrintChecks == true || pm.APCreateBatchPayment == true);					
					if ((bool)det.APAutoNextNbr && (object.Equals((string)SetNumber, NewNumber) || updateNextNumber))
					{
						try
						{
                            PXDatabase.Update<PaymentMethodAccount>(
								new PXDataFieldAssign("APLastRefNbr", SetNumber),
								new PXDataFieldRestrict("CashAccountID", det.CashAccountID),
								new PXDataFieldRestrict("PaymentMethodID", det.PaymentMethodID),                                
								new PXDataFieldRestrict("APLastRefNbr", det.APLastRefNbr),
								PXDataFieldRestrict.OperationSwitchAllowed);
						}
						catch (PXDatabaseException ex)
						{
							if (ex.ErrorCode == PXDbExceptions.OperationSwitchRequired)
							{
                                PXDatabase.Insert<PaymentMethodAccount>(
									new PXDataFieldAssign("CashAccountID", det.CashAccountID),
									new PXDataFieldAssign("PaymentMethodID", det.PaymentMethodID),                                    
                                    new PXDataFieldAssign("UseForAP", det.UseForAP),
                                    new PXDataFieldAssign("APLastRefNbr", NewNumber),
									new PXDataFieldAssign("APAutoNextNbr", det.APAutoNextNbr),
									new PXDataFieldAssign("APIsDefault", det.APIsDefault),
                                    new PXDataFieldAssign("UseForAR", det.UseForAR),
                                    new PXDataFieldAssign("ARIsDefault", det.ARIsDefault),
                                    new PXDataFieldAssign("ARAutoNextNbr", det.ARIsDefault));
							}
							else
							{
								throw;
							}
						}
					}
					else if ((bool)det.APAutoNextNbr == false) //SetNumber is not emptpy on print only.
					{
						NewNumber = AutoNumberAttribute.NextNumber((string) SetNumber, (int)StubCntr - 1);
						try
						{
                            PXDatabase.Update<PaymentMethodAccount>(
								new PXDataFieldAssign("APLastRefNbr", NewNumber),
								new PXDataFieldRestrict("CashAccountID", det.CashAccountID),
								new PXDataFieldRestrict("PaymentMethodID", det.PaymentMethodID),
								new PXDataFieldRestrict("APLastRefNbr", PXDbType.NVarChar, 15, det.APLastRefNbr, det.APLastRefNbrIsNull == true ? PXComp.ISNULL : PXComp.EQ),
								PXDataFieldRestrict.OperationSwitchAllowed);
						}
						catch (PXDatabaseException ex)
						{
							if (ex.ErrorCode == PXDbExceptions.OperationSwitchRequired)
							{
                                PXDatabase.Insert<PaymentMethodAccount>(
									new PXDataFieldAssign("CashAccountID", det.CashAccountID),
									new PXDataFieldAssign("PaymentMethodID", det.PaymentMethodID),
                                    new PXDataFieldAssign("UseForAP", det.UseForAP),
									new PXDataFieldAssign("APLastRefNbr", NewNumber),
									new PXDataFieldAssign("APAutoNextNbr", det.APAutoNextNbr),
									new PXDataFieldAssign("APIsDefault", det.APIsDefault),
                                    new PXDataFieldAssign("UseForAR", det.UseForAR),
                                    new PXDataFieldAssign("ARIsDefault", det.ARIsDefault),
                                    new PXDataFieldAssign("ARAutoNextNbr", det.ARIsDefault));
							}
							else
							{
								throw;
							}
						}
					}
				}
			}

			string OldNumber = GetOldField(sender, e.Row);

			if ((int)StubCntr == 1)
			{
				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update && object.Equals(OldNumber, SetNumber) == false ||
					(e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
				{
					if (string.IsNullOrEmpty(OldNumber) == false)
					{
						DeleteCheck(det, OldNumber);
					}
				}
				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update && object.Equals(OldNumber, SetNumber) == false ||
					(e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
				{
					if (string.IsNullOrEmpty((string) SetNumber) == false)
					{
						try
						{
							InsertCheck(sender, det, (APRegister)e.Row, (string)SetNumber);
						}
						catch (PXDatabaseException)
						{
							throw new PXCommandPreparingException(_FieldName, SetNumber, ErrorMessages.DuplicateEntryAdded);
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
				bool autoNumberOnPrint = (pm.APPrintChecks == true || pm.APCreateBatchPayment == true);
				if (autoNumberOnPrint == false && det.APAutoNextNbr == true)
				{
					int i = 0;
					do
					{
						try
						{
							e.NewValue = AutoNumberAttribute.NextNumber(det.APLastRefNbr, ++i);
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


		public PaymentRefAttribute(Type CashAccountID, Type PaymentTypeID, Type StubCntr, Type UpdateNextNumber)
		{
			_CashAccountID = CashAccountID;
			_PaymentTypeID = PaymentTypeID;
			_StubCntr = StubCntr;
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

            _TargetDisplayName = PXUIFieldAttribute.GetDisplayName<PaymentMethodAccount.aPLastRefNbr>(sender.Graph.Caches[typeof(PaymentMethodAccount)]);
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


    /// <summary>
    /// This attribute implements auto-generation of the next Batch sequential number for CABatch Document<br/>
    /// according to the settings in the CashAccount and PaymentMethod. <br/>
    /// Depends upon CashAccountID, PaymentMethodID fields of the row.<br/>    
    /// </summary>
    public class BatchRefAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber, IPXFieldDefaultingSubscriber, IPXFieldVerifyingSubscriber
    {
        protected Type _CashAccountID;
        protected Type _PaymentTypeID;
        protected Type _ClassType;

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
                PXSelectBase<PaymentMethodAccount> cmd = new PXSelectReadonly<PaymentMethodAccount, Where<PaymentMethodAccount.cashAccountID, Equal<Required<PaymentMethodAccount.cashAccountID>>, And<PaymentMethodAccount.paymentMethodID, Equal<Required<PaymentMethodAccount.paymentMethodID>>>>>(sender.Graph);
                PaymentMethodAccount det = cmd.Select(CashAccountID, PaymentTypeID);
                cmd.View.Clear();
                if (det != null && det.APBatchLastRefNbr == null)
                {
                    det.APBatchLastRefNbr = string.Empty;                    
                }
                return det;
            }
            return null;
        }

        void IPXFieldVerifyingSubscriber.FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
#if false
            PaymentMethodAccount det = GetCashAccountDetail(sender, e.Row);
            if (det != null)
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
                                det.LastRefNbr = (string)e.NewValue;
                                sender.Graph.Caches[typeof(PaymentMethodAccount)].Update(det);
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
#endif
        }
               
        void IPXRowPersistingSubscriber.RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            object SetNumber = sender.GetValue(e.Row, _FieldOrdinal);
            PaymentMethodAccount det = GetCashAccountDetail(sender, e.Row);

            if (det == null)
            {
                return;
            }
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                string NewNumber = AutoNumberAttribute.NextNumber(det.APBatchLastRefNbr);                
                if (SetNumber != null)
                {
                    if (object.Equals((string)SetNumber, NewNumber))
                    {
                         PXDatabase.Update<PaymentMethodAccount>(
                                new PXDataFieldAssign("APBatchLastRefNbr", NewNumber),
                                new PXDataFieldRestrict("CashAccountID", det.CashAccountID),
                                new PXDataFieldRestrict("PaymentMethodID", det.PaymentMethodID),
                                new PXDataFieldRestrict("APBatchLastRefNbr", det.APBatchLastRefNbr),
                                PXDataFieldRestrict.OperationSwitchAllowed);                        
                    }                    
                }
            }       
        }

        void IPXFieldDefaultingSubscriber.FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PaymentMethodAccount det = GetCashAccountDetail(sender, e.Row);
            if (det != null)
            {
                try
                {
                   e.NewValue = AutoNumberAttribute.NextNumber(det.APBatchLastRefNbr);
                }
                catch (Exception)
                {
                    sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<PaymentMethodAccount.aPBatchLastRefNbr>(sender.Graph.Caches[typeof(PaymentMethodAccount)])));
                }
            }
            else
            {
                e.NewValue = null;
            }
        }
        
        public BatchRefAttribute(Type CashAccountID, Type PaymentTypeID)
        {
            _CashAccountID = CashAccountID;
            _PaymentTypeID = PaymentTypeID;            
        }


        private void DefaultRef(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            sender.SetValue(e.Row, _FieldName, null);
            sender.SetDefaultExt(e.Row, _FieldName);
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            _ClassType = sender.GetItemType();           
            sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_CashAccountID), _CashAccountID.Name, DefaultRef);
            sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_PaymentTypeID), _PaymentTypeID.Name, DefaultRef);
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


    /// <summary>
    /// This is a specialized version of the Vendor attribute.<br/>
    /// Displays only Active or OneTime vendors<br/>
    /// See VendorAttribute for description. <br/>
    /// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Vendor", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<Vendor.status, Equal<BAccount.status.active>,
						Or<Vendor.status, Equal<BAccount.status.oneTime>>>), Messages.VendorIsInStatus, typeof(Vendor.status))]
	public class VendorActiveAttribute : VendorAttribute
	{
		public VendorActiveAttribute(Type search)
			:base(search)
		{
		}

		public VendorActiveAttribute()
			: base()
		{ 
		}
	}

    /// <summary>
    /// This is a specialized version of the Vendor attribute.<br/>
    /// Displays only Active or OneTime vendors, filtering out Employees<br/>
    /// See VendorAttribute for description. <br/>
    /// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Vendor", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<Vendor.type, NotEqual<BAccountType.employeeType>>), Messages.VendorIsEmployee)]
	public class VendorNonEmployeeActiveAttribute : VendorActiveAttribute
	{
		public VendorNonEmployeeActiveAttribute(Type search)
			:base(search)
		{ 
		}

		public VendorNonEmployeeActiveAttribute()
			: base()
		{
		}
	}

    /// <summary>
    /// Provides a UI Selector for Vendors.<br/>
    /// Properties of the selector - mask, length of the key, etc.<br/>
    /// are defined in the Dimension with predefined name "VENDOR".<br/>
    /// By default, search uses the following tables (linked) BAccount, Vendor (strict), Contact, Address (optional).<br/> 
    /// List of the Vendors is filtered based on the user's access rights.<br/>
    /// Default column's list in the Selector - Vendor.acctCD,Vendor.acctName,<br/>
    ///	Vendor.vendorClassID, Vendor.status, Contact.phone1, Address.city, Address.countryID<br/>
    ///	As most Dimention Selector - substitutes BAccountID by AcctCD.<br/>
    ///	List of properties - inherited from AcctSubAttribute <br/>
    ///	<example> 
    ///	[Vendor(typeof(Search<BAccountR.bAccountID,Where<BAccountR.type, Equal<BAccountType.companyType>,
	///		Or<Where<Vendor.type, NotEqual<BAccountType.employeeType>, And<Where<Vendor.status, Equal<BAccount.status.active>,
    ///	    Or<Vendor.status, Equal<BAccount.status.oneTime>>>>>>>>,
    ///	    DescriptionField = typeof(BAccount.acctName), CacheGlobal = true, Filterable = true)]
    ///	</example>
    /// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Vendor", Visibility = PXUIVisibility.Visible)]
    [Serializable]
	public class VendorAttribute : AcctSubAttribute
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

		public const string DimensionName = "VENDOR";		
			public VendorAttribute()
			: this(typeof(Search<BAccountR.bAccountID>))
		{
		}
        public VendorAttribute(Type search)
				: this(search,
					typeof(Vendor.acctCD), typeof(Vendor.acctName),
					typeof(Address.addressLine1), typeof(Address.addressLine2), typeof(Address.postalCode),
					typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID),
					typeof(Location.taxRegistrationID), typeof(Vendor.curyID),
					typeof(Contact.salutation),
					typeof(Vendor.vendorClassID), typeof(Vendor.status))
		{			
		}

        /// <summary>
        /// </summary>
        /// <param name="search">Defines BQL Select. Must be a type, compatible with BQL Search<></param>
        /// <param name="fields">List of the fields to display in the selector. Must be PX.Data.IBqlField</param>
        public VendorAttribute(Type search, params Type[] fields)
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
								typeof(Vendor),
								typeof(On<Vendor.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Vendor, Current<AccessInfo.userName>>>>),
								typeof(LeftJoin<,,>),
								typeof(Contact),
								typeof(On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>),
								typeof(LeftJoin<,,>),
								typeof(Address),
								typeof(On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>),
								typeof(LeftJoin<,>),
								typeof(Location),
								typeof(On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>),
								typeof(Where<Vendor.bAccountID, IsNotNull>));
			}
			else if (searchType == typeof(Search<,>))
			{
				cmd = BqlCommand.Compose(
								typeof(Search2<,,>),
								typeof(BAccountR.bAccountID),
								typeof(LeftJoin<,,>),
								typeof(Vendor),
								typeof(On<Vendor.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Vendor, Current<AccessInfo.userName>>>>),
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
								typeof(Vendor),
								typeof(On<Vendor.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Vendor, Current<AccessInfo.userName>>>>),
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
								typeof(Vendor),
								typeof(On<Vendor.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Vendor, Current<AccessInfo.userName>>>>),
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
								typeof(Where<Vendor.bAccountID, IsNotNull>));
			}
			else if (searchType == typeof(Search2<,,>))
			{
				cmd = BqlCommand.Compose(
								typeof(Search2<,,>),
								typeof(BAccountR.bAccountID),
								typeof(LeftJoin<,,>),
								typeof(Vendor),
								typeof(On<Vendor.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Vendor, Current<AccessInfo.userName>>>>),
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
								typeof(Vendor),
								typeof(On<Vendor.bAccountID, Equal<BAccountR.bAccountID>, And<Match<Vendor, Current<AccessInfo.userName>>>>),
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
			_fields = fields;
			PXDimensionSelectorAttribute attr;
			_Attributes.Add(attr = new PXDimensionSelectorAttribute(DimensionName, cmd, typeof(BAccountR.acctCD),
				fields));			
			attr.DescriptionField = typeof(Vendor.acctName);
			attr.CacheGlobal = true;
			_SelAttrIndex = _Attributes.Count - 1;
			this.Filterable = true;
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
			EmitColumnForVendorField(sender);
		}

		protected void EmitColumnForVendorField(PXCache sender)
		{
			if (this.DescriptionField  == null) 
				return;			

			{
				string alias = _FieldName + "_" + typeof(Vendor).Name + "_" + DescriptionField.Name;
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

    /// <summary>
    /// Provides a UI Selector for Vendors AcctCD as a string. <br/>
    /// Should be used where the definition of the AccountCD is needed - mostly, in a Vendor DAC class.<br/>
    /// Properties of the selector - mask, length of the key, etc.<br/>
    /// are defined in the Dimension with predefined name "VENDOR".<br/>
    /// By default, search uses the following tables (linked) BAccount, Vendor (strict), Contact, Address (optional).<br/> 
    /// List of the Vendors is filtered based on the user's access rights.<br/>
    /// Default column's list in the Selector - Vendor.acctCD,Vendor.acctName,<br/>
    ///	Vendor.vendorClassID, Vendor.status, Contact.phone1, Address.city, Address.countryID<br/>    
    ///	List of properties - inherited from AcctSubAttribute <br/>
    ///	<example> 
    ///	[VendorRaw(typeof(Where<Vendor.type, Equal<BAccountType.vendorType>,
	///   Or<Vendor.type, Equal<BAccountType.combinedType>>>), DescriptionField = typeof(Vendor.acctName), IsKey = true, DisplayName = "Vendor ID")]
    ///	</example>
    /// </summary>    
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Vendor", Visibility = PXUIVisibility.Visible)]
	public sealed class VendorRawAttribute : AcctSubAttribute
	{
		public const string DimensionName = "VENDOR";		
		public VendorRawAttribute() : this(null) {}
        /// <summary>
        /// </summary>
        /// <param name="where">Defines a where clause for the BQL Select. Must be compatible with BQL Where<></param>
		public VendorRawAttribute(Type where) 
		{			
			Type SearchType = BqlCommand.Compose(
				typeof (Search2<,,>),
				typeof (Vendor.acctCD),
				typeof (
					LeftJoin<Contact, On<Contact.bAccountID, Equal<Vendor.bAccountID>, And<Contact.contactID, Equal<Vendor.defContactID>>>,
					LeftJoin<Address, On<Address.bAccountID, Equal<Vendor.bAccountID>, And<Address.addressID, Equal<Vendor.defAddressID>>>>>),
				where == null
					? typeof (Where<Match<Current<AccessInfo.userName>>>)
					: BqlCommand.Compose(
						typeof (Where2<,>),
						typeof (Where<Match<Current<AccessInfo.userName>>>),
						typeof (And<>),
						where));			
				
			PXDimensionSelectorAttribute attr;
			_Attributes.Add(attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(Vendor.acctCD),
					typeof(Vendor.acctCD), typeof(Vendor.acctName), typeof(Vendor.vendorClassID), typeof(Vendor.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID)
				));
			attr.DescriptionField = typeof(Vendor.acctName);
			_SelAttrIndex = _Attributes.Count - 1;
			this.Filterable = true;
			((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).CacheGlobal = true;
		}
	}

    /// <summary>
    /// Specialized for APQuickCheck version of the APTaxAttribute(override).<br/>
    /// Provides Tax calculation for APTran, by default is attached to APTran (details) and APQuickCheck (header) <br/>
    /// Normally, should be placed on the TaxCategoryID field. <br/>
    /// Internally, it uses APQuickCheckEntry graph, otherwise taxes are not calculated (tax calc Level is set to NoCalc).<br/>
    /// As a result of this attribute work a set of APTax tran related to each AP Tran and to their parent will created <br/>
    /// May be combined with other attrbibutes with similar type - for example, APTaxAttribute <br/>
    /// <example>
    ///[APQuickCheckTax(typeof(Standalone.APQuickCheck), typeof(APTax), typeof(APTaxTran))]
    /// </example>    
    /// </summary>
	public class APQuickCheckTaxAttribute : APTaxAttribute
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ParentType">Type of parent(main) document. Must Be IBqlTable </param>
        /// <param name="TaxType">Type of the TaxTran records for the row(details). Must be IBqlTable</param>
        /// <param name="TaxSumType">Type of the TaxTran recorde for the main documect (summary). Must be iBqlTable</param>		
		public APQuickCheckTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			:base(ParentType, TaxType, TaxSumType)
		{
            DocDate = typeof(APQuickCheck.adjDate);
            FinPeriodID = typeof(APQuickCheck.adjFinPeriodID);
			CuryLineTotal = typeof(APQuickCheck.curyLineTotal);
			CuryTranAmt = typeof(APTran.curyTranAmt);

			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(APTran.curyTranAmt), typeof(SumCalc<APQuickCheck.curyLineTotal>)));
		}

		protected override void ParentFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APQuickCheck doc;
			if (e.Row is APQuickCheck && ((APQuickCheck)e.Row).DocType != APDocType.VoidQuickCheck)
			{
				base.ParentFieldUpdated(sender, e);
			}
			else if (e.Row is CurrencyInfo && (doc = PXSelect<APQuickCheck, Where<APQuickCheck.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(sender.Graph, ((CurrencyInfo)e.Row).CuryInfoID)) != null && doc.DocType != APDocType.VoidQuickCheck)
			{
				base.ParentFieldUpdated(sender, e);
			}
		}

		protected override void ZoneUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (((APQuickCheck)e.Row).DocType != APDocType.VoidQuickCheck)
			{
				base.ZoneUpdated(sender, e);
			}
		}

		protected override void DateUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (((APQuickCheck)e.Row).DocType != APDocType.VoidQuickCheck)
			{
				base.DateUpdated(sender, e);
			}
		}
	}

    /// <summary>
    /// Specialized for AP version of the TaxAttribute. <br/>
    /// Provides Tax calculation for APTran, by default is attached to APTran (details) and APInvoice (header) <br/>
    /// Normally, should be placed on the TaxCategoryID field. <br/>
    /// Internally, it uses APInvoiceEntry graphs, otherwise taxes are not calculated (tax calc Level is set to NoCalc).<br/>
    /// As a result of this attribute work a set of APTax tran related to each AP Tran  and to their parent will created <br/>
    /// May be combined with other attrbibutes with similar type - for example, APTaxAttribute <br/>
    /// <example>
    /// [APTax(typeof(APRegister), typeof(APTax), typeof(APTaxTran))]
    /// </example>    
    /// </summary>
	public class APTaxAttribute : TaxAttribute
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ParentType">Type of parent(main) document. Must Be IBqlTable </param>
        /// <param name="TaxType">Type of the TaxTran records for the row(details). Must be IBqlTable</param>
        /// <param name="TaxSumType">Type of the TaxTran recorde for the main documect (summary). Must be iBqlTable</param>
		public APTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			:base(ParentType, TaxType, TaxSumType)
		{
            CuryTranAmt = typeof(APTran.curyTranAmt);
            GroupDiscountRate = typeof(APTran.groupDiscountRate);
			CuryLineTotal = typeof(APInvoice.curyLineTotal);

            this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<APTran.lineType, NotEqual<SO.SOLineType.discount>>, APTran.curyTranAmt>, decimal0>), typeof(SumCalc<APInvoice.curyLineTotal>)));
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
            sender.SetValue<APTran.curyTaxableAmt>(row, value);
        }

        protected override void SetTaxAmt(PXCache sender, object row, decimal? value)
        {
            sender.SetValue<APTran.curyTaxAmt>(row, value);
        }

        protected override void ParentSetValue(PXGraph graph, string fieldname, object value)
        {
            PXCache cache = graph.Caches[typeof(APSetup)];
            if (fieldname == _CuryDocBal && cache.Current != null && ((APSetup)cache.Current).InvoiceRounding != RoundingType.Currency)
            {
                decimal? oldval = (decimal?)value;
                value = APReleaseProcess.RoundAmount((decimal?)value, ((APSetup)cache.Current).InvoiceRounding, ((APSetup)cache.Current).InvoicePrecision);

                decimal oldbaseval;
                decimal baseval;
                PXDBCurrencyAttribute.CuryConvBase(ParentCache(graph), ParentRow(graph), (decimal)oldval, out oldbaseval);
                PXDBCurrencyAttribute.CuryConvBase(ParentCache(graph), ParentRow(graph), (decimal)value, out baseval);

                oldbaseval -= baseval;
                oldval -= (decimal?)value;

                base.ParentSetValue(graph, typeof(APRegister.curyRoundDiff).Name, oldval);
                base.ParentSetValue(graph, typeof(APRegister.roundDiff).Name, oldbaseval);
            }
            else
            {
                base.ParentSetValue(graph, typeof(APRegister.curyRoundDiff).Name, 0m);
                base.ParentSetValue(graph, typeof(APRegister.roundDiff).Name, 0m);
            }

            base.ParentSetValue(graph, fieldname, value);
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
					And<TaxRev.outdated, Equal<False>,
					And2<Where<TaxRev.taxType, Equal<TaxType.purchase>, And<Tax.reverseTax, Equal<False>,
						Or<TaxRev.taxType, Equal<TaxType.sales>, And<Where<Tax.reverseTax, Equal<True>,
						Or<Tax.taxType, Equal<CSTaxType.use>, 
						Or<Tax.taxType, Equal<CSTaxType.withholding>>>>>>>>,
					And<Current<APRegister.docDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>,
				Where>
				.SelectMultiBound(graph, new object[] { row }, parameters))
			{
				tail[((Tax)record).TaxID] = record;
			}
			List<object> ret = new List<object>();
			switch (taxchk)
			{
				case PXTaxCheck.Line:
					foreach (APTax record in PXSelect<APTax,
						Where<APTax.tranType, Equal<Current<APTran.tranType>>,
							And<APTax.refNbr, Equal<Current<APTran.refNbr>>,
							And<APTax.lineNbr, Equal<Current<APTran.lineNbr>>>>>>
						.SelectMultiBound(graph, new object[] { row }))
			{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
			{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& String.Compare(((Tax)(PXResult<APTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<APTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
			}
		}
					return ret;
				case PXTaxCheck.RecalcLine:
					foreach (APTax record in PXSelect<APTax,
						Where<APTax.tranType, Equal<Current<APRegister.docType>>,
							And<APTax.refNbr, Equal<Current<APRegister.refNbr>>>>>
						.SelectMultiBound(graph, new object[] { row }))
		{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
			{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& ((APTax)(PXResult<APTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
								&& String.Compare(((Tax)(PXResult<APTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<APTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
				return ret;
				case PXTaxCheck.RecalcTotals:
					foreach (APTaxTran record in PXSelect<APTaxTran,
						Where<APTaxTran.module, Equal<BatchModule.moduleAP>,
							And<APTaxTran.tranType, Equal<Current<APRegister.docType>>,
							And<APTaxTran.refNbr, Equal<Current<APRegister.refNbr>>>>>>
						.SelectMultiBound(graph, new object[] { row }))
					{
						PXResult<Tax, TaxRev> line;
						if (record.TaxID != null && tail.TryGetValue(record.TaxID, out line))
			{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& String.Compare(((Tax)(PXResult<APTaxTran, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<APTaxTran, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
			}
		}
					return ret;
				default:
					return ret;
			}
		}

		protected override void CalcDocTotals(PXCache sender, object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
		{
			base.CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);

			if (ParentGetStatus(sender.Graph) != PXEntryStatus.Deleted)
			{
				decimal doc_CuryWhTaxTotal = (decimal)(ParentGetValue(sender.Graph, _CuryWhTaxTotal) ?? 0m);

				if (object.Equals(CuryWhTaxTotal, doc_CuryWhTaxTotal) == false)
				{
					ParentSetValue(sender.Graph, _CuryWhTaxTotal, CuryWhTaxTotal);
				}
			}
		}

        protected override void _CalcDocTotals(PXCache sender, object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
        {
            decimal CuryDiscountTotal = (decimal)(ParentGetValue<APRegister.curyDiscTot>(sender.Graph) ?? 0m);

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

        protected override void AdjustTaxableAmount(PXCache sender, object row, List<object> taxitems, ref decimal CuryTaxableAmt)
        {
            decimal CuryLineTotal = (decimal?)ParentGetValue<APInvoice.curyLineTotal>(sender.Graph) ?? 0m;
            decimal CuryDiscountTotal = (decimal)(ParentGetValue<APInvoice.curyDiscTot>(sender.Graph) ?? 0m);

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

	}

	public class APQuickCheckCashTranIDAttribute : CashTranIDAttribute
	{
		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			//will be null for DebitAdj and Prepayment request
			APQuickCheck parentDoc = (APQuickCheck)orig_Row;
			if (parentDoc.CashAccountID == null ||
				(parentDoc.Released == true) && (catran_Row.TranID != null) ||
				 parentDoc.CuryOrigDocAmt == null ||
				 parentDoc.CuryOrigDocAmt == 0)
			{
				return null;
			}
			catran_Row.OrigModule = BatchModule.AP;
			catran_Row.OrigTranType = parentDoc.DocType;
			catran_Row.OrigRefNbr = parentDoc.RefNbr;
			catran_Row.ExtRefNbr = parentDoc.ExtRefNbr;
			catran_Row.CashAccountID = parentDoc.CashAccountID;
			catran_Row.CuryInfoID = parentDoc.CuryInfoID;
			catran_Row.CuryID = parentDoc.CuryID;
            string voidedType = string.Empty;
			switch (parentDoc.DocType)
			{
				case APDocType.QuickCheck:
					catran_Row.CuryTranAmt = -parentDoc.CuryOrigDocAmt;
					catran_Row.DrCr = "C";
					break;
				case APDocType.VoidQuickCheck:
					catran_Row.CuryTranAmt = parentDoc.CuryOrigDocAmt;
					catran_Row.DrCr = "D";
                    voidedType = APDocType.QuickCheck;
					break;
				default:
					throw new PXException();
			}

			catran_Row.TranDate = parentDoc.DocDate;
			catran_Row.TranDesc = parentDoc.DocDesc;
			catran_Row.FinPeriodID = parentDoc.FinPeriodID;
			catran_Row.ReferenceID = parentDoc.VendorID;
			catran_Row.Released = parentDoc.Released;
			catran_Row.Hold = parentDoc.Hold;
			catran_Row.Cleared = parentDoc.Cleared;
			catran_Row.ClearDate = parentDoc.ClearDate;

            if (!string.IsNullOrEmpty(voidedType))
            {
                APPayment voidedDoc = PXSelectReadonly<APPayment, Where<APPayment.refNbr, Equal<Required<APPayment.refNbr>>,
                                                And<APPayment.docType, Equal<Required<APPayment.docType>>>>>.Select(sender.Graph, parentDoc.RefNbr, voidedType);
                if (voidedDoc != null)
                {
                    catran_Row.VoidedTranID = voidedDoc.CATranID;
                }
            }

			PXSelectBase<CashAccount> selectStatement = new PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>(sender.Graph);
			CashAccount cashacc						  = (CashAccount) selectStatement.View.SelectSingle(catran_Row.CashAccountID);
			if (cashacc != null && cashacc.Reconcile == false && (catran_Row.Cleared != true || catran_Row.TranDate == null))
			{
				catran_Row.Cleared   = true;
				catran_Row.ClearDate = catran_Row.TranDate;
			}

			return catran_Row;
		}
	}

    /// <summary>
    /// Specialized for the APPayment version of the <see cref="CashTranIDAttribute"/><br/>
    /// Since CATran created from the source row, it may be used only the fields <br/>
    /// of APPayment compatible DAC. <br/>
    /// The main purpuse of the attribute - to create CATran <br/>
    /// for the source row and provide CATran and source synchronization on persisting.<br/>
    /// CATran cache must exists in the calling Graph.<br/>
    /// </summary>
	public class APCashTranIDAttribute : CashTranIDAttribute
	{
		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			//will be null for DebitAdj and Prepayment request
            APPayment parentDoc = (APPayment)orig_Row;
			if ( parentDoc.CashAccountID == null ||
				(parentDoc.Released == true) && (catran_Row.TranID != null) ||
				 parentDoc.CuryOrigDocAmt == null || 
				 parentDoc.CuryOrigDocAmt == 0)
			{
				return null;
			}
			catran_Row.OrigModule    = BatchModule.AP;
			catran_Row.OrigTranType  = parentDoc.DocType;
			catran_Row.OrigRefNbr    = parentDoc.RefNbr;
			catran_Row.ExtRefNbr     = parentDoc.ExtRefNbr;
			catran_Row.CashAccountID = parentDoc.CashAccountID;
			catran_Row.CuryInfoID    = parentDoc.CuryInfoID;
			catran_Row.CuryID		 = parentDoc.CuryID;
            string voidedType = string.Empty;
            switch (parentDoc.DocType)
			{
				case APDocType.Check:
				case APDocType.Prepayment:
				case APDocType.QuickCheck:
					catran_Row.CuryTranAmt = -parentDoc.CuryOrigDocAmt;
					catran_Row.DrCr = "C";
					break;
				case APDocType.VoidCheck:
					catran_Row.CuryTranAmt = -parentDoc.CuryOrigDocAmt;
					catran_Row.DrCr = "D";
                    voidedType = APDocType.Check;
					break;
				case APDocType.Refund:
				case APDocType.VoidQuickCheck:
					catran_Row.CuryTranAmt = parentDoc.CuryOrigDocAmt;
					catran_Row.DrCr = "D";
                    if(parentDoc.DocType == APDocType.VoidQuickCheck) 
                        voidedType = APDocType.QuickCheck;
					break;
				default:
					throw new PXException();
			}

			catran_Row.TranDate      = parentDoc.DocDate;
			catran_Row.TranDesc      = parentDoc.DocDesc;
			catran_Row.FinPeriodID   = parentDoc.FinPeriodID;
			catran_Row.ReferenceID   = parentDoc.VendorID;
			catran_Row.Released      = parentDoc.Released;
			catran_Row.Hold          = parentDoc.Hold;
			catran_Row.Cleared       = parentDoc.Cleared;
            catran_Row.ClearDate     = parentDoc.ClearDate;
			//This coping is required in one specfic case - when payment reclassification is made
			if (parentDoc.CARefTranID.HasValue)
			{
				catran_Row.RefTranAccountID = parentDoc.CARefTranAccountID;
				catran_Row.RefTranID = parentDoc.CARefTranID;
                catran_Row.RefSplitLineNbr = parentDoc.CARefSplitLineNbr;
			}

            if (!string.IsNullOrEmpty(voidedType))
            {
                APPayment voidedDoc = PXSelectReadonly<APPayment, Where<APPayment.refNbr, Equal<Required<APPayment.refNbr>>,
                                                And<APPayment.docType, Equal<Required<APPayment.docType>>>>>.Select(sender.Graph, parentDoc.RefNbr, voidedType);
                if (voidedDoc != null)
                {
                    catran_Row.VoidedTranID = voidedDoc.CATranID;
                }
            }

			PXSelectBase<CashAccount> selectStatement = new PXSelectReadonly<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>(sender.Graph);
			CashAccount cashacc						  = (CashAccount) selectStatement.View.SelectSingle(catran_Row.CashAccountID);
			if (cashacc != null && cashacc.Reconcile == false && (catran_Row.Cleared != true || catran_Row.TranDate == null))
			{
				catran_Row.Cleared   = true;
				catran_Row.ClearDate = catran_Row.TranDate;
			}

			return catran_Row;
		}
	}

	public static class LangEN
	{
		public static string ToWords(Decimal amt, int Precision)
		{
			StringBuilder sb = new StringBuilder(ToWords(amt));
			Decimal Cents = Math.Floor((amt - Math.Truncate(amt)) * (decimal)Math.Pow(10, Precision));

			if (amt != 0m)
			{
				if (Cents != 0m)
				{
					sb.Append(" and ");
					sb.Append((int)Cents);
					sb.Append("/");
					sb.Append((int)Math.Pow(10, Precision));
				}
				else
				{
					sb.Append(" Only");
				}
			}
			else
			{
				sb.Append("Zero");
			}
			return sb.ToString();
		}

		public static string ToWords(Decimal? amt)
		{
			Decimal baseamt = Math.Floor((Decimal)amt);

			string[] less10 = new string[] { "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
			string[] less20 = new string[] { "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
			string[] less100 = new string[] { "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
			string[] great100 = new string[] { "", "Thousand", "Million", "Billion" };
			string is100 = "Hundred";
			string space = " ";

			int count = (int)Math.Floor(Math.Log10((double)baseamt));
			int tricount = (int)Math.Floor((double)count / 3) + 1;

			StringBuilder sb = new StringBuilder();

			for (int i = tricount; i > 0; i--)
			{
				int prevlen = sb.Length;
				int triamt = (int)Math.Floor((double)baseamt / Math.Pow(10, (i - 1) * 3));
				{
					if (triamt >= 100)
					{
						int h = (int)Math.Floor((double)triamt / 100);
						sb.Append(less10[h - 1]);
						sb.Append(space);
						sb.Append(is100);
						sb.Append(space);
						triamt = triamt - 100 * h;
						//Six Hundred
					}
					if (triamt >= 20 || triamt == 10)
					{
						int h = (int)Math.Floor((double)triamt / 10);
						sb.Append(less100[h - 1]);
						sb.Append(space);
						triamt = triamt - 10 * h;
						//Six Hundred Twenty
					}
					if (triamt < 20 && triamt > 10)
					{
						sb.Append(less20[triamt - 10 - 1]);
						sb.Append(space);
						//Six Hundred Eleven
					}
					if (triamt > 0 && triamt < 10)
					{
						sb.Append(less10[triamt - 1]);
						sb.Append(space);
						//Six Hundred Twenty One
					}
				}

				if (sb.Length > prevlen)
				{
					sb.Append(great100[i - 1]);
					sb.Append(space);
				}

				int newbase;
				Math.DivRem((int)baseamt, (int)Math.Pow(10, (i - 1) * 3), out newbase);

				baseamt = (Decimal)newbase;
			}

			return sb.ToString().TrimEnd();
		}
	}
    /// <summary>
    /// Converts Decimal value to it's word representation (English only) one way only<br/>
    /// For example, 1921.14 would be converted to "One thousand nine hundred twenty one and fourteen".<br/>
    /// Should be placed on the string field   
    /// <example>
    /// [ToWords(typeof(APPayment.curyOrigDocAmt))]
    /// </example>
    /// </summary>
	public class ToWordsAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		protected string _DecimalField = null;
		protected short? _Precision = null;
		public ToWordsAttribute(Type DecimalField)
		{
			_DecimalField = DecimalField.Name;
		}

		public ToWordsAttribute(short Precision)
		{
			_Precision = Precision; ;
		}

		public virtual void  FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 255, null, _FieldName, null, null, null, null, null, false, null);

			object DecimalVal;
			if (!string.IsNullOrEmpty(_DecimalField))
			{
				DecimalVal = sender.GetValue(e.Row, _DecimalField);
				sender.RaiseFieldSelecting(_DecimalField, e.Row, ref DecimalVal, true);
			}
			else
			{
				DecimalVal = PXDecimalState.CreateInstance(e.ReturnValue, (short)_Precision, _FieldName, false, 0, Decimal.MinValue, Decimal.MaxValue);
			}

			if (DecimalVal is PXDecimalState)
			{
				if (((PXDecimalState)DecimalVal).Value == null)
				{
					e.ReturnValue = string.Empty;
					return;
				}

				e.ReturnValue = LangEN.ToWords((decimal)((PXDecimalState)DecimalVal).Value, ((PXDecimalState)DecimalVal).Precision);
			}
		}
	}

    /// <summary>
    /// Specialized version of the selector for AP Open Financial Periods.<br/>
    /// Displays a list of FinPeriods, having flags Active = true and  APClosed = false.<br/>
    /// </summary>
	public class APOpenPeriodAttribute : OpenPeriodAttribute
	{
		#region Ctor
		public APOpenPeriodAttribute(Type SourceType)
			: base(typeof(Search<FinPeriod.finPeriodID, Where<FinPeriod.aPClosed, Equal<False>, And<FinPeriod.active, Equal<True>>>>), SourceType)
		{
		}

		public APOpenPeriodAttribute()
			: this(null)
		{
		}
		#endregion
		
		#region Implementation
		public override void IsValidPeriod(PXCache sender, object Row, object NewValue)
		{
			string OldValue = (string)sender.GetValue(Row, _FieldName);
			base.IsValidPeriod(sender, Row, NewValue);			

			if (NewValue != null && _ValidatePeriod != PeriodValidation.Nothing)
			{
				FinPeriod p = (FinPeriod)PXSelect<FinPeriod, Where<FinPeriod.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>>>.Select(sender.Graph, (string)NewValue);
				if (p.APClosed == true)
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
		}
		#endregion
	}

	/// <summary>
	/// FinPeriod selector that extends <see cref="FinPeriodSelectorAttribute"/>. 
	/// Displays and accepts only Closed Fin Periods. 
	/// When Date is supplied through aSourceType parameter FinPeriod is defaulted with the FinPeriod for the given date.
	/// </summary>
	public class APClosedPeriodAttribute : FinPeriodSelectorAttribute
	{
		public APClosedPeriodAttribute(Type aSourceType)
            : base(typeof(Search<FinPeriod.finPeriodID, Where<FinPeriod.closed, Equal<True>, Or<FinPeriod.aPClosed, Equal<True>, Or<FinPeriod.active, Equal<True>>>>, OrderBy<Desc<FinPeriod.finPeriodID>>>), aSourceType)
        {
		}

		public APClosedPeriodAttribute()
			: this(null)
		{

		}
	}

	#region accountsPayableModule

	public sealed class accountsPayableModule : Constant<string>
	{
		public accountsPayableModule() : base(typeof(accountsPayableModule).Namespace) { }
	}

	#endregion
	#region vendorType
	public sealed class vendorType : Constant<string>
	{
		public vendorType()
			: base(typeof(PX.Objects.AP.Vendor).FullName)
		{
		}
	}
	#endregion

	#region PXDBVendorCuryAttribute	
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBVendorCuryAttribute : PXDBDecimalAttribute
	{
		public PXDBVendorCuryAttribute(Type vendorID)
			: base(BqlCommand.Compose(typeof(Search2<,,>), typeof(Vendor.taxReportPrecision),
				typeof(CrossJoin<Company,
						   LeftJoin<Currency, On<Currency.curyID, Equal<Vendor.curyID>>,
							 LeftJoin<Currency2, On<Currency2.curyID, Equal<Company.baseCuryID>>>>>),
				typeof(Where<,>), typeof(Vendor.bAccountID), typeof(Equal<>), typeof(Current<>), vendorID))
		{			
		}

		protected override int? GetItemPrecision(PXView view, object item)
		{
			var result = item as PX.Data.PXResult<Vendor, Company, Currency, Currency2>;
			if (result == null) return null;

			PXCache vendor = view.Graph.Caches[typeof (Vendor)];
			bool? useVendorCur = (bool?)vendor.GetValue<Vendor.taxUseVendorCurPrecision>((Vendor)result);
			if(useVendorCur == true)
			{
				return
					(short?)view.Graph.Caches[typeof(Currency)].GetValue<Currency.decimalPlaces>((Currency)result) ??
					(short?)view.Graph.Caches[typeof(Currency2)].GetValue<Currency2.decimalPlaces>((Currency2)result);
			}
			return (short?)vendor.GetValue<Vendor.taxReportPrecision>((Vendor)result);
		}

        public override void CacheAttached(PXCache sender)
        {
            sender.SetAltered(_FieldName, true);
            base.CacheAttached(sender);
        }


	}
	#endregion

    public class APPaymentChargeCashTranIDAttribute : CashTranIDAttribute
    {
        protected bool _IsIntegrityCheck = false;

        public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
        {
            APPaymentChargeTran parentDoc = (APPaymentChargeTran)orig_Row;
            if (parentDoc.Released == true || parentDoc.CuryTranAmt == null || parentDoc.CuryTranAmt == 0m)
            {
                return null;
            }
            catran_Row.OrigModule = BatchModule.AP;
            catran_Row.OrigTranType = parentDoc.DocType;
            catran_Row.OrigRefNbr = parentDoc.RefNbr;
            catran_Row.CashAccountID = parentDoc.CashAccountID;
            catran_Row.CuryInfoID = parentDoc.CuryInfoID;
            catran_Row.CuryTranAmt = -parentDoc.CuryTranAmt;
            catran_Row.ExtRefNbr = parentDoc.ExtRefNbr;
            catran_Row.DrCr = parentDoc.DrCr;
            catran_Row.FinPeriodID = parentDoc.FinPeriodID;
            APRegister register = PXSelect<APRegister, Where<APRegister.docType, Equal<Required<APPaymentChargeTran.docType>>,
                                                                            And<APRegister.refNbr, Equal<Required<APPaymentChargeTran.refNbr>>>>>.Select(sender.Graph, parentDoc.DocType, parentDoc.RefNbr);
            catran_Row.ReferenceID = register.VendorID;
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
                if (attr is APPaymentChargeCashTranIDAttribute)
                {
                    ((APPaymentChargeCashTranIDAttribute)attr)._IsIntegrityCheck = true;
                    return ((APPaymentChargeCashTranIDAttribute)attr).DefaultValues(sender, new CATran(), data);
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
}

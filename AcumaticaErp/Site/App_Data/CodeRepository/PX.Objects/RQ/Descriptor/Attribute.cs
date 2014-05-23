using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.TX;
using PX.Objects.CS;
using PX.Objects.IN;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using PX.Objects.AR;
using System.Collections;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.SM;
using PX.TM;
using PX.Objects.GL;


namespace PX.Objects.RQ
{
	/// <summary>
	/// Selector.<br/>
	/// Show inventory items available for using in requisition.<br/>
	/// Hide Inventory Items witch restricted for current logged in user, <br/> 
	/// With status inactive, marked for deletion, no purchases and no request. <br/>
	/// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]	
	public class RQRequisitionInventoryItemAttribute : CrossItemAttribute
	{
		public RQRequisitionInventoryItemAttribute()
			: base(
			typeof(Where2<Match<Current<AccessInfo.userName>>,
						 And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
								And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>,
								And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noPurchases>,
								And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noRequest>>>>>>),
					INPrimaryAlternateType.VPN)
		{			
		}	
	}
	/// <summary>
	/// Selector.<br/>
	/// Show inventory items available for using in request.<br/>
	/// Hide Inventory Items witch restricted for current logged in user, <br/> 
	/// With status inactive, marked for deletion, no purchases and no request,
	/// Restricted for request class.<br/>
	/// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory", Visibility = PXUIVisibility.Visible)]
	public class RQRequestInventoryItemAttribute : CrossItemAttribute
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="classID">Request class field.</param>
		public RQRequestInventoryItemAttribute(Type classID)
			: base(
			BqlCommand.Compose(typeof(LeftJoin<,,>),
				typeof(RQRequestClass),
					typeof(On<,>), typeof(RQRequestClass.reqClassID), typeof(Equal<>), typeof(Current<>), classID,
				typeof(LeftJoin<RQRequestClassItem,
							 On<RQRequestClassItem.inventoryID, Equal<InventoryItem.inventoryID>,
							And<RQRequestClassItem.reqClassID, Equal<RQRequestClass.reqClassID>,
							And<RQRequestClass.restrictItemList, Equal<BQLConstants.BitOn>>>>>)),
			typeof(
			Where2<Match<Current<AccessInfo.userName>>,
						 And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
						 And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>,
						 And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noPurchases>,
						 And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noRequest>,
						 And<Where<RQRequestClass.restrictItemList, Equal<BQLConstants.BitOff>, 
			        Or<RQRequestClassItem.inventoryID, IsNotNull,
							Or<RQRequestClass.reqClassID, IsNull>>>>>>>>>),				
					INPrimaryAlternateType.VPN)
		{			
		}
	}

	/// <summary>
	/// Selector.<br/>
	/// Show list of employees or list of customers according to definition of request class.
	/// </summary>
	public class RQRequesterSelectorAttribute : PXDimensionSelectorAttribute
	{
		/// <summary>
		/// Internal selector used only for dimension.
		/// </summary>
		public class CustomSelector : PXCustomSelectorAttribute
		{
			protected Type reqClassID;
			protected PXView viewEmployees;
			protected PXView viewCustomers;
			protected PXView viewClass;

			public CustomSelector(Type reqClassID)
				: base(
				typeof(Search2<BAccountR.bAccountID,
				LeftJoin<Contact, On<Contact.bAccountID, Equal<BAccountR.bAccountID>,
						 And<Contact.contactID, Equal<BAccount.defContactID>>>,
				LeftJoin<Address, On<Address.bAccountID, Equal<BAccountR.bAccountID>,
						 And<Address.addressID, Equal<BAccount.defAddressID>>>>>>),				
				typeof(BAccountR.acctCD),
				typeof(BAccountR.acctName),
				typeof(BAccountR.status),
				typeof(Contact.phone1),
				typeof(Address.city),
				typeof(Address.countryID))
			{
				this.DescriptionField = typeof(BAccountR.acctName);
				this.reqClassID = reqClassID;
				base._ViewName = "_Requester_";
			}
			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);				
				this.viewEmployees = new PXView(sender.Graph, true,
					BqlCommand.CreateInstance(
					typeof(Search5<BAccountR.bAccountID,
						InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<BAccountR.bAccountID>>,
						LeftJoin<Contact, On<Contact.bAccountID, Equal<EPEmployee.parentBAccountID>,
						 And<Contact.contactID, Equal<EPEmployee.defContactID>>>,
						LeftJoin<Address, On<Address.bAccountID, Equal<EPEmployee.parentBAccountID>,
						 And<Address.addressID, Equal<EPEmployee.defAddressID>>>,
						LeftJoin<Users, On<Users.pKID, Equal<EPEmployee.userID>>,
						LeftJoin<EPCompanyTreeMember, On<EPCompanyTreeMember.userID, Equal<EPEmployee.userID>>>>>>>,
						Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
						Or<EPCompanyTreeMember.workGroupID, Owned<Current<AccessInfo.userID>>>>,
						Aggregate<GroupBy<BAccountR.bAccountID>>>)));

				this.viewCustomers = new PXView(sender.Graph, true,
					BqlCommand.CreateInstance(typeof(Search2<BAccountR.bAccountID,
							InnerJoin<Customer, On<Customer.bAccountID, Equal<BAccountR.bAccountID>>,
							LeftJoin<Contact, On<Contact.bAccountID, Equal<Customer.bAccountID>,
									 And<Contact.contactID, Equal<Customer.defContactID>>>,
							LeftJoin<Address, On<Address.bAccountID, Equal<Customer.bAccountID>,
									 And<Address.addressID, Equal<Customer.defAddressID>>>>>>,
							Where<Match<Customer, Current<AccessInfo.userName>>>>)));

				this.viewClass = new PXView(sender.Graph, true,
					BqlCommand.CreateInstance(
					BqlCommand.Compose(
					typeof(Search<,>), typeof(RQRequestClass.reqClassID),
					typeof(Where<,>), typeof(RQRequestClass.reqClassID), typeof(Equal<>), typeof(Optional<>), reqClassID ?? typeof(RQRequestClass.reqClassID))));

				if (this.reqClassID != null)
				{
					sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(reqClassID), reqClassID.Name,
						ReqClassFieldUpdated);
				}
				PXUIFieldAttribute.SetDisplayName<BAccountR.acctCD>(sender.Graph.Caches[typeof (BAccountR)], Messages.Requester);
				PXUIFieldAttribute.SetDisplayName<BAccountR.acctName>(sender.Graph.Caches[typeof(BAccountR)], Messages.RequesterName);
				Type[] _fields = new Type[]
				              	{
				              		typeof (BAccountR.acctCD),
				              		typeof (BAccountR.acctName),
				              		typeof (BAccountR.status),
				              		typeof (Contact.phone1),
				              		typeof (Address.city),
				              		typeof (Address.countryID)
				              	};
				string[] selFields = new string[_fields.Length];
				string[] selHeaders = new string[_fields.Length];				
				for (int i = 0; i < _fields.Length; i++)
				{
					Type field = _fields[i];
					Type cacheType = BqlCommand.GetItemType(field);
					PXCache cache = sender.Graph.Caches[cacheType];
					
					if (cacheType.IsAssignableFrom(typeof(BAccountR)) || 
						field.Name == typeof(BAccountR.acctCD).Name ||
						field.Name == typeof(BAccountR.acctName).Name)
					{
						selFields[i] = field.Name;						
					}
					else
					{
						selFields[i] = cacheType.Name + "__" + field.Name;
					}					
					selHeaders[i] = PXUIFieldAttribute.GetDisplayName(cache, field.Name);
				}
				PXSelectorAttribute.SetColumns(sender, _FieldName, selFields, selHeaders);								

			}

			private void ReqClassFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				string oldClassId = (string)e.OldValue;
				string newClassId = (string)sender.GetValue(e.Row, reqClassID.Name);
				object value = sender.GetValue(e.Row, _FieldName);
				if (oldClassId != newClassId)
				{
					RQRequestClass newClass = (RQRequestClass)this.viewClass.SelectSingle(newClassId);
					RQRequestClass oldClass = (RQRequestClass)this.viewClass.SelectSingle(oldClassId);

					if (newClass != null && oldClass != null && newClass.CustomerRequest != oldClass.CustomerRequest)
					{
						sender.SetValue(e.Row, _FieldOrdinal, null);
						sender.SetDefaultExt(e.Row, _FieldName);
					}
					else if(newClass != null && value != null)
					{
						BAccount account =
							PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.Select(sender.Graph, value);
						if(account == null ) return;

						if((account.Type != BAccountType.CustomerType && newClass.CustomerRequest == true) ||
							(account.Type != BAccountType.EmployeeType && newClass.CustomerRequest != true))
							sender.SetValue(e.Row, _FieldOrdinal, null);						
					}
				}
			}

			protected virtual IEnumerable GetRecords()
			{
				RQRequestClass reqClass =
					this.reqClassID != null ?
					(RQRequestClass)this.viewClass.SelectSingle() :
					null;
				
				if (reqClass == null || !(reqClass.CustomerRequest == true))
					foreach (object emp in this.viewEmployees.SelectMulti())
						yield return emp;

				if (reqClass == null || reqClass.CustomerRequest == true)
					foreach (object cust in this.viewCustomers.SelectMulti())
						yield return cust;
			}			
		}

		public RQRequesterSelectorAttribute()
			: this(null)
		{			
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="reqClassID">Request class field.</param>
		public RQRequesterSelectorAttribute(Type reqClassID)
			: base("BIZACCT",
			typeof(Search2<BAccount.bAccountID,
				LeftJoin<Contact, On<Contact.bAccountID, Equal<BAccount.bAccountID>,
						 And<Contact.contactID, Equal<BAccount.defContactID>>>,
				LeftJoin<Address, On<Address.bAccountID, Equal<BAccount.bAccountID>,
						 And<Address.addressID, Equal<BAccount.defAddressID>>>>>>),
			typeof(BAccount.acctCD),
			typeof(BAccount.acctCD),
			typeof(BAccount.acctName),
			typeof(BAccount.status),
			typeof(Contact.phone1),
			typeof(Address.city),
			typeof(Address.countryID))
		{
			this.DescriptionField = typeof(BAccount.acctName);
			_Attributes[_Attributes.Count - 1] = new CustomSelector(reqClassID);
		}

		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			BAccountR baccount = null;
			if (e.NewValue != null) 
			{
				baccount = PXSelect<BAccountR, 
					Where<BAccountR.acctCD, Equal<Required<BAccountR.acctCD>>>>.
					SelectWindowed(sender.Graph, 0, 1, e.NewValue);
			}
			if (baccount != null)
			{
				e.NewValue = baccount.BAccountID;
				e.Cancel = true;
			}
			else if(e.NewValue != null)
				throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ValueDoesntExist, _FieldName, e.NewValue));				
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			object value = e.ReturnValue;
			e.ReturnValue = null;
			base.FieldSelecting(sender, e);

			BAccountR baccount = PXSelect<BAccountR, Where<BAccountR.bAccountID, Equal<Required<BAccountR.bAccountID>>>>
				.SelectWindowed(sender.Graph, 0, 1, value);
			if (baccount != null)
			{
				e.ReturnValue = baccount.AcctCD;				
			}
			else
			{
				if (e.Row != null)													
					e.ReturnValue = null;									
			}
		}				
	}

	
	public class RQSiteStatusLookup<Status, StatusFilter> : INSiteStatusLookup<Status, StatusFilter>
		where Status : class, IBqlTable, new()
		where StatusFilter : class, IBqlTable, new()
	{
		#region Ctor
		public RQSiteStatusLookup(PXGraph graph)
			: base(graph)
		{
			this.View = CreateView(graph, null);
		}

		public RQSiteStatusLookup(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
			this.View = CreateView(graph, handler);
		}

		private static PXView CreateView(PXGraph graph, Delegate handler)
		{
			Type inventoryID = GetTypeField<Status>(typeof(INSiteStatus.inventoryID).Name);
			Type subItemID = GetTypeField<Status>(typeof(INSiteStatus.subItemID).Name);
			Type siteID = GetTypeField<Status>(typeof(INSiteStatus.siteID).Name);

			Type join =
				BqlCommand.Compose(
				typeof(LeftJoin<,,>),
				typeof(RQRequestClass),
				typeof(On<RQRequestClass.reqClassID, Equal<Current<RQRequest.reqClassID>>>),
				typeof(LeftJoin<,>),
				typeof(RQRequestClassItem),
				typeof(On<,,>),
						typeof(RQRequestClassItem.inventoryID), typeof(Equal<>), inventoryID,
				typeof(And<RQRequestClassItem.reqClassID, Equal<RQRequestClass.reqClassID>,
						And<RQRequestClass.restrictItemList, Equal<BQLConstants.BitOn>>>));

			Type where =
			typeof(Where<RQRequestClass.restrictItemList, Equal<BQLConstants.BitOff>,
					Or<RQRequestClassItem.inventoryID, IsNotNull,
							Or<RQRequestClass.reqClassID, IsNull>>>);

			where = BqlCommand.Compose(typeof(Where2<,>), where, typeof(And<>), CreateWhere(graph));

			Type selectType =
			BqlCommand.Compose(
				typeof(Select5<,,,>),
				typeof(Status),
				join,
				where,
				typeof(Aggregate<>),
				typeof(GroupBy<,>), inventoryID,
				typeof(GroupBy<,>), subItemID,
				typeof(GroupBy<>), siteID);

			return
				handler == null ?
				new PXView(graph, false, BqlCommand.CreateInstance(selectType)) :
				new PXView(graph, false, BqlCommand.CreateInstance(selectType), handler);
		}

		#endregion
	}

	#region RQAcctSubDefault

	/// <summary>
	/// Helper. Used to define expence sub. in request.
	/// </summary>
	public class RQAcctSubDefault
	{
		/// <summary>
		/// List of allowed source to compune sub. in request.
		/// </summary>
		public class ClassListAttribute : PXCustomStringListAttribute
		{
			public ClassListAttribute()
				: base(new string[] { MaskClass, MaskDepartment, MaskItem, MaskRequester},
					new string[] { Messages.MaskClass, Messages.MaskDepartment, Messages.MaskItem, Messages.MaskRequester })
			{
			}
		}				
		/// <summary>
		/// Mask for inventory item.
		/// </summary>
		public const string MaskItem = "I";
		/// <summary>
		/// Mask for requester.
		/// </summary>
		public const string MaskRequester = "R";
		/// <summary>
		/// Mask for department.
		/// </summary>
		public const string MaskDepartment = "D";
		/// <summary>
		/// Mask for request class.
		/// </summary>
		public const string MaskClass = "Q";		
	}

	#endregion

	#region SubAccountMaskAttribute
	/// <summary>
	/// Dimension selector for expense sub. account used in request.
	/// </summary>
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class SubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "RQSETUP";
		public SubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, 
				RQAcctSubDefault.MaskDepartment,
				new RQAcctSubDefault.ClassListAttribute().AllowedValues,
				new RQAcctSubDefault.ClassListAttribute().AllowedLabels);
			attr.ValidComboRequired = true;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField 
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new RQAcctSubDefault.ClassListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new RQAcctSubDefault.ClassListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	#endregion
}

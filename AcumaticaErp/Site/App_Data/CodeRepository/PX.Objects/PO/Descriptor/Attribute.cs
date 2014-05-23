using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.TX;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.GL;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using LocationStatus = PX.Objects.IN.Overrides.INDocumentRelease.LocationStatus;
using LotSerialStatus = PX.Objects.IN.Overrides.INDocumentRelease.LotSerialStatus;
using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;
using System.Collections;
using PX.Objects.CR;
using PX.Objects.AP;
using PX.Objects.CM;


namespace PX.Objects.PO
{
    /// <summary>
    /// Extension of the string PXStringList attribute, which allows to define list <br/>
    /// of hidden(possible) values and their lables. Usually, this list must be wider, then list of <br/> 
    /// enabled values - which mean that UI control may display more values then user is allowed to select in it<br/>
    /// </summary>
	public class PXStringListExtAttribute : PXStringListAttribute
	{
		#region State
		protected string[] _HiddenValues;
		protected string[] _HiddenLabels;
		protected string[] _HiddenLabelsLocal;
		#endregion

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="allowedValues">List of the string values, that user can select in the UI</param>
        /// <param name="allowedLabels">List of the labels for these values</param>
        /// <param name="hiddenValues">List of the string values, that may appear in the list. Normally, it must contain all the values from allowedList </param>
        /// <param name="hiddenLabels">List of the labels for these values</param>
		public PXStringListExtAttribute(string[] allowedValues, string[] allowedLabels, string[] hiddenValues, string[] hiddenLabels)
			: base(allowedValues, allowedLabels)
		{
			_HiddenValues = hiddenValues;
			_HiddenLabels = hiddenLabels;
			_HiddenLabelsLocal = null;
			_ExclusiveValues = false;
		}
		public override void  CacheAttached(PXCache sender)
		{
			TryLocalize(sender);
 			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, OnFieldUpdating);
		}
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);
			if (_HiddenLabelsLocal == null)
			{
				if (!System.Globalization.CultureInfo.InvariantCulture.Equals(System.Threading.Thread.CurrentThread.CurrentCulture) && 
					_HiddenLabels != null && _HiddenValues != null)
				{
					_HiddenLabelsLocal = new string[_HiddenLabels.Length];

					_HiddenLabels.CopyTo(_HiddenLabelsLocal, 0);
					if (_BqlTable != null)
					{
						for (int i = 0; i < _HiddenLabels.Length; i++)
						{
							string value = PXUIFieldAttribute.GetNeutralDisplayName(sender, _FieldName) + " -> " + _HiddenLabels[i];
							string temp = PXLocalizer.Localize(value, _BqlTable.FullName);
							if (!string.IsNullOrEmpty(temp) && temp != value)
								_HiddenLabelsLocal[i] = temp;
						}
					}
				}
				else
					_HiddenLabelsLocal = _HiddenLabels;
			}

			if (e.Row != null && e.ReturnValue != null && IndexAllowedValue((string)e.ReturnValue) < 0)
			{
				int index = IndexValue((string)e.ReturnValue);
				if (index >= 0)
					e.ReturnValue = _HiddenLabelsLocal != null ? _HiddenLabelsLocal[index] : _HiddenLabels[index];
			}
		}

		protected int IndexAllowedValue(string value)
		{
			if (_AllowedValues != null)
				for (int i = 0; i < _AllowedValues.Length; i++)
					if (string.Compare(_AllowedValues[i], value, true) == 0)
						return i;
			return -1;
		}

		protected int IndexValue(string value)
		{
			if (_HiddenValues != null)
				for (int i = 0; i < _HiddenValues.Length; i++)
					if (string.Compare(_HiddenValues[i], value, true) == 0)
						return i;
			return -1;
		}

		protected string SearchValueByName(string name)
		{
			if (_HiddenValues != null)
				for (int i = 0; i < _HiddenValues.Length; i++)
				{
					if (_HiddenLabelsLocal != null && string.Compare(_HiddenLabelsLocal[i], name, true) == 0)
						return _HiddenValues[i];
					if (_HiddenLabels != null && string.Compare(_HiddenLabels[i], name, true) == 0)
						return _HiddenValues[i];
				}
			return null;
		}

		#region IPXFieldUpdatingSubscriber Members

		protected virtual void OnFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if(e.NewValue != null)
			{
				if(IndexValue((string)e.NewValue) != -1) return;
				e.NewValue = SearchValueByName((string) e.NewValue);
			}
		}

		#endregion
	}

    /// <summary>
    /// Specialized PXStringList attribute for PO Order Line types.<br/>
    /// Provides a list of possible values for line types depending upon InventoryID <br/>
    /// specified in the row. For stock- and not-stock inventory items the allowed values <br/>
    /// are different. If item is changed and old value is not compatible with inventory item <br/>
    /// - it will defaulted to the applicable value.<br/>
    /// <example>
    /// [POLineTypeList(typeof(POLine.inventoryID))]
    /// </example>
    /// </summary>
	public class POLineTypeListAttribute : PXStringListExtAttribute, IPXFieldVerifyingSubscriber, IPXFieldDefaultingSubscriber
	{
		#region State
		protected Type _inventoryID;
		#endregion
		
        /// <summary>
        /// Ctor, short version. List of allowed values is defined as POLineType.GoodsForInventory, POLineType.NonStock, POLineType.Service, POLineType.Freight, POLineType.Description  
        /// </summary>
        /// <param name="inventoryID">Must be IBqlField. Represents an InventoryID field in the row</param>
		public POLineTypeListAttribute(Type inventoryID)
			:this(
			inventoryID,
			new string[] { POLineType.GoodsForInventory, POLineType.NonStock, POLineType.Service, POLineType.Freight, POLineType.Description },
			new string[] { Messages.GoodsForInventory, Messages.NonStockItem, Messages.Service, Messages.Freight, Messages.Description })
		{
			
		}

        /// <summary>
        /// Ctor. Shorter version. User may define a list of allowed values and their descriptions
        /// List for hidden values is defined as POLineType.GoodsForInventory, POLineType.GoodsForSalesOrder, 
        /// POLineType.GoodsForReplenishment, POLineType.GoodsForDropShip, POLineType.NonStockForDropShip, 
        /// POLineType.NonStockForSalesOrder, POLineType.NonStock, POLineType.Service, 
        /// POLineType.Freight, POLineType.Description - it includes all the values for the POLine types. 
        /// </summary>
        /// <param name="inventoryID">Must be IBqlField. Represents an InventoryID field in the row</param>
        /// <param name="allowedValues">List of allowed values. </param>
        /// <param name="allowedLabels">List of allowed values labels. Will be shown in the combo-box in UI</param>        
	    public POLineTypeListAttribute(Type inventoryID, string[] allowedValues, string[] allowedLabels)
			:this(				
				inventoryID,
				allowedValues, allowedLabels,
				new string[] { POLineType.GoodsForInventory, POLineType.GoodsForSalesOrder, POLineType.GoodsForReplenishment, POLineType.GoodsForDropShip, POLineType.NonStockForDropShip, POLineType.NonStockForSalesOrder, POLineType.NonStock, POLineType.Service, POLineType.Freight, POLineType.Description },
				new string[] { Messages.GoodsForInventory, Messages.GoodsForSalesOrder, Messages.GoodsForReplenishment, Messages.GoodsForDropShip, Messages.NonStockForDropShip, Messages.NonStockForSalesOrder, Messages.NonStockItem, Messages.Service, Messages.Freight, Messages.Description }
				)			
		{			
		}

        /// <summary>
        /// Ctor. Full version. User may define a list of allowed values and their descriptions, and a list of hidden values.
        /// </summary>
        /// <param name="inventoryID">Must be IBqlField. Represents an InventoryID field of the row</param>
        /// <param name="allowedValues">List of allowed values. </param>
        /// <param name="allowedLabels"> Labels for the allowed values. List should have the same size as the list of the values</param>
        /// <param name="hiddenValues"> List of possible values for the control. Must include all the values from the allowedValues list</param>
        /// <param name="hiddenLabels"> Labels for the possible values. List should have the same size as the list of the values</param>
		public POLineTypeListAttribute(Type inventoryID, string[] allowedValues, string[] allowedLabels, string[] hiddenValues, string[] hiddenLabels)
			: base(allowedValues, allowedLabels, hiddenValues, hiddenLabels)
		{
			_inventoryID = inventoryID;
		}

		#region Implementation
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _inventoryID.Name, InventoryIDUpdated);
		}


		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			int? inventoryID = (int?)sender.GetValue(e.Row, _inventoryID.Name);
			if (e.Row != null && e.NewValue != null )
			{
				if (inventoryID != null)
				{
					InventoryItem nonStock = PXSelect<InventoryItem,
						Where<InventoryItem.stkItem, Equal<False>, And<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>>.Select(sender.Graph, inventoryID);

					if (nonStock != null && nonStock.KitItem == true)
					{
						INKitSpecStkDet component = PXSelect<INKitSpecStkDet, Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.SelectWindowed(sender.Graph, 0, 1, nonStock.InventoryID);
						if (component != null) nonStock = null;
					}

					if ((nonStock != null && !POLineType.IsNonStock((string) e.NewValue)) ||
							(nonStock == null && !POLineType.IsStock((string)e.NewValue)))
						throw new PXSetPropertyException(Messages.UnsupportedLineType);
				}
				/*
				else if((string)e.NewValue != POLineType.Freight && (string) e.NewValue != POLineType.Description)
						throw new PXSetPropertyException(Messages.UnsupportedLineType);
				*/
				if(IndexValue((string)e.NewValue) == -1)
					throw new PXSetPropertyException(Messages.UnsupportedLineType);
			}
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			int? inventoryID = (int?)sender.GetValue(e.Row, _inventoryID.Name);
			if (inventoryID != null)
			{
				InventoryItem nonStock = PXSelect<InventoryItem,
					Where<InventoryItem.stkItem, Equal<False>, And<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>>.Select(sender.Graph, inventoryID);

				if (nonStock !=null && nonStock.KitItem == true)
				{
					INKitSpecStkDet component = PXSelect<INKitSpecStkDet, Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.SelectWindowed(sender.Graph, 0, 1, nonStock.InventoryID);

					if (component != null)
						e.NewValue = POLineType.GoodsForInventory;
					return;
				}


				e.NewValue =
					nonStock != null && nonStock.NonStockReceipt == true
						? POLineType.NonStock
						: nonStock != null && nonStock.NonStockReceipt != true
						  	? POLineType.Service
						  	: POLineType.GoodsForInventory;
				e.Cancel = true;
			}
		}

    	protected virtual void InventoryIDUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt(e.Row, _FieldName);
		}	
		#endregion		
	}
	/// <summary>
	/// Specialized PXStringList attribute for Receipt Line types.<br/>
	/// Provides a list of possible values for line types depending upon InventoryID <br/>
	/// specified in the row. For stock- and not-stock inventory items the allowed values <br/>
	/// are different. If item is changed and old value is not compatible with inventory item <br/>
	/// - it will defaulted to the applicable value.<br/>
	/// <example>
	/// [POReceiptLineTypeList(typeof(POLine.inventoryID))]
	/// </example>
	/// </summary>
	public class POReceiptLineTypeListAttribute : POLineTypeListAttribute
	{
		public POReceiptLineTypeListAttribute(Type inventoryID)
			:base(
			inventoryID,
			new string[] { POLineType.GoodsForInventory, POLineType.NonStock, POLineType.Service, POLineType.Freight },
			new string[] { Messages.GoodsForInventory, Messages.NonStockItem, Messages.Service, Messages.Freight })
		{
			
		}
	}

    /// <summary>
    /// Specialized for POOrder version of the VendorAttribute, which defines a list of vendors, <br/>
    /// which may be used in the PO Order (for example, employee are filtered <br/>
    /// out for all order types except Transfer ) <br/>
    /// Depends upon POOrder current. <br/>
    /// <example>
    /// [POVendor()]
    /// </example>
    /// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Vendor", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<Vendor.status, IsNull, 
							Or<Vendor.status, Equal<BAccount.status.active>, 
				  		    Or<Vendor.status, Equal<BAccount.status.oneTime>>>>), AP.Messages.VendorIsInStatus, typeof(Vendor.status))]
	public class POVendorAttribute : VendorAttribute
	{
		public POVendorAttribute()
			: base(typeof(Search<BAccountR.bAccountID,
				Where<Current<POOrder.orderType>, NotEqual<POOrderType.transfer>, 
						And<Vendor.type, NotEqual<BAccountType.employeeType>,
			      Or<Current<POOrder.orderType>, Equal<POOrderType.transfer>,
						And<BAccountR.type, Equal<BAccountType.companyType>>>>>>))
		{
		}
	}

    /// <summary>
    /// Specialized for PO version of the Address attribute.<br/>
    /// Uses POAddress tables for Address versions storage <br/>
    /// Prameters AddressID, IsDefault address are assigned to the <br/>
    /// corresponded fields in the POAddress table. <br/>
    /// Cache for POShipAddress(inherited from POAddress) must be present in the graph <br/>
    /// Special derived type is needed to be able to use different instances of POAddress <br/>
    /// (like PORemitAddress and POShipAddress)in the same graph - otherwise is not possible <br/>
    /// to enable/disable controls in the forms correctly <br/>    
    /// Depends upon row instance. <br/>
    /// <example>
    ///[POShipAddress(typeof(Select2<Address,
    ///               LeftJoin<Location, On<Address.bAccountID, Equal<Location.bAccountID>,
    ///                And<Address.addressID, Equal<Location.defAddressID>,
    ///                And<Current<POOrder.shipDestType>, NotEqual<POShippingDestination.site>,
    ///                And<Location.bAccountID, Equal<Current<POOrder.shipToBAccountID>>,
    ///                And<Location.locationID, Equal<Current<POOrder.shipToLocationID>>>>>>>,
    ///                LeftJoin<INSite, On<Address.addressID, Equal<INSite.addressID>,
    ///                  And<Current<POOrder.shipDestType>, Equal<POShippingDestination.site>,
    ///                    And<INSite.siteID, Equal<Current<POOrder.siteID>>>>>,
    ///                LeftJoin<POShipAddress, On<POShipAddress.bAccountID, Equal<Address.bAccountID>,
    ///                    And<POShipAddress.bAccountAddressID, Equal<Address.addressID>,
    ///                    And<POShipAddress.revisionID, Equal<Address.revisionID>,
    ///                    And<POShipAddress.isDefaultAddress, Equal<boolTrue>>>>>>>>,
    ///                Where<Location.locationCD, IsNotNull, Or<INSite.siteCD, IsNotNull>>>))]
    /// </example>
    /// </summary>	
	public class POShipAddressAttribute : AddressAttribute
	{
        /// <summary>
        /// Internaly, it expects POShipAddress as a POAddress type. 
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/> 
        /// a source Address record from which PO address is defaulted and for selecting default version of POAddress, <br/>
        /// created  from source Address (having  matching ContactID, revision and IsDefaultContact = true) <br/>
        /// if it exists - so it must include both records. See example above. <br/>
        /// </param>		
		public POShipAddressAttribute(Type SelectType)
			: base(typeof(POShipAddress.addressID), typeof(POShipAddress.isDefaultAddress), SelectType)
		{

		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<POShipAddress.overrideAddress>(Record_Override_FieldVerifying);
		}


		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<POShipAddress.overrideAddress>(sender, e.Row, sender.AllowUpdate);
				PXUIFieldAttribute.SetEnabled<POShipAddress.isValidated>(sender, e.Row, false);
			}
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<POShipAddress, POShipAddress.addressID>(sender, DocumentRow, Row);
		}

		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<POShipAddress, POShipAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<POShipAddress>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}			
		}


	}

    /// <summary>
    /// Specialized for PO version of the Address attribute.<br/>
    /// Uses POAddress tables for Address versions storage <br/>
    /// Prameters AddressID, IsDefault address are assigned to the <br/>
    /// corresponded fields in the POAddress table. <br/>
    /// Cache for PORemitAddress(inherited POAddess) must be present in the graph <br/>
    /// Special derived type is needed to be able to use different instances of POAddress <br/>
    /// (like PORemitAddress and POShipAddress)in the same graph - otherwise is not possible <br/>
    /// to enable/disable controls in the forms correctly <br/>    
    /// Depends upon row instance.
    /// <example>
    /// [PORemitAddress(typeof(Select2<BAccount2,
    ///		InnerJoin<Location, On<Location.bAccountID, Equal<BAccount2.bAccountID>>,
    ///		InnerJoin<Address, On<Address.bAccountID, Equal<Location.bAccountID>, And<Address.addressID, Equal<Location.defAddressID>>>,
    ///		LeftJoin<PORemitAddress, On<PORemitAddress.bAccountID, Equal<Address.bAccountID>, 
    ///			And<PORemitAddress.bAccountAddressID, Equal<Address.addressID>,
    ///			And<PORemitAddress.revisionID, Equal<Address.revisionID>, And<PORemitAddress.isDefaultAddress, Equal<boolTrue>>>>>>>>,
    ///		Where<Location.bAccountID, Equal<Current<POOrder.vendorID>>, And<Location.locationID, Equal<Current<POOrder.vendorLocationID>>>>>))]		
    /// </example>
    /// </summary>
	public class PORemitAddressAttribute : AddressAttribute
	{
        /// <summary>
        /// Internaly, it expects PORemitAddress as a POAddress type. 
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/> 
        /// a source Address record from which PO address is defaulted and for selecting default version of POAddress, <br/>
        /// created  from source Address (having  matching ContactID, revision and IsDefaultContact = true) <br/>
        /// if it exists - so it must include both records. See example above. <br/>
        /// </param>
		public PORemitAddressAttribute(Type SelectType)
			: base(typeof(PORemitAddress.addressID), typeof(PORemitAddress.isDefaultAddress), SelectType)
		{

		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<PORemitAddress.overrideAddress>(Record_Override_FieldVerifying);
		}


		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<PORemitAddress.overrideAddress>(sender, e.Row, sender.AllowUpdate);
				PXUIFieldAttribute.SetEnabled<PORemitAddress.isValidated>(sender, e.Row, false);
			}
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<PORemitAddress, PORemitAddress.addressID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<PORemitAddress, PORemitAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<PORemitAddress>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}			
		}


	}

    /// <summary>
    /// Specialized for PO version of the Contact attribute.<br/>
    /// Uses POContact tables for Contact versions storage <br/>
    /// Parameters ContactID, IsDefaultContact are assigned to the <br/>
    /// corresponded fields in the POContact table. <br/>
    /// Cache for PORShipContact (inherited from POContact) must be present in the graph <br/>
    /// Special derived type is needed to be able to use different instances of POContact <br/>
    /// (like PORemitContact and POShipContact)in the same graph - otherwise is not possible <br/>
    /// to enable/disable controls in the forms correctly <br/>
    /// Depends upon row instance.    
    ///<example>
    ///[POShipContactAttribute(typeof(Select2<Contact,
    ///                LeftJoin<Location, On<Contact.bAccountID, Equal<Location.bAccountID>,
    ///                    And<Contact.contactID, Equal<Location.defContactID>,
    ///                    And<Current<POOrder.shipDestType>, NotEqual<POShippingDestination.site>,
    ///                    And<Location.bAccountID, Equal<Current<POOrder.shipToBAccountID>>,
    ///                    And<Location.locationID, Equal<Current<POOrder.shipToLocationID>>>>>>>,
    ///                LeftJoin<INSite, On<Contact.contactID, Equal<INSite.contactID>,
    ///                  And<Current<POOrder.shipDestType>, Equal<POShippingDestination.site>,
    ///                    And<INSite.siteID, Equal<Current<POOrder.siteID>>>>>,
    ///                LeftJoin<POShipContact, On<POShipContact.bAccountID, Equal<Contact.bAccountID>,
    ///                    And<POShipContact.bAccountContactID, Equal<Contact.contactID>,
    ///                    And<POShipContact.revisionID, Equal<Contact.revisionID>,
    ///                    And<POShipContact.isDefaultContact, Equal<boolTrue>>>>>>>>,
    ///                Where<Location.locationCD, IsNotNull, Or<INSite.siteCD, IsNotNull>>>))]
    ///</example>    
    ///</summary>		
	public class POShipContactAttribute : ContactAttribute
	{
        /// <summary>
        /// Ctor. Internaly, it expects POShipContact as a POContact type
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/> 
        /// a source Contact record from which PO Contact is defaulted and for selecting version of POContact, <br/>
        /// created from source Contact (having  matching ContactID, revision and IsDefaultContact = true).<br/>
        /// - so it must include both records. See example above. <br/>
        /// </param>		
		public POShipContactAttribute(Type SelectType)
			: base(typeof(POShipContact.contactID), typeof(POShipContact.isDefaultContact), SelectType)
		{

		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<POShipContact.overrideContact>(Record_Override_FieldVerifying);
		}


		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<POShipContact, POShipContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<POShipContact, POShipContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<POShipContact.overrideContact>(sender, e.Row, sender.AllowUpdate);
			}
		}

		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<POShipContact>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

	}

    /// <summary>
    /// Specialized for PO version of the Contact attribute.<br/>
    /// Uses POContact tables for Contact versions storage <br/>
    /// Parameters ContactID, IsDefaultContact are assigned to the <br/>
    /// corresponded fields in the POContact table. <br/>
    /// Cache for PORemitContact (inherited from POContact) must be present in the graph <br/>
    /// Special derived type is needed to be able to use different instances of POContact <br/>
    /// (like PORemitContact and POShipContact)in the same graph otherwise is not possible <br/>
    /// to enable/disable controls in the forms correctly <br/>
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
	public class PORemitContactAttribute : ContactAttribute
	{
        /// <summary>
        /// Ctor. Internaly, it expects PORemitContact as a POContact type
        /// </summary>
        /// <param name="SelectType">Must have type IBqlSelect. This select is used for both selecting <br/> 
        /// a source Contact record from which PO Contact is defaulted and for selecting version of POContact, <br/>
        /// created from source Contact (having  matching ContactID, revision and IsDefaultContact = true).<br/>
        /// - so it must include both records. See example above. <br/>
        /// </param>		
		public PORemitContactAttribute(Type SelectType)
			: base(typeof(PORemitContact.contactID), typeof(PORemitContact.isDefaultContact), SelectType)
		{

		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<PORemitContact.overrideContact>(Record_Override_FieldVerifying);
		}


		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<PORemitContact, PORemitContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<PORemitContact, PORemitContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<PORemitContact.overrideContact>(sender, e.Row, sender.AllowUpdate);
			}
		}

		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<PORemitContact>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}

		}

	}

	public class POTaxAttribute : TaxAttribute
	{
		public POTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryDocBal = typeof(POOrder.curyOrderTotal);
			this.CuryLineTotal = typeof(POOrder.curyLineTotal);
			this.DocDate = typeof(POOrder.orderDate);
            this.CuryTranAmt = typeof(POLine.curyExtCost);
            this.GroupDiscountRate = typeof(POLine.groupDiscountRate);
			this.CuryTaxTotal = typeof(POOrder.curyTaxTotal);

			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(POLine.curyExtCost), typeof(SumCalc<POOrder.curyLineTotal>)));
		}

        protected override decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType)
        {
            if (TaxCalcType == "I")
                return base.GetCuryTranAmt(sender, row) * (decimal?)sender.GetValue(row, _GroupDiscountRate);
            else
                return base.GetCuryTranAmt(sender, row);
        }

		protected override decimal CalcLineTotal(PXCache sender, object row)
		{
			return (decimal?)ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m;
		}

		protected override object InitializeTaxDet(object data)
		{
			((POTax)data).DetailType = POTaxDetailType.OrderTax;

			return data;
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, POTaxDetailType.orderTax, Current<POLine.lineNbr>>(graph, new object[] { row, ((POOrderEntry)graph).Document.Current }, taxchk, parameters);
		}

		protected List<object> SelectTaxes<Where, DetailType, LineNbr>(PXGraph graph, object[] currents, PXTaxCheck taxchk, params object[] parameters)
			where Where : IBqlWhere, new()
			where DetailType : IBqlOperand
			where LineNbr : IBqlOperand
		{
			Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
			foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
				LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
					And<TaxRev.outdated, Equal<boolFalse>, 
					And2<Where<TaxRev.taxType, Equal<TaxType.purchase>, And<Tax.reverseTax, Equal<boolFalse>, 
						Or<TaxRev.taxType, Equal<TaxType.sales>, And<Where<Tax.reverseTax, Equal<boolTrue>, 
						Or<Tax.taxType, Equal<CSTaxType.use>, Or<Tax.taxType, Equal<CSTaxType.withholding>>>>>>>>, 
					And<Current<POOrder.orderDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>,
				Where>
				.SelectMultiBound(graph, currents, parameters))
			{
				tail[((Tax)record).TaxID] = record;
			}
			List<object> ret = new List<object>();
			switch (taxchk)
			{
				case PXTaxCheck.Line:
					foreach (POTax record in PXSelect<POTax,
						Where<POTax.orderType, Equal<Current<POOrder.orderType>>,
							And<POTax.orderNbr, Equal<Current<POOrder.orderNbr>>,
							And<POTax.detailType, Equal<DetailType>,
							And<POTax.lineNbr, Equal<LineNbr>>>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& String.Compare(((Tax)(PXResult<POTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<POTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				case PXTaxCheck.RecalcLine:
					foreach (POTax record in PXSelect<POTax,
						Where<POTax.orderType, Equal<Current<POOrder.orderType>>,
							And<POTax.orderNbr, Equal<Current<POOrder.orderNbr>>,
							And<POTax.detailType, Equal<DetailType>,
							And<POTax.lineNbr, Less<intMax>>>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& ((POTax)(PXResult<POTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
								&& String.Compare(((Tax)(PXResult<POTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<POTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				case PXTaxCheck.RecalcTotals:
					foreach (POTaxTran record in PXSelect<POTaxTran,
						Where<POTaxTran.orderType, Equal<Current<POOrder.orderType>>,
							And<POTaxTran.orderNbr, Equal<Current<POOrder.orderNbr>>,
							And<POTaxTran.detailType, Equal<DetailType>>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (record.TaxID != null && tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& String.Compare(((Tax)(PXResult<POTaxTran, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<POTaxTran, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				default:
					return ret;
			}
		}

		public override void CacheAttached(PXCache sender)
		{
            inserted = new Dictionary<object, object>();
            updated = new Dictionary<object, object>(); 
            
            if (this.EnableTaxCalcOn(sender.Graph))
			{
				base.CacheAttached(sender);
				sender.Graph.RowInserting.AddHandler(_TaxSumType, Tax_RowInserting);
				sender.Graph.FieldUpdated.AddHandler(typeof(POOrder), _CuryTaxTotal, POOrder_CuryTaxTot_FieldUpdated);
                sender.Graph.FieldUpdated.AddHandler(typeof(POOrder), typeof(POOrder.curyDiscTot).Name, POOrder_CuryDiscTot_FieldUpdated);
            }
			else
			{
				this.TaxCalc = TaxCalc.NoCalc;
			}
		}

        protected virtual void POOrder_CuryDiscTot_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            bool calc = true;
            TaxZone taxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, (string)sender.GetValue(e.Row, _TaxZoneID));
            if (taxZone != null && taxZone.IsExternal == true)
                calc = false;

            this._ParentRow = e.Row;
            CalcTotals(sender, e.Row, calc);
            this._ParentRow = null;
        }

		virtual protected bool EnableTaxCalcOn(PXGraph aGraph) 
		{
			return (aGraph is POOrderEntry);
		}

		protected virtual void Tax_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (e.ExternalCall)
			{
				((POTax)e.Row).DetailType = POTaxDetailType.OrderTax;
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
            decimal CuryDiscountTotal = (decimal)(ParentGetValue<POOrder.curyDiscTot>(sender.Graph) ?? 0m);

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

		protected virtual void POOrder_CuryTaxTot_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			decimal? curyTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryTaxTotal);
			decimal? curyWhTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryWhTaxTotal);
			CalcDocTotals(sender, e.Row, curyTaxTotal.GetValueOrDefault(), 0, curyWhTaxTotal.GetValueOrDefault());
		}

        protected override void AdjustTaxableAmount(PXCache sender, object row, List<object> taxitems, ref decimal CuryTaxableAmt)
        {
            decimal CuryLineTotal = (decimal?)ParentGetValue<POOrder.curyLineTotal>(sender.Graph) ?? 0m;
            decimal CuryDiscountTotal = (decimal)(ParentGetValue<POOrder.curyDiscTot>(sender.Graph) ?? 0m);

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

	public class POOpenTaxAttribute : POTaxAttribute
	{
		public POOpenTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryDocBal = typeof(POOrder.curyOpenOrderTotal);
			this.CuryLineTotal = typeof(POOrder.curyOpenLineTotal);
			this.CuryTaxTotal = typeof(POOrder.curyOpenTaxTotal);
			this.DocDate = typeof(POOrder.orderDate);
			this.CuryTranAmt = typeof(POLine.curyOpenAmt);

			this._Attributes[0] = new PXUnboundFormulaAttribute(typeof(POLine.curyOpenAmt), typeof(SumCalc<POOrder.curyOpenLineTotal>));
		}

		protected override object InitializeTaxDet(object data)
		{
			((POTax)data).DetailType = POTaxDetailType.OrderOpenTax;

			return data;
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, POTaxDetailType.orderOpenTax, Current<POLine.lineNbr>>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}
	}

	public class POOpenTaxRAttribute : POOpenTaxAttribute
	{
		public POOpenTaxRAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryTranAmt = typeof(POLineUOpen.curyOpenAmt);

			this._Attributes[0] = new PXUnboundFormulaAttribute(typeof(POLineUOpen.curyOpenAmt), typeof(SumCalc<POOrder.curyOpenLineTotal>));
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, POTaxDetailType.orderOpenTax, Current<POLineUOpen.lineNbr>>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}
		
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_ChildType = sender.GetItemType();
			_TaxCalc = TaxCalc.Calc;
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _CuryTranAmt, CuryOpenAmt_FieldUpdated);
		}

		override protected bool EnableTaxCalcOn(PXGraph aGraph)
		{
			return (aGraph is POOrderEntry || aGraph is POReceiptEntry || aGraph is AP.APReleaseProcess);
		} 

		public virtual void CuryOpenAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			CalcTaxes(sender, e.Row, PXTaxCheck.Line);
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowInserted(sender, e);
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowUpdated(sender, e);
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowDeleted(sender, e);
		}
	}

	public class POReceiptTaxAttribute : TaxAttribute
	{
		public POReceiptTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryDocBal = typeof(POReceipt.curyOrderTotal);
			this.CuryLineTotal = typeof(POReceipt.curyLineTotal);
			this.DocDate = typeof(POReceipt.receiptDate);
            this.CuryTranAmt = typeof(POReceiptLine.curyExtCost);
            this.GroupDiscountRate = typeof(POReceiptLine.groupDiscountRate);
			this.CuryTaxTotal = typeof(POReceipt.curyTaxTotal);

			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(POReceiptLine.curyExtCost), typeof(SumCalc<POReceipt.curyLineTotal>)));
		}

        protected override decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType)
        {
            if (TaxCalcType == "I")
                return base.GetCuryTranAmt(sender, row) * (decimal?)sender.GetValue(row, _GroupDiscountRate);
            else
                return base.GetCuryTranAmt(sender, row);
        }

		protected override decimal CalcLineTotal(PXCache sender, object row)
		{
			return (decimal?)ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m;
		}

		protected override object InitializeTaxDet(object data)
		{
			((POReceiptTax)data).DetailType = POReceiptTaxDetailType.ReceiptTax;

			return data;
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, POReceiptTaxDetailType.receiptTax, Current<POReceiptLine.lineNbr>>(graph, new object[] { row, ((POReceiptEntry)graph).Document.Current }, taxchk, parameters);
		}

		protected List<object> SelectTaxes<Where, DetailType, LineNbr>(PXGraph graph, object[] currents, PXTaxCheck taxchk, params object[] parameters)
			where Where : IBqlWhere, new()
			where DetailType : IBqlOperand
			where LineNbr : IBqlOperand
		{
			Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
			foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
				LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
					And<TaxRev.outdated, Equal<boolFalse>,
					And2<Where<TaxRev.taxType, Equal<TaxType.purchase>, And<Tax.reverseTax, Equal<boolFalse>,
						Or<TaxRev.taxType, Equal<TaxType.sales>, And<Where<Tax.reverseTax, Equal<boolTrue>,
						Or<Tax.taxType, Equal<CSTaxType.use>, Or<Tax.taxType, Equal<CSTaxType.withholding>>>>>>>>,
					And<Current<POReceipt.receiptDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>,
				Where>
				.SelectMultiBound(graph, currents, parameters))
			{
				tail[((Tax)record).TaxID] = record;
			}
			List<object> ret = new List<object>();
			switch (taxchk)
			{
				case PXTaxCheck.Line:
					foreach (POReceiptTax record in PXSelect<POReceiptTax,
						Where<POReceiptTax.receiptNbr, Equal<Current<POReceipt.receiptNbr>>,
							And<POReceiptTax.detailType, Equal<DetailType>,
							And<POReceiptTax.lineNbr, Equal<LineNbr>>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& String.Compare(((Tax)(PXResult<POReceiptTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<POReceiptTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				case PXTaxCheck.RecalcLine:
					foreach (POReceiptTax record in PXSelect<POReceiptTax,
						Where<POReceiptTax.receiptNbr, Equal<Current<POReceipt.receiptNbr>>,
							And<POReceiptTax.detailType, Equal<DetailType>,
							And<POReceiptTax.lineNbr, Less<intMax>>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& ((POReceiptTax)(PXResult<POReceiptTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
								&& String.Compare(((Tax)(PXResult<POReceiptTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<POReceiptTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				case PXTaxCheck.RecalcTotals:
					foreach (POReceiptTaxTran record in PXSelect<POReceiptTaxTran,
						Where<POReceiptTaxTran.receiptNbr, Equal<Current<POReceipt.receiptNbr>>,
							And<POReceiptTaxTran.detailType, Equal<DetailType>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (record.TaxID != null && tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& String.Compare(((Tax)(PXResult<POReceiptTaxTran, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<POReceiptTaxTran, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				default:
					return ret;
			}
		}

		public override void CacheAttached(PXCache sender)
		{
            inserted = new Dictionary<object, object>();
            updated = new Dictionary<object, object>();

			if (this.EnableTaxCalcOn(sender.Graph))
			{
				base.CacheAttached(sender);

				sender.Graph.RowInserting.AddHandler(_TaxSumType, Tax_RowInserting);
				sender.Graph.FieldUpdated.AddHandler(typeof(POReceipt), _CuryTaxTotal, POReceipt_CuryTaxTot_FieldUpdated);
                sender.Graph.FieldUpdated.AddHandler(typeof(POReceipt), typeof(POReceipt.curyDiscTot).Name, POReceipt_CuryDiscTot_FieldUpdated);
			}
			else
			{
				this.TaxCalc = TaxCalc.NoCalc;
			}
		}

        protected virtual void POReceipt_CuryDiscTot_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            bool calc = true;
            TaxZone taxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, (string)sender.GetValue(e.Row, _TaxZoneID));
            if (taxZone != null && taxZone.IsExternal == true)
                calc = false;

            this._ParentRow = e.Row;
            CalcTotals(sender, e.Row, calc);
            this._ParentRow = null;
        }

		virtual protected bool EnableTaxCalcOn(PXGraph aGraph) 
		{
			return (aGraph is POReceiptEntry);
		}

		protected virtual void Tax_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (e.ExternalCall)
			{
				((POReceiptTax)e.Row).DetailType = POReceiptTaxDetailType.ReceiptTax;
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
            decimal CuryDiscountTotal = (decimal)(ParentGetValue<POOrder.curyDiscTot>(sender.Graph) ?? 0m);

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

		protected virtual void POReceipt_CuryTaxTot_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			decimal? curyTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryTaxTotal);
			decimal? curyWhTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryWhTaxTotal);
			CalcDocTotals(sender, e.Row, curyTaxTotal.GetValueOrDefault(), 0, curyWhTaxTotal.GetValueOrDefault());
		}

        protected override void AdjustTaxableAmount(PXCache sender, object row, List<object> taxitems, ref decimal CuryTaxableAmt)
        {
            decimal CuryLineTotal = (decimal?)ParentGetValue<POReceipt.curyLineTotal>(sender.Graph) ?? 0m;
            decimal CuryDiscountTotal = (decimal)(ParentGetValue<POReceipt.curyDiscTot>(sender.Graph) ?? 0m);

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

	public class POReceiptUnbilledTaxAttribute : POReceiptTaxAttribute
	{
		public POReceiptUnbilledTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryDocBal = typeof(POReceipt.curyUnbilledTotal);
			this.CuryLineTotal = typeof(POReceipt.curyUnbilledLineTotal);
			this.CuryTaxTotal = typeof(POReceipt.curyUnbilledTaxTotal);
			this.DocDate = typeof(POReceipt.receiptDate);
			this.CuryTranAmt = typeof(POReceiptLine.curyUnbilledAmt);

			this._Attributes[0] = new PXUnboundFormulaAttribute(typeof(POReceiptLine.curyUnbilledAmt), typeof(SumCalc<POReceipt.curyUnbilledLineTotal>));
		}

		protected override object InitializeTaxDet(object data)
		{
			((POReceiptTax)data).DetailType = POReceiptTaxDetailType.UnbilledTax;

			return data;
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, POReceiptTaxDetailType.unbilledTax, Current<POReceiptLine.lineNbr>>(graph, new object[] { row, ((POReceiptEntry)graph).Document.Current }, taxchk, parameters);
		}
	}
	public class POReceiptUnbilledTaxRAttribute : POReceiptUnbilledTaxAttribute
	{
		public POReceiptUnbilledTaxRAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryTranAmt = typeof(POReceiptLineR1.curyUnbilledAmt);

			this._Attributes[0] = new PXUnboundFormulaAttribute(typeof(POReceiptLineR1.curyUnbilledAmt), typeof(SumCalc<POReceipt.curyUnbilledLineTotal>));
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, POReceiptTaxDetailType.unbilledTax, Current<POReceiptLineR1.lineNbr>>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}

		override protected bool EnableTaxCalcOn(PXGraph aGraph)
		{
			return ((aGraph is POOrderEntry) || (aGraph is AP.APReleaseProcess));
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_ChildType = sender.GetItemType();
			_TaxCalc = TaxCalc.Calc;
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _CuryTranAmt, CuryOpenAmt_FieldUpdated);
		}

		public virtual void CuryOpenAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			CalcTaxes(sender, e.Row, PXTaxCheck.Line);
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowInserted(sender, e);
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowUpdated(sender, e);
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowDeleted(sender, e);
		}
	}

	public class LSPOReceiptLine : LSSelect<POReceiptLine, POReceiptLineSplit,
		Where<POReceiptLineSplit.receiptNbr, Equal<Current<POReceipt.receiptNbr>>>>
	{
		#region State
		protected virtual bool IsLSEntryEnabled(object row)
		{
			POReceiptLine line = row as POReceiptLine;

			return line == null
					|| line.LineType == POLineType.GoodsForInventory
					|| line.LineType == POLineType.GoodsForSalesOrder
					|| line.LineType == POLineType.GoodsForReplenishment;							
		}
        protected string _OrigOrderQtyField = "OrigOrderQty";
        protected string _OpenOrderQtyField = "OpenOrderQty";
		#endregion
		#region Ctor
		public LSPOReceiptLine(PXGraph graph)
			: base(graph)
		{
			MasterQtyField = typeof (POReceiptLine.receiptQty);
			graph.FieldDefaulting.AddHandler<POReceiptLineSplit.subItemID>(POReceiptLineSplit_SubItemID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<POReceiptLineSplit.locationID>(POReceiptLineSplit_LocationID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<POReceiptLineSplit.invtMult>(POReceiptLineSplit_InvtMult_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<POReceiptLineSplit.lotSerialNbr>(POReceiptLineSplit_LotSerialNbr_FieldDefaulting);
			graph.FieldVerifying.AddHandler<POReceiptLineSplit.qty>(POReceiptLineSplit_Qty_FieldVerifying);
			graph.RowUpdated.AddHandler<POReceipt>(POReceipt_RowUpdated);
            graph.FieldUpdated.AddHandler<POReceiptLine.receiptQty>(POReceiptLine_ReceiptQty_FieldUpdated);
            graph.Caches[typeof(POReceiptLine)].Fields.Add(_OrigOrderQtyField);
            graph.Caches[typeof(POReceiptLine)].Fields.Add(_OpenOrderQtyField);
            graph.FieldSelecting.AddHandler(typeof(POReceiptLine), _OrigOrderQtyField, OrigOrderQty_FieldSelecting);
            graph.FieldSelecting.AddHandler(typeof(POReceiptLine), _OpenOrderQtyField, OpenOrderQty_FieldSelecting);			
		}
		#endregion

		#region Implementation
		protected virtual void POReceipt_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<POReceipt.hold>(e.Row, e.OldRow) && (bool?)sender.GetValue<POReceipt.hold>(e.Row) == false)
			{
				PXCache cache = sender.Graph.Caches[typeof(POReceiptLine)];

				foreach (POReceiptLine item in PXParentAttribute.SelectSiblings(cache, null, typeof(POReceipt)))
				{
					if (IsLSEntryEnabled(item) &&  Math.Abs((decimal)item.BaseQty) >= 0.0000005m && item.UnassignedQty >= 0.0000005m)
					{
						cache.RaiseExceptionHandling<POReceiptLine.receiptQty>(item, item.Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned));

						if (cache.GetStatus(item) == PXEntryStatus.Notchanged)
						{
							cache.SetStatus(item, PXEntryStatus.Updated);
						}
					}
				}
			}
		}
		protected virtual void POReceiptLine_ReceiptQty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			POReceiptLine row = e.Row as POReceiptLine;
			if(row != null && row.ReceiptQty != (Decimal?)e.OldValue)
				sender.RaiseFieldUpdated<POReceiptLine.baseReceiptQty>(e.Row, row.BaseReceiptQty);
		}
		public virtual void POReceiptLineSplit_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((POReceiptLineSplit)e.Row).InventoryID);

			if (item != null && INLotSerialNbrAttribute.IsTrackSerial(item, ((POReceiptLineSplit)e.Row).TranType))
			{
				if (e.NewValue != null && e.NewValue is decimal && (decimal)e.NewValue != 0m && (decimal)e.NewValue != 1m)
				{
					e.NewValue = 1m;
				}
			}
		}

		protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (IsLSEntryEnabled(e.Row) && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				PXCache cache = sender.Graph.Caches[typeof(POReceipt)];
				object doc = PXParentAttribute.SelectParent(sender, e.Row, typeof(POReceipt)) ?? cache.Current;

				bool? OnHold = (bool?)cache.GetValue<POReceipt.hold>(doc);

				if (OnHold == false && Math.Abs((decimal)((POReceiptLine)e.Row).BaseQty) >= 0.0000005m && ((POReceiptLine)e.Row).UnassignedQty >= 0.0000005m)
				{
					if (sender.RaiseExceptionHandling<POReceiptLine.receiptQty>(e.Row, ((POReceiptLine)e.Row).Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned)))
					{
						throw new PXRowPersistingException(typeof(POReceiptLine.receiptQty).Name, ((POReceiptLine)e.Row).Qty, Messages.BinLotSerialNotAssigned);
					}
				}
			}
			base.Master_RowPersisting(sender, e);
		}

		protected virtual void OrigOrderQty_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				POLine orig_line = PXSelect<POLine, Where<POLine.orderType, Equal<Current<POReceiptLine.pOType>>,
					And<POLine.orderNbr, Equal<Current<POReceiptLine.pONbr>>,
					And<POLine.lineNbr, Equal<Current<POReceiptLine.pOLineNbr>>>>>>.SelectSingleBound(_Graph, new object[] { (POReceiptLine)e.Row });

				if (orig_line != null)
				{
					if (string.Equals(((POReceiptLine)e.Row).UOM, orig_line.UOM) == false)
					{
						decimal BaseOrderQty = INUnitAttribute.ConvertToBase<POReceiptLine.inventoryID>(sender, e.Row, orig_line.UOM, (decimal)orig_line.OrderQty, INPrecision.QUANTITY);
						e.ReturnValue = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID>(sender, e.Row, ((POReceiptLine)e.Row).UOM, BaseOrderQty, INPrecision.QUANTITY);
					}
					else
					{
						e.ReturnValue = orig_line.OrderQty;
					}
				}
			}
			e.ReturnState = PXDecimalState.CreateInstance(e.ReturnState, ((INSetup)_Graph.Caches[typeof(INSetup)].Current).DecPlQty, _OrigOrderQtyField, false, 0, decimal.MinValue, decimal.MaxValue);
			((PXFieldState)e.ReturnState).DisplayName = PXMessages.LocalizeNoPrefix(SO.Messages.OrigOrderQty);
			((PXFieldState)e.ReturnState).Enabled = false;
		}

        protected virtual void OpenOrderQty_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (e.Row != null)
            {
                POLine orig_line = PXSelect<POLine, Where<POLine.orderType, Equal<Current<POReceiptLine.pOType>>,
                    And<POLine.orderNbr, Equal<Current<POReceiptLine.pONbr>>,
                    And<POLine.lineNbr, Equal<Current<POReceiptLine.pOLineNbr>>>>>>.SelectSingleBound(_Graph, new object[] { (POReceiptLine)e.Row });

                if (orig_line != null)
                {
                    if (string.Equals(((POReceiptLine)e.Row).UOM, orig_line.UOM) == false)
                    {
                        decimal BaseOpenQty = INUnitAttribute.ConvertToBase<POReceiptLine.inventoryID>(sender, e.Row, orig_line.UOM, (decimal)orig_line.OrderQty - (decimal)orig_line.ReceivedQty, INPrecision.QUANTITY);
                        e.ReturnValue = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID>(sender, e.Row, ((POReceiptLine)e.Row).UOM, BaseOpenQty, INPrecision.QUANTITY);
                    }
                    else
                    {
                        e.ReturnValue = orig_line.OrderQty - orig_line.ReceivedQty;
                    }
                }
            }
            e.ReturnState = PXDecimalState.CreateInstance(e.ReturnState, ((INSetup)_Graph.Caches[typeof(INSetup)].Current).DecPlQty, _OpenOrderQtyField, false, 0, decimal.MinValue, decimal.MaxValue);
            ((PXFieldState)e.ReturnState).DisplayName = PXMessages.LocalizeNoPrefix(SO.Messages.OpenOrderQty);
            ((PXFieldState)e.ReturnState).Enabled = false;
        }

		protected override void Master_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (IsLSEntryEnabled(e.Row))
			{
				base.Master_RowInserted(sender, e);
			}
			else
			{
				sender.SetValue<POReceiptLine.locationID>(e.Row, null);
				sender.SetValue<POReceiptLine.lotSerialNbr>(e.Row, null);
				sender.SetValue<POReceiptLine.expireDate>(e.Row, null);
			}
		}

		protected override void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (IsLSEntryEnabled(e.Row))
			{
				base.Master_RowUpdated(sender, e);
			}
			else
			{
				sender.SetValue<POReceiptLine.locationID>(e.Row, null);
				sender.SetValue<POReceiptLine.lotSerialNbr>(e.Row, null);
				sender.SetValue<POReceiptLine.expireDate>(e.Row, null);
			}
		}

		protected override void Master_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (IsLSEntryEnabled(e.Row))
			{
				base.Master_RowDeleted(sender, e);
			}
		}

		public override POReceiptLineSplit Convert(POReceiptLine item)
		{
			using (InvtMultScope<POReceiptLine> ms = new InvtMultScope<POReceiptLine>(item))
			{
				POReceiptLineSplit ret = item;
				//baseqty will be overriden in all cases but AvailabilityFetch
				ret.BaseQty = item.BaseQty - item.UnassignedQty;
				return ret;
			}
		}

		public void ThrowFieldIsEmpty<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Field).Name)))
			{
				throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, typeof(Field).Name);
			}
		}


		public virtual void POReceiptLineSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				if (((POReceiptLineSplit)e.Row).BaseQty != 0m && ((POReceiptLineSplit)e.Row).LocationID == null)
				{
					ThrowFieldIsEmpty<POReceiptLineSplit.locationID>(sender, e.Row);
				}
			}
		}

		public virtual void POReceiptLineSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(POReceiptLine)];
			if (cache.Current != null && (e.Row == null || ((POReceiptLine)cache.Current).LineNbr == ((POReceiptLineSplit)e.Row).LineNbr))
			{
				e.NewValue = ((POReceiptLine)cache.Current).SubItemID;
				e.Cancel = true;
			}
		}

		public virtual void POReceiptLineSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(POReceiptLine)];
			if (cache.Current != null && (e.Row == null || ((POReceiptLine)cache.Current).LineNbr == ((POReceiptLineSplit)e.Row).LineNbr))
			{
				e.NewValue = ((POReceiptLine)cache.Current).LocationID;
				e.Cancel = true;
			}
		}

		public virtual void POReceiptLineSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(POReceiptLine)];
			if (cache.Current != null && (e.Row == null || ((POReceiptLine)cache.Current).LineNbr == ((POReceiptLineSplit)e.Row).LineNbr))
			{
				using (InvtMultScope<POReceiptLine> ms = new InvtMultScope<POReceiptLine>((POReceiptLine)cache.Current))
				{
					e.NewValue = ((POReceiptLine)cache.Current).InvtMult;
					e.Cancel = true;
				}
			}
		}

		public virtual void POReceiptLineSplit_LotSerialNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((POReceiptLineSplit)e.Row).InventoryID);

			if (item != null)
			{
				object InvtMult = ((POReceiptLineSplit)e.Row).InvtMult;
				if (InvtMult == null)
				{
					sender.RaiseFieldDefaulting<POReceiptLineSplit.invtMult>(e.Row, out InvtMult);
				}
				string TranType = ((POReceiptLineSplit)e.Row).TranType;

				INLotSerTrack.Mode mode = INLotSerialNbrAttribute.TranTrackMode(item, (string)TranType, (short?)InvtMult);
				if (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Create) > 0)
				{
					foreach (POReceiptLineSplit lssplit in INLotSerialNbrAttribute.CreateNumbers<POReceiptLineSplit>(sender, item, mode, 1m))
					{
						e.NewValue = lssplit.LotSerialNbr;
						e.Cancel = true;
					}
				}
				//otherwise default via attribute
			}
		}

		protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXSetPropertyException e)
		{
			if (row is POReceiptLine)
			{
				sender.RaiseExceptionHandling<POReceiptLine.receiptQty>(row, newValue, new PXSetPropertyException(e.Message, PXErrorLevel.Warning, sender.GetValueExt<POReceiptLine.inventoryID>(row), sender.GetValueExt<POReceiptLine.subItemID>(row), sender.GetValueExt<POReceiptLine.siteID>(row), sender.GetValueExt<POReceiptLine.locationID>(row), sender.GetValue<POReceiptLine.lotSerialNbr>(row)));
			}
			else
			{
				sender.RaiseExceptionHandling<POReceiptLineSplit.qty>(row, newValue, new PXSetPropertyException(e.Message, PXErrorLevel.Warning, sender.GetValueExt<POReceiptLineSplit.inventoryID>(row), sender.GetValueExt<POReceiptLineSplit.subItemID>(row), sender.GetValueExt<POReceiptLineSplit.siteID>(row), sender.GetValueExt<POReceiptLineSplit.locationID>(row), sender.GetValue<POReceiptLineSplit.lotSerialNbr>(row)));
			}
		}

		public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			IQtyAllocated availability = AvailabilityFetch(sender, (POReceiptLine)e.Row, true);

			if (availability != null)
			{
				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((POReceiptLine)e.Row).InventoryID);

				availability.QtyOnHand = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID, POReceiptLine.uOM>(sender, e.Row, (decimal)availability.QtyOnHand, INPrecision.QUANTITY);
				availability.QtyAvail = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID, POReceiptLine.uOM>(sender, e.Row, (decimal)availability.QtyAvail, INPrecision.QUANTITY);
				availability.QtyNotAvail = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID, POReceiptLine.uOM>(sender, e.Row, (decimal)availability.QtyNotAvail, INPrecision.QUANTITY);
				availability.QtyHardAvail = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID, POReceiptLine.uOM>(sender, e.Row, (decimal)availability.QtyHardAvail, INPrecision.QUANTITY);

				e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(Messages.Availability_Info,
						sender.GetValue<POReceiptLine.uOM>(e.Row), FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail));
									

				AvailabilityCheck(sender, (POReceiptLine)e.Row, availability);
			}
			else
			{
				e.ReturnValue = string.Empty;
			}

			base.Availability_FieldSelecting(sender, e);
		}
		#endregion

		public override PXSelectBase<INLotSerialStatus> GetSerialStatusCmd(PXCache sender, POReceiptLine Row, PXResult<InventoryItem, INLotSerClass> item)
		{
			PXSelectBase<INLotSerialStatus> cmd = new PXSelectJoin<INLotSerialStatus, InnerJoin<INLocation, On<INLocation.locationID, Equal<INLotSerialStatus.locationID>>>, Where<INLotSerialStatus.inventoryID, Equal<Current<INLotSerialStatus.inventoryID>>, And<INLotSerialStatus.siteID, Equal<Current<INLotSerialStatus.siteID>>, And<INLotSerialStatus.qtyOnHand, Greater<decimal0>>>>>(sender.Graph);

			if (Row.SubItemID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.subItemID, Equal<Current<INLotSerialStatus.subItemID>>>>();
			}
			if (Row.LocationID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.locationID, Equal<Current<INLotSerialStatus.locationID>>>>();
			}
			else
			{
				cmd.WhereAnd<Where<INLocation.receiptsValid, Equal<boolTrue>>>();
			}
			if (string.IsNullOrEmpty(Row.LotSerialNbr) == false)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.lotSerialNbr, Equal<Current<INLotSerialStatus.lotSerialNbr>>>>();
			}

			switch (((INLotSerClass)item).LotSerIssueMethod)
			{
				case INLotSerIssueMethod.FIFO:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.receiptDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
					break;
				case INLotSerIssueMethod.LIFO:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Desc<INLotSerialStatus.receiptDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
					break;
				case INLotSerIssueMethod.Expiration:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.expireDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
					break;
				case INLotSerIssueMethod.Sequential:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.lotSerialNbr>>>>();
					break;
				case INLotSerIssueMethod.UserEnterable:
					cmd.WhereAnd<Where<boolTrue, Equal<boolFalse>>>();
					break;
				default:
					throw new PXException();
			}

			return cmd;
		}

	}

	public class POLinePlanIDAttribute : INItemPlanIDAttribute
	{
		#region Ctor
		public POLinePlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry)
			: base(ParentNoteID, ParentHoldEntry)
		{
		}
		#endregion
		#region Implementation
		public override void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<POOrder.status, POOrder.cancelled>(e.Row, e.OldRow))
			{
				POOrder order = (POOrder) e.Row;			
				PXCache plancache = sender.Graph.Caches[typeof(INItemPlan)];
				bool Cancelled = (bool)sender.GetValue<POOrder.cancelled>(e.Row);
				
				foreach (POLine split in PXSelect<POLine, 
					Where<POLine.orderType, Equal<Current<POOrder.orderType>>, 
						And<POLine.orderNbr, Equal<Current<POOrder.orderNbr>>>>>.Select(sender.Graph))
				{
					foreach (INItemPlan plan in plancache.Inserted)
					{
						if (object.Equals(plan.PlanID, split.PlanID))
						{
							if (Cancelled)
							{
								plancache.Delete(plan);
							}
							else
							{
								INItemPlan copy = PXCache<INItemPlan>.CreateCopy(plan);

								bool IsOnHold = order.Status != POOrderStatus.Open && order.Status != POOrderStatus.Closed;
								switch (plan.PlanType)
								{
									case INPlanConstants.Plan70:
									case INPlanConstants.Plan73:
										plan.PlanType = IsOnHold ? INPlanConstants.Plan73 : INPlanConstants.Plan70;
										break;
									case INPlanConstants.Plan76:
									case INPlanConstants.Plan78:
										plan.PlanType = IsOnHold ? INPlanConstants.Plan78 : INPlanConstants.Plan76;
										break;
									case INPlanConstants.Plan74:
									case INPlanConstants.Plan79:
										plan.PlanType = IsOnHold ? INPlanConstants.Plan79 : INPlanConstants.Plan74;
										break;
								}

								plan.Hold = IsOnHold;
								if (!string.Equals(copy.PlanType, plan.PlanType))
									plancache.RaiseRowUpdated(plan, copy);
							}
						}
					}					
				}
				
				foreach (INItemPlan plan in PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<POOrder.noteID>>>>.Select(sender.Graph))
				{
					INItemPlan copy = PXCache<INItemPlan>.CreateCopy(plan);
					if(Cancelled)
                    {
                        plancache.Delete(plan);
                    }
					else
					{
						bool IsOnHold = order.Status != POOrderStatus.Open && order.Status != POOrderStatus.Closed;
						switch (plan.PlanType)
						{
							case INPlanConstants.Plan70:
							case INPlanConstants.Plan73:
								plan.PlanType = IsOnHold ? INPlanConstants.Plan73 : INPlanConstants.Plan70;
								break;
							case INPlanConstants.Plan76:
							case INPlanConstants.Plan78:
								plan.PlanType = IsOnHold ? INPlanConstants.Plan78 : INPlanConstants.Plan76;
								break;
							case INPlanConstants.Plan74:
							case INPlanConstants.Plan79:
								plan.PlanType = IsOnHold ? INPlanConstants.Plan79 : INPlanConstants.Plan74;
								break;
						}

						plan.Hold = IsOnHold;

						if (!string.Equals(copy.PlanType, plan.PlanType))
							plancache.RaiseRowUpdated(plan, copy);

						if (plancache.GetStatus(plan) == PXEntryStatus.Notchanged || plancache.GetStatus(plan) == PXEntryStatus.Held)
						{
							plancache.SetStatus(plan, PXEntryStatus.Updated);
						}
					}
				}
			}
		}

		public override void Parent_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				_KeyToAbort = sender.GetValue<POOrder.orderNbr>(e.Row);
			}
		}

		public override void Parent_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open && _KeyToAbort != null)
			{
				foreach (POLine split in PXSelect<POLine, Where<POLine.orderType, Equal<Required<POOrder.orderType>>, And<POLine.orderNbr, Equal<Required<POOrder.orderNbr>>>>>.Select(sender.Graph, ((POOrder)e.Row).OrderType, _KeyToAbort))
				{
					foreach (INItemPlan plan in sender.Graph.Caches[typeof(INItemPlan)].Inserted)
					{
						if (object.Equals(plan.PlanID, split.PlanID))
						{
							plan.RefNoteID = (long?)sender.GetValue(e.Row, _ParentNoteID.Name);
						}
					}
				}
			}
			_KeyToAbort = null;
		}

		public override INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row)
		{
			POLine split_Row = (POLine)orig_Row;
			if (split_Row.OrderType != POOrderType.RegularOrder && 
				  split_Row.OrderType != POOrderType.DropShip &&
				  split_Row.OrderType != POOrderType.Blanket &&
					split_Row.OrderType != POOrderType.Transfer)
			{
				return null;
			}

			if (split_Row.InventoryID == null || split_Row.SiteID == null)
			{
				return null;
			}
			PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ParentNoteID)];
			string status = (string)cache.GetValue<POOrder.status>(cache.Current);	
			bool IsOnHold = status != POOrderStatus.Open && status != POOrderStatus.Closed;

			if (split_Row.OrderType == POOrderType.Transfer)
			{
				plan_Row.FixedSource = INReplenishmentSource.Transfer;
				plan_Row.VendorID = (int?)cache.GetValue<POOrder.vendorID>(cache.Current);
				plan_Row.VendorLocationID = (int?)cache.GetValue<POOrder.vendorLocationID>(cache.Current);

				INItemSiteSettings settings = PXSelect<INItemSiteSettings,
					Where<INItemSiteSettings.inventoryID, Equal<Current<POLine.inventoryID>>,
						And<INItemSiteSettings.siteID, Equal<Current<POLine.siteID>>>>>
						.SelectSingleBound(sender.Graph, new object[] {split_Row});
				if (settings != null)
					plan_Row.SourceSiteID = settings.ReplenishmentSourceSiteID;
			}

			if(split_Row.OrderType == POOrderType.Blanket)
				plan_Row.PlanType = INPlanConstants.Plan7B;			
			else
				switch (split_Row.LineType)
				{
					case POLineType.GoodsForSalesOrder:
					case POLineType.NonStockForSalesOrder:
						plan_Row.PlanType = IsOnHold ? INPlanConstants.Plan78 : INPlanConstants.Plan76;
						break;
					case POLineType.GoodsForDropShip:
					case POLineType.NonStockForDropShip:
						plan_Row.PlanType = IsOnHold ? INPlanConstants.Plan79 : INPlanConstants.Plan74;
						break;
					case POLineType.GoodsForInventory:
					case POLineType.GoodsForReplenishment:
					case POLineType.NonStock:
						plan_Row.PlanType = IsOnHold ? INPlanConstants.Plan73 : INPlanConstants.Plan70;
						break;
					default:
						return null;
				}
			plan_Row.BAccountID = split_Row.VendorID;
			plan_Row.InventoryID = split_Row.InventoryID;
			plan_Row.SubItemID = split_Row.SubItemID;
			plan_Row.SiteID = split_Row.SiteID;
			plan_Row.PlanDate = split_Row.PromisedDate;
			plan_Row.PlanQty = ((bool)split_Row.Cancelled || (bool)split_Row.Completed) ? Decimal.Zero : split_Row.BaseOpenQty;
			
			plan_Row.RefNoteID = (long?)cache.GetValue(cache.Current, _ParentNoteID.Name);
			plan_Row.Hold = IsOnHold;

			if (plan_Row.RefNoteID < 0L)
			{
				plan_Row.RefNoteID = null;
			}

			if (string.IsNullOrEmpty(plan_Row.PlanType))
			{
				return null;
			}
			return plan_Row;
		}

		public override void Plan_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				if (e.TranStatus == PXTranStatus.Open)
				{
					foreach (INItemPlan poplan in sender.Updated)
					{
						if (poplan.SupplyPlanID == (long?)_SelfKeyToAbort && _SelfKeyToAbort != null)
						{
							poplan.SupplyPlanID = ((INItemPlan)e.Row).PlanID;
						}
					}
				}
				else if (e.TranStatus == PXTranStatus.Aborted)
				{
					foreach (INItemPlan poplan in sender.Updated)
					{
						if (poplan.SupplyPlanID != null && _persisted.TryGetValue(poplan.SupplyPlanID, out _SelfKeyToAbort))
						{
							poplan.SupplyPlanID = (long?)_SelfKeyToAbort;
						}
					}
				}
			}

			base.Plan_RowPersisted(sender, e);
		}
		#endregion
	}

	public class POLineRPlanIDAttribute : INItemPlanIDAttribute
	{
		#region Ctor
		public POLineRPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry)
			: base(ParentNoteID, ParentHoldEntry)
		{
		}
		#endregion
		#region Implementation
		public override void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (e.Row != null && e.OldRow != null && (
				(bool?)sender.GetValue(e.Row, _ParentHoldEntry.Name) != (bool?)sender.GetValue(e.OldRow, _ParentHoldEntry.Name)))
			{
				PXCache cache = sender.Graph.Caches[typeof(POLineUOpen)];
				foreach (POLineUOpen split in PXSelect<POLineUOpen, Where<POLineUOpen.orderType, Equal<Required<POOrder.orderType>>, And<POLine.orderNbr, Equal<Required<POOrder.orderNbr>>>>>.Select(sender.Graph, ((POOrder)e.Row).OrderType, ((POOrder)e.Row).OrderNbr))
				{
					cache.RaiseRowUpdated(split, PXCache<POLineUOpen>.CreateCopy(split));
				}
			}
		}

		public override void Parent_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				_KeyToAbort = sender.GetValue<POOrder.orderNbr>(e.Row);
			}
		}

		public override void Parent_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open && _KeyToAbort != null)
			{
				foreach (POLineUOpen split in PXSelect<POLineUOpen, Where<POLineUOpen.orderType, Equal<Required<POOrder.orderType>>, And<POLine.orderNbr, Equal<Required<POOrder.orderNbr>>>>>.Select(sender.Graph, ((POOrder)e.Row).OrderType, _KeyToAbort))
				{
					foreach (INItemPlan plan in sender.Graph.Caches[typeof(INItemPlan)].Inserted)
					{
						if (object.Equals(plan.PlanID, split.PlanID))
						{
							plan.RefNoteID = (long?)sender.GetValue(e.Row, _ParentNoteID.Name);
						}
					}
				}
			}
			_KeyToAbort = null;
		}

		public override INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row)
		{
			POLineUOpen split_Row = (POLineUOpen)orig_Row;
			if (split_Row.OrderType != POOrderType.RegularOrder && split_Row.OrderType != POOrderType.DropShip)
			{
				return null;
			}
			if (split_Row.InventoryID == null || split_Row.SiteID == null)
			{
				return null;
			}
			switch (split_Row.LineType)
			{
				case POLineType.GoodsForSalesOrder:
				case POLineType.NonStockForSalesOrder:
				case POLineType.GoodsForDropShip:
				case POLineType.NonStockForDropShip:
				case POLineType.GoodsForInventory:
				case POLineType.GoodsForReplenishment:
				case POLineType.NonStock:
					if (plan_Row != null)
						plan_Row.PlanQty = (split_Row.Cancelled ==true || split_Row.Completed == true) ? Decimal.Zero : split_Row.BaseOpenQty;

					return plan_Row;
				default:
					return null;
			}
		}
		

		protected ObjectRef<POLineUOpen> _Current;
		protected POLineUOpen Current
		{
			get
			{
				return _Current.Value;
			}
			set
			{
				_Current.Value = value;
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			//should precede base method invocation
			if (sender.Graph.Views.Caches.Contains(typeof(INItemPlan)))
			{
				sender.Graph.RowInserted.AddHandler<INItemPlan>(Plan_RowInserted);
				sender.Graph.RowUpdated.AddHandler<INItemPlan>(Plan_RowUpdated);
				sender.Graph.RowDeleted.AddHandler<INItemPlan>(Plan_RowDeleted);
			}

			base.CacheAttached(sender);

			_Current = new ObjectRef<POLineUOpen>();
		}

		public override void Plan_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
            bool CurrentIsNull = (Current == null);
            Current = Current ?? PXSelect<POLineUOpen, Where<POLineUOpen.planID, Equal<Required<POLineUOpen.planID>>>>.Select(sender.Graph, ((INItemPlan)e.Row).PlanID);
			try
			{
				base.Plan_RowDeleted(sender, e);
			}
			finally
			{
                if (CurrentIsNull)
                {
                    Current = null;
                }
			}
		}
		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Current = (POLineUOpen)e.Row;
			try
			{
				base.RowInserted(sender, e);
			}
			finally
			{
				Current = null;
			}
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Current = (POLineUOpen)e.Row;
			try
			{
				base.RowUpdated(sender, e);
			}
			finally
			{
				Current = null;
			}
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			Current = (POLineUOpen)e.Row;
			try
			{
				base.RowDeleted(sender, e);
			}
			finally
			{
				Current = null;
			}
		}

		protected override INPlanType GetTargetPlanType<TNode>(PXGraph graph, INItemPlan plan, INPlanType plantype)
		{
			//in release po receipts POLineRPlanID() coexist with POReceiptLineSplitPlanID() thus 2 subscribers for plan cache events would exist
			if (Current == null)
			{
				INPlanType ret = new INPlanType();
				return ret ^ ret;
			}
			return base.GetTargetPlanType<TNode>(graph, plan, plantype);
		}
		#endregion
	}

	public class POReceiptLineSplitPlanIDAttribute : INItemPlanIDAttribute
	{
		#region State
		protected Type _ParentOrderDate;
		#endregion
		#region Ctor
		public POReceiptLineSplitPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry, Type ParentOrderDate)
			: base(ParentNoteID, ParentHoldEntry)
		{
			this._ParentOrderDate = ParentOrderDate;
		}
		#endregion
		#region Implementation
		public override void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<POReceipt.receiptDate, POReceipt.hold>(e.Row, e.OldRow))
			{
				PXCache plancache = sender.Graph.Caches[typeof(INItemPlan)];
				foreach (POReceiptLineSplit split in PXSelect<POReceiptLineSplit, Where<POReceiptLineSplit.receiptNbr, Equal<Current<POReceipt.receiptNbr>>>>.Select(sender.Graph))
				{
					foreach (INItemPlan plan in plancache.Inserted)
					{
						if (object.Equals(plan.PlanID, split.PlanID))
						{
							plan.Hold = (bool?)sender.GetValue<POReceipt.hold>(e.Row);
							plan.PlanDate = (DateTime?)sender.GetValue<POReceipt.receiptDate>(e.Row);
						}
					}
				}

				foreach(INItemPlan plan in PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<POReceipt.noteID>>>>.Select(sender.Graph))
				{
					plan.Hold = (bool?)sender.GetValue<POReceipt.hold>(e.Row);
					plan.PlanDate = (DateTime?)sender.GetValue<POReceipt.receiptDate>(e.Row);

					if (plancache.GetStatus(plan) == PXEntryStatus.Notchanged || plancache.GetStatus(plan) == PXEntryStatus.Held)
					{
						plancache.SetStatus(plan, PXEntryStatus.Updated);
					}
				}
			}
		}

		public override void Parent_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				_KeyToAbort = sender.GetValue<POReceipt.receiptNbr>(e.Row);
			}
		}

		public override void Parent_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open && _KeyToAbort != null)
			{
				foreach (POReceiptLineSplit split in PXSelect<POReceiptLineSplit, 
						Where<POReceiptLineSplit.receiptNbr, Equal<Required<POReceipt.receiptNbr>>>>.Select(sender.Graph, _KeyToAbort))
				{
					foreach (INItemPlan plan in sender.Graph.Caches[typeof(INItemPlan)].Inserted)
					{
						if (object.Equals(plan.PlanID, split.PlanID))
						{
						    plan.RefNoteID = (long?)sender.GetValue(e.Row, _ParentNoteID.Name);
						}
					}
				}
			}
			_KeyToAbort = null;
		}

		public override INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row)
		{
			POReceiptLineSplit split_Row = (POReceiptLineSplit)orig_Row;

			PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ParentNoteID)];
			DateTime? receiptDate = (DateTime?)cache.GetValue(cache.Current, this._ParentOrderDate.Name);
			POReceiptLine parent = (POReceiptLine)PXParentAttribute.SelectParent(sender, orig_Row);
			plan_Row.BAccountID = parent.VendorID;

			switch (split_Row.LineType)
			{
				case POLineType.GoodsForInventory:
				case POLineType.GoodsForReplenishment:
					if (split_Row.ReceiptType == POReceiptType.POReceipt)
					{
						plan_Row.PlanType = INPlanConstants.Plan71;
					}
					else
					{
						plan_Row.PlanType = INPlanConstants.Plan72;
					}
					break;
				case POLineType.GoodsForSalesOrder:
					if (split_Row.ReceiptType == POReceiptType.POReceipt)
					{
						plan_Row.PlanType = INPlanConstants.Plan77;
					}
					else
					{
						throw new PXException();
					}
					break;
				case POLineType.GoodsForDropShip:
					if (split_Row.ReceiptType == POReceiptType.POReceipt)
					{
						plan_Row.PlanType = INPlanConstants.Plan75;
					}
					else
					{
						throw new PXException();
					} 
					break;
				default:
					return null;
			}
			plan_Row.InventoryID= split_Row.InventoryID;
			plan_Row.SubItemID = split_Row.SubItemID;
			plan_Row.SiteID = split_Row.SiteID;
			plan_Row.LocationID = split_Row.LocationID;
			plan_Row.LotSerialNbr = split_Row.LotSerialNbr;
			if (string.IsNullOrEmpty(split_Row.AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(split_Row.AssignedNbr, split_Row.LotSerialNbr))
			{
				plan_Row.LotSerialNbr = null;
			}
			plan_Row.PlanDate = receiptDate;
			plan_Row.PlanQty = split_Row.BaseQty;

			plan_Row.RefNoteID = (long?)cache.GetValue(cache.Current, _ParentNoteID.Name);
			plan_Row.Hold = (bool?)cache.GetValue(cache.Current, _ParentHoldEntry.Name);

			if (plan_Row.RefNoteID < 0L)
			{
				plan_Row.RefNoteID = null;
			}
			if (string.IsNullOrEmpty(plan_Row.PlanType))
			{
				return null;
			}
			return plan_Row;
		}
	
		protected ObjectRef<POReceiptLineSplit> _Current;
		protected POReceiptLineSplit Current
		{
			get
			{
				return _Current.Value;
			}
			set
			{
				_Current.Value = value;
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			//should precede base method invocation
			if (sender.Graph.Views.Caches.Contains(typeof(INItemPlan)))
			{
				sender.Graph.RowInserted.AddHandler<INItemPlan>(Plan_RowInserted);
				sender.Graph.RowUpdated.AddHandler<INItemPlan>(Plan_RowUpdated);
				sender.Graph.RowDeleted.AddHandler<INItemPlan>(Plan_RowDeleted);
			}

			base.CacheAttached(sender);

			_Current = new ObjectRef<POReceiptLineSplit>();
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Current = (POReceiptLineSplit)e.Row;

			try
			{
				base.RowInserted(sender, e);
			}
			finally
			{
				Current = null;
			}
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Current = (POReceiptLineSplit)e.Row;

			try
			{
			base.RowUpdated(sender, e);
			}
			finally
			{
				Current = null;
			}
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			Current = (POReceiptLineSplit)e.Row;

			try
			{
				base.RowDeleted(sender, e);
			}
			finally
			{
				Current = null;
			}
		}

		public override void Plan_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
            bool CurrentIsNull = (Current == null);
            Current = Current ?? PXSelect<POReceiptLineSplit, Where<POReceiptLineSplit.planID, Equal<Required<POReceiptLineSplit.planID>>>>.Select(sender.Graph, ((INItemPlan)e.Row).PlanID);
			
			try
			{
				base.Plan_RowDeleted(sender, e);
			}
			finally
			{
                if (CurrentIsNull)
                {
                    Current = null;
                }
			}
		}

		public override decimal GetInclQtyAvail<TNode>(PXGraph graph, object data)
		{
			Current = (POReceiptLineSplit)data;
			try
			{
				return base.GetInclQtyAvail<TNode>(graph, data);
			}
			finally
			{
				Current = null;
			}
		}

		protected override INPlanType GetTargetPlanType<TNode>(PXGraph graph, INItemPlan plan, INPlanType plantype)
		{
			//in release po receipts POLineRPlanID() coexist with POReceiptLineSplitPlanID() thus 2 subscribers for plan cache events would exist
			if (Current == null || plantype == null)
			{
				INPlanType ret = new INPlanType();
				return ret ^ ret;
			}

			string POPlanType;
			switch (plantype.PlanType)
			{
				case INPlanConstants.Plan71:
				case INPlanConstants.Plan72:
					POPlanType = INPlanConstants.Plan70;
					break;
				case INPlanConstants.Plan77:
					POPlanType = INPlanConstants.Plan76;
					break;
				case INPlanConstants.Plan75:
					POPlanType = INPlanConstants.Plan74;
					break;
				default:
					throw new PXException();
			}

			INPlanType newplantype = plantype;
			if (typeof(TNode) != typeof(SiteStatus))
			{
				//Exclude PO Plan options for Locations and LotSerial - they are never presents for PO
				INPlanType poOrderType = (INPlanType)PXSelect<INPlanType, Where<INPlanType.planType, Equal<Required<INPlanType.planType>>>>.Select(graph, POPlanType);
				return plantype * (poOrderType ^ plantype);
			}
			else 
			{
				//Do Plans XOR for site too if there is no reference to POOrder
				if (string.IsNullOrEmpty(Current.PONbr))
				{
					INPlanType poOrderType = (INPlanType)PXSelect<INPlanType, Where<INPlanType.planType, Equal<Required<INPlanType.planType>>>>.Select(graph, POPlanType);
					return plantype * (poOrderType ^ plantype);
				}			
			}
			return plantype;
		}
		#endregion
	}

    /// <summary>
    /// Specialized for POLine version of the CrossItemAttribute.<br/> 
    /// Providing an Inventory ID selector for the field, it allows also user <br/>
    /// to select both InventoryID and SubItemID by typing AlternateID in the control<br/>
    /// As a result, if user type a correct Alternate id, values for InventoryID, SubItemID, <br/>
    /// and AlternateID fields in the row will be set.<br/>
    /// In this attribute, InventoryItems with a status inactive, markedForDeletion,<br/>
    /// noPurchase and noRequest are filtered out. It also fixes  INPrimaryAlternateType parameter to VPN <br/>    
    /// This attribute may be used in combination with AlternativeItemAttribute on the AlternateID field of the row <br/>
    /// <example>
    /// [POLineInventoryItem(Filterable = true)]
    /// </example>
    /// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where2<Match<Current<AccessInfo.userName>>,
							 And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
									And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>,
								And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noPurchases>,
									And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noRequest>>>>>>), IN.Messages.ItemCannotPurchase)]
	public class POLineInventoryItemAttribute : CrossItemAttribute
	{
	
        /// <summary>
        /// Default ctor
        /// </summary>
		public POLineInventoryItemAttribute()
			: base(typeof(Where<boolTrue, Equal<boolTrue>>), INPrimaryAlternateType.VPN)
		{
		}

        /// <summary>
        /// Extended ctor. User may specified additional Where clause for the InventoryItem Selector, which will be combined with the default one.
        /// </summary>		
        /// <param name="Where"> Must be IBqlWhere type. Allows to specify additional where criteria for select.</param>
		public POLineInventoryItemAttribute(Type Where)
			: base(BqlCommand.Compose(typeof(Where<>),Where), INPrimaryAlternateType.VPN)
		{
		}

        /// <summary>
        /// Extended ctor. User may specified both Where  and Join clause for the InventoryItem Selector, which will be combined with the default one.
        /// </summary>	
        /// <param name="JoinType">Must be IBqlJoin type. Allows to join other tables to select.</param>
        /// <param name="Where"> Must be IBqlWhere type. Allows to specify additional where criteria for select.</param>
		public POLineInventoryItemAttribute(Type JoinType, Type Where)
			: base(JoinType,BqlCommand.Compose(typeof(Where<>), Where), INPrimaryAlternateType.VPN)
		{

		}
	}

	#region POOpenPeriod
	public class POOpenPeriodAttribute : OpenPeriodAttribute
	{
		#region Ctor
		public POOpenPeriodAttribute(Type SourceType)
			: base(typeof(Search<FinPeriod.finPeriodID, Where<FinPeriod.iNClosed, Equal<False>, And<FinPeriod.aPClosed, Equal<False>, And<FinPeriod.active, Equal<True>>>>>), SourceType)
		{
		}

		public POOpenPeriodAttribute()
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
				if (p.Closed == true)
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
				if (p.INClosed == true)
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
	#endregion

    /// <summary>
    /// This attribute defines, if the vendor and it's location specified 
    /// are the preffered Vendor for the inventory item. May be placed on field of boolean type, 
    /// to display this information dynamically 
    /// <example>
    /// [PODefaultVendor(typeof(POVendorInventory.inventoryID), typeof(POVendorInventory.vendorID), typeof(POVendorInventory.vendorLocationID))]
    /// </example>
    /// </summary>
	public class PODefaultVendor : PXEventSubscriberAttribute, IPXRowSelectingSubscriber
	{
		private Type inventoryID;
		private Type vendorID;
		private Type vendorLocationID;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="inventoryID">Must be IBqlField. Field which contains inventory id, for which Vendor/location is checked for beeng a preferred Vendor</param>
        /// <param name="vendorID">Must be IBqlField. Field which contains VendorID of the vendor checking for beeng a preferred Vendor</param>
        /// <param name="vendorLocationID">Must be IBqlField. Field which contains VendorLocationID of the vendor checking for beeng a preferred Vendor</param>
		public PODefaultVendor(Type inventoryID, Type vendorID, Type vendorLocationID)
		{
			this.inventoryID = inventoryID;
			this.vendorID    = vendorID;
			this.vendorLocationID = vendorLocationID;
		}

		#region IPXRowSelectingSubscriber Members
		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			object itemID = sender.GetValue(e.Row, this.inventoryID.Name);
			object vendorID = sender.GetValue(e.Row, this.vendorID.Name);
			object vendorLocationID = sender.GetValue(e.Row, this.vendorLocationID.Name);

			if (itemID == null || vendorID == null) return;
						
			using (new PXConnectionScope())
			{
				InventoryItem item = PXSelect<InventoryItem, 
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
					.SelectWindowed(sender.Graph, 0, 1, itemID);
				sender.SetValue(e.Row, _FieldName, item != null && object.Equals(item.PreferredVendorID, vendorID) && object.Equals(item.PreferredVendorLocationID, vendorLocationID));	
			}												
		}
		#endregion
	}

	public class POXRefUpdate : PXEventSubscriberAttribute
	{
		public POXRefUpdate(Type inventoryID, Type subItem, Type vendorID)
		{
		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

		}
	}

	public class POVendorInventorySelect<Table, Join, Where, PrimaryType> : PXSelectJoin<Table, Join, Where>
		where Table : POVendorInventory, new()
		where Join : class, PX.Data.IBqlJoin, new()
		where Where : PX.Data.IBqlWhere, new()
		where PrimaryType : class, IBqlTable, new()
	{
		protected const string _UPDATEVENDORPRICE_COMMAND = "UpdateVendorPrice";
		protected const string _UPDATEVENDORPRICE_VIEW    = "VendorInventory$UpdatePrice";

		public POVendorInventorySelect(PXGraph graph)
			: base(graph)
		{
			graph.Views.Caches.Add(typeof(INItemXRef));
			graph.RowSelected.AddHandler<Table>(OnRowSelected);
			graph.RowInserted.AddHandler<Table>(OnRowInserted);
			graph.RowUpdated.AddHandler<Table>(OnRowUpdated);
			graph.RowDeleted.AddHandler<Table>(OnRowDeleted);
			graph.RowPersisting.AddHandler<Table>(OnRowPersisting);
			graph.RowSelected.AddHandler<PrimaryType>(OnParentRowSelected); 
			
			var filter = new PXFilter<POVendorPriceUpdate>(graph);
			graph.Views.Add(_UPDATEVENDORPRICE_VIEW, filter.View);				
			AddAction(graph, _UPDATEVENDORPRICE_COMMAND, Messages.UpdateEffectivePrice, UpdateVendorPrice);
		}

		public POVendorInventorySelect(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
			graph.Views.Caches.Add(typeof(INItemXRef));
			graph.RowSelected.AddHandler<Table>(OnRowSelected);
			graph.RowInserted.AddHandler<Table>(OnRowInserted);
			graph.RowUpdated.AddHandler<Table>(OnRowUpdated);
			graph.RowDeleted.AddHandler<Table>(OnRowDeleted);
			graph.RowPersisting.AddHandler<Table>(OnRowPersisting);
			graph.RowSelected.AddHandler<PrimaryType>(OnParentRowSelected);

			var filter = new PXFilter<POVendorPriceUpdate>(graph);
			graph.Views.Add(_UPDATEVENDORPRICE_VIEW, filter.View);
			AddAction(graph, _UPDATEVENDORPRICE_COMMAND, Messages.UpdateEffectivePrice, UpdateVendorPrice);
		}

		private void AddAction(PXGraph graph, string name, string displayName, PXButtonDelegate handler)
		{
			var uiAtt = new PXUIFieldAttribute { DisplayName = PXMessages.LocalizeNoPrefix(displayName)};
			graph.Actions[name] = (PXAction)Activator.CreateInstance(typeof(PXNamedAction<>).MakeGenericType(
				new Type[] { typeof(PrimaryType) }), 
				new object[] { graph, name, handler, uiAtt });
		}

		protected virtual InventoryItem ReadInventory(object current)
		{					
			return PXSelect<InventoryItem,
				Where<InventoryItem.inventoryID, Equal<Current<POVendorInventory.inventoryID>>>>
				.SelectSingleBound(this._Graph, new object[] {current});			
		}

		protected virtual void OnRowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			Table current = e.Row as Table;
			if (current == null) return;
			INSetup setup = (INSetup)cache.Graph.Caches[typeof(INSetup)].Current;
			if (setup != null && setup.UseInventorySubItem == true)
			{
				InventoryItem item = ReadInventory(current);
				if (item != null && item.DefaultSubItemID == null && item.StkItem == true)
					current.OverrideSettings = true;
			}
			UpdateXRef(current);			
		}

		protected virtual void OnParentRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INSetup setup = (INSetup)sender.Graph.Caches[typeof(INSetup)].Current;
			PXUIFieldAttribute.SetVisible<POVendorInventory.overrideSettings>
				(sender.Graph.Caches[typeof(POVendorInventory)], null, setup.UseInventorySubItem == true);
		}
		protected virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Table row = (Table)e.Row;
			CMSetup cm = (CMSetup)sender.Graph.Caches[typeof(CMSetup)].Current;						
			PXUIFieldAttribute.SetVisible<POVendorInventory.curyID>(sender, null, cm != null && cm.MCActivated == true);

			if (row == null) return;
			INSetup setup = (INSetup)sender.Graph.Caches[typeof(INSetup)].Current;
			
			InventoryItem item = ReadInventory(row);

			bool isEnabled =
				row.OverrideSettings == true || 
				item == null ||
				setup.UseInventorySubItem != true || 				
				item.DefaultSubItemID == row.SubItemID;
			PXUIFieldAttribute.SetEnabled<POVendorInventory.overrideSettings>(sender, row, setup.UseInventorySubItem == true && item != null && item.StkItem == true);
			PXUIFieldAttribute.SetEnabled<POVendorInventory.addLeadTimeDays>(sender, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<POVendorInventory.eRQ>(sender, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<POVendorInventory.lotSize>(sender, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<POVendorInventory.maxOrdQty>(sender, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<POVendorInventory.minOrdFreq>(sender, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<POVendorInventory.minOrdQty>(sender, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<POVendorInventory.subItemID>(sender, row, item != null && item.StkItem == true);
									
		}
		
		protected virtual void OnRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Table current = e.Row as Table;
			Table old     = e.OldRow as Table;
			if (current == null) return;
			
			InventoryItem item = ReadInventory(current);
	
			if (item != null && item.DefaultSubItemID != null && item.DefaultSubItemID == current.SubItemID)
			{				
				foreach (POVendorInventory vi in
					PXSelect<POVendorInventory,
					Where<POVendorInventory.vendorID, Equal<Required<POVendorInventory.vendorID>>,
						And2<Where<POVendorInventory.vendorLocationID, Equal<Required<POVendorInventory.vendorLocationID>>,
									 Or<Where<Required<POVendorInventory.vendorLocationID>, IsNull, And<POVendorInventory.vendorLocationID, IsNull>>>>,
						And<POVendorInventory.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
						And<POVendorInventory.subItemID, NotEqual<Required<POVendorInventory.subItemID>>,
						And<POVendorInventory.overrideSettings, Equal<boolFalse>>>>>>>
						.Select(sender.Graph, current.VendorID, current.VendorLocationID, current.VendorLocationID, current.InventoryID, current.SubItemID))
				{
					if (vi.RecordID == current.RecordID) continue;
					POVendorInventory rec = PXCache<POVendorInventory>.CreateCopy(vi);								
					rec.AddLeadTimeDays = current.AddLeadTimeDays;
					rec.ERQ = current.ERQ;
					rec.VLeadTime = current.VLeadTime;
					rec.LotSize = current.LotSize;
					rec.MaxOrdQty = current.MaxOrdQty;
					rec.MinOrdFreq = current.MinOrdFreq;
					rec.MinOrdQty = current.MinOrdQty;
					sender.Update(rec);			
				}				
			}

			if (current.PendingPrice != old.PendingPrice && 
					current.PendingPrice > 0 && current.PendingDate == null)
				current.PendingDate = sender.Graph.Accessinfo.BusinessDate;

			if (!IsEqualByItemXRef(current, old))
			{
				DeleteXRef(old);
				UpdateXRef(current);
			}
		}

		protected virtual void OnRowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			DeleteXRef((Table)e.Row);
		}

		protected virtual void OnRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if(e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				InventoryItem item = ReadInventory(e.Row);
				PXDefaultAttribute.SetPersistingCheck<POVendorInventory.subItemID>(sender, e.Row, item == null || item.StkItem == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			}
		}

		protected virtual IEnumerable UpdateVendorPrice(PXAdapter adapter)
		{
			if (adapter.View.Graph.Views[_UPDATEVENDORPRICE_VIEW].Cache.Current == null)
				adapter.View.Graph.Views[_UPDATEVENDORPRICE_VIEW].Cache.Current = new POVendorPriceUpdate();

			if (this.View.AskExt(true) == WebDialogResult.OK)
			{
				POVendorPriceUpdate filter = (POVendorPriceUpdate)adapter.View.Graph.Views[_UPDATEVENDORPRICE_VIEW].Cache.Current;
				if(filter != null)
					foreach (PrimaryType source in adapter.Get())
					{
						foreach (PXResult<POVendorInventory> result in this.View.SelectMultiBound(new object[] { source }))
						{
							POVendorInventory item = (POVendorInventory)result;
							if (item.PendingDate != null && 
									item.PendingDate <= filter.PendingDate)
							{
								POVendorInventory upd = (POVendorInventory)this.View.Cache.CreateCopy(item);
								upd.LastPrice = item.EffPrice > 0 ? item.EffPrice : upd.LastPrice;
								upd.EffPrice = item.PendingPrice;
								upd.EffDate = item.PendingDate;
								upd.PendingPrice = 0m;
								upd.PendingDate = null;
								this.View.Cache.Update(upd);
							}
						}					
					}
			}
			return adapter.Get();
		}

		static bool IsEqualByItemXRef(Table op1, Table op2)
		{
			return (op1.VendorID == op2.VendorID
				&& op1.InventoryID == op2.InventoryID
				&& op1.SubItemID == op2.SubItemID
				&& op1.VendorInventoryID == op2.VendorInventoryID);
		}

		private void DeleteXRef(Table doc)
		{
			PXCache cache = _Graph.Caches[typeof(INItemXRef)];
			if (doc.InventoryID.HasValue && doc.SubItemID.HasValue && doc.VendorID.HasValue
						&& !String.IsNullOrEmpty(doc.VendorInventoryID))
			{
				foreach (INItemXRef it in PXSelect<INItemXRef,
										Where<INItemXRef.alternateID, Equal<Required<POVendorInventory.vendorInventoryID>>,
										And<INItemXRef.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
										And<INItemXRef.subItemID, Equal<Required<POVendorInventory.subItemID>>,
										And<INItemXRef.bAccountID, Equal<Required<POVendorInventory.vendorID>>,
										And<INItemXRef.alternateType, Equal<INAlternateType.vPN>>>>>>>.										
										Select(_Graph, doc.VendorInventoryID, doc.InventoryID, doc.SubItemID, doc.VendorID))
				{
					cache.Delete(it);
				}
			}
		}
		private void UpdateXRef(Table doc)
		{
			PXCache cache = _Graph.Caches[typeof(INItemXRef)];
			if (doc.InventoryID.HasValue && doc.SubItemID.HasValue && doc.VendorID.HasValue
						&& !String.IsNullOrEmpty(doc.VendorInventoryID))
			{
				INItemXRef itemXRef = null;
				INItemXRef globalXRef = null;
				foreach (INItemXRef it in PXSelect<INItemXRef,
										Where<INItemXRef.alternateID, Equal<Required<POVendorInventory.vendorInventoryID>>,
										And<INItemXRef.inventoryID, Equal<Required<POVendorInventory.inventoryID>>,
										And<INItemXRef.subItemID, Equal<Required<POVendorInventory.subItemID>>,
										And<Where2<Where<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>,
											And<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>>>,
										Or<Where<INItemXRef.alternateType, Equal<INAlternateType.vPN>,
										And<INItemXRef.bAccountID, Equal<Required<POVendorInventory.vendorID>>
										>>>>>>>>, OrderBy<Asc<INItemXRef.alternateType>>>.
										Select(_Graph, doc.VendorInventoryID, doc.InventoryID, doc.SubItemID, doc.VendorID))
				{
					if (it.AlternateType == INAlternateType.VPN)
					{
						itemXRef = it;
					}
					else
					{
						if (globalXRef == null)
							globalXRef = it;
					}
				}
				if (itemXRef == null)
				{
					if (globalXRef == null)
					{
						itemXRef = new INItemXRef();
						Copy(itemXRef, doc);
						itemXRef = (INItemXRef)cache.Insert(itemXRef);
					}
				}
				else
				{
					INItemXRef itemXRef2 = (INItemXRef)cache.CreateCopy(itemXRef);
					Copy(itemXRef2, doc);
					itemXRef = (INItemXRef)cache.Update(itemXRef2);
				}
			}
		}

		static void Copy(INItemXRef dest, Table src)
		{
			dest.InventoryID = src.InventoryID;
			if(PXAccess.FeatureInstalled<FeaturesSet.subItem>())
			dest.SubItemID = src.SubItemID;
			dest.BAccountID = src.VendorID;
			dest.AlternateType = INAlternateType.VPN;
			dest.AlternateID = src.VendorInventoryID;						
		}
	}


    /// <summary>
    /// Specialized for Landed Cost version of VendorAttribute.
    /// Displayes only Vendors having LandedCostVendor = true.
    /// Employee and non-active vendors are filtered out
    /// <example>
    /// [LandedCostVendorActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
    /// </example>
    /// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Vendor", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<AP.Vendor.landedCostVendor,Equal<boolTrue>>), Messages.VendorIsNotLandedCostVendor)]
	public class LandedCostVendorActiveAttribute : AP.VendorNonEmployeeActiveAttribute
	{
        /// <summary>
        /// Default ctor.
        /// </summary>
		public LandedCostVendorActiveAttribute()
			: base()
		{
		}

		//public override void Verify(PXCache sender, Vendor item)
		//{
		//    if (item.LandedCostVendor == false)
		//    {
		//        throw new PXException(Messages.VendorIsNotLandedCostVendor);
		//    }
		//}
	}



}

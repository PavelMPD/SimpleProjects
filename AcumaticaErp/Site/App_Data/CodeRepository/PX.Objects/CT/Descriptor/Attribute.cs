using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CS;

namespace PX.Objects.CT
{
	public class ContractLineNbr : PXLineNbrAttribute
	{
		public ContractLineNbr(Type sourceType)
			: base(sourceType)
		{ 
		}

		protected HashSet<object> _justInserted = null;

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_justInserted = new HashSet<object>();
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			base.RowInserted(sender, e);
			_justInserted.Add(e.Row);
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			//the idea is not to decrement linecntr on deletion
			if (_justInserted.Contains(e.Row))
			{
				base.RowDeleted(sender, e);
				_justInserted.Remove(e.Row);
			}
		}
	}

	/// <summary>
	/// Contract Selector. Dispalys all contracts.
	/// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Contract", Visibility = PXUIVisibility.Visible)]
	public class ContractAttribute : AcctSubAttribute
	{		
		public const string DimensionName = "CONTRACT";

		public ContractAttribute()
		{
			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(
				DimensionName,
				typeof(Search2<Contract.contractID, InnerJoin<ContractBillingSchedule, On<ContractBillingSchedule.contractID, Equal<Contract.contractID>>>, Where<Contract.isTemplate, Equal<boolFalse>, And<Contract.baseType, Equal<Contract.ContractBaseType>>>>)
				,typeof(Contract.contractCD)
				, typeof(Contract.contractCD), typeof(Contract.customerID), typeof(Contract.locationID), typeof(Contract.description), typeof(Contract.status), typeof(Contract.expireDate), typeof(ContractBillingSchedule.lastDate), typeof(ContractBillingSchedule.nextDate));

			select.DescriptionField = typeof(Contract.description);
			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public ContractAttribute(Type WhereType)
		{
			Type SearchType =
				BqlCommand.Compose(
				typeof(Search2<,,,>),
				typeof(Contract.contractID),
				typeof(InnerJoin<ContractBillingSchedule, On<ContractBillingSchedule.contractID, Equal<Contract.contractID>>>),
				typeof(Where<,,>),
				typeof(Contract.isTemplate),
				typeof(Equal<boolFalse>),
				typeof(And<,,>),
				typeof(Contract.baseType),
				typeof(Equal<>),
				typeof(Contract.ContractBaseType),
				typeof(And<>),
				WhereType,
				typeof(OrderBy<Desc<Contract.contractCD>>));

			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(Contract.contractCD),
				typeof(Contract.contractCD), typeof(Contract.customerID), typeof(Contract.locationID), typeof(Contract.description), typeof(Contract.status), typeof(Contract.expireDate), typeof(ContractBillingSchedule.lastDate), typeof(ContractBillingSchedule.nextDate));
						
			select.DescriptionField = typeof(Contract.description);
			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}

	/// <summary>
	/// Contract Template Selector. Displays all Contract Templates.
	/// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Contract Template", Visibility = PXUIVisibility.Visible)]
	public class ContractTemplateAttribute : AcctSubAttribute
	{
		public const string DimensionName = "TMCONTRACT";

		public ContractTemplateAttribute()
		{
			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(
				DimensionName,
				typeof(Search<ContractTemplate.contractID, Where<ContractTemplate.isTemplate, Equal<boolTrue>, And<ContractTemplate.baseType, Equal<ContractTemplate.ContractBaseType>>>>)
				, typeof(ContractTemplate.contractCD)
				, typeof(ContractTemplate.contractCD), typeof(ContractTemplate.description), typeof(ContractTemplate.status));

			select.DescriptionField = typeof(ContractTemplate.description);
			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public ContractTemplateAttribute(Type WhereType)
		{
			Type SearchType =
				BqlCommand.Compose(
				typeof(Search<,>),
				typeof(ContractTemplate.contractID),
				typeof(Where<,,>),
				typeof(ContractTemplate.isTemplate),
				typeof(Equal<boolTrue>),
				typeof(And<,,>),
				typeof(ContractTemplate.baseType),
				typeof(Equal<>),
				typeof(ContractTemplate.ContractBaseType),
				typeof(And<>),
				WhereType);

			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(ContractTemplate.contractCD),
				typeof(ContractTemplate.contractCD), typeof(ContractTemplate.description), typeof(ContractTemplate.status));

			select.DescriptionField = typeof(ContractTemplate.description);
			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}

    /// <summary>
    /// Contract Item Selector. Displays all Contract Items.
    /// </summary>
    [PXDBString(InputMask = "", IsUnicode = true)]
    [PXUIField(DisplayName = "Contract Item", Visibility = PXUIVisibility.Visible)]
    public class ContractItemAttribute : AcctSubAttribute
    {
        public const string DimensionName = "CONTRACTITEM";

        public ContractItemAttribute()
        {
            PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(
                DimensionName,
                typeof(Search<ContractItem.contractItemCD>)
                , typeof(ContractItem.contractItemCD)
                , typeof(ContractItem.contractItemCD), typeof(ContractItem.descr), typeof(ContractItem.baseItemID));

            select.DescriptionField = typeof(ContractItem.descr);
            _Attributes.Add(select);
            _SelAttrIndex = _Attributes.Count - 1;
        }

        public ContractItemAttribute(Type WhereType)
        {
            Type SearchType =
                BqlCommand.Compose(
                typeof(Search<,,>),
                typeof(ContractItem.contractItemCD),
                WhereType,
                typeof(OrderBy<Desc<ContractItem.contractItemCD>>));

            PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(DimensionName, SearchType, 
                typeof(ContractItem.contractItemCD)
                , typeof(ContractItem.contractItemCD), typeof(ContractItem.descr), typeof(ContractItem.baseItemID));

            select.DescriptionField = typeof(ContractItem.descr);
            _Attributes.Add(select);
            _SelAttrIndex = _Attributes.Count - 1;
        }
    }

	public class ContractDetailAccumAttribute : PXAccumulatorAttribute
	{
		public ContractDetailAccumAttribute()
		{
			base._SingleRecord = true;
		}
		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			columns.UpdateOnly = true;

			ContractDetailAcum item = (ContractDetailAcum)row;
			columns.Update<ContractDetailAcum.used>(item.Used, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ContractDetailAcum.usedTotal>(item.UsedTotal, PXDataFieldAssign.AssignBehavior.Summarize);

			//columns.Update<ContractDetailAcum.contractID>(item.ContractID, PXDataFieldAssign.AssignBehavior.Initialize);
			//columns.Update<ContractDetailAcum.inventoryID>(item.InventoryID, PXDataFieldAssign.AssignBehavior.Initialize);
			
			return true;
		}
	}

	public static class GroupType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Contract },
				new string[] { Messages.AttributeEntity_Contract }) { }
		}
		public const string Contract = "CONTRACT";

		public class ContractType : Constant<string>
		{
			public ContractType() : base(GroupType.Contract) { }
		}

	}

}

		

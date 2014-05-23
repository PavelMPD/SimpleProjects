using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.IN
{
    [Serializable]
	public class INItemCategory : PX.Data.IBqlTable
	{
		#region CategoryID
		public abstract class categoryID : PX.Data.IBqlField
		{
		}
		protected int? _CategoryID;
		[PXDBInt(IsKey = true)]
		[PXSelector(typeof(INCategory.categoryID), SubstituteKey = typeof(INCategory.categoryCD), DescriptionField = typeof(INCategory.description))]
		[PXUIField(DisplayName = "Category ID")]
		[PXParent(typeof(Select<INCategory, Where<INCategory.categoryID, Equal<Current<INItemCategory.categoryID>>>>))]
		[PXDBLiteDefault(typeof(INCategory.categoryID))]
		public virtual int? CategoryID
		{
			get
			{
				return this._CategoryID;
			}
			set
			{
				this._CategoryID = value;
			}
		}
		#endregion

		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[StockItem(IsKey=true)]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
	}
}

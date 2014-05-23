using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.IN
{
    [Serializable]
	public class INCategoryMaint : PXGraph<INCategoryMaint>
	{
        [Serializable]
		public partial class SelectedNode : IBqlTable
		{
			#region FolderID
			public abstract class folderID : PX.Data.IBqlField
			{
			}
			protected int? _FolderID;
			[PXDBInt(IsKey = true)]
			public virtual int? FolderID
			{
				get
				{
					return this._FolderID;
				}
				set
				{
					this._FolderID = value;
				}
			}
			#endregion			

			#region CategoryID
			public abstract class categoryID : PX.Data.IBqlField
			{
			}
			protected int? _CategoryID;
			[PXDBInt(IsKey = true)]
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
		}

		public PXSelect<INCategory, Where<INCategory.parentID, Equal<Argument<int?>>>, OrderBy<Asc<INCategory.sortOrder>>> Folders;
		public PXSelect<INCategory, Where<INCategory.parentID, Equal<Argument<int?>>>, OrderBy<Asc<INCategory.sortOrder>>> Items;
		public PXSelectJoin<INItemCategory,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INItemCategory.inventoryID>>>,
			Where<INItemCategory.categoryID, Equal<Current<SelectedNode.categoryID>>>,
			OrderBy<Asc<InventoryItem.inventoryCD>>> Members;

		public PXSave<INCategory> Save;
		public PXCancel<INCategory> Cancel;

		protected virtual IEnumerable folders(
			[PXInt]
			int? categoryID
		)
		{
			if (categoryID == null)
			{
				yield return new INCategory()
				{
					CategoryID = 0,
					Description = PXSiteMap.RootNode.Title
				};

			}
			foreach (INCategory item in PXSelect<INCategory,
			 Where<INCategory.parentID,
				Equal<Required<INCategory.categoryID>>>>.Select(this, categoryID))
			{
				if (!string.IsNullOrEmpty(item.Description))
					yield return item;
			}
		}

		protected virtual IEnumerable items(
			[PXInt]
			int? categoryID
		)
		{
            if (categoryID == null && Folders.Current != null)
                categoryID = Folders.Current.CategoryID;

            if (categoryID == null)
            {
                Items.Cache.AllowInsert = false;
                return null;
            }
		    this.CurrentSelected.FolderID = categoryID;

			return PXSelect<INCategory,
				 Where<INCategory.parentID,
					Equal<Required<INCategory.categoryID>>>>.Select(this, categoryID);
		}


		protected virtual void INCategory_SortOrder_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INCategory row = (INCategory)e.Row;
			if (row == null) return;

			e.NewValue = 1;
			e.Cancel = true;

			PXResultset<INCategory> list = Items.Select(row.ParentID);
			if (list.Count > 0)
			{
				INCategory last = list[list.Count - 1];
				e.NewValue = last.SortOrder + 1;
			}
		}

		protected virtual void INCategory_parentID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INCategory row = e.Row as INCategory;
			if (row == null) return;
			e.NewValue = this.CurrentSelected.FolderID ?? 0;
			e.Cancel = true;
		}

        protected virtual void INCategory_CategoryCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            INCategory row = e.Row as INCategory;
            if (row != null && row.CategoryID != null)
            {
                INCategory item = PXSelect<INCategory,
                                      Where<INCategory.categoryCD, Equal<Required<INCategory.categoryCD>>,
                                          And<INCategory.categoryID, NotEqual<Required<INCategory.categoryID>>>>>
                                  .SelectWindowed(this, 0, 1, e.NewValue, row.CategoryID);
                if (item != null)
                    throw new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded, "CategoryID");
            }
        }

		protected virtual void INCategory_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			INCategory row = e.Row as INCategory;
			if (row == null || row.CategoryID == null) return;
			deleteRecurring(row);
		}

        private void deleteRecurring(INCategory map, bool deleteRootNode = false)
		{
			if (map != null)
			{
                foreach (INCategory child in PXSelect<INCategory,
                                                 Where<INCategory.parentID, Equal<Required<INCategory.categoryID>>>>
                                             .Select(this, map.CategoryID))
					deleteRecurring(child);
                if (deleteRootNode)
				    Items.Cache.Delete(map);
			}
		}

		#region Moving Actions
		
		public PXSelect<INCategory, Where<INCategory.categoryID, Equal<Required<INCategory.categoryID>>>> Item;
		public PXAction<INCategory> down;
        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageUrl = "~/Icons/NavBar/xps_collapse.gif", DisabledImageUrl = "~/Icons/NavBar/xps_collapseD.gif")]
		public virtual IEnumerable Down(PXAdapter adapter)
		{
			int currentItemIndex;
			PXResultset<INCategory> items =
				SelectSiblings(CurrentSelected.FolderID, CurrentSelected.CategoryID, out currentItemIndex);

			if (currentItemIndex >= 0 && currentItemIndex < items.Count - 1)
			{
				INCategory current = items[currentItemIndex];
				INCategory next = items[currentItemIndex + 1];

				current.SortOrder += 1;
				next.SortOrder -= 1;

				Items.Update(current);
				Items.Update(next);
			}
			return adapter.Get();
		}

		public PXAction<INCategory> up;
        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageUrl = "~/Icons/NavBar/xps_expand.gif", DisabledImageUrl = "~/Icons/NavBar/xps_expandD.gif")]
		public virtual IEnumerable Up(PXAdapter adapter)
		{
			int currentItemIndex;
			PXResultset<INCategory> items =
				SelectSiblings(CurrentSelected.FolderID, CurrentSelected.CategoryID, out currentItemIndex);
			if (currentItemIndex > 0)
			{
				INCategory current = items[currentItemIndex];
				INCategory prev = items[currentItemIndex - 1];

				current.SortOrder -= 1;
				prev.SortOrder += 1;

				Items.Update(current);
				Items.Update(prev);
			}
			return adapter.Get();
		}

		public PXAction<INCategory> left;
        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton/*(ImageUrl = "~/Icons/NavBar/left.gif", DisabledImageUrl = "~/Icons/NavBar/leftD.gif")*/]
		public virtual IEnumerable Left(PXAdapter adapter)
		{
			INCategory current = Item.SelectWindowed(0, 1, CurrentSelected.FolderID);

			if (current != null && current.ParentID != 0)
			{
				INCategory parent = Item.SelectWindowed(0, 1, current.ParentID);
				if (parent != null)
				{
					int parentIndex;
					PXResultset<INCategory> items = SelectSiblings(parent.ParentID, parent.CategoryID, out parentIndex);
					if (parentIndex >= 0)
					{
						INCategory last = items[items.Count - 1];
						current = (INCategory)Items.Cache.CreateCopy(current);
						current.ParentID = parent.ParentID;
						current.SortOrder = last.SortOrder + 1;
						Items.Update(current);
                        PXSelect<INCategory, Where<INCategory.parentID, Equal<Required<INCategory.categoryID>>>>.Clear(this);
					}
				}
			}
			return adapter.Get();
		}

		public PXAction<INCategory> right;
		[PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton/*(ImageUrl = "~/Icons/NavBar/right.gif", DisabledImageUrl = "~/Icons/NavBar/rightD.gif")*/]
		public virtual IEnumerable Right(PXAdapter adapter)
		{
			INCategory current = Item.SelectWindowed(0, 1, CurrentSelected.FolderID);
			if (current != null)
			{
				int currentItemIndex;
				PXResultset<INCategory> items =
					SelectSiblings(current.ParentID, current.CategoryID, out currentItemIndex);
				if (currentItemIndex > 0)
				{
					INCategory prev = items[currentItemIndex - 1];
					items = SelectSiblings(prev.CategoryID);
					int index = 1;
					if (items.Count > 0)
					{
						INCategory last = items[items.Count - 1];
						index = (last.SortOrder ?? 0) + 1;
					}
					current = (INCategory)Items.Cache.CreateCopy(current);
					current.ParentID = prev.CategoryID;
					current.SortOrder = index;
					Items.Update(current);
                    PXSelect<INCategory, Where<INCategory.parentID, Equal<Required<INCategory.categoryID>>>>.Clear(this);
				}
			}
			return adapter.Get();
		}


		private PXResultset<INCategory> SelectSiblings(int? patentWGID)
		{
			int currentIndex;
			return SelectSiblings(patentWGID, 0, out currentIndex);
		}

		private PXResultset<INCategory> SelectSiblings(int? parentID, int? categoryID, out int currentIndex)
		{
			currentIndex = -1;
			if (parentID == null) return null;
			PXResultset<INCategory> items = this.Items.Select(parentID);

			int i = 0;
			foreach (INCategory item in items)
			{
				if (item.CategoryID == categoryID)
					currentIndex = i;
				item.SortOrder = i + 1;
				Items.Update(item);
				i += 1;
			}
			return items;
		}
		#endregion

		#region Members events
		protected virtual IEnumerable members(
			[PXInt]
			int? CategoryID
			)
		{
			this.Members.Cache.AllowInsert = (CategoryID != null);
			CurrentSelected.CategoryID = CategoryID;
			return PXSelectJoin<INItemCategory,
				LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INItemCategory.inventoryID>>>,
				Where<INItemCategory.categoryID, Equal<Required<INItemCategory.categoryID>>>>.Select(this, CategoryID);
		}
		protected virtual void INItemCategory_CategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INItemCategory row = e.Row as INItemCategory;
			if (row == null) return;
			e.NewValue = this.CurrentSelected.CategoryID ?? 0;
			e.Cancel = true;
		}
		#endregion		

		private SelectedNode CurrentSelected
		{
			get
			{
				PXCache cache = this.Caches[typeof(SelectedNode)];
				if (cache.Current == null)
				{
					cache.Insert();
					cache.IsDirty = false;
				}
				return (SelectedNode)cache.Current;
			}
		}
	}		
}

using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using System.Collections;
using PX.Objects.CS;

namespace PX.Objects.SO
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class ARUpdateDiscounts : PXGraph<ARUpdateDiscounts>
	{
		public PXCancel<ItemFilter> Cancel;
		public PXFilter<ItemFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<SelectedItem, ItemFilter> Items;

		public virtual IEnumerable items()
		{
			ItemFilter filter = Filter.Current;
			if (filter == null)
			{
				yield break;
			}
			bool found = false;
			foreach (SelectedItem item in Items.Cache.Inserted)
			{
				found = true;
				yield return item;
			}
			if (found)
				yield break;


			List<string> added = new List<string>();
			PXSelect<DiscountSequence,
				Where<DiscountSequence.startDate, LessEqual<Current<ItemFilter.pendingDiscountDate>>,
				And<DiscountSequence.isPromotion, Equal<boolFalse>>>> s1 = new PXSelect<DiscountSequence, Where<DiscountSequence.startDate, LessEqual<Current<ItemFilter.pendingDiscountDate>>, And<DiscountSequence.isPromotion, Equal<boolFalse>>>>(this);

			foreach (DiscountSequence sequence in s1.Select())
			{
				string key = string.Format("{0}.{1}", sequence.DiscountID, sequence.DiscountSequenceID);
				added.Add(key);

				SelectedItem item = new SelectedItem();
				item.DiscountID = sequence.DiscountID;
				item.DiscountSequenceID = sequence.DiscountSequenceID;
				item.Description = sequence.Description;
				item.DiscountedFor = sequence.DiscountedFor;
				item.BreakBy = sequence.BreakBy;
				item.IsPromotion = sequence.IsPromotion;
				item.IsActive = sequence.IsActive;
				item.StartDate = sequence.StartDate;
				item.EndDate = sequence.EndDate;

				yield return Items.Insert(item);
			}

			foreach (DiscountDetail detail in PXSelectGroupBy<DiscountDetail, Where<DiscountDetail.startDate, LessEqual<Current<ItemFilter.pendingDiscountDate>>>,
				Aggregate<GroupBy<DiscountDetail.discountID, GroupBy<DiscountDetail.discountSequenceID>>>>.Select(this))
			{
				string key = string.Format("{0}.{1}", detail.DiscountID, detail.DiscountSequenceID);

				if (!added.Contains(key))
				{
					DiscountSequence sequence = PXSelect<DiscountSequence,
						Where<DiscountSequence.discountID, Equal<Required<DiscountSequence.discountID>>,
						And<DiscountSequence.discountSequenceID, Equal<Required<DiscountSequence.discountSequenceID>>>>>.Select(this, detail.DiscountID, detail.DiscountSequenceID);

					if (sequence != null && sequence.IsPromotion == false)
					{
						SelectedItem item = new SelectedItem();
						item.DiscountID = sequence.DiscountID;
						item.DiscountSequenceID = sequence.DiscountSequenceID;
						item.Description = sequence.Description;
						item.DiscountedFor = sequence.DiscountedFor;
						item.BreakBy = sequence.BreakBy;
						item.IsPromotion = sequence.IsPromotion;
						item.IsActive = sequence.IsActive;
						item.StartDate = sequence.StartDate;
						item.EndDate = sequence.EndDate;

						yield return Items.Insert(item);
					}
				}
			}

			Items.Cache.IsDirty = false;
		}

		public ARUpdateDiscounts()
		{
			Items.SetSelected<SelectedItem.selected>();
			Items.SetProcessCaption(Messages.Process);
			Items.SetProcessAllCaption(Messages.ProcessAll);
		}

		#region EventHandlers
		protected virtual void ItemFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Items.Cache.Clear();
		}

		protected virtual void ItemFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ItemFilter filter = Filter.Current;
			DateTime? date = Filter.Current.PendingDiscountDate;
			Items.SetProcessDelegate<UpdateDiscountProcess>(
					delegate(UpdateDiscountProcess graph, PX.Objects.SO.ARUpdateDiscounts.SelectedItem item)
					{						
						UpdateDiscount(graph, item, date);
					});
		}
		
		#endregion

		public static void UpdateDiscount(UpdateDiscountProcess graph, SelectedItem item, DateTime? filterDate)
		{
			graph.UpdateDiscount(item, filterDate);
		}

		public static void UpdateDiscount(string discountID, string discountSequenceID, DateTime? filterDate)
		{
			UpdateDiscountProcess graph = PXGraph.CreateInstance<UpdateDiscountProcess>();

			graph.UpdateDiscount(discountID, discountSequenceID, filterDate);
		}



		#region Local Types

		[Serializable]
		public partial class ItemFilter : IBqlTable
		{
			#region PendingDiscountDate
			public abstract class pendingDiscountDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _PendingDiscountDate;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Max. Pending Discount Date")]
			public virtual DateTime? PendingDiscountDate
			{
				get
				{
					return this._PendingDiscountDate;
				}
				set
				{
					this._PendingDiscountDate = value;
				}
			}
			#endregion
		}

		[Serializable]
		public partial class SelectedItem : IBqlTable
		{
			#region Selected
			public abstract class selected : PX.Data.IBqlField
			{
			}
			protected Boolean? _Selected = false;
			[PXBool()]
			[PXUIField(DisplayName = "Selected")]
			public virtual Boolean? Selected
			{
				get
				{
					return this._Selected;
				}
				set
				{
					this._Selected = value;
				}
			}
			#endregion


			#region DiscountID
			public abstract class discountID : PX.Data.IBqlField
			{
			}
			protected String _DiscountID;
			[PXDBString(10, IsUnicode = true, IsKey=true)]
			[PXUIField(DisplayName = "Discount Code", Visibility = PXUIVisibility.Visible)]
			public virtual String DiscountID
			{
				get
				{
					return this._DiscountID;
				}
				set
				{
					this._DiscountID = value;
				}
			}
			#endregion
			#region DiscountSequenceID
			public abstract class discountSequenceID : PX.Data.IBqlField
			{
			}
			protected String _DiscountSequenceID;
			[PXDBString(10, IsUnicode = true, IsKey=true)]
			[PXUIField(DisplayName = "Sequence", Visibility = PXUIVisibility.Visible)]
			public virtual String DiscountSequenceID
			{
				get
				{
					return this._DiscountSequenceID;
				}
				set
				{
					this._DiscountSequenceID = value;
				}
			}
			#endregion
			#region Description
			public abstract class description : PX.Data.IBqlField
			{
			}
			protected String _Description;
			[PXDBString(250, IsUnicode = true)]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String Description
			{
				get
				{
					return this._Description;
				}
				set
				{
					this._Description = value;
				}
			}
			#endregion
			#region DiscountedFor
			public abstract class discountedFor : PX.Data.IBqlField
			{
			}
			protected String _DiscountedFor;
			[PXDBString(1, IsFixed = true)]
			[PXDefault(DiscountOption.Percent)]
			[DiscountOption.List]
			[PXUIField(DisplayName = "Discount by", Visibility = PXUIVisibility.Visible)]
			public virtual String DiscountedFor
			{
				get
				{
					return this._DiscountedFor;
				}
				set
				{
					this._DiscountedFor = value;
				}
			}
			#endregion
			#region BreakBy
			public abstract class breakBy : PX.Data.IBqlField
			{
			}
			protected String _BreakBy;
			[PXDBString(1, IsFixed = true)]
			[PXDefault(BreakdownType.Amount)]
			[BreakdownType.List]
			[PXUIField(DisplayName = "Break by", Visibility = PXUIVisibility.Visible)]
			public virtual String BreakBy
			{
				get
				{
					return this._BreakBy;
				}
				set
				{
					this._BreakBy = value;
				}
			}
			#endregion
			#region IsPromotion
			public abstract class isPromotion : PX.Data.IBqlField
			{
			}
			protected Boolean? _IsPromotion;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Promotional", Visibility = PXUIVisibility.Visible)]
			public virtual Boolean? IsPromotion
			{
				get
				{
					return this._IsPromotion;
				}
				set
				{
					this._IsPromotion = value;
				}
			}
			#endregion
			#region IsActive
			public abstract class isActive : PX.Data.IBqlField
			{
			}
			protected Boolean? _IsActive;
			[PXDBBool()]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.Visible)]
			public virtual Boolean? IsActive
			{
				get
				{
					return this._IsActive;
				}
				set
				{
					this._IsActive = value;
				}
			}
			#endregion
			#region StartDate
			public abstract class startDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _StartDate;
			[PXDBDate()]
			[PXDefault()]
			[PXUIField(DisplayName = "Effective Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? StartDate
			{
				get
				{
					return this._StartDate;
				}
				set
				{
					this._StartDate = value;
				}
			}
			#endregion
			#region EndDate
			public abstract class endDate : PX.Data.IBqlField
			{
			}
			protected DateTime? _EndDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Last Update Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual DateTime? EndDate
			{
				get
				{
					return this._EndDate;
				}
				set
				{
					this._EndDate = value;
				}
			}
			#endregion
			
		} 

		#endregion
	}

	public class UpdateDiscountProcess : PXGraph<UpdateDiscountProcess>
	{

		public virtual void UpdateDiscount(PX.Objects.SO.ARUpdateDiscounts.SelectedItem item, DateTime? filterDate)
		{
			UpdateDiscount(item.DiscountID, item.DiscountSequenceID, filterDate);
		}

		public virtual void UpdateDiscount(string discountID, string discountSequenceID, DateTime? filterDate)
		{
			using (PXConnectionScope cs = new PXConnectionScope())
			{
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    foreach (DiscountDetail detail in PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Required<DiscountDetail.discountID>>,
                        And<DiscountDetail.discountSequenceID, Equal<Required<DiscountDetail.discountSequenceID>>>>>.Select(this, discountID, discountSequenceID))
                    {
                        if (detail.StartDate != null && detail.StartDate.Value <= filterDate.Value)
                        {
                            PXDatabase.Update<DiscountDetail>(
                                                new PXDataFieldAssign("LastAmount", PXDbType.DirectExpression, "Amount"),
                                                new PXDataFieldAssign("LastAmountTo", PXDbType.DirectExpression, "AmountTo"),
                                                new PXDataFieldAssign("LastQuantity", PXDbType.DirectExpression, "Quantity"),
                                                new PXDataFieldAssign("LastQuantityTo", PXDbType.DirectExpression, "QuantityTo"),
                                                new PXDataFieldAssign("LastDiscount", PXDbType.DirectExpression, "Discount"),
                                                new PXDataFieldAssign("LastFreeItemQty", PXDbType.DirectExpression, "FreeItemQty"),
                                                new PXDataFieldAssign("Amount", PXDbType.DirectExpression, "PendingAmount"),
                                                new PXDataFieldAssign("Quantity", PXDbType.DirectExpression, "PendingQuantity"),
                                                new PXDataFieldAssign("Discount", PXDbType.DirectExpression, "PendingDiscount"),
                                                new PXDataFieldAssign("FreeItemQty", PXDbType.DirectExpression, "PendingFreeItemQty"),
                                                new PXDataFieldAssign("LastDate", PXDbType.DirectExpression, "StartDate"),
                                                new PXDataFieldAssign("PendingAmount", PXDbType.Decimal, null),
                                                new PXDataFieldAssign("PendingQuantity", PXDbType.Decimal, 0m),
                                                new PXDataFieldAssign("PendingDiscount", PXDbType.Decimal, null),
                                                new PXDataFieldAssign("PendingFreeItemQty", PXDbType.Decimal, null),
                                                new PXDataFieldAssign("StartDate", PXDbType.DateTime, null),
                                                new PXDataFieldRestrict("DiscountDetailsID", PXDbType.Int, detail.DiscountDetailsID)
                                                );
                        }
                    }
                    foreach (DiscountDetail detail in PXSelectReadonly<DiscountDetail, Where<DiscountDetail.discountID, Equal<Required<DiscountDetail.discountID>>,
                        And<DiscountDetail.discountSequenceID, Equal<Required<DiscountDetail.discountSequenceID>>>>>.Select(this, discountID, discountSequenceID))
                    {
                        DiscountDetail amonextval = PXSelectReadonly<DiscountDetail, Where<DiscountDetail.discountID, Equal<Required<DiscountDetail.discountID>>,
                                               And<DiscountDetail.discountSequenceID, Equal<Required<DiscountDetail.discountSequenceID>>, And<DiscountDetail.amount, Greater<Required<DiscountDetail.amount>>>>>,
                                               OrderBy<Asc<DiscountDetail.amount>>>.SelectWindowed(this, 0, 1, discountID, discountSequenceID, detail.Amount);
                        DiscountDetail qtynextval = PXSelectReadonly<DiscountDetail, Where<DiscountDetail.discountID, Equal<Required<DiscountDetail.discountID>>,
                                                And<DiscountDetail.discountSequenceID, Equal<Required<DiscountDetail.discountSequenceID>>, And<DiscountDetail.quantity, Greater<Required<DiscountDetail.quantity>>>>>,
                                                OrderBy<Asc<DiscountDetail.quantity>>>.SelectWindowed(this, 0, 1, discountID, discountSequenceID, detail.Quantity);
                        PXDatabase.Update<DiscountDetail>(
                                            new PXDataFieldAssign("AmountTo", PXDbType.Decimal, (amonextval == null ? null : amonextval.Amount)),
                                            new PXDataFieldAssign("QuantityTo", PXDbType.Decimal, (qtynextval == null ? null : qtynextval.Quantity)),
                                            new PXDataFieldRestrict("DiscountDetailsID", PXDbType.Int, detail.DiscountDetailsID)
                                            );
                    }

                    foreach (DiscountDetail detail in PXSelectReadonly<DiscountDetail, Where<DiscountDetail.discountID, Equal<Required<DiscountDetail.discountID>>,
                        And<DiscountDetail.discountSequenceID, Equal<Required<DiscountDetail.discountSequenceID>>>>>.Select(this, discountID, discountSequenceID))
                    {
                        DiscountDetail amonextval = PXSelectReadonly<DiscountDetail, Where<DiscountDetail.discountID, Equal<Required<DiscountDetail.discountID>>,
                                               And<DiscountDetail.discountSequenceID, Equal<Required<DiscountDetail.discountSequenceID>>, And<DiscountDetail.lastAmount, Greater<Required<DiscountDetail.lastAmount>>>>>,
                                               OrderBy<Asc<DiscountDetail.lastAmount>>>.SelectWindowed(this, 0, 1, discountID, discountSequenceID, detail.Amount);
                        DiscountDetail qtynextval = PXSelectReadonly<DiscountDetail, Where<DiscountDetail.discountID, Equal<Required<DiscountDetail.discountID>>,
                                                And<DiscountDetail.discountSequenceID, Equal<Required<DiscountDetail.discountSequenceID>>, And<DiscountDetail.lastQuantity, Greater<Required<DiscountDetail.lastQuantity>>>>>,
                                                OrderBy<Asc<DiscountDetail.lastQuantity>>>.SelectWindowed(this, 0, 1, discountID, discountSequenceID, detail.Quantity);
                        PXDatabase.Update<DiscountDetail>(
                                            new PXDataFieldAssign("LastAmountTo", PXDbType.Decimal, (amonextval == null ? null : amonextval.LastAmount)),
                                            new PXDataFieldAssign("LastQuantityTo", PXDbType.Decimal, (qtynextval == null ? null : qtynextval.LastQuantity)),
                                            new PXDataFieldRestrict("DiscountDetailsID", PXDbType.Int, detail.DiscountDetailsID)
                                            );
                    }

                    DiscountSequence sequence = PXSelect<DiscountSequence,
                        Where<DiscountSequence.discountID, Equal<Required<DiscountSequence.discountID>>,
                        And<DiscountSequence.discountSequenceID, Equal<Required<DiscountSequence.discountSequenceID>>>>>.Select(this, discountID, discountSequenceID);

                    if (sequence != null && sequence.StartDate != null && sequence.StartDate.Value <= filterDate.Value && sequence.PendingFreeItemID != null)
                    {
                        PXDatabase.Update<DiscountSequence>(
                                                new PXDataFieldAssign("LastFreeItemID", PXDbType.DirectExpression, "FreeItemID"),
                                                new PXDataFieldAssign("FreeItemID", PXDbType.DirectExpression, "PendingFreeItemID"),
                                                new PXDataFieldAssign("PendingFreeItemID", PXDbType.Int, null),
                                                new PXDataFieldAssign("EndDate", PXDbType.DirectExpression, "StartDate"),
                            //new PXDataFieldAssign("StartDate", PXDbType.DateTime, null),
                                                new PXDataFieldRestrict("DiscountID", PXDbType.VarChar, discountID),
                                                new PXDataFieldRestrict("DiscountSequenceID", PXDbType.VarChar, discountSequenceID)
                                                );
                    }

                    ts.Complete();
                }
			}

		}
		
	}
}

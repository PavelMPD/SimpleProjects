using System.Collections;
using System.Linq;
using System.Runtime.Serialization;
using PX.Common;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.IN
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.CS;
	using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
	using LocationStatus = PX.Objects.IN.Overrides.INDocumentRelease.LocationStatus;
	using LotSerialStatus = PX.Objects.IN.Overrides.INDocumentRelease.LotSerialStatus;
	using ItemLotSerial = PX.Objects.IN.Overrides.INDocumentRelease.ItemLotSerial;
	using ReadOnlyLotSerialStatus = PX.Objects.IN.Overrides.INDocumentRelease.ReadOnlyLotSerialStatus;
	using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;
	using IQtyAllocatedBase = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocatedBase;

	public class INSetupDecPl : IPrefetchable
	{
		protected int _Qty = 6;
		protected int _PrcCst = 6;
		void IPrefetchable.Prefetch()
		{
			using (PXDataRecord record = PXDatabase.SelectSingle<INSetup>(
				new PXDataField(typeof(INSetup.decPlQty).Name),
				new PXDataField(typeof(INSetup.decPlPrcCst).Name)))
			{
				if (record != null)
				{
					_Qty = (int)record.GetInt16(0);
					_PrcCst = (int)record.GetInt16(1);
				}
			}
		}

		public static int Qty
		{
			get
			{
				INSetupDecPl definition = PXDatabase.GetSlot<INSetupDecPl>(typeof(INSetupDecPl).Name, typeof(INSetup));
				if (definition != null)
				{
					return definition._Qty;
				}
				return 6;
			}
		}

		public static int PrcCst
		{
			get
			{
				INSetupDecPl definition = PXDatabase.GetSlot<INSetupDecPl>(typeof(INSetupDecPl).Name, typeof(INSetup));
				if (definition != null)
				{
					return definition._PrcCst;
				}
				return 6;
			}
		}
	}

	public class PXUnitConversionException : PXSetPropertyException
	{
		public PXUnitConversionException()
			: base(Messages.MissingUnitConversion)
		{
		}

		public PXUnitConversionException(string UOM)
			: base(Messages.MissingUnitConversionVerbose, UOM)
		{
		}
		public PXUnitConversionException(SerializationInfo info, StreamingContext context)
			: base(info, context){}

	}

	public interface IQtyPlanned
	{
		bool? Reverse { get; set; }
		decimal? PlanQty { get; set; }
	}

	#region INPrecision

	public enum INPrecision
	{
		NOROUND = 0,
		QUANTITY = 1,
		UNITCOST = 2
	}

	#endregion

	#region PXDBCostScalarAttribute
	public class PXDBCostScalarAttribute : PXDBScalarAttribute
	{
		public PXDBCostScalarAttribute(Type search)
			: base(search)
		{
		}

		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			base.RowSelecting(sender, e);

			if (sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, 0m);
			}
		}
	}
	#endregion

	#region PXExistance

	public class PXExistance : PXBoolAttribute, IPXRowSelectingSubscriber
	{
		#region Implementation
		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			foreach (string key in sender.Keys)
			{
				if (sender.GetValue(e.Row, key) == null)
				{
					return;
				}
			}
			sender.SetValue(e.Row, _FieldOrdinal, true);
		}
		#endregion
	}

	#endregion

	#region INTranSplitPlanIDAttribute

	public class INTranSplitPlanIDAttribute : INItemPlanIDAttribute
	{
		#region State
		protected Type _ParentTransferType;
		#endregion
		#region Ctor
		public INTranSplitPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry, Type ParentTransferType)
			: base(ParentNoteID, ParentHoldEntry)
		{
			_ParentTransferType = ParentTransferType;
		}
		#endregion
		#region Implementation
		public override void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<INRegister.hold, INRegister.transferType>(e.Row, e.OldRow))
			{
				bool transfertypeupdated = !sender.ObjectsEqual<INRegister.transferType>(e.Row, e.OldRow);
				PXCache plancache = sender.Graph.Caches[typeof(INItemPlan)];

				//preload plans in cache first
				PXResultset<INItemPlan> not_inserted = PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<INRegister.noteID>>>>.Select(sender.Graph);

				foreach (INTranSplit split in PXSelect<INTranSplit, Where<INTranSplit.docType, Equal<Current<INRegister.docType>>, And<INTranSplit.refNbr, Equal<Current<INRegister.refNbr>>>>>.Select(sender.Graph))
				{
					foreach (INItemPlan plan in plancache.Cached)
					{
						if (object.Equals(plan.PlanID, split.PlanID) && plancache.GetStatus(plan) != PXEntryStatus.Deleted && plancache.GetStatus(plan) != PXEntryStatus.InsertedDeleted)
						{
							if (transfertypeupdated)
							{
								split.TransferType = (string)sender.GetValue<INRegister.transferType>(e.Row);
								if (sender.Graph.Caches[typeof(INTranSplit)].GetStatus(split) == PXEntryStatus.Notchanged)
								{
									sender.Graph.Caches[typeof(INTranSplit)].SetStatus(split, PXEntryStatus.Updated);
								}

								INItemPlan copy = PXCache<INItemPlan>.CreateCopy(plan);
								copy = DefaultValues(sender, copy, split);
								plancache.Update(copy);
							}
							else
							{
								plan.Hold = (bool?)sender.GetValue<INRegister.hold>(e.Row);
							}
						}
					}
				}
			}
		}

		public override void Parent_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				_KeyToAbort = sender.GetValue<INRegister.refNbr>(e.Row);
			}
		}

		public override void Parent_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open && _KeyToAbort != null)
			{
				foreach (INTranSplit split in PXSelect<INTranSplit, Where<INTranSplit.docType, Equal<Required<INRegister.docType>>, And<INTranSplit.refNbr, Equal<Required<INRegister.refNbr>>>>>.Select(sender.Graph, ((INRegister)e.Row).DocType, _KeyToAbort))
				{
					foreach (INItemPlan plan in sender.Graph.Caches[typeof(INItemPlan)].Inserted)
					{
						if (Equals(plan.PlanID, split.PlanID))
						{
							plan.RefNoteID = (long?)sender.GetValue(e.Row, _ParentNoteID.Name);
						}
					}
				}
			}
			_KeyToAbort = null;
		}

		protected override INPlanType GetTargetPlanType<TNode>(PXGraph graph, INItemPlan plan, INPlanType plantype)
		{
			string POPlanType;

			switch (plantype.PlanType)
			{
				case INPlanConstants.Plan77:
					POPlanType = INPlanConstants.Plan76;
					break;
				case INPlanConstants.Plan75:
					POPlanType = INPlanConstants.Plan74;
					break;
				default:
					return plantype;
			}

			INPlanType poOrderType = (INPlanType)PXSelect<INPlanType, Where<INPlanType.planType, Equal<Required<INPlanType.planType>>>>.Select(graph, POPlanType);
			return plantype * (poOrderType ^ plantype);
		}

		public override INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row)
		{
			PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ParentNoteID)];

			INTranSplit split_Row;
			object doc_Row;

			if (orig_Row is PXResult)
			{
				split_Row = (INTranSplit)((PXResult)orig_Row)[typeof(INTranSplit)];
				doc_Row = ((PXResult)orig_Row)[BqlCommand.GetItemType(_ParentNoteID)];
			}
			else
			{
				split_Row = (INTranSplit)orig_Row;
				doc_Row = cache.Current;
			}

			plan_Row.InventoryID = split_Row.InventoryID;
			plan_Row.SubItemID = split_Row.SubItemID;
			plan_Row.SiteID = split_Row.SiteID;
			plan_Row.LocationID = split_Row.LocationID;
			plan_Row.LotSerialNbr = split_Row.LotSerialNbr;
			if (string.IsNullOrEmpty(split_Row.AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(split_Row.AssignedNbr, split_Row.LotSerialNbr))
			{
				plan_Row.LotSerialNbr = null;
			}
			plan_Row.PlanDate = new DateTime(1900, 1, 1);
			plan_Row.PlanQty = split_Row.BaseQty;

			plan_Row.RefNoteID = (long?)cache.GetValue(doc_Row, _ParentNoteID.Name);
			plan_Row.Hold = (bool?)cache.GetValue(doc_Row, _ParentHoldEntry.Name);

			if (plan_Row.RefNoteID < 0L)
			{
				plan_Row.RefNoteID = null;
			}

			switch (split_Row.TranType)
			{
				case INTranType.Receipt:
				case INTranType.Return:
				case INTranType.CreditMemo:
					if (split_Row.Released == true)
					{
						return null;
					}

					switch (split_Row.POLineType)
					{
						case PO.POLineType.GoodsForSalesOrder:
							plan_Row.PlanType = INPlanConstants.Plan77;
							break;
						case PO.POLineType.GoodsForDropShip:
							plan_Row.PlanType = INPlanConstants.Plan75;
							break;
						default:
							plan_Row.PlanType = INPlanConstants.Plan10;
							break;
					}
					break;
				case INTranType.Issue:
				case INTranType.Invoice:
				case INTranType.DebitMemo:
					if (split_Row.Released == true)
					{
						return null;
					}

					plan_Row.PlanType = INPlanConstants.Plan20;
					break;
				case INTranType.Transfer:
					if (split_Row.InvtMult == -1)
					{
						if (split_Row.TransferType == INTransferType.OneStep)
						{
							if (split_Row.Released == true)
							{
								return null;
							}

							plan_Row.PlanType = INPlanConstants.Plan40;
						}
						else if (split_Row.Released == true)
						{
							plan_Row.PlanType = INPlanConstants.Plan42;
							plan_Row.SiteID = split_Row.ToSiteID;
							plan_Row.LocationID = split_Row.ToLocationID;
						}
						else
						{
							plan_Row.PlanType = INPlanConstants.Plan41;
						}
					}
					else
					{
						if (split_Row.Released == true)
						{
							return null;
						}

						plan_Row.PlanType = INPlanConstants.Plan43;
					}
					break;
				case INTranType.Assembly:
					if (split_Row.Released == true)
					{
						return null;
					}

					if (split_Row.InvtMult == (short)-1)
					{
						plan_Row.PlanType = INPlanConstants.Plan50;
					}
					else
					{
						plan_Row.PlanType = INPlanConstants.Plan51;
					}
					break;
				case INTranType.Adjustment:
				case INTranType.StandardCostAdjustment:
				case INTranType.NegativeCostAdjustment:
				default:
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

	#endregion

	#region INItemPlanIDAttribute

	public abstract class INItemPlanIDAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber, IPXRowInsertedSubscriber, IPXRowUpdatedSubscriber, IPXRowDeletedSubscriber
	{
		#region State
		protected object _KeyToAbort;
		protected Type _ParentNoteID;
		protected Type _ParentHoldEntry;
		protected Type _ItemType;
		protected ObjectRef<bool> _ReleaseMode;
		#endregion
		#region Ctor
		public INItemPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry)
		{
			_ParentNoteID = ParentNoteID;
			_ParentHoldEntry = ParentHoldEntry;
		}
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_ReleaseMode = new ObjectRef<bool>();
			_ItemType = sender.GetItemType();

            if (!typeof(IItemPlanMaster).IsAssignableFrom(_ItemType))
            {
                throw new PXArgumentException();
            }

			sender.Graph.RowInserted.AddHandler(BqlCommand.GetItemType(_ParentNoteID), Parent_RowInserted);
			sender.Graph.RowUpdated.AddHandler(BqlCommand.GetItemType(_ParentNoteID), Parent_RowUpdated);
			sender.Graph.RowPersisting.AddHandler(BqlCommand.GetItemType(_ParentNoteID), Parent_RowPersisting);
			sender.Graph.RowPersisted.AddHandler(BqlCommand.GetItemType(_ParentNoteID), Parent_RowPersisted);

			if (!sender.Graph.Views.Caches.Contains(typeof(INItemPlan)))
			{
				sender.Graph.RowInserting.AddHandler<INItemPlan>(Plan_RowInserting);
				sender.Graph.RowInserted.AddHandler<INItemPlan>(Plan_RowInserted);
				sender.Graph.RowUpdated.AddHandler<INItemPlan>(Plan_RowUpdated);
				sender.Graph.RowDeleted.AddHandler<INItemPlan>(Plan_RowDeleted);

                sender.Graph.CommandPreparing.AddHandler<INItemPlan.planID>(Parameter_CommandPreparing);

				if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>())
				{
					sender.Graph.FieldDefaulting.AddHandler<INItemPlan.siteID>(Feature_FieldDefaulting);
					sender.Graph.FieldDefaulting.AddHandler<LocationStatus.siteID>(Feature_FieldDefaulting);
					sender.Graph.FieldDefaulting.AddHandler<LotSerialStatus.siteID>(Feature_FieldDefaulting);
				}

				if (!PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
				{
					sender.Graph.FieldDefaulting.AddHandler<INItemPlan.locationID>(Feature_FieldDefaulting);
					sender.Graph.FieldDefaulting.AddHandler<LocationStatus.locationID>(Feature_FieldDefaulting);
					sender.Graph.FieldDefaulting.AddHandler<LotSerialStatus.locationID>(Feature_FieldDefaulting);
				}
			}

			sender.Graph.RowPersisting.AddHandler<INItemPlan>(Plan_RowPersisting);
			sender.Graph.RowPersisted.AddHandler<INItemPlan>(Plan_RowPersisted);

			PXCache dummy_cache;
			dummy_cache = sender.Graph.Caches[typeof(INSiteStatus)];
			dummy_cache = sender.Graph.Caches[typeof(INLocationStatus)];
			dummy_cache = sender.Graph.Caches[typeof(INLotSerialStatus)];
            dummy_cache = sender.Graph.Caches[typeof(INItemLotSerial)];

			dummy_cache = sender.Graph.Caches[typeof(SiteStatus)];
			dummy_cache = sender.Graph.Caches[typeof(LocationStatus)];
			dummy_cache = sender.Graph.Caches[typeof(LotSerialStatus)];
            dummy_cache = sender.Graph.Caches[typeof(ItemLotSerial)];

            if (sender.Graph.IsImport || sender.Graph.UnattendedMode)
            {
                if (!sender.Graph.Views.Caches.Contains(typeof(INItemPlan)))
                    sender.Graph.Views.Caches.Add(typeof(INItemPlan));
                if (sender.Graph.Views.Caches.Contains(_ItemType))
                    sender.Graph.Views.Caches.Remove(_ItemType);
                sender.Graph.Views.Caches.Add(_ItemType);
            }
            else
            {
                //plan source should go before plan
                //to bind errors from INItemPlan.SubItemID -> SOLine.SubItemID or SOLineSplit.SubItemID
                if (!sender.Graph.Views.Caches.Contains(_ItemType))
                    sender.Graph.Views.Caches.Add(_ItemType);
                if (!sender.Graph.Views.Caches.Contains(typeof(INItemPlan)))
                    sender.Graph.Views.Caches.Add(typeof(INItemPlan));
            }
			if (!sender.Graph.Views.Caches.Contains(typeof(LotSerialStatus)))
				sender.Graph.Views.Caches.Add(typeof(LotSerialStatus));
            if (!sender.Graph.Views.Caches.Contains(typeof(ItemLotSerial)))
                sender.Graph.Views.Caches.Add(typeof(ItemLotSerial));
			if (!sender.Graph.Views.Caches.Contains(typeof(LocationStatus)))
				sender.Graph.Views.Caches.Add(typeof(LocationStatus));
			if (!sender.Graph.Views.Caches.Contains(typeof(SiteStatus)))
				sender.Graph.Views.Caches.Add(typeof(SiteStatus));

			sender.Graph.FieldVerifying.AddHandler<SiteStatus.subItemID>(SurrogateID_FieldVerifying);
			sender.Graph.FieldVerifying.AddHandler<LocationStatus.subItemID>(SurrogateID_FieldVerifying);
			sender.Graph.FieldVerifying.AddHandler<LotSerialStatus.subItemID>(SurrogateID_FieldVerifying);
			sender.Graph.FieldVerifying.AddHandler<LocationStatus.locationID>(SurrogateID_FieldVerifying);
			sender.Graph.FieldVerifying.AddHandler<LotSerialStatus.locationID>(SurrogateID_FieldVerifying);

			sender.Graph.RowPersisted.AddHandler<SiteStatus>(Accumulator_RowPersisted);
			sender.Graph.RowPersisted.AddHandler<LocationStatus>(Accumulator_RowPersisted);
			sender.Graph.RowPersisted.AddHandler<LotSerialStatus>(Accumulator_RowPersisted);
            sender.Graph.RowPersisted.AddHandler<ItemLotSerial>(Accumulator_RowPersisted);

            sender.Graph.CommandPreparing.AddHandler(_ItemType, _FieldName, Parameter_CommandPreparing);

			_persisted = new Dictionary<long?, object>();
		}
		#endregion
		#region Implementation

        protected virtual void Parameter_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            long? Key;
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && (e.Operation & PXDBOperation.Option) != PXDBOperation.External &&
                (e.Operation & PXDBOperation.Option) != PXDBOperation.ReadOnly && e.Row == null && (Key = e.Value as long?) != null)
            {
                if (Key < 0L)
                {
                    e.DataValue = null;
                    e.Cancel = true;
                }
            }
        }

		public static void SetReleaseMode<Field>(PXCache cache,  bool releaseMode)
			where Field : IBqlField
		{
			foreach (INItemPlanIDAttribute attr in cache.GetAttributes<Field>(null).OfType<INItemPlanIDAttribute>())
				(attr)._ReleaseMode.Value = releaseMode;
		}
		
		public abstract INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row);
		public abstract void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e);
		public abstract void Parent_RowPersisting(PXCache sender, PXRowPersistingEventArgs e);
		public abstract void Parent_RowPersisted(PXCache sender, PXRowPersistedEventArgs e);

        protected INItemPlan DefaultValuesInt(PXCache sender, INItemPlan plan_Row, object orig_Row)
        {
            INItemPlan info = DefaultValues(sender, plan_Row, orig_Row);
            if (info != null && info.InventoryID != null && info.SiteID != null)
            {
                return info;
            }
            return null;
        }

		public virtual void Feature_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		public virtual void SurrogateID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue is Int32)
			{
				e.Cancel = true;
			}
		}

        protected Dictionary<Type, List<PXView>> _views;

        protected void Clear<TNode>(PXGraph graph)
			where TNode : class, IBqlTable
		{
            if (_views == null)
            {
                _views = new Dictionary<Type, List<PXView>>();
            }

            List<PXView> views;
            if (!_views.TryGetValue(typeof(TNode), out views))
            {
                views = _views[typeof(TNode)] = new List<PXView>();

			    List<PXView> namedviews = new List<PXView>(graph.Views.Values);
			    foreach (PXView view in namedviews)
			    {
				    if (typeof(TNode).IsAssignableFrom(view.GetItemType()))
				    {
					    views.Add(view);
				    }
			    }

			    List<PXView> typedviews = new List<PXView>(graph.TypedViews.Values);
			    foreach (PXView view in typedviews)
			    {
				    if (typeof(TNode).IsAssignableFrom(view.GetItemType()))
				    {
					    views.Add(view);
				    }
			    }

			    List<PXView> readonlyviews = new List<PXView>(graph.TypedViews.ReadOnlyValues);
			    foreach (PXView view in readonlyviews)
			    {
				    if (typeof(TNode).IsAssignableFrom(view.GetItemType()))
				    {
					    views.Add(view);
				    }
			    }
            }

            foreach(PXView view in views)
            {
                view.Clear();
			    view.Cache.Clear();            
            }
		}

		public virtual void Accumulator_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation != PXDBOperation.Delete && e.TranStatus == PXTranStatus.Completed)
			{
				if (sender.GetItemType() == typeof(SiteStatus))
				{
					Clear<INSiteStatus>(sender.Graph);
				}
				if (sender.GetItemType() == typeof(LocationStatus))
				{
					Clear<INLocationStatus>(sender.Graph);
				}
				if (sender.GetItemType() == typeof(LotSerialStatus))
				{
					Clear<INLotSerialStatus>(sender.Graph);
				}
                if (sender.GetItemType() == typeof(ItemLotSerial))
                {
                    Clear<ItemLotSerial>(sender.Graph);
                }
			}
		}

		public virtual void Parent_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PXNoteAttribute.GetNoteID(sender, e.Row, _ParentNoteID.Name);
			sender.Graph.Caches[typeof(Note)].IsDirty = false;
		}

		protected object _SelfKeyToAbort;
		protected Dictionary<long?, object> _persisted;

		public virtual void Plan_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				_SelfKeyToAbort = sender.GetValue<INItemPlan.planID>(e.Row);
			}
		}

        Dictionary<long?, object> _inserted = null;
        Dictionary<long?, object> _updated = null;

		public virtual void Plan_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				PXCache cache = sender.Graph.Caches[_ItemType];
				long? NewKey;

				if (e.TranStatus == PXTranStatus.Open && _SelfKeyToAbort != null)
				{
					NewKey = (long?)sender.GetValue<INItemPlan.planID>(e.Row);

					if (!_persisted.ContainsKey(NewKey))
					{
						_persisted.Add(NewKey, _SelfKeyToAbort);
					}

                    if (_inserted == null)
                    {
                        _inserted = new Dictionary<long?, object>();

                        foreach (object item in cache.Inserted)
                        {
													if (cache.GetValue(item, _FieldOrdinal) != null)
                            _inserted.Add((long?)cache.GetValue(item, _FieldOrdinal), item);
                        }
                    }


                    object row;
                    if (_inserted.TryGetValue((long?)_SelfKeyToAbort, out row))
                    {
                        cache.SetValue(row, _FieldOrdinal, NewKey);
                        _SelfKeyToAbort = null;
                    }

                    if (_updated == null)
                    {
                        _updated = new Dictionary<long?, object>();

                        foreach (object item in cache.Updated)
                        {
													if (cache.GetValue(item, _FieldOrdinal) != null)                          
                            _updated.Add((long?)cache.GetValue(item, _FieldOrdinal), item);
                        }
                    }

                    if (_SelfKeyToAbort != null && _updated.TryGetValue((long?)_SelfKeyToAbort, out row))
                    {
                        cache.SetValue(row, _FieldOrdinal, NewKey);
                    }

                    _SelfKeyToAbort = null;
                }

				if (e.TranStatus == PXTranStatus.Aborted)
				{
					foreach (object item in cache.Inserted)
					{
						if ((NewKey = (long?)cache.GetValue(item, _FieldOrdinal)) != null && _persisted.TryGetValue(NewKey, out _SelfKeyToAbort))
						{
							cache.SetValue(item, _FieldOrdinal, _SelfKeyToAbort);
						}
					}

					foreach (object item in cache.Updated)
					{
						if ((NewKey = (long?)cache.GetValue(item, _FieldOrdinal)) != null && _persisted.TryGetValue(NewKey, out _SelfKeyToAbort))
						{
							cache.SetValue(item, _FieldOrdinal, _SelfKeyToAbort);
						}
					}
				}

                if (e.TranStatus == PXTranStatus.Completed || e.TranStatus == PXTranStatus.Aborted)
                {
                    _inserted = null;
                    _updated = null;
                }
			}
		}

		public static TNode ConvertPlan<TNode>(INItemPlan item)
			where TNode : class, IQtyAllocatedBase
		{
			if (typeof(TNode) == typeof(SiteStatus))
			{
				return (SiteStatus)item as TNode;
			}
			if (typeof(TNode) == typeof(LocationStatus))
			{
				return (LocationStatus)item as TNode;
			}
			if (typeof(TNode) == typeof(LotSerialStatus))
			{
				return (LotSerialStatus)item as TNode;
			}
            if (typeof(TNode) == typeof(ItemLotSerial))
            {
                return (ItemLotSerial)item as TNode;
            }


			return null;
		}

		public virtual decimal GetInclQtyAvail<TNode>(PXGraph graph, object data)
			where TNode : class, IQtyAllocated
		{
			INItemPlan plan = DefaultValuesInt(graph.Caches[_ItemType], new INItemPlan(), data);
			if (plan != null)
			{
				INPlanType plantype = PXSelectReadonly<INPlanType, Where<INPlanType.planType, Equal<Required<INPlanType.planType>>>>.Select(graph, plan.PlanType);
				plantype = GetTargetPlanType<TNode>(graph, plan, plantype);

				return GetInclQtyAvail<TNode>(graph, plan, plantype);
			}
			else
			{
				return 0m;
			}
		}
		public virtual decimal GetInclQtyHardAvail<TNode>(PXGraph graph, object data)
		where TNode : class, IQtyAllocated
		{
			INItemPlan plan = DefaultValuesInt(graph.Caches[_ItemType], new INItemPlan(), data);
			if (plan != null)
			{
				INPlanType plantype = PXSelectReadonly<INPlanType, Where<INPlanType.planType, Equal<Required<INPlanType.planType>>>>.Select(graph, plan.PlanType);
				plantype = GetTargetPlanType<TNode>(graph, plan, plantype);

				return GetInclQtyHardAvail<TNode>(graph, plan, plantype);
			}
			else
			{
				return 0m;
			}
		}

		public static decimal GetInclQtyAvail<TNode>(PXCache sender, object data)
			where TNode : class, IQtyAllocated
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(null))
			{
				if (attr is INItemPlanIDAttribute)
				{
					return ((INItemPlanIDAttribute)attr).GetInclQtyAvail<TNode>(sender.Graph, data);
				}
			}
			return 0m;
		}

		public static decimal GetInclQtyHardAvail<TNode>(PXCache sender, object data)
			where TNode : class, IQtyAllocated
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(null))
			{
				if (attr is INItemPlanIDAttribute)
				{
					return ((INItemPlanIDAttribute)attr).GetInclQtyHardAvail<TNode>(sender.Graph, data);
				}
			}
			return 0m;
		}

		public static INItemPlan DefaultValues(PXCache sender, object data)
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(null))
			{
				if (attr is INItemPlanIDAttribute)
				{
					return ((INItemPlanIDAttribute)attr).DefaultValuesInt(sender, new INItemPlan(), data);
				}
			}
			return null;
		}

		protected decimal GetInclQtyAvail<TNode>(PXGraph graph, INItemPlan plan, INPlanType plantype)
			where TNode : class, IQtyAllocated
		{
			using (var scope = new ReadOnlyScope( graph.Caches[typeof (TNode)]))
			{
				TNode target = (TNode) graph.Caches[typeof (TNode)].Insert(ConvertPlan<TNode>(plan));

				decimal result = 0m;

				result -= target.InclQtyINIssues == true ? (decimal) plantype.InclQtyINIssues : 0m;
				result += target.InclQtyINReceipts == true ? (decimal) plantype.InclQtyINReceipts : 0m;
				result += target.InclQtyInTransit == true ? (decimal) plantype.InclQtyInTransit : 0m;
				result += target.InclQtyPOPrepared == true ? (decimal) plantype.InclQtyPOPrepared : 0m;
				result += target.InclQtyPOOrders == true ? (decimal) plantype.InclQtyPOOrders : 0m;
				result += target.InclQtyPOReceipts == true ? (decimal) plantype.InclQtyPOReceipts : 0m;
				result += target.InclQtyINAssemblySupply == true ? (decimal) plantype.InclQtyINAssemblySupply : 0m;
				result -= target.InclQtySOBackOrdered == true ? (decimal) plantype.InclQtySOBackOrdered : 0m;
				result -= target.InclQtySOBooked == true ? (decimal) plantype.InclQtySOBooked : 0m;
				result -= target.InclQtySOShipped == true ? (decimal) plantype.InclQtySOShipped : 0m;
				result -= target.InclQtySOShipping == true ? (decimal) plantype.InclQtySOShipping : 0m;
				result -= target.InclQtyINAssemblyDemand == true ? (decimal) plantype.InclQtyINAssemblyDemand : 0m;

				if (plan.Reverse == true)
					return target.InclQtySOReverse != true ? 0 : -result;

				return result;
			}
		}

		protected decimal GetInclQtyHardAvail<TNode>(PXGraph graph, INItemPlan plan, INPlanType plantype)
			where TNode : class, IQtyAllocated
		{
			TNode target = (TNode)graph.Caches[typeof(TNode)].Insert(ConvertPlan<TNode>(plan));

			decimal result = 0m;
			result -= target.InclQtySOShipped == true ? (decimal)plantype.InclQtySOShipped : 0m;
			result -= target.InclQtySOShipping == true ? (decimal)plantype.InclQtySOShipping : 0m;
			result -= target.InclQtyINIssues == true ? (decimal)plantype.InclQtyINIssues : 0m;
			return result;
		}

		public static TNode UpdateAllocatedQuantitiesBase<TNode>(PXGraph graph, INItemPlan plan, INPlanType plantype, bool InclQtyAvail)
			where TNode : class, IQtyAllocatedBase
		{
			bool isDirty = graph.Caches[typeof(TNode)].IsDirty;
			TNode target = (TNode)graph.Caches[typeof(TNode)].Insert(ConvertPlan<TNode>(plan));
			graph.Caches[typeof(TNode)].IsDirty = isDirty;

			return UpdateAllocatedQuantitiesBase<TNode>(graph, target, plan, plantype, InclQtyAvail);
		}

		public static TNode UpdateAllocatedQuantitiesBase<TNode>(PXGraph graph, TNode target, IQtyPlanned plan, INPlanType plantype, bool? InclQtyAvail)
			where TNode : class, IQtyAllocatedBase
		{
			decimal qty = plan.PlanQty ?? 0;
			if (plan.Reverse == true)
			{
				if (target.InclQtySOReverse != true)
					return target;
				else
					qty = -qty;
			}

            IQtyAllocated exttarget = target as IQtyAllocated;
            if (exttarget != null)
            {
                exttarget.QtyINIssues += (plantype.InclQtyINIssues ?? 0) * qty;
                exttarget.QtyINReceipts += (plantype.InclQtyINReceipts ?? 0) * qty;
                exttarget.QtyPOPrepared += (plantype.InclQtyPOPrepared ?? 0) * qty;
                exttarget.QtyPOOrders += (plantype.InclQtyPOOrders ?? 0) * qty;
                exttarget.QtyPOReceipts += (plantype.InclQtyPOReceipts ?? 0) * qty;
                exttarget.QtySOBackOrdered += (plantype.InclQtySOBackOrdered ?? 0) * qty;
                exttarget.QtySOBooked += (plantype.InclQtySOBooked ?? 0) * qty;
                exttarget.QtySOShipped += (plantype.InclQtySOShipped ?? 0) * qty;
                exttarget.QtySOShipping += (plantype.InclQtySOShipping ?? 0) * qty;
                exttarget.QtyINAssemblySupply += (plantype.InclQtyINAssemblySupply ?? 0) * qty;
                exttarget.QtyINAssemblyDemand += (plantype.InclQtyINAssemblyDemand ?? 0) * qty;
                exttarget.QtyINReplaned += (plantype.InclQtyINReplaned ?? 0) * qty;
                exttarget.QtySOFixed += (plantype.InclQtySOFixed ?? 0) * qty;
                exttarget.QtyPOFixedOrders += (plantype.InclQtyPOFixedOrders ?? 0) * qty;
                exttarget.QtyPOFixedPrepared += (plantype.InclQtyPOFixedPrepared ?? 0) * qty;
                exttarget.QtyPOFixedReceipts += (plantype.InclQtyPOFixedReceipts ?? 0) * qty;
                exttarget.QtySODropShip += (plantype.InclQtySODropShip ?? 0) * qty;
                exttarget.QtyPODropShipOrders += (plantype.InclQtyPODropShipOrders ?? 0) * qty;
                exttarget.QtyPODropShipPrepared += (plantype.InclQtyPODropShipPrepared ?? 0) * qty;
                exttarget.QtyPODropShipReceipts += (plantype.InclQtyPODropShipReceipts ?? 0) * qty;
            }
            target.QtyInTransit += (plantype.InclQtyInTransit ?? 0) * qty;

			decimal avail = 0m, hardAvail = 0m;

			avail -= target.InclQtyINIssues == true ? (plantype.InclQtyINIssues ?? 0) * qty : 0m;
			avail += target.InclQtyINReceipts == true ? (plantype.InclQtyINReceipts ?? 0) * qty : 0m;
			avail += target.InclQtyInTransit == true ? (plantype.InclQtyInTransit ?? 0) * qty : 0m;
			avail += target.InclQtyPOPrepared == true ? (plantype.InclQtyPOPrepared ?? 0) * qty : 0m;
			avail += target.InclQtyPOOrders == true ? (plantype.InclQtyPOOrders ?? 0) * qty : 0m;
			avail += target.InclQtyPOReceipts == true ? (plantype.InclQtyPOReceipts ?? 0) * qty : 0m;
			avail += target.InclQtyINAssemblySupply == true ? (plantype.InclQtyINAssemblySupply ?? 0) * qty : 0m;
			avail -= target.InclQtySOBackOrdered == true ? (plantype.InclQtySOBackOrdered ?? 0) * qty : 0m;
			avail -= target.InclQtySOBooked == true ? (plantype.InclQtySOBooked ?? 0) * qty : 0m;
			avail -= target.InclQtySOShipped == true ? (plantype.InclQtySOShipped ?? 0) * qty : 0m;
			avail -= target.InclQtySOShipping == true ? (plantype.InclQtySOShipping ?? 0) * qty : 0m;
			avail -= target.InclQtyINAssemblyDemand == true ? (plantype.InclQtyINAssemblyDemand ?? 0) * qty : 0m;
			
			hardAvail -= target.InclQtySOShipped == true ? (plantype.InclQtySOShipped ?? 0) * qty : 0m;
			hardAvail -= target.InclQtySOShipping == true ? (plantype.InclQtySOShipping ?? 0) * qty : 0m;
			hardAvail -= target.InclQtyINIssues == true ? (plantype.InclQtyINIssues ?? 0) * qty : 0m;				

			if (InclQtyAvail == true)
			{
				target.QtyAvail += avail;
				target.QtyHardAvail += hardAvail;
			}
			else if (InclQtyAvail == false)
			{
				target.QtyNotAvail += avail;
			}
			return target;
		}

		protected virtual INPlanType GetTargetPlanType<TNode>(PXGraph graph, INItemPlan plan, INPlanType plantype)
			where TNode : class, IQtyAllocatedBase
		{
			return plantype;
		}

		protected TNode UpdateAllocatedQuantities<TNode>(PXGraph graph, INItemPlan plan, INPlanType plantype, bool InclQtyAvail)
			where TNode : class, IQtyAllocatedBase
		{
			INPlanType targettype = GetTargetPlanType<TNode>(graph, plan, plantype);
			return UpdateAllocatedQuantitiesBase<TNode>(graph, plan, targettype, InclQtyAvail);
		}

        public static TNode Sum<TNode>(TNode a, TNode b)
            where TNode : class, IQtyAllocatedBase, IBqlTable, new()
        {
            TNode ret = PXCache<TNode>.CreateCopy(a);

            ret.QtyOnHand += b.QtyOnHand;
            ret.QtyAvail += b.QtyAvail;
            ret.QtyHardAvail += b.QtyHardAvail;
            ret.QtyInTransit += b.QtyInTransit;
            ret.QtyNotAvail += b.QtyNotAvail;

            return ret;
        }

		public virtual void Plan_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (e.Row != null && ((INItemPlan)e.Row).InventoryID == null)
			{
				e.Cancel = true;
			}
		}

		public virtual void Plan_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			try
			{
				INPlanType plantype = PXSelectReadonly<INPlanType, Where<INPlanType.planType, Equal<Required<INPlanType.planType>>>>.Select(sender.Graph, ((INItemPlan)e.Row).PlanType);

				if (((INItemPlan)e.Row).InventoryID != null &&
						((INItemPlan)e.Row).SubItemID != null &&
						((INItemPlan)e.Row).SiteID != null)
				{
					if (((INItemPlan)e.Row).LocationID != null)
					{
						LocationStatus item = UpdateAllocatedQuantities<LocationStatus>(sender.Graph, (INItemPlan)e.Row, plantype, true);
						UpdateAllocatedQuantities<SiteStatus>(sender.Graph, (INItemPlan)e.Row, plantype, (bool)item.InclQtyAvail);
						if (!string.IsNullOrEmpty(((INItemPlan)e.Row).LotSerialNbr))
						{
							UpdateAllocatedQuantities<LotSerialStatus>(sender.Graph, (INItemPlan)e.Row, plantype, true);
                            UpdateAllocatedQuantities<ItemLotSerial>(sender.Graph, (INItemPlan)e.Row, plantype, true);
                        }
					}
					else
					{
						UpdateAllocatedQuantities<SiteStatus>(sender.Graph, (INItemPlan)e.Row, plantype, true);
					}
				}
			}
			catch 
			{; }
		}

		public virtual void Plan_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			try
			{
				INPlanType plantype = PXSelectReadonly<INPlanType, Where<INPlanType.planType, Equal<Required<INPlanType.planType>>>>.Select(sender.Graph, ((INItemPlan)e.Row).PlanType);

				if (((INItemPlan)e.Row).InventoryID != null &&
						((INItemPlan)e.Row).SubItemID != null &&
						((INItemPlan)e.Row).SiteID != null)
				{
					if (((INItemPlan)e.Row).LocationID != null)
					{
						LocationStatus item = UpdateAllocatedQuantities<LocationStatus>(sender.Graph, (INItemPlan)e.Row, -plantype, true);
						UpdateAllocatedQuantities<SiteStatus>(sender.Graph, (INItemPlan)e.Row, -plantype, (bool)item.InclQtyAvail);

						if (!string.IsNullOrEmpty(((INItemPlan)e.Row).LotSerialNbr))
						{
							UpdateAllocatedQuantities<LotSerialStatus>(sender.Graph, (INItemPlan)e.Row, -plantype, true);
                            UpdateAllocatedQuantities<ItemLotSerial>(sender.Graph, (INItemPlan)e.Row, -plantype, true);
                        }
					}
					else
					{
						UpdateAllocatedQuantities<SiteStatus>(sender.Graph, (INItemPlan)e.Row, -plantype, true);
					}
				}
			}
			catch
			{ ; }
		}

		public virtual void Plan_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Plan_RowInserted(sender, new PXRowInsertedEventArgs(e.Row, false));
			Plan_RowDeleted(sender, new PXRowDeletedEventArgs(e.OldRow, false));
		}

		public static INItemPlan FetchPlan<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly<Field>(data))
			{
				if (attr is INItemPlanIDAttribute)
				{
					return ((INItemPlanIDAttribute)attr).FetchPlan(sender, data);
				}
			}
			return null;
		}

		public virtual INItemPlan FetchPlan(PXCache sender, object orig_Row)
		{
			return FetchPlan(sender, orig_Row, true);
		}

		public virtual INItemPlan FetchPlan(PXCache sender, object orig_Row, bool ReturnCopy)
		{
			object key = sender.GetValue(orig_Row, _FieldOrdinal);
			PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
			INItemPlan info = null;

			if (key != null)
			{
				long id = Convert.ToInt64(key);
				if (id < 0)
				{
					info = new INItemPlan();
					info.PlanID = id;
                    info.InventoryID = ((IItemPlanMaster)orig_Row).InventoryID;
                    info.SiteID = ((IItemPlanMaster)orig_Row).SiteID;
					if (info != null)
					{
						info = (INItemPlan)cache.Locate(info);
					}
				}
				if (info == null)
				{
					if (id < 0)
					{
						foreach (INItemPlan plan in cache.Inserted)
						{
							if (plan.PlanID == id)
							{
								info = plan;
								break;
							}
						}
					}
					else
					{
						info = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(sender.Graph, key);
					}
				}
				if (info == null)
				{
					key = null;
					sender.SetValue(orig_Row, _FieldOrdinal, null);
				}
				else if (ReturnCopy)
				{
					return PXCache<INItemPlan>.CreateCopy(info);
				}
			}

			return info;
		}

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (_ReleaseMode.Value) return;

			PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
			INItemPlan info = FetchPlan(sender, e.Row);

			if (info == null)
			{
				info = DefaultValuesInt(sender, new INItemPlan(), e.Row);
				if (info == null)
				{
					return;
				}
				try
				{
					info = (INItemPlan)cache.Insert(info);
					sender.SetValue(e.Row, _FieldOrdinal, info.PlanID);
				}
				catch
				{ ;}
			}
			else
			{
				INItemPlan old_info = PXCache<INItemPlan>.CreateCopy(info);
				info = DefaultValuesInt(sender, info, e.Row);
				if (info != null)
				{
					if (!cache.ObjectsEqual(info, old_info))
					{
						info.PlanID = null;
						cache.Delete(old_info);
						info = (INItemPlan)cache.Insert(info);
						sender.SetValue(e.Row, _FieldOrdinal, info.PlanID);
					}
					else
					{
						info = (INItemPlan)cache.Update(info);
					}
				}
			}
		}

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (_ReleaseMode.Value) return;

			PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
			INItemPlan info = FetchPlan(sender, e.Row);

			if (info == null)
			{
				info = DefaultValuesInt(sender, new INItemPlan(), e.Row);
				if (info == null)
				{
					return;
				}
				try
				{
					info = (INItemPlan)cache.Insert(info);
					sender.SetValue(e.Row, _FieldOrdinal, info.PlanID);
				}
				catch
				{ ;}
			}
			else
			{
				INItemPlan old_info = PXCache<INItemPlan>.CreateCopy(info);
				info = DefaultValuesInt(sender, info, e.Row);
                if (info != null)
				{
					if (!cache.ObjectsEqual(info, old_info) || !string.Equals(info.LotSerialNbr, old_info.LotSerialNbr))
					{
						info.PlanID = null;
						cache.Delete(old_info);
						info = (INItemPlan)cache.Insert(info);
						sender.SetValue(e.Row, _FieldOrdinal, info.PlanID);

						foreach (INItemPlan demand_info in PXSelect<INItemPlan, Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>>>.Select(sender.Graph, old_info.PlanID))
						{
							demand_info.SupplyPlanID = info.PlanID;
							if (cache.GetStatus(demand_info) == PXEntryStatus.Notchanged)
							{
								cache.SetStatus(demand_info, PXEntryStatus.Updated);
							}
						}
					}
					else
					{
						info = (INItemPlan)cache.Update(info);
					}
				}
				else
				{
					cache.Delete(old_info);
					sender.SetValue(e.Row, _FieldOrdinal, null);
				}
			}
		}

		public virtual void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (_ReleaseMode.Value) return;

			PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
			INItemPlan info = FetchPlan(sender, e.Row);

			if (info != null)
			{
				cache.Delete(info);
			}
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
			object key = sender.GetValue(e.Row, _FieldOrdinal);

			//INItemPlan info = FetchPlan(sender, e.Row, false);

			if (key != null)
			{
				if (Convert.ToInt64(key) < 0L)
				{
					foreach (INItemPlan data in cache.Inserted)
					{
						if (Equals(key, data.PlanID))
						{
							_KeyToAbort = data.PlanID;
							try
							{
							cache.PersistInserted(data);
							}
							catch (PXOuterException ex)
							{
								_KeyToAbort = null;
								for (int i =0; i < ex.InnerFields.Length; i++)
								{
									if (sender.RaiseExceptionHandling(ex.InnerFields[i], e.Row, null, new PXSetPropertyKeepPreviousException(ex.InnerMessages[i])))
									{
										throw new PXRowPersistingException(ex.InnerFields[i], null, ex.InnerMessages[i]);
									}
								}
								return;
							}
							long id = Convert.ToInt64(PXDatabase.SelectIdentity());
							sender.SetValue(e.Row, _FieldOrdinal, id);
							data.PlanID = id;
							cache.Normalize();
							break;
						}
					}
				}
				else
				{
					foreach (INItemPlan data in cache.Updated)
					{
						if (Equals(key, data.PlanID))
						{
							cache.PersistUpdated(data);
							break;
						}
					}
				}
			}
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];

			if (e.TranStatus == PXTranStatus.Aborted)
			{
				if (_KeyToAbort != null && (long)_KeyToAbort < 0L)
				{
					object key = sender.GetValue(e.Row, _FieldOrdinal);
					sender.SetValue(e.Row, _FieldOrdinal, _KeyToAbort);
					foreach (INItemPlan data in cache.Inserted)
					{
						if (Equals(key, data.PlanID))
						{
							data.PlanID = (long)_KeyToAbort;
							break;
						}
					}
				}
				else
				{
					object key = sender.GetValue(e.Row, _FieldOrdinal);
					foreach (INItemPlan data in cache.Updated)
					{
						if (Equals(key, data.PlanID))
						{
							cache.ResetPersisted(data);
						}
					}
				}
				cache.Normalize();
			}
			else if (e.TranStatus == PXTranStatus.Completed)
			{
				object key = sender.GetValue(e.Row, _FieldOrdinal);
				foreach (INItemPlan data in cache.Inserted)
				{
					if (Equals(key, data.PlanID))
					{
						cache.SetStatus(data, PXEntryStatus.Notchanged);
						PXTimeStampScope.PutPersisted(cache, data, sender.Graph.TimeStamp);
						cache.ResetPersisted(data);
					}
				}
				foreach (INItemPlan data in cache.Updated)
				{
					if (Equals(key, data.PlanID))
					{
						cache.SetStatus(data, PXEntryStatus.Notchanged);
						PXTimeStampScope.PutPersisted(cache, data, sender.Graph.TimeStamp);
						cache.ResetPersisted(data);
					}
				}
				//cache.IsDirty = false;
				cache.Normalize();
				_KeyToAbort = null;
			}
		}
		#endregion
	}

	#endregion

	#region INUnitType

	public class INUnitType
	{
		public const short Global = 3;
		public const short ItemClass = 2;
		public const short InventoryItem = 1;

		public class global : Constant<short>
		{
			public global() : base(Global) { ;}
		}
		public class itemClass : Constant<short>
		{
			public itemClass() : base(ItemClass) { ;}
		}
		public class inventoryItem : Constant<short>
		{
			public inventoryItem() : base(InventoryItem) { ;}
		}
	}

	#endregion

	#region AcctSub2Attribute

	public class AcctSub2Attribute : AcctSubAttribute
	{
		public override Type DescriptionField
		{
			get
			{
				return (_SelAttrIndex == -1) ? null : ((PXSelectorAttribute)_Attributes[_SelAttrIndex]).DescriptionField;
			}
			set
			{
				if (_SelAttrIndex != -1)
					((PXSelectorAttribute)_Attributes[_SelAttrIndex]).DescriptionField = value;
			}
		}
	}

	#endregion

	#region INAcctSubDefault

	public class INAcctSubDefault
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
		public class ClassListAttribute : CustomListAttribute
		{
			public ClassListAttribute()
				: base(new string[] { MaskItem, MaskSite, MaskClass }, new string[] { Messages.MaskItem, Messages.MaskSite, Messages.MaskClass })
			{
			}
		}

		public class ReasonCodeListAttribute : CustomListAttribute
		{
			public ReasonCodeListAttribute()
				: base(new string[] { MaskReasonCode, MaskItem, MaskSite, MaskClass }, new string[] { Messages.MaskReasonCode, Messages.MaskItem, Messages.MaskSite, Messages.MaskClass })
			{
			}
		}

        public class POAccrualListAttribute : CustomListAttribute
        {
            public POAccrualListAttribute()
                : base(new string[] { MaskItem, MaskSite, MaskClass, MaskVendor }, new string[] { Messages.MaskItem, Messages.MaskSite, Messages.MaskClass, Messages.MaskVendor })
            {
            }
        }

		public const string MaskReasonCode = "0";
		public const string MaskItem = "I";
		public const string MaskSite = "W";
		public const string MaskClass = "P";
        public const string MaskVendor = "V";

		public static void Required(PXCache sender, PXRowSelectedEventArgs e)
		{
			AcctSubRequired required = new AcctSubRequired(sender, e);
		}

		public static void Required(PXCache sender, PXRowPersistingEventArgs e)
		{
			AcctSubRequired required = new AcctSubRequired(sender, e);
		}

		public class AcctSubRequired
		{
			protected enum AcctSubDefaultClass { FromItem = 0, FromSite = 1, FromClass = 2 }
			protected enum AcctSubDefaultReasonCode { FromReasonCode = 0, FromItem = 1, FromSite = 2, FromClass = 3 }

			#region State
			public bool InvtAcct = false;
			public bool InvtSub = false;
			public bool SalesAcct = false;
			public bool SalesSub = false;
			public bool COGSAcct = false;
			public bool COGSSub = false;
			public bool ReasonCodeSub = false;
			public bool StdCstVarAcct = false;
			public bool StdCstVarSub = false;
			public bool StdCstRevAcct = false;
			public bool StdCstRevSub = false;
			public bool POAccrualAcct = false;
			public bool POAccrualSub = false;

			protected string[] _sources = new ClassListAttribute().AllowedValues;
			protected string[] _rcsources = new ReasonCodeListAttribute().AllowedValues;
			#endregion

			#region Initialization
			protected virtual void Populate(INPostClass postclass, AcctSubDefaultClass option)
			{
				if (postclass != null)
				{
					InvtAcct = InvtAcct || (postclass.InvtAcctDefault == _sources[(int)option]);
					InvtSub = InvtSub || (string.IsNullOrEmpty(postclass.InvtSubMask) == false && postclass.InvtSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);

					SalesAcct = SalesAcct || (postclass.SalesAcctDefault == _sources[(int)option]);
					SalesSub = SalesSub || (string.IsNullOrEmpty(postclass.SalesSubMask) == false && postclass.SalesSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);

					COGSAcct = COGSAcct || (postclass.COGSAcctDefault == _sources[(int)option]);
					COGSSub = COGSSub || (postclass.COGSSubFromSales == false && string.IsNullOrEmpty(postclass.COGSSubMask) == false && postclass.COGSSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);

					StdCstVarAcct = StdCstVarAcct || (postclass.StdCstVarAcctDefault == _sources[(int)option]);
					StdCstVarSub = StdCstVarSub || (string.IsNullOrEmpty(postclass.StdCstVarSubMask) == false && postclass.StdCstVarSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);

					StdCstRevAcct = StdCstRevAcct || (postclass.StdCstRevAcctDefault == _sources[(int)option]);
					StdCstRevSub = StdCstRevSub || (string.IsNullOrEmpty(postclass.StdCstRevSubMask) == false && postclass.StdCstRevSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);

					POAccrualAcct = POAccrualAcct || (postclass.POAccrualAcctDefault == _sources[(int)option]);
					POAccrualSub = POAccrualSub || (string.IsNullOrEmpty(postclass.POAccrualSubMask) == false && postclass.POAccrualSubMask.IndexOf(char.Parse(_sources[(int)option])) > -1);
				}
			}

			protected virtual void Populate(ReasonCode reasoncode, AcctSubDefaultReasonCode option)
			{
				if (reasoncode != null)
				{
					ReasonCodeSub = ReasonCodeSub || (string.IsNullOrEmpty(reasoncode.SubMask) == false && reasoncode.SubMask.IndexOf(char.Parse(_rcsources[(int)option])) > -1);
				}
			}
			#endregion

			#region Ctor
			public AcctSubRequired(PXCache sender, object data)
			{
				if (sender.GetItemType() == typeof(InventoryItem))
				{
					PXCache cache = sender.Graph.Caches[typeof(INPostClass)];
					Populate((INPostClass)cache.Current, AcctSubDefaultClass.FromItem);

					StdCstVarAcct = StdCstVarAcct && (data != null && ((InventoryItem)data).ValMethod == INValMethod.Standard);
                    StdCstVarSub = StdCstVarSub && (data != null && ((InventoryItem)data).ValMethod == INValMethod.Standard);
                    StdCstRevAcct = StdCstRevAcct && (data != null && ((InventoryItem)data).ValMethod == INValMethod.Standard);
                    StdCstRevSub = StdCstRevSub && (data != null && ((InventoryItem)data).ValMethod == INValMethod.Standard);

					foreach (ReasonCode reasoncode in PXSelectReadonly<ReasonCode, Where<ReasonCode.usage, NotEqual<ReasonCodeUsages.sales>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.creditWriteOff>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.balanceWriteOff>>>>>.Select(sender.Graph))
					{
						Populate(reasoncode, AcctSubDefaultReasonCode.FromItem);
					}
				}

				else if (sender.GetItemType() == typeof(INPostClass))
				{
					//class accounts are used for defaulting, combine requirements.
					Populate((INPostClass)data, AcctSubDefaultClass.FromItem);
					Populate((INPostClass)data, AcctSubDefaultClass.FromClass);

					foreach (ReasonCode reasoncode in PXSelectReadonly<ReasonCode, Where<ReasonCode.usage, NotEqual<ReasonCodeUsages.sales>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.creditWriteOff>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.balanceWriteOff>>>>>.Select(sender.Graph))
					{
						Populate(reasoncode, AcctSubDefaultReasonCode.FromClass);
					}
				}

				else if (sender.GetItemType() == typeof(INSite) && PXAccess.FeatureInstalled<FeaturesSet.warehouse>())
				{
					foreach (INPostClass postclass in PXSelectReadonly<INPostClass>.Select(sender.Graph))
					{
						Populate(postclass, AcctSubDefaultClass.FromSite);
					}

					foreach (ReasonCode reasoncode in PXSelectReadonly<ReasonCode, Where<ReasonCode.usage, NotEqual<ReasonCodeUsages.sales>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.creditWriteOff>, And<ReasonCode.usage, NotEqual<ReasonCodeUsages.balanceWriteOff>>>>>.Select(sender.Graph))
					{
						Populate(reasoncode, AcctSubDefaultReasonCode.FromSite);
					}
				}
			}

			public AcctSubRequired(PXCache sender, PXRowSelectedEventArgs e)
				: this(sender, e.Row)
			{
				OnRowSelected(sender, e);
			}

			public AcctSubRequired(PXCache sender, PXRowPersistingEventArgs e)
				: this(sender, e.Row)
			{
				OnRowPersisting(sender, e);
			}
			#endregion

			#region Implementation
			public virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				PXUIFieldAttribute.SetRequired<INPostClass.invtAcctID>(sender, this.InvtAcct || this.InvtSub);
				PXUIFieldAttribute.SetRequired<INPostClass.invtSubID>(sender, this.InvtSub);
				PXUIFieldAttribute.SetRequired<INPostClass.salesAcctID>(sender, this.SalesAcct || this.SalesSub);
				PXUIFieldAttribute.SetRequired<INPostClass.salesSubID>(sender, this.SalesSub);
				PXUIFieldAttribute.SetRequired<INPostClass.cOGSAcctID>(sender, this.COGSAcct || this.COGSSub);
				PXUIFieldAttribute.SetRequired<INPostClass.cOGSSubID>(sender, this.COGSSub);
				PXUIFieldAttribute.SetRequired<INPostClass.stdCstVarAcctID>(sender, this.StdCstVarAcct || this.StdCstVarSub);
				PXUIFieldAttribute.SetRequired<INPostClass.stdCstVarSubID>(sender, this.StdCstVarSub);
				PXUIFieldAttribute.SetRequired<INPostClass.stdCstRevAcctID>(sender, this.StdCstRevAcct || this.StdCstRevSub);
				PXUIFieldAttribute.SetRequired<INPostClass.stdCstRevSubID>(sender, this.StdCstRevSub);
				PXUIFieldAttribute.SetRequired<INPostClass.pOAccrualAcctID>(sender, this.POAccrualAcct || this.POAccrualSub);
				PXUIFieldAttribute.SetRequired<INPostClass.pOAccrualSubID>(sender, this.POAccrualSub);
				PXUIFieldAttribute.SetRequired<INPostClass.reasonCodeSubID>(sender, this.ReasonCodeSub);
			}

			public void ThrowFieldIsEmpty<Field>(PXCache sender, object data)
				where Field : IBqlField
			{
				if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Field).Name)))
				{
					throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, typeof(Field).Name);
				}
			}

			public virtual void OnRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
			{
				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
				{
					if (this.InvtAcct && sender.GetValue<INPostClass.invtAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.invtAcctID>(sender, e.Row);
					}
					if (this.InvtSub && sender.GetValue<INPostClass.invtSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.invtSubID>(sender, e.Row);
					}
					if (this.SalesAcct && sender.GetValue<INPostClass.salesAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.salesAcctID>(sender, e.Row);
					}
					if (this.SalesSub && sender.GetValue<INPostClass.salesSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.salesSubID>(sender, e.Row);
					}
					if (this.COGSAcct && sender.GetValue<INPostClass.cOGSAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.cOGSAcctID>(sender, e.Row);
					}
					if (this.COGSSub && sender.GetValue<INPostClass.cOGSSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.cOGSSubID>(sender, e.Row);
					}
					if (this.StdCstVarAcct && sender.GetValue<INPostClass.stdCstVarAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.stdCstVarAcctID>(sender, e.Row);
					}
					if (this.StdCstVarSub && sender.GetValue<INPostClass.stdCstVarSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.stdCstVarSubID>(sender, e.Row);
					}
					if (this.StdCstRevAcct && sender.GetValue<INPostClass.stdCstRevAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.stdCstRevAcctID>(sender, e.Row);
					}
					if (this.StdCstRevSub && sender.GetValue<INPostClass.stdCstRevSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.stdCstRevSubID>(sender, e.Row);
					}
					if (this.POAccrualAcct && sender.GetValue<INPostClass.pOAccrualAcctID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.pOAccrualAcctID>(sender, e.Row);
					}
					if (this.POAccrualSub && sender.GetValue<INPostClass.pOAccrualSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.pOAccrualSubID>(sender, e.Row);
					}
					if (this.ReasonCodeSub && sender.GetValue<INPostClass.reasonCodeSubID>(e.Row) == null)
					{
						ThrowFieldIsEmpty<INPostClass.reasonCodeSubID>(sender, e.Row);
					}
				}
			}
			#endregion
		}
	}

	#endregion

	#region SubAccountMaskAttribute

	[PXDBString(30, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class SubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "ZZZZZZZZZZ";

		public SubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, INAcctSubDefault.MaskItem, new INAcctSubDefault.ClassListAttribute().AllowedValues, new INAcctSubDefault.ClassListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, params object[] sources)
			where Field : IBqlField
		{
			return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new INAcctSubDefault.ClassListAttribute().AllowedValues, sources);
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			if (string.IsNullOrEmpty(mask))
			{
				object newval;
				graph.Caches[BqlCommand.GetItemType(typeof(Field))].RaiseFieldDefaulting<Field>(null, out newval);
				mask = (string)newval;
			}

			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new INAcctSubDefault.ClassListAttribute().AllowedValues, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(ex, new INAcctSubDefault.ClassListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, bool? stkItem, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			if (string.IsNullOrEmpty(mask))
			{
				object newval;
				graph.Caches[BqlCommand.GetItemType(typeof(Field))].RaiseFieldDefaulting<Field>(null, out newval);
				mask = (string)newval;
			}

			try
			{
					return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, stkItem, new INAcctSubDefault.ClassListAttribute().AllowedValues, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(ex, new INAcctSubDefault.ClassListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	#endregion

    #region POAccrualSubAccountMaskAttribute

    [PXDBString(30, InputMask = "")]
    [PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
    public sealed class POAccrualSubAccountMaskAttribute : AcctSubAttribute
    {
        private const string _DimensionName = "SUBACCOUNT";
				private const string _MaskName = "ZZZZZZZZZX";
        public POAccrualSubAccountMaskAttribute()
            : base()
        {
            PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, INAcctSubDefault.MaskItem, new INAcctSubDefault.POAccrualListAttribute().AllowedValues, new INAcctSubDefault.POAccrualListAttribute().AllowedLabels);
            attr.ValidComboRequired = false;
            _Attributes.Add(attr);
            _SelAttrIndex = _Attributes.Count - 1;
        }

        public static string MakeSub<Field>(PXGraph graph, string mask, params object[] sources)
            where Field : IBqlField
        {
            return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new INAcctSubDefault.POAccrualListAttribute().AllowedValues, sources);
        }

        public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
            where Field : IBqlField
        {
            if (string.IsNullOrEmpty(mask))
            {
                object newval;
                graph.Caches[typeof(Field).DeclaringType].RaiseFieldDefaulting<Field>(null, out newval);
                mask = (string)newval;
            }

            try
            {
                return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new INAcctSubDefault.POAccrualListAttribute().AllowedValues, sources);
            }
            catch (PXMaskArgumentException ex)
            {
                PXCache cache = graph.Caches[fields[ex.SourceIdx].DeclaringType];
                string fieldName = fields[ex.SourceIdx].Name;
                throw new PXMaskArgumentException(ex, new INAcctSubDefault.POAccrualListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
            }
        }
    }

    #endregion

	#region ReasonCodeSubAccountMaskAttribute

	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class ReasonCodeSubAccountMaskAttribute : AcctSubAttribute
	{
		public const string ReasonCode = "R";
		public const string InventoryItem = "I";
		public const string PostingClass = "P";
		public const string Warehouse = "W";

		private static readonly string[] writeOffValues = new string[] { ReasonCode, InventoryItem, Warehouse, PostingClass };
		private static readonly string[] writeOffLabels = new string[] { CS.Messages.ReasonCode, Messages.InventoryItem, Messages.Warehouse, Messages.PostingClass };

		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "ReasonCodeIN";
		public ReasonCodeSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, ReasonCode, writeOffValues, writeOffLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, writeOffValues, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(writeOffLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	#endregion

	#region ILSDetail

	public interface ILSDetail : ILSMaster
	{
		Int32? SplitLineNbr
		{
			get;
			set;
		}
		string AssignedNbr
		{
			get;
			set;
		}
		string LotSerClassID
		{
			get;
			set;
		}
		bool? IsStockItem
		{
			get;
			set;
		}
	}

	#endregion

	#region ILSPrimary
	public interface ILSPrimary : ILSMaster
	{
		decimal? UnassignedQty
		{
			get;
			set;
		}
	}
	#endregion

	#region ILSMaster

	public interface ILSMaster : IItemPlanMaster
	{
		string TranType
		{
			get;
		}
		DateTime? TranDate 
		{ 
			get; 
		}
		Int16? InvtMult
		{
			get;
			set;
		}
	    new Int32? InventoryID
		{
			get;
			set;
		}
	    new Int32? SiteID
		{
			get;
			set;
		}
		Int32? LocationID
		{
			get;
			set;
		}
		Int32? SubItemID
		{
			get;
			set;
		}
		string LotSerialNbr
		{
			get;
			set;
		}
		DateTime? ExpireDate
		{
			get;
			set;
		}
		string UOM
		{
			get;
			set;
		}
		Decimal? Qty
		{
			get;
			set;
		}
		decimal? BaseQty
		{
			get;
			set;
		}
	}

	#endregion

    #region IItemPlanMaster
    public interface IItemPlanMaster
    {
        Int32? InventoryID
        {
            get;
            set;
        }
        Int32? SiteID
        {
            get;
            set;
        }
    }
    #endregion

    #region INUnitSelect2

    public class INUnitSelect2<Table, itemClassID, salesUnit, purchaseUnit, baseUnit, lotSerClass> : PXSelect<INUnit, Where<INUnit.itemClassID, Equal<Current<itemClassID>>, And<INUnit.toUnit, Equal<Optional<baseUnit>>, And<INUnit.fromUnit, NotEqual<Optional<baseUnit>>>>>>
		where Table : INUnit
		where itemClassID : IBqlField
		where salesUnit : IBqlField
		where purchaseUnit : IBqlField
		where baseUnit : IBqlField
		where lotSerClass : IBqlField
	{
		#region State
		protected PXCache TopCache;
		#endregion

		#region Ctor
		public INUnitSelect2(PXGraph graph)
			: base(graph)
		{
			TopCache = this.Cache.Graph.Caches[BqlCommand.GetItemType(typeof(itemClassID))];

			graph.FieldVerifying.AddHandler<salesUnit>(SalesUnit_FieldVerifying);
			graph.FieldVerifying.AddHandler<purchaseUnit>(PurchaseUnit_FieldVerifying);
			graph.FieldVerifying.AddHandler<baseUnit>(BaseUnit_FieldVerifying);
			graph.FieldVerifying.AddHandler<lotSerClass>(LotSerClass_FieldVerifying);

			graph.FieldUpdated.AddHandler<salesUnit>(SalesUnit_FieldUpdated);
			graph.FieldUpdated.AddHandler<purchaseUnit>(PurchaseUnit_FieldUpdated);
			graph.FieldUpdated.AddHandler<baseUnit>(BaseUnit_FieldUpdated);

			graph.RowInserted.AddHandler(TopCache.GetItemType(), Top_RowInserted);
			graph.RowPersisting.AddHandler(TopCache.GetItemType(), Top_RowPersisting);

			graph.FieldDefaulting.AddHandler<INUnit.itemClassID>(INUnit_ItemClassID_FieldDefaulting);
			graph.FieldVerifying.AddHandler<INUnit.itemClassID>(INUnit_ItemClassID_FieldVerifying);
			graph.FieldDefaulting.AddHandler<INUnit.toUnit>(INUnit_ToUnit_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INUnit.unitType>(INUnit_UnitType_FieldDefaulting);
			graph.FieldVerifying.AddHandler<INUnit.unitType>(INUnit_UnitType_FieldVerifying);
            graph.FieldVerifying.AddHandler<INUnit.unitRate>(INUnit_UnitRate_FieldVerifying);
			graph.RowSelected.AddHandler<INUnit>(INUnit_RowSelected);
			graph.RowPersisting.AddHandler<INUnit>(INUnit_RowPersisting);

			this.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>();
		}
		#endregion

		#region Implementation
		protected object TopGetValue<Field>(object data)
			where Field : IBqlField
		{
			if (BqlCommand.GetItemType(typeof(Field)) == TopCache.GetItemType() || TopCache.GetItemType().IsAssignableFrom(BqlCommand.GetItemType(typeof(Field))))
			{
				return this.TopCache.GetValue<Field>(data);
			}
			else
			{
				PXCache cache = this.Cache.Graph.Caches[BqlCommand.GetItemType(typeof(Field))];
				return cache.GetValue<Field>(cache.Current);
			}
		}

		protected DataType TopGetValue<Field, DataType>(object data)
			where Field : IBqlField
		{
			return (DataType)TopGetValue<Field>(data);
		}

		protected object TopGetValue<Field>()
			where Field : IBqlField
		{
			return TopGetValue<Field>(this.TopCache.Current);
		}

		protected DataType TopGetValue<Field, DataType>()
			where Field : IBqlField
		{
			return (DataType)TopGetValue<Field>();
		}

		protected virtual void SalesUnit_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void SalesUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INUnit salesunit = PXSelect<INUnit, Where<INUnit.unitType, Equal<INUnitType.itemClass>, And<INUnit.itemClassID, Equal<Current<itemClassID>>, And<INUnit.toUnit, Equal<Current<baseUnit>>, And<INUnit.fromUnit, Equal<Current<salesUnit>>>>>>>.Select(sender.Graph);

			if (salesunit == null && string.IsNullOrEmpty(TopGetValue<salesUnit, string>(e.Row)) == false)
			{
				salesunit = new INUnit();
				salesunit.UnitType = INUnitType.ItemClass;
				salesunit.ItemClassID = ((INItemClass)e.Row).ItemClassID;
				salesunit.InventoryID = 0;
				salesunit.FromUnit = TopGetValue<salesUnit, string>(e.Row);
				salesunit.ToUnit = TopGetValue<baseUnit, string>(e.Row);
				salesunit.UnitRate = 1m;
				salesunit.UnitMultDiv = "M";

				this.Cache.Insert(salesunit);
			}
		}

		protected virtual void PurchaseUnit_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void PurchaseUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INUnit purchaseunit = PXSelect<INUnit, Where<INUnit.unitType, Equal<INUnitType.itemClass>, And<INUnit.itemClassID, Equal<Current<itemClassID>>, And<INUnit.toUnit, Equal<Current<baseUnit>>, And<INUnit.fromUnit, Equal<Current<purchaseUnit>>>>>>>.Select(sender.Graph);

			if (purchaseunit == null && string.IsNullOrEmpty(TopGetValue<purchaseUnit, string>(e.Row)) == false)
			{
				purchaseunit = new INUnit();
				purchaseunit.UnitType = INUnitType.ItemClass;
				purchaseunit.ItemClassID = ((INItemClass)e.Row).ItemClassID;
				purchaseunit.InventoryID = 0;
				purchaseunit.FromUnit = TopGetValue<purchaseUnit, string>(e.Row);
				purchaseunit.ToUnit = TopGetValue<baseUnit, string>(e.Row);
				purchaseunit.UnitRate = 1m;
				purchaseunit.UnitMultDiv = "M";

				this.Cache.Insert(purchaseunit);
			}
		}

		protected virtual void BaseUnit_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void LotSerClass_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INLotSerClass lotSerClass = ReadLotSerClass(e.NewValue);
			if (lotSerClass != null && lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered)
			{
				foreach (INUnit unit in this.Select())
				{
					if (INUnitAttribute.IsFractional(unit))
					{
						if (this.Cache.GetStatus(unit) == PXEntryStatus.Notchanged)
						{
							this.Cache.SetStatus(unit, PXEntryStatus.Updated);
						}
						this.Cache.RaiseExceptionHandling<INUnit.unitMultDiv>(unit, ((INUnit)unit).UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
					}
				}
			}
		}

		protected virtual void BaseUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (string.Equals((string)e.OldValue, TopGetValue<baseUnit, string>(e.Row)) == false)
			{
				if (string.IsNullOrEmpty((string)e.OldValue) == false)
				{
					INUnit baseunit = new INUnit();
					baseunit.UnitType = INUnitType.ItemClass;
					baseunit.ItemClassID = TopGetValue<itemClassID, string>(e.Row);
					baseunit.InventoryID = 0;
					baseunit.FromUnit = (string)e.OldValue;
					baseunit.ToUnit = (string)e.OldValue;
					baseunit.UnitRate = 1m;
					baseunit.UnitMultDiv = "M";

					this.Cache.Delete(baseunit);

					foreach (INUnit oldunits in this.Select((string)e.OldValue, (string)e.OldValue))
					{
						this.Cache.Delete(oldunits);
					}
				}

				foreach (INUnit globalunit in PXSelect<INUnit, Where<INUnit.unitType, Equal<INUnitType.global>, And<INUnit.toUnit, Equal<Current<baseUnit>>, And<INUnit.fromUnit, NotEqual<Current<baseUnit>>>>>>.Select(sender.Graph))
				{
					INUnit classunit = PXCache<INUnit>.CreateCopy(globalunit);
					classunit.ItemClassID = null;
					classunit.UnitType = null;

					this.Cache.Insert(classunit);
				}
			}

			if (string.IsNullOrEmpty(TopGetValue<baseUnit, string>(e.Row)) == false)
			{
				INUnit baseunit = PXSelect<INUnit, Where<INUnit.itemClassID, Equal<Required<INItemClass.itemClassID>>, And<INUnit.toUnit, Equal<Required<INItemClass.baseUnit>>, And<INUnit.fromUnit, Equal<Required<INItemClass.baseUnit>>>>>>.Select(sender.Graph, TopGetValue<itemClassID>(e.Row), TopGetValue<baseUnit>(e.Row), TopGetValue<baseUnit>(e.Row));

				if (baseunit == null)
				{
					baseunit = new INUnit();
					baseunit.UnitType = INUnitType.ItemClass;
					baseunit.ItemClassID = TopGetValue<itemClassID, string>(e.Row);
					baseunit.InventoryID = 0;
					baseunit.FromUnit = TopGetValue<baseUnit, string>(e.Row);
					baseunit.ToUnit = TopGetValue<baseUnit, string>(e.Row);
					baseunit.UnitRate = 1m;
					baseunit.UnitMultDiv = "M";

					this.Cache.Insert(baseunit);
				}
				sender.RaiseFieldUpdated<salesUnit>(e.Row, TopGetValue<salesUnit>(e.Row));
				sender.RaiseFieldUpdated<purchaseUnit>(e.Row, TopGetValue<purchaseUnit>(e.Row));
			}
		}

		protected virtual void Top_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (string.IsNullOrEmpty(TopGetValue<baseUnit, string>(e.Row)) == false)
			{
				using (ReadOnlyScope rs = new ReadOnlyScope(Cache))
				{
					sender.RaiseFieldUpdated<baseUnit>(e.Row, null);
				}
			}
		}

		protected virtual void Top_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				sender.RaiseFieldUpdated<baseUnit>(e.Row, TopGetValue<baseUnit>(e.Row));
			}
		}

		protected virtual void INUnit_ToUnit_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				((INUnit)e.Row).SampleToUnit = TopGetValue<baseUnit, string>();
				e.NewValue = TopGetValue<baseUnit, string>();
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_ItemClassID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = TopGetValue<itemClassID>();
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_ItemClassID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = TopGetValue<itemClassID>();
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_UnitType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = INUnitType.ItemClass;
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_UnitType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = INUnitType.ItemClass;
				e.Cancel = true;
			}
		}

        protected virtual void INUnit_UnitRate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            Decimal? conversion = (Decimal?)e.NewValue;
            if (conversion == 0m && (string)cache.GetValue<INUnit.unitMultDiv>(e.Row) == "D")
            {
                throw new PXSetPropertyException(CS.Messages.Entry_NE, "0");
            }
        }

		protected virtual void INUnit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INLotSerClass lotSerClass = ReadLotSerClass();

			if (lotSerClass != null && lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered && INUnitAttribute.IsFractional((INUnit)e.Row))
			{
				sender.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, ((INUnit)e.Row).UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
			}
			else
			{
				sender.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, null, null);
			}
		}

		protected virtual void INUnit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				PXCache cache = sender.Graph.Caches[typeof(INLotSerClass)];

				if (cache.Current != null && ((INLotSerClass)cache.Current).LotSerTrack == INLotSerTrack.SerialNumbered && INUnitAttribute.IsFractional((INUnit)e.Row))
				{
					sender.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, ((INUnit)e.Row).UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
				}
			}
		}

		private INLotSerClass ReadLotSerClass()
		{
			PXCache cache = this._Graph.Caches[BqlCommand.GetItemType(typeof(lotSerClass))];
			return ReadLotSerClass(cache.GetValue(cache.Current, typeof(lotSerClass).Name));
		}
		private INLotSerClass ReadLotSerClass(object lotSerClass)
		{
			return PXSelect<INLotSerClass,
			Where<INLotSerClass.lotSerClassID, Equal<Required<INLotSerClass.lotSerClassID>>>>.SelectWindowed(this._Graph, 0, 1, lotSerClass);
		}
		#endregion
	}

	#endregion

	#region INUnitSelect

	public class INUnitSelect<Table, inventoryID, itemClassID, salesUnit, purchaseUnit, baseUnit, lotSerClass> : PXSelect<INUnit, Where<INUnit.inventoryID, Equal<Current<inventoryID>>, And<INUnit.toUnit, Equal<Optional<baseUnit>>, And<INUnit.fromUnit, NotEqual<Optional<baseUnit>>>>>>
		where Table : INUnit
		where inventoryID : IBqlField
		where itemClassID : IBqlField
		where salesUnit : IBqlField
		where purchaseUnit : IBqlField
		where baseUnit : IBqlField
		where lotSerClass : IBqlField
	{
		#region State
		protected PXCache TopCache;
		#endregion

		#region Ctor
		public INUnitSelect(PXGraph graph)
			: base(graph)
		{
			TopCache = this.Cache.Graph.Caches[BqlCommand.GetItemType(typeof(inventoryID))];

			graph.FieldVerifying.AddHandler<salesUnit>(SalesUnit_FieldVerifying);
			graph.FieldVerifying.AddHandler<purchaseUnit>(PurchaseUnit_FieldVerifying);
			graph.FieldVerifying.AddHandler<baseUnit>(BaseUnit_FieldVerifying);

			graph.FieldUpdated.AddHandler<salesUnit>(SalesUnit_FieldUpdated);
			graph.FieldUpdated.AddHandler<purchaseUnit>(PurchaseUnit_FieldUpdated);
			graph.FieldUpdated.AddHandler<baseUnit>(BaseUnit_FieldUpdated);
			graph.FieldVerifying.AddHandler<lotSerClass>(LotSerClass_FieldVerifying);
			graph.RowInserted.AddHandler(TopCache.GetItemType(), Top_RowInserted);
			graph.RowPersisting.AddHandler(TopCache.GetItemType(), Top_RowPersisting);

			graph.FieldDefaulting.AddHandler<INUnit.inventoryID>(INUnit_InventoryID_FieldDefaulting);
			graph.FieldVerifying.AddHandler<INUnit.inventoryID>(INUnit_InventoryID_FieldVerifying);
			graph.RowPersisting.AddHandler<INUnit>(INUnit_RowPersisting);
			graph.RowPersisted.AddHandler<INUnit>(INUnit_RowPersisted);
			graph.FieldDefaulting.AddHandler<INUnit.toUnit>(INUnit_ToUnit_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INUnit.unitType>(INUnit_UnitType_FieldDefaulting);
			graph.FieldVerifying.AddHandler<INUnit.unitType>(INUnit_UnitType_FieldVerifying);
            graph.FieldVerifying.AddHandler<INUnit.unitRate>(INUnit_UnitRate_FieldVerifying);
			graph.RowSelected.AddHandler<INUnit>(INUnit_RowSelected);
            graph.RowInserting.AddHandler<INUnit>(INUnit_RowInserting);
			graph.RowInserted.AddHandler<INUnit>(INUnit_RowInserted);
			graph.RowDeleted.AddHandler<INUnit>(INUnit_RowDeleted);

			this.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>();
		}
		#endregion

		#region Implementation
		protected object TopGetValue<Field>(object data)
			where Field : IBqlField
		{
			if (BqlCommand.GetItemType(typeof(Field)) == TopCache.GetItemType() || TopCache.GetItemType().IsAssignableFrom(BqlCommand.GetItemType(typeof(Field))))
			{
				return this.TopCache.GetValue<Field>(data);
			}
			else
			{
				PXCache cache = this.Cache.Graph.Caches[BqlCommand.GetItemType(typeof(Field))];
				return cache.GetValue<Field>(cache.Current);
			}
		}

		protected DataType TopGetValue<Field, DataType>(object data)
			where Field : IBqlField
		{
			return (DataType)TopGetValue<Field>(data);
		}

		protected object TopGetValue<Field>()
			where Field : IBqlField
		{
			return TopGetValue<Field>(this.TopCache.Current);
		}

		protected DataType TopGetValue<Field, DataType>()
			where Field : IBqlField
		{
			return (DataType)TopGetValue<Field>();
		}

		protected virtual void SalesUnit_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void SalesUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INUnit salesunit = PXSelect<INUnit, Where<INUnit.unitType, Equal<INUnitType.inventoryItem>, And<INUnit.inventoryID, Equal<Current<inventoryID>>, And<INUnit.toUnit, Equal<Current<baseUnit>>, And<INUnit.fromUnit, Equal<Current<salesUnit>>>>>>>.Select(sender.Graph);

            if (salesunit == null && string.IsNullOrEmpty(TopGetValue<salesUnit, string>(e.Row)) == false)
            {
                if ((salesunit = PXSelect<INUnit, Where<INUnit.unitType, Equal<INUnitType.global>, And<INUnit.toUnit, Equal<Current<baseUnit>>, And<INUnit.fromUnit, Equal<Current<salesUnit>>>>>>.Select(sender.Graph)) != null)
                {
                    salesunit = PXCache<INUnit>.CreateCopy(salesunit);
                    salesunit.UnitType = INUnitType.InventoryItem;
                    salesunit.ItemClassID = "";
                    salesunit.InventoryID = TopGetValue<inventoryID, Int32?>(e.Row);
                }
                else
                {
                    salesunit = new INUnit();
                    salesunit.UnitType = INUnitType.InventoryItem;
                    salesunit.ItemClassID = "";
                    salesunit.InventoryID = TopGetValue<inventoryID, Int32?>(e.Row);
                    salesunit.FromUnit = TopGetValue<salesUnit, string>(e.Row);
                    salesunit.ToUnit = TopGetValue<baseUnit, string>(e.Row);
                    salesunit.UnitRate = 1m;
                    salesunit.UnitMultDiv = "M";
                }
                this.Cache.Insert(salesunit);
            }

            //try to delete conversions added when changing base unit copied from item class
            //if purchaseunit is not equal to oldvalue -> delete it
            if (string.IsNullOrEmpty((string)e.OldValue) == false 
                && string.Equals((string)e.OldValue, TopGetValue<purchaseUnit, string>(e.Row)) == false
                && string.Equals((string)e.OldValue, TopGetValue<salesUnit, string>(e.Row)) == false
                && string.Equals((string)e.OldValue, TopGetValue<baseUnit, string>(e.Row)) == false)
            {
                INUnit oldunit = new INUnit();
                oldunit.UnitType = INUnitType.InventoryItem;
                oldunit.ItemClassID = "";
                oldunit.InventoryID = TopGetValue<inventoryID, Int32?>(e.Row);
                oldunit.FromUnit = (string)e.OldValue;
                oldunit.ToUnit = TopGetValue<baseUnit, string>(e.Row);
                oldunit.UnitRate = 1m;
                oldunit.UnitMultDiv = "M";

                if (this.Cache.GetStatus(oldunit) == PXEntryStatus.Inserted)
                {
                    this.Cache.Delete(oldunit);
                }
            }
		}

		protected virtual void PurchaseUnit_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void PurchaseUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INUnit purchaseunit = PXSelect<INUnit, Where<INUnit.unitType, Equal<INUnitType.inventoryItem>, And<INUnit.inventoryID, Equal<Current<inventoryID>>, And<INUnit.toUnit, Equal<Current<baseUnit>>, And<INUnit.fromUnit, Equal<Current<purchaseUnit>>>>>>>.Select(sender.Graph);

            if (purchaseunit == null && string.IsNullOrEmpty(TopGetValue<purchaseUnit, string>(e.Row)) == false)
            {
                if ((purchaseunit = PXSelect<INUnit, Where<INUnit.unitType, Equal<INUnitType.global>, And<INUnit.toUnit, Equal<Current<baseUnit>>, And<INUnit.fromUnit, Equal<Current<purchaseUnit>>>>>>.Select(sender.Graph)) != null)
                {
                    purchaseunit = PXCache<INUnit>.CreateCopy(purchaseunit);
                    purchaseunit.UnitType = INUnitType.InventoryItem;
                    purchaseunit.ItemClassID = "";
                    purchaseunit.InventoryID = TopGetValue<inventoryID, Int32?>(e.Row);
                }
                else
                {
                    purchaseunit = new INUnit();
                    purchaseunit.UnitType = INUnitType.InventoryItem;
                    purchaseunit.ItemClassID = "";
                    purchaseunit.InventoryID = TopGetValue<inventoryID, Int32?>(e.Row);
                    purchaseunit.FromUnit = TopGetValue<purchaseUnit, string>(e.Row);
                    purchaseunit.ToUnit = TopGetValue<baseUnit, string>(e.Row);
                    purchaseunit.UnitRate = 1m;
                    purchaseunit.UnitMultDiv = "M";
                }
                this.Cache.Insert(purchaseunit);
            }

            //try to delete conversions added when changing base unit copied from item class
            //if salesunit is not equal to oldvalue -> delete it
            if (string.IsNullOrEmpty((string)e.OldValue) == false
                && string.Equals((string)e.OldValue, TopGetValue<purchaseUnit, string>(e.Row)) == false
                && string.Equals((string)e.OldValue, TopGetValue<salesUnit, string>(e.Row)) == false
                && string.Equals((string)e.OldValue, TopGetValue<baseUnit, string>(e.Row)) == false)
            {
                INUnit oldunit = new INUnit();
                oldunit.UnitType = INUnitType.InventoryItem;
                oldunit.ItemClassID = "";
                oldunit.InventoryID = TopGetValue<inventoryID, Int32?>(e.Row);
                oldunit.FromUnit = (string)e.OldValue;
                oldunit.ToUnit = TopGetValue<baseUnit, string>(e.Row);
                oldunit.UnitRate = 1m;
                oldunit.UnitMultDiv = "M";

                if (this.Cache.GetStatus(oldunit) == PXEntryStatus.Inserted)
                {
                    this.Cache.Delete(oldunit);
                }
            }
		}

		protected virtual void BaseUnit_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void LotSerClass_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INLotSerClass lotSerClass = ReadLotSerClass(e.NewValue);
			if (lotSerClass != null && lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered)
			{
				foreach (INUnit unit in this.Select())
				{
					if (INUnitAttribute.IsFractional(unit))
					{
						if (this.Cache.GetStatus(unit) == PXEntryStatus.Notchanged)
						{
							this.Cache.SetStatus(unit, PXEntryStatus.Updated);
						}
						this.Cache.RaiseExceptionHandling<INUnit.unitMultDiv>(unit, ((INUnit)unit).UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
					}
				}
			}
		}

		protected virtual void BaseUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (string.Equals((string)e.OldValue, TopGetValue<baseUnit, string>(e.Row)) == false)
			{
				if (string.IsNullOrEmpty((string)e.OldValue) == false)
				{
					INUnit baseunit = new INUnit();
					baseunit.UnitType = INUnitType.InventoryItem;
					baseunit.ItemClassID = "";
					baseunit.InventoryID = TopGetValue<inventoryID, Int32?>(e.Row);
					baseunit.FromUnit = (string)e.OldValue;
					baseunit.ToUnit = (string)e.OldValue;
					baseunit.UnitRate = 1m;
					baseunit.UnitMultDiv = "M";

					this.Cache.Delete(baseunit);

					foreach (INUnit oldunits in this.Select((string)e.OldValue, (string)e.OldValue))
					{
						this.Cache.Delete(oldunits);
					}
				}

				foreach (INUnit classunit in PXSelect<INUnit, Where<INUnit.unitType, Equal<INUnitType.itemClass>, And<INUnit.itemClassID, Equal<Current<itemClassID>>, And<INUnit.toUnit, Equal<Current<baseUnit>>, And<INUnit.fromUnit, NotEqual<Current<baseUnit>>>>>>>.Select(sender.Graph))
				{
					INUnit itemunit = PXCache<INUnit>.CreateCopy(classunit);
					itemunit.InventoryID = TopGetValue<inventoryID, Int32?>(e.Row);
					itemunit.ItemClassID = "";
					itemunit.UnitType = INUnitType.InventoryItem;

					this.Cache.Insert(itemunit);
				}
			}

			if (string.IsNullOrEmpty(TopGetValue<baseUnit, string>(e.Row)) == false)
			{
				INUnit baseunit = PXSelect<INUnit, Where<INUnit.inventoryID, Equal<Required<inventoryID>>, And<INUnit.toUnit, Equal<Required<baseUnit>>, And<INUnit.fromUnit, Equal<Required<baseUnit>>>>>>.Select(sender.Graph, TopGetValue<inventoryID>(e.Row), TopGetValue<baseUnit>(e.Row), TopGetValue<baseUnit>(e.Row));

                if (baseunit == null)
                {
                    baseunit = new INUnit();
                    baseunit.UnitType = INUnitType.InventoryItem;
                    baseunit.ItemClassID = "";
                    baseunit.InventoryID = TopGetValue<inventoryID, Int32?>(e.Row);
                    baseunit.FromUnit = TopGetValue<baseUnit, string>(e.Row);
                    baseunit.ToUnit = TopGetValue<baseUnit, string>(e.Row);
                    baseunit.UnitRate = 1m;
                    baseunit.UnitMultDiv = "M";

                    this.Cache.Insert(baseunit);
                }

				sender.RaiseFieldUpdated<salesUnit>(e.Row, TopGetValue<salesUnit, string>(e.Row));
				sender.RaiseFieldUpdated<purchaseUnit>(e.Row, TopGetValue<purchaseUnit, string>(e.Row));
			}
		}

		protected virtual void Top_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (string.IsNullOrEmpty(TopGetValue<baseUnit, string>(e.Row)) == false)
			{
				using (ReadOnlyScope rs = new ReadOnlyScope(Cache))
				{
					sender.RaiseFieldUpdated<baseUnit>(e.Row, null);
				}
			}
		}

		protected virtual void Top_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				sender.RaiseFieldUpdated<baseUnit>(e.Row, TopGetValue<baseUnit, string>(e.Row));
			}
		}

		protected virtual void INUnit_ToUnit_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				((INUnit)e.Row).SampleToUnit = TopGetValue<baseUnit, string>();
				e.NewValue = TopGetValue<baseUnit, string>();
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_InventoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = TopGetValue<inventoryID>();
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = TopGetValue<inventoryID>();
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_UnitType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = INUnitType.InventoryItem;
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_UnitType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = INUnitType.InventoryItem;
				e.Cancel = true;
			}
		}

        protected virtual void INUnit_UnitRate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            Decimal? conversion = (Decimal?)e.NewValue;
            if (conversion == 0m && (string)cache.GetValue<INUnit.unitMultDiv>(e.Row) == "D")
            {
                throw new PXSetPropertyException(CS.Messages.Entry_NE, "0");
            }
            
        }

		protected virtual void INUnit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				PXCache cache = sender.Graph.Caches[typeof(INLotSerClass)];

				if (cache.Current != null && ((INLotSerClass)cache.Current).LotSerTrack == INLotSerTrack.SerialNumbered && INUnitAttribute.IsFractional((INUnit)e.Row))
				{
					sender.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, ((INUnit)e.Row).UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
				}
			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && ((INUnit)e.Row).InventoryID < 0 && TopCache.Current != null)
			{
				int? _KeyToAbort = TopGetValue<inventoryID, Int32?>();
				if (!_persisted.ContainsKey(_KeyToAbort))
				{
					_persisted.Add(_KeyToAbort, ((INUnit)e.Row).InventoryID);
				}
				((INUnit)e.Row).InventoryID = _KeyToAbort;
				sender.Normalize();
			}
		}

		Dictionary<int?, int?> _persisted = new Dictionary<int?, int?>();

		protected virtual void INUnit_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Aborted && (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				int? _KeyToAbort;
				if (_persisted.TryGetValue(((INUnit)e.Row).InventoryID, out _KeyToAbort))
				{
					((INUnit)e.Row).InventoryID = _KeyToAbort;
				}
			}
		}

		protected virtual void INUnit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INLotSerClass lotSerClass = ReadLotSerClass();

			if (lotSerClass != null && lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered && INUnitAttribute.IsFractional((INUnit)e.Row))
			{
				sender.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, ((INUnit)e.Row).UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
			}
			else
			{
				sender.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, null, null);
			}
		}

        protected virtual void INUnit_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            INUnit unit = (INUnit)e.Row;
            if (unit != null && unit.ToUnit == null)
                e.Cancel = true;
        }
        
        protected virtual void INUnit_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			INUnit unit = (INUnit)e.Row;

			if (unit.UnitType == INUnitType.InventoryItem)
			{
				INUnit global = PXSelect<INUnit, Where<INUnit.unitType, Equal<INUnitType.global>>>.Search<INUnit.fromUnit, INUnit.toUnit>(sender.Graph, unit.FromUnit, unit.FromUnit);
				if (global == null)
				{
					global = new INUnit();
					global.UnitType = INUnitType.Global;
					global.FromUnit = unit.FromUnit;
					global.ToUnit = unit.FromUnit;
					global.InventoryID = 0;
					global.ItemClassID = "";
					global.UnitRate = 1m;
					global.UnitMultDiv = "M";

					sender.RaiseRowInserting(global);

					sender.SetStatus(global, PXEntryStatus.Inserted);
					sender.ClearQueryCache();
				}
			}
		}

		protected virtual void INUnit_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			INUnit unit = (INUnit)e.Row;

			if (unit.UnitType == INUnitType.InventoryItem)
			{
				INUnit global = new INUnit();
				global.UnitType = INUnitType.Global;
				global.FromUnit = unit.FromUnit;
				global.ToUnit = unit.FromUnit;
				global.InventoryID = 0;
				global.ItemClassID = "";

				if (sender.GetStatus(global) == PXEntryStatus.Inserted)
				{
					sender.SetStatus(global, PXEntryStatus.InsertedDeleted);
					sender.ClearQueryCache();
				}
			}
		}

		private INLotSerClass ReadLotSerClass()
		{
			PXCache cache = this._Graph.Caches[BqlCommand.GetItemType(typeof(lotSerClass))];
			return ReadLotSerClass(cache.GetValue(cache.Current, typeof(lotSerClass).Name));
		}

		private INLotSerClass ReadLotSerClass(object lotSerClass)
		{
			return PXSelect<INLotSerClass,
			Where<INLotSerClass.lotSerClassID, Equal<Required<INLotSerClass.lotSerClassID>>>>.SelectWindowed(this._Graph, 0, 1, lotSerClass);
		}

		#endregion
	}

	#endregion

	#region LSParentAttribute

	public class LSParentAttribute : PXParentAttribute
	{
		public LSParentAttribute(Type selectParent)
			: base(selectParent)
		{
		}

		public new static object SelectParent(PXCache cache, object row, Type ParentType)
		{
			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					if (((PXParentAttribute)attr).ParentType == ParentType)
					{
						parents.Insert(0, attr);
					}
					else if (ParentType.IsSubclassOf(((PXParentAttribute)attr).ParentType))
					{
						parents.Add(attr);
					}
				}
			}

			if (parents.Count > 0)
			{
				PXParentAttribute attr = (PXParentAttribute)parents[0];
				PXView parentview = attr.GetParentSelect(cache);

				if (!(parentview.Cache.GetItemType() == ParentType || ParentType.IsAssignableFrom(parentview.Cache.GetItemType())))
				{
					return null;
				}

				//clear view cache
				parentview.Clear();
				return parentview.SelectSingleBound(new object[] { row });
			}
			return null;
		}
	}

	#endregion

	#region LSSelect

    [Serializable]	
	public abstract class LSSelect
	{
        [Serializable]
        public partial class LotSerOptions : IBqlTable
        {
            #region StartNumVal
            public abstract class startNumVal : IBqlField
            {
            }
            protected string _StartNumVal;
            [PXDBString(30)]
            [PXUIField(DisplayName = "Start Lot/Serial Number")]
            public virtual string StartNumVal
            {
                get
                {
                    return _StartNumVal;
                }
                set
                {
                    _StartNumVal = value;
                }
            }
            #endregion
            #region UnassignedQty
            public abstract class unassignedQty : IBqlField
            {
            }
            protected decimal? _UnassignedQty;
            [PXDBDecimal]
            [PXUIField(DisplayName = "Unassigned Qty.", Enabled = false)]
            public virtual decimal? UnassignedQty
            {
                get
                {
                    return _UnassignedQty;
                }
                set
                {
                    _UnassignedQty = value;
                }
            }
            #endregion
            #region Qty
            public abstract class qty : IBqlField
            {
            }
            protected decimal? _Qty;
            [PXDBDecimal]
            [PXUIField(DisplayName = "Quantity to Generate")]
            public virtual decimal? Qty
            {
                get
                {
                    return _Qty;
                }
                set
                {
                    _Qty = value;
                }
            }
            #endregion
            #region AllowGenerate
            public abstract class allowGenerate : IBqlField
            {
            }
            protected bool? _AllowGenerate;
            [PXDBBool]
            public virtual bool? AllowGenerate
            {
                get
                {
                    return _AllowGenerate;
                }
                set
                {
                    _AllowGenerate = value;
                }
            }
            #endregion
            #region IsSerial
            public abstract class isSerial : IBqlField
            {
            }
            protected bool? _IsSerial;
            [PXDBBool]
            public virtual bool? IsSerial
            {
                get
                {
                    return _IsSerial;
                }
                set
                {
                    _IsSerial = value;
                }
            }
            #endregion
        }
	}

	[Obsolete()]
	public abstract class LSSelect<TLSMaster, TLSDetail, TLSDetail_UOM, TLSDetail_Where> : LSSelect<TLSMaster, TLSDetail, TLSDetail_Where>
		where TLSMaster : class, IBqlTable, ILSPrimary, new()
		where TLSDetail : class, IBqlTable, ILSDetail, new()
		where TLSDetail_UOM : IBqlField
		where TLSDetail_Where : IBqlWhere, new()
	{
		public LSSelect(PXGraph graph)
			:base(graph)
		{ 
		}
	}


	public abstract class LSSelect<TLSMaster, TLSDetail, Where> : PXSelect<TLSMaster>
		where TLSMaster : class, IBqlTable, ILSPrimary, new()
		where TLSDetail : class, IBqlTable, ILSDetail, new()
		where Where : IBqlWhere, new()
	{
		#region State
		protected bool _InternallCall = false;
        protected PXDBOperation _Operation = PXDBOperation.Normal;
		protected string _MasterQtyField = "qty";
		protected string _AvailField = "Availability";

		protected BqlCommand _detailbylotserialstatus;

		public string AvailabilityField
		{
			get
			{
				return _AvailField;
			}
		}

		protected Type MasterQtyField
		{
			set
			{
				if (!value.IsNested || BqlCommand.GetItemType(value) != typeof(TLSMaster))
				{
					throw new PXArgumentException();
				}
				this._MasterQtyField = value.Name.ToLower();
				this._Graph.FieldVerifying.AddHandler(MasterCache.GetItemType(), _MasterQtyField, Master_Qty_FieldVerifying);
			}
		}

		protected PXCache MasterCache
		{
			get
			{
				return this._Graph.Caches[typeof(TLSMaster)];
			}
		}
		
		protected TLSMaster MasterCurrent
		{
			get
			{
				return (TLSMaster)MasterCache.Current;
			}
		}

		protected PXCache DetailCache
		{
			get
			{
				return this._Graph.Caches[typeof(TLSDetail)];
			}
		}

		protected virtual void SetEditMode()
		{
			if (!Initialized || PrevCorrectionMode != CorrectionMode || PrevFullMode != FullMode)
			{
				PXUIFieldAttribute.SetEnabled(MasterCache, null, false);
				PXUIFieldAttribute.SetEnabled(DetailCache, null, false);

				if (PrevCorrectionMode = CorrectionMode)
				{
					PXUIFieldAttribute.SetEnabled(MasterCache, null, "LocationID", true);
					PXUIFieldAttribute.SetEnabled(MasterCache, null, "LotSerialNbr", true);
					PXUIFieldAttribute.SetEnabled(MasterCache, null, "ExpireDate", true);

					PXUIFieldAttribute.SetEnabled(DetailCache, null, "Qty", true);
					PXUIFieldAttribute.SetEnabled(DetailCache, null, "LocationID", true);
					PXUIFieldAttribute.SetEnabled(DetailCache, null, "LotSerialNbr", true);
					PXUIFieldAttribute.SetEnabled(DetailCache, null, "ExpireDate", true);
				}

				if (PrevFullMode = FullMode)
				{
					PXUIFieldAttribute.SetEnabled(MasterCache, null, true);
					PXUIFieldAttribute.SetEnabled(DetailCache, null, true);
					PXUIFieldAttribute.SetEnabled(DetailCache, null, "UOM", false);
				}

				Initialized = true;
			}
		}

		public virtual void SetEnabled(bool isEnabled)
		{
			this._Graph.Actions[Prefixed("binLotSerial")].SetEnabled(isEnabled);
		}

		protected bool _AllowInsert = true;
		public override bool AllowInsert
		{
			get
			{
				return this._AllowInsert;
			}
			set
			{
				this._AllowInsert = value;
				this.MasterCache.AllowInsert = value;

				SetEditMode();
			}
		}


		protected bool _AllowUpdate = true;
		public override bool AllowUpdate
		{
			get
			{
				return this._AllowUpdate;
			}
			set
			{
				this._AllowUpdate = value;
				this.MasterCache.AllowUpdate = value;
				this.DetailCache.AllowInsert = value;
				this.DetailCache.AllowUpdate = value;
				this.DetailCache.AllowDelete = value;

				SetEditMode();
			}
		}


		protected bool _AllowDelete = true;
		public override bool AllowDelete
		{
			get
			{
				return this._AllowDelete;
			}
			set
			{
				this._AllowDelete = value;
				this.MasterCache.AllowDelete = value;

				SetEditMode();
			}
		}

		protected bool Initialized;
		protected bool PrevCorrectionMode;
		protected bool PrevFullMode;

		protected bool CorrectionMode
		{
			get 
			{
				return this._AllowUpdate && this._AllowInsert == false && this._AllowDelete == false;
			}
		}

		protected bool FullMode
		{
			get
			{
				return this._AllowUpdate && this._AllowInsert && this._AllowDelete;
			}
		}

		protected Type PrimaryViewType
		{
			get
			{
				string primaryView = _Graph.PrimaryView;

				if(primaryView == null || !_Graph.Views.ContainsKey(primaryView))
					throw new PXException(Messages.CantGetPrimaryView, _Graph.GetType().FullName);

				return _Graph.Views[primaryView].GetItemType();
			}
		}

		protected class Counters
		{
			public int RecordCount;
			public decimal BaseQty;
			public Dictionary<DateTime?, int> ExpireDates = new Dictionary<DateTime?, int>();
			public int ExpireDatesNull;
			public DateTime? ExpireDate;
			public Dictionary<int?, int> SubItems = new Dictionary<int?, int>();
			public int SubItemsNull;
			public int? SubItem;
			public Dictionary<int?, int> Locations = new Dictionary<int?, int>();
			public int LocationsNull;
			public int? Location;
			public Dictionary<string, int> LotSerNumbers = new Dictionary<string, int>();
			public int LotSerNumbersNull;
			public string LotSerNumber;
            public int UnassignedNumber;
		}
		protected Dictionary<object, Counters> DetailCounters = new Dictionary<object, Counters>();

		#endregion

		#region Ctor

		public LSSelect(PXGraph graph)
			: base(graph)
		{
			graph.RowInserted.AddHandler<TLSMaster>(Master_RowInserted);
			graph.RowUpdated.AddHandler<TLSMaster>(Master_RowUpdated);
			graph.RowDeleted.AddHandler<TLSMaster>(Master_RowDeleted);
			graph.RowPersisting.AddHandler<TLSMaster>(Master_RowPersisting);
			graph.RowPersisted.AddHandler<TLSMaster>(Master_RowPersisted);

			graph.RowInserting.AddHandler<TLSDetail>(Detail_RowInserting);
			graph.RowInserted.AddHandler<TLSDetail>(Detail_RowInserted);
			graph.RowUpdated.AddHandler<TLSDetail>(Detail_RowUpdated);
			graph.RowDeleted.AddHandler<TLSDetail>(Detail_RowDeleted);
			graph.RowPersisting.AddHandler<TLSDetail>(Detail_RowPersisting);
			graph.RowPersisted.AddHandler<TLSDetail>(Detail_RowPersisted);

			Type inventoryType = null;
			Type subItemType = null;
			Type siteType = null;
			Type locationType = null;
			Type lotSerialNbrType = null;

			foreach (PXEventSubscriberAttribute attr in this.DetailCache.GetAttributesReadonly(null))
			{
				if (attr is INUnitAttribute)
				{
					graph.FieldDefaulting.AddHandler(this.DetailCache.GetItemType(), attr.FieldName, Detail_UOM_FieldDefaulting);
				}

				if (attr is InventoryAttribute)
				{
					inventoryType = this.DetailCache.GetBqlField(attr.FieldName);
				}

				if (attr is SubItemAttribute)
				{
					subItemType = this.DetailCache.GetBqlField(attr.FieldName);
				}

				if (attr is SiteAttribute)
				{
					siteType = this.DetailCache.GetBqlField(attr.FieldName);
				}

				if (attr is LocationAttribute)
				{
					locationType = this.DetailCache.GetBqlField(attr.FieldName);
				}

				if (attr is INLotSerialNbrAttribute)
				{
					lotSerialNbrType = this.DetailCache.GetBqlField(attr.FieldName);
				}

				if (attr is PXDBQuantityAttribute && ((PXDBQuantityAttribute)attr).KeyField != null)
				{
					graph.FieldVerifying.AddHandler(this.DetailCache.GetItemType(), attr.FieldName, Detail_Qty_FieldVerifying);
				}
			}

			_detailbylotserialstatus = BqlCommand.CreateInstance(
				typeof(Select<,>),
				typeof(TLSDetail),
				typeof(Where<,,>),
				inventoryType,
				typeof(Equal<>),
				typeof(Required<>),
				inventoryType,
				typeof(And<,,>),
				subItemType,
				typeof(Equal<>),
				typeof(Required<>),
				subItemType,
				typeof(And<,,>),
				siteType,
				typeof(Equal<>),
				typeof(Required<>),
				siteType,
				typeof(And<,,>),
				locationType,
				typeof(Equal<>),
				typeof(Required<>),
				locationType,
				typeof(And<,,>),
				lotSerialNbrType,
				typeof(Equal<>),
				typeof(Required<>),
				lotSerialNbrType,
				typeof(And<>),
				typeof(Where));

			graph.Caches[typeof(TLSMaster)].Fields.Add(_AvailField);
			graph.FieldSelecting.AddHandler(typeof(TLSMaster), _AvailField, Availability_FieldSelecting);

			graph.Views.Caches.Add(typeof(InventoryItem));
			graph.Views.Caches.Add(typeof(INLotSerClass));

			graph.Views.Add(Prefixed("lotseropts"), new PXView(graph, false, new Select<LotSerOptions>(), new PXSelectDelegate(GetLotSerialOpts)));
			graph.RowPersisting.AddHandler<LotSerOptions>(LotSerOptions_RowPersisting);
			graph.RowSelected.AddHandler<LotSerOptions>(LotSerOptions_RowSelected);
			graph.FieldSelecting.AddHandler(typeof(LotSerOptions), "StartNumVal", LotSerOptions_StartNumVal_FieldSelecting);
			graph.FieldVerifying.AddHandler(typeof(LotSerOptions), "StartNumVal", LotSerOptions_StartNumVal_FieldVerifying);

			AddAction(Prefixed("generateLotSerial"), Messages.Generate, true, GenerateLotSerial, PXCacheRights.Update);
			AddAction(Prefixed("binLotSerial"), Messages.BinLotSerial, true, BinLotSerial, PXCacheRights.Select);

		}
		#endregion

		#region Implementation

		protected string Prefixed(string name)
		{
			return string.Format("{0}_{1}", GetType().Name, name);
		}

		protected void AddAction(string name, string displayName, bool visible, PXButtonDelegate handler, PXCacheRights EnableRights)
		{
			var uiAtt = new PXUIFieldAttribute
			            	{
			            		DisplayName = PXMessages.LocalizeNoPrefix(displayName),
			            		MapEnableRights = EnableRights,
							};
			if (!visible) uiAtt.Visible = false;
			var buttAttr = new PXButtonAttribute();
			var addAttrs = new List<PXEventSubscriberAttribute> { uiAtt, buttAttr };
			_Graph.Actions[name] = (PXAction)Activator.CreateInstance(typeof(PXNamedAction<>).MakeGenericType(
				new []{ PrimaryViewType }), new object[] { _Graph, name, handler, addAttrs.ToArray() });
		}

		public virtual IEnumerable BinLotSerial(PXAdapter adapter)
		{
			View.AskExt(true);
			return adapter.Get();
		}

		protected virtual void LotSerOptions_StartNumVal_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if(MasterCurrent == null) return;
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(MasterCache, MasterCurrent.InventoryID);
			LotSerOptions opt = (LotSerOptions)_Graph.Caches[typeof(LotSerOptions)].Current;
			if(item == null || opt == null) return;

			INLotSerialNbrAttribute.LSParts parts = INLotSerialNbrAttribute.GetLSParts(MasterCache, item);
			if (string.IsNullOrEmpty(((string)e.NewValue)) || ((string)e.NewValue).Length < parts.len)
			{
				opt.StartNumVal = null;
				throw new PXSetPropertyException(Messages.TooShortNum, parts.len);
			}
		}

		public virtual IEnumerable GenerateLotSerial(PXAdapter adapter)
		{
			PXCache clscache = _Graph.Caches[typeof(INLotSerClass)];
			PXCache itcache = _Graph.Caches[typeof(InventoryItem)];

			LotSerOptions opt = (LotSerOptions) _Graph.Caches[typeof (LotSerOptions)].Current;
			if (opt.StartNumVal == null || opt.Qty == null) return adapter.Get();

			InventoryItem initem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(MasterCache.Graph, MasterCurrent.InventoryID);
			INLotSerClass lsclass = PXSelect<INLotSerClass, Where<INLotSerClass.lotSerClassID, Equal<Current<InventoryItem.lotSerClassID>>>>.SelectSingleBound(MasterCache.Graph, new object[] { initem });

			PXResult<InventoryItem, INLotSerClass> item = new PXResult<InventoryItem, INLotSerClass>(initem, lsclass);

			string lotSerialNbr = null;
			INLotSerialNbrAttribute.LSParts parts = INLotSerialNbrAttribute.GetLSParts(MasterCache, item);
			string numVal = opt.StartNumVal.Substring(parts.nidx, parts.nlen); 
			string numStr = opt.StartNumVal.Substring(0, parts.flen) + new string('0', parts.nlen) + opt.StartNumVal.Substring(parts.lidx, parts.llen);

			try
			{
				MasterCurrent.LotSerialNbr = null;
				if (lsclass.LotSerTrack != INLotSerTrack.LotNumbered || (opt.Qty != 0 && MasterCurrent.BaseQty != 0m))
				{
					CreateNumbers(MasterCache, MasterCurrent, (decimal)opt.Qty, true);
				}

				foreach (TLSDetail split in PXParentAttribute.SelectSiblings(DetailCache, null, typeof (TLSMaster)))
				{
					if (string.IsNullOrEmpty(split.AssignedNbr) ||
					    !INLotSerialNbrAttribute.StringsEqual(split.AssignedNbr, split.LotSerialNbr)) continue;

                    TLSDetail copy = PXCache<TLSDetail>.CreateCopy(split);

					if (lotSerialNbr != null)
						numVal = AutoNumberAttribute.NextNumber(numVal);

					if ((decimal)opt.Qty != split.Qty && lsclass.LotSerTrack == INLotSerTrack.LotNumbered)
					{
						split.BaseQty = (decimal)opt.Qty;
						split.Qty = (decimal)opt.Qty;
					}

					lotSerialNbr = INLotSerialNbrAttribute.UpdateNumber(split.AssignedNbr, numStr, numVal);
					split.LotSerialNbr = lotSerialNbr;
					DetailCache.RaiseRowUpdated(split, copy);
				}
			}
			catch(Exception)
			{
				UpdateParent(MasterCache, MasterCurrent);
			}

			if (lotSerialNbr != null)
			{
				PXCache cache = lsclass.LotSerNumShared ?? false ? clscache : itcache;
				ILotSerNumVal copy = (ILotSerNumVal)cache.CreateCopy(lsclass.LotSerNumShared ?? false ? (object)lsclass : initem);
				copy.LotSerNumVal = numVal;
				cache.Update(copy);
			}
			return adapter.Get();
		}

		public virtual IEnumerable GetLotSerialOpts()
		{
			LotSerOptions opt = new LotSerOptions();
			InventoryItem initem = null;
			if (MasterCurrent != null)
			{
				opt.UnassignedQty = MasterCurrent.UnassignedQty;
				initem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(MasterCache.Graph, MasterCurrent.InventoryID);
			}
			if (initem != null && initem.LotSerClassID != null)
			{
				INLotSerClass lsclass = PXSelect<INLotSerClass, Where<INLotSerClass.lotSerClassID, Equal<Required<InventoryItem.lotSerClassID>>>>.Select(MasterCache.Graph, initem.LotSerClassID);
				PXResult<InventoryItem, INLotSerClass> item = new PXResult<InventoryItem, INLotSerClass>(initem, lsclass);
				if (lsclass.LotSerNumShared != true && string.IsNullOrEmpty(initem.LotSerNumVal))
				{
					((InventoryItem)item).LotSerNumVal = new string('0', INLotSerialNbrAttribute.GetNumberLength(MasterCache, item)); 
				}
				if (lsclass.LotSerNumShared == true && string.IsNullOrEmpty(lsclass.LotSerNumVal))
				{
					((INLotSerClass)item).LotSerNumVal = new string('0', INLotSerialNbrAttribute.GetNumberLength(MasterCache, item));
				}

				bool disabled;
				bool allowGernerate;
				using (InvtMultScope<TLSMaster> ms = new InvtMultScope<TLSMaster>(MasterCurrent))
				{
					INLotSerTrack.Mode mode = INLotSerialNbrAttribute.TranTrackMode(lsclass, MasterCurrent.TranType, MasterCurrent.InvtMult);
					disabled = (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Manual) != 0);
					allowGernerate = (mode & INLotSerTrack.Mode.Create) != 0;
				}
				if (!disabled && AllowUpdate)
				{
					string numval = AutoNumberAttribute.NextNumber(lsclass.LotSerNumShared == true ? ((INLotSerClass)item).LotSerNumVal : ((InventoryItem)item).LotSerNumVal);
					string emptynbr = INLotSerialNbrAttribute.GetNextNumber(MasterCache, item);
					string format = INLotSerialNbrAttribute.GetNextFormat(MasterCache, item);
					opt.StartNumVal = INLotSerialNbrAttribute.UpdateNumber(format, emptynbr, numval);
					opt.AllowGenerate = allowGernerate;
                    if (lsclass.LotSerTrack == INLotSerTrack.SerialNumbered)
                        opt.Qty = (int)(MasterCurrent.UnassignedQty ?? 0);
                    else opt.Qty = (MasterCurrent.UnassignedQty ?? 0);
                    opt.IsSerial = (lsclass.LotSerTrack == INLotSerTrack.SerialNumbered ? true : false);
                }
			}
			_Graph.Caches[typeof(LotSerOptions)].Clear();
			opt = (LotSerOptions)_Graph.Caches[typeof(LotSerOptions)].Insert(opt);
			_Graph.Caches[typeof (LotSerOptions)].IsDirty = false;
			yield return opt;
		}

		protected virtual void LotSerOptions_StartNumVal_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			LotSerOptions opt = (LotSerOptions)e.Row;
			if (opt == null || opt.StartNumVal == null || MasterCurrent == null) return;
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(MasterCache, MasterCurrent.InventoryID);
			string mask = INLotSerialNbrAttribute.GetDisplayMask(MasterCache, item);
			if(mask == null) return;
			e.ReturnState = PXStringState.CreateInstance(e.ReturnState, mask.Length, true, "StartNumVal", false, 1, mask, null, null, null, null);
		}

		protected virtual void LotSerOptions_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			LotSerOptions opt = (LotSerOptions)e.Row;

			bool enabled = opt != null && opt.StartNumVal != null;

			PXUIFieldAttribute.SetEnabled<LotSerOptions.startNumVal>(sender, opt, enabled);
			PXUIFieldAttribute.SetEnabled<LotSerOptions.qty>(sender, opt, enabled);
            PXDBDecimalAttribute.SetPrecision(sender, opt, "Qty", (opt.IsSerial == true ? 0 : INSetupDecPl.Qty));
            _Graph.Actions[Prefixed("generateLotSerial")].SetEnabled(opt != null && opt.AllowGenerate == true && enabled);
		}

		protected virtual void LotSerOptions_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}


		public class InvtMultScope<TNode> : IDisposable
			where TNode : class, ILSMaster
		{
			private TNode _item;
			private TNode _olditem;
			private bool? _Reverse;
			private bool? _ReverseOld;

			public InvtMultScope(TNode item)
			{
				_Reverse = (item.InvtMult == (short)1 && item.Qty < 0m);
				_item = item;

				if (_Reverse == true)
				{
					_item.InvtMult = (short)-1;
					_item.Qty = -1m * (decimal)_item.Qty;
					_item.BaseQty = -1m * (decimal)_item.BaseQty;
				}
			}

			public InvtMultScope(TNode item, TNode olditem)
				: this(item)
			{
				_ReverseOld = (olditem.InvtMult == (short)1 && olditem.Qty < 0m);
				_olditem = olditem;

				if (_ReverseOld == true)
				{
					_olditem.InvtMult = (short)-1;
					_olditem.Qty = -1m * (decimal)_olditem.Qty;
					_olditem.BaseQty = -1m * (decimal)_olditem.BaseQty;
				}
			}

			void IDisposable.Dispose()
			{
				if (_Reverse == true)
				{
					_item.InvtMult = (short)1;
					_item.Qty = -1m * (decimal)_item.Qty;
					_item.BaseQty = -1m * (decimal)_item.BaseQty;
				}

				if (_ReverseOld == true)
				{
					_olditem.InvtMult = (short)1;
					_olditem.Qty = -1m * (decimal)_olditem.Qty;
					_olditem.BaseQty = -1m * (decimal)_olditem.BaseQty;
				}
			}
		}

		public class PXRowInsertedEventArgs<Table>
			where Table : class, IBqlTable
		{
			protected Table _Row;
			protected bool Cancel;
			public readonly bool ExternalCall;

			public Table Row
			{
				get
				{
					return this._Row;
				}
				set
				{
					this._Row = value;
				}
			}

			public PXRowInsertedEventArgs(Table Row, bool ExternalCall)
			{
				this.Row = Row;
				this.ExternalCall = ExternalCall;
			}

			public PXRowInsertedEventArgs(PXRowInsertedEventArgs e)
				: this((Table)e.Row, e.ExternalCall)
			{
			}
		}

		public class PXRowUpdatedEventArgs<Table>
			where Table : class, IBqlTable
		{
			protected Table _Row;
			protected Table _OldRow;
			protected bool Cancel;
			public readonly bool ExternalCall;

			public Table Row
			{
				get
				{
					return this._Row;
				}
				set
				{
					this._Row = value;
				}
			}

			public Table OldRow
			{
				get
				{
					return this._OldRow;
				}
				set
				{
					this._OldRow = value;
				}
			}

			public PXRowUpdatedEventArgs(Table Row, Table OldRow, bool ExternalCall)
			{
				this.Row = Row;
				this.OldRow = OldRow;
				this.ExternalCall = ExternalCall;
			}

			public PXRowUpdatedEventArgs(PXRowUpdatedEventArgs e)
				: this((Table)e.Row, (Table)e.OldRow, e.ExternalCall)
			{
			}
		}

		public class PXRowDeletedEventArgs<Table>
			where Table : class, IBqlTable
		{
			protected Table _Row;
			protected bool Cancel;
			public readonly bool ExternalCall;

			public Table Row
			{
				get
				{
					return this._Row;
				}
				set
				{
					this._Row = value;
				}
			}

			public PXRowDeletedEventArgs(Table Row, bool ExternalCall)
			{
				this.Row = Row;
				this.ExternalCall = ExternalCall;
			}

			public PXRowDeletedEventArgs(PXRowDeletedEventArgs e)
				: this((Table)e.Row, e.ExternalCall)
			{
			}
		}

		public abstract TLSDetail Convert(TLSMaster item);

		protected virtual INLotSerialStatus INLotSerialStatus(ILSMaster item)
		{
			INLotSerialStatus ret = new INLotSerialStatus();
			ret.InventoryID = item.InventoryID;
			ret.SiteID = item.SiteID;
			ret.LocationID = item.LocationID;
			ret.SubItemID = item.SubItemID;
			ret.LotSerialNbr = item.LotSerialNbr;

			return ret;
		}

		public virtual string FormatQty(decimal? value)
		{
			return (value == null) ? string.Empty : ((decimal)value).ToString("N" + INSetupDecPl.Qty.ToString(), System.Globalization.NumberFormatInfo.InvariantInfo);
		}

		protected virtual TLSMaster SelectMaster(PXCache sender, TLSDetail row)
		{
			return (TLSMaster)PXParentAttribute.SelectParent(sender, row, typeof(TLSMaster));
		}

		protected virtual bool SameInventoryItem(ILSMaster a, ILSMaster b)
		{
			return a.InventoryID == b.InventoryID;
		}

		protected virtual object[] SelectDetail(PXCache sender, TLSMaster row)
		{
			object[] ret = PXParentAttribute.SelectSiblings(sender, Convert(row), typeof(TLSMaster));

			return Array.FindAll<object>(ret, new Predicate<object>(delegate(object a)
			{
				return SameInventoryItem((ILSMaster)a, (ILSMaster)row);
			}));
		}

		protected virtual object[] SelectDetail(PXCache sender, TLSDetail row)
		{
			object[] ret = PXParentAttribute.SelectSiblings(sender, row, typeof(TLSMaster));

			return Array.FindAll<object>(ret, new Predicate<object>(delegate(object a)
			{
				return SameInventoryItem((ILSMaster)a, (ILSMaster)row);
			}));
		}

		protected object[] SelectDetailOrdered(PXCache sender, TLSMaster row)
		{
			return SelectDetailOrdered(sender, Convert(row));
		}

		protected virtual object[] SelectDetailOrdered(PXCache sender, TLSDetail row)
		{
			object[] ret = SelectDetail(sender, row);

			Array.Sort<object>(ret, new Comparison<object>(delegate(object a, object b)
			{
				object aSplitLineNbr = ((ILSDetail)a).SplitLineNbr;
				object bSplitLineNbr = ((ILSDetail)b).SplitLineNbr;

				return ((IComparable)aSplitLineNbr).CompareTo(bSplitLineNbr);
			}));

			return ret;
		}

		protected object[] SelectDetailReversed(PXCache sender, TLSMaster row)
		{
			return SelectDetailReversed(sender, Convert(row));
		}

		protected virtual object[] SelectDetailReversed(PXCache sender, TLSDetail row)
		{
			object[] ret = SelectDetail(sender, row);

			Array.Sort<object>(ret, new Comparison<object>(delegate(object a, object b)
			{
				object aSplitLineNbr = ((ILSDetail)a).SplitLineNbr;
				object bSplitLineNbr = ((ILSDetail)b).SplitLineNbr;

				return -((IComparable)aSplitLineNbr).CompareTo(bSplitLineNbr);
			}));

			return ret;
		}

		protected virtual void ExpireCached(PXCache sender, object item)
		{
			object cached = sender.Locate(item);

			if (cached != null && (sender.GetStatus(cached) == PXEntryStatus.Held || sender.GetStatus(cached) == PXEntryStatus.Notchanged))
			{
				sender.SetStatus(cached, PXEntryStatus.Notchanged);
				sender.Remove(cached);
			}
		}

		protected PXSelectBase<InventoryItem> _itemselect;

		protected PXResult<InventoryItem, INLotSerClass> ReadInventoryItem(PXCache sender, int? InventoryID)
		{
			if (_itemselect == null)
			{
				_itemselect = new PXSelectJoin<InventoryItem, LeftJoin<INLotSerClass, On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>>, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>, And<Where<InventoryItem.stkItem, Equal<boolFalse>, Or<INLotSerClass.lotSerClassID, IsNotNull>>>>>(sender.Graph);
			}

			return (PXResult<InventoryItem, INLotSerClass>)_itemselect.View.SelectSingle(InventoryID);
		}

		public virtual void CreateNumbers(PXCache sender, TLSMaster Row, decimal BaseQty)
		{
			CreateNumbers(sender, Row, BaseQty, false);
		}

		public virtual void CreateNumbers(PXCache sender, TLSMaster Row, decimal BaseQty, bool ForceAutoNextNbr)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);
			TLSDetail split = Convert(Row);

			if (Row != null)
				DetailCounters.Remove(Row);

			if (!ForceAutoNextNbr && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered &&
				((INLotSerClass)item).AutoSerialMaxCount > 0 && ((INLotSerClass)item).AutoSerialMaxCount < BaseQty)
			{
				BaseQty = ((INLotSerClass) item).AutoSerialMaxCount.GetValueOrDefault();
			}

			INLotSerTrack.Mode mode = INLotSerialNbrAttribute.TranTrackMode(item, Row.TranType, Row.InvtMult);
			foreach (TLSDetail lssplit in INLotSerialNbrAttribute.CreateNumbers<TLSDetail>(sender, item, mode, ForceAutoNextNbr, BaseQty))
			{
				string LotSerTrack = (mode & INLotSerTrack.Mode.Create) > 0 ? ((INLotSerClass)item).LotSerTrack : INLotSerTrack.NotNumbered;

				split.SplitLineNbr = null;
				split.LotSerialNbr = lssplit.LotSerialNbr;
				split.AssignedNbr = lssplit.AssignedNbr;
				split.LotSerClassID = lssplit.LotSerClassID;

				if (!string.IsNullOrEmpty(Row.LotSerialNbr) &&
					((LotSerTrack == INLotSerTrack.SerialNumbered && Row.Qty == 1m) ||
						LotSerTrack == INLotSerTrack.LotNumbered))
				{
					split.LotSerialNbr = Row.LotSerialNbr;
				}

				if (LotSerTrack == "S")
				{
					split.UOM = null;
					split.Qty = 1m;
					split.BaseQty = 1m;
				}
				else 
				{
					split.UOM = null;
					split.BaseQty = BaseQty;
					split.Qty = BaseQty;
				}
				if (((INLotSerClass)item).LotSerTrackExpiration == true)
					split.ExpireDate = ExpireDateByLot(sender, split, Row);

				sender.Graph.Caches[typeof(TLSDetail)].Insert(PXCache<TLSDetail>.CreateCopy(split));
				BaseQty -= (decimal)split.BaseQty;
			}

			if (BaseQty > 0m && (((INLotSerClass)item).LotSerTrack != "S" || decimal.Remainder(BaseQty, 1m) == 0m))
			{
				Row.UnassignedQty += BaseQty;
			}
			else if (BaseQty > 0m)
			{
				TLSMaster oldrow = PXCache<TLSMaster>.CreateCopy(Row);

				Row.BaseQty -= BaseQty;
				Row.Qty = INUnitAttribute.ConvertFromBase(sender, Row.InventoryID, Row.UOM, (decimal)Row.BaseQty, INPrecision.QUANTITY);

				if (Math.Abs((Decimal)oldrow.Qty - (Decimal)Row.Qty) >= 0.0000005m)
				{
				sender.RaiseFieldUpdated(_MasterQtyField, Row, oldrow.Qty);
				sender.RaiseRowUpdated(Row, oldrow);
			}
		}
			if(Row.UnassignedQty > 0)
				sender.RaiseExceptionHandling(_MasterQtyField, Row, null, new PXSetPropertyException(Messages.BinLotSerialNotAssigned, PXErrorLevel.Warning));
		}

		public virtual void CreateNumbers(PXCache sender, TLSMaster Row)
		{
			CreateNumbers(sender, Row, (decimal)Row.BaseQty);
		}

		public virtual void TruncateNumbers(PXCache sender, TLSMaster Row, decimal BaseQty)
		{
			PXCache cache = sender.Graph.Caches[typeof(TLSDetail)];
			PXCache lscache = sender.Graph.Caches[typeof(INLotSerialStatus)];

			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);

			if (((INLotSerClass)item).LotSerTrack == "S" && Math.Abs(Decimal.Floor(BaseQty) - BaseQty) > 0.0000005m)
			{
				TLSMaster oldrow = PXCache<TLSMaster>.CreateCopy(Row);
				Row.BaseQty += BaseQty - Decimal.Truncate(BaseQty);
				Row.Qty = INUnitAttribute.ConvertFromBase(sender, Row.InventoryID, Row.UOM, (decimal)Row.BaseQty, INPrecision.QUANTITY);

				sender.RaiseFieldUpdated(_MasterQtyField, Row, oldrow.Qty);
				sender.RaiseRowUpdated(Row, oldrow);

				BaseQty = Decimal.Truncate(BaseQty);
			}

			if (Row != null)
				DetailCounters.Remove(Row);
			if (Row.UnassignedQty > 0m)
			{
				if (Row.UnassignedQty >= BaseQty)
				{
					Row.UnassignedQty -= BaseQty;
					BaseQty = 0m;
				}
				else
				{
					BaseQty -= (decimal)Row.UnassignedQty;
					Row.UnassignedQty = 0m;
				}
			}

			foreach (object detail in SelectDetailReversed(cache, (TLSMaster)Row))
			{
				if (BaseQty >= ((ILSDetail)detail).BaseQty)
				{
					BaseQty -= (decimal)((ILSDetail)detail).BaseQty;
					cache.Delete(detail);

					ExpireCached(lscache, INLotSerialStatus((TLSDetail)detail));
				}
				else
				{
					TLSDetail newdetail = PXCache<TLSDetail>.CreateCopy((TLSDetail)detail);
					newdetail.BaseQty -= BaseQty;
					newdetail.Qty = INUnitAttribute.ConvertFromBase(sender, newdetail.InventoryID, newdetail.UOM, (decimal)newdetail.BaseQty, INPrecision.QUANTITY);

					cache.Update(newdetail);

					ExpireCached(lscache, INLotSerialStatus((TLSDetail)detail));
					break;
				}
			}
		}

		public virtual void UpdateNumbers(PXCache sender, object Row)
		{
			PXCache cache = sender.Graph.Caches[typeof(TLSDetail)];

			if (Row is TLSMaster)
				DetailCounters.Remove((TLSMaster)Row);
			foreach (object detail in SelectDetail(cache, (TLSMaster)Row))
			{
				TLSDetail newdetail = PXCache<TLSDetail>.CreateCopy((TLSDetail)detail);

				if (((ILSMaster)Row).LocationID == null && newdetail.LocationID != null && cache.GetStatus(newdetail) == PXEntryStatus.Inserted && newdetail.Qty == 0m)
				{
					cache.Delete(newdetail);
				}
				else
				{
					newdetail.SubItemID = ((ILSMaster)Row).SubItemID ?? newdetail.SubItemID;
					newdetail.SiteID = ((ILSMaster)Row).SiteID;
					newdetail.LocationID = ((ILSMaster)Row).LocationID ?? newdetail.LocationID;
					newdetail.ExpireDate = ExpireDateByLot(sender, newdetail, (ILSMaster)Row);
                    
					cache.Update(newdetail);
				}
			}
		}

		public virtual void UpdateNumbers(PXCache sender, object Row, decimal BaseQty)
		{
			PXCache cache = sender.Graph.Caches[typeof(TLSDetail)];
			PXCache lscache = sender.Graph.Caches[typeof(INLotSerialStatus)];

			bool deleteflag = false;

			if (Row is TLSMaster)
				DetailCounters.Remove((TLSMaster)Row);

            if (_Operation == PXDBOperation.Update)
            {
			foreach (object detail in SelectDetail(cache, (TLSMaster)Row))
			{
				if (deleteflag)
				{
					cache.Delete(detail);
					ExpireCached(lscache, INLotSerialStatus((TLSDetail)detail));
				}
				else
				{
					TLSDetail newdetail = PXCache<TLSDetail>.CreateCopy((TLSDetail)detail);

					newdetail.SubItemID = ((ILSMaster)Row).SubItemID;
					newdetail.SiteID = ((ILSMaster)Row).SiteID;
					newdetail.LocationID = ((ILSMaster)Row).LocationID;
					newdetail.LotSerialNbr = ((ILSMaster)Row).LotSerialNbr;
					newdetail.ExpireDate = ExpireDateByLot(sender, newdetail, (ILSMaster)Row);

					newdetail.BaseQty = ((ILSMaster)Row).BaseQty;
					newdetail.Qty = INUnitAttribute.ConvertFromBase(sender, newdetail.InventoryID, newdetail.UOM, (decimal)newdetail.BaseQty, INPrecision.QUANTITY);

					cache.Update(newdetail);

					ExpireCached(lscache, INLotSerialStatus((TLSDetail)detail));

					deleteflag = true;
				}
			}
            }

			if (!deleteflag)
			{
				TLSDetail newdetail = Convert((TLSMaster)Row);
				newdetail.ExpireDate = ExpireDateByLot(sender, newdetail, (TLSMaster)Row);
				DefaultLotSerialNbr(cache, newdetail);

				cache.Insert(newdetail);

				ExpireCached(lscache, INLotSerialStatus((TLSDetail)newdetail));
			}
		}

		protected virtual DateTime? ExpireDateByLot(PXCache sender, ILSMaster item, ILSMaster master)
        {
			if (master != null && master.ExpireDate != null && master.InvtMult > 0)
				return master.ExpireDate;

			var rec = (PXResult<INSite, InventoryItem, INItemRep, S.INItemSite, INItemLotSerial>)
				PXSelectJoin<INSite,
				CrossJoin<InventoryItem,
				LeftJoin<INItemRep, 
			         On<INItemRep.replenishmentClassID, Equal<INSite.replenishmentClassID>,
							And<INItemRep.inventoryID, Equal<InventoryItem.inventoryID>>>,
			 LeftJoin<S.INItemSite,
				     On<S.INItemSite.inventoryID, Equal<InventoryItem.inventoryID>,
						And<S.INItemSite.siteID, Equal<INSite.siteID>>>,
				LeftJoin<INItemLotSerial,
			        On<INItemLotSerial.inventoryID, Equal<InventoryItem.inventoryID>,
				And<INItemLotSerial.lotSerialNbr, Equal<Required<INItemLotSerial.lotSerialNbr>>,
				      And<INItemLotSerial.expireDate, IsNotNull>>>>>>>,
			Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>,
				And<INSite.siteID, Equal<Required<INItemSite.siteID>>>>>
				.SelectWindowed(sender.Graph, 0, 1, item.LotSerialNbr, item.InventoryID, item.SiteID);

			if(rec == null) 
				return (master == null ? null : master.ExpireDate) ?? item.ExpireDate;

			INItemLotSerial status = rec;

			int? shelfLife = ((S.INItemSite)rec).MaxShelfLife ?? ((INItemRep)rec).MaxShelfLife;

			DateTime? defaultExpireDate = shelfLife > 0 && item.TranDate != null
																			? ((DateTime)item.TranDate).AddDays(shelfLife.Value)
																			: (DateTime?)null;

			return (status.InventoryID == null ? null : status.ExpireDate) ??
			       (master == null ? null : master.ExpireDate) ??
			       item.ExpireDate ?? 
						 defaultExpireDate;

		}

		public virtual void Summarize(PXCache sender, object Row, INLotSerialStatus LSRow)
		{
			PXView view = sender.Graph.TypedViews.GetView(_detailbylotserialstatus, false);
			foreach (TLSDetail det in view.SelectMultiBound(new object[] { Row }, LSRow.InventoryID, LSRow.SubItemID, LSRow.SiteID, LSRow.LocationID, LSRow.LotSerialNbr))
			{
				LSRow.QtyOnHand += (decimal?)det.InvtMult * det.BaseQty;
			}
			sender.SetStatus(LSRow, PXEntryStatus.Held);
		}

		public virtual PXSelectBase<INLotSerialStatus> GetSerialStatusCmd(PXCache sender, TLSMaster Row, PXResult<InventoryItem, INLotSerClass> item)
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
				cmd.WhereAnd<Where<INLocation.salesValid, Equal<boolTrue>>>();
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

		public virtual void IssueNumbers(PXCache sender, TLSMaster Row, decimal BaseQty)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);

			PXDBOperation prevOperation = _Operation;
			if (_Operation == PXDBOperation.Update && ((INLotSerClass)item).LotSerTrack == "S" && SelectDetail(DetailCache, Row).Count() == 0)
			{
				_Operation = PXDBOperation.Normal;
			}

			try
			{
				IssueNumbersInt(sender, Row, item, BaseQty);
			}
			finally
			{
				_Operation = prevOperation;
			}
		}

		protected void IssueNumbersInt(PXCache sender, TLSMaster Row, PXResult<InventoryItem, INLotSerClass> item, decimal BaseQty)
		{
			PXSelectBase<INLotSerialStatus> cmd = GetSerialStatusCmd(sender, Row, item);

			TLSDetail split = Convert(Row);

			INLotSerialStatus pars = INLotSerialStatus(Row);

			PXCache lscache = sender.Graph.Caches[typeof(INLotSerialStatus)];
			
			if (Row != null)
				DetailCounters.Remove(Row);

			if((INLotSerialNbrAttribute.TranTrackMode(item, Row.TranType, Row.InvtMult) & INLotSerTrack.Mode.Issue) > 0)
			foreach (PXResult res in cmd.View.SelectMultiBound(new object[] { pars }))
			{
				INLotSerialStatus lsmaster = (INLotSerialStatus)res[typeof(INLotSerialStatus)];

				split.SplitLineNbr = null;
				split.SubItemID = lsmaster.SubItemID;
				split.LocationID = lsmaster.LocationID;
				split.LotSerialNbr = lsmaster.LotSerialNbr;
				split.ExpireDate = lsmaster.ExpireDate;
				split.UOM = ((InventoryItem)item).BaseUnit;

				decimal InclQtyAvail = INItemPlanIDAttribute.GetInclQtyAvail<LotSerialStatus>(_Graph.Caches[typeof(TLSDetail)], split);

				if (InclQtyAvail < 0m)
				{
					LotSerialStatus accumavail = new LotSerialStatus();
					PXCache<INLotSerialStatus>.RestoreCopy(accumavail, lsmaster);

					accumavail = (LotSerialStatus)_Graph.Caches[typeof(LotSerialStatus)].Insert(accumavail);

					decimal? AvailableQty = lsmaster.QtyAvail + accumavail.QtyAvail;

					if (AvailableQty <= 0m)
					{
						continue;
					}

					if (AvailableQty <= BaseQty)
					{
						split.BaseQty = AvailableQty;
						BaseQty -= (decimal)AvailableQty;
					}
					else
					{
						if (((INLotSerClass)item).LotSerTrack == "S")
						{
							split.BaseQty = 1m;
							BaseQty -= 1m;
						}
						else
						{
							split.BaseQty = BaseQty;
							BaseQty = 0m;
						}
					}
				}
				else
				{
					if (lscache.GetStatus(lsmaster) == PXEntryStatus.Notchanged)
					{
						Summarize(lscache, Row, lsmaster);
					}

					if (lsmaster.QtyOnHand <= 0m)
					{
						continue;
					}

					if (lsmaster.QtyOnHand <= BaseQty)
					{
						split.BaseQty = lsmaster.QtyOnHand;
						BaseQty -= (decimal)lsmaster.QtyOnHand;
					}
					else
					{
						if (((INLotSerClass)item).LotSerTrack == "S")
						{
							split.BaseQty = 1m;
							BaseQty -= 1m;
						}
						else
						{
							split.BaseQty = BaseQty;
							BaseQty = 0m;
						}
					}

					lsmaster.QtyOnHand -= split.BaseQty;
					sender.Graph.Caches[typeof(INLotSerialStatus)].SetStatus(lsmaster, PXEntryStatus.Held);
				}

				split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
				sender.Graph.Caches[typeof(TLSDetail)].Insert(PXCache<TLSDetail>.CreateCopy(split));

				if (BaseQty <= 0m)
				{
					break;
				}
			}

			if (BaseQty > 0m && Row.InventoryID != null && Row.SubItemID != null && Row.SiteID != null && Row.LocationID != null && !string.IsNullOrEmpty(Row.LotSerialNbr))
			{
				if (((INLotSerClass)item).LotSerTrack == "S")
				{
					split.BaseQty = 1m;
					BaseQty -= 1m;

					split.Qty = split.BaseQty;
					split.ExpireDate = ExpireDateByLot(sender, split, null);

					if (sender.Graph.Caches[typeof(TLSDetail)].Insert(PXCache<TLSDetail>.CreateCopy(split)) == null)
					{
						BaseQty += 1m;
					}
				}
				else
				{
					split.BaseQty = BaseQty;
					BaseQty = 0m;

					split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
					split.ExpireDate = ExpireDateByLot(sender, split, null);

					sender.Graph.Caches[typeof(TLSDetail)].Insert(PXCache<TLSDetail>.CreateCopy(split));
				}
			}

			if (BaseQty > 0m && (((INLotSerClass)item).LotSerTrack != "S" || decimal.Remainder(BaseQty, 1m) == 0m))
			{
				Row.UnassignedQty += BaseQty;
				sender.RaiseExceptionHandling(_MasterQtyField, Row, null, new PXSetPropertyException(Messages.BinLotSerialNotAssigned, PXErrorLevel.Warning));
			}
			else if (BaseQty != 0m)
			{
				TLSMaster oldrow = PXCache<TLSMaster>.CreateCopy(Row);

				Row.BaseQty -= BaseQty;
				Row.Qty = INUnitAttribute.ConvertFromBase(sender, Row.InventoryID, Row.UOM, (decimal)Row.BaseQty, INPrecision.QUANTITY);

				sender.RaiseFieldUpdated(_MasterQtyField, Row, oldrow.Qty);
				sender.RaiseRowUpdated(Row, oldrow);

				if (((INLotSerClass)item).LotSerTrack == "S" && Math.Abs(decimal.Remainder(BaseQty, 1m)) >= 0.0000005m)
				{
					sender.RaiseExceptionHandling(_MasterQtyField, Row, null, new PXSetPropertyException(Messages.SerialItem_LineQtyUpdated, PXErrorLevel.Warning));
				}
				else
				{
					sender.RaiseExceptionHandling(_MasterQtyField, Row, null, new PXSetPropertyException(Messages.InsuffQty_LineQtyUpdated, PXErrorLevel.Warning));
				}
			}
		}

		public virtual void IssueNumbers(PXCache sender, TLSMaster Row)
		{
			IssueNumbers(sender, Row, (decimal)Row.BaseQty);
		}

        protected virtual void UpdateCounters(PXCache sender, Counters counters, TLSDetail detail)
				{
					counters.RecordCount += 1;
					detail.BaseQty = INUnitAttribute.ConvertToBase(sender, detail.InventoryID, detail.UOM, (decimal)detail.Qty, INPrecision.QUANTITY);
					counters.BaseQty += (decimal)detail.BaseQty;
					if (detail.ExpireDate == null)
					{
						counters.ExpireDatesNull += 1;
					}
					else
					{
						if (counters.ExpireDates.ContainsKey(detail.ExpireDate))
						{
							counters.ExpireDates[detail.ExpireDate] += 1;
						}
						else
						{
							counters.ExpireDates[detail.ExpireDate] = 1;
						}
						counters.ExpireDate = detail.ExpireDate;
					}
					if (detail.SubItemID == null)
					{
						counters.SubItemsNull += 1;
					}
					else
					{
						if (counters.SubItems.ContainsKey(detail.SubItemID))
						{
							counters.SubItems[detail.SubItemID] += 1;
						}
						else
						{
							counters.SubItems[detail.SubItemID] = 1;
						}
						counters.SubItem = detail.SubItemID;
					}
					if (detail.LocationID == null)
					{
						counters.LocationsNull += 1;
					}
					else
					{
						if (counters.Locations.ContainsKey(detail.LocationID))
						{
							counters.Locations[detail.LocationID] += 1;
						}
						else
						{
							counters.Locations[detail.LocationID] = 1;
						}
						counters.Location = detail.LocationID;
					}
					if (detail.LotSerialNbr == null)
					{
						counters.LotSerNumbersNull += 1;
					}
					else
					{
                if (string.IsNullOrEmpty(detail.AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(detail.AssignedNbr, detail.LotSerialNbr))
                {
                    counters.UnassignedNumber++;
                }

						if (counters.LotSerNumbers.ContainsKey(detail.LotSerialNbr))
						{
							counters.LotSerNumbers[detail.LotSerialNbr] += 1;
						}
						else
						{
							counters.LotSerNumbers[detail.LotSerialNbr] = 1;
						}
						counters.LotSerNumber = detail.LotSerialNbr;
					}
				}
        protected Counters counters;
		public virtual void UpdateParent(PXCache sender, TLSMaster Row, TLSDetail Det, TLSDetail OldDet, out decimal BaseQty)
			{
			counters=null;
			if (!DetailCounters.TryGetValue(Row, out counters))
				{
				DetailCounters[Row] = counters = new Counters();
				foreach (TLSDetail detail in SelectDetail(sender.Graph.Caches[typeof(TLSDetail)], Row))
					{
                    UpdateCounters(sender, counters, detail);
					}
						}
						else
						{
				if (Det != null)
						{
                    UpdateCounters(sender, counters, Det);
				}
				if (OldDet != null)
				{
					TLSDetail detail = OldDet;
					counters.RecordCount -= 1;
					detail.BaseQty = INUnitAttribute.ConvertToBase(sender, detail.InventoryID, detail.UOM, (decimal)detail.Qty, INPrecision.QUANTITY);
					counters.BaseQty -= (decimal)detail.BaseQty;
					if (detail.ExpireDate == null)
					{
						counters.ExpireDatesNull -= 1;
					}
					else if (counters.ExpireDates.ContainsKey(detail.ExpireDate))
					{
						if ((counters.ExpireDates[detail.ExpireDate] -= 1) == 0)
						{
							counters.ExpireDates.Remove(detail.ExpireDate);
						}
					}
					if (detail.SubItemID == null)
					{
						counters.SubItemsNull -= 1;
					}
					else if (counters.SubItems.ContainsKey(detail.SubItemID))
					{
						if ((counters.SubItems[detail.SubItemID] -= 1) == 0)
						{
							counters.SubItems.Remove(detail.SubItemID);
						}
					}
					if (detail.LocationID == null)
					{
						counters.LocationsNull -= 1;
					}
					else if (counters.Locations.ContainsKey(detail.LocationID))
					{
						if ((counters.Locations[detail.LocationID] -= 1) == 0)
						{
							counters.Locations.Remove(detail.LocationID);
						}
					}
					if (detail.LotSerialNbr == null)
					{
						counters.LotSerNumbersNull -= 1;
					}
					else if (counters.LotSerNumbers.ContainsKey(detail.LotSerialNbr))
					{
                        if (string.IsNullOrEmpty(detail.AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(detail.AssignedNbr, detail.LotSerialNbr))
                        {
                            counters.UnassignedNumber--;
                        }
						if ((counters.LotSerNumbers[detail.LotSerialNbr] -= 1) == 0)
						{
							counters.LotSerNumbers.Remove(detail.LotSerialNbr);
						}
					}
				}
				if (Det == null && OldDet != null)
				{
					if (counters.ExpireDates.Count == 1 && counters.ExpireDatesNull == 0)
					{
						foreach (DateTime? key in counters.ExpireDates.Keys)
						{
							counters.ExpireDate = key;
						}
					}
					if (counters.SubItems.Count == 1 && counters.SubItemsNull == 0)
					{
						foreach (int? key in counters.SubItems.Keys)
						{
							counters.SubItem = key;
						}
					}
					if (counters.Locations.Count == 1 && counters.LocationsNull == 0)
					{
						foreach (int? key in counters.Locations.Keys)
						{
							counters.Location = key;
						}
					}
					if (counters.LotSerNumbers.Count == 1 && counters.LotSerNumbersNull == 0)
					{
						foreach (string key in counters.LotSerNumbers.Keys)
						{
							counters.LotSerNumber = key;
						}
					}
				}
			}

			BaseQty = counters.BaseQty;

            switch (counters.RecordCount)
			{
				case 0:
					Row.LotSerialNbr = string.Empty;
					break;
				case 1:
				    Row.ExpireDate = counters.ExpireDate;
					Row.SubItemID = counters.SubItem;
					Row.LocationID = counters.Location;
					Row.LotSerialNbr = counters.LotSerNumber;
					break;
				default:
					Row.ExpireDate = counters.ExpireDates.Count == 1 && counters.ExpireDatesNull == 0 ? counters.ExpireDate : null;
					Row.SubItemID = counters.SubItems.Count == 1 && counters.SubItemsNull == 0 ? counters.SubItem : null;
					Row.LocationID = counters.Locations.Count == 1 && counters.LocationsNull == 0 ? counters.Location : null;
					PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);
					INLotSerTrack.Mode mode = INLotSerialNbrAttribute.TranTrackMode(item, Row.TranType, Row.InvtMult);
					if (mode == INLotSerTrack.Mode.None)
					{
						Row.LotSerialNbr = string.Empty;
					}
					else if ((mode & INLotSerTrack.Mode.Create) > 0 || (mode & INLotSerTrack.Mode.Issue) > 0)
					{
						//if more than 1 split exist at lotserial creation time ignore equilness and display <SPLIT>
						Row.LotSerialNbr = null;
					}
					else
					{
						Row.LotSerialNbr = counters.LotSerNumbers.Count == 1 && counters.LotSerNumbersNull == 0 ? counters.LotSerNumber : null;
					}
					break;
			}
		}

		public virtual void UpdateParent(PXCache sender, TLSMaster Row)
		{
			decimal BaseQty;
			UpdateParent(sender, Row, null, null, out BaseQty);
			Row.UnassignedQty = PXDBQuantityAttribute.Round(sender, (decimal)(Row.BaseQty - BaseQty));
		}

		public virtual void UpdateParent(PXCache sender, TLSDetail Row, TLSDetail OldRow)
		{
			TLSMaster parent = (TLSMaster)LSParentAttribute.SelectParent(sender, Row ?? OldRow, typeof(TLSMaster));

			if (parent != null && (Row ?? OldRow) != null && SameInventoryItem((ILSMaster)(Row ?? OldRow), (ILSMaster)parent))
			{
				TLSMaster oldrow = PXCache<TLSMaster>.CreateCopy(parent);
				decimal BaseQty;

				UpdateParent(sender, parent, Row, OldRow, out BaseQty);

				using (InvtMultScope<TLSMaster> ms = new InvtMultScope<TLSMaster>(parent))
				{
					if (BaseQty < parent.BaseQty)
					{
						parent.UnassignedQty = PXDBQuantityAttribute.Round(sender, (decimal)(parent.BaseQty - BaseQty));
					}
					else
					{
						parent.UnassignedQty = 0m;
						parent.BaseQty = BaseQty;
						parent.Qty = INUnitAttribute.ConvertFromBase(sender, parent.InventoryID, parent.UOM, (decimal)parent.BaseQty, INPrecision.QUANTITY);
					}
				}

				if (sender.Graph.Caches[typeof(TLSMaster)].GetStatus(parent) == PXEntryStatus.Notchanged)
				{
					sender.Graph.Caches[typeof(TLSMaster)].SetStatus(parent, PXEntryStatus.Updated);
				}

				if (Math.Abs((Decimal)oldrow.Qty - (Decimal)parent.Qty) >= 0.0000005m)
				{
				sender.Graph.Caches[typeof(TLSMaster)].RaiseFieldUpdated(_MasterQtyField, parent, oldrow.Qty);
				sender.Graph.Caches[typeof(TLSMaster)].RaiseRowUpdated(parent, oldrow);
			}
		}
		}

		public virtual void DefaultLotSerialNbr(PXCache sender, TLSDetail row)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, row.InventoryID);

			if (item != null)
			{
				INLotSerTrack.Mode mode = INLotSerialNbrAttribute.TranTrackMode(item, row.TranType, row.InvtMult);
				if (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Create) > 0)
				{
					foreach (TLSDetail lssplit in INLotSerialNbrAttribute.CreateNumbers<TLSDetail>(sender, item, mode, 1m))
					{
						if (string.IsNullOrEmpty(row.LotSerialNbr))
							row.LotSerialNbr = lssplit.LotSerialNbr;

						row.AssignedNbr = lssplit.AssignedNbr;
						row.LotSerClassID = lssplit.LotSerClassID;
					}
				}
			}
		}

		protected virtual bool Detail_ObjectsEqual(TLSDetail a, TLSDetail b)
		{
			if (a != null && b != null)
			{
				return (a.InventoryID == b.InventoryID && (a.IsStockItem != true ||
								a.SubItemID == b.SubItemID &&
								a.LocationID == b.LocationID &&
								(string.Equals(a.LotSerialNbr, b.LotSerialNbr) || string.IsNullOrEmpty(a.LotSerialNbr) && string.IsNullOrEmpty(b.LotSerialNbr)) &&
								(string.IsNullOrEmpty(a.AssignedNbr) || INLotSerialNbrAttribute.StringsEqual(a.AssignedNbr, a.LotSerialNbr) == false) &&
								(string.IsNullOrEmpty(b.AssignedNbr) || INLotSerialNbrAttribute.StringsEqual(b.AssignedNbr, b.LotSerialNbr) == false)));
			}
			else
			{
				return (a != null);
			}
		}

		protected virtual void Detail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			TLSDetail a = (TLSDetail)e.Row;

			if (!string.IsNullOrEmpty(a.AssignedNbr) && INLotSerialNbrAttribute.StringsEqual(a.AssignedNbr, a.LotSerialNbr))
			{
				return;
			}

            if (_InternallCall && _Operation == PXDBOperation.Insert)
            {
                Counters counters;
                if (!DetailCounters.TryGetValue(MasterCache.Current, out counters))
                {
                    DetailCounters[MasterCache.Current] = counters = new Counters();
                }
                UpdateCounters(MasterCache, counters, a);
            }

			if (_InternallCall && _Operation == PXDBOperation.Update)
			{
				foreach (object item in SelectDetail(sender, (TLSDetail)e.Row))
				{
					TLSDetail detailitem = (TLSDetail)item;

					if (Detail_ObjectsEqual((TLSDetail)e.Row, detailitem))
					{
						PXResult<InventoryItem, INLotSerClass> invtitem = ReadInventoryItem(sender, a.InventoryID);

						if (((INLotSerClass)invtitem).LotSerTrack != "S" || detailitem.BaseQty == 0m)
						{
							object oldDetailItem = PXCache<TLSDetail>.CreateCopy(detailitem);
							detailitem.BaseQty += ((TLSDetail)e.Row).BaseQty;
							detailitem.Qty = INUnitAttribute.ConvertFromBase(sender, detailitem.InventoryID, detailitem.UOM, (decimal)detailitem.BaseQty, INPrecision.QUANTITY);

							sender.RaiseRowUpdated(detailitem, oldDetailItem);
							if (sender.GetStatus(detailitem) == PXEntryStatus.Notchanged)
							{
								sender.SetStatus(detailitem, PXEntryStatus.Updated);
							}
						}
						e.Cancel = true;
						break;
					}
				}
			}

			if (((TLSDetail)e.Row).InventoryID == null || string.IsNullOrEmpty(((TLSDetail)e.Row).UOM))
			{
				e.Cancel = true;
			}

			if (!e.Cancel)
			{
				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((TLSDetail)e.Row).InventoryID);
				INLotSerTrack.Mode mode = INLotSerialNbrAttribute.TranTrackMode(item, ((TLSDetail)e.Row).TranType, ((TLSDetail)e.Row).InvtMult);

				if (mode != INLotSerTrack.Mode.None && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered && ((TLSDetail) e.Row).Qty == 0 && MasterCurrent.UnassignedQty >= 1)				
					((TLSDetail) e.Row).Qty = 1;

				if (((TLSDetail)e.Row).BaseQty == null || ((TLSDetail)e.Row).BaseQty == 0m || ((TLSDetail)e.Row).BaseQty != ((TLSDetail)e.Row).Qty || ((TLSDetail)e.Row).UOM != ((InventoryItem)item).BaseUnit)
				{
					((TLSDetail)e.Row).BaseQty = INUnitAttribute.ConvertToBase(sender, ((TLSDetail)e.Row).InventoryID, ((TLSDetail)e.Row).UOM, ((TLSDetail)e.Row).Qty ?? 0m, INPrecision.QUANTITY);
				}

				((TLSDetail)e.Row).UOM = ((InventoryItem)item).BaseUnit;
				((TLSDetail)e.Row).Qty = ((TLSDetail)e.Row).BaseQty;
			}
		}

		protected virtual void Detail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (_InternallCall)
			{
				return;
			}
			((TLSDetail)e.Row).BaseQty = INUnitAttribute.ConvertToBase(sender, ((TLSDetail)e.Row).InventoryID, ((TLSDetail)e.Row).UOM, (decimal)((TLSDetail)e.Row).Qty, INPrecision.QUANTITY);

			DefaultLotSerialNbr(sender, (TLSDetail)e.Row);

            if (!sender.Graph.UnattendedMode)
            {
                ((TLSDetail)e.Row).ExpireDate = ExpireDateByLot(sender, ((TLSDetail)e.Row), null);
            }

			try
			{
				_InternallCall = true;
				UpdateParent(sender, (TLSDetail)e.Row, null);

                if (!sender.Graph.UnattendedMode)
                {
                    AvailabilityCheck(sender, (TLSDetail)e.Row);
                }
			}
			finally
			{
				_InternallCall = false;
			}
		}

		protected virtual void Detail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PXCache lscache = sender.Graph.Caches[typeof(INLotSerialStatus)];
			ExpireCached(lscache, INLotSerialStatus((TLSDetail)e.OldRow));

			if (_InternallCall)
			{
				return;
			}

			if (((TLSDetail)e.Row).LotSerialNbr != ((TLSDetail)e.OldRow).LotSerialNbr)
			{
				((TLSDetail)e.Row).ExpireDate = ExpireDateByLot(sender, ((TLSDetail)e.Row), null);
            }

			((TLSDetail)e.Row).BaseQty = INUnitAttribute.ConvertToBase(sender, ((TLSDetail)e.Row).InventoryID, ((TLSDetail)e.Row).UOM, (decimal)((TLSDetail)e.Row).Qty, INPrecision.QUANTITY);

			try
			{
				_InternallCall = true;
				UpdateParent(sender, (TLSDetail)e.Row, (TLSDetail)e.OldRow);

                if (!sender.Graph.UnattendedMode)
                {
                    AvailabilityCheck(sender, (TLSDetail)e.Row);
                }
			}
			finally
			{
				_InternallCall = false;
			}
		}

		protected virtual void Detail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			PXCache lscache = sender.Graph.Caches[typeof(INLotSerialStatus)];
			ExpireCached(lscache, INLotSerialStatus((TLSDetail)e.Row));

			if (_InternallCall)
			{
				return;
			}

			try
			{
				_InternallCall = true;
				UpdateParent(sender, null, (TLSDetail)e.Row);
			}
			finally
			{
				_InternallCall = false;
			}
		}

		protected abstract void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXSetPropertyException e);

		public virtual void AvailabilityCheck(PXCache sender, ILSMaster Row)
		{
			if (Row != null && Row.InvtMult == (short)-1 && Row.BaseQty > 0m)
			{
				IQtyAllocated availability = AvailabilityFetch(sender, Row, true);

				AvailabilityCheck(sender, Row, availability);
			}
		}

		public virtual void AvailabilityCheck(PXCache sender, ILSMaster Row, IQtyAllocated availability)
		{
			if (Row.InvtMult == (short)-1 && Row.BaseQty > 0m && availability != null )
			{
				if (availability.QtyNotAvail < 0m && (availability.QtyAvail + availability.QtyNotAvail) < 0m)
				{
					if (availability is LotSerialStatus)
					{
						RaiseQtyExceptionHandling(sender, Row, Row.Qty, new PXSetPropertyException(Messages.StatusCheck_QtyLotSerialNegative));
					}
					else if (availability is LocationStatus)
					{
						RaiseQtyExceptionHandling(sender, Row, Row.Qty, new PXSetPropertyException(Messages.StatusCheck_QtyLocationNegative));
					}
					else if (availability is SiteStatus)
					{
						RaiseQtyExceptionHandling(sender, Row, Row.Qty, new PXSetPropertyException(Messages.StatusCheck_QtyNegative));
					}
				}				
			}
		}

		public virtual void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 255, null, _AvailField, false, null, null, null, null, null, null);
			((PXFieldState)e.ReturnState).Visible = false;
			((PXFieldState)e.ReturnState).Visibility = PXUIVisibility.Invisible;
			((PXFieldState)e.ReturnState).Enabled = false;
		}

		public virtual IQtyAllocated AvailabilityFetch(PXCache sender, ILSMaster Row, bool ExcludeCurrent)
		{
			if (Row != null)
			{
				TLSDetail copy = Row as TLSDetail;
				if (copy == null)
				{
					copy = Convert(Row as TLSMaster);

					PXParentAttribute.SetParent(DetailCache, copy, typeof(TLSMaster), Row);

					if (string.IsNullOrEmpty(Row.LotSerialNbr) == false)
					{
						DefaultLotSerialNbr(sender.Graph.Caches[typeof(TLSDetail)], copy);
					}
				}
				return AvailabilityFetch(sender, copy, ExcludeCurrent);
			}
			return null;
		}

		public virtual IQtyAllocated AvailabilityFetch(PXCache sender, ILSDetail Row, bool ExcludeCurrent)
		{
			if (Row != null && Row.InventoryID != null)
			{
				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);

				if (item == null || (INLotSerClass)item == null || ((INLotSerClass)item).LotSerTrack == null)
				{ 
				}
				else if (Row.SubItemID != null && Row.SiteID != null && Row.LocationID != null && string.IsNullOrEmpty(Row.LotSerialNbr) == false && (string.IsNullOrEmpty(Row.AssignedNbr) || INLotSerialNbrAttribute.StringsEqual(Row.AssignedNbr, Row.LotSerialNbr) == false) && ((INLotSerClass)item).LotSerAssign == "R")
				{
					LotSerialStatus acc = new LotSerialStatus();
					acc.InventoryID = Row.InventoryID;
					acc.SubItemID = Row.SubItemID;
					acc.SiteID = Row.SiteID;
					acc.LocationID = Row.LocationID;
					acc.LotSerialNbr = Row.LotSerialNbr;

					acc = (LotSerialStatus)sender.Graph.Caches[typeof(LotSerialStatus)].Insert(acc);
					acc.QtyNotAvail = 0m;
					sender.Graph.Caches[typeof(LotSerialStatus)].IsDirty = false;

					//do not use INLotSerialStatus for serial items which modify QtyOnHand in INLotSerialStatus and hold row.
					ReadOnlyLotSerialStatus status = (ReadOnlyLotSerialStatus)PXSelect<ReadOnlyLotSerialStatus, 
						Where<ReadOnlyLotSerialStatus.inventoryID, Equal<Required<ReadOnlyLotSerialStatus.inventoryID>>, 
							And<ReadOnlyLotSerialStatus.subItemID, Equal<Required<ReadOnlyLotSerialStatus.subItemID>>, 
							And<ReadOnlyLotSerialStatus.siteID, Equal<Required<ReadOnlyLotSerialStatus.siteID>>, 
							And<ReadOnlyLotSerialStatus.locationID, Equal<Required<ReadOnlyLotSerialStatus.locationID>>, 
							And<ReadOnlyLotSerialStatus.lotSerialNbr, Equal<Required<ReadOnlyLotSerialStatus.lotSerialNbr>>>>>>>>
							.Select(sender.Graph, Row.InventoryID, Row.SubItemID, Row.SiteID, Row.LocationID, Row.LotSerialNbr);


					return AvailabilityFetch<LotSerialStatus>(Row as TLSDetail,
						PXCache<LotSerialStatus>.CreateCopy(acc),
						status,
						ExcludeCurrent);											
				}
				else if (Row.SubItemID != null && Row.SiteID != null && Row.LocationID != null)
				{
					LocationStatus acc = new LocationStatus();
					acc.InventoryID = Row.InventoryID;
					acc.SubItemID = Row.SubItemID;
					acc.SiteID = Row.SiteID;
					acc.LocationID = Row.LocationID;

					acc = (LocationStatus)sender.Graph.Caches[typeof(LocationStatus)].Insert(acc);
					acc.QtyNotAvail = 0m;
					sender.Graph.Caches[typeof(LocationStatus)].IsDirty = false;

					INLocationStatus status = (INLocationStatus)PXSelect<INLocationStatus, 
						Where<INLocationStatus.inventoryID, Equal<Required<INLocationStatus.inventoryID>>, 
							And<INLocationStatus.subItemID, Equal<Required<INLocationStatus.subItemID>>, 
							And<INLocationStatus.siteID, Equal<Required<INLocationStatus.siteID>>, 
							And<INLocationStatus.locationID, Equal<Required<INLocationStatus.locationID>>>>>>>
							.Select(sender.Graph, Row.InventoryID, Row.SubItemID, Row.SiteID, Row.LocationID);


					return AvailabilityFetch<LocationStatus>(Row as TLSDetail,
						PXCache<LocationStatus>.CreateCopy(acc),
						status,
						ExcludeCurrent);										
				}
				else if (Row.SubItemID != null && Row.SiteID != null)
				{
					SiteStatus acc = new SiteStatus();
					acc.InventoryID = Row.InventoryID;
					acc.SubItemID = Row.SubItemID;
					acc.SiteID = Row.SiteID;

					acc = (SiteStatus)sender.Graph.Caches[typeof(SiteStatus)].Insert(acc);
					acc.QtyNotAvail = 0m;
					sender.Graph.Caches[typeof(SiteStatus)].IsDirty = false;

					INSiteStatus status = (INSiteStatus)PXSelect<INSiteStatus,
						Where<INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>,
							And<INSiteStatus.subItemID, Equal<Required<INSiteStatus.subItemID>>,
							And<INSiteStatus.siteID, Equal<Required<INSiteStatus.siteID>>>>>>
							.Select(sender.Graph, Row.InventoryID, Row.SubItemID, Row.SiteID);

					return AvailabilityFetch<SiteStatus>(Row as TLSDetail,  
						PXCache<SiteStatus>.CreateCopy(acc), 
						status, 
						ExcludeCurrent);					
				}
			}
			return null;
		}

		protected virtual IQtyAllocated AvailabilityFetch<TNode>(ILSDetail Row, IQtyAllocated allocated, IStatus status, bool ExcludeCurrent)
			where TNode : class, IQtyAllocated
		{
			if (status != null)
			{
				allocated.QtyOnHand += status.QtyOnHand;
				allocated.QtyAvail += status.QtyAvail;
				allocated.QtyHardAvail += status.QtyHardAvail;				
			}
			if (ExcludeCurrent)
			{
				decimal SignQtyAvail = INItemPlanIDAttribute.GetInclQtyAvail<TNode>
					(_Graph.Caches[typeof(TLSDetail)], 
					Row);
				decimal SignQtyHardAvail = INItemPlanIDAttribute.GetInclQtyHardAvail<TNode>
					(_Graph.Caches[typeof(TLSDetail)],
					Row);
				if (SignQtyAvail != 0)
				{
					allocated.QtyAvail -= SignQtyAvail * (Row.BaseQty ?? 0m);
					allocated.QtyNotAvail += SignQtyAvail * (Row.BaseQty ?? 0m);
				}

				if (SignQtyHardAvail != 0)
				{
					allocated.QtyHardAvail -= SignQtyHardAvail * (Row.BaseQty ?? 0m);					
				}
			}
			return allocated;			
		}

		protected virtual void _Master_RowInserted(PXCache sender, PXRowInsertedEventArgs<TLSMaster> e)
		{
			e.Row.BaseQty = INUnitAttribute.ConvertToBase(sender, e.Row.InventoryID, e.Row.UOM, (decimal)e.Row.Qty, INPrecision.QUANTITY);

			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, e.Row.InventoryID);

			if (item != null && (((InventoryItem)item).StkItem == true || ((InventoryItem)item).KitItem != true))
			{
				INLotSerTrack.Mode mode = INLotSerialNbrAttribute.TranTrackMode(item, e.Row.TranType, e.Row.InvtMult);
				if (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Create) > 0)
				{
          //count for ZERO serial items only here
					if (string.IsNullOrEmpty(e.Row.LotSerialNbr) == false && (e.Row.BaseQty == 0m || e.Row.BaseQty == 1m || ((INLotSerClass)item).LotSerTrack != INLotSerTrack.SerialNumbered))
					{
						UpdateNumbers(sender, e.Row, (decimal)e.Row.BaseQty );
					}
					else
					{
						CreateNumbers(sender, e.Row);
          }
					UpdateParent(sender, e.Row);
				}
				else if ((mode & INLotSerTrack.Mode.Issue) > 0 && e.Row.BaseQty > 0m)
				{
					IssueNumbers(sender, e.Row);

					//do not set Zero LotSerial which will prevent IssueNumbers() on quantity update
					if (e.Row.BaseQty > 0)
					{
						UpdateParent(sender, e.Row);
					}
				}
				//PCB AvailabilityCheck(sender, e.Row);
			}
			else if (item != null)
			{
				foreach (PXResult<INKitSpecStkDet, InventoryItem> res in PXSelectJoin<INKitSpecStkDet,
					InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INKitSpecStkDet.compInventoryID>>>,
					Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.Select(sender.Graph, e.Row.InventoryID))
				{
					INKitSpecStkDet kititem = (INKitSpecStkDet)res;
					InventoryItem item2 = (InventoryItem)res;

					TLSMaster copy = PXCache<TLSMaster>.CreateCopy(e.Row);

					copy.InventoryID = kititem.CompInventoryID;
					copy.SubItemID = kititem.CompSubItemID;
					copy.UOM = kititem.UOM;
					copy.Qty = kititem.DfltCompQty * copy.BaseQty;

					_Master_RowInserted(sender, new PXRowInsertedEventArgs<TLSMaster>(copy, e.ExternalCall));
				}

				foreach (PXResult<INKitSpecNonStkDet, InventoryItem> res in PXSelectJoin<INKitSpecNonStkDet,
					InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INKitSpecNonStkDet.compInventoryID>>>,
					Where<INKitSpecNonStkDet.kitInventoryID, Equal<Required<INKitSpecNonStkDet.kitInventoryID>>,
						And<Where<InventoryItem.kitItem, Equal<True>, Or<InventoryItem.nonStockShip, Equal<True>>>>>>.Select(sender.Graph, e.Row.InventoryID))
				{
					INKitSpecNonStkDet kititem = res;
					InventoryItem item2 = res;

					TLSMaster copy = PXCache<TLSMaster>.CreateCopy(e.Row);

					copy.InventoryID = kititem.CompInventoryID;
					copy.SubItemID = null;
					copy.UOM = kititem.UOM;
					copy.Qty = kititem.DfltCompQty * copy.BaseQty;

					_Master_RowInserted(sender, new PXRowInsertedEventArgs<TLSMaster>(copy, e.ExternalCall));
				}
			}
		}

		protected virtual void _Master_RowDeleted(PXCache sender, PXRowDeletedEventArgs<TLSMaster> e)
		{
			PXCache cache = sender.Graph.Caches[typeof(TLSDetail)];
			if (e.Row != null)
				DetailCounters.Remove(e.Row);
			foreach (object detail in SelectDetail(cache, e.Row))
			{
				cache.Delete(detail);
			}
		}

		public virtual void RaiseRowInserted(PXCache sender, TLSMaster Row)
		{
			_Master_RowInserted(sender, new PXRowInsertedEventArgs<TLSMaster>(Row, false));
		}

		public virtual void RaiseRowDeleted(PXCache sender, TLSMaster Row)
		{
			_Master_RowDeleted(sender, new PXRowDeletedEventArgs<TLSMaster>(Row, false));
		}

		protected virtual void _Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs<TLSMaster> e)
		{
			//Debug.Print("_Master_RowUpdated");
			if (e.OldRow != null && (e.OldRow.InventoryID != e.Row.InventoryID || e.OldRow.InvtMult != e.Row.InvtMult))
			{
				if (e.OldRow.InventoryID != e.Row.InventoryID)
				{
					((TLSMaster)e.Row).LotSerialNbr = null;
					((TLSMaster)e.Row).ExpireDate = null;
				}
				else if (e.OldRow.InvtMult != e.Row.InvtMult)
				{
					if (((TLSMaster)e.Row).LotSerialNbr == ((TLSMaster)e.OldRow).LotSerialNbr)
					{
						((TLSMaster)e.Row).LotSerialNbr = null;
					}
					if (((TLSMaster)e.Row).ExpireDate == ((TLSMaster)e.OldRow).ExpireDate)
					{
						((TLSMaster)e.Row).ExpireDate = null;
					}
				}

				RaiseRowDeleted(sender, e.OldRow);
				RaiseRowInserted(sender, e.Row);
			}
			else
			{
				e.Row.BaseQty = INUnitAttribute.ConvertToBase(sender, e.Row.InventoryID, e.Row.UOM, (decimal)e.Row.Qty, INPrecision.QUANTITY);

				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, e.Row.InventoryID);

				if (e.Row.ExpireDate == e.OldRow.ExpireDate && e.OldRow.LotSerialNbr != e.Row.LotSerialNbr)
				{
					((TLSMaster)e.Row).ExpireDate = null;
				}
				if (item != null && (((InventoryItem)item).StkItem == true || ((InventoryItem)item).KitItem != true))
				{
					INLotSerTrack.Mode mode = INLotSerialNbrAttribute.TranTrackMode(item, ((TLSMaster)e.Row).TranType, e.Row.InvtMult);
					if (mode == INLotSerTrack.Mode.None ||(mode & INLotSerTrack.Mode.Create) > 0 )
					{
						if (e.Row.SubItemID != e.OldRow.SubItemID || e.Row.SiteID != e.OldRow.SiteID || e.Row.LocationID != e.OldRow.LocationID || e.Row.ExpireDate != e.OldRow.ExpireDate)
						{
							if (CorrectionMode == false && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.NotNumbered)
							{
								RaiseRowDeleted(sender, e.OldRow);
								RaiseRowInserted(sender, e.Row);
								return;
							}
							else
							{
								UpdateNumbers(sender, e.Row);
							}
						}
						//count for ZERO serial items only here
						if (string.IsNullOrEmpty(e.Row.LotSerialNbr) == false && (e.Row.BaseQty == 0m || e.Row.BaseQty == 1m || ((INLotSerClass)item).LotSerTrack != INLotSerTrack.SerialNumbered))
						{
							UpdateNumbers(sender, e.Row, (decimal)e.Row.BaseQty - (decimal)e.OldRow.BaseQty);
						}
						else if (e.Row.BaseQty > e.OldRow.BaseQty)
						{
							CreateNumbers(sender, e.Row, (decimal)e.Row.BaseQty - (decimal)e.OldRow.BaseQty);
						}
						//do not truncate ZERO quantity lotserials
						else if (e.Row.BaseQty < e.OldRow.BaseQty)
						{
							TruncateNumbers(sender, e.Row, (decimal)e.OldRow.BaseQty - (decimal)e.Row.BaseQty);
						}

						UpdateParent(sender, e.Row);
					}
					else if ((mode & INLotSerTrack.Mode.Issue) > 0)
					{
						if (e.Row.SubItemID != e.OldRow.SubItemID || e.Row.SiteID != e.OldRow.SiteID || e.Row.LocationID != e.OldRow.LocationID || string.Equals(e.Row.LotSerialNbr, e.OldRow.LotSerialNbr) == false)
						{
							RaiseRowDeleted(sender, e.OldRow);
							RaiseRowInserted(sender, e.Row);
						}
						else if (string.IsNullOrEmpty(e.Row.LotSerialNbr) == false && (e.Row.BaseQty == 1m || ((INLotSerClass)item).LotSerTrack != INLotSerTrack.SerialNumbered))
						{
							UpdateNumbers(sender, e.Row, (decimal)e.Row.BaseQty - (decimal)e.OldRow.BaseQty);
						}
						else if (e.Row.BaseQty > e.OldRow.BaseQty)
						{
							IssueNumbers(sender, e.Row, (decimal)e.Row.BaseQty - (decimal)e.OldRow.BaseQty);
						}
						else
						{
							TruncateNumbers(sender, e.Row, (decimal)e.OldRow.BaseQty - (decimal)e.Row.BaseQty);
						}

						//do not set Zero LotSerial which will prevent IssueNumbers() on quantity update
						if (e.Row.BaseQty > 0)
						{
							UpdateParent(sender, e.Row);
						}
					}
					//PCB AvailabilityCheck(sender, e.Row);
				}
				else if (item != null)
				{
					foreach (PXResult<INKitSpecStkDet, InventoryItem> res in PXSelectJoin<INKitSpecStkDet, InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INKitSpecStkDet.compInventoryID>>>, Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.Select(sender.Graph, e.Row.InventoryID))
					{
						INKitSpecStkDet kititem = (INKitSpecStkDet)res;
						InventoryItem item2 = (InventoryItem)res;

						TLSMaster copy = PXCache<TLSMaster>.CreateCopy(e.Row);

						copy.InventoryID = kititem.CompInventoryID;
						copy.SubItemID = kititem.CompSubItemID;
						copy.UOM = kititem.UOM;
						copy.Qty = kititem.DfltCompQty * copy.BaseQty;

						TLSMaster oldcopy = PXCache<TLSMaster>.CreateCopy(e.OldRow);

						oldcopy.InventoryID = kititem.CompInventoryID;
						oldcopy.SubItemID = kititem.CompSubItemID;
						oldcopy.UOM = kititem.UOM;
						oldcopy.Qty = kititem.DfltCompQty * oldcopy.BaseQty;
						oldcopy.BaseQty = INUnitAttribute.ConvertToBase(sender, oldcopy.InventoryID, oldcopy.UOM, (decimal)oldcopy.Qty, INPrecision.QUANTITY);

						_Master_RowUpdated(sender, new PXRowUpdatedEventArgs<TLSMaster>(copy, oldcopy, e.ExternalCall));
					}

					foreach (PXResult<INKitSpecNonStkDet, InventoryItem> res in PXSelectJoin<INKitSpecNonStkDet, 
						InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INKitSpecNonStkDet.compInventoryID>>>, 
						Where<INKitSpecNonStkDet.kitInventoryID, Equal<Required<INKitSpecNonStkDet.kitInventoryID>>,
							And<Where<InventoryItem.kitItem, Equal<True>, Or<InventoryItem.nonStockShip, Equal<True>>>>>>.Select(sender.Graph, e.Row.InventoryID))
					{
						INKitSpecNonStkDet kititem = res;
						InventoryItem item2 = res;

						TLSMaster copy = PXCache<TLSMaster>.CreateCopy(e.Row);

						copy.InventoryID = kititem.CompInventoryID;
						copy.SubItemID = null;
						copy.UOM = kititem.UOM;
						copy.Qty = kititem.DfltCompQty * copy.BaseQty;

						TLSMaster oldcopy = PXCache<TLSMaster>.CreateCopy(e.OldRow);

						oldcopy.InventoryID = kititem.CompInventoryID;
						oldcopy.SubItemID = null;
						oldcopy.UOM = kititem.UOM;
						oldcopy.Qty = kititem.DfltCompQty * oldcopy.BaseQty;
						oldcopy.BaseQty = INUnitAttribute.ConvertToBase(sender, oldcopy.InventoryID, oldcopy.UOM, (decimal)oldcopy.Qty, INPrecision.QUANTITY);

						_Master_RowUpdated(sender, new PXRowUpdatedEventArgs<TLSMaster>(copy, oldcopy, e.ExternalCall));
					}
				}
			}
		}

		protected virtual void Master_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (_InternallCall)
			{
				return;
			}

			try
			{
				_InternallCall = true;
                _Operation = PXDBOperation.Insert;
				using (InvtMultScope<TLSMaster> ms = new InvtMultScope<TLSMaster>((TLSMaster)e.Row))
				{
					_Master_RowInserted(sender, new PXRowInsertedEventArgs<TLSMaster>(e));
				}
			}
			finally
			{
				_InternallCall = false;
                _Operation = PXDBOperation.Normal;
			}
		}

		protected virtual void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (_InternallCall)
			{
				return;
			}

			try
			{
				_InternallCall = true;
                _Operation = PXDBOperation.Update;
				using (InvtMultScope<TLSMaster> ms = new InvtMultScope<TLSMaster>((TLSMaster)e.Row, (TLSMaster)e.OldRow))
				{
					_Master_RowUpdated(sender, new PXRowUpdatedEventArgs<TLSMaster>(e));
				}
			}
			finally
			{
				_InternallCall = false;
                _Operation = PXDBOperation.Normal;
			}
		}

		protected virtual void Master_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			//LS will be deleted by PXParentAttribute
		}

		public override TLSMaster Insert(TLSMaster item)
		{
			try
			{
				_InternallCall = true;
                _Operation = PXDBOperation.Delete;
				return (TLSMaster)MasterCache.Insert(item);
			}
			finally
			{
				_InternallCall = false;
                _Operation = PXDBOperation.Normal;
			}
		}

		protected virtual void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				PXCache cache = sender.Graph.Caches[typeof(TLSDetail)];

                Counters counters;
                if (DetailCounters.TryGetValue(e.Row, out counters) && counters.UnassignedNumber == 0)
                {
                    return;
                }

				foreach (object detail in SelectDetail(cache, (TLSMaster)e.Row))
				{
					try
					{
						_InternallCall = true;
						Detail_RowPersisting(cache, new PXRowPersistingEventArgs(e.Operation, detail));
					}
					finally
					{
						_InternallCall = false;
					}
					if (string.IsNullOrEmpty(((TLSMaster)e.Row).LotSerialNbr) == false)
					{
						((TLSMaster)e.Row).LotSerialNbr = ((TLSDetail)detail).LotSerialNbr;
						break;
					}
                   
                    if (((TLSDetail)detail).ExpireDate == null)
                    {
                        PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((TLSMaster)e.Row).InventoryID);
                        if (item != null && ((INLotSerClass)item).LotSerTrackExpiration == true && 
                            sender.RaiseExceptionHandling<INComponentTran.inventoryID>(e.Row, ((InventoryItem)item).InventoryCD, new PXSetPropertyException(Messages.OneOrMoreExpDateIsEmpty)))
                        {
                            throw new PXRowPersistingException(typeof(INComponentTran.inventoryID).Name, null, Messages.OneOrMoreExpDateIsEmpty);
                        }
                    }
				}
			}
		}

		protected virtual void Master_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Aborted && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				PXCache cache = sender.Graph.Caches[typeof(TLSDetail)];

				foreach (object detail in SelectDetail(cache, (TLSMaster)e.Row))
				{
					Detail_RowPersisted(cache, new PXRowPersistedEventArgs(detail, e.Operation, e.TranStatus, e.Exception));
					if (string.IsNullOrEmpty(((TLSMaster)e.Row).LotSerialNbr) == false)
					{
						((TLSMaster)e.Row).LotSerialNbr = ((TLSDetail)detail).LotSerialNbr;
						break;
					}
				}
			}
		}

		public virtual void Master_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);

			if (item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
			{
				if (e.NewValue != null)
				{
					try
					{
						decimal BaseQty = INUnitAttribute.ConvertToBase(sender, ((ILSMaster)e.Row).InventoryID, ((ILSMaster)e.Row).UOM, (decimal)e.NewValue, INPrecision.NOROUND);
						if (decimal.Remainder(BaseQty, 1m) > 0m)
						{
							decimal power = (decimal)Math.Pow(10, (double)INSetupDecPl.Qty);
							for (decimal i = Math.Floor(BaseQty); ; i++)
							{
								e.NewValue = INUnitAttribute.ConvertFromBase(sender, ((ILSMaster)e.Row).InventoryID, ((ILSMaster)e.Row).UOM, i, INPrecision.NOROUND);

								if (decimal.Remainder((decimal)e.NewValue * power, 1m) == 0m)
									break;
							}
							sender.RaiseExceptionHandling(_MasterQtyField, e.Row, null, new PXSetPropertyException(Messages.SerialItem_LineQtyUpdated, PXErrorLevel.Warning));
						}
					}
					catch (PXUnitConversionException) { }
				}
			}
		}

		public virtual void Detail_UOM_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((ILSDetail)e.Row).InventoryID);

			if (item != null)
			{
				e.NewValue = ((InventoryItem)item).BaseUnit;
				e.Cancel = true;
				//otherwise default via attribute
			}
		}

		public virtual void Detail_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((ILSDetail)e.Row).InventoryID);

			if (item != null && INLotSerialNbrAttribute.IsTrackSerial(item, ((ILSDetail)e.Row).TranType))
			{
				if (e.NewValue != null && e.NewValue is decimal && (decimal)e.NewValue != 0m && (decimal)e.NewValue != 1m)
				{
					e.NewValue = 1m;
				}
			}
		}

		protected Dictionary<object, string> _persisted = new Dictionary<object, string>();

		protected virtual void ThrowEmptyLotSerNumVal(PXCache sender, object data)
		{
			string _ItemFieldName = null;
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(null))
			{
				if (attr is InventoryAttribute)
				{
					_ItemFieldName = attr.FieldName;
					break;
				}
			}
			//the only reason can be overflow in serial numbering which will cause '0000' number to be treated like not-generated
			throw new PXException(Messages.LSCannotAutoNumberItem, sender.GetValueExt(data, _ItemFieldName));
		}       

		public virtual void Detail_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				if (string.IsNullOrEmpty(((ILSDetail)e.Row).AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(((ILSDetail)e.Row).AssignedNbr, ((ILSDetail)e.Row).LotSerialNbr))
				{
					string newval = string.Empty;
					try
					{
						if (((ILSDetail)e.Row).LotSerClassID == null)
						{
							newval = INLotSerialNbrAttribute.AssignNumber<InventoryItem, InventoryItem.inventoryID>(sender, ((ILSDetail)e.Row).InventoryID.ToString());
						}
						else
						{
							newval = INLotSerialNbrAttribute.AssignNumber<INLotSerClass, INLotSerClass.lotSerClassID>(sender, ((ILSDetail)e.Row).LotSerClassID);
						}
					}
					catch (AutoNumberException)
					{
						ThrowEmptyLotSerNumVal(sender, e.Row);
					}

					string _KeyToAbort = INLotSerialNbrAttribute.UpdateNumber(
						((ILSDetail)e.Row).AssignedNbr,
						((ILSDetail)e.Row).LotSerialNbr,
						newval);

					((ILSDetail)e.Row).LotSerialNbr = _KeyToAbort;

					try
					{
						_persisted.Add(e.Row, _KeyToAbort);
					}
					catch (ArgumentException)
					{
						//the only reason can be overflow in serial numbering which will cause '0000' number to be treated like not-generated
						ThrowEmptyLotSerNumVal(sender, e.Row);
					}

					sender.RaiseRowUpdated(e.Row, PXCache<TLSDetail>.CreateCopy((TLSDetail)e.Row));
				}
			}
		}

		public virtual void Detail_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Aborted)
			{
				string _KeyToAbort = null;

				if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update) &&
					_persisted.TryGetValue(e.Row, out _KeyToAbort))
				{
					((ILSDetail)e.Row).LotSerialNbr = INLotSerialNbrAttribute.MakeNumber(((ILSDetail)e.Row).AssignedNbr, ((ILSDetail)e.Row).LotSerialNbr, sender.Graph.Accessinfo.BusinessDate.GetValueOrDefault());
					_persisted.Remove(e.Row);
				}
				PXOuterException exception = e.Exception as PXOuterException;
				if (exception != null && object.ReferenceEquals(e.Row, exception.Row) && !sender.Graph.UnattendedMode)
				{
					TLSDetail row = (TLSDetail)e.Row;
					TLSMaster master = SelectMaster(sender, row);

					for (int i = 0; i < exception.InnerFields.Length; i++)
					{
						if (!MasterCache.RaiseExceptionHandling(exception.InnerFields[i], master, null, new PXSetPropertyException(exception.InnerMessages[i])))
						{
							exception.InnerRemove(exception.InnerFields[i]);
						}
					}
				}
			}
			else if (e.TranStatus == PXTranStatus.Completed)
			{
				_persisted.Remove(e.Row);
			}

		}
		#endregion

		#region Inner Types
        [Serializable]
        public partial class LotSerOptions : LSSelect.LotSerOptions
        {
        }
		#endregion

	}

	public interface ILotSerNumVal
	{
		String LotSerNumVal
		{
			get;
			set;
		}
	}



	#endregion

	#region LSINTran

	public class LSINTran : LSSelect<INTran, INTranSplit,
		Where<INTranSplit.docType, Equal<Current<INRegister.docType>>,
		And<INTranSplit.refNbr, Equal<Current<INRegister.refNbr>>>>>
	{
		#region Ctor
		public LSINTran(PXGraph graph)
			: base(graph)
		{
			this.MasterQtyField = typeof(INTran.qty);
			graph.FieldDefaulting.AddHandler<INTranSplit.subItemID>(INTranSplit_SubItemID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INTranSplit.locationID>(INTranSplit_LocationID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INTranSplit.invtMult>(INTranSplit_InvtMult_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INTranSplit.lotSerialNbr>(INTranSplit_LotSerialNbr_FieldDefaulting);
			graph.RowUpdated.AddHandler<INRegister>(INRegister_RowUpdated);
		}
		#endregion

		#region Implementation
		protected virtual void INRegister_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<INRegister.hold>(e.Row, e.OldRow) && (bool?)sender.GetValue<INRegister.hold>(e.Row) == false)
			{ 
				PXCache cache = sender.Graph.Caches[typeof(INTran)];

				foreach (INTran item in PXParentAttribute.SelectSiblings(cache, null, typeof(INRegister)))
				{
					if (Math.Abs((decimal)item.BaseQty) >= 0.0000005m && item.UnassignedQty >= 0.0000005m)
					{
						cache.RaiseExceptionHandling<INTran.qty>(item, item.Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned));

						if (cache.GetStatus(item) == PXEntryStatus.Notchanged)
						{
							cache.SetStatus(item, PXEntryStatus.Updated);
						}
					}
				}
			}
		}

		protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				PXCache cache = sender.Graph.Caches[typeof(INRegister)];
				object doc = PXParentAttribute.SelectParent(sender, e.Row, typeof(INRegister)) ?? cache.Current;

				bool? OnHold = (bool?)cache.GetValue<INRegister.hold>(doc);

				if (OnHold == false && Math.Abs((decimal)((INTran)e.Row).BaseQty) >= 0.0000005m && ((INTran)e.Row).UnassignedQty >= 0.0000005m)
				{
					if (sender.RaiseExceptionHandling<INTran.qty>(e.Row, ((INTran)e.Row).Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned)))
					{
						throw new PXRowPersistingException(typeof(INTran.qty).Name, ((INTran)e.Row).Qty, Messages.BinLotSerialNotAssigned); 
					}
				}
			}
			base.Master_RowPersisting(sender, e);
		}

		public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (!PXLongOperation.Exists(sender.Graph.UID))
			{
				IQtyAllocated availability = AvailabilityFetch(sender, (INTran)e.Row, true);

				if (availability != null)
				{
					PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((INTran)e.Row).InventoryID);

					availability.QtyOnHand = INUnitAttribute.ConvertFromBase<INTran.inventoryID, INTran.uOM>(sender, e.Row, (decimal)availability.QtyOnHand, INPrecision.QUANTITY);
					availability.QtyAvail = INUnitAttribute.ConvertFromBase<INTran.inventoryID, INTran.uOM>(sender, e.Row, (decimal)availability.QtyAvail, INPrecision.QUANTITY);
					availability.QtyNotAvail = INUnitAttribute.ConvertFromBase<INTran.inventoryID, INTran.uOM>(sender, e.Row, (decimal)availability.QtyNotAvail, INPrecision.QUANTITY);
					availability.QtyHardAvail = INUnitAttribute.ConvertFromBase<INTran.inventoryID, INTran.uOM>(sender, e.Row, (decimal)availability.QtyHardAvail, INPrecision.QUANTITY);

					e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(Messages.Availability_Info,
						sender.GetValue<INTran.uOM>(e.Row), FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail));
					
					AvailabilityCheck(sender, (INTran)e.Row, availability);
				}
				else
				{
					e.ReturnValue = string.Empty;
				}
			}
			else
			{
				e.ReturnValue = string.Empty;
			}

			base.Availability_FieldSelecting(sender, e);
		}

		public override INTranSplit Convert(INTran item)
		{
			using (InvtMultScope<INTran> ms = new InvtMultScope<INTran>(item))
			{
				INTranSplit ret = item;
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

		public virtual void INTranSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				if (((INTranSplit)e.Row).BaseQty != 0m && ((INTranSplit)e.Row).LocationID == null)
				{
					ThrowFieldIsEmpty<INTranSplit.locationID>(sender, e.Row);
				}
			}
		}

		public virtual void INTranSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INTran)];
			if (cache.Current != null && (e.Row == null || ((INTran)cache.Current).LineNbr == ((INTranSplit)e.Row).LineNbr))
			{
				e.NewValue = ((INTran)cache.Current).SubItemID;
				e.Cancel = true;
			}
		}

		public virtual void INTranSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INTran)];
			if (cache.Current != null && (e.Row == null || ((INTran)cache.Current).LineNbr == ((INTranSplit)e.Row).LineNbr))
			{
				e.NewValue = ((INTran)cache.Current).LocationID;
				e.Cancel = true;
			}
		}

		public virtual void INTranSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INTran)];
			if (cache.Current != null && (e.Row == null || ((INTran)cache.Current).LineNbr == ((INTranSplit)e.Row).LineNbr))
			{
				using (InvtMultScope<INTran> ms = new InvtMultScope<INTran>((INTran)cache.Current))
				{
					e.NewValue = ((INTran)cache.Current).InvtMult;
					e.Cancel = true;
				}
			}
		}

		public virtual void INTranSplit_LotSerialNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((INTranSplit)e.Row).InventoryID);

			if (item != null)
			{
				object InvtMult = ((INTranSplit)e.Row).InvtMult;
				if (InvtMult == null)
				{
					sender.RaiseFieldDefaulting<INTranSplit.invtMult>(e.Row, out InvtMult);
				}

				object TranType = ((INTranSplit)e.Row).TranType;
				if (TranType == null)
				{
					sender.RaiseFieldDefaulting<INTranSplit.tranType>(e.Row, out TranType);
				}

				INLotSerTrack.Mode mode = INLotSerialNbrAttribute.TranTrackMode(item, (string)TranType, (short?)InvtMult);
				if (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Create) > 0)
				{
					foreach (INTranSplit lssplit in INLotSerialNbrAttribute.CreateNumbers<INTranSplit>(sender, item, mode, 1m))
					{
						e.NewValue = lssplit.LotSerialNbr;
						e.Cancel = true;
					}
				}
				//otherwise default via attribute
			}
		}

		public override void Master_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((decimal?)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, PXErrorLevel.Error, (int)0);
			}
		}

		protected override void Master_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (((INTran)e.Row).InvtMult != (short)0)
			{
				base.Master_RowInserted(sender, e);
			}
			else
			{
				sender.SetValue<INTran.lotSerialNbr>(e.Row, null);
				sender.SetValue<INTran.expireDate>(e.Row, null);
			}
		}

		protected override void Master_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (((INTran)e.Row).InvtMult != (short)0)
			{
				base.Master_RowDeleted(sender, e);
			}
		}

		protected override void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (((INTran)e.Row).InvtMult != (short)0)
			{
				if (Equals(((INTran)e.Row).TranType, ((INTran)e.OldRow).TranType) == false)
				{
					sender.SetDefaultExt<INTran.invtMult>(e.Row);

					PXCache cache = sender.Graph.Caches[typeof(INTranSplit)];
					foreach (INTranSplit split in PXParentAttribute.SelectSiblings(cache, (INTranSplit)(INTran)e.Row, typeof(INTran)))
					{
						INTranSplit copy = PXCache<INTranSplit>.CreateCopy(split);

						split.TranType = ((INTran)e.Row).TranType;

						if (cache.GetStatus(split) == PXEntryStatus.Notchanged)
						{
							cache.SetStatus(split, PXEntryStatus.Updated);
						}
						cache.RaiseRowUpdated(split, copy);
					}
				}

				base.Master_RowUpdated(sender, e);
			}
			else
			{
				sender.SetValue<INTran.lotSerialNbr>(e.Row, null);
				sender.SetValue<INTran.expireDate>(e.Row, null);
			}
		}
		public override void AvailabilityCheck(PXCache sender, ILSMaster Row, IQtyAllocated availability)
		{
			base.AvailabilityCheck(sender, Row, availability);
			if (Row.InvtMult == -1 && Row.BaseQty > 0m && availability != null)
			{
				INRegister doc = (INRegister)sender.Graph.Caches[typeof (INRegister)].Current;
				if (availability.QtyOnHand - Row.Qty < 0m && doc != null && doc.Released == false)
				{
					if (availability is LotSerialStatus)
					{
						RaiseQtyRowExceptionHandling(sender, Row, Row.Qty, new PXSetPropertyException(Messages.StatusCheck_QtyLotSerialOnHandNegative));
					}
					else if (availability is LocationStatus)
					{
						RaiseQtyRowExceptionHandling(sender, Row, Row.Qty, new PXSetPropertyException(Messages.StatusCheck_QtyLocationOnHandNegative));
					}
					else if (availability is SiteStatus)
					{
						RaiseQtyRowExceptionHandling(sender, Row, Row.Qty, new PXSetPropertyException(Messages.StatusCheck_QtyOnHandNegative));
					}
				}
			}
		}
		private void RaiseQtyRowExceptionHandling(PXCache sender, object row, object newValue, PXSetPropertyException e)
		{
			if (row is INTran)
			{
				sender.RaiseExceptionHandling<INTran.qty>(row, newValue, 
					e == null ? e : new PXSetPropertyException(e.Message, PXErrorLevel.RowWarning, sender.GetStateExt<INTran.inventoryID>(row), sender.GetStateExt<INTran.subItemID>(row), sender.GetStateExt<INTran.siteID>(row), sender.GetStateExt<INTran.locationID>(row), sender.GetValue<INTran.lotSerialNbr>(row)));
			}
			else
			{
				sender.RaiseExceptionHandling<INTranSplit.qty>(row, newValue,
					e == null ? e : new PXSetPropertyException(e.Message, PXErrorLevel.RowWarning, sender.GetStateExt<INTranSplit.inventoryID>(row), sender.GetStateExt<INTranSplit.subItemID>(row), sender.GetStateExt<INTranSplit.siteID>(row), sender.GetStateExt<INTranSplit.locationID>(row), sender.GetValue<INTranSplit.lotSerialNbr>(row)));
			}
		}
		protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXSetPropertyException e)
		{
			if (row is INTran)
			{
				sender.RaiseExceptionHandling<INTran.qty>(row, null, new PXSetPropertyException(e.Message, PXErrorLevel.Warning, sender.GetStateExt<INTran.inventoryID>(row), sender.GetStateExt<INTran.subItemID>(row), sender.GetStateExt<INTran.siteID>(row), sender.GetStateExt<INTran.locationID>(row), sender.GetValue<INTran.lotSerialNbr>(row)));
			}
			else
			{
				sender.RaiseExceptionHandling<INTranSplit.qty>(row, null, new PXSetPropertyException(e.Message, PXErrorLevel.Warning, sender.GetStateExt<INTranSplit.inventoryID>(row), sender.GetStateExt<INTranSplit.subItemID>(row), sender.GetStateExt<INTranSplit.siteID>(row), sender.GetStateExt<INTranSplit.locationID>(row), sender.GetValue<INTranSplit.lotSerialNbr>(row)));
			}
		}
		#endregion

		public override PXSelectBase<INLotSerialStatus> GetSerialStatusCmd(PXCache sender, INTran Row, PXResult<InventoryItem, INLotSerClass> item)
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
				switch (Row.TranType)
				{
					case INTranType.Issue:
						cmd.WhereAnd<Where<INLocation.receiptsValid, Equal<boolTrue>>>();
						break;
					case INTranType.Transfer:
						cmd.WhereAnd<Where<INLocation.transfersValid, Equal<boolTrue>>>();
						break;
					default:
						cmd.WhereAnd<Where<INLocation.salesValid, Equal<boolTrue>>>();
						break;
				}
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

	#endregion

	#region LSINAdjustmentTran
	public class LSINAdjustmentTran : LSINTran
	{
		public LSINAdjustmentTran(PXGraph graph)
			: base(graph)
		{
			graph.FieldVerifying.AddHandler<INTran.uOM>(INTran_UOM_FieldVerifying);
		}

		public virtual void INTran_UOM_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((INTran)e.Row).InventoryID);
			if (item != null && INLotSerialNbrAttribute.IsTrackSerial(item, ((INTran)e.Row).TranType))
			{
				object newval;

				sender.RaiseFieldDefaulting<INTran.uOM>(e.Row, out newval);

				if (object.Equals(newval, e.NewValue) == false)
				{
					e.NewValue = newval;
					sender.RaiseExceptionHandling<INTran.uOM>(e.Row, null, new PXSetPropertyException(Messages.SerialItemAdjustment_UOMUpdated, PXErrorLevel.Warning, newval));
				}
			}
		}

		public override void Master_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((INTran)e.Row).InventoryID);

			if (item != null && INLotSerialNbrAttribute.IsTrackSerial(item, ((INTran)e.Row).TranType))
			{
				if (e.NewValue != null && e.NewValue is decimal && (decimal)e.NewValue != 0m && (decimal)e.NewValue != 1m && (decimal)e.NewValue != -1m)
				{
					e.NewValue = (decimal)e.NewValue > 0 ? 1m : -1m;
					sender.RaiseExceptionHandling<INTran.qty>(e.Row, null, new PXSetPropertyException(Messages.SerialItemAdjustment_LineQtyUpdated, PXErrorLevel.Warning, ((InventoryItem)item).BaseUnit));
				}
			}
		}

		public override void CreateNumbers(PXCache sender, INTran Row, decimal BaseQty, bool AlwaysAutoNextNbr)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);
			INLotSerClass itemclass = item;

			if(itemclass.LotSerTrack != INLotSerTrack.NotNumbered &&
				 itemclass.LotSerAssign == INLotSerAssign.WhenReceived &&
				 (Row.SubItemID == null || Row.LocationID == null))
				return;

			base.CreateNumbers(sender, Row, BaseQty, AlwaysAutoNextNbr);
		}
		public override void IssueNumbers(PXCache sender, INTran Row, decimal BaseQty)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);
			INLotSerClass itemclass = item;

			if (itemclass.LotSerTrack != INLotSerTrack.NotNumbered &&
				 itemclass.LotSerAssign == INLotSerAssign.WhenReceived &&
				 (Row.LotSerialNbr == null || Row.SubItemID == null || Row.LocationID == null))
				return;

			base.IssueNumbers(sender, Row, BaseQty);
		}
	}

	#endregion

	#region INExpireDateAttribute

	[PXDBDate(InputMask = "d", DisplayMask = "d")]
	[PXUIField(DisplayName = "Expiration Date")]
	[PXDefault()]
	public class INExpireDateAttribute : AcctSubAttribute, IPXFieldSelectingSubscriber, IPXRowSelectedSubscriber, IPXFieldDefaultingSubscriber, IPXRowPersistingSubscriber
	{
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (!typeof(ILSMaster).IsAssignableFrom(sender.GetItemType()))
			{
				throw new PXArgumentException();
			}
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXFieldDefaultingSubscriber) || typeof(ISubscriber) == typeof(IPXRowPersistingSubscriber))
			{
				subscribers.Add(this as ISubscriber);
			}
			else
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}

		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			sender.GetAttributes(e.Row, _FieldName);
		}

		protected virtual PXResult<InventoryItem, INLotSerClass> ReadInventoryItem(PXCache sender, int? InventoryID)
		{
			return (PXResult<InventoryItem, INLotSerClass>)PXSelectJoin<InventoryItem, InnerJoin<INLotSerClass, On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>>, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(sender.Graph, InventoryID);
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);

				((PXUIFieldAttribute) _Attributes[_UIAttrIndex]).Enabled = 
					item != null && sender.AllowUpdate &&
					INLotSerialNbrAttribute.IsTrackExpiration(item, ((ILSMaster)e.Row).TranType);				
			}
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
			if (item == null || ((ILSMaster)e.Row).SubItemID == null || ((ILSMaster)e.Row).LocationID == null) return;

			if (INLotSerialNbrAttribute.IsTrackExpiration(item, ((ILSMaster)e.Row).TranType) &&
					((ILSMaster)e.Row).BaseQty != 0m)
			{							
				((IPXRowPersistingSubscriber)_Attributes[_DefAttrIndex]).RowPersisting(sender, e);
			}
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			((IPXFieldDefaultingSubscriber)_Attributes[_DefAttrIndex]).FieldDefaulting(sender, e);
		}
	}

	#endregion

	#region PXEmptyAutoIncValueException

	public class PXEmptyAutoIncValueException : PXException
	{
		public PXEmptyAutoIncValueException(string Source)
			: base(Messages.EmptyAutoIncValue, Source)
		{
		}

		public PXEmptyAutoIncValueException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			PXReflectionSerializer.RestoreObjectProps(this, info);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			PXReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}

	}

	#endregion

	#region INLotSerialNbrAttribute

	[PXDBString(100, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Lot/Serial Nbr.")]
	[PXDefault("")]
	public class INLotSerialNbrAttribute : AcctSubAttribute, IPXFieldVerifyingSubscriber, IPXFieldDefaultingSubscriber, IPXRowPersistingSubscriber, IPXFieldSelectingSubscriber, IPXRowSelectedSubscriber
	{
		private const string _NumFormatStr = "{0}";

		protected Type _InventoryType;
		protected Type _SubItemType;
		protected Type _LocationType;

		public INLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType)
			: base()
		{
			if (!typeof(ILSMaster).IsAssignableFrom(BqlCommand.GetItemType(InventoryType)))
			{
				throw new PXArgumentException();
			}

			_InventoryType = InventoryType;
			_SubItemType = SubItemType;
			_LocationType = LocationType;

			Type SearchType = BqlCommand.Compose(
				typeof(Search<,>),
				typeof(INLotSerialStatus.lotSerialNbr),
				typeof(Where<,,>),
				typeof(INLotSerialStatus.inventoryID),
				typeof(Equal<>),
				typeof(Optional<>),
				InventoryType,
				typeof(And<,,>),
				typeof(INLotSerialStatus.subItemID),
				typeof(Equal<>),
				typeof(Optional<>),
				SubItemType,
				typeof(And<,,>),
				typeof(INLotSerialStatus.locationID),
				typeof(Equal<>),
				typeof(Optional<>),
				LocationType,
				typeof(And<,>),
				typeof(INLotSerialStatus.qtyOnHand),
				typeof(Greater<>),
				typeof(decimal0)
				);

			{
				PXSelectorAttribute attr = new PXSelectorAttribute(SearchType,
																	 typeof(INLotSerialStatus.lotSerialNbr),
																	 typeof(INLotSerialStatus.siteID),
																	 typeof(INLotSerialStatus.locationID),
																	 typeof(INLotSerialStatus.qtyOnHand),
																	 typeof(INLotSerialStatus.qtyAvail),
																	 typeof(INLotSerialStatus.expireDate));
				_Attributes.Add(attr);
				_SelAttrIndex = _Attributes.Count - 1;
			}
		}

		public INLotSerialNbrAttribute(Type InventoryType, Type SubItemType, Type LocationType, Type ParentLotSerialNbrType)
			: this(InventoryType, SubItemType, LocationType)
		{
			_Attributes[_DefAttrIndex] = new PXDefaultAttribute(ParentLotSerialNbrType) { PersistingCheck = PXPersistingCheck.NullOrBlank };
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber) || typeof(ISubscriber) == typeof(IPXFieldDefaultingSubscriber) || typeof(ISubscriber) == typeof(IPXRowPersistingSubscriber))
			{
				subscribers.Add(this as ISubscriber);
			}
			else if (typeof(ISubscriber) == typeof(IPXFieldSelectingSubscriber))
			{
				base.GetSubscriber<ISubscriber>(subscribers);

				subscribers.Remove(this as ISubscriber);
				subscribers.Add(this as ISubscriber);
				subscribers.Reverse();
			}
			else
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
			if (item == null || ((ILSMaster)e.Row).SubItemID == null || ((ILSMaster)e.Row).LocationID == null)
			{
				return;
			}

			if (((INLotSerClass)item).LotSerTrack != "N" && ((ILSMaster)e.Row).InvtMult == (short)-1 && ((INLotSerClass)item).LotSerAssign == "R" && string.IsNullOrEmpty((string)e.NewValue) == false)
			{
				((IPXFieldVerifyingSubscriber)_Attributes[_SelAttrIndex]).FieldVerifying(sender, e);
			}
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null)
			{
				PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
				if (item == null)
				{
					return;
				}

				if (((INLotSerClass)item).LotSerTrack != INLotSerTrack.SerialNumbered)
				{
					((IPXFieldDefaultingSubscriber)_Attributes[_DefAttrIndex]).FieldDefaulting(sender, e);
				}
			}
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
			if (item == null || ((ILSMaster)e.Row).SubItemID == null || ((ILSMaster)e.Row).LocationID == null)
			{
				return;
			}

			if (INLotSerialNbrAttribute.IsTrack(item, ((ILSMaster)e.Row).TranType))
			{
				((IPXRowPersistingSubscriber)_Attributes[_DefAttrIndex]).RowPersisting(sender, e);
			}
		}

		protected virtual PXResult<InventoryItem, INLotSerClass> ReadInventoryItem(PXCache sender, int? InventoryID)
		{
			return (PXResult<InventoryItem, INLotSerClass>)PXSelectJoin<InventoryItem, InnerJoin<INLotSerClass, On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>>, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(sender.Graph, InventoryID);
		}

		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			sender.GetAttributes(e.Row, _FieldName);
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				PXResult<InventoryItem, INLotSerClass> item = this.ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
				((PXUIFieldAttribute) _Attributes[_UIAttrIndex]).Enabled =
					item != null && sender.AllowUpdate &&
					INLotSerialNbrAttribute.IsTrack(item, ((ILSMaster) e.Row).TranType);
			}
		}

		public static string MakeFormatStr(PXCache sender, INLotSerClass lsclass)
		{
			StringBuilder format = new StringBuilder();

			if (lsclass != null)
			{
				foreach (INLotSerSegment seg in PXSelect<INLotSerSegment, Where<INLotSerSegment.lotSerClassID, Equal<Required<INLotSerSegment.lotSerClassID>>>, OrderBy<Asc<INLotSerSegment.lotSerClassID, Asc<INLotSerSegment.segmentID>>>>.Select(sender.Graph, lsclass.LotSerClassID))
				{
					switch (seg.SegmentType)
					{
						case INLotSerSegmentType.FixedConst:
							format.Append(seg.SegmentValue);
							break;
						case INLotSerSegmentType.NumericVal:
							format.Append("{1}");
							break;
						case INLotSerSegmentType.DateConst:
							format.Append("{0");
							if (!string.IsNullOrEmpty(seg.SegmentValue))
								format.Append(":").Append(seg.SegmentValue);
							format.Append("}");
							break;
						case INLotSerSegmentType.DayConst:
							format.Append("{0:dd}");
							break;
						case INLotSerSegmentType.MonthConst:
							format.Append("{0:MM}");
							break;
						case INLotSerSegmentType.MonthLongConst:
							format.Append("{0:MMM}");
							break;
						case INLotSerSegmentType.YearConst:
							format.Append("{0:yy}");
							break;
						case INLotSerSegmentType.YearLongConst:
							format.Append("{0:yyyy}");
							break;
						default:
							throw new PXException();
					}
				}
			}
			return format.ToString();
		}

		public static string AssignNumber<LSTable, LSKey>(PXCache sender, string LSKeyValue)
			where LSTable : class, IBqlTable
			where LSKey : class, IBqlField
		{
			string LSNumVal = "";
			string LSNewVal = "";

			using (PXDataRecord record = PXDatabase.SelectSingle<LSTable>(
				new PXDataField("LotSerNumVal"),
				new PXDataFieldValue(typeof(LSKey).Name, PXDbType.VarChar, 255, LSKeyValue)
				))
			{
				if (record == null)
				{
					throw new AutoNumberException();
				}
				LSNumVal = record.GetString(0);
			}

			try
			{
				LSNewVal = AutoNumberAttribute.NextNumber(LSNumVal);
			}
			catch (NullReferenceException)
			{
				throw new AutoNumberException();
			}

			PXDatabase.Update<LSTable>(
				new PXDataFieldAssign("LotSerNumVal", LSNewVal),
				new PXDataFieldRestrict(typeof(LSKey).Name, LSKeyValue),
				new PXDataFieldRestrict("LotSerNumVal", LSNumVal));

			return LSNewVal;
		}

		public static int GetNumberLength(PXCache sender, PXResult<InventoryItem, INLotSerClass> item)
			{
				return ((INLotSerClass) item).LotSerNumShared == true
					? (string.IsNullOrEmpty(((INLotSerClass)item).LotSerNumVal) ? 6 : ((INLotSerClass)item).LotSerNumVal.Length)
					: (string.IsNullOrEmpty(((InventoryItem)item).LotSerNumVal) ? 6 : ((InventoryItem)item).LotSerNumVal.Length);
		}

		public static string GetNextNumber(PXCache sender, PXResult<InventoryItem, INLotSerClass> item)
		{
			string numval = new string('0', GetNumberLength(sender, item));
			return string.Format(((INLotSerClass)item).LotSerFormatStr, sender.Graph.Accessinfo.BusinessDate, numval).ToUpper();
		}

		public static string GetNextFormat(PXCache sender, PXResult<InventoryItem, INLotSerClass> item)
		{
			string numFormat = "{1:" + new string('0', GetNumberLength(sender, item)) + "}";
			return ((INLotSerClass)item).LotSerFormatStr.Replace("{1}", numFormat);
		}

		public static string GetNextClassID(PXResult<InventoryItem, INLotSerClass> item)
		{
			return (bool)((INLotSerClass)item).LotSerNumShared
							?
								((INLotSerClass)item).LotSerClassID
							:
								null;
		}

		protected class PXUnknownSegmentTypeException: PXException
		{
			public PXUnknownSegmentTypeException():base(Messages.UnknownSegmentType){}

			public PXUnknownSegmentTypeException(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		public static string GetDisplayMask(PXCache sender, PXResult<InventoryItem, INLotSerClass> item)
		{
			if ((INLotSerClass)item == null) return null;
			StringBuilder mask = new StringBuilder();
			foreach (INLotSerSegment seg in PXSelect<INLotSerSegment, Where<INLotSerSegment.lotSerClassID, Equal<Current<INLotSerClass.lotSerClassID>>>, OrderBy<Asc<INLotSerSegment.lotSerClassID, Asc<INLotSerSegment.segmentID>>>>.SelectMultiBound(sender.Graph, new object[]{(INLotSerClass)item}))
			{
				switch (seg.SegmentType)
				{
					case INLotSerSegmentType.FixedConst:
						mask.Append(!string.IsNullOrEmpty(seg.SegmentValue) ? new string('C', seg.SegmentValue.Length) : string.Empty);
						break;
					case INLotSerSegmentType.NumericVal:
						mask.Append( new string('9', GetNumberLength(sender, item)));
						break;
					case INLotSerSegmentType.DateConst:
						mask.Append(!string.IsNullOrEmpty(seg.SegmentValue)
						            	? new string('C', seg.SegmentValue.Length)
						            	: new string('C', string.Format("{0}", sender.Graph.Accessinfo.BusinessDate).Length));
						break;
					case INLotSerSegmentType.DayConst:
					case INLotSerSegmentType.MonthConst:
					case INLotSerSegmentType.YearConst:
						mask.Append(new string('9', 2));
						break;
					case INLotSerSegmentType.MonthLongConst:
						mask.Append(new string('C', 3));
						break;
					case INLotSerSegmentType.YearLongConst:
						mask.Append(new string('9', 4));
						break;
					default:
						throw new PXUnknownSegmentTypeException();
				}
			} 
			return mask.ToString();
		}

		public class LSParts
		{
			public LSParts(int flen, int nlen, int llen)
			{
				_flen = flen;
				_nlen = nlen;
				_llen = llen;
			}

			private readonly int _flen;
			private readonly int _nlen;
			private readonly int _llen;

			public int flen
			{
				get { return _flen; }
			}

			public int nlen
			{
				get { return _nlen; }
			}
			
			public int llen
			{
				get { return _llen; }
			}
			
			public int len
			{
				get { return _flen + _nlen + _llen; }
			}

			public int nidx
			{
				get { return _flen; }
			}

			public int lidx
			{
				get { return _flen + _nlen; }
			}
		}

		public static LSParts GetLSParts(PXCache sender, PXResult<InventoryItem, INLotSerClass> item)
		{
			if ((INLotSerClass)item == null) return null;
			int flen = 0, nlen = 0, llen = 0;
			foreach (INLotSerSegment seg in PXSelect<INLotSerSegment, Where<INLotSerSegment.lotSerClassID, Equal<Current<INLotSerClass.lotSerClassID>>>, OrderBy<Asc<INLotSerSegment.lotSerClassID, Asc<INLotSerSegment.segmentID>>>>.SelectMultiBound(sender.Graph, new object[] { (INLotSerClass)item }))
			{
				int tmp = 0;
				switch (seg.SegmentType)
				{
					case INLotSerSegmentType.FixedConst:
						tmp = seg.SegmentValue.Length;
						break;
					case INLotSerSegmentType.NumericVal:
						nlen = GetNumberLength(sender, item);
						break;
					case INLotSerSegmentType.DateConst:
						tmp = !string.IsNullOrEmpty(seg.SegmentValue)
										? seg.SegmentValue.Length
										: string.Format("{0}", sender.Graph.Accessinfo.BusinessDate).Length;
						break;
					case INLotSerSegmentType.DayConst:
					case INLotSerSegmentType.MonthConst:
					case INLotSerSegmentType.YearConst:
						tmp = 2;
						break;
					case INLotSerSegmentType.MonthLongConst:
						tmp = 3;
						break;
					case INLotSerSegmentType.YearLongConst:
						tmp = 4;
						break;
					default:
						throw new PXUnknownSegmentTypeException();
				}
				if (nlen == 0)
					flen += tmp;
				else
					llen += tmp;
			}
			return new LSParts(flen, nlen, llen);
		}

		public static string GetNextNumber(PXCache sender, int inventoryID)
		{
			var item = (PXResult<InventoryItem, INLotSerClass>)
			PXSelectJoin<InventoryItem, 
			LeftJoin<INLotSerClass, On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>>, 
			Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>, 
				And<Where<InventoryItem.stkItem, Equal<boolFalse>, Or<INLotSerClass.lotSerClassID, IsNotNull>>>>>.SelectWindowed(sender.Graph, 0,1, inventoryID);
			
			if (item == null || ((InventoryItem)item).InventoryID == null || ((INLotSerClass)item).LotSerTrack == INLotSerTrack.NotNumbered) return null;
			
			string number = GetNextNumber(sender, item);
			string format = GetNextFormat(sender, item);

			string newval = GetNextClassID(item) != null
			                	? AssignNumber<INLotSerClass, INLotSerClass.lotSerClassID>(sender,
			                	                                                           ((InventoryItem) item).LotSerClassID)
			                	: AssignNumber<InventoryItem, InventoryItem.inventoryID>(sender,
			                	                                                         ((InventoryItem) item).InventoryID.
			                	                                                         	ToString());

			return UpdateNumber(format, number, newval);
		}

		public static TLSDetail GetNextSplit<TLSDetail>(PXCache sender, PXResult<InventoryItem, INLotSerClass> item)
			where TLSDetail : class, ILSDetail, new()
		{
			TLSDetail det = new TLSDetail();
			det.LotSerialNbr = GetNextNumber(sender, item);
			det.AssignedNbr = GetNextFormat(sender, item);
			det.LotSerClassID = GetNextClassID(item);
			return det;
		}

		public static string MakeNumber(string FormatStr, string NumberStr, DateTime date)
		{
			if (FormatStr.Contains(_NumFormatStr))
			{
				string numval = new string('0', NumberStr.Length - FormatStr.Length + _NumFormatStr.Length);
				return string.Format(FormatStr, date, numval).ToUpper();
			}
			else
				return string.Format(FormatStr, date, 0).ToUpper();
		}

		public static bool StringsEqual(string FormatStr, string NumberStr)
		{
			int numIndex = 0;
			for (int i = 0; i < FormatStr.Length; i++)
			{
				if (FormatStr[i] == '{' && i + 5 < FormatStr.Length && FormatStr[i + 2] == ':')
				{
					int lenIndex = FormatStr.IndexOf("}", i + 3);
					if (lenIndex != -1)
					{
						int lenght = lenIndex - i - 3;
						if (FormatStr[i + 1] == '1')
						{
							if (NumberStr.Length < numIndex + lenght)
								return false;

							for (int n = 0; n < lenght; n++)
								if (NumberStr[numIndex + n] != '0')
									return false;
						}
						numIndex += lenght;
						i = lenIndex;
						continue;
					}
				}

                if (NumberStr == null || NumberStr.Length <= numIndex) return false;
				if (char.ToUpper(FormatStr[i]) != char.ToUpper(NumberStr[numIndex++])) return false;
			}
			return true;
		}

		public static string UpdateNumber(string FormatStr, string NumberStr, string number)
		{
			int numIndex = 0;
			StringBuilder result = new StringBuilder();
			for (int i = 0; i < FormatStr.Length; i++)
			{
				if (FormatStr[i] == '{' && i + 5 < FormatStr.Length && FormatStr[i + 2] == ':')
				{
					int lenIndex = FormatStr.IndexOf("}", i + 3);
					if (lenIndex != -1)
					{
						int lenght = lenIndex - i - 3;
						if (FormatStr[i + 1] == '1')
							result.Append(number);
						else
							result.Append(NumberStr.Substring(numIndex, lenght));
						numIndex += lenght;
						i = lenIndex;
						continue;
					}
				}
				if (NumberStr.Length <= numIndex) break;
				result.Append(NumberStr[numIndex++]);
			}
			return result.ToString().ToUpper();
		}

		public static List<TLSDetail> CreateNumbers<TLSDetail>(PXCache sender, PXResult<InventoryItem, INLotSerClass> item, INLotSerTrack.Mode mode, decimal count)
			where TLSDetail : class, ILSDetail, new()
		{
			return CreateNumbers<TLSDetail>(sender, item, mode, false, count);
		}

		public static List<TLSDetail> CreateNumbers<TLSDetail>(PXCache sender, PXResult<InventoryItem, INLotSerClass> item, INLotSerTrack.Mode mode, bool ForceAutoNextNbr, decimal count)
			where TLSDetail : class, ILSDetail, new()
		{
			List<TLSDetail> ret = new List<TLSDetail>();

			if (item != null)
			{
				string LotSerTrack = (mode & INLotSerTrack.Mode.Create) > 0 ? ((INLotSerClass)item).LotSerTrack : INLotSerTrack.NotNumbered;
				bool AutoNextNbr = (mode & INLotSerTrack.Mode.Manual) == 0 && (((INLotSerClass)item).AutoNextNbr == true || ForceAutoNextNbr);

				switch (LotSerTrack)
				{
					case "N":
						TLSDetail detail = new TLSDetail();
						detail.AssignedNbr = string.Empty;
						detail.LotSerialNbr = string.Empty;
						detail.LotSerClassID = string.Empty;

						ret.Add(detail);
						break;
					case "L":
						if (AutoNextNbr)
						{
							ret.Add(GetNextSplit<TLSDetail>(sender, item));
						}
						break;
					case "S":
						if (AutoNextNbr)
						{
							for (int i = 0; i < (int)count; i++)
							{
								ret.Add(GetNextSplit<TLSDetail>(sender, item));
							}
						}
						break;
				}
			}
			return ret;
		}

		public static INLotSerTrack.Mode TranTrackMode(INLotSerClass lotSerClass, string tranType, int? invMult)
		{
			if (lotSerClass == null || lotSerClass.LotSerTrack == null || lotSerClass.LotSerTrack == INLotSerTrack.NotNumbered) return INLotSerTrack.Mode.None;

			switch (tranType)
			{
				case INTranType.Invoice:
				case INTranType.DebitMemo:
				case INTranType.Issue:
					return lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.Create : INLotSerTrack.Mode.Issue;

				case INTranType.Transfer:
				case INTranType.Assembly:
					return
						lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.None
							: invMult ==  1 ? INLotSerTrack.Mode.Create
							: invMult == -1 ? INLotSerTrack.Mode.Issue
							: INLotSerTrack.Mode.Manual;

				case INTranType.Adjustment:
				case INTranType.StandardCostAdjustment:
				case INTranType.NegativeCostAdjustment:
				case INTranType.ReceiptCostAdjustment:
					return lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.None
						: invMult ==  1 ? INLotSerTrack.Mode.Create | INLotSerTrack.Mode.Manual
						: invMult == -1 ? INLotSerTrack.Mode.Issue | INLotSerTrack.Mode.Manual
						: INLotSerTrack.Mode.Manual;;					

				case INTranType.Receipt:
					return lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.None : INLotSerTrack.Mode.Create;

				case INTranType.Return:
					return lotSerClass.LotSerAssign == INLotSerAssign.WhenUsed ? INLotSerTrack.Mode.None : INLotSerTrack.Mode.Create | INLotSerTrack.Mode.Manual;

				case INTranType.CreditMemo:
					return INLotSerTrack.Mode.Create | INLotSerTrack.Mode.Manual;

				default:
					return INLotSerTrack.Mode.None;
			}
		}
		public static bool IsTrack(INLotSerClass lotSerClass, string tranType)
		{
			return TranTrackMode(lotSerClass, tranType, null) != INLotSerTrack.Mode.None;
		}
		public static bool IsTrackExpiration(INLotSerClass lotSerClass, string tranType)
		{
			return lotSerClass.LotSerTrackExpiration == true && IsTrack(lotSerClass, tranType);
		}
		public static bool IsTrackSerial(INLotSerClass lotSerClass, string tranType)
		{
			return lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered && IsTrack(lotSerClass, tranType);		
		}
	}

	#endregion

	#region INUnitAttribute

	[PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa")]
	[PXUIField(DisplayName = "UOM", Visibility = PXUIVisibility.Visible)]
	public sealed class INUnitAttribute : AcctSub2Attribute, IPXFieldVerifyingSubscriber, IPXRowSelectedSubscriber, IPXRowPersistingSubscriber
	{
		private Type _InventoryType = null;
		private Type _BaseUnitType = null;

		private string _AccountIDField = null;
		private string _AccountRequireUnitsField = null;

		public INUnitAttribute()
			: base()
		{
			PXSelectorAttribute attr = new PXSelectorAttribute(typeof(Search4<INUnit.fromUnit, Where<INUnit.unitType, Equal<INUnitType.global>>, Aggregate<GroupBy<INUnit.fromUnit>>>));
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public INUnitAttribute(Type InventoryType, Type BaseUnitType)
			: base()
		{
			_BaseUnitType = BaseUnitType;
			Type searchType = BqlCommand.Compose(
				typeof(Search4<,,>),
				typeof(INUnit.fromUnit),
				typeof(Where<,,>),
				typeof(INUnit.unitType),
				typeof(Equal<INUnitType.global>),
				typeof(And<,>),
				typeof(INUnit.toUnit),
				typeof(Equal<>),
				typeof(Optional<>),
				BaseUnitType,
				typeof(Aggregate<GroupBy<INUnit.fromUnit>>));

			PXSelectorAttribute attr = new PXSelectorAttribute(searchType);
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public INUnitAttribute(Type InventoryType, Type AccountIDType, Type AccountRequireUnitsType)
			: this()
		{
			_InventoryType = InventoryType;
			_AccountIDField = AccountIDType.Name;
			_AccountRequireUnitsField = AccountRequireUnitsType.Name;
		}

		//it is assumed that only conversions to BASE item unit exist.
		public INUnitAttribute(Type InventoryType)
			: base()
		{
			_InventoryType = InventoryType;
			Type searchType = BqlCommand.Compose(
				typeof(Search4<,,>),
				typeof(INUnit.fromUnit),
				typeof(Where<,,>),
				typeof(INUnit.unitType),
				typeof(Equal<INUnitType.inventoryItem>),
				typeof(And<,,>),
				typeof(INUnit.inventoryID),
				typeof(Equal<>),
				typeof(Optional<>),
				InventoryType,
				typeof(Or<,,>),
				typeof(INUnit.unitType),
				typeof(Equal<INUnitType.global>),
				typeof(And<,>),
				typeof(Optional<>),
				InventoryType,
				typeof(IsNull),
				typeof(Aggregate<GroupBy<INUnit.fromUnit>>));

			PXSelectorAttribute attr = new PXSelectorAttribute(searchType);
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

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


		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (string.IsNullOrEmpty(_AccountIDField) || string.IsNullOrEmpty(_AccountRequireUnitsField))
			{
				return;
			}

			object AccountID = sender.GetValue(e.Row, _AccountIDField);
			object AccountRequireUnits = sender.GetValue(e.Row, _AccountRequireUnitsField);

			if (AccountRequireUnits == null && e.Row != null)
			{
				Account account = (Account)PXSelectReadonly<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(sender.Graph, AccountID);

				if (account != null)
				{
					sender.SetValue(e.Row, _AccountRequireUnitsField, account.RequireUnits);
				}
				else
				{
					sender.SetValue(e.Row, _AccountRequireUnitsField, null);
				}
			}
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (string.IsNullOrEmpty(_AccountRequireUnitsField))
			{
				return;
			}

			object AccountRequireUnits = sender.GetValue(e.Row, _AccountRequireUnitsField);
			string FieldValue = (string)sender.GetValue(e.Row, _FieldOrdinal);

			if (AccountRequireUnits != null && (bool)AccountRequireUnits && string.IsNullOrEmpty(FieldValue))
			{
				throw new PXRowPersistingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
			}
		}

		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_FieldName != "FromUnit" && _FieldName != "ToUnit")
			{
				if (_InventoryType == null || sender.GetValue(e.Row, _InventoryType.Name) == null || string.IsNullOrEmpty(_AccountRequireUnitsField))
				{
					((IPXFieldVerifyingSubscriber)_Attributes[_SelAttrIndex]).FieldVerifying(sender, e);
				}
			}
		}
		public static decimal ConvertFromBase<InventoryIDField, UOMField>(PXCache sender, object Row, decimal value, INPrecision prec)
			where InventoryIDField : IBqlField
			where UOMField : IBqlField
		{
			string ToUnit = (string)sender.GetValue<UOMField>(Row);
			try
			{
				return ConvertFromBase<InventoryIDField>(sender, Row, ToUnit, value, prec);
			}
			catch (PXUnitConversionException ex)
			{
				sender.RaiseExceptionHandling<UOMField>(Row, ToUnit, ex);
			}
			return 0m;
		}
		public static decimal ConvertFromBase<InventoryIDField>(PXCache sender, object Row, string ToUnit, decimal value, INPrecision prec)
			where InventoryIDField : IBqlField
		{
			return Convert<InventoryIDField>(sender, Row, ToUnit, value, prec, true);
		}
		public static decimal ConvertToBase<InventoryIDField, UOMField>(PXCache sender, object Row, decimal value, INPrecision prec)
			where InventoryIDField : IBqlField
			where UOMField : IBqlField
		{
			string FromUnit = (string)sender.GetValue<UOMField>(Row);
			try
			{
				return ConvertToBase<InventoryIDField>(sender, Row, FromUnit, value, prec);
			}
			catch (PXUnitConversionException ex)
			{
				sender.RaiseExceptionHandling<UOMField>(Row, FromUnit, ex);
			}
			return 0m;
		}
		public static decimal ConvertToBase<InventoryIDField>(PXCache sender, object Row, string FromUnit, decimal value, INPrecision prec)
			where InventoryIDField : IBqlField
		{
			return Convert<InventoryIDField>(sender, Row, FromUnit, value, prec, false);
		}
		public static decimal ConvertFromTo<InventoryIDField>(PXCache sender, object Row, string FromUnit, string ToUnit, decimal value, INPrecision prec)
			where InventoryIDField : IBqlField
		{
			if (string.Equals(FromUnit, ToUnit))
			{
				return value;
			}
			decimal baseValue = ConvertToBase<InventoryIDField>(sender, Row, FromUnit, value, prec);
			return ConvertFromBase<InventoryIDField>(sender, Row, ToUnit, baseValue, prec);
		}
		private static decimal Convert<InventoryIDField>(PXCache sender, object Row, string FromUnit, decimal value, INPrecision prec, bool ViceVersa)
			where InventoryIDField : IBqlField
		{
			object val = sender.GetValueExt<InventoryIDField>(Row);
			object InventoryID = null;

			if (val is PXFieldState)
			{
				if (((PXFieldState)val).Value is string)
				{
					InventoryID = ((PXFieldState)val).Value;
				}
			}
			else
			{
				InventoryID = val;
			}

			int Precision = 6;

			switch (prec)
			{
				case INPrecision.QUANTITY:
					Precision = INSetupDecPl.Qty;
					break;
				case INPrecision.UNITCOST:
					Precision = INSetupDecPl.PrcCst;
					break;
			}

			INUnit unit = (INUnit)PXSelectReadonly2<INUnit, InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INUnit.inventoryID>, And<InventoryItem.baseUnit, Equal<INUnit.toUnit>>>>, Where<INUnit.unitType, Equal<INUnitType.inventoryItem>, And<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>, And<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>>>>>.Select(sender.Graph, InventoryID, FromUnit);

			if (unit == null && (InventoryID == null || FromUnit == null))
			{
				return value;
			}
			else if (unit != null)
			{
				if (unit.UnitMultDiv == "M" && !ViceVersa || unit.UnitMultDiv == "D" && ViceVersa)
				{
					value *= (decimal)unit.UnitRate;
				}
				else
				{
					value /= (decimal)unit.UnitRate;
				} 
				
				if (prec != INPrecision.NOROUND)
				{
					return (decimal)Math.Round(value, Precision, MidpointRounding.AwayFromZero);
				}
				return value;
			}
			else
			{
				throw new PXUnitConversionException();
			}
		}

		public static decimal ConvertFromBase(PXCache sender, Int32? InventoryID, string ToUnit, decimal value, INPrecision prec)
		{
			return Convert(sender, InventoryID, ToUnit, value, prec, true);
		}
		public static decimal ConvertToBase(PXCache sender, Int32? InventoryID, string FromUnit, decimal value, INPrecision prec)
		{
			return Convert(sender, InventoryID, FromUnit, value, prec, false);
		}

		private static decimal Convert(PXCache sender, Int32? InventoryID, string FromUnit, decimal value, INPrecision prec, bool ViceVersa)
		{
			INUnit unit;

			if (typeof(InventoryItem).IsAssignableFrom(sender.GetItemType()))
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(sender.Graph, InventoryID);
				unit = PXSelect<INUnit, Where<INUnit.inventoryID, Equal<Required<INUnit.inventoryID>>, And<INUnit.toUnit, Equal<Required<INUnit.toUnit>>, And<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>>>>>.Select(sender.Graph, InventoryID, item.BaseUnit, FromUnit);
			}
			else
			{
				unit = (INUnit)PXSelectReadonly2<INUnit, InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INUnit.inventoryID>, And<InventoryItem.baseUnit, Equal<INUnit.toUnit>>>>, Where<INUnit.unitType, Equal<INUnitType.inventoryItem>, And<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>, And<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>>>>>.Select(sender.Graph, InventoryID, FromUnit);
			}

			int Precision = 6;

			switch (prec)
			{
				case INPrecision.QUANTITY:
					Precision = INSetupDecPl.Qty;
					break;
				case INPrecision.UNITCOST:
					Precision = INSetupDecPl.PrcCst;
					break;
			}
			if (unit == null && InventoryID == null && FromUnit == null)
			{
				return value;
			}
			else if (unit != null)
			{
				if (unit.UnitMultDiv == "M" && !ViceVersa || unit.UnitMultDiv == "D" && ViceVersa)
				{
					value *= (decimal)unit.UnitRate ;
				}
				else
				{
					value /=  (decimal)unit.UnitRate;
				}
				
				if (prec != INPrecision.NOROUND)
				{
					return (decimal)Math.Round(value, Precision, MidpointRounding.AwayFromZero);
				}
				return value;
			}
			else
			{
				throw new PXUnitConversionException();
			}
		}

		private static decimal Convert(PXCache sender, INUnit unit, decimal value, INPrecision prec, bool ViceVersa)
		{
			int Precision = 6;

			switch (prec)
			{
				case INPrecision.QUANTITY:
					Precision = INSetupDecPl.Qty;
					break;
				case INPrecision.UNITCOST:
					Precision = INSetupDecPl.PrcCst;
					break;
			}

			if (unit == null)
			{
				return value;
			}
			else if (unit != null)
			{
				if (unit.UnitMultDiv == "M" && !ViceVersa || unit.UnitMultDiv == "D" && ViceVersa)
				{
					value *= (decimal)unit.UnitRate;
				}
				else
				{
					value /= (decimal)unit.UnitRate;
				} 
				
				if (prec != INPrecision.NOROUND)
				{
					return (decimal)Math.Round(value, Precision, MidpointRounding.AwayFromZero);
				}
				return value;
			}
			else
			{
				throw new PXUnitConversionException();
			}
		}

		public static decimal ConvertFromBase(PXCache sender, INUnit unit, decimal value, INPrecision prec)
		{
			return Convert(sender, unit, value, prec, true);
		}

		public static decimal ConvertToBase(PXCache sender, INUnit unit, decimal value, INPrecision prec)
		{
			return Convert(sender, unit, value, prec, false);
		}

		public static bool IsFractional(INUnit conv)
		{
			return conv != null && (conv.UnitMultDiv == "D" && conv.UnitRate != 1m || decimal.Remainder((decimal)conv.UnitRate, 1m) != 0m);
		}

		/// <summary>
		/// Converts units using Global converion Table.
		/// </summary>
		/// <exception cref="PXException">Is thrown if converion is not found in the table.</exception>
		public static decimal ConvertGlobalUnits(PXGraph graph, string from, string to, decimal value, INPrecision prec)
		{
			decimal result = 0;

			if (TryConvertGlobalUnits(graph, from, to, value, prec, out result))
			{
				return result;
			}
			else
			{
				throw new PXException(Messages.ConversionNotFound, from, to);
			}

		}

		public static bool TryConvertGlobalUnits(PXGraph graph, string from, string to, decimal value, INPrecision prec, out decimal result)
		{
			result = 0;
			if (from == to)
			{
				result = value;
				return true;
			}
						

			INUnit unit = PXSelect<INUnit,
				Where<INUnit.unitType, Equal<INUnitType.global>,
				And<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>,
				And<INUnit.toUnit, Equal<Required<INUnit.toUnit>>>>>>.Select(graph, from, to);

			if (unit == null)
			{
				return false;
			}

			if (unit.UnitMultDiv == MultDiv.Divide)
			{
				if (unit.UnitRate > 0)
					result = value / unit.UnitRate.Value;
			}
			else
			{
				result = value * unit.UnitRate.Value;
			}

			int precision = 6;

			switch (prec)
			{
				case INPrecision.QUANTITY:
					precision = INSetupDecPl.Qty;
					break;
				case INPrecision.UNITCOST:
					precision = INSetupDecPl.PrcCst;
					break;
			}

			result = decimal.Round(result, precision, MidpointRounding.AwayFromZero);
			return true;
		}

	}

	#endregion

	#region InventoryRawAttribute

	[PXDBString(InputMask = "", IsUnicode = true)]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.SelectorVisible)]
	public sealed class InventoryRawAttribute : AcctSubAttribute
	{
		public const string DimensionName = "INVENTORY";

		private Type _whereType;

		public InventoryRawAttribute()
			: base()
		{
			Type SearchType = typeof(Search<InventoryItem.inventoryCD, Where<Match<Current<AccessInfo.userName>>>>);
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(InventoryItem.inventoryCD));
			attr.CacheGlobal = true;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public InventoryRawAttribute(Type WhereType)
			: this()
		{
			if (WhereType != null)
			{
				_whereType = WhereType;

				Type SearchType = BqlCommand.Compose(
					typeof(Search<,>),
					typeof(InventoryItem.inventoryCD),
					typeof(Where2<,>),
					typeof(Match<>),
					typeof(Current<AccessInfo.userName>),
					typeof(And<>),
					_whereType);
				PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(InventoryItem.inventoryCD));
				attr.CacheGlobal = true;
				_Attributes[_SelAttrIndex] = attr;
			}
		}
	}

	#endregion

	#region INPrimaryAlternateType

	public enum INPrimaryAlternateType
	{
		VPN,
		CPN
	}

	#endregion

	#region CrossItemAttribute

	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	public class CrossItemAttribute : InventoryAttribute, IPXFieldVerifyingSubscriber
	{
		protected INPrimaryAlternateType? _PrimaryAltType = null;
		protected string _AlternateID = "AlternateID";
		protected string _SubItemID = "SubItemID";
		private bool putTyped = false;

		#region Ctor
		public CrossItemAttribute()
			: base(typeof(Where<InventoryItem.stkItem, Equal<boolTrue>, Or<InventoryItem.kitItem, Equal<boolTrue>>>))
		{
		}

		public CrossItemAttribute(Type WhereType)
			: base(WhereType)
		{
		}

		public CrossItemAttribute(Type JoinType, Type WhereType)
			: base(JoinType, WhereType)
		{
		}

		public CrossItemAttribute(INPrimaryAlternateType PrimaryAltType)
			: this()
		{
			_PrimaryAltType = PrimaryAltType;
		}

		public CrossItemAttribute(Type WhereType, INPrimaryAlternateType PrimaryAltType)
			: this(WhereType)
		{
			_PrimaryAltType = PrimaryAltType;
		}

		public CrossItemAttribute(Type JoinType, Type WhereType, INPrimaryAlternateType PrimaryAltType)
			: this(JoinType, WhereType)
		{
			_PrimaryAltType = PrimaryAltType;
		}

		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdating.RemoveHandler(sender.GetItemType(), _FieldName, ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldUpdating);
			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, this.FieldUpdating);
		}
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber))
			{
				subscribers.Remove(_Attributes[_SelAttrIndex] as ISubscriber);
			}
		}
		#endregion
		#region Implementation
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			try
			{
				((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldUpdating(sender, e);
				return;
			}
			catch (PXSetPropertyException)
			{
			}

			string AlternateID;
			string SubItemID;
			findAlternate(sender, e, out AlternateID, out SubItemID);

			((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldUpdating(sender, e);
			if (AlternateID != null)
			{
				sender.SetValuePending(e.Row, _AlternateID, AlternateID);
			}
			if (SubItemID != null)
			{
				sender.SetValuePending(e.Row, _SubItemID, SubItemID);
			}
		}
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs a)
		{
			try
			{
				((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldVerifying(sender, a);
				return;
			}
			catch (PXSetPropertyException)
			{
				if (a.Row == null)
				{
					throw;
				}
			}

			PXFieldUpdatingEventArgs e = new PXFieldUpdatingEventArgs(a.Row, sender.GetValuePending(a.Row, _FieldName));
			string AlternateID;
			string SubItemID;
			findAlternate(sender, e, out AlternateID, out SubItemID);

			((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldUpdating(sender, e);
			a.NewValue = e.NewValue;
			((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldVerifying(sender, a);
			if (AlternateID != null)
			{
				sender.SetValuePending(e.Row, _AlternateID, AlternateID);
			}
			if (SubItemID != null)
			{
				sender.SetValuePending(e.Row, _SubItemID, SubItemID);
			}
		}
		protected virtual void findAlternate(PXCache sender, PXFieldUpdatingEventArgs e, out string AlternateID, out string SubItemID)
		{
			PXResultset<INItemXRef> res = null;

			if (e.NewValue is string && !String.IsNullOrEmpty((string)e.NewValue))
			{
				//Sorting order is important for correct alternateType pick-up. Default attribute takes records from the tail
				PXSelectBase<INItemXRef> cmd = new PXSelectJoin<INItemXRef, InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INItemXRef.inventoryID>, And<InventoryItem.stkItem, Equal<boolTrue>>>, LeftJoin<INSubItem, On<INSubItem.subItemID, Equal<INItemXRef.subItemID>>>>, Where<INItemXRef.alternateID, Equal<Required<INItemXRef.alternateID>>>, OrderBy<Asc<INItemXRef.alternateType>>>(sender.Graph);

				switch (_PrimaryAltType)
				{
					case INPrimaryAlternateType.CPN:
						cmd.WhereAnd<Where<INItemXRef.alternateType, Equal<INAlternateType.cPN>, And<INItemXRef.bAccountID, Equal<Current<Customer.bAccountID>>, Or<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>, And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>>>();
						break;
					case INPrimaryAlternateType.VPN:
						cmd.WhereAnd<Where<INItemXRef.alternateType, Equal<INAlternateType.vPN>, And<INItemXRef.bAccountID, Equal<Current<Vendor.bAccountID>>, Or<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>, And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>>>();
						break;
					default:
						cmd.WhereAnd<Where<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>, And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>();
						break;
				}
				res = cmd.Select(e.NewValue);
			}

			AlternateID = null;
			SubItemID = null;
			if (res != null && res.Count > 0)
			{
				string inventoryCD = ((InventoryItem)res[0][typeof(InventoryItem)]).InventoryCD;
				e.NewValue = inventoryCD;
				if (putTyped)
				{
					AlternateID = ((INItemXRef)res[0]).AlternateID; //Place typed value
				}
				else
				{
					AlternateID = null; //Keep founded value - according to the priority in select
					if (this._PrimaryAltType.HasValue)
					{
						string alternateType = INAlternateType.ConvertFromPrimary(this._PrimaryAltType.Value);
						if (!String.IsNullOrEmpty(alternateType) && ((INItemXRef)res[0]).AlternateType == alternateType)
						{
							AlternateID = ((INItemXRef)res[0]).AlternateID; //Place typed value
						}
					}
				}
				//Skip assignment for the case when AlternateID is the same as InventoryID
				if (AlternateID != null && string.Compare(inventoryCD.Trim(), AlternateID.Trim()) == 0)
				{
					AlternateID = null;
				}
				else
				{
					SubItemID = ((INSubItem)res[0][typeof(INSubItem)]).SubItemCD;
				}
			}
		}
		#endregion
	}

	#endregion

	public enum AlternateIDOnChangeAction
	{
		StoreLocally,
		InsertNew,
		UpdateOriginal,
		AskUser,
	}

	#region AlternativeItemAttribute
	[PXUIField(DisplayName = "Alternate ID")]
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	public class AlternativeItemAttribute : PXAggregateAttribute, IPXRowUpdatingSubscriber, IPXRowDeletingSubscriber
	{
		protected INPrimaryAlternateType? _PrimaryAltType = null;
		protected Type _InventoryID;
		protected Type _SubItemID;
		protected Type _BAccountID;
		protected Type _AlternateIDChangeAction;
		

		protected AlternateIDOnChangeAction? _OnChangeAction = null;
		protected bool _KeepSinglePrimaryAltID = true;

		#region Ctor
		public AlternativeItemAttribute(INPrimaryAlternateType PrimaryAltType, Type InventoryID, Type SubItemID)
			: this(PrimaryAltType, null, InventoryID, SubItemID)
		{
		}

		public AlternativeItemAttribute(INPrimaryAlternateType PrimaryAltType, Type BAccountID, Type InventoryID, Type SubItemID )
		{
			_InventoryID = InventoryID;
			_SubItemID = SubItemID;
			_PrimaryAltType = PrimaryAltType;
			_BAccountID = BAccountID;			
			Type whereAltType = null;
			switch (_PrimaryAltType)
			{
				case INPrimaryAlternateType.CPN:
					whereAltType =
						_BAccountID == null ?
						typeof(Where<INItemXRef.alternateType, Equal<INAlternateType.cPN>,
										 And<INItemXRef.bAccountID, Equal<Current<Customer.bAccountID>>,
											Or<INItemXRef.alternateType, Equal<INAlternateType.global>>>>) :
						BqlCommand.Compose(
							typeof(Where<,,>), typeof(INItemXRef.alternateType), typeof(Equal<INAlternateType.cPN>),
							typeof(And<,,>), typeof(INItemXRef.bAccountID), typeof(Equal<>), typeof(Current<>), _BAccountID,
							typeof(Or<INItemXRef.alternateType, Equal<INAlternateType.global>>)
						);
					break;
				case INPrimaryAlternateType.VPN:
					whereAltType =
						_BAccountID == null ?
						typeof(Where<INItemXRef.alternateType, Equal<INAlternateType.vPN>,
												 And<INItemXRef.bAccountID, Equal<Current<Vendor.bAccountID>>,
													Or<INItemXRef.alternateType, Equal<INAlternateType.global>>>>) :
						BqlCommand.Compose(
							typeof(Where<,,>), typeof(INItemXRef.alternateType), typeof(Equal<INAlternateType.vPN>),
							typeof(And<,,>), typeof(INItemXRef.bAccountID), typeof(Equal<>), typeof(Current<>), _BAccountID,
							typeof(Or<INItemXRef.alternateType, Equal<INAlternateType.global>>)
						);
					break;
				default:
					whereAltType =
					typeof(Where<INItemXRef.alternateType, Equal<INAlternateType.global>>);
					break;
			}

			Type defaultType = 
				BqlCommand.Compose(
				typeof(Coalesce<,>),
				typeof(Search<,,>), typeof(INItemXRef.alternateID),
				typeof(Where<,,>), typeof(INItemXRef.inventoryID), typeof(Equal<>), typeof(Current<>), _InventoryID,
				typeof(And<,,>), typeof(INItemXRef.subItemID), typeof(Equal<>), typeof(Current<>), _SubItemID,
				typeof(And<>), whereAltType,
				typeof(OrderBy<>), typeof(Desc<>), typeof(INItemXRef.alternateType),
				typeof(Search2<,,,>), typeof(INItemXRef.alternateID),
				typeof(InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INItemXRef.inventoryID>>>),
				typeof(Where<,,>), typeof(INItemXRef.inventoryID), typeof(Equal<>), typeof(Current<>), _InventoryID,
				typeof(And<,,>), typeof(INItemXRef.subItemID), typeof(Equal<>), typeof(InventoryItem.defaultSubItemID),
				typeof(And<>), whereAltType,
				typeof(OrderBy<>), typeof(Desc<>), typeof(INItemXRef.alternateType)
				);

			//Sorting order is important - it defines a priority of Xref pick-up (obsolete - global - specific)
			//Default will take the last item, so the order is inversed
			Type formulaType =
				_BAccountID != null ?
				BqlCommand.Compose(typeof(Default<,,>), _InventoryID, _SubItemID, _BAccountID) :
				BqlCommand.Compose(typeof(Default<,>), _InventoryID, _SubItemID);
		
			this._Attributes.Add(new PXDefaultAttribute(defaultType) { PersistingCheck = PXPersistingCheck.Nothing });
			this._Attributes.Add(new PXFormulaAttribute(formulaType));
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (!sender.Graph.Views.TryGetValue("_" + typeof(INItemXRef) + "_", out _XRefView))
			{
				_XRefView = new PXView(sender.Graph, false, new Select<INItemXRef>());
				sender.Graph.Views.Add("_" + typeof(INItemXRef) + "_", _XRefView);
			}

			if (!sender.Graph.Views.Caches.Contains(typeof(INItemXRef)))
				sender.Graph.Views.Caches.Add(typeof(INItemXRef));

		}
		#endregion

		#region Implementation
		public virtual void RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			DeleteUnsavedNumber(sender,
				(int?)GetBAccountID(sender, e.Row),
				(int?)GetCurrentValue(sender, e.Row, _InventoryID),
				(int?)GetCurrentValue(sender, e.Row, _SubItemID),
				(string)sender.GetValue(e.Row, _FieldName));
		}

		public virtual void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			bool isChangedInvID = IsChanged(sender, e.Row, e.NewRow, _InventoryID);
			bool isChangedSubID = IsChanged(sender, e.Row, e.NewRow, _SubItemID);
			bool isChangedBAccountID = IsChanged(sender, e.Row, e.NewRow, _BAccountID);
			bool isChangedAltID = IsChanged(sender, e.Row, e.NewRow, _FieldName);
			if (isChangedInvID || isChangedSubID|| isChangedBAccountID || isChangedAltID )
			{
				DeleteUnsavedNumber(sender,
					(int?)GetBAccountID(sender, e.Row),
					(int?)GetCurrentValue(sender, e.Row, _InventoryID),
					(int?)GetCurrentValue(sender, e.Row, _SubItemID),
					(string)sender.GetValue(e.Row, _FieldName));

				if (!(isChangedInvID || isChangedSubID || isChangedBAccountID) && isChangedAltID)
				{
					UpdateAltNumber(sender,
					(int?)GetBAccountID(sender, e.NewRow),
					(int?)GetCurrentValue(sender, e.NewRow, _InventoryID),
					(int?)GetCurrentValue(sender, e.NewRow, _SubItemID),
					(string)sender.GetValue(e.Row, _FieldName),
					(string)sender.GetValue(e.NewRow, _FieldName));					
				}
			}
		}

		private void DeleteUnsavedNumber(PXCache sender, int? baccountId, int? inventoryId, int? subItem, string altId)
		{
			if (inventoryId == null || subItem == null || altId == null) return;

			PXCache cache = sender.Graph.Caches[typeof(INItemXRef)];
			foreach (INItemXRef item in cache.Inserted)
			{
				if (item.BAccountID == baccountId &&
						item.AlternateID == altId &&
						item.InventoryID == inventoryId &&
						item.SubItemID == subItem)
					cache.Delete(item);
			}
		}

		private void UpdateAltNumber(PXCache sender, int? baccountId, int? inventoryId, int? subItemId, string altId, string newAltId)
		{
			if (inventoryId == null || subItemId == null || newAltId == null || _PrimaryAltType == null) return;
			AlternateIDOnChangeAction action = this.GetOnChangeAction(sender.Graph);
			//Uniquieness validation
			PXSelectBase<INItemXRef> cmdAlt = new PXSelectReadonly<INItemXRef,
					Where<INItemXRef.alternateID, Equal<Required<INItemXRef.alternateID>>,
					And<INItemXRef.inventoryID, NotEqual<Required<INItemXRef.inventoryID>>>>>
					(sender.Graph);
			AddAlternativeTypeWhere(cmdAlt, false);
			foreach (INItemXRef it in cmdAlt.Select(newAltId, inventoryId, baccountId))
			{
				if (it.InventoryID != inventoryId || it.SubItemID != subItemId)
				{
					throw new PXSetPropertyException(Messages.AlternatieIDNotUnique, PXErrorLevel.Error, newAltId );
				}
			}
			if (action == AlternateIDOnChangeAction.StoreLocally) return;
			PXSelectBase<INItemXRef> cmdFullSearch = new PXSelectReadonly<INItemXRef,
					Where<INItemXRef.inventoryID, Equal<Required<INItemXRef.inventoryID>>,
					And<INItemXRef.subItemID, Equal<Required<INItemXRef.subItemID>>,
					And<INItemXRef.alternateID, Equal<Required<INItemXRef.alternateID>>>>>, OrderBy<Asc<INItemXRef.alternateType, Desc<INItemXRef.alternateID>>>>(sender.Graph);
			AddAlternativeTypeWhere(cmdFullSearch, false);
			INItemXRef existing = cmdFullSearch.Select(inventoryId, subItemId, newAltId, baccountId);
			if (existing != null)
				return; //Applicable record with new AlternateID exists - no need to update Xref

			INItemXRef xref = null;
			if(action == AlternateIDOnChangeAction.UpdateOriginal || action == AlternateIDOnChangeAction.AskUser)
			{
				PXSelectBase<INItemXRef> cmdInv = new PXSelectReadonly<INItemXRef,
					Where<INItemXRef.inventoryID, Equal<Required<INItemXRef.inventoryID>>,
					And<INItemXRef.subItemID, Equal<Required<INItemXRef.subItemID>>,
					And<INItemXRef.alternateID, Equal<Required<INItemXRef.alternateID>>>>>, OrderBy<Asc<INItemXRef.alternateType, Desc<INItemXRef.alternateID>>>>(sender.Graph);
				AddAlternativeTypeWhere(cmdInv, true);
				xref = cmdInv.Select(inventoryId, subItemId, altId, baccountId);
				//Need update
				if (xref != null)
				{
					if (!string.IsNullOrEmpty(xref.AlternateID))
					{
						if (action == AlternateIDOnChangeAction.AskUser
							&& (_XRefView.Ask(Messages.ConfirmationXRefUpdate, MessageButtons.YesNo, false) != WebDialogResult.Yes))
							return; // Store locally						
					}
					_XRefView.Cache.Delete(xref);
				}
				else
				{
					if (this._KeepSinglePrimaryAltID)
					{
						PXSelectBase<INItemXRef> cmdOtherInv = new PXSelectReadonly<INItemXRef,
							Where<INItemXRef.inventoryID, Equal<Required<INItemXRef.inventoryID>>,
							And<INItemXRef.subItemID, Equal<Required<INItemXRef.subItemID>>,
							And<INItemXRef.alternateID, NotEqual<Required<INItemXRef.alternateID>>>>>>(sender.Graph);
						AddAlternativeTypeWhere(cmdOtherInv, true);
						foreach (INItemXRef it in cmdOtherInv.Select(inventoryId, subItemId, newAltId, baccountId))
						{
							if (!String.IsNullOrEmpty(it.AlternateID)) return; //There is another
						}
					}
				}
			}

			xref = (xref != null)? (INItemXRef)_XRefView.Cache.CreateCopy(xref):new INItemXRef();
			xref.InventoryID = inventoryId;
			xref.SubItemID = subItemId;
			xref.BAccountID = baccountId;
			xref.AlternateID = newAltId;
			xref.AlternateType = INAlternateType.ConvertFromPrimary(_PrimaryAltType.Value);
			_XRefView.Cache.Update(xref);
			_XRefView.Answer = WebDialogResult.None;
		} 

		private void AddAlternativeTypeWhere(PXSelectBase<INItemXRef> cmd, bool typeExclusive)
		{
			switch (_PrimaryAltType)
			{
				case INPrimaryAlternateType.CPN:
					if (typeExclusive)
					{
						cmd.WhereAnd<Where<INItemXRef.alternateType, Equal<INAlternateType.cPN>,
						And<INItemXRef.bAccountID, Equal<Required<INItemXRef.bAccountID>>>>>();
					}
					else
					{
						cmd.WhereAnd<Where<INItemXRef.alternateType, Equal<INAlternateType.cPN>,
						And<INItemXRef.bAccountID, Equal<Required<INItemXRef.bAccountID>>,
							Or<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>,
								And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>>>();
					}
					break;
				case INPrimaryAlternateType.VPN:
					if (typeExclusive)
					{
						cmd.WhereAnd<Where<INItemXRef.alternateType, Equal<INAlternateType.vPN>,
							And<INItemXRef.bAccountID, Equal<Required<INItemXRef.bAccountID>>>>>();
					}
					else
					{
						cmd.WhereAnd<Where<INItemXRef.alternateType, Equal<INAlternateType.vPN>,
						And<INItemXRef.bAccountID, Equal<Required<INItemXRef.bAccountID>>,
							Or<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>,
								And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>>>();
					}
					break;
				default:
					cmd.WhereAnd<Where<INItemXRef.alternateType, NotEqual<INAlternateType.cPN>,
							And<INItemXRef.alternateType, NotEqual<INAlternateType.vPN>>>>();
					break;
			}
		}

		private object GetBAccountID(PXCache sender, object row)
		{
			return
				_BAccountID == null ?
				(_PrimaryAltType == INPrimaryAlternateType.VPN
													?
														(int?)GetCurrentValue(sender, typeof(Vendor.bAccountID))
													:
														(int?)GetCurrentValue(sender, typeof(Customer.bAccountID))
				) :
				GetCurrentValue(sender, row, _BAccountID);
		}

		private object GetCurrentValue(PXCache sender, object row, Type _item)
		{
			PXCache source = sender.Graph.Caches[BqlCommand.GetItemType(_item)];
			return source.GetValue(row, _item.Name);
		}

		private object GetCurrentValue(PXCache sender, Type _item)
		{
			PXCache source = sender.Graph.Caches[BqlCommand.GetItemType(_item)];
			return source.GetValue(source.Current, _item.Name);
		}

		private bool IsChanged(PXCache sender, object row, object newrow, Type fieldSource)
		{
			if (fieldSource != null && BqlCommand.GetItemType(fieldSource).IsAssignableFrom(sender.GetItemType()))
			{
				return IsChanged(sender, row, newrow, fieldSource.Name);
			}
			return false;
		}

		private bool IsChanged(PXCache sender, object row, object newrow, string name)
		{
			return !object.Equals(sender.GetValue(newrow, name), sender.GetValue(row, name));
		}

		private AlternateIDOnChangeAction GetOnChangeAction(PXGraph caller) 
		{
			if (this._OnChangeAction == null) 
			{
				this._OnChangeAction = AlternateIDOnChangeAction.AskUser;				
			}
			return this._OnChangeAction.Value;
		}
		private PXView _XRefView;
		#endregion
	}
	#endregion

	#region StockItemAttribute

	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	public class StockItemAttribute : InventoryAttribute
	{
		public StockItemAttribute()
			: base(typeof(Where<InventoryItem.stkItem, Equal<boolTrue>>))
		{
		}
	}

	#endregion

	#region NonStockItemAttribute

	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	public class NonStockItemAttribute : InventoryAttribute
	{
		public NonStockItemAttribute()
			: base(typeof(Where<InventoryItem.stkItem, Equal<boolFalse>>))
		{			
		}
	}

	#endregion

	#region InventoryAttribute

	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	public class InventoryAttribute : AcctSubAttribute
	{
		#region State
		public const string DimensionName = "INVENTORY";

		public class dimensionName : Constant<string>
		{
			public dimensionName() : base(DimensionName) { ;}
		}
		#endregion
		#region Ctor
		public InventoryAttribute()
			: this(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
		{
		}

		public InventoryAttribute(Type SearchType, Type SubstituteKey, Type DescriptionField)
			: base()
		{
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, SubstituteKey);
			attr.CacheGlobal = true;
			attr.DescriptionField = DescriptionField;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public InventoryAttribute(Type WhereType)
			: this(BqlCommand.Compose(
					typeof(Search<,>),
					typeof(InventoryItem.inventoryID),
					typeof(Where2<,>),
					typeof(Match<>),
					typeof(Current<AccessInfo.userName>),
					typeof(And2<,>),
					typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
								And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>),
					typeof(And<>),
					WhereType),typeof(InventoryItem.inventoryCD),typeof(InventoryItem.descr))
		{
		}

		public InventoryAttribute(Type JoinType, Type WhereType)
			: this(BqlCommand.Compose(
					typeof(Search2<,,>),
					typeof(InventoryItem.inventoryID),
					JoinType,
					typeof(Where2<,>),
					typeof(Match<>),
					typeof(Current<AccessInfo.userName>),
					typeof(And2<,>),
					typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
								And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>),
					typeof(And<>),
					WhereType), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))
		{
		}
		#endregion
	}
	#endregion

	#region SubItemRawAttribute
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subitem ID", Visibility = PXUIVisibility.SelectorVisible, FieldClass = DimensionName)]
	public class SubItemRawAttribute : AcctSubAttribute
	{
		public bool SuppressValidation;
		public const string DimensionName = "INSUBITEM";

		public SubItemRawAttribute()
			: base()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute(DimensionName);
			attr.ValidComboRequired = false;

			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}

	#endregion

	#region SubItemRawExtAttribute
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subitem ID", Visibility = PXUIVisibility.SelectorVisible, FieldClass = DimensionName)]
	public class SubItemRawExtAttribute : AcctSubAttribute
	{
		public const string DimensionName = "INSUBITEM";

		#region Ctors
		public SubItemRawExtAttribute()
			: base()
		{
		}

		public SubItemRawExtAttribute(Type inventoryItem)
			: this()
		{
			if (inventoryItem != null)
			{
				Type SearchType = BqlCommand.Compose(
					typeof(Search<,>),
					typeof(INSubItem.subItemCD),
					typeof(Where2<,>),
					typeof(Match<>),
					typeof(Current<AccessInfo.userName>),
					typeof(And<>),
					typeof(Where<,,>),
					typeof(Optional<>), inventoryItem, typeof(IsNull),
					typeof(Or<>),
					typeof(Where<>),
					typeof(Match<>),
					typeof(Optional<>),
					inventoryItem);

				var attr = new PXDimensionSelectorAttribute(DimensionName, SearchType);
				attr.ValidComboRequired = false;
				_Attributes.Add(attr);
				_SelAttrIndex = _Attributes.Count - 1;
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.subItem>())
			{
				((PXDimensionSelectorAttribute)this._Attributes[_Attributes.Count - 1]).ValidComboRequired = false;
				sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, FieldDefaulting);
			}

			base.CacheAttached(sender);
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = this.Definitions.DefaultSubItemCD;
			e.Cancel = true;
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber))
			{
				subscribers.Clear();
			}
		}


		#endregion

		#region Default SubItemID
		protected virtual Definition Definitions
		{
			get
			{
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("INSubItem.DefinitionCD", typeof(INSubItem)));
				}
				return defs;
			}
		}

		protected class Definition : IPrefetchable
		{
			private string _DefaultSubItemCD;
			public string DefaultSubItemCD
			{
				get { return _DefaultSubItemCD; }
			}

			public void Prefetch()
			{
				using (PXDataRecord record = PXDatabase.SelectSingle<INSubItem>(
					new PXDataField<INSubItem.subItemCD>(),
					new PXDataFieldOrder<INSubItem.subItemID>()))
				{
					_DefaultSubItemCD = null;
					if (record != null)
						_DefaultSubItemCD = record.GetString(0);
				}
			}
		}
		#endregion

	}
	#endregion

	#region inventoryModule

	public sealed class inventoryModule : Constant<string>
	{
		public inventoryModule()
			: base(typeof(inventoryModule).Namespace)
		{
		}
	}

	#endregion

	#region warehouseType

	public sealed class warehouseType : Constant<string>
	{
		public warehouseType()
			: base(typeof(PX.Objects.IN.INSite).FullName)
		{
		}
	}

	#endregion

	#region itemType

	public sealed class itemType : Constant<string>
	{
		public itemType()
			: base(typeof(PX.Objects.IN.InventoryItem).FullName)
		{
		}
	}

	#endregion

	#region itemClassType

	public sealed class itemClassType : Constant<string>
	{
		public itemClassType()
			: base(typeof(PX.Objects.IN.INItemClass).FullName)
		{
		}
	}

	#endregion

	#region INSetupSelect

	public sealed class INSetupSelect : Data.PXSetupSelect<INSetup>
	{
		public INSetupSelect(PXGraph graph) : base(graph) { }
	}

	#endregion

	#region SubItemAttribute
	[PXDBInt]
	[PXUIField(DisplayName = "Subitem", Visibility = PXUIVisibility.Visible, FieldClass = SubItemAttribute.DimensionName)]
	public class SubItemAttribute : AcctSubAttribute
	{
		#region dimensionName

		public class dimensionName : Constant<string>
		{
			public dimensionName() : base(DimensionName) { }
		}

		#endregion

		#region INSubItemDimensionSelector
		private class INSubItemDimensionSelector: PXDimensionSelectorAttribute
		{
			private readonly Type _inventoryID = null;

			public INSubItemDimensionSelector(Type inventoryID, Type search)
				: base(DimensionName, search, typeof(INSubItem.subItemCD))
			{				
				this._inventoryID = inventoryID;				
				if (this._inventoryID != null)
				{
					this.CacheGlobal = false;
					this.SetSegmentDelegate((PXSelectDelegate<short?>) (DoSegmentSelect));
				}				 
			}		

			private IEnumerable DoSegmentSelect([PXShort] short? segment)
			{
				PXGraph graph = PXView.CurrentGraph;
				if (_inventoryID == null) yield break;

                int? inventoryID = null;
                if (PXView.Currents != null)
                    foreach (object item in PXView.Currents)
                    {
                        if (item.GetType() == _inventoryID.DeclaringType)
                            inventoryID = (int?)graph.Caches[_inventoryID.DeclaringType].GetValue(item, _inventoryID.Name);
                    }

				int startRow = PXView.StartRow;
				int totalRows = 0;
				
				PXView intView = new PXView(PXView.CurrentGraph, false,
					BqlCommand.CreateInstance(
						typeof(Select2<,,>),
						typeof(INSubItemSegmentValue),
						typeof(InnerJoin<SegmentValue,
										On<SegmentValue.segmentID, Equal<INSubItemSegmentValue.segmentID>,
									And<SegmentValue.value, Equal<INSubItemSegmentValue.value>,
									And<SegmentValue.dimensionID, Equal<SubItemAttribute.dimensionName>>>>>),
						typeof(Where<,,>), typeof(INSubItemSegmentValue.segmentID), typeof(Equal<Required<SegmentValue.segmentID>>),
						typeof(And<,>), typeof(INSubItemSegmentValue.inventoryID), typeof(Equal<>), typeof(Optional<>), _inventoryID));

				foreach (PXResult<INSubItemSegmentValue, SegmentValue> rec in intView
					.Select(PXView.Currents, new object[]{segment, inventoryID}, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
								 ref startRow, PXView.MaximumRows, ref totalRows))
				{
					SegmentValue value = rec;
					PXDimensionAttribute.SegmentValue ret = new PXDimensionAttribute.SegmentValue();					
					ret.Value = value.Value;					
					ret.Descr = value.Descr;
					ret.IsConsolidatedValue = value.IsConsolidatedValue;
					yield return ret;
				}
				PXView.StartRow = 0;
			}

		}
		#endregion 

		#region Fields

		public const string DimensionName = "INSUBITEM";

		#endregion

		#region Ctors

		public SubItemAttribute()
			: base()	
		{
			//var attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(INSubItem.subItemCD));
			//attr.CacheGlobal = true;
			
			var attr = new PXDimensionSelectorAttribute(DimensionName, typeof(Search<INSubItem.subItemID, Where<Match<Current<AccessInfo.userName>>>>), typeof(INSubItem.subItemCD));
			attr.CacheGlobal = true;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public SubItemAttribute(Type inventoryID)
			: this(inventoryID, null)
		{					
		}

		public SubItemAttribute(Type inventoryID, Type JoinType)
			: base()
		{			
			Type SearchType = 
				JoinType == null ? 	typeof(Search<INSubItem.subItemID, Where<Match<Current<AccessInfo.userName>>>>) :
				BqlCommand.Compose(
				typeof(Search2<,,>),
				typeof(INSubItem.subItemID),
				JoinType,
				typeof(Where<>),
				typeof(Match<>),
				typeof(Current<AccessInfo.userName>));				

			var attr = PXAccess.FeatureInstalled<FeaturesSet.subItem>() ?
				new INSubItemDimensionSelector(inventoryID, SearchType) :
				new PXDimensionSelectorAttribute(DimensionName,
					SearchType,
					typeof(INSubItem.subItemCD)) { CacheGlobal = true };
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;	
		}

		#endregion

		#region Implementation

		public override void CacheAttached(PXCache sender)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.subItem>())
			{
				((PXDimensionSelectorAttribute)this._Attributes[_Attributes.Count - 1]).ValidComboRequired = false;
				sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, FieldDefaulting);
			}

			base.CacheAttached(sender);
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.Definitions.DefaultSubItemID == null)
			{
				object newValue = "0";
				sender.RaiseFieldUpdating(_FieldName, e.Row, ref newValue);
				e.NewValue = newValue;
			}
			else
			{
				e.NewValue = this.Definitions.DefaultSubItemID;
			}

			e.Cancel = true;
		}

		#endregion

		#region Default SubItemID
		protected virtual Definition Definitions
		{
			get
			{
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("INSubItem.Definition", typeof(INSubItem)));
				}
				return defs;
			}
		}
		
		protected class Definition : IPrefetchable
		{
			private int? _DefaultSubItemID;
			public int? DefaultSubItemID
			{
				get { return _DefaultSubItemID; }
			}

			public void Prefetch()
			{
				using (PXDataRecord record = PXDatabase.SelectSingle<INSubItem>(
					new PXDataField<INSubItem.subItemID>(),
					new PXDataFieldOrder<INSubItem.subItemID>()))
				{
					_DefaultSubItemID = null;
					if (record != null)
						_DefaultSubItemID = record.GetInt32(0);
				}
			}
		}
		#endregion
	}
	#endregion

	#region SiteRawAttribute

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Warehouse ID", Visibility = PXUIVisibility.SelectorVisible)]
	public sealed class SiteRawAttribute : AcctSubAttribute
	{
		public string DimensionName = "INSITE";
		public SiteRawAttribute()
			: base()
		{
			Type SearchType = typeof(Search<INSite.siteCD, Where<Match<Current<AccessInfo.userName>>>>);
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(INSite.siteCD));
			attr.CacheGlobal = true;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}

	#endregion

	#region SiteAvailAttribute

	[PXDBInt()]
	[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
    public class POSiteAvailAttribute : SiteAvailAttribute
	{
		#region Ctor
		public POSiteAvailAttribute(Type InventoryType, Type SubItemType)
			: base(InventoryType, SubItemType)
		{
		}
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldUpdated.RemoveHandler(sender.GetItemType(), _inventoryType.Name, InventoryID_FieldUpdated);
		}
		#endregion
		#region Implementation
		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
		}
        #endregion
	}

	[PXDBInt()]
	[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
    [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), IN.Messages.InactiveWarehouse, typeof(INSite.siteCD))]
	public class SiteAvailAttribute : SiteAttribute, IPXFieldDefaultingSubscriber
	{
		#region State
		protected Type _inventoryType;
		protected Type _subItemType;
		protected BqlCommand _Select;
		#endregion

		#region Ctor
		public SiteAvailAttribute(Type InventoryType, Type SubItemType)
			: base()
		{
			_inventoryType = InventoryType;
			_subItemType = SubItemType;

			Type SearchType = BqlCommand.Compose(
				typeof(Search2<,,>),
				typeof(INSite.siteID),
				typeof(LeftJoin<,>),
				typeof(INSiteStatus),
				typeof(On<,,>),
				typeof(INSiteStatus.siteID),
				typeof(Equal<>),
				typeof(INSite.siteID),
				typeof(And<,,>),
				typeof(INSiteStatus.inventoryID),
				typeof(Equal<>),
				typeof(Optional<>),
				InventoryType,
				typeof(And<,>),
				typeof(INSiteStatus.subItemID),
				typeof(Equal<>),
				typeof(Optional<>),
				SubItemType,
				typeof(Where<>),
				typeof(Match<>),
				typeof(Current<AccessInfo.userName>)
				);
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(INSite.siteCD), new Type[] { typeof(INSite.siteCD), typeof(INSiteStatus.qtyOnHand), typeof(INSiteStatus.active), typeof(INSite.descr) });
			//should ALWAYS be false, otherwise parameters will be ignored
			attr.CacheGlobal = false;
			attr.DescriptionField = typeof(INSite.descr);
			_Attributes[_SelAttrIndex] = attr;

			Type SelectType = BqlCommand.Compose(
							typeof(Select2<,,>),
							typeof(InventoryItem),
							typeof(InnerJoin<INSite, On<INSite.siteID, Equal<InventoryItem.dfltSiteID>>>),
							typeof(Where<,,>), typeof(InventoryItem.inventoryID), typeof(Equal<>), typeof(Current<>),
							_inventoryType,
							typeof(And<Match<INSite, Current<AccessInfo.userName>>>));

			_Select = BqlCommand.CreateInstance(SelectType);
		}

		public SiteAvailAttribute(Type InventoryType, Type SubItemType, Type[] colsType)
			: base()
		{
			_inventoryType = InventoryType;
			_subItemType = SubItemType;

			Type SearchType = BqlCommand.Compose(
				typeof(Search2<,,>),
				typeof(INSite.siteID),
				typeof(LeftJoin<,>),
				typeof(INSiteStatus),
				typeof(On<,,>),
				typeof(INSiteStatus.siteID),
				typeof(Equal<>),
				typeof(INSite.siteID),
				typeof(And<,,>),
				typeof(INSiteStatus.inventoryID),
				typeof(Equal<>),
				typeof(Optional<>),
				InventoryType,
				typeof(And<,>),
				typeof(INSiteStatus.subItemID),
				typeof(Equal<>),
				typeof(Optional<>),
				SubItemType,
				typeof(Where<>),
				typeof(Match<>),
				typeof(Current<AccessInfo.userName>)
				);
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(INSite.siteCD), colsType);
			//should ALWAYS be false, otherwise parameters will be ignored
			attr.CacheGlobal = false;
			attr.DescriptionField = typeof(INSite.descr);
			_Attributes[_SelAttrIndex] = attr;

			Type SelectType = BqlCommand.Compose(
							typeof(Select2<,,>),
							typeof(InventoryItem),
							typeof(InnerJoin<INSite, On<INSite.siteID, Equal<InventoryItem.dfltSiteID>>>),
							typeof(Where<,,>), typeof(InventoryItem.inventoryID), typeof(Equal<>), typeof(Current<>),
							_inventoryType,
							typeof(And<Match<INSite, Current<AccessInfo.userName>>>));

			_Select = BqlCommand.CreateInstance(SelectType);
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _inventoryType.Name, InventoryID_FieldUpdated);
		}
		#endregion

		#region Implementation
		public virtual void InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			try
			{
				sender.SetDefaultExt(e.Row, _FieldName);
			}
			catch (PXUnitConversionException) { }
			catch (PXSetPropertyException)
			{
				PXUIFieldAttribute.SetError(sender, e.Row, _FieldName, null);
				sender.SetValue(e.Row, _FieldOrdinal, null);
			}
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXView view = sender.Graph.TypedViews.GetView(_Select, false);

			PXResult<InventoryItem, INSite> item = (PXResult<InventoryItem, INSite>)view.SelectSingleBound(new object[] { e.Row });
			//in transfers entry do not default after PXDefault2()
			if (!e.Cancel && item != null)
			{
				object val = view.Cache.GetValueExt<InventoryItem.dfltSiteID>((InventoryItem)item);
				if (val is PXFieldState)
				{
					e.NewValue = ((PXFieldState)val).Value;
				}
				else
				{
					e.NewValue = val;
				}
			}
		}
		#endregion
	}
	#endregion

	#region SiteAttribute

	[PXDBInt()]
	[PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
	public class SiteAttribute : AcctSubAttribute
	{
		public const string DimensionName = "INSITE";

		public class dimensionName : Constant<string>
		{
			public dimensionName() : base(DimensionName) { ;}
		}

		private Type _whereType;

		public SiteAttribute()
			: this(typeof(Where<Match<Current<AccessInfo.userName>>>), false)
		{
		}
		public SiteAttribute(Type WhereType)
			: this(WhereType, true)
		{			
		}

		public SiteAttribute(Type WhereType, bool validateAccess)
		{			
			if (WhereType != null)
			{
				_whereType = WhereType;

				Type SearchType = validateAccess 
				? BqlCommand.Compose(
					typeof(Search<,>),
					typeof(INSite.siteID),
					typeof(Where2<,>),
					typeof(Match<>),
					typeof(Current<AccessInfo.userName>),
					typeof(And<>),
					_whereType)
				: BqlCommand.Compose(
					typeof(Search<,>),
					typeof(INSite.siteID),
					_whereType);

				PXDimensionSelectorAttribute attr;
				_Attributes.Add(attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(INSite.siteCD))); 
				attr.CacheGlobal = true;
				attr.DescriptionField = typeof(INSite.descr);
				_SelAttrIndex = _Attributes.Count - 1;
			}
		}

		#region Implemetation
		public override void CacheAttached(PXCache sender)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>() && sender.Graph.GetType() != typeof(PXGraph))
			{
				((PXDimensionSelectorAttribute)this._Attributes[_Attributes.Count - 1]).ValidComboRequired = false;
				sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, Feature_FieldDefaulting);
			}

			base.CacheAttached(sender);
		}

		public virtual void Feature_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!e.Cancel)
			{
				if (this.Definitions.DefaultSiteID == null)
				{
					object newValue = INSite.Main;
					sender.RaiseFieldUpdating(_FieldName, e.Row, ref newValue);
					e.NewValue = newValue;
				}
				else
				{
					e.NewValue = this.Definitions.DefaultSiteID;
				}

				e.Cancel = true;
			}
		}
		#endregion

		#region Default SiteID
		protected virtual Definition Definitions
		{
			get
			{
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("INSite.Definition", typeof(INSite)));
				}
				return defs;
			}
		}

		protected class Definition : IPrefetchable
		{
			private int? _DefaultSiteID;
			public int? DefaultSiteID
			{
				get { return _DefaultSiteID; }
			}

			public void Prefetch()
			{
				using (PXDataRecord record = PXDatabase.SelectSingle<INSite>(
					new PXDataField<INSite.siteID>(),
					new PXDataFieldOrder<INSite.siteID>()))
				{
					_DefaultSiteID = null;
					if (record != null)
						_DefaultSiteID = record.GetInt32(0);
				}
			}
		}
		#endregion

	}

    [PXDBInt()]
    [PXUIField(DisplayName = "To Site ID", Visibility = PXUIVisibility.Visible)]
    [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), IN.Messages.InactiveWarehouse, typeof(INSite.siteCD))]
    public class ToSiteAttribute : SiteAttribute
	{
		public ToSiteAttribute()
			: base(typeof(Where<Where2<Match<Current<AccessInfo.userName>>, Or<Current<INRegister.transferType>, Equal<INTransferType.twoStep>>>>), 
				false)
		{			
		}
	}
	#endregion

	#region ItemSiteAttribute
	
	public class ItemSiteAttribute : PXSelectorAttribute
	{
		public ItemSiteAttribute()
			:base(typeof(Search2<INItemSite.siteID,				
				InnerJoin<INSite, On<INSite.siteID, Equal<INItemSite.siteID>,
				      And<Where<CurrentMatch<INSite, AccessInfo.userName>>>>>,
				Where<INItemSite.inventoryID, Equal<Current<INItemSite.inventoryID>>>>))
		{
			this._SubstituteKey = typeof (INSite.siteCD);			
			this._UnconditionalSelect = BqlCommand.CreateInstance(typeof(Search<INSite.siteID, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>));
			this._NaturalSelect = BqlCommand.CreateInstance(typeof(Search<INSite.siteCD, Where<INSite.siteCD, Equal<Required<INSite.siteCD>>>>));
		}
		public override void SubstituteKeyCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{			
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select &&
				(e.Operation & PXDBOperation.Option) == PXDBOperation.External &&
				(e.Value == null || e.Value is string))
			{
				e.Cancel = true;
				foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(_FieldName))
				{
					if (attr is PXDBFieldAttribute)
					{
						e.FieldName = BqlCommand.SubSelect + _SubstituteKey.Name + 
							" FROM " + BqlCommand.GetTableName(typeof(INSite)) + " " + _Type.Name + "Ext WHERE " + 
							_Type.Name + "Ext." + typeof(INSite.siteID).Name + " = " + (e.Table == null ? _BqlTable.Name : e.Table.Name) + "." + ((PXDBFieldAttribute)attr).DatabaseFieldName + ")";
						if (e.Value != null)
						{
							e.DataValue = e.Value;
							e.DataType = PXDbType.NVarChar;
							e.DataLength = ((string)e.Value).Length;
						}
						break;
					}
				}
			}
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}
	}
	#endregion

	#region ReplenishmentSourceSiteAttribute
	public class ReplenishmentSourceSiteAttribute : SiteAttribute
	{
		public ReplenishmentSourceSiteAttribute(Type replenishmentSource)
			:base()
		{
			DescriptionField = typeof (INSite.descr);
			this.source = replenishmentSource;
		}
		private Type source;

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.RowSelected.AddHandler(sender.GetItemType(), OnRowSelected);
			sender.Graph.RowUpdated.AddHandler(sender.GetItemType(), OnRowUpdated);
		}
		protected virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(sender, e.Row, _FieldName,
				e.Row != null &&
				INReplenishmentSource.IsTransfer((string)sender.GetValue(e.Row, this.source.Name)) );
		}
		protected virtual void OnRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (e.Row == null) return;
			if (!INReplenishmentSource.IsTransfer((string)sender.GetValue(e.Row, this.source.Name)))
				sender.SetValue(e.Row, _FieldName, null);				
		}		

	}
	#endregion

	#region LocationRawAttribute

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Location ID", Visibility = PXUIVisibility.SelectorVisible)]
	public sealed class LocationRawAttribute : AcctSubAttribute
	{
		public string DimensionName = "INLOCATION";
		public LocationRawAttribute()
			: base()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute(DimensionName);
			attr.ValidComboRequired = false;

			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}

	#endregion

	#region LocationAvailAttribute

	[PXDBInt()]
	[PXUIField(DisplayName = "Location", Visibility = PXUIVisibility.Visible, FieldClass = LocationAttribute.DimensionName)]
	public class LocationAvailAttribute : LocationAttribute, IPXFieldVerifyingSubscriber, IPXFieldDefaultingSubscriber
	{
		#region State
		protected Type _InventoryType;
		protected Type _IsSalesType;
		protected Type _IsReceiptType;
		protected Type _IsTransferType;
		protected Type _IsStandardCostAdjType;
		protected BqlCommand _Select;
		#endregion

		#region Ctor

		public LocationAvailAttribute(Type InventoryType, Type SubItemType, Type SiteIDType, bool IsSalesType, bool IsReceiptType, bool IsTransferType)
			: this(InventoryType, SubItemType, SiteIDType, null, null, null)
		{
			_IsSalesType = BqlCommand.Compose(
				typeof(Where<,>),
				IsSalesType ? typeof(boolTrue) : typeof(boolFalse),
				typeof(Equal<boolTrue>)
				);

			_IsReceiptType = BqlCommand.Compose(
				typeof(Where<,>),
				IsReceiptType ? typeof(boolTrue) : typeof(boolFalse),
				typeof(Equal<boolTrue>)
				);

			_IsTransferType = BqlCommand.Compose(
				typeof(Where<,>),
				IsTransferType ? typeof(boolTrue) : typeof(boolFalse),
				typeof(Equal<boolTrue>)
				);

			_IsStandardCostAdjType = typeof(Where<boolFalse, Equal<boolTrue>>);

		}

		public LocationAvailAttribute(Type InventoryType, Type SubItemType, Type SiteIDType, Type TranType, Type InvtMultType)
			: this(InventoryType, SubItemType, SiteIDType, null, null, null)
		{
			_IsSalesType = BqlCommand.Compose(
				typeof(Where<,,>),
				TranType,
				typeof(Equal<INTranType.invoice>),
				typeof(Or<,>),
				TranType,
				typeof(Equal<INTranType.debitMemo>)
				);

			_IsReceiptType = BqlCommand.Compose(
				typeof(Where<,,>),
				TranType,
				typeof(Equal<INTranType.receipt>),
				typeof(Or<,,>),
				TranType,
				typeof(Equal<INTranType.issue>),
				typeof(Or<,,>),
				TranType,
				typeof(Equal<INTranType.return_>),
				typeof(Or<,>),
				TranType,
				typeof(Equal<INTranType.creditMemo>)
				);

			_IsTransferType = BqlCommand.Compose(
				typeof(Where<,,>),
				TranType,
				typeof(Equal<INTranType.transfer>),
				typeof(And<,,>),
				InvtMultType,
				typeof(Equal<short1>),
				typeof(Or<,,>),
				TranType,
				typeof(Equal<INTranType.transfer>),
				typeof(And<,>),
				InvtMultType,
				typeof(Equal<shortMinus1>)
				);

			_IsStandardCostAdjType = BqlCommand.Compose(
				typeof(Where<,,>),
				TranType,
				typeof(Equal<INTranType.standardCostAdjustment>),
				typeof(Or<,>),
				TranType,
				typeof(Equal<INTranType.negativeCostAdjustment>)
				);
		}

		public LocationAvailAttribute(Type InventoryType, Type SubItemType, Type SiteIDType, Type IsSalesType, Type IsReceiptType, Type IsTransferType)
			: base(SiteIDType)
		{
			_InventoryType = InventoryType;
			_IsSalesType = IsSalesType;
			_IsReceiptType = IsReceiptType;
			_IsTransferType = IsTransferType;
			_IsStandardCostAdjType = typeof(Where<boolFalse, Equal<boolTrue>>);

			Type SearchType = BqlCommand.Compose(
				typeof(Search2<,,>),
				typeof(INLocation.locationID),
				typeof(LeftJoin<,>),
				typeof(INLocationStatus),
				typeof(On<,,>),
				typeof(INLocationStatus.locationID),
				typeof(Equal<>),
				typeof(INLocation.locationID),
				typeof(And<,,>),
				typeof(INLocationStatus.inventoryID),
				typeof(Equal<>),
				typeof(Optional<>),
				InventoryType,
				typeof(And<,>),
				typeof(INLocationStatus.subItemID),
				typeof(Equal<>),
				typeof(Optional<>),
				SubItemType,
				typeof(Where2<,>),
				typeof(Match<>),
				typeof(Current<AccessInfo.userName>),
				typeof(And<>),
				typeof(Where<,,>),
				typeof(INLocation.siteID),
				typeof(Equal<>),
				typeof(Optional<>),
				SiteIDType,
				typeof(And<,>),
				typeof(INLocation.active),
				typeof(Equal<>),
				typeof(boolTrue)
				);
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(INLocation.locationCD), new Type[] { typeof(INLocation.locationCD), typeof(INLocationStatus.qtyOnHand), typeof(INLocationStatus.active), typeof(INLocation.primaryItemID), typeof(INLocation.primaryItemClassID), typeof(INLocation.receiptsValid), typeof(INLocation.salesValid), typeof(INLocation.transfersValid) });
			//should ALWAYS be false, otherwise SiteID parameter will be ignored
			attr.ValidComboRequired = true;
			attr.CacheGlobal = false;
			attr.DirtyRead = true;
			attr.DescriptionField = typeof(INLocation.descr);
			_Attributes[_SelAttrIndex] = attr;

			Type SelectType = BqlCommand.Compose(
				typeof(Select2<,,>),
				typeof(INSite),
				typeof(LeftJoin<,>),
				typeof(INItemSite),
				typeof(On<,,>),
				typeof(INItemSite.siteID),
				typeof(Equal<>),
				typeof(INSite.siteID),
				typeof(And<,>),
				typeof(INItemSite.inventoryID),
				typeof(Equal<>),
				typeof(Current<>),
				_InventoryType,
				typeof(Where<,>), typeof(INSite.siteID), typeof(Equal<>), typeof(Current2<>), _SiteIDType);

			_Select = BqlCommand.CreateInstance(SelectType);
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			//sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _SiteIDType.Name, SiteID_FieldUpdated);
		}

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
		public override void SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				sender.SetValuePending(e.Row, _FieldName, PXCache.NotSetValue); 
				try
				{
					sender.SetDefaultExt(e.Row, _FieldName);
				}
				catch (PXSetPropertyException)
				{
					PXUIFieldAttribute.SetError(sender, e.Row, _FieldName, null);
					sender.SetValue(e.Row, _FieldOrdinal, null);
				}
			}
			else
			{
				base.SiteID_FieldUpdated(sender, e);

				object locationid;
				if ((locationid = sender.GetValue(e.Row, _FieldOrdinal)) == null || (int?)locationid < 0)
				{
					PXUIFieldAttribute.SetError(sender, e.Row, _FieldName, null);
					sender.SetValuePending(e.Row, _FieldName, PXCache.NotSetValue);
					try
					{
						sender.SetDefaultExt(e.Row, _FieldName);
					}
					catch (PXSetPropertyException)
					{
						PXUIFieldAttribute.SetError(sender, e.Row, _FieldName, null);
						sender.SetValue(e.Row, _FieldOrdinal, null);
					}
				}
			}
		}

     
		public virtual void ThrowErrorItem(string message, PXFieldVerifyingEventArgs e, object ErrorValue)
		{
			e.NewValue = ErrorValue;
			throw new PXSetPropertyException(message);
		}

		protected bool? VerifyExpr(PXCache cache, object data, Type whereType)
		{
			object value = null;
			bool? ret = null;
			IBqlWhere where = (IBqlWhere)Activator.CreateInstance(whereType);
			where.Verify(cache, data, new List<object>(), ref ret, ref value);

			return ret;
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXView view = sender.Graph.TypedViews.GetView(_Select, false);

			PXResult<INSite, INItemSite> res = (PXResult<INSite, INItemSite>)view.SelectSingleBound(new object[] { e.Row });
			if (res == null) return;
			INItemSite itemsite = res;
			bool IsStandardCostAdj = (bool)VerifyExpr(sender, e.Row, _IsStandardCostAdjType);

			if (IsStandardCostAdj)
			{
				e.NewValue = null;
				e.Cancel = true;
				return;
			}
			if (!UpdateDefault<INItemSite.dfltReceiptLocationID, INItemSite.dfltShipLocationID>(sender, e, itemsite))
				UpdateDefault<INSite.receiptLocationID, INSite.shipLocationID>(sender, e, (INSite) res);			
			
		}

		private bool UpdateDefault<ReceiptLocationID, ShipLocationID>(PXCache sender, PXFieldDefaultingEventArgs e,
		                                                              object source)
			where ReceiptLocationID : IBqlField
			where	ShipLocationID : IBqlField
		{
			if(source == null) return false;
			PXCache cache = sender.Graph.Caches[source.GetType()];

			if(cache.Keys.Exists(key => cache.GetValue(source, key) == null)) return false;				

			bool IsReceipt = (bool)VerifyExpr(sender, e.Row, _IsReceiptType);			
			
			object newvalue = (IsReceipt) ? cache.GetValue<ReceiptLocationID>(source) : cache.GetValue<ShipLocationID>(source);
			object val = (IsReceipt) ? cache.GetValueExt<ReceiptLocationID>(source) : cache.GetValueExt<ShipLocationID>(source);

			if (val is PXFieldState)
			{
				e.NewValue = ((PXFieldState)val).Value;
			}
			else
			{
				e.NewValue = val;
			}

			try
			{
				sender.RaiseFieldVerifying(_FieldName, e.Row, ref newvalue);
			}
			catch (PXSetPropertyException)
			{
				e.NewValue = null;
			}
			return true;
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INLocation location = null;
			try
			{
				location = (INLocation)PXSelectorAttribute.Select(sender, e.Row, _FieldName, e.NewValue);
			}
			catch (FormatException) { }

			if (location == null)
			{
				((PXDimensionSelectorAttribute)this.SelectorAttr).FieldVerifying(sender, e);
				return;
			}

			bool? IsReceipt = VerifyExpr(sender, e.Row, _IsReceiptType);
			bool? IsSales = VerifyExpr(sender, e.Row, _IsSalesType);
			bool? IsTransfer = VerifyExpr(sender, e.Row, _IsTransferType);

			if (IsReceipt == true || IsTransfer == true)
			{
				if (IsReceipt == true && location.ReceiptsValid == false)
				{
					ThrowErrorItem(Messages.LocationReceiptsInvalid, e, location.LocationCD);
				}

                Int32? ItemID;
                InventoryItem item;
				switch (location.PrimaryItemValid)
				{
					case INPrimaryItemValid.PrimaryItemError:
						ItemID = (Int32?)sender.GetValue(e.Row, _InventoryType.Name);

						if (ItemID != null && Equals(ItemID, location.PrimaryItemID) == false)
						{
							ThrowErrorItem(Messages.NotPrimaryLocation, e, location.LocationCD);
						}
						break;
					case INPrimaryItemValid.PrimaryItemClassError:
						item = (InventoryItem)PXSelectorAttribute.Select(sender, e.Row, _InventoryType.Name);

						if (item != null && string.Equals(item.ItemClassID, location.PrimaryItemClassID) == false)
						{
							ThrowErrorItem(Messages.NotPrimaryLocation, e, location.LocationCD);
						}
						break;
                    case INPrimaryItemValid.PrimaryItemWarning:
                        ItemID = (Int32?)sender.GetValue(e.Row, _InventoryType.Name);

                        if (ItemID != null && Equals(ItemID, location.PrimaryItemID) == false)
                        {
                            sender.RaiseExceptionHandling(_FieldName, e.Row, e.NewValue, new PXSetPropertyException(Messages.NotPrimaryLocation, PXErrorLevel.Warning));
                        }
                        break;
                    case INPrimaryItemValid.PrimaryItemClassWarning:
                        item = (InventoryItem)PXSelectorAttribute.Select(sender, e.Row, _InventoryType.Name);

                        if (item != null && string.Equals(item.ItemClassID, location.PrimaryItemClassID) == false)
                        {
                            sender.RaiseExceptionHandling(_FieldName, e.Row, e.NewValue, new PXSetPropertyException(Messages.NotPrimaryLocation, PXErrorLevel.Warning));
                        }
                        break;
					                                        
                    default:
						break;
				}
			}

			if (IsSales == true)
			{
				if (location.SalesValid == false)
				{
					ThrowErrorItem(Messages.LocationSalesInvalid, e, location.LocationCD);
				}
			}

			if (IsTransfer == true)
			{
				if (location.TransfersValid == false)
				{
					ThrowErrorItem(Messages.LocationTransfersInvalid, e, location.LocationCD);
				}
			}
		}
		#endregion
	}

	#endregion

	#region LocationAttribute

	[PXDBInt()]
	[PXUIField(DisplayName = "Location", Visibility = PXUIVisibility.Visible, FieldClass = LocationAttribute.DimensionName)]
	public class LocationAttribute : AcctSubAttribute
	{
		public const string DimensionName = "INLOCATION";

		public class dimensionName : Constant<string>
		{
			public dimensionName() : base(DimensionName) { ;}
		}

		protected Type _SiteIDType;

		protected bool _KeepEntry = true;

		public bool KeepEntry
		{
			get
			{
				return this._KeepEntry;
			}
			set
			{
				this._KeepEntry = value;
			}
		}

		public LocationAttribute()
			: base()
		{
			Type SearchType = typeof(Search<INLocation.locationID, Where<Match<Current<AccessInfo.userName>>>>);
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(INLocation.locationCD));
			//should ALWAYS be false, otherwise SiteID parameter will be ignored
			attr.CacheGlobal = false;
			attr.DescriptionField = typeof(INLocation.descr);
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public LocationAttribute(Type SiteIDType)
			: this()
		{
			_SiteIDType = SiteIDType;

			if (_SiteIDType != null)
			{
				Type SearchType = BqlCommand.Compose(
					typeof(Search<,>),
					typeof(INLocation.locationID),
					typeof(Where2<,>),
					typeof(Match<>),
					typeof(Current<AccessInfo.userName>),
					typeof(And<>),
					typeof(Where<,>),
					typeof(INLocation.siteID),
					typeof(Equal<>),
					typeof(Optional<>),
					_SiteIDType);
				PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(INLocation.locationCD));
				//should ALWAYS be false, otherwise SiteID parameter will be ignored
				attr.ValidComboRequired = true;
				attr.CacheGlobal = false;
				attr.DescriptionField = typeof(INLocation.descr);
				_Attributes[_SelAttrIndex] = attr;
			}
			else
			{
				throw new PXArgumentException("SiteID");
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			if (_SiteIDType != null && !PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>() && sender.Graph.GetType() != typeof(PXGraph))
			{
				((PXDimensionSelectorAttribute)this._Attributes[_Attributes.Count - 1]).ValidComboRequired = false;

				sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, Feature_FieldDefaulting);
				sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, Feature_FieldUpdating);
				sender.Graph.FieldUpdating.RemoveHandler(sender.GetItemType(), _FieldName, ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldUpdating);

				if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>() && sender.GetItemType() == typeof(IN.INSite))
				{
					_JustPersisted = new Dictionary<int?, int?>();
					sender.Graph.RowPersisting.AddHandler<INLocation>(Feature_RowPersisting);
					sender.Graph.RowPersisted.AddHandler<INLocation>(Feature_RowPersisted);
				}

				if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>() && !sender.Graph.Views.Caches.Contains(typeof(INSite)))
				{
					sender.Graph.Views.Caches.Add(typeof(INSite));
				}

				if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>() && !sender.Graph.Views.Caches.Contains(typeof(INLocation)))
				{
					sender.Graph.Views.Caches.Add(typeof(INLocation));
				}
			}

			base.CacheAttached(sender);

			if (_SiteIDType != null)
			{
				sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _SiteIDType.Name, SiteID_FieldUpdated);
				sender.Graph.FieldDefaulting.AddHandler<INLocation.siteID>(Location_SiteID_FieldDefaulting);
				sender.Graph.FieldVerifying.AddHandler<INLocation.siteID>(Location_SiteID_FieldVerifying);

				if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
				{
					string name = _FieldName.ToLower();
					sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), name, FieldUpdating);
					sender.Graph.FieldUpdating.RemoveHandler(sender.GetItemType(), name, ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldUpdating);

					sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), name, FieldSelecting);
					sender.Graph.FieldSelecting.RemoveHandler(sender.GetItemType(), name, ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).FieldSelecting);

					PXDimensionSelectorAttribute.SetValidCombo(sender, name, false);
					((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).ValidComboRequired = true;
				}
			}
		}

		protected Dictionary<Int32?, Int32?> _JustPersisted;

		public virtual void Feature_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			Int32? _KeyToAbort = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && _KeyToAbort < 0)
			{ 
				PXCache cache = sender.Graph.Caches[typeof(INSite)];
				INSite record = ((IEnumerable<INSite>)cache.Inserted).First();

				sender.SetValue(e.Row, _SiteIDType.Name, record.SiteID);

				_JustPersisted.Add(record.SiteID, _KeyToAbort);
			}
		}

		public virtual void Feature_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			Int32? _NewKey = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Aborted)
			{
				Int32? _KeyToAbort;
				if (_JustPersisted.TryGetValue(_NewKey, out _KeyToAbort))
				{
					sender.SetValue(e.Row, _SiteIDType.Name, _KeyToAbort);
				}
			}
		}

		protected object _current;

		public virtual void Feature_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			Int32? siteval = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);
			PXCache cache = sender.Graph.Caches[typeof(INSite)];
			if ((_current = (INSite)PXSelectReadonly<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(sender.Graph, siteval)) == null)
			{
				_current = object.ReferenceEquals(cache, sender) ? e.Row : siteval == null ? null : ((IEnumerable<INSite>)cache.Inserted).First(a => a.SiteID == siteval);
			}

			PXDimensionSelectorAttribute attr = ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]);

			try
			{
				attr.FieldUpdating(sender, e);
			}
			finally
			{
				_current = null;
			}
		}

		public virtual void Feature_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!e.Cancel)
			{
				Int32? siteval = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);
				object newValue = null;
				if (siteval != null)
				{

					if (!this.Definitions.DefaultLocations.TryGetValue(siteval, out newValue))
					{
						try
						{
							newValue = INLocation.Main;
							sender.RaiseFieldUpdating(_FieldName, e.Row, ref newValue);
						}
						catch (InvalidOperationException)
						{
						}
					}
				}
				e.NewValue = newValue;
				e.Cancel = true;
			}
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PXDimensionSelectorAttribute attr = ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]);
			attr.DirtyRead = true;
			attr.FieldSelecting(sender, e);

			if ((int?)sender.GetValue(e.Row, _FieldOrdinal) < 0)
			{
				Int32? siteval = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);
				PXCache cache = sender.Graph.Caches[typeof(INSite)];
				INSite site = (INSite)PXSelectReadonly<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(sender.Graph, siteval);

				if ((string)cache.GetValue<INSite.locationValid>(site) == "W")
				{
					sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyException(ErrorMessages.ElementDoesntExist, PXErrorLevel.Warning, Messages.Location));
				}
			}
		}

		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			Int32? siteval = (Int32?)sender.GetValue(e.Row, _SiteIDType.Name);
			PXCache cache = sender.Graph.Caches[typeof(INSite)];
			_current = (INSite)PXSelectReadonly<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(sender.Graph, siteval);
			PXDimensionSelectorAttribute attr = ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]);

			try
			{
				//keep valid combos for PXSegmentMask not to go crazy
				if ((_current ?? cache.Current) != null && (string)cache.GetValue<INSite.locationValid>(_current ?? cache.Current) != "V")
				{
					attr.ValidComboRequired = false;
				}

				attr.FieldUpdating(sender, e);

				if (e.NewValue is int && (int?)e.NewValue < 0 && (string)cache.GetValue<INSite.locationValid>(_current ?? cache.Current) == "W")
				{
					sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyException(ErrorMessages.ElementDoesntExist, PXErrorLevel.Warning, Messages.Location));
				}
			}
			finally
			{
				_current = null;
				attr.ValidComboRequired = true;
			}
		}

		public virtual void Location_SiteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INSite)];
			if ((_current ?? cache.Current) != null)
			{
				e.NewValue = cache.GetValue<INSite.siteID>(_current ?? cache.Current);
				e.Cancel = true;
			}
		}

		public virtual void Location_SiteID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INSite)];
			if ((_current ?? cache.Current) != null)
			{
				e.NewValue = cache.GetValue<INSite.siteID>(_current ?? cache.Current);
				e.Cancel = true;
			}
		}

		public virtual void SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_KeepEntry)
			{
				object val = sender.GetValueExt(e.Row, _FieldName);
				sender.SetValue(e.Row, _FieldOrdinal, null);

				try
				{
					if (val is PXFieldState)
					{
						if (((PXFieldState)val).Value != null)
						{
							sender.SetValueExt(e.Row, _FieldName, ((PXFieldState)val).Value);
						}
					}
					else
					{
						if (val != null)
						{
							sender.SetValueExt(e.Row, _FieldName, val);
						}
					}
				}
				catch (PXException) { }
			}
			else
			{
				sender.SetValue(e.Row, _FieldName, null);				
				//Just hack, for poper populating saved values in inquiry screens
				if(e.OldValue != null)
					sender.SetValuePending(e.Row, _FieldName, null);				 
			}
		}

		#region Default LocationID
		protected virtual Definition Definitions
		{
			get
			{
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("INLocation.Definition", typeof(INLocation)));
				}
				return defs;
			}
		}

		protected class Definition : IPrefetchable
		{
			public Dictionary<int?, object> DefaultLocations;

			public void Prefetch()
			{
				DefaultLocations = new Dictionary<int?, object>(); 
				
				foreach (PXDataRecord record in PXDatabase.SelectMulti<INLocation>(
					new PXDataField<INLocation.siteID>(),
					new PXDataField<INLocation.locationID>(),
					new PXDataFieldOrder<INLocation.siteID>(),
					new PXDataFieldOrder<INLocation.locationID>()))
				{
					int? siteID = record.GetInt32(0);

					if (!DefaultLocations.ContainsKey(siteID))
					{
						DefaultLocations.Add(siteID, record.GetInt32(1));
					}
				}
			}
		}
		#endregion
	}

	#endregion

	#region PXDBPriceCostAttribute

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
	public class PXDBPriceCostAttribute : PXDBDecimalAttribute
	{
		#region Implementation
		public static decimal Round(PXCache sender, decimal value)
		{
			return (decimal)Math.Round(value, INSetupDecPl.PrcCst, MidpointRounding.AwayFromZero);
		}

		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			base.RowSelecting(sender, e);

			if (sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, 0m);
			}
		}
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_Precision = INSetupDecPl.PrcCst;
		}
		#endregion
	}

	#endregion

	#region PXDBPriceCostCalced
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
	public class PXDBPriceCostCalcedAttribute : PXDBCalcedAttribute
	{
		#region Ctor
		public PXDBPriceCostCalcedAttribute(Type operand, Type type)
			: base(operand, type)
		{
		}
		#endregion
		#region Implementation
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			base.RowSelecting(sender, e);

			if (sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, 0m);
			}
		}
		#endregion
	}
	#endregion

	#region PXPriceCostAttribute

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
	public class PXPriceCostAttribute : PXDecimalAttribute
	{
		#region Implementation
		public static decimal Round(PXCache sender, decimal value)
		{
			return (decimal)Math.Round(value, INSetupDecPl.PrcCst, MidpointRounding.AwayFromZero);
		}
		#endregion
		#region Static methods
		public static Decimal MinPrice(InventoryItem ii, INItemCost cost)
		{					
			if (ii.ValMethod != INValMethod.Standard)
				{
					return (cost.AvgCost ?? 0) + ((cost.AvgCost ?? 0) * 0.01m * (ii.MinGrossProfitPct ?? 0));
				}
				else
				{
					return (ii.StdCost ?? 0) + ((ii.StdCost ?? 0) * 0.01m * (ii.MinGrossProfitPct ?? 0));
				}
		}
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_Precision = INSetupDecPl.PrcCst;
		}
		#endregion
	}

	#endregion

	#region PXQuantityAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
	public class PXQuantityAttribute : PXDecimalAttribute, IPXFieldVerifyingSubscriber, IPXRowInsertingSubscriber, IPXRowPersistingSubscriber
	{
		#region State
		protected int _ResultOrdinal;
		protected int _KeyOrdinal;
		protected Type _KeyField = null;
		protected Type _ResultField = null;
		protected bool _HandleEmptyKey = false; 


		#endregion

		#region Ctor
		public PXQuantityAttribute()
		{
		}

		public PXQuantityAttribute(Type keyField, Type resultField)
		{
			_KeyField = keyField;
			_ResultField = resultField;
		}

		public bool HandleEmptyKey
		{
			set { this._HandleEmptyKey = value; }
			get { return this._HandleEmptyKey; }
		}
		#endregion

		#region Runtime
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_Precision = INSetupDecPl.Qty;

			if (_ResultField != null)
			{
				_ResultOrdinal = sender.GetFieldOrdinal(_ResultField.Name);
			}

			if (_KeyField != null)
			{
				_KeyOrdinal = sender.GetFieldOrdinal(_KeyField.Name);
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_KeyField), _KeyField.Name, KeyFieldUpdated);
			}
		}
		#endregion

		#region Implementation
		internal object Select(PXCache cache, object data)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, _KeyField.Name))
			{
				if (attr is INUnitAttribute)
				{
					object value = cache.GetValue(data, _KeyOrdinal);
					return PXSelectorAttribute.GetItem(cache, (PXSelectorAttribute)((INUnitAttribute)attr).SelectorAttr, data, value);
				}
			}
			return null;
		}

		protected virtual void CalcBaseQty(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			decimal? resultval = null;

			if (_ResultField != null)
			{
				if (e.NewValue != null)
				{
					bool handled = false;
					if (this._HandleEmptyKey)
					{
						object value = sender.GetValue(e.Row, _KeyField.Name);
						if (String.IsNullOrEmpty((String)value))
						{
							resultval = (decimal)e.NewValue;
							handled = true;
						}
					}
					if (!handled)
					{
						INUnit conv = (INUnit)Select(sender, e.Row);
						if (conv != null && conv.UnitRate != 0m)
						{
							_ensurePrecision(sender, e.Row);
							resultval = Math.Round((decimal)e.NewValue * (conv.UnitMultDiv == "M" ? (decimal)conv.UnitRate : 1 / (decimal)conv.UnitRate), (int)_Precision, MidpointRounding.AwayFromZero);
						}

						if (conv == null && !e.ExternalCall)
						{
							throw new PXUnitConversionException((string)sender.GetValue(e.Row, _KeyField.Name));
						}
					}
				}
				sender.SetValue(e.Row, _ResultOrdinal, resultval);
			}
		}

		protected virtual void CalcBaseQty(PXCache sender, object data)
		{
			object NewValue = sender.GetValue(data, _FieldOrdinal);
			try
			{
				CalcBaseQty(sender, new PXFieldVerifyingEventArgs(data, NewValue, false));
			}
			catch (PXUnitConversionException)
			{
				sender.SetValue(data, _ResultField.Name, null);
			}
		}

		protected virtual void CalcTranQty(PXCache sender, object data)
		{
			decimal? resultval = null;

			if (_ResultField != null)
			{
				object NewValue = sender.GetValue(data, _ResultOrdinal);

				if (NewValue != null)
				{
					INUnit conv = (INUnit)Select(sender, data);

					if (conv != null && conv.UnitRate != 0m)
					{
						_ensurePrecision(sender, data);
						resultval = Math.Round((decimal)NewValue * (conv.UnitMultDiv == "M" ? 1 / (decimal)conv.UnitRate : (decimal)conv.UnitRate), (int)_Precision, MidpointRounding.AwayFromZero);
					}
				}
				sender.SetValue(data, _FieldOrdinal, resultval);
			}
		}

		public static void CalcBaseQty<TField>(PXCache cache, object data)
			where TField : class, IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<TField>(data))
			{
				if (attr is PXDBQuantityAttribute)
				{
					((PXQuantityAttribute)attr).CalcBaseQty(cache, data);
					break;
				}
			}
		}

		public static void CalcTranQty<TField>(PXCache cache, object data)
			where TField : class, IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<TField>(data))
			{
				if (attr is PXDBQuantityAttribute)
				{
					((PXQuantityAttribute)attr).CalcTranQty(cache, data);
					break;
				}
			}
		}

		public virtual void KeyFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcBaseQty(sender, e.Row);
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object NewValue = sender.GetValue(e.Row, _FieldOrdinal);
			CalcBaseQty(sender, new PXFieldVerifyingEventArgs(e.Row, NewValue, false));
		}

		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CalcBaseQty(sender, e.Row);
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CalcBaseQty(sender, e);
		}
		#endregion
	}

	#endregion

	#region PXDBBaseQuantityAttribute

	public class PXDBBaseQuantityAttribute : PXDBQuantityAttribute
	{
		internal override object Select(PXCache cache, object data)
		{
			INUnit ret = (INUnit)base.Select(cache, data);

			if (ret != null)
			{
				ret = PXCache<INUnit>.CreateCopy(ret);
				ret.UnitMultDiv = (ret.UnitMultDiv == "M") ? "D" : "M";
			}
			return ret;
		}

		public PXDBBaseQuantityAttribute()
			: base()
		{
		}

		public PXDBBaseQuantityAttribute(Type keyField, Type resultField)
			: base(keyField, resultField)
		{
		}
	}

	#endregion

	#region PXDBQuantityAttribute

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class)]
	public class PXDBQuantityAttribute : PXDBDecimalAttribute, IPXFieldVerifyingSubscriber, IPXRowInsertingSubscriber
	{
		#region State
		protected int _ResultOrdinal;
		protected int _KeyOrdinal;
		protected Type _KeyField = null;
		protected Type _ResultField = null;
		protected bool _HandleEmptyKey = false;
		protected int? _OverridePrecision = null;

		public Type KeyField
		{
			get
			{
				return _KeyField;
			}
		}

		#endregion

		#region Ctor
		public PXDBQuantityAttribute()
		{
		}

		public PXDBQuantityAttribute(Type keyField, Type resultField)
		{
			_KeyField = keyField;
			_ResultField = resultField;
		}

		public PXDBQuantityAttribute(int precision, Type keyField, Type resultField)
		{
			_OverridePrecision = precision;
			_KeyField = keyField;
			_ResultField = resultField;
		}

		public bool HandleEmptyKey
		{
			set { this._HandleEmptyKey = value; }
			get { return this._HandleEmptyKey; }
		}

		#endregion

		#region Runtime
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_Precision = INSetupDecPl.Qty;

			if (_OverridePrecision != null)
				_Precision = _OverridePrecision.Value;

			if (_ResultField != null)
			{
				_ResultOrdinal = sender.GetFieldOrdinal(_ResultField.Name);
			}

			if (_KeyField != null)
			{
				_KeyOrdinal = sender.GetFieldOrdinal(_KeyField.Name);
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_KeyField), _KeyField.Name, KeyFieldUpdated);
			}
		}
		#endregion

		#region Implementation
		internal virtual object Select(PXCache cache, object data)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, _KeyField.Name))
			{
				if (attr is INUnitAttribute)
				{
					object value = cache.GetValue(data, _KeyField.Name);
					return PXSelectorAttribute.GetItem(cache, (PXSelectorAttribute)((INUnitAttribute)attr).SelectorAttr, data, value);
				}
			}
			return null;
		}

		protected virtual void CalcBaseQty(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			decimal? resultval = null;

			if (_ResultField != null)
			{
				if (e.NewValue != null)
				{
					bool handled = false;
					if (this._HandleEmptyKey)
					{
						object value = sender.GetValue(e.Row, _KeyField.Name);
						if (String.IsNullOrEmpty((String)value))
						{
							resultval = (decimal)e.NewValue;
							handled = true;
						}
					}
					if (!handled)
					{
						INUnit conv = (INUnit)Select(sender, e.Row);

						if (conv != null && conv.UnitRate != 0m)
						{
							_ensurePrecision(sender, e.Row);
							resultval = Math.Round((decimal)e.NewValue * (conv.UnitMultDiv == "M" ? (decimal)conv.UnitRate : 1 / (decimal)conv.UnitRate), (int)_Precision, MidpointRounding.AwayFromZero);
						}

						if (conv == null && !e.ExternalCall)
						{
							throw new PXUnitConversionException((string)sender.GetValue(e.Row, _KeyField.Name)); 
						}
					}
				}
				if (e.ExternalCall)
				{
					sender.SetValueExt(e.Row, this._ResultField.Name, resultval);
				}
				else
				{
					sender.SetValue(e.Row, this._ResultField.Name, resultval);
				}
			}
		}

		protected virtual void CalcBaseQty(PXCache sender, object data)
		{
			object NewValue = sender.GetValue(data, _FieldOrdinal);
			try
			{
				CalcBaseQty(sender, new PXFieldVerifyingEventArgs(data, NewValue, false));
			}
			catch (PXUnitConversionException)
			{
				sender.SetValue(data, _ResultField.Name, null);
			}
		}

		protected virtual void CalcTranQty(PXCache sender, object data)
		{
			decimal? resultval = null;

			if (_ResultField != null)
			{
				object NewValue = sender.GetValue(data, _ResultOrdinal);

				if (NewValue != null)
				{
					INUnit conv = (INUnit)Select(sender, data);

					if (conv != null && conv.UnitRate != 0m)
					{
						_ensurePrecision(sender, data);
						resultval = Math.Round((decimal)NewValue * (conv.UnitMultDiv == "M" ? 1 / (decimal)conv.UnitRate : (decimal)conv.UnitRate), (int)_Precision, MidpointRounding.AwayFromZero);
					}
				}
				sender.SetValue(data, _FieldOrdinal, resultval);
			}
		}

		public static void CalcBaseQty<TField>(PXCache cache, object data)
			where TField : class, IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<TField>(data))
			{
				if (attr is PXDBQuantityAttribute)
				{
					((PXDBQuantityAttribute)attr).CalcBaseQty(cache, data);
					break;
				}
			}
		}

		public static void CalcTranQty<TField>(PXCache cache, object data)
			where TField : class, IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<TField>(data))
			{
				if (attr is PXDBQuantityAttribute)
				{
					((PXDBQuantityAttribute)attr).CalcTranQty(cache, data);
					break;
				}
			}
		}

		public static decimal Round(PXGraph graph, decimal value)
		{
			return (decimal)Math.Round(value, INSetupDecPl.Qty, MidpointRounding.AwayFromZero);
		}

		public static decimal Round(PXCache sender, decimal value)
		{
			return Round(sender.Graph, value);
		}

		public virtual void KeyFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcBaseQty(sender, e.Row);
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object NewValue = sender.GetValue(e.Row, _FieldOrdinal);
			CalcBaseQty(sender, new PXFieldVerifyingEventArgs(e.Row, NewValue, false));
		}

		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CalcBaseQty(sender, e.Row);
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PXFieldUpdatingEventArgs args = new PXFieldUpdatingEventArgs(e.Row, e.NewValue);
			if (!e.ExternalCall)
			{
				base.FieldUpdating(sender, args);
			}
			CalcBaseQty(sender, new PXFieldVerifyingEventArgs(args.Row, args.NewValue, true));
			e.NewValue = args.NewValue;
		}
		#endregion
	}

	#endregion

	#region PXDBForeignIdentityAttribute

	public class PXDBForeignIdentityAttribute : PXDBIdentityAttribute, IPXRowPersistingSubscriber
	{
		Type _ForeignIdentity = null;

		public PXDBForeignIdentityAttribute(Type ForeignIdentity)
		{
			_ForeignIdentity = ForeignIdentity;
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				try
				{
					PXDatabase.Insert(_ForeignIdentity, PXDataFieldAssign.OperationSwitchAllowed);
				}
				catch (PXDatabaseException ex)
				{
					if (ex.ErrorCode == PXDbExceptions.OperationSwitchRequired)
					{
						PXDatabase.Update(_ForeignIdentity);
					}
					else
					{
						throw;
					}
				}
				_KeyToAbort = (int?)sender.GetValue(e.Row, _FieldOrdinal);
				sender.SetValue(e.Row, _FieldOrdinal, Convert.ToInt32(PXDatabase.SelectIdentity()));
			}
		}

		protected override int getLastInsertedIdentity()
		{
			return 0; // these tables never have autoincrement fields, so no use to look for identity there
		}

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (_DatabaseFieldName != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName;
				//e.FieldName = (e.Table == null ? _DatabaseFieldName : e.Table.Name + '.' + _DatabaseFieldName);
			}
			e.DataType = PXDbType.Int;
			e.DataValue = e.Value;
			e.DataLength = 4;
			e.IsRestriction = e.IsRestriction || _IsKey;
		}
	}

	#endregion

	#region PXSetupOptional

	public class PXSetupOptional<Table> : PXSelectReadonly<Table>
		where Table : class, IBqlTable, new()
	{
		protected Table _Record;
		public PXSetupOptional(PXGraph graph)
			: base(graph)
		{
			graph.Defaults[typeof(Table)] = getRecord;
		}
		private object getRecord()
		{
			if (_Record == null)
			{
				_Record = base.Select();
				if (_Record == null)
				{
					_Record = new Table();
					PXCache cache = this._Graph.Caches[typeof(Table)];
					foreach (Type field in cache.BqlFields)
					{
						object newvalue;
						cache.RaiseFieldDefaulting(field.Name.ToLower(), _Record, out newvalue);
						cache.SetValue(_Record, field.Name.ToLower(), newvalue);
					}
					base.StoreCached(new PXCommandKey(new object[] { }), new List<object>{ _Record });
				}
			}
			return _Record;
		}
	}

	#endregion

	#region PXCalcQuantityAttribute
	public class PXCalcQuantityAttribute : PXDecimalAttribute
	{
		#region State
		protected int _SourceOrdinal;
		protected int _KeyOrdinal;
		protected Type _KeyField = null;
		protected Type _SourceField = null;

		#endregion

		#region Ctor
		public PXCalcQuantityAttribute()
		{			
		}

		public PXCalcQuantityAttribute(Type keyField, Type sourceField)
		{
			_KeyField = keyField;
			_SourceField = sourceField;
		}
		#endregion

		#region Runtime
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_Precision = INSetupDecPl.Qty;

			sender.Graph.RowSelected.AddHandler(sender.GetItemType(), RowSelected);				

			if (_SourceField != null)
			{
				_SourceOrdinal = sender.GetFieldOrdinal(_SourceField.Name);
			}

			if (_KeyField != null)
			{
				_KeyOrdinal = sender.GetFieldOrdinal(_KeyField.Name);
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_KeyField), _KeyField.Name, KeyFieldUpdated);
			}
		}
		#endregion

		#region Implementation
		internal object Select(PXCache cache, object data)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, _KeyField.Name))
			{
				if (attr is INUnitAttribute)
				{
					object value = cache.GetValue(data, _KeyOrdinal);
					return PXSelectorAttribute.GetItem(cache, (PXSelectorAttribute)((INUnitAttribute)attr).SelectorAttr, data, value);
				}
			}
			return null;
		}

		protected virtual void CalcTranQty(PXCache sender, object data)
		{
			decimal? resultval = null;

			if (_SourceField != null)
			{
				object NewValue = sender.GetValue(data, _SourceOrdinal);

				if (NewValue != null)
				{
					INUnit conv = (INUnit)Select(sender, data);

					if (conv != null && conv.UnitRate != 0m)
					{
						_ensurePrecision(sender, data);
						resultval = Math.Round((decimal)NewValue * (conv.UnitMultDiv == "M" ? 1 / (decimal)conv.UnitRate : (decimal)conv.UnitRate), (int)_Precision, MidpointRounding.AwayFromZero);
					}
				}
			}
			sender.SetValue(data, _FieldOrdinal, resultval ?? 0m);
		}

		public virtual void KeyFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcTranQty(sender, e.Row);
		}
		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (sender.GetValue(e.Row, _FieldOrdinal) == null)
				CalcTranQty(sender, e.Row);
		}
		#endregion
	}
	#endregion

	#region INSiteStatusLookup
	public class INSiteStatusLookup<Status, StatusFilter> : PXSelectBase<Status>
		where Status : class, IBqlTable, new()
		where StatusFilter : class, IBqlTable, new()
	{
		public const string Selected = "Selected";
		public const string QtySelected = "QtySelected";
		private PXView intView;
		#region Ctor
		public INSiteStatusLookup(PXGraph graph)
		{
			this.View = new PXView(graph, false, 
				BqlCommand.CreateInstance(BqlCommand.Compose(typeof(Select<>), typeof(Status))), 
				new PXSelectDelegate(viewHandler));
			InitHandlers(graph);
		}

		public INSiteStatusLookup(PXGraph graph, Delegate handler)
		{
			this.View = new PXView(graph, false,
				BqlCommand.CreateInstance(BqlCommand.Compose(typeof(Select<>), typeof(Status))),
				handler);			
			InitHandlers(graph);
		}
		#endregion

		#region Implementations

		private void InitHandlers(PXGraph graph)
		{
			graph.RowSelected.AddHandler(typeof(StatusFilter), OnFilterSelected);
			graph.RowSelected.AddHandler(typeof(Status), OnRowSelected);
			graph.FieldUpdated.AddHandler(typeof(Status), Selected, OnSelectedUpdated);
		}

		protected virtual void OnFilterSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INSiteStatusFilter row = e.Row as INSiteStatusFilter;
			if (row != null)
				PXUIFieldAttribute.SetVisible(sender.Graph.Caches[typeof(Status)], typeof(INSiteStatus.siteID).Name, row.SiteID == null);
		}

		
		protected virtual void OnSelectedUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			bool? selected = (bool?)sender.GetValue(e.Row, Selected);
			decimal? qty = (decimal?)sender.GetValue(e.Row, QtySelected);
			if (selected == true)
			{
				if(qty == null || qty == 0m)
					sender.SetValue(e.Row, QtySelected, 1m);
			}
			else
				sender.SetValue(e.Row, QtySelected, 0m);
		}

		protected virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, Selected, true);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, QtySelected, true);
		}	

		protected virtual PXView CreateIntView(PXGraph graph)
		{
			PXCache cache = graph.Caches[typeof(Status)];

			List<Type> select = new List<Type>();
			select.Add(typeof(Select4<,,>));
			select.Add(typeof(Status));
			select.Add(CreateWhere(graph));
			select.Add(typeof(Aggregate<>));

			List<Type> groupFields = cache.BqlKeys;
			groupFields.AddRange(cache.BqlFields.Where(field => field.IsDefined(typeof (PXExtraKeyAttribute), false)));			

			for (int i = 0; i < groupFields.Count; i++)
			{
				select.Add((i != groupFields.Count - 1) ? typeof(GroupBy<,>) : typeof(GroupBy<>));
				select.Add(groupFields[i]);
			}

			Type selectType = BqlCommand.Compose(select.ToArray());

			return new PXView(graph, true, BqlCommand.CreateInstance(selectType));
		}

		protected virtual IEnumerable viewHandler()
		{
			if (intView == null) intView = CreateIntView(this.View.Graph);
			var startRow = PXView.StartRow;
			var totalRows = 0;			
			foreach (Status item in intView.Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
								 ref startRow, PXView.MaximumRows, ref totalRows))
			{
				Status result = item;
				Status updated = this.Cache.Locate(item) as Status;
				if (updated != null && this.Cache.GetValue(updated, Selected) as bool? == true)
				{
					Decimal? qty = this.Cache.GetValue(updated, QtySelected) as Decimal?;
					this.Cache.RestoreCopy(updated,item);
					this.Cache.SetValue(updated, Selected, true);
					this.Cache.SetValue(updated, QtySelected, qty);
					result = updated;
				}
				yield return result;
			}
			PXView.StartRow = 0;
		}

		protected static Type CreateWhere(PXGraph graph)
		{
			PXCache filter = graph.Caches[typeof(INSiteStatusFilter)];
			PXCache cache = graph.Caches[typeof(Status)];

			Type where = typeof(Where<boolTrue, Equal<boolTrue>>);
			foreach (string field in filter.Fields)
			{
				if (cache.Fields.Contains(field))
				{
					if (filter.Fields.Contains(field + "Wildcard")) continue;
					Type sourceType = filter.GetBqlField(field);
					Type destinationType = cache.GetBqlField(field);
					if (sourceType != null && destinationType != null)
					{
						where = BqlCommand.Compose(
							typeof(Where2<,>),
							typeof(Where<,,>),
							typeof(Current<>), sourceType, typeof(IsNull),
							typeof(Or<,>), destinationType, typeof(Equal<>), typeof(Current<>), sourceType,
							typeof(And<>), where
						);
					}
				}
				string f;
				if (field.Length > 8 && field.EndsWith("Wildcard") && cache.Fields.Contains(f = field.Substring(0, field.Length - 8)))
				{
					Type like = filter.GetBqlField(field);
					Type dest = cache.GetBqlField(f);
					where = BqlCommand.Compose(
						typeof(Where2<,>),
						typeof(Where<,,>), typeof(Current<>), like, typeof(IsNull),
						typeof(Or<,>), dest, typeof(Like<>), typeof(Current<>), like,
						typeof(And<>), where
						);
				}
			}		
			return where;
		}

		protected static Type GetTypeField<Source>(string field)
		{
			Type sourceType = typeof(Source);
			Type fieldType = null;
			while (fieldType == null && sourceType != null)
			{
				fieldType = sourceType.GetNestedType(field, System.Reflection.BindingFlags.Public);
				sourceType = sourceType.BaseType;
			}
			return fieldType;
		}

		private class Zero : Constant<Decimal>
		{
			public Zero() : base(0m) { }
		}
		#endregion
	}
	#endregion

	#region INBarCodeItemLookup
	public class INBarCodeItemLookup<Filter> : PXFilter<Filter>
		where Filter : INBarCodeItem, new()
	{
		#region Ctor
		public INBarCodeItemLookup(PXGraph graph)
			:base(graph)
		{
			InitHandlers(graph);
		}

		public INBarCodeItemLookup(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
			InitHandlers(graph);
		}
		#endregion

		private void InitHandlers(PXGraph graph)
		{
			graph.RowSelected.AddHandler(typeof(Filter), OnFilterSelected);

			graph.FieldUpdated.AddHandler(typeof(Filter), typeof(INBarCodeItem.barCode).Name, Filter_BarCode_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(Filter), typeof(INBarCodeItem.inventoryID).Name, Filter_InventoryID_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(Filter), typeof(INBarCodeItem.byOne).Name, Filter_ByOne_FieldUpdated);					
		}

		public virtual void Reset(bool keepDescription)
		{
			Filter s = this.Current;
			this.Cache.Remove(s);
			this.Cache.Insert(this.Cache.CreateInstance());
			this.Current.ByOne = s.ByOne;
			this.Current.AutoAddLine = s.AutoAddLine;		
			if(keepDescription)
				this.Current.Description = s.Description;							
		}
		
		#region Filter Events
		protected virtual void Filter_BarCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.ExternalCall)
			{
				var rec = (PXResult<INItemXRef, InventoryItem, INSubItem>)
									PXSelectJoin<INItemXRef,
										InnerJoin<InventoryItem,
														On<InventoryItem.inventoryID, Equal<INItemXRef.inventoryID>,
														And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
														And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noPurchases>,
														And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noRequest>,
														And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>>>>>>,
										InnerJoin<INSubItem,
													 On<INSubItem.subItemID, Equal<INItemXRef.subItemID>>>>,
										Where<INItemXRef.alternateID, Equal<Current<INBarCodeItem.barCode>>,
											And<INItemXRef.alternateType, Equal<INAlternateType.barcode>>>>
										.SelectSingleBound(this._Graph, new object[] { e.Row });
				if (rec != null)
				{
					sender.SetValue<INBarCodeItem.inventoryID>(e.Row, null);
					sender.SetValuePending<INBarCodeItem.inventoryID>(e.Row, ((InventoryItem)rec).InventoryCD);
					sender.SetValuePending<INBarCodeItem.subItemID>(e.Row, ((INSubItem)rec).SubItemCD);
				}
				else
				{
					sender.SetValuePending<INBarCodeItem.inventoryID>(e.Row, null);
					sender.SetValuePending<INBarCodeItem.subItemID>(e.Row, null);
				}
			}
		}
		protected virtual void Filter_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.ExternalCall)
			{
				Filter row = e.Row as Filter;
				if (e.OldValue != null && row.InventoryID != null)
					row.BarCode = null;
				sender.SetDefaultExt<INBarCodeItem.subItemID>(e);
				sender.SetDefaultExt<INBarCodeItem.qty>(e);
			}
		}
		protected virtual void Filter_ByOne_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if(e.ExternalCall)
			{
				Filter row = e.Row as Filter;				
				if (row != null && row.ByOne == true)
					row.Qty = 1m;
			}
		}
		
		protected virtual void OnFilterSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Filter row = e.Row as Filter;
			if (row != null)
			{
				INLotSerClass lotclass =
				PXSelectJoin<INLotSerClass,
					InnerJoin<InventoryItem, On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>>,
				Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
				.SelectWindowed(this._Graph, 0, 1, row.InventoryID);

				bool requestLotSer = lotclass != null && lotclass.LotSerTrack != INLotSerTrack.NotNumbered &&
														 lotclass.LotSerAssign == INLotSerAssign.WhenReceived;

				PXUIFieldAttribute.SetEnabled<INBarCodeItem.lotSerialNbr>(sender, null, requestLotSer);
				PXDefaultAttribute.SetPersistingCheck<INBarCodeItem.lotSerialNbr>(sender, null, requestLotSer ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXUIFieldAttribute.SetEnabled<INBarCodeItem.expireDate>(sender, null, requestLotSer && lotclass.LotSerTrackExpiration == true);
				PXDefaultAttribute.SetPersistingCheck<INBarCodeItem.expireDate>(sender, null, requestLotSer && lotclass.LotSerTrackExpiration == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXUIFieldAttribute.SetEnabled<INBarCodeItem.uOM>(sender, null, !(requestLotSer && lotclass.LotSerTrack == INLotSerTrack.SerialNumbered) && row.ByOne != true && row.InventoryID != null);
				PXUIFieldAttribute.SetEnabled<INBarCodeItem.inventoryID>(sender, null, (string.IsNullOrEmpty(row.BarCode) || row.InventoryID == null));
			}
		}
		#endregion
	}
	#endregion

	#region INOpenPeriod
	public class INOpenPeriodAttribute : OpenPeriodAttribute
	{
		#region Ctor
		public INOpenPeriodAttribute(Type SourceType)
			: base(typeof(Search<FinPeriod.finPeriodID, Where<FinPeriod.iNClosed, Equal<False>, And<FinPeriod.active, Equal<True>>>>), SourceType)
		{
		}

		public INOpenPeriodAttribute()
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
				FinPeriod p = (FinPeriod)PXSelectReadonly<FinPeriod, Where<FinPeriod.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>>>.Select(sender.Graph, (string)NewValue);
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
	/// FinPeriod selector that extends <see cref="FinPeriodSelectorAttribute"/>. 
	/// Displays and accepts only Closed Fin Periods. 
	/// When Date is supplied through aSourceType parameter FinPeriod is defaulted with the FinPeriod for the given date.
	/// </summary>
	public class INClosedPeriodAttribute : FinPeriodSelectorAttribute
	{
		public INClosedPeriodAttribute(Type aSourceType)
			: base(typeof(Search<FinPeriod.finPeriodID, Where<FinPeriod.closed, Equal<True>, Or<FinPeriod.iNClosed, Equal<True>, Or<FinPeriod.active, Equal<True>>>>, OrderBy<Desc<FinPeriod.finPeriodID>>>))
		{
		}

		public INClosedPeriodAttribute()
			: this(null)
		{

		}
	}
	
	#region SubItemStatusVeryfier
	public class SubItemStatusVeryfierAttribute : PXEventSubscriberAttribute
	{
		protected readonly Type inventoryID;
		protected readonly Type siteID;
		protected readonly string[] statusrestricted;

		public SubItemStatusVeryfierAttribute(Type inventoryID, Type siteID, params string[] statusrestricted)
		{
			this.inventoryID = inventoryID;
			this.siteID = siteID;
			this.statusrestricted = statusrestricted;
		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler(sender.GetItemType(), _FieldName, OnSubItemFieldVerifying);
			sender.Graph.FieldVerifying.AddHandler(sender.GetItemType(), siteID.Name, OnSiteFieldVerifying);
		}
		
		protected virtual void OnSubItemFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{			
			if(!Validate(sender, 
				(int?)sender.GetValue(e.Row, inventoryID.Name),
				(int?)e.NewValue,
				(int?)sender.GetValue(e.Row, siteID.Name)))
				throw new PXSetPropertyException(Messages.RestictedSubItem);
		}

		protected virtual void OnSiteFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{			
			if (!Validate(sender,
				(int?)sender.GetValue(e.Row, inventoryID.Name),
				(int?)sender.GetValue(e.Row, _FieldName),
				(int?)e.NewValue))
				throw new PXSetPropertyException(Messages.RestictedSubItem);			
		}

		private bool Validate(PXCache sender, int? invetroyID, int? subitemID, int? siteID)
		{
			INItemSiteReplenishment settings =
				PXSelect<INItemSiteReplenishment,
					Where<INItemSiteReplenishment.inventoryID, Equal<Required<INItemSiteReplenishment.inventoryID>>,
						And<INItemSiteReplenishment.subItemID, Equal<Required<INItemSiteReplenishment.subItemID>>,
							And<INItemSiteReplenishment.siteID, Equal<Required<INItemSiteReplenishment.siteID>>>>>>.SelectWindowed(
								sender.Graph, 0, 1, invetroyID, subitemID, siteID);
			if(settings != null)
				foreach (string status in statusrestricted)
				{
					if(status == settings.ItemStatus) return false;
				}
			return true;
		}
	}
	#endregion

	#region PXSelectorWithoutVerificationAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class PXSelectorWithoutVerificationAttribute : PXSelectorAttribute
	{
		public PXSelectorWithoutVerificationAttribute(Type type) : base(type)
		{
		}

		public PXSelectorWithoutVerificationAttribute(Type type, params Type[] fieldList) : base(type, fieldList)
		{
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			//base.FieldVerifying(sender, e);
		}
	}

	#endregion

	#region INRegisterCacheNameAttribute

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class INRegisterCacheNameAttribute : PX.Data.PXCacheNameAttribute
	{
		public INRegisterCacheNameAttribute(string name)
			: base(name)
		{
		}

		public override string GetName(object row)
		{
			var register = row as INRegister;
			if (register == null) return base.GetName();

			var result = Messages.Receipt;
			switch (register.DocType)
			{
				case INDocType.Issue:
					result = Messages.Issue;
					break;
				case INDocType.Transfer:
					result = Messages.Transfer;
					break;
				case INDocType.Adjustment:
					result = Messages.Adjustment;
					break;
				case INDocType.Production:
					result = Messages.Production;
					break;
				case INDocType.Disassembly:
					result = Messages.Disassembly;
					break;
			}
			return result;
		}
	}

	#endregion


	public class INSubItemSegmentValueList : 
		PXSelect<INSubItemSegmentValue, Where<INSubItemSegmentValue.inventoryID, Equal<Current<InventoryItem.inventoryID>>>>		
	{
		public class SValue : IBqlTable
		{
			#region SegmentID
			public abstract class segmentID : PX.Data.IBqlField
			{
			}
			protected Int16? _SegmentID;
			[PXDBShort(IsKey = true)]			
			[PXUIField(DisplayName = "Segment ID", Visibility = PXUIVisibility.Invisible, Visible = false)]
			public virtual Int16? SegmentID
			{
				get
				{
					return this._SegmentID;
				}
				set
				{
					this._SegmentID = value;
				}
			}
			#endregion
			#region Value
			public abstract class value : PX.Data.IBqlField
			{
			}
			protected String _Value;
			[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
			[PXDefault()]
			[PXUIField(DisplayName = "Value", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]			
			public virtual String Value
			{
				get
				{
					return this._Value;
				}
				set
				{
					this._Value = value;
				}
			}
			#endregion
			#region Descr
			public abstract class descr : PX.Data.IBqlField
			{
			}
			protected String _Descr;
			[PXDBString(60, IsUnicode = true)]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual String Descr
			{
				get
				{
					return this._Descr;
				}
				set
				{
					this._Descr = value;
				}
			}
			#endregion
			#region Active
			public abstract class active : PX.Data.IBqlField
			{
			}
			protected Boolean? _Active;
			[PXDBBool()]
			[PXDefault((bool)false)]
			[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.Visible)]
			public virtual Boolean? Active
			{
				get
				{
					return this._Active;
				}
				set
				{
					this._Active = value;
				}
			}
			#endregion			
		}
		public INSubItemSegmentValueList(PXGraph graph)
			:base(graph)
		{
			graph.Caches[typeof (SValue)].AllowInsert = graph.Caches[typeof (SValue)].AllowDelete = false;
			if (!PXAccess.FeatureInstalled<FeaturesSet.subItem>())
			{
				this.AllowSelect = false;
				return;
			}

			foreach (Segment s in PXSelect<Segment,
				Where<Segment.dimensionID, Equal<SubItemAttribute.dimensionName>>,
				OrderBy<Asc<Segment.segmentID>>>.Select(graph))
			{
				int? segmentID = s.SegmentID;
				graph.Views.Add("DimensionsSubItem",
					new PXView(graph, false, BqlCommand.CreateInstance(typeof(Select<Segment, 
						Where<Segment.dimensionID, Equal<SubItemAttribute.dimensionName>>,
						OrderBy<Asc<Segment.segmentID>>>)))
					);
				graph.Views.Add("SubItem_" + s.SegmentID, new PXView(graph, false,
					BqlCommand.CreateInstance(typeof (Select<SValue>)),
					(PXSelectDelegate) delegate()
					{
						PXCache cache = graph.Caches[typeof (SValue)];
						List<SValue> list = new List<SValue>();
						foreach (PXResult<SegmentValue, INSubItemSegmentValue> r in 
							PXSelectJoin<SegmentValue,							
								LeftJoin<INSubItemSegmentValue, 
											On<INSubItemSegmentValue.inventoryID, Equal<Current<InventoryItem.inventoryID>>,
										 And<INSubItemSegmentValue.segmentID, Equal<SegmentValue.segmentID>,
										 And<INSubItemSegmentValue.value, Equal<SegmentValue.value>>>>>,
							Where<SegmentValue.dimensionID, Equal<SubItemAttribute.dimensionName>,
 								And<SegmentValue.segmentID, Equal<Required<SegmentValue.segmentID>>>>>.Select(graph,  segmentID))
						{
							SegmentValue segValue = r;
							INSubItemSegmentValue itemValue = r;
							SValue result = (SValue)cache.CreateInstance();
							result.SegmentID = segValue.SegmentID;
							result.Value = segValue.Value;
							result.Descr = segValue.Descr;
							if (itemValue.InventoryID != null)
								result.Active = true;
							result = (SValue) (cache.Insert(result) ?? cache.Locate(result));
							list.Add(result);
						}
						return list;
					}));			
				graph.RowUpdated.AddHandler<SValue>(OnRowUpdated);
			}
		}

		protected virtual void OnRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SValue row = e.Row as SValue;
			if (row == null) return;
			INSubItemSegmentValue result = (INSubItemSegmentValue)this.Cache.CreateInstance();
			this.Cache.SetDefaultExt<INSubItemSegmentValue.inventoryID>(result);			
			result.SegmentID = row.SegmentID;
			result.Value = row.Value;
			if(row.Active == true)			
				this.Cache.Update(result);
			else
				this.Cache.Delete(result);							
		}
	}
}

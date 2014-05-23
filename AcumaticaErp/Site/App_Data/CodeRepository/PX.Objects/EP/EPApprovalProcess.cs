using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Compilation;
using PX.Data;
using PX.Objects.CR;
using PX.TM;

namespace PX.Objects.EP
{
	[System.SerializableAttribute()]
	public partial class EPApprovalFilter : IBqlTable
	{
		#region CurrentOwnerID
		public abstract class currentOwnerID : PX.Data.IBqlField
		{
		}

		[PXDBGuid]
		[CR.CRCurrentOwnerID]
		public virtual Guid? CurrentOwnerID { get; set; }
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.IBqlField
		{
		}
		protected Guid? _OwnerID;
		[PXDBGuid]
		[PXUIField(DisplayName = "Assigned To")]
		[PX.TM.PXSubordinateOwnerSelector]
		public virtual Guid? OwnerID
		{
			get
			{
				return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
			}
			set
			{
				_OwnerID = value;
			}
		}
		#endregion
		#region MyOwner
		public abstract class myOwner : PX.Data.IBqlField
		{
		}
		protected Boolean? _MyOwner;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Me")]
		public virtual Boolean? MyOwner
		{
			get
			{
				return _MyOwner;
			}
			set
			{
				_MyOwner = value;
			}
		}
		#endregion
		#region WorkGroupID
		public abstract class workGroupID : PX.Data.IBqlField
		{
		}
		protected Int32? _WorkGroupID;
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
			Where<EPCompanyTree.workGroupID, Owned<Current<AccessInfo.userID>>>>),
		 SubstituteKey = typeof(EPCompanyTree.description))]
		public virtual Int32? WorkGroupID
		{
			get
			{
				return (_MyWorkGroup == true) ? null : _WorkGroupID;
			}
			set
			{
				_WorkGroupID = value;
			}
		}
		#endregion
		#region MyWorkGroup
		public abstract class myWorkGroup : PX.Data.IBqlField
		{
		}
		protected Boolean? _MyWorkGroup;
		[PXDefault(false)]
		[PXDBBool]
		[PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? MyWorkGroup
		{
			get
			{
				return _MyWorkGroup;
			}
			set
			{
				_MyWorkGroup = value;
			}
		}
		#endregion
		#region MyEscalated
		public abstract class myEscalated : PX.Data.IBqlField
		{
		}
		protected Boolean? _MyEscalated;
		[PXDefault(true)]
		[PXDBBool]
		[PXUIField(DisplayName = "Display Escalated", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? MyEscalated
		{
			get
			{
				return _MyEscalated;
			}
			set
			{
				_MyEscalated = value;
			}
		}
		#endregion
		#region FilterSet
		public abstract class filterSet : PX.Data.IBqlField
		{
		}
		[PXDefault(false)]
		[PXDBBool]
		public virtual Boolean? FilterSet
		{
			get
			{
				return
					this.OwnerID != null ||
					this.WorkGroupID != null ||
					this.MyWorkGroup == true ||
					this.MyEscalated == true;
			}
		}
		#endregion		
	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class EPApprovalProcess : PXGraph<EPApprovalProcess>
	{
		public PXSelect<BAccount> bAccount;

		public PXFilter<EPApprovalFilter> Filter;
		public PXCancel<EPApprovalFilter> Cacnel;

		
		[Serializable]
		[PX.TM.OwnedEscalatedFilter.Projection(
			typeof(EPApprovalFilter),
			typeof(EPApproval),
			typeof(InnerJoin<Note,
										On<Note.noteID, Equal<EPApproval.refNoteID>,
									 And<EPApproval.status, Equal<EPApprovalStatus.pending>>>>),
			typeof(Aggregate<GroupBy<EPApproval.refNoteID, 
			GroupBy<EPApproval.curyInfoID, 
			GroupBy<EPApproval.bAccountID, 
			GroupBy<EPApproval.ownerID, 
			GroupBy<EPApproval.approvedByID,
			GroupBy<EPApproval.curyTotalAmount>>>>>>>),
			typeof(EPApproval.workgroupID),
			typeof(EPApproval.ownerID),
			typeof(EPApproval.createdDateTime))]
		public partial class EPOwned : EPApproval
		{
			#region RefNoteID
			public new abstract class refNoteID : PX.Data.IBqlField
			{
			}
			[PXRefNote(BqlTable = typeof(EPApproval), LastKeyOnly=true)]
			[PXUIField(DisplayName = "Reference Nbr.")]
			[PXNoUpdate]
			public override Int64? RefNoteID
			{
				get
				{
					return this._RefNoteID;
				}
				set
				{
					this._RefNoteID = value;
				}
			}
			#endregion
			#region Selected
			public abstract class selected : PX.Data.IBqlField
			{
			}
			protected bool? _Selected = false;
			[PXBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Selected")]
			public virtual bool? Selected
			{
				get
				{
					return _Selected;
				}
				set
				{
					_Selected = value;
				}
			}
			#endregion
			#region Escalated
			public abstract class escalated : PX.Data.IBqlField
			{
			}
			protected int? _Escalated;
			/*[PXDBCalced(typeof(Switch<Case<Where<EPApproval.workgroupID,
				Escalated<CurrentValue<EPApprovalFilter.currentOwnerID>,
					EP.orderDate>>, OwnedFilter.boolTrue>, OwnedFilter.boolFalse>), typeof(Boolean))]*/
			public virtual int? Escalated
			{
				get
				{
					return this._Escalated;
				}
				set
				{
					this._Escalated = value;
				}
			}
			#endregion
			#region BAccountID
			public new abstract class bAccountID : PX.Data.IBqlField
			{
			}
			[PXDBInt(BqlTable = typeof(EPApproval))]
			[PXUIField(DisplayName = "Business Account")]
			[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctName))]
			public override Int32? BAccountID
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
			#region CuryInfoID
			public new abstract class curyInfoID : PX.Data.IBqlField
			{
			}
			[PXDBLong(BqlTable = typeof(EPApproval))]
			[CM.CurrencyInfo()]
			public override Int64? CuryInfoID
			{
				get
				{
					return this._CuryInfoID;
				}
				set
				{
					this._CuryInfoID = value;
				}
			}
			#endregion			
			#region CuryTotalAmount
			public new abstract class curyTotalAmount : PX.Data.IBqlField
			{
			}
			[PXDBDecimal(4, BqlTable = typeof(EPApproval))]
			[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Total Amount")]
			public override Decimal? CuryTotalAmount
			{
				get
				{
					return this._CuryTotalAmount;
				}
				set
				{
					this._CuryTotalAmount = value;
				}
			}
			#endregion
			#region TotalAmount
			public new abstract class totalAmount : PX.Data.IBqlField
			{
			}
			
			[PXDBDecimal(4, BqlTable = typeof(EPApproval))]
			[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
			public override Decimal? TotalAmount
			{
				get
				{
					return this._TotalAmount;
				}
				set
				{
					this._TotalAmount = value;
				}
			}
			#endregion                        
			#region CreatedDateTime
			public new abstract class createdDateTime : PX.Data.IBqlField
			{
			}
			[PXDBDate(PreserveTime = true, DisplayMask = "g", BqlTable = typeof(EPApproval))]
			[PXUIField(DisplayName = "Requested Time")]
			public override DateTime? CreatedDateTime
			{
				get
				{
					return this._CreatedDateTime;
				}
				set
				{
					this._CreatedDateTime = value;
				}
			}
			#endregion
			#region EntityType
			public abstract class entityType : IBqlField
			{
			}
			private string _EntityType;
			[PXDBString(BqlTable = typeof(Note))]
			public string EntityType
			{
				get
				{
					return _EntityType;
				}
				set
				{
					_EntityType = value;
				}
			}
			#endregion
			#region DocType
			public new abstract class docType : IBqlField
			{
			}
			private string _DocType;
			[PXString()]
			[PXUIField(DisplayName = "Type")]
			[PXFormula(typeof(ApprovalDocType<EPOwned.entityType>))]
			public override string DocType
			{
				get
				{
					return _DocType;
				}
				set
				{
					_DocType = value;
				}
			}
			#endregion
		}

		public PXCancel<EPOwned> Cancel;
		[PXFilterable]
		public PXFilteredProcessing<EPOwned, EPApprovalFilter> Records;

		public PXSetup<EPSetup> EPSetup;

		[PXHidden]
		public PXSelect<PM.PMProject> Projects;

		public EPApprovalProcess()
		{
			Records.SetProcessCaption(EP.Messages.Approve);
			Records.SetProcessAllCaption(EP.Messages.ApproveAll);
			Records.SetSelected<EPOwned.selected>();
			Records.SetProcessDelegate(Approve);
		}

		protected virtual IEnumerable records()
		{
			Records.Cache.AllowInsert = false;
			Records.Cache.AllowDelete = false;

			PXSelectBase<EPOwned> select =
				new PXSelect<EPOwned,
					Where<True,Equal<True>>,
					OrderBy<Desc<EPOwned.docDate, Asc<EPOwned.refNoteID>>>>(this);
			select.View.Clear();			
			foreach (EPOwned doc in select.Select())
			{
				doc.Escalated = GetEscalated(doc.RefNoteID) ? 1 : 0;
				yield return doc;
			}
		}
		
		private bool GetEscalated(long? refNoteID)
		{
			EPOwned item =
			PXSelect<EPOwned,
			Where<EPOwned.refNoteID, Equal<Required<EPOwned.refNoteID>>,
				And<EPOwned.status, Equal<EPApprovalStatus.pending>,
				And<EPOwned.workgroupID, Escalated<CurrentValue<EPApprovalFilter.currentOwnerID>, EPApproval.workgroupID, EPApproval.ownerID, EPApproval.createdDateTime>>>>>
			.Select(this, refNoteID);

			return item != null;
		}

		protected virtual void EPOwned_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			EPOwned row = e.Row as EPOwned;
			if (row != null && row.Selected != true)
				sender.SetStatus(row, PXEntryStatus.Notchanged);
		}

		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}


		public PXAction<EPApprovalFilter> details;
		[PXUIField(DisplayName = AR.Messages.ViewDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable Details(PXAdapter adapter)
		{
			if (Filter.Current != null && Records.Current != null && Records.Current.RefNoteID != null)
			{
				EntityHelper helper = new EntityHelper(this);
				helper.NavigateToRow(Records.Current.RefNoteID.Value);
			}
			return adapter.Get();
		}

		protected static void Approve(List<EPOwned> items)
		{
			EntityHelper helper = new EntityHelper(new PXGraph());
			var graphs = new Dictionary<Type, PXGraph>();

			bool errorOccured = false;
			foreach (EPOwned item in items)
			{
				try
				{
					PXProcessing<EPApproval>.SetCurrentItem(item);
					if (item.RefNoteID == null) throw new PXException(Messages.ApprovalRefNoteIDNull);
					object row = helper.GetEntityRow(item.RefNoteID.Value, true);

					if (row == null) throw new PXException(Messages.ApprovalRecordNotFound);

					Type cahceType = row.GetType();
					Type graphType = helper.GetPrimaryGraphType(row, false);
					PXGraph graph;
					if(!graphs.TryGetValue(graphType, out graph))
					{
						graphs.Add(graphType, graph = PXGraph.CreateInstance(graphType));
					}
					graph.Clear();
					graph.Caches[cahceType].Current = row;
					graph.Caches[cahceType].SetStatus(row, PXEntryStatus.Notchanged);
					graph.AutomationView = PXAutomation.GetView(graph);
					string approved = typeof (EPExpenseClaim.approved).Name;
					if (graph.AutomationView != null)
					{
						PXAutomation.GetStep(graph,
						                                            new object[] {graph.Views[graph.AutomationView].Cache.Current},
						                                            BqlCommand.CreateInstance(
						                                            	typeof (Select<>),
						                                            	graph.Views[graph.AutomationView].Cache.GetItemType())
							);
					}

					if(graph.Actions.Contains("Approve"))
						graph.Actions["Approve"].Press();
					else if (graph.AutomationView != null)
					{
						PXView view = graph.Views[graph.AutomationView];
						BqlCommand select = view.BqlSelect;																				
						PXAdapter adapter = new PXAdapter(new DummyView(graph, select, new List<object> { row }));
						adapter.Menu = "Approve";
						if (graph.Actions.Contains("Action"))
							foreach (var i in graph.Actions["Action"].Press(adapter)) ;
						else
						{
							throw new PXException("Automation for screen/graph {0} exists but is not configured properly. Failed to find action - 'Action'", graph);
						}
						//PXAutomation.ApplyAction(graph, graph.Actions["Action"], "Approve", row, out rollback);							
					}
					else if (graph.Caches[cahceType].Fields.Contains(approved))
					{
						object upd = graph.Caches[cahceType].CreateCopy(row);
						graph.Caches[cahceType].SetValue(upd, approved, true);
						graph.Caches[cahceType].Update(upd);
					}
					graph.Persist();
				}
				catch (Exception ex)
				{
					errorOccured = true;
					PXProcessing<EPApproval>.SetError(ex);
				}
			}
			if(errorOccured)
				throw new PXOperationCompletedException(ErrorMessages.SeveralItemsFailed);
		}
		private sealed class DummyView : PXView
		{
			private readonly List<object> _records;
			internal DummyView(PXGraph graph, BqlCommand command, List<object> records)
				: base(graph, true, command)
			{
				_records = records;
			}
			public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
			{
				return _records;
			}
		}
	}
}

using System;
using System.Collections;
using System.Collections.Specialized;
using PX.Data;
using PX.Objects.GL;
using System.Collections.Generic;

namespace PX.Objects.CR
{
	#region CRLeadBatch
	[Serializable]
	public class CRLeadBatch : Lead, IPXSelectable
	{
		#region Selected
		public abstract class selected : IBqlField
		{
		}
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected", Filterable = false)]
		[CRMergeableAttribute(false)]
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
		#region DefAddressID
		public new abstract class defAddressID : PX.Data.IBqlField
		{
		}
		#endregion
		#region BAccountID
		public new abstract class bAccountID : PX.Data.IBqlField
		{
		}
		#endregion
		#region ContactID
		public new abstract class contactID : PX.Data.IBqlField
		{
		}
		#endregion
		#region WorkgroupID
		public new abstract class workgroupID : PX.Data.IBqlField
		{
		}
		#endregion
	}
	#endregion

	#region SelectedLead
	[PXSubstitute(GraphType = typeof(CRLeadMergeMaint))]
	[PXSubstitute(GraphType = typeof(LeadMaint))]
	[PXCacheName(Messages.Lead)]
	public class SelectedLead : Lead
	{
		public static SelectedLead FromLead(Lead lead)
		{
			SelectedLead result = new SelectedLead();
			SelectedContact.Copy(lead, result);
			result.CompanyName = lead.CompanyName;
			result.Source = lead.Source;
			result.Status = lead.Status;
			result.StatusDate = lead.StatusDate;
			result.ConvertedDate = lead.ConvertedDate;
			return result;
		}
	}
	#endregion

	#region CRLeadMergeMaint
	[PXGraphName(Messages.CRLeadMergeMaint, typeof(Lead))]
	public class CRLeadMergeMaint : CRMergeMaint<CRLeadMergeMaint, CRContactBatch>, PXFilterableAttribute.IAdditionalFilterTables
	{
		public const string ASSIGNLEADS_ACTIONNAME = "AssignLeads";

		#region MergeProcessor
		public class LeadMergeProcessor : MergeProcessor<CRContactBatch>
		{
			private readonly PXGraph _graph;
			private readonly PXCache _leadCache;

			public LeadMergeProcessor(PXGraph graph, PXView view)
				: base(graph, view)
			{
				_graph = graph;
				_leadCache = _graph.Caches[typeof(CRLeadBatch)];
			}

			protected override Dictionary<string, List<object>> GetDiffItems()
			{
				Dictionary<string, List<object>> items = base.GetDiffItems();
				List<string> leadFields = new List<string>(_leadCache.Fields);
				foreach (string contactField in _graph.Caches[typeof(CRContactBatch)].Fields)
					leadFields.Remove(contactField);
				if (!OnlySelectedState) GetLeadsView(_graph).SelectMulti();
				foreach (KeyValuePair<string, List<object>> pair in
					CRMergeValuesListAttribute.GetPropertyValuesPairs(_leadCache, GetItemsByState(_leadCache), leadFields))
				{
					items.Add(pair.Key, pair.Value);
				}
				return items;
			}

			private static PXView GetLeadsView(PXGraph graph)
			{
				return graph.Views["LeadItems"];
			}

			protected override IEnumerable<CRFieldNewValues> GetFields(List<string> avaliableProperties)
			{
				List<CRFieldNewValues> result = new List<CRFieldNewValues>(base.GetFields(avaliableProperties));
				foreach(CRFieldNewValues item in CRFieldNewValues.GetFields(typeof (CRMergeNewValues<CRContactBatch>), _leadCache, avaliableProperties, GetMergingViewName))
					if (result.Find(delegate(CRFieldNewValues obj) { return obj.DataField == item.DataField; }) == null)
						result.Add(item);
				result.Sort(new Comparison<CRFieldNewValues>(
					delegate(CRFieldNewValues x, CRFieldNewValues y)
						{
							return string.Compare(x.DataFieldName, y.DataFieldName, false);
						}));
				return result;
			}

			protected override void AddMergingViews()
			{
				base.AddMergingViews();

				foreach (string field in _leadCache.Fields)
					if (CRFieldNewValues.GetSelectorAttribute(_leadCache, field) != null || CRFieldNewValues.GetDimensionAttribute(_leadCache, field) != null)
						AddMergingView(_leadCache, GetViewName(_leadCache, field), field);
			}

			protected override object GetValueExt(object row, CRFieldNewValues item)
			{
				return base.GetValueExt(row, item) ?? _leadCache.GetValueExt(row, item.DataField);
			}

			protected override Type[] MergeTypes
			{
				get
				{
					List<Type> result = new List<Type>(base.MergeTypes);
					result.Add(typeof(CRLeadBatch));
					return result.ToArray();
				}
			}

			protected override void OnValuesListPreInit(PXGraph graph)
			{
				base.OnValuesListPreInit(graph);
				if (!OnlySelectedState) GetLeadsView(graph).SelectMulti();
			}
		}
		#endregion

		#region MPCRAssignmentMap
		[Serializable]
		public class MPCRAssignmentMap : IBqlTable
		{
			public abstract class cRAssignmentMapID : PX.Data.IBqlField { }
			protected Int32? _CRAssignmentMapID;
			[PXInt]
			[PXUIField(DisplayName = Messages.AssignMap)]
			[PXSelector(typeof(Search<CRAssignmentMap.cRAssignmentMapID, Where<CRAssignmentMap.type, Equal<AssignmentMapType.AssignmentMapTypeLead>>>), DescriptionField = typeof(CRAssignmentMap.name))]
			public virtual Int32? CRAssignmentMapID
			{
				get
				{
					return this._CRAssignmentMapID;
				}
				set
				{
					this._CRAssignmentMapID = value;
				}
			}
		}
		#endregion

		#region Selects
		[PXHidden]
		public PXSetup<Company> company;

		public PXSelect<SelectedContact> Contact;
		public PXSelect<SelectedLead> Lead;

		public PXFilter<MPCRAssignmentMap> AssignmentMap;
        
		[CRUpdate(typeof(CRContactBatch), ClearCaches = true, AdditionalClearCaches = new Type[] { typeof(CRContactBatch) })]
		public PXSelect<CRLeadBatch> LeadItems;

		[PXFilterable(typeof(SelectedContact), typeof(SelectedLead), AdditionalFilters = new Type[] { typeof(CRAttributeFilter) })]
		[CRMassProcessing(typeof(CRContactBatch), ClearCaches = true, AdditionalClearCaches = new Type[] { typeof(CRLeadBatch) })]
		[CRMerge(null, typeof(LeadMergeProcessor))]
		public PXSelectJoin<CRContactBatch, LeftJoin<CRLeadBatch, On<CRLeadBatch.contactID, Equal<CRContactBatch.contactID>>>> Items;
		#endregion

		#region Constructors
		public CRLeadMergeMaint()
			: base()
		{
		}
		public CRLeadMergeMaint(PXGraph graph)
			: base(graph)
		{
		}

		protected override void Initialize()
		{
			base.Initialize();

			PXUIFieldAttribute.SetEnabled(Items.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<CRContactBatch.selected>(Items.Cache, null, true);

			CRHelper.PXUIFieldAttributeHelper helper = new CRHelper.PXUIFieldAttributeHelper(Items);
			CRHelper.PXUIFieldAttributeHelper helperAddress = new CRHelper.PXUIFieldAttributeHelper(this.Caches[typeof(Address)]);
			CRHelper.PXUIFieldAttributeHelper helperLead = new CRHelper.PXUIFieldAttributeHelper(this.Caches[typeof(CRLeadBatch)]);
			helper.SetVisibleToFalse<CRContactBatch.isActive>();
			helper.SetVisibleToFalse<CRContactBatch.title>();
			helper.SetVisibleToFalse<CRContactBatch.firstName>();
			helper.SetVisibleToFalse<CRContactBatch.midName>();
			helper.SetVisibleToFalse<CRContactBatch.lastName>();
			helper.SetVisibleToFalse<CRContactBatch.phone2>();
			helper.SetVisibleToFalse<CRContactBatch.phone3>();
			helper.SetVisibleToFalse<CRContactBatch.fax>();
			helper.SetVisibleToFalse<CRContactBatch.webSite>();
			helper.SetVisibleToFalse<CRContactBatch.dateOfBirth>();
			helper.SetVisibleToFalse<CRContactBatch.createdByID>();
			helper.SetVisibleToFalse<CRContactBatch.createdDateTime>();
			helper.SetVisibleToFalse<CRContactBatch.lastModifiedByID>();
			helper.SetVisibleToFalse<CRContactBatch.lastModifiedDateTime>();
			helper.SetVisibleToFalse<CRContactBatch.workgroupID>();
			helper.SetVisibleToFalse<CRContactBatch.ownerID>();
			helperAddress.SetVisibleToFalse<Address.addressLine1>();
			helperAddress.SetVisibleToFalse<Address.addressLine2>();
			helperLead.SetVisibleToFalse<CRLeadBatch.source>();
			helperLead.SetVisibleToFalse<CRLeadBatch.companyName>();
			helperLead.SetVisibleToFalse<CRLeadBatch.status>();
			helperLead.SetVisibleToFalse<CRLeadBatch.classID>();
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (CRHelper.StrEquals(viewName, "Items"))
			{
				OrderedDictionary correctValues = new OrderedDictionary();
				if (values.Contains("Selected")) correctValues.Add("Selected", values["Selected"]);
				base.ExecuteUpdate("LeadItems", keys, correctValues, parameters);
				return base.ExecuteUpdate(viewName, keys, correctValues, parameters);
			}
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}
		#endregion

		#region AssignLeads
		public PXAction<CRContactBatch> assignLeads;
		[PXUIField(DisplayName = Messages.AssignLeads)]
		[CRButtonGroup(CRMassProcessingAttribute.MASSPROCESSING_GROUPNAME, ImageUrl = "~/Icons/Menu/process_16.gif")]
		public IEnumerable AssignLeads(PXAdapter adapter)
		{
			PXCache cache = this.Caches[typeof(CRLeadBatch)];
			bool onlySelected = CRMassProcessingAttribute.GetOnlySelectedParameter(adapter.Parameters);
			if ((!onlySelected || CRMassProcessingAttribute.ExistSelectedItems(cache)) &&
				AssignmentMap.View.Ask(null, MessageButtons.Panel) == WebDialogResult.OK)
			{
				CRAssignmentProcessHelper<Lead> helper = new CRAssignmentProcessHelper<Lead>();
				int assignmentMapID = AssignmentMap.Current.CRAssignmentMapID.Value;
				foreach (CRLeadBatch item in onlySelected ?
						CRMassProcessingAttribute.GetSelectedItems(cache) :
						CRMassProcessingAttribute.GetAllItems(cache))
				{
					helper.Assign(item, assignmentMapID);
				}
				CRHelper.SafetyPersist(cache, PXDBOperation.Update);
				this.SelectTimeStamp();
			}

			return adapter.Get();
		}
		#endregion

		#region Merge
		protected override PXGraph GetGraphForDetails(CRContactBatch selectedItem)
		{
			if (CRLeadBatch.IsLead(selectedItem))
			{
				LeadMaint leadGraph = CreateGraph<LeadMaint>();
				leadGraph.Lead.Current = leadGraph.Lead.Search<Lead.contactID>(selectedItem.ContactID);
				return leadGraph;
			}
			ContactMaint contactGraph = new ContactMaint();
			contactGraph.Contact.Current = contactGraph.Contact.Search<Contact.contactID>(selectedItem.ContactID);
			return contactGraph;
		}

		protected override PXSelectBase<CRContactBatch> MargeItems
		{
			get { return Items; }
		}

		protected override bool IsBatchsEqual(CRContactBatch x, CRContactBatch y)
		{
			return x.ContactID == y.ContactID;
		}
		#endregion

		public void SetAction(string actionName)
		{
			CRMassProcessingAttribute.SetSelectedAction(Items.View, Items.GetItemType().Name + actionName);
		}

		#region Select handlers
		protected IEnumerable items()
		{
			string actionName = CRMassProcessingAttribute.GetSelectedAction(Items.View);
			if (CRMergeAttribute.IsMergeAction(actionName))
				return new PXSelectJoin<CRContactBatch,
					LeftJoin<CRLeadBatch, On<CRLeadBatch.contactID, Equal<CRContactBatch.contactID>>,
					LeftJoin<BAccount, On<CRContactBatch.bAccountID, Equal<BAccount.bAccountID>>, 
					LeftJoin<Address, On<Address.addressID, Equal<CRContactBatch.defAddressID>>>>>,
					Where<CRContactBatch.contactType, Equal<ContactTypes.person>,
						And<Where<CRContactBatch.bAccountID, NotEqual<Current<Company.bAccountID>>, 
							Or<CRContactBatch.bAccountID, IsNull>>>>>(this).Select();
			if (CRUpdateAttribute.IsUpdateAction(actionName))
				return new PXSelectJoin<CRContactBatch,
					InnerJoin<CRLeadBatch, On<CRLeadBatch.contactID, Equal<CRContactBatch.contactID>>,
					LeftJoin<Address, On<Address.addressID, Equal<CRLeadBatch.defAddressID>>>>,
					Where<CRLeadBatch.bAccountID, IsNull>>(this).Select();
			if (actionName == ASSIGNLEADS_ACTIONNAME)
				return new PXSelectJoin<CRContactBatch,
					InnerJoin<CRLeadBatch, On<CRLeadBatch.contactID, Equal<CRContactBatch.contactID>>,
					LeftJoin<Address, On<Address.addressID, Equal<CRLeadBatch.defAddressID>>>>,
					Where<CRLeadBatch.workgroupID, IsNull>>(this).Select();
			return new PXResultset<CRContactBatch, CRLeadBatch, BAccount, Address>();
		}

		protected IEnumerable leadItems()
		{
			string actionName = CRMassProcessingAttribute.GetSelectedAction(Items.View);
			if (CRMergeAttribute.IsMergeAction(actionName) || CRUpdateAttribute.IsUpdateAction(actionName))
				return new PXSelect<CRLeadBatch, Where<CRLeadBatch.bAccountID, IsNull>>(this).Select();
			if (actionName == ASSIGNLEADS_ACTIONNAME)
				return new PXSelect<CRLeadBatch, Where<CRLeadBatch.workgroupID, IsNull>>(this).Select();
			return new PXResultset<CRLeadBatch>();
		}
		#endregion

		#region PXFilterableAttribute.IAdditionalFilterTables
		public Type[] GetTables(string viewName)
		{
			if (CRHelper.StrEquals(viewName, "Items")) 
				return new Type[] { typeof(CRLeadBatch), typeof(BAccount), typeof(Address) };

			return null;
		}
		#endregion
	}
	#endregion
}

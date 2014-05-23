using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PX.Objects.CR
{
	#region Filter DAOs
	#region CRFilterTemplate1
	[System.Serializable()]
	public class CRFilterTemplate1 : CRFilterTemplate
	{
	}
	#endregion
	#region CRFilterTemplate
	[System.Serializable()]
	public class CRFilterTemplate : IBqlTable
	{
		#region FilterTemplateID
		public abstract class filterTemplateID : PX.Data.IBqlField
		{
		}
		protected int? _FilterTemplateID;
		[PXDBIdentity(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Template", Visibility = PXUIVisibility.Invisible)]
		[CRFilterTemplateSelector()]
		public virtual int? FilterTemplateID
		{
			get
			{
				return this._FilterTemplateID;
			}
			set
			{
				this._FilterTemplateID = value;
			}
		}
		#endregion
		#region GraphType
		public abstract class graphType : PX.Data.IBqlField
		{
		}
		protected String _GraphType;
		[PXDBString(255, IsUnicode = true)]
		public virtual String GraphType
		{
			get
			{
				return this._GraphType;
			}
			set
			{
				this._GraphType = value;
			}
		}
		#endregion
		#region Name
		public abstract class name : PX.Data.IBqlField
		{
		}
		protected String _Name;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Name", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				this._Name = value;
			}
		}
		#endregion
		#region IsDefault
		public abstract class isDefault : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsDefault;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Default")]
		public virtual Boolean? IsDefault
		{
			get
			{
				return this._IsDefault;
			}
			set
			{
				this._IsDefault = value;
			}
		}
		#endregion
		#region IsShared
		public abstract class isShared : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsShared;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Shared")]
		public virtual Boolean? IsShared
		{
			get
			{
				return this._IsShared;
			}
			set
			{
				this._IsShared = value;
			}
		}
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.IBqlField
		{
		}
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.IBqlField
		{
		}
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
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
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.IBqlField
		{
		}
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		//#region tstamp
		//public abstract class Tstamp : PX.Data.IBqlField
		//{
		//}
		//protected Byte[] _tstamp;
		//[PXDBTimestamp()]
		//public virtual Byte[] tstamp
		//{
		//    get
		//    {
		//        return this._tstamp;
		//    }
		//    set
		//    {
		//        this._tstamp = value;
		//    }
		//}
		//#endregion
	}
	#endregion
	#region CRFilterItem
	[System.Serializable()]
	public class CRFilterItem : IBqlTable
	{
		#region FilterItemID
		public abstract class filterItemID : PX.Data.IBqlField
		{
		}
		protected int? _FilterItemID;
		[PXDBIdentity(IsKey = true)]
		[PXDefault()]
		[PXUIField(Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual int? FilterItemID
		{
			get
			{
				return this._FilterItemID;
			}
			set
			{
				this._FilterItemID = value;
			}
		}
		#endregion
		#region FilterTemplateID
		public abstract class filterTemplateID : PX.Data.IBqlField
		{
		}
		protected int? _FilterTemplateID;
		[PXDBInt()]
		[PXUIField(Visibility = PXUIVisibility.Invisible, Visible = false)]
		[PXDBDefault(typeof(CRFilterTemplate.filterTemplateID))]
		public virtual int? FilterTemplateID
		{
			get
			{
				return this._FilterTemplateID;
			}
			set
			{
				this._FilterTemplateID = value;
			}
		}
		#endregion
		#region IsUsed
		public abstract class isUsed : IBqlField
		{
		}
		protected Boolean? _IsUsed = false;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use")]
		public Boolean? IsUsed
		{
			get
			{
				return _IsUsed;
			}
			set
			{
				_IsUsed = value;
			}
		}
		#endregion
		#region DataField
		public abstract class dataField : PX.Data.IBqlField
		{
		}
		protected String _DataField;
		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "DataField")]
		public virtual String DataField
		{
			get
			{
				return this._DataField;
			}
			set
			{
				this._DataField = value;
			}
		}
		#endregion
		#region Condition
		public abstract class condition : PX.Data.IBqlField
		{
		}
		protected String _Condition;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Condition")]
		public virtual String Condition
		{
			get
			{
				return this._Condition;
			}
			set
			{
				this._Condition = value;
			}
		}
		#endregion
		#region Value
		public abstract class value : PX.Data.IBqlField
		{
		}
		protected String _Value;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Value")]
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
		#region Value2
		public abstract class value2 : PX.Data.IBqlField
		{
		}
		protected String _Value2;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Value2")]
		public virtual String Value2
		{
			get
			{
				return this._Value2;
			}
			set
			{
				this._Value2 = value;
			}
		}
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.IBqlField
		{
		}
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.IBqlField
		{
		}
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
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
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.IBqlField
		{
		}
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		//#region tstamp
		//public abstract class Tstamp : PX.Data.IBqlField
		//{
		//}
		//protected Byte[] _tstamp;
		//[PXDBTimestamp()]
		//public virtual Byte[] tstamp
		//{
		//    get
		//    {
		//        return this._tstamp;
		//    }
		//    set
		//    {
		//        this._tstamp = value;
		//    }
		//}
		//#endregion
	}
	#endregion
	#region CRPermanentFilterItem
	public class CRPermanentFilterItem : CRFilterItem
	{
		#region DataField
		public new abstract class dataField : PX.Data.IBqlField
		{
		}
		#endregion
	}
	#endregion
	#region CRFilterTemplateSelectorAttribute
	public class CRFilterTemplateSelectorAttribute : PXCustomSelectorAttribute
	{
		private List<CRFilterTemplate> templates;

		public CRFilterTemplateSelectorAttribute()
			: base(typeof(CRFilterTemplate.filterTemplateID))
		{
			templates = new List<CRFilterTemplate>();
			DescriptionField = typeof(CRFilterTemplate.name);
		}

		public override void CacheAttached(PXCache sender)
		{
			LoadTemplates(sender.Graph.GetType().FullName);
			base.CacheAttached(sender);
		}

		internal IEnumerable GetRecords()
		{
			return templates;
		}

		private void LoadTemplates(string graphTypeName)
		{
			templates.Clear();
			if (!string.IsNullOrEmpty(graphTypeName))
			{
				CRFilterTemplate currentTemplate;
				try
				{
					foreach (PXDataRecord item in PXDatabase.SelectMulti<CRFilterTemplate>(
							new PXDataField("FilterTemplateID"),
							new PXDataField("Name"),
							new PXDataFieldValue("GraphType", PXDbType.NVarChar, 255, graphTypeName, PXComp.EQ)))
					{
						currentTemplate = new CRFilterTemplate();
						currentTemplate.FilterTemplateID = (int?)item["FilterTemplateID"];
						currentTemplate.Name = item["Name"].ToString();
						templates.Add(currentTemplate);
					}
				}
				catch (ExecutionEngineException) { throw; }
				catch (OutOfMemoryException) { throw; }
				catch (StackOverflowException) { throw; }
				catch (Exception)
				{
					templates.Clear();
				}
			}
		}
	}
	#endregion
	#endregion
	#region CRFilterMaint
	public abstract class CRFilterMaint<TGraph, PVTable, Select> : PXGraph<TGraph, PVTable>
		where TGraph : PXGraph
		where PVTable : class, IBqlTable, new()
		where Select : PXSelectBase
	{
		#region Readonly Fields
		private static Regex _findFieldNameRegEx = new Regex(@"_.+$");
		private static Regex _findForeignFieldNameRegEx = new Regex(@"\..+$");
		#endregion
		#region Fields
		public PXSelect<CRFilterTemplate,
			Where<CRFilterTemplate.graphType,
				Equal<Required<CRFilterTemplate.graphType>>,
				And<
					Where<CRFilterTemplate.isShared, Equal<BoolTrue>,
						Or<CRFilterTemplate.createdByID, Equal<Required<CRFilterTemplate.createdByID>>>>>>> Templates;
		public PXSelect<CRFilterTemplate1,
			Where<CRFilterTemplate1.graphType,
				Equal<Required<CRFilterTemplate1.graphType>>,
				And<
					Where<CRFilterTemplate1.isShared, Equal<BoolTrue>,
						Or<CRFilterTemplate1.createdByID, Equal<Required<CRFilterTemplate1.createdByID>>>>>>> TemplatesSecond;
		public PXSelect<CRFilterItem,
			Where<CRFilterItem.filterTemplateID,
				Equal<Current<CRFilterTemplate.filterTemplateID>>>> Filters;
		public PXSelect<CRPermanentFilterItem> PermanentFilters;
		[PXFilterable()]
		public Select Items;
		private CRPropertyListAttribute propertyListAtt;
		private string[] conditionNames;
		private string[] conditionValues;
		private string graphTypeName;
		private Type selectTable;
		private PropertyInfo itemsKeyProperty;
		#endregion
		#region Costructors
		public CRFilterMaint()
			: base()
		{
			Initialize();
		}
		public CRFilterMaint(PXGraph graph)
			: base(graph)
		{
			Initialize();
		}

		protected virtual bool LiftPrimaryView
		{
			get
			{
				return true;
			}
		}

		private void Initialize()
		{
			MovePrimaryViewToFirst();

			graphTypeName = GetType().FullName;

			propertyListAtt = new CRPropertyListAttribute(Items.View.BqlSelect);
			propertyListAtt.Initialize();

			conditionNames = PXEnumDescriptionAttribute.GetNames(typeof(PXCondition));
			List<string> values = new List<string>();
			foreach (object item in Enum.GetValues(typeof(PXCondition)))
				values.Add(item.ToString());
			conditionValues = values.ToArray();

			selectTable = Items.Cache.GetItemType();
			itemsKeyProperty = SearchKeyProperty(selectTable);
		}

		private void MovePrimaryViewToFirst()
		{
			if (LiftPrimaryView)
			{
				Views.Caches.Remove(typeof(PVTable));
				Views.Caches.Insert(0, typeof(PVTable));
			}
		}
		#endregion

		#region Buttons
		public PXAction<PVTable> clearTemplate;
		[PXUIField(DisplayName = Messages.CancelTemplate, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
		[PXButton(ImageUrl = "~/Icons/Menu/entry_16.gif", DisabledImageUrl="~/Icons/Menu/entry_16_NotActive.gif")]
		public IEnumerable ClearTemplate(PXAdapter adapter)
		{
			ClearPermanentData();
			return CancelTemplate(adapter);
		}

		public PXAction<PVTable> cancelTemplate;
		[PXButton()]
		public IEnumerable CancelTemplate(PXAdapter adapter)
		{
			Templates.Cache.Clear();
			Filters.Cache.Clear();
			//Items.View.RequestRefresh();
			return adapter.Get();
		}
		public PXAction<PVTable> removeTemplate;
		[PXUIField(DisplayName = Messages.RemoveTemplate, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
		[PXButton(ImageUrl = "~/Icons/Menu/entry_16.gif", DisabledImageUrl="~/Icons/Menu/entry_16_NotActive.gif")]
		public IEnumerable RemoveTemplate(PXAdapter adapter)
		{
			if (Templates.Current != null)
			{
				foreach (CRFilterItem item in Filters.Select())
					Filters.Delete(item);
				Templates.Delete(Templates.Current);
				this.Persist();
				Items.View.RequestRefresh();
			}
			return adapter.Get();
		}
		public PXAction<PVTable> runTemplate;
		[PXUIField(DisplayName = Messages.RunTemplate, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXButton(ImageUrl = "~/Icons/Menu/entry_16.gif", DisabledImageUrl="~/Icons/Menu/entry_16_NotActive.gif")]
		public IEnumerable RunTemplate(PXAdapter adapter)
		{
			Items.Cache.Clear();
			Items.View.RequestRefresh();
			return adapter.Get();
		}
		public PXAction<PVTable> saveTemplate;
		[PXUIField(DisplayName = Messages.SaveTemplate, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
		[PXButton(ImageUrl = "~/Icons/Menu/entry_16.gif", DisabledImageUrl="~/Icons/Menu/entry_16_NotActive.gif")]
		public IEnumerable SaveTemplate(PXAdapter adapter)
		{
			SaveAs(true);
			return adapter.Get();
		}
		public PXAction<PVTable> saveAsTemplate;
		[PXUIField(DisplayName = Messages.SaveAsTemplate, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
		[PXButton(ImageUrl = "~/Icons/Menu/entry_16.gif", DisabledImageUrl="~/Icons/Menu/entry_16_NotActive.gif")]
		public IEnumerable SaveAsTemplate(PXAdapter adapter)
		{
			SaveAs(false);
			return adapter.Get();
		}

		private void SaveAs(bool onlyUpdate)
		{
			if (Templates.Current != null)
			{
				BqlCommand command = BqlCommand.CreateInstance(typeof(Select<CRFilterTemplate,
					Where<CRFilterTemplate.name, Equal<Current<CRFilterTemplate.name>>,
					And<CRFilterTemplate.graphType, Equal<Required<CRFilterTemplate.graphType>>>>>));
				CRFilterTemplate templateToUpdate = (new PXView(this, true, command)).SelectSingle(graphTypeName) as CRFilterTemplate;

				if (onlyUpdate && templateToUpdate == null) return;

				if (templateToUpdate == null) InsertNewTemplate();
				else UpdateExistingTemplate(templateToUpdate);

				CorrectGraphType();
				CorrectDefaultValues();

				SafetyPersist(Templates.Cache, PXDBOperation.Insert, PXDBOperation.Update);
				SafetyPersist(Filters.Cache, PXDBOperation.Delete, PXDBOperation.Insert, PXDBOperation.Update);

				Filters.Cache.Clear();
				Templates.Cache.Clear();
				Templates.View.RequestRefresh();
			}
		}

		public virtual void CRFilterTemplate_Name_FieldVarifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			CRFilterTemplate row = e.Row as CRFilterTemplate;
			if (row != null && GetExistingTemplateNames().Contains(Convert.ToString(e.NewValue)))
				throw new PXSetPropertyException(Messages.TemplateDublicateError);
		}

		private List<string> GetExistingTemplateNames()
		{
			List<string> list = new List<string>();
			foreach (CRFilterTemplate item in PXSelect<CRFilterTemplate,
				Where<CRFilterTemplate.graphType, Equal<Required<CRFilterTemplate.graphType>>>>.Select(this, graphTypeName))
				list.Add(item.Name);
			return list;
		}

		public PXAction<PVTable> process;
		#endregion

		#region Selects Handlers
		public virtual IEnumerable templates()
		{
			return getTemplates<CRFilterTemplate>(Templates);
		}

		public virtual IEnumerable templatesSecond()
		{
			return getTemplates<CRFilterTemplate1>(TemplatesSecond);
		}

		//public virtual IEnumerable items()
		//{
		//    Hashtable hashTable = new Hashtable();
		//    foreach (object item in Items.Cache.Updated)
		//    {
		//        hashTable.Add(GetKeyValue(item), item);
		//        yield return item;
		//    }
		//    foreach (object item in Items.Cache.Inserted)
		//    {
		//        hashTable.Add(GetKeyValue(item), item);
		//        yield return item;
		//    }
		//    PXView view = new PXView(this, true, Items.View.BqlSelect);
		//    object row;
		//    foreach (object item in SelectItems(view))
		//    {
		//        row = item is PXResult ? ((PXResult)item)[selectTable] : item;
		//        if (!hashTable.Contains(GetKeyValue(row)))
		//            yield return item;
		//    }
		//    if (hashTable.Count < 1) Items.Cache.IsDirty = false;
		//}

		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			if (searches != null && searches.Length > 0 && searches[0] != null)
			{
				if (viewName == "Templates") TemplatesSecond.Current = TemplatesSecond.Search<CRFilterTemplate1.filterTemplateID>(searches[0]);
				if (viewName == "TemplatesSecond") Templates.Current = Templates.Search<CRFilterTemplate.filterTemplateID>(searches[0]);
			}

			CRFilterTemplate current = Templates.Current;
			CRFilterTemplate1 current1 = TemplatesSecond.Current;

			IEnumerable result = base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
			if (viewName == "Templates" || viewName == "TemplatesSecond")
			{
				startRow = 0;
				if (maximumRows != 1)
				{
					if (viewName == "Templates") Templates.Current = current;
					if (viewName == "TemplatesSecond") TemplatesSecond.Current = current1;
				}
			}
			return result;
		}
		#endregion

		#region Public Methods
		public void SelectDefaultTemplate()
		{
			PXResultset<CRFilterTemplate> defaultTemplates = GetAllDefaultTemplates();
			Templates.Current = defaultTemplates.Count > 0 ? defaultTemplates[0] : GetAnyTemplate();
		}
		public void FillCurrentTemplate(object source)
		{
			FillCurrentTemplate(source, false);
		}
		public void FillCurrentTemplate(object source, bool permanent)
		{
			if (source == null) return;
			IDictionary<string, object> values = GetObjectValues(source);
			string currentPropertyName;
			foreach (CRFilterItem item in Filters.Select())
			{
				currentPropertyName = FindFieldName(item.DataField, true);
				if (currentPropertyName != null &&
					values.ContainsKey(currentPropertyName) &&
					string.IsNullOrEmpty(item.Value))
				{
					if (string.IsNullOrEmpty(item.Value = Convert.ToString(values[currentPropertyName])))
						item.IsUsed = false;
					Filters.Update(item);
				}
			}
			if (permanent) SetPermanentFilterItems(values);
		}
		public void ClearPermanentData()
		{
			PermanentFilters.Cache.Clear();
		}
		#endregion

		#region Private Methods
		private string FindFieldName(string data, bool isPrimary)
		{
			if (isPrimary) return FindFieldName(data, _findFieldNameRegEx, '_');
			return FindFieldName(data, _findForeignFieldNameRegEx, '.');
		}
		private string FindFieldName(string data, Regex regEx, char trimChar)
		{
			Match currentMatch = regEx.Match(data);
			return currentMatch.Success ? currentMatch.Value.TrimStart(trimChar) : null;
		}

		private void SetPermanentFilterItems(IDictionary<string, object> items)
		{
			CRPermanentFilterItem currentFilterItem;
			string currentValueString;
			foreach (KeyValuePair<string, object> pair in items)
				if (pair.Value != null && !string.IsNullOrEmpty(currentValueString = pair.Value.ToString()))
				{
					currentFilterItem = (CRPermanentFilterItem)PermanentFilters.Search<CRPermanentFilterItem.dataField>(pair.Key) ??
						PermanentFilters.Insert(new CRPermanentFilterItem());
					currentFilterItem.DataField = pair.Key;
					currentFilterItem.Value = currentValueString;
				}
			PermanentFilters.Cache.IsDirty = false;
		}
		private IEnumerable getTemplates<TTemplate>(PXSelectBase<TTemplate> select)
			where TTemplate : CRFilterTemplate, new()
		{
			Hashtable hashTable = new Hashtable();
			foreach (TTemplate item in select.Cache.Updated)
			{
				hashTable.Add(item.FilterTemplateID, item);
				yield return item;
			}
			foreach (TTemplate item in select.Cache.Inserted)
			{
				hashTable.Add(item.FilterTemplateID, item);
				yield return item;
			}
			PXView view = new PXView(this, true, select.View.BqlSelect);
			foreach (TTemplate item in view.SelectMulti(graphTypeName, PXAccess.GetUserID()))
				if (!hashTable.Contains(item.FilterTemplateID))
					yield return item;
			if (hashTable.Count < 1) select.Cache.IsDirty = false;
		}

		private static IDictionary<string, object> GetObjectValues(object source)
		{
			Dictionary<string, object> values = new Dictionary<string, object>();
			PXUIFieldAttribute currentAttribute;
			foreach (System.Reflection.PropertyInfo field in source.GetType().GetProperties())
			{
				currentAttribute = Attribute.GetCustomAttribute(field,
					typeof(PXUIFieldAttribute), true) as PXUIFieldAttribute;
				if (currentAttribute != null && !string.IsNullOrEmpty(currentAttribute.DisplayName))
					values.Add(field.Name, field.GetValue(source, null));
			}
			return values;
		}

		private PXResultset<CRFilterTemplate> GetAllDefaultTemplates()
		{
			return new PXSelect<CRFilterTemplate,
				Where<CRFilterTemplate.isDefault, Equal<BoolTrue>,
				And<CRFilterTemplate.graphType, Equal<Required<CRFilterTemplate.graphType>>>>>(this).
				Select(this.graphTypeName);
		}

		private CRFilterTemplate GetAnyTemplate()
		{
			return Templates.View.SelectSingle() as CRFilterTemplate;
		}

		private object GetKeyValue(object row)
		{
			return itemsKeyProperty != null ? itemsKeyProperty.GetValue(row, null) : null;
		}

		private static PropertyInfo SearchKeyProperty(Type table)
		{
			PXDBFieldAttribute currentAttribute;
			foreach (PropertyInfo item in table.GetProperties())
			{
				currentAttribute = Attribute.GetCustomAttribute(item, typeof(PXDBFieldAttribute), true) as PXDBFieldAttribute;
				if (currentAttribute != null && currentAttribute.IsKey) return item;
			}
			return null;
		}

		private IEnumerable SelectItems(PXView view)
		{
			List<PXFilterRow> list = new List<PXFilterRow>();
			foreach (CRFilterItem item in Filters.Select())
				if ((item.IsUsed ?? false) && !string.IsNullOrEmpty(item.DataField))
					list.Add(new PXFilterRow(item.DataField, ParsePXCondition(item.Condition),
						item.Value, item.Value2));
			int startRow = 0;
			int totalRows = 0;
			return view.Select(null, null, null, null, null,
				list.ToArray(), ref startRow,
				0, ref totalRows);
		}

		private PXCondition ParsePXCondition(string value)
		{
			return Enum.IsDefined(typeof(PXCondition), value) ?
				(PXCondition)Enum.Parse(typeof(PXCondition), value) :
				PXCondition.ISNOTNULL;
		}

		private static Dictionary<string, string> GetFieldNamesAndValues(BqlCommand command)
		{
			Dictionary<string, string> dic = new Dictionary<string, string>();
			bool isFirstTable = true;
			PXUIFieldAttribute currentAttribute;
			string currentValue;
			foreach (Type item in command.GetTables())
			{
				foreach (System.Reflection.PropertyInfo field in item.GetProperties())
				{
					currentAttribute = Attribute.GetCustomAttribute(field,
						typeof(PXUIFieldAttribute), true) as PXUIFieldAttribute;
					if (currentAttribute != null && currentAttribute.Filterable)
					{
						currentValue = string.Format(isFirstTable ? "{0}__{1}" : "{0}.{1}", item.Name, field.Name);
						if (!string.IsNullOrEmpty(currentAttribute.DisplayName))
							dic.Add(currentValue, currentAttribute.DisplayName);
						else dic.Add(currentValue, currentValue);
					}
				}
				isFirstTable = false;
			}
			return dic;
		}

		protected void SafetyPersist(PXCache cache, params PXDBOperation[] operations)
		{
			if (operations.Length < 1) return;
			bool isAborted = false;
			try
			{
				using (PXTransactionScope tscope = new PXTransactionScope())
				{
					foreach (PXDBOperation operation in operations)
						cache.Persist(operation);
					tscope.Complete();
				}
			}
			catch (Exception)
			{
				isAborted = true;
				throw;
			}
			finally
			{
				cache.Persisted(isAborted);
			}
		}

		private void CopyFilterItem(CRFilterItem source, CRFilterItem target)
		{
			target.IsUsed = source.IsUsed;
			target.DataField = source.DataField;
			target.Condition = source.Condition;
			target.Value = source.Value;
			target.Value2 = source.Value2;
		}

		private void CopyTemplate(CRFilterTemplate source, CRFilterTemplate target)
		{
			target.Name = source.Name;
			target.IsDefault = source.IsDefault;
			target.IsShared = source.IsShared;
		}

		private void InsertNewTemplate()
		{
			if (Templates.Cache.GetStatus(Templates.Current) != PXEntryStatus.Inserted)
			{
				Templates.Cache.SetStatus(Templates.Current, PXEntryStatus.Notchanged);
				CRFilterTemplate newTemplate = new CRFilterTemplate();
				CopyTemplate(Templates.Current, newTemplate);
				CRFilterItem newItem;
				PXEntryStatus itemStatus;
				foreach (CRFilterItem item in Filters.Select())
				{
					itemStatus = Filters.Cache.GetStatus(item);
					if (itemStatus == PXEntryStatus.Notchanged || itemStatus == PXEntryStatus.Inserted ||
						itemStatus == PXEntryStatus.Updated)
					{
						newItem = Filters.Insert(new CRFilterItem());
						CopyFilterItem(item, newItem);
					}
					Filters.Cache.SetStatus(item, PXEntryStatus.Notchanged);
				}
				Templates.Insert(newTemplate);
			}
		}

		private void CorrectDefaultValues()
		{
			if ((bool)Templates.Current.IsDefault)
				foreach (CRFilterTemplate item in Templates.Select())
					if (item.FilterTemplateID != Templates.Current.FilterTemplateID && (bool)item.IsDefault)
					{
						item.IsDefault = false;
						Templates.Update(item);
					}
		}

		private void CorrectGraphType()
		{
			foreach (CRFilterTemplate item in Templates.Cache.Inserted)
				item.GraphType = graphTypeName;
		}

		private void UpdateExistingTemplate(CRFilterTemplate templateToUpdate)
		{
			if (Templates.Current.FilterTemplateID < 0 ||
				Templates.Current.FilterTemplateID != templateToUpdate.FilterTemplateID)
			{
				CopyTemplate(Templates.Current, templateToUpdate);
				Templates.Cache.SetStatus(Templates.Current, PXEntryStatus.Notchanged);
				Templates.Current = templateToUpdate;
				Templates.Update(templateToUpdate);
			}
		}
		#endregion

		#region Filter Items Grid Handlers
		protected virtual void CRFilterItem_DataField_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row is CRFilterItem && string.IsNullOrEmpty(e.NewValue as string))
				throw new PXSetPropertyException(Messages.BadFilterDataField);
		}

		protected virtual void CRFilterItem_Condition_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row is CRFilterItem && string.IsNullOrEmpty(e.NewValue as string))
				throw new PXSetPropertyException(Messages.BadFilterCondition);
		}

		protected virtual void CRFilterItem_DataField_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			propertyListAtt.FieldSelecting(sender, e);
		}

		protected virtual void CRFilterItem_Condition_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, "Description", null,
				-1, null, conditionValues, conditionNames, (bool?)true);
		}

		protected virtual void CRFilterItem_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CRFilterItem row = e.Row as CRFilterItem;
			if (row != null && string.IsNullOrEmpty(row.Value))
			{
				string fieldName = FindFieldName(row.DataField, true);
				if (fieldName != null)
				{
					CRPermanentFilterItem permanentRow = PermanentFilters.Search
						<CRPermanentFilterItem.dataField>(fieldName);
					if (permanentRow != null)
					{
						row.Value = permanentRow.Value;
						if (row.IsUsed ?? false) row.IsUsed = !string.IsNullOrEmpty(permanentRow.Value);
					}
				}
			}
		}
		#endregion
	}
	#endregion
}

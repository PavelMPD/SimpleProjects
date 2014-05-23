// This File is Distributed as Part of Acumatica Shared Source Code 
/* ---------------------------------------------------------------------*
*                               Acumatica Inc.                          *
*              Copyright (c) 1994-2011 All rights reserved.             *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ProjectX PRODUCT.        *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Collections;
using System.Threading;
using PX.Common;
using PX.Data.EP;
using PX.SM;
using PX.Web.UI;
using PX.Api;

namespace PX.Data
{
	#region PXNameAttribute
	/// <summary>
	/// Base class for PXCacheName and PXViewName. 
	/// Do not use directly.
	/// </summary>
	public class PXNameAttribute : Attribute
	{
		public string Name
		{
			get { return this._name; }
		}
		protected string _name;
		public virtual string GetName()
		{
			string prefix;
			uint number;
			return PXMessages.Localize(_name, out number, out prefix);
		}
		public PXNameAttribute(string name)
		{
			_name = name;
		}
	}
	#endregion

	#region PXCacheNameAttribute
	/// <summary>
	/// User friendly name of DAC class.
	/// </summary>
	public class PXCacheNameAttribute : PXNameAttribute
	{
		public PXCacheNameAttribute(string name)
			: base(name)
		{
		}

		public virtual string GetName(object row)
		{
			return GetName();
		}
	}
	#endregion

	#region PXViewNameAttribute
	/// <summary>
	/// User friendly name of graph View.
	/// </summary>
	public class PXViewNameAttribute : PXNameAttribute
	{
		public PXViewNameAttribute(string name)
			: base(name)
		{
		}
	}
	#endregion

	#region PXEMailSourceAttibute
	[AttributeUsage(AttributeTargets.Class)]
	public class PXEMailSourceAttribute : Attribute
	{
		private readonly Type[] _types;

		public PXEMailSourceAttribute() { }

		public PXEMailSourceAttribute(params Type[] types)
		{
			_types = types;
		}

		public Type[] Types
		{
			get { return _types ?? new Type[0]; }
		}
	}
	#endregion

	#region PXHiddenAttribute
	/// <summary>
	/// Allows to hide DAC class from reports and API clients.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class PXHiddenAttribute : Attribute
	{
		private bool _ServiceVisible;
		public bool ServiceVisible
		{
			get
			{
				return _ServiceVisible;
			}
			set
			{
				_ServiceVisible = value;
			}
		}

		public Type Target { get; set; }
	}
	#endregion

	#region PXPrimaryGraphBaseAttribute

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public abstract class PXPrimaryGraphBaseAttribute : Attribute
	{
		public abstract Type GetGraphType(PXCache cache, ref object row, bool checkRights, Type preferedType);
        public virtual bool UseParent { get; set; }
    }

	#endregion

	#region PXPrimaryGraphAttribute
	/// <summary>
	/// This attribute is placed on DAC class.
	/// Allows to specify which graph should be used to edit data row.
	/// </summary>
	public class PXPrimaryGraphAttribute : PXPrimaryGraphBaseAttribute
	{
		#region Nested
		private class PrimaryAttributeInfo
		{
			public Type DeclaredType;
			public PXPrimaryGraphBaseAttribute Attribute;

			public PrimaryAttributeInfo(Type type, PXPrimaryGraphBaseAttribute attr)
			{
				DeclaredType = type;
				Attribute = attr;
			}
		}
		#endregion

		private static Dictionary<Type, List<Attribute>> _pgAtts;
		private static readonly object _syncObj = new object();
		protected static Dictionary<Type, List<KeyValuePair<Type, PXPrimaryGraphAttribute>>> _graphDefined;
		protected static Dictionary<Type, List<KeyValuePair<Type, PXPrimaryGraphAttribute>>> GraphDefined
		{
			get
			{
				if (_graphDefined != null) return _graphDefined;

				lock (_syncObj)
				{
					if (_graphDefined != null) return _graphDefined;

					_graphDefined = new Dictionary<Type, List<KeyValuePair<Type, PXPrimaryGraphAttribute>>>();
					foreach (Graph graph in ServiceManager.AllGraphsNotCustomized)
					{
						Type graphType = ServiceManager.GetGraphTypeByFullName(graph.GraphName);
						if (graphType == null) continue;

						foreach (Type extensionType in PXGraph._GetExtensions(graphType).AsEnumerable().Reverse())
						{
							AppendGraphsAttributes(extensionType, graphType);
						}
						AppendGraphsAttributes(graphType, null);
					}
				}
				return _graphDefined;
			}
		}

        protected bool _UseParent = true;
        protected Type _TheOnlyGraph;
		protected Type _TheOnlyDac;
		protected Type[] _GraphTypes;
		protected Type[] _Conditions;
		protected Type[] _DacTypes;
		protected Type _Filter;
		protected Type[] _Filters;
		public virtual Type Filter
		{
			get
			{
				return _Filter;
			}
			set
			{
				_Filter = value;
			}
		}
		public virtual Type[] Filters
		{
			get
			{
				return _Filters;
			}
			set
			{
				_Filters = value;
			}
		}
        public override bool UseParent
        {
            get { return _UseParent; }
            set { _UseParent = value; }
        }

		public PXPrimaryGraphAttribute(Type type)
		{
			if (type.IsSubclassOf(typeof(PXGraph))) _TheOnlyGraph = type;
			if (type.GetInterface(typeof(IBqlTable).FullName) != null) _TheOnlyDac = type;
		}
		public PXPrimaryGraphAttribute(Type[] types, Type[] conditions)
		{
			List<Type> dacs = new List<Type>();
			List<Type> graphs = new List<Type>();
			foreach (Type type in types)
			{
				if (type.IsSubclassOf(typeof(PXGraph))) graphs.Add(type);
				if (type.GetInterface(typeof(IBqlTable).FullName) != null) dacs.Add(type);
			}

			if (graphs.Count >= dacs.Count)	_GraphTypes = graphs.ToArray();
			else _DacTypes = dacs.ToArray();
			_Conditions = conditions;
		}

		public override Type GetGraphType(PXCache cache, ref object row, bool checkRights, Type preferedType)
		{
			if (_TheOnlyGraph != null)
			{
				return _TheOnlyGraph;
			}
			if (_GraphTypes == null ||
				_GraphTypes.Length == 0 ||
				_Conditions == null
				)
			{
				return null;
			}

			for (int i = 0; i < _GraphTypes.Length && i < _Conditions.Length; i++)
			{
				if (!typeof(PXGraph).IsAssignableFrom(_GraphTypes[i]) || checkRights && !PXAccess.VerifyRights(_GraphTypes[i]))
				{
					continue;
				}
				if (typeof(IBqlWhere).IsAssignableFrom(_Conditions[i]))
				{
					IBqlWhere where = (IBqlWhere)Activator.CreateInstance(_Conditions[i]);
					bool? result = null;
					object value = null;
					where.Verify(cache, row, new List<object>(), ref result, ref value);
					if (result == true && (preferedType == null || preferedType.IsAssignableFrom(_GraphTypes[i])))
					{
						if (_Filters != null && i < _Filters.Length)
						{
							_Filter = Filters[i];
						}
						return _GraphTypes[i];
					}
				}
				else if (typeof(BqlCommand).IsAssignableFrom(_Conditions[i]))
				{
					BqlCommand command = (BqlCommand)Activator.CreateInstance(_Conditions[i]);
					PXView view = new PXView(cache.Graph, false, command);
					object item = view.SelectSingleBound(new object[] { row });
					if (item != null && (preferedType == null || preferedType.IsAssignableFrom(_GraphTypes[i])))
					{
						row = item;

						if (row is PXResult)
						{
							row = ((PXResult)row)[0];
						}

						if (_Filters != null && i < _Filters.Length)
						{
							_Filter = Filters[i];
						}
						return _GraphTypes[i];
					}
				}
			}
			if (row == null && _GraphTypes != null && _GraphTypes.Length > 0)
			{
				return _GraphTypes[_GraphTypes.Length - 1];
			}
			return null;
		}
		public virtual Type ValidateGraphType(PXCache cache, Type graphType, Type dacType, ref object row, bool checkRights)
		{
			if (_TheOnlyDac != null)
			{
				return _TheOnlyDac;
			}
			if (_DacTypes == null ||
				_DacTypes.Length == 0 ||
				_Conditions == null
				)
			{
				return null;
			}

			for (int i = 0; i < _DacTypes.Length && i < _Conditions.Length; i++)
			{
				if (!dacType.IsAssignableFrom(_DacTypes[i])) continue;
				if (!typeof(IBqlTable).IsAssignableFrom(_DacTypes[i]) || checkRights && !PXAccess.VerifyRights(graphType))
				{
					continue;
				}
				if (typeof(IBqlWhere).IsAssignableFrom(_Conditions[i]))
				{
					IBqlWhere where = (IBqlWhere)Activator.CreateInstance(_Conditions[i]);
					bool? result = null;
					object value = null;
					where.Verify(cache, row, new List<object>(), ref result, ref value);
					if (result == true)
					{
						if (_Filters != null && i < _Filters.Length)
						{
							_Filter = Filters[i];
						}
						return graphType;
					}
				}
				else if (typeof(BqlCommand).IsAssignableFrom(_Conditions[i]))
				{
					BqlCommand command = (BqlCommand)Activator.CreateInstance(_Conditions[i]);
					PXView view = new PXView(cache.Graph, false, command);
					object item = view.SelectSingleBound(new object[] { row });
					if (item != null)
					{
						row = item;

						if (row is PXResult)
						{
							row = ((PXResult)row)[0];
						}

						if (_Filters != null && i < _Filters.Length)
						{
							_Filter = Filters[i];
						}
						return graphType;
					}
				}
			}
			return null;
		}

		public static PXPrimaryGraphBaseAttribute FindPrimaryGraph(PXCache cache, out Type graphType)
		{
			Object row = null;
			Type declaredType = null;
			PXCache declaredCache = null;

			return FindPrimaryGraph(cache, ref row, out graphType, out declaredType, out declaredCache);
		}
		public static PXPrimaryGraphBaseAttribute FindPrimaryGraph(PXCache cache, ref Object row, out Type graphType)
		{
			return FindPrimaryGraph(cache, true, ref row, out graphType);
		}
		public static PXPrimaryGraphBaseAttribute FindPrimaryGraph(PXCache cache, Boolean checkRights, ref Object row, out Type graphType)
		{
			Type declaredType = null;
			PXCache declaredCache = null;
			return FindPrimaryGraph(cache, checkRights, ref row, out graphType, out declaredType, out declaredCache);
		}
		public static PXPrimaryGraphBaseAttribute FindPrimaryGraph(PXCache cache, ref Object row, out Type graphType, out Type declaredType, out PXCache declaredCache)
		{
			return FindPrimaryGraph(cache, true, ref row, out graphType, out declaredType, out declaredCache);
		}
		public static PXPrimaryGraphBaseAttribute FindPrimaryGraph(PXCache cache, Type preferedType, ref Object row, out Type graphType, out Type declaredType, out PXCache declaredCache)
		{
			return FindPrimaryGraph(cache, preferedType, true, ref row, out graphType, out declaredType, out declaredCache);
		}
		public static PXPrimaryGraphBaseAttribute FindPrimaryGraph(PXCache cache, Boolean checkRights, ref Object row, out Type graphType, out Type declaredType, out PXCache declaredCache)
		{
			return FindPrimaryGraph(cache, null, checkRights, ref row, out graphType, out declaredType, out declaredCache);
		}
		public static PXPrimaryGraphBaseAttribute FindPrimaryGraph(PXCache cache, Type preferedType, Boolean checkRights, ref Object row, out Type graphType, out Type declaredType, out PXCache declaredCache)
		{
			Type origType;
			graphType = null;
			declaredCache = cache;
			declaredType = declaredCache.GetItemType();
			while (declaredCache != null && declaredType != null)
			{
				origType = declaredType;

				//search graphs
				while (declaredType != null)
				{
					if (GraphDefined.ContainsKey(declaredType))
					{
						foreach (KeyValuePair<Type, PXPrimaryGraphAttribute> pair in GraphDefined[declaredType])
						{
							Type dacType = pair.Value.ValidateGraphType(cache, pair.Key, declaredType, ref row, checkRights);
							if (dacType != null)
							{
								graphType = pair.Key;
								return pair.Value;
							}
						}
					}
					declaredType = declaredType.BaseType;
				}

				declaredType = origType;
				PXPrimaryGraphBaseAttribute attribute = null;

				//search extensions
				foreach (Type ext in cache.GetExtensionTypes().Reverse())
				{
					foreach(PrimaryAttributeInfo info in GetPrimaryAttribute(ext, true))
					{
						if (Unwrap(info, declaredCache, checkRights, preferedType, ref row, out graphType, out declaredType, out attribute)) return attribute;
					}
				}

				//search dacs
			    bool UseParent = true;
				foreach(PrimaryAttributeInfo info in GetPrimaryAttribute(declaredType, true))
				{
				    UseParent = UseParent && info.Attribute.UseParent;
					if (Unwrap(info, declaredCache, checkRights, preferedType, ref row, out graphType, out declaredType, out attribute)) return attribute;
				}
				bool found = false;
				//searching parent
				bool clear = false;
				foreach (PXEventSubscriberAttribute attr in declaredCache.GetAttributes(row, null))
				{
					if (attr is PXParentAttribute && UseParent)
					{
						found = true;
						if (row == null)
						{
							declaredType = PXParentAttribute.GetParentType(declaredCache);
							break;
						}
						else
						{
							PXView view = ((PXParentAttribute)attr).GetParentSelect(declaredCache);
							row = view.SelectSingleBound(new object[] { row });

							if (row == null)
							{
								clear = true;
								continue;
							}
							else
							{
								clear = false;
								declaredType = row.GetType();
								declaredCache = cache.Graph.Caches[declaredType];
								break;
							}
						}
					}
					if (clear)
					{
						declaredType = null;
						declaredCache = null;
					}
				}
				if (!found) return null;
			}
			return null;
		}
		
		private static IEnumerable<Attribute> GetAssemblyAttribute(Type type)
		{
			lock (_syncObj)
			{
				if (_pgAtts == null)
				{
					_pgAtts = new Dictionary<Type, List<Attribute>>();
					foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
					{
						// ignore some assemblies including dynamic ones
						if (!PXSubstManager.IsSuitableTypeExportAssembly(a, true))
						{
							continue;
						}

						try
						{
							foreach (PXDACDescriptionAttribute att in a.GetCustomAttributes(typeof(PXDACDescriptionAttribute), true))
							{
								List<Attribute> list;
								if (!_pgAtts.TryGetValue(att.Target, out list))
								{
									list = new List<Attribute>();
									_pgAtts[att.Target] = list;
								}
								list.Add(att.Attribute);
							}
						}
						catch { }
					}
				}
				List<Attribute> res;
				if (_pgAtts.TryGetValue(type, out res)) return res;
				return new Attribute[0];
			}
		}
		private static List<PrimaryAttributeInfo> GetPrimaryAttribute(Type declaredType, Boolean searchBase)
		{
			List<PrimaryAttributeInfo> list = new List<PrimaryAttributeInfo>();
			if (declaredType.IsDefined(typeof(PXPrimaryGraphBaseAttribute), true))
			{
				while (declaredType != null)
				{
					Boolean isAvailable = false;
					if (typeof(IBqlTable).IsAssignableFrom(declaredType)) isAvailable = true;
					else if (declaredType.IsSubclassOf(typeof(PXGraph))) isAvailable = true;
					else if (declaredType.IsSubclassOf(typeof(PXCacheExtension))) isAvailable = true;
					else if (declaredType.IsSubclassOf(typeof(PXGraphExtension))) isAvailable = true;

					if (isAvailable)
					{
						List<Attribute> attributes = new List<Attribute>();
						attributes.AddRange(GetAssemblyAttribute(declaredType));
						attributes.AddRange(declaredType.GetCustomAttributes(false).Cast<Attribute>());
						foreach (Attribute attr in attributes)
						{
							if (attr is PXPrimaryGraphBaseAttribute)
							{
								list.Add( new PrimaryAttributeInfo(declaredType, attr as PXPrimaryGraphBaseAttribute));
							}
						}
					}

					
					declaredType = searchBase ? declaredType.BaseType : null;
				}
			}
			return list;
		}
		private static void AppendGraphsAttributes(Type type, Type primaryType)
		{
			foreach (PrimaryAttributeInfo info in GetPrimaryAttribute(type, false))
			{
				PXPrimaryGraphAttribute attr = info.Attribute as PXPrimaryGraphAttribute;
				if (attr == null || (attr._TheOnlyDac == null && (attr._DacTypes == null || attr._DacTypes.Length <= 0))) return;

				IEnumerable<Type> types = attr._TheOnlyDac != null ? new Type[] { attr._TheOnlyDac} : attr._DacTypes;
				foreach (Type dacType in types)
				{
					List<KeyValuePair<Type, PXPrimaryGraphAttribute>> list;
					if (!_graphDefined.ContainsKey(dacType)) list = _graphDefined[dacType] = list = new List<KeyValuePair<Type, PXPrimaryGraphAttribute>>();
					else list = _graphDefined[dacType];

					if (!list.Any(p => p.Key == primaryType)) list.Add(new KeyValuePair<Type, PXPrimaryGraphAttribute>(primaryType ?? type, attr));
				}
			}
		}
		private static Boolean Unwrap(PrimaryAttributeInfo info, PXCache cache, Boolean checkRights, Type preferedType, ref Object row, out Type graphType, out Type declaredType, out PXPrimaryGraphBaseAttribute attribute)
		{
			attribute = info.Attribute;
			declaredType = info.DeclaredType;

			if (preferedType != null)
			{
				Object copy = row;
				graphType = attribute.GetGraphType(cache, ref copy, checkRights, preferedType);
				if (graphType != null)
				{
					row = copy;
					return true;
				}
			}
			graphType = attribute.GetGraphType(cache, ref row, checkRights, null);
			return graphType != null;			
		}
	}
	#endregion

	#region PXDACDescriptionAttribute
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class PXDACDescriptionAttribute : Attribute
	{
		private readonly Type _target;
		private readonly Attribute _attribute;

		public PXDACDescriptionAttribute(Type target, Attribute attribute)
		{
			if (target == null) throw new ArgumentNullException("target");
			if (attribute == null) throw new ArgumentNullException("attribute");

			_target = target;
			_attribute = attribute;
		}

		public Type Target
		{
			get { return _target; }
		}

		public Attribute Attribute
		{
			get { return _attribute; }
		}
	}
	#endregion

	#region PXNotCleanableCache<TNode>

	internal class PXNotCleanableCache<TNode> : PXCache<TNode>
		where TNode : class, IBqlTable, new()
	{
		public PXNotCleanableCache(PXGraph graph)
			: base(graph)
		{
		}

		public PXNotCleanableCache(PXGraph graph, PXCache source)
			: this(graph)
		{
			foreach (var item in source.Cached)
			{
				Insert(item);
				SetStatus(item, source.GetStatus(item));
			}
		}

		public override void Clear()
		{
			//base.Clear();
		}

		public static PXNotCleanableCache<TNode> Attach(PXGraph graph)
		{
			var state = new PXNotCleanableCache<TNode>(graph);
			state.Load();
			graph.Caches[typeof(TNode)] = state;
			graph.SynchronizeByItemType(state);
			return state;
		}

		public static PXNotCleanableCache<TNode> Attach(PXGraph graph, PXCache cache)
		{
			var state = new PXNotCleanableCache<TNode>(graph, cache);
			state.Load();
			graph.Caches[typeof(TNode)] = state;
			graph.SynchronizeByItemType(state);
			return state;
		}
	}

	#endregion

	#region PXLineNbrMarkerAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class PXLineNbrMarkerAttribute : PXEventSubscriberAttribute { }

	#endregion

	#region PXImportAttribute
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class PXImportAttribute : PXViewExtensionAttribute
	{
		#region PXContentBag
		[Serializable]
		public class PXContentBag : IBqlTable
		{
			#region FileExtension
			public abstract class fileExtension : IBqlField
			{
			}
			protected String _FileExtension;
			[PXString]
			public virtual String FileExtension
			{
				get
				{
					return this._FileExtension;
				}
				set
				{
					this._FileExtension = value;
				}
			}
			#endregion

			#region Data
			public abstract class data : IBqlField
			{
			}
			protected object _Data;
			[PXDBVariant]
			public virtual object Data
			{
				get
				{
					return this._Data;
				}
				set
				{
					this._Data = value;
				}
			}
			#endregion

			#region Loaded
			public abstract class loaded : IBqlField
			{
			}
			protected Boolean? _Loaded;
			[PXBool]
			[PXUIField(Visible = false)]
			public virtual Boolean? Loaded
			{
				get
				{
					return this._Loaded;
				}
				set
				{
					this._Loaded = value;
				}
			}
			#endregion
		}
		#endregion

		#region PXImportSettings
		[Serializable]
		public class PXImportSettings : IBqlTable
		{
			#region Index
			public abstract class index : IBqlField
			{
			}
			private Int32? _Index;
			[PXDBIdentity(IsKey = true)]
			[PXUIField(Visible = false)]
			public Int32? Index
			{
				get
				{
					return this._Index;
				}
				set
				{
					this._Index = value;
				}
			}
			#endregion

			#region ViewName

			public abstract class viewName : IBqlField { }

			protected String _ViewName;

			[PXString]
			[PXUIField(Visible = false)]
			public virtual String ViewName
			{
				get { return _ViewName; }
				set { _ViewName = value; }
			}

			#endregion

			#region FileExtension
			public abstract class fileExtension : IBqlField
			{
			}
			protected String _FileExtension;
			[PXString]
			[PXUIField(DisplayName = "File Extension")]
			public virtual String FileExtension
			{
				get
				{
					return this._FileExtension;
				}
				set
				{
					this._FileExtension = value;
				}
			}
			#endregion

			#region Data
			public abstract class data : IBqlField
			{
			}
			protected object _Data;
			[PXDBVariant]
			[PXUIField(Visible = false)]
			public virtual object Data
			{
				get
				{
					return this._Data;
				}
				set
				{
					this._Data = value;
				}
			}
			#endregion

			#region NullValue
			public abstract class nullValue : IBqlField
			{
			}
			protected String _NullValue;
			[PXString]
			[PXUIField(DisplayName = "Null Value")]
			public virtual String NullValue
			{
				get
				{
					return this._NullValue;
				}
				set
				{
					this._NullValue = value;
				}
			}
			#endregion

			#region Culture
			public abstract class culture : IBqlField
			{
			}
			protected Int32? _Culture;
			[PXInt]
			[PXUIField(DisplayName = "Culture")]
			public Int32? Culture
			{
				get
				{
					return this._Culture;
				}
				set
				{
					this._Culture = value;
				}
			}
			#endregion

			#region OnlyInsert
			public abstract class onlyInsert : IBqlField
			{
			}
			protected Boolean _OnlyInsert;
			[PXBool]
			[PXUIField(DisplayName = "Do not update existing records")]
			public virtual Boolean OnlyInsert
			{
				get
				{
					return this._OnlyInsert;
				}
				set
				{
					this._OnlyInsert = value;
				}
			}
			#endregion
		}
		#endregion

		#region CSVSettings
		[Serializable]
		public sealed class CSVSettings : PXImportSettings
		{
			#region ViewName

			public new abstract class viewName : IBqlField { }

			#endregion

			#region Separator
			public abstract class separator : IBqlField
			{
			}

			private String _Separator;
			[PXString(50)]
			[PXUIField(DisplayName = "Separator Chars")]
			public String Separator
			{
				get
				{
					return this._Separator;
				}
				set
				{
					this._Separator = value;
				}
			}
			#endregion

			#region CodePage
			public abstract class codePage : IBqlField
			{
			}

			private Int32? _CodePage;
			[PXInt]
			[PXUIField(DisplayName = "Encoding")]
			public Int32? CodePage
			{
				get
				{
					return this._CodePage;
				}
				set
				{
					this._CodePage = value;
				}
			}
			#endregion
		}
		#endregion

		#region XLSXSettings
		[Serializable]
		public sealed class XLSXSettings : PXImportSettings
		{
			#region ViewName

			public new abstract class viewName : IBqlField { }

			#endregion
		}
		#endregion

		#region PXImportColumnsSettings
		[Serializable]
		public class PXImportColumnsSettings : IBqlTable
		{
			#region Index
			public abstract class index : IBqlField
			{
			}
			private Int32? _Index;
			[PXDBIdentity(IsKey = true)]
			[PXUIField(Visible = false)]
			public Int32? Index
			{
				get
				{
					return this._Index;
				}
				set
				{
					this._Index = value;
				}
			}
			#endregion

			#region ViewName

			public abstract class viewName : IBqlField { }

			protected String _ViewName;

			[PXString]
			[PXUIField(Visible = false)]
			public virtual String ViewName
			{
				get { return _ViewName; }
				set { _ViewName = value; }
			}

			#endregion

			#region ColumnIndex
			public abstract class columnIndex : IBqlField
			{
			}
			private Int32? _ColumnIndex;
			[PXDBIdentity()]
			[PXUIField(Visible = false)]
			public Int32? ColumnIndex
			{
				get
				{
					return this._ColumnIndex;
				}
				set
				{
					this._ColumnIndex = value;
				}
			}
			#endregion

			#region ColumnName
			public abstract class columnName : IBqlField
			{
			}
			private String _ColumnName;
			[PXString]
			[PXUIField(DisplayName = "Column Name", Enabled = false)]
			public String ColumnName
			{
				get
				{
					return this._ColumnName;
				}
				set
				{
					this._ColumnName = value;
				}
			}
			#endregion

			#region PropertyName
			public abstract class propertyName : IBqlField
			{
			}
			private String _PropertyName;
			[PXString]
			[PXUIField(DisplayName = "Property Name")]
			public String PropertyName
			{
				get
				{
					return this._PropertyName;
				}
				set
				{
					this._PropertyName = value;
				}
			}
			#endregion
		}
		#endregion

		#region CommonSettings

		public struct CommonSettings
		{
			private readonly byte[] _content;
			private readonly string _nullValue;
			private readonly string _culture;
			private readonly bool _onlyInsert;

			public CommonSettings(byte[] content, string nullValue, string culture, bool onlyInsert)
			{
				_content = content;
				_nullValue = nullValue;
				_culture = culture;
				_onlyInsert = onlyInsert;
			}

			public byte[] Content
			{
				get { return _content; }
			}

			public string NullValue
			{
				get { return _nullValue; }
			}

			public string Culture
			{
				get { return _culture; }
			}

			public bool OnlyInsert
			{
				get { return _onlyInsert; }
			}
		}

		#endregion

		#region Interfaces

		public interface IHardImporter
		{
			void Process(CommonSettings commonSettings, string[] columnsMap);
		}

		public interface IPXImportWizard
		{
			bool TryUploadData(byte[] content, string ext);
			void RunWizard();
			void PreRunWizard();
			event CreateImportRowEventHandler OnCreateImportRow;
			event RowImportingEventHandler OnRowImporting;
			event RowImportedEventHandler OnRowImported;
		}

		public sealed class CreateImportRowEventArguments
		{
			private readonly IDictionary _keys;
			private readonly IDictionary _values;
			private readonly bool _dontUpdateExistRecords;

			public CreateImportRowEventArguments(IDictionary keys, IDictionary values, bool dontUpdateExistRecords)
			{
				if (keys == null) throw new ArgumentNullException("keys");
				if (values == null) throw new ArgumentNullException("values");

				_keys = keys;
				_values = values;
				_dontUpdateExistRecords = dontUpdateExistRecords;
			}

			public IDictionary Keys
			{
				get { return _keys; }
			}

			public IDictionary Values
			{
				get { return _values; }
			}

			public bool Cancel { get; set; }

			public bool DontUpdateExistRecords
			{
				get { return _dontUpdateExistRecords; }
			}
		}

		public sealed class RowImportingEventArguments
		{
			private readonly object _row;

			public RowImportingEventArguments(object row)
			{
				_row = row;
			}

			public bool Cancel { get; set; }

			public object Row
			{
				get { return _row; }
			}
		}

		public sealed class RowImportedEventArguments
		{
			private readonly object _row;
			private readonly object _oldRow;

			public RowImportedEventArguments(object row, object oldRow)
			{
				if (row == null) throw new ArgumentNullException("row");

				_row = row;
				_oldRow = oldRow;
			}

			public bool Cancel { get; set; }

			public object Row
			{
				get { return _row; }
			}

			public object OldRow
			{
				get { return _oldRow; }
			}
		}

		public delegate void CreateImportRowEventHandler(CreateImportRowEventArguments args);

		public delegate void RowImportingEventHandler(RowImportingEventArguments args);

		public delegate void RowImportedEventHandler(RowImportedEventArguments args);
		#endregion

		#region CSVImporter

		public sealed class CSVImporter<TBatchTable> : PXImporter<TBatchTable>
			where TBatchTable : class, IBqlTable, new()
		{
			private static int[] _codePages;
			private static string[] _codePagesNames;

			static CSVImporter()
			{
				InitCodePagesInfo();
			}

			public CSVImporter(PXCache itemsCache, string viewName, bool rollbackPreviousImport)
				: base(itemsCache, viewName, typeof(CSVSettings), typeof(CSVSettings.viewName), ImportCSVSettingsName, rollbackPreviousImport)
			{
				itemsCache.Graph.FieldSelecting.AddHandler<CSVSettings.codePage>(GetCodePages);
			}

			private static void GetCodePages(PXCache sender, PXFieldSelectingEventArgs args)
			{
				args.ReturnState = PXIntState.CreateInstance(args.ReturnState, "CodePage", null,
					null, null, null, _codePages, _codePagesNames, typeof(Int32), Encoding.ASCII.CodePage);
			}

			private static void InitCodePagesInfo()
			{
				var codePages = new List<int>();
				var codePagesNames = new List<string>();
				var infoList = new List<EncodingInfo>(Encoding.GetEncodings());
				infoList.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, true));
				foreach (var info in infoList)
				{
					codePages.Add(info.CodePage);
					codePagesNames.Add(info.DisplayName);
				}
				_codePages = codePages.ToArray();
				_codePagesNames = codePagesNames.ToArray();
			}

			protected override IContentReader GetReader(byte[] content)
			{
				var commonSettings = (CSVSettings)ImportSettingsCurrent;
				return new CSVReader(content, commonSettings.CodePage ?? Encoding.ASCII.CodePage)
						{
							Separator = commonSettings.Separator
						};
			}

			protected override PXImportSettings CreateDefaultCommonSettings()
			{
				return new CSVSettings()
						{
							Separator = ",",
							CodePage = Encoding.ASCII.CodePage
						};
			}
		}

		#endregion

		#region XLSXImporter

		public sealed class XLSXImporter<TBatchTable> : PXImporter<TBatchTable>
			where TBatchTable : class, IBqlTable, new()
		{
			public XLSXImporter(PXCache itemsCache, string viewName, bool rollbackPreviousImport)
				: base(itemsCache, viewName, typeof(XLSXSettings), typeof(XLSXSettings.viewName), ImportXLSXSettingsName, rollbackPreviousImport)
			{
			}

			protected override IContentReader GetReader(byte[] content)
			{
				return new XLSXReader(content);
			}

			protected override PXImportSettings CreateDefaultCommonSettings()
			{
				return new XLSXSettings()
						{
							Culture = CultureInfo.CurrentCulture.LCID
						};
			}
		}

		#endregion

		#region PXImporter

		public abstract class PXImporter<TBatchTable> : IPXImportWizard, IHardImporter
			where TBatchTable : class, IBqlTable, new()
		{
			#region StateInfo

			protected class StateInfo
			{
				private readonly PXFieldState _state;

				private string _inputeMask;

				private Dictionary<string, object> _labelValuePairs;

				public StateInfo(PXFieldState state)
				{
					if (state == null) throw new ArgumentNullException("state");

					_state = state;
				}

				public string InputMask
				{
					get
					{
						if (_inputeMask == null && _state is PXStringState)
						{
							_inputeMask = ((PXStringState)_state).InputMask;
							var segmentState = _state as PXSegmentedState;
							if (segmentState != null && !string.IsNullOrEmpty(segmentState.Wildcard))
							{
								var arr = _inputeMask.Split('|');
								if (arr.Length < 2)
								{
									_inputeMask += "||";
								}
								else if (arr.Length < 3)
								{
									_inputeMask += "|";
								}
								_inputeMask += segmentState.Wildcard;
							}
						}
						return _inputeMask;
					}
				}

				public Type DataType
				{
					get { return _state.DataType; }
				}

				public string ViewName
				{
					get { return _state.ViewName; }
				}

				public Dictionary<string, object> LabelValuePairs
				{
					get
					{
						if (_labelValuePairs == null)
						{
							_labelValuePairs = new Dictionary<string, object>();
							var strState = _state as PXStringState;
							if (strState != null && strState.AllowedValues != null)
							{
								for (int i = 0; i < strState.AllowedValues.Length; i++)
								{
									var label = strState.AllowedLabels[i];
									var val = strState.AllowedValues[i];
									_labelValuePairs[label] = val;
								}
							}
							var intState = _state as PXIntState;
							if (intState != null && intState.AllowedValues != null)
							{
								for (int i = 0; i < intState.AllowedValues.Length; i++)
								{
									var label = intState.AllowedLabels[i];
									var val = intState.AllowedValues[i];
									_labelValuePairs[label] = val;
								}
							}
						}
						return _labelValuePairs;
					}
				}
			}

			#endregion

			#region PXCachedView
			private class PXCachedView : PXView
			{
				public PXCachedView(PXCache cache)
					: base(cache.Graph, true,
					BqlCommand.CreateInstance(typeof(Select<>), typeof(TBatchTable)),
					new PXSelectDelegate(delegate() { return cache.Cached; }))
				{
				}

				public bool Contains(IDictionary keys)
				{
					foreach (TBatchTable table in SelectMulti())
					{
						bool equalKeys = true;
						foreach (string key in _Cache.Keys)
						{
							object keyValue = keys[key];
							_Cache.RaiseFieldUpdating(key, null, ref keyValue);
							if (keyValue == null || !_Cache.GetValueExt(table, key).Equals(keyValue))
							{
								equalKeys = false;
								break;
							}
						}
						if (equalKeys) return true;
					}
					return false;
				}
			}
			#endregion

			#region Constants
			private const string _CSV_EXT_NAME = "csv";
			private const string _XLSX_EXT_NAME = "xlsx";

			private const string _ERRORS_MESSAGE = "Import errors";
			#endregion

			#region Fields
			private static readonly bool _isCRSelectable;
			private readonly bool _rollbackPreviousOperation;
			private readonly PXCache _cache;
			private readonly PXCache _backupCache;
			private readonly PXCache _importedCache;
			private readonly PXView _importCommonSettings;
			private readonly PXView _importColumnsSettings;
			private string[] _propertiesNames;
			private string[] _propertiesDisplayNames;
			private ICollection<string> _lineProperties;
			private ICollection<string> _shortLineProperties;

			private readonly IDictionary<Type, IList<Exception>> _exceptions;
			private readonly PXCachedView _cachedItems;
			private readonly string _viewName;

			private static int[] _cultures;
			private static string[] _culturesNames;
			#endregion

			#region Ctors
			static PXImporter()
			{
				_isCRSelectable = typeof(IPXSelectable).IsAssignableFrom(typeof(TBatchTable));
				InitCulturesInfo();
			}

			protected PXImporter(PXCache itemsCache, string viewName, Type commonSettingsType, Type commonSettingsViewNameField,
				string commonSettingsViewName, bool rollbackPreviousImport)
			{
				AssertType<TBatchTable>(itemsCache);
				AssertType<PXImportSettings>(commonSettingsType);

				_rollbackPreviousOperation = rollbackPreviousImport;

				_exceptions = new Dictionary<Type, IList<Exception>>();

				_cache = itemsCache;
				_viewName = viewName;
				_cachedItems = new PXCachedView(itemsCache);
				if (_rollbackPreviousOperation)
				{
					_backupCache = new PXCache<TBatchTable>(itemsCache.Graph);
					_importedCache = new PXCache<TBatchTable>(itemsCache.Graph);
				}
				_importCommonSettings = AddView(itemsCache.Graph, viewName + commonSettingsViewName,
					commonSettingsType,
					BqlCommand.Compose(typeof(Where<,>), commonSettingsViewNameField,
						typeof(Equal<>), typeof(Required<>), commonSettingsViewNameField),
					null);
				_importColumnsSettings = AddView(itemsCache.Graph, viewName + ImportColumnsSettingsName,
						typeof(PXImportColumnsSettings),
						typeof(Where<PXImportColumnsSettings.viewName, Equal<Required<PXImportColumnsSettings.viewName>>>),
						typeof(OrderBy<Asc<PXImportColumnsSettings.index>>));

				_cache.Graph.FieldSelecting.AddHandler<PXImportColumnsSettings.propertyName>(GetTBatchTableProperties);
				_cache.Graph.FieldUpdating.AddHandler<PXImportColumnsSettings.propertyName>(CorrectColumnAssociations);

				_cache.Graph.FieldSelecting.AddHandler(commonSettingsType, "Culture", GetCultures);
			}

			private static void GetCultures(PXCache sender, PXFieldSelectingEventArgs args)
			{
				args.ReturnState = PXIntState.CreateInstance(args.ReturnState, "Culture", null,
					null, null, null, _cultures, _culturesNames, typeof(Int32), Encoding.ASCII.CodePage);
			}

			private static void InitCulturesInfo()
			{
				var culturesList = new List<int>();
				var culturesNamesList = new List<string>();
				var infoList = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.AllCultures));
				infoList.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, true));
				foreach (var info in infoList)
				{
					culturesList.Add(info.LCID);
					culturesNamesList.Add(info.DisplayName);
				}
				_cultures = culturesList.ToArray();
				_culturesNames = culturesNamesList.ToArray();
			}

			private void InitPropertiesInfo()
			{
				if (_propertiesNames == null || _propertiesDisplayNames == null ||
					_lineProperties == null || _shortLineProperties == null)
				{
					ForceInitPropertiesInfo();
				}
			}

			private void ForceInitPropertiesInfo()
			{
				var sortedProperties = new List<KeyValuePair<string, string>>();
				_lineProperties = new List<string>(_cache.Fields.Count);
				_shortLineProperties = new List<string>(_cache.Fields.Count);
				foreach (string field in _cache.Fields)
				{
					var state = _cache.GetStateExt(null, field) as PXFieldState;
					if (state != null)
					{
						if (state.Visible && state.Enabled && state.Visibility != PXUIVisibility.Invisible)
							sortedProperties.Add(new KeyValuePair<string, string>(field, state.DisplayName));
						foreach (var att in _cache.GetAttributes(field))
							if (att is PXLineNbrAttribute || att is PXLineNbrMarkerAttribute)
							{
								switch (Type.GetTypeCode(state.DataType))
								{
									case TypeCode.Int32:
										_lineProperties.Add(field);
										break;
									case TypeCode.Int16:
										_shortLineProperties.Add(field);
										break;
								}
								break;
							}
					}
				}
				sortedProperties.Sort((x, y) => string.Compare(x.Value, y.Value, true));

				_propertiesNames = new string[sortedProperties.Count + 1];
				_propertiesDisplayNames = new string[sortedProperties.Count + 1];

				_propertiesNames[0] = string.Empty;
				_propertiesDisplayNames[0] = string.Empty;
				for (int i = 1; i <= sortedProperties.Count; i++)
				{
					var pair = sortedProperties[i - 1];
					_propertiesNames[i] = pair.Key;
					_propertiesDisplayNames[i] = pair.Value;
				}
			}

			#endregion

			#region Public Methods

			public static IPXImportWizard Create(string fileExt, PXCache itemsCache, string viewName, bool rollbackPreviousImport)
			{
				switch (fileExt)
				{
					case _CSV_EXT_NAME:
						return new CSVImporter<TBatchTable>(itemsCache, viewName, rollbackPreviousImport);
					case _XLSX_EXT_NAME:
						return new XLSXImporter<TBatchTable>(itemsCache, viewName, rollbackPreviousImport);
				}
				return null;
			}

			public bool RollbackPreviousOperation
			{
				get { return _rollbackPreviousOperation; }
			}

			public bool TryUploadData(byte[] content, string ext)
			{
				if (_cache == null || content == null || string.IsNullOrEmpty(ext)) return false;
				SetContent(content, ext);
				return true;
			}

			public void RunWizard()
			{
				if (AskCommonSettings()) AskColumnAssociations();
			}

			public virtual void PreRunWizard()
			{
				_cachedItems.Cache.Graph.Views[_viewName].SelectMulti();
			}

			public event CreateImportRowEventHandler OnCreateImportRow;

			public event RowImportingEventHandler OnRowImporting;

			public event RowImportedEventHandler OnRowImported;

			public IEnumerable<KeyValuePair<Type, IList<Exception>>> Exceptions
			{
				get { return _exceptions; }
			}

			#endregion

			#region Protected Methods

			#region SaftyPerformOperation
			protected delegate void Operation();

			protected bool SaftyPerformOperation(Operation handler)
			{
				if (handler == null) throw new ArgumentNullException("handler");
				bool result = true;
				try
				{
					handler();
				}
				catch (OutOfMemoryException) { throw; }
				catch (StackOverflowException) { throw; }
				catch (Exception e)
				{
					AddException(e);
					result = false;
				}
				return result;
			}
			#endregion

			protected abstract IContentReader GetReader(byte[] content);

			protected virtual void SetValue(TBatchTable result, string fieldName, string value)
			{
				var stringState = _cache.GetStateExt(result, fieldName) as PXStringState;
				if (stringState != null && !string.IsNullOrEmpty(stringState.InputMask))
					value = PX.Common.Mask.Parse(stringState.InputMask, value);

				SaftyPerformOperation(() => _cache.SetValueExt(result, fieldName, value));
			}

			protected void AddException(Exception e)
			{
				Type eType = e.GetType();
				if (!_exceptions.ContainsKey(eType)) _exceptions.Add(eType, new List<Exception>());
				_exceptions[eType].Add(e);
			}

			protected PXImportSettings ImportSettingsCurrent
			{
				get
				{
					var currentSettings = _importCommonSettings.SelectSingle(_viewName);
					if (currentSettings != null) return (PXImportSettings)currentSettings;
					SetDefaultCommonSettings();
					return (PXImportSettings)_importCommonSettings.SelectSingle(_viewName);
				}
			}
			#endregion

			#region Private Methods

			private IEnumerable<KeyValuePair<IDictionary, IDictionary>> ReadItems(IContentReader reader, System.Globalization.CultureInfo culture)
			{
				reader.Reset();
				var statesCache = new Dictionary<string, StateInfo>();
				int index = 0;
				while (reader.MoveNext())
				{
					KeyValuePair<IDictionary, IDictionary> item;
					if (GetKeysAndValues(reader, index, culture, statesCache, out item))
					{
						yield return item;
						index++;
					}
				}
			}

			private bool GetKeysAndValues(IContentReader contentReader, int lineNumber, System.Globalization.CultureInfo culture,
				IDictionary<string, StateInfo> statesCache, out KeyValuePair<IDictionary, IDictionary> result)
			{
				result = new KeyValuePair<IDictionary, IDictionary>(
					new OrderedDictionary(StringComparer.OrdinalIgnoreCase), 
					new OrderedDictionary(StringComparer.OrdinalIgnoreCase));

				var importSettingsCurrent = ImportSettingsCurrent;
				var nullValueStr = importSettingsCurrent == null ? null : importSettingsCurrent.NullValue;
				var lineFields = new List<string>(_lineProperties);
				var shortLineFields = new List<string>(_shortLineProperties);
				var hasAnyData = false;
				foreach (PXImportColumnsSettings property in _importColumnsSettings.SelectMulti(_viewName))
				{
					if (string.IsNullOrEmpty(property.PropertyName)) continue;
					string contentValue = contentReader.GetValue((int)property.ColumnIndex);
					if (string.Compare(contentValue, nullValueStr, false) == 0) contentValue = null;
					if (contentValue != null)
					{
						hasAnyData |= contentValue.Trim() != string.Empty;
						StateInfo stateInfo;
						if (!statesCache.TryGetValue(property.PropertyName, out stateInfo))
						{
							var state = _cachedItems.Cache.GetStateExt(null, property.PropertyName) as PXFieldState;
							if (state != null) stateInfo = new StateInfo(state);
							statesCache.Add(property.PropertyName, stateInfo);
						}
						if (stateInfo != null)
						{
							if (string.IsNullOrEmpty(contentValue) && !string.IsNullOrEmpty(stateInfo.ViewName))
							{
								contentValue = null;
							}
							else
							{
								if (!string.IsNullOrEmpty(stateInfo.InputMask))
								{
									contentValue = Mask.Parse(stateInfo.InputMask, contentValue);
								}
								object dropDownValue;
								if (stateInfo.LabelValuePairs != null && stateInfo.LabelValuePairs.TryGetValue(contentValue, out dropDownValue))
								{
									contentValue = dropDownValue != null ? dropDownValue.ToString() : null;
								}
								else if (culture != null && stateInfo.DataType != null)
									contentValue = ParseValue(contentValue, stateInfo.DataType, culture);
							}
						}
						if (string.IsNullOrEmpty(contentValue)) contentValue = null;
						if (_cache.Keys.Contains(property.PropertyName))
						{
							if (result.Key.Contains(property.PropertyName))
								result.Key[property.PropertyName] = contentValue;
							else result.Key.Add(property.PropertyName, contentValue);
						}
						if (result.Value.Contains(property.PropertyName))
							result.Value[property.PropertyName] = contentValue;
						else result.Value.Add(property.PropertyName, contentValue);
					}
					lineFields.Remove(property.PropertyName);
					shortLineFields.Remove(property.PropertyName);
				}
				if (!hasAnyData) return false;
				foreach (var field in lineFields)
					if (_cache.Keys.Contains(field)) result.Key.Add(field, lineNumber);
					else result.Value.Add(field, lineNumber);
				var shortLineNumber = (short)lineNumber;
				foreach (var field in shortLineFields)
					if (_cache.Keys.Contains(field)) result.Key.Add(field, shortLineNumber);
					else result.Value.Add(field, shortLineNumber);

				if (_cache.Keys.Count > result.Key.Count)
				{
					var defaultRow = _cache.CreateInstance();
					foreach (string key in _cache.Keys)
						if (!result.Key.Contains(key))
						{
							object defaultValue;
							if (!_cache.RaiseFieldDefaulting(key, defaultRow, out defaultValue))
							{
								_cache.RaiseFieldSelecting(key, defaultRow, ref defaultValue, false);
								defaultValue = PXFieldState.UnwrapValue(defaultValue);
							}

							result.Key.Add(key, defaultValue);
						}
				}

				result.Value.Add(PXImportAttribute.ImportFlag, PXCache.NotSetValue);
				return true;
			}

			private static string ParseValue(string contentValue, Type targetType, IFormatProvider formater)
			{
				object parsedValue = contentValue;
				try
				{
					switch (Type.GetTypeCode(targetType))
					{
						case TypeCode.Boolean:
							parsedValue = Convert.ToBoolean(contentValue, formater);
							break;
						case TypeCode.Byte:
							parsedValue = Convert.ToByte(contentValue, formater);
							break;
						case TypeCode.DateTime:
							parsedValue = Convert.ToDateTime(contentValue, formater);
							break;
						case TypeCode.Decimal:
							parsedValue = Convert.ToDecimal(contentValue, formater);
							break;
						case TypeCode.Double:
							parsedValue = Convert.ToDouble(contentValue, formater);
							break;
						case TypeCode.Int16:
							parsedValue = Convert.ToInt16(contentValue, formater);
							break;
						case TypeCode.Int32:
							parsedValue = Convert.ToInt32(contentValue, formater);
							break;
						case TypeCode.Int64:
							parsedValue = Convert.ToInt64(contentValue, formater);
							break;
						case TypeCode.SByte:
							parsedValue = Convert.ToSByte(contentValue, formater);
							break;
						case TypeCode.Single:
							parsedValue = Convert.ToSingle(contentValue, formater);
							break;
						case TypeCode.UInt16:
							parsedValue = Convert.ToUInt16(contentValue, formater);
							break;
						case TypeCode.UInt32:
							parsedValue = Convert.ToUInt32(contentValue, formater);
							break;
						case TypeCode.UInt64:
							parsedValue = Convert.ToUInt64(contentValue, formater);
							break;
					}
				}
				catch (FormatException) { }
				catch (OverflowException) { }
				contentValue = parsedValue.ToString();
				return contentValue;
			}

			private void GetTBatchTableProperties(PXCache sender, PXFieldSelectingEventArgs args)
			{
				var row = args.Row as PXImportColumnsSettings;
				if (row == null || string.Compare(row.ViewName, _viewName, true) != 0) return;

				InitPropertiesInfo();
				args.ReturnState = PXStringState.CreateInstance(args.ReturnState, null, null, "PropertyName", null,
								-1, null, _propertiesNames, _propertiesDisplayNames, true, null);
			}

			private void CorrectColumnAssociations(PXCache sender, PXFieldUpdatingEventArgs e)
			{
				var row = e.Row as PXImportColumnsSettings;
				if (row == null || string.Compare(row.ViewName, _viewName, true) != 0) return;

				//TODO: set only unique property-column pairs
				var newValue = e.NewValue == null ? string.Empty : e.NewValue.ToString();
				foreach (PXImportColumnsSettings item in sender.Cached)
					if (!item.Index.Equals(row.Index) &&
						string.Compare(item.PropertyName, newValue, true) == 0)
					{
						e.Cancel = true;
						break;
					}
			}

			private void SetDefaultColumnsAssociations(byte[] content)
			{
				var reader = GetReader(content);
				reader.Reset();
				if (!reader.MoveNext()) throw new Exception(PX.TM.Messages.DataAreAbsent);

				ForceInitPropertiesInfo();

				foreach (PXImportColumnsSettings cachedItem in _importColumnsSettings.SelectMulti(_viewName))
					_importColumnsSettings.Cache.Delete(cachedItem);

				foreach (var key in reader.IndexKeyPairs)
				{
					var item = new PXImportColumnsSettings
								{
									ViewName = _viewName,
									ColumnIndex = key.Key,
									ColumnName = key.Value
								};
					int nameIndex = Array.IndexOf<string>(_propertiesDisplayNames, key.Value);
					if (nameIndex < 0) nameIndex = Array.IndexOf<string>(_propertiesNames, key.Value);
					if (nameIndex > -1) item.PropertyName = _propertiesNames[nameIndex];
					_importColumnsSettings.Cache.Insert(item);
				}
				_importColumnsSettings.Cache.IsDirty = false;
			}

			//private static void ForceCacheClear(PXCache cache)
			//{
			//    var deleteItems = new List<object>();
			//    foreach (var item in cache.Inserted)
			//        deleteItems.Add(item);
			//    foreach (var item in cache.Updated)
			//        deleteItems.Add(item);
			//    foreach (var item in deleteItems)
			//        cache.Delete(item);
			//}

			private void SetDefaultCommonSettings()
			{
				foreach (var cachedItem in _importCommonSettings.SelectMulti(_viewName))
					_importCommonSettings.Cache.Delete(cachedItem);

				var importSettings = CreateDefaultCommonSettings();
				importSettings.ViewName = _viewName;
				importSettings.Culture = Thread.CurrentThread.CurrentCulture.LCID;
				_importCommonSettings.Cache.Insert(importSettings);
				_importCommonSettings.Cache.IsDirty = false;
			}

			private void SetContent(byte[] content, string ext)
			{
				var importSettings = ImportSettingsCurrent;
				importSettings.FileExtension = ext;
				importSettings.Data = content;
			}

			protected abstract PXImportSettings CreateDefaultCommonSettings();

			public void Process(CommonSettings commonSettings, string[] columnsMap)
			{
				if (columnsMap == null) throw new ArgumentNullException("columnsMap");

				System.Globalization.CultureInfo culture = null;
				if (commonSettings.Culture != null) 
					culture = CultureInfo.GetCultureInfo(commonSettings.Culture);

				var idx = 0;
				foreach (string property in columnsMap)
				{
					var column = (PXImportColumnsSettings)_importColumnsSettings.Cache.Insert();
					column.ViewName = _viewName;
					column.PropertyName = property;
					column.ColumnIndex = idx;
					idx++;
				}

				var oldCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
				var oldGraphCulture = _cache.Graph.Culture;
				System.Threading.Thread.CurrentThread.CurrentCulture = _cache.Graph.Culture = culture;
				PX.Common.PXContext.SetSlot<PX.Translation.PXDictionaryManager>(null); //NOTE: because of localization initialization bug
				ConvertData(commonSettings.Content, culture, commonSettings.OnlyInsert);
				System.Threading.Thread.CurrentThread.CurrentCulture = oldCulture;
				_cache.Graph.Culture = oldGraphCulture;
			}

			private void ConvertData(byte[] content, System.Globalization.CultureInfo culture, bool onlyInsert)
			{
				_exceptions.Clear();

				if (RollbackPreviousOperation)
				{
					foreach (TBatchTable item in _backupCache.Cached)
						_cache.Update(item);
					_backupCache.Clear();
					foreach (TBatchTable item in _importedCache.Cached)
						_cache.Delete(item);
					_importedCache.Clear();
				}

				ForceInitPropertiesInfo();
				using (var reader = GetReader(content))
					foreach (var item in ReadItems(reader, culture))
					{
						var keys = item.Key;
						var values = item.Value;
						if (_isCRSelectable && !values.Contains("Selected")) values.Add("Selected", true);
						if (OnCreateImportRow != null)
						{
							var dontUpdateExistRecords = ImportSettingsCurrent.With(_ => _.OnlyInsert);
							var args = new CreateImportRowEventArguments(keys, values, dontUpdateExistRecords);
							OnCreateImportRow(args);
							if (args.Cancel) continue;
						}
						var oldCurrent = _cache.Current;
						var originalRow = _cache.Locate(keys) > 0 ? _cache.Current : null;
						var wasInserted = originalRow == null;
						if (onlyInsert)
						{
							if (OnRowImporting != null)
							{
								var args = new RowImportingEventArguments(originalRow);
								OnRowImporting(args);
								if (args.Cancel) continue;
							}
							if (originalRow != null) originalRow = _cache.CreateCopy(originalRow);
						}
						if (!SaftyPerformOperation(() => _cache.Update(keys, values)))
						{
							_cache.Current = oldCurrent;
							continue;
						}
						var impRow = _cache.Current;
						if (onlyInsert)
						{
							var impArgs = new RowImportedEventArguments(impRow, originalRow);
							if (OnRowImported != null) OnRowImported(impArgs);
							else impArgs.Cancel = !wasInserted;
							if (impArgs.Cancel)
							{
								_cache.Remove(_cache.Current);
								_cache.Current = oldCurrent;
							}
						}
						if (RollbackPreviousOperation)
						{
							if (wasInserted) InsertHeldItem(_importedCache, keys);
							else InsertHeldItem(_backupCache, keys);
						}
					}
			}

			private static void InsertHeldItem(PXCache cache, IDictionary keys)
			{
				object keysObj = cache.CreateInstance();
				foreach (DictionaryEntry key in keys)
					cache.SetValueExt(keysObj, key.Key.ToString(), key.Value);

				object locatedObj = cache.Locate(keysObj);
				cache.SetStatus(locatedObj, PXEntryStatus.Held);

				cache.Insert(locatedObj);
			}

			private bool AskCommonSettings()
			{
				var commonSettings = ImportSettingsCurrent;
				if (commonSettings == null) return true;

				var result = _importCommonSettings.AskExt(
								new PXView.InitializePanel((graph, name) =>
								                           	{
								                           		var content = commonSettings.Data as byte[];
								                           		var ext = commonSettings.FileExtension;
								                           		SetDefaultCommonSettings();
																SetContent(content, ext);
								                           	}));
				return result == WebDialogResult.OK;
			}

			private void AskColumnAssociations()
			{
				var columnsSettingsAsk = _importColumnsSettings.AskExt(
					(graph, name) =>
						{
							var commonSettings = ImportSettingsCurrent;
							SetDefaultColumnsAssociations(commonSettings.Data as byte[]);
						});
				if (columnsSettingsAsk == WebDialogResult.OK)
				{
					var commonSettings = ImportSettingsCurrent;
					var content = commonSettings.Data as byte[];
					if (content != null && content.Length > 0)
					{
						System.Globalization.CultureInfo culture = null;
						if (commonSettings.Culture != null) culture = CultureInfo.GetCultureInfo((int)commonSettings.Culture);
				//		ConvertData(content, formater, commonSettings.OnlyInsert == true);

						PXLongOperation.StartOperation(_cache.Graph,
						                               delegate
						                               	{
						                               		Thread.Sleep(2000);
															var oldCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
															var oldGraphCulture = _cache.Graph.Culture;
															System.Threading.Thread.CurrentThread.CurrentCulture = _cache.Graph.Culture = culture;
															PX.Common.PXContext.SetSlot<PX.Translation.PXDictionaryManager>(null); //NOTE: because of localization initialization bug
															ConvertData(content, culture, commonSettings.OnlyInsert);
															System.Threading.Thread.CurrentThread.CurrentCulture = oldCulture;
															_cache.Graph.Culture = oldGraphCulture;
						                               		Func<PXGraph> getImportGraph = () => _cache.Graph;
						                               		PXLongOperation.SetCustomInfo(getImportGraph, new object[0]);
						                               		ShowExceptions();
						                               	});
					}
					
				}
			
			}

			private void ShowExceptions()
			{
				StringBuilder sb = new StringBuilder();
				int limit = 10; //TODO: need scroll in dialog box.
				foreach (KeyValuePair<Type, IList<Exception>> pair in Exceptions)
					foreach (Exception exception in pair.Value)
					{
						sb.AppendLine(exception.Message);
						if (--limit < 1) break;
					}
				string message = sb.ToString();
				if (!string.IsNullOrEmpty(message))
					throw new PXDialogRequiredException(_viewName, null, _ERRORS_MESSAGE, message,
						MessageButtons.RetryCancel, MessageIcon.Error);
			}

			private static void AssertType<ItemType>(PXCache itemsCache)
			{
				Type itemsCacheType = itemsCache.GetItemType();
				AssertType<ItemType>(itemsCacheType);
			}

			private static void AssertType<ItemType>(Type cacheItemType)
			{
				if (!typeof(ItemType).IsAssignableFrom(cacheItemType))
					throw new ArgumentException(string.Format("The items type '{0}' of cache must be an inheritor of '{1}' type.",
															  cacheItemType, typeof(ItemType)));
			}

			#endregion
		}

		#endregion

		#region PXImportException

		public sealed class PXImportException : Exception
		{
			public readonly KeyValuePair<IDictionary, IDictionary> Row;

			public PXImportException(string message, KeyValuePair<IDictionary, IDictionary> row, Exception innerException)
				: base(message, innerException)
			{
				Row = row;
			}

			public PXImportException(SerializationInfo info, StreamingContext context)
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

		#region IPXPrepareItems
		public interface IPXPrepareItems
		{
			bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values);
			bool RowImporting(string viewName, object row);
			bool RowImported(string viewName, object row, object oldRow);
			void PrepareItems(string viewName, IEnumerable items);
		}
		#endregion

		#region Constants
		private const string _IMPORT_MESSAGE = "Import";

		public const string _RUNWIZARD_ACTION_NAME = "$ImportWizardAction";
		public const string _IMPORT_ACTION_NAME = "$ImportAction";

		public const string ImportCSVSettingsName = "$ImportCSVSettings";
		public const string ImportXLSXSettingsName = "$ImportXLSXSettings";
		public const string ImportColumnsSettingsName = "$ImportColumns";
		public const string ImportContentBagName = "$ImportContentBag";

		public const string _DONT_UPDATE_EXIST_RECORDS = "_DONT_UPDATE_EXIST_RECORDS";
		#endregion

		#region Fields
		private readonly Type _table;

		private IPXImportWizard _importer;
		private PXCache _itemsCache;
		private IPXPrepareItems _prepareItemsHandler;
		private string _itemsViewName;

		public static readonly string ImportFlag = Guid.NewGuid().ToString();

		#endregion

		#region Ctors
		public PXImportAttribute(Type primaryTable)
		{
			_table = primaryTable;
		}

		public PXImportAttribute(Type primaryTable, IPXImportWizard importer)
			: this(primaryTable)
		{
			_importer = importer;
		}

		public override void ViewCreated(PXGraph graph, string viewName)
		{
			_itemsCache = graph.Views[viewName].Cache;
			_itemsViewName = viewName;

			AddAction(_IMPORT_ACTION_NAME, viewName, _table, graph, new PXButtonDelegate(Import));
			AddAction(_RUNWIZARD_ACTION_NAME, viewName, _table, graph, new PXButtonDelegate(ImportWizard));

			var importContentBag = AddView(graph, _itemsViewName + ImportContentBagName, typeof(PXContentBag));
			if (_importer == null)
			{
				var current = importContentBag.Cache.Current as PXContentBag;
				var createParams = new object[]
				                 	{
				                 		current == null ? null : current.FileExtension,
				                 		_itemsCache,
				                 		_itemsViewName,
				                 		RollbackPreviousImport
				                 	};
				var importerType = MakeGenericType(typeof(PXImporter<>), _itemsCache.GetItemType());
				_importer = importerType.GetMethod("Create").Invoke(null, createParams) as IPXImportWizard;
			}
			if (_importer != null)
			{
				TryUploadData(_importer, importContentBag);
				if (_itemsCache.Graph is IPXPrepareItems)
				{
					_prepareItemsHandler = ((IPXPrepareItems)_itemsCache.Graph);
					_importer.OnCreateImportRow += 
						args =>
							{
								using (PXExecutionContext.Scope.Instantiate(new PXExecutionContext()))
								{
									PXExecutionContext.Current.Bag[_DONT_UPDATE_EXIST_RECORDS] = args.DontUpdateExistRecords;
									args.Cancel = !_importer_OnImportRow(args.Keys, args.Values);
								}
							};
					_importer.OnRowImporting +=
						args =>
							{
								args.Cancel = !_importer_OnRowImporting(args.Row);
							};
					_importer.OnRowImported +=
						args =>
							{
								args.Cancel = !_importer_OnRowImported(args.Row, args.OldRow);
							};
				}
				graph.RowUpdated.AddHandler<PXContentBag>(
					(sender, args) =>
					{
						if (!TryUploadData(_importer, importContentBag))
							throw new PXDialogRequiredException(_itemsViewName, null, "Validation failed",
																"Uploading file contains incorrect data.",
																MessageButtons.OK, MessageIcon.Error);
					});
			}
		}
		#endregion

		#region Public Methods

		public static void SetEnabled(PXGraph graph, string viewName, bool isEnabled)
		{
			graph.Actions[viewName + _RUNWIZARD_ACTION_NAME].SetEnabled(isEnabled);
			graph.Actions[viewName + _IMPORT_ACTION_NAME].SetEnabled(isEnabled);
		}

		public bool RollbackPreviousImport { get; set; }

		#endregion

		#region Protected Methods
		protected static PXAction AddAction(string name, string viewName, Type table, PXGraph graph, PXButtonDelegate handler)
		{
			var actionName = viewName + name;
			var action = (PXAction)Activator.CreateInstance(
				MakeGenericType(typeof(PXNamedAction<>), table), graph, actionName, handler);
			action.SetVisible(false);
			graph.Actions[actionName] = action;
			return action;
		}

		protected static PXView AddView(PXGraph graph, string viewName, Type itemType)
		{
			return AddView(graph, viewName, itemType, null, null);
		}

		protected static PXView AddView(PXGraph graph, string viewName, Type itemType, Type whereType, Type orderType)
		{
			if (graph.Views.ContainsKey(viewName)) return graph.Views[viewName];

			var itemCacheType = MakeGenericType(typeof(PXNotCleanableCache<>), itemType);
			if (!graph.Caches.ContainsKey(itemType) ||
				!itemCacheType.IsAssignableFrom(graph.Caches[itemType].GetType()))
			{
				var newCache = (PXCache)Activator.CreateInstance(itemCacheType, graph);
				newCache.Load();
				graph.Caches[itemType] = newCache;
				graph.SynchronizeByItemType(newCache);
			}

			var handler = new PXSelectInsertedHandler();
			var command = BqlCommand.CreateInstance(typeof(Select<>), itemType);
			if (whereType != null) command = command.WhereNew(whereType);
			if (orderType != null) command = command.OrderByNew(orderType);

			handler.View = new PXView(graph, false, command, new PXSelectDelegate(handler.Select));
			graph.Views.Add(viewName, handler.View);
			graph.RowUpdated.AddHandler(itemType, new PXRowUpdated(ClearDirtyOnRowUpdated));
			graph.RowInserted.AddHandler(itemType, new PXRowInserted(ClearDirtyOnRowInserted));
			return handler.View;
		}

		private class PXSelectInsertedHandler
		{
			public PXView View;

			public IEnumerable Select()
			{
				View.Cache.IsDirty = false;
				return View.Cache.Inserted;
			}

		}

		private sealed class viewErrorInterceptor : PXView
		{
			private PXView _View;

			private viewErrorInterceptor(PXGraph graph, bool isReadOnly, BqlCommand select)
				: base(graph, isReadOnly, select)
			{
			}

			private viewErrorInterceptor(PXGraph graph, bool isReadOnly, BqlCommand select, Delegate handler)
				: base(graph, isReadOnly, select, handler)
			{
			}

			public static viewErrorInterceptor FromView(PXView view)
			{
				viewErrorInterceptor instance = view.BqlDelegate == null ?
					new viewErrorInterceptor(view.Graph, view.IsReadOnly, view.BqlSelect) :
					new viewErrorInterceptor(view.Graph, view.IsReadOnly, view.BqlSelect, view.BqlDelegate);
				instance._View = view;
				return instance;
			}

			public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
			{
				if (maximumRows == 0)
				{
					return _View.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
				}
				int start = 0;
				int maximum = 0;
				List<object> sel = _View.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref start, maximum, ref totalRows);
				List<object> ret = new List<object>();
				for (int i = 0; i < sel.Count; )
				{
					object item = sel[i];
					if (item is PXResult)
					{
						item = ((PXResult)item)[0];
					}
					PXEntryStatus status = _View.Cache.GetStatus(item);
					if (status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated)
					{
						Dictionary<string, string> errors = PXUIFieldAttribute.GetErrors(_View.Cache, item);
						if (errors.Count > 0)
						{
							ret.Add(sel[i]);
							sel.RemoveAt(i);
						}
						else
						{
							i++;
						}
					}
					else
					{
						i++;
					}
				}
				if (startRow < 0)
				{
					startRow = 0;
				}
				for (int i = startRow; i < sel.Count && ret.Count <= maximumRows; i++)
				{
					ret.Add(sel[i]);
				}
				return ret;
			}
		}
		#endregion

		#region Private Methods
		private static bool TryUploadData(IPXImportWizard importer, PXView contentBag)
		{
			var current = contentBag.Cache.Current as PXContentBag;
			return current != null && current.Loaded.HasValue && current.Loaded.Value &&
				   importer.TryUploadData((byte[])current.Data, current.FileExtension);
		}

		private static void ClearDirtyOnRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.IsDirty = false;
		}

		private static void ClearDirtyOnRowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			sender.IsDirty = false;
		}

		private bool _importer_OnImportRow(IDictionary keys, IDictionary values)
		{
			return _prepareItemsHandler.PrepareImportRow(_itemsViewName, keys, values);
		}

		private bool _importer_OnRowImporting(object row)
		{
			return _prepareItemsHandler.RowImporting(_itemsViewName, row);
		}

		private bool _importer_OnRowImported(object row, object oldRow)
		{
			return _prepareItemsHandler.RowImported(_itemsViewName, row, oldRow);
		}

		[PXUIField(Visible = false)]
		[PXButton(CommitChanges = true)]
		private IEnumerable ImportWizard(PXAdapter adapter)
		{
			if (_importer != null)
			{
				_importer.PreRunWizard();
				_importer.RunWizard();
				_itemsCache.Graph.Views[_itemsViewName].RequestRefresh();
				adapter.View.Graph.Views[_itemsViewName] = viewErrorInterceptor.FromView(adapter.View.Graph.Views[_itemsViewName]);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = _IMPORT_MESSAGE, Enabled = true)]
		[PXButton(ImageKey = Sprite.Main.Process, CommitChanges = true)]
		private IEnumerable Import(PXAdapter adapter)
		{
			if (_itemsCache.Graph is IPXPrepareItems)
				((IPXPrepareItems)_itemsCache.Graph).PrepareItems(_itemsViewName, GetImportedItems());
			SafetyPersistInserted(_itemsCache, GetImportedItems());
			return adapter.Get();
		}

		private IEnumerable GetImportedItems()
		{
			foreach(object item in _itemsCache.Inserted)
			{
				var selectable = item as IPXSelectable;
				if ((selectable == null || selectable.Selected == true)/* &&
					_itemsCache.GetValuePending(item, PXImportAttribute.ImportFlag) != null*/)
				{
					yield return item;
				}
			}
		}

		private void SafetyPersistInserted(PXCache cache, IEnumerable items)
		{
			bool isAborted = false;
			PXTransactionScope tscope = null;
			var persistedItems = new List<object>();
			try
			{
				tscope = new PXTransactionScope();
				foreach (var item in items)
				{
					cache.PersistInserted(item);
					persistedItems.Add(item);
				}
				tscope.Complete(cache.Graph);
			}
			catch (Exception)
			{
				isAborted = true;
				throw;
			}
			finally
			{
				if (tscope != null) tscope.Dispose();
				cache.Normalize();
				cache.Persisted(isAborted);
				cache.Graph.Views[_itemsViewName].Clear();
			}
		}

		private static Type MakeGenericType(params Type[] types)
		{
			int index = 0;
			return MakeGenericType(types, ref index);
		}

		private static Type MakeGenericType(Type[] types, ref int index)
		{
			if (types == null) throw new ArgumentNullException("types");
			if (types.Length == 0) throw new ArgumentException("types list is empty");
			if (index >= types.Length) throw new ArgumentOutOfRangeException("types", "types list is not correct");

			Type ret = types[index];
			index++;
			if (!ret.IsGenericTypeDefinition) return ret;
			Type[] args = new Type[ret.GetGenericArguments().Length];
			for (int i = 0; i < args.Length; i++)
				args[i] = MakeGenericType(types, ref index);
			return ret.MakeGenericType(args);
		}
		#endregion
	}
	#endregion

	#region PXLineNbrAttribute
	/// <summary>
	/// Automatically generates unique line numbers for child rows in parent/child relationship.
	/// This attribute does not works without PXParent attribute.
	/// </summary>
	public class PXLineNbrAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber, IPXRowDeletedSubscriber, IPXRowInsertedSubscriber
	{
		private Type _sourceType;
		private Type _DataType;
		private string _sourceField;		
		private short incrementStep = 1;

		public PXLineNbrAttribute(Type sourceType)
		{
			if (typeof(IBqlField).IsAssignableFrom(sourceType) && sourceType.IsNested)
			{
				_sourceType = BqlCommand.GetItemType(sourceType);
				_sourceField = sourceType.Name;
			}
			else if (typeof(IBqlTable).IsAssignableFrom(sourceType))
			{
				_sourceType = sourceType;
			}
			else
			{
				throw new PXArgumentException("type", ErrorMessages.CantCreateForeignKeyReference, sourceType);
			}
		}

		public short IncrementStep
		{
			get { return this.incrementStep; }
			set { this.incrementStep = value; }
		}

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (_sourceField != null)
			{
				PXCache cache = sender.Graph.Caches[_sourceType];
				object currentVal = DefaultValue;

				if (cache.Current != null)
				{
					currentVal = cache.GetValue(cache.Current, _sourceField);
					object LineNbr = sender.GetValue(e.Row, _FieldOrdinal);
					if (LineNbr != null && (((IComparable)currentVal).CompareTo(LineNbr) < 0))
					{
						cache.SetValue(cache.Current, _sourceField, LineNbr);
						if (cache.GetStatus(cache.Current) == PXEntryStatus.Notchanged)
						{
							cache.SetStatus(cache.Current, PXEntryStatus.Updated);
						}
					}
				}
			}
		}

		public virtual void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (_sourceField != null)
			{
				PXCache cache = sender.Graph.Caches[_sourceType];
				object currentVal = DefaultValue;

				if (cache.Current != null)
				{
					currentVal = cache.GetValue(cache.Current, _sourceField);
					object LineNbr = sender.GetValue(e.Row, _FieldOrdinal);
					if (LineNbr != null && (((IComparable)currentVal).CompareTo(LineNbr) == 0))
					{
						cache.SetValue(cache.Current, _sourceField, Decrement(currentVal,1));
						if (cache.GetStatus(cache.Current) == PXEntryStatus.Notchanged)
						{
							cache.SetStatus(cache.Current, PXEntryStatus.Updated);
						}
					}
				}
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			PXFieldState state = (PXFieldState)sender.GetStateExt(null, _FieldName);
			_DataType = state.DataType;
		}

		public object NewLineNbr(PXCache sender, object Current)
		{
			PXCache cache = sender.Graph.Caches[_sourceType];
			object currentVal = DefaultValue;
																	
			if (Current != null)
			{
				currentVal = Increment(cache.GetValue(Current, _sourceField), this.incrementStep);
				cache.SetValue(Current, _sourceField, currentVal);
				if (cache.GetStatus(Current) == PXEntryStatus.Notchanged || cache.GetStatus(Current) == PXEntryStatus.Held)
				{
					cache.SetStatus(Current, PXEntryStatus.Updated);
				}
			}
			return currentVal;
		}

		public static object NewLineNbr<TField>(PXCache cache, object Current)
			where TField : class, IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<TField>())
			{
				if (attr is PXLineNbrAttribute)
				{
					return ((PXLineNbrAttribute)attr).NewLineNbr(cache, Current);
				}
			}
			return null;
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (_sourceField != null)
			{
				if (e.Row != null)
				{
					object copy = sender.GetCopy(e.Row);
					object value;
					if ((value = sender.GetValue(copy, _FieldOrdinal)) != null)
					{
						e.NewValue = value;
						return;
					}
				}

				e.NewValue = NewLineNbr(sender, sender.Graph.Caches[_sourceType].Current);
			}
			else
			{
				object newLineNbr = DefaultValue;				

				bool found = false;
				foreach (object data in sender.Inserted)
				{
					object lastLineNbr = sender.GetValue(data, _FieldOrdinal);

					if (((IComparable)lastLineNbr).CompareTo(newLineNbr) > 0)
					{
						newLineNbr = lastLineNbr;
						found = true;
					}
				}

				if (!found)
				{
					foreach (object data in PXParentAttribute.SelectSiblings(sender, e.Row, _sourceType))
					{
						object lastLineNbr = sender.GetValue(data, _FieldOrdinal);

						if (((IComparable)lastLineNbr).CompareTo(newLineNbr) > 0)
						{
							newLineNbr = lastLineNbr;
						}
					}
				}

				e.NewValue = Increment(newLineNbr, 1);
			}
		}

		private object DefaultValue
		{
			get
			{
				if (_DataType == typeof(Int32))
				{
					return (Int32)0;
				}
				else if (_DataType == typeof(Int16))
				{
					return (Int16)0;
				}
				return (long)0;
			}
		}
		private object Increment(object value, short step)
		{
			if (_DataType == typeof(Int32))
			{
				return (Int32)((Int32)value + step);
			}
			else if (_DataType == typeof(Int16))
			{
				return (Int16)((Int16)value + step);
			}
			return (long)((long)value + step);
		}

		private object Decrement(object value, short step)
		{
			if (_DataType == typeof(Int32))
			{
				return (Int32)((Int32)value - step);
			}
			else if (_DataType == typeof(Int16))
			{
				return (Int16)((Int16)value - step);
			}
			return (long)((long)value - step);
		}
	}

	#endregion

	#region PXTimeZoneAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class PXTimeZoneAttribute : PXStringListAttribute
	{
		private static readonly string[] _values;
		private static readonly string[] _labels;
		public override bool IsLocalizable { get { return false; } }
		static PXTimeZoneAttribute()
		{
			var values = new List<string>();
			var labels = new List<string>();
			values.Add(string.Empty);
			labels.Add(string.Empty);
			var zoneCollection = new List<PXTimeZoneInfo>(PXTimeZoneInfo.GetSystemTimeZones());
			zoneCollection.Sort((x, y) =>
			{
				var diff = x.BaseUtcOffset.CompareTo(y.BaseUtcOffset);
				if (diff == 0) diff = x.DisplayName.CompareTo(y.DisplayName);
				return diff;
			});
			foreach (var zone in zoneCollection)
			{
				values.Add(zone.Id);
				labels.Add(zone.DisplayName);
			}
			_values = values.ToArray();
			_labels = labels.ToArray();
		}

		public PXTimeZoneAttribute() : base(_values, _labels) { }
	}

	#endregion

	#region DB List Attributes

	#region PXDBStringListAttribute

	public sealed class PXDBStringListAttribute : PXBaseListAttribute
	{
		private class PXDBStringAttributeHelper : PXDBListAttributeHelper<string>
		{
			public PXDBStringAttributeHelper(Type table, Type valueField, Type descriptionField)
				: base(table, valueField, descriptionField)
			{
			}

			protected override object CreateState(PXCache sender, PXFieldSelectingEventArgs e, string[] values, string[] labels,
												  string fieldName, string defaultValue)
			{
				if (values.Length != labels.Length)
				{
					PXTrace.WriteInformation(string.Format("CRStringAttributeHelper CreateState {0}_{1}: Invalide values and labels",
						sender.GetItemType().Name, fieldName));
					int count = Math.Max(values.Length, labels.Length);
					for (int i = 0; i < count; i++)
					{
						PXTrace.WriteInformation(string.Format("'{0}' -> '{1}'",
															   i < values.Length ? values[i] : "<error>",
															   i < labels.Length ? labels[i] : "<error>"));
					}
				}
				return PXStringState.CreateInstance(e.ReturnState, null, null, fieldName, null,
													-1, null, values, labels, true, defaultValue);
			}

			protected override string EmptyLabelValue
			{
				get
				{
					return string.Empty;
				}
			}
		}

		public PXDBStringListAttribute(Type table, Type valueField, Type descriptionField)
			: base(new PXDBStringAttributeHelper(table, valueField, descriptionField))
		{
		}
	}

	#endregion

	#region PXDBIntListAttribute

	public sealed class PXDBIntListAttribute : PXBaseListAttribute
	{
		private class PXDBIntAttributeHelper : PXDBListAttributeHelper<int>
		{
			public PXDBIntAttributeHelper(Type table, Type valueField, Type descriptionField)
				: base(table, valueField, descriptionField)
			{
			}

			protected override object CreateState(PXCache sender, PXFieldSelectingEventArgs e, int[] values, string[] labels,
												  string fieldName, int defaultValue)
			{
				return PXIntState.CreateInstance(e.ReturnState, fieldName, null,
												 -1, null, null, values, labels, null, defaultValue);
			}
		}

		public PXDBIntListAttribute(Type table, Type valueField, Type descriptionField)
			: base(new PXDBIntAttributeHelper(table, valueField, descriptionField))
		{
		}
	}

	#endregion

	#region IPXDBListAttributeHelper

	public interface IPXDBListAttributeHelper : ILocalizableValues
	{
		Type DefaultValueField { get; set; }
		String EmptyLabel { get; set; }
		object DefaultValue { get; }
		void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, PXAttributeLevel attributeLevel, string fieldName);
		Dictionary<object, string> ValueLabelDic(PXGraph graph);
	}

	#endregion

	#region PXBaseListAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	[PXAttributeFamily(typeof(PXBaseListAttribute))]
	public abstract class PXBaseListAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber,
												IPXFieldDefaultingSubscriber, ILocalizableValues
	{
		private readonly IPXDBListAttributeHelper _helper;

		protected PXBaseListAttribute(IPXDBListAttributeHelper helper)
		{
			_helper = helper;
		}

		public Type DefaultValueField
		{
			get { return _helper.DefaultValueField; }
			set { _helper.DefaultValueField = value; }
		}

		public string EmptyLabel
		{
			get { return _helper.EmptyLabel; }
			set { _helper.EmptyLabel = value; }
		}

		#region IPXFieldSelectingSubscriber Members

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			_helper.FieldSelecting(sender, e, _AttributeLevel, base._FieldName);
		}

		#endregion

		public Dictionary<object, string> ValueLabelDic(PXGraph graph)
		{
			return _helper.ValueLabelDic(graph);
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (_helper.DefaultValueField != null)
			{
				e.NewValue = _helper.DefaultValue;
			}
		}

		#region ILocalizableValues
		public string Key
		{
			get { return _helper.Key; }
		}

		public string[] Values
		{
			get { return _helper.Values; }
		}
		#endregion
	}

	#endregion

	#region PXDBListAttributeHelper<TValue>

	public abstract class PXDBListAttributeHelper<TValue> : IPXDBListAttributeHelper
	{
		#region ListParameters

		private struct ListParameters
		{
			public readonly Type DescriptionField;
			public readonly Type ValueField;
			public readonly Type DefaultValueField;
			public readonly Type Table;
			public readonly KeyValuePair<string, TValue> EmptyLabel;

			public ListParameters(Type table, Type descriptionField, Type valueField, Type defaultValueField, KeyValuePair<string, TValue> emptyLabel)
			{
				Table = table;
				DescriptionField = descriptionField;
				ValueField = valueField;
				DefaultValueField = defaultValueField;
				EmptyLabel = emptyLabel;
			}

			public ListParameters(Type table, Type descriptionField, Type valueField, KeyValuePair<string, TValue> emptyLabel)
				: this(table, descriptionField, valueField, null, emptyLabel)
			{
			}

			public ListParameters(Type table, Type descriptionField, Type valueField)
				: this(table, descriptionField, valueField, null, new KeyValuePair<string, TValue>(null, default(TValue)))
			{
			}

			public ListParameters ChangeDefaultValueField(Type defaultValueField)
			{
				return new ListParameters(Table, DescriptionField, ValueField, defaultValueField, EmptyLabel);
			}

			public ListParameters ChangeEmptyLabel(string label, TValue value)
			{
				return new ListParameters(Table, DescriptionField, ValueField, DefaultValueField, new KeyValuePair<string, TValue>(label, value));
			}
		}

		#endregion

		#region ValueLabelPairs

		private class ValueLabelPairs : IPrefetchable<ListParameters>
		{
			private TValue[] _values;
			private string[] _labels;
			private string _descriptionFieldName;

			private TValue _defaultValue;

			public void Prefetch(ListParameters parameter)
			{
				Type table = parameter.Table;
				Type valueField = parameter.ValueField;
				Type descriptionField = parameter.DescriptionField;
				Type defaultValueField = parameter.DefaultValueField;

				_defaultValue = default(TValue);
				List<TValue> allowedValues = new List<TValue>();
				List<string> allowedLabels = new List<string>();

				if (parameter.EmptyLabel.Key != null)
				{
					allowedValues.Add(parameter.EmptyLabel.Value);
					allowedLabels.Add(parameter.EmptyLabel.Key);
				}

				List<PXDataField> dataFields = new List<PXDataField>(3);
				dataFields.Add(new PXDataField(valueField.Name));
				dataFields.Add(new PXDataField(descriptionField.Name));
				if (defaultValueField != null) dataFields.Add(new PXDataField(defaultValueField.Name));
				foreach (PXDataRecord record in PXDatabase.SelectMulti(table, dataFields.ToArray()))
				{
					TValue currentValue = (TValue)record.GetValue(0);
					allowedValues.Add(currentValue);
					allowedLabels.Add(record.GetString(1));
					if (defaultValueField != null)
					{
						bool? isDefault = record.GetBoolean(2);
						if (isDefault.HasValue && isDefault.Value)
							_defaultValue = currentValue;
					}
				}

				_values = allowedValues.ToArray();
				_labels = allowedLabels.ToArray();
				_descriptionFieldName = descriptionField.Name;
			}

			public TValue DefaultValue
			{
				get { return _defaultValue; }
			}

			public TValue[] Values
			{
				get { return _values; }
			}

			public string[] Labels
			{
				get { return _labels; }
			}

			public string DescriptionFieldName
			{
				get { return _descriptionFieldName; }
			}
		}

		#endregion

		private ListParameters _parameters;
		private readonly string _slotKey;
		private readonly string _locKey;

		protected PXDBListAttributeHelper(Type table, Type valueField, Type descriptionField)
		{
			_parameters = new ListParameters(table, descriptionField, valueField);
			_slotKey = string.Format("_{0}_slotKey", GetType());
			_locKey = table.FullName;
		}

		public Type DefaultValueField
		{
			get { return _parameters.DefaultValueField; }
			set { _parameters = _parameters.ChangeDefaultValueField(value); }
		}

		public string EmptyLabel
		{
			get { return _parameters.EmptyLabel.Key; }
			set { _parameters = _parameters.ChangeEmptyLabel(value, EmptyLabelValue); }
		}

		protected virtual TValue EmptyLabelValue
		{
			get
			{
				return default(TValue);
			}
		}

		public object DefaultValue
		{
			get
			{
				ValueLabelPairs pairs = Data;
				return pairs == null ? default(TValue) : pairs.DefaultValue;
			}
		}

		private ValueLabelPairs Data
		{
			get
			{
				return PXDatabase.GetSlot<ValueLabelPairs, ListParameters>(
					_slotKey, _parameters, _parameters.Table);
			}
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, PXAttributeLevel attributeLevel,
								   string fieldName)
		{
			if (attributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				ValueLabelPairs pairs = Data;
				e.ReturnState = CreateState(sender, e, pairs.Values, Localize(pairs.DescriptionFieldName, pairs.Labels), fieldName, pairs.DefaultValue);
			}
		}

		protected string[] Localize(string descriptionFieldName, string[] labels)
		{
			if (string.IsNullOrEmpty(descriptionFieldName) || labels == null)
				return null;
			string[] trans = new string[labels.Length];
			for (int i = 0; i < labels.Length; i++)
				trans[i] = PXLocalizer.LocalizeCompound(labels[i], descriptionFieldName, Key);
			return trans;
		}

		protected abstract object CreateState(PXCache sender, PXFieldSelectingEventArgs e, TValue[] values, string[] labels,
											  string fieldName, TValue defaultValue);

		public Dictionary<object, string> ValueLabelDic(PXGraph graph)
		{
			ValueLabelPairs pairs = Data;
			Dictionary<object, string> result = new Dictionary<object, string>(pairs.Values.Length);
			for (int index = 0; index < pairs.Values.Length; index++)
				result.Add(pairs.Values[index], pairs.Labels[index]);
			return result;
		}

		#region ILocalizableValues
		public string Key
		{
			get { return _locKey; }
		}
		public string[] Values
		{
			get { return Localize(Data.DescriptionFieldName, Data.Labels); }
		}
		#endregion
	}

	#endregion

	#endregion

	#region PXPreviewAttribute

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class PXPreviewAttribute : PXViewExtensionAttribute
	{
		private const string _ACTION_POSTFIX = "$RefreshPreview";
		private const string _VIEW_POSTFIX = "$Preview";

		private readonly Type _primaryViewType;
		private Type _previewType;

		private PXGraph _graph;
		private string _viewName;
		private Type _cacheType;
		private PXSelectDelegate _dataHandler;

		private BqlCommand _bqlSelect;

		public PXPreviewAttribute(Type primaryViewType) : this(primaryViewType, null) { }

		public PXPreviewAttribute(Type primaryViewType, Type previewType)
		{
			if (primaryViewType == null) throw new ArgumentNullException("primaryViewType");
			if (previewType != null && !typeof(IBqlTable).IsAssignableFrom(previewType))
				throw new ArgumentException(string.Format("'{0}' must impement PX.Data.IBqlTable interface.", previewType), "previewType");

			_primaryViewType = primaryViewType;
			_previewType = previewType;
		}

		public override void ViewCreated(PXGraph graph, string viewName)
		{
			_graph = graph;
			_viewName = viewName;
			_cacheType = Graph.Views[ViewName].GetItemType();
			if (PreviewType == null) _previewType = CacheType;

			AddView();
			AddAction();
		}

		private void AddAction()
		{
			var newAction = PXNamedAction.AddAction(Graph, _primaryViewType, ViewName + _ACTION_POSTFIX, null, RefreshPreview);
			newAction.SetVisible(false);			
		}

		private void AddView()
		{
			var select = BqlSelect;
			var newView = new PXView(Graph, false, select, SelectHandler);
			Graph.Views.Add(ViewName + _VIEW_POSTFIX, newView);
		}

		protected virtual BqlCommand BqlSelect
		{
			get
			{
				if (_bqlSelect == null)
					_bqlSelect = (BqlCommand)Activator.CreateInstance(BqlCommand.Compose(typeof(Select<>), PreviewType));
				return _bqlSelect;
			}
		}

		protected virtual PXSelectDelegate SelectHandler
		{
			get
			{
				return () => new[] { Graph.Caches[PreviewType].Current };
			}
		}

		protected Type PreviewType
		{
			get { return _previewType; }
		}

		protected PXGraph Graph
		{
			get { return _graph; }
		}

		protected string ViewName
		{
			get { return _viewName; }
		}

		protected Type CacheType
		{
			get { return _cacheType; }
		}

		[PermissionSet(SecurityAction.Assert, Name = "FullTrust")]
		protected virtual IEnumerable GetPreview()
		{
			if (_dataHandler == null)
			{
				var dataHandler = new PXSelectDelegate(() =>
														{
															var cache = Graph.Caches[PreviewType];
															return new[] { cache.Current ?? cache.CreateInstance() };
														});
				var customHandlerName = PreviewType + "_GetPreview";
				var customHandler = Graph.GetType().GetMethod(customHandlerName,
						BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { CacheType }, null);
				if (customHandler != null && typeof(IEnumerable).IsAssignableFrom(customHandler.ReturnType))
					dataHandler = new PXSelectDelegate(() =>
														{
															var row = Graph.Caches[CacheType].Current;
															return (IEnumerable)customHandler.Invoke(Graph, new[] { row });
														});
				_dataHandler = dataHandler;
			}
			return _dataHandler().Cast<object>().ToList();
		}

		[PXButton]
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select)]
		private IEnumerable RefreshPreview(PXAdapter adapter)
		{
			PerformRefresh();
			return adapter.Get();
		}

		protected virtual void PerformRefresh()
		{
			var previewCache = Graph.Caches[PreviewType];
			foreach (object row in GetPreview())
				previewCache.Current = row;
		}
	}

	#endregion

	#region PXFontListAttribute

	public sealed class PXFontListAttribute : PXStringListAttribute
	{
		private static readonly string[] _values;
		private static readonly string[] _labels;

		static PXFontListAttribute()
		{
			var fonts = PX.Common.FontFamilyEx.GetFontNames();
			_values = new string[fonts.Length];
			_labels = new string[fonts.Length];
			var i = 0;
			foreach (string font in fonts)
			{
				_values[i] = font;
				_labels[i] = font;
				i++;
			}
		}

		public PXFontListAttribute()
			: base(_values, _labels)
		{

		}
	}

	#endregion

	#region PXFontSizeListAttribute
	public sealed class PXFontSizeListAttribute : PXIntListAttribute
	{
		private static readonly int[] _values;
		private static readonly string[] _labels;

		static PXFontSizeListAttribute()
		{
			var sizes = PX.Common.FontFamilyEx.DefaultSizes;
			_values = new int[sizes.Length];
			_labels = new string[sizes.Length];
			var i = 0;
			foreach (int font in sizes)
			{
				_values[i] = font;
				_labels[i] = font.ToString();
				i++;
			}
		}

		public PXFontSizeListAttribute()
			: base(_values, _labels)
		{			
		}
	}
	public sealed class PXFontSizeStrListAttribute : PXIntListAttribute
	{
		public PXFontSizeStrListAttribute()
			: base(PX.Common.FontFamilyEx.FontSizes, PX.Common.FontFamilyEx.FontSizesStr)
		{
		}
	}

	#endregion

	#region PXEmailLoadTemplateAttribute

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class PXEmailLoadTemplateAttribute : PXViewExtensionAttribute
	{
		private const string _ACTION_KEY_POSTFIX = "$loadTemplate";
		private const string _ACTION_NAME_POSTFIX = "_load_template";
		private const string _CONTENT_FIELD_NAME = "content";
		private const string _VIEW_NAME_POSTFIX = "$entity";

		private class StubDAC : IBqlTable { }

		#region Fields

		private readonly Type _primaryView;
		private PXGraph _graph;

		#endregion

		public PXEmailLoadTemplateAttribute(Type primaryView)
		{
			if (primaryView == null) throw new ArgumentNullException("primaryView");

			_primaryView = primaryView;
		}

		public Type ContentField { get; set; }

		public Type ReferenceField { get; set; }

		public override void ViewCreated(PXGraph graph, string viewName)
		{
			_graph = graph;

			if (ContentField != null)
			{
				var cache = graph.Views[viewName].Cache;
				cache.FieldDefaultingEvents.Add(
					cache.GetField(ContentField),
					(sender, args) => args.NewValue = "<br />" + GetSignature());
			}

			var key = viewName + _ACTION_KEY_POSTFIX;
			var name = viewName + _ACTION_NAME_POSTFIX;

			var action = PXNamedAction.AddAction(graph, PrimaryView, key, name,
				adapter =>
					{
						if (adapter.Parameters != null)
							for (int i = 0; i < adapter.Parameters.Length; i++)
							{
								object param = adapter.Parameters[i];
								if (param is KeyValuePair<string, string>)
								{
									var pair = (KeyValuePair<string, string>) param;
									var column = pair.Key;
									var value = pair.Value;
									ProcessParameter(column, ref value);
									adapter.Parameters[i] = new KeyValuePair<string, string>(column, value);
								}
							}
						/*var result = new List<object>();
						var resultIterator = adapter.Get().GetEnumerator();
						if (resultIterator.MoveNext())
							result.Add(resultIterator.Current);
						return result;*/
						var current = adapter.View.Cache.Current;
						return new [] { current };
					});
			action.SetVisible(false);

			if (ReferenceField != null)
			{
				var entityView = new PXView(graph, true, new Select<StubDAC>(), 
					new PXSelectDelegate(() =>
						{
							object result = null;
							var cache = graph.Views[viewName].Cache;
							var current = cache.Current;
							var refNoteID = (long?)cache.GetValue(current, ReferenceField.Name);
							if (refNoteID != null)
								result = new EntityHelper(graph).GetEntityRow((long)refNoteID);
							return result == null ? new object[0] : new[] {result};
						}));
				graph.Views.Add(viewName + _VIEW_NAME_POSTFIX, entityView);
			}
		}

		protected PXGraph Graph
		{
			get { return _graph; }
		}

		public Type PrimaryView
		{
			get { return _primaryView; }
		}

		protected virtual void ProcessParameter(string column, ref string value)
		{
			if (string.Compare(column, _CONTENT_FIELD_NAME, true) == 0)
			{
				value = value + "<br />" + GetSignature();
			}
		}

		private string GetSignature()
		{
			var signature = Graph.With(graph => 
				(UserPreferences)PXSelect<UserPreferences>.
				Search<UserPreferences.userID>(graph, PXAccess.GetUserID())).
				With(pref => pref.MailSignature).
				With(sig => sig.Trim());
			return signature != string.Empty ? signature : null;
		}
	}

	#endregion

	#region IncomingMailProtocolsAttribute

	public class IncomingMailProtocolsAttribute : PXIntListAttribute
	{
		public const int _POP3 = 0;
		public const int _IMAP = 1;

		public IncomingMailProtocolsAttribute()
			: base(
				new[] { _POP3, _IMAP },
				new[] { "Pop3", "Imap" })
		{

		}
	}
	#endregion

	#region TypeDelete
	public class TypeDeleteAttribute : PXIntListAttribute
	{
		public const int _Any = 0;
		public const int _Failed = 1;
		public const int _Successful = 2;

		public TypeDeleteAttribute()
			: base(
				new[] { _Any, _Failed, _Successful },
				new[] { "Any", "Failed", "Successful" })
		{

		}
	}
	#endregion

	#region SSlRequest
	public class SSlRequestAttribute : PXIntListAttribute
	{
		public const int _None = 0;
		public const int _SSL = 1;
		public const int _TLS = 2;

		public SSlRequestAttribute()
			: base(
				new[] { _None, _SSL, _TLS },
				new[] { "None", "SSL", "TLS" })
		{

		}
	}
	#endregion

	#region PXVirtualAttribute
	/// <summary>
	/// Prevents rows to be saved to database.
	/// Should be applied to DAC class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class PXVirtualAttribute : Attribute
	{
		static PXVirtualAttribute()
		{
			PXCacheCollection.OnCacheCreated += CacheAttached;
			PXCacheCollection.OnCacheChanged += CacheAttached;
		}

		private static void CacheAttached(PXGraph graph, PXCache cache)
		{
			if (cache != null && IsDefined(cache.GetItemType(), typeof(PXVirtualAttribute), true))
			{
				new PXVirtualDACAttribute().CacheAttached(graph, cache);
				graph.RowPersisting.AddHandler(cache.GetItemType(), RowPersisting);
			}
		}

		private static void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}
	}

	#endregion

	#region PXBypassAuditAttribute
	public class PXBypassAuditAttribute : PXEventSubscriberAttribute
	{
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if(!sender.BypassAuditFields.Contains(this.FieldName))
				sender.BypassAuditFields.Add(this.FieldName);
		}
	}
	#endregion


	#region PXDBUserPasswordAttribute
	public class PXDBUserPasswordAttribute : PXDBCalcedAttribute, IPXFieldUpdatingSubscriber
	{
		public static string DefaultVeil
		{
			get
			{
				return new string('*', 8);
			}
		}

		public PXDBUserPasswordAttribute()
			: base(typeof(Users.password), typeof(string))
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (!sender.BypassAuditFields.Contains(this.FieldName))
				sender.BypassAuditFields.Add(this.FieldName);
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null) e.ReturnValue = GetViewString(e.ReturnValue as String);

			base.FieldSelecting(sender, e);
		}
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null && (!string.IsNullOrWhiteSpace(((Users)e.Row).Password)) && (e.NewValue as string) == DefaultVeil)
			{
				e.NewValue = ((Users)e.Row).Password;
				e.Cancel = true;
			}
		}

		private string GetViewString(String str)
		{
			return String.IsNullOrEmpty(str) ? str : DefaultVeil;
		}
	} 
	#endregion


	#region PXRowsListAttribute
	/// <summary>
	/// User friendly name of DAC class.
	/// </summary>
	public class PXPossibleRowsListAttribute : Attribute
	{
		protected BqlCommand _Select;
		protected string _IDFieldName;
		protected string _ValueFieldName;
		public PXPossibleRowsListAttribute(Type select, Type idField, Type valueField)
		{
			if (select == null)
			{
				throw new PXArgumentException("select", ErrorMessages.ArgumentNullException);
			}
			if (valueField == null)
			{
				throw new PXArgumentException("valueField", ErrorMessages.ArgumentNullException);
			}
			if (idField == null)
			{
				throw new PXArgumentException("idField", ErrorMessages.ArgumentNullException);
			}
			_ValueFieldName = char.ToUpper(valueField.Name[0]) + (valueField.Name.Length > 1 ? valueField.Name.Substring(1) : "");
			_IDFieldName = char.ToUpper(idField.Name[0]) + (idField.Name.Length > 1 ? idField.Name.Substring(1) : "");
			if (typeof(IBqlSearch).IsAssignableFrom(select))
			{
				_Select = BqlCommand.CreateInstance(select);
				Type idfield = BqlCommand.GetItemType(((IBqlSearch)_Select).GetField());
			}
			else if (select.IsNested && typeof(IBqlField).IsAssignableFrom(select))
			{
				_Select = BqlCommand.CreateInstance(typeof(Search<>), select);
			}
			else
			{
				throw new PXArgumentException("type", ErrorMessages.CantCreateForeignKeyReference, select);
			}
		}
		public virtual List<string> GetPossibleRows(PXGraph graph, out string idField, out string valueField)
		{
			idField = _IDFieldName;
			valueField = _ValueFieldName;
			HashSet<string> ret = new HashSet<string>();
			PXView view = new PXView(graph, true, _Select);
			string key = ((IBqlSearch)_Select).GetField().Name;
			foreach (object row in view.SelectMulti())
			{
				string val = view.Cache.GetValue(row, key) as string;
				if (!String.IsNullOrWhiteSpace(val))
				{
					ret.Add(val);
				}
			}
			return ret.ToList();
		}
	}
	#endregion
}

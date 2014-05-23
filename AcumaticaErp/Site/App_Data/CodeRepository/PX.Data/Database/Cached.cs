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
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Reflection.Emit;
using PX.CS;
using PX.SM;

namespace PX.Data
{
	/// <summary>
	/// Selection command, supports sorting, searching, merging with changed items, caching select results.
	/// </summary>

	[System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
	[System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
	public class PXView
	{
		[Serializable]
		protected internal sealed class VersionedList : List<object>
		{
			public int Version = -1;
			public bool AnyMerged;

			public VersionedList()
				: base()
			{
			}

			public VersionedList(IEnumerable<object> collection, int Version)
				: base(collection)
			{
				this.Version = Version;
			}
		}
		protected sealed class Context
		{
			public readonly PXView View;
			public PXSearchColumn[] Sorts;
			public PXFilterRow[] Filters;
			public readonly object[] Currents;
			public readonly object[] Parameters;
			public readonly bool ReverseOrder;
			public int StartRow;
			public readonly int MaximumRows;
			public readonly Type[] RestrictedFields;

			public Context(PXView view, PXSearchColumn[] sorts, PXFilterRow[] filters, object[] currents, object[] parameters, bool reverseOrder, int startRow, int maximumRows,
				Type[] restrictedFields)
			{
				View = view;
				Sorts = sorts;
				Filters = filters;
				Currents = currents;
				Parameters = parameters;
				ReverseOrder = reverseOrder;
				StartRow = startRow;
				MaximumRows = maximumRows;
				RestrictedFields = restrictedFields;
			}
		}
		protected static Stack<Context> _Executing
		{
			get
			{
				Stack<Context> context = PX.Common.PXContext.GetSlot<Stack<Context>>();
				if (context == null)
				{
					context = PX.Common.PXContext.SetSlot<Stack<Context>>(new Stack<Context>());
				}
				return context;
			}
		}

		internal HashSet<Type> RestrictedFields = new HashSet<Type>();

		internal static HashSet<Type> CurrentRestrictedFields
		{
			get
			{
				if (_Executing != null && _Executing.Count > 0)
				{
					var context = _Executing.Peek();
					if (context.RestrictedFields != null)
					{
						return new HashSet<Type>(context.RestrictedFields);
					}
				}
				return new HashSet<Type>();
			}
		}

		public static string[] SortColumns
		{
			get
			{
				List<string> ret = new List<string>();
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null && context.Sorts != null)
					{
						foreach (PXSearchColumn sc in context.Sorts)
						{
							ret.Add(sc.Column);
						}
					}
				}
				return ret.ToArray();
			}
		}
		public static bool[] Descendings
		{
			get
			{
				List<bool> ret = new List<bool>();
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null && context.Sorts != null)
					{
						foreach (PXSearchColumn sc in context.Sorts)
						{
							ret.Add(sc.Descending);
						}
					}
				}
				return ret.ToArray();
			}
		}
		public static object[] Searches
		{
			get
			{
				List<object> ret = new List<object>();
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null && context.Sorts != null)
					{
						foreach (PXSearchColumn sc in context.Sorts)
						{
							ret.Add(sc.OrigSearchValue);
						}
					}
				}
				return ret.ToArray();
			}
		}

		public static PXGraph CurrentGraph
		{
			get
			{
				Context context;
				return _Executing != null && _Executing.Count > 0 && (context = _Executing.Peek()) != null ? context.View.Graph : null;
			}
		}

		public sealed class PXFilterRowCollection : IEnumerable
		{
			private PXFilterRow[] _Array;
			public IEnumerator GetEnumerator()
			{
				return _Array.GetEnumerator();
			}
			public int Length
			{
				get
				{
					return _Array.Length;
				}
			}
			public static implicit operator PXFilterRow[](PXFilterRowCollection collection)
			{
				return collection._Array;
			}
			public PXFilterRowCollection(PXFilterRow[] source)
			{
				_Array = source;
			}
			public PXFilterRow this[int index]
			{
				get
				{
					return _Array[index];
				}
			}

			public void Add(params PXFilterRow[] filters)
			{
				if (_Executing != null && _Executing.Count > 0 && filters.Length > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						List<PXFilterRow> list = new List<PXFilterRow>();
						if (context.Filters != null && context.Filters.Length > 0)
						{
							foreach (PXFilterRow fr in context.Filters)
							{
								list.Add(new PXFilterRow(fr.DataField, fr.Condition, fr.OrigValue ?? fr.Value, fr.OrigValue2 ?? fr.Value2));
								list[list.Count - 1].OpenBrackets = fr.OpenBrackets;
								list[list.Count - 1].CloseBrackets = fr.CloseBrackets;
								list[list.Count - 1].OrOperator = fr.OrOperator;
							}
							if (list.Count > 1)
							{
								list[0].OpenBrackets += 1;
								list[list.Count - 1].CloseBrackets += 1;
							}
							list[list.Count - 1].OrOperator = false;
						}
						foreach (PXFilterRow fr in filters)
						{
							list.Add(new PXFilterRow(fr.DataField, fr.Condition, fr.OrigValue ?? fr.Value, fr.OrigValue2 ?? fr.Value2));
							list[list.Count - 1].OpenBrackets = fr.OpenBrackets;
							list[list.Count - 1].CloseBrackets = fr.CloseBrackets;
							list[list.Count - 1].OrOperator = fr.OrOperator;
						}
						context.Filters = list.ToArray();
					}
				}
			}

			internal void Clear()
			{
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						context.Filters = new PXFilterRow[0];
					}
				}
			}
			internal bool PrepareFileters()
			{
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						return context.View.prepareFilters(_Array);
					}
				}
				return false;
			}
		}
		public static PXFilterRowCollection Filters
		{
			get
			{
				List<PXFilterRow> ret = new List<PXFilterRow>();
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null && context.Filters != null)
					{
						foreach (PXFilterRow fr in context.Filters)
						{
							ret.Add(new PXFilterRow(fr.DataField, fr.Condition, fr.OrigValue ?? fr.Value, fr.OrigValue2 ?? fr.Value));
							ret[ret.Count - 1].OpenBrackets = fr.OpenBrackets;
							ret[ret.Count - 1].CloseBrackets = fr.CloseBrackets;
							ret[ret.Count - 1].OrOperator = fr.OrOperator;
						}
					}
				}
				return new PXFilterRowCollection(ret.ToArray());
			}
		}
		protected internal sealed class PXSortColumnCollection : IEnumerable
		{
			private PXSortColumn[] _Array;
			public IEnumerator GetEnumerator()
			{
				return _Array.GetEnumerator();
			}
			public int Length
			{
				get
				{
					return _Array.Length;
				}
			}
			public static implicit operator PXSortColumn[](PXSortColumnCollection collection)
			{
				return collection._Array;
			}
			public PXSortColumnCollection(PXSortColumn[] source)
			{
				_Array = source;
			}
			public PXSortColumn this[int index]
			{
				get
				{
					return _Array[index];
				}
			}
			public void Add(params PXSortColumn[] sorts)
			{
				if (_Executing != null && _Executing.Count > 0 && sorts.Length > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						List<PXSearchColumn> list = new List<PXSearchColumn>();
						if (context.Filters != null && context.Filters.Length > 0)
						{
							foreach (PXSearchColumn sc in context.Sorts)
							{
								PXSearchColumn copy = new PXSearchColumn(sc.Column, sc.Descending, sc.SearchValue);
								copy.Description = sc.Description;
								copy.OrigSearchValue = sc.OrigSearchValue;
								copy.UseExt = sc.UseExt;
								list.Add(copy);
							}
						}
						foreach (PXSortColumn sc in sorts)
						{
							PXSearchColumn column = null;
							foreach (PXSearchColumn exist in list)
							{
								if (exist.Column == sc.Column)
								{
									column = exist;
									break;
								}
							}

							if (column == null)
							{
								column = new PXSearchColumn(sc.Column, sc.Descending, null);
								list.Add(column);
							}
							else
							{
								column.Descending = sc.Descending;
							}
							column.UseExt = sc.UseExt;
						}
						context.Sorts = list.ToArray();
					}
				}
			}
			internal void Clear()
			{
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						context.Sorts = new PXSearchColumn[0];
					}
				}
			}
		}
		protected internal static PXSortColumnCollection Sorts
		{
			get
			{
				List<PXSortColumn> ret = new List<PXSortColumn>();
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null && context.Sorts != null)
					{
						foreach (PXSearchColumn sc in context.Sorts)
						{
							ret.Add(new PXSortColumn(sc.Column, sc.Descending));
							ret[ret.Count - 1].UseExt = sc.UseExt;
						}
					}
				}
				return new PXSortColumnCollection(ret.ToArray());
			}
		}

		public static object[] Currents
		{
			get
			{
				List<object> ret = new List<object>();
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null && context.Currents != null)
					{
						foreach (object item in context.Currents)
						{
							ret.Add(item);
						}
					}
				}
				return ret.ToArray();
			}
		}
		public static object[] Parameters
		{
			get
			{
				List<object> ret = new List<object>();
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null && context.Parameters != null)
					{
						foreach (object item in context.Parameters)
						{
							ret.Add(item);
						}
					}
				}
				return ret.ToArray();
			}
		}

		public static int StartRow
		{
			get
			{
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						if (context.ReverseOrder && context.MaximumRows > 0)
						{
							return -1 - context.StartRow;
						}
						else
						{
							return context.StartRow;
						}
					}
				}
				return 0;
			}
			set
			{
				if (_Executing != null)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						context.StartRow = value;
					}
				}
			}
		}
		public static int MaximumRows
		{
			get
			{
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						return context.MaximumRows;
					}
				}
				return 0;
			}
		}
		public static bool ReverseOrder
		{
			get
			{
				if (_Executing != null && _Executing.Count > 0)
				{
					Context context = _Executing.Peek();
					if (context != null)
					{
						return context.ReverseOrder;
					}
				}
				return false;
			}
		}
		public static IEnumerable Sort(IEnumerable list)
		{
			if (_Executing != null)
			{
				Context context = _Executing.Peek();
				if (context != null)
				{
					if (list is List<object>)
					{
						context.View.SortResult((List<object>)list, context.Sorts, context.ReverseOrder);
						return list;
					}
					else
					{
						List<object> result = list is VersionedList ? new VersionedList() : new List<object>();
						foreach (object item in list)
						{
							result.Add(item);
						}
						context.View.SortResult(result, context.Sorts, context.ReverseOrder);
						if (result is VersionedList)
						{
							((VersionedList)result).Version = ((VersionedList)list).Version;
						}
						return result;
					}
				}
			}
			return list;
		}
		public static IEnumerable Filter(IEnumerable list)
		{
			if (_Executing != null)
			{
				Context context = _Executing.Peek();
				if (context != null)
				{
					if (list is List<object>)
					{
						context.View.FilterResult((List<object>)list, context.Filters);
						return list;
					}
					else
					{
						List<object> result = list is VersionedList ? new VersionedList() : new List<object>();
						foreach (object item in list)
						{
							result.Add(item);
						}
						context.View.FilterResult(result, context.Filters);
						if (result is VersionedList)
						{
							((VersionedList)result).Version = ((VersionedList)list).Version;
						}
						return result;
					}
				}
			}
			return list;
		}

		internal PXGraphExtension[] Extensions;
		protected PXGraphExtension[] GraphExtensions { get { return Extensions; } }
		private Func<PXFilterRow[]> _ExternalFilterDelegate;
		internal void StoreExternalFilters(Func<PXFilterRow[]> del)
		{
			_ExternalFilterDelegate = del;
		}

		public PXFilterRow[] GetExternalFilters()
		{
			if (_ExternalFilterDelegate != null)
			{
				return _ExternalFilterDelegate();
			}
			return null;
		}

		protected PXGraph _Graph;
		/// <summary>
		/// Parent business object
		/// </summary>
		public virtual PXGraph Graph
		{
			get
			{
				return _Graph;
			}
			set
			{
				_Graph = value;
			}
		}

		public PXViewExtensionAttribute[] Attributes;

		protected bool _IsReadOnly = false;
		/// <summary>
		/// If updates and merging allowed
		/// </summary>
		public virtual bool IsReadOnly
		{
			get
			{
				return _IsReadOnly;
			}
			set
			{
				_IsReadOnly = value;
				_SelectQueries = null;
			}
		}

		private BqlCommand _Select;
		public override string ToString()
		{
			return _Select.GetText(_Graph, RestrictedFields.Any() ? this : null);
		}

		protected Delegate _Delegate;
		public Delegate BqlDelegate
		{
			get
			{
				return _Delegate;
			}
		}

		protected Type _CacheType;
		protected Type CacheType
		{
			get
			{
				if (_CacheType == null)
				{
					_CacheType = _Select.GetTables()[0];
				}
				return _CacheType;
			}
		}
		protected internal PXCache _Cache;
		/// <summary>
		/// Primary table cache
		/// </summary>
		public virtual PXCache Cache
		{
			get
			{
				if (_Cache == null || _Graph.stateLoading)
				{
					_Cache = Graph.Caches[CacheType];
				}
				return _Cache;
			}
		}

		/// <summary>
		/// Gets item type of the primary cache
		/// </summary>
		/// <returns>Item type</returns>
		public virtual Type GetItemType()
		{
			return CacheType;
		}

		public virtual BqlCommand BqlSelect
		{
			get
			{
				return _Select;
			}
		}

		public virtual Type BqlTarget
		{
			get
			{
				if (_Delegate != null)
				{
					return _Delegate.Method.DeclaringType;
				}
				return null;
			}
		}

		/// <summary>
		/// Gets item types of the caches
		/// </summary>
		/// <returns>Item type</returns>
		public virtual Type[] GetItemTypes()
		{
			return _Select.GetTables();
		}

		protected string[] _ParameterNames;
		/// <summary>
		/// Gets the names of the parameters
		/// </summary>
		/// <returns></returns>
		public virtual string[] GetParameterNames()
		{
			if (_ParameterNames == null)
			{
				var list = EnumParameters();
				_ParameterNames = new string[list.Count];

				foreach (PXViewParameter info in list)
				{
					_ParameterNames[info.Ordinal] = info.Name;
				}

			}

			return _ParameterNames;


		}

		public virtual List<PXViewParameter> EnumParameters()
		{
			var result = new List<PXViewParameter>();
			//if (_ParameterNames == null)
			{
				IBqlParameter[] pars = _Select.GetParameters();
				ParameterInfo[] pi = _Delegate == null ? null : _Delegate.Method.GetParameters();
				int k = 0;
				//var names = new List<string>();
				for (int i = 0; i < pars.Length; i++)
				{
					if (!pars[i].IsVisible)
					{
						continue;
					}
					Type rt = pars[i].GetReferencedType();
					if (typeof(IBqlField).IsAssignableFrom(rt) && rt.IsNested)
					{
						string n = String.Format("{0}.{1}", BqlCommand.GetItemType(rt).Name, rt.Name);
						result.Add(new PXViewParameter { Name = n, Bql = pars[i], Ordinal = result.Count });
					}
					else if (pars[i].IsArgument)
					{
						if (pi == null || k >= pi.Length || rt != pi[k].ParameterType && rt != pi[k].ParameterType.GetElementType())
						{
							throw new PXException(ErrorMessages.DelegateArgsDontMeetSelectionCommandPars);
						}
						string n = pi[k].Name;
						result.Add(new PXViewParameter { Name = n, Bql = pars[i], Argument = pi[k], Ordinal = result.Count });
						k++;
					}
				}
				if (pi != null)
				{
					for (; k < pi.Length; k++)
					{
						string n = pi[k].Name;
						result.Add(new PXViewParameter { Name = n, Argument = pi[k], Ordinal = result.Count });
					}
				}
				//_ParameterNames = names.ToArray();
			}
			return result;
		}

		PXViewQueryCollection _SelectQueries;
		protected PXViewQueryCollection SelectQueries
		{
			get
			{


				if (_SelectQueries != null)
					return _SelectQueries;


				if (this.GetType() != typeof(PXView) || _Delegate != null)
				{
					_SelectQueries = new PXViewQueryCollection { CacheType = CacheType, IsViewReadonly = _IsReadOnly };
					_Graph.TypedViews._NonstandardViews.Add(_SelectQueries);
					return _SelectQueries;
				}



				_Graph.LoadQueryCache();

				//if(!_Graph.TypedViews._QueriesLoaded)
				//{
				//    _SelectQueries = new PXViewQueryCollection { CacheType = CacheType, IsViewReadonly = _IsReadOnly };
				//    return _SelectQueries;

				//}


				Type selectType = _Select.GetSelectType();
				PXViewQueryCollection viewQueries;
				var viewKey = new ViewKey(selectType, _IsReadOnly);
				if (!_Graph.QueryCache.TryGetValue(viewKey, out viewQueries))
				{
					_SelectQueries = new PXViewQueryCollection { CacheType = CacheType, IsViewReadonly = _IsReadOnly };
					_Graph.QueryCache[viewKey] = _SelectQueries;
					return _SelectQueries;

				}


				_SelectQueries = viewQueries;


				if (!IsReadOnly)
				{
					PXCache cache = Cache;
					foreach (var queryResult in viewQueries.Values)
					{
						for (int i = 0; i < queryResult.Items.Count; i++)
						{
							PXResult result = null;
							object item = queryResult.Items[i];
							if (item is PXResult)
							{
								result = ((PXResult)item);
								item = result[0];
							}
							try
							{
								bool wasUpdated;
								object newitem = cache.PlaceNotChanged(item, out wasUpdated);
								if (!object.ReferenceEquals(newitem, item))
								{
									if (result != null)
									{
										result[0] = newitem;
									}
									else
									{
										queryResult.Items[i] = newitem;
									}
								}
							}
							catch (InvalidCastException)
							{
								_SelectQueries = new PXViewQueryCollection { CacheType = CacheType, IsViewReadonly = false };
								_Graph.QueryCache[viewKey] = _SelectQueries;
								break;
							}
						}
					}
				}


				return _SelectQueries;
			}
		}
		/// <summary>
		/// Clears internal results cache
		/// </summary>
		public virtual void Clear()
		{
			SelectQueries.Clear();
		}

		public void DetachCache()
		{
			_SelectQueries = new PXViewQueryCollection();
		}

		/// <summary>
		/// Instantiates a selection command
		/// </summary>
		/// <param name="graph">Parent business object</param>
		/// <param name="isReadOnly">If updates and merging allowed</param>
		/// <param name="select">Bql command</param>
		public PXView(PXGraph graph, bool isReadOnly, BqlCommand select)
		{
			_Graph = graph;
			_IsReadOnly = isReadOnly;
			_Select = select;
			if (!_IsReadOnly)
			{
				_Cache = graph.Caches[CacheType];
			}
			PXExtensionManager.InitExtensions(this);
		}

		/// <summary>
		/// Instantiates a selection command
		/// </summary>
		/// <param name="graph">Parent business object</param>
		/// <param name="isReadOnly">If updates and merging allowed</param>
		/// <param name="select">Bql command</param>
		/// <param name="handler">Either PXPrepareDelegate or PXSelectDelegate</param>
		public PXView(PXGraph graph, bool isReadOnly, BqlCommand select, Delegate handler)
			: this(graph, isReadOnly, select)
		{
			if (handler == null)
			{
				throw new PXArgumentException("handler", ErrorMessages.ArgumentNullException);
			}
			Type t = handler.GetType();
			if (!t.IsGenericType)
			{
				if (t != typeof(PXSelectDelegate) && t != typeof(PXPrepareDelegate))
				{
					throw new PXException(ErrorMessages.InvalidDelegate);
				}
			}
			else
			{
				t = t.GetGenericTypeDefinition();
				if (t != typeof(PXSelectDelegate<>) && t != typeof(PXPrepareDelegate<>) &&
					t != typeof(PXSelectDelegate<,>) && t != typeof(PXPrepareDelegate<,>) &&
					t != typeof(PXSelectDelegate<,,>) && t != typeof(PXPrepareDelegate<,,>) &&
					t != typeof(PXSelectDelegate<,,,>) && t != typeof(PXPrepareDelegate<,,,>) &&
					t != typeof(PXSelectDelegate<,,,,>) && t != typeof(PXPrepareDelegate<,,,,>) &&
					t != typeof(PXSelectDelegate<,,,,,>) && t != typeof(PXPrepareDelegate<,,,,,>) &&
					t != typeof(PXSelectDelegate<,,,,,,>) && t != typeof(PXPrepareDelegate<,,,,,,>) &&
					t != typeof(PXSelectDelegate<,,,,,,,>) && t != typeof(PXPrepareDelegate<,,,,,,,>) &&
					t != typeof(PXSelectDelegate<,,,,,,,,>) && t != typeof(PXPrepareDelegate<,,,,,,,,>) &&
					t != typeof(PXSelectDelegate<,,,,,,,,>) && t != typeof(PXPrepareDelegate<,,,,,,,,>) &&
					t != typeof(PXSelectDelegate<,,,,,,,,,,>) && t != typeof(PXPrepareDelegate<,,,,,,,,,,>)
				)
				{
					throw new PXException(ErrorMessages.InvalidDelegate);
				}
			}
			_Delegate = handler;

			PXExtensionManager.InitExtensions(this);
		}

		protected bool _AllowSelect = true;
		public bool AllowSelect
		{
			get
			{
				if (_AllowSelect)
				{
					return Cache.AllowSelect;
				}
				return false;
			}
			set
			{
				_AllowSelect = value;
			}
		}

		protected bool _AllowInsert = true;
		public bool AllowInsert
		{
			get
			{
				if (_AllowInsert && !IsReadOnly)
				{
					return Cache.AllowInsert;
				}
				return false;
			}
			set
			{
				_AllowInsert = value;
			}
		}

		protected bool _AllowUpdate = true;
		public bool AllowUpdate
		{
			get
			{
				if (_AllowUpdate && !IsReadOnly)
				{
					return Cache.AllowUpdate;
				}
				return false;
			}
			set
			{
				_AllowUpdate = value;
			}
		}

		protected bool _AllowDelete = true;
		public bool AllowDelete
		{
			get
			{
				if (_AllowDelete && !IsReadOnly)
				{
					return Cache.AllowDelete;
				}
				return false;
			}
			set
			{
				_AllowDelete = value;
			}
		}

		private Type ConstructSort(PXSearchColumn[] sorts, List<PXDataValue> pars, bool reverseOrder)
		{
			Type newsort = null;
			List<Type> fields = Cache.BqlFields;
			int parspos = pars.Count;
			for (int i = sorts.Length - 1; i >= 0; i--)
			{
				Type b = null;
				for (int j = fields.Count - 1; j >= 0; j--)
				{
					if (String.Compare(fields[j].Name, sorts[i].Column, StringComparison.OrdinalIgnoreCase) == 0)
					{
						b = fields[j];
						break;
					}
				}
				if (!sorts[i].UseExt)
				{
					if (b != null)
					{
						if (reverseOrder ^ sorts[i].Descending)
						{
							if (newsort == null)
							{
								newsort = typeof(Desc<>).MakeGenericType(b);
							}
							else
							{
								newsort = typeof(Desc<,>).MakeGenericType(b, newsort);
							}
						}
						else
						{
							if (newsort == null)
							{
								newsort = typeof(Asc<>).MakeGenericType(b);
							}
							else
							{
								newsort = typeof(Asc<,>).MakeGenericType(b, newsort);
							}
						}
					}
				}
				else if (sorts[i].Description != null && !String.IsNullOrEmpty(sorts[i].Description.FieldName))
				{
					PXCommandPreparingEventArgs.FieldDescription descr = null;
					if (b != null && b.IsNested && CacheType != Cache.GetItemType()
						&& (BqlCommand.GetItemType(b) == CacheType || CacheType.IsSubclassOf(BqlCommand.GetItemType(b)))
						&& sorts[i].Description.FieldName != BqlCommand.Null)
					{
						Cache.RaiseCommandPreparing(sorts[i].Column, null, sorts[i].SearchValue, PXDBOperation.Select | PXDBOperation.External, CacheType, out descr);
					}
					if (descr == null || String.IsNullOrEmpty(descr.FieldName))
					{
						descr = sorts[i].Description;
					}
					if (reverseOrder ^ sorts[i].Descending)
					{
						if (newsort == null)
						{
							newsort = typeof(FieldNameDesc);
						}
						else
						{
							newsort = typeof(FieldNameDesc<>).MakeGenericType(newsort);
						}
					}
					else
					{
						if (newsort == null)
						{
							newsort = typeof(FieldNameAsc);
						}
						else
						{
							newsort = typeof(FieldNameAsc<>).MakeGenericType(newsort);
						}
					}
					string fieldName = descr.FieldName;
					int idxdot, idxscore;
					if ((idxdot = fieldName.IndexOf('.')) != -1 && fieldName.IndexOf('(') == -1 && (idxscore = sorts[i].Column.IndexOf("__")) != -1)
					{
						fieldName = sorts[i].Column.Substring(0, idxscore) + fieldName.Substring(idxdot);
					}
					if (_Select is IBqlAggregate && !String.IsNullOrEmpty(fieldName))
					{
						if (_Selection == null)
						{
							_Selection = new BqlCommand.Selection();
							StringBuilder text = new StringBuilder();
							_Select.Parse(_Graph, new List<IBqlParameter>(), new List<Type>(), null, null, text, _Selection);
						}
						string newname = _Selection.Get(fieldName);
						if (!String.Equals(newname, fieldName, StringComparison.OrdinalIgnoreCase))
						{
							fieldName = newname;
						}
						else
						{
							Cache.RaiseCommandPreparing(sorts[i].Column, null, null, PXDBOperation.Select, Cache.GetItemType(), out descr);
							if (descr != null && !String.IsNullOrEmpty(descr.FieldName))
							{
								newname = _Selection.Get(descr.FieldName);
								if (!String.IsNullOrEmpty(newname) && !String.Equals(newname, descr.FieldName, StringComparison.OrdinalIgnoreCase))
								{
									int pos = fieldName.IndexOf(descr.FieldName);
									while (pos != -1)
									{
										if ((pos == 0 || !char.IsLetterOrDigit(fieldName[pos - 1]))
											&& (pos + descr.FieldName.Length == fieldName.Length || !char.IsLetterOrDigit(fieldName[pos + descr.FieldName.Length])))
										{
											fieldName = fieldName.Substring(0, pos) + newname + (pos + descr.FieldName.Length == fieldName.Length ? "" : fieldName.Substring(pos + descr.FieldName.Length));
											pos = pos + newname.Length + 1;
										}
										else
										{
											pos = pos + descr.FieldName.Length + 1;
										}
										pos = fieldName.IndexOf(descr.FieldName, pos);
									}
								}
							}
						}
					}
					pars.Insert(parspos, new PXFieldName(fieldName));
				}
			}
			if (newsort != null)
			{
				return typeof(OrderBy<>).MakeGenericType(newsort);
			}
			else
			{
				return null;
			}
		}

		protected BqlCommand.Selection _Selection;

		public virtual KeyValuePair<string, bool>[] GetSortColumns()
		{
			bool nos, search, resetTopCount = false;
			PXSearchColumn[] columns = prepareSorts(null, null, null, 1, out nos, out search, ref resetTopCount);
			KeyValuePair<string, bool>[] ret = new KeyValuePair<string, bool>[columns.Length];
			for (int i = 0; i < columns.Length; i++)
			{
				string name = columns[i].Column;
				if (!String.IsNullOrEmpty(name))
				{
					name = char.ToUpper(columns[i].Column[0]) + columns[i].Column.Substring(1);
				}
				ret[i] = new KeyValuePair<string, bool>(name, columns[i].Descending);
			}
			return ret;
		}

		protected bool HasUnboundSort(PXSearchColumn[] sorts)
		{
			for (int i = 0; i < sorts.Length; i++)
			{
				if (sorts[i].Description == null)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Prepares sort columns
		/// </summary>
		/// <param name="sortcolumns">First sort columns</param>
		/// <param name="needOverrideSort">Output if the Bql command need to be composed with the new sort expression</param>
		/// <returns>Sort columns</returns>
		protected virtual PXSearchColumn[] prepareSorts(string[] sortcolumns, bool[] descendings, object[] searches, int topCount, out bool needOverrideSort, out bool anySearch, ref bool resetTopCount)
		{
			object row = null;
			IBqlSortColumn[] selsort = _Select.GetSortColumns();
			bool needUpdateSelect = false;
			anySearch = false;

			bool hasNonFieldSorts = false;

			if (selsort.Length == 0)
			{
				if (sortcolumns == null || sortcolumns.Length == 0)
				{
					sortcolumns = new string[Cache.Keys.Count];
					Cache.Keys.CopyTo(sortcolumns);
					needUpdateSelect = Cache.Keys.Count > 0;
					needOverrideSort = false;
				}
				else
				{
					List<string> cols = new List<string>(sortcolumns);
					foreach (string key in Cache.Keys)
					{
						if (!CompareIgnoreCase.IsInList(cols, key))
						{
							cols.Add(key);
						}
					}
					sortcolumns = cols.ToArray();
					needOverrideSort = true;
				}
			}
			else
			{
				List<string> cols;
				List<bool> descs;
				if (sortcolumns != null && sortcolumns.Length > 0)
				{
					cols = new List<string>(sortcolumns);
					if (descendings != null)
					{
						descs = new List<bool>(descendings);
					}
					else
					{
						descs = new List<bool>(sortcolumns.Length);
					}
					for (int i = descs.Count; i < cols.Count; i++)
					{
						descs.Add(false);
					}
					for (int i = 0; i < selsort.Length; i++)
					{
						Type ct = selsort[i].GetReferencedType();
						if (ct != null && typeof(IBqlField).IsAssignableFrom(ct))
						{
							string field;
							if (!(_Select is IBqlJoinedSelect) || BqlCommand.GetItemType(ct) == CacheType || CacheType.IsSubclassOf(BqlCommand.GetItemType(ct)))
							{
								field = ct.Name;
							}
							else
							{
								field = BqlCommand.GetItemType(ct).Name + "__" + ct.Name;
							}
							if (!CompareIgnoreCase.IsInList(cols, field))
							{
								cols.Add(field);
								descs.Add(selsort[i].IsDescending);
							}
						} else // Most likely an expression (such as Switch<Case<...>>) that cannot be represented with just (String fieldname, boolean isDesc)
							hasNonFieldSorts = true;
					}
					needOverrideSort = true;
				}
				else
				{
					cols = new List<string>();
					descs = new List<bool>();
					for (int i = 0; i < selsort.Length; i++)
					{
						Type ct = selsort[i].GetReferencedType();
						if (ct != null && typeof(IBqlField).IsAssignableFrom(ct)) {
							string field;
							if (!(_Select is IBqlJoinedSelect) || BqlCommand.GetItemType(ct) == CacheType || CacheType.IsSubclassOf(BqlCommand.GetItemType(ct))) {
								field = ct.Name;
							}
							else {
								field = BqlCommand.GetItemType(ct).Name + "__" + ct.Name;
							}
							cols.Add(field);
							descs.Add(selsort[i].IsDescending);
						}
						else // Most likely an expression (such as Switch<Case<...>>) that cannot be represented with just (String fieldname, boolean isDesc)
							hasNonFieldSorts = true;
					}
					needOverrideSort = false;
				}
				
				foreach (string key in Cache.Keys)
				{
					if (!CompareIgnoreCase.IsInList(cols, key))
					{
						cols.Add(key);
						if (!hasNonFieldSorts) // if there are complex expressions, don't try to rebuild the sort clause from string+bool arrays in PXView.GetResult
							needOverrideSort = true;
					}
				}
				sortcolumns = cols.ToArray();
				descendings = descs.ToArray();
			}

			// Fill PXSearchColumn "sorts" array
			PXSearchColumn[] sorts = new PXSearchColumn[sortcolumns.Length];
			for (int i = 0; i < sortcolumns.Length; i++)
			{
				object val = searches != null && i < searches.Length ? searches[i] : null;
				bool desc = descendings != null && i < descendings.Length && descendings[i];

				string columnName = sortcolumns[i];
				PXSearchColumn searchColumn = sorts[i] = new PXSearchColumn(columnName, desc, val);
				
				try
				{
					if (val != null) {
						anySearch = true;
						row = RaiseFieldUpdatingForSearchColumn(searchColumn, topCount, columnName, row, val);
					}
				}
				catch (Exception)
				{
					searchColumn.UseExt = true;
				}
				bool topCountNeedsReset = false;
				try {
					RaiseCommandPreparingForSearchColumn(searchColumn, topCount, columnName, row, out topCountNeedsReset);
				} catch (Exception) {}
				if( topCountNeedsReset )
					resetTopCount = true;
			}
			if (needUpdateSelect)
			{
				List<PXDataValue> pars = new List<PXDataValue>();
				Type neworder = ConstructSort(sorts, pars, false);
				if (pars.Count == 0)
				{
					_Select = _Select.OrderByNew(neworder);
				}
			}
			return sorts;
		}

		private object RaiseFieldUpdatingForSearchColumn(PXSearchColumn searchColumn, int topCount, string columnName, object row, object val)
		{
			Cache.RaiseFieldUpdating(columnName, row, ref val);
			if (val == null) {
				searchColumn.UseExt = true;
				return row;
			}

			try
			{
				if (columnName.IndexOf("__") == -1)
				{
					if (row == null)
					{
						row = Cache.CreateInstance();
					}
					Cache.SetValue(row, columnName, val);
				}
			}
			catch
			{
			}
			if (topCount == 1)
			{
				searchColumn.SearchValue = val;
			}
			else
			{
				PXFieldState state = Cache.GetStateExt(row, columnName) as PXFieldState;
				if (state != null && state.DataType == val.GetType())
				{
					if (searchColumn.Descending && val is string && ((string) val).Length < state.Length)
					{
						searchColumn.SearchValue = val + new string((char) 0xF8FF, state.Length - ((string) val).Length);
					}
					else
					{
						searchColumn.SearchValue = val;
					}
				}
				else
				{
					searchColumn.UseExt = true;
				}
			}
			return row;
		}

		private void RaiseCommandPreparingForSearchColumn(PXSearchColumn searchColumn, int topCount, string columnName, object row, out bool resetTopCount)
		{
			resetTopCount = false;

			PXCommandPreparingEventArgs.FieldDescription descr;
			if (topCount == 1 && searchColumn.SearchValue == null)
			{
				Cache.RaiseCommandPreparing(columnName, null, null, PXDBOperation.Select, Cache.GetItemType(), out descr);
			}
			else
			{
				Cache.RaiseCommandPreparing(columnName, null, searchColumn.SearchValue, PXDBOperation.Select | PXDBOperation.External, Cache.GetItemType(), out descr);
				if (descr != null && !String.IsNullOrEmpty(descr.FieldName))
				{
					PXCommandPreparingEventArgs.FieldDescription emptydescr;
					Cache.RaiseCommandPreparing(columnName, null, null, PXDBOperation.Select | PXDBOperation.External,
						Cache.GetItemType(), out emptydescr);
					if (emptydescr == null || String.IsNullOrEmpty(emptydescr.FieldName))
					{
						descr = null;
					}
				}
			}
			if (descr != null && !String.IsNullOrEmpty(descr.FieldName) && descr.FieldName != BqlCommand.Null)
			{
				searchColumn.Description = descr;
				if (descr.FieldName.StartsWith(BqlCommand.SubSelect))
				{
					searchColumn.UseExt = true;
					searchColumn.SearchValue = searchColumn.OrigSearchValue;
				}
			}
			else
			{
				PXFieldState state = Cache.GetStateExt(row, searchColumn.Column) as PXFieldState;
				if (state == null || searchColumn.OrigSearchValue == null || state.DataType == searchColumn.OrigSearchValue.GetType())
				{
					searchColumn.SearchValue = searchColumn.OrigSearchValue;
				}
				searchColumn.UseExt = true;
				//sorts[i].SearchValue = sorts[i].OrigSearchValue;
				resetTopCount = true;
			}
		}

		/// <summary>
		/// Prepares parameters, formats input values, gets default values for the hidden and not supplied parameters
		/// </summary>
		/// <param name="currents">Objects to use as current instances when processing Current and Optional parameters</param>
		/// <param name="parameters">Input parameter values</param>
		/// <returns>Parameters including those for the manual method if defined</returns>
		public virtual object[] PrepareParameters(object[] currents, object[] parameters)
		{
			List<object> list = new List<object>();
			IBqlParameter[] selpars = _Select.GetParameters();
			int pcnt = 0;
			int acnt = 0;
			ParameterInfo[] args = null;
			for (int i = 0; i < selpars.Length; i++)
			{
				object val = null;
				if (selpars[i].IsVisible && parameters != null && pcnt < parameters.Length)
				{
					val = parameters[pcnt];
					pcnt++;
				}
				if (val == null)
				{
					if (selpars[i].HasDefault)
					{
						Type ft = selpars[i].GetReferencedType();
						if (ft.IsNested)
						{
							Type ct = BqlCommand.GetItemType(ft);
							PXCache cache = _Graph.Caches[ct];
							bool currfound = false;
							if (currents != null)
							{
								for (int k = 0; k < currents.Length; k++)
								{
									if (currents[k] != null && (currents[k].GetType() == ct || currents[k].GetType().IsSubclassOf(ct)))
									{
										val = cache.GetValue(currents[k], ft.Name);
										currfound = true;
										break;
									}
								}
							}
							if (!currfound && val == null && (cache._Current ?? cache.Current) != null)
							{
								val = cache.GetValue((cache._Current ?? cache.Current), ft.Name);
							}
							if (val == null && selpars[i].TryDefault)
							{
								if (cache.RaiseFieldDefaulting(ft.Name, null, out val))
								{
									cache.RaiseFieldUpdating(ft.Name, null, ref val);
								}
							}
							if (selpars[i].MaskedType != null && !selpars[i].IsArgument)
							{
								object row = (cache._Current ?? cache.Current);
								if (currents != null)
								{
									for (int k = 0; k < currents.Length; k++)
									{
										if (currents[k] != null && (currents[k].GetType() == ct || currents[k].GetType().IsSubclassOf(ct)))
										{
											row = currents[k];
											break;
										}
									}
								}
								val = GroupHelper.GetReferencedValue(cache, row, ft.Name, val, _Graph._ForceUnattended);
							}
						}
					}
					else if (selpars[i].IsArgument && _Delegate != null)
					{
						if (args == null)
						{
							args = _Delegate.Method.GetParameters();
						}
						if (acnt < args.Length)
						{
							object[] attributes = args[acnt].GetCustomAttributes(typeof(PXEventSubscriberAttribute), false);
							foreach (PXEventSubscriberAttribute attr in attributes)
							{
								List<IPXFieldDefaultingSubscriber> del = new List<IPXFieldDefaultingSubscriber>();
								attr.GetSubscriber<IPXFieldDefaultingSubscriber>(del);
								if (del.Count > 0)
								{
									PXFieldDefaultingEventArgs defs = new PXFieldDefaultingEventArgs(null);
									for (int l = 0; l < del.Count; l++)
									{
										del[l].FieldDefaulting(Cache, defs);
									}
									val = defs.NewValue;
									break;
								}
							}
							foreach (PXEventSubscriberAttribute attr in attributes)
							{
								List<IPXFieldUpdatingSubscriber> del = new List<IPXFieldUpdatingSubscriber>();
								attr.GetSubscriber<IPXFieldUpdatingSubscriber>(del);
								if (del.Count > 0)
								{
									PXFieldUpdatingEventArgs upds = new PXFieldUpdatingEventArgs(null, val);
									for (int l = 0; l < del.Count; l++)
									{
										del[l].FieldUpdating(Cache, upds);
									}
									val = upds.NewValue;
								}
							}
						}
						acnt++;
					}
				}
				else
				{
					if (selpars[i].HasDefault)
					{
						Type ft = selpars[i].GetReferencedType();
						if (ft.IsNested)
						{
							Type ct = BqlCommand.GetItemType(ft);
							PXCache cache = _Graph.Caches[ct];
							object row = (cache._Current ?? cache.Current);
							if (currents != null)
							{
								for (int k = 0; k < currents.Length; k++)
								{
									if (currents[k] != null && (currents[k].GetType() == ct || currents[k].GetType().IsSubclassOf(ct)))
									{
										row = currents[k];
										break;
									}
								}
							}
							cache.RaiseFieldUpdating(ft.Name, row, ref val);
							if (selpars[i].MaskedType != null && !selpars[i].IsArgument)
							{
								val = GroupHelper.GetReferencedValue(cache, row, ft.Name, val, _Graph._ForceUnattended);
							}
						}
					}
					else if (selpars[i].IsArgument && _Delegate != null)
					{
						if (args == null)
						{
							args = _Delegate.Method.GetParameters();
						}
						if (acnt < args.Length)
						{
							foreach (PXEventSubscriberAttribute attr in args[acnt].GetCustomAttributes(typeof(PXEventSubscriberAttribute), false))
							{
								List<IPXFieldUpdatingSubscriber> del = new List<IPXFieldUpdatingSubscriber>();
								attr.GetSubscriber<IPXFieldUpdatingSubscriber>(del);
								if (del.Count > 0)
								{
									PXFieldUpdatingEventArgs upds = new PXFieldUpdatingEventArgs(null, val);
									for (int l = 0; l < del.Count; l++)
									{
										del[l].FieldUpdating(Cache, upds);
									}
									val = upds.NewValue;
								}
							}
						}
						acnt++;
					}
				}
				list.Add(val);
			}
			if (parameters != null)
			{
				for (; pcnt < parameters.Length; pcnt++)
				{
					object val = parameters[pcnt];
					if (_Delegate != null)
					{
						if (args == null)
						{
							args = _Delegate.Method.GetParameters();
						}
						if (acnt < args.Length)
						{
							foreach (PXEventSubscriberAttribute attr in args[acnt].GetCustomAttributes(typeof(PXEventSubscriberAttribute), false))
							{
								List<IPXFieldUpdatingSubscriber> del = new List<IPXFieldUpdatingSubscriber>();
								attr.GetSubscriber<IPXFieldUpdatingSubscriber>(del);
								if (del.Count > 0)
								{
									PXFieldUpdatingEventArgs upds = new PXFieldUpdatingEventArgs(null, val);
									for (int l = 0; l < del.Count; l++)
									{
										del[l].FieldUpdating(Cache, upds);
									}
									val = upds.NewValue;
								}
							}
						}
						acnt++;
					}
					list.Add(val);
				}
			}
			return list.ToArray();
		}

		protected virtual bool prepareFilters(PXFilterRow[] filters)
		{
			bool anyFailed = false;
			if (filters != null)
			{
				PXCache cache = Cache;
				int brackets = 0;
				foreach (PXFilterRow fr in filters)
				{
					if (string.IsNullOrEmpty(fr.DataField)) continue;
					fr.OrigValue = fr.Value;
					fr.OrigValue2 = fr.Value2;
					if (brackets > 0)
					{
						brackets += fr.OpenBrackets;
						brackets -= fr.CloseBrackets;
						if (cache == Cache)
						{
							fr.Value = null;
							fr.Value2 = null;
							continue;
						}
					}
					else
					{
						cache = Cache;
					}
					int idx = fr.DataField.IndexOf("__");
					string fieldName = fr.DataField;
					if (idx != -1 && cache == Cache)
					{
						Type[] tables = _Select.GetTables();
						string tableName = fr.DataField.Substring(0, idx);
						for (int i = 1; i < tables.Length; i++)
						{
							if (String.Equals(tables[i].Name, tableName))
							{
								cache = _Graph.Caches[tables[i]];
								fieldName = fr.DataField.Substring(idx + 2);
								break;
							}
						}
					}
					if (fr.Condition == PXCondition.EQ && fr.Value is bool && !((bool)fr.Value))
					{
						fr.Condition = PXCondition.NE;
						fr.OrigValue = fr.Value = true;
					}
					if (fr.Value is string
							&& (fr.Condition == PXCondition.EQ && string.Equals((string)fr.Value, "False", StringComparison.OrdinalIgnoreCase)
							|| fr.Condition == PXCondition.NE && string.Equals((string)fr.Value, "True", StringComparison.OrdinalIgnoreCase)))
					{
						PXFieldState fs = cache.GetStateExt(null, fieldName) as PXFieldState;
						if (fs != null && fs.DataType == typeof(bool))
						{
							fr.Condition = PXCondition.NE;
							fr.OrigValue = fr.Value = true;
						}
					}
					if (fr.Value == null
						&& (fr.Condition == PXCondition.EQ
						|| fr.Condition == PXCondition.LIKE
						|| fr.Condition == PXCondition.LLIKE
						|| fr.Condition == PXCondition.RLIKE))
					{
						fr.Condition = PXCondition.ISNULL;
					}
					if (fr.Value == null
						&& (fr.Condition == PXCondition.NE
						|| fr.Condition == PXCondition.NOTLIKE))
					{
						fr.Condition = PXCondition.ISNOTNULL;
					}
					PXCommandPreparingEventArgs.FieldDescription descr;
					switch (fr.Condition)
					{
						case PXCondition.IN:
						case PXCondition.NI:
							if (fr.Value is Type)
							{
								fr.Value = ((Type)fr.Value).FullName;
							}
							if (cache != Cache && fieldName == fr.DataField)
							{
								cache = Cache;
								fr.Value = null;
								fr.Value2 = null;
							}
							else if (fr.DataField != null && fr.Value is string)
							{
								Type field = System.Web.Compilation.BuildManager.GetType((string)fr.Value, false);
								if (field != null && typeof(IBqlField).IsAssignableFrom(field)
									&& field.IsNested && typeof(IBqlTable).IsAssignableFrom(BqlCommand.GetItemType(field)))
								{
									fr.Value = field;
									cache = _Graph.Caches[BqlCommand.GetItemType(field)];
								}
								else
								{
									fr.Value = null;
									fr.Value2 = null;
								}
								brackets = fr.OpenBrackets - fr.CloseBrackets;
							}
							else
							{
								cache = Cache;
								fr.Value = null;
								fr.Value2 = null;
								brackets = fr.OpenBrackets - fr.CloseBrackets;
							}
							break;
						case PXCondition.ER:
							fr.DataField = typeof(Note.noteID).Name;
							PXCache noteCache = (new PXGraph()).Caches[typeof(Note)];
							noteCache.RaiseCommandPreparing(typeof(Note.externalKey).Name, null, fr.Value, PXDBOperation.Select, typeof(Note), out descr);
							if (descr != null)
							{
								fr.Description = descr;
							}
							break;
						case PXCondition.EQ:
						case PXCondition.NE:
							if (fr.Value != null)
							{
								bool updatingFailed = false;
								try
								{
									object val = fr.Value;
									cache.RaiseFieldUpdating(fieldName, null, ref val);
									fr.Value = val;
								}
								catch (Exception esp)
								{
									updatingFailed = true;
									fr.UseExt = true;
									if (esp is PXSetPropertyException && ((PXSetPropertyException)esp).ErrorValue is string && fr.Value is string)
									{
										fr.Value = ((PXSetPropertyException)esp).ErrorValue;
									}
								}
								if (fr.Value != null)
								{
									try
									{
										descr = null;
										if (!updatingFailed)
										{
											cache.RaiseCommandPreparing(fieldName, null, fr.Value, PXDBOperation.Select, cache.GetItemType(), out descr);
										}
										if (descr != null && !String.IsNullOrEmpty(descr.FieldName) && descr.FieldName != BqlCommand.Null)
										{
											fr.Description = descr;
										}
										else
										{
											cache.RaiseCommandPreparing(fieldName, null, fr.Value, PXDBOperation.Select | PXDBOperation.External, cache.GetItemType(), out descr);
											if (descr != null && !String.IsNullOrEmpty(descr.FieldName) && descr.FieldName != BqlCommand.Null
												&& (!updatingFailed || descr.FieldName.StartsWith(BqlCommand.SubSelect)))
											{
												fr.Description = descr;
												fr.UseExt = true;
											}
											else
											{
												anyFailed = true;
												fr.UseExt = fr.UseExt || (fieldName).IndexOf("_") != -1;
											}
										}
									}
									catch
									{
										anyFailed = true;
									}
								}
							}
							break;
						case PXCondition.GT:
						case PXCondition.GE:
						case PXCondition.LT:
						case PXCondition.LE:
							if (fr.Value != null)
							{
								object val = fr.Value;
								try
								{
									try
									{
										cache.RaiseFieldUpdating(fieldName, null, ref val);
									}
									catch
									{
										fr.Value = val;
										throw;
									}
									if (val != null)
									{
										PXFieldState state = cache.GetStateExt(null, fieldName) as PXFieldState;
										if (state != null && state.DataType == val.GetType())
										{
											fr.Value = val;
										}
										else
										{
											fr.UseExt = true;
											if (state != null)
											{
												cache.RaiseFieldSelecting(fieldName, null, ref val, false);
												if (val is PXFieldState)
												{
													val = ((PXFieldState)val).Value;
												}
												if (val != null && val.GetType() == state.DataType)
												{
													fr.Value = val;
												}
											}

										}
									}
									else
									{
										fr.UseExt = true;
									}
								}
								catch
								{
									fr.UseExt = true;
								}
								try
								{
									cache.RaiseCommandPreparing(fieldName, null, fr.Value, PXDBOperation.Select | PXDBOperation.External, cache.GetItemType(), out descr);
									if (descr != null && !String.IsNullOrEmpty(descr.FieldName) && descr.FieldName != BqlCommand.Null)
									{
										fr.Description = descr;
										if (!fr.UseExt && descr.FieldName.StartsWith(BqlCommand.SubSelect))
										{
											fr.UseExt = true;
										}
									}
									else
									{
										fr.UseExt = true;
										anyFailed = true;
									}
								}
								catch
								{
									anyFailed = true;
								}
							}
							break;
						case PXCondition.LIKE:
						case PXCondition.NOTLIKE:
						case PXCondition.LLIKE:
						case PXCondition.RLIKE:
							string s = fr.Value as string;
							if (s != null)
							{
								try
								{
									object val;
									PXStringState state = cache.GetStateExt(null, fieldName) as PXStringState;
									if (state != null && !String.IsNullOrEmpty(state.InputMask) && PX.Common.Mask.IsMasked(s, state.InputMask, true))
									{
										val = PX.Common.Mask.Parse(state.InputMask, s);
									}
									else
									{
										val = s;
									}
									cache.RaiseFieldUpdating(fieldName, null, ref val);
									if (val is string)
									{
										if (!(state != null && state.Length > 0 && s.Length > state.Length && ((string)val).Length < s.Length && s.StartsWith((string)val)))
										{
											s = (string)val;
										}
									}
									else
									{
										fr.UseExt = true;
									}
								}
								catch
								{
									fr.UseExt = true;
								}
								s = s.TrimEnd();
								fr.Value = s;
								if (fr.Condition == PXCondition.RLIKE)
								{
									s = s.Replace("[", "[[]") + "%";
								}
								else if (fr.Condition == PXCondition.LLIKE)
								{
									s = "%" + s.Replace("[", "[[]");
								}
								else
								{
									s = "%" + s.Replace("[", "[[]") + "%";
								}
								try
								{
									cache.RaiseCommandPreparing(fieldName, null, s, PXDBOperation.Select | PXDBOperation.External, cache.GetItemType(), out descr);
									if (descr != null && !String.IsNullOrEmpty(descr.FieldName) && descr.FieldName != BqlCommand.Null)
									{
										if (descr.DataLength != null)
										{
											if (fr.Condition == PXCondition.LIKE || fr.Condition == PXCondition.NOTLIKE)
											{
												fr.Description = new PXCommandPreparingEventArgs.FieldDescription(descr.BqlTable, descr.FieldName, descr.DataType, ((int)descr.DataLength) + 2, descr.DataValue, descr.IsRestriction);
											}
											else
											{
												fr.Description = new PXCommandPreparingEventArgs.FieldDescription(descr.BqlTable, descr.FieldName, descr.DataType, ((int)descr.DataLength) + 1, descr.DataValue, descr.IsRestriction);
											}
										}
										else
										{
											fr.Description = descr;
										}
										if (!fr.UseExt && descr.FieldName.StartsWith(BqlCommand.SubSelect))
										{
											fr.UseExt = true;
										}
									}
									else
									{
										anyFailed = true;
										fr.UseExt = fr.UseExt || (fieldName).IndexOf("_") != -1;
									}
								}
								catch
								{
									fr.UseExt = true;
									anyFailed = true;
								}
							}
							else
							{
								fr.Value = null;
							}
							break;
						case PXCondition.ISNULL:
						case PXCondition.ISNOTNULL:
						case PXCondition.TODAY_OVERDUE:											
						case PXCondition.NEXT_WEEK:
						case PXCondition.NEXT_MONTH:
							try
							{
								cache.RaiseCommandPreparing(fieldName, null, null, PXDBOperation.Select, cache.GetItemType(), out descr);
								if (descr != null && !String.IsNullOrEmpty(descr.FieldName) && descr.FieldName != BqlCommand.Null)
								{
									fr.Description = descr;
									if (!fr.UseExt && descr.FieldName.StartsWith(BqlCommand.SubSelect))
									{
										fr.UseExt = true;
									}
								}
								else
								{
									anyFailed = true;
									fr.UseExt = (fieldName).IndexOf("_") != -1;
								}
							}
							catch
							{
								anyFailed = true;
							}
							break;
						case PXCondition.TODAY:
						case PXCondition.OVERDUE:
						case PXCondition.TOMMOROW:	
						case PXCondition.THIS_MONTH:
						case PXCondition.THIS_WEEK:
						case PXCondition.BETWEEN:
							if (fr.Condition != PXCondition.BETWEEN)
							{
								fr.Value = DateTime.MinValue;
								fr.Value2 = DateTime.MinValue;
								object value = PXView.DateTimeFactory.GetDateRage(fr.Condition, Graph.Accessinfo.BusinessDate);
								if ((value as DateTime?[])[0] != null)
									fr.Value = (value as DateTime?[])[0].Value;
								if ((value as DateTime?[])[1] != null)
									fr.Value2 = (value as DateTime?[])[1].Value;
							}
							if (fr.Value != null && fr.Value2 != null)
							{
								object val = fr.Value;
								object val2 = fr.Value2;
								try
								{
									Exception ex1 = null;
									try
									{
										cache.RaiseFieldUpdating(fieldName, null, ref val);
									}
									catch (Exception ex)
									{
										fr.Value = val;
										ex1 = ex;
									}

									try
									{
										cache.RaiseFieldUpdating(fieldName, null, ref val2);
									}
									catch
									{
										fr.Value2 = val2;
										throw;
									}
									if (ex1 != null) throw ex1;

									if (val != null && val2 != null)
									{
										PXFieldState state = cache.GetStateExt(null, fieldName) as PXFieldState;
										if (state != null && state.DataType == val.GetType() && state.DataType == val2.GetType())
										{
											fr.Value = val;
											fr.Value2 = val2;
										}
										else
										{
											fr.UseExt = true;
											if (state != null)
											{
												cache.RaiseFieldSelecting(fieldName, null, ref val, false);
												if (val is PXFieldState)
												{
													val = ((PXFieldState)val).Value;
												}
												if (val != null && val.GetType() == state.DataType)
												{
													fr.Value = val;
												}
												cache.RaiseFieldSelecting(fieldName, null, ref val2, false);
												if (val2 is PXFieldState)
												{
													val2 = ((PXFieldState)val2).Value;
												}
												if (val2 != null && val2.GetType() == state.DataType)
												{
													fr.Value2 = val2;
												}
											}
										}
									}
									else
									{
										fr.UseExt = true;
									}
								}
								catch
								{
									fr.UseExt = true;
								}
								try
								{
									PXCommandPreparingEventArgs.FieldDescription descr2;
									cache.RaiseCommandPreparing(fieldName, null, fr.Value2, PXDBOperation.Select | PXDBOperation.External, cache.GetItemType(), out descr2);
									if (descr2 != null && !String.IsNullOrEmpty(descr2.FieldName))
									{
										cache.RaiseCommandPreparing(fieldName, null, fr.Value, PXDBOperation.Select | PXDBOperation.External, cache.GetItemType(), out descr);
										if (descr != null && !String.IsNullOrEmpty(descr.FieldName) && descr.FieldName != BqlCommand.Null)
										{
											fr.Description = descr;
											fr.Description2 = descr2;
										}
										else
										{
											anyFailed = true;
											fr.UseExt = fr.UseExt || (fieldName).IndexOf("_") != -1;
										}
									}
									else
									{
										fr.UseExt = true;
										anyFailed = true;
									}
								}
								catch
								{
									anyFailed = true;
								}
							}
							break;
					}
					if (fr.Description != null)
					{
						cache.RaiseCommandPreparing(fieldName, null, null, PXDBOperation.Select | PXDBOperation.External, cache.GetItemType(), out descr);
						if (descr == null || String.IsNullOrEmpty(descr.FieldName))
						{
							fr.Description = null;
							fr.Description2 = null;
							if (!fr.UseExt)
							{
								object val = fr.Value;
								if (val != null)
								{
									PXFieldState state = cache.GetStateExt(null, fieldName) as PXFieldState;
									if (state != null && state.DataType != val.GetType())
									{
										cache.RaiseFieldSelecting(fieldName, null, ref val, false);
										if (val is PXFieldState)
										{
											val = ((PXFieldState)val).Value;
										}
										if (val != null && val.GetType() == state.DataType)
										{
											fr.Value = val;
										}
									}
								}
								val = fr.Value2;
								if (val != null)
								{
									PXFieldState state = cache.GetStateExt(null, fieldName) as PXFieldState;
									if (state != null && state.DataType != val.GetType())
									{
										cache.RaiseFieldSelecting(fieldName, null, ref val, false);
										if (val is PXFieldState)
										{
											val = ((PXFieldState)val).Value;
										}
										if (val != null && val.GetType() == state.DataType)
										{
											fr.Value2 = val;
										}
									}
								}
							}
							fr.UseExt = true;
							anyFailed = true;
						}
					}
				}
			}
			return anyFailed;
		}

		private BqlCommand appendFilters(PXFilterRow[] filters, List<PXDataValue> pars, BqlCommand cmd)
		{
			if (filters != null)
			{
				int brackets = 0;
				foreach (PXFilterRow row in filters)
				{
					brackets += row.OpenBrackets;
					brackets -= row.CloseBrackets;
				}
				if (brackets > 0)
				{
					filters[filters.Length - 1].CloseBrackets += brackets;
				}
				else if (brackets < 0)
				{
					filters[0].OpenBrackets -= brackets;
				}
				List<Type> fields = Cache.BqlFields;
				Type[] expressions = new Type[filters.Length];
				int exprCnt = 0;
				brackets = 0;
				PXCache subcache = null;
				for (int i = 0; i < filters.Length; i++)
				{
					PXFilterRow fr = filters[i];
					Type field = null;
					if (brackets > 0)
					{
						field = subcache.GetBqlField(fr.DataField);
						brackets += fr.OpenBrackets;
						brackets -= fr.CloseBrackets;
					}
					else
					{
						for (int j = fields.Count - 1; j >= 0; j--)
						{
							Type bf = fields[j];
							if (String.Compare(bf.Name, fr.DataField, StringComparison.OrdinalIgnoreCase) == 0)
							{
								field = bf;
								break;
							}
						}
					}
					Type required = null;
					if (field != null && !fr.UseExt)
					{
						required = typeof(Required<>).MakeGenericType(field);
					}
					else if (fr.Description != null && !String.IsNullOrEmpty(fr.Description.FieldName))
					{
						field = typeof(FieldNameParam);
						required = typeof(Argument<object>);
					}
					if (field != null)
					{
						int parspos = pars.Count;
						switch (fr.Condition)
						{
							case PXCondition.IN:
							case PXCondition.NI:
								if (fr.Value is Type)
								{
									expressions[i] = typeof(Where<,>).MakeGenericType(field, typeof(Equal<>).MakeGenericType((Type)fr.Value));
									subcache = _Graph.Caches[BqlCommand.GetItemType((Type)fr.Value)];
									brackets = fr.OpenBrackets - fr.CloseBrackets;
								}
								break;
							case PXCondition.ER:
								if (string.Compare(field.Name, "noteID", StringComparison.InvariantCultureIgnoreCase) == 0 &&
									!string.IsNullOrEmpty(fr.Value as string))
								{
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength, fr.Description.DataValue));
									expressions[i] = typeof(Where<,,>).MakeGenericType(
										field,
										typeof(Equal<>).MakeGenericType(
											typeof(Note.noteID)
										),
										typeof(And<,>).MakeGenericType(
											typeof(Note.externalKey),
											typeof(Like<>).MakeGenericType(
												typeof(Required<>).MakeGenericType(
													typeof(Note.externalKey)
												)
											)
										)
									);
								}
								break;
							case PXCondition.EQ:
								if (fr.Description != null)
								{
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength, fr.Description.DataValue));
									expressions[i] = typeof(Where<,>).MakeGenericType(field, typeof(Equal<>).MakeGenericType(required));
								}
								break;
							case PXCondition.NE:
								if (fr.Description != null)
								{
									if (fr.OrigValue is bool && ((bool)fr.OrigValue))
									{
										expressions[i] = typeof(Where<,,>).MakeGenericType(field, typeof(NotEqual<>).MakeGenericType(required), typeof(Or<,>).MakeGenericType(field, typeof(IsNull)));
									}
									else
									{
										expressions[i] = typeof(Where<,>).MakeGenericType(field, typeof(NotEqual<>).MakeGenericType(required));
									}
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength, fr.Description.DataValue));
								}
								break;
							case PXCondition.GT:
								if (fr.Description != null)
								{
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength, fr.Description.DataValue));
									expressions[i] = typeof(Where<,>).MakeGenericType(field, typeof(Greater<>).MakeGenericType(required));
								}
								break;
							case PXCondition.GE:
								if (fr.Description != null)
								{
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength, fr.Description.DataValue));
									expressions[i] = typeof(Where<,>).MakeGenericType(field, typeof(GreaterEqual<>).MakeGenericType(required));
								}
								break;
							case PXCondition.LT:
								if (fr.Description != null)
								{
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength, fr.Description.DataValue));
									expressions[i] = typeof(Where<,>).MakeGenericType(field, typeof(Less<>).MakeGenericType(required));
								}
								break;
							case PXCondition.LE:
								if (fr.Description != null)
								{
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength, fr.Description.DataValue));
									expressions[i] = typeof(Where<,>).MakeGenericType(field, typeof(LessEqual<>).MakeGenericType(required));
								}
								break;
							case PXCondition.LIKE:
							case PXCondition.LLIKE:
							case PXCondition.RLIKE:
								if (fr.Description != null)
								{
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength <= ((string)fr.Description.DataValue).Length ? fr.Description.DataLength : ((string)fr.Description.DataValue).Length, fr.Description.DataValue));
									expressions[i] = typeof(Where<,>).MakeGenericType(field, typeof(Like<>).MakeGenericType(required));
								}
								break;
							case PXCondition.NOTLIKE:
								if (fr.Description != null)
								{
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength <= ((string)fr.Description.DataValue).Length ? fr.Description.DataLength : ((string)fr.Description.DataValue).Length, fr.Description.DataValue));
									expressions[i] = typeof(Where<,>).MakeGenericType(field, typeof(NotLike<>).MakeGenericType(required));
								}
								break;
							case PXCondition.ISNULL:
								if (fr.Description != null)
								{
									expressions[i] = typeof(Where<,>).MakeGenericType(field, typeof(IsNull));
								}
								break;
							case PXCondition.ISNOTNULL:
								if (fr.Description != null)
								{
									expressions[i] = typeof(Where<,>).MakeGenericType(field, typeof(IsNotNull));
								}
								break;
							case PXCondition.OVERDUE:
							case PXCondition.TOMMOROW:
							case PXCondition.THIS_WEEK:
							case PXCondition.TODAY:
							case PXCondition.THIS_MONTH:
							case PXCondition.BETWEEN:
								if (fr.Description != null && fr.Description2 != null)
								{
									pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength, fr.Description.DataValue));
									pars.Add(new PXDataValue(fr.Description2.DataType, fr.Description2.DataLength, fr.Description2.DataValue));
									expressions[i] = typeof(Where<,>).MakeGenericType(field, typeof(Between<,>).MakeGenericType(required, required));
								}
								break;
							case PXCondition.TODAY_OVERDUE:														
							case PXCondition.NEXT_WEEK:
							case PXCondition.NEXT_MONTH:
								if (fr.Description != null)
								{
									DateTime?[] range = DateTimeFactory.GetDateRage(fr.Condition, null);
									var startDate = range[0];
									var endDate = range[1];
									Type condition = null;
									if (startDate != null)
									{
										pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength, startDate));
										condition = typeof(GreaterEqual<>).MakeGenericType(required);
									}
									if (endDate != null)
									{
										pars.Add(new PXDataValue(fr.Description.DataType, fr.Description.DataLength, endDate));
										condition = typeof(LessEqual<>).MakeGenericType(required);
									}
									if (startDate != null && endDate != null)
										condition = typeof(Between<,>).MakeGenericType(required, required);
									if (condition != null)
										expressions[i] = typeof(Where<,>).MakeGenericType(field, condition);
								}
								break;
						}
						if (field == typeof(FieldNameParam) && expressions[i] != null)
						{
							string fieldName = fr.Description.FieldName;
							int idxdot, idxscore;
							if ((idxdot = fieldName.IndexOf('.')) != -1 && fieldName.IndexOf('(') == -1 && (idxscore = filters[i].DataField.IndexOf("__")) != -1)
							{
								fieldName = filters[i].DataField.Substring(0, idxscore) + fieldName.Substring(idxdot);
							}
							pars.Insert(parspos, new PXFieldName(fieldName));
							if (fr.Condition == PXCondition.NE && fr.OrigValue is bool && ((bool)fr.OrigValue))
							{
								pars.Add(new PXFieldName(fieldName));
							}
						}
					}
					if (expressions[i] == null /*&& (fr.OpenBrackets > 0 || fr.CloseBrackets > 0)*/)
					{
						expressions[i] = typeof(Where<True, Equal<True>>);
					}
					if (expressions[i] != null)
					{
						exprCnt++;
					}
				}
				Type filter = null;
				if (exprCnt > 0)
				{
					List<Type> list = new List<Type>();
					List<int> levels = new List<int>();
					levels.Add(list.Count);
					list.Add(typeof(Where<>));
					for (int j = 0; j < filters[0].OpenBrackets - filters[0].CloseBrackets; j++)
					{
						levels.Add(list.Count);
						list.Add(typeof(Where<>));
					}
					for (int i = 0; i < filters.Length; i++)
					{
						if (expressions[i] != null)
						{
							if (i > 0)
							{
								Type clause = list[levels[levels.Count - 1]];
								if (typeof(Where<>).IsAssignableFrom(clause))
								{
									list[levels[levels.Count - 1]] = typeof(Where2<,>);
								}
								else if (typeof(And<>).IsAssignableFrom(clause))
								{
									list[levels[levels.Count - 1]] = typeof(And2<,>);
								}
								else if (typeof(Or<>).IsAssignableFrom(clause))
								{
									list[levels[levels.Count - 1]] = typeof(Or2<,>);
								}
								levels[levels.Count - 1] = list.Count;
								if (filters[i - 1].OrOperator)
								{
									list.Add(typeof(Or<>));
								}
								else
								{
									list.Add(typeof(And<>));
								}
								for (int j = 0; j < filters[i].OpenBrackets - filters[i].CloseBrackets; j++)
								{
									levels.Add(list.Count);
									list.Add(typeof(Where<>));
								}
								for (int j = 0; j < filters[i].CloseBrackets - filters[i].OpenBrackets; j++)
								{
									levels.RemoveAt(levels.Count - 1);
								}
							}
							if (filters[i].Condition == PXCondition.IN)
							{
								list.Add(typeof(Exists<>));
								list.Add(typeof(Select<,>));
								list.Add(BqlCommand.GetItemType((Type)filters[i].Value));
							}
							else if (filters[i].Condition == PXCondition.NI)
							{
								list.Add(typeof(NotExists<>));
								list.Add(typeof(Select<,>));
								list.Add(BqlCommand.GetItemType((Type)filters[i].Value));
							}
							else if (filters[i].Condition == PXCondition.ER)
							{
								list.Add(typeof(Exists<>));
								list.Add(typeof(Select<,>));
								list.Add(typeof(Note));
							}
							list.Add(expressions[i]);
						}
					}
					filter = BqlCommand.Compose(list.ToArray());
				}
				if (filter != null)
				{
					cmd = cmd.WhereAnd(filter);
				}
			}
			return cmd;
		}

		internal static class DateTimeFactory
		{
			public static double GetDayOfWeek(DayOfWeek day)
			{
				switch (day)
				{
					case DayOfWeek.Monday: return 0D;
					case DayOfWeek.Tuesday: return 1D;
					case DayOfWeek.Wednesday: return 2D;
					case DayOfWeek.Thursday: return 3D;
					case DayOfWeek.Friday: return 4D;
					case DayOfWeek.Saturday: return 5D;
					case DayOfWeek.Sunday: return 6D;
					default: return 0D;
				}
			}

			public static DateTime?[] GetDateRage(PXCondition condition, DateTime? businessDate)
			{
				businessDate = businessDate ?? DateTime.Today;
				DateTime? startDate;
				DateTime? endDate;
				switch (condition)
				{
					case PXCondition.TODAY:
						startDate = businessDate;
						endDate = startDate.Value.AddDays(1D);
						break;
					case PXCondition.OVERDUE:
						startDate = null;
						endDate = businessDate;
						break;
					case PXCondition.TODAY_OVERDUE:
						startDate = null;
						endDate = ((DateTime)businessDate).AddDays(1D);
						break;
					case PXCondition.TOMMOROW:
						startDate = ((DateTime)businessDate).AddDays(1D);
						endDate = startDate.Value.AddDays(1D);
						break;
					case PXCondition.THIS_WEEK:
						startDate = businessDate;
						startDate = startDate.Value.AddDays(-GetDayOfWeek(startDate.Value.DayOfWeek));
						endDate = startDate.Value.AddDays(7D);
						break;
					case PXCondition.NEXT_WEEK:
						startDate = businessDate;
						startDate = startDate.Value.AddDays(7D - GetDayOfWeek(startDate.Value.DayOfWeek));
						endDate = startDate.Value.AddDays(7D);
						break;
					case PXCondition.THIS_MONTH:
						startDate = businessDate;
						startDate = new DateTime(startDate.Value.Year, startDate.Value.Month, 1);
						endDate = startDate.Value.AddMonths(1);
						break;
					case PXCondition.NEXT_MONTH:
						startDate = businessDate;
						startDate = new DateTime(startDate.Value.Year, startDate.Value.Month, 1);
						startDate = startDate.Value.AddMonths(1);
						endDate = startDate.Value.AddMonths(1);
						break;
					default: return null;
				}
				if (endDate != null) endDate = endDate.Value.AddSeconds(-1D);
				return new[] { startDate, endDate };
			}
		}

		private BqlCommand appendSearches(PXSearchColumn[] sorts, List<PXDataValue> pars, BqlCommand cmd, ref int topCount, bool reverseOrder)
		{
			if (sorts != null && topCount > 0)
			{
				IBqlSortColumn[] sort = cmd.GetSortColumns();

				Type newsearch = null;
				int lastSearch = pars.Count;
				for (int i = sorts.Length - 1; i >= 0; i--)
				{
					if (i >= sort.Length)
					{
						topCount = 0;
						continue;
					}
					if (sorts[i].SearchValue != null && sorts[i].Description != null)
					{
						PXFieldName name = null;
						Type b = null;
						if (sort[i].GetReferencedType() == null)
						{
							for (int j = Cache.BqlFields.Count - 1; j >= 0; j--)
							{
								if (String.Compare(Cache.BqlFields[j].Name, sorts[i].Column, StringComparison.OrdinalIgnoreCase) == 0)
								{
									b = Cache.BqlFields[j];
									break;
								}
							}
							PXCommandPreparingEventArgs.FieldDescription descr = null;
							if (b != null && b.IsNested && CacheType != Cache.GetItemType()
								&& (BqlCommand.GetItemType(b) == CacheType || CacheType.IsSubclassOf(BqlCommand.GetItemType(b)))
								&& sorts[i].Description.FieldName != BqlCommand.Null)
							{
								Cache.RaiseCommandPreparing(sorts[i].Column, null, sorts[i].SearchValue, PXDBOperation.Select | PXDBOperation.External, CacheType, out descr);
							}
							if (descr == null || String.IsNullOrEmpty(descr.FieldName))
							{
								descr = sorts[i].Description;
							}
							string fieldName = descr.FieldName;
							int idxdot, idxscore;
							if ((idxdot = fieldName.IndexOf('.')) != -1 && fieldName.IndexOf('(') == -1 && (idxscore = sorts[i].Column.IndexOf("__")) != -1)
							{
								fieldName = sorts[i].Column.Substring(0, idxscore) + fieldName.Substring(idxdot);
							}
							name = new PXFieldName(fieldName);
						}
						PXDataValue val = new PXDataValue(sorts[i].Description.DataType, sorts[i].Description.DataLength, sorts[i].Description.DataValue);
						if (lastSearch != pars.Count && (topCount != 1 || reverseOrder))
						{
							pars.Insert(lastSearch, val);
							if (name != null)
							{
								pars.Insert(lastSearch, name);
							}
						}
						pars.Insert(lastSearch, val);
						if (name != null)
						{
							pars.Insert(lastSearch, name);
						}

						Type ft = sort[i].GetReferencedType();
						Type fb = null;
						if (ft == null)
						{
							ft = typeof(FieldNameParam);
							fb = typeof(Required<>).MakeGenericType(b ?? typeof(FieldNameParam.PXRequiredField));
						}
						else
						{
							fb = typeof(Required<>).MakeGenericType(ft);
						}
						if (fb != null)
						{
							if (newsearch == null)
							{
								if (topCount == 1 && !reverseOrder)
								{
									newsearch = typeof(Where<,>).MakeGenericType(ft, typeof(Equal<>).MakeGenericType(fb));
								}
								else if (sort[i].IsDescending)
								{
									if (reverseOrder)
									{
										newsearch = typeof(Where<,>).MakeGenericType(ft, typeof(Less<>).MakeGenericType(fb));
									}
									else
									{
										newsearch = typeof(Where<,>).MakeGenericType(ft, typeof(LessEqual<>).MakeGenericType(fb));
									}
								}
								else
								{
									if (reverseOrder)
									{
										newsearch = typeof(Where<,>).MakeGenericType(ft, typeof(Greater<>).MakeGenericType(fb));
									}
									else
									{
										newsearch = typeof(Where<,>).MakeGenericType(ft, typeof(GreaterEqual<>).MakeGenericType(fb));
									}
								}
							}
							else
							{
								if (topCount == 1 && !reverseOrder)
								{
									newsearch = typeof(Where2<,>).MakeGenericType(typeof(Where<,>).MakeGenericType(ft, typeof(Equal<>).MakeGenericType(fb)), typeof(And<>).MakeGenericType(newsearch));
								}
								else if (sort[i].IsDescending)
								{
									newsearch = typeof(Where2<,>).MakeGenericType(typeof(Where<,>).MakeGenericType(ft, typeof(Less<>).MakeGenericType(fb)), typeof(Or2<,>).MakeGenericType(typeof(Where<,>).MakeGenericType(ft, typeof(Equal<>).MakeGenericType(fb)), typeof(And<>).MakeGenericType(newsearch)));
								}
								else
								{
									newsearch = typeof(Where2<,>).MakeGenericType(typeof(Where<,>).MakeGenericType(ft, typeof(Greater<>).MakeGenericType(fb)), typeof(Or2<,>).MakeGenericType(typeof(Where<,>).MakeGenericType(ft, typeof(Equal<>).MakeGenericType(fb)), typeof(And<>).MakeGenericType(newsearch)));
								}
							}
						}
					}
				}
				if (newsearch != null)
				{
					cmd = cmd.WhereAnd(newsearch);
				}
			}
			return cmd;
		}

		/// <summary>
		/// Retrieves resultset out of the database
		/// </summary>
		/// <param name="parameters">Parameters for the command</param>
		/// <param name="searches">Search values</param>
		/// <param name="reverseOrder">If reversing of the sort expression is required</param>
		/// <param name="topCount">Number of rows required</param>
		/// <param name="sortcolumns">Sort columns</param>
		/// <param name="descendings">Descending flags</param>
		/// <param name="needOverrideSort">If the Bql command needs to be updated with the new sort expression</param>
		/// <returns>Resultset, if there is no empty parameters, otherwise empty list</returns>
		protected virtual List<object> GetResult(
			object[] parameters,
			PXFilterRow[] filters,
			bool reverseOrder,
			int topCount,
			PXSearchColumn[] sorts,
			ref bool overrideSort,
			ref bool extFilter
			)
		{
			List<object> ret = new VersionedList();
			if (!Cache.AllowSelect)
			{
				return ret;
			}
			IBqlParameter[] cmdpars = _Select.GetParameters();
			if (parameters == null && cmdpars.Length > 0)
			{
				return ret;
			}
			if (parameters != null)
			{
				if (cmdpars.Length > parameters.Length)
				{
					return ret;
				}
				for (int i = 0; i < parameters.Length && i < cmdpars.Length; i++)
				{
					if (!cmdpars[i].NullAllowed && parameters[i] == null)
					{
						return ret;
					}
				}
			}

			BqlCommand cmd = _Select;

			List<PXDataValue> sortnames = new List<PXDataValue>();
			if (overrideSort)
			{
				Type newsort = ConstructSort(sorts, sortnames, reverseOrder);
				if (newsort != null)
				{
					cmd = cmd.OrderByNew(newsort);
				}
				overrideSort = false;
			}

			List<PXDataValue> pars = new List<PXDataValue>();

			if (parameters != null)
			{
				int argNbr = 0;
				for (int i = 0; i < cmdpars.Length && i < parameters.Length; i++)
				{
					PXCommandPreparingEventArgs.FieldDescription descr = null;
					Type cross = null;
					if (!cmdpars[i].IsArgument)
					{
						Type field = cmdpars[i].GetReferencedType();
						if (!field.IsNested || BqlCommand.GetItemType(field) == CacheType)
						{
							Cache.RaiseCommandPreparing(field.Name, null, parameters[i], PXDBOperation.Select | (_IsReadOnly ? PXDBOperation.ReadOnly : PXDBOperation.Normal), null, out descr);
							if (cmdpars[i].MaskedType != null)
							{
								cross = GroupHelper.GetReferencedType(Cache, field.Name);
							}
						}
						else
						{
							PXCache cache = _Graph.Caches[BqlCommand.GetItemType(field)];
							cache.RaiseCommandPreparing(field.Name, null, parameters[i], PXDBOperation.Select | (_IsReadOnly ? PXDBOperation.ReadOnly : PXDBOperation.Normal), null, out descr);
							if (cmdpars[i].MaskedType != null)
							{
								cross = GroupHelper.GetReferencedType(cache, field.Name);
							}
						}
					}
					else if (_Delegate != null)
					{
						foreach (PXEventSubscriberAttribute attr in _Delegate.Method.GetParameters()[argNbr].GetCustomAttributes(typeof(PXEventSubscriberAttribute), false))
						{
							List<IPXCommandPreparingSubscriber> del = new List<IPXCommandPreparingSubscriber>();
							attr.GetSubscriber<IPXCommandPreparingSubscriber>(del);
							if (del.Count > 0)
							{
								PXCommandPreparingEventArgs preps = new PXCommandPreparingEventArgs(null, parameters[i], PXDBOperation.Select, null);
								for (int l = 0; l < del.Count; l++)
								{
									del[l].CommandPreparing(Cache, preps);
								}
								descr = preps.GetFieldDescription();
								argNbr++;
								break;
							}
						}
					}
					if (descr == null || descr.DataValue == null && !cmdpars[i].NullAllowed)
					{
						return ret;
					}
					if (cmdpars[i].MaskedType == null)
					{
						pars.Add(new PXDataValue(descr.DataType, descr.DataLength, descr.DataValue));
					}
					else if (_Graph.Caches[cmdpars[i].MaskedType].Fields.Contains(GroupHelper.FieldName))
					{
						byte[] mask = descr.DataValue as byte[];
						foreach (GroupHelper.ParamsPair pair in GroupHelper.GetParams(cross, cmdpars[i].MaskedType, mask))
						{
							pars.Add(new PXDataValue(PXDbType.Int, 4, pair.First));
							pars.Add(new PXDataValue(PXDbType.Int, 4, pair.Second));
						}
					}
				}
			}

			cmd = appendSearches(sorts, pars, cmd, ref topCount, reverseOrder);

			cmd = appendFilters(filters, pars, cmd);

			pars.AddRange(sortnames);

			Type[] tables = cmd.GetTables();
			bool hascount = !typeof(IBqlTable).IsAssignableFrom(tables[tables.Length - 1]);
			PXCache[] caches = new PXCache[hascount ? tables.Length - 1 : tables.Length];
			caches[0] = Cache;
			for (int i = 1; i < caches.Length; i++)
			{
				caches[i] = _Graph.Caches[tables[i]];
			}
			EnsureCreateInstance(tables);
			var result = _Graph.ProviderSelect(cmd, topCount, this.RestrictedFields.Any() ? this : null, pars.ToArray());
			PXDataRecordMap map = null;
			if (cmd.RecordMapEntries.Any())
			{
				 map = new PXDataRecordMap(cmd.RecordMapEntries);
			}
			foreach (PXDataRecord r in result)
			{
				var rec = r;
				if (map != null)
				{
					map.SetRow(r);
					rec = map;
				}
				int position = 0;
				object[] items = new object[caches.Length];
				for (int i = 0; i < caches.Length; i++)
				{
					bool wasUpdated;
					items[i] = caches[i].Select(rec, ref position, _IsReadOnly || i > 0, out wasUpdated);
					if (wasUpdated)
					{
						overrideSort = true;
						extFilter = true;
					}
				}
				if (items[0] != null)
				{
					if (_CreateInstance == null)
					{
						ret.Add(items[0]);
					}
					else
					{
						PXResult res = (PXResult)_CreateInstance(items);
						if (hascount)
						{
							res.RowCount = rec.GetInt32(position);
						}
						ret.Add(res);
					}
				}
			}
			return ret;
		}

		internal virtual void StoreCached(PXCommandKey queryKey, List<object> records)
		{
			List<object> items = (this.IsReadOnly) ? records : new VersionedList(records, Cache.Version);
			SelectQueries.StoreCached(this, queryKey, items);

		}



		/// <summary>
		/// Looks for a resultset inside the internal cache, merges updated and deleted items, adjusts top count
		/// </summary>
		/// <param name="queryKey">Key to search</param>
		/// <param name="topCount">Number of rows required</param>
		/// <returns>Resultset if found in the cache, otherwise null</returns>
		protected virtual List<object> LookupCache(PXCommandKey queryKey, ref int topCount)
		{
			List<object> list = null;
			if (_IsCommandMutable)
			{
				SelectQueries.IsCommandMutable = _IsCommandMutable;
			}
			if (SelectQueries.IsCommandMutable && queryKey.CommandText == null)
			{
				queryKey.CommandText = this.ToString();
			}
			if (SelectQueries.ContainsKey(queryKey))
			{
				list = SelectQueries[queryKey].Items;
				if (!_IsReadOnly)
				{
					foreach (object item in Cache.Deleted)
					{
						int index = list.IndexOf(item);
						if (index >= 0)
						{
							list.RemoveAt(index);
						}
					}
				}
			}
			else
			{
				if (!_IsReadOnly && topCount > 0)
				{
					foreach (object item in Cache.Deleted)
					{
						topCount++;
					}
					foreach (object item in Cache.Updated)
					{
						topCount++;
					}
				}
			}
			return list;
		}

		protected PXView _TailSelect;
		/// <summary>
		/// Selects rows joined with the item
		/// </summary>
		/// <param name="item">First data item</param>
		/// <param name="parameters">Parameters</param>
		/// <returns>The first item plus joined rows</returns>
		public virtual void AppendTail(object item, List<object> list, params object[] parameters)
		{
			if (!(_Select is IBqlJoinedSelect))
			{
				return;
			}
			if (_TailSelect == null)
			{
				_TailSelect = _Graph.TypedViews.GetView(((IBqlJoinedSelect)_Select).GetTail(), true);
			}
			List<object> tail = _TailSelect.SelectMultiBound(new object[] { item }, parameters);
			Type[] tables = _Select.GetTables();
			EnsureCreateInstance(tables);
			if (tail.Count == 0)
			{
				if (!(((IBqlJoinedSelect)_Select).IsInner))
				{
					object[] res = new object[tables.Length];
					res[0] = item;
					for (int i = 1; i < tables.Length; i++)
					{
						if (typeof(IBqlTable).IsAssignableFrom(tables[i]))
						{
							res[i] = _Graph.Caches[tables[i]].CreateInstance();
						}
					}
					list.Add(_CreateInstance(res));
				}
			}
			else if (!(tail[0] is PXResult))
			{
				for (int i = 0; i < tail.Count; i++)
				{
					list.Add(_CreateInstance(new object[] { item, tail[i] }));
				}
			}
			else
			{
				for (int i = 0; i < tail.Count; i++)
				{
					object[] res = new object[((PXResult)tail[i]).TableCount + 1];
					res[0] = item;
					for (int j = 1; j < res.Length; j++)
					{
						res[j] = ((PXResult)tail[i])[j - 1];
					}
					list.Add(_CreateInstance(res));
				}
			}
		}

		private bool? keyReferenceOnly;
		private bool? fullKeyReferenced;
		/// <summary>
		/// Merges resultset with the updated/inserted/deleted items, stores the resultset in the internal cache
		/// </summary>
		/// <param name="list">Resultset</param>
		/// <param name="parameters">Query parameters</param>
		/// <param name="sortcolumns">Sort columns</param>
		/// <param name="descendings">Descending flags</param>
		/// <param name="reverseOrder">If reverse of the sort expression is required</param>
		/// <param name="queryKey">Key to store the result in the cache</param>
		protected virtual bool MergeCache(
			List<object> list,
			object[] parameters,
			PXSearchColumn[] sorts,
			bool reverseOrder,
			//PXCommandKey queryKey,
			bool overrideSort,
			bool filterExists
			)
		{
			bool sortReq = overrideSort || HasUnboundSort(sorts);
			bool anyMerged = false;
			if (!_IsReadOnly)
			{
				int version = Cache.Version;
				if (_Select is IBqlJoinedSelect || !(list is VersionedList) || ((VersionedList)list).Version != version)
				{
					if (fullKeyReferenced == null)
					{
						if (Cache.BqlSelect != null || _Select is IBqlJoinedSelect)
						{
							fullKeyReferenced = false;
						}
						else
						{
							HashSet<Type> keys = new HashSet<Type>(Cache.BqlImmutables);
							foreach (Type field in _Select.GetReferencedFields(true))
							{
								if (!keys.Remove(field))
								{
									fullKeyReferenced = false;
									break;
								}
							}
							if (fullKeyReferenced == null)
							{
								if (keys.Count == 0)
								{
									fullKeyReferenced = true;
								}
								else
								{
									fullKeyReferenced = false;
								}
							}
						}
					}
					if (!(fullKeyReferenced == true && list.Count == 1 && list[0] != null && object.ReferenceEquals(Cache.Locate(list[0]), list[0])))
					{
						HashSet<object> existing = null;
						foreach (object item in Cache.Inserted)
						{
							if (_Select.Meet(Cache, item, parameters))
							{
								if (_Select is IBqlJoinedSelect)
								{
									for (int i = 0; i < list.Count; )
									{
										PXResult res = list[i] as PXResult;
										if (res != null && Cache.ObjectsEqual(item, res[0]))
										{
											list.RemoveAt(i);
										}
										else
										{
											i++;
										}
									}
									AppendTail(item, list, parameters);
								}
								else
								{
									if (existing == null)
									{
										existing = new HashSet<object>(list);
									}
									if (!existing.Contains(item))
									{
										list.Add(item);
									}
								}
								sortReq = true;
								anyMerged = true;
								if (fullKeyReferenced == true)
								{
									break;
								}
							}
						}
					}
				}
				if (keyReferenceOnly != true || filterExists)
				{
					foreach (object item in Cache.Updated)
					{
						if (keyReferenceOnly == null)
						{
							if (Cache.BqlSelect != null)
							{
								keyReferenceOnly = false;
							}
							else
							{
								List<Type> keys = Cache.BqlImmutables;
								keyReferenceOnly = true;
								foreach (Type field in _Select.GetReferencedFields(false))
								{
									if (BqlCommand.GetItemType(field) == CacheType && !keys.Contains(field))
									{
										keyReferenceOnly = false;
										break;
									}
								}
							}
						}
						if (keyReferenceOnly == true && !filterExists)
						{
							break;
						}
						if (_Select.Meet(Cache, item, parameters))
						{
							if (_Select is IBqlJoinedSelect)
							{
								for (int i = 0; i < list.Count; )
								{
									PXResult res = list[i] as PXResult;
									if (res != null && Cache.ObjectsEqual(item, res[0]))
									{
										list.RemoveAt(i);
									}
									else
									{
										i++;
									}
								}
								AppendTail(item, list, parameters);
							}
							else if (!list.Contains(item))
							{
								list.Add(item);
							}
							sortReq = true;
							anyMerged = true;
						}
						else
						{
							if (_Select is IBqlJoinedSelect)
							{
								for (int i = 0; i < list.Count; )
								{
									PXResult res = list[i] as PXResult;
									if (res != null && Cache.ObjectsEqual(item, res[0]))
									{
										list.RemoveAt(i);
									}
									else
									{
										i++;
									}
								}
							}
							else if (list.Contains(item))
							{
								list.Remove(item);
							}
						}
					}
				}
				if (list is VersionedList)
				{
					((VersionedList)list).Version = version;
					((VersionedList)list).AnyMerged |= anyMerged;
				}
			}

			if (sortReq)
			{
				SortResult(list, sorts, reverseOrder);
			}

			//if (queryKey != null)
			//{
			//	SelectQueries.StoreCached(this, queryKey, list);
			//}
			PX.SM.PXPerformanceMonitor.SetPeakMemory();
			return anyMerged;
		}

		//static readonly Regex ExtractTablesRegex = new Regex(@"((from)|(join))\s+(?<tableName>\w+)\s+", RegexOptions.IgnoreCase|RegexOptions.ExplicitCapture|RegexOptions.CultureInvariant |RegexOptions.Compiled);

		/// <summary>
		/// Sorts the resultset
		/// </summary>
		/// <param name="list">Resultset to sort</param>
		/// <param name="sortcolumns">Sort columns</param>
		/// <param name="descendings">Descending flags</param>
		/// <param name="reverseOrder">If reversing of the sort order is required</param>
		protected virtual void SortResult(List<object> list, PXSearchColumn[] sorts, bool reverseOrder)
		{
			if (list.Count < 2 || sorts.Length == 0)
			{
				return;
			}

			compareDelegate[] comparisons = new compareDelegate[sorts.Length];
			for (int i = 0; i < sorts.Length; i++)
			{
				string fieldName = sorts[i].Column;
				Type tableType = null;
				PXCache cache = Cache;
				bool descending = sorts[i].Descending;
				bool useExt = sorts[i].UseExt;
				int idx = fieldName.IndexOf("__");
				if (idx != -1)
				{
					tableType = list[0] is PXResult ? (((PXResult)list[0]).GetItemType(fieldName.Substring(0, idx))) : null;
					fieldName = fieldName.Substring(idx + 2);
					if (tableType != null)
					{
						cache = _Graph.Caches[tableType];
					}
				}
				else if (list[0] is PXResult)
				{
					tableType = ((PXResult)list[0]).GetItemType(0);
				}

				PXCollationComparer collationCmp = PXLocalesProvider.CollationComparer;
				if (!reverseOrder)
				{
					comparisons[i] = (a, b) => CompareMethod(a, b, cache, fieldName, @descending, useExt, tableType, collationCmp);
				}
				else
				{
					comparisons[i] = (a, b) => -CompareMethod(a, b, cache, fieldName, @descending, useExt, tableType, collationCmp);
				}
			}
			list.Sort((a, b) => Compare(a, b, comparisons));
		}

		protected delegate object getPXResultValue(object item);

		private static readonly int[] guidOrder = { 3, 2, 1, 0, 5, 4, 7, 6, 9, 8, 15, 14, 13, 12, 11, 10 };
		private static int compareGuid(Guid a, Guid b)
		{
			byte[] avals = a.ToByteArray();
			byte[] bvals = b.ToByteArray();
			for (int i = guidOrder.Length - 1; i >= 0; i--)
			{
				int res = avals[guidOrder[i]].CompareTo(bvals[guidOrder[i]]);
				if (res != 0)
				{
					return res;
				}
			}
			return 0;
		}

		protected sealed class HashList : List<object>
		{
			private HashSet<object> _hashset;
			public HashList()
				: base()
			{
				_hashset = new HashSet<object>();
			}
			public HashList(IEnumerable<object> collection)
				: base(collection)
			{
				_hashset = new HashSet<object>(collection);
			}
			public new bool Contains(object item)
			{
				return _hashset.Contains(item);
			}
			public new void Add(object item)
			{
				base.Add(item);
				_hashset.Add(item);
			}
			public new void RemoveAt(int index)
			{
				_hashset.Remove(base[index]);
				base.RemoveAt(index);
			}
		}

		protected virtual void FilterResult(
			List<object> list,
			PXFilterRow[] filters
			)
		{
			if (filters == null || filters.Length == 0 || list.Count == 0)
			{
				return;
			}

			List<KeyValuePair<HashList, bool>> levels = new List<KeyValuePair<HashList, bool>>();
			int currlevel = 0;

			for (int i = 0; i < filters.Length; i++)
			{
				PXFilterRow fr = filters[i];
				if (fr.Condition != PXCondition.ISNULL
					&& fr.Condition != PXCondition.ISNOTNULL
					&& fr.Condition != PXCondition.TODAY
					&& fr.Condition != PXCondition.OVERDUE
					&& fr.Condition != PXCondition.TODAY_OVERDUE
					&& fr.Condition != PXCondition.TOMMOROW
					&& fr.Condition != PXCondition.THIS_WEEK
					&& fr.Condition != PXCondition.NEXT_WEEK
					&& fr.Condition != PXCondition.THIS_MONTH
					&& fr.Condition != PXCondition.NEXT_MONTH
					&& fr.Value == null
					|| fr.Condition == PXCondition.BETWEEN
					&& fr.Value2 == null || string.IsNullOrEmpty(fr.DataField))
				{
					continue;
				}

				int idx = fr.DataField.IndexOf("__");
				Type t = list[0] is PXResult ? (idx != -1 ? ((PXResult)list[0]).GetItemType(fr.DataField.Substring(0, idx)) : ((PXResult)list[0]).GetItemType(0)) : Cache.GetItemType();
				string dataField = idx != -1 ? fr.DataField.Substring(idx + 2) : fr.DataField;
				if (idx != -1 && t == null)
				{
					continue;
				}
				PXCache cache = idx != -1 ? _Graph.Caches[t] : Cache;
				if (String.IsNullOrEmpty(dataField) || !cache.Fields.Contains(dataField))
				{
					continue;
				}
				getPXResultValue getMethod = list[0] is PXResult ?
					new getPXResultValue(delegate(object item)
					{ return cache.GetValue(((PXResult)item)[t], dataField); }) :
					new getPXResultValue(delegate(object item)
					{ return cache.GetValue(item, dataField); });
				getPXResultValue getExtMethod = list[0] is PXResult ?
					new getPXResultValue(delegate(object item)
					{ object val = cache.GetValueExt(((PXResult)item)[t], dataField); if (val is PXFieldState) val = ((PXFieldState)val).Value; return val; }) :
					new getPXResultValue(delegate(object item)
					{ object val = cache.GetValueExt(item, dataField); if (val is PXFieldState) val = ((PXFieldState)val).Value; return val; });

				Predicate<object> cmp = null;
				switch (fr.Condition)
				{
					case PXCondition.IN:
					case PXCondition.NI:
						if (fr.Value is Type)
						{
							PXCache subcache = _Graph.Caches[BqlCommand.GetItemType((Type)fr.Value)];
							List<PXDataField> pars = new List<PXDataField>();
							int brackets = fr.OpenBrackets;
							for (; brackets > 0 && i + 1 < filters.Length; i++)
							{
								brackets += filters[i + 1].OpenBrackets;
								brackets -= filters[i + 1].CloseBrackets;
								if (filters[i + 1].Description != null)
								{
									PXComp comparison;
									switch (filters[i + 1].Condition)
									{
										case PXCondition.EQ:
											comparison = PXComp.EQ;
											break;
										case PXCondition.NE:
											comparison = PXComp.NE;
											break;
										case PXCondition.GE:
											comparison = PXComp.GE;
											break;
										case PXCondition.GT:
											comparison = PXComp.GT;
											break;
										case PXCondition.LE:
											comparison = PXComp.LE;
											break;
										case PXCondition.LT:
											comparison = PXComp.LT;
											break;
										case PXCondition.ISNULL:
											comparison = PXComp.ISNULL;
											break;
										case PXCondition.ISNOTNULL:
											comparison = PXComp.ISNOTNULL;
											break;
										case PXCondition.LIKE:
											comparison = PXComp.LIKE;
											break;
										default:
											continue;
									}
									string fieldName = filters[i + 1].Description.FieldName;
									fieldName = fieldName.Replace(subcache.GetItemType().Name + ".", subcache.BqlTable.Name + ".");
									pars.Add(new PXDataFieldValue(
										fieldName,
										filters[i + 1].Description.DataType,
										filters[i + 1].Description.DataLength,
										filters[i + 1].Description.DataValue,
										comparison));
								}
							}
							cmp = delegate(object item)
							{
								object val = getMethod(item);
								PXCommandPreparingEventArgs.FieldDescription descr;
								subcache.RaiseCommandPreparing(((Type)fr.Value).Name, null, val, PXDBOperation.Select, null, out descr);
								if (descr != null && !String.IsNullOrEmpty(descr.FieldName) && descr.FieldName != BqlCommand.Null)
								{
									pars.Add(new PXDataFieldValue(
										descr.FieldName,
										descr.DataType,
										descr.DataLength,
										descr.DataValue));
									pars.Add(new PXDataField(
										descr.FieldName));
									using (PXDataRecord record = PXDatabase.SelectSingle(subcache.BqlTable, pars.ToArray()))
									{
										if (fr.Condition == PXCondition.IN && record != null
											|| fr.Condition == PXCondition.NI && record == null)
										{
											return true;
										}
										return false;
									}
								}
								return true;
							};
						}
						break;
					case PXCondition.ER:
							cmp = delegate(object item)
							{
								object val = getMethod(item);
								if (!(val is long) || !(fr.Value is string))
								{
									return false;
								}
								OrderedDictionary dict = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
								dict["NoteID"] = val;
								PXCache nc = _Graph.Caches[typeof(Note)];
								if (nc.Locate(dict) > 0 && nc.Current != null && String.Equals(((Note)nc.Current).ExternalKey, (string)fr.Value, StringComparison.InvariantCultureIgnoreCase))
								{
									return true;
								}
								return false;
							};
						break;
					case PXCondition.EQ:
						if (fr.Value != null)
						{
							if (fr.Value is string)
							{
								string sval = (string)fr.Value;
								if (fr.UseExt)
								{
									cmp = delegate(object item)
									{
										string val = getExtMethod(item) as string;
										return String.Compare(val, sval, StringComparison.OrdinalIgnoreCase) == 0;
									};
								}
								else
								{
									cmp = delegate(object item)
									{
										string val = getMethod(item) as string;
										return String.Compare(val, sval, StringComparison.OrdinalIgnoreCase) == 0;
									};
								}
							}
							else
							{
								if (fr.UseExt)
								{
									cmp = delegate(object item)
									{
										return object.Equals(getExtMethod(item), fr.Value);
									};
								}
								else
								{
									cmp = delegate(object item)
									{
										return object.Equals(getMethod(item), fr.Value);
									};
								}
							}
						}
						else
						{
							list.Clear();
							return;
						}
						break;
					case PXCondition.NE:
						if (fr.Value != null)
						{
							if (fr.Value is string)
							{
								string sval = (string)fr.Value;
								if (fr.UseExt)
								{
									cmp = delegate(object item)
									{
										string val = getExtMethod(item) as string;
										return String.Compare(val, sval, StringComparison.OrdinalIgnoreCase) != 0;
									};
								}
								else
								{
									cmp = delegate(object item)
									{
										string val = getMethod(item) as string;
										return String.Compare(val, sval, StringComparison.OrdinalIgnoreCase) != 0;
									};
								}
							}
							else
							{
								if (fr.UseExt)
								{
									cmp = delegate(object item)
									{
										return !object.Equals(getExtMethod(item), fr.Value);
									};
								}
								else
								{
									cmp = delegate(object item)
									{
										return !object.Equals(getMethod(item), fr.Value);
									};
								}
							}
						}
						break;
					case PXCondition.GT:
						if (fr.Value is string)
						{
							string sval = (string)fr.Value;
							if (fr.UseExt)
							{
								cmp = delegate(object item)
								{
									string val = getExtMethod(item) as string;
									return PXLocalesProvider.CollationComparer.Compare(sval, val) < 0;
								};
							}
							else
							{
								cmp = delegate(object item)
								{
									string val = getMethod(item) as string;
									return PXLocalesProvider.CollationComparer.Compare(sval, val) < 0;
								};
							}
						}
						else if (fr.Value is IComparable)
						{
							if (fr.UseExt)
							{
								cmp = delegate(object item)
								{
									return ((IComparable)fr.Value).CompareTo(getExtMethod(item)) < 0;
								};
							}
							else
							{
								cmp = delegate(object item)
								{
									return ((IComparable)fr.Value).CompareTo(getMethod(item)) < 0;
								};
							}
						}
						break;
					case PXCondition.GE:
						if (fr.Value is string)
						{
							string sval = (string)fr.Value;
							if (fr.UseExt)
							{
								cmp = delegate(object item)
								{
									string val = getExtMethod(item) as string;
									return PXLocalesProvider.CollationComparer.Compare(sval, val) <= 0;
								};
							}
							else
							{
								cmp = delegate(object item)
								{
									string val = getMethod(item) as string;
									return PXLocalesProvider.CollationComparer.Compare(sval, val) <= 0;
								};
							}
						}
						else if (fr.Value is IComparable)
						{
							if (fr.UseExt)
							{
								cmp = delegate(object item)
								{
									return ((IComparable)fr.Value).CompareTo(getExtMethod(item)) <= 0;
								};
							}
							else
							{
								cmp = delegate(object item)
								{
									return ((IComparable)fr.Value).CompareTo(getMethod(item)) <= 0;
								};
							}
						}
						break;
					case PXCondition.LT:
						if (fr.Value is string)
						{
							string sval = (string)fr.Value;
							if (fr.UseExt)
							{
								cmp = delegate(object item)
								{
									string val = getExtMethod(item) as string;
									return PXLocalesProvider.CollationComparer.Compare(sval, val) > 0;
								};
							}
							else
							{
								cmp = delegate(object item)
								{
									string val = getMethod(item) as string;
									return PXLocalesProvider.CollationComparer.Compare(sval, val) > 0;
								};
							}
						}
						else if (fr.Value is IComparable)
						{
							if (fr.UseExt)
							{
								cmp = delegate(object item)
								{
									return ((IComparable)fr.Value).CompareTo(getExtMethod(item)) > 0;
								};
							}
							else
							{
								cmp = delegate(object item)
								{
									return ((IComparable)fr.Value).CompareTo(getMethod(item)) > 0;
								};
							}
						}
						else
						{
							list.Clear();
							return;
						}
						break;
					case PXCondition.LE:
						if (fr.Value is string)
						{
							string sval = (string)fr.Value;
							if (fr.UseExt)
							{
								cmp = delegate(object item)
								{
									string val = getExtMethod(item) as string;
									return PXLocalesProvider.CollationComparer.Compare(sval, val) >= 0;
								};
							}
							else
							{
								cmp = delegate(object item)
								{
									string val = getMethod(item) as string;
									return PXLocalesProvider.CollationComparer.Compare(sval, val) >= 0;
								};
							}
						}
						else if (fr.Value is IComparable)
						{
							if (fr.UseExt)
							{
								cmp = delegate(object item)
								{
									return ((IComparable)fr.Value).CompareTo(getExtMethod(item)) >= 0;
								};
							}
							else
							{
								cmp = delegate(object item)
								{
									return ((IComparable)fr.Value).CompareTo(getMethod(item)) >= 0;
								};
							}
						}
						else
						{
							list.Clear();
							return;
						}
						break;
					case PXCondition.LIKE:
						if (fr.Value is string)
						{
							string lower = ((string)fr.Value).ToLower();
							int underscore;
							if ((underscore = lower.IndexOf(PXDatabase.Provider.SqlDialect.WildcardAnySingle)) == -1)
							{
								if (fr.UseExt)
								{
									cmp = delegate(object item)
									{
										object val = getExtMethod(item);
										return val is string && ((string)val).ToLower().Contains(lower);
									};
								}
								else
								{
									cmp = delegate(object item)
									{
										object val = getMethod(item);
										return val is string && ((string)val).ToLower().Contains(lower);
									};
								}
							}
							else
							{
								int[] underscores = getUnderscores(lower, underscore);
								lower = lower.Replace("_", "");
								if (fr.UseExt)
								{
									cmp = delegate(object item)
									{
										object val = getExtMethod(item);
										if (val is string)
										{
											string stringval = ((string)val).ToLower();
											for (int j = underscores.Length - 1; j >= 0; j--)
											{
												if (underscores[j] >= stringval.Length)
												{
													return false;
												}
												stringval = stringval.Remove(underscores[j], 1);
											}
											return stringval.Contains(lower);
										}
										return false;
									};
								}
								else
								{
									cmp = delegate(object item)
									{
										object val = getMethod(item);
										if (val is string)
										{
											string stringval = ((string)val).ToLower();
											for (int j = underscores.Length - 1; j >= 0; j--)
											{
												if (underscores[j] >= stringval.Length)
												{
													return false;
												}
												stringval = stringval.Remove(underscores[j], 1);
											}
											return stringval.Contains(lower);
										}
										return false;
									};
								}
							}
						}
						break;
					case PXCondition.NOTLIKE:
						if (fr.Value is string)
						{
							string lower = ((string)fr.Value).ToLower();
							int underscore;
							if ((underscore = lower.IndexOf(PXDatabase.Provider.SqlDialect.WildcardAnySingle)) == -1)
							{
								if (fr.UseExt)
								{
									cmp = delegate(object item)
									{
										object val = getExtMethod(item);
										return val is string && !(((string)val).ToLower().Contains(lower));
									};
								}
								else
								{
									cmp = delegate(object item)
									{
										object val = getMethod(item);
										return val is string && !(((string)val).ToLower().Contains(lower));
									};
								}
							}
							else
							{
								int[] underscores = getUnderscores(lower, underscore);
								lower = lower.Replace("_", "");
								if (fr.UseExt)
								{
									cmp = delegate(object item)
									{
										object val = getExtMethod(item);
										if (val is string)
										{
											string stringval = ((string)val).ToLower();
											for (int j = underscores.Length - 1; j >= 0; j--)
											{
												if (underscores[j] >= stringval.Length)
												{
													return true;
												}
												stringval = stringval.Remove(underscores[j], 1);
											}
											return !stringval.Contains(lower);
										}
										return false;
									};
								}
								else
								{
									cmp = delegate(object item)
									{
										object val = getMethod(item);
										if (val is string)
										{
											string stringval = ((string)val).ToLower();
											for (int j = underscores.Length - 1; j >= 0; j--)
											{
												if (underscores[j] >= stringval.Length)
												{
													return true;
												}
												stringval = stringval.Remove(underscores[j], 1);
											}
											return !stringval.Contains(lower);
										}
										return false;
									};
								}
							}
						}
						break;
					case PXCondition.LLIKE:
						if (fr.Value is string)
						{
							string lower = ((string)fr.Value).ToLower();
							int underscore;
							if ((underscore = lower.IndexOf(PXDatabase.Provider.SqlDialect.WildcardAnySingle)) == -1)
							{
								if (fr.UseExt)
								{
									cmp = delegate(object item)
									{
										object val = getExtMethod(item);
										return val is string && ((string)val).ToLower().EndsWith(lower);
									};
								}
								else
								{
									cmp = delegate(object item)
									{
										object val = getMethod(item);
										return val is string && ((string)val).ToLower().EndsWith(lower);
									};
								}
							}
							else
							{
								int[] underscores = getUnderscores(lower, underscore);
								lower = lower.Replace("_", "");
								if (fr.UseExt)
								{
									cmp = delegate(object item)
									{
										object val = getExtMethod(item);
										if (val is string)
										{
											string stringval = ((string)val).ToLower();
											for (int j = underscores.Length - 1; j >= 0; j--)
											{
												if (underscores[j] >= stringval.Length)
												{
													return false;
												}
												stringval = stringval.Remove(underscores[j], 1);
											}
											return stringval.EndsWith(lower);
										}
										return false;
									};
								}
								else
								{
									cmp = delegate(object item)
									{
										object val = getMethod(item);
										if (val is string)
										{
											string stringval = ((string)val).ToLower();
											for (int j = underscores.Length - 1; j >= 0; j--)
											{
												if (underscores[j] >= stringval.Length)
												{
													return false;
												}
												stringval = stringval.Remove(underscores[j], 1);
											}
											return stringval.EndsWith(lower);
										}
										return false;
									};
								}
							}
						}
						break;
					case PXCondition.RLIKE:
						if (fr.Value is string)
						{
							string lower = ((string)fr.Value).ToLower();
							int underscore;
							if ((underscore = lower.IndexOf(PXDatabase.Provider.SqlDialect.WildcardAnySingle)) == -1)
							{
								if (fr.UseExt)
								{
									cmp = delegate(object item)
									{
										object val = getExtMethod(item);
										return val is string && ((string)val).ToLower().StartsWith(lower);
									};
								}
								else
								{
									cmp = delegate(object item)
									{
										object val = getMethod(item);
										return val is string && ((string)val).ToLower().StartsWith(lower);
									};
								}
							}
							else
							{
								int[] underscores = getUnderscores(lower, underscore);
								lower = lower.Replace("_", "");
								if (fr.UseExt)
								{
									cmp = delegate(object item)
									{
										object val = getExtMethod(item);
										if (val is string)
										{
											string stringval = ((string)val).ToLower();
											for (int j = underscores.Length - 1; j >= 0; j--)
											{
												if (underscores[j] >= stringval.Length)
												{
													return false;
												}
												stringval = stringval.Remove(underscores[j], 1);
											}
											return stringval.StartsWith(lower);
										}
										return false;
									};
								}
								else
								{
									cmp = delegate(object item)
									{
										object val = getMethod(item);
										if (val is string)
										{
											string stringval = ((string)val).ToLower();
											for (int j = underscores.Length - 1; j >= 0; j--)
											{
												if (underscores[j] >= stringval.Length)
												{
													return false;
												}
												stringval = stringval.Remove(underscores[j], 1);
											}
											return stringval.StartsWith(lower);
										}
										return false;
									};
								}
							}
						}
						break;
					case PXCondition.ISNULL:
						{
							if (fr.UseExt)
							{
								cmp = delegate(object item)
								{
									return getExtMethod(item) == null;
								};
							}
							else
							{
								cmp = delegate(object item)
								{
									return getMethod(item) == null;
								};
							}
						}
						break;
					case PXCondition.ISNOTNULL:
						{
							if (fr.UseExt)
							{
								cmp = delegate(object item)
								{
									return getExtMethod(item) != null;
								};
							}
							else
							{
								cmp = delegate(object item)
								{
									return getMethod(item) != null;
								};
							}
						}
						break;
					case PXCondition.OVERDUE:
					case PXCondition.TOMMOROW:
					case PXCondition.THIS_WEEK:
					case PXCondition.TODAY:
					case PXCondition.THIS_MONTH:
					case PXCondition.BETWEEN:
						if (fr.Value is string && fr.Value2 is string)
						{
							if (fr.UseExt)
							{
								cmp = delegate(object item)
								{
									object val = getExtMethod(item);
									if (val is string)
									{
										return PXLocalesProvider.CollationComparer.Compare((string)fr.Value, (string)val) <= 0 && PXLocalesProvider.CollationComparer.Compare((string)fr.Value2, (string)val) >= 0;
									}
									return ((IComparable)fr.Value).CompareTo(val) <= 0 && ((IComparable)fr.Value2).CompareTo(val) >= 0;
								};
							}
							else
							{
								cmp = delegate(object item)
								{
									object val = getMethod(item);
									if (val is string)
									{
										return PXLocalesProvider.CollationComparer.Compare((string)fr.Value, (string)val) <= 0 && PXLocalesProvider.CollationComparer.Compare((string)fr.Value2, (string)val) >= 0;
									}
									return ((IComparable)fr.Value).CompareTo(val) <= 0 && ((IComparable)fr.Value2).CompareTo(val) >= 0;
								};
							}
						}
						else if (fr.Value is IComparable && fr.Value2 is IComparable)
						{
							if (fr.UseExt)
							{
								cmp = delegate(object item)
								{
									object val = getExtMethod(item);
									return ((IComparable)fr.Value).CompareTo(val) <= 0 && ((IComparable)fr.Value2).CompareTo(val) >= 0;
								};
							}
							else
							{
								cmp = delegate(object item)
								{
									object val = getMethod(item);
									return ((IComparable)fr.Value).CompareTo(val) <= 0 && ((IComparable)fr.Value2).CompareTo(val) >= 0;
								};
							}
						}
						else
						{
							list.Clear();
							return;
						}
						break;										
					case PXCondition.TODAY_OVERDUE:
					case PXCondition.NEXT_WEEK:
					case PXCondition.NEXT_MONTH:
						DateTime?[] range = DateTimeFactory.GetDateRage(fr.Condition, null);
						cmp = FilterResultBetweenDate(fr, getMethod, getExtMethod, range[0], range[1]);
						break;
				}
				if (cmp != null)
				{
					currlevel += fr.OpenBrackets;
					if (currlevel < levels.Count)
					{
						if (!levels[currlevel].Value)
						{
							levels[currlevel] = new KeyValuePair<HashList, bool>(new HashList(levels[currlevel].Key.FindAll(cmp)), fr.OrOperator);
						}
						else
						{
							List<object> filtered = list.FindAll(cmp);
							foreach (object found in filtered)
							{
								if (!levels[currlevel].Key.Contains(found))
								{
									levels[currlevel].Key.Add(found);
								}
							}
							levels[currlevel] = new KeyValuePair<HashList, bool>(levels[currlevel].Key, fr.OrOperator);
						}
					}
					else
					{
						for (int k = levels.Count; k <= currlevel; k++)
						{
							levels.Add(new KeyValuePair<HashList, bool>(new HashList(), true));
						}
						levels[currlevel] = new KeyValuePair<HashList, bool>(new HashList(list.FindAll(cmp)), fr.OrOperator);
					}
					for (int k = 0; k < fr.CloseBrackets; k++)
					{
						if (currlevel == 0)
						{
							break;
						}
						if (levels[currlevel - 1].Value)
						{
							foreach (object found in levels[currlevel].Key)
							{
								if (!levels[currlevel - 1].Key.Contains(found))
								{
									levels[currlevel - 1].Key.Add(found);
								}
							}
						}
						else
						{
							for (int l = 0; l < levels[currlevel - 1].Key.Count; )
							{
								if (!levels[currlevel].Key.Contains(levels[currlevel - 1].Key[l]))
								{
									levels[currlevel - 1].Key.RemoveAt(l);
								}
								else
								{
									l++;
								}
							}
						}
						levels[currlevel - 1] = new KeyValuePair<HashList, bool>(levels[currlevel - 1].Key, fr.OrOperator);
						levels.RemoveAt(currlevel);
						currlevel--;
					}
				}
			}
			for (; currlevel > 0; currlevel--)
			{
				if (levels[currlevel - 1].Value)
				{
					foreach (object found in levels[currlevel].Key)
					{
						if (!levels[currlevel - 1].Key.Contains(found))
						{
							levels[currlevel - 1].Key.Add(found);
						}
					}
				}
				else
				{
					for (int l = 0; l < levels[currlevel - 1].Key.Count; )
					{
						if (!levels[currlevel].Key.Contains(levels[currlevel - 1].Key[l]))
						{
							levels[currlevel - 1].Key.RemoveAt(l);
						}
						else
						{
							l++;
						}
					}
				}
			}
			if (levels.Count > 0)
			{
				for (int i = 0; i < list.Count; )
				{
					if (levels[0].Key.Contains(list[i]))
					{
						i++;
					}
					else
					{
						list.RemoveAt(i);
					}
				}
			}
		}

		protected int[] getUnderscores(string value, int pos)
		{
			List<int> ret = new List<int>();
			while (pos != -1)
			{
				ret.Add(pos);
				pos = value.IndexOf(PXDatabase.Provider.SqlDialect.WildcardAnySingle, pos + 1);
			}
			return ret.ToArray();
		}

		/// <summary>
		/// Cuts the resultset by search values, starting row and number of rows required
		/// </summary>
		/// <param name="list">The resultset</param>
		/// <param name="searches">Values to search</param>
		/// <param name="sortcolumns">Sort columns</param>
		/// <param name="descendings">Descending flags</param>
		/// <param name="reverseOrder">If reversing of the sort expression is required</param>
		/// <param name="startRow">Index of row to start with</param>
		/// <param name="maximumRows">Maximum number of rows to return</param>
		/// <param name="totalRows">Total number of rows fetched</param>
		/// <param name="searchFound">If there is an item that meets search values</param>
		/// <returns>Filtered resultset</returns>


		private static Predicate<object> FilterResultBetweenDate(PXFilterRow fr, getPXResultValue getMethod, getPXResultValue getExtMethod, DateTime? startDate, DateTime? endDate)
		{
			if (fr.UseExt)
				return delegate(object item)
				{
					DateTime? val = (DateTime?)getExtMethod(item);
					return val != null &&
						(startDate == null || startDate.Value.CompareTo(val) <= 0) &&
						(endDate == null || endDate.Value.CompareTo(val) >= 0);
				};

			return delegate(object item)
			{
				DateTime? val = (DateTime?)getMethod(item);
				return val != null &&
					(startDate == null || startDate.Value.CompareTo(val) <= 0) &&
					(endDate == null || endDate.Value.CompareTo(val) >= 0);
			};
		}

		protected virtual List<object> SearchResult(
			List<object> list,
			PXSearchColumn[] sorts,
			bool reverseOrder,
			bool findAll,
			ref int startRow,
			int maximumRows,
			ref int totalRows,
			out bool searchFound
			)
		{
			if (list.Count == 0)
			{
				searchFound = false;
				return list;
			}

			searchFound = false;
			int startOffset = 0;
			List<object> ret = list is VersionedList ? new VersionedList() : new List<object>();

			List<searchDelegate> comparisons = null;
			for (int k = 0; k < sorts.Length; k++)
			{
				if (sorts[k].SearchValue != null)
				{
					if (comparisons == null)
					{
						comparisons = new List<searchDelegate>();
					}
					string fieldName = sorts[k].Column;
					Type tableType = null;
					PXCache cache = Cache;
					bool descending = sorts[k].Descending;
					bool useExt = sorts[k].UseExt;
					int idx = fieldName.IndexOf("__");
					object searchValue = sorts[k].SearchValue;
					if (idx != -1)
					{
						tableType = list[0] is PXResult ? (((PXResult)list[0]).GetItemType(fieldName.Substring(0, idx))) : null;
						if (tableType != null)
						{
							cache = _Graph.Caches[tableType];
						}
						fieldName = fieldName.Substring(idx + 2);
					}
					else if (list[0] is PXResult)
					{
						tableType = ((PXResult)list[0]).GetItemType(0);
					}
					comparisons.Add(delegate(object a)
					{
						return SearchMethod(a, searchValue, cache, fieldName, descending, useExt, tableType);
					});
				}
			}

			if (comparisons != null)
			{
				searchDelegate[] searches = comparisons.ToArray();
				int i = 0;
				if (comparisons.Count > 0)
				{
					while (i < list.Count && (!reverseOrder && Search(list[i], searches) < 0 || reverseOrder && Search(list[i], searches) >= 0))
					{
						i++;
					}
				}
				searchFound = !reverseOrder && i < list.Count && Search(list[i], searches) == 0;

				if (totalRows == -1 && maximumRows > 2)
				{
					if (!reverseOrder)
					{
						i = (i / maximumRows * maximumRows);
					}
					else
					{
						int j = list.Count - i - 1;
						j = ((j / maximumRows + 1) * maximumRows);
						i = list.Count - j;
						if (i < 0)
						{
							maximumRows += i;
							i = 0;
						}
					}
				}

				int k = list.Count;
				if (searchFound && !reverseOrder && findAll)
				{
					k = i + 1;
					while (k < list.Count && Search(list[k], searches) == 0)
					{
						k++;
					}
				}

				for (int j = i; j < k; j++)
				{
					ret.Add(list[j]);
				}

				if (totalRows == -1)
				{
					if (reverseOrder)
					{
						startOffset = list.Count - i - 1;
					}
					else
					{
						startOffset = i;
					}
				}
			}
			else
			{
				for (int i = 0; i < list.Count; i++)
				{
					ret.Add(list[i]);
				}
				if (totalRows == -1)
				{
					if (reverseOrder)
					{
						if (maximumRows > 2)
						{
							int j = list.Count - startRow - 1;
							j = ((j / maximumRows + 1) * maximumRows);
							if (j > 0 && j < list.Count && list.Count - j < startRow)
							{
								maximumRows = list.Count - j;
								startRow = maximumRows - 1;
							}
						}
						startOffset = list.Count;
					}
				}
			}

			totalRows = list.Count;

			if (!reverseOrder)
			{
				if (startRow > 0)
				{
					if (startRow <= ret.Count)
					{
						ret.RemoveRange(0, startRow);
					}
					else
					{
						ret.Clear();
					}
				}
			}
			else
			{
				if (maximumRows > 0 && startRow >= ret.Count)
				{
					maximumRows -= (startRow - ret.Count + 1);
					if (maximumRows < 0) maximumRows = 0;
				}
				if (startRow + 1 < ret.Count)
				{
					ret.RemoveRange(startRow + 1, ret.Count - startRow - 1);
				}
				ret.Reverse();
			}

			if (maximumRows >= 0 && maximumRows < ret.Count)
			{
				ret.RemoveRange(maximumRows, ret.Count - maximumRows);
			}

			if (!reverseOrder)
			{
				startRow += startOffset;
			}
			else
			{
				startRow = startOffset - startRow - 1;
			}

			if (ret is VersionedList)
			{
				((VersionedList)ret).Version = ((VersionedList)list).Version;
			}

			return ret;
		}

		public class PXSortColumn
		{
			public string Column;
			public bool Descending;
			public bool UseExt;

			public PXSortColumn(string column, bool descending)
			{
				Column = column;
				Descending = descending;
			}
		}
		protected sealed class PXSearchColumn : PXSortColumn
		{
			public object SearchValue;
			public object OrigSearchValue;
			public PXCommandPreparingEventArgs.FieldDescription Description;

			public PXSearchColumn(string column, bool descending, object searchValue)
				: base(column, descending)
			{

				OrigSearchValue = SearchValue = searchValue;
			}
		}

		/// <summary>
		/// Main selection procedure
		/// </summary>
		/// <param name="currents">Items to replace current values when retrieving Current and Optional parameters</param>
		/// <param name="parameters">Query parameters</param>
		/// <param name="searches">Search values</param>
		/// <param name="sortcolumns">Sort columns</param>
		/// <param name="descendings">Descending flags</param>
		/// <param name="startRow">Index of row to start with</param>
		/// <param name="maximumRows">Maximum rows to return</param>
		/// <param name="totalRows">Total rows fetched</param>
		/// <returns>Resultset requested</returns>
		public virtual List<object> Select(
			object[] currents,
			object[] parameters,
			object[] searches,
			string[] sortcolumns,
			bool[] descendings,
			PXFilterRow[] filters,
			ref int startRow,
			int maximumRows,
			ref int totalRows
			)
		{
			var perf = PXPerformanceMonitor.CurrentSample;
			if (perf != null)
			{
				perf.SelectCount++;
				perf.SelectTimer.Start();
			}
			int realTotalRows = totalRows;
			if (totalRows == -2) totalRows = 0;
			bool overrideSort;
			bool anySearch;
			bool resetTopCount = false;
			PXSearchColumn[] sorts = prepareSorts(sortcolumns, descendings, searches, maximumRows, out overrideSort, out anySearch, ref resetTopCount);
			parameters = PrepareParameters(currents, parameters);
			bool extFilter = prepareFilters(filters);
			if (extFilter)
			{
				resetTopCount = true;
			}
			int topCount = 0;
			bool reverseOrder = false;
			if (startRow < 0)
			{
				reverseOrder = true;
				overrideSort = true;
				startRow = -1 - startRow;
				if (maximumRows > 0 && totalRows != -1)
				{
					topCount = startRow + 1;
				}
			}
			else
			{
				if (maximumRows > 0 && totalRows != -1)
				{
					topCount = startRow + maximumRows;
				}
			}
			if (maximumRows == 0) maximumRows = -1;
			if (resetTopCount)
			{
				topCount = 0;
			}
			List<object> list = null;
			bool windowed = (totalRows != -1 && maximumRows > 0);
			try
			{
				if (_Executing != null)
				{
					_Executing.Push(new Context(this, sorts, filters, currents, parameters, reverseOrder, windowed ? startRow : 0, windowed ? maximumRows : 0, this.RestrictedFields.ToArray()));
				}
				list = InvokeDelegate(parameters);
			}
			finally
			{
				if (_Executing != null)
				{
					Context context = _Executing.Pop();
					if (list != null && windowed)
					{
						if (reverseOrder && context.StartRow == 0 && startRow != 0)
						{
							startRow = maximumRows - 1;
						}
						else
						{
							startRow = context.StartRow;
						}
					}
					if (!object.ReferenceEquals(context.Filters, filters))
					{
						filters = context.Filters;
						extFilter = prepareFilters(filters);
					}
					if (!object.ReferenceEquals(context.Sorts, sorts))
					{
						sorts = context.Sorts;
					}
				}
			}
			int realTopCount = topCount;
			if (list == null)
			{
				PXCommandKey queryKey = null;
				if (!PXDatabase.ReadDeleted)
				{
					string[] fields = RestrictedFields.Any() ? RestrictedFields.Select(f => f.Name).ToArray() : null;
					queryKey = topCount == 0 
						? new PXCommandKey(parameters, searches, sortcolumns, descendings, null, null, filters, PXDatabase.ReadBranchRestricted, fields) 
						: new PXCommandKey(parameters, searches, sortcolumns, descendings, (reverseOrder ? -1 - startRow : startRow), maximumRows, filters, PXDatabase.ReadBranchRestricted, fields);
					list = LookupCache(queryKey, ref topCount);
				}
				if (list == null)
				{
					list = GetResult(parameters, filters, reverseOrder, topCount, sorts, ref overrideSort, ref extFilter);

				}
				else
				{
					overrideSort = false;
					extFilter |= !(list is VersionedList) || ((VersionedList)list).AnyMerged;
				}
				extFilter = MergeCache(list, parameters, sorts, reverseOrder, overrideSort, filters != null && filters.Length > 0) || extFilter;
				if (queryKey != null)
				{
					SelectQueries.StoreCached(this, queryKey, list);
				}
			}
			else
			{
				extFilter = true;
				SortResult(list, sorts, reverseOrder);
			}
			bool searchFound = false;
			if (extFilter)
			{
				FilterResult(list, filters);
			}
			list = SearchResult(list, sorts, reverseOrder, (realTotalRows == -2), ref startRow, maximumRows, ref totalRows, out searchFound);
			if (!reverseOrder && (realTotalRows == -2 || realTopCount == 1 || realTopCount == 0 && maximumRows == 1) && anySearch && !searchFound && list.Count > 0)
			{
				list.Clear();
			}
			if (_Graph.AutomationView != null)
			{
				string viewName;
				if (_Graph.ViewNames.TryGetValue(this, out viewName) && viewName == _Graph.AutomationView)
				{
					foreach (object t in list)
					{
						object row = t;
						if (row is PXResult)
							row = ((PXResult)row)[0];

						Cache.Current = row;
					}
					PXAutomation.GetStep(_Graph, list, _Select);
					if (list.Count > 0)
					{
						object row = list[list.Count - 1];
						object item = row;
						if (item is PXResult)
						{
							item = ((PXResult)item)[0];
						}
						Cache.Current = item;
						PXAutomation.ApplyStep(_Graph, row, false);
					}
				}
			}

			if (perf != null)
			{
				perf.SelectTimer.Stop();
			}
			return list;
		}

		private delegate List<object> _InvokeDelegate(Delegate method, object[] parameters);
		private _InvokeDelegate _CustomMethod;
		private delegate object _InstantiateDelegate(object[] parameters);
		private _InstantiateDelegate _CreateInstance;
		private static System.Threading.ReaderWriterLock _CreateInstanceLock = new System.Threading.ReaderWriterLock();
		private static Dictionary<PXCreateInstanceKey, _InstantiateDelegate> _CreateInstanceDict = new Dictionary<PXCreateInstanceKey, _InstantiateDelegate>();

		private void EnsureCreateInstance(Type[] tables)
		{
			if (tables.Length > 1 && _CreateInstance == null)
			{
				_CreateInstanceLock.AcquireReaderLock(-1);
				try
				{
					PXCreateInstanceKey createkey = new PXCreateInstanceKey(tables);
					if (!_CreateInstanceDict.TryGetValue(createkey, out _CreateInstance))
					{
						System.Threading.LockCookie c = _CreateInstanceLock.UpgradeToWriterLock(-1);
						try
						{
							if (!_CreateInstanceDict.TryGetValue(createkey, out _CreateInstance))
							{
								Type result = null;
								if (!typeof(IBqlTable).IsAssignableFrom(tables[tables.Length - 1]))
								{
									Type[] bqltables = new Type[tables.Length - 1];
									Array.Copy(tables, bqltables, bqltables.Length);
									tables = bqltables;
								}
								switch (tables.Length)
								{
									case 1:
										result = typeof(PXResult<>).MakeGenericType(tables);
										break;
									case 2:
										result = typeof(PXResult<,>).MakeGenericType(tables);
										break;
									case 3:
										result = typeof(PXResult<,,>).MakeGenericType(tables);
										break;
									case 4:
										result = typeof(PXResult<,,,>).MakeGenericType(tables);
										break;
									case 5:
										result = typeof(PXResult<,,,,>).MakeGenericType(tables);
										break;
									case 6:
										result = typeof(PXResult<,,,,,>).MakeGenericType(tables);
										break;
									case 7:
										result = typeof(PXResult<,,,,,,>).MakeGenericType(tables);
										break;
									case 8:
										result = typeof(PXResult<,,,,,,,>).MakeGenericType(tables);
										break;
									case 9:
										result = typeof(PXResult<,,,,,,,,>).MakeGenericType(tables);
										break;
									case 10:
										result = typeof(PXResult<,,,,,,,,,>).MakeGenericType(tables);
										break;
									case 11:
										result = typeof(PXResult<,,,,,,,,,,>).MakeGenericType(tables);
										break;
								}
								DynamicMethod dm;
								if (!PXGraph.IsRestricted)
								{
									dm = new DynamicMethod("_CreateInstance", typeof(object), new Type[] { typeof(object[]) }, typeof(PXView));
								}
								else
								{
									dm = new DynamicMethod("_CreateInstance", typeof(object), new Type[] { typeof(object[]) });
								}
								ILGenerator il = dm.GetILGenerator();
								int[] idx = new int[tables.Length];
								for (int i = 0; i < tables.Length; i++)
								{
									idx[i] = il.DeclareLocal(tables[i]).LocalIndex;
									il.Emit(OpCodes.Ldarg_0);
									il.Emit(OpCodes.Ldc_I4, i);
									il.Emit(OpCodes.Ldelem_Ref);
									if (tables[i].IsValueType)
									{
										il.Emit(OpCodes.Unbox_Any, tables[i]);
									}
									else
									{
										il.Emit(OpCodes.Castclass, tables[i]);
									}
									il.Emit(OpCodes.Stloc, idx[i]);
								}
								for (int i = 0; i < tables.Length; i++)
								{
									il.Emit(OpCodes.Ldloc, idx[i]);
								}
								il.Emit(OpCodes.Newobj, result.GetConstructor(tables));
								il.Emit(OpCodes.Ret);
								_CreateInstanceDict[createkey] = _CreateInstance = (_InstantiateDelegate)dm.CreateDelegate(typeof(_InstantiateDelegate));
							}
						}
						finally
						{
							_CreateInstanceLock.DowngradeFromWriterLock(ref c);
						}
					}
				}
				finally
				{
					_CreateInstanceLock.ReleaseReaderLock();
				}
			}
		}

		private static System.Threading.ReaderWriterLock _InvokeLock = new System.Threading.ReaderWriterLock();
		private static Dictionary<Type, _InvokeDelegate> _InvokeDict = new Dictionary<Type, _InvokeDelegate>();

		private void EnsureDelegate()
		{
			if (_CustomMethod == null)
			{
				_InvokeLock.AcquireReaderLock(-1);
				try
				{
					Type deltype = _Delegate.GetType();
					if (!_InvokeDict.TryGetValue(deltype, out _CustomMethod))
					{
						System.Threading.LockCookie c = _InvokeLock.UpgradeToWriterLock(-1);
						try
						{
							if (!_InvokeDict.TryGetValue(deltype, out _CustomMethod))
							{
								DynamicMethod dm;
								if (!PXGraph.IsRestricted)
								{
									dm = new DynamicMethod("_CustomMethod", typeof(List<object>), new Type[] { typeof(object), typeof(object[]) }, typeof(PXView));
								}
								else
								{
									dm = new DynamicMethod("_CustomMethod", typeof(List<object>), new Type[] { typeof(object), typeof(object[]) });
								}
								ILGenerator il = dm.GetILGenerator();
								ParameterInfo[] pars = _Delegate.Method.GetParameters();
								int[] idx = new int[pars.Length];
								bool byRef = (_Delegate.Method.ReturnType == typeof(void));
								for (int i = 0; i < pars.Length; i++)
								{
									Type pt = byRef ? pars[i].ParameterType.GetElementType() : pars[i].ParameterType;
									idx[i] = il.DeclareLocal(pt).LocalIndex;
									il.Emit(OpCodes.Ldarg_1);
									il.Emit(OpCodes.Ldc_I4, i);
									il.Emit(OpCodes.Ldelem_Ref);
									if (pt.IsValueType)
									{
										il.Emit(OpCodes.Unbox_Any, pt);
									}
									else
									{
										il.Emit(OpCodes.Castclass, pt);
									}
									il.Emit(OpCodes.Stloc, idx[i]);
								}
								LocalBuilder lb = il.DeclareLocal(typeof(List<object>));
								il.Emit(OpCodes.Ldarg_0);
								il.Emit(OpCodes.Castclass, deltype);
								for (int i = 0; i < pars.Length; i++)
								{
									if (byRef)
									{
										il.Emit(OpCodes.Ldloca_S, idx[i]);
									}
									else
									{
										il.Emit(OpCodes.Ldloc_S, idx[i]);
									}
								}
								il.Emit(OpCodes.Callvirt, deltype.GetMethod("Invoke"));
								if (byRef)
								{
									il.Emit(OpCodes.Ldnull);
									il.Emit(OpCodes.Stloc, lb.LocalIndex);
								}
								else
								{
									LocalBuilder lr = il.DeclareLocal(typeof(IEnumerable));
									il.Emit(OpCodes.Stloc, lr.LocalIndex);
									il.Emit(OpCodes.Ldloc, lr.LocalIndex);
									System.Reflection.Emit.Label retrive = il.DefineLabel();
									il.Emit(OpCodes.Ldnull);
									il.Emit(OpCodes.Ceq);
									il.Emit(OpCodes.Brfalse_S, retrive);
									il.Emit(OpCodes.Ldnull);
									il.Emit(OpCodes.Ret);
									il.MarkLabel(retrive);
									il.Emit(OpCodes.Newobj, typeof(List<object>).GetConstructor(new Type[0]));
									il.Emit(OpCodes.Stloc, lb.LocalIndex);
									LocalBuilder le = il.DeclareLocal(typeof(IEnumerator));
									LocalBuilder lo = il.DeclareLocal(typeof(object));
									il.Emit(OpCodes.Ldloc, lr.LocalIndex);
									il.Emit(OpCodes.Callvirt, typeof(IEnumerable).GetMethod("GetEnumerator"));
									il.Emit(OpCodes.Stloc, le.LocalIndex);
									System.Reflection.Emit.Label next = il.DefineLabel();
									il.Emit(OpCodes.Br_S, next);
									System.Reflection.Emit.Label load = il.DefineLabel();
									il.MarkLabel(load);
									il.Emit(OpCodes.Ldloc, le.LocalIndex);
									il.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetProperty("Current").GetGetMethod());
									il.Emit(OpCodes.Stloc, lo.LocalIndex);
									il.Emit(OpCodes.Ldloc, lb.LocalIndex);
									il.Emit(OpCodes.Ldloc, lo.LocalIndex);
									il.Emit(OpCodes.Callvirt, typeof(List<object>).GetMethod("Add", new Type[] { typeof(object) }));
									il.MarkLabel(next);
									il.Emit(OpCodes.Ldloc, le.LocalIndex);
									il.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetMethod("MoveNext"));
									il.Emit(OpCodes.Brtrue_S, load);
								}
								if (byRef)
								{
									for (int i = 0; i < pars.Length; i++)
									{
										Type pt = pars[i].ParameterType.GetElementType();
										il.Emit(OpCodes.Ldarg_1);
										il.Emit(OpCodes.Ldc_I4, i);
										il.Emit(OpCodes.Ldloc, idx[i]);
										if (pt.IsValueType)
										{
											il.Emit(OpCodes.Box, pt);
										}
										il.Emit(OpCodes.Stelem_Ref);
									}
								}
								il.Emit(OpCodes.Ldloc, lb.LocalIndex);
								il.Emit(OpCodes.Ret);
								_InvokeDict[deltype] = _CustomMethod = (_InvokeDelegate)dm.CreateDelegate(typeof(_InvokeDelegate));
							}
						}
						finally
						{
							_InvokeLock.DowngradeFromWriterLock(ref c);
						}
					}
				}
				finally
				{
					_InvokeLock.ReleaseReaderLock();
				}
			}
		}

		/// <summary>
		/// Invokes the manual method if provided in the onstructor
		/// </summary>
		/// <param name="parameters">Query parameters</param>
		/// <returns>Either resultset or null, if the method is intended to override parameters only</returns>
		protected virtual List<object> InvokeDelegate(object[] parameters)
		{
			List<object> list = null;
			if (_Delegate != null)
			{
				ParameterInfo[] pi = _Delegate.Method.GetParameters();
				IBqlParameter[] pb = _Select.GetParameters();
				object[] vals = new object[pi.Length];
				if (parameters != null)
				{
					int j = 0;
					for (int i = 0; i < pb.Length && j < pi.Length; i++)
					{
						if (pb[i].IsArgument)
						{
							if (i < parameters.Length)
							{
								vals[j] = parameters[i];
							}
							else
							{
								vals[j] = null;
							}
							j++;
						}
					}
					int k = pb.Length;
					for (; j < vals.Length; j++)
					{
						if (k < parameters.Length)
						{
							vals[j] = parameters[k];
							k++;
						}
						else
						{
							vals[j] = null;
						}
					}
				}
				EnsureDelegate();
				list = _CustomMethod(_Delegate, vals);
				if (parameters != null)
				{
					int j = 0;
					for (int i = 0; i < pb.Length && j < pi.Length; i++)
					{
						if (pb[i].IsArgument)
						{
							if (i < parameters.Length)
							{
								parameters[i] = vals[j];
							}
							j++;
						}
					}
					int k = pb.Length;
					for (; j < vals.Length; j++)
					{
						if (k < parameters.Length)
						{
							parameters[k] = vals[j];
							k++;
						}
						else
						{
							break;
						}
					}
				}
			}
			return list;
		}

		protected delegate int compareDelegate(object a, object b);
		protected int CompareMethod(object a, object b, PXCache cache, string fieldName, bool descending, bool useExt, Type tableType, PXCollationComparer collationComparer)
		{
			if (tableType != null)
			{
				a = ((PXResult)a)[tableType];
				b = ((PXResult)b)[tableType];
			}
			object aVal;
			if (!useExt)
			{
				aVal = cache.GetValue(a, fieldName);
			}
			else
			{
				aVal = cache.GetValueExt(a, fieldName);
				if (aVal is PXFieldState)
				{
					aVal = ((PXFieldState)aVal).Value;
				}
			}
			if (!(aVal is IComparable))
			{
				if (descending)
				{
					return 1;
				}
				return -1;
			}
			object bVal;
			if (!useExt)
			{
				bVal = cache.GetValue(b, fieldName);
			}
			else
			{
				bVal = cache.GetValueExt(b, fieldName);
				if (bVal is PXFieldState)
				{
					bVal = ((PXFieldState)bVal).Value;
				}
			}
			int ret;
			if (aVal is string && bVal is String)
			{
				ret = collationComparer.Compare((string)aVal, (string)bVal);
			}
			else if (aVal is Guid && bVal is Guid)
			{
				ret = compareGuid((Guid)aVal, (Guid)bVal);
			}
			else
			{
				// TODO: need review
				try
				{
					ret = ((IComparable)aVal).CompareTo(bVal);
				}
				catch
				{
					if (descending)
					{
						return 1;
					}
					return -1;
				}
			}
			if (descending && ret != 0)
			{
				ret = -ret;
			}
			return ret;
		}

		protected delegate int searchDelegate(object a);
		protected int SearchMethod(object a, object bVal, PXCache cache, string fieldName, bool descending, bool useExt, Type tableType)
		{
			if (tableType != null)
			{
				a = ((PXResult)a)[tableType];
			}
			object aVal;
			if (!useExt)
			{
				aVal = cache.GetValue(a, fieldName);
			}
			else
			{
				aVal = cache.GetValueExt(a, fieldName);
				if (aVal is PXFieldState)
				{
					aVal = ((PXFieldState)aVal).Value;
				}
			}
			if (!(aVal is IComparable))
			{
				if (descending)
				{
					return 1;
				}
				return -1;
			}
			int ret;
			if (aVal is string && bVal is String)
			{
				ret = PXLocalesProvider.CollationComparer.Compare((string)aVal, (string)bVal);
			}
			else if (aVal is Guid && bVal is Guid)
			{
				ret = compareGuid((Guid)aVal, (Guid)bVal);
			}
			else
			{
				ret = ((IComparable)aVal).CompareTo(bVal);
			}
			if (descending && ret != 0)
			{
				ret = -ret;
			}
			return ret;
		}

		/// <summary>
		/// Compare two items
		/// </summary>
		/// <param name="a">First item, might be a dictionary</param>
		/// <param name="b">Second item, might be a dictionary</param>
		/// <param name="columns">Sort columns</param>
		/// <param name="descendings">Sort descendings</param>
		/// <returns>-1, 0, 1</returns>
		protected virtual int Compare(object a, object b, compareDelegate[] comparisons)
		{
			if (Object.ReferenceEquals(a, b))
			{
				return 0;
			}
			for (int i = 0; i < comparisons.Length; i++)
			{
				int result = comparisons[i](a, b);
				if (result != 0)
				{
					return result;
				}
			}
			return 0;
		}

		protected virtual int Search(object a, searchDelegate[] comparisons)
		{
			for (int i = 0; i < comparisons.Length; i++)
			{
				int result = comparisons[i](a);
				if (result != 0)
				{
					return result;
				}
			}
			return 0;
		}

		/// <summary>
		/// Gets all the records from the query
		/// </summary>
		/// <param name="parameters">Query parameters</param>
		/// <returns>The resultset</returns>
		public virtual List<object> SelectMulti(params object[] parameters)
		{
			int startRow = 0;
			int totalRows = 0;
			return Select(null, parameters, null, null, null, null, ref startRow, 0, ref totalRows);
		}

		/// <summary>
		/// Gets top single record from the query
		/// </summary>
		/// <param name="parameters">Query parameters</param>
		/// <returns>The resultset</returns>
		public virtual object SelectSingle(params object[] parameters)
		{
			int startRow = 0;
			int totalRows = 0;
			List<object> list = Select(null, parameters, null, null, null, null, ref startRow, 1, ref totalRows);
			if (list.Count == 0)
			{
				return null;
			}
			return list[0];
		}

		/// <summary>
		/// Gets top single record from the query
		/// </summary>
		/// <param name="currents">Current items to use for getting Current and Optional parameter values</param>
		/// <param name="parameters">Values to search the item by the default sort expression</param>
		/// <returns>The resultset</returns>
		public virtual object SelectSingleBound(object[] currents, params object[] parameters)
		{
			int startRow = 0;
			int totalRows = 0;
			List<object> list = Select(currents, parameters, null, null, null, null, ref startRow, 1, ref totalRows);
			if (list.Count == 0)
			{
				return null;
			}
			return list[0];
		}
		protected bool _IsCommandMutable;
		/// <summary>
		/// Gets all the records from the query
		/// </summary>
		/// <param name="currents">Current items to use for getting Current and Optional parameter values</param>
		/// <param name="parameters">Values to search the item by the default sort expression</param>
		/// <returns>The resultset</returns>
		public virtual List<object> SelectMultiBound(object[] currents, params object[] parameters)
		{
			int startRow = 0;
			int totalRows = 0;
			return Select(currents, parameters, null, null, null, null, ref startRow, 0, ref totalRows);
		}

		public void WhereAnd<TWhere>()
			where TWhere : IBqlWhere, new()
		{
			_SelectQueries = null;
			Clear();
			_IsCommandMutable = true;
			_ParameterNames = null;
			_Select = _Select.WhereAnd<TWhere>();
		}
		public void WhereAnd(Type where)
		{
			_SelectQueries = null;
			Clear();
			_IsCommandMutable = true;
			_ParameterNames = null;
			_Select = _Select.WhereAnd(where);
		}
		public void WhereOr<TWhere>()
			where TWhere : IBqlWhere, new()
		{
			_SelectQueries = null;
			Clear();
			_IsCommandMutable = true;
			_ParameterNames = null;
			_Select = _Select.WhereOr<TWhere>();
		}
		public void WhereOr(Type where)
		{
			_SelectQueries = null;
			Clear();
			_IsCommandMutable = true;
			_ParameterNames = null;
			_Select = _Select.WhereOr(where);
		}
		public void OrderByNew<newOrderBy>()
			where newOrderBy : IBqlOrderBy, new()
		{
			_SelectQueries = null;
			Clear();
			_Select = _Select.OrderByNew<newOrderBy>();

			bool needOverrideSort;
			bool anySearch;
			bool resetTopCount = false;
			PXSearchColumn[] sorts = prepareSorts(null, null, null, 0, out needOverrideSort, out anySearch, ref resetTopCount);
			PXView.Sorts.Clear();
			PXView.Sorts.Add(sorts);
		}
		public void OrderByNew(Type newOrderBy)
		{
			_SelectQueries = null;
			Clear();
			_Select = _Select.OrderByNew(newOrderBy);

			bool needOverrideSort;
			bool anySearch;
			bool resetTopCount = false;
			PXSearchColumn[] sorts = prepareSorts(null, null, null, 0, out needOverrideSort, out anySearch, ref resetTopCount);
			PXView.Sorts.Clear();
			PXView.Sorts.Add(sorts);
		}
		public void Join<join>()
			where join : IBqlJoin, new()
		{
			_SelectQueries = null;
			Clear();
			_Select = BqlCommand.AppendJoin<join>(_Select);
		}
		public void Join(Type join)
		{
			_SelectQueries = null;
			Clear();
			_Select = (BqlCommand)Activator.CreateInstance(BqlCommand.AppendJoin(_Select.GetSelectType(), join));
		}
		public void WhereNot()
		{
			_SelectQueries = null;
			Clear();
			_IsCommandMutable = true;
			_ParameterNames = null;
			_Select = _Select.WhereNot();
		}
		public void WhereNew<newWhere>()
			where newWhere : IBqlWhere, new()
		{
			_SelectQueries = null;
			Clear();
			_IsCommandMutable = true;
			_ParameterNames = null;
			_Select = _Select.WhereNew<newWhere>();
		}
		public void WhereNew(Type newWhere)
		{
			_SelectQueries = null;
			Clear();
			_IsCommandMutable = true;
			_ParameterNames = null;
			_Select = _Select.WhereNew(newWhere);
		}

		public EventHandler RefreshRequested;
		protected void OnRefreshRequested(object sender, EventArgs e)
		{
			if (RefreshRequested != null)
			{
				RefreshRequested(sender, e);
			}
		}
		public void RequestRefresh()
		{
			OnRefreshRequested(this, new EventArgs());
		}

		public WebDialogResult Answer
		{
			get
			{
				return GetAnswer(string.Empty);
			}
			set
			{
				SetAnswer(string.Empty, value);
			}
		}

		public WebDialogResult GetAnswer(string key)
		{
			return DialogManager.GetAnswer(this, key);
		}

		public void SetAnswer(string key, WebDialogResult answer)
		{
			DialogManager.SetAnswer(this, key, answer);
		}

		public static void SetAnswer(PXGraph graph, string viewName, string key, WebDialogResult answer)
		{
			DialogManager.SetAnswer(graph, viewName, key, answer);
		}

		public void ClearDialog()
		{
			DialogManager.Clear(this.Graph);
		}

		public WebDialogResult Ask(string key, object row, string header, string message, MessageButtons buttons, MessageIcon icon, bool refreshRequired)
		{
			return DialogManager.Ask(this, key, row, header, message, buttons, icon, refreshRequired);
		}

		public WebDialogResult Ask(string key, object row, string header, string message, MessageButtons buttons, MessageIcon icon)
		{
			return Ask(key, row, header, message, buttons, icon, false);
		}

		public WebDialogResult Ask(object row, string header, string message, MessageButtons buttons, MessageIcon icon, bool refreshRequired)
		{
			return Ask(null, row, header, message, buttons, icon, refreshRequired);
		}

		public WebDialogResult Ask(object row, string header, string message, MessageButtons buttons, MessageIcon icon)
		{
			return Ask(null, row, header, message, buttons, icon);
		}

		public WebDialogResult Ask(string key, string message, MessageButtons buttons, bool refreshRequired)
		{
			return Ask(key, null, String.Empty, message, buttons, MessageIcon.None, refreshRequired);
		}

		public WebDialogResult Ask(string key, string message, MessageButtons buttons)
		{
			return Ask(key, null, String.Empty, message, buttons, MessageIcon.None);
		}

		public WebDialogResult Ask(string message, MessageButtons buttons, bool refreshRequired)
		{
			return Ask(null, null, String.Empty, message, buttons, MessageIcon.None, refreshRequired);
		}

		public WebDialogResult Ask(string message, MessageButtons buttons)
		{
			return Ask(null, null, String.Empty, message, buttons, MessageIcon.None);
		}

		public WebDialogResult Ask(string key, object row, string message, MessageButtons buttons, bool refreshRequired)
		{
			return Ask(key, row, String.Empty, message, buttons, MessageIcon.None, refreshRequired);
		}

		public WebDialogResult Ask(string key, object row, string message, MessageButtons buttons)
		{
			return Ask(key, row, String.Empty, message, buttons, MessageIcon.None);
		}

		public WebDialogResult Ask(object row, string message, MessageButtons buttons, bool refreshRequired)
		{
			return Ask(null, row, String.Empty, message, buttons, MessageIcon.None, refreshRequired);
		}

		public WebDialogResult Ask(object row, string message, MessageButtons buttons)
		{
			return Ask(null, row, String.Empty, message, buttons, MessageIcon.None);
		}

		public delegate void InitializePanel(PXGraph graph, string viewName);

		public WebDialogResult AskExt(string key, bool refreshRequired)
		{
			return DialogManager.AskExt(this, key, null, refreshRequired);
		}

		public WebDialogResult AskExt(string key)
		{
			return AskExt(key, false);
		}

		public WebDialogResult AskExt(bool refreshRequired)
		{
			return DialogManager.AskExt(this, null, null, refreshRequired);
		}

		public WebDialogResult AskExt()
		{
			return AskExt(false);
		}

		public WebDialogResult AskExt(string key, InitializePanel initializeHandler, bool refreshRequired)
		{
			return DialogManager.AskExt(this, key, new DialogManager.InitializePanel(initializeHandler), refreshRequired);
		}

		public WebDialogResult AskExt(string key, InitializePanel initializeHandler)
		{
			return AskExt(key, initializeHandler, false);
		}

		public WebDialogResult AskExt(InitializePanel initializeHandler, bool refreshRequired)
		{
			return DialogManager.AskExt(this, null, new DialogManager.InitializePanel(initializeHandler), refreshRequired);
		}

		public WebDialogResult AskExt(InitializePanel initializeHandler)
		{
			return AskExt(initializeHandler, false);
		}

		public static WebDialogResult AskExt(PXGraph graph, string viewName, string key, InitializePanel initializeHandler)
		{
			return DialogManager.AskExt(graph, viewName, key, new DialogManager.InitializePanel(initializeHandler), false);
		}

		public static object FieldGetValue(PXCache sender, object data, Type sourceType, string sourceField)
		{
			object newValue;
			PXCache sourceCache = sender.Graph.Caches[sourceType];

			if (InheritsType(sender.GetItemType(), sourceType))
			{
				newValue = sender.GetValue(data, sourceField);
				if (newValue == null)
				{
					if (sender.RaiseFieldDefaulting(sourceField, data, out newValue))
					{
						sender.RaiseFieldUpdating(sourceField, data, ref newValue);
					}
				}
			}
			else if ((sourceCache._Current ?? sourceCache.Current) == null)
			{
				object newRow = Activator.CreateInstance(sourceType);
				if (sourceCache.RaiseFieldDefaulting(sourceField, newRow, out newValue))
				{
					sourceCache.RaiseFieldUpdating(sourceField, newRow, ref newValue);
				}
			}
			else
			{
				newValue = sourceCache.GetValue((sourceCache._Current ?? sourceCache.Current), sourceField);
			}
			return newValue;
		}
		private static bool InheritsType(Type ChildType, Type BaseType)
		{
			while (ChildType != null && ChildType != BaseType)
			{
				ChildType = ChildType.BaseType;
			}
			return (ChildType == BaseType);
		}
	}

	public class PXViewParameter
	{
		public string Name;
		public int Ordinal;
		public IBqlParameter Bql;
		public ParameterInfo Argument;

	}
}

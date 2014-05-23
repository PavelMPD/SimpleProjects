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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Reflection.Emit;
using System.Data;
using System.Web.SessionState;
using System.Web;
using PX.Api;
using PX.Common;

namespace PX.Data
{
	/// <summary>
	/// Cache that allows update/insert/delete operations.
	/// </summary>
    /// <typeparam name="TNode">The type of data rows contained in the cache.</typeparam>
	[System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
	[System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
	//[DebuggerDisplay("{ToString()}")]
    [DebuggerTypeProxy(typeof(PXCache<>.PXCacheDebugView))]
	public class PXCache<TNode> : PXCache
		where TNode : class, IBqlTable, new()
	{
		#region Events
		private delegate object memberwiseCloneDelegate(TNode item);
		private static memberwiseCloneDelegate memberwiseClone;
		internal override object _Clone(object item)
		{
			if (item is TNode)
			{
				if (memberwiseClone != null && _CreateExtensions == null)
				{
					return memberwiseClone((TNode)item);
				}
				return CreateCopy((TNode)item);
			}
			return null;
		}

		private void cloneRowSubscribers(Dictionary<object, object> clones, EventsRowAttr list)
		{
			if (_EventsRowAttr.RowSelecting != null)
			{
				list.RowSelecting = new IPXRowSelectingSubscriber[_EventsRowAttr.RowSelecting.Length];
				for (int i = 0; i < _EventsRowAttr.RowSelecting.Length; i++)
				{
					list.RowSelecting[i] = (IPXRowSelectingSubscriber)clones[_EventsRowAttr.RowSelecting[i]];
				}
			}
			if (_EventsRowAttr.RowSelected != null)
			{
				list.RowSelected = new IPXRowSelectedSubscriber[_EventsRowAttr.RowSelected.Length];
				for (int i = 0; i < _EventsRowAttr.RowSelected.Length; i++)
				{
					list.RowSelected[i] = (IPXRowSelectedSubscriber)clones[_EventsRowAttr.RowSelected[i]];
				}
			}
			if (_EventsRowAttr.RowInserting != null)
			{
				list.RowInserting = new IPXRowInsertingSubscriber[_EventsRowAttr.RowInserting.Length];
				for (int i = 0; i < _EventsRowAttr.RowInserting.Length; i++)
				{
					list.RowInserting[i] = (IPXRowInsertingSubscriber)clones[_EventsRowAttr.RowInserting[i]];
				}
			}
			if (_EventsRowAttr.RowInserted != null)
			{
				list.RowInserted = new IPXRowInsertedSubscriber[_EventsRowAttr.RowInserted.Length];
				for (int i = 0; i < _EventsRowAttr.RowInserted.Length; i++)
				{
					list.RowInserted[i] = (IPXRowInsertedSubscriber)clones[_EventsRowAttr.RowInserted[i]];
				}
			}
			if (_EventsRowAttr.RowUpdating != null)
			{
				list.RowUpdating = new IPXRowUpdatingSubscriber[_EventsRowAttr.RowUpdating.Length];
				for (int i = 0; i < _EventsRowAttr.RowUpdating.Length; i++)
				{
					list.RowUpdating[i] = (IPXRowUpdatingSubscriber)clones[_EventsRowAttr.RowUpdating[i]];
				}
			}
			if (_EventsRowAttr.RowUpdated != null)
			{
				list.RowUpdated = new IPXRowUpdatedSubscriber[_EventsRowAttr.RowUpdated.Length];
				for (int i = 0; i < _EventsRowAttr.RowUpdated.Length; i++)
				{
					list.RowUpdated[i] = (IPXRowUpdatedSubscriber)clones[_EventsRowAttr.RowUpdated[i]];
				}
			}
			if (_EventsRowAttr.RowDeleting != null)
			{
				list.RowDeleting = new IPXRowDeletingSubscriber[_EventsRowAttr.RowDeleting.Length];
				for (int i = 0; i < _EventsRowAttr.RowDeleting.Length; i++)
				{
					list.RowDeleting[i] = (IPXRowDeletingSubscriber)clones[_EventsRowAttr.RowDeleting[i]];
				}
			}
			if (_EventsRowAttr.RowDeleted != null)
			{
				list.RowDeleted = new IPXRowDeletedSubscriber[_EventsRowAttr.RowDeleted.Length];
				for (int i = 0; i < _EventsRowAttr.RowDeleted.Length; i++)
				{
					list.RowDeleted[i] = (IPXRowDeletedSubscriber)clones[_EventsRowAttr.RowDeleted[i]];
				}
			}
			if (_EventsRowAttr.RowPersisting != null)
			{
				list.RowPersisting = new IPXRowPersistingSubscriber[_EventsRowAttr.RowPersisting.Length];
				for (int i = 0; i < _EventsRowAttr.RowPersisting.Length; i++)
				{
					list.RowPersisting[i] = (IPXRowPersistingSubscriber)clones[_EventsRowAttr.RowPersisting[i]];
				}
			}
			if (_EventsRowAttr.RowPersisted != null)
			{
				list.RowPersisted = new IPXRowPersistedSubscriber[_EventsRowAttr.RowPersisted.Length];
				for (int i = 0; i < _EventsRowAttr.RowPersisted.Length; i++)
				{
					list.RowPersisted[i] = (IPXRowPersistedSubscriber)clones[_EventsRowAttr.RowPersisted[i]];
				}
			}
		}
		private Dictionary<string, Subscriber[]> cloneFieldSubscribers<Subscriber>(Dictionary<object, object> clones, Dictionary<string, Subscriber[]> list)
			where Subscriber : class
		{
			Dictionary<string, Subscriber[]> ret = new Dictionary<string, Subscriber[]>(list.Count);
			foreach (KeyValuePair<string, Subscriber[]> p in list)
			{
				List<Subscriber> copy = new List<Subscriber>(p.Value.Length);
				for (int i = 0; i < p.Value.Length; i++)
				{
					var subscriber = p.Value[i];
					if (clones.ContainsKey(subscriber)) copy.Add((Subscriber)clones[subscriber]);
				}
				ret[p.Key] = copy.ToArray();
			}
			return ret;
		}

		private void AddAggregatedAttributes(ref Dictionary<object, object> clones, PXEventSubscriberAttribute attr, PXEventSubscriberAttribute clone)
		{
			clones.Add(attr, clone);
			if (clone is PXAggregateAttribute)
			{
				PXEventSubscriberAttribute[] oldaggr = ((PXAggregateAttribute)attr).GetAttributes();
				PXEventSubscriberAttribute[] newaggr = ((PXAggregateAttribute)clone).GetAttributes();
				for (int k = 0; k < oldaggr.Length; k++)
				{
					AddAggregatedAttributes(ref clones, oldaggr[k], newaggr[k]);
				}
			}
		}

		public override bool HasAttributes(object data)
		{
			if (_ItemAttributes != null && data is TNode)
			{
				return _ItemAttributes.ContainsKey((TNode)data);
			}
			return false;
		}

		public override List<PXEventSubscriberAttribute> GetAttributesReadonly(string name)
		{
			return GetAttributesReadonly(name, true);
		}

		public override List<PXEventSubscriberAttribute> GetAttributesReadonly(string name, bool extractEmmbeddedAttr)
		{
			List<PXEventSubscriberAttribute> ret = new List<PXEventSubscriberAttribute>();
			int idx;
			if (name == null)
			{
				foreach (PXEventSubscriberAttribute attr in _CacheAttributes)
				{
					ret.Add(attr);
					if(extractEmmbeddedAttr)
						extractEmbeded(ret, attr);
				}
			}
			else if (_FieldsMap.TryGetValue(name, out idx))
			{
				for (int i = _AttributesFirst[idx]; i <= _AttributesLast[idx]; i++)
				{
					ret.Add(_CacheAttributes[i]);
					if(extractEmmbeddedAttr)
						extractEmbeded(ret, _CacheAttributes[i]);
				}
			}
			return ret;
		}

		public override List<PXEventSubscriberAttribute> GetAttributesReadonly(object data, string name)
		{
			if (data == null || _ItemAttributes == null || !_ItemAttributes.ContainsKey((TNode)data))
			{
				return GetAttributesReadonly(name);
			}
			List<PXEventSubscriberAttribute> ret = new List<PXEventSubscriberAttribute>();
			int idx;
			if (name == null)
			{
				foreach (PXEventSubscriberAttribute attr in _ItemAttributes[(TNode)data])
				{
					ret.Add(attr);
					extractEmbeded(ret, attr);
				}
			}
			else if (_FieldsMap.TryGetValue(name, out idx))
			{
				for (int i = _AttributesFirst[idx]; i <= _AttributesLast[idx]; i++)
				{
					ret.Add(_ItemAttributes[(TNode)data][i]);
					extractEmbeded(ret, _ItemAttributes[(TNode)data][i]);
				}
			}
			return ret;
		}

		public override List<PXEventSubscriberAttribute> GetAttributes(object data, string name)
		{
			if (data == null)
			{
				return GetAttributes(name);
			}
			if (_ItemAttributes == null || !_ItemAttributes.ContainsKey((TNode)data))
			{
				if (_ItemAttributes == null)
				{
					_ItemAttributes = new Dictionary<TNode, List<PXEventSubscriberAttribute>>();
				}
				Dictionary<object, object> clones = new Dictionary<object, object>(_CacheAttributes.Count);
				List<PXEventSubscriberAttribute> descrlist = new List<PXEventSubscriberAttribute>(_CacheAttributes.Count);
				_ItemAttributes.Add((TNode)data, descrlist);
				if (_PendingItems != null)
				{
					_PendingItems.Add((TNode)data);
				}
				foreach (PXEventSubscriberAttribute descr in _CacheAttributes)
				{
					PXEventSubscriberAttribute attr = descr.Clone(PXAttributeLevel.Item);
					descrlist.Add(attr);
					AddAggregatedAttributes(ref clones, descr, attr);
				}
				if (_CommandPreparingEventsAttr != null)
				{
					if (_CommandPreparingEventsItem == null)
					{
						_CommandPreparingEventsItem = new Dictionary<object, Dictionary<string, IPXCommandPreparingSubscriber[]>>();
					}
					_CommandPreparingEventsItem[data] = cloneFieldSubscribers<IPXCommandPreparingSubscriber>(clones, _CommandPreparingEventsAttr);
				}
				if (_FieldDefaultingEventsAttr != null)
				{
					if (_FieldDefaultingEventsItem == null)
					{
						_FieldDefaultingEventsItem = new Dictionary<object, Dictionary<string, IPXFieldDefaultingSubscriber[]>>();
					}
					_FieldDefaultingEventsItem[data] = cloneFieldSubscribers<IPXFieldDefaultingSubscriber>(clones, _FieldDefaultingEventsAttr);
				}
				if (_FieldUpdatingEventsAttr != null)
				{
					if (_FieldUpdatingEventsItem == null)
					{
						_FieldUpdatingEventsItem = new Dictionary<object, Dictionary<string, IPXFieldUpdatingSubscriber[]>>();
					}
					_FieldUpdatingEventsItem[data] = cloneFieldSubscribers<IPXFieldUpdatingSubscriber>(clones, _FieldUpdatingEventsAttr);
				}
				if (_FieldVerifyingEventsAttr != null)
				{
					if (_FieldVerifyingEventsItem == null)
					{
						_FieldVerifyingEventsItem = new Dictionary<object, Dictionary<string, IPXFieldVerifyingSubscriber[]>>();
					}
					_FieldVerifyingEventsItem[data] = cloneFieldSubscribers<IPXFieldVerifyingSubscriber>(clones, _FieldVerifyingEventsAttr);
				}
				if (_FieldUpdatedEventsAttr != null)
				{
					if (_FieldUpdatedEventsItem == null)
					{
						_FieldUpdatedEventsItem = new Dictionary<object, Dictionary<string, IPXFieldUpdatedSubscriber[]>>();
					}
					_FieldUpdatedEventsItem[data] = cloneFieldSubscribers<IPXFieldUpdatedSubscriber>(clones, _FieldUpdatedEventsAttr);
				}
				if (_FieldSelectingEventsAttr != null)
				{
					if (_FieldSelectingEventsItem == null)
					{
						_FieldSelectingEventsItem = new Dictionary<object, Dictionary<string, IPXFieldSelectingSubscriber[]>>();
					}
					_FieldSelectingEventsItem[data] = cloneFieldSubscribers<IPXFieldSelectingSubscriber>(clones, _FieldSelectingEventsAttr);
				}
				if (_ExceptionHandlingEventsAttr != null)
				{
					if (_ExceptionHandlingEventsItem == null)
					{
						_ExceptionHandlingEventsItem = new Dictionary<object, Dictionary<string, IPXExceptionHandlingSubscriber[]>>();
					}
					_ExceptionHandlingEventsItem[data] = cloneFieldSubscribers<IPXExceptionHandlingSubscriber>(clones, _ExceptionHandlingEventsAttr);
				}
				EventsRowAttr dellist = new EventsRowAttr();
				cloneRowSubscribers(clones, dellist);
				if (_EventsRowItem == null)
				{
					_EventsRowItem = new Dictionary<object, EventsRowAttr>();
				}
				_EventsRowItem[data] = dellist;
			}
			List<PXEventSubscriberAttribute> ret = new List<PXEventSubscriberAttribute>();
			int idx;
			if (name == null)
			{
				foreach (PXEventSubscriberAttribute attr in _ItemAttributes[(TNode)data])
				{
					ret.Add(attr);
					extractEmbeded(ret, attr);
				}
			}
			else if (_FieldsMap.TryGetValue(name, out idx))
			{
				for (int i = _AttributesFirst[idx]; i <= _AttributesLast[idx]; i++)
				{
					ret.Add(_ItemAttributes[(TNode)data][i]);
					extractEmbeded(ret, _ItemAttributes[(TNode)data][i]);
				}
			}
			return ret;
		}
		internal override List<Type> GetExtensionTables()
		{
			return _ExtensionTables;
		}
		private void extractEmbeded(List<PXEventSubscriberAttribute> list, PXEventSubscriberAttribute attr)
		{
			if (attr is PXAggregateAttribute)
			{
				foreach (PXEventSubscriberAttribute embeded in ((PXAggregateAttribute)attr).GetAttributes())
				{
					list.Add(embeded);
					extractEmbeded(list, embeded);
				}
			}
		}
		public override List<PXEventSubscriberAttribute> GetAttributes(string name)
		{
			List<PXEventSubscriberAttribute> ret = new List<PXEventSubscriberAttribute>();
			int idx;
			if (name == null)
			{
				foreach (PXEventSubscriberAttribute attr in _CacheAttributes)
				{
					ret.Add(attr);
					extractEmbeded(ret, attr);
				}
			}
			else if (_FieldsMap.TryGetValue(name, out idx))
			{
				for (int i = _AttributesFirst[idx]; i <= _AttributesLast[idx]; i++)
				{
					ret.Add(_CacheAttributes[i]);
					extractEmbeded(ret, _CacheAttributes[i]);
				}
			}
			if (_ItemAttributes != null)
			{
				foreach (List<PXEventSubscriberAttribute> dict in _ItemAttributes.Values)
				{
					if (name == null)
					{
						foreach (PXEventSubscriberAttribute attr in dict)
						{
							ret.Add(attr);
							extractEmbeded(ret, attr);
						}
					}
					else if (_FieldsMap.TryGetValue(name, out idx))
					{
						for (int i = _AttributesFirst[idx]; i <= _AttributesLast[idx]; i++)
						{
							ret.Add(dict[i]);
							extractEmbeded(ret, dict[i]);
						}
					}
				}
			}
			return ret;
		}
		#endregion

		#region Field Manipulation

		protected class CacheStaticInfo : CacheStaticInfoBase
		{
			public CreateExtensionsDelegate _CreateExtensions;
			public SetValueByOrdinalDelegate _SetValueByOrdinal;
			public GetValueByOrdinalDelegate _GetValueByOrdinal;
			public System.Threading.ReaderWriterLock _DelegatesLock;
			public Dictionary<string, GetHashCodeDelegate> _DelegatesGetHashCode;
			public Dictionary<string, EqualsDelegate> _DelegatesEquals;
		}

		protected delegate PXCacheExtension[] CreateExtensionsDelegate(TNode data);
		private CreateExtensionsDelegate _CreateExtensions;
		private List<Type> _ExtensionTables;
		private Type[] _ExtensionTypes;

		protected delegate void SetValueByOrdinalDelegate(TNode data, int ordinal, object value, PXCacheExtension[] extensions);
		private SetValueByOrdinalDelegate _SetValueByOrdinal;
		private TNode _LastAccessedNode;
		private PXCacheExtension[] _LastAccessedExtensions;
		public override void SetValue(object data, int ordinal, object value)
		{
			TNode node;
			if (_LastAccessedNode != null && object.ReferenceEquals(_LastAccessedNode, data))
			{
				_SetValueByOrdinal(_LastAccessedNode, ordinal, value, _LastAccessedExtensions);
			}
			else if ((node = data as TNode) != null)
			{
				_LastAccessedNode = node;
				if (_Extensions == null)
				{
					_SetValueByOrdinal(node, ordinal, value, null);
				}
				else
				{
					PXCacheExtension[] extensions;
					lock (((ICollection)_Extensions).SyncRoot)
					{
						if (!_Extensions.TryGetValue(node, out extensions))
						{
							_Extensions[node] = extensions = _CreateExtensions(node);
						}
					}
					_LastAccessedExtensions = extensions;
					_SetValueByOrdinal(node, ordinal, value, extensions);
				}
			}
			else if (ordinal < _ClassFields.Count)
			{
				IDictionary dict = data as IDictionary;
				if (dict != null)
				{
					dict[_ClassFields[ordinal]] = value;
				}
			}
		}
		public override void SetValue(object data, string fieldName, object value)
		{
			TNode node;
			if (_LastAccessedNode != null && object.ReferenceEquals(_LastAccessedNode, data))
			{
				if (_FieldsMap.ContainsKey(fieldName))
				{
					_SetValueByOrdinal(_LastAccessedNode, _FieldsMap[fieldName], value, _LastAccessedExtensions);
				}
			}
			else if ((node = data as TNode) != null)
			{
				if (_FieldsMap.ContainsKey(fieldName))
				{
					_LastAccessedNode = node;
					if (_Extensions == null)
					{
						_SetValueByOrdinal(node, _FieldsMap[fieldName], value, null);
					}
					else
					{
						PXCacheExtension[] extensions;
						lock (((ICollection)_Extensions).SyncRoot)
						{
							if (!_Extensions.TryGetValue(node, out extensions))
							{
								_Extensions[node] = extensions = _CreateExtensions(node);
							}
						}
						_LastAccessedExtensions = extensions;
						_SetValueByOrdinal(node, _FieldsMap[fieldName], value, extensions);
					}
				}
			}
			else
			{
				IDictionary dict = data as IDictionary;
				if (dict != null)
				{
					if (!dict.Contains(fieldName))
					{
						string key = CompareIgnoreCase.GetCollectionKey(dict.Keys, fieldName);
						if (key != null)
						{
							fieldName = key;
						}
					}
					dict[fieldName] = value;
				}
			}
		}

		internal override object GetCopy(object data)
		{
			object newitem;
			return GetCopy(data, out newitem);
		}

		protected virtual object GetCopy(object data, out object pending)
		{
			object copy = null;
			pending = null;
			if (_PendingValues != null && data is TNode && _PendingValues.TryGetValue((TNode)data, out pending))
			{
				foreach (TNode key in _PendingValues.Keys)
				{
					//try to find copy in _PendingValues[copy]
					if (!object.ReferenceEquals(key, data) && object.ReferenceEquals(_PendingValues[key], pending))
					{
						copy = key;
						break;
					}
				}
			}
			return copy;
		}

		public override void SetDefaultExt(object data, string fieldName)
		{
			bool externalCall = _PendingValues != null && data is TNode && _PendingValues.ContainsKey((TNode)data) && _PendingValues[(TNode)data] is IDictionary;
			object pendingval;

			object newitem = null;
			object copy = null;
			if (!externalCall) 
			{
				copy = GetCopy(data, out newitem);
			}

			if (Fields.Contains(fieldName) && (externalCall || (pendingval = GetValuePending(data, fieldName)) == null || copy != null && OnFieldUpdating(fieldName, newitem, ref pendingval) && object.Equals(GetValue(copy, fieldName), pendingval))) 
			{
				object value = null;
				try
				{
					if (data is PXResult)
					{
						data = ((PXResult)data)[0];
					}
					if (OnFieldDefaulting(fieldName, data, out value))
					{
						OnFieldUpdating(fieldName, data, ref value);
					}
					OnFieldVerifying(fieldName, data, ref value, externalCall);
					object oldValue = null;
					TNode node;
					if (_LastAccessedNode != null && object.ReferenceEquals(_LastAccessedNode, data))
					{
						int ordinal;
						if (!_FieldsMap.TryGetValue(fieldName, out ordinal))
						{
							return;
						}
						oldValue = _GetValueByOrdinal(_LastAccessedNode, ordinal, _LastAccessedExtensions);
						_SetValueByOrdinal(_LastAccessedNode, ordinal, value, _LastAccessedExtensions);
					}
					else if ((node = data as TNode) != null)
					{
						_LastAccessedNode = node;
						int ordinal;
						if (!_FieldsMap.TryGetValue(fieldName, out ordinal))
						{
							return;
						}
						if (_Extensions == null)
						{
							oldValue = _GetValueByOrdinal(node, ordinal, null);
							_SetValueByOrdinal(node, ordinal, value, null);
						}
						else
						{
							PXCacheExtension[] extensions;
							lock (((ICollection)_Extensions).SyncRoot)
							{
								if (!_Extensions.TryGetValue(node, out extensions))
								{
									_Extensions[node] = extensions = _CreateExtensions(node);
								}
							}
							_LastAccessedExtensions = extensions;
							oldValue = _GetValueByOrdinal(node, ordinal, extensions);
							_SetValueByOrdinal(node, ordinal, value, extensions);
						}
					}
					else
					{
						IDictionary dict = data as IDictionary;
						if (dict != null)
						{
							if (dict.Contains(fieldName))
							{
								oldValue = dict[fieldName];
							}
							else
							{
								string key = CompareIgnoreCase.GetCollectionKey(dict.Keys, fieldName);
								if (key != null)
								{
									oldValue = dict[key];
									fieldName = key;
								}
							}
							dict[fieldName] = value;
						}
					}
					OnFieldUpdated(fieldName, data, oldValue, externalCall);
				}
				catch (PXSetPropertyException e)
				{
					if (OnExceptionHandling(fieldName, data, value, e))
					{
						throw;
					}
					PXTrace.WriteWarning(e);
				}
			}
		}
		public override void SetValueExt(object data, string fieldName, object value)
		{
			if (Fields.Contains(fieldName))
			{
				try
				{
					if (data is PXResult)
					{
						data = ((PXResult)data)[0];
					}
					OnFieldUpdating(fieldName, data, ref value);
                    bool externalCall = _PendingValues != null && data is TNode && _PendingValues.ContainsKey((TNode)data) && _PendingValues[(TNode)data] is IDictionary;
					OnFieldVerifying(fieldName, data, ref value, externalCall);
					object oldValue = null;
					TNode node;
					if (_LastAccessedNode != null && object.ReferenceEquals(_LastAccessedNode, data))
					{
						int ordinal;
						if (!_FieldsMap.TryGetValue(fieldName, out ordinal))
						{
							return;
						}
						oldValue = _GetValueByOrdinal(_LastAccessedNode, ordinal, _LastAccessedExtensions);
						_SetValueByOrdinal(_LastAccessedNode, ordinal, value, _LastAccessedExtensions);
					}
					else if ((node = data as TNode) != null)
					{
						_LastAccessedNode = node;
						int ordinal;
						if (!_FieldsMap.TryGetValue(fieldName, out ordinal))
						{
							return;
						}
						if (_Extensions == null)
						{
							oldValue = _GetValueByOrdinal(node, ordinal, null);
							_SetValueByOrdinal(node, ordinal, value, null);
						}
						else
						{
							PXCacheExtension[] extensions;
							lock (((ICollection)_Extensions).SyncRoot)
							{
								if (!_Extensions.TryGetValue(node, out extensions))
								{
									_Extensions[node] = extensions = _CreateExtensions(node);
								}
							}
							_LastAccessedExtensions = extensions;
							oldValue = _GetValueByOrdinal(node, ordinal, extensions);
							_SetValueByOrdinal(node, ordinal, value, extensions);
						}
					}
					else
					{
						IDictionary dict = data as IDictionary;
						if (dict != null)
						{
							if (dict.Contains(fieldName))
							{
								oldValue = dict[fieldName];
							}
							else
							{
								string key = CompareIgnoreCase.GetCollectionKey(dict.Keys, fieldName);
								if (key != null)
								{
									oldValue = dict[key];
									fieldName = key;
								}
							}
							dict[fieldName] = value;
						}
					}
					OnFieldUpdated(fieldName, data, oldValue, externalCall);
				}
				catch (PXSetPropertyException e)
				{
					if (OnExceptionHandling(fieldName, data, value, e))
					{
						throw;
					}
					PXTrace.WriteWarning(e);
				}
			}
		}
		protected delegate object GetValueByOrdinalDelegate(TNode data, int ordinal, PXCacheExtension[] extensions);
		private GetValueByOrdinalDelegate _GetValueByOrdinal;
		public override object GetValue(object data, int ordinal)
		{
			TNode node;
			if (_LastAccessedNode != null && object.ReferenceEquals(_LastAccessedNode, data))
			{
				return _GetValueByOrdinal(_LastAccessedNode, ordinal, _LastAccessedExtensions);
			}
			else if ((node = data as TNode) != null)
			{
				_LastAccessedNode = node;
				if (_Extensions == null)
				{
					return _GetValueByOrdinal(node, ordinal, null);
				}
				else
				{
					PXCacheExtension[] extensions;
					lock (((ICollection)_Extensions).SyncRoot)
					{
						if (!_Extensions.TryGetValue(node, out extensions))
						{
							_Extensions[node] = extensions = _CreateExtensions(node);
						}
					}
					_LastAccessedExtensions = extensions;
					return _GetValueByOrdinal(node, ordinal, extensions);
				}
			}
			else if (ordinal < _ClassFields.Count)
			{
				IDictionary dict = data as IDictionary;
				if (dict != null)
				{
					string fieldName = _ClassFields[ordinal];
					if (dict.Contains(fieldName))
					{
						return dict[fieldName];
					}
					else
					{
						string key = CompareIgnoreCase.GetCollectionKey(dict.Keys, fieldName);
						if (key != null)
						{
							return dict[key];
						}
					}
				}
			}
			return null;
		}
		public override object GetValue(object data, string fieldName)
		{
			int ordinal;
			if (_FieldsMap.TryGetValue(fieldName, out ordinal))
			{
				TNode node;
				if (_LastAccessedNode != null && object.ReferenceEquals(_LastAccessedNode, data))
				{
					return _GetValueByOrdinal(_LastAccessedNode, ordinal, _LastAccessedExtensions);
				}
				if ((node = data as TNode) != null)
				{
					_LastAccessedNode = node;
					if (_Extensions == null)
					{
						return _GetValueByOrdinal(node, ordinal, null);
					}
					else
					{
						PXCacheExtension[] extensions;
						lock (((ICollection)_Extensions).SyncRoot)
						{
							if (!_Extensions.TryGetValue(node, out extensions))
							{
								_Extensions[node] = extensions = _CreateExtensions(node);
							}
						}
						_LastAccessedExtensions = extensions;
						return _GetValueByOrdinal(node, ordinal, extensions);
					}
				}
				else
				{
					IDictionary dict = data as IDictionary;
					if (dict != null)
					{
						if (dict.Contains(fieldName))
						{
							return dict[fieldName];
						}
						else
						{
							string key = CompareIgnoreCase.GetCollectionKey(dict.Keys, fieldName);
							if (key != null)
							{
								return dict[key];
							}
						}
					}
				}
			}
			else if (fieldName != null)
			{
				int pos = fieldName.IndexOf('.');
				if (pos > 0 && pos < fieldName.Length - 1)
				{
					fieldName = fieldName.Substring(pos + 1);
					if (_FieldsMap.ContainsKey(fieldName))
					{
						TNode node;
						if (_LastAccessedNode != null && object.ReferenceEquals(_LastAccessedNode, data))
						{
							return _GetValueByOrdinal(_LastAccessedNode, _FieldsMap[fieldName], _LastAccessedExtensions);
						}
						if ((node = data as TNode) != null)
						{
							_LastAccessedNode = node;
							if (_Extensions == null)
							{
								return _GetValueByOrdinal(node, _FieldsMap[fieldName], null);
							}
							else
							{
								PXCacheExtension[] extensions;
								lock (((ICollection)_Extensions).SyncRoot)
								{
									if (!_Extensions.TryGetValue(node, out extensions))
									{
										_Extensions[node] = extensions = _CreateExtensions(node);
									}
								}
								_LastAccessedExtensions = extensions;
								return _GetValueByOrdinal(node, _FieldsMap[fieldName], extensions);
							}
						}
						else
						{
							IDictionary dict = data as IDictionary;
							if (dict != null)
							{
								if (dict.Contains(fieldName))
								{
									return dict[fieldName];
								}
								else
								{
									string key = CompareIgnoreCase.GetCollectionKey(dict.Keys, fieldName);
									if (key != null)
									{
										return dict[key];
									}
								}
							}
						}
					}
				}
			}
			return null;
		}
		public override object GetValueInt(object data, string fieldName)
		{
			return GetValueInt(data, fieldName, _AlteredFields != null && _AlteredFields.IndexOf(fieldName.ToLower()) >= 0, false);
		}
		public override object GetValueExt(object data, string fieldName)
		{
			return GetValueInt(data, fieldName, _AlteredFields != null && _AlteredFields.IndexOf(fieldName.ToLower()) >= 0, true);
		}
		public override object GetStateExt(object data, string fieldName)
		{
			return GetValueInt(data, fieldName, true, true);
		}
		public override object GetValueOriginal(object data, string fieldName)
		{
			object original = GetOriginal(data);
			if (original != null)
			{
				return GetValue(original, fieldName);
			}
			return null;
		}
		internal override object GetOriginal(object data)
		{
			TNode item = data as TNode;
			if (item != null)
			{
				BqlTablePair orig = null;
				try
				{
					lock (((ICollection)_Originals).SyncRoot)
					{
						if (!_Originals.TryGetValue(item, out orig))
						{
							TNode output;
							if ((output = readItem(item, true)) != null)
							{
								_Originals[item] = orig = new BqlTablePair { LastModified = CreateCopy(output), Unchanged = output };
							}
						}
					}
				}
				catch
				{
				}
				if (orig != null && orig.Unchanged != null)
				{
					return orig.Unchanged;
				}
			}
			return null;
		}
		protected Dictionary<TNode, object> _PendingValues;
        protected Dictionary<TNode, List<Exception>> _PendingExceptions;
		public override object GetValuePending(object data, string fieldName)
		{
			object vals;
			if (_PendingValues == null || !(data is TNode) || !_PendingValues.TryGetValue((TNode)data, out vals))
			{
				return null;
			}
			object ret = GetValueExt(vals, fieldName);
			if (ret is PXFieldState)
			{
				ret = ((PXFieldState)ret).Value;
			}
			else if (ret == null && vals is IDictionary)
			{
				ret = ((IDictionary)vals)[fieldName];
			}
			return ret;
		}
		public override void SetValuePending(object data, string fieldName, object value)
		{
			object vals;
			if (_PendingValues != null && data is TNode && _PendingValues.TryGetValue((TNode)data, out vals))
			{
				if (vals is TNode)
				{
					if (!object.ReferenceEquals(value, NotSetValue))
					{
						OnFieldUpdating(fieldName, data, ref value);
						SetValue(vals, fieldName, value);
					}
				}
				else
				{
                    IDictionary values = vals as IDictionary;
                    if (values == null || !values.Contains(PXImportAttribute.ImportFlag) || !object.ReferenceEquals(value, NotSetValue))
                    {
    					SetValue(vals, fieldName, value);
                    }
				}
			}
		}
		protected internal virtual object GetValueInt(object data, string fieldName, bool forceState, bool externalCall)
		{
			object returnValue = null;
            if(data == null)
            {

                OnFieldSelecting(fieldName, null, ref returnValue, forceState, true);
                return returnValue;

            }

		    int ordinal;
			if (_FieldsMap.TryGetValue(fieldName, out ordinal))
			{
				if (_LastAccessedNode != null && object.ReferenceEquals(_LastAccessedNode, data))
				{
					returnValue = _GetValueByOrdinal(_LastAccessedNode, ordinal, _LastAccessedExtensions);
					OnFieldSelecting(fieldName, _LastAccessedNode, ref returnValue, forceState, true);
					return returnValue;
				}
				if (data is IBqlTable)
				{
                    TNode node = (TNode)data;
					_LastAccessedNode = node;
					if (_Extensions == null)
					{
						returnValue = _GetValueByOrdinal(node, ordinal, null);
					}
					else
					{
						PXCacheExtension[] extensions;
						lock (((ICollection)_Extensions).SyncRoot)
						{
							if (!_Extensions.TryGetValue(node, out extensions))
							{
								_Extensions[node] = extensions = _CreateExtensions(node);
							}
						}
						_LastAccessedExtensions = extensions;
						returnValue = _GetValueByOrdinal(node, ordinal, extensions);
					}
                    OnFieldSelecting(fieldName, data as IBqlTable, ref returnValue, forceState, true);
                    return returnValue;
				}
			    IDictionary dict = (IDictionary)data;
			    if (dict.Contains(fieldName))
			    {
			        return dict[fieldName];
			    }
			    string key = CompareIgnoreCase.GetCollectionKey(dict.Keys, fieldName);
			    if (key != null)
			    {
			        return dict[key];
			    }
			    // OnFieldSelecting(fieldName, data, ref returnValue, forceState, true);
				return null;
			}
            else if (Fields.Contains(fieldName) && data is IBqlTable)
			{

                OnFieldSelecting(fieldName, data, ref returnValue, forceState, true);
				return returnValue;
			}
			return null;
		}

        /// <summary>
        /// Copy values from dictionary to item with event handling.<br/>
        /// for insert operation raise OnFieldDefaulting, OnFieldUpdating, OnFieldVerifying, OnFieldUpdated<br/>
        /// for update operation raise OnFieldUpdating, OnFieldVerifying, OnFieldUpdated<br/>
        /// for delete operation events raised for key fields, OnFieldUpdating, OnFieldUpdated
        /// returns key updated flag<br/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="copy"></param>
        /// <param name="values"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
		private bool FillWithValues(TNode item, TNode copy, IDictionary values, PXCacheOperation operation)
		{
			return FillWithValues(item, copy, values, operation, true);
		}
		private bool FillWithValues(TNode item, TNode copy, IDictionary values, PXCacheOperation operation, bool externalCall)
		{
			bool keyUpdated = false;
			bool skipFieldUpdated = copy == null && operation == PXCacheOperation.Update;
			if (operation == PXCacheOperation.Insert && copy == null)
			{
				copy = new TNode();
			}
			if (_PendingValues == null)
			{
				_PendingValues = new Dictionary<TNode, object>();
			}
			_PendingValues[item] = values;
			PXCacheExtension[] copyextensions = null;
			PXCacheExtension[] itemextensions = null;
			if (_Extensions != null)
			{
				lock (((ICollection)_Extensions).SyncRoot)
				{
					if (!_Extensions.TryGetValue(item, out itemextensions))
					{
						_Extensions[item] = itemextensions = _CreateExtensions(item);
					}
				}
			}
			if (copy != null)
			{
				_PendingValues[copy] = values;
				if (_Extensions != null)
				{
					lock (((ICollection)_Extensions).SyncRoot)
					{
						if (!_Extensions.TryGetValue(copy, out copyextensions))
						{
							_Extensions[copy] = copyextensions = _CreateExtensions(copy);
						}
					}
				}
			}
			else
			{
				copyextensions = null;
			}
			string descr = null;
			try
			{
				for (int i = 0; i < Fields.Count; i++)
				{
					descr = Fields[i];
					int ordinal;
					bool inClass = _FieldsMap.TryGetValue(descr, out ordinal);
					bool contains = values.Contains(descr);
					object newvalue = null;
					if (contains)
					{
						newvalue = values[descr];
						if (newvalue is PXFieldState)
						{
							newvalue = ((PXFieldState)newvalue).Value;
						}
					}
					if (inClass && (operation == PXCacheOperation.Insert /*|| contains && newvalue == null*/))
					{
						object value;
						if (operation != PXCacheOperation.Insert)
						{
							_SetValueByOrdinal(item, ordinal, null, itemextensions);
						}
						if (OnFieldDefaulting(descr, item, out value))
						{
							OnFieldUpdating(descr, item, ref value);
						}
						if (_GetValueByOrdinal(item, ordinal, itemextensions) == null)
						{
							_SetValueByOrdinal(item, ordinal, value, itemextensions);
						}
						if (copy != null)
						{
							object copyValue;
							if (OnFieldDefaulting(descr, copy, out copyValue))
							{
								OnFieldUpdating(descr, copy, ref copyValue);
							}
							_SetValueByOrdinal(copy, ordinal, copyValue, copyextensions);
						}
					}
					if (contains && (newvalue != null || operation != PXCacheOperation.Insert) && !object.ReferenceEquals(newvalue, NotSetValue))
					{
						try
						{
							bool isKeyAndUpdated = false;
							object copyValue = null;
							if (inClass && copy != null)
							{
								copyValue = _GetValueByOrdinal(copy, ordinal, copyextensions);
								if (_Keys.Contains(descr))
								{
									//if (copyValue != null && operation != PXCacheOperation.Insert)
									//{
									//    continue;
									//}
									if (newvalue != null)
									{
										isKeyAndUpdated = true;
									}
								}
							}
							Exception ex = null;
							bool skipSetValue = false;
							try
							{
								OnFieldUpdating(descr, item, ref newvalue);
							}
							catch (Exception outer)
							{
								skipSetValue = true;
								if (copy != null)
								{
									ex = outer;
									try
									{
										OnFieldUpdating(descr, copy, ref newvalue);
									}
									catch (Exception)
									{
										throw outer;
									}
								}
							}
							if (copy != null && Object.Equals(newvalue, copyValue))
							{
								continue;
							}
							if (isKeyAndUpdated)
							{
								keyUpdated = true;
								}
							if (ex != null)
							{
								throw ex;
							}
							if (operation != PXCacheOperation.Delete)
							{
								OnFieldVerifying(descr, item, ref newvalue, externalCall);
							}
							if (!inClass || copy != null && Object.Equals(newvalue, copyValue))
							{
								continue;
							}
							if (!skipSetValue)
							{
								object oldValue = _GetValueByOrdinal(item, ordinal, itemextensions);
								_SetValueByOrdinal(item, ordinal, newvalue, itemextensions);
								if (!skipFieldUpdated)
								{
									OnFieldUpdated(descr, item, oldValue, externalCall);
								}
							}
						}
						catch (PXSetPropertyException e)
						{
							if (operation == PXCacheOperation.Insert
								&& _Keys.Contains(descr)
								|| OnExceptionHandling(descr, item, newvalue, e))
							{
								throw;
							}
							PXTrace.WriteWarning(e);
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (String.IsNullOrEmpty(descr))
				{
					throw;
				}
				if (ex is PXSetPropertyException)
				{
					string dispname = PXUIFieldAttribute.GetDisplayName(this, descr);
					string errortext = ex.Message;

					if (dispname != null && descr != dispname)
					{
						int fid = errortext.IndexOf(descr, StringComparison.OrdinalIgnoreCase);
						if (fid >= 0)
						{
							errortext = errortext.Remove(fid, descr.Length).Insert(fid, dispname);
						}
					}
					else
					{
						dispname = descr;
					}

					if (ex is PXSetupNotEnteredException)
					{
						throw;
					}

					throw new PXFieldProcessingException(descr, ex, ((PXSetPropertyException)ex).ErrorLevel, dispname, errortext);
				}
				if (ex is PXDialogRequiredException)
				{
					throw;
				}
				PXTrace.WriteWarning(ex);
				throw new PXException(ex, ErrorMessages.ErrorFieldProcessing, descr, ex.Message);
			}
			finally
			{
				_PendingValues.Remove(item);
				if (copy != null)
				{
					_PendingValues.Remove(copy);
				}
			}

			return keyUpdated;
		}

        /// <summary>
        /// Create new node, then assigns all default values, then copy non empty fields from item.<br/>
        /// If no exceptions, replace item with copy.
        /// </summary>
        /// <param name="item"></param>
		private void FillWithValues(ref TNode item)
		{
			TNode copy = new TNode();
			if (_PendingValues == null)
			{
				_PendingValues = new Dictionary<TNode, object>();
			}
			PXCacheExtension[] itemextensions = null;
			PXCacheExtension[] copyextensions = null;
			if (_Extensions != null)
			{
				lock (((ICollection)_Extensions).SyncRoot)
				{
					if (!_Extensions.TryGetValue(item, out itemextensions))
					{
						_Extensions[item] = itemextensions = _CreateExtensions(item);
					}
					_Extensions[copy] = copyextensions = _CreateExtensions(copy);
				}
			}
			_PendingValues[copy] = item;
			try
			{
				foreach (string descr in _ClassFields)
				{
					object itemValue = null;
					try
					{
						int ordinal = _FieldsMap[descr];
						object defValue;
						if (OnFieldDefaulting(descr, copy, out defValue))
						{
							OnFieldUpdating(descr, copy, ref defValue);
						}
						object copyValue = _GetValueByOrdinal(copy, ordinal, copyextensions);
						if (copyValue == null && defValue != null)
						{
							_SetValueByOrdinal(copy, ordinal, defValue, copyextensions);
							copyValue = defValue;
						}
						itemValue = _GetValueByOrdinal(item, ordinal, itemextensions);
						if (itemValue != null)
						{
							OnFieldVerifying(descr, copy, ref itemValue, false);
							_SetValueByOrdinal(copy, ordinal, itemValue, copyextensions);
							OnFieldUpdated(descr, copy, copyValue, false);
						}
					}
					catch (PXSetupNotEnteredException)
					{
						throw;
					}
					catch (PXSetPropertyException ex)
					{
						string dispname = PXUIFieldAttribute.GetDisplayName(this, descr);
						string errortext = ex.Message;

						if (dispname != null && descr != dispname)
						{
							int fid = errortext.IndexOf(descr, StringComparison.OrdinalIgnoreCase);
							if (fid >= 0)
							{
								errortext = errortext.Remove(fid, descr.Length).Insert(fid, dispname);
							}
						}
						else
						{
							dispname = descr;
						}

						if (itemValue == null)
						{
							ex = new PXFieldProcessingException(descr, ex, ((PXSetPropertyException)ex).ErrorLevel, dispname, errortext);
						}
						else
						{
							object fs = GetStateExt(null, descr);

							if (itemValue is string && fs is PXStringState && !string.IsNullOrEmpty(((PXStringState)fs).InputMask))
							{
								itemValue = PX.Common.Mask.Format(((PXStringState)fs).InputMask, (string)itemValue);
							}

							ex = new PXFieldValueProcessingException(descr, ex, ((PXSetPropertyException)ex).ErrorLevel, dispname, itemValue, errortext);
						}
						if (_Keys.Contains(descr))
						{
							throw ex;
						}
                        List<Exception> pending;
                        if (!_PendingExceptions.TryGetValue(copy, out pending))
                        {
                            _PendingExceptions[copy] = pending = new List<Exception>();
                        }
                        pending.Add(ex);
					}
					catch (PXDialogRequiredException)
					{
						throw;
					}
					catch (Exception ex)
					{
						PXTrace.WriteWarning(ex);
						throw new PXException(ex, ErrorMessages.ErrorFieldProcessing, descr, ex.Message);
					}
				}
				item = copy;
			}
			finally
			{
				_PendingValues.Remove(copy);
			}
		}

		private void FillWithValues(TNode item, TNode copy, TNode newitem)
		{
			if (_PendingValues == null)
			{
				_PendingValues = new Dictionary<TNode, object>();
			}
			_PendingValues[item] = newitem;
            if (copy != null)
            {
                _PendingValues[copy] = newitem;
            }
			PXCacheExtension[] itemextensions = null;
			PXCacheExtension[] newitemextensions = null;
			PXCacheExtension[] copyextensions = null;
			if (_Extensions != null)
			{
				lock (((ICollection)_Extensions).SyncRoot)
				{
					if (!_Extensions.TryGetValue(item, out itemextensions))
					{
						_Extensions[item] = itemextensions = _CreateExtensions(item);
					}
					if (!_Extensions.TryGetValue(newitem, out newitemextensions))
					{
						_Extensions[newitem] = newitemextensions = _CreateExtensions(newitem);
					}
					if (!_Extensions.TryGetValue(copy, out copyextensions))
					{
						_Extensions[copy] = copyextensions = _CreateExtensions(copy);
					}
				}
			}
			try
			{
				foreach (string descr in _ClassFields)
				{
					object newvalue = null;
					try
					{
						int ordinal = _FieldsMap[descr];
						object oldvalue = _GetValueByOrdinal(copy, ordinal, copyextensions);
						newvalue = _GetValueByOrdinal(newitem, ordinal, newitemextensions);
						if (!object.Equals(oldvalue, newvalue))
						{
							OnFieldVerifying(descr, item, ref newvalue, false);
							if (!object.Equals(oldvalue, newvalue))
							{
								_SetValueByOrdinal(item, ordinal, newvalue, itemextensions);
								OnFieldUpdated(descr, item, oldvalue, false);
							}
						}
					}
					catch (Exception ex)
					{
						if (ex is PXSetPropertyException)
						{
							string dispname = PXUIFieldAttribute.GetDisplayName(this, descr);
							string errortext = ex.Message;

							if (dispname != null && descr != dispname)
							{
								int fid = errortext.IndexOf(descr, StringComparison.OrdinalIgnoreCase);
								if (fid >= 0)
								{
									errortext = errortext.Remove(fid, descr.Length).Insert(fid, dispname);
								}
							}
							else
							{
								dispname = descr;
							}

							if (newvalue == null)
							{
								throw new PXFieldProcessingException(descr, ex, ((PXSetPropertyException)ex).ErrorLevel, dispname, errortext);
							}
							else
							{
								object fs = GetStateExt(null, descr);

								if (newvalue is string && fs is PXStringState && !string.IsNullOrEmpty(((PXStringState)fs).InputMask))
								{
									newvalue = PX.Common.Mask.Format(((PXStringState)fs).InputMask, (string)newvalue);
								}

								throw new PXFieldValueProcessingException(descr, ex, ((PXSetPropertyException)ex).ErrorLevel, dispname, newvalue, errortext);
							}
						}
						if (ex is PXDialogRequiredException)
						{
							throw;
						}
						PXTrace.WriteWarning(ex);
						throw new PXException(ex, ErrorMessages.ErrorFieldProcessing, descr, ex.Message);
					}
				}
			}
			finally
			{
				_PendingValues.Remove(item);
				if (copy != null)
				{
					_PendingValues.Remove(copy);
				}
			}
		}
        /// <summary>
        /// Creates new node, clone field values
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
		public static TNode CreateCopy(TNode item)
		{
			CacheStaticInfo result = _Initialize(false);

			if (item.GetType() == typeof(TNode) && memberwiseClone != null && result._CreateExtensions == null)
			{
				return (TNode)memberwiseClone(item);
			}
			else
			{
				TNode copy = new TNode();
				if (result._CreateExtensions != null)
				{
					PXCacheExtensionCollection dict = PXContext.GetSlot<PXCacheExtensionCollection>();
					if (dict == null)
					{
						dict = PXContext.SetSlot<PXCacheExtensionCollection>(new PXCacheExtensionCollection());
					}
					PXCacheExtension[] copyextensions;
					PXCacheExtension[] itemextensions;

					lock (((ICollection)dict).SyncRoot)
					{
						dict[copy] = copyextensions = result._CreateExtensions(copy);
						if (!dict.TryGetValue(item, out itemextensions))
						{
							dict[item] = itemextensions = result._CreateExtensions(item);
						}
					}
					for (int i = 0; i < result._ClassFields.Count; i++)
					{
						result._SetValueByOrdinal(copy, i, result._GetValueByOrdinal(item, i, itemextensions), copyextensions);
					}
				}
				else
				{
					for (int i = 0; i < result._ClassFields.Count; i++)
					{
						result._SetValueByOrdinal(copy, i, result._GetValueByOrdinal(item, i, null), null);
					}
				}
				return copy;
			}
		}

		public override void RestoreCopy(object item, object copy)
		{
			if (item is TNode && copy is TNode)
			{
				RestoreCopy((TNode)item, (TNode)copy);
			}
		}

		public override object CreateCopy(object item)
		{
			if (item is TNode)
			{
				return CreateCopy((TNode)item);
			}
			return null;
		}

		public override Dictionary<string, object> ToDictionary(object data)
		{
			Dictionary<string, object> ret = new Dictionary<string, object>();
			object value;

			foreach (string fieldname in _ClassFields)
			{
				ret.Add(fieldname, (value = GetValueExt(data, fieldname)) is PXFieldState ? ((PXFieldState)value).Value : value);
			}
			return ret;
		}

		public override string ToXml(object data)
		{
			TNode node = data as TNode;
			if (node != null)
			{
				PXCacheExtension[] extensions = null;
				if (_Extensions != null)
				{
					lock (((ICollection)_Extensions).SyncRoot)
					{
						if (!_Extensions.TryGetValue(node, out extensions))
						{
							_Extensions[node] = extensions = _CreateExtensions(node);
						}
					}
				}
				StringBuilder bld = new StringBuilder();
				using (System.Xml.XmlTextWriter xw = new System.Xml.XmlTextWriter(new System.IO.StringWriter(bld)))
				{
					xw.Formatting = System.Xml.Formatting.Indented;
					xw.Indentation = 2;
					xw.WriteStartElement("Row");
					xw.WriteAttributeString("type", typeof(TNode).FullName);
					for (int i = 0; i < _ClassFields.Count; i++)
					{
						object val = _GetValueByOrdinal(node, _FieldsMap[_ClassFields[i]], extensions);
						if (val != null)
						{
							xw.WriteStartElement("Field");
							xw.WriteAttributeString("name", _ClassFields[i]);
							switch (Type.GetTypeCode(_FieldTypes[i]))
							{
								case TypeCode.String:
									xw.WriteAttributeString("value", (string)val);
									break;
								case TypeCode.Int16:
									xw.WriteAttributeString("value", ((short)val).ToString(System.Globalization.CultureInfo.InvariantCulture));
									break;
								case TypeCode.Int32:
									xw.WriteAttributeString("value", ((int)val).ToString(System.Globalization.CultureInfo.InvariantCulture));
									break;
								case TypeCode.Int64:
									xw.WriteAttributeString("value", ((long)val).ToString(System.Globalization.CultureInfo.InvariantCulture));
									break;
								case TypeCode.Boolean:
									xw.WriteAttributeString("value", ((bool)val).ToString(System.Globalization.CultureInfo.InvariantCulture));
									break;
								case TypeCode.Decimal:
									xw.WriteAttributeString("value", ((decimal)val).ToString(System.Globalization.CultureInfo.InvariantCulture));
									break;
								case TypeCode.Single:
									xw.WriteAttributeString("value", ((float)val).ToString(System.Globalization.CultureInfo.InvariantCulture));
									break;
								case TypeCode.Double:
									xw.WriteAttributeString("value", ((double)val).ToString(System.Globalization.CultureInfo.InvariantCulture));
									break;
								case TypeCode.DateTime:
									xw.WriteAttributeString("value", ((DateTime)val).ToString(System.Globalization.CultureInfo.InvariantCulture));
									break;
								case TypeCode.Object:
									if (_FieldTypes[i] == typeof(Guid))
									{
										xw.WriteAttributeString("value", ((Guid)val).ToString(null, System.Globalization.CultureInfo.InvariantCulture));
									}
									else if (_FieldTypes[i] == typeof(byte[]))
									{
										xw.WriteAttributeString("value", Convert.ToBase64String((byte[])val));
									}
									break;
							}
							xw.WriteEndElement();
						}
					}
					xw.WriteEndElement();
				}
				return bld.ToString();
			}
			return null;
		}

		public override object FromXml(string xml)
		{
			if (xml != null)
			{
				using (System.IO.TextReader tr = new System.IO.StringReader(xml))
				using (System.Xml.XmlTextReader xr = new System.Xml.XmlTextReader(tr))
				{
					string type = null;
					if (xr.ReadToDescendant("Row"))
					{
						type = xr.GetAttribute("type");
					}
					if (type == typeof(TNode).FullName)
					{
						TNode node = new TNode();
						PXCacheExtension[] extensions = null;
						if (_Extensions != null)
						{
							lock (((ICollection)_Extensions).SyncRoot)
							{
								_Extensions[node] = extensions = _CreateExtensions(node);
							}
						}
						while (xr.Read())
						{
							if (xr.Name == "Field")
							{
								string name = xr.GetAttribute("name");
								if (!String.IsNullOrEmpty(name))
								{
									string value = xr.GetAttribute("value");
									int i;
									if (value != null && _FieldsMap.TryGetValue(name, out i))
									{
										int k = _ReverseMap[name];
										switch (Type.GetTypeCode(_FieldTypes[k]))
										{
											case TypeCode.String:
												_SetValueByOrdinal(node, i, value, extensions);
												break;
											case TypeCode.Int16:
												{
													short val;
													if (short.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
													{
														_SetValueByOrdinal(node, i, val, extensions);
													}
												}
												break;
											case TypeCode.Int32:
												{
													int val;
													if (int.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
													{
														_SetValueByOrdinal(node, i, val, extensions);
													}
												}
												break;
											case TypeCode.Int64:
												{
													long val;
													if (long.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
													{
														_SetValueByOrdinal(node, i, val, extensions);
													}
												}
												break;
											case TypeCode.Boolean:
												{
													bool val;
													if (bool.TryParse(value, out val))
													{
														_SetValueByOrdinal(node, i, val, extensions);
													}
												}
												break;
											case TypeCode.Decimal:
												{
													decimal val;
													if (decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
													{
														_SetValueByOrdinal(node, i, val, extensions);
													}
												}
												break;
											case TypeCode.Single:
												{
													float val;
													if (float.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
													{
														_SetValueByOrdinal(node, i, val, extensions);
													}
												}
												break;
											case TypeCode.Double:
												{
													double val;
													if (double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
													{
														_SetValueByOrdinal(node, i, val, extensions);
													}
												}
												break;
											case TypeCode.DateTime:
												{
													DateTime val;
													if (DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out val))
													{
														_SetValueByOrdinal(node, i, val, extensions);
													}
												}
												break;
											case TypeCode.Object:
												if (_FieldTypes[k] == typeof(Guid))
												{
													Guid val;
													if (PX.Common.GUID.TryParse(value, out val))
													{
														_SetValueByOrdinal(node, i, val, extensions);
													}
												}
												else if (_FieldTypes[k] == typeof(byte[]))
												{
													_SetValueByOrdinal(node, i, Convert.FromBase64String(value), extensions);
												}
												break;
										}
									}
								}
							}
						}
						return node;
					}
				}
			}
			return null;
		}

		public override string ValueToString(string fieldName, object val)
		{
			if (val == null)
			{
				return null;
			}

			if (_BypassAuditFields != null && _BypassAuditFields.Contains(fieldName))
				return PXDBUserPasswordAttribute.DefaultVeil;

			switch (Type.GetTypeCode(_FieldTypes[_ReverseMap[fieldName]]))
			{
				case TypeCode.String:
					return (string)val;
				case TypeCode.Int16:
					return ((short)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
				case TypeCode.Int32:
					return ((int)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
				case TypeCode.Int64:
					return ((long)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
				case TypeCode.Boolean:
					return ((bool)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
				case TypeCode.Decimal:
					return ((decimal)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
				case TypeCode.Single:
					return ((float)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
				case TypeCode.Double:
					return ((double)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
				case TypeCode.DateTime:
					return ((DateTime)val).ToString(System.Globalization.CultureInfo.InvariantCulture);
				case TypeCode.Object:
					if (_FieldTypes[_ReverseMap[fieldName]] == typeof(Guid))
					{
						return ((Guid)val).ToString(null, System.Globalization.CultureInfo.InvariantCulture);
					}
					else if (_FieldTypes[_ReverseMap[fieldName]] == typeof(byte[]))
					{
						return Convert.ToBase64String((byte[])val);
					}
					return null;
			}
			return null;
		}

		public override object ValueFromString(string fieldName, string val)
		{
			int k = -1;
			Type t = null;
			if (_ReverseMap.TryGetValue(fieldName, out k))
			{
				t = _FieldTypes[k];
			}
			else
			{
				PXFieldState state = GetStateExt(null, fieldName) as PXFieldState;
				if (state != null)
				{
					t = state.DataType;
				}
			}

			if (t == null) return null;

			switch (Type.GetTypeCode(t))
			{
				case TypeCode.String:
					return val;
				case TypeCode.Int16:
					{
						short value;
						if (short.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
						{
							return value;
						}
					}
					break;
				case TypeCode.Int32:
					{
						int value;
						if (int.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
						{
							return value;
						}
					}
					break;
				case TypeCode.Int64:
					{
						long value;
						if (long.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
						{
							return value;
						}
					}
					break;
				case TypeCode.Boolean:
					{
						bool value;
						if (bool.TryParse(val, out value))
						{
							return value;
						}
					}
					break;
				case TypeCode.Decimal:
					{
						decimal value;
						if (decimal.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
						{
							return value;
						}
					}
					break;
				case TypeCode.Single:
					{
						float value;
						if (float.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
						{
							return value;
						}
					}
					break;
				case TypeCode.Double:
					{
						double value;
						if (double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
						{
							return value;
						}
					}
					break;
				case TypeCode.DateTime:
					{
						DateTime value;
						if (DateTime.TryParse(val, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out value))
						{
							return value;
						}
					}
					break;
				case TypeCode.Object:
					if (_FieldTypes[k] == typeof(Guid))
					{
						Guid value;
						if (PX.Common.GUID.TryParse(val, out value))
						{
							return value;
						}
					}
					else if (_FieldTypes[k] == typeof(byte[]))
					{
						return Convert.FromBase64String(val);
					}
					break;
			}
			return null;
		}

		public static void RestoreCopy(TNode item, TNode copy)
		{
			CacheStaticInfo result = _Initialize(false);

			if (result._CreateExtensions != null)
			{
				PXCacheExtension[] copyextensions;
				PXCacheExtension[] itemextensions;
				PXCacheExtensionCollection dict = PXContext.GetSlot<PXCacheExtensionCollection>();
				if (dict == null)
				{
					dict = PXContext.SetSlot<PXCacheExtensionCollection>(new PXCacheExtensionCollection());
				}
				lock (((ICollection)dict).SyncRoot)
				{
					if (!dict.TryGetValue(copy, out copyextensions))
					{
						dict[copy] = copyextensions = result._CreateExtensions(copy);
					}
					if (!dict.TryGetValue(item, out itemextensions))
					{
						dict[item] = itemextensions = result._CreateExtensions(item);
					}
				}
				for (int i = 0; i < result._ClassFields.Count; i++)
				{
					result._SetValueByOrdinal(item, i, result._GetValueByOrdinal(copy, i, copyextensions), itemextensions);
				}
			}
			else
			{
				for (int i = 0; i < result._ClassFields.Count; i++)
				{
					result._SetValueByOrdinal(item, i, result._GetValueByOrdinal(copy, i, null), null);
				}
			}
		}

		internal static Type GetItemTypeInternal()
		{
			return typeof(TNode);
		}

		public override PXFieldCollection Fields
		{
			get
			{
				if (_Fields == null)
				{
					_Fields = new PXFieldCollection(_ClassFields, _FieldsMap);
				}
				return _Fields;
			}
		}

		public override List<Type> BqlFields
		{
			get
			{
				if (_BqlFields == null)
				{
					_BqlFields = new List<Type>(_BqlFieldsMap.Keys);
				}
				return _BqlFields;
			}
		}

		public override List<Type> BqlKeys
		{
			get
			{
				if (_BqlKeys == null)
				{
					_BqlKeys = new List<Type>();
					foreach (string key in Keys)
					{
						foreach (Type t in BqlFields)
						{
							if (String.Compare(t.Name, key, StringComparison.OrdinalIgnoreCase) == 0)
							{
								_BqlKeys.Add(t);
								break;
							}
						}
					}
				}
				return _BqlKeys;
			}
		}

        public override List<Type> BqlImmutables
        {
            get
            {
                if (_BqlImmutables == null)
                {
                    _BqlImmutables = new List<Type>();
                    foreach (string key in Immutables)
                    {
                        foreach (Type t in BqlFields)
                        {
                            if (String.Compare(t.Name, key, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                _BqlImmutables.Add(t);
                                break;
                            }
                        }
                    }
                }
                return _BqlImmutables;
            }
        }

		private BqlCommand _BqlSelect;
		public override BqlCommand BqlSelect
		{
			get
			{
				return _BqlSelect;
			}
			set
			{
				_BqlSelect = value;
			}
		}

		public override Type BqlTable
		{
			get
			{
				return _BqlTable;
			}
		}

		internal override Type GetFieldType(string fieldName)
		{
			int iField;
			return _ReverseMap.TryGetValue(fieldName, out iField) ? _FieldTypes[iField] : null;
		}

		public override Type[] GetExtensionTypes()
		{
			return _ExtensionTypes;
		}
		#endregion

		#region Object
		private System.Threading.ReaderWriterLock _DelegatesLock;
		protected delegate bool EqualsDelegate(TNode a, TNode b);
		private EqualsDelegate _Equals;
		private Dictionary<string, EqualsDelegate> _DelegatesEquals;
		public override bool ObjectsEqual(object a, object b)
		{
			TNode anode = a as TNode;
			TNode bnode = b as TNode;
			if (anode != null && bnode != null)
			{
				return _Equals((TNode)a, (TNode)b);
			}
			else
			{
				IDictionary adict = a as IDictionary;
				IDictionary bdict = b as IDictionary;
				if (adict != null && bdict != null)
				{
					foreach (string key in _Keys)
					{
						if (!adict.Contains(key) || !bdict.Contains(key) || !Object.Equals(adict[key], bdict[key]))
						{
							return false;
						}
						return true;
					}
				}
				else
				{
					return false;
				}
			}
			return false;
		}
		protected delegate int GetHashCodeDelegate(TNode a);
		private GetHashCodeDelegate _GetHashCode;
		private Dictionary<string, GetHashCodeDelegate> _DelegatesGetHashCode;
		public override int GetObjectHashCode(object data)
		{
			TNode node = data as TNode;
			if (node != null)
			{
				return _GetHashCode((TNode)data);
			}
			else
			{
				IDictionary dict = data as IDictionary;
				if (dict != null)
				{
					unchecked
					{
						int ret = 13;
						foreach (string key in _Keys)
						{
							object value = null;
							if (!dict.Contains(key) || (value = dict[key]) == null)
							{
								return 0;
							}
							ret = ret * 37 + dict[key].GetHashCode();
						}
						return ret;
					}
				}
			}
			return 0;
		}
		public override string ObjectToString(object data)
		{
			if (data == null)
			{
				return String.Empty;
			}
			if (data is PXResult)
			{
				data = ((PXResult)data)[0];
			}
			TNode item = data as TNode;
			if (item == null)
			{
				return data.ToString();
			}
			StringBuilder bld = new StringBuilder(typeof(TNode).Name);
			bld.Append("{");
			bool first = true;
			foreach (string name in Keys)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					bld.Append(", ");
				}
				bld.Append(name);
				bld.Append(" = ");
				bld.Append(GetValue(data, name));
			}
			bld.Append("}");
			return bld.ToString();
		}
		#endregion

		#region Schema
		/// <summary>
		/// Gets an extension of appropriate type
		/// </summary>
		/// <typeparam name="Extension">
		/// The type of extension requested
		/// </typeparam>
		/// <param name="item">
		/// Parent standard object
		/// </param>
		/// <returns>
		/// Object of type Extension
		/// </returns>
		public override Extension GetExtension<Extension>(object item)
		{
			int idx = Array.IndexOf(_ExtensionTypes, typeof(Extension));
			if (idx == -1 || _Extensions == null)
			{
				throw new PXException(ErrorMessages.IncorrectExtensionRequested);
			}
			PXCacheExtension[] extensions;
			lock (((ICollection)_Extensions).SyncRoot)
			{
				if (!_Extensions.TryGetValue((TNode)item, out extensions))
				{
					_Extensions[(TNode)item] = extensions = _CreateExtensions((TNode)item);
				}
			}
			return (Extension)extensions[idx];
		}
		/// <summary>
		/// Gets an extension of appropriate type
		/// </summary>
		/// <typeparam name="Extension">
		/// The type of extension requested
		/// </typeparam>
		/// <param name="item">
		/// Parent standard object
		/// </param>
		/// <returns>
		/// Object of type Extension
		/// </returns>
		public static Extension GetExtension<Extension>(TNode item)
			where Extension : PXCacheExtension<TNode>
		{
			CacheStaticInfo result = _Initialize(false);

			int idx = Array.IndexOf(result._ExtensionTypes, typeof(Extension));
			if (idx == -1)
			{
				throw new PXException(ErrorMessages.IncorrectExtensionRequested);
			}
			PXCacheExtensionCollection dict = PXContext.GetSlot<PXCacheExtensionCollection>();
			if (dict == null)
			{
				dict = PXContext.SetSlot<PXCacheExtensionCollection>(new PXCacheExtensionCollection());
			}
			PXCacheExtension[] extensions;
			lock (((ICollection)dict).SyncRoot)
			{
				if (!dict.TryGetValue(item, out extensions))
				{
					dict[item] = extensions = result._CreateExtensions(item);
				}
			}
			return (Extension)extensions[idx];
		}
		/// <summary>
		/// Gets the type of items in the cache.
		/// </summary>
		/// <returns>Containing type.</returns>
		public override Type GetItemType()
		{
			return typeof(TNode);
		}

		internal List<string> _ClassFields;
		private Dictionary<string, int> _ReverseMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		internal List<Type> _FieldTypes;
		private Dictionary<Type, int> _BqlFieldsMap;
		private static Type _BqlTable;
		private PXDBInterceptorAttribute _Interceptor;
		public override PXDBInterceptorAttribute Interceptor
		{
			get
			{
				return _Interceptor;
			}
			set
			{
				_Interceptor = value;
				_Interceptor.CacheAttached(this);
			}
		}
		internal Dictionary<string, int> _FieldsMap;
		internal List<PXEventSubscriberAttribute> _FieldAttributes;
		internal EventsRowMap _EventsRowMap;
		internal Dictionary<string, EventsFieldMap> _EventsFieldMap;
		internal int[] _FieldAttributesFirst;
		internal int[] _FieldAttributesLast;
		internal EventPosition[] _EventPositions;

		private List<PXEventSubscriberAttribute> _CacheAttributes;
		private Dictionary<TNode, List<PXEventSubscriberAttribute>> _ItemAttributes;
		private List<TNode> _PendingItems;
		private int[] _AttributesFirst;
		private int[] _AttributesLast;
		#endregion

		#region Cache
		public override int GetFieldCount()
		{
			return Fields.Count;
		}
		public override int GetFieldOrdinal<Field>()
		{
			int ordinal;
			if (_FieldsMap.TryGetValue(typeof(Field).Name, out ordinal))
			{
				return ordinal;
			}
			return -1;
		}
		public override int GetFieldOrdinal(string field)
		{
			int ordinal;
			if (_FieldsMap.TryGetValue(field, out ordinal))
			{
				return ordinal;
			}
			return -1;
		}
		public override void Normalize()
		{
			_Items.Normalize(null);
		}

		private PXCollection<TNode> _Items;
		private TNode _CurrentPlacedIntoCache;
		private bool _ItemsDenormalized;
		public override object Current
		{
			get
			{
				if (_Current == null)
				{
					if (!stateLoaded)
					{
						Load();
						if (_Current != null)
						{
							return _Current;
						}
					}
					_Current = _Graph.GetDefault<TNode>();
				}
				_CurrentPlacedIntoCache = null;
				return _Current;
			}
			set
			{
				_Current = value as TNode;
				OnRowSelected(_Current);
			}
		}


		#endregion

		#region Ctor

		protected static CacheStaticInfo _Initialize(bool ignoredResult)
		{
			CacheStaticInfoBase result = null;
			Dictionary<Type, CacheStaticInfoBase> initialized = PXContext.GetSlot<Dictionary<Type, CacheStaticInfoBase>>();
			if (initialized == null)
			{
				initialized = PXContext.SetSlot<Dictionary<Type, CacheStaticInfoBase>>(PXDatabase.GetSlot<Dictionary<Type, CacheStaticInfoBase>>("CacheStaticInfo"));
			}
			if (initialized != null)
			{
				PXExtensionManager._StaticInfoLock.AcquireReaderLock(-1);
				try
				{
					if (initialized.TryGetValue(typeof(TNode), out result))
					{
						return (CacheStaticInfo)result;
					}
				}
				finally
				{
					PXExtensionManager._StaticInfoLock.ReleaseReaderLock();
				}
			}
			List<Type> extensions = _GetExtensions(typeof(TNode), initialized != null);
			Dictionary<PXExtensionManager.ListOfTypes, CacheStaticInfoBase> multiple;
			PXExtensionManager._StaticInfoLock.AcquireReaderLock(-1);
			try
			{
				if (PXExtensionManager._CacheStaticInfo.TryGetValue(typeof(TNode), out multiple)
					&& multiple.TryGetValue(new PXExtensionManager.ListOfTypes(extensions), out result))
				{
					System.Threading.LockCookie lc = PXExtensionManager._StaticInfoLock.UpgradeToWriterLock(-1);
					try
					{
						initialized[typeof(TNode)] = result;
						return (CacheStaticInfo)result;
					}
					finally
					{
						PXExtensionManager._StaticInfoLock.DowngradeFromWriterLock(ref lc);
					}
				}
			}
			finally
			{
				PXExtensionManager._StaticInfoLock.ReleaseReaderLock();
			}

			result = new CacheStaticInfo();


			PropertyInfo[] orig = typeof(TNode).GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(f => f, new PXGraph.FieldInfoComparer()).ToArray();
			PropertyInfo[] info = new PropertyInfo[orig.Length];
			int ii = 0;
			while (ii < info.Length)
			{
				int oi = orig.Length - ii - 1;
				while (oi > 0 && orig[orig.Length - ii - 1].DeclaringType == orig[oi - 1].DeclaringType)
				{
					oi--;
				}
				Array.Copy(orig, oi, info, ii, orig.Length - ii - oi);
				ii = orig.Length - oi;
			}

			Dictionary<string, List<PropertyInfo>> extproperties = null;
			if (extensions == null)
			{
				extensions = _GetExtensions(typeof(TNode), initialized != null);
			}
			if (extensions.Count > 0)
			{
				if (!ignoredResult)
				{
					result._ExtensionTypes = extensions.ToArray();
				}
				HashSet<string> baseproperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				List<PropertyInfo> addproperties = new List<PropertyInfo>();
				foreach (PropertyInfo basepi in info)
				{
					baseproperties.Add(basepi.Name);
				}
				extproperties = new Dictionary<string, List<PropertyInfo>>(StringComparer.OrdinalIgnoreCase);
				foreach (Type ext in extensions)
				{
					foreach (PropertyInfo extpi in ext.GetProperties(BindingFlags.Public | BindingFlags.Instance))
					{
						List<PropertyInfo> found;
						if (!extproperties.TryGetValue(extpi.Name, out found))
						{
							extproperties[extpi.Name] = found = new List<PropertyInfo>();
						}
						found.Insert(0, extpi);
					}
				}
				{
					foreach (List<PropertyInfo> extpi in extproperties.Values)
					{
						if (!baseproperties.Contains(extpi[0].Name))
						{
							addproperties.Add(extpi[0]);
						}
					}
				}
				if (addproperties.Count > 0)
				{
					Array.Resize(ref info, info.Length + addproperties.Count);
					Array.Copy(addproperties.ToArray(), 0, info, info.Length - addproperties.Count, addproperties.Count);
				}
				DynamicMethod dm_ext;
				if (!ignoredResult)
				{
					if (!PXGraph.IsRestricted)
					{
						dm_ext = new DynamicMethod("_CreateExtensions", typeof(PXCacheExtension[]), new Type[] { typeof(TNode) }, typeof(PXCache<TNode>));
					}
					else
					{
						dm_ext = new DynamicMethod("_CreateExtensions", typeof(PXCacheExtension[]), new Type[] { typeof(TNode) });
					}
					ILGenerator il = dm_ext.GetILGenerator();
					LocalBuilder ret = il.DeclareLocal(typeof(PXCacheExtension[]));
					il.Emit(OpCodes.Ldc_I4, extensions.Count);
					il.Emit(OpCodes.Newarr, typeof(PXCacheExtension));
					il.Emit(OpCodes.Stloc, ret);
					LocalBuilder extcreated = il.DeclareLocal(typeof(PXCacheExtension));
					for (int i = 0; i < extensions.Count; i++)
					{
						Type ext = extensions[i];
						il.Emit(OpCodes.Ldloc, ret);
						il.Emit(OpCodes.Ldc_I4, i);
						il.Emit(OpCodes.Newobj, ext.GetConstructor(new Type[0]));
						il.Emit(OpCodes.Dup);
						il.Emit(OpCodes.Stloc, extcreated);
						il.Emit(OpCodes.Stelem_Ref);
						il.Emit(OpCodes.Ldloc, extcreated);
						il.Emit(OpCodes.Castclass, ext);
						il.Emit(OpCodes.Ldarg_0);
						il.Emit(OpCodes.Newobj, typeof(WeakReference).GetConstructor(new Type[] { typeof(object) }));
						il.Emit(OpCodes.Castclass, typeof(WeakReference));
						il.Emit(OpCodes.Stfld, ext.GetField("_Base", BindingFlags.Instance | BindingFlags.NonPublic));
						int extcnt;
						for (int idx = 0; idx < (extcnt = ext.BaseType.GetGenericArguments().Length - 1); idx++)
						{
							Type extbase = ext.BaseType.GetGenericArguments()[idx];
							int j;
							if ((j = extensions.IndexOf(extbase)) != -1)
							{
								il.Emit(OpCodes.Ldloc, extcreated);
								il.Emit(OpCodes.Castclass, ext);
								il.Emit(OpCodes.Ldloc, ret);
								il.Emit(OpCodes.Ldc_I4, j);
								il.Emit(OpCodes.Ldelem_Ref);
								il.Emit(OpCodes.Castclass, extbase);
								il.Emit(OpCodes.Stfld, ext.GetField("_Base" + (extcnt - idx).ToString(), BindingFlags.Instance | BindingFlags.NonPublic));
							}
							else
							{
								throw new PXException("Dependant extension does not belong to the same cache.");
							}
						}
					}
					il.Emit(OpCodes.Ldloc, ret);
					il.Emit(OpCodes.Ret);
					((CacheStaticInfo)result)._CreateExtensions = (CreateExtensionsDelegate)dm_ext.CreateDelegate(typeof(CreateExtensionsDelegate));
				}
			}
			else if (!ignoredResult)
			{
				result._ExtensionTypes = new Type[0];
			}

			DynamicMethod dm_set = null;
			DynamicMethod dm_get = null;
			ILGenerator il_set = null;
			ILGenerator il_get = null;
			System.Reflection.Emit.Label[] labels_set = new System.Reflection.Emit.Label[info.Length];
			System.Reflection.Emit.Label[] labels_get = new System.Reflection.Emit.Label[info.Length];
			System.Reflection.Emit.Label? ret_set = null;
			System.Reflection.Emit.Label? ret_get = null;
			if (!ignoredResult)
			{
				if (!PXGraph.IsRestricted)
				{
					dm_set = new DynamicMethod("_SetValueByOrdinal", null, new Type[] { typeof(TNode), typeof(int), typeof(object), typeof(PXCacheExtension[]) }, typeof(PXCache<TNode>));
					dm_get = new DynamicMethod("_GetValueByOrdinal", typeof(object), new Type[] { typeof(TNode), typeof(int), typeof(PXCacheExtension[]) }, typeof(PXCache<TNode>));
				}
				else
				{
					dm_set = new DynamicMethod("_SetValueByOrdinal", null, new Type[] { typeof(TNode), typeof(int), typeof(object), typeof(PXCacheExtension[]) });
					dm_get = new DynamicMethod("_GetValueByOrdinal", typeof(object), new Type[] { typeof(TNode), typeof(int), typeof(PXCacheExtension[]) });
				}
				il_set = dm_set.GetILGenerator();
				for (int l = 0; l < info.Length; l++)
				{
					labels_set[l] = il_set.DefineLabel();
				}
				ret_set = il_set.DefineLabel();
				il_get = dm_get.GetILGenerator();
				for (int l = 0; l < info.Length; l++)
				{
					labels_get[l] = il_get.DefineLabel();
				}
				ret_get = il_get.DefineLabel();
				il_set.Emit(OpCodes.Ldarg_1);
				il_set.Emit(OpCodes.Switch, labels_set);
				il_set.Emit(OpCodes.Br, ret_set.Value);
				il_get.Emit(OpCodes.Ldarg_1);
				il_get.Emit(OpCodes.Switch, labels_get);
				il_get.Emit(OpCodes.Br, ret_get.Value);
			}

			List<KeyValuePair<string, int>> tabs = new List<KeyValuePair<string, int>>();

			Dictionary<string, PropertyInfo> cstInjectedProps = null;
			foreach (Type t in typeof(TNode).CreateList(_ => _.BaseType))
			{
				if (t != typeof(object))
				{
					string cst_name = CustomizedTypeManager.GetCustomizedTypeFullName(t);
					Type cstInjectedType = System.Web.Compilation.BuildManager.GetType(cst_name, false);
					if (cstInjectedType != null)
					{
						if (cstInjectedProps == null)
						{
							cstInjectedProps = new Dictionary<string, PropertyInfo>();
						}
						foreach (PropertyInfo cpi in cstInjectedType
							.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
							.Where(_ => _.Name.StartsWith("Append_")
								|| _.Name.StartsWith("Replace_")))
						{
							cstInjectedProps[cpi.Name] = cpi;
						}
					}
				}
			}

			for (int i = 0; i < info.Length; i++)
			{
				//Type declaringType = null;
				PropertyInfo prop = info[i];
				//string propName = prop.Name;
				//object[] propattribues = prop.GetCustomAttributes(typeof(PXEventSubscriberAttribute), false);
				//if (propattribues.Length == 0)
				//{
				//    Type bt = prop.DeclaringType.GetNestedType(prop.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
				//    if (bt != null && (bt = bt.BaseType).IsNested && typeof(IBqlField).IsAssignableFrom(bt))
				//    {
				//        PropertyInfo bp = bt.DeclaringType.GetProperty(bt.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
				//        if (bp != null)
				//        {
				//            propattribues = bp.GetCustomAttributes(typeof(PXEventSubscriberAttribute), false);
				//            declaringType = bt.DeclaringType;
				//            propName = bp.Name;
				//        }
				//    }
				//}

				PXEventSubscriberAttribute[] attributes;
				if (extproperties == null || !extproperties.ContainsKey(prop.Name) || Attribute.IsDefined(extproperties[prop.Name][0].DeclaringType, typeof(PXDBInterceptorAttribute)))
				{
					attributes = (PXEventSubscriberAttribute[])prop.GetCustomAttributesEx(typeof(PXEventSubscriberAttribute), false);
				}
				else
				{
					attributes = (PXEventSubscriberAttribute[])extproperties[prop.Name][0].GetCustomAttributesEx(typeof(PXEventSubscriberAttribute), false);
				}

				if (cstInjectedProps != null && cstInjectedProps.Count > 0)
				{
					Type declaringType = prop.DeclaringType;
					Type[] range = typeof(TNode).CreateList(_ => _.BaseType)
						.TakeWhile(declaringType.IsAssignableFrom)
						.Reverse()
						.ToArray();

					foreach (Type type in range)
					{

						string suffix = type.FullName.Replace('.', '_').Replace('+', '_') + "_" + prop.Name;
						PropertyInfo cstInjectedProp;
						if (cstInjectedProps.TryGetValue("Replace_" + suffix, out cstInjectedProp))
						{
							attributes = (PXEventSubscriberAttribute[])cstInjectedProp.GetCustomAttributesEx(typeof(PXEventSubscriberAttribute), false);

						}

						if (cstInjectedProps.TryGetValue("Append_" + suffix, out cstInjectedProp))
						{

							var appended = (PXEventSubscriberAttribute[])cstInjectedProp
								.GetCustomAttributesEx(typeof(PXEventSubscriberAttribute), false);

							if (appended.Any())
								attributes = attributes.Concat(appended).ToArray();

						}

					}
					//string suffix = typeof(TNode).FullName.Replace('.', '_').Replace('+', '_') + "_" + prop.Name;
					//foreach (PropertyInfo cstInjectedProp in cstInjectedProps)
					//{
					//    if (cstInjectedProp.Name == "Replace_" + suffix)
					//    {
					//        attributes = (PXEventSubscriberAttribute[])cstInjectedProp.GetCustomAttributes(typeof(PXEventSubscriberAttribute), false);
					//        break;
					//    }
					//    if (cstInjectedProp.Name == "Append_" + suffix)
					//    {
					//        List<PXEventSubscriberAttribute> ol = new List<PXEventSubscriberAttribute>((PXEventSubscriberAttribute[])prop.GetCustomAttributes(typeof(PXEventSubscriberAttribute), false));
					//        ol.AddRange((PXEventSubscriberAttribute[])cstInjectedProp.GetCustomAttributes(typeof(PXEventSubscriberAttribute), false));
					//        attributes = ol.ToArray();
					//        break;
					//    }
					//}
				}

				PXAttributeFamilyAttribute.CheckAttributes(prop, attributes);
				//if (attributes == null)
				//{
				//    attributes = (PXEventSubscriberAttribute[])prop.GetCustomAttributes(typeof(PXEventSubscriberAttribute), false);
				//}
				Type tabletype = null;
				if (!typeof(PXCacheExtension).IsAssignableFrom(prop.DeclaringType) || !extproperties.ContainsKey(prop.Name))
				{
					tabletype = typeof(TNode);
				}
				else
				{
					if (Attribute.IsDefined(prop.DeclaringType, typeof(PXDBInterceptorAttribute)))
					{
						tabletype = prop.DeclaringType;
					}
					else
					{
						foreach (PropertyInfo sibling in extproperties[prop.Name])
						{
							if (Attribute.IsDefined(sibling.DeclaringType, typeof(PXDBInterceptorAttribute)))
							{
								tabletype = sibling.DeclaringType;
							}
						}
					}
					if (tabletype == null)
					{
						if (prop.DeclaringType.BaseType.IsGenericType)
						{
							Type[] genericArgs = prop.DeclaringType.BaseType.GetGenericArguments();
							tabletype = genericArgs[genericArgs.Length-1];
						}
						else
						{
							tabletype = typeof(TNode);
						}
					}
					else if (!ignoredResult)
					{
						if (result._ExtensionTables == null)
						{
							result._ExtensionTables = new List<Type>();
						}
						if (!result._ExtensionTables.Contains(tabletype))
						{
							result._ExtensionTables.Add(tabletype);
						}
					}
				}

				for (int k = 0; k < attributes.Length; k++)
				{
					PXEventSubscriberAttribute prepared = prepareAttribute(attributes[k], prop.Name, i, tabletype);

					if (!ignoredResult)
					{
						result._FieldAttributes.Add(prepared);

						IPXInterfaceField ui = attributes[k] as IPXInterfaceField;
						if (ui != null && ui.TabOrder != i)
						{
							tabs.Add(new KeyValuePair<string, int>(prop.Name, ui.TabOrder));
						}

						if (prepared is PXDBTimestampAttribute)
						{
							result._TimestampOrdinal = i;
						}
					}
				}

				if (ignoredResult)
				{
					continue;
				}

				if (!typeof(IBqlTable).IsAssignableFrom(prop.DeclaringType)
					&& (CustomizedTypeManager.IsCustomizedType(prop.DeclaringType) || i >= orig.Length)
					&& (tabs.Count == 0 || tabs[tabs.Count - 1].Key != prop.Name))
				{
					tabs.Add(new KeyValuePair<string, int>(prop.Name, int.MaxValue));
				}

				il_set.MarkLabel(labels_set[i]);
				il_get.MarkLabel(labels_get[i]);

				Type propType = prop.PropertyType;
				if (i < orig.Length)
				{
					if (prop.CanWrite)
					{
						il_set.Emit(OpCodes.Ldarg_0);
						il_set.Emit(OpCodes.Ldarg_2);
						if (!propType.IsValueType)
						{
							il_set.Emit(OpCodes.Castclass, propType);
						}
						else
						{
							il_set.Emit(OpCodes.Unbox_Any, propType);
						}
						il_set.Emit(OpCodes.Callvirt, prop.GetSetMethod());
					}
				}
				if (extproperties != null && extproperties.ContainsKey(prop.Name))
				{
					foreach (PropertyInfo extpi in extproperties[prop.Name])
					{
						if (extpi.CanWrite && extpi.PropertyType == propType)
						{
							il_set.Emit(OpCodes.Ldarg_3);
							il_set.Emit(OpCodes.Ldc_I4, extensions.IndexOf(extpi.DeclaringType));
							il_set.Emit(OpCodes.Ldelem_Ref);
							il_set.Emit(OpCodes.Castclass, extpi.DeclaringType);
							il_set.Emit(OpCodes.Ldarg_2);
							if (!propType.IsValueType)
							{
								il_set.Emit(OpCodes.Castclass, propType);
							}
							else
							{
								il_set.Emit(OpCodes.Unbox_Any, propType);
							}
							il_set.Emit(OpCodes.Callvirt, extpi.GetSetMethod());
						}
					}
				}
				il_set.Emit(OpCodes.Br, ret_set.Value);

				if (prop.CanRead)
				{
					if (i < orig.Length)
					{
						il_get.Emit(OpCodes.Ldarg_0);
					}
					else
					{
						il_get.Emit(OpCodes.Ldarg_2);
						il_get.Emit(OpCodes.Ldc_I4, extensions.IndexOf(prop.DeclaringType));
						il_get.Emit(OpCodes.Ldelem_Ref);
						il_get.Emit(OpCodes.Castclass, prop.DeclaringType);
					}
					il_get.Emit(OpCodes.Callvirt, prop.GetGetMethod());
					if (propType.IsValueType)
					{
						il_get.Emit(OpCodes.Box, propType);
					}
					il_get.Emit(OpCodes.Ret);
				}
				else
				{
					il_get.Emit(OpCodes.Br, ret_get.Value);
				}

				result._ClassFields.Add(prop.Name);
				result._FieldTypes.Add(prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? prop.PropertyType.GetGenericArguments()[0] : prop.PropertyType);
				result._FieldsMap[prop.Name] = i;
			}

			if (ignoredResult)
			{
				return null;
			}

			il_set.MarkLabel(ret_set.Value);
			il_set.Emit(OpCodes.Ret);
			il_get.MarkLabel(ret_get.Value);
			il_get.Emit(OpCodes.Ldnull);
			il_get.Emit(OpCodes.Ret);
			((CacheStaticInfo)result)._SetValueByOrdinal = (SetValueByOrdinalDelegate)dm_set.CreateDelegate(typeof(SetValueByOrdinalDelegate));
			((CacheStaticInfo)result)._GetValueByOrdinal = (GetValueByOrdinalDelegate)dm_get.CreateDelegate(typeof(GetValueByOrdinalDelegate));

			foreach (KeyValuePair<string, int> ui in tabs)
			{
				int value = ui.Value;
				if (value < 0)
				{
					value = 0;
				}
				else if (value >= info.Length)
				{
					value = info.Length - 1;
				}
				int idx = result._ClassFields.IndexOf(ui.Key);
				Type ft = result._FieldTypes[idx];
				result._ClassFields.RemoveAt(idx);
				result._FieldTypes.RemoveAt(idx);
				result._ClassFields.Insert(value, ui.Key);
				result._FieldTypes.Insert(value, ft);
				int targetpos = 0;
				for (int i = 0; i < result._FieldAttributes.Count; i++)
				{
					int ord = result._ClassFields.IndexOf(result._FieldAttributes[i].FieldName);
					if (ord < value)
					{
						targetpos = i + 1;
					}
				}
				PXEventSubscriberAttribute first = null;
				for (int i = 0; i < result._FieldAttributes.Count; i++)
				{
					if (result._FieldAttributes[i].FieldName == ui.Key)
					{
						if (first == null)
						{
							first = result._FieldAttributes[i];
						}
						else if (first == result._FieldAttributes[i])
						{
							break;
						}
						result._FieldAttributes.Insert(targetpos, result._FieldAttributes[i]);
						result._FieldAttributes.RemoveAt(i < targetpos ? i : i + 1);
						if (i >= targetpos)
						{
							targetpos++;
						}
						else
						{
							i--;
						}
					}
				}
			}

			result._FieldAttributesFirst = new int[result._ClassFields.Count];
			result._FieldAttributesLast = new int[result._ClassFields.Count];
			for (int i = 0; i < result._FieldAttributesLast.Length; i++)
			{
				result._FieldAttributesLast[i] = -1;
			}
			for (int i = 0; i < result._FieldAttributes.Count; i++)
			{
				if (i == 0
					|| result._FieldAttributes[i - 1].FieldOrdinal != result._FieldAttributes[i].FieldOrdinal)
				{
					result._FieldAttributesFirst[result._FieldAttributes[i].FieldOrdinal] = result._FieldAttributesLast[result._FieldAttributes[i].FieldOrdinal] = i;
				}
				else
				{
					result._FieldAttributesLast[result._FieldAttributes[i].FieldOrdinal] = i;
				}
			}

			result._EventPositions = new EventPosition[result._ClassFields.Count];
			for (int i = 0; i < result._ClassFields.Count; i++)
			{
				result._EventPositions[i] = new EventPosition();
			}
			for (int i = 0; i < result._FieldAttributes.Count; i++)
			{
				PXEventSubscriberAttribute attr = result._FieldAttributes[i];
				EventPosition position = result._EventPositions[result._FieldsMap[attr.FieldName]];

				if (position.RowSelectingFirst == -1)
					position.RowSelectingFirst = result._EventsRowMap.RowSelecting.Count;
				attr.GetSubscriber<IPXRowSelectingSubscriber>(result._EventsRowMap.RowSelecting);
				position.RowSelectingLength = result._EventsRowMap.RowSelecting.Count - position.RowSelectingFirst;

				if (position.RowSelectedFirst == -1)
					position.RowSelectedFirst = result._EventsRowMap.RowSelected.Count;
				attr.GetSubscriber<IPXRowSelectedSubscriber>(result._EventsRowMap.RowSelected);
				position.RowSelectedLength = result._EventsRowMap.RowSelected.Count - position.RowSelectedFirst;

				if (position.RowInsertingFirst == -1)
					position.RowInsertingFirst = result._EventsRowMap.RowInserting.Count;
				attr.GetSubscriber<IPXRowInsertingSubscriber>(result._EventsRowMap.RowInserting);
				position.RowInsertingLength = result._EventsRowMap.RowInserting.Count - position.RowInsertingFirst;

				if (position.RowInsertedFirst == -1)
					position.RowInsertedFirst = result._EventsRowMap.RowInserted.Count;
				attr.GetSubscriber<IPXRowInsertedSubscriber>(result._EventsRowMap.RowInserted);
				position.RowInsertedLength = result._EventsRowMap.RowInserted.Count - position.RowInsertedFirst;

				if (position.RowUpdatingFirst == -1)
					position.RowUpdatingFirst = result._EventsRowMap.RowUpdating.Count;
				attr.GetSubscriber<IPXRowUpdatingSubscriber>(result._EventsRowMap.RowUpdating);
				position.RowUpdatingLength = result._EventsRowMap.RowUpdating.Count - position.RowUpdatingFirst;

				if (position.RowUpdatedFirst == -1)
					position.RowUpdatedFirst = result._EventsRowMap.RowUpdated.Count;
				attr.GetSubscriber<IPXRowUpdatedSubscriber>(result._EventsRowMap.RowUpdated);
				position.RowUpdatedLength = result._EventsRowMap.RowUpdated.Count - position.RowUpdatedFirst;

				if (position.RowDeletingFirst == -1)
					position.RowDeletingFirst = result._EventsRowMap.RowDeleting.Count;
				attr.GetSubscriber<IPXRowDeletingSubscriber>(result._EventsRowMap.RowDeleting);
				position.RowDeletingLength = result._EventsRowMap.RowDeleting.Count - position.RowDeletingFirst;

				if (position.RowDeletedFirst == -1)
					position.RowDeletedFirst = result._EventsRowMap.RowDeleted.Count;
				attr.GetSubscriber<IPXRowDeletedSubscriber>(result._EventsRowMap.RowDeleted);
				position.RowDeletedLength = result._EventsRowMap.RowDeleted.Count - position.RowDeletedFirst;

				if (position.RowPersistingFirst == -1)
					position.RowPersistingFirst = result._EventsRowMap.RowPersisting.Count;
				attr.GetSubscriber<IPXRowPersistingSubscriber>(result._EventsRowMap.RowPersisting);
				position.RowPersistingLength = result._EventsRowMap.RowPersisting.Count - position.RowPersistingFirst;

				if (position.RowPersistedFirst == -1)
					position.RowPersistedFirst = result._EventsRowMap.RowPersisted.Count;
				attr.GetSubscriber<IPXRowPersistedSubscriber>(result._EventsRowMap.RowPersisted);
				position.RowPersistedLength = result._EventsRowMap.RowPersisted.Count - position.RowPersistedFirst;

				string name = attr.FieldName.ToLower();
				EventsFieldMap map;
				if (!result._EventsFieldMap.TryGetValue(name, out map))
				{
					result._EventsFieldMap[name] = map = new EventsFieldMap();
				}

				attr.GetSubscriber<IPXCommandPreparingSubscriber>(map.CommandPreparing);
				attr.GetSubscriber<IPXFieldDefaultingSubscriber>(map.FieldDefaulting);
				attr.GetSubscriber<IPXFieldUpdatingSubscriber>(map.FieldUpdating);
				attr.GetSubscriber<IPXFieldVerifyingSubscriber>(map.FieldVerifying);
				attr.GetSubscriber<IPXFieldUpdatedSubscriber>(map.FieldUpdated);
				attr.GetSubscriber<IPXFieldSelectingSubscriber>(map.FieldSelecting);
				attr.GetSubscriber<IPXExceptionHandlingSubscriber>(map.ExceptionHandling);
			}

			_BqlTable = GetBqlTable(typeof(TNode));
			//while (typeof(IBqlTable).IsAssignableFrom(_BqlTable.BaseType)
			//    && !_BqlTable.IsDefined(typeof(PXTableAttribute), false))
			//{
			//    _BqlTable = _BqlTable.BaseType;
			//}

			Type table = typeof(TNode);
			while (table != typeof(object))
			{
				foreach (Type type in table.GetNestedTypes())
				{
					int pos;
					if (typeof(IBqlField).IsAssignableFrom(type) && result._FieldsMap.TryGetValue(type.Name, out pos))
					{
						result._BqlFieldsMap[type] = pos;
					}
				}
				table = table.BaseType;
			}
			for (int i = 0; i < extensions.Count; i++)
			{
				foreach (Type type in extensions[i].GetNestedTypes())
				{
					int pos;
					if (typeof(IBqlField).IsAssignableFrom(type) && result._FieldsMap.TryGetValue(type.Name, out pos))
					{
						result._BqlFieldsMap[type] = pos;
					}
				}
			}

			if (!PXGraph.IsRestricted)
			{
				DynamicMethod dm_del = new DynamicMethod("memberwiseClone", typeof(object), new Type[] { typeof(TNode) }, typeof(PXCache<TNode>), true);
				ILGenerator il_del = dm_del.GetILGenerator();
				il_del.Emit(OpCodes.Ldarg_0);
				il_del.Emit(OpCodes.Callvirt, typeof(TNode).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance));
				il_del.Emit(OpCodes.Ret);
				memberwiseClone = (memberwiseCloneDelegate)dm_del.CreateDelegate(typeof(memberwiseCloneDelegate));
			}

			((CacheStaticInfo)result)._DelegatesLock = new System.Threading.ReaderWriterLock();
			((CacheStaticInfo)result)._DelegatesEquals = new Dictionary<string, PXCache<TNode>.EqualsDelegate>(1, StringComparer.OrdinalIgnoreCase);
			((CacheStaticInfo)result)._DelegatesGetHashCode = new Dictionary<string, PXCache<TNode>.GetHashCodeDelegate>(1, StringComparer.OrdinalIgnoreCase);

			for (int l = 0; l < result._ClassFields.Count; l++)
			{
				result._ReverseMap[result._ClassFields[l]] = l;
			}

			PXExtensionManager._StaticInfoLock.AcquireWriterLock(-1);
			try
			{
				if (initialized != null)
				{
					initialized[typeof(TNode)] = result;
				}
				if (!PXExtensionManager._CacheStaticInfo.TryGetValue(typeof(TNode), out multiple))
				{
					PXExtensionManager._CacheStaticInfo[typeof(TNode)] = multiple = new Dictionary<PXExtensionManager.ListOfTypes, CacheStaticInfoBase>();
				}
				multiple[new PXExtensionManager.ListOfTypes(extensions)] = result;
			}
			finally
			{
				PXExtensionManager._StaticInfoLock.ReleaseWriterLock();
			}

			return (CacheStaticInfo)result;
		
		}

		private PXCacheExtensionCollection _Extensions;
		private int _OriginalsRequested;

		static PXCache()
		{
			_Initialize(true);
		}

		protected HashSet<string> _GraphSpecificFields;
		protected int? _TimestampOrdinal;

		protected void SetAutomationFieldDefaulting(Type cacheType)
		{
			var d = PXAccess.GetDefaultingDelegate(cacheType);
			if (d != null)
			{
				this.AutomationFieldDefaulting = d;
			}
		}

		public PXCache(PXGraph graph)
		{
			CacheStaticInfo result = _Initialize(false);

			_CreateExtensions = result._CreateExtensions;
			_ExtensionTables = result._ExtensionTables;
			_ExtensionTypes = result._ExtensionTypes;
			_SetValueByOrdinal = result._SetValueByOrdinal;
			_GetValueByOrdinal = result._GetValueByOrdinal;
			_DelegatesLock = result._DelegatesLock;
			_DelegatesGetHashCode = result._DelegatesGetHashCode;
			_DelegatesEquals = result._DelegatesEquals;
			_FieldsMap = result._FieldsMap;
			_FieldAttributes = result._FieldAttributes;
			_EventsRowMap = result._EventsRowMap;
			_EventsFieldMap = result._EventsFieldMap;
			_FieldAttributesFirst = result._FieldAttributesFirst;
			_FieldAttributesLast = result._FieldAttributesLast;
			_EventPositions = result._EventPositions;
			_ClassFields = result._ClassFields;
			_ReverseMap = result._ReverseMap;
			_FieldTypes = result._FieldTypes;
			_BqlFieldsMap = result._BqlFieldsMap;
			_TimestampOrdinal = result._TimestampOrdinal;

			SetAutomationFieldDefaulting(typeof(TNode));

			_Graph = graph;
			if (_CreateExtensions != null)
			{
				_Extensions = PXContext.GetSlot<PXCacheExtensionCollection>();
				if (_Extensions == null)
				{
					_Extensions = PXContext.SetSlot<PXCacheExtensionCollection>(new PXCacheExtensionCollection());
				}
			}
			_Originals = PXContext.GetSlot<PXCacheOriginalCollection>();
			if (_Originals == null)
			{
				_Originals = PXContext.SetSlot<PXCacheOriginalCollection>(new PXCacheOriginalCollection());
			}
			AlteredDescriptor altered = graph.GetAlteredAttributes(typeof(TNode));
			if (altered != null && altered.Fields != null && altered.Fields.Count > 0)
			{
				_GraphSpecificFields = altered.Fields;
			}
			_Items = new PXCollection<TNode>(this);
			{
				Dictionary<object, object> clones;
				Dictionary<string, EventsFieldMap> eventsFieldMap;
				List<IPXRowSelectingSubscriber> mapRowSelecting;
				List<IPXRowSelectedSubscriber> mapRowSelected;
				List<IPXRowInsertingSubscriber> mapRowInserting;
				List<IPXRowInsertedSubscriber> mapRowInserted;
				List<IPXRowUpdatingSubscriber> mapRowUpdating;
				List<IPXRowUpdatedSubscriber> mapRowUpdated;
				List<IPXRowDeletingSubscriber> mapRowDeleting;
				List<IPXRowDeletedSubscriber> mapRowDeleted;
				List<IPXRowPersistingSubscriber> mapRowPersisting;
				List<IPXRowPersistedSubscriber> mapRowPersisted;
				if (altered != null)
				{
					clones = new Dictionary<object, object>(altered._FieldAttributes.Count);
					_AttributesFirst = altered._FieldAttributesFirst;
					_AttributesLast = altered._FieldAttributesLast;
					_CacheAttributes = new List<PXEventSubscriberAttribute>(altered._FieldAttributes.Count);
					foreach (PXEventSubscriberAttribute descr in altered._FieldAttributes)
					{
						PXEventSubscriberAttribute attr = descr.Clone(PXAttributeLevel.Cache);
						_CacheAttributes.Add(attr);
						AddAggregatedAttributes(ref clones, descr, attr);
					}
					eventsFieldMap = altered._EventsFieldMap;
					mapRowSelecting = altered._EventsRowMap.RowSelecting;
					mapRowSelected = altered._EventsRowMap.RowSelected;
					mapRowInserting = altered._EventsRowMap.RowInserting;
					mapRowInserted = altered._EventsRowMap.RowInserted;
					mapRowUpdating = altered._EventsRowMap.RowUpdating;
					mapRowUpdated = altered._EventsRowMap.RowUpdated;
					mapRowDeleting = altered._EventsRowMap.RowDeleting;
					mapRowDeleted = altered._EventsRowMap.RowDeleted;
					mapRowPersisting = altered._EventsRowMap.RowPersisting;
					mapRowPersisted = altered._EventsRowMap.RowPersisted;
				}
				else
				{
					clones = new Dictionary<object, object>(_FieldAttributes.Count);
					_AttributesFirst = _FieldAttributesFirst;
					_AttributesLast = _FieldAttributesLast;
					_CacheAttributes = new List<PXEventSubscriberAttribute>(_FieldAttributes.Count);
					foreach (PXEventSubscriberAttribute descr in _FieldAttributes)
					{
						PXEventSubscriberAttribute attr = descr.Clone(PXAttributeLevel.Cache);
						_CacheAttributes.Add(attr);
						AddAggregatedAttributes(ref clones, descr, attr);
					}
					eventsFieldMap = _EventsFieldMap;
					mapRowSelecting = _EventsRowMap.RowSelecting;
					mapRowSelected = _EventsRowMap.RowSelected;
					mapRowInserting = _EventsRowMap.RowInserting;
					mapRowInserted = _EventsRowMap.RowInserted;
					mapRowUpdating = _EventsRowMap.RowUpdating;
					mapRowUpdated = _EventsRowMap.RowUpdated;
					mapRowDeleting = _EventsRowMap.RowDeleting;
					mapRowDeleted = _EventsRowMap.RowDeleted;
					mapRowPersisting = _EventsRowMap.RowPersisting;
					mapRowPersisted = _EventsRowMap.RowPersisted;
				}

				foreach (KeyValuePair<string, EventsFieldMap> map in eventsFieldMap)
				{
					if (map.Value.CommandPreparing.Count > 0)
					{
						IPXCommandPreparingSubscriber[] arr = new IPXCommandPreparingSubscriber[map.Value.CommandPreparing.Count];
						for (int i = 0; i < arr.Length; i++)
						{
							arr[i] = (IPXCommandPreparingSubscriber)clones[map.Value.CommandPreparing[i]];
						}
						_CommandPreparingEventsAttr[map.Key] = arr;
					}
					if (map.Value.FieldDefaulting.Count > 0)
					{
						IPXFieldDefaultingSubscriber[] arr = new IPXFieldDefaultingSubscriber[map.Value.FieldDefaulting.Count];
						for (int i = 0; i < arr.Length; i++)
						{
							arr[i] = (IPXFieldDefaultingSubscriber)clones[map.Value.FieldDefaulting[i]];
						}
						_FieldDefaultingEventsAttr[map.Key] = arr;
					}
					if (map.Value.FieldUpdating.Count > 0)
					{
						IPXFieldUpdatingSubscriber[] arr = new IPXFieldUpdatingSubscriber[map.Value.FieldUpdating.Count];
						for (int i = 0; i < arr.Length; i++)
						{
							arr[i] = (IPXFieldUpdatingSubscriber)clones[map.Value.FieldUpdating[i]];
						}
						_FieldUpdatingEventsAttr[map.Key] = arr;
					}
					if (map.Value.FieldVerifying.Count > 0)
					{
						IPXFieldVerifyingSubscriber[] arr = new IPXFieldVerifyingSubscriber[map.Value.FieldVerifying.Count];
						for (int i = 0; i < arr.Length; i++)
						{
							arr[i] = (IPXFieldVerifyingSubscriber)clones[map.Value.FieldVerifying[i]];
						}
						_FieldVerifyingEventsAttr[map.Key] = arr;
					}
					if (map.Value.FieldUpdated.Count > 0)
					{
						IPXFieldUpdatedSubscriber[] arr = new IPXFieldUpdatedSubscriber[map.Value.FieldUpdated.Count];
						for (int i = 0; i < arr.Length; i++)
						{
							arr[i] = (IPXFieldUpdatedSubscriber)clones[map.Value.FieldUpdated[i]];
						}
						_FieldUpdatedEventsAttr[map.Key] = arr;
					}
					if (map.Value.FieldSelecting.Count > 0)
					{
						IPXFieldSelectingSubscriber[] arr = new IPXFieldSelectingSubscriber[map.Value.FieldSelecting.Count];
						for (int i = 0; i < arr.Length; i++)
						{
							arr[i] = (IPXFieldSelectingSubscriber)clones[map.Value.FieldSelecting[i]];
						}
						_FieldSelectingEventsAttr[map.Key] = arr;
					}
					if (map.Value.ExceptionHandling.Count > 0)
					{
						IPXExceptionHandlingSubscriber[] arr = new IPXExceptionHandlingSubscriber[map.Value.ExceptionHandling.Count];
						for (int i = 0; i < arr.Length; i++)
						{
							arr[i] = (IPXExceptionHandlingSubscriber)clones[map.Value.ExceptionHandling[i]];
						}
						_ExceptionHandlingEventsAttr[map.Key] = arr;
					}
				}

				if (mapRowSelecting.Count > 0)
				{
					IPXRowSelectingSubscriber[] arr = new IPXRowSelectingSubscriber[mapRowSelecting.Count];
					for (int i = 0; i < arr.Length; i++)
					{
						arr[i] = (IPXRowSelectingSubscriber)clones[mapRowSelecting[i]];
					}
					_EventsRowAttr.RowSelecting = arr;
				}
				if (mapRowSelected.Count > 0)
				{
					IPXRowSelectedSubscriber[] arr = new IPXRowSelectedSubscriber[mapRowSelected.Count];
					for (int i = 0; i < arr.Length; i++)
					{
						arr[i] = (IPXRowSelectedSubscriber)clones[mapRowSelected[i]];
					}
					_EventsRowAttr.RowSelected = arr;
				}
				if (mapRowInserting.Count > 0)
				{
					IPXRowInsertingSubscriber[] arr = new IPXRowInsertingSubscriber[mapRowInserting.Count];
					for (int i = 0; i < arr.Length; i++)
					{
						arr[i] = (IPXRowInsertingSubscriber)clones[mapRowInserting[i]];
					}
					_EventsRowAttr.RowInserting = arr;
				}
				if (mapRowInserted.Count > 0)
				{
					IPXRowInsertedSubscriber[] arr = new IPXRowInsertedSubscriber[mapRowInserted.Count];
					for (int i = 0; i < arr.Length; i++)
					{
						arr[i] = (IPXRowInsertedSubscriber)clones[mapRowInserted[i]];
					}
					_EventsRowAttr.RowInserted = arr;
				}
				if (mapRowUpdating.Count > 0)
				{
					IPXRowUpdatingSubscriber[] arr = new IPXRowUpdatingSubscriber[mapRowUpdating.Count];
					for (int i = 0; i < arr.Length; i++)
					{
						arr[i] = (IPXRowUpdatingSubscriber)clones[mapRowUpdating[i]];
					}
					_EventsRowAttr.RowUpdating = arr;
				}
				if (mapRowUpdated.Count > 0)
				{
					IPXRowUpdatedSubscriber[] arr = new IPXRowUpdatedSubscriber[mapRowUpdated.Count];
					for (int i = 0; i < arr.Length; i++)
					{
						arr[i] = (IPXRowUpdatedSubscriber)clones[mapRowUpdated[i]];
					}
					_EventsRowAttr.RowUpdated = arr;
				}
				if (mapRowDeleting.Count > 0)
				{
					IPXRowDeletingSubscriber[] arr = new IPXRowDeletingSubscriber[mapRowDeleting.Count];
					for (int i = 0; i < arr.Length; i++)
					{
						arr[i] = (IPXRowDeletingSubscriber)clones[mapRowDeleting[i]];
					}
					_EventsRowAttr.RowDeleting = arr;
				}
				if (mapRowDeleted.Count > 0)
				{
					IPXRowDeletedSubscriber[] arr = new IPXRowDeletedSubscriber[mapRowDeleted.Count];
					for (int i = 0; i < arr.Length; i++)
					{
						arr[i] = (IPXRowDeletedSubscriber)clones[mapRowDeleted[i]];
					}
					_EventsRowAttr.RowDeleted = arr;
				}
				if (mapRowPersisting.Count > 0)
				{
					IPXRowPersistingSubscriber[] arr = new IPXRowPersistingSubscriber[mapRowPersisting.Count];
					for (int i = 0; i < arr.Length; i++)
					{
						arr[i] = (IPXRowPersistingSubscriber)clones[mapRowPersisting[i]];
					}
					_EventsRowAttr.RowPersisting = arr;
				}
				if (mapRowPersisted.Count > 0)
				{
					IPXRowPersistedSubscriber[] arr = new IPXRowPersistedSubscriber[mapRowPersisted.Count];
					for (int i = 0; i < arr.Length; i++)
					{
						arr[i] = (IPXRowPersistedSubscriber)clones[mapRowPersisted[i]];
					}
					_EventsRowAttr.RowPersisted = arr;
				}
			}

			Type tkey = typeof(TNode);
			while (tkey != typeof(object))
			{
				if (!_Graph.Caches.ContainsKey(tkey))
				{
					_Graph.Caches[tkey] = this;
				}
				if ((tkey == typeof(TNode) || typeof(TNode).IsSubclassOf(tkey)) && Attribute.IsDefined(tkey, typeof(PXBreakInheritanceAttribute), false)
					|| _Graph.GetType() == typeof(PXGraph) || _Graph.GetType() == typeof(PXGenericInqGrph) || _Graph.GetType() == typeof(PX.Data.Maintenance.GI.GenericInquiryDesigner))
				{
					break;
				}
				tkey = tkey.BaseType;
			}
			List<PXEventSubscriberAttribute> secondPassInit = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute descr in _CacheAttributes)
			{
				if (!(descr is PXDBAttributeAttribute))
					descr.InvokeCacheAttached(this);
				else
					secondPassInit.Add(descr);
			}
			foreach (PXEventSubscriberAttribute descr in secondPassInit)
			{
				descr.InvokeCacheAttached(this);
			}

			if (altered != null && altered._Method != null)
			{
				altered._Method(_Graph, this);
			}
			if (typeof(TNode).IsDefined(typeof(PXDBInterceptorAttribute), true))
			{
				object[] interceptors = typeof(TNode).GetCustomAttributes(typeof(PXDBInterceptorAttribute), true);
				_Interceptor = (PXDBInterceptorAttribute)interceptors[0];
				if (_ExtensionTables != null && _ExtensionTables.Count > 0
					&& _ExtensionTables.Last().BaseType.IsGenericType
					&& (_ExtensionTables.Last().BaseType.GetGenericArguments().Last() == typeof(TNode)
					|| _ExtensionTables.Last().BaseType.GetGenericArguments().Last().IsAssignableFrom(typeof(TNode))))
				{
					interceptors = _ExtensionTables[_ExtensionTables.Count - 1].GetCustomAttributes(typeof(PXDBInterceptorAttribute), true);
					_Interceptor.Child = (PXDBInterceptorAttribute)interceptors[0];
				}
				_Interceptor.CacheAttached(this);
				_BqlSelect = _Interceptor.GetTableCommand();
			}
			else if (_ExtensionTables != null && _ExtensionTables.Count > 0)
			{
				object[] interceptors = _ExtensionTables[_ExtensionTables.Count - 1].GetCustomAttributes(typeof(PXDBInterceptorAttribute), true);
				_Interceptor = (PXDBInterceptorAttribute)interceptors[0];
				_Interceptor.CacheAttached(this);
				_BqlSelect = _Interceptor.GetTableCommand();
			}

            if (typeof(TNode).IsDefined(typeof(PXClassAttribute), true))
            {
                object[] attrs = typeof(TNode).GetCustomAttributes(typeof(PXClassAttribute), true);
                for (int i = 0; i < attrs.Length; i++)
                {
                    PXClassAttribute attr = (PXClassAttribute)attrs[i];
                    attr.CacheAttached(this);
                }
            }

			StringBuilder bld = new StringBuilder();
			foreach (string key in Keys)
			{
				bld.Append(key);
			}
			string keysequence = bld.ToString();
			_DelegatesLock.AcquireReaderLock(-1);
			try
			{
				if (!_DelegatesEquals.TryGetValue(keysequence, out _Equals)
					|| !_DelegatesGetHashCode.TryGetValue(keysequence, out _GetHashCode))
				{
					System.Threading.LockCookie c = _DelegatesLock.UpgradeToWriterLock(-1);
					try
					{
						if (!_DelegatesEquals.TryGetValue(keysequence, out _Equals)
							|| !_DelegatesGetHashCode.TryGetValue(keysequence, out _GetHashCode))
						{
							DynamicMethod dm_eq;
							DynamicMethod dm_hash;
							if (!PXGraph.IsRestricted)
							{
								dm_eq = new DynamicMethod("_Equals", typeof(bool), new Type[] { typeof(TNode), typeof(TNode) }, this.GetType());
								dm_hash = new DynamicMethod("_GetHashCode", typeof(int), new Type[] { typeof(TNode) }, this.GetType());
							}
							else
							{
								dm_eq = new DynamicMethod("_Equals", typeof(bool), new Type[] { typeof(TNode), typeof(TNode) });
								dm_hash = new DynamicMethod("_GetHashCode", typeof(int), new Type[] { typeof(TNode) });
							}
							ILGenerator il_eq = dm_eq.GetILGenerator();
							System.Reflection.Emit.Label ret_eq_false = il_eq.DefineLabel();
							System.Reflection.Emit.Label ret_eq = il_eq.DefineLabel();
							ILGenerator il_hash = dm_hash.GetILGenerator();
							il_hash.DeclareLocal(typeof(int));
							il_hash.Emit(OpCodes.Ldc_I4_S, 13);
							il_hash.Emit(OpCodes.Stloc_0);
							il_eq.DeclareLocal(typeof(bool));
							MethodInfo equality = typeof(object).GetMethod("Equals", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object), typeof(object) }, null);
							foreach (string key in Keys)
							{
								PropertyInfo prop = typeof(TNode).GetProperty(key);
								Type propType = prop.PropertyType;
								if (prop.CanRead)
								{
									MethodInfo getter = prop.GetGetMethod();

									il_eq.Emit(OpCodes.Ldarg_0);
									il_eq.Emit(OpCodes.Callvirt, getter);
									if (propType.IsValueType)
									{
										il_eq.Emit(OpCodes.Box, propType);
									}
									il_eq.Emit(OpCodes.Ldarg_1);
									il_eq.Emit(OpCodes.Callvirt, getter);
									if (propType.IsValueType)
									{
										il_eq.Emit(OpCodes.Box, propType);
									}
									il_eq.Emit(OpCodes.Call, equality);
									il_eq.Emit(OpCodes.Brfalse, ret_eq_false);


									LocalBuilder local = il_hash.DeclareLocal(propType);
									il_hash.Emit(OpCodes.Ldloc_0);
									il_hash.Emit(OpCodes.Ldc_I4_S, 37);
									il_hash.Emit(OpCodes.Mul);
									il_hash.Emit(OpCodes.Stloc_0);
									il_hash.Emit(OpCodes.Ldarg_0);
									il_hash.Emit(OpCodes.Callvirt, getter);
									il_hash.Emit(OpCodes.Stloc_S, local.LocalIndex);
									if (!propType.IsValueType)
									{
										System.Reflection.Emit.Label hash_next = il_hash.DefineLabel();
										il_hash.Emit(OpCodes.Ldnull);
										il_hash.Emit(OpCodes.Ldloc_S, local.LocalIndex);
										il_hash.Emit(OpCodes.Ceq);
										il_hash.Emit(OpCodes.Brtrue_S, hash_next);
										il_hash.Emit(OpCodes.Ldloc_S, local.LocalIndex);
										il_hash.Emit(OpCodes.Callvirt, propType.GetMethod("GetHashCode", new Type[0]));
										il_hash.Emit(OpCodes.Ldloc_0);
										il_hash.Emit(OpCodes.Add);
										il_hash.Emit(OpCodes.Stloc_0);
										il_hash.MarkLabel(hash_next);
									}
									else
									{
										il_hash.Emit(OpCodes.Ldloca_S, local.LocalIndex);
										il_hash.Emit(OpCodes.Call, propType.GetMethod("GetHashCode", new Type[0]));
										il_hash.Emit(OpCodes.Ldloc_0);
										il_hash.Emit(OpCodes.Add);
										il_hash.Emit(OpCodes.Stloc_0);
									}
								}
							}
							il_eq.Emit(OpCodes.Ldc_I4_1);
							il_eq.Emit(OpCodes.Stloc_0);
							il_eq.Emit(OpCodes.Br, ret_eq);
							il_eq.MarkLabel(ret_eq_false);
							il_eq.Emit(OpCodes.Ldc_I4_0);
							il_eq.Emit(OpCodes.Stloc_0);
							il_eq.MarkLabel(ret_eq);
							il_eq.Emit(OpCodes.Ldloc_0);
							il_eq.Emit(OpCodes.Ret);
							il_hash.Emit(OpCodes.Ldloc_0);
							il_hash.Emit(OpCodes.Ret);
							_DelegatesEquals[keysequence] = _Equals = (EqualsDelegate)dm_eq.CreateDelegate(typeof(EqualsDelegate));
							_DelegatesGetHashCode[keysequence] = _GetHashCode = (GetHashCodeDelegate)dm_hash.CreateDelegate(typeof(GetHashCodeDelegate));
						}
					}
					finally
					{
						_DelegatesLock.DowngradeFromWriterLock(ref c);
					}
				}
			}
			finally
			{
				_DelegatesLock.ReleaseReaderLock();
			}

			//if (initaliases)
			//{
			//    lock (_FieldAliases)
			//    {
			//        foreach (string key in Keys)
			//        {
			//            _FieldAliases[key] = _BqlTables[0].Name;
			//        }
			//        initaliases = false;
			//    }
			//}
		}
		#endregion

		#region Data manipulation methods
		//protected static string _TableName;
		//public override string TableName
		//{
		//    get
		//    {
		//        return _TableName;
		//    }
		//    set
		//    {
		//        _TableName = value;
		//    }
		//}
		public override object Select(PXDataRecord record, ref int position, bool isReadOnly, out bool wasUpdated)
		{
			wasUpdated = false;
			TNode item = new TNode();
			OnRowSelecting(item, record, ref position, isReadOnly || (_Interceptor != null && !_Interceptor.CacheSelected));
			if (!AllowSelect)
			{
				return new TNode();
			}
			//if (!isKeysFilled(item))
			//{
			//    return null;
			//}
			if (!isReadOnly && (_Interceptor == null || _Interceptor.CacheSelected))
			{
				if (_ItemsDenormalized)
				{
					_Items.Normalize(null);
					_ItemsDenormalized = false;
				}
				object placed;
				if (_ChangedKeys == null || !_ChangedKeys.ContainsKey(item))
				{
					placed = _Items.PlaceNotChanged(item, out wasUpdated);
				}
				else
				{
					placed = _Items.PlaceNotChanged(_ChangedKeys[item], out wasUpdated);
				}
				if (_CurrentPlacedIntoCache != null && !wasUpdated && object.ReferenceEquals(placed, _CurrentPlacedIntoCache))
				{
					bool restore = true;
					if (_TimestampOrdinal != null)
					{
						byte[] oldstamp = GetValue(placed, _TimestampOrdinal.Value) as byte[];
						byte[] newstamp = GetValue(item, _TimestampOrdinal.Value) as byte[];
						if (newstamp != null)
						{
							if (oldstamp != null)
							{
								restore = false;
								for (int m = 0; m < oldstamp.Length && m < newstamp.Length; m++)
								{
									if (oldstamp[m] != newstamp[m])
									{
										restore = true;
										break;
									}
								}
							}
						}
						else
						{
							restore = false;
						}
					}
					if (restore)
					{
						RestoreCopy(placed, item);
						_CurrentPlacedIntoCache = null;
					}
				}
				return placed;
			}
			else
			{
				return item;
			}
		}

		public override PXEntryStatus GetStatus(object item)
		{
			return _Items.GetStatus((TNode)item);
		}
		public override void SetStatus(object item, PXEntryStatus status)
		{
			if (item != null)
			{
				_Items.SetStatus((TNode)item, status);
				if (status == PXEntryStatus.Updated)
				{
					try
					{
						BqlTablePair orig = null;
						lock (((ICollection)_Originals).SyncRoot)
						{
							if (!_Originals.TryGetValue((TNode)item, out orig))
							{
								TNode output;
								if ((output = readItem((TNode)item, true)) != null)
								{
									_Originals[(TNode)item] = new BqlTablePair { Unchanged = output };
								}
							}
						}
					}
					catch
					{
					}
				}
			}
		}
		public override object Locate(object item)
		{
			return _Items.Locate((TNode)item);
		}
		protected internal override bool IsPresent(object item)
		{
			TNode placed;
			return (isKeysFilled((TNode)item) && (placed = _Items.Locate((TNode)item)) != null
				&& (_CurrentPlacedIntoCache == null || !object.ReferenceEquals(placed, _CurrentPlacedIntoCache)));
		}
		protected internal override bool IsGraphSpecificField(string fieldName)
		{
			return _GraphSpecificFields != null && fieldName != null && _GraphSpecificFields.Contains(fieldName);
		}
		public override int Locate(IDictionary keys)
		{
			if (!_AllowSelect)
			{
				return 0;
			}

			TNode item = (TNode)new TNode();
			FillWithValues(item, null, keys, PXCacheOperation.Update);

			TNode placed = _Items.Locate(item);

			if (placed == null)
			{
				if (readItem(item) != null)
				{
					bool wasUpdated;
					placed = _Items.PlaceNotChanged(item, out wasUpdated);
				}
			}

			if (placed == null)
			{
				return 0;
			}

			Current = placed;
			return 1;
		}
		public override void Remove(object item)
		{
			_Items.Remove((TNode)item);
		}

		/// <summary>
		/// Reads row from Database. Raise event OnRowSelecting.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		protected virtual TNode readItem(TNode item)
		{
			return readItem(item, false);
		}
		protected virtual TNode readItem(TNode item, bool donotplace)
		{
			if (isKeysFilled(item))
			{
				if (_Interceptor == null)
				{
					List<PXDataField> pars = new List<PXDataField>();
					foreach (string descr in _ClassFields)
					{
						object val = null;
						if (_Keys.Contains(descr))
						{
							val = GetValue(item, descr);
						}
						PXCommandPreparingEventArgs.FieldDescription description = null;
						OnCommandPreparing(descr, item, val, PXDBOperation.Select | PXDBOperation.Internal, null, out description);
						if (description != null && !String.IsNullOrEmpty(description.FieldName))
						{
							pars.Add(new PXDataField(description.FieldName));
							if (val != null)
							{
								pars.Add(new PXDataFieldValue(description.FieldName, description.DataType, description.DataLength, description.DataValue));
							}
						}
					}
					using (PXDataRecord record = _Graph.ProviderSelectSingle(_BqlTable, pars.ToArray()))
					{
						if (record != null)
						{
							TNode output = new TNode();
							int position = 0;
							OnRowSelecting(output, record, ref position, donotplace);
							if (donotplace)
							{
								return output;
							}
							bool wasUpdated;
							return _Items.PlaceNotChanged(output, out wasUpdated);
						}
					}
				}
				else
				{
					List<PXDataValue> pars = new List<PXDataValue>();
					foreach (string descr in _ClassFields)
					{
						if (_Keys.Contains(descr))
						{
							PXCommandPreparingEventArgs.FieldDescription description = null;
							OnCommandPreparing(descr, item, GetValue(item, descr), PXDBOperation.Select | PXDBOperation.Internal, null, out description);
							if (description != null)
							{
								pars.Add(new PXDataValue(description.DataType, description.DataLength, description.DataValue));
							}
						}
					}
					foreach (PXDataRecord record in _Graph.ProviderSelect(_Interceptor.GetRowCommand(), 1, pars.ToArray()))
					{
						TNode output = new TNode();
						int position = 0;
						OnRowSelecting(output, record, ref position, donotplace);
						if (donotplace)
						{
							return output;
						}
						bool wasUpdated;
						return _Items.PlaceNotChanged(output, out wasUpdated);
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Updates item in the cache.
		/// </summary>
		/// <param name="viewName">The selection command alias.</param>
		/// <param name="keys">Primary key of the item.</param>
		/// <param name="values">New field values to update the item with.</param>
		/// <returns>1 if updated successfully, otherwise 0.</returns>
		public override int Update(IDictionary keys, IDictionary values)
		{
			if (!_AllowUpdate)
			{
				return 0;
			}

			TNode item = (TNode)new TNode();
			FillWithValues(item, null, keys, PXCacheOperation.Update);

			if (_ChangedKeys != null && _ChangedKeys.ContainsKey(item))
			{
				throw new PXBadDictinaryException();
			}

			TNode placed = _Items.PlaceUpdated(item, false);

			BqlTablePair orig = null;

			if (placed == null && Keys.Count == 0)
			{
				placed = Current as TNode;
			}

			if (placed == null)
			{
				TNode output;
				if ((output = readItem(item)) != null)
				{
					placed = _Items.PlaceUpdated(item, true);
					try
					{
						if (placed != null)
						{
							lock (((ICollection)_Originals).SyncRoot)
							{
								_Originals[placed] = orig = new BqlTablePair { Unchanged = CreateCopy(output) };
							}
						}
					}
					catch
					{
					}
				}
				else
				{
					Dictionary<string, object> inserted = new Dictionary<string, object>();
					bool nokeys = true;
					if (keys != null)
					{
						foreach (DictionaryEntry entry in keys)
						{
							inserted[(string)entry.Key] = entry.Value;
							if (entry.Value != null)
							{
								nokeys = false;
							}
						}
					}
					if (values != null)
					{
						foreach (DictionaryEntry entry in values)
						{
							if (!inserted.ContainsKey((string)entry.Key) || entry.Value != NotSetValue)
							{
								inserted[(string)entry.Key] = entry.Value;
							}
						}
					}
					bool allowinsertswitched = false;
					bool isdirtystored = _IsDirty;
					int ret;
					if (nokeys && !_AllowInsert)
					{
						allowinsertswitched = true;
						_AllowInsert = true;
					}
					try
					{
						ret = Insert(inserted);
					}
					finally
					{
						if (allowinsertswitched)
						{
							_AllowInsert = false;
						}
					}
					if (allowinsertswitched && ret > 0)
					{
						nokeys = false;
						foreach (string key in Keys)
						{
							if (!inserted.ContainsKey(key) || PXFieldState.UnwrapValue(inserted[key]) == null)
							{
								nokeys = true;
							}
						}
						if (nokeys)
						{
							Delete(Current);
							_IsDirty = isdirtystored;
							return 0;
						}
					}
					if (values != null)
					{
						foreach (string key in values.Keys.ToArray<Object>())
						{
							if (ret > 0)
							{
								values[key] = inserted[key];
							}
							else
							{
								PXFieldState fs = inserted[key] as PXFieldState;
								if (fs != null && !String.IsNullOrEmpty(fs.Error))
								{
									values[key] = fs;
								}
								else if (key.EndsWith("_description", StringComparison.OrdinalIgnoreCase))
								{
									values[key] = inserted[key];
								}
							}
						}
					}
					if (keys != null)
					{
						foreach (string key in keys.Keys.ToArray<Object>())
						{
							object val = inserted[key];
							PXFieldState fs = val as PXFieldState;
							if (fs != null)
							{
								val = fs.Value;
							}
							keys[key] = val;
						}
					}
					return ret;
				}
			}
			else
			{
				try
				{
					lock (((ICollection)_Originals).SyncRoot)
					{
						if (!_Originals.TryGetValue(placed, out orig))
						{
							_OriginalsRequested++;
							TNode output;
							if ((output = readItem(item, true)) != null)
							{
								_Originals[placed] = orig = new BqlTablePair { Unchanged = output };
							}
							if (_OriginalsRequested >= 5)
							{
								_OriginalsRequested = 0;
								int i = 0;
								foreach (TNode cached in _Items.NotChanged)
								{
									i++;
									if (i >= 50)
									{
										break;
									}
									_Originals[cached] = new BqlTablePair { Unchanged = CreateCopy(cached), LastModified = CreateCopy(cached) };
								}
							}
						}
					}
				}
				catch
				{
				}
			}

			if (placed == null)
			{
				return 0;
			}

			Current = placed;
			TNode copy = CreateCopy(placed);
			if (orig != null)
			{
				orig.LastModified = copy;
			}

			bool cancel = false;

			bool normalized = false;
			bool keysUpdated = false;
			if (values != null)
			{
				if (!_AllowUpdate)
				{
					return 0;
				}
				if (!DisableCloneAttributes) 
					GetAttributes(placed, null);
				try
				{
					if (FillWithValues(placed, copy, values, PXCacheOperation.Update))
					{
						try
						{
							_Items.Normalize(placed);
							normalized = true;
						}
						catch (PXBadDictinaryException)
						{
							RestoreCopy(placed, copy);
							throw;
						}
						keysUpdated = true;
					}
				}
				catch (PXDialogRequiredException)
				{
					cancel = true;
					throw;
				}
				finally
				{
					if (cancel)
					{
						RestoreCopy(placed, copy);
					}
				}
			}

			try
			{
				cancel = !OnRowUpdating(copy, placed, true);
			}
			catch (Exception)
			{
				cancel = true;
				throw;
			}
			finally
			{
				if (cancel)
				{
					RestoreCopy(placed, copy);
					if (normalized)
					{
						_Items.Normalize(null);
					}
				}
			}
			if (cancel)
			{
				if (values != null)
				{
					foreach (string key in values.Keys.ToArray<Object>())
					{
						if (Fields.Contains(key))
						{
							values[key] = GetValueExt(placed, key);
						}
					}
				}
				if (keys != null)
				{
					foreach (string key in keys.Keys.ToArray<Object>())
					{
						object val = GetValueExt(placed, key);
						PXFieldState fs = val as PXFieldState;
						if (fs != null)
						{
							val = fs.Value;
						}
						keys[key] = val;
					}
				}
				return 0;
			}

			_IsDirty = true;

			Current = placed;

			OnRowUpdated(placed, copy, true);

			if (values != null)
			{
				foreach (string key in values.Keys.ToArray<Object>())
				{
					if (Fields.Contains(key))
					{
						values[key] = GetValueExt(placed, key);
					}
				}
			}
			if (keys != null)
			{
				foreach (string key in keys.Keys.ToArray<Object>())
				{
					object val = GetValueExt(placed, key);
					PXFieldState fs = val as PXFieldState;
					if (fs != null)
					{
						val = fs.Value;
					}
					keys[key] = val;
				}
			}

			if (orig != null)
			{
				try
				{
					orig.LastModified = CreateCopy(placed);
					if (keysUpdated && orig.Unchanged is TNode && !object.ReferenceEquals(placed, orig.Unchanged) && !ObjectsEqual(placed, orig.Unchanged))
					{
						if (_ChangedKeys == null)
						{
							_ChangedKeys = new Dictionary<TNode, TNode>(new ItemComparer(this));
						}
						_ChangedKeys[(TNode)orig.Unchanged] = placed;
						ClearQueryCache();
					}
				}
				catch
				{
				}
			}

			return 1;
		}

		public override object Update(object data)
		{
			return Update(data, false);
		}

		protected internal override object Update(object data, bool bypassinterceptor)
		{
			bool checkpending = _PendingItems == null;
			try
			{
				if (checkpending)
				{
					_PendingItems = new List<TNode>();
				}

				if (!bypassinterceptor && _Interceptor != null)
				{
					return _Interceptor.Update(this, data);
				}
				if (data is PXResult)
				{
					data = ((PXResult)data)[0];
				}
				TNode item = data as TNode;
				if (item == null)
				{
					return null;
				}

				TNode placed = _Items.PlaceUpdated(item, false);

				BqlTablePair orig = null;

				if (placed == null)
				{
					TNode output;
					if ((output = readItem(item, true)) != null)
					{
						placed = _Items.PlaceUpdated(item, true);
						try
						{
							if (placed != null)
							{
								lock (((ICollection)_Originals).SyncRoot)
								{
									_Originals[placed] = orig = new BqlTablePair { LastModified = CreateCopy(output), Unchanged = CreateCopy(output) };
								}
							}
						}
						catch
						{
						}
					}
					else
					{
						return Insert(data);
					}
				}
				else
				{
					try
					{
						lock (((ICollection)_Originals).SyncRoot)
						{
							if (!_Originals.TryGetValue(placed, out orig))
							{
								_OriginalsRequested++;
								TNode output;
								if ((output = readItem(item, true)) != null)
								{
									_Originals[placed] = orig = new BqlTablePair { LastModified = CreateCopy(output), Unchanged = output };
								}
								if (_OriginalsRequested >= 5)
								{
									_OriginalsRequested = 0;
									int i = 0;
									foreach (TNode cached in _Items.NotChanged)
									{
										i++;
										if (i >= 50)
										{
											break;
										}
										_Originals[cached] = new BqlTablePair { Unchanged = CreateCopy(cached), LastModified = CreateCopy(cached) };
									}
								}
							}
							else if (orig.LastModified == null)
							{
								orig.LastModified = CreateCopy((TNode)orig.Unchanged);
							}
						}
					}
					catch
					{
					}
				}

				if (placed == null)
				{
					return null;
				}

				bool thesame = Object.ReferenceEquals(placed, item);
				bool keysUpdated = false;

				if (thesame)
				{
					if (orig != null && orig.LastModified is TNode && !Object.ReferenceEquals(orig.LastModified, item))
					{
						item = CreateCopy(item);
						TNode lastModified = (TNode)orig.LastModified;
						foreach (string key in Keys)
						{
							if (keysUpdated = keysUpdated || !object.Equals(GetValue(item, key), GetValue(lastModified, key)))
							{
								break;
							}
						}
						RestoreCopy(placed, orig.LastModified);
						Current = placed;
						if (keysUpdated)
						{
							Normalize();
						}
						thesame = false;
					}
				}

				TNode copy = CreateCopy(placed);

				if (!thesame)
				{
					FillWithValues(placed, copy, item);
					if (keysUpdated)
					{
						Normalize();
					}
				}

				bool cancel = false;
				try
				{
					cancel = !OnRowUpdating(copy, placed, false);
				}
				catch (Exception)
				{
					cancel = true;
					throw;
				}
				finally
				{
					if (cancel)
					{
						RestoreCopy(placed, copy);
					}
				}

				if (cancel)
				{
					return null;
				}

				_IsDirty = true;

				Current = placed;

				OnRowUpdated(placed, thesame ? placed : copy, false);

				if (orig != null)
				{
					try
					{
						orig.LastModified = CreateCopy(placed);
					}
					catch
					{
					}
				}

				return placed;
			}
			finally
			{
				if (checkpending)
				{
					for (int i = 0; i < _PendingItems.Count; i++)
					{
						if (_Items.Locate(_PendingItems[i]) == null)
						{
							_ItemAttributes.Remove(_PendingItems[i]);
						}
					}
					_PendingItems = null;
				}
			}
		}

		/// <summary>
		/// Inserts item in the cache.
		/// </summary>
		/// <param name="values">Field values to populate the item before inserting.</param>
		/// <returns>1 if inserted successfully, otherwise 0.</returns>
		public override int Insert(IDictionary values)
		{
			if (!_AllowInsert)
			{
				return 0;
			}

			TNode item = (TNode)new TNode();
			if(!DisableCloneAttributes)
				GetAttributes(item, null);
			FillWithValues(item, null, values, PXCacheOperation.Insert);

			if (_ChangedKeys != null && _ChangedKeys.ContainsKey(item))
			{
				throw new PXBadDictinaryException();
			}

			string[] keys = new string[values.Keys.Count];
			values.Keys.CopyTo(keys, 0);

			if (!OnRowInserting(item, true))
			{
				foreach (string key in keys)
				{
					if (Fields.Contains(key))
					{
						values[key] = GetValueExt(item, key);
					}
				}
				return 0;
			}

			//readItem(item);

			bool deleted;
			item = _Items.PlaceInserted(item, out deleted);
			if (deleted)
			{
				ClearQueryCache();
			}

			if (item == null)
			{
				return 0;
			}

			_IsDirty = true;

			Current = item;

			OnRowInserted(item, true);

			try
			{
				lock (((ICollection)_Originals).SyncRoot)
				{
					_Originals[item] = new BqlTablePair { LastModified = CreateCopy(item) };
				}
			}
			catch
			{
			}

			foreach (string key in keys)
			{
				if (Fields.Contains(key))
				{
					values[key] = GetValueExt(item, key);
				}
			}
			foreach (string key in _Keys)
			{
				values[key] = GetValueExt(item, key);
			}

			return 1;
		}

		internal override object FillItem(IDictionary values)
		{
			TNode item = (TNode)new TNode();
			if (!DisableCloneAttributes)
				GetAttributes(item, null);
			FillWithValues(item, null, values, PXCacheOperation.Insert, false);
			return item;
		}

		public override object Insert(object data)
		{
			return Insert(data, false);
		}

		protected internal override object Insert(object data, bool bypassinterceptor)
		{
			bool checkpending = _PendingItems == null;
			try
			{
				if (checkpending)
				{
					_PendingItems = new List<TNode>();
				}

                if (_PendingExceptions == null)
                {
                    _PendingExceptions = new Dictionary<TNode, List<Exception>>();
                }

				if (!bypassinterceptor && _Interceptor != null)
				{
					return _Interceptor.Insert(this, data);
				}
				if (data is PXResult)
				{
					data = ((PXResult)data)[0];
				}
				TNode item = data as TNode;
				if (item == null)
				{
					return null;
				}
				FillWithValues(ref item);

				if (!OnRowInserting(item, false))
				{
					return null;
				}

                List<Exception> pending;
                if (_PendingExceptions.TryGetValue(item, out pending) && pending.Count > 0)
                {
                    throw pending[0];
                }

				//readItem(item);

				bool deleted;
				TNode placed = _Items.PlaceInserted(item, out deleted);
				if (deleted)
				{
					ClearQueryCache();
				}

				if (placed == null)
				{
					return null;
				}

				_IsDirty = true;

				Current = placed;

				OnRowInserted(placed, false);

				try
				{
					lock (((ICollection)_Originals).SyncRoot)
					{
						_Originals[placed] = new BqlTablePair { LastModified = CreateCopy(placed) };
					}
				}
				catch
				{
				}

				return item;
			}
			finally
			{
				if (checkpending)
				{
					if (_ItemAttributes != null)
					{
						for (int i = 0; i < _PendingItems.Count; i++)
						{
							if (_Items.Locate(_PendingItems[i]) == null)
							{
								_ItemAttributes.Remove(_PendingItems[i]);
							}
						}
					}
					_PendingItems = null;
				}
			}
		}

		private static CacheStaticInfo _GetInitializer()
		{
			return _Initialize(false);
		}

		public override object Extend<Parent>(Parent item)
		{
			if (!typeof(TNode).IsSubclassOf(typeof(Parent)))
			{
				throw new PXArgumentException("Parent", ErrorMessages.ArgumentOutOfRangeException);
			}
			TNode data = new TNode();
			PXCacheExtension[] itemextension = null;
			if (PXCache<Parent>._GetInitializer()._CreateExtensions != null)
			{
				PXCacheExtensionCollection dict = _Extensions;
				if (dict == null)
				{
					dict = PXContext.GetSlot<PXCacheExtensionCollection>()
						?? PXContext.SetSlot<PXCacheExtensionCollection>(new PXCacheExtensionCollection());
				}
				lock (((ICollection)dict).SyncRoot)
				{
					if (!dict.TryGetValue(item, out itemextension))
					{
						dict[item] = itemextension = PXCache<Parent>._GetInitializer()._CreateExtensions(item);
					}
				}
			}
			foreach (string name in PXCache<Parent>._GetInitializer()._ClassFields)
			{
				SetValue(data, name, PXCache<Parent>._GetInitializer()._GetValueByOrdinal(item, PXCache<Parent>._GetInitializer()._FieldsMap[name], itemextension));
			}
			TNode loc = (TNode)Locate(data);
			if (loc != null && GetStatus(loc) != PXEntryStatus.Deleted && GetStatus(loc) != PXEntryStatus.InsertedDeleted)
			{
				if (_Extensions == null)
				{
					foreach (string name in _ClassFields)
					{
						if (!PXCache<Parent>._GetInitializer()._ClassFields.Contains(name))
						{
							int ordinal = _FieldsMap[name];
							_SetValueByOrdinal(data, ordinal, _GetValueByOrdinal(loc, ordinal, null), null);
						}
					}
				}
				else
				{
					PXCacheExtension[] dataextension;
					PXCacheExtension[] locextension;
					lock (((ICollection)_Extensions).SyncRoot)
					{
						if (!_Extensions.TryGetValue(data, out dataextension))
						{
							_Extensions[data] = dataextension = _CreateExtensions(data);
						}
						if (!_Extensions.TryGetValue(loc, out locextension))
						{
							_Extensions[loc] = locextension = _CreateExtensions(loc);
						}
					}
					foreach (string name in _ClassFields)
					{
						if (!PXCache<Parent>._GetInitializer()._ClassFields.Contains(name))
						{
							int ordinal = _FieldsMap[name];
							_SetValueByOrdinal(data, ordinal, _GetValueByOrdinal(loc, ordinal, locextension), dataextension);
						}
					}
				}
				return Update(data);
			}
			TNode ins = (TNode)Insert(data);
			if (ins != null)
			{
				if (!ObjectsEqual(ins, data))
				{
					TNode lastModified = null;
					lock (((ICollection)_Originals).SyncRoot)
					{
						BqlTablePair orig;
						if (_Originals.TryGetValue(ins, out orig))
						{
							lastModified = (TNode)orig.LastModified;
						}
					}
					foreach (string key in Keys)
					{
						object val;
						SetValue(ins, key, val = GetValue(data, key));
						if (lastModified != null)
						{
							SetValue(lastModified, key, val);
						}
					}
					Normalize();
				}
				SetStatus(ins, PXEntryStatus.Updated);
			}
			return ins;
		}

		public override object Insert()
		{
			return Insert(new TNode());
		}

		public override object CreateInstance()
		{
			return new TNode();
		}

		public override void ClearQueryCache()
		{

			//if (_Graph.TypedViews._QueriesLoaded)
			//{
				foreach (var viewQueries in _Graph.QueryCache.Values)
				{
					if (viewQueries.CacheType == typeof(TNode) || viewQueries.CacheType.IsAssignableFrom(typeof(TNode)) && !Attribute.IsDefined(typeof(TNode), typeof(PXBreakInheritanceAttribute), false))
					{
						viewQueries.Clear();
					}
				}
			//}
			//foreach (KeyValuePair<Type, Dictionary<PXCommandKey, List<object>>> pair in _Graph.TypedViews._NonstandardQueries)
			//{
			//    if (pair.Key == typeof(TNode) || pair.Key.IsAssignableFrom(typeof(TNode)))
			//    {
			//        pair.Value.Clear();
			//    }
			//}
		}



		public override int Delete(IDictionary keys, IDictionary values)
		{
			TNode item = (TNode)new TNode();
			FillWithValues(item, null, keys, PXCacheOperation.Delete);

			TNode placed = _Items.PlaceDeleted(item, false);

			if (placed == null)
			{
				if (readItem(item) != null)
				{
					placed = _Items.PlaceDeleted(item, true);
				}
			}

			if (placed == null)
			{
				return 0;
			}

			ClearQueryCache();

			bool cancel = false;
			try
			{
				cancel = !OnRowDeleting(placed, true);
			}
			catch (Exception)
			{
				cancel = true;
				throw;
			}
			finally
			{
				if (cancel)
				{
					bool deleted;
					_Items.PlaceInserted(placed, out deleted);
				}
			}
			if (cancel)
			{
				return 0;
			}

			Current = placed;
			if (!_AllowDelete && _Items.GetStatus(placed) != PXEntryStatus.InsertedDeleted)
			{
				bool deleted;
				_Items.PlaceInserted(placed, out deleted);
				return 0;
			}

			_IsDirty = true;

			OnRowDeleted(placed, true);

			_Current = null;

			return 1;
		}

		public override object Delete(object data)
		{
			return Delete(data, false);
		}

		protected internal override object Delete(object data, bool bypassinterceptor)
		{
			if (!bypassinterceptor && _Interceptor != null)
			{
				return _Interceptor.Delete(this, data);
			}
			if (data is PXResult)
			{
				data = ((PXResult)data)[0];
			}
			TNode item = data as TNode;
			if (item == null)
			{
				return null;
			}

			TNode placed = _Items.PlaceDeleted(item, false);

			if (placed == null)
			{
				if (readItem(item) != null)
				{
					placed = _Items.PlaceDeleted(item, true);
				}
			}

			if (placed == null)
			{
				return null;
			}

			ClearQueryCache();

			bool cancel = false;
			try
			{
				cancel = !OnRowDeleting(placed, false);
			}
			catch (Exception)
			{
				cancel = true;
				throw;
			}
			finally
			{
				if (cancel)
				{
					bool deleted;
					_Items.PlaceInserted(placed, out deleted);
				}
			}
			if (cancel)
			{
				return null;
			}

			_IsDirty = true;

			OnRowDeleted(placed, false);

			_Current = null;

			return placed;
		}

		protected internal override void PlaceNotChanged(object data)
		{
			PlaceNotChanged((TNode)data);
		}

		protected internal override object PlaceNotChanged(object data, out bool wasUpdated)
		{
			return _Items.PlaceNotChanged((TNode)data, out wasUpdated);
		}

		protected void PlaceNotChanged(TNode data)
		{
			bool wasUpdated;
			_Items.PlaceNotChanged(data, out wasUpdated);
		}

		protected void PlaceInserted(TNode data)
		{
			bool wasDeleted;
			_Items.PlaceInserted(data, out wasDeleted);
		}

		protected void PlaceUpdated(TNode data)
		{
			_Items.PlaceUpdated(data, false);
		}

		protected void PlaceDeleted(TNode data)
		{
			_Items.PlaceDeleted(data, false);
		}
		#endregion

		#region Dirty items enumerators
        /// <summary>
        /// Diry items.
        /// </summary>
        public override IEnumerable Dirty
        {
            get
            {
                return (IEnumerable)_Items.Dirty;
            }
        }
		/// <summary>
		/// Updated items.
		/// </summary>
		public override IEnumerable Updated
		{
			get
			{
				return (IEnumerable)_Items.Updated;
			}
		}

		/// <summary>
		/// Inserted items.
		/// </summary>
		public override IEnumerable Inserted
		{
			get
			{
				return (IEnumerable)_Items.Inserted;
			}
		}


		/// <summary>
		/// All cached items.
		/// </summary>
		public override IEnumerable Cached
		{
			get
			{
				return (IEnumerable)_Items.Cached;
			}
		}

		/// <summary>
		/// Deleted items.
		/// </summary>
		public override IEnumerable Deleted
		{
			get
			{
				return (IEnumerable)_Items.Deleted;
			}
		}
		internal override int Version
		{
			get
			{
				return _Items.Version;
			}
		}

		/// <summary>
		/// If cache contains data rows to be saved to database.
		/// </summary>		
		public override bool IsInsertedUpdatedDeleted
		{
			get
			{
				return _Items.IsDirty;
			}
		}
		#endregion

		#region Persistance to the database
		private Dictionary<TNode, bool> persistedItems;
		private object[] getKeys(TNode node)
		{
			object[] ret = new object[Keys.Count];
			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = GetValue(node, Keys[i]);
			}
			return ret;
		}
		/// <summary>
		/// Saves the changes to the database.
		/// </summary>
		/// <returns>The first item saved.</returns>
		public override int Persist(PXDBOperation operation)
		{
			int ret = 0;

			if (persistedItems == null)
			{
				persistedItems = new Dictionary<TNode, bool>();
			}

			switch (operation)
			{
				case PXDBOperation.Update:
					foreach (TNode node in _Items.Updated)
					{
						PersistUpdated(node);
						ret++;
					}
					break;
				case PXDBOperation.Insert:
					foreach (TNode node in _Items.Inserted)
					{
						PersistInserted(node);
						ret++;
						_ItemsDenormalized = true;
					}
					_Items.Normalize(null);
					_ItemsDenormalized = false;
					break;
				case PXDBOperation.Delete:
					foreach (TNode node in _Items.Deleted)
					{
						PersistDeleted(node);
						ret++;
					}
					break;
			}

			return ret;
		}

		/// <summary>
		/// Saves the changes to the database for a particular row.
		/// </summary>
		public override void Persist(object row, PXDBOperation operation)
		{
			switch (operation)
			{
				case PXDBOperation.Update:
					PersistUpdated(row);
					break;
				case PXDBOperation.Insert:
					PersistInserted(row);
					break;
				case PXDBOperation.Delete:
					PersistDeleted(row);
					break;
			}
		}

		/// <summary>
		/// Saves the changes to the database for a particular updated row.
		/// </summary>
		public override bool PersistUpdated(object row)
		{
			if (persistedItems == null)
			{
				persistedItems = new Dictionary<TNode, bool>();
			}

			if (persistedItems.ContainsKey((TNode)row))
			{
				return false;
			}

			if (!DisableCloneAttributes)
				GetAttributes(row, null);
			bool cancel = true;
			try
			{
				cancel = !OnRowPersisting(row, PXDBOperation.Update);
				if (!cancel && _Interceptor != null)
				{
					cancel = true;
					cancel = !_Interceptor.PersistUpdated(this, row);
					return !cancel;
				}
			}
			catch (PXCommandPreparingException e)
			{
				if (OnExceptionHandling(e.Name, row, e.Value, e))
				{
					throw;
				}
				PXTrace.WriteWarning(e);
				return false;
			}
			catch (PXRowPersistingException e)
			{
				if (OnExceptionHandling(e.Name, row, e.Value, e))
				{
					throw;
				}
				PXTrace.WriteWarning(e);
				return false;
			}
			catch (PXDatabaseException e)
			{
				if (e.ErrorCode == PXDbExceptions.PrimaryKeyConstraintViolation)
				{
					throw new PXLockViolationException(_BqlTable, PXDBOperation.Insert, getKeys((TNode)row));
				}
				e.Keys = getKeys((TNode)row);
				throw;
			}
			finally
			{
				persistedItems.Add((TNode)row, cancel);
			}
			if (!cancel)
			{
				TNode unchanged = null;
				try
				{
					lock (((ICollection)_Originals).SyncRoot)
					{
						BqlTablePair orig;
						if (_Originals.TryGetValue((TNode)row, out orig))
						{
							unchanged = orig.Unchanged as TNode;
						}
					}
				}
				catch
				{
				}
				List<PXDataFieldParam> pars = new List<PXDataFieldParam>();
				try
				{
					foreach (string descr in _ClassFields)
					{
						PXCommandPreparingEventArgs.FieldDescription description = null;
						object val = GetValue(row, descr);
						OnCommandPreparing(descr, row, val, PXDBOperation.Update, null, out description);
						if (description != null && !String.IsNullOrEmpty(description.FieldName))
						{
							if (description.IsRestriction)
							{
								object origval;
								if (unchanged != null && description.DataType != PXDbType.Timestamp
									&& Keys.Contains(descr) && !object.Equals((origval = GetValue(unchanged, descr)), val)
									&& origval != null)
								{
									PXCommandPreparingEventArgs.FieldDescription origdescription;
									OnCommandPreparing(descr, row, origval, PXDBOperation.Update, null, out origdescription);
									if (origdescription != null && !String.IsNullOrEmpty(origdescription.FieldName))
									{
										PXDataFieldAssign assign = new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue, ValueToString(descr, val));
										pars.Add(assign);
										pars.Add(new PXDataFieldRestrict(origdescription.FieldName, origdescription.DataType, origdescription.DataLength, origdescription.DataValue));
									}
									else
									{
										pars.Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
									}
								}
								else
								{
									pars.Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
								}
							}
							else
							{
								PXDataFieldAssign assign = new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue, null);
								if (unchanged != null)
								{
									if (assign.IsChanged = !object.Equals(val, GetValue(unchanged, descr)))
									{
										assign.NewValue = ValueToString(descr, val);
									}
								}
								else assign.IsChanged = false;
								pars.Add(assign);
							}
						}
					}
				}
				catch (PXCommandPreparingException e)
				{
					if (OnExceptionHandling(e.Name, row, e.Value, e))
					{
						throw;
					}
					PXTrace.WriteWarning(e);
					return false;
				}
				try
				{
					pars.Add(PXDataFieldRestrict.OperationSwitchAllowed);
					if (!_Graph.ProviderUpdate(_BqlTable, pars.ToArray()))
					{
						throw new PXLockViolationException(_BqlTable, PXDBOperation.Update, getKeys((TNode)row));
					}
					try
					{
						OnRowPersisted(row, PXDBOperation.Update, PXTranStatus.Open, null);
						lock (((ICollection)_Originals).SyncRoot)
						{
							_Originals.Remove((TNode)row);
						}
					}
					catch (PXRowPersistedException e)
					{
						OnExceptionHandling(e.Name, row, e.Value, e);
						throw;
					}
				}
				catch (PXDatabaseException e)
				{
					if (e.ErrorCode == PXDbExceptions.OperationSwitchRequired)
					{
						List<PXDataFieldAssign> ipars = new List<PXDataFieldAssign>();
						try
						{
							foreach (string descr in _ClassFields)
							{
								PXCommandPreparingEventArgs.FieldDescription description = null;
								OnCommandPreparing(descr, row, GetValue(row, descr), PXDBOperation.Insert, null, out description);
								if (description != null && !String.IsNullOrEmpty(description.FieldName))
								{
									ipars.Add(new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue));
								}
							}
						}
						catch (PXCommandPreparingException ex)
						{
							if (OnExceptionHandling(ex.Name, row, ex.Value, ex))
							{
								throw;
							}
							PXTrace.WriteWarning(e);
							return false;
						}
						try
						{
							_Graph.ProviderInsert(_BqlTable, ipars.ToArray());
							try
							{
								OnRowPersisted(row, PXDBOperation.Update, PXTranStatus.Open, null);
								lock (((ICollection)_Originals).SyncRoot)
								{
									_Originals.Remove((TNode)row);
								}
							}
							catch (PXRowPersistedException ex)
							{
								OnExceptionHandling(ex.Name, row, ex.Value, ex);
								throw;
							}
						}
						catch (PXDatabaseException ex)
						{
							if (ex.ErrorCode == PXDbExceptions.PrimaryKeyConstraintViolation)
							{
								throw new PXLockViolationException(_BqlTable, PXDBOperation.Insert, getKeys((TNode)row));
							}
							ex.Keys = getKeys((TNode)row);
							throw;
						}
					}
					else
					{
						e.Keys = getKeys((TNode)row);
						throw;
					}
				}
			}
			return !cancel;
		}

		/// <summary>
		/// Saves the changes to the database for a particular inserted row.
		/// </summary>
		public override bool PersistInserted(object row)
		{
			if (persistedItems == null)
			{
				persistedItems = new Dictionary<TNode, bool>();
			}

			if (persistedItems.ContainsKey((TNode)row))
			{
				return false;
			}

			if (!DisableCloneAttributes)
				GetAttributes(row, null);
			bool cancel = true;
			try
			{
				cancel = !OnRowPersisting(row, PXDBOperation.Insert);
				if (!cancel && _Interceptor != null)
				{
					cancel = true;
					cancel = !_Interceptor.PersistInserted(this, row);
					return !cancel;
				}
			}
			catch (PXCommandPreparingException e)
			{
				_Items.Normalize(null);
				if (OnExceptionHandling(e.Name, row, e.Value, e))
				{
					throw;
				}
				PXTrace.WriteWarning(e);
				return false;
			}
			catch (PXRowPersistingException e)
			{
				if (OnExceptionHandling(e.Name, row, e.Value, e))
				{
					throw;
				}
				PXTrace.WriteWarning(e);
				return false;
			}
			catch (PXDatabaseException e)
			{
				if (e.ErrorCode == PXDbExceptions.PrimaryKeyConstraintViolation)
				{
					throw new PXLockViolationException(_BqlTable, PXDBOperation.Insert, getKeys((TNode)row));
				}
				e.Keys = getKeys((TNode)row);
				throw;
			}
			finally
			{
				persistedItems.Add((TNode)row, cancel);
			}
			if (!cancel)
			{
				bool audit = false;
				Type table = BqlTable;
				while (!(audit = PXDatabase.AuditRequired(table)) && table.BaseType != typeof(object))
				{
					table = table.BaseType;
				}
				List<PXDataFieldAssign> pars = new List<PXDataFieldAssign>();
				try
				{
					foreach (string descr in _ClassFields)
					{
						PXCommandPreparingEventArgs.FieldDescription description = null;
						object val = GetValue(row, descr);
						OnCommandPreparing(descr, row, GetValue(row, descr), PXDBOperation.Insert, null, out description);
						if (description != null && !String.IsNullOrEmpty(description.FieldName))
						{
							PXDataFieldAssign assign;
							pars.Add(assign = new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue, null));
							if (audit && val != null)
							{
								assign.IsChanged = true;
								assign.NewValue = ValueToString(descr, val);
							}
							else assign.IsChanged = false;
						}
					}
				}
				catch (PXCommandPreparingException e)
				{
					_Items.Normalize(null);
					if (OnExceptionHandling(e.Name, row, e.Value, e))
					{
						throw;
					}
					PXTrace.WriteWarning(e);
					return false;
				}
				try
				{
					pars.Add(PXDataFieldAssign.OperationSwitchAllowed);
					_Graph.ProviderInsert(_BqlTable, pars.ToArray());
					try
					{
						OnRowPersisted(row, PXDBOperation.Insert, PXTranStatus.Open, null);
						lock (((ICollection)_Originals).SyncRoot)
						{
							_Originals.Remove((TNode)row);
						}
					}
					catch (PXRowPersistedException e)
					{
						OnExceptionHandling(e.Name, row, e.Value, e);
						throw;
					}
				}
				catch (PXDatabaseException e)
				{
					if (e.ErrorCode == PXDbExceptions.OperationSwitchRequired)
					{
						TNode unchanged = null;
						try
						{
							lock (((ICollection)_Originals).SyncRoot)
							{
								BqlTablePair orig;
								if (_Originals.TryGetValue((TNode)row, out orig))
								{
									unchanged = orig.Unchanged as TNode;
								}
							}
						}
						catch
						{
						}
						List<PXDataFieldParam> upars = new List<PXDataFieldParam>();
						try
						{
							foreach (string descr in _ClassFields)
							{
								PXCommandPreparingEventArgs.FieldDescription description = null;
								object val = GetValue(row, descr);
								OnCommandPreparing(descr, row, val, PXDBOperation.Update | PXDBOperation.Second, null, out description);
								if (description != null && !String.IsNullOrEmpty(description.FieldName))
								{
									if (description.IsRestriction)
									{
										upars.Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
									}
									else
									{
										PXDataFieldAssign assign = new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue, null);
										if (unchanged != null)
										{
											if (assign.IsChanged = !object.Equals(val, GetValue(unchanged, descr)))
											{
												assign.NewValue = ValueToString(descr, val);
											}
										}
										else assign.IsChanged = false;
										upars.Add(assign);
									}
								}
							}
						}
						catch (PXCommandPreparingException ex)
						{
							if (OnExceptionHandling(ex.Name, row, ex.Value, ex))
							{
								throw;
							}
							PXTrace.WriteWarning(e);
							return false;
						}
						try
						{
							if (!_Graph.ProviderUpdate(_BqlTable, upars.ToArray()))
							{
								throw new PXLockViolationException(_BqlTable, PXDBOperation.Update, getKeys((TNode)row));
							}
							try
							{
								OnRowPersisted(row, PXDBOperation.Insert, PXTranStatus.Open, null);
								lock (((ICollection)_Originals).SyncRoot)
								{
									_Originals.Remove((TNode)row);
								}
							}
							catch (PXRowPersistedException ex)
							{
								OnExceptionHandling(ex.Name, row, ex.Value, ex);
								throw;
							}
						}
						catch (PXDatabaseException ex)
						{
							if (ex.ErrorCode == PXDbExceptions.PrimaryKeyConstraintViolation)
							{
								throw new PXLockViolationException(_BqlTable, PXDBOperation.Insert, getKeys((TNode)row));
							}
							ex.Keys = getKeys((TNode)row);
							throw;
						}
					}
					else
					{
						if (e.ErrorCode == PXDbExceptions.PrimaryKeyConstraintViolation)
						{
							throw new PXLockViolationException(_BqlTable, PXDBOperation.Insert, getKeys((TNode)row));
						}
						e.Keys = getKeys((TNode)row);
						throw;
					}
				}
			}
			return !cancel;
		}

		/// <summary>
		/// Saves the changes to the database for a particular deleted row.
		/// </summary>
		public override bool PersistDeleted(object row)
		{
			if (persistedItems == null)
			{
				persistedItems = new Dictionary<TNode, bool>();
			}

			if (persistedItems.ContainsKey((TNode)row))
			{
				return false;
			}
			if (!DisableCloneAttributes)
				GetAttributes(row, null);
			bool cancel = true;
			try
			{
				cancel = !OnRowPersisting(row, PXDBOperation.Delete);
				if (!cancel && _Interceptor != null)
				{
					cancel = true;
					cancel = !_Interceptor.PersistDeleted(this, row);
					return !cancel;
				}
			}
			catch (PXCommandPreparingException e)
			{
				if (OnExceptionHandling(e.Name, row, e.Value, e))
				{
					throw;
				}
				PXTrace.WriteWarning(e);
				return false;
			}
			catch (PXRowPersistingException e)
			{
				if (OnExceptionHandling(e.Name, row, e.Value, e))
				{
					throw;
				}
				PXTrace.WriteWarning(e);
				return false;
			}
			catch (PXDatabaseException e)
			{
				e.Keys = getKeys((TNode)row);
				throw;
			}
			finally
			{
				persistedItems.Add((TNode)row, cancel);
			}
			if (!cancel)
			{
				List<PXDataFieldRestrict> pars = new List<PXDataFieldRestrict>();
				try
				{
					foreach (string descr in _ClassFields)
					{
						PXCommandPreparingEventArgs.FieldDescription description = null;
						OnCommandPreparing(descr, row, GetValue(row, descr), PXDBOperation.Delete, null, out description);
						if (description != null && !String.IsNullOrEmpty(description.FieldName))
						{
							if (description.IsRestriction)
							{
								pars.Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
							}
						}
					}
				}
				catch (PXCommandPreparingException e)
				{
					if (OnExceptionHandling(e.Name, row, e.Value, e))
					{
						throw;
					}
					PXTrace.WriteWarning(e);
					return false;
				}
				try
				{
					if (!_Graph.ProviderDelete(_BqlTable, pars.ToArray()))
					{
						throw new PXLockViolationException(_BqlTable, PXDBOperation.Delete, getKeys((TNode)row));
					}
					try
					{
						OnRowPersisted(row, PXDBOperation.Delete, PXTranStatus.Open, null);
					}
					catch (PXRowPersistedException e)
					{
						OnExceptionHandling(e.Name, row, e.Value, e);
						throw;
					}
				}
				catch (PXDatabaseException e)
				{
					if (e.ErrorCode == PXDbExceptions.OperationSwitchRequired)
					{
						List<PXDataFieldAssign> ipars = new List<PXDataFieldAssign>();
						try
						{
							foreach (string descr in _ClassFields)
							{
								PXCommandPreparingEventArgs.FieldDescription description = null;
								OnCommandPreparing(descr, row, GetValue(row, descr), PXDBOperation.Insert, null, out description);
								if (description != null && !String.IsNullOrEmpty(description.FieldName))
								{
									ipars.Add(new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue));
								}
							}
						}
						catch (PXCommandPreparingException ex)
						{
							if (OnExceptionHandling(ex.Name, row, ex.Value, ex))
							{
								throw;
							}
							PXTrace.WriteWarning(e);
							return false;
						}
						try
						{
							_Graph.ProviderInsert(_BqlTable, ipars.ToArray());
							try
							{
								OnRowPersisted(row, PXDBOperation.Delete, PXTranStatus.Open, null);
							}
							catch (PXRowPersistedException ex)
							{
								OnExceptionHandling(ex.Name, row, ex.Value, ex);
								throw;
							}
						}
						catch (PXDatabaseException ex)
						{
							if (ex.ErrorCode == PXDbExceptions.PrimaryKeyConstraintViolation)
							{
								throw new PXLockViolationException(_BqlTable, PXDBOperation.Insert, getKeys((TNode)row));
							}
							ex.Keys = getKeys((TNode)row);
							throw;
						}
					}
					else
					{
						e.Keys = getKeys((TNode)row);
						throw;
					}
				}
			}
			return !cancel;
		}

		public override void ResetPersisted(object row)
		{
			if (persistedItems != null && row is TNode && persistedItems.ContainsKey((TNode)row))
			{
				persistedItems.Remove((TNode)row);
			}
		}

		public override void Persisted(bool isAborted)
		{
			Persisted(isAborted, null);
		}

		protected internal override void Persisted(bool isAborted, Exception exception)
		{
			if (persistedItems == null)
			{
				return;
			}
			List<object> successed = new List<object>();
			foreach (TNode node in _Items.Updated)
			{
				if (persistedItems.ContainsKey(node))
				{
					OnRowPersisted(node, PXDBOperation.Update, isAborted ? PXTranStatus.Aborted : PXTranStatus.Completed, exception);
					if (!isAborted && !persistedItems[node])
					{
						SetStatus(node, PXEntryStatus.Notchanged);
						successed.Add(node);
					}
				}
			}
			foreach (TNode node in _Items.Inserted)
			{
				if (persistedItems.ContainsKey(node))
				{
					OnRowPersisted(node, PXDBOperation.Insert, isAborted ? PXTranStatus.Aborted : PXTranStatus.Completed, exception);
					if (!isAborted && !persistedItems[node])
					{
						SetStatus(node, PXEntryStatus.Notchanged);
						successed.Add(node);
					}
				}
			}
			if (isAborted)
			{
				_Items.Normalize(null);
			}
			foreach (TNode node in _Items.Deleted)
			{
				if (persistedItems.ContainsKey(node))
				{
					OnRowPersisted(node, PXDBOperation.Delete, isAborted ? PXTranStatus.Aborted : PXTranStatus.Completed, exception);
					if (!isAborted && !persistedItems[node])
					{
						SetStatus(node, PXEntryStatus.InsertedDeleted);
					}
				}
			}
			persistedItems = null;
			if (!isAborted)
			{
				_IsDirty = false;
			}
			PXAutomation.StorePersisted(_Graph, _BqlTable, successed);
		}
		#endregion

		#region Session state managment methods
		protected Dictionary<TNode, TNode> _ChangedKeys;
		protected sealed class ItemComparer : IEqualityComparer<TNode>
		{
			private PXCache _Cache;
			public ItemComparer(PXCache cache)
			{
				_Cache = cache;
			}
			public int GetHashCode(TNode item)
			{
				return _Cache.GetObjectHashCode(item);
			}
			public bool Equals(TNode item1, TNode item2)
			{
				return _Cache.ObjectsEqual(item1, item2);
			}
		}
		private bool stateLoaded;
		/// <summary>
		/// Loads dirty items and other cache state objects from the session.
		/// </summary>
		public override void Load()
		{
			Load(PXGraph.GraphStatePrefix);
		}
		internal override void Load(string prefix)
		{
			if (stateLoaded)
			{
				if (_Graph.stateLoading && _Current != null)
				{
					try
					{
						OnRowSelected((TNode)_Current);
					}
					catch
					{
					}
				}
				return;
			}
			if (!PXContext.Session.IsSessionEnabled)
			{
				return;
			}			
			stateLoaded = true;
			if (Graph.UnattendedMode)
			{
				return;
			}
			string key = prefix + String.Format("{0}${1}", _Graph.GetType().FullName, typeof(TNode).FullName);
			object[] bucket = PXContext.Session.CacheInfo[key] as object[];
			if (bucket != null && bucket.Length > 0)
			{
				TNode[] items = bucket[0] as TNode[];
				if (items != null)
				{
					for (int i = 0; i < items.Length; ++i)
					{
						TNode item = _Items.PlaceUpdated(items[i], true);
						if (item != null)
						{
							lock (((ICollection)_Originals).SyncRoot)
							{
								BqlTablePair orig;
								if (_Originals.TryGetValue(item, out orig) && orig.Unchanged is TNode && !object.ReferenceEquals(item, orig.Unchanged) && !ObjectsEqual(item, orig.Unchanged))
								{
									if (_ChangedKeys == null)
									{
										_ChangedKeys = new Dictionary<TNode, TNode>(new ItemComparer(this));
									}
									_ChangedKeys[(TNode)orig.Unchanged] = item;
								}
							}
						}
					}
					if (_ChangedKeys != null)
					{
						ClearQueryCache();
					}
				}
				if (bucket.Length > 1)
				{
					items = bucket[1] as TNode[];
					if (items != null)
					{
						for (int i = 0; i < items.Length; ++i)
						{
							bool deleted;
							_Items.PlaceInserted(items[i], out deleted);
						}
					}
				}
				if (bucket.Length > 2)
				{
					items = bucket[2] as TNode[];
					if (items != null)
					{
						for (int i = 0; i < items.Length; ++i)
						{
							_Items.PlaceDeleted(items[i], true);
						}
					}
				}
				if (bucket.Length > 3)
				{
					items = bucket[3] as TNode[];
					if (items != null)
					{
						bool wasUpdated;
						for (int i = 0; i < items.Length; ++i)
						{
							_Items.PlaceNotChanged(items[i], out wasUpdated);
							_Items.SetStatus(items[i], PXEntryStatus.Held);
						}
					}
				}
				if (bucket.Length > 4)
				{
					TNode item = bucket[4] as TNode;
					if (item != null)
					{
						try
						{
							TNode cached = _Items.Locate(item);
							if (cached != null)
							{
								Current = cached;
							}
							else
							{
								Current = item;
								if (bucket.Length > 6 && (bool)bucket[6])
								{
									bool wasUpdated;
									_Items.PlaceNotChanged(item, out wasUpdated);
									_CurrentPlacedIntoCache = item;
								}
							}
						}
						catch
						{
						}
					}
				}
				if (bucket.Length > 5)
				{
					_IsDirty = (bool)bucket[5];
				}
				if (bucket.Length > 7)
				{
					_Items.Version = (int)bucket[7];
				}
			}
		}

		/// <summary>
		/// Clear all the info stored in the session previously.
		/// </summary>
		/// <param name="storage">A kind of dictionary representing the session object.</param>
		public override void Clear()
		{
			//ClearSession();
			_Current = null;
			_CurrentPlacedIntoCache = null;
			_Items = new PXCollection<TNode>(this);
			//if (_AlteredFields != null)
			//{
			//    _AlteredFields.Clear();
			//}
			_EventsRowItem = null;
			_CommandPreparingEventsItem = null;
			_FieldDefaultingEventsItem = null;
			_FieldUpdatingEventsItem = null;
			_FieldVerifyingEventsItem = null;
			_FieldUpdatedEventsItem = null;
			_FieldSelectingEventsItem = null;
			_ExceptionHandlingEventsItem = null;
			_ItemAttributes = null;
			_IsDirty = false;
			_ChangedKeys = null;
		}
		public override void ClearItemAttributes()
		{
			_ItemAttributes = null;

			_EventsRowItem = null;
			_CommandPreparingEventsItem = null;
			_FieldDefaultingEventsItem = null;
			_FieldUpdatingEventsItem = null;
			_FieldVerifyingEventsItem = null;
			_FieldUpdatedEventsItem = null;
			_FieldSelectingEventsItem = null;
			_ExceptionHandlingEventsItem = null;
		}
		internal override void ClearSession(string prefix)
		{
			if (PXContext.Session.IsSessionEnabled)
			{
				string key = prefix + String.Format("{0}${1}", _Graph.GetType().FullName, typeof (TNode).FullName);
				PXContext.Session.Remove(key);
			}
		}


		public override void Unload()
		{
			Unload("");
		}
		internal override void Unload(string prefix)
		{
			if (!AutoSave)
			{
				List<TNode> updated = new List<TNode>(_Items.Updated);
				List<TNode> inserted = new List<TNode>(_Items.Inserted);
				List<TNode> deleted = new List<TNode>(_Items.Deleted);
				List<TNode> held = new List<TNode>(_Items.Held);

				if (PXContext.Session.IsSessionEnabled)
				{
					string key = prefix + String.Format("{0}${1}", _Graph.GetType().FullName, typeof(TNode).FullName);
					if (updated.Count > 0 || inserted.Count > 0 || deleted.Count > 0 || held.Count > 0 || _Current != null || _IsDirty)
					{
						PXContext.Session.CacheInfo[key] = new object[] { updated.ToArray(), inserted.ToArray(), deleted.ToArray(), held.ToArray(), _Current, _IsDirty, _Current is TNode && _Items.Locate((TNode)_Current) != null, _Items.Version };
					}
					else
					{
						PXContext.Session.Remove(key);
					}
				}
			}
			else
			{
				using (PXTransactionScope tscope = new PXTransactionScope())
				{
					Persist(PXDBOperation.Insert);
					Persist(PXDBOperation.Update);
					Persist(PXDBOperation.Delete);
					tscope.Complete();
				}
			}
			_CurrentPlacedIntoCache = null;
		}
		#endregion

        #region Debugger support
        public override string ToString()
        {
            //throw new Exception();
            return String.Format("PXCache<{0}>({1})", typeof (TNode).FullName, _Items.CachedCount);
        }

        internal class PXCacheDebugView
        {
            

            private readonly PXCache<TNode> Src;
            public PXCacheDebugView(PXCache<TNode> src)
            {
                Src = src;

               
            }


            public TNode Current { get { return (TNode)Src.Current; } }
            public TNode[] Inserted { get { return Src.Inserted.OfType<TNode>().ToArray(); } }
            public TNode[] Updated { get { return Src.Updated.OfType<TNode>().ToArray(); } }
            public TNode[] Deleted { get { return Src.Deleted.OfType<TNode>().ToArray(); } }
            public string[] Fields { get { return Src.Fields.ToArray(); } }
            public string[] BqlFields { get { return Src.BqlFields.Select(_ => _.Name).ToArray(); } }
             
        }
           
        #endregion
	}
}

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
using System.ComponentModel;
using System.Data;
using System.Linq;
using PX.SM;

namespace PX.Data
{
    /// <summary>
    /// Untyped interface to access PXCache&lt;TNode&gt; without knowledge of the type TNode<br/>
    /// Cache contains Data Rows modified by user until they are saved to database.(Unit of work pattern)<br/>
    /// Instance of cache created and destroyed on each callback, the modified data rows are stored in session between callbacks.<br/>
    /// </summary>
	public abstract class PXCache
	{
		#region Ctors

		public PXCache()
		{
			var att = Attribute.GetCustomAttributes(GetItemType(), typeof(PXCacheNameAttribute), true).FirstOrDefault() as PXCacheNameAttribute;
			if (att != null)
				DisplayName = att.Name;
		}

		static PXCache()
		{
			List<Type> ret = new List<Type>();
			foreach (System.Reflection.Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				// ignore some assemblies including dynamic ones
				if (!PXSubstManager.IsSuitableTypeExportAssembly(a, false))
					continue;
				try
				{
					Type[] types = null;
					try
					{
						if (!a.IsDynamic)
							types = a.GetExportedTypes();
					}
					catch (System.Reflection.ReflectionTypeLoadException te)
					{
						types = te.Types;
					}
					if (types == null)
					{
						continue;
					}
					// iterate through assembly types
					foreach (Type t in types)
					{
						// processing extension objects
						if (typeof(PXCacheExtension).IsAssignableFrom(t))
						{
							ret.Add(t);
							if (t.IsDefined(typeof(PXDBInterceptorAttribute), true)
								&& t.BaseType.IsGenericType)
							{
								PXDBInterceptorAttribute inter = (PXDBInterceptorAttribute)t.GetCustomAttributes(typeof(PXDBInterceptorAttribute), true)[0];
								Type table = t.BaseType.GetGenericArguments()[t.BaseType.GetGenericArguments().Length - 1];
								while (table != typeof(object))
								{
									if ((table.BaseType == typeof(object) || !typeof(IBqlTable).IsAssignableFrom(table.BaseType)) &&
										typeof(IBqlTable).IsAssignableFrom(table) || table.IsDefined(typeof(PXTableAttribute), false))
									{
										List<KeyValuePair<string, List<string>>> extensions;
										if (!_ExtensionTables.TryGetValue(table.Name, out extensions))
										{
											_ExtensionTables[table.Name] = extensions = new List<KeyValuePair<string, List<string>>>();
										}
										extensions.Add(new KeyValuePair<string, List<string>>(t.Name, inter.Keys));
									}
									table = table.BaseType;
								}
							}
						}
					}
				}
				catch
				{
				}
			}
			_AvailableExtensions = ret.ToArray();
		}

		private static Dictionary<string, List<KeyValuePair<string, List<string>>>> _ExtensionTables = new Dictionary<string, List<KeyValuePair<string, List<string>>>>(StringComparer.OrdinalIgnoreCase);

		internal static List<KeyValuePair<string, List<string>>> GetExtensionTables(string table)
		{
			List<KeyValuePair<string, List<string>>> ret;
			_ExtensionTables.TryGetValue(table, out ret);
			return ret;
		}

		#endregion

		#region Attributes
		internal static PXEventSubscriberAttribute prepareAttribute(PXEventSubscriberAttribute attr, string fieldName, int fieldOrdinal, Type itemType)
		{
			attr.FieldName = fieldName;
			attr.FieldOrdinal = (int)fieldOrdinal;
			if (attr.BqlTable == null)
			{
				attr.SetBqlTable(itemType);
			}

			return attr;
		}

		public abstract List<PXEventSubscriberAttribute> GetAttributes(string name);
		public abstract List<PXEventSubscriberAttribute> GetAttributesReadonly(string name);
		public abstract List<PXEventSubscriberAttribute> GetAttributesReadonly(string name, bool extractEmmbeddedAttr);

		public abstract List<PXEventSubscriberAttribute> GetAttributesReadonly(object data, string name);
		/// <summary>
		/// Returns collection of row specific attributes
		/// This method creates a copy of all cache level attributes 
		/// and stores this clones to internal collection that contains row specific attributes
		/// To avoid cloning cache level attributes, use GetAttributesReadonly method
		/// </summary>
		public abstract List<PXEventSubscriberAttribute> GetAttributes(object data, string name);
		public abstract bool HasAttributes(object data);
		public List<PXEventSubscriberAttribute> GetAttributes<Field>()
			where Field : IBqlField
		{
			return GetAttributes(typeof(Field).Name);
		}
		public List<PXEventSubscriberAttribute> GetAttributesReadonly<Field>()
			where Field : IBqlField
		{
			return GetAttributesReadonly(typeof(Field).Name);
		}
		public List<PXEventSubscriberAttribute> GetAttributesReadonly<Field>(object data)
			where Field : IBqlField
		{
			return GetAttributesReadonly(data, typeof(Field).Name);
		}
		public List<PXEventSubscriberAttribute> GetAttributes<Field>(object data)
			where Field : IBqlField
		{
			return GetAttributes(data, typeof(Field).Name);
		}
		internal abstract List<Type> GetExtensionTables();

    	public bool DisableCloneAttributes;
		#endregion

		#region Field Manipulation
		protected HashSet<string> _BypassAuditFields;

		/// <summary>
		/// Bypass collecting values for audit. May be used for passwords ans so on. <br/>
		/// </summary>
		public virtual HashSet<string> BypassAuditFields
		{
			get
			{
				if (_BypassAuditFields == null)
				{
					_BypassAuditFields = new HashSet<string>();
				}
				return _BypassAuditFields;
			}
		}

		protected List<string> _AlteredFields;

        /// <summary>
        /// Force calculating PXFieldState in methods GetValueExt, GetValueInt. <br/>
        /// This methods internally calls OnFieldSelecting event with flag IsAltered
        /// </summary>
		public virtual List<string> AlteredFields
		{
			get
			{
				if (_AlteredFields == null)
				{
					_AlteredFields = new List<string>();
				}
				return _AlteredFields;
			}
		}
        /// <summary>
        /// Add or remove field to AlteredFields list.
        /// </summary>
		public virtual void SetAltered<Field>(bool isAltered)
			where Field : IBqlField
		{
			int i;
			if (!isAltered)
			{
				if (_AlteredFields != null && (i = _AlteredFields.IndexOf(typeof(Field).Name.ToLower())) >= 0)
				{
					_AlteredFields.RemoveAt(i);
				}
			}
			else if (_AlteredFields == null || _AlteredFields.IndexOf(typeof(Field).Name.ToLower()) < 0)
			{
				AlteredFields.Add(typeof(Field).Name.ToLower());
			}
		}
        /// <summary>
        /// Add or remove field to AlteredFields list.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="isAltered"></param>
		public virtual void SetAltered(string field, bool isAltered)
		{
			if (!String.IsNullOrEmpty(field))
			{
				int i;
				if (!isAltered)
				{
					if (_AlteredFields != null && (i = _AlteredFields.IndexOf(field.ToLower())) >= 0)
					{
						_AlteredFields.RemoveAt(i);
					}
				}
				else if (_AlteredFields == null || _AlteredFields.IndexOf(field.ToLower()) < 0)
				{
					AlteredFields.Add(field.ToLower());
				}
			}
		}

        /// <summary>
        /// sets value without any validation or event handling
        /// </summary>
        /// <param name="data">IBqlTable or IDictionary</param>
        /// <param name="ordinal"></param>
        /// <param name="value"></param>
		public abstract void SetValue(object data, int ordinal, object value);
		public abstract object GetValue(object data, int ordinal);
        /// <summary>
        /// sets value without any validation or event handling
        /// </summary>
        /// <param name="data">IBqlTable or IDictionary</param>
		public abstract void SetValue(object data, string fieldName, object value);

        /// <summary>
        /// Gets field value by field name without raising any events
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
		public abstract object GetValue(object data, string fieldName);
        /// <summary>
        /// Set field value. <br/>
        /// Raises events: OnFieldUpdating, OnFieldVerifying, OnFieldUpdated. <br/>
        /// If exception - OnExceptionHandling. <br/>
        /// </summary>
        /// <param name="data">IBqlTable or IDictionary</param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
		public abstract void SetValueExt(object data, string fieldName, object value);

		internal abstract object GetCopy(object data);
        /// <summary>
        /// Raises OnFieldUpdating event to set field value
        /// </summary>
        /// <param name="data">IBqlTable or IDictionary</param>
        /// <param name="fieldName"></param>
		public abstract void SetDefaultExt(object data, string fieldName);
		public void SetValue<Field>(object data, object value)
			where Field : IBqlField
		{
			SetValue(data, typeof(Field).Name, value);
		}
		public void SetValueExt<Field>(object data, object value)
			where Field : IBqlField
		{
			SetValueExt(data, typeof(Field).Name, value);
		}
		public void SetDefaultExt<Field>(object data)
			where Field : IBqlField
		{
			SetDefaultExt(data, typeof(Field).Name);
		}

        /// <summary>
        /// Gets value by field name.<br/>
        /// Raises OnFieldSelecting event.<br/>
        /// Returns pure value or PXFieldState( if field is in AlteredFields List ).
        /// </summary>
        /// <param name="data">IBqlTable or IDictionary</param>
        /// <param name="fieldName"></param>
        /// <returns>Pure value or PXFieldState</returns>
		public abstract object GetValueExt(object data, string fieldName);


        /// <summary>
        /// The same as GetValueExt
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
		public abstract object GetValueInt(object data, string fieldName);

        /// <summary>
        /// Gets field state by field name.<br/>
        /// Raises OnFieldSelecting event. <br/>
        /// returns PXFieldState
        /// </summary>
        /// <param name="data">IBqlTable or IDictionary</param>
        /// <param name="fieldName"></param>
        /// <returns>PXFieldState</returns>
		public abstract object GetStateExt(object data, string fieldName);
		public object GetValue<Field>(object data)
			where Field : IBqlField
		{
			return GetValue(data, typeof(Field).Name);
		}
		public object GetValueExt<Field>(object data)
			where Field : IBqlField
		{
			return GetValueExt(data, typeof(Field).Name);
		}
		public object GetValueInt<Field>(object data)
			where Field : IBqlField
		{
			return GetValueInt(data, typeof(Field).Name);
		}
		public object GetStateExt<Field>(object data)
			where Field : IBqlField
		{
			return GetStateExt(data, typeof(Field).Name);
		}

        /// <summary>
        /// Specified value stored in internal dictionary associated with data row,  <br/>
        /// and can be retrived later by GetValuePending method.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
		public abstract void SetValuePending(object data, string fieldName, object value);

        /// <summary>
        /// returns value stored by SetValuePending
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
		public abstract object GetValuePending(object data, string fieldName);
		public object GetValuePending<Field>(object data)
			where Field : IBqlField
		{
			string fieldName = typeof(Field).Name;
			return GetValuePending(data, char.ToUpper(fieldName[0]).ToString() + fieldName.Substring(1));
		}
		public void SetValuePending<Field>(object data, object value)
			where Field : IBqlField
		{
			string fieldName = typeof(Field).Name;
			SetValuePending(data, char.ToUpper(fieldName[0]).ToString() + fieldName.Substring(1), value);
		}

		/// <summary>
		/// returns value from instance copy stored in database
		/// </summary>
		/// <param name="data"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public abstract object GetValueOriginal(object data, string fieldName);
		public object GetValueOriginal<Field>(object data)
			where Field : IBqlField
		{
			string fieldName = typeof(Field).Name;
			return GetValueOriginal(data, char.ToUpper(fieldName[0]).ToString() + fieldName.Substring(1));
		}

		internal abstract object GetOriginal(object data);
		private static Dictionary<Type, Dictionary<Type, string[]>> _keys = new Dictionary<Type, Dictionary<Type, string[]>>();
		internal static string[] GetKeyNames(PXGraph graph, Type cacheType)
		{
			Dictionary<Type, string[]> dict;
			string[] ret;
			lock (((ICollection)_keys).SyncRoot)
			{
				if (!_keys.TryGetValue(cacheType, out dict))
				{
					_keys[cacheType] = dict = new Dictionary<Type, string[]>();
				}
				dict.TryGetValue(graph.GetType(), out ret);
			}
			if (ret == null)
			{
				PXCache cache = graph.Caches[cacheType];
				ret = cache.Keys.ToArray();
				lock (((ICollection)_keys).SyncRoot)
				{
					dict[graph.GetType()] = ret;
				}
			}
			return ret;
		}

		protected KeysCollection _Keys;
        /// <summary>
        /// List of fied names forming the identity of data row.  <br/>
        /// Values to this collection usually added by attributes having IsKey = true
        /// </summary>
		public virtual KeysCollection Keys
		{
			get
			{
				if (_Keys == null)
				{
					_Keys = new KeysCollection();
				}
				return _Keys;
			}
		}

		internal protected string _Identity;
		/// <summary>
		/// Identity column if exists for a table
		/// </summary>
		public virtual string Identity
		{
			get
			{
				return _Identity;
			}
		}

        protected List<string> _Immutables;
        /// <summary>
        /// List of fied names considered as immutable.  <br/>
        /// Values to this collection usually added by attributes having IsImmutable = true
        /// </summary>
        public virtual List<string> Immutables
        {
            get
            {
                if (_Immutables == null)
                {
                    _Immutables = new List<string>();
                }
                return _Immutables;
            }
        }
		protected PXFieldCollection _Fields;

        /// <summary>
        /// Collection of Cache field names. <br/>
        /// By default, contains all DAC class public properties and virtual fields injected by attributes.<br/>
        /// Programmer can add custom fields to this collection.
        /// </summary>
		public virtual PXFieldCollection Fields
		{
			get
			{
				if (_Fields == null)
				{
					_Fields = new PXFieldCollection(new string[0], new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));
				}
				return _Fields;
			}
		}

        /// <summary>
        /// allow to intercept the database operations(insert, update and delete row in database).
        /// </summary>
		public abstract PXDBInterceptorAttribute Interceptor
		{
			get;
			set;
		}

		protected List<Type> _BqlFields;

        /// <summary>
        /// List of classes that implements IBqlField nested in DAC class and ancestor types.<br/>
        /// This list is distinct of the list, that Fields property returns.
        /// </summary>
		public virtual List<Type> BqlFields
		{
			get
			{
				if (_BqlFields == null)
				{
					_BqlFields = new List<Type>();
				}
				return _BqlFields;
			}
		}

        /// <summary>
        /// search for bqlField in Fields collection 
        /// </summary>
        /// <param name="bqlField"></param>
        /// <returns></returns>
		public string GetField(Type bqlField)
		{
			string bqlFieldName = bqlField.Name;
			foreach (string field in Fields)
				if (string.Equals(field, bqlFieldName, StringComparison.OrdinalIgnoreCase)) return field;
			return null;
		}


        /// <summary>
        /// search for field in BqlFields collection
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
		public Type GetBqlField(string field)
		{
			foreach (Type bqlField in BqlFields)
				if (string.Equals(field, bqlField.Name, StringComparison.OrdinalIgnoreCase)) return bqlField;
			return null;
		}

		protected internal string GetFieldName(string name, bool needBql)
		{
			if (needBql)
			{
				for (int j = BqlFields.Count - 1; j >= 0; j--)
				{
					if (String.Compare(BqlFields[j].Name, name, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return BqlFields[j].Name;
					}
				}
			}
			else
			{
				for (int j = 0; j < Fields.Count; j++)
				{
					if (String.Compare(Fields[j], name, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return Fields[j];
					}
				}
			}
			return null;
		}

		protected List<Type> _BqlKeys;
		public virtual List<Type> BqlKeys
		{
			get
			{
				if (_BqlKeys == null)
				{
					_BqlKeys = new List<Type>();
				}
				return _BqlKeys;
			}
		}

        protected List<Type> _BqlImmutables;
        public virtual List<Type> BqlImmutables
        {
            get
            {
                if (_BqlImmutables == null)
                {
                    _BqlImmutables = new List<Type>();
                }
                return _BqlImmutables;
            }
        }
        /// <summary>
        /// Looks like obsolete property. Value of this property does not used.
        /// </summary>
		public abstract BqlCommand BqlSelect
		{
			get;
			set;
		}
		internal bool BypassCalced;
		public abstract Type BqlTable
		{
			get;
		}

		public static Type GetBqlTable(Type dac)
		{
			var result = dac;
			while (result != null && 
				typeof(IBqlTable).IsAssignableFrom(result.BaseType) && 
				!result.IsDefined(typeof(PXTableAttribute), false) &&
				!result.IsDefined(typeof(PXTableNameAttribute), false))
			{
				result = result.BaseType;
			}
			return result;
		}

        /// <summary>
        /// Save row field values to Dictionary 
        /// </summary>
        /// <param name="data">IBqlTable</param>
        /// <returns></returns>
		public abstract Dictionary<string, object> ToDictionary(object data);

		public abstract string ToXml(object data);

		public abstract object FromXml(string xml);

		public abstract string ValueToString(string fieldName, object val);

		public abstract object ValueFromString(string fieldName, string val);

		//protected abstract Dictionary<string, string> FieldAliases
		//{
		//    get;
		//}

		public abstract int GetFieldCount();
		public abstract int GetFieldOrdinal(string field);
		public abstract int GetFieldOrdinal<Field>()
			where Field : IBqlField;

    	internal abstract Type GetFieldType(string fieldName);

        /// <summary>
        /// Repair internal hashtable, if user has changed the key values of stored data rows.
        /// </summary>
		public abstract void Normalize();
		#endregion

		#region External References
		protected PXGraph _Graph;
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
		#endregion

		#region Access Rights
		protected internal object _CacheSecurity;

		protected bool _SelectRights = true;
		protected internal virtual bool SelectRights
		{
			get
			{
				return _SelectRights;
			}
			set
			{
				_SelectRights = value;
				if (!value)
				{
					_AllowSelect = false;
				}
			}
		}
		protected internal bool _AllowSelect = true;
		protected internal bool _AllowSelectChanged;
		internal bool AutomationHidden;
		internal bool AutomationInsertDisabled;
		internal bool AutomationUpdateDisabled;
		internal bool AutomationDeleteDisabled;
		public virtual bool AllowSelect
		{
			get
			{
				if (!AutomationHidden)
				{
					return _AllowSelect;
				}
				return false;
			}
			set
			{
				if (_SelectRights)
				{
					if (_AllowSelect != value)
					{
						_AllowSelectChanged = true;
					}
					_AllowSelect = value;
				}
			}
		}
		protected bool _UpdateRights = true;
		protected internal virtual bool UpdateRights
		{
			get
			{
				return _UpdateRights;
			}
			set
			{
				_UpdateRights = value;
				if (!value)
				{
					_AllowUpdate = false;
				}
			}
		}
		protected internal bool _AllowUpdate = true;

        /// <summary>
        /// If the cache allows update rows from user interface.
        /// This flag does not affects the ability to update a row by using the API
        /// </summary>
		public virtual bool AllowUpdate
		{
			get
			{
				if (!AutomationUpdateDisabled)
				{
					return _AllowUpdate;
				}
				return false;
			}
			set
			{
				if (_UpdateRights)
				{
					_AllowUpdate = value;
				}
			}
		}
		protected bool _InsertRights = true;
		protected internal virtual bool InsertRights
		{
			get
			{
				return _InsertRights;
			}
			set
			{
				_InsertRights = value;
				if (!value)
				{
					_AllowInsert = false;
				}
			}
		}
		protected internal bool _AllowInsert = true;

        /// <summary>
        /// If the cache allows insert rows from user interface.
        /// This flag does not affects the ability to insert a row by using the API
        /// </summary>
        public virtual bool AllowInsert
		{
			get
			{
				if (!AutomationInsertDisabled)
				{
					return _AllowInsert;
				}
				return false;
			}
			set
			{
				if (_InsertRights)
				{
					_AllowInsert = value;
				}
			}
		}
		protected bool _DeleteRights = true;
		protected internal virtual bool DeleteRights
		{
			get
			{
				return _DeleteRights;
			}
			set
			{
				_DeleteRights = value;
				if (!value)
				{
					_AllowDelete = false;
				}
			}
		}
		protected internal bool _AllowDelete = true;
		
        /// <summary>
        /// If the cache allows delete rows from user interface.
        /// This flag does not affects the ability to delete a row by using the API
        /// </summary>
        public virtual bool AllowDelete
		{
			get
			{
				if (!AutomationDeleteDisabled)
				{
					return _AllowDelete;
				}
				return false;
			}
			set
			{
				if (_DeleteRights)
				{
					_AllowDelete = value;
				}
			}
		}
		#endregion

		#region Commands
		internal abstract object _Clone(object item);
		//public abstract string TableName
		//{
		//    get;
		//    set;
		//}
		protected bool _AutoSave;
		public virtual bool AutoSave
		{
			get
			{
				return _AutoSave;
			}
			set
			{
				_AutoSave = value;
			}
		}
		#endregion

		#region Item manipulation methods
		public abstract Type[] GetExtensionTypes();
		private static Type[] _AvailableExtensions;
		internal static List<Type> _GetExtensions(Type tnode, bool checkActive)
		{
			List<Type> ret = new List<Type>();
			List<Type> tables = new List<Type>();
			while (typeof(IBqlTable).IsAssignableFrom(tnode))
			{
				tables.Add(tnode);
				tnode = tnode.BaseType;
			}
			for (int i = tables.Count - 1; i >= 0; i--)
			{
				tnode = tables[i];
				List<Type> ext = new List<Type>();
				foreach (Type t in _AvailableExtensions)
				{
					// processing extension objects
					if (checkActive)
					{
						try
						{
							System.Reflection.MethodInfo info = t.GetMethod("IsActive", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase, null, new Type[0], null);
							object res;
							if (info != null && info.ReturnType == typeof(bool) && (res = info.Invoke(null, new object[0])) is bool && !(bool)res)
							{
								continue;
							}
						}
						catch
						{
						}
					}
					Type[] args;
					if (typeof(PXCacheExtension).IsAssignableFrom(t)
						&& t.BaseType.IsGenericType
						&& (args = t.BaseType.GetGenericArguments()).Length > 0
						&& (args[args.Length - 1] == tnode))
					{
						ext.Add(t);
					}
				}
				ret.AddRange(PXExtensionManager.Sort(ext));
			}
			return ret;
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
		public abstract Extension GetExtension<Extension>(object item)
			where Extension : PXCacheExtension;

		public static readonly object NotSetValue = new object();
		public static readonly string IsNewRow = Guid.NewGuid().ToString();

		/// <summary>
		/// Gets the type of data rows in the cache.
		/// </summary>
		/// <returns>Containing type.</returns>
		public abstract Type GetItemType();

		internal object _Current;
        /// <summary>
        /// Contains value of type IBqlTable.<br/>
        /// Can be used in PXSelect query<br/>
        /// This property may point to last row displayed in the user interface<br/>
        /// If user selects row in PXGrid, this property may point to this row on callback.<br/>
        /// if user inserts, updates or deletes row, Current points to such row.<br/>
        /// API methods Insert and Update also assigned this property.<br/>
        /// When this property assigned, OnRowSelected event is raised. 
        /// 
        /// </summary>
		public abstract object Current
		{
			get;
			set;
		}
        /// <summary>
        /// Compares values of key fields. List of key fields is taken from the cache
        /// </summary>
        /// <param name="a">IBqlTable or IDictionary</param>
        /// <param name="b">IBqlTable or IDictionary</param>
        /// <returns></returns>
		public abstract bool ObjectsEqual(object a, object b);

        /// <summary>
        /// Compares values of specified fields.
        /// </summary>
		public bool ObjectsEqual<Field1>(object a, object b)
			where Field1 : IBqlField
		{
			return object.Equals(GetValue<Field1>(a), GetValue<Field1>(b));
		}
        /// <summary>
        /// Compares values of specified fields.
        /// </summary>
		public bool ObjectsEqual<Field1, Field2>(object a, object b)
			where Field1 : IBqlField
			where Field2 : IBqlField
		{
			return ObjectsEqual<Field1>(a, b) && ObjectsEqual<Field2>(a, b);
		}

        /// <summary>
        /// Compares values of specified fields.
        /// </summary>
		public bool ObjectsEqual<Field1, Field2, Field3>(object a, object b)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
		{
			return ObjectsEqual<Field1>(a, b) && ObjectsEqual<Field2, Field3>(a, b);
		}
        /// <summary>
        /// Compares values of specified fields.
        /// </summary>
		public bool ObjectsEqual<Field1, Field2, Field3, Field4>(object a, object b)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
		{
			return ObjectsEqual<Field1>(a, b) && ObjectsEqual<Field2, Field3, Field4>(a, b);
		}
        /// <summary>
        /// Compares values of specified fields.
        /// </summary>
		public bool ObjectsEqual<Field1, Field2, Field3, Field4, Field5>(object a, object b)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
		{
			return ObjectsEqual<Field1>(a, b) && ObjectsEqual<Field2, Field3, Field4, Field5>(a, b);
		}
        /// <summary>
        /// Compares values of specified fields.
        /// </summary>
		public bool ObjectsEqual<Field1, Field2, Field3, Field4, Field5, Field6>(object a, object b)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
			where Field6 : IBqlField
		{
			return ObjectsEqual<Field1>(a, b) && ObjectsEqual<Field2, Field3, Field4, Field5, Field6>(a, b);
		}
        /// <summary>
        /// Compares values of specified fields.
        /// </summary>
		public bool ObjectsEqual<Field1, Field2, Field3, Field4, Field5, Field6, Field7>(object a, object b)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
			where Field6 : IBqlField
			where Field7 : IBqlField
		{
			return ObjectsEqual<Field1>(a, b) && ObjectsEqual<Field2, Field3, Field4, Field5, Field6, Field7>(a, b);
		}
        /// <summary>
        /// Compares values of specified fields.
        /// </summary>
		public bool ObjectsEqual<Field1, Field2, Field3, Field4, Field5, Field6, Field7, Field8>(object a, object b)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
			where Field6 : IBqlField
			where Field7 : IBqlField
			where Field8 : IBqlField
		{
			return ObjectsEqual<Field1>(a, b) && ObjectsEqual<Field2, Field3, Field4, Field5, Field6, Field7, Field8>(a, b);
		}
        /// <summary>
        /// Returns hash code generated from key field values.
        /// </summary>
        /// <param name="data">IBqlTable or IDictionary</param>
        /// <returns></returns>
		public abstract int GetObjectHashCode(object data);

        /// <summary>
        /// Displays key fields in format {k1=v1, k2=v2}
        /// </summary>
        /// <param name="data">IBqlTable or PXResult</param>
        /// <returns></returns>
		public abstract string ObjectToString(object data);

        /// <summary>
        /// Looks for an object in the cache, returns its status.
        /// Item located by key fields. 
        /// </summary>
        /// <param name="item">Cache item to test, IBqlTable</param>
        /// <returns></returns>
		public abstract PXEntryStatus GetStatus(object item);

        /// <summary>
        /// Looks for item in the cache, sets status. 
        /// If item not not in the cache, it will be inserted.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="status"></param>
		public abstract void SetStatus(object item, PXEntryStatus status);


        /// <summary>
        /// Searches data row in the cache.<br/>
        /// Returns located row.
        /// </summary>
        /// <param name="item">IBqlRow</param>
        /// <returns></returns>
		public abstract object Locate(object item);

		protected internal abstract bool IsPresent(object item);

		protected internal abstract bool IsGraphSpecificField(string fieldName);

        /// <summary>
        /// Searches data row in the cache. <br/>
        /// If row not found in cache, <br/>
        /// reads it from the database and places into the cache with status NotChanged.<br/>
        /// </summary>
		public abstract int Locate(IDictionary keys);


        /// <summary>
        /// Remove entry from cache items hashtable.
        /// </summary>
        /// <param name="item"></param>
        public abstract void Remove(object item);

		/// <summary>
        /// Places row into the cache with status Updated.<br/>
        /// If row does not exists in the cache, looks for it in database.<br/>
        /// If row does not exists in database, inserts row with status Inserted.<br/>
        /// Raise events OnRowUpdating, OnRowUpdated and other events.<br/>
        /// This method is used to update row from user interface.<br/>
        /// Flag AllowUpdate may cancel this method.<br/>
        /// returns 1 if updated successfully, otherwise 0.
		/// </summary>
		/// <param name="keys">Primary key of the item.</param>
		/// <param name="values">New field values to update the item with.</param>
		/// <returns>1 if updated successfully, otherwise 0.</returns>
		public abstract int Update(IDictionary keys, IDictionary values);


        /// <summary>
        /// Places row into the cache with status Updated.<br/>
        /// If row does not exists in the cache, looks for it in database.<br/>
        /// If row does not exists in database, inserts row with status Inserted.<br/>
        /// Raise events OnRowUpdating, OnRowUpdated and other events.
        /// Flag AllowUpdate does not affects this method.
        /// </summary>
        /// <param name="item">IBqlTable of type [CacheItemType]</param>
        /// <returns></returns>
		public abstract object Update(object item);


        /// <summary>
        /// Place row into the cache with status Updated.<br/>
        /// If row does not exists in the cache, looks for it in database.<br/>
        /// If row does not exists in database, inserts row with status Inserted.<br/>
        /// Raise events OnRowUpdating, OnRowUpdated and other events.
        /// </summary>
        /// <param name="item">IBqlTable of type [CacheItemType]</param>
        /// <param name="bypassinterceptor">whether to ignore PXDBInterceptorAttribute</param>
        protected internal abstract object Update(object item, bool bypassinterceptor);

		/// <summary>
        /// Inserts  new row into the cache. <br/>
        /// Raises events OnRowInserting, OnRowInserted and other field related events.<br/>
        /// Does not check the database for existing row.<br/>
        /// Values of dictionary are not readonly and can be updated during method call.<br/>
        /// Flag AllowInsert may cancel this method.<br/>
        /// Returns 1 if inserted successfully, otherwise 0.<br/>
		/// </summary>
		/// <param name="values">Field values to populate the item before inserting.</param>
		/// <returns>1 if inserted successfully, otherwise 0.</returns>
		public abstract int Insert(IDictionary values);
		internal abstract object FillItem(IDictionary values);


        /// <summary>
        /// Inserts  new row into the cache. <br/>
        /// Returns inserted row of type [CacheItemType] or null if row was not inserted.<br/>
        /// Raises events OnRowInserting, OnRowInserted and other field related events.<br/>
        /// Does not check the database for existing row.
        /// Flag AllowInsert does not affects this method.
        /// </summary>
        /// <param name="item">IBqlTable of type [CacheItemType]</param>
		public abstract object Insert(object item);

        /// <summary>
        /// Inserts new row into the cache. <br/>
        /// Returns inserted row of type [CacheItemType] or null if row was not inserted.<br/>
        /// Raises events OnRowInserting, OnRowInserted and other field related events.<br/>
        /// Does not check the database for existing row.<br/>
        /// </summary>
        /// <param name="item">IBqlTable of type [CacheItemType]</param>
        /// <param name="bypassinterceptor">whether to ignore PXDBInterceptorAttribute</param>
        /// <returns></returns>
        protected internal abstract object Insert(object item, bool bypassinterceptor);

        /// <summary>
        /// Place new row into the cache.
        /// </summary>
        /// <returns></returns>
        public abstract object Insert();

        /// <summary>
        /// return new TNode();
        /// </summary>
		public abstract object CreateInstance();

        /// <summary>
        /// Inserts row into the cache, when row type is distinct from cache item type.<br/>
        /// Returns inserted row.
        /// </summary>
        /// <typeparam name="Parent"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
		public abstract object Extend<Parent>(Parent item)
			where Parent : class, IBqlTable, new();

        /// <summary>
        /// Place item into the cache with status Deleted.
        /// Raise events OnRowDeleting, OnRowDeleted
        /// This method is used to delete row from user interface.<br/>
        /// Flag AllowDelete may cancel this method.<br/>
        /// returns 1 if deleted successfully, otherwise 0.
        /// </summary>
        /// <param name="keys">Primary key of the item.</param>
        /// <param name="values">Not used, can be null</param>
        /// <returns>1 if deleted successfully, otherwise 0.</returns>
        public abstract int Delete(IDictionary keys, IDictionary values);

        /// <summary>
        /// Place item into the cache with status Deleted.
        /// Raise events OnRowDeleting, OnRowDeleted
        /// Flag AllowDelete does not affects this method.
        /// </summary>
		public abstract object Delete(object item);


        protected internal abstract object Delete(object item, bool bypassinterceptor);
		/// <summary>
		/// Looks for row in cache collection. If row has status deleted or insertedDeleted - returns null. 
		/// returns row inserted to cache.
		/// </summary>
		internal protected abstract void PlaceNotChanged(object data);

		/// <summary>
		/// Looks for row in cache collection. If row has status deleted or insertedDeleted - returns null.
		/// If status inserted or updated - wasUpdated flag is true. 
		/// returns row inserted to cache.
		/// </summary>
		internal protected abstract object PlaceNotChanged(object data, out bool wasUpdated);
		
		/// <summary>
        /// Inserts item created from the PXDataRecord into the cache with status NotChanged.
        /// Raise event OnRowSelecting
        /// </summary>
		public abstract object Select(PXDataRecord record, ref int position, bool isReadOnly, out bool wasUpdated);
		
        /// <summary>
        /// Clone field values.
        /// </summary>
        /// <param name="item">IBqlTable</param>
        /// <returns></returns>
        public abstract object CreateCopy(object item);

        /// <summary>
        /// Copy field values from copy to item.
        /// </summary>
        /// <param name="item">IBqlTable</param>
        /// <param name="copy">IBqlTable</param>
		public abstract void RestoreCopy(object item, object copy);
		#endregion

		#region Session state managment methods
		/// <summary>
		/// Loads data rows and other cache state objects from the session.
		/// </summary>
		public abstract void Load();
		internal abstract void Load(string prefix);

		/// <summary>
        /// Saves dirty data rows and other cache state objects into the session.
		/// </summary>
		public abstract void Unload();
		internal abstract void Unload(string prefix);

		/// <summary>
		/// Clear all the info stored in the session previously.
		/// </summary>
		public abstract void Clear();
		public abstract void ClearItemAttributes();

		/// <summary>
		/// Clear all cached query result for a given table.
		/// </summary>
		public abstract void ClearQueryCache();
		#endregion

		#region Dirty items enumerators
        /// <summary>
        /// Dirty items.
        /// </summary>
        public abstract IEnumerable Dirty
        {
            get;
        }

		/// <summary>
		/// Deleted items.
		/// </summary>
		public abstract IEnumerable Deleted
		{
			get;
		}

		/// <summary>
		/// Updated items.
		/// </summary>
		public abstract IEnumerable Updated
		{
			get;
		}

		/// <summary>
		/// Inserted items.
		/// </summary>
		public abstract IEnumerable Inserted
		{
			get;
		}
		/// <summary>
		/// All cached items.
		/// </summary>
		public abstract IEnumerable Cached
		{
			get;
		}
		internal abstract int Version
		{
			get;
		}
		internal PXCacheOriginalCollection _Originals;
		#endregion

		#region Persistance to the database
		/// <summary>
        /// Saves changed data rows to the database.<br/>
        /// Raise events OnRowPersisting, OnCommandPreparing, OnRowPersisted, OnExceptionHandling
		/// </summary>
		/// <returns>The first item saved.</returns>
		public abstract int Persist(PXDBOperation operation);


        /// <summary>
        /// executes PersistUpdated or PersistInserted or PersistDeleted depends on operation
        /// </summary>
        /// <param name="row"></param>
        /// <param name="operation"></param>
		public abstract void Persist(object row, PXDBOperation operation);


        /// <summary>
        /// Saves updated data row to the database.<br/>
        /// Raise events OnRowPersisting, OnCommandPreparing, OnRowPersisted, OnExceptionHandling<br/>
        /// Default behavior can be modified by PXDBInterceptorAttribute
        /// </summary>
		public abstract bool PersistUpdated(object row);
       
        
        /// <summary>
        /// Inserts data row into the database.<br/>
        /// Exception is trown if row with such keys exists in database. <br/>
        /// Raise events OnRowPersisting, OnCommandPreparing, OnRowPersisted, OnExceptionHandling<br/>
        /// Default behavior can be modified by PXDBInterceptorAttribute
        /// </summary>
		public abstract bool PersistInserted(object row);


        /// <summary>
        /// Deletes row from the database by the keys.<br/>
        /// Raise events OnRowPersisting, OnCommandPreparing, OnRowPersisted, OnExceptionHandling<br/>
        /// Default behavior can be modified by PXDBInterceptorAttribute
        /// </summary>
		public abstract bool PersistDeleted(object row);

        /// <summary>
        /// For each persisted row - raise OnRowPersisted, SetStatus(Notchanged)
        /// </summary>
		public abstract void Persisted(bool isAborted);

        /// <summary>
        /// For each persisted row - raise OnRowPersisted, SetStatus(Notchanged)
        /// </summary>
		protected internal abstract void Persisted(bool isAborted, Exception exception);

        /// <summary>
        /// remove row from list of persited items
        /// </summary>
        /// <param name="row"></param>
		public abstract void ResetPersisted(object row);


		protected bool _IsDirty;

		/// <summary>
		/// If cache contains data rows to be saved to database.
		/// </summary>		
		public virtual bool IsDirty
		{
			get
			{
				if (!AutoSave)
				{
					return _IsDirty;
				}
				return false;
			}
			set
			{
				_IsDirty = value;
			}
		}

		/// <summary>
		/// If cache contains data rows to be saved to database.
		/// </summary>		
        public abstract bool IsInsertedUpdatedDeleted
        {
            get;
        }
		#endregion

		#region Events
		internal protected sealed class EventsRow
		{
			internal List<PXRowSelecting> _RowSelectingList = new List<PXRowSelecting>();
			private PXRowSelecting _RowSelectingDelegate;
			public PXRowSelecting RowSelecting
			{
				get
				{
					if (_RowSelectingList != null && _RowSelectingList.Count > 0)
					{
						_RowSelectingDelegate = (PXRowSelecting)Delegate.Combine(_RowSelectingList.ToArray());
						_RowSelectingList = null;
					}
					return _RowSelectingDelegate;
				}
				set
				{
					_RowSelectingDelegate = value;
				}
			}
			internal List<PXRowSelected> _RowSelectedList = new List<PXRowSelected>();
			private PXRowSelected _RowSelectedDelegate;
			public PXRowSelected RowSelected
			{
				get
				{
					if (_RowSelectedList != null && _RowSelectedList.Count > 0)
					{
						_RowSelectedDelegate = (PXRowSelected)Delegate.Combine(_RowSelectedList.ToArray());
						_RowSelectedList = null;
					}
					return _RowSelectedDelegate;
				}
				set
				{
					_RowSelectedDelegate = value;
				}
			}
			internal List<PXRowInserting> _RowInsertingList = new List<PXRowInserting>();
			private PXRowInserting _RowInsertingDelegate;
			public PXRowInserting RowInserting
			{
				get
				{
					if (_RowInsertingList != null && _RowInsertingList.Count > 0)
					{
						_RowInsertingDelegate = (PXRowInserting)Delegate.Combine(_RowInsertingList.ToArray());
						_RowInsertingList = null;
					}
					return _RowInsertingDelegate;
				}
				set
				{
					_RowInsertingDelegate = value;
				}
			}
			internal List<PXRowInserted> _RowInsertedList = new List<PXRowInserted>();
			private PXRowInserted _RowInsertedDelegate;
			public PXRowInserted RowInserted
			{
				get
				{
					if (_RowInsertedList != null && _RowInsertedList.Count > 0)
					{
						_RowInsertedDelegate = (PXRowInserted)Delegate.Combine(_RowInsertedList.ToArray());
						_RowInsertedList = null;
					}
					return _RowInsertedDelegate;
				}
				set
				{
					_RowInsertedDelegate = value;
				}
			}
			internal List<PXRowUpdating> _RowUpdatingList = new List<PXRowUpdating>();
			private PXRowUpdating _RowUpdatingDelegate;
			public PXRowUpdating RowUpdating
			{
				get
				{
					if (_RowUpdatingList != null && _RowUpdatingList.Count > 0)
					{
						_RowUpdatingDelegate = (PXRowUpdating)Delegate.Combine(_RowUpdatingList.ToArray());
						_RowUpdatingList = null;
					}
					return _RowUpdatingDelegate;
				}
				set
				{
					_RowUpdatingDelegate = value;
				}
			}
			internal List<PXRowUpdated> _RowUpdatedList = new List<PXRowUpdated>();
			private PXRowUpdated _RowUpdatedDelegate;
			public PXRowUpdated RowUpdated
			{
				get
				{
					if (_RowUpdatedList != null && _RowUpdatedList.Count > 0)
					{
						_RowUpdatedDelegate = (PXRowUpdated)Delegate.Combine(_RowUpdatedList.ToArray());
						_RowUpdatedList = null;
					}
					return _RowUpdatedDelegate;
				}
				set
				{
					_RowUpdatedDelegate = value;
				}
			}
			internal List<PXRowDeleting> _RowDeletingList = new List<PXRowDeleting>();
			private PXRowDeleting _RowDeletingDelegate;
			public PXRowDeleting RowDeleting
			{
				get
				{
					if (_RowDeletingList != null && _RowDeletingList.Count > 0)
					{
						_RowDeletingDelegate = (PXRowDeleting)Delegate.Combine(_RowDeletingList.ToArray());
						_RowDeletingList = null;
					}
					return _RowDeletingDelegate;
				}
				set
				{
					_RowDeletingDelegate = value;
				}
			}
			internal List<PXRowDeleted> _RowDeletedList = new List<PXRowDeleted>();
			private PXRowDeleted _RowDeletedDelegate;
			public PXRowDeleted RowDeleted
			{
				get
				{
					if (_RowDeletedList != null && _RowDeletedList.Count > 0)
					{
						_RowDeletedDelegate = (PXRowDeleted)Delegate.Combine(_RowDeletedList.ToArray());
						_RowDeletedList = null;
					}
					return _RowDeletedDelegate;
				}
				set
				{
					_RowDeletedDelegate = value;
				}
			}
			internal List<PXRowPersisting> _RowPersistingList = new List<PXRowPersisting>();
			private PXRowPersisting _RowPersistingDelegate;
			public PXRowPersisting RowPersisting
			{
				get
				{
					if (_RowPersistingList != null && _RowPersistingList.Count > 0)
					{
						_RowPersistingDelegate = (PXRowPersisting)Delegate.Combine(_RowPersistingList.ToArray());
						_RowPersistingList = null;
					}
					return _RowPersistingDelegate;
				}
				set
				{
					_RowPersistingDelegate = value;
				}
			}
			internal List<PXRowPersisted> _RowPersistedList = new List<PXRowPersisted>();
			private PXRowPersisted _RowPersistedDelegate;
			public PXRowPersisted RowPersisted
			{
				get
				{
					if (_RowPersistedList != null && _RowPersistedList.Count > 0)
					{
						_RowPersistedDelegate = (PXRowPersisted)Delegate.Combine(_RowPersistedList.ToArray());
						_RowPersistedList = null;
					}
					return _RowPersistedDelegate;
				}
				set
				{
					_RowPersistedDelegate = value;
				}
			}
		}
		//internal protected sealed class EventsField
		//{
		//    public PXCommandPreparing CommandPreparing;
		//    public PXFieldDefaulting FieldDefaulting;
		//    public PXFieldUpdating FieldUpdating;
		//    public PXFieldVerifying FieldVerifying;
		//    public PXFieldUpdated FieldUpdated;
		//    public PXFieldSelecting FieldSelecting;
		//    public PXExceptionHandling ExceptionHandling;
		//}
		protected sealed class EventsRowAttr
		{
			public IPXRowSelectingSubscriber[] RowSelecting;
			public IPXRowSelectedSubscriber[] RowSelected;
			public IPXRowInsertingSubscriber[] RowInserting;
			public IPXRowInsertedSubscriber[] RowInserted;
			public IPXRowUpdatingSubscriber[] RowUpdating;
			public IPXRowUpdatedSubscriber[] RowUpdated;
			public IPXRowDeletingSubscriber[] RowDeleting;
			public IPXRowDeletedSubscriber[] RowDeleted;
			public IPXRowPersistingSubscriber[] RowPersisting;
			public IPXRowPersistedSubscriber[] RowPersisted;
		}
		//protected sealed class EventsFieldAttr
		//{
		//    public IPXCommandPreparingSubscriber[] CommandPreparing;
		//    public IPXFieldDefaultingSubscriber[] FieldDefaulting;
		//    public IPXFieldUpdatingSubscriber[] FieldUpdating;
		//    public IPXFieldVerifyingSubscriber[] FieldVerifying;
		//    public IPXFieldUpdatedSubscriber[] FieldUpdated;
		//    public IPXFieldSelectingSubscriber[] FieldSelecting;
		//    public IPXExceptionHandlingSubscriber[] ExceptionHandling;
		//}
		protected internal sealed class EventsRowMap
		{
			public List<IPXRowSelectingSubscriber> RowSelecting;
			public List<IPXRowSelectedSubscriber> RowSelected;
			public List<IPXRowInsertingSubscriber> RowInserting;
			public List<IPXRowInsertedSubscriber> RowInserted;
			public List<IPXRowUpdatingSubscriber> RowUpdating;
			public List<IPXRowUpdatedSubscriber> RowUpdated;
			public List<IPXRowDeletingSubscriber> RowDeleting;
			public List<IPXRowDeletedSubscriber> RowDeleted;
			public List<IPXRowPersistingSubscriber> RowPersisting;
			public List<IPXRowPersistedSubscriber> RowPersisted;
			public EventsRowMap()
			{
				RowSelecting = new List<IPXRowSelectingSubscriber>();
				RowSelected = new List<IPXRowSelectedSubscriber>();
				RowInserting = new List<IPXRowInsertingSubscriber>();
				RowInserted = new List<IPXRowInsertedSubscriber>();
				RowUpdating = new List<IPXRowUpdatingSubscriber>();
				RowUpdated = new List<IPXRowUpdatedSubscriber>();
				RowDeleting = new List<IPXRowDeletingSubscriber>();
				RowDeleted = new List<IPXRowDeletedSubscriber>();
				RowPersisting = new List<IPXRowPersistingSubscriber>();
				RowPersisted = new List<IPXRowPersistedSubscriber>();
			}
			public EventsRowMap(EventsRowMap map)
			{
				RowSelecting = new List<IPXRowSelectingSubscriber>(map.RowSelecting);
				RowSelected = new List<IPXRowSelectedSubscriber>(map.RowSelected);
				RowInserting = new List<IPXRowInsertingSubscriber>(map.RowInserting);
				RowInserted = new List<IPXRowInsertedSubscriber>(map.RowInserted);
				RowUpdating = new List<IPXRowUpdatingSubscriber>(map.RowUpdating);
				RowUpdated = new List<IPXRowUpdatedSubscriber>(map.RowUpdated);
				RowDeleting = new List<IPXRowDeletingSubscriber>(map.RowDeleting);
				RowDeleted = new List<IPXRowDeletedSubscriber>(map.RowDeleted);
				RowPersisting = new List<IPXRowPersistingSubscriber>(map.RowPersisting);
				RowPersisted = new List<IPXRowPersistedSubscriber>(map.RowPersisted);
			}
		}
		protected internal sealed class EventsFieldMap
		{
			public List<IPXCommandPreparingSubscriber> CommandPreparing = new List<IPXCommandPreparingSubscriber>();
			public List<IPXFieldDefaultingSubscriber> FieldDefaulting = new List<IPXFieldDefaultingSubscriber>();
			public List<IPXFieldUpdatingSubscriber> FieldUpdating = new List<IPXFieldUpdatingSubscriber>();
			public List<IPXFieldVerifyingSubscriber> FieldVerifying = new List<IPXFieldVerifyingSubscriber>();
			public List<IPXFieldUpdatedSubscriber> FieldUpdated = new List<IPXFieldUpdatedSubscriber>();
			public List<IPXFieldSelectingSubscriber> FieldSelecting = new List<IPXFieldSelectingSubscriber>();
			public List<IPXExceptionHandlingSubscriber> ExceptionHandling = new List<IPXExceptionHandlingSubscriber>();
		}
		internal class EventPosition
		{
			public int RowSelectingFirst = -1;
			public int RowSelectingLength;
			public int RowSelectedFirst = -1;
			public int RowSelectedLength;
			public int RowInsertingFirst = -1;
			public int RowInsertingLength;
			public int RowInsertedFirst = -1;
			public int RowInsertedLength;
			public int RowUpdatingFirst = -1;
			public int RowUpdatingLength;
			public int RowUpdatedFirst = -1;
			public int RowUpdatedLength;
			public int RowDeletingFirst = -1;
			public int RowDeletingLength;
			public int RowDeletedFirst = -1;
			public int RowDeletedLength;
			public int RowPersistingFirst = -1;
			public int RowPersistingLength;
			public int RowPersistedFirst = -1;
			public int RowPersistedLength;
			public int CommandPreparingFirst = -1;
			public int CommandPreparingLength;
		}
		internal delegate void _CacheAttachedDelegate(PXGraph graph, PXCache sender);
		internal protected EventsRow _EventsRow = new EventsRow();
		protected EventsRowAttr _EventsRowAttr = new EventsRowAttr();
		protected Dictionary<object, EventsRowAttr> _EventsRowItem;

		protected internal class CacheStaticInfoBase
		{
			public List<Type> _ExtensionTables;
			public Type[] _ExtensionTypes;
			public readonly Dictionary<string, int> _FieldsMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			public readonly List<PXEventSubscriberAttribute> _FieldAttributes = new List<PXEventSubscriberAttribute>();
			public readonly PXCache.EventsRowMap _EventsRowMap = new EventsRowMap();
			public readonly Dictionary<string, EventsFieldMap> _EventsFieldMap = new Dictionary<string, EventsFieldMap>();
			public int[] _FieldAttributesFirst;
			public int[] _FieldAttributesLast;
			internal EventPosition[] _EventPositions;
			public readonly List<string> _ClassFields = new List<string>();
			public readonly Dictionary<string, int> _ReverseMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			public readonly List<Type> _FieldTypes = new List<Type>();
			public readonly Dictionary<Type, int> _BqlFieldsMap = new Dictionary<Type, int>();
			public int? _TimestampOrdinal;
		}

		internal sealed class AlteredSource
		{
			public readonly PXEventSubscriberAttribute[] Attributes;
			public readonly _CacheAttachedDelegate Method;
			public readonly Type CacheType;
			public readonly HashSet<string> Fields;
			public AlteredSource(PXEventSubscriberAttribute[] attributes, HashSet<string> fields, _CacheAttachedDelegate method, Type cacheType)
			{
				Attributes = attributes;
				Method = method;
				CacheType = cacheType;
				Fields = fields;
			}
		}

		internal sealed class AlteredDescriptor
		{
			public List<PXEventSubscriberAttribute> _FieldAttributes;
			public PXCache.EventsRowMap _EventsRowMap;
			public Dictionary<string, EventsFieldMap> _EventsFieldMap;
			public int[] _FieldAttributesFirst;
			public int[] _FieldAttributesLast;
			public _CacheAttachedDelegate _Method;
			public readonly HashSet<string> Fields;
			public AlteredDescriptor(PXEventSubscriberAttribute[] attributes, HashSet<string> fields, _CacheAttachedDelegate method, Type cacheType)
			{
				Fields = fields;
				_Method = method;
				cacheType = typeof(PXCache<>).MakeGenericType(cacheType);

				CacheStaticInfoBase info = (CacheStaticInfoBase)cacheType
					.GetMethod("_Initialize", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
					.Invoke(null, new object[] { false });

				Dictionary<string, int> fieldsMap = info._FieldsMap;
				_FieldAttributes = new List<PXEventSubscriberAttribute>(info._FieldAttributes);
				_EventsRowMap = new EventsRowMap(info._EventsRowMap);
				_EventsFieldMap = new Dictionary<string, EventsFieldMap>(info._EventsFieldMap);
				_FieldAttributesFirst = (int[])(info._FieldAttributesFirst).Clone();
				_FieldAttributesLast = (int[])(info._FieldAttributesLast).Clone();
				EventPosition[] eventPositions = info._EventPositions;
				List<string> classFields = info._ClassFields;

				Dictionary<string, int> order = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
				for (int i = 0; i < classFields.Count; i++)
				{
					order[classFields[i]] = i;
				}

				string lastname = null;
				int lastidx = -1;
				int pos = -1;

				int deltaRowSelecting = 0;
				int deltaRowSelected = 0;
				int deltaRowInserting = 0;
				int deltaRowInserted = 0;
				int deltaRowUpdating = 0;
				int deltaRowUpdated = 0;
				int deltaRowDeleting = 0;
				int deltaRowDeleted = 0;
				int deltaRowPersisting = 0;
				int deltaRowPersisted = 0;

				Array.Sort(attributes, (a, b) =>
					{
						int i1;
						if (order.TryGetValue(a.FieldName, out i1))
						{
							int i2;
							if (order.TryGetValue(b.FieldName, out i2))
							{
								return ((IComparable<int>)i1).CompareTo(i2);
							}
							else
							{
								return -1;
							}
						}
						else
						{
							if (order.ContainsKey(b.FieldName))
							{
								return 1;
							}
							else
							{
								return 0;
							}
						}
					});

				foreach (PXEventSubscriberAttribute descr in attributes)
				{
					string name = descr.FieldName.ToLower();

					if (!String.Equals(lastname, descr.FieldName, StringComparison.OrdinalIgnoreCase))
					{
						if (lastname != null)
						{
							int delta = lastidx - _FieldAttributesLast[pos] - 1;
							if (delta != 0)
							{
								for (int i = 0; i < _FieldAttributesFirst.Length; i++)
								{
									if (i != pos && _FieldAttributesFirst[i] > _FieldAttributesFirst[pos])
									{
										_FieldAttributesFirst[i] += delta;
									}
									if (_FieldAttributesLast[i] >= _FieldAttributesFirst[pos])
									{
										_FieldAttributesLast[i] += delta;
									}
								}
							}
						}
						if (fieldsMap.TryGetValue(descr.FieldName, out pos))
						{
							if (descr.FieldOrdinal == -1)
							{
								descr.FieldOrdinal = pos;
							}
							_FieldAttributes.RemoveRange(_FieldAttributesFirst[pos], _FieldAttributesLast[pos] - _FieldAttributesFirst[pos] + 1);
							lastname = descr.FieldName;
							lastidx = _FieldAttributesFirst[pos];

							_EventsFieldMap[name] = new EventsFieldMap();

							EventPosition position = eventPositions[pos];
							if (position.RowSelectingLength > 0)
							{
								_EventsRowMap.RowSelecting.RemoveRange(position.RowSelectingFirst + deltaRowSelecting, position.RowSelectingLength);
								deltaRowSelecting -= position.RowSelectingLength;
							}
							if (position.RowSelectedLength > 0)
							{
								_EventsRowMap.RowSelected.RemoveRange(position.RowSelectedFirst + deltaRowSelected, position.RowSelectedLength);
								deltaRowSelected -= position.RowSelectedLength;
							}
							if (position.RowInsertingLength > 0)
							{
								_EventsRowMap.RowInserting.RemoveRange(position.RowInsertingFirst + deltaRowInserting, position.RowInsertingLength);
								deltaRowInserting -= position.RowInsertingLength;
							}
							if (position.RowInsertedLength > 0)
							{
								_EventsRowMap.RowInserted.RemoveRange(position.RowInsertedFirst + deltaRowInserted, position.RowInsertedLength);
								deltaRowInserted -= position.RowInsertedLength;
							}
							if (position.RowUpdatingLength > 0)
							{
								_EventsRowMap.RowUpdating.RemoveRange(position.RowUpdatingFirst + deltaRowUpdating, position.RowUpdatingLength);
								deltaRowUpdating -= position.RowUpdatingLength;
							}
							if (position.RowUpdatedLength > 0)
							{
								_EventsRowMap.RowUpdated.RemoveRange(position.RowUpdatedFirst + deltaRowUpdated, position.RowUpdatedLength);
								deltaRowUpdated -= position.RowUpdatedLength;
							}
							if (position.RowDeletingLength > 0)
							{
								_EventsRowMap.RowDeleting.RemoveRange(position.RowDeletingFirst + deltaRowDeleting, position.RowDeletingLength);
								deltaRowDeleting -= position.RowDeletingLength;
							}
							if (position.RowDeletedLength > 0)
							{
								_EventsRowMap.RowDeleted.RemoveRange(position.RowDeletedFirst + deltaRowDeleted, position.RowDeletedLength);
								deltaRowDeleted -= position.RowDeletedLength;
							}
							if (position.RowPersistingLength > 0)
							{
								_EventsRowMap.RowPersisting.RemoveRange(position.RowPersistingFirst + deltaRowPersisting, position.RowPersistingLength);
								deltaRowPersisting -= position.RowPersistingLength;
							}
							if (position.RowPersistedLength > 0)
							{
								_EventsRowMap.RowPersisted.RemoveRange(position.RowPersistedFirst + deltaRowPersisted, position.RowPersistedLength);
								deltaRowPersisted -= position.RowPersistedLength;
							}
						}
						else
						{
							lastname = null;
							continue;
						}
					}
					else
					{
						if (descr.FieldOrdinal == -1)
						{
							descr.FieldOrdinal = pos;
						}
					}

					_FieldAttributes.Insert(lastidx, descr);
					{
						EventPosition position = eventPositions[pos];

						List<IPXRowSelectingSubscriber> rowSelecting = new List<IPXRowSelectingSubscriber>();
						descr.GetSubscriber<IPXRowSelectingSubscriber>(rowSelecting);
						if (rowSelecting.Count > 0)
						{
							_EventsRowMap.RowSelecting.InsertRange(position.RowSelectingFirst + deltaRowSelecting + position.RowSelectingLength, rowSelecting);
							deltaRowSelecting += rowSelecting.Count;
						}

						List<IPXRowSelectedSubscriber> rowSelected = new List<IPXRowSelectedSubscriber>();
						descr.GetSubscriber<IPXRowSelectedSubscriber>(rowSelected);
						if (rowSelected.Count > 0)
						{
							_EventsRowMap.RowSelected.InsertRange(position.RowSelectedFirst + deltaRowSelected + position.RowSelectedLength, rowSelected);
							deltaRowSelected += rowSelected.Count;
						}

						List<IPXRowInsertingSubscriber> rowInserting = new List<IPXRowInsertingSubscriber>();
						descr.GetSubscriber<IPXRowInsertingSubscriber>(rowInserting);
						if (rowInserting.Count > 0)
						{
							_EventsRowMap.RowInserting.InsertRange(position.RowInsertingFirst + deltaRowInserting + position.RowInsertingLength, rowInserting);
							deltaRowInserting += rowInserting.Count;
						}

						List<IPXRowInsertedSubscriber> rowInserted = new List<IPXRowInsertedSubscriber>();
						descr.GetSubscriber<IPXRowInsertedSubscriber>(rowInserted);
						if (rowInserted.Count > 0)
						{
							_EventsRowMap.RowInserted.InsertRange(position.RowInsertedFirst + deltaRowInserted + position.RowInsertedLength, rowInserted);
							deltaRowInserted += rowInserted.Count;
						}

						List<IPXRowUpdatingSubscriber> rowUpdating = new List<IPXRowUpdatingSubscriber>();
						descr.GetSubscriber<IPXRowUpdatingSubscriber>(rowUpdating);
						if (rowUpdating.Count > 0)
						{
							_EventsRowMap.RowUpdating.InsertRange(position.RowUpdatingFirst + deltaRowUpdating + position.RowUpdatingLength, rowUpdating);
							deltaRowUpdating += rowUpdating.Count;
						}

						List<IPXRowUpdatedSubscriber> rowUpdated = new List<IPXRowUpdatedSubscriber>();
						descr.GetSubscriber<IPXRowUpdatedSubscriber>(rowUpdated);
						if (rowUpdated.Count > 0)
						{
							_EventsRowMap.RowUpdated.InsertRange(position.RowUpdatedFirst + deltaRowUpdated + position.RowUpdatedLength, rowUpdated);
							deltaRowUpdated += rowUpdated.Count;
						}

						List<IPXRowDeletingSubscriber> rowDeleting = new List<IPXRowDeletingSubscriber>();
						descr.GetSubscriber<IPXRowDeletingSubscriber>(rowDeleting);
						if (rowDeleting.Count > 0)
						{
							_EventsRowMap.RowDeleting.InsertRange(position.RowDeletingFirst + deltaRowDeleting + position.RowDeletingLength, rowDeleting);
							deltaRowDeleting += rowDeleting.Count;
						}

						List<IPXRowDeletedSubscriber> rowDeleted = new List<IPXRowDeletedSubscriber>();
						descr.GetSubscriber<IPXRowDeletedSubscriber>(rowDeleted);
						if (rowDeleted.Count > 0)
						{
							_EventsRowMap.RowDeleted.InsertRange(position.RowDeletedFirst + deltaRowDeleted + position.RowDeletedLength, rowDeleted);
							deltaRowDeleted += rowDeleted.Count;
						}

						List<IPXRowPersistingSubscriber> rowPersisting = new List<IPXRowPersistingSubscriber>();
						descr.GetSubscriber<IPXRowPersistingSubscriber>(rowPersisting);
						if (rowPersisting.Count > 0)
						{
							_EventsRowMap.RowPersisting.InsertRange(position.RowPersistingFirst + deltaRowPersisting + position.RowPersistingLength, rowPersisting);
							deltaRowPersisting += rowPersisting.Count;
						}

						List<IPXRowPersistedSubscriber> rowPersisted = new List<IPXRowPersistedSubscriber>();
						descr.GetSubscriber<IPXRowPersistedSubscriber>(rowPersisted);
						if (rowPersisted.Count > 0)
						{
							_EventsRowMap.RowPersisted.InsertRange(position.RowPersistedFirst + deltaRowPersisted + position.RowPersistedLength, rowPersisted);
							deltaRowPersisted += rowPersisted.Count;
						}

						EventsFieldMap map = _EventsFieldMap[name];
						descr.GetSubscriber<IPXCommandPreparingSubscriber>(map.CommandPreparing);
						descr.GetSubscriber<IPXFieldDefaultingSubscriber>(map.FieldDefaulting);
						descr.GetSubscriber<IPXFieldUpdatingSubscriber>(map.FieldUpdating);
						descr.GetSubscriber<IPXFieldVerifyingSubscriber>(map.FieldVerifying);
						descr.GetSubscriber<IPXFieldUpdatedSubscriber>(map.FieldUpdated);
						descr.GetSubscriber<IPXExceptionHandlingSubscriber>(map.ExceptionHandling);
						descr.GetSubscriber<IPXFieldSelectingSubscriber>(map.FieldSelecting);
					}
					lastidx++;
				}
				if (lastname != null)
				{
					int delta = lastidx - _FieldAttributesLast[pos] - 1;
					if (delta != 0)
					{
						for (int i = 0; i < _FieldAttributesFirst.Length; i++)
						{
							if (i != pos && _FieldAttributesFirst[i] > _FieldAttributesFirst[pos])
							{
								_FieldAttributesFirst[i] += delta;
							}
							if (_FieldAttributesLast[i] >= _FieldAttributesFirst[pos])
							{
								_FieldAttributesLast[i] += delta;
							}
						}
					}
				}
			}
		}

		protected internal sealed class EventDictionary<T> : Dictionary<string, T>
		{
			public EventDictionary(int count)
				: base(count)
			{
			}
			public EventDictionary()
			{
			}
			public new T this[string key]
			{
				get
				{
					if (!base.ContainsKey(key))
					{
						base[key] = default(T);
					}
					return base[key];
				}
				set
				{
					base[key] = value;
					if (value == null)
					{
						base.Remove(key);
					}
				}
			}
		}

		protected EventDictionary<PXCommandPreparing> _CommandPreparingEvents;
		protected internal EventDictionary<PXCommandPreparing> CommandPreparingEvents
		{
			get
			{
				if (_CommandPreparingEvents == null)
				{
					_CommandPreparingEvents = new EventDictionary<PXCommandPreparing>();
				}
				return _CommandPreparingEvents;
			}
		}
		protected Dictionary<string, IPXCommandPreparingSubscriber[]> _CommandPreparingEventsAttr = new Dictionary<string, IPXCommandPreparingSubscriber[]>();
		protected Dictionary<object, Dictionary<string, IPXCommandPreparingSubscriber[]>> _CommandPreparingEventsItem;
		protected bool OnCommandPreparing(string name, object row, object value, PXDBOperation operation, Type table, out PXCommandPreparingEventArgs.FieldDescription description)
		{
			name = name.ToLower();
			PXCommandPreparing handler;
			IPXCommandPreparingSubscriber[] handlerAttr;
			Dictionary<string, IPXCommandPreparingSubscriber[]> dictAttr;
			if (_CommandPreparingEvents != null && _CommandPreparingEvents.TryGetValue(name, out handler))
			{
				PXCommandPreparingEventArgs args = new PXCommandPreparingEventArgs(row, value, operation, table);
				handler(this, args);
				if (!args.Cancel)
				{
					if (_CommandPreparingEventsItem != null && row != null && _CommandPreparingEventsItem.TryGetValue(row, out dictAttr))
					{
						if (dictAttr.TryGetValue(name, out handlerAttr))
						{
							for (int l = 0; l < handlerAttr.Length; l++)
							{
								handlerAttr[l].CommandPreparing(this, args);
							}
						}
					}
					else if (_CommandPreparingEventsAttr != null && _CommandPreparingEventsAttr.TryGetValue(name, out handlerAttr))
					{
						for (int l = 0; l < handlerAttr.Length; l++)
						{
							handlerAttr[l].CommandPreparing(this, args);
						}
					}
				}
				description = args.GetFieldDescription();
				return !args.Cancel;
			}
			else if (_CommandPreparingEventsItem != null && row != null && _CommandPreparingEventsItem.TryGetValue(row, out dictAttr))
			{
				if (dictAttr.TryGetValue(name, out handlerAttr))
				{
					PXCommandPreparingEventArgs args = new PXCommandPreparingEventArgs(row, value, operation, table);
					for (int l = 0; l < handlerAttr.Length; l++)
					{
						handlerAttr[l].CommandPreparing(this, args);
					}
					description = args.GetFieldDescription();
					return !args.Cancel;
				}
			}
			else if (_CommandPreparingEventsAttr != null && _CommandPreparingEventsAttr.TryGetValue(name, out handlerAttr))
			{
				PXCommandPreparingEventArgs args = new PXCommandPreparingEventArgs(row, value, operation, table);
				for (int l = 0; l < handlerAttr.Length; l++)
				{
					handlerAttr[l].CommandPreparing(this, args);
				}
				description = args.GetFieldDescription();
				return !args.Cancel;
			}
			description = null;
			return true;
		}
		public bool RaiseCommandPreparing(string name, object row, object value, PXDBOperation operation, Type table, out PXCommandPreparingEventArgs.FieldDescription description)
		{
			return OnCommandPreparing(name, row, value, operation, table, out description);
		}
		public bool RaiseCommandPreparing<Field>(object row, object value, PXDBOperation operation, Type table, out PXCommandPreparingEventArgs.FieldDescription description)
			where Field : IBqlField
		{
			return OnCommandPreparing(typeof(Field).Name, row, value, operation, table, out description);
		}

		protected internal event PXRowSelecting RowSelecting
		{
			add
			{
				if (_EventsRow._RowSelectingList != null)
				{
					_EventsRow._RowSelectingList.Add(value);
				}
				else
				{
					_EventsRow.RowSelecting = (PXRowSelecting)Delegate.Combine(_EventsRow.RowSelecting, value);
				}
			}
			remove
			{
				if (_EventsRow._RowSelectingList != null)
				{
					_EventsRow._RowSelectingList.Remove(value);
				}
				else
				{
					_EventsRow.RowSelecting = (PXRowSelecting)Delegate.Remove(_EventsRow.RowSelecting, value);
				}
			}
		}
		protected internal event PXRowSelected RowSelected
		{
			add
			{
				if (_EventsRow._RowSelectedList != null)
				{
					_EventsRow._RowSelectedList.Add(value);
				}
				else
				{
					_EventsRow.RowSelected = (PXRowSelected)Delegate.Combine(_EventsRow.RowSelected, value);
				}
			}
			remove
			{
				if (_EventsRow._RowSelectedList != null)
				{
					_EventsRow._RowSelectedList.Remove(value);
				}
				else
				{
					_EventsRow.RowSelected = (PXRowSelected)Delegate.Remove(_EventsRow.RowSelected, value);
				}
			}
		}
		protected internal bool OnRowSelecting(object item, PXDataRecord record, ref int position, bool isReadOnly)
		{
			IPXRowSelectingSubscriber[] handlerAttr = null;
			if (_EventsRowItem != null && item != null && _EventsRowItem.ContainsKey(item))
			{
				handlerAttr = (IPXRowSelectingSubscriber[])_EventsRowItem[item].RowSelecting;
			}
			else
			{
				handlerAttr = _EventsRowAttr.RowSelecting;
			}
			if (_EventsRow.RowSelecting != null || handlerAttr != null)
			{
				PXRowSelectingEventArgs args = new PXRowSelectingEventArgs(item, record, position, isReadOnly);
				if (handlerAttr != null)
				{
					for (int l = 0; l < handlerAttr.Length; l++)
					{
						try
						{
							handlerAttr[l].RowSelecting(this, args);
						}
						catch (Exception ex)
						{
							if (handlerAttr[l] is PXEventSubscriberAttribute)
							{
								string fieldname = ((PXEventSubscriberAttribute)handlerAttr[l]).FieldName;
								string dispname = PXUIFieldAttribute.GetDisplayName(this, fieldname);
								throw new PXException(ex, ErrorMessages.ErrorFieldProcessing, dispname ?? fieldname, ex.Message);
							} 
							throw;
						}
					}
				}
				if (_EventsRow.RowSelecting != null)
				{
					//if (args.Row == null)
					//{
					//	args.Row = item;
					//}
					//
					_EventsRow.RowSelecting(this, args);
				}
				position = args.Position;
				return !args.Cancel;
			}
			return true;
		}
		public bool RaiseRowSelecting(object item, PXDataRecord record, ref int position, bool isReadOnly)
		{
			return OnRowSelecting(item, record, ref position, isReadOnly);
		}
		protected void OnRowSelected(object item)
		{
			IPXRowSelectedSubscriber[] handlerAttr = null;
			if (_EventsRowItem != null && item != null && _EventsRowItem.ContainsKey(item))
			{
				handlerAttr = _EventsRowItem[item].RowSelected;
			}
			else
			{
				handlerAttr = _EventsRowAttr.RowSelected;
			}
			if (_EventsRow.RowSelected != null || handlerAttr != null)
			{
				PXRowSelectedEventArgs args = new PXRowSelectedEventArgs(item);
				if (handlerAttr != null)
				{
					for (int l = 0; l < handlerAttr.Length; l++)
					{
						handlerAttr[l].RowSelected(this, args);
					}
				}
				if (_EventsRow.RowSelected != null)
				{
					_EventsRow.RowSelected(this, args);
				}
			}
		}
		public void RaiseRowSelected(object item)
		{
			OnRowSelected(item);
		}

		protected internal event PXRowInserting RowInserting
		{
			add
			{
				if (_EventsRow._RowInsertingList != null)
				{
					_EventsRow._RowInsertingList.Insert(0, value);
				}
				else
				{
					_EventsRow.RowInserting = (PXRowInserting)Delegate.Combine(value, _EventsRow.RowInserting);
				}
			}
			remove
			{
				if (_EventsRow._RowInsertingList != null)
				{
					_EventsRow._RowInsertingList.Remove(value);
				}
				else
				{
					_EventsRow.RowInserting = (PXRowInserting)Delegate.Remove(_EventsRow.RowInserting, value);
				}
			}
		}
		protected internal event PXRowInserted RowInserted
		{
			add
			{
				if (_EventsRow._RowInsertedList != null)
				{
					_EventsRow._RowInsertedList.Add(value);
				}
				else
				{
					_EventsRow.RowInserted = (PXRowInserted)Delegate.Combine(_EventsRow.RowInserted, value);
				}
			}
			remove
			{
				if (_EventsRow._RowInsertedList != null)
				{
					_EventsRow._RowInsertedList.Remove(value);
				}
				else
				{
					_EventsRow.RowInserted = (PXRowInserted)Delegate.Remove(_EventsRow.RowInserted, value);
				}
			}
		}
		protected bool OnRowInserting(object item, bool externalCall)
		{
			IPXRowInsertingSubscriber[] handlerAttr = null;
			if (_EventsRowItem != null && item != null && _EventsRowItem.ContainsKey(item))
			{
				handlerAttr = _EventsRowItem[item].RowInserting;
			}
			else
			{
				handlerAttr = _EventsRowAttr.RowInserting;
			}
			if (_EventsRow.RowInserting != null || handlerAttr != null)
			{
				PXRowInsertingEventArgs args = new PXRowInsertingEventArgs(item, externalCall);
				if (_EventsRow.RowInserting != null)
				{
					_EventsRow.RowInserting(this, args);
				}
				if (!args.Cancel && handlerAttr != null)
				{
					for (int l = 0; l < handlerAttr.Length; l++)
					{
						handlerAttr[l].RowInserting(this, args);
					}
				}
				return !args.Cancel;
			}
			return true;
		}
		public bool RaiseRowInserting(object item)
		{
			return OnRowInserting(item, false);
		}
		protected void OnRowInserted(object item, bool externalCall)
		{
			IPXRowInsertedSubscriber[] handlerAttr = null;
			if (_EventsRowItem != null && item != null && _EventsRowItem.ContainsKey(item))
			{
				handlerAttr = _EventsRowItem[item].RowInserted;
			}
			else
			{
				handlerAttr = _EventsRowAttr.RowInserted;
			}
			if (_EventsRow.RowInserted != null || handlerAttr != null)
			{
				PXRowInsertedEventArgs args = new PXRowInsertedEventArgs(item, externalCall);
				if (handlerAttr != null)
				{
					for (int l = 0; l < handlerAttr.Length; l++)
					{
						handlerAttr[l].RowInserted(this, args);
					}
				}
				if (_EventsRow.RowInserted != null)
				{
					_EventsRow.RowInserted(this, args);
				}
			}
		}
		public void RaiseRowInserted(object item)
		{
			OnRowInserted(item, false);
		}

		protected internal event PXRowUpdating RowUpdating
		{
			add
			{
				if (_EventsRow._RowUpdatingList != null)
				{
					_EventsRow._RowUpdatingList.Insert(0, value);
				}
				else
				{
					_EventsRow.RowUpdating = (PXRowUpdating)Delegate.Combine(value, _EventsRow.RowUpdating);
				}
			}
			remove
			{
				if (_EventsRow._RowUpdatingList != null)
				{
					_EventsRow._RowUpdatingList.Remove(value);
				}
				else
				{
					_EventsRow.RowUpdating = (PXRowUpdating)Delegate.Remove(_EventsRow.RowUpdating, value);
				}
			}
		}
		protected internal event PXRowUpdated RowUpdated
		{
			add
			{
				if (_EventsRow._RowUpdatedList != null)
				{
					_EventsRow._RowUpdatedList.Add(value);
				}
				else
				{
					_EventsRow.RowUpdated = (PXRowUpdated)Delegate.Combine(_EventsRow.RowUpdated, value);
				}
			}
			remove
			{
				if (_EventsRow._RowUpdatedList != null)
				{
					_EventsRow._RowUpdatedList.Remove(value);
				}
				else
				{
					_EventsRow.RowUpdated = (PXRowUpdated)Delegate.Remove(_EventsRow.RowUpdated, value);
				}
			}
		}
		protected bool OnRowUpdating(object item, object newitem, bool externalCall)
		{
			IPXRowUpdatingSubscriber[] handlerAttr = null;
			if (_EventsRowItem != null && newitem != null && _EventsRowItem.ContainsKey(newitem))
			{
				handlerAttr = _EventsRowItem[newitem].RowUpdating;
			}
			else
			{
				handlerAttr = _EventsRowAttr.RowUpdating;
			}
			if (_EventsRow.RowUpdating != null || handlerAttr != null)
			{
				PXRowUpdatingEventArgs args = new PXRowUpdatingEventArgs(item, newitem, externalCall);
				if (_EventsRow.RowUpdating != null)
				{
					_EventsRow.RowUpdating(this, args);
				}
				if (!args.Cancel && handlerAttr != null)
				{
					for (int l = 0; l < handlerAttr.Length; l++)
					{
						handlerAttr[l].RowUpdating(this, args);
					}
				}
				return !args.Cancel;
			}
			return true;
		}
		public bool RaiseRowUpdating(object item, object newItem)
		{
			return OnRowUpdating(item, newItem, false);
		}
		protected void OnRowUpdated(object newItem, object oldItem, bool externalCall)
		{
			IPXRowUpdatedSubscriber[] handlerAttr = null;
			if (_EventsRowItem != null && newItem != null && _EventsRowItem.ContainsKey(newItem))
			{
				handlerAttr = _EventsRowItem[newItem].RowUpdated;
			}
			else
			{
				handlerAttr = _EventsRowAttr.RowUpdated;
			}
			if (_EventsRow.RowUpdated != null || handlerAttr != null)
			{
				PXRowUpdatedEventArgs args = new PXRowUpdatedEventArgs(newItem, oldItem, externalCall);
				if (handlerAttr != null)
				{
					for (int l = 0; l < handlerAttr.Length; l++)
					{
						handlerAttr[l].RowUpdated(this, args);
					}
				}
				if (_EventsRow.RowUpdated != null)
				{
					_EventsRow.RowUpdated(this, args);
				}
			}
		}
		public void RaiseRowUpdated(object newItem, object oldItem)
		{
			OnRowUpdated(newItem, oldItem, false);
		}

		protected internal event PXRowDeleting RowDeleting
		{
			add
			{
				if (_EventsRow._RowDeletingList != null)
				{
					_EventsRow._RowDeletingList.Insert(0, value);
				}
				else
				{
					_EventsRow.RowDeleting = (PXRowDeleting)Delegate.Combine(value, _EventsRow.RowDeleting);
				}
			}
			remove
			{
				if (_EventsRow._RowDeletingList != null)
				{
					_EventsRow._RowDeletingList.Remove(value);
				}
				else
				{
					_EventsRow.RowDeleting = (PXRowDeleting)Delegate.Remove(_EventsRow.RowDeleting, value);
				}
			}
		}
		protected internal event PXRowDeleted RowDeleted
		{
			add
			{
				if (_EventsRow._RowDeletedList != null)
				{
					_EventsRow._RowDeletedList.Add(value);
				}
				else
				{
					_EventsRow.RowDeleted = (PXRowDeleted)Delegate.Combine(_EventsRow.RowDeleted, value);
				}
			}
			remove
			{
				if (_EventsRow._RowDeletedList != null)
				{
					_EventsRow._RowDeletedList.Remove(value);
				}
				else
				{
					_EventsRow.RowDeleted = (PXRowDeleted)Delegate.Remove(_EventsRow.RowDeleted, value);
				}
			}
		}
		protected bool OnRowDeleting(object item, bool externalCall)
		{
			IPXRowDeletingSubscriber[] handlerAttr = null;
			if (_EventsRowItem != null && item != null && _EventsRowItem.ContainsKey(item))
			{
				handlerAttr = _EventsRowItem[item].RowDeleting;
			}
			else
			{
				handlerAttr = _EventsRowAttr.RowDeleting;
			}
			if (_EventsRow.RowDeleting != null || handlerAttr != null)
			{
				PXRowDeletingEventArgs args = new PXRowDeletingEventArgs(item, externalCall);
				if (_EventsRow.RowDeleting != null)
				{
					_EventsRow.RowDeleting(this, args);
				}
				if (!args.Cancel && handlerAttr != null)
				{
					for (int l = 0; l < handlerAttr.Length; l++)
					{
						handlerAttr[l].RowDeleting(this, args);
					}
				}
				return !args.Cancel;
			}
			return true;
		}
		public bool RaiseRowDeleting(object item)
		{
			return OnRowDeleting(item, false);
		}
		protected void OnRowDeleted(object item, bool externalCall)
		{
			IPXRowDeletedSubscriber[] handlerAttr = null;
			if (_EventsRowItem != null && item != null && _EventsRowItem.ContainsKey(item))
			{
				handlerAttr = _EventsRowItem[item].RowDeleted;
			}
			else
			{
				handlerAttr = _EventsRowAttr.RowDeleted;
			}
			if (_EventsRow.RowDeleted != null || handlerAttr != null)
			{
				PXRowDeletedEventArgs args = new PXRowDeletedEventArgs(item, externalCall);
				if (handlerAttr != null)
				{
					for (int l = 0; l < handlerAttr.Length; l++)
					{
						handlerAttr[l].RowDeleted(this, args);
					}
				}
				if (_EventsRow.RowDeleted != null)
				{
					_EventsRow.RowDeleted(this, args);
				}
			}
		}
		public void RaiseRowDeleted(object item)
		{
			OnRowDeleted(item, false);
		}

		protected internal event PXRowPersisting RowPersisting
		{
			add
			{
				if (_EventsRow._RowPersistingList != null)
				{
					_EventsRow._RowPersistingList.Insert(0, value);
				}
				else
				{
					_EventsRow.RowPersisting = (PXRowPersisting)Delegate.Combine(value, _EventsRow.RowPersisting);
				}
			}
			remove
			{
				if (_EventsRow._RowPersistingList != null)
				{
					_EventsRow._RowPersistingList.Remove(value);
				}
				else
				{
					_EventsRow.RowPersisting = (PXRowPersisting)Delegate.Remove(_EventsRow.RowPersisting, value);
				}
			}
		}
		protected internal event PXRowPersisted RowPersisted
		{
			add
			{
				if (_EventsRow._RowPersistedList != null)
				{
					_EventsRow._RowPersistedList.Add(value);
				}
				else
				{
					_EventsRow.RowPersisted = (PXRowPersisted)Delegate.Combine(_EventsRow.RowPersisted, value);
				}
			}
			remove
			{
				if (_EventsRow._RowPersistedList != null)
				{
					_EventsRow._RowPersistedList.Remove(value);
				}
				else
				{
					_EventsRow.RowPersisted = (PXRowPersisted)Delegate.Remove(_EventsRow.RowPersisted, value);
				}
			}
		}
		protected bool OnRowPersisting(object item, PXDBOperation operation)
		{
			IPXRowPersistingSubscriber[] handlerAttr = null;
			if (_EventsRowItem != null && item != null && _EventsRowItem.ContainsKey(item))
			{
				handlerAttr = _EventsRowItem[item].RowPersisting;
			}
			else
			{
				handlerAttr = _EventsRowAttr.RowPersisting;
			}
			if (_EventsRow.RowPersisting != null || handlerAttr != null)
			{
				PXRowPersistingEventArgs args = new PXRowPersistingEventArgs(operation, item);
				if (_EventsRow.RowPersisting != null)
				{
					_EventsRow.RowPersisting(this, args);
					if (_EventsRowItem != null && item != null && _EventsRowItem.ContainsKey(item))
					{
						handlerAttr = _EventsRowItem[item].RowPersisting;
					}
				}
				if (!args.Cancel && handlerAttr != null)
				{
					for (int l = 0; l < handlerAttr.Length; l++)
					{
						handlerAttr[l].RowPersisting(this, args);
					}
				}
				return !args.Cancel;
			}
			return true;
		}
		public bool RaiseRowPersisting(object item, PXDBOperation operation)
		{
			return OnRowPersisting(item, operation);
		}
		protected void OnRowPersisted(object item, PXDBOperation operation, PXTranStatus tranStatus, Exception exception)
		{
			IPXRowPersistedSubscriber[] handlerAttr = null;
			if (_EventsRowItem != null && item != null && _EventsRowItem.ContainsKey(item))
			{
				handlerAttr = _EventsRowItem[item].RowPersisted;
			}
			else
			{
				handlerAttr = _EventsRowAttr.RowPersisted;
			}
			if (_EventsRow.RowPersisted != null || handlerAttr != null)
			{
				PXRowPersistedEventArgs args = new PXRowPersistedEventArgs(item, operation, tranStatus, exception);
				if (handlerAttr != null)
				{
					for (int l = 0; l < handlerAttr.Length; l++)
					{
						handlerAttr[l].RowPersisted(this, args);
					}
				}
				if (_EventsRow.RowPersisted != null)
				{
					_EventsRow.RowPersisted(this, args);
				}
			}
		}
		public void RaiseRowPersisted(object item, PXDBOperation operation, PXTranStatus tranStatus, Exception exception)
		{
			OnRowPersisted(item, operation, tranStatus, exception);
		}

		protected EventDictionary<PXFieldDefaulting> _FieldDefaultingEvents;
		protected internal EventDictionary<PXFieldDefaulting> FieldDefaultingEvents
		{
			get
			{
				if (_FieldDefaultingEvents == null)
				{
					_FieldDefaultingEvents = new EventDictionary<PXFieldDefaulting>();
				}
				return _FieldDefaultingEvents;
			}
		}
		protected Dictionary<string, IPXFieldDefaultingSubscriber[]> _FieldDefaultingEventsAttr = new Dictionary<string, IPXFieldDefaultingSubscriber[]>();
		protected Dictionary<object, Dictionary<string, IPXFieldDefaultingSubscriber[]>> _FieldDefaultingEventsItem;
		protected bool OnFieldDefaulting(string name, object row, out object newValue)
		{
			name = name.ToLower();
			PXFieldDefaulting handler;
			IPXFieldDefaultingSubscriber[] handlerAttr;
			Dictionary<string, IPXFieldDefaultingSubscriber[]> dictAttr;
			if (_FieldDefaultingEvents != null && _FieldDefaultingEvents.TryGetValue(name, out handler))
			{
				PXFieldDefaultingEventArgs args = new PXFieldDefaultingEventArgs(row);
				handler(this, args);
				if (!args.Cancel)
				{
					if (_FieldDefaultingEventsItem != null && row != null && _FieldDefaultingEventsItem.TryGetValue(row, out dictAttr))
					{
						if (dictAttr.TryGetValue(name, out handlerAttr))
						{
							for (int l = 0; l < handlerAttr.Length; l++)
							{
								handlerAttr[l].FieldDefaulting(this, args);
							}
						}
					}
					else if (_FieldDefaultingEventsAttr != null && _FieldDefaultingEventsAttr.TryGetValue(name, out handlerAttr))
					{
						for (int l = 0; l < handlerAttr.Length; l++)
						{
							handlerAttr[l].FieldDefaulting(this, args);
						}
					}
				}
				newValue = args.NewValue;
				if (AutomationFieldDefaulting != null)
				{
					return AutomationFieldDefaulting(this, name, ref newValue, row != null) || !args.Cancel;
				}
				return !args.Cancel;
			}
			else if (_FieldDefaultingEventsItem != null && row != null && _FieldDefaultingEventsItem.TryGetValue(row, out dictAttr))
			{
				if (dictAttr.TryGetValue(name, out handlerAttr))
				{
					PXFieldDefaultingEventArgs args = new PXFieldDefaultingEventArgs(row);
					for (int l = 0; l < handlerAttr.Length; l++)
					{
						handlerAttr[l].FieldDefaulting(this, args);
					}
					newValue = args.NewValue;
					if (AutomationFieldDefaulting != null)
					{
						return AutomationFieldDefaulting(this, name, ref newValue, row != null) || !args.Cancel;
					}
					return !args.Cancel;
				}
			}
			else if (_FieldDefaultingEventsAttr != null && _FieldDefaultingEventsAttr.TryGetValue(name, out handlerAttr))
			{
				PXFieldDefaultingEventArgs args = new PXFieldDefaultingEventArgs(row);
				for (int l = 0; l < handlerAttr.Length; l++)
				{
					handlerAttr[l].FieldDefaulting(this, args);
				}
				newValue = args.NewValue;
				if (AutomationFieldDefaulting != null)
				{
					return AutomationFieldDefaulting(this, name, ref newValue, row != null) || !args.Cancel;
				}
				return !args.Cancel;
			}
			newValue = null;
			if (AutomationFieldDefaulting != null)
			{
				AutomationFieldDefaulting(this, name, ref newValue, row != null);
			}
			return true;
		}
		public bool RaiseFieldDefaulting(string name, object row, out object newValue)
		{
			return OnFieldDefaulting(name, row, out newValue);
		}
		public bool RaiseFieldDefaulting<Field>(object row, out object newValue)
			where Field : IBqlField
		{
			return OnFieldDefaulting(typeof(Field).Name, row, out newValue);
		}

		protected EventDictionary<PXFieldUpdating> _FieldUpdatingEvents;
		protected internal EventDictionary<PXFieldUpdating> FieldUpdatingEvents
		{
			get
			{
				if (_FieldUpdatingEvents == null)
				{
					_FieldUpdatingEvents = new EventDictionary<PXFieldUpdating>();
				}
				return _FieldUpdatingEvents;
			}
		}
		protected Dictionary<string, IPXFieldUpdatingSubscriber[]> _FieldUpdatingEventsAttr = new Dictionary<string, IPXFieldUpdatingSubscriber[]>();
		protected Dictionary<object, Dictionary<string, IPXFieldUpdatingSubscriber[]>> _FieldUpdatingEventsItem;
		protected bool OnFieldUpdating(string name, object row, ref object newValue)
		{
			name = name.ToLower();
			PXFieldUpdating handler;
			IPXFieldUpdatingSubscriber[] handlerAttr;
			Dictionary<string, IPXFieldUpdatingSubscriber[]> dictAttr;
			if (_FieldUpdatingEvents != null && _FieldUpdatingEvents.TryGetValue(name, out handler))
			{
				PXFieldUpdatingEventArgs args = new PXFieldUpdatingEventArgs(row, newValue);
				try
				{
					handler(this, args);
					if (!args.Cancel)
					{
						if (_FieldUpdatingEventsItem != null && row != null && _FieldUpdatingEventsItem.TryGetValue(row, out dictAttr))
						{
							if (dictAttr.TryGetValue(name, out handlerAttr))
							{
								for (int l = 0; l < handlerAttr.Length; l++)
								{
									handlerAttr[l].FieldUpdating(this, args);
								}
							}
						}
						else if (_FieldUpdatingEventsAttr != null && _FieldUpdatingEventsAttr.TryGetValue(name, out handlerAttr))
						{
							for (int l = 0; l < handlerAttr.Length; l++)
							{
								handlerAttr[l].FieldUpdating(this, args);
							}
						}
					}
				}
				finally
				{
					newValue = args.NewValue;
				}
				return !args.Cancel;
			}
			else if (_FieldUpdatingEventsItem != null && row != null && _FieldUpdatingEventsItem.TryGetValue(row, out dictAttr))
			{
				if (dictAttr.TryGetValue(name, out handlerAttr))
				{
					PXFieldUpdatingEventArgs args = new PXFieldUpdatingEventArgs(row, newValue);
					try
					{
						for (int l = 0; l < handlerAttr.Length; l++)
						{
							handlerAttr[l].FieldUpdating(this, args);
						}
					}
					finally
					{
						newValue = args.NewValue;
					}
					return !args.Cancel;
				}
			}
			else if (_FieldUpdatingEventsAttr != null && _FieldUpdatingEventsAttr.TryGetValue(name, out handlerAttr))
			{
				PXFieldUpdatingEventArgs args = new PXFieldUpdatingEventArgs(row, newValue);
				try
				{
					for (int l = 0; l < handlerAttr.Length; l++)
					{
						handlerAttr[l].FieldUpdating(this, args);
					}
				}
				finally
				{
					newValue = args.NewValue;
				}
				return !args.Cancel;
			}
			return true;
		}
		public bool RaiseFieldUpdating(string name, object row, ref object newValue)
		{
			return OnFieldUpdating(name, row, ref newValue);
		}
		public bool RaiseFieldUpdating<Field>(object row, ref object newValue)
			where Field : IBqlField
		{
			return OnFieldUpdating(typeof(Field).Name, row, ref newValue);
		}

		protected EventDictionary<PXFieldVerifying> _FieldVerifyingEvents;
		protected internal EventDictionary<PXFieldVerifying> FieldVerifyingEvents
		{
			get
			{
				if (_FieldVerifyingEvents == null)
				{
					_FieldVerifyingEvents = new EventDictionary<PXFieldVerifying>();
				}
				return _FieldVerifyingEvents;
			}
		}
		protected Dictionary<string, IPXFieldVerifyingSubscriber[]> _FieldVerifyingEventsAttr = new Dictionary<string, IPXFieldVerifyingSubscriber[]>();
		protected Dictionary<object, Dictionary<string, IPXFieldVerifyingSubscriber[]>> _FieldVerifyingEventsItem;
		protected bool OnFieldVerifying(string name, object row, ref object newValue, bool externalCall)
		{
			name = name.ToLower();
			PXFieldVerifying handler;
			IPXFieldVerifyingSubscriber[] handlerAttr;
			Dictionary<string, IPXFieldVerifyingSubscriber[]> dictAttr;
			if (_FieldVerifyingEvents != null && _FieldVerifyingEvents.TryGetValue(name, out handler))
			{
				PXFieldVerifyingEventArgs args = new PXFieldVerifyingEventArgs(row, newValue, externalCall);
				try
				{
					handler(this, args);
					if (!args.Cancel)
					{
						if (_FieldVerifyingEventsItem != null && row != null && _FieldVerifyingEventsItem.TryGetValue(row, out dictAttr))
						{
							if (dictAttr.TryGetValue(name, out handlerAttr))
							{
								for (int l = 0; l < handlerAttr.Length; l++)
								{
									handlerAttr[l].FieldVerifying(this, args);
								}
							}
						}
						else if (_FieldVerifyingEventsAttr != null && _FieldVerifyingEventsAttr.TryGetValue(name, out handlerAttr))
						{
							for (int l = 0; l < handlerAttr.Length; l++)
							{
								handlerAttr[l].FieldVerifying(this, args);
							}
						}
					}
				}
				finally
				{
					newValue = args.NewValue;
				}
				return !args.Cancel;
			}
			else if (_FieldVerifyingEventsItem != null && row != null && _FieldVerifyingEventsItem.TryGetValue(row, out dictAttr))
			{
				if (dictAttr.TryGetValue(name, out handlerAttr))
				{
					PXFieldVerifyingEventArgs args = new PXFieldVerifyingEventArgs(row, newValue, externalCall);
					try
					{
						for (int l = 0; l < handlerAttr.Length; l++)
						{
							handlerAttr[l].FieldVerifying(this, args);
						}
					}
					finally
					{
						newValue = args.NewValue;
					}
					return !args.Cancel;
				}
			}
			else if (_FieldVerifyingEventsAttr != null && _FieldVerifyingEventsAttr.TryGetValue(name, out handlerAttr))
			{
				PXFieldVerifyingEventArgs args = new PXFieldVerifyingEventArgs(row, newValue, externalCall);
				try
				{
					for (int l = 0; l < handlerAttr.Length; l++)
					{
						handlerAttr[l].FieldVerifying(this, args);
					}
				}
				finally
				{
					newValue = args.NewValue;
				}
				return !args.Cancel;
			}
			return true;
		}
		public bool RaiseFieldVerifying(string name, object row, ref object newValue)
		{
			return OnFieldVerifying(name, row, ref newValue, false);
		}
		public bool RaiseFieldVerifying<Field>(object row, ref object newValue)
			where Field : IBqlField
		{
			return OnFieldVerifying(typeof(Field).Name, row, ref newValue, false);
		}

		protected EventDictionary<PXFieldUpdated> _FieldUpdatedEvents;
		protected internal EventDictionary<PXFieldUpdated> FieldUpdatedEvents
		{
			get
			{
				if (_FieldUpdatedEvents == null)
				{
					_FieldUpdatedEvents = new EventDictionary<PXFieldUpdated>();
				}
				return _FieldUpdatedEvents;
			}
		}
		protected Dictionary<string, IPXFieldUpdatedSubscriber[]> _FieldUpdatedEventsAttr = new Dictionary<string, IPXFieldUpdatedSubscriber[]>();
		protected Dictionary<object, Dictionary<string, IPXFieldUpdatedSubscriber[]>> _FieldUpdatedEventsItem;
		protected void OnFieldUpdated(string name, object row, object oldValue, bool externalCall)
		{
			name = name.ToLower();
			PXFieldUpdated handler = null;
			IPXFieldUpdatedSubscriber[] handlerAttr = null;
			if (_FieldUpdatedEvents != null && _FieldUpdatedEvents.ContainsKey(name))
			{
				handler = _FieldUpdatedEvents[name];
			}
			if (_FieldUpdatedEventsItem != null && row != null && _FieldUpdatedEventsItem.ContainsKey(row))
			{
				if (_FieldUpdatedEventsItem[row].ContainsKey(name))
				{
					handlerAttr = _FieldUpdatedEventsItem[row][name];
				}
			}
			else if (_FieldUpdatedEventsAttr != null && _FieldUpdatedEventsAttr.ContainsKey(name))
			{
				handlerAttr = _FieldUpdatedEventsAttr[name];
			}
			if (handler != null || handlerAttr != null)
			{
				PXFieldUpdatedEventArgs args = new PXFieldUpdatedEventArgs(row, oldValue, externalCall);
				if (handlerAttr != null)
				{
					for (int l = 0; l < handlerAttr.Length; l++)
					{
						handlerAttr[l].FieldUpdated(this, args);
					}
				}
				if (handler != null)
				{
					handler(this, args);
				}
			}
		}
		public void RaiseFieldUpdated(string name, object row, object oldValue)
		{
			OnFieldUpdated(name, row, oldValue, false);
		}
		public void RaiseFieldUpdated<Field>(object row, object oldValue)
			where Field : IBqlField
		{
			OnFieldUpdated(typeof(Field).Name, row, oldValue, false);
		}

		protected EventDictionary<PXFieldSelecting> _FieldSelectingEvents;
		protected internal EventDictionary<PXFieldSelecting> FieldSelectingEvents
		{
			get
			{
				if (_FieldSelectingEvents == null)
				{
					_FieldSelectingEvents = new EventDictionary<PXFieldSelecting>();
				}
				return _FieldSelectingEvents;
			}
		}
		protected Dictionary<string, IPXFieldSelectingSubscriber[]> _FieldSelectingEventsAttr = new Dictionary<string, IPXFieldSelectingSubscriber[]>();
		protected Dictionary<object, Dictionary<string, IPXFieldSelectingSubscriber[]>> _FieldSelectingEventsItem;
		internal delegate void FieldSelectingDelegate(string name, ref object returnState, bool rowSpecific);
		internal FieldSelectingDelegate AutomationFieldSelecting;
		internal delegate bool FieldDefaultingDelegate(PXCache sender, string name, ref object defaultValue, bool rowSpecific);
		internal FieldDefaultingDelegate AutomationFieldDefaulting;
		protected bool OnFieldSelecting(string name, object row, ref object returnValue, bool forceState, bool externalCall)
		{
			name = name.ToLower();
			PXFieldSelecting handler;
			IPXFieldSelectingSubscriber[] handlerAttr;
			Dictionary<string, IPXFieldSelectingSubscriber[]> dictAttr;
			if (_FieldSelectingEvents != null && _FieldSelectingEvents.TryGetValue(name, out handler))
			{
				SetAltered(name, true);
				forceState = true;
				PXFieldSelectingEventArgs args = new PXFieldSelectingEventArgs(row, returnValue, forceState, externalCall);
				handler(this, args);
				if (!args.Cancel)
				{
					if (_FieldSelectingEventsItem != null && row != null && _FieldSelectingEventsItem.TryGetValue(row, out dictAttr))
					{
						if (dictAttr.TryGetValue(name, out handlerAttr))
						{
							for (int l = 0; l < handlerAttr.Length; l++)
							{
								handlerAttr[l].FieldSelecting(this, args);
							}
						}
					}
					else if (_FieldSelectingEventsAttr != null && _FieldSelectingEventsAttr.TryGetValue(name, out handlerAttr))
					{
						for (int l = 0; l < handlerAttr.Length; l++)
						{
							handlerAttr[l].FieldSelecting(this, args);
						}
					}
				}
				returnValue = args.ReturnState;
				if (AutomationFieldSelecting != null && returnValue is PXFieldState)
				{
					AutomationFieldSelecting(name, ref returnValue, row != null);
				}
				return !args.Cancel;
			}
			else if (_FieldSelectingEventsItem != null && row != null && _FieldSelectingEventsItem.TryGetValue(row, out dictAttr))
			{
				if (dictAttr.TryGetValue(name, out handlerAttr))
				{
					PXFieldSelectingEventArgs args = new PXFieldSelectingEventArgs(row, returnValue, forceState, externalCall);
					for (int l = 0; l < handlerAttr.Length; l++)
					{
						handlerAttr[l].FieldSelecting(this, args);
					}
					returnValue = args.ReturnState;
					if (AutomationFieldSelecting != null && returnValue is PXFieldState)
					{
						AutomationFieldSelecting(name, ref returnValue, row != null);
					}
					return !args.Cancel;
				}
			}
			else if (_FieldSelectingEventsAttr != null && _FieldSelectingEventsAttr.TryGetValue(name, out handlerAttr))
			{
				PXFieldSelectingEventArgs args = new PXFieldSelectingEventArgs(row, returnValue, forceState, externalCall);
				for (int l = 0; l < handlerAttr.Length; l++)
				{
					handlerAttr[l].FieldSelecting(this, args);
				}
				returnValue = args.ReturnState;
				if (AutomationFieldSelecting != null && returnValue is PXFieldState)
				{
					AutomationFieldSelecting(name, ref returnValue, row != null);
				}
				return !args.Cancel;
			}
			return true;
		}
		public bool RaiseFieldSelecting(string name, object row, ref object returnValue, bool forceState)
		{
			return OnFieldSelecting(name, row, ref returnValue, forceState, true);
		}
		public bool RaiseFieldSelecting<Field>(object row, ref object returnValue, bool forceState)
			where Field : IBqlField
		{
			return OnFieldSelecting(typeof(Field).Name, row, ref returnValue, forceState, true);
		}

		protected EventDictionary<PXExceptionHandling> _ExceptionHandlingEvents;
		protected internal EventDictionary<PXExceptionHandling> ExceptionHandlingEvents
		{
			get
			{
				if (_ExceptionHandlingEvents == null)
				{
					_ExceptionHandlingEvents = new EventDictionary<PXExceptionHandling>();
				}
				return _ExceptionHandlingEvents;
			}
		}
		protected Dictionary<string, IPXExceptionHandlingSubscriber[]> _ExceptionHandlingEventsAttr = new Dictionary<string, IPXExceptionHandlingSubscriber[]>();
		protected Dictionary<object, Dictionary<string, IPXExceptionHandlingSubscriber[]>> _ExceptionHandlingEventsItem;
		protected bool OnExceptionHandling(string name, object row, object newValue, Exception exception)
		{
			name = (exception is PXOverridableException && ((PXOverridableException)exception).MapErrorTo != null) ? ((PXOverridableException)exception).MapErrorTo.ToLower() : name.ToLower();
			PXExceptionHandling handler;
			IPXExceptionHandlingSubscriber[] handlerAttr;
			Dictionary<string, IPXExceptionHandlingSubscriber[]> dictAttr;
			if (_ExceptionHandlingEvents != null && _ExceptionHandlingEvents.TryGetValue(name, out handler))
			{
				PXExceptionHandlingEventArgs args = new PXExceptionHandlingEventArgs(row, newValue, exception);
				handler(this, args);
				if (!args.Cancel)
				{
					if (_ExceptionHandlingEventsItem != null && row != null && _ExceptionHandlingEventsItem.TryGetValue(row, out dictAttr))
					{
						if (dictAttr.TryGetValue(name, out handlerAttr))
						{
							for (int l = 0; l < handlerAttr.Length; l++)
							{
								handlerAttr[l].ExceptionHandling(this, args);
							}
						}
					}
					else if (_ExceptionHandlingEventsAttr != null && _ExceptionHandlingEventsAttr.TryGetValue(name, out handlerAttr))
					{
						for (int l = 0; l < handlerAttr.Length; l++)
						{
							handlerAttr[l].ExceptionHandling(this, args);
						}
					}
				}
				newValue = args.NewValue;
				return !args.Cancel;
			}
			else if (_ExceptionHandlingEventsItem != null && row != null && _ExceptionHandlingEventsItem.TryGetValue(row, out dictAttr))
			{
				if (dictAttr.TryGetValue(name, out handlerAttr))
				{
					PXExceptionHandlingEventArgs args = new PXExceptionHandlingEventArgs(row, newValue, exception);
					for (int l = 0; l < handlerAttr.Length; l++)
					{
						handlerAttr[l].ExceptionHandling(this, args);
					}
					newValue = args.NewValue;
					return !args.Cancel;
				}
			}
			else if (_ExceptionHandlingEventsAttr != null && _ExceptionHandlingEventsAttr.TryGetValue(name, out handlerAttr))
			{
				PXExceptionHandlingEventArgs args = new PXExceptionHandlingEventArgs(row, newValue, exception);
				for (int l = 0; l < handlerAttr.Length; l++)
				{
					handlerAttr[l].ExceptionHandling(this, args);
				}
				newValue = args.NewValue;
				return !args.Cancel;
			}
			return true;
		}
		public bool RaiseExceptionHandling(string name, object row, object newValue, Exception exception)
		{
			if (exception is PXOverridableException && ((PXOverridableException)exception).MapErrorTo != null)
			{
				name = ((PXOverridableException)exception).MapErrorTo;
			}
			GetAttributes(row, name);
			bool result = OnExceptionHandling(name, row, newValue, exception);

			//Logging exeption
			//TODO Need Review for excluding dublicate errors
            //when not handled result == true
			if (result && exception != null && row != null)
			{
				PXSetPropertyException pe = exception as PXSetPropertyException;
				if(pe == null || (pe.ErrorLevel != PXErrorLevel.RowInfo && pe.ErrorLevel != PXErrorLevel.RowWarning && pe.ErrorLevel != PXErrorLevel.RowError))
				{
					System.Diagnostics.TraceEventType et = System.Diagnostics.TraceEventType.Error;
					if(pe != null && pe.ErrorLevel == PXErrorLevel.Warning) et = System.Diagnostics.TraceEventType.Warning;

					PXTrace.WriteEvent(new PXTrace.Event(et, exception));
				}
			}

			return result;
		}
		public bool RaiseExceptionHandling<Field>(object row, object newValue, Exception exception)
			where Field : IBqlField
		{
			return RaiseExceptionHandling(typeof(Field).Name, row, newValue, exception);
		}
		#endregion

		#region Public Properties
        /// <summary>
        /// Initialized from PXCacheNameAttribute
        /// </summary>
		public string DisplayName { get; set; }

		#endregion

    	internal abstract void ClearSession(string prefix);

	    internal bool isKeysFilled(object item)
	    {
		    foreach (string key in _Keys)
		    {
			    if (GetValue(item, key) == null)
			    {
				    return false;
			    }
		    }
		    return true;
	    }
	}
}

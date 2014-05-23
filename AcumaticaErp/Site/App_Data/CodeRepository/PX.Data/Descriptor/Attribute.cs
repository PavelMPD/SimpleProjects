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
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI.WebControls;
using PX.Common;
using PX.Common.Collection;
using System.Collections.Specialized;
using PX.CS;
using PX.DbServices.Model.CrossPlatform;
using PX.SM;
using PX.Api;
using PX.Translation;

namespace PX.Data
{
	#region PXDefaultAttribute
	public enum PXPersistingCheck
	{
		Null,
		NullOrBlank,
		Nothing
	}


	/// <summary>
	/// Provides default value for the field.<br/>
	/// Default value can be constant or result of BQL query.<br/>
	/// Default value is assigned when FiedlDefaulting event is raised.<br/>
	/// (this happens when a new row inserted from API or from UI)<br/>
	/// This attribute also checks that the field value is not null before saving to the database.<br/>
	/// This behavior can be adjusted by PersistingCheck property(check null, check null or blank, do not check).<br/>
	/// Error message can be redirected to another field by MapErrorTo property.
	/// </summary>
	/// <example>
	/// [PXDefault(false)]
	/// 
	/// [PXDefault(typeof(AccessInfo.businessDate))]
	///  
	/// [PXDefault(typeof(Search&lt;CAEntryType.referenceID, Where&lt;CAEntryType.entryTypeId, Equal&lt;Current&lt;AddTrxFilter.entryTypeID>>>>))]
	///	public virtual Int32? ReferenceID{get;set;}
	/// </example>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXDefaultAttribute))]
	public class PXDefaultAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber, IPXRowPersistingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected object _Constant;
		protected Type _SourceType;
		protected string _SourceField;
		protected BqlCommand _Select;
		protected PXPersistingCheck _PersistingCheck = PXPersistingCheck.Null;
		protected Type _MapErrorTo;
		protected bool _SearchOnDefault = true;


		/// <summary>
		/// if SearchOnDefault=false, Bql query is ignored when default value is calculated.<br/>
		/// Default value: true
		/// </summary>
		public virtual bool SearchOnDefault
		{
			get
			{
				return this._SearchOnDefault;
			}
			set
			{
				this._SearchOnDefault = value;
			}
		}

		/// <summary>
		/// How to check the field value before saving to the database.<br/>
		/// Can be Null, NullOrBlank, Nothing<br/>
		/// Default value: Null
		/// </summary>
		public virtual PXPersistingCheck PersistingCheck
		{
			get
			{
				return _PersistingCheck;
			}
			set
			{
				_PersistingCheck = value;
			}
		}

		/// <summary>
		/// Allows to show error message over another field.<br/>
		/// IBqlField<br/>
		/// </summary>
		/// <example>
		/// [PXDefault(MapErrorTo = typeof(PMRegister.date))]
		/// public virtual String TranPeriodID{get;set;}
		/// </example>
		public virtual Type MapErrorTo
		{
			get
			{
				return _MapErrorTo;
			}
			set
			{
				_MapErrorTo = value;
			}
		}

		public virtual object Constant
		{
			get { return this._Constant; }
			set { this._Constant = value; }
		}

		/// <summary>
		/// Which field of select result is used to obtain a default value.<br/>
		/// IBqlField<br/>
		/// </summary>
		/// <example>
		///	[PXDefault(false, typeof(Select&lt;VendorClass, Where&lt;VendorClass.vendorClassID, Equal&lt;Current&lt;Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.allowOverrideRate))]
		/// </example>
		public virtual Type SourceField
		{
			get
			{
				return null;
			}
			set
			{
				if (value != null && typeof(IBqlField).IsAssignableFrom(value) && value.IsNested)
				{
					_SourceType = BqlCommand.GetItemType(value);
					_SourceField = value.Name;
				}
			}
		}
		#endregion

		#region Ctor
		/// <summary>
		/// Defines default from the item of the type sourceType
		/// </summary>
		/// <param name="sourceType">
		/// Bql query that is used to calculate default value.<br/>
		/// Accepts following types: IBqlSearch, IBqlSelect, IBqlField, IBqlTable
		/// </param>
		public PXDefaultAttribute(Type sourceType)
		{
			if (sourceType == null)
			{
				throw new PXArgumentException("type", ErrorMessages.ArgumentNullException);
			}
			if (typeof(IBqlSearch).IsAssignableFrom(sourceType))
			{
				_Select = BqlCommand.CreateInstance(sourceType);
				_SourceType = BqlCommand.GetItemType(((IBqlSearch)_Select).GetField());
				_SourceField = ((IBqlSearch)_Select).GetField().Name;
			}
			else if (typeof(IBqlSelect).IsAssignableFrom(sourceType))
			{
				_Select = BqlCommand.CreateInstance(sourceType);
				_SourceType = _Select.GetTables()[0];
			}
			else if (sourceType.IsNested && typeof(IBqlField).IsAssignableFrom(sourceType))
			{
				_SourceType = BqlCommand.GetItemType(sourceType);
				_SourceField = sourceType.Name;
			}
			else if (typeof(IBqlTable).IsAssignableFrom(sourceType))
			{
				_SourceType = sourceType;
			}
			else if (typeof(Constant).IsAssignableFrom(sourceType))
			{
				_Constant = ((Constant)Activator.CreateInstance(sourceType)).Value;
			}
			else
			{
				throw new PXArgumentException("type", ErrorMessages.CantCreateForeignKeyReference, sourceType);
			}
		}
		/// <summary>
		/// Defines default as a constant value
		/// </summary>
		/// <param name="constant">Constant default value</param>
		public PXDefaultAttribute(object constant)
		{
			_Constant = constant;
		}


		/// <param name="constant">Constant default value</param>
		/// <param name="sourceType">
		/// Bql query that is used to calculate default value.<br/>
		/// Accepts following types: IBqlSearch, IBqlSelect, IBqlField, IBqlTable
		/// </param>
		public PXDefaultAttribute(object constant, Type sourceType)
			: this(sourceType)
		{
			_Constant = constant;
		}

		///// <param name="constant">Constant default value</param>
		///// <param name="sourceType">
		///// Bql query that is used to calculate default value.<br/>
		///// Accepts following types: IBqlSearch, IBqlSelect, IBqlField, IBqlTable
		///// </param>
		//public PXDefaultAttribute(object constant, Type sourceType, Type selectType)
		//    : this(sourceType, selectType)
		//{
		//    _Constant = constant;
		//}


		/// <summary>
		/// Only check if the field value is not null before saving to the database, no default value provided
		/// </summary>
		public PXDefaultAttribute()
		{
		}
		/// <summary>
		/// Allows set the default value from the string representation
		/// </summary>
		/// <param name="converter">Type code of the resulting constant</param>
		/// <param name="constant">String representation of the constant</param>
		public PXDefaultAttribute(TypeCode converter, string constant)
		{
			switch (converter)
			{
				case TypeCode.Boolean:
					_Constant = Boolean.Parse(constant);
					break;
				case TypeCode.Byte:
					_Constant = Byte.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Char:
					_Constant = Char.Parse(constant);
					break;
				case TypeCode.DateTime:
					_Constant = DateTime.Parse(constant, CultureInfo.InvariantCulture, DateTimeStyles.None);
					break;
				case TypeCode.Decimal:
					_Constant = Decimal.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Double:
					_Constant = Double.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Int16:
					_Constant = Int16.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Int32:
					_Constant = Int32.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Int64:
					_Constant = Int64.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.SByte:
					_Constant = SByte.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Single:
					_Constant = Single.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.String:
					_Constant = (String)constant;
					break;
				case TypeCode.UInt16:
					_Constant = UInt16.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.UInt32:
					_Constant = UInt32.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.UInt64:
					_Constant = UInt64.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
			}
		}
		/// <summary>
		/// Defines default either from the item of the type sourceType or constant, if the item doesn't exist
		/// </summary>
		/// <param name="converter">Type code of the resulting constant</param>
		/// <param name="constant">String representation of the constant</param>
		/// <param name="sourceType">
		/// Bql query that is used to calculate default value.<br/>
		/// Accepts following types: IBqlSearch, IBqlSelect, IBqlField, IBqlTable
		/// </param>
		public PXDefaultAttribute(TypeCode converter, string constant, Type sourceType)
			: this(sourceType)
		{
			switch (converter)
			{
				case TypeCode.Boolean:
					_Constant = Boolean.Parse(constant);
					break;
				case TypeCode.Byte:
					_Constant = Byte.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Char:
					_Constant = Char.Parse(constant);
					break;
				case TypeCode.DateTime:
					_Constant = DateTime.Parse(constant, CultureInfo.InvariantCulture, DateTimeStyles.None);
					break;
				case TypeCode.Decimal:
					_Constant = Decimal.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Double:
					_Constant = Double.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Int16:
					_Constant = Int16.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Int32:
					_Constant = Int32.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Int64:
					_Constant = Int64.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.SByte:
					_Constant = SByte.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.Single:
					_Constant = Single.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.String:
					_Constant = (String)constant;
					break;
				case TypeCode.UInt16:
					_Constant = UInt16.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.UInt32:
					_Constant = UInt32.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case TypeCode.UInt64:
					_Constant = UInt64.Parse(constant, NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
			}
		}
		#endregion

		#region Runtime
		/// <summary>
		/// Overrides the default value for the particular data item
		/// </summary>
		/// <param name="cache">Cache containing the data item</param>
		/// <param name="data">Data item</param>
		/// <param name="field">Field to set default value for</param>
		/// <param name="def">Default value</param>
		public static void SetDefault(PXCache cache, object data, string field, object def)
		{
			if (data == null)
			{
				cache.SetAltered(field, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, field))
			{
				if (attr is PXDefaultAttribute)
				{
					((PXDefaultAttribute)attr)._Constant = def;
				}
			}
		}
		/// <summary>
		/// Overrides the default value for the whole cache
		/// </summary>
		/// <param name="cache">Cache</param>
		/// <param name="field">Field to set default value for</param>
		/// <param name="def">Default value</param>
		public static void SetDefault(PXCache cache, string field, object def)
		{
			cache.SetAltered(field, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(field))
			{
				if (attr is PXDefaultAttribute)
				{
					((PXDefaultAttribute)attr)._Constant = def;
				}
			}
		}
		/// <summary>
		/// Overrides the default value for the particular data item
		/// </summary>
		/// <typeparam name="Field">Field to set default value for</typeparam>
		/// <param name="cache">Cache containing the data item</param>
		/// <param name="data">Data iten</param>
		/// <param name="def">Default value</param>
		public static void SetDefault<Field>(PXCache cache, object data, object def)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXDefaultAttribute)
				{
					((PXDefaultAttribute)attr)._Constant = def;
				}
			}
		}
		/// <summary>
		/// Overrides BypassNullCheck property for the particular data item
		/// </summary>
		/// <typeparam name="Field">Field to bypassnullcheck for</typeparam>
		/// <param name="cache">Cache containing the data item</param>
		/// <param name="data">Data item</param>
		/// <param name="def">Property value</param>
		public static void SetPersistingCheck<Field>(PXCache cache, object data, PXPersistingCheck check)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXDefaultAttribute)
				{
					((PXDefaultAttribute)attr)._PersistingCheck = check;
				}
			}
		}
		/// <summary>
		/// Overrides BypassNullCheck property for the particular data item
		/// </summary>
		/// <typeparam name="Field">Field to bypassnullcheck for</typeparam>
		/// <param name="cache">Cache containing the data item</param>
		/// <param name="data">Data item</param>
		/// <param name="def">Property value</param>
		public static void SetPersistingCheck(PXCache cache, string field, object data, PXPersistingCheck check)
		{
			if (data == null)
			{
				cache.SetAltered(field, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, field))
			{
				if (attr is PXDefaultAttribute)
				{
					((PXDefaultAttribute)attr)._PersistingCheck = check;
				}
			}
		}
		/// <summary>
		/// Overrides the default value for the whole cache
		/// </summary>
		/// <typeparam name="Field">Field to set default value for</typeparam>
		/// <param name="cache">Cache</param>
		/// <param name="def">Default value</param>
		public static void SetDefault<Field>(PXCache cache, object def)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXDefaultAttribute)
				{
					((PXDefaultAttribute)attr)._Constant = def;
				}
			}
		}
		#endregion

		#region Implementation
		public static object Select(PXGraph graph, BqlCommand Select, Type sourceType, string sourceField, object row)
		{
			PXView view = graph.TypedViews.GetView(Select, false);
			int startRow = -1;
			int totalRows = 0;
			List<object> source = view.Select(
				new object[] { row },
				null,
				null,
				null,
				null,
				null,
				ref startRow,
				1,
				ref totalRows);
			if (source != null && source.Count > 0)
			{
				object item = source[source.Count - 1];
				if (item != null && item is PXResult)
				{
					item = ((PXResult)item)[sourceType];
				}
				return graph.Caches[sourceType].GetValue(item, sourceField);
			}
			return null;
		}

		/// <summary>
		/// Provides the default value
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="e">Event arguments to set the NewValue</param>
		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.NewValue == null)
			{
				if (_Select != null && !_SearchOnDefault && e.Row == null)
				{
				}
				else if (_Select != null)
				{
					List<BqlCommand> cmds = new List<BqlCommand>();
					if (_Select is IBqlCoalesce)
					{
						((IBqlCoalesce)_Select).GetCommands(cmds);

						foreach (BqlCommand Select in cmds)
						{
							Type SourceType = BqlCommand.GetItemType(((IBqlSearch)Select).GetField());
							string SourceField = ((IBqlSearch)Select).GetField().Name;

							e.NewValue = PXDefaultAttribute.Select(sender.Graph, Select, SourceType, SourceField, e.Row);
							if (e.NewValue != null)
							{
								e.Cancel = true;
								return;
							}
						}
					}
					else
					{
						e.NewValue = PXDefaultAttribute.Select(sender.Graph, _Select, _SourceType, _SourceField == null ? _FieldName : _SourceField, e.Row);
						if (e.NewValue != null)
						{
							e.Cancel = true;
							return;
						}
					}
				}
				else if (_SourceType != null)
				{
					PXCache cache = sender.Graph.Caches[_SourceType];
					if (cache.Current != null)
					{
						e.NewValue = cache.GetValue(cache.Current, _SourceField == null ? _FieldName : _SourceField);
						if (e.NewValue != null)
						{
							e.Cancel = true;
							return;
						}
					}
				}
				if (_Constant != null)
				{
					e.NewValue = _Constant;
				}
			}
		}
		/// <summary>
		/// Check if the value was set before saving the record to the database
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="e">Event arguments to retrive the value from the Row</param>
		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object val;
			if (PersistingCheck != PXPersistingCheck.Nothing &&
				((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update) &&
				((val = sender.GetValue(e.Row, _FieldOrdinal)) == null ||
				PersistingCheck == PXPersistingCheck.NullOrBlank && val is string && ((string)val).Trim() == String.Empty))
			{
				if (_MapErrorTo == null)
				{
					if (sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyKeepPreviousException(PXMessages.LocalizeFormat(ErrorMessages.FieldIsEmpty, _FieldName))))
					{
						throw new PXRowPersistingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
					}
				}
				else
				{
					string name = _MapErrorTo.Name;
					name = char.ToUpper(name[0]) + name.Substring(1);
					val = sender.GetValueExt(e.Row, name);
					if (val is PXFieldState)
					{
						val = ((PXFieldState)val).Value;
					}
					if (sender.RaiseExceptionHandling(name, e.Row, val, new PXSetPropertyKeepPreviousException(PXMessages.LocalizeFormat(ErrorMessages.IncorrectValueResultedEmptyField, name, _FieldName))))
					{
						throw new PXRowPersistingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
					}
				}
			}
		}
		/// <summary>
		/// Provides the default value as a part of the field state
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="e">Event arguments to set the ReturnState</param>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, null, _Constant == null, _PersistingCheck == PXPersistingCheck.Nothing ? 0 : 1, null, null, _Constant, _FieldName, null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
			}
		}
		#endregion

		//#region Initialization
		//public override void CacheAttached(PXCache sender)
		//{
		//    base.CacheAttached(sender);
		//    if (_Features != 0L && _PersistingCheck != PXPersistingCheck.Nothing)
		//    {
		//        sender.GetFieldType(_FieldOrdinal);
		//    }
		//}
		//#endregion
	}
	#endregion

	#region PXUnboundDefaultAttribute
	public class PXUnboundDefaultAttribute : PXDefaultAttribute, IPXRowSelectingSubscriber
	{
		#region Ctor
		/// <summary>
		/// Defines default from the item of the type sourceType
		/// </summary>
		/// <param name="sourceType">Type to get the default value from, if implements IBqlField and is nested, then defines the source field as well</param>
		public PXUnboundDefaultAttribute(Type sourceType)
			: base(sourceType)
		{
			_PersistingCheck = PXPersistingCheck.Nothing;
		}
		/// <summary>
		/// Defines default as a constant value
		/// </summary>
		/// <param name="constant">Constant default value</param>
		public PXUnboundDefaultAttribute(object constant)
			: base(constant)
		{
			_PersistingCheck = PXPersistingCheck.Nothing;
		}
		/// <summary>
		/// Defines default either from the item of the type sourceType or constant, if the item doesn't exist
		/// </summary>
		/// <param name="constant">Constant default value</param>
		/// <param name="sourceType">Type to get the default value from, if implements IBqlField and is nested, then defines the source field as well</param>
		public PXUnboundDefaultAttribute(object constant, Type sourceType)
			: base(constant, sourceType)
		{
			_PersistingCheck = PXPersistingCheck.Nothing;
		}
		/// <summary>
		/// Only check if the field value is not null before saving to the database, no default value provided
		/// </summary>
		public PXUnboundDefaultAttribute()
		{
			_PersistingCheck = PXPersistingCheck.Nothing;
		}
		/// <summary>
		/// Allows set the default value out of the string representation
		/// </summary>
		/// <param name="converter">Type code of the resulting constant</param>
		/// <param name="constant">String representation of the constant</param>
		public PXUnboundDefaultAttribute(TypeCode converter, string constant)
			: base(converter, constant)
		{
			_PersistingCheck = PXPersistingCheck.Nothing;
		}
		/// <summary>
		/// Defines default either from the item of the type sourceType or constant, if the item doesn't exist
		/// </summary>
		/// <param name="converter">Type code of the resulting constant</param>
		/// <param name="constant">String representation of the constant</param>
		/// <param name="sourceType">Type to get the default value from, if implements IBqlField and is nested, then defines the source field as well</param>
		public PXUnboundDefaultAttribute(TypeCode converter, string constant, Type sourceType)
			: base(converter, constant, sourceType)
		{
			_PersistingCheck = PXPersistingCheck.Nothing;
		}
		#endregion

		#region Implementation
		/// <summary>
		/// Provides the default value
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="e">Event arguments with a row reading</param>
		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				object val = null;
				using (PXConnectionScope ts = new PXConnectionScope())
				{
					bool res = sender.RaiseFieldDefaulting(_FieldName, e.Row, out val);
					if (res)
					{
						sender.RaiseFieldUpdating(_FieldName, e.Row, ref val);
					}
				}
				sender.SetValue(e.Row, _FieldOrdinal, val);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBDefaultAttribute
	/// <summary>
	/// Provides default value for the property or parameter.
	/// Use for defaulting from the auto generated key field
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
	public class PXDBDefaultAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber
	{
		#region State
		protected Type _SourceType;
		protected Type _OriginSourceType = null;
		protected string _SourceField;
		protected BqlCommand _Select;
		protected bool _DefaultForInsert = true;
		protected bool _DefaultForUpdate = true;
		protected PXPersistingCheck _PersistingCheck = PXPersistingCheck.Null;

		public virtual PXPersistingCheck PersistingCheck
		{
			get
			{
				return _PersistingCheck;
			}
			set
			{
				_PersistingCheck = value;
			}
		}

		/// <summary>
		/// Determines whethe the default value is used on update operation.
		/// </summary>
		public bool DefaultForUpdate
		{
			get
			{
				return _DefaultForUpdate;
			}
			set
			{
				_DefaultForUpdate = value;
			}
		}

		/// <summary>
		/// Determines whethe the default value is used on insert operation.
		/// </summary>
		public bool DefaultForInsert
		{
			get
			{
				return _DefaultForInsert;
			}
			set
			{
				_DefaultForInsert = value;
			}
		}
		protected class FlagHandler
		{
			public bool? Value;
			public Dictionary<object, object> Persisted;
		}
		protected FlagHandler _IsRestriction;
		protected virtual void EnsureIsRestriction(PXCache sender)
		{
			if (_IsRestriction.Value != null || _SourceType == null) return;

			string name = _SourceField ?? _FieldName;
			PXCache parentCache = sender.Graph.Caches[_SourceType];
			if (string.Equals(parentCache.Identity, name, StringComparison.OrdinalIgnoreCase))
			{
				_IsRestriction.Value = true;
			}
			else
			{
				PXCommandPreparingEventArgs.FieldDescription description;
				parentCache.RaiseCommandPreparing(name, null, null, PXDBOperation.Update, null, out description);
				_IsRestriction.Value = description != null && description.IsRestriction;
			}
		}
		#endregion

		#region Ctor
		public PXDBDefaultAttribute()
			: this(null)
		{

		}
		/// <summary>
		/// Defines default from the item of the type sourceType
		/// </summary>
		/// <param name="sourceType">Type to get the default value from, if implements IBqlField and is nested, then defines the source field as well</param>
		public PXDBDefaultAttribute(Type sourceType)
		{
			_OriginSourceType = sourceType;
			SetSourceType(null, null);
		}
		#endregion

		#region Implementation
		/// <summary>
		/// Provides the default value
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="e">Event arguments to set the NewValue</param>
		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (_Select != null)
			{
				PXView view = sender.Graph.TypedViews.GetView(_Select, false);
				List<object> source = view.SelectMultiBound(new object[] { e.Row });
				if (source != null && source.Count > 0)
				{
					object item = source[source.Count - 1];
					if (item != null && item is PXResult)
					{
						item = ((PXResult)item)[_SourceType];
					}
					e.NewValue = sender.Graph.Caches[_SourceType].GetValue(item, _SourceField ?? _FieldName);
					e.Cancel = true;
					return;
				}
			}
			else if (_SourceType != null)
			{
				PXCache cache = sender.Graph.Caches[_SourceType];
				if (cache.Current != null)
				{
					e.NewValue = cache.GetValue(cache.Current, _SourceField ?? _FieldName);
					e.Cancel = true;
					return;
				}
			}
		}
		/// <summary>
		/// Re-default the value. Check if the value was set before saving the record to the database
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="e">Event arguments to retrive the value from the Row</param>
		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && _DefaultForInsert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update && _DefaultForUpdate) && _SourceType != null)
			{
				EnsureIsRestriction(sender);
				if (_IsRestriction.Value == true)
				{
					object key = sender.GetValue(e.Row, _FieldOrdinal);
					if (_IsRestriction.Persisted != null && key != null)
					{
						object parent;
						if (_IsRestriction.Persisted.TryGetValue(key, out parent))
						{
							key = sender.Graph.Caches[_SourceType].GetValue(parent, _SourceField ?? _FieldName);
							sender.SetValue(e.Row, _FieldOrdinal, key);
							if (key != null)
							{
								_IsRestriction.Persisted[key] = parent;
							}
						}
					}
				}
				else
				{
					sender.SetValue(e.Row, _FieldOrdinal, null);
					if (_Select != null)
					{
						PXView view = sender.Graph.TypedViews.GetView(_Select, false);
						List<object> source = view.SelectMultiBound(new object[] { e.Row });
						if (source != null && source.Count > 0)
						{
							object result = source[source.Count - 1];
							if (result is PXResult) result = ((PXResult)result)[0];
							sender.SetValue(e.Row, _FieldOrdinal, sender.Graph.Caches[_SourceType].GetValue(result, _SourceField ?? _FieldName));
						}
					}
					else if (_SourceType != null)
					{
						PXCache cache = sender.Graph.Caches[_SourceType];
						if (cache.Current != null)
						{
							sender.SetValue(e.Row, _FieldOrdinal, cache.GetValue(cache.Current, _SourceField ?? _FieldName));
						}
					}
				}
			}
			if (PersistingCheck != PXPersistingCheck.Nothing &&
				((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && _DefaultForInsert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update && _DefaultForUpdate) &&
				sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				if (sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyKeepPreviousException(PXMessages.LocalizeFormat(ErrorMessages.FieldIsEmpty, _FieldName))))
				{
					throw new PXRowPersistingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
				}
			}
		}

		/// <summary>
		/// Rollback changes if the record was not saved to the database
		/// </summary>
		/// <param name="sender">Cache</param>
		/// <param name="e">Event arguments to check operation and get the row to process</param>
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && _DefaultForInsert && e.TranStatus == PXTranStatus.Aborted && _SourceType != null)
			{
				EnsureIsRestriction(sender);
				if (_IsRestriction.Value == true)
				{
					object key = sender.GetValue(e.Row, _FieldOrdinal);
					if (_IsRestriction.Persisted != null && key != null)
					{
						object parent;
						if (_IsRestriction.Persisted.TryGetValue(key, out parent))
						{
							sender.SetValue(e.Row, _FieldOrdinal, sender.Graph.Caches[_SourceType].GetValue(parent, _SourceField ?? _FieldName));
						}
					}
				}
				else
				{
					sender.SetValue(e.Row, _FieldOrdinal, null);
					if (_Select != null)
					{
						PXView view = sender.Graph.TypedViews.GetView(_Select, false);
						List<object> source = view.SelectMultiBound(new object[] { e.Row });
						if (source != null && source.Count > 0)
						{
							sender.SetValue(e.Row, _FieldOrdinal, sender.Graph.Caches[_SourceType].GetValue(source[source.Count - 1], _SourceField ?? _FieldName));
						}
					}
					else if (_SourceType != null)
					{
						PXCache cache = sender.Graph.Caches[_SourceType];
						if (cache.Current != null)
						{
							sender.SetValue(e.Row, _FieldOrdinal, cache.GetValue(cache.Current, _SourceField ?? _FieldName));
						}
					}
				}
			}
		}

		public void SourceRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			EnsureIsRestriction(sender);
			if (_IsRestriction.Value == true)
			{
				object key = sender.GetValue(e.Row, _SourceField ?? _FieldName);
				if (key != null)
				{
					if (_IsRestriction.Persisted == null)
					{
						_IsRestriction.Persisted = new Dictionary<object, object>();
					}
					if (_Select == null || _Select.Meet(sender, e.Row))
					{
						_IsRestriction.Persisted[key] = e.Row;
					}
				}
			}
		}

		protected virtual void Parameter_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select &&
				(e.Operation & PXDBOperation.Option) != PXDBOperation.ReadOnly && e.Row == null && _SourceType != null)
			{
				PXCache cache = sender.Graph.Caches[_SourceType];
				PXCommandPreparingEventArgs.FieldDescription descr;

				string key = cache.GetItemType().FullName + "_AutoNumber";
				HashSet<string> fields;
				if ((fields = PXContext.GetSlot<HashSet<string>>(key)) == null || !fields.Contains(_SourceField ?? _FieldName))
				{
					return;
				}

				if (!cache.RaiseCommandPreparing(_SourceField ?? _FieldName, null, e.Value, e.Operation, null, out descr))
				{
					e.DataValue = descr.DataValue;
					e.Cancel = true;
				}
			}
		}
		#endregion

		#region Runtime
		/// <summary>
		/// Overrides the defaulting for updated items.
		/// </summary>
		/// <typeparam name="Field">Field to set default value for</typeparam>
		/// <param name="cache">Cache containing the data item</param>
		/// <param name="data">Data item</param>
		/// <param name="def">Flag value</param>
		public static void SetDefaultForUpdate<Field>(PXCache cache, object data, bool def)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXDBDefaultAttribute attr in cache.GetAttributes<Field>(data).OfType<PXDBDefaultAttribute>())
			{
				(attr)._DefaultForUpdate = def;
			}
		}

		/// <summary>
		/// Overrides the defaulting for inserted items.
		/// </summary>
		/// <typeparam name="Field">Field to set default value for</typeparam>
		/// <param name="cache">Cache containing the data item</param>
		/// <param name="data">Data item</param>
		/// <param name="def">Flag value</param>
		public static void SetDefaultForInsert<Field>(PXCache cache, object data, bool def)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXDBDefaultAttribute attr in cache.GetAttributes<Field>(data).OfType<PXDBDefaultAttribute>())
			{
				(attr)._DefaultForInsert = def;
			}
		}

		public static void SetSourceType(PXCache cache, string field, Type sourceType)
		{
			cache.SetAltered(field, true);
			foreach (PXDBDefaultAttribute attribute in cache.GetAttributes(field).OfType<PXDBDefaultAttribute>())
			{
				attribute.SetSourceType(cache, sourceType);
			}
		}

		public static void SetSourceType<Field>(PXCache cache, Type sourceType)
			where Field : IBqlField
		{
			SetSourceType(cache, typeof(Field).Name, sourceType);
		}

		public static void SetSourceType(PXCache cache, object data, string field, Type sourceType)
		{
			if (data == null)
			{
				cache.SetAltered(field, true);
			}
			foreach (PXDBDefaultAttribute attribute in cache.GetAttributes(data, field).OfType<PXDBDefaultAttribute>())
			{
				attribute.SetSourceType(cache, sourceType);
			}
		}

		public static void SetSourceType<Field>(PXCache cache, object data, Type sourceType)
			where Field : IBqlField
		{
			SetSourceType(cache, data, typeof(Field).Name, sourceType);
		}

		#endregion

		#region Initialization

		protected virtual void SetSourceType(PXCache cache, Type sourceType)
		{
			sourceType = sourceType ?? _OriginSourceType;
			Type oldSourceType = _SourceType;

			//if (sourceType != _OriginSourceType || sourceType == null)
			{
				if (sourceType == null)
				{
					_Select = null;
					_SourceType = null;
					_SourceField = null;
				}
				else if (typeof(IBqlSearch).IsAssignableFrom(sourceType))
				{
					_Select = BqlCommand.CreateInstance(sourceType);
					_SourceType = BqlCommand.GetItemType(((IBqlSearch)_Select).GetField());
					_SourceField = ((IBqlSearch)_Select).GetField().Name;
					_SourceField = char.ToUpper(_SourceField[0]) + _SourceField.Substring(1);
				}
				else if (sourceType.IsNested && typeof(IBqlField).IsAssignableFrom(sourceType))
				{
					_SourceType = BqlCommand.GetItemType(sourceType);
					_SourceField = sourceType.Name;
					_SourceField = char.ToUpper(_SourceField[0]) + _SourceField.Substring(1);
				}
				else if (typeof(IBqlTable).IsAssignableFrom(sourceType))
				{
					_Select = null;
					_SourceType = sourceType;
					_SourceField = null;
				}
				else
				{
					throw new PXArgumentException("sourceType", ErrorMessages.CantCreateForeignKeyReference, sourceType);
				}
			}
			
			if (cache != null && oldSourceType != _SourceType)
			{
				if (oldSourceType != null)
					cache.Graph.RowPersisting.RemoveHandler(oldSourceType, SourceRowPersisting);
				if (_SourceType != null)
					cache.Graph.RowPersisting.AddHandler(_SourceType, SourceRowPersisting);
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			if (_SourceType != null)
				sender.Graph.RowPersisting.AddHandler(_SourceType, SourceRowPersisting);
			_IsRestriction = new FlagHandler();

			if (_Select == null)
			{
				sender.Graph.CommandPreparing.AddHandler(sender.GetItemType(), _FieldName, Parameter_CommandPreparing);
			}
		}
		#endregion
	}
	#endregion

	#region PXSelectorAttribute
	/// <summary>
	/// This attribute provides foreign key functionality.<br/>
	/// It displays the list of records available for selection, verifies if value provided is included in this list,<br/>
	/// controls surrogate-natural key conversion, defines list of columns shown in UI, and displaying description of a foreign record.<br/>
	/// For performance reasons the attribute caches referenced records.<br/>
	/// </summary>
	/// <example>
	/// In this example ClassID property will accept identifiers of active Lead Classes only.<br/>
	/// User Interface will display descriptions for Lead Class records taking it from CRLeadClass.description field and will cache records in memory to reduce the number of database calls.<br/>
	/// 
	/// <![CDATA[
	///public abstract class classID : PX.Data.IBqlField {}
	///[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
	///[PXUIField(DisplayName = "Class ID")]
	///[PXSelector(typeof(Search<CRLeadClass.cRLeadClassID, Where<CRLeadClass.isActive, Equal<True>>>), DescriptionField = typeof(CRLeadClass.description), CacheGlobal = true)]
	///public virtual String ClassID
	///{
	///	get;
	///	set;
	///}
	/// ]]>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXSelectorAttribute))]
	public class PXSelectorAttribute : PXEventSubscriberAttribute, IPXFieldVerifyingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected Type _Type;
		protected Type _BqlType;
		protected Type _CacheType;
		protected BqlCommand _Select;
		/// <summary>
		/// Returns Bql command used for selection of referenced records.
		/// </summary>
		public virtual BqlCommand GetSelect()
		{
			return _Select;
		}
		protected int _ParsCount;
		protected BqlCommand _PrimarySelect;
		protected BqlCommand _OriginalSelect;
		protected BqlCommand _NaturalSelect;
		protected BqlCommand _UnconditionalSelect;
		protected string[] _FieldList;
		protected string[] _HeaderList;
		protected string _ViewName;
		protected Type _DescriptionField;
		protected Type _SubstituteKey;
		protected bool _DirtyRead;
		protected bool _Filterable;
		protected bool _CacheGlobal;
		protected bool _ViewCreated;
		protected bool _IsOwnView;
		protected UIFieldRef _UIFieldRef;

		protected Delegate _ViewHandler;

		/// <summary>
		/// When this flag is set, attribute caches records retrieved from the database.
		/// </summary>
		public virtual bool CacheGlobal
		{
			get
			{
				return _CacheGlobal;
			}
			set
			{
				if (_NaturalSelect != null && _CacheGlobal != value)
				{
					IBqlParameter[] pars = _NaturalSelect.GetParameters();
					Type surrogate = pars[pars.Length - 1].GetReferencedType();
					if (!value)
					{
						_NaturalSelect = _Select.WhereAnd(BqlCommand.Compose(typeof(Where<,>), surrogate, typeof(Equal<>), typeof(Required<>), surrogate));
					}
					else
					{
						Type field = ((IBqlSearch)_Select).GetField();
						_NaturalSelect = BqlCommand.CreateInstance(typeof(Search<,>), field, typeof(Where<,>), surrogate, typeof(Equal<>), typeof(Required<>), surrogate);
					}
				}
				_CacheGlobal = value;
			}
		}
		/// <summary>
		/// Field of the referenced table that contains the description
		/// </summary>
		public virtual Type DescriptionField
		{
			get
			{
				return _DescriptionField;
			}
			set
			{
				if (value == null || typeof(IBqlField).IsAssignableFrom(value) && value.IsNested)
				{
					_DescriptionField = value;
				}
				else
				{
					throw new PXException(ErrorMessages.CantSetDescriptionField, value);
				}
			}
		}
		/// <summary>
		/// Field of the referenced table that contains substitution unique key.</br>
		/// When set, external calls to the field will work with substitute key instead of value stored in the field.</br>
		/// For example, Account table has two unique identifiers - numeric AccountID and user-friendly text AccountCD.</br>
		/// To reduce size of the database we reference to Account table with AccountID, for example in GLTran table.</br>
		/// However, when displaying stored value to a user we need to convert it to a text field AccountCD.</br>
		/// It can be achived with SubstituteKey property assigned in the attribute declaration:</>
		/// [PXSelector(typeof(Search<Account.accountID>), SubstituteKey = typeof(Account.accountCD))]</br>
		/// Notice, that in order to provide proper integrity, there should be two separate unique keys by AccountID and AccountCD fields on Account table.</br>
		/// But mark only AccountCD field with IsKey = true property in the data access class.
		/// </summary>
		public virtual Type SubstituteKey
		{
			get
			{
				return _SubstituteKey;
			}
			set
			{
				if (value != null && typeof(IBqlField).IsAssignableFrom(value) && value.IsNested)
				{
					_SubstituteKey = value;
					if (!_CacheGlobal)
					{
						_NaturalSelect = _Select.WhereAnd(BqlCommand.Compose(typeof(Where<,>), value, typeof(Equal<>), typeof(Required<>), value));
					}
					else
					{
						Type field = ((IBqlSearch)_Select).GetField();
						_NaturalSelect = BqlCommand.CreateInstance(typeof(Search<,>), field, typeof(Where<,>), value, typeof(Equal<>), typeof(Required<>), value);
					}
				}
				else
				{
					throw new PXException(ErrorMessages.CantSubstituteKey, value);
				}
			}
		}
		/// <summary>
		/// Returns a BqlField that unique identifies a referenced record. Substitute Key property does not affect this, it will always ID, not CD.</br>
		/// Generally it is the first parameter of the Search<> command passed to constructor of the attribute.
		/// </summary>
		public virtual Type Field
		{
			get
			{
				return ((IBqlSearch)_Select).GetField();
			}
		}
		/// <summary>
		/// if false then selector rows are taken from database, not saved rows ignored.
		/// if true then not saved rows are taken into account.
		/// </summary>
		public virtual bool DirtyRead
		{
			get
			{
				return _DirtyRead;
			}
			set
			{
				_DirtyRead = value;
			}
		}

		/// <summary>
		/// Allows to control validation process.
		/// </summary>
		public bool ValidateValue = true;

		/// <summary>
		/// Allows storing user-defined filters in the database.
		/// </summary>
		public virtual bool Filterable
		{
			get
			{
				return _Filterable;
			}
			set
			{
				_Filterable = value;
			}
		}
		/// <summary>
		/// The list of explicitly defined headers for selector columns, if not specified, it will be taken from selector columns' display names.
		/// </summary>
		public virtual string[] Headers
		{
			get
			{
				return _HeaderList;
			}
			set
			{
				if (_FieldList == null || value.Length != _FieldList.Length)
				{
					throw new PXArgumentException("Headers", ErrorMessages.HeadersNotMeetColList);
				}
				_HeaderList = value;
			}
		}

		protected Type _ValueField;
		public Type ValueField
		{
			get
			{
				return _ValueField;
			}
		}

		protected PXSelectorMode _SelectorMode;
		public virtual PXSelectorMode SelectorMode
		{
			get
			{
				string key = _CacheType.FullName + "_AutoNumber";
				HashSet<string> fields;
				if ((fields = PXContext.GetSlot<HashSet<string>>(key)) == null || !fields.Contains(_FieldName))
				{
					return _SelectorMode;
				}
				return _SelectorMode & ~PXSelectorMode.NoAutocomplete;
			}
			set
			{
				this._SelectorMode = value;
			}
		}

		public BqlCommand PrimarySelect
		{
			get { return _PrimarySelect; }
		}

		public int ParsCount
		{
			get { return _ParsCount; }
		}
		#endregion

		#region Ctor
		/// <summary>
		/// Creates a selector. List of columns will be created automatically.</br>
		/// The attribute will include all columns marked with the property Visibility = PXUIVisibility.SelectorVisible in that list.</br>
		/// </summary>
		/// <param name="type">Referenced table along with a field that identifies a record in that table.</>
		/// Should be either IBqlField or IBqlSearch</param>
		public PXSelectorAttribute(Type type)
		{
			if (type == null)
			{
				throw new PXArgumentException("type", ErrorMessages.ArgumentNullException);
			}
			if (typeof(IBqlSearch).IsAssignableFrom(type))
			{
				_Select = BqlCommand.CreateInstance(type);
				_Type = BqlCommand.GetItemType(((IBqlSearch)_Select).GetField());
			}
			else if (type.IsNested && typeof(IBqlField).IsAssignableFrom(type))
			{
				_Type = BqlCommand.GetItemType(type);
				_Select = BqlCommand.CreateInstance(typeof(Search<>), type);
			}
			else
			{
				throw new PXArgumentException("type", ErrorMessages.CantCreateForeignKeyReference, type);
			}
			_BqlType = _Type;
			while (typeof(IBqlTable).IsAssignableFrom(_BqlType.BaseType)
				&& !_BqlType.IsDefined(typeof(PXTableAttribute), false)
				&& !_BqlType.IsDefined(typeof(PXTableNameAttribute), false))
			{
				_BqlType = _BqlType.BaseType;
			}
			Type field = ((IBqlSearch)_Select).GetField();
			_ValueField = field;
			_ViewName = GenerateViewName();
			_PrimarySelect = _Select.WhereAnd(BqlCommand.Compose(typeof(Where<,>), field, typeof(Equal<>), typeof(Required<>), field));
			_OriginalSelect = BqlCommand.CreateInstance(_Select.GetSelectType());
			_UnconditionalSelect = BqlCommand.CreateInstance(typeof(Search<,>), field, typeof(Where<,>), field, typeof(Equal<>), typeof(Required<>), field);
		}
		/// <summary>
		/// Creates a selector overriding columns list.
		/// </summary>
		/// <param name="type">Referenced table. Should be either IBqlField or IBqlSearch</param>
		/// <param name="fieldList">Fields to display in the selector</param>
		/// <param name="headerList">Headers of the selector columns</param>
		public PXSelectorAttribute(Type type, params Type[] fieldList)
			: this(type)
		{
			_FieldList = new string[fieldList.Length];
			Type[] tables = _Select.GetTables();
			for (int i = 0; i < fieldList.Length; i++)
			{
				if (!fieldList[i].IsNested || !typeof(IBqlField).IsAssignableFrom(fieldList[i]))
				{
					throw new PXArgumentException("fieldList", ErrorMessages.InvalidSelectorColumn);
				}
				if (tables.Length <= 1 || BqlCommand.GetItemType(fieldList[i]).IsAssignableFrom(tables[0]))
				{
					_FieldList[i] = fieldList[i].Name;
				}
				else
				{
					_FieldList[i] = BqlCommand.GetItemType(fieldList[i]).Name + "__" + fieldList[i].Name;
				}
			}
		}

		public interface IPXAdjustableView { }

		public class PXAdjustableView : PXView, IPXAdjustableView
		{
			public PXAdjustableView(PXGraph graph, bool isReadOnly, BqlCommand @select, Delegate handler)
				: base(graph, isReadOnly, @select, handler)
			{
			}
		}

		public BqlCommand WhereAnd(PXCache sender, Type whr)
		{
			if (!typeof(IBqlWhere).IsAssignableFrom(whr)) return _PrimarySelect;

			_Select = _Select.WhereAnd(whr);

			if (_ViewHandler == null)
			{
				_ViewHandler = new PXSelectDelegate(
					delegate
					{
						int startRow = PXView.StartRow;
						int totalRows = 0;

						if (PXView.MaximumRows == 1)
						{
							IBqlParameter[] selpars = _Select.GetParameters();
							object[] parameters = PXView.Parameters;
							List<object> pars = new List<object>();

							for (int i = 0; i < selpars.Length && i < parameters.Length; i++)
							{
								if (selpars[i].MaskedType != null)
								{
									break;
								}
								if (selpars[i].IsVisible)
								{
									pars.Add(parameters[i]);
								}
							}

							return new PXView(sender.Graph, !_DirtyRead, _OriginalSelect).Select(PXView.Currents, pars.ToArray(), PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
						}

						return null;
					});
			}

			if (_ViewCreated)
			{
				// recreate selector view
				CreateView(sender);
			}

			return _PrimarySelect.WhereAnd(whr);
		}

		/// <summary>
		/// Generates default view name. View name is used by UI controls when selecting list of records available for selection.
		/// </summary>
		/// <returns>A string that references a PXView instance which will be used to retrive a list of records.</returns>
		protected virtual string GenerateViewName()
		{
			if (!typeof(IBqlSearch).IsAssignableFrom(_Select.GetType())) return null;

			var parameters = _Select.GetParameters();
			var bld = new StringBuilder("_");
			bld.Append(_Type.Name);
			if (parameters != null)
				foreach (var par in parameters)
				{
					if (!par.HasDefault) throw new PXArgumentException("sourceType", ErrorMessages.NotCurrentOrOptionalParameter);
					if (par.IsVisible) _ParsCount++;
					var t = par.GetReferencedType();
					bld.Append('_');
					bld.Append(BqlCommand.GetItemType(t).Name);
					bld.Append('.');
					bld.Append(t.Name);
				}
			bld.Append('_');
			return bld.ToString();
		}

		#endregion

		#region Runtime
		/// <summary>
		/// A wrapper to PXView.SelectMultiBound() method, extracts the first table in a row if a result of a join is returned.<br/>
		/// While we are looking for a single record here, we still call SelectMulti() for performance reason, to hit cache and get the result of previously executed queries if any.<br/>
		/// 'Bound' means we will take omitted parameters from explicitly defined array of rows, not from current records set in the graph.
		/// </summary>
		/// <param name="view">PXView instance to be called for a selection result</param>
		/// <param name="currents">List of rows used as a source for omitted parameter values</param>
		/// <param name="pars">List of parameters to be passed to the query</param>
		/// <returns>Foreign record retrieved</returns>
		internal static object SelectSingleBound(PXView view, object[] currents, params object[] pars)
		{
			List<object> ret = view.SelectMultiBound(currents, pars);
			if (ret.Count > 0)
			{
				if (ret[0] is PXResult)
				{
					return ((PXResult)ret[0])[0];
				}
				return ret[0];
			}

			return null;
		}
		/// <summary>
		/// A wrapper to PXView.SelectSingleBound() method, extracts the first table in a row if a result of a join is returned.<br/>
		/// </summary>
		/// <param name="view">PXView instance to be called for a selection result</param>
		/// <param name="pars">List of parameters to be passed to the query</param>
		/// <returns>Foreign record retrieved</returns>
		internal static object SelectSingle(PXView view, params object[] pars)
		{
			List<object> ret = view.SelectMulti(pars);
			if (ret.Count > 0)
			{
				if (ret[0] is PXResult)
				{
					return ((PXResult)ret[0])[0];
				}
				return ret[0];
			}
			return null;
		}

		internal static object SelectSingle(PXCache cache, object data, string field)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(field))
			{
				if (attr is PXSelectorAttribute)
				{
					PXView select = ((PXSelectorAttribute)attr).GetView(cache, ((PXSelectorAttribute)attr)._PrimarySelect, !((PXSelectorAttribute)attr)._DirtyRead);
					object[] pars = new object[((PXSelectorAttribute)attr)._ParsCount + 1];
					pars[pars.Length - 1] = cache.GetValue(data, ((PXSelectorAttribute)attr)._FieldOrdinal);

					List<object> ret = select.SelectMultiBound(new object[] { data }, pars);
					if (ret.Count > 0)
					{
						return ret[0];
					}

					return null;
				}
			}
			return null;
		}
		/// <summary>
		/// Returns cached typed view, can be ovirriden to substitute a view with a delegate instead.
		/// </summary>
		/// <param name="cache">PXCache instance, used to retrive a graph object</param>
		/// <param name="select">Bql command to be searched</param>
		/// <param name="dirtyRead">Flag to separate result sets either merged with not saved changes or not</param>
		/// <returns></returns>
		protected virtual PXView GetView(PXCache cache, BqlCommand select, bool isReadOnly)
		{
			return cache.Graph.TypedViews.GetView(select, isReadOnly);
		}
		/// <summary>
		/// Gets the data item referenced by the selector
		/// </summary>
		/// <param name="cache">Cache</param>
		/// <param name="data">Data item to get parameter values</param>
		/// <param name="field">Name of the field the selector is attached to</param>
		/// <returns>Foreign record</returns>
		public static object Select(PXCache cache, object data, string field)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(field))
			{
				if (attr is PXSelectorAttribute)
				{
					PXView select = ((PXSelectorAttribute)attr).GetView(cache, ((PXSelectorAttribute)attr)._PrimarySelect, !((PXSelectorAttribute)attr)._DirtyRead);
					object[] pars = new object[((PXSelectorAttribute)attr)._ParsCount + 1];
					pars[pars.Length - 1] = cache.GetValue(data, ((PXSelectorAttribute)attr)._FieldOrdinal);
					return SelectSingleBound(select, new object[] { data }, pars);
				}
			}
			return null;
		}

		/// <summary>
		/// Selects the first row returned by selector attribute.
		/// </summary>
		/// <param name="cache">Cache</param>
		/// <param name="data">Data item to get parameter values</param>
		/// <param name="field">Name of the field the selector is attached to</param>
		/// <returns>Foreign record</returns>
		public static object SelectFirst<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			return SelectFirst(cache, data, typeof(Field).Name);
		}
		public static object SelectFirst(PXCache cache, object data, string field)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(field))
			{
				if (attr is PXSelectorAttribute)
				{
					PXView view = cache.Graph.TypedViews.GetView(((PXSelectorAttribute)attr)._Select, !((PXSelectorAttribute)attr)._DirtyRead);
					int startRow = 0;
					int totalRows = 0;
					List<object> source = view.Select(new object[] { data }, null, null, null, null, null, ref startRow, 1, ref totalRows);
					if (source != null && source.Count > 0)
					{
						object item = source[source.Count - 1];
						if (item != null && item is PXResult)
						{
							item = ((PXResult)item)[0];
						}
						return item;
					}
					return null;
				}
			}
			return null;
		}

		/// <summary>
		/// Selects the last row returned by selector attribute. Similar to PXDefaultAttribute
		/// </summary>
		/// <param name="cache">Cache</param>
		/// <param name="data">Data item to get parameter values</param>
		/// <param name="field">Name of the field the selector is attached to</param>
		/// <returns>Foreign record</returns>
		public static object SelectLast<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			return SelectLast(cache, data, typeof(Field).Name);
		}
		public static object SelectLast(PXCache cache, object data, string field)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(field))
			{
				if (attr is PXSelectorAttribute)
				{
					PXView view = cache.Graph.TypedViews.GetView(((PXSelectorAttribute)attr)._Select, !((PXSelectorAttribute)attr)._DirtyRead);
					int startRow = -1;
					int totalRows = 0;
					List<object> source = view.Select(new object[] { data }, null, null, null, null, null, ref startRow, 1, ref totalRows);
					if (source != null && source.Count > 0)
					{
						object item = source[source.Count - 1];
						if (item != null && item is PXResult)
						{
							item = ((PXResult)item)[0];
						}
						return item;
					}
					return null;
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the data item referenced by the selector
		/// </summary>
		/// <param name="cache">Cache</param>
		/// <param name="data">Data item to get parameter values</param>
		/// <param name="field">Name of the field the selector is attached to</param>
		/// <param name="value">Value to search the referenced table for</param>
		/// <returns>Foreign record</returns>
		public static object Select(PXCache cache, object data, string field, object value)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(field))
			{
				if (attr is PXSelectorAttribute)
				{
					return GetItem(cache, (PXSelectorAttribute)attr, data, value);
				}
			}
			return null;
		}
		/// <summary>
		/// Gets the data item referenced by the selector
		/// </summary>
		/// <param name="cache">Cache</param>
		/// <param name="attr">The instance of the PXSelectorAttribute to query for an item</param>
		/// <param name="data">Data item to get parameter values</param>
		/// <param name="key">Value to search the referenced table for</param>
		/// <returns>Foreign record</returns>
		public static object GetItem(PXCache cache, PXSelectorAttribute attr, object data, object key)
		{
			object row = null;
			Dictionary<object, KeyValuePair<object, bool>> dict = null;
			if (attr._CacheGlobal && key != null)
			{
				dict = PXDatabase.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(attr._Type.FullName, cache.Graph.Caches[attr._Type].BqlTable);
				lock (((ICollection)dict).SyncRoot)
				{
					KeyValuePair<object, bool> pair;
					if (dict.TryGetValue(key, out pair) && !pair.Value)
					{
						row = pair.Key;
					}
				}
			}
			if (row == null)
			{
				PXView select = attr.GetView(cache, attr._PrimarySelect, !attr._DirtyRead);
				object[] pars = new object[attr._ParsCount + 1];
				pars[pars.Length - 1] = key;
				row = SelectSingleBound(select, new object[] { data }, pars);
				if (row == null)
				{
					return null;
				}
				if (attr._CacheGlobal && key != null && select.Cache.GetStatus(row) == PXEntryStatus.Notchanged && !PXDatabase.ReadDeleted && select.Cache.Keys.Count <= 1)
				{
					lock (((ICollection)dict).SyncRoot)
					{
						dict[key] = new KeyValuePair<object, bool>(row, false);
					}
				}
			}
			return row;
		}
		/// <summary>
		/// Clears internal PXSelectorAttribute cache in case of modifications made to the foreign table for instance.</br>
		/// Typically you need not call this method because the attribute subscribes on the change notifications and drops the cache automatically.
		/// </summary>
		/// <typeparam name="Table">A Bql table with stored records that need to be dropped</typeparam>
		public static void ClearGlobalCache<Table>()
			where Table : IBqlTable
		{
			PXDatabase.ResetSlot<Dictionary<object, KeyValuePair<object, bool>>>(typeof(Table).FullName, typeof(Table));
		}
		/// <summary>
		/// Clears internal PXSelectorAttribute cache in case of modifications made to the foreign table for instance.</br>
		/// Typically you need not call this method because the attribute subscribes on the change notifications and drops the cache automatically.
		/// </summary>
		/// <param name="table">A Bql table with stored records that need to be dropped</param>
		public static void ClearGlobalCache(Type table)
		{
			if (table == null)
			{
				throw new PXArgumentException(ErrorMessages.ArgumentNullException, "table");
			}
			PXDatabase.ResetSlot<Dictionary<object, KeyValuePair<object, bool>>>(table.FullName, table);
		}
		/// <summary>
		/// The method is used to get a value of the field from a foreign record.
		/// </summary>
		/// <param name="cache">PXCache instance for a data access class where the PXSelectorAttribute is defined</param>
		/// <param name="data">Instance of a data access class which contains a reference to the foreign record</param>
		/// <param name="field">Field name of a reference</param>
		/// <param name="value">Value of a reference, i.e. a key value of the referenced record</param>
		/// <param name="foreignField">Field name on the referenced record what value we are looking for</param>
		/// <returns>Value retrieved</returns>
		public static object GetField(PXCache cache, object data, string field, object value, string foreignField)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(data, field))
			{
				if (attr is PXSelectorAttribute)
				{
					object row = null;
					Dictionary<object, KeyValuePair<object, bool>> dict = null;
					if (((PXSelectorAttribute)attr)._CacheGlobal && value != null)
					{
						dict = PXDatabase.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(((PXSelectorAttribute)attr)._Type.FullName, cache.Graph.Caches[((PXSelectorAttribute)attr)._Type].BqlTable);
						lock (((ICollection)dict).SyncRoot)
						{
							KeyValuePair<object, bool> pair;
							if (dict.TryGetValue(value, out pair) && !pair.Value)
							{
								row = pair.Key;
							}
						}
					}
					if (row == null)
					{
						PXView select = ((PXSelectorAttribute)attr).GetView(cache, ((PXSelectorAttribute)attr)._PrimarySelect, !((PXSelectorAttribute)attr)._DirtyRead);
						object[] pars = new object[((PXSelectorAttribute)attr)._ParsCount + 1];
						pars[pars.Length - 1] = value;
						row = SelectSingleBound(select, new object[] { data }, pars);
						if (row == null)
						{
							return null;
						}
						if (((PXSelectorAttribute)attr)._CacheGlobal && value != null && !PXDatabase.ReadDeleted && select.Cache.Keys.Count <= 1)
						{
							lock (((ICollection)dict).SyncRoot)
							{
								dict[value] = new KeyValuePair<object, bool>(row, false);
							}
						}
					}
					return cache.Graph.Caches[((PXSelectorAttribute)attr)._Type].GetValue(row, foreignField) ?? new byte[0];
				}
			}
			return null;
		}
		/// <summary>
		/// Allows getting of a type of a referenced data access class.
		/// </summary>
		/// <param name="cache">PXCache instance for a data access class where the PXSelectorAttribute is defined</param>
		/// <param name="field">Field annotated with the PXSelectorAttribute we are querying in this call</param>
		/// <returns>Type of the foreign table</returns>
		public static Type GetItemType(PXCache cache, string field)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(field))
			{
				if (attr is PXSelectorAttribute)
				{
					return ((PXSelectorAttribute)attr)._Type;
				}
			}
			return null;
		}
		/// <summary>
		/// Gets the all data from selector
		/// </summary>
		/// <typeparam name="Field">Data field the selector is attached to</typeparam>
		/// <param name="cache">Cache</param>
		/// <returns>All records</returns>
		public static List<object> SelectAll<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			return SelectAll(cache, typeof(Field).Name, data);
		}
		public static List<object> SelectAll(PXCache cache, string fieldname, object data)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(fieldname))
			{
				if (attr is PXSelectorAttribute)
				{
					PXView select = ((PXSelectorAttribute)attr).GetView(cache, ((PXSelectorAttribute)attr)._Select, !((PXSelectorAttribute)attr)._DirtyRead);
					return select.SelectMultiBound(new object[] { data });
				}
			}
			return null;
		}
		/// <summary>
		/// Gets the data item referenced by the selector
		/// </summary>
		/// <typeparam name="Field">Data field the selector is attached to</typeparam>
		/// <param name="cache">Cache</param>
		/// <param name="data">Data item to get parameter values</param>
		/// <returns>Foreign record</returns>
		public static object Select<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(typeof(Field).Name))
			{
				if (attr is PXSelectorAttribute)
				{
					PXView select = ((PXSelectorAttribute)attr).GetView(cache, ((PXSelectorAttribute)attr)._PrimarySelect, !((PXSelectorAttribute)attr)._DirtyRead);
					object[] pars = new object[((PXSelectorAttribute)attr)._ParsCount + 1];
					pars[pars.Length - 1] = cache.GetValue(data, ((PXSelectorAttribute)attr)._FieldOrdinal);
					return SelectSingleBound(select, new object[] { data }, pars);
				}
			}
			return null;
		}
		/// <summary>
		/// Gets the data item referenced by the selector
		/// </summary>
		/// <typeparam name="Field">Data field the selector is attached to</typeparam>
		/// <param name="cache">Cache</param>
		/// <param name="data">Data item to get parameter values</param>
		/// <param name="value">Value to search the referenced table for</param>
		/// <returns>Foreign record</returns>
		public static object Select<Field>(PXCache cache, object data, object value)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(typeof(Field).Name))
			{
				if (attr is PXSelectorAttribute)
				{
					PXView select = ((PXSelectorAttribute)attr).GetView(cache, ((PXSelectorAttribute)attr)._PrimarySelect, !((PXSelectorAttribute)attr)._DirtyRead);
					object[] pars = new object[((PXSelectorAttribute)attr)._ParsCount + 1];
					pars[pars.Length - 1] = value;
					return SelectSingleBound(select, new object[] { data }, pars);
				}
			}
			return null;
		}
		/// <summary>
		/// Overrides the list of columns for a particular data item
		/// </summary>
		/// <param name="cache">Cache</param>
		/// <param name="data">Data item to override</param>
		/// <param name="field">Field name the selector is attached to</param>
		/// <param name="fieldList">New field list</param>
		/// <param name="headerList">New header list</param>
		public static void SetColumns(PXCache cache, object data, string field, string[] fieldList, string[] headerList)
		{
			if (data == null)
			{
				cache.SetAltered(field, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, field))
			{
				if (attr is PXSelectorAttribute)
				{
					((PXSelectorAttribute)attr)._FieldList = fieldList;
					((PXSelectorAttribute)attr)._HeaderList = headerList;
				}
			}
		}
		/// <summary>
		/// Overrides the list of columns for a whole cache
		/// </summary>
		/// <param name="cache">Cache</param>
		/// <param name="field">Field name the selector is attached to</param>
		/// <param name="fieldList">New field list</param>
		/// <param name="headerList">New header list</param>
		public static void SetColumns(PXCache cache, string field, string[] fieldList, string[] headerList)
		{
			cache.SetAltered(field, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(field))
			{
				if (attr is PXSelectorAttribute)
				{
					((PXSelectorAttribute)attr)._FieldList = fieldList;
					((PXSelectorAttribute)attr)._HeaderList = headerList;
				}
			}
		}
		/// <summary>
		/// Overrides the list of columns for a particular data item
		/// </summary>
		/// <typeparam name="Field">Field the selector is attached to</typeparam>
		/// <param name="cache">Cache</param>
		/// <param name="data">Data item to override</param>
		/// <param name="fieldList">New field list</param>
		/// <param name="headerList">New header list</param>
		public static void SetColumns<Field>(PXCache cache, object data, Type[] fieldList, string[] headerList)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXSelectorAttribute)
				{
					((PXSelectorAttribute)attr)._FieldList = new string[fieldList.Length];
					Type[] tables = ((PXSelectorAttribute)attr)._Select.GetTables();
					for (int i = 0; i < fieldList.Length; i++)
					{
						if (!fieldList[i].IsNested || !typeof(IBqlField).IsAssignableFrom(fieldList[i]))
						{
							throw new PXArgumentException("fieldList", ErrorMessages.InvalidSelectorColumn);
						}
						if (tables.Length <= 1 || BqlCommand.GetItemType(fieldList[i]).IsAssignableFrom(tables[0]))
						{
							((PXSelectorAttribute)attr)._FieldList[i] = fieldList[i].Name;
						}
						else
						{
							((PXSelectorAttribute)attr)._FieldList[i] = BqlCommand.GetItemType(fieldList[i]).Name + "__" + fieldList[i].Name;
						}
					}
					((PXSelectorAttribute)attr)._HeaderList = headerList;
				}
			}
		}
		/// <summary>
		/// Overrides the list of columns for a whole cache
		/// </summary>
		/// <typeparam name="Field">Field the selector is attached to</typeparam>
		/// <param name="cache">Cache</param>
		/// <param name="fieldList">New field list</param>
		/// <param name="headerList">New header list</param>
		public static void SetColumns<Field>(PXCache cache, Type[] fieldList, string[] headerList)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXSelectorAttribute)
				{
					((PXSelectorAttribute)attr)._FieldList = new string[fieldList.Length];
					Type[] tables = ((PXSelectorAttribute)attr)._Select.GetTables();
					for (int i = 0; i < fieldList.Length; i++)
					{
						if (!fieldList[i].IsNested || !typeof(IBqlField).IsAssignableFrom(fieldList[i]))
						{
							throw new PXArgumentException("fieldList", ErrorMessages.InvalidSelectorColumn);
						}
						if (tables.Length <= 1 || BqlCommand.GetItemType(fieldList[i]).IsAssignableFrom(tables[0]))
						{
							((PXSelectorAttribute)attr)._FieldList[i] = fieldList[i].Name;
						}
						else
						{
							((PXSelectorAttribute)attr)._FieldList[i] = BqlCommand.GetItemType(fieldList[i]).Name + "__" + fieldList[i].Name;
						}
					}
					((PXSelectorAttribute)attr)._HeaderList = headerList;
				}
			}
		}

		public static void StoreCached<Field>(PXCache cache, object data, object item)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly<Field>())
			{
				if (attr is PXSelectorAttribute)
				{
					PXView select = ((PXSelectorAttribute)attr).GetView(cache, ((PXSelectorAttribute)attr)._PrimarySelect, !((PXSelectorAttribute)attr)._DirtyRead);
					object[] pars = new object[((PXSelectorAttribute)attr)._ParsCount + 1];
					pars[pars.Length - 1] = cache.GetValue(data, ((PXSelectorAttribute)attr)._FieldOrdinal);

					pars = select.PrepareParameters(new object[] { data }, pars);
					select.StoreCached(new PXCommandKey(pars), new List<object> { item });

					return;
				}
			}
		}
		/// <summary>
		/// Checks foreing keys and raises exception on violation. Works only if foreing key feild has PXSelectorAttribute
		/// </summary>
		/// <param name="Row">Current record</param>
		/// <param name="fieldType">BQL type of foreing key</param>
		/// <param name="searchType">Optional additional BQL statement to be checked</param>
        public static void CheckAndRaiseForeignKeyException(PXCache sender, object Row, Type fieldType, Type searchType = null)
        {
            string foreingTableName;
            string currentTableName;
            if (IsExists(sender, Row, fieldType, searchType, out currentTableName, out foreingTableName))
            {
                throw new PXException(ErrorMessages.ExtRefError, currentTableName, foreingTableName);
            }
        }
        private static bool IsExists(PXCache sender, object row, Type foreingFieldType, Type searchType, out string currentTableName, out string foreingTableName)
        {
            if (searchType != null && !typeof(IBqlSearch).IsAssignableFrom(searchType))
            {
                throw new PXArgumentException("selectType", ErrorMessages.ArgumentException);
            }
            Type currentTableType = row.GetType();
            Type cmd;
            Type tableType = BqlCommand.GetItemType(foreingFieldType);
            foreingTableName = GetTableName(tableType);
            currentTableName = GetTableName(currentTableType);
            Type currentFieldType = GetCurrentFieldType(sender, foreingFieldType);
            if (currentFieldType == null)
            {
                return false;
            }
            if (searchType == null)
            {
                cmd = BqlCommand.Compose(
                    typeof(Search<,>),
                    foreingFieldType,
                    typeof(Where<,>),
                    foreingFieldType,
                    typeof(Equal<>), typeof(Current<>), currentFieldType);
            }
            else
            {
                IBqlSearch Select = (IBqlSearch)Activator.CreateInstance(searchType);
                if (Select.GetType() != searchType)
                {
                    throw new PXArgumentException("selectType", ErrorMessages.ArgumentException);
                }
                List<Type> args = new List<Type> { searchType.GetGenericTypeDefinition() };
                args.AddRange(searchType.GetGenericArguments());
                int j = args.FindIndex(arg => typeof(IBqlWhere).IsAssignableFrom(arg));
                if (j == -1)
                {
                    throw new PXArgumentException("selectType", ErrorMessages.ArgumentException);
                }
                args[j] = BqlCommand.Compose(
                    typeof(Where2<,>),
                    typeof(Where<,>),
                    foreingFieldType,
                    typeof(Equal<>), typeof(Current<>), currentFieldType,
                    typeof(And<>),
                    args[j]);
                cmd = BqlCommand.Compose(args.ToArray());
            }
            PXView view = new PXView(sender.Graph, false, BqlCommand.CreateInstance(cmd));
            object refObject = view.SelectSingleBound(new object[] { row });
            return refObject != null;
        }
		private static string GetTableName(Type TableType)
		{
			if (TableType.IsDefined(typeof(PXCacheNameAttribute), true))
			{
				var attr = (PXCacheNameAttribute)(TableType.GetCustomAttributes(typeof(PXCacheNameAttribute), true)[0]);
				return attr.GetName();
			}
			return TableType.Name;
		}
		private static Type GetCurrentFieldType(PXCache sender, Type foreingFieldType)
		{
			foreach (var attr in sender.Graph.Caches[BqlCommand.GetItemType(foreingFieldType)].GetAttributesReadonly(foreingFieldType.Name))
			{
				if (attr is PXSelectorAttribute)
				{
					return ((IBqlSearch)((PXSelectorAttribute)attr)._Select).GetField();
				}
			}
			return null;
		}
		#endregion

		#region Implementation
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue == null || !ValidateValue)
			{
				return;
			}

			PXView view = GetView(sender, _PrimarySelect, !_DirtyRead);
			if (sender.Keys.Count == 0 || _FieldName != sender.Keys[sender.Keys.Count - 1])
			{
				object[] pars = new object[_ParsCount + 1];
				pars[pars.Length - 1] = e.NewValue;
				object item = null;
				try
				{
					item = SelectSingleBound(view, new object[] { e.Row }, pars);
				}
				catch (FormatException)
				{
				}
				if (item == null)
				{
					if (_SubstituteKey != null)
					{
						if (e.ExternalCall)
						{
							object incoming = sender.GetValuePending(e.Row, _FieldName);
							if (incoming != null)
							{
								e.NewValue = incoming;
							}
						}
						else if (object.Equals(e.NewValue, sender.GetValue(e.Row, _FieldOrdinal)))
						{
							try
							{
								object incoming = sender.GetValueExt(e.Row, _FieldName);
								if (incoming is PXFieldState)
								{
									e.NewValue = ((PXFieldState)incoming).Value;
								}
								if (incoming != null)
								{
									e.NewValue = incoming;
								}
							}
							catch
							{
							}
						}
					}
					throwNoItem(hasRestrictedAccess(sender, _PrimarySelect, e.Row), e.ExternalCall, e.NewValue);
				}
			}
		}

		protected string[] hasRestrictedAccess(PXCache sender, BqlCommand command, object row)
		{
			List<string> descr = new List<string>();
			foreach (IBqlParameter par in command.GetParameters())
			{
				if (par.MaskedType != null)
				{
					Type ft = par.GetReferencedType();
					if (ft.IsNested)
					{
						Type ct = ft.DeclaringType;
						PXCache cache = sender.Graph.Caches[ct];
						object val = null;
						bool currfound = false;
						if (row != null && (row.GetType() == ct || row.GetType().IsSubclassOf(ct)))
						{
							val = cache.GetValue(row, ft.Name);
							currfound = true;
						}
						if (!currfound && val == null && cache.Current != null)
						{
							val = cache.GetValue(cache.Current, ft.Name);
						}
						if (val == null && par.TryDefault)
						{
							if (cache.RaiseFieldDefaulting(ft.Name, null, out val))
							{
								cache.RaiseFieldUpdating(ft.Name, null, ref val);
							}
						}

						if (val != null)
						{
							descr.Add(string.Format("{0}={1}", char.ToUpper(ft.Name[0]) + ft.Name.Substring(1), val.ToString()));
						}
					}
				}
			}

			return (descr.Count > 0) ? descr.ToArray() : null;
		}

		protected void throwNoItem(string[] restricted, bool external, object value)
		{
			PXTrace.WriteInformation("Item {0} not found (restricted:{1},external:{2},value:{3})",
				this.FieldName, restricted != null ? string.Join(",", restricted) : false.ToString(), external, value);

			if (restricted == null)
			{
				if (external || value == null)
				{
					throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ElementDoesntExist, _FieldName));
				}
				else
				{
					throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ValueDoesntExist, _FieldName, value));
				}
			}
			else
			{
				if (external || value == null)
				{
					throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ElementDoesntExistOrNoRights, _FieldName));
				}
				else
				{
					throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ValueDoesntExistOrNoRights, _FieldName, value));
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			bool deleted = false;
			if (_SubstituteKey == null && e.ReturnValue != null && IsReadDeletedSupported &&
				(!_BqlTable.IsAssignableFrom(_BqlType) || sender.Keys.Count == 0 || String.Compare(sender.Keys[sender.Keys.Count - 1], _FieldName, StringComparison.OrdinalIgnoreCase) != 0))
			{
				object key = e.ReturnValue;
				Dictionary<object, KeyValuePair<object, bool>> dict = null;
				object item = null;
				if (_CacheGlobal)
				{
					dict = PXContext.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName);
					if (dict == null)
					{
						PXContext.SetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName, (dict = PXDatabase.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName, _BqlType)));
					}
					lock (((ICollection)dict).SyncRoot)
					{
						KeyValuePair<object, bool> pair;
						if (dict.TryGetValue(key, out pair))
						{
							item = pair.Key;
							deleted = pair.Value;
						}
					}
				}
				if (item == null)
				{
					PXView select = GetView(sender, _PrimarySelect, !_DirtyRead);
					object[] pars = new object[_ParsCount + 1];
					pars[pars.Length - 1] = key;
					item = SelectSingleBound(select, new object[] { e.Row }, pars);
					if (item == null)
					{
						using (PXReadDeletedScope rds = new PXReadDeletedScope())
						{
							item = SelectSingleBound(select, new object[] { e.Row }, pars);
							deleted = (item != null);
						}
					}
					if (dict != null && item != null && select.Cache.GetStatus(item) == PXEntryStatus.Notchanged && !PXDatabase.ReadDeleted && select.Cache.Keys.Count <= 1)
					{
						lock (((ICollection)dict).SyncRoot)
						{
							dict[key] = new KeyValuePair<object, bool>(item, deleted);
						}
					}
				}
			}
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				if (_HeaderList == null)
				{
					populateFields(sender, PXContext.GetSlot<bool>(selectorBypassInit));
				}
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, null, null, null, null, null, null, _FieldName, _DescriptionField != null ? _DescriptionField.Name : null, null, deleted ? ErrorMessages.ForeignRecordDeleted : null, deleted ? PXErrorLevel.Warning : PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, _ViewName, _FieldList, _HeaderList);
				((PXFieldState)e.ReturnState).ValueField = _SubstituteKey == null ? ValueField.Name : _SubstituteKey.Name;
				((PXFieldState)e.ReturnState).SelectorMode = SelectorMode;
			}
			else if (deleted)
			{
				e.ReturnState = sender.GetStateExt(e.Row, _FieldName);
			}
		}


		protected virtual bool IsReadDeletedSupported
		{
			get { return PXDatabase.IsReadDeletedSupported(_BqlType); }
		}

		public virtual void DescriptionFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, string alias)
		{
			bool deleted = false;
			if (e.Row != null)
			{
				object key = sender.GetValue(e.Row, _FieldOrdinal);
				if (key != null)
				{
					object item = null;
					Dictionary<object, KeyValuePair<object, bool>> dict = null;
					if (_CacheGlobal)
					{
						dict = PXContext.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName);
						if (dict == null)
						{
							PXContext.SetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName, (dict = PXDatabase.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName, _BqlType)));
						}
						lock (((ICollection)dict).SyncRoot)
						{
							KeyValuePair<object, bool> pair;
							if (dict.TryGetValue(key, out pair))
							{
								item = pair.Key;
								deleted = pair.Value;
							}
						}
					}
					if (item == null)
					{
						PXView select = GetView(sender, _UnconditionalSelect, !_DirtyRead);
						using (new PXReadBranchRestrictedScope())
						{
							item = SelectSingleBound(select, new object[] { e.Row }, key);
							if (item == null && IsReadDeletedSupported)
							{
								using (PXReadDeletedScope rds = new PXReadDeletedScope())
								{
									item = select.SelectSingleBound(new object[] { e.Row }, key);
									deleted = (item != null);
								}
							}
						}
						if (item != null && _CacheGlobal && select.Cache.GetStatus(item) == PXEntryStatus.Notchanged && !PXDatabase.ReadDeleted && select.Cache.Keys.Count <= 1)
						{
							lock (((ICollection)dict).SyncRoot)
							{
								dict[key] = new KeyValuePair<object, bool>(item, deleted);
							}
						}
					}
					if (item != null)
					{
						e.ReturnValue = sender.Graph.Caches[_Type].GetValue(item, _DescriptionField.Name);
					}
				}
			}
			if (e.Row == null || e.IsAltered || deleted)
			{
				int? length;
				string displayname = getDescriptionName(sender, out length);
				if (_UIFieldRef != null && _UIFieldRef.UIFieldAttribute == null)
				{
					_UIFieldRef.UIFieldAttribute = sender.GetAttributes(FieldName)
												   .OfType<PXUIFieldAttribute>()
												   .FirstOrDefault();
				}
				bool isVisible = _UIFieldRef != null && _UIFieldRef.UIFieldAttribute != null
					? _UIFieldRef.UIFieldAttribute.Visible
					: true;
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(string), false, true, null, null, length, null,
					alias//_FieldName + "_" + _Type.Name + "_" + _DescriptionField.Name
					, null, displayname, deleted ? ErrorMessages.ForeignRecordDeleted : null, deleted ? PXErrorLevel.Warning : PXErrorLevel.Undefined, false, isVisible, null, PXUIVisibility.Invisible, null, null, null);
			}
		}
		protected static Dictionary<Type, KeyValuePair<KeyValuePair<string, int?>, bool?>> _substitutekeys = new Dictionary<Type, KeyValuePair<KeyValuePair<string, int?>, bool?>>();
		protected string getSubstituteKeyMask(PXCache sender, out int? length, out bool? isUnicode)
		{
			length = null;
			isUnicode = null;
			string mask = null;
			if (_SubstituteKey != null)
			{
				KeyValuePair<KeyValuePair<string, int?>, bool?> pair;
				if (!_substitutekeys.TryGetValue(_SubstituteKey, out pair))
				{
					PXCache cache = sender.Graph.Caches[_Type];
					foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(_SubstituteKey.Name))
					{
						if (attr is PXDBStringAttribute)
						{
							length = ((PXDBStringAttribute)attr).Length;
							isUnicode = ((PXDBStringAttribute)attr).IsUnicode;
							mask = ((PXDBStringAttribute)attr).InputMask;
						}
						else if (attr is PXStringAttribute)
						{
							length = ((PXStringAttribute)attr).Length;
							isUnicode = ((PXStringAttribute)attr).IsUnicode;
							mask = ((PXStringAttribute)attr).InputMask;
						}
						if (mask != null && length != null)
						{
							break;
						}
					}
					if (cache.BqlTable.IsAssignableFrom(_Type))
					{
						lock (((ICollection)_substitutekeys).SyncRoot)
						{
							_substitutekeys[_SubstituteKey] = new KeyValuePair<KeyValuePair<string, int?>, bool?>(new KeyValuePair<string, int?>(mask, length), isUnicode);
						}
					}
				}
				else
				{
					mask = pair.Key.Key;
					length = pair.Key.Value;
					isUnicode = pair.Value;
				}
			}
			return mask;
		}
		protected string getDescriptionName(PXCache sender, out int? length)
		{
			length = null;
			string displayname = null;
			KeyValuePair<string, int?> pair;
			Dictionary<Type, KeyValuePair<string, int?>> descriptions = PXContext.GetSlot<Dictionary<Type, KeyValuePair<string, int?>>>("_DescriptionFieldFullName$" + System.Threading.Thread.CurrentThread.CurrentCulture.Name);
			if (descriptions == null)
			{
				PXContext.SetSlot<Dictionary<Type, KeyValuePair<string, int?>>>("_DescriptionFieldFullName$" + System.Threading.Thread.CurrentThread.CurrentCulture.Name, descriptions = PXDatabase.GetSlot<Dictionary<Type, KeyValuePair<string, int?>>>("_DescriptionFieldFullName$" + System.Threading.Thread.CurrentThread.CurrentCulture.Name, _BqlType));
			}
			if (!descriptions.TryGetValue(_DescriptionField, out pair))
			{
				PXCache cache = sender.Graph.Caches[_Type];
				foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(_DescriptionField.Name))
				{
					if (attr is PXUIFieldAttribute)
					{
						displayname = ((PXUIFieldAttribute)attr).DisplayName;
					}
					else if (attr is PXDBStringAttribute)
					{
						length = ((PXDBStringAttribute)attr).Length;
					}
					else if (attr is PXStringAttribute)
					{
						length = ((PXStringAttribute)attr).Length;
					}
					if (displayname != null && length != null)
					{
						break;
					}
				}
				if (displayname == null)
				{
					displayname = _DescriptionField.Name;
				}
				if (cache.BqlTable.IsAssignableFrom(_Type))
				{
					lock (((ICollection)descriptions).SyncRoot)
					{
						descriptions[_DescriptionField] = new KeyValuePair<string, int?>(displayname, length);
					}
				}
			}
			else
			{
				displayname = pair.Key;
				length = pair.Value;
			}
			if (_FieldList != null && _HeaderList != null && _FieldList.Length == _HeaderList.Length)
			{
				for (int i = 0; i < _FieldList.Length; i++)
				{
					if (_FieldList[i] == _DescriptionField.Name)
						return _HeaderList[i];
				}
			}
			return displayname;
		}
		public virtual void SubstituteKeyFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			bool deleted = false;
			if (e.ReturnValue != null)
			{
				Type field = ((IBqlSearch)_Select).GetField();
				object item = null;
				Dictionary<object, KeyValuePair<object, bool>> dict = null;
				if (_CacheGlobal)
				{
					dict = PXContext.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName);
					if (dict == null)
					{
						PXContext.SetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName, (dict = PXDatabase.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName, _BqlType)));
					}
					lock (((ICollection)dict).SyncRoot)
					{
						KeyValuePair<object, bool> pair;
						if (dict.TryGetValue(e.ReturnValue, out pair))
						{
							item = pair.Key;
							deleted = pair.Value;
						}
					}
				}
				if (item == null)
				{
					if (e.ReturnValue == null || e.ReturnValue.GetType() == sender.GetFieldType(_FieldName))
					{
						PXView substituteView = GetView(sender, _UnconditionalSelect, !_DirtyRead);
						using (new PXReadBranchRestrictedScope())
						{
							try
							{
								item = SelectSingleBound(substituteView, new object[] { e.Row }, e.ReturnValue);
								if (item == null && IsReadDeletedSupported)
								{
									using (PXReadDeletedScope rds = new PXReadDeletedScope())
									{
										item = SelectSingleBound(substituteView, new object[] { e.Row }, e.ReturnValue);
										deleted = (item != null);
									}
								}
							}
							catch (FormatException)
							{
							}
						}
						if (item != null)
						{
							if (_SubstituteKey != null)
							{
								object ret = substituteView.Cache.GetValue(item, _SubstituteKey.Name);
								if (dict != null && e.ReturnValue != null && substituteView.Cache.GetStatus(item) == PXEntryStatus.Notchanged && !PXDatabase.ReadDeleted && substituteView.Cache.Keys.Count <= 1)
								{
									lock (((ICollection)dict).SyncRoot)
									{
										dict[e.ReturnValue] = new KeyValuePair<object, bool>(item, deleted);
										if (ret != null)
										{
											dict[ret] = new KeyValuePair<object, bool>(item, deleted);
										}
									}
								}
								e.ReturnValue = ret;
							}
						}
					}
				}
				else
				{
					PXCache cache = sender.Graph.Caches[_Type];
					object p = e.ReturnValue;
					e.ReturnValue = cache.GetValue(item, _SubstituteKey.Name);
					if (e.ReturnValue == null)
					{
						PXView substituteView = GetView(sender, _UnconditionalSelect, !_DirtyRead);
						using (new PXReadBranchRestrictedScope())
						{
							item = SelectSingleBound(substituteView, new object[] { e.Row }, p);
							if (item == null && IsReadDeletedSupported)
							{
								using (PXReadDeletedScope rds = new PXReadDeletedScope())
								{
									item = SelectSingleBound(substituteView, new object[] { e.Row }, p);
									deleted = (item != null);
								}
							}
						}
						if (item != null)
						{
							e.ReturnValue = substituteView.Cache.GetValue(item, _SubstituteKey.Name);
						}
					}
				}
			}
			if (!e.IsAltered)
			{
				e.IsAltered = deleted || sender.HasAttributes(e.Row);
			}
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				int? length;
				bool? isUnicode;
				string mask = getSubstituteKeyMask(sender, out length, out isUnicode);
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, length, null, _FieldName, null, null, mask, null, null, null, null);
				if (deleted)
				{
					((PXFieldState)e.ReturnState).Error = ErrorMessages.ForeignRecordDeleted;
					((PXFieldState)e.ReturnState).ErrorLevel = PXErrorLevel.Warning;
					((PXFieldState)e.ReturnState).SelectorMode = SelectorMode;
				}
			}

			//if (e.ReturnState is PXFieldState )
			//{
			//    var returnState = (PXFieldState)e.ReturnState;
			//    returnState.ValueField = _SubstituteKey;
			//}
		}
		protected internal bool _BypassFieldVerifying;
		public virtual void SubstituteKeyFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (!e.Cancel && e.NewValue != null)
			{
				object item = null;
				Dictionary<object, KeyValuePair<object, bool>> dict = null;
				if (_CacheGlobal)
				{
					dict = PXContext.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName);
					if (dict == null)
					{
						PXContext.SetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName, (dict = PXDatabase.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName, _BqlType)));
					}
					lock (((ICollection)dict).SyncRoot)
					{
						KeyValuePair<object, bool> pair;
						if (dict.TryGetValue(e.NewValue, out pair))
						{
							if (pair.Value && !PXDatabase.ReadDeleted)
							{
								throw new PXForeignRecordDeletedException();
							}
							item = pair.Key;
						}
					}
				}
				if (item == null)
				{
					PXView select = GetView(sender, _NaturalSelect, !_DirtyRead);
					if (!_CacheGlobal)
					{
						object[] pars = new object[_ParsCount + 1];
						pars[pars.Length - 1] = e.NewValue;
						item = SelectSingleBound(select, new object[] { e.Row }, pars);
					}
					else
					{
						item = SelectSingleBound(select, new object[] { e.Row }, e.NewValue);
					}
					if (item != null)
					{
						object ret = select.Cache.GetValue(item, ((IBqlSearch)_Select).GetField().Name);
						if (dict != null && e.NewValue != null && select.Cache.GetStatus(item) == PXEntryStatus.Notchanged && !PXDatabase.ReadDeleted && select.Cache.Keys.Count <= 1)
						{
							lock (((ICollection)dict).SyncRoot)
							{
								dict[e.NewValue] = new KeyValuePair<object, bool>(item, false);
								if (ret != null)
								{
									dict[ret] = new KeyValuePair<object, bool>(item, false);
								}
							}
						}
						e.NewValue = ret;
					}
					else
					{
						using (new PXReadBranchRestrictedScope())
						{
							if (!_CacheGlobal)
							{
								object[] pars = new object[_ParsCount + 1];
								pars[pars.Length - 1] = e.NewValue;
								item = SelectSingleBound(select, new object[] { e.Row }, pars);
							}
							else
							{
								item = SelectSingleBound(select, new object[] { e.Row }, e.NewValue);
							}
							if (item == null && IsReadDeletedSupported)
							{
								using (PXReadDeletedScope rds = new PXReadDeletedScope())
								{
									if (!_CacheGlobal)
									{
										object[] pars = new object[_ParsCount + 1];
										pars[pars.Length - 1] = e.NewValue;
										item = SelectSingleBound(select, new object[] { e.Row }, pars);
									}
									else
									{
										item = SelectSingleBound(select, new object[] { e.Row }, e.NewValue);
									}
									if (item != null)
									{
										object ret = select.Cache.GetValue(item, ((IBqlSearch)_Select).GetField().Name);
										if (dict != null && select.Cache.GetStatus(item) == PXEntryStatus.Notchanged && select.Cache.Keys.Count <= 1)
										{
											lock (((ICollection)dict).SyncRoot)
											{
												dict[e.NewValue] = new KeyValuePair<object, bool>(item, true);
												if (ret != null)
												{
													dict[ret] = new KeyValuePair<object, bool>(item, true);
												}
											}
										}
										throw new PXForeignRecordDeletedException();
									}
								}
							}
							if (e.NewValue.GetType() == sender.GetFieldType(_FieldName))
							{
								PXView view = GetView(sender, _PrimarySelect, !_DirtyRead);
								object[] pars = new object[_ParsCount + 1];
								pars[pars.Length - 1] = e.NewValue;
								item = null;
								try
								{
									item = SelectSingleBound(view, new object[] { e.Row }, pars);
								}
								catch (FormatException)
								{
								}
								if (item != null)
								{
									return;
								}
							}
							_BypassFieldVerifying = true;
							try
							{
								object val = e.NewValue;
								sender.RaiseFieldVerifying(_FieldName, e.Row, ref val);

								if (val != null && val.GetType() == sender.GetFieldType(_FieldName))
								{
									e.NewValue = val;
									return;
								}
							}
							catch (Exception ex)
							{
								if (ex is PXSetPropertyException)
								{
									throw PXException.PreserveStack(ex);
								}
							}
							finally
							{
								_BypassFieldVerifying = false;
							}
							string[] restricted = (item != null) ? new string[] { true.ToString() } : hasRestrictedAccess(sender, _NaturalSelect, e.Row);
							throwNoItem(restricted, true, e.NewValue);
						}
					}
				}
				else
				{
					PXCache cache = sender.Graph.Caches[_Type];
					object p = e.NewValue;
					e.NewValue = cache.GetValue(item, ((IBqlSearch)_Select).GetField().Name);
					if (e.NewValue == null)
					{
						PXView select = GetView(sender, _NaturalSelect, !_DirtyRead);
						item = SelectSingleBound(select, new object[] { e.Row }, p);
						if (item != null)
						{
							e.NewValue = select.Cache.GetValue(item, ((IBqlSearch)_Select).GetField().Name);
						}
						else
						{
							throwNoItem(hasRestrictedAccess(sender, _NaturalSelect, e.Row), true, p);
						}
					}
				}
			}
		}
		public virtual void SubstituteKeyCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
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
						e.BqlTable = _BqlTable;
						e.FieldName = BqlCommand.SubSelect + _SubstituteKey.Name + " FROM " + BqlCommand.GetTableName(_Type, sender.Graph) + " " + _Type.Name + "Ext WHERE " + _Type.Name + "Ext." + ((IBqlSearch)_Select).GetField().Name + " = " + (e.Table == null ? _BqlTable.Name : e.Table.Name) + "." + (sender.BqlSelect == null ? ((PXDBFieldAttribute)attr).DatabaseFieldName : _FieldName) + ")";
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
		public virtual void DescriptionFieldCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
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
						e.BqlTable = _BqlTable;
						e.FieldName = BqlCommand.SubSelect + _DescriptionField.Name + " FROM " + BqlCommand.GetTableName(_Type, sender.Graph) + " " + _Type.Name + "Ext WHERE " + _Type.Name + "Ext." + ((IBqlSearch)_Select).GetField().Name + " = " + (e.Table == null ? _BqlTable.Name : e.Table.Name) + "." + (sender.BqlSelect == null ? ((PXDBFieldAttribute)attr).DatabaseFieldName : _FieldName) + ")";
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
		public virtual void ForeignTableRowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Completed)
			{
				ClearGlobalCache(_Type);
			}
		}
		public virtual void ReadDeletedFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue == null ||
				_BqlTable.IsAssignableFrom(_BqlType) && sender.Keys.Count > 0 && String.Compare(sender.Keys[sender.Keys.Count - 1], _FieldName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return;
			}
			Dictionary<object, KeyValuePair<object, bool>> dict = null;
			object key = e.NewValue;
			if (_CacheGlobal)
			{
				dict = PXContext.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName);
				if (dict == null)
				{
					PXContext.SetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName, (dict = PXDatabase.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName, _BqlType)));
				}
				lock (((ICollection)dict).SyncRoot)
				{
					KeyValuePair<object, bool> pair;
					if (dict.TryGetValue(key, out pair))
					{
						if (pair.Value && !PXDatabase.ReadDeleted)
						{
							throw new PXForeignRecordDeletedException();
						}
						else
						{
							return;
						}
					}
				}
			}
			PXView select = GetView(sender, _PrimarySelect, !_DirtyRead);
			object[] pars = new object[_ParsCount + 1];
			pars[pars.Length - 1] = key;
			bool deleted = false;
			object item = SelectSingleBound(select, new object[] { e.Row }, pars);
			if (item == null)
			{
				using (PXReadDeletedScope rds = new PXReadDeletedScope())
				{
					item = SelectSingleBound(select, new object[] { e.Row }, pars);
					deleted = true;
				}
			}
			if (item != null)
			{
				if (select.Cache.GetStatus(item) == PXEntryStatus.Notchanged)
				{
					if (dict != null && !PXDatabase.ReadDeleted && select.Cache.Keys.Count <= 1)
					{
						lock (((ICollection)dict).SyncRoot)
						{
							dict[key] = new KeyValuePair<object, bool>(item, deleted);
						}
					}
					if (deleted)
					{
						throw new PXForeignRecordDeletedException();
					}
				}
			}
		}
		#endregion

		#region Initialization
		protected static Dictionary<Type, List<KeyValuePair<string, Type>>> _SelectorFields = new Dictionary<Type, List<KeyValuePair<string, Type>>>();
		protected internal override void SetBqlTable(Type bqlTable)
		{
			base.SetBqlTable(bqlTable);
			lock (((ICollection)_SelectorFields).SyncRoot)
			{
				List<KeyValuePair<string, Type>> list;
				if (!_SelectorFields.TryGetValue(bqlTable, out list))
				{
					_SelectorFields[bqlTable] = list = new List<KeyValuePair<string, Type>>();
				}
				bool found = false;
				foreach (KeyValuePair<string, Type> pair in list)
				{
					if (pair.Key == base.FieldName)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					Type field = ((IBqlSearch)_Select).GetField();
					Type table = BqlCommand.GetItemType(field);
					if (table == null || !bqlTable.IsAssignableFrom(table) || !String.Equals(field.Name, base.FieldName, StringComparison.OrdinalIgnoreCase))
					{
						list.Add(new KeyValuePair<string, Type>(base.FieldName, ((IBqlSearch)_Select).GetField()));
					}
				}
			}
		}
		public static List<KeyValuePair<string, Type>> GetSelectorFields(Type table)
		{
			if (PX.Api.ServiceManager.EnsureCachesInstatiated(true))
			{
				List<KeyValuePair<string, Type>> list;
				if (_SelectorFields.TryGetValue(table, out list))
				{
					return list;
				}
			}
			return new List<KeyValuePair<string, Type>>();
		}

		protected internal const string selectorBypassInit = "selectorBypassInit";
		protected static object _tlock = new object();
		protected static Dictionary<string, string[]> _fields = new Dictionary<string, string[]>();
		protected static Dictionary<string, string[]> _headers = new Dictionary<string, string[]>();
		protected static object _flock = new object();
		protected static Dictionary<Type, string> _descriptions = new Dictionary<Type, string>();

		protected void populateFields(PXCache sender, bool bypassInit)
		{
			string key;
			if (_FieldList == null)
			{
				key = _Type.FullName;
			}
			else
			{
				key = sender.GetItemType().FullName + "$" + _FieldName;
				if (sender.IsGraphSpecificField(_FieldName))
				{
					key = key + "$" + sender.Graph.GetType().FullName;
				}
			}
			string culture = key + "@" + System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
			string[] headerList = null;
			lock (_flock)
			{
				if (_FieldList == null)
				{
					_fields.TryGetValue(key, out _FieldList);
				}
				if (_FieldList != null)
				{
					_headers.TryGetValue(culture + "$" + _FieldList.Length.ToString(CultureInfo.InvariantCulture), out headerList);
				}
			}
			if (_FieldList == null || headerList == null)
			{
				if (_FieldList != null && _HeaderList != null)
				{
					for (int i = 0; i < _HeaderList.Length; i++)
					{
						uint msgnum;
						string msgprefix;
						_HeaderList[i] = PXMessages.Localize(_HeaderList[i], out msgnum, out msgprefix);
					}
					lock (_flock)
					{
						_fields[key] = _FieldList;
						_headers[culture + "$" + _FieldList.Length.ToString(CultureInfo.InvariantCulture)] = _HeaderList;
					}
				}
				else if (!bypassInit)
				{
					_HeaderList = new string[0];
					List<string> fields = new List<string>();
					List<string> headers = new List<string>();
					PXCache cache = sender.GetItemType() == _Type || sender.GetItemType().IsSubclassOf(_Type) && !Attribute.IsDefined(sender.GetItemType(), typeof(PXBreakInheritanceAttribute), false) ? sender : sender.Graph.Caches[_Type];
					PXContext.SetSlot<bool>(selectorBypassInit, true);
					try
					{
						if (_FieldList == null)
						{
							foreach (string name in cache.Fields)
							{
								PXFieldState st = cache.GetStateExt(null, name) as PXFieldState;
								if (st != null &&
									((st.Visibility & PXUIVisibility.SelectorVisible) == PXUIVisibility.SelectorVisible ||
									(st.Visibility & PXUIVisibility.Dynamic) == PXUIVisibility.Dynamic))
								{
									fields.Add(st.Name);
									headers.Add(st.DisplayName);
								}
							}
						}
						else
						{
							for (int i = 0; i < _FieldList.Length; i++)
							{
								bool found = false;
								{
									PXFieldState st = cache.GetStateExt(null, _FieldList[i]) as PXFieldState;
									if (st != null)
									{
										fields.Add(_FieldList[i]);
										headers.Add(st.DisplayName);
										found = true;
									}
								}
								int idx;
								if (!found)
								{
									if ((idx = _FieldList[i].IndexOf("__")) > 0)
									{
										if (idx + 2 < _FieldList[i].Length)
										{
											string tname = _FieldList[i].Substring(0, idx);
											foreach (Type table in _Select.GetTables())
											{
												if (table.Name == tname)
												{
													string fname = _FieldList[i].Substring(idx + 2, _FieldList[i].Length - idx - 2);
													PXCache tcache = sender.Graph.Caches[table];
													{
														PXFieldState st = tcache.GetStateExt(null, fname) as PXFieldState;
														if (st != null)
														{
															fields.Add(_FieldList[i]);
															headers.Add(st.DisplayName);
														}
													}
													break;
												}
											}
										}
									}
									else if ((idx = _FieldList[i].IndexOf('_')) > 0)
									{
										string fname = _FieldList[i].Substring(0, idx);
										foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(fname))
										{
											if (attr is PXSelectorAttribute)
											{
												fields.Add(attr.FieldName);
												int? length;
												headers.Add(((PXSelectorAttribute)attr).getDescriptionName(sender, out length));
												break;
											}
										}
									}
								}
							}
						}
					}
					finally
					{
						PXContext.SetSlot<bool>(selectorBypassInit, false);
					}
					_FieldList = fields.ToArray();
					_HeaderList = headers.ToArray();
					lock (_flock)
					{
						_fields[key] = _FieldList;
						_headers[culture + "$" + _FieldList.Length.ToString(CultureInfo.InvariantCulture)] = _HeaderList;
					}
				}
			}
			else
			{
				_HeaderList = headerList;
			}
		}

		public void CreateView(PXCache sender)
		{
			PXView view;
			if (sender.Graph.Views.TryGetValue(_ViewName, out view))
			{
				if (view.BqlSelect.GetType() != _Select.GetType())
				{
					if (!_IsOwnView)
					{
						_ViewName = Guid.NewGuid().ToString();
					}
					view = null;
				}
			}
			if (view == null)
			{
				if (_ViewHandler != null)
				{
					if (_CacheGlobal) view = new adjustableViewGlobal(sender.Graph, true, _Select, _ViewHandler);
					else view = new PXAdjustableView(sender.Graph, true, _Select, _ViewHandler);
				}
				else
				{
					view = _CacheGlobal ? new viewGlobal(sender.Graph, true, _Select) : new PXView(sender.Graph, true, _Select);
				}
				sender.Graph.Views[_ViewName] = view;
				_IsOwnView = true;
				if (_DirtyRead)
				{
					view.IsReadOnly = false;
				}
				if (_Filterable)
				{
					sender.Graph.Views[_ViewName + PXFilterableAttribute.FilterHeaderName] = new PXFilterView(sender.Graph, "SELECTOR", "_" + _Type.Name + "_");
					sender.Graph.Views[_ViewName + PXFilterableAttribute.FilterRowName] = new PXView(sender.Graph, false, new Select<FilterRow, Where<FilterRow.filterID, Equal<Required<FilterRow.filterID>>>>());
				}
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			_UIFieldRef = new UIFieldRef();
			_CacheType = sender.GetItemType();
			if (_CacheGlobal && (_CacheType == _Type || _CacheType.IsSubclassOf(_Type)))
			{
				sender.RowPersisted += ForeignTableRowPersisted;
				Dictionary<object, KeyValuePair<object, bool>> dict = PXContext.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName);
				if (dict == null)
				{
					PXContext.SetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName, (dict = PXDatabase.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName, _BqlType)));
				}
			}
			populateFields(sender, true);
			CreateView(sender);
			_ViewCreated = true;

			if (!(_CacheType == _Type || _CacheType.IsSubclassOf(_Type)) || !String.Equals(_FieldName, ((IBqlSearch)_Select).GetField().Name, StringComparison.OrdinalIgnoreCase))
			{
				EmitColumnForDescriptionField(sender);
			}
			else
			{
				SelectorMode |= PXSelectorMode.NoAutocomplete;
			}

			if (_SubstituteKey != null)
			{
				string name = _FieldName.ToLower();
				sender.FieldSelectingEvents[name] += SubstituteKeyFieldSelecting;
				sender.FieldUpdatingEvents[name] += SubstituteKeyFieldUpdating;
				if (String.Compare(_SubstituteKey.Name, _FieldName, StringComparison.OrdinalIgnoreCase) != 0)
				{
					sender.CommandPreparingEvents[name] += SubstituteKeyCommandPreparing;
				}
			}
			else if (IsReadDeletedSupported)
			{
				sender.FieldVerifyingEvents[_FieldName.ToLower()] += ReadDeletedFieldVerifying;
				_CacheGlobal = true;
			}
		}

		protected void EmitColumnForDescriptionField(PXCache sender)
		{
			if (_DescriptionField == null) return;
			if (true)
			{
				string alias = _FieldName + "_" + _Type.Name + "_" + _DescriptionField.Name;
				if (!sender.Fields.Contains(alias))
				{
					sender.Fields.Add(alias);
					sender.FieldSelectingEvents[alias.ToLower()] +=
						delegate(PXCache s, PXFieldSelectingEventArgs e) { DescriptionFieldSelecting(s, e, alias); };
					sender.CommandPreparingEvents[alias.ToLower()] += DescriptionFieldCommandPreparing;
				}
			}

			if (true)
			{
				string alias = _FieldName + "_description";
				if (!sender.Fields.Contains(alias))
				{
					sender.Fields.Add(alias);
					sender.FieldSelectingEvents[alias.ToLower()] += //DescriptionFieldSelecting;
							delegate(PXCache s, PXFieldSelectingEventArgs e) { DescriptionFieldSelecting(s, e, alias); };
					sender.CommandPreparingEvents[alias.ToLower()] += DescriptionFieldCommandPreparing;

				}
			}

		}

		#endregion

		private sealed class adjustableViewGlobal : viewGlobal, IPXAdjustableView
		{
			public adjustableViewGlobal(PXGraph graph, bool isReadOnly, BqlCommand select, Delegate handler)
				: base(graph, isReadOnly, select, handler)
			{
			}
		}

		private class viewGlobal : PXView
		{
			public viewGlobal(PXGraph graph, bool isReadOnly, BqlCommand select)
				: base(graph, isReadOnly, select)
			{
			}
			public viewGlobal(PXGraph graph, bool isReadOnly, BqlCommand select, Delegate handler)
				: base(graph, isReadOnly, select, handler)
			{
			}

			public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
			{
				List<object> ret = null;
				object key = null;
				Dictionary<object, KeyValuePair<object, bool>> dict = null;
				if (startRow == 0 && maximumRows == 1 && searches != null && searches.Length == 1 && searches[0] != null)
				{
					key = searches[0];
					KeyValuePair<object, bool> item;
					dict = PXContext.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(Cache.GetItemType().FullName);
					if (dict == null)
					{
						PXContext.SetSlot<Dictionary<object, KeyValuePair<object, bool>>>(Cache.GetItemType().FullName, (dict = PXDatabase.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(Cache.GetItemType().FullName, Cache.BqlTable)));
					}
					lock (((ICollection)dict).SyncRoot)
					{
						if (dict.TryGetValue(searches[0], out item))
						{
							ret = new List<object>();
							ret.Add(item.Key);
						}
					}
					if (ret == null)
					{
						ret = base.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
						bool deleted = false;
						if ((ret == null || ret.Count == 0) && PXDatabase.IsReadDeletedSupported(Cache.BqlTable))
						{
							using (PXReadDeletedScope rds = new PXReadDeletedScope())
							{
								ret = base.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
							}
							deleted = true;
						}
						if (ret != null && ret.Count == 1 && !PXDatabase.ReadDeleted && base.Cache.Keys.Count <= 1)
						{
							lock (((ICollection)dict).SyncRoot)
							{
								if (ret[0] is PXResult)
								{
									dict[key] = new KeyValuePair<object, bool>(((PXResult)ret[0])[0], deleted);
								}
								else
								{
									dict[key] = new KeyValuePair<object, bool>(ret[0], deleted);
								}
							}
						}
					}
				}
				if (ret == null)
				{
					ret = base.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
				}
				return ret;
			}
		}
	}
	#endregion

	#region PXCustomSelectorAttribute
	/// <summary>
	/// Base class for custom selectors. 
	/// Derive from this class and override the GetRecords() method.
	/// </summary>
	public class PXCustomSelectorAttribute : PXSelectorAttribute
	{
		readonly long hashCode;
		protected PXGraph _Graph;


		#region Ctor
		/// <summary>
		/// Creates a selector
		/// </summary>
		/// <param name="type">Referenced table. Should be either IBqlField or IBqlSearch</param>
		public PXCustomSelectorAttribute(Type type)
			: base(type)
		{
			this.hashCode = DateTime.Now.Ticks;
		}

		/// <summary>
		/// Creates a selector overriding the columns
		/// </summary>
		/// <param name="type">Referenced table. Should be either IBqlField or IBqlSearch</param>
		/// <param name="fieldList">Fields to display in the selector</param>
		/// <param name="headerList">Headers of the selector columns</param>
		public PXCustomSelectorAttribute(Type type, params Type[] fieldList)
			: base(type, fieldList)
		{
			this.hashCode = DateTime.Now.Ticks;
		}

		protected sealed class FilteredView : PXView
		{
			private PXView _OuterView;
			private PXView _TemplateView;
			public FilteredView(PXView outerView, PXView templateView)
				: base(templateView.Graph, templateView.IsReadOnly, templateView.BqlSelect)
			{
				_OuterView = outerView;
				_TemplateView = templateView;
			}
			public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
			{
				if (parameters != null && parameters.Length > 0)
				{
					string[] names = _TemplateView.GetParameterNames();
					int idx;
					if (names.Length > 0 && !String.IsNullOrEmpty(names[names.Length - 1]) && (idx = names[names.Length - 1].LastIndexOf('.')) != -1 && idx + 1 < names[names.Length - 1].Length)
					{
						string field = names[names.Length - 1].Substring(idx + 1);
						object val = parameters[parameters.Length - 1];
						try
						{
							Cache.RaiseFieldSelecting(field, currents != null && currents.Length > 0 ? currents[0] : null, ref val, false);
							val = PXFieldState.UnwrapValue(val);
						}
						catch
						{
						}
						if (val == null)
						{
							val = parameters[parameters.Length - 1];
						}
						PXFilterRow filter = new PXFilterRow(field, PXCondition.EQ, val);
						if (filters == null || filters.Length == 0)
						{
							filters = new PXFilterRow[] { filter };
						}
						else
						{
							filters = filters.Append(filter).ToArray();
							if (filters.Length > 2)
							{
								filters[0].OpenBrackets++;
								filters[filters.Length - 2].CloseBrackets++;
							}
							filters[filters.Length - 2].OrOperator = false;
						}
						Array.Resize(ref parameters, parameters.Length - 1);
					}
				}
				return _OuterView.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
			}
		}
		protected override PXView GetView(PXCache cache, BqlCommand select, bool isReadOnly)
		{
			return new FilteredView(cache.Graph.Views[_ViewName], base.GetView(cache, select, isReadOnly));
		}
		#endregion

		#region Implementation
		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!ValidateValue)
				return;

			if (e.NewValue == null)
			{
				return;
			}

			if (sender.Keys.Count == 0 || _FieldName != sender.Keys[sender.Keys.Count - 1])
			{
				List<object> records = sender.Graph.Views[_ViewName].SelectMultiBound(new object[] { e.Row });
				PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(((IBqlSearch)_Select).GetField())];
				foreach (object rec in records)
				{
					object item = (rec is PXResult) ? ((PXResult)rec)[0] : rec;
					object val = cache.GetValue(item, ((IBqlSearch)_Select).GetField().Name);
					if (Equals(val, e.NewValue))
						return;
				}
				throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ElementDoesntExist, _FieldName));
			}
		}

		private string GetHash()
		{
			return this.GetType().FullName + this.hashCode.ToString();
		}
		#endregion

		#region Initialization
		private delegate PXView CreateViewDelegate(PXCustomSelectorAttribute attr, PXGraph graph, bool IsReadOnly, BqlCommand select);
		// Dictionary of createView delegates for each user-defined class which are derived from PXCustomSelector, dictionary key - is a type of derived class
		private static Dictionary<string, CreateViewDelegate> createView = new Dictionary<string, CreateViewDelegate>();
		private static object _vlock = new object();


		[System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]

		private static CreateViewDelegate CreateDelegate(PXCustomSelectorAttribute attr)
		{
			DynamicMethod dm;
			if (!PXGraph.IsRestricted)
			{
				dm = new DynamicMethod("InitView", typeof(PXView), new Type[] { typeof(PXCustomSelectorAttribute), typeof(PXGraph), typeof(bool), typeof(BqlCommand) }, typeof(PXCustomSelectorAttribute), true);
			}
			else
			{
				dm = new DynamicMethod("InitView", typeof(PXView), new Type[] { typeof(PXCustomSelectorAttribute), typeof(PXGraph), typeof(bool), typeof(BqlCommand) }, true);
			}
			MethodInfo del = attr.GetType().GetMethod("GetRecords", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (del == null)
				return null;

			Type tdel = null;
			ParameterInfo[] pars = del.GetParameters();
			if (typeof(IEnumerable).IsAssignableFrom(del.ReturnType))
			{
				switch (pars.Length)
				{
					case 0:
						tdel = typeof(PXSelectDelegate);
						break;
					case 1:
						tdel = typeof(PXSelectDelegate<>).MakeGenericType(
							pars[0].ParameterType);
						break;
					case 2:
						tdel = typeof(PXSelectDelegate<,>).MakeGenericType(
							pars[0].ParameterType,
							pars[1].ParameterType);
						break;
					case 3:
						tdel = typeof(PXSelectDelegate<,,>).MakeGenericType(
							pars[0].ParameterType,
							pars[1].ParameterType,
							pars[2].ParameterType);
						break;
					case 4:
						tdel = typeof(PXSelectDelegate<,,,>).MakeGenericType(
							pars[0].ParameterType,
							pars[1].ParameterType,
							pars[2].ParameterType,
							pars[3].ParameterType);
						break;
					case 5:
						tdel = typeof(PXSelectDelegate<,,,,>).MakeGenericType(
							pars[0].ParameterType,
							pars[1].ParameterType,
							pars[2].ParameterType,
							pars[3].ParameterType,
							pars[4].ParameterType);
						break;
					case 6:
						tdel = typeof(PXSelectDelegate<,,,,,>).MakeGenericType(
							pars[0].ParameterType,
							pars[1].ParameterType,
							pars[2].ParameterType,
							pars[3].ParameterType,
							pars[4].ParameterType,
							pars[5].ParameterType);
						break;
					case 7:
						tdel = typeof(PXSelectDelegate<,,,,,,>).MakeGenericType(
							pars[0].ParameterType,
							pars[1].ParameterType,
							pars[2].ParameterType,
							pars[3].ParameterType,
							pars[4].ParameterType,
							pars[5].ParameterType,
							pars[6].ParameterType);
						break;
					case 8:
						tdel = typeof(PXSelectDelegate<,,,,,,,>).MakeGenericType(
							pars[0].ParameterType,
							pars[1].ParameterType,
							pars[2].ParameterType,
							pars[3].ParameterType,
							pars[4].ParameterType,
							pars[5].ParameterType,
							pars[6].ParameterType,
							pars[7].ParameterType);
						break;
					case 9:
						tdel = typeof(PXSelectDelegate<,,,,,,,,>).MakeGenericType(
							pars[0].ParameterType,
							pars[1].ParameterType,
							pars[2].ParameterType,
							pars[3].ParameterType,
							pars[4].ParameterType,
							pars[5].ParameterType,
							pars[6].ParameterType,
							pars[7].ParameterType,
							pars[8].ParameterType);
						break;
					case 10:
						tdel = typeof(PXSelectDelegate<,,,,,,,,,>).MakeGenericType(
							pars[0].ParameterType,
							pars[1].ParameterType,
							pars[2].ParameterType,
							pars[3].ParameterType,
							pars[4].ParameterType,
							pars[5].ParameterType,
							pars[6].ParameterType,
							pars[7].ParameterType,
							pars[8].ParameterType,
							pars[9].ParameterType);
						break;
					case 11:
						tdel = typeof(PXSelectDelegate<,,,,,,,,,,>).MakeGenericType(
							pars[0].ParameterType,
							pars[1].ParameterType,
							pars[2].ParameterType,
							pars[3].ParameterType,
							pars[4].ParameterType,
							pars[5].ParameterType,
							pars[6].ParameterType,
							pars[7].ParameterType,
							pars[8].ParameterType,
							pars[9].ParameterType,
							pars[10].ParameterType);
						break;
				}
			}
			if (tdel == null)
				return null;

			ILGenerator ilgen = dm.GetILGenerator();
			LocalBuilder result = ilgen.DeclareLocal(typeof(PXView));
			ilgen.Emit(OpCodes.Nop);
			ilgen.Emit(OpCodes.Ldarg_1);
			ilgen.Emit(OpCodes.Ldarg_2);
			ilgen.Emit(OpCodes.Ldarg_3);
			ilgen.Emit(OpCodes.Ldarg_0);
			ilgen.Emit(OpCodes.Castclass, attr.GetType());
			ilgen.Emit(OpCodes.Ldftn, del);
			ilgen.Emit(OpCodes.Newobj, tdel.GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
			ilgen.Emit(OpCodes.Newobj, typeof(PXView).GetConstructor(new Type[] { typeof(PXGraph), typeof(bool), typeof(BqlCommand), typeof(Delegate) }));
			ilgen.Emit(OpCodes.Stloc, result.LocalIndex);
			ilgen.Emit(OpCodes.Ldloc, result.LocalIndex);
			ilgen.Emit(OpCodes.Ret);

			return (CreateViewDelegate)dm.CreateDelegate(typeof(CreateViewDelegate));
		}

		protected bool writeLog = false;
		public override void CacheAttached(PXCache sender)
		{
			_CacheType = sender.GetItemType();
			_Graph = sender.Graph;

			lock (_vlock)
			{
				if (!createView.ContainsKey(this.GetHash()))
					createView.Add(this.GetHash(), CreateDelegate(this));
			}

			populateFields(sender, true);
			PXView view;
			if (!sender.Graph.Views.TryGetValue(_ViewName, out view))
			{
				//if(createView != null)
				view = createView[this.GetHash()](this, sender.Graph, !_DirtyRead, _Select);
				sender.Graph.Views[_ViewName] = view;
				//else
				//    sender.Graph.Views[_ViewName] = new PXView(sender.Graph, !_DirtyRead, _Select);

				if (_Filterable)
				{
					sender.Graph.Views[_ViewName + PXFilterableAttribute.FilterHeaderName] = new PXFilterView(sender.Graph, "SELECTOR", _Type.Name);
					sender.Graph.Views[_ViewName + PXFilterableAttribute.FilterRowName] = new PXView(sender.Graph, false, new Select<FilterRow, Where<FilterRow.filterID, Equal<Required<FilterRow.filterID>>>>());
				}
			}
			else if (view.BqlTarget != this.GetType())
			{
				_ViewName = Guid.NewGuid().ToString();
				view = createView[this.GetHash()](this, sender.Graph, !_DirtyRead, _Select);
				sender.Graph.Views[_ViewName] = view;

				if (_Filterable)
				{
					sender.Graph.Views[_ViewName + PXFilterableAttribute.FilterHeaderName] = new PXFilterView(sender.Graph, "SELECTOR", _Type.Name);
					sender.Graph.Views[_ViewName + PXFilterableAttribute.FilterRowName] = new PXView(sender.Graph, false, new Select<FilterRow, Where<FilterRow.filterID, Equal<Required<FilterRow.filterID>>>>());
				}
			}

			if (!(_CacheType == _Type || _CacheType.IsSubclassOf(_Type)))
			{
				EmitColumnForDescriptionField(sender);
			}

			//if (_DescriptionField != null)
			//{
			//    string alias = _FieldName + "_" + _Type.Name + "_" + _DescriptionField.Name;
			//    if (!sender.Fields.Contains(alias))
			//    {
			//        sender.Fields.Add(alias);
			//        sender.FieldSelectingEvents[alias.ToLower()] += DescriptionFieldSelecting;
			//    }
			//}
			if (_SubstituteKey != null)
			{
				string name = _FieldName.ToLower();
				sender.FieldSelectingEvents[name] += SubstituteKeyFieldSelecting;
				sender.FieldUpdatingEvents[name] += SubstituteKeyFieldUpdating;
			}
		}
		#endregion
	}
	#endregion

	public class UIFieldRef
	{
		public PXUIFieldAttribute UIFieldAttribute;
	}

	#region PXExtensionAttribute
	/// <summary>
	/// Not used.
	/// </summary>
	public class PXExtensionAttribute : PXSelectorAttribute
	{
		#region Ctor
		/// <summary>
		/// Creates an extension
		/// </summary>
		/// <param name="type">Referenced table. Should be either IBqlField or IBqlSearch</param>
		public PXExtensionAttribute(Type type)
			: base(type)
		{
		}
		#endregion
	}
	#endregion
	#region PXVirtualSelectorAttribute
	/// <summary>
	/// Suppress GUI selector, used in formula.
	/// </summary>
	public class PXVirtualSelectorAttribute : PXSelectorAttribute
	{
		#region Ctor
		/// <summary>
		/// Creates an virtual selector
		/// </summary>
		/// <param name="type">Referenced table. Should be either IBqlField or IBqlSearch</param>
		public PXVirtualSelectorAttribute(Type type)
			: base(type)
		{
			ValidateValue = false;
		}
		#endregion

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);
			PXFieldState state = e.ReturnState as PXFieldState;
			if (state != null && state.ViewName != _ViewName)
				state.ViewName = null;
		}
	}
	#endregion

	#region PXParentAttribute
	/// <summary>
	/// Provides reference to the parent record.
	/// Use this attribute to create parent/child relationship between tables.
	/// Attribute is placed on any field of child table.
	/// Attribute contains bql statement to select parent record for current child.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
	public class PXParentAttribute : PXEventSubscriberAttribute
	{
		#region State
		protected Type _ChildType;
		protected Type _ParentType;
		protected BqlCommand _SelectParent;
		protected BqlCommand _SelectChildren;
		protected bool _UseCurrent;
		protected bool _LeaveChildren;
		protected bool _ParentCreate;

		public virtual bool ParentCreate
		{
			get
			{
				return _ParentCreate;
			}
			set
			{
				_ParentCreate = value;
			}
		}

		public virtual bool LeaveChildren
		{
			get
			{
				return _LeaveChildren;
			}
			set
			{
				_LeaveChildren = value;
			}
		}

		public virtual Type ParentType
		{
			get
			{
				return this._ParentType;
			}
		}

		protected Dictionary<object, object> _Currents = null;
		protected Dictionary<object, object> _Pendings = null;

		public static bool GetParentCreate(PXCache cache, Type ParentType)
		{
			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					if (((PXParentAttribute)attr)._ParentType == ParentType)
					{
						parents.Insert(0, attr);
						break;
					}
					else if (ParentType.IsSubclassOf(((PXParentAttribute)attr)._ParentType))
					{
						parents.Add(attr);
					}
				}
			}

			if (parents.Count > 0)
			{
				return ((PXParentAttribute)parents[0])._ParentCreate;
			}
			return false;
		}

		public static void CreateParent(PXCache cache, object row, Type ParentType)
		{
			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					if (((PXParentAttribute)attr)._ParentType == ParentType)
					{
						parents.Insert(0, attr);
						break;
					}
					else if (ParentType.IsSubclassOf(((PXParentAttribute)attr)._ParentType))
					{
						parents.Add(attr);
					}
				}
			}

			if (parents.Count > 0 && ((PXParentAttribute)parents[0])._ParentCreate == true)
			{
				object parent;

				PXParentAttribute attr = (PXParentAttribute)parents[0];

				PXView parentView = attr.GetParentSelect(cache);
				BqlCommand selectParent = parentView.BqlSelect;

				IBqlParameter[] pars = selectParent.GetParameters();
				Type[] refs = selectParent.GetReferencedFields(false);

				PXCache parentcache = cache.Graph.Caches[ParentType];
				parent = parentcache.CreateInstance();

				object val;

				for (int i = 0; i < refs.Length; i++)
				{
					Type partype = pars[i].GetReferencedType();
					if ((val = cache.GetValue(row, partype.Name)) == null)
					{
						return;
					}
					parentcache.SetValue(parent, refs[i].Name, val);
				}

				List<object> vals = new List<object>();

				for (int i = 0; i < pars.Length; i++)
				{
					Type partype = pars[i].GetReferencedType();

					if ((val = cache.GetValue(row, partype.Name)) == null)
					{
						return;
					}
					vals.Add(val);
				}

				object value = null;
				bool? ret = null;

				selectParent.Verify(parentcache, parent, vals, ref ret, ref value);

				if (ret == true)
				{
					parentcache.Insert(parent);
					parentView.Clear();
				}
				return;
			}
		}

		public static void SetLeaveChildren<Field>(PXCache cache, object data, bool isLeaveChildren)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXParentAttribute)
				{
					((PXParentAttribute)attr).LeaveChildren = isLeaveChildren;
					if (isLeaveChildren == false)
					{
						cache.Graph.RowDeleted.AddHandler(((PXParentAttribute)attr)._ParentType, ((PXParentAttribute)attr).RowDeleted);
					}
					else
					{
						cache.Graph.RowDeleted.RemoveHandler(((PXParentAttribute)attr)._ParentType, ((PXParentAttribute)attr).RowDeleted);
					}
				}
			}
		}

		public virtual PXView GetParentSelect(PXCache sender)
		{
			return sender.Graph.TypedViews.GetView(_SelectParent, false);
		}

		public virtual PXView GetChildrenSelect(PXCache sender)
		{
			if (_SelectChildren == null)
			{
				_initialize(sender);
			}
			return sender.Graph.TypedViews.GetView(_SelectChildren, false);
		}

		public virtual bool UseCurrent
		{
			get
			{
				return _UseCurrent;
			}
			set
			{
				_UseCurrent = value;
			}
		}
		#endregion

		#region Ctor
		public PXParentAttribute(Type selectParent)
		{
			if (selectParent == null)
			{
				throw new PXArgumentException("selectParent", ErrorMessages.ArgumentNullException);
			}
			if (!typeof(IBqlSelect).IsAssignableFrom(selectParent) || selectParent.GetGenericArguments().Length == 0)
			{
				throw new PXArgumentException("selectParent", ErrorMessages.PXParentAllowsOnlyBQLSelections);
			}
			_SelectParent = BqlCommand.CreateInstance(selectParent);
			_ParentType = _SelectParent.GetTables()[0];
		}
		#endregion

		#region Implementation
		public virtual void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (_SelectChildren == null)
			{
				_initialize(sender);
			}
			foreach (object item in sender.Graph.TypedViews.GetView(_SelectChildren, false).SelectMultiBound(new object[] { e.Row }))
			{
				sender.Graph.Caches[_ChildType].Delete(item);
			}
			sender.Graph.TypedViews.GetView(_SelectChildren, false).Clear();
		}

		/// <summary>
		/// Selects record from parent table and a list of corresponding records from child table.
		/// </summary>
		/// <param name="cache">Cache.</param>
		/// <param name="row">Row.</param>
		/// <param name="field">Field of child table.</param>
		/// <param name="records">[out] Resulting list of needed records from child table.</param>
		/// <returns>Record of parent table or null in case of failure.</returns>
		public static object[] SelectSiblings(PXCache cache, object row)
		{
			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					parents.Add(attr);
				}
			}

			if (parents.Count > 1)
			{
				throw new PXException(ErrorMessages.UnconditionalUniqueParent, cache.GetItemType().ToString());
			}

			if (parents.Count > 0)
			{
				PXParentAttribute attr = (PXParentAttribute)parents[0];

				PXView parentview = attr.GetParentSelect(cache);
				object parent;
				if (attr._UseCurrent)
				{
					parent = parentview.Cache.Current;
				}
				else
				{
					parent = PXSelectorAttribute.SelectSingleBound(parentview, new object[] { row });
				}
				if (parent != null || parentview.Cache.Current != null && parentview.Cache.GetStatus(parentview.Cache.Current) != PXEntryStatus.Deleted)
				{
					PXView view = attr.GetChildrenSelect(cache);
					return view.SelectMultiBound(new object[] { parent }).ToArray();
				}
			}
			return new object[0];
		}

		public static object[] SelectSiblings(PXCache cache, object row, Type ParentType)
		{
			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					if (attr is PXParentAttribute)
					{
						if (((PXParentAttribute)attr)._ParentType == ParentType)
						{
							parents.Insert(0, attr);
						}
						else if (ParentType.IsSubclassOf(((PXParentAttribute)attr)._ParentType))
						{
							parents.Add(attr);
						}
					}
				}
			}

			if (parents.Count > 0)
			{
				PXEventSubscriberAttribute attr = parents[0];
				PXView parentview = ((PXParentAttribute)attr).GetParentSelect(cache);
				object parent;
				if (((PXParentAttribute)attr)._UseCurrent)
				{
					parent = parentview.Cache.Current;
				}
				else
				{
					parent = PXSelectorAttribute.SelectSingleBound(parentview, new object[] { row });
				}
				if (parent != null || parentview.Cache.Current != null && parentview.Cache.GetStatus(parentview.Cache.Current) != PXEntryStatus.Deleted)
				{
					PXView view = ((PXParentAttribute)attr).GetChildrenSelect(cache);
					return view.SelectMultiBound(new object[] { parent }).ToArray();
				}
			}

			return new object[0];
		}

		public static Type GetParentType(PXCache cache)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					return ((PXParentAttribute)attr)._ParentType;
				}
			}
			return null;
		}

		public static void SetParent(PXCache cache, object row, Type ParentType, object parent)
		{
			if (row == null)
			{
				throw new PXArgumentException();
			}

			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					if (((PXParentAttribute)attr)._ParentType == ParentType)
					{
						parents.Insert(0, attr);
					}
					else if (ParentType.IsSubclassOf(((PXParentAttribute)attr)._ParentType))
					{
						parents.Add(attr);
					}
				}
			}

			if (parents.Count > 0)
			{
				PXParentAttribute attr = (PXParentAttribute)parents[0];

				if (parent == null)
				{
					if (attr._Currents.ContainsKey(row))
					{
						attr._Currents.Remove(row);
					}
				}
				else
				{
					object cached;
					if ((cached = cache.Locate(row)) == null || cache.GetStatus(row) == PXEntryStatus.InsertedDeleted || cache.GetStatus(row) == PXEntryStatus.Deleted)
					{
						attr._Pendings[row] = parent;
					}
					else
					{
						attr._Currents[cached] = parent;
					}
				}
			}

		}

		public static void CopyParent(PXCache cache, object item, object copy, Type ParentType)
		{
			if (item == null)
			{
				throw new PXArgumentException();
			}

			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					if (((PXParentAttribute)attr)._ParentType == ParentType)
					{
						parents.Insert(0, attr);
					}
					else if (ParentType.IsSubclassOf(((PXParentAttribute)attr)._ParentType))
					{
						parents.Add(attr);
					}
				}
			}

			if (parents.Count > 0)
			{
				PXParentAttribute attr = (PXParentAttribute)parents[0];

				object parent;
				if (attr._Currents.TryGetValue(item, out parent))
				{
					attr._Currents[copy] = parent;
				}
			}
		}

		public static object SelectParent(PXCache cache, object row)
		{
			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					parents.Add(attr);
				}
			}

			if (parents.Count > 1)
			{
				throw new PXException(ErrorMessages.UnconditionalUniqueParent, cache.GetItemType().ToString());
			}

			if (parents.Count > 0)
			{
				PXParentAttribute attr = (PXParentAttribute)parents[0];

				PXView parentview = attr.GetParentSelect(cache);

				object parent;
				if (attr._UseCurrent)
				{
					parent = parentview.Cache.Current;

					if (parentview.Cache.GetStatus(parent) != PXEntryStatus.Deleted)
					{
						return parent;
					}
					else
					{
						return null;
					}
				}
				else
				{
					if (attr._Currents.TryGetValue(row, out parent))
					{
						if (parentview.Cache.GetStatus(parent) != PXEntryStatus.Deleted)
						{
							return parent;
						}
						else
						{
							return null;
						}
					}

					object pending = null;
					foreach (KeyValuePair<object, object> pair in attr._Pendings)
					{
						if (cache.ObjectsEqual(pair.Key, row))
						{
							pending = pair.Key;
							parent = pair.Value;

							break;
						}
					}

					if (parent != null)
					{
						attr._Currents[row] = parent;
						attr._Pendings.Remove(pending);

						return parent;
					}

					return PXSelectorAttribute.SelectSingleBound(parentview, new object[] { row });
				}
			}

			return null;
		}
		public static object SelectParent(PXCache cache, object row, Type ParentType)
		{
			List<PXEventSubscriberAttribute> parents = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute)
				{
					if (((PXParentAttribute)attr)._ParentType == ParentType)
					{
						parents.Insert(0, attr);
					}
					else if (ParentType.IsSubclassOf(((PXParentAttribute)attr)._ParentType))
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

				object parent;
				if (attr._UseCurrent)
				{
					parent = parentview.Cache.Current;

					if (parentview.Cache.GetStatus(parent) != PXEntryStatus.Deleted)
					{
						return parent;
					}
					else
					{
						return null;
					}
				}
				else
				{
					if (row != null)
					{
						if (attr._Currents.TryGetValue(row, out parent))
						{
							if (parentview.Cache.GetStatus(parent) != PXEntryStatus.Deleted)
							{
								return parent;
							}
							else
							{
								return null;
							}
						}

						object pending = null;
						foreach (KeyValuePair<object, object> pair in attr._Pendings)
						{
							if (cache.ObjectsEqual(pair.Key, row))
							{
								pending = pair.Key;
								parent = pair.Value;

								break;
							}
						}

						if (parent != null)
						{
							attr._Currents[row] = parent;
							attr._Pendings.Remove(pending);

							return parent;
						}
					}
					return PXSelectorAttribute.SelectSingleBound(parentview, new object[] { row });
				}

			}
			return null;
		}
		#endregion

		#region Initialization
		protected static Dictionary<Type, BqlCommand> _selects = new Dictionary<Type, BqlCommand>();
		protected static object _slock = new object();
		protected void _initialize(PXCache sender)
		{
			BqlCommand selectchild;
			lock (_slock)
			{
				if (!_selects.TryGetValue(_SelectParent.GetType(), out selectchild))
				{
					Type childType = _BqlTable;
					selectchild = BqlCommand.CreateInstance(_inverse(_SelectParent.GetType(), _ParentType, ref childType));
					_selects.Add(_SelectParent.GetType(), selectchild);
				}
			}
			_SelectChildren = selectchild;
		}
		protected static Type _inverse(Type command, Type parent, ref Type child)
		{
			if (!command.IsGenericType)
			{
				if (command == parent)
				{
					return child;
				}
				if (typeof(IBqlField).IsAssignableFrom(command) && command.IsNested && BqlCommand.GetItemType(command) == parent)
				{
					return BqlCommand.Compose(typeof(Current<>), command);
				}
				return command;
			}
			else
			{
				Type[] args = command.GetGenericArguments();
				if ((command.GetGenericTypeDefinition() == typeof(Current<>) || command.GetGenericTypeDefinition() == typeof(Current2<>)) && typeof(IBqlField).IsAssignableFrom(args[0]) && args[0].IsNested && child.IsAssignableFrom(BqlCommand.GetItemType(args[0])))
				{
					child = BqlCommand.GetItemType(args[0]);
					return args[0];
				}
				bool anyChanged = false;
				for (int i = args.Length - 1; i >= 0; i--)
				{
					Type t = _inverse(args[i], parent, ref child);
					if (t != args[i])
					{
						args[i] = t;
						anyChanged = true;
					}
				}
				if (!anyChanged)
				{
					return command;
				}
				Type[] pars = new Type[args.Length + 1];
				pars[0] = command.GetGenericTypeDefinition();
				for (int i = 1; i < pars.Length; i++)
				{
					pars[i] = args[i - 1];
				}
				return BqlCommand.Compose(pars);
			}
		}
		public override void CacheAttached(PXCache sender)
		{
			_ChildType = sender.GetItemType();
			if (!_LeaveChildren && sender.Graph.Caches.ContainsKey(_ParentType))
			{
				sender.Graph.RowDeleted.AddHandler(_ParentType, this.RowDeleted);
			}
			_Currents = new Dictionary<object, object>();
			_Pendings = new Dictionary<object, object>();
		}
		#endregion
	}
	#endregion

	#region PXUIFieldAttribute
	/// <summary>
	/// Allows you to specify some properties displayed in the user interface.<br/>
	/// You can specify the display name of field editor control,<br/> 
	/// Specify, whether control is visible or enabled,<br/> 
	/// Display red marker for required value,<br/>
	/// Control access rigts assignment.<br/>
	/// At runtime it is possible to show error and warning messages related to the field.<br/>
	/// </summary>
	/// <example>
	/// [PXUIField(DisplayName = "Status Date", Enabled = false)]
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class)]
	[PXAttributeFamily(typeof(PXUIFieldAttribute))]
	public class PXUIFieldAttribute : PXEventSubscriberAttribute, IPXInterfaceField, IPXExceptionHandlingSubscriber, IPXCommandPreparingSubscriber, IPXFieldSelectingSubscriber, IPXFieldVerifyingSubscriber
	{
		#region State
		protected string _ErrorText = null;
		protected object _ErrorValue = null;
		protected PXErrorLevel _ErrorLevel = PXErrorLevel.Undefined;
		protected bool _Enabled = true;
		protected bool _Visible = true;
		protected bool _ReadOnly = false;
		protected string _DisplayName = null;
		protected bool _Filterable = true;
		protected PXUIVisibility _Visibility = PXUIVisibility.Visible;
		protected int _TabOrder = -1;
		protected bool _ViewRights = true;
		protected bool _EnableRights = true;
		protected PXCacheRights _MapViewRights = PXCacheRights.Select;
		protected PXCacheRights _MapEnableRights = PXCacheRights.Update;
		protected PXErrorHandling _ErrorHandling = PXErrorHandling.WhenVisible;
		protected bool? _Required;
		protected object _RestoredValue;
		protected string _NeutralDisplayName = null;
		protected string _FieldClass;

		public virtual string FieldClass
		{
			get
			{
				return _FieldClass;
			}
			set
			{
				_FieldClass = value;
			}
		}
		/// <summary>
		/// Shows red marker over field editor control.
		/// </summary>
		public virtual bool Required
		{
			get
			{
				return _Required == true;
			}
			set
			{
				_Required = value;
			}
		}
		public virtual PXErrorHandling ErrorHandling
		{
			get
			{
				return _ErrorHandling;
			}
			set
			{
				_ErrorHandling = value;
			}
		}
		protected internal virtual bool ViewRights
		{
			get
			{
				return _ViewRights;
			}
			set
			{
				_ViewRights = value;
				if (!value)
				{
					_Visible = false;
				}
			}
		}
		protected internal virtual bool EnableRights
		{
			get
			{
				return _EnableRights;
			}
			set
			{
				_EnableRights = value;
				if (!value)
				{
					_Enabled = false;
				}
			}
		}
		public virtual PXCacheRights MapViewRights
		{
			get
			{
				return _MapViewRights;
			}
			set
			{
				_MapViewRights = value;
			}
		}
		public virtual PXCacheRights MapEnableRights
		{
			get
			{
				return _MapEnableRights;
			}
			set
			{
				_MapEnableRights = value;
			}
		}
		public PXUIFieldAttribute()
		{
		}
		string IPXInterfaceField.ErrorText
		{
			get
			{
				return _ErrorText;
			}
			set
			{
				_ErrorText = value;
			}
		}
		object IPXInterfaceField.ErrorValue
		{
			get
			{
				return _ErrorValue;
			}
			set
			{
				_ErrorValue = value;
			}
		}
		void IPXInterfaceField.ForceEnabled()
		{
			_Enabled = true;
		}
		bool IPXInterfaceField.ViewRights
		{
			get
			{
				return _ViewRights;
			}
		}

		/// <summary>
		/// Allows to disable field edit control or grid column in user interface.<br/>
		/// See also IsReadOnly property with similar behavior.
		/// </summary>
		public virtual bool Enabled
		{
			get
			{
				return _Enabled;
			}
			set
			{
				if (_EnableRights || !value)
				{
					_Enabled = value;
				}
			}
		}

		/// <summary>
		/// Allows to show/hide field edit control or grid column in user interface.<br/>
		/// To control, whether form designer should generate template for this field, use Visibility property instead.
		/// </summary>
		public virtual bool Visible
		{
			get
			{
				return _Visible;
			}
			set
			{
				if (_ViewRights || !value)
				{
					_Visible = value;
				}
			}
		}

		/// <summary>
		/// Does not allows to modify value from user interface.<br/>
		/// User can select value, but can not change it.<br/>
		/// Editor conrol does not gets gray color, unlike Enabled=false.
		/// </summary>
		public virtual bool IsReadOnly
		{
			get
			{
				return _ReadOnly;
			}
			set
			{
				_ReadOnly = value;
			}
		}

		/// <summary>
		/// Field name displayed in the user interface.<br/>
		/// This name is rendered as a label of fied editor on a FormView<br/>
		/// or as grid column header.
		/// </summary>
		public virtual string DisplayName
		{
			get
			{
				if (_DisplayName == null)
				{
					return _DisplayName = _FieldName;
				}
				return _DisplayName;
			}
			set
			{
				_DisplayName = value;
			}
		}

		/// <summary>
		/// controls whether the form designer should generate template for this field.<br/>
		/// Visible means that template will be generated on a FormView and in a Grid<br/>
		/// SelectorVisible - the same as visible plus columns will be added to Selectors<br/>
		/// Invisible means that the field should never appear in the user interface.
		/// </summary>
		public virtual PXUIVisibility Visibility
		{
			get
			{
				return _Visibility;
			}
			set
			{
				_Visibility = value;
			}
		}
		public virtual int TabOrder
		{
			get
			{
				if (_TabOrder == -1)
				{
					return _FieldOrdinal;
				}
				return _TabOrder;
			}
			set
			{
				_TabOrder = value;
			}
		}
		internal string NeutralDisplayName
		{
			get
			{
				if (_NeutralDisplayName == null && _DisplayName != null)
				{
					_NeutralDisplayName = _DisplayName;
				}
				return _NeutralDisplayName;
			}
		}
		#endregion

		#region Runtime
		public void ChangeNeutralDisplayName(string newValue)
		{
			_NeutralDisplayName = newValue;
		}

		public static Dictionary<string, string> GetErrors(PXCache cache, object data)
		{
			Dictionary<string, string> ret = new Dictionary<string, string>();
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, null))
			{
				if (attr is IPXInterfaceField)
				{
					string err = ((IPXInterfaceField)attr).ErrorText;
					if (!String.IsNullOrEmpty(err))
					{
						ret[attr.FieldName] = err;
					}
				}
			}
			return ret;
		}

		public static void SetError(PXCache cache, object data, string name, string error)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is IPXInterfaceField)
				{
					((IPXInterfaceField)attr).ErrorText = PXMessages.LocalizeNoPrefix(error);
				}
			}
		}

		public static void SetError<Field>(PXCache cache, object data, string error)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is IPXInterfaceField)
				{
					((IPXInterfaceField)attr).ErrorText = PXMessages.LocalizeNoPrefix(error);
				}
			}
		}

		public static void SetError(PXCache cache, object data, string name, string error, string errorvalue)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr)._ErrorText = PXMessages.LocalizeNoPrefix(error);
					((PXUIFieldAttribute)attr)._ErrorLevel = PXErrorLevel.Error;
					((PXUIFieldAttribute)attr)._ErrorValue = errorvalue;
				}
			}
		}

		public static void SetError<Field>(PXCache cache, object data, string error, string errorvalue)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr)._ErrorText = PXMessages.LocalizeNoPrefix(error);
					((PXUIFieldAttribute)attr)._ErrorLevel = PXErrorLevel.Error;
					((PXUIFieldAttribute)attr)._ErrorValue = errorvalue;
				}
			}
		}

		public static void SetWarning(PXCache cache, object data, string name, string error)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr)._ErrorText = PXMessages.LocalizeNoPrefix(error);
					((PXUIFieldAttribute)attr)._ErrorLevel = PXErrorLevel.Warning;
					((PXUIFieldAttribute)attr)._ErrorValue = null;
				}
			}
		}

		public static void SetWarning<Field>(PXCache cache, object data, string error)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr)._ErrorText = PXMessages.LocalizeNoPrefix(error);
					((PXUIFieldAttribute)attr)._ErrorLevel = PXErrorLevel.Warning;
					((PXUIFieldAttribute)attr)._ErrorValue = null;
				}
			}
		}

		public static string GetError<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXUIFieldAttribute)
				{
					return ((PXUIFieldAttribute)attr)._ErrorText;
				}
			}
			return null;
		}

		public static string GetError(PXCache cache, object data, string name)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXUIFieldAttribute)
				{
					return ((PXUIFieldAttribute)attr)._ErrorText;
				}
			}
			return null;
		}

		public static void SetEnabled(PXCache cache, object data, string name, bool isEnabled)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Enabled = isEnabled;
				}
			}
		}
		public static void SetEnabled(PXCache cache, object data, bool isEnabled)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, null))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Enabled = isEnabled;
					if (data == null)
					{
						cache.SetAltered(attr.FieldName, true);
					}
				}
			}
		}
		public static void SetEnabled<Field>(PXCache cache, object data, bool isEnabled)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Enabled = isEnabled;
				}
			}
		}
		public static void SetEnabled(PXCache cache, object data, string name)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Enabled = true;
				}
			}
		}
		public static void SetEnabled<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Enabled = true;
				}
			}
		}

		#region SetVisible

		public static void SetVisible<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Visible = true;
				}
			}
		}
		public static void SetVisible<Field>(PXCache cache, object data, bool isVisible)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Visible = isVisible;
				}
			}
		}
		public static void SetVisible(PXCache cache, object data, string name, bool isVisible)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Visible = isVisible;
				}
			}
		}
		public static void SetVisible(PXCache cache, object data, string name)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Visible = true;
				}
			}
		}

		#endregion

		#region SetReadOnly

		public static void SetReadOnly<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			SetReadOnly<Field>(cache, data, true);
		}
		public static void SetReadOnly(PXCache cache, object data, string name)
		{
			SetReadOnly(cache, data, name, true);
		}
		public static void SetReadOnly(PXCache cache, object data)
		{
			SetReadOnly(cache, data, true);
		}
		public static void SetReadOnly(PXCache cache, object data, bool isReadOnly)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, null))
			{
				var uiAtt = attr as PXUIFieldAttribute;
				if (uiAtt == null) continue;

				uiAtt.IsReadOnly = isReadOnly;
				if (data == null) cache.SetAltered(attr.FieldName, true);
			}
		}
		public static void SetReadOnly<Field>(PXCache cache, object data, bool isReadOnly)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr)._ReadOnly = isReadOnly;
				}
			}
		}
		public static void SetReadOnly(PXCache cache, object data, string name, bool isReadOnly)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr)._ReadOnly = isReadOnly;
				}
			}
		}

		#endregion

		public static void SetEnabled(PXCache cache, string name, bool isEnabled)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Enabled = isEnabled;
				}
			}
		}
		public static void SetVisible(PXCache cache, string name, bool isVisible)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr).Visible = isVisible;
				}
			}
		}
		public static void SetVisibility<Field>(PXCache cache, object data, PXUIVisibility visibility)
			where Field : IBqlField
		{
			if (visibility == PXUIVisibility.Undefined)
			{
				return;
			}
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr)._Visibility = visibility;
				}
			}
		}
		public static void SetVisibility(PXCache cache, object data, string name, PXUIVisibility visibility)
		{
			if (visibility == PXUIVisibility.Undefined)
			{
				return;
			}
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr)._Visibility = visibility;
				}
			}
		}
		public static void SetVisibility(PXCache cache, string name, PXUIVisibility visibility)
		{
			if (visibility == PXUIVisibility.Undefined)
			{
				return;
			}
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr)._Visibility = visibility;
				}
			}
		}
		public static string GetDisplayName<Field>(PXCache cache)
			where Field : IBqlField
		{
			return GetDisplayName(cache, typeof(Field).Name);
		}
		public static string GetDisplayName(PXCache cache, string fieldName)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(fieldName))
			{
				if (attr is PXUIFieldAttribute)
				{
					return ((PXUIFieldAttribute)attr).DisplayName;
				}
			}
			return fieldName;
		}
		public static string GetNeutralDisplayName(PXCache cache, string fieldName)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(fieldName))
			{
				if (attr is PXUIFieldAttribute)
				{
					return ((PXUIFieldAttribute)attr).NeutralDisplayName;
				}
			}
			return fieldName;
		}
		public static void SetDisplayName<Field>(PXCache cache, string displayName)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXUIFieldAttribute)
				{
					uint msgnum;
					string msgprefix;
					((PXUIFieldAttribute)attr).DisplayName = PXMessages.Localize(displayName, out msgnum, out msgprefix);
					break;
				}
			}
		}
		public static void SetDisplayName(PXCache cache, string fieldName, string displayName)
		{
			cache.SetAltered(fieldName, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(fieldName))
			{
				if (attr is PXUIFieldAttribute)
				{
					uint msgnum;
					string msgprefix;
					((PXUIFieldAttribute)attr).DisplayName = PXMessages.Localize(displayName, out msgnum, out msgprefix);
					break;
				}
			}
		}
		public static void SetRequired<Field>(PXCache cache, bool required)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr)._Required = required;
				}
			}
		}
		public static void SetRequired(PXCache cache, string name, bool required)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXUIFieldAttribute)
				{
					((PXUIFieldAttribute)attr)._Required = required;
				}
			}
		}
		#endregion

		#region Implementation
		public virtual void ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item && e.Row != null)
			{
				if (e.Exception != null)
				{
					if (_RestoredValue != null && e.NewValue == _RestoredValue)
					{
						e.Cancel = true;
						return;
					}
					if (!String.IsNullOrEmpty(_ErrorText) && _ErrorLevel != PXErrorLevel.Warning && _ErrorLevel != PXErrorLevel.RowWarning && _ErrorLevel != PXErrorLevel.Undefined)
					{
						if (e.Exception is PXSetPropertyKeepPreviousException)
						{
							e.Cancel = true;
							return;
						}
						if (e.Exception is PXRowPersistingException)
						{
							return;
						}
					}
					_ErrorValue = e.NewValue;
					_ErrorText = e.Exception.Message;
					_ErrorLevel = PXErrorLevel.Error;
					if (e.Exception is PXSetPropertyException)
					{
						_ErrorLevel = ((PXSetPropertyException)e.Exception).ErrorLevel;
						_ErrorText = ((PXSetPropertyException)e.Exception).MessageNoPrefix;
						if (((PXSetPropertyException)e.Exception).ErrorValue != null)
						{
							_ErrorValue = ((PXSetPropertyException)e.Exception).ErrorValue;
						}
					}

					e.Cancel = e.Exception is PXSetPropertyException && (_ErrorHandling == PXErrorHandling.Always || Visible && _ErrorHandling == PXErrorHandling.WhenVisible);

					int fid = _ErrorText.IndexOf("{0}", StringComparison.OrdinalIgnoreCase);
					if (fid >= 0)
					{
						_ErrorText = _ErrorText.Remove(fid, 3).Insert(fid, _DisplayName ?? _FieldName);
						if (e.Exception is PXOverridableException)
						{
							((PXOverridableException)e.Exception).SetMessage(_ErrorText);
						}
					}

					if (_DisplayName != null && _FieldName != _DisplayName)
					{
						fid = _ErrorText.IndexOf(_FieldName, StringComparison.OrdinalIgnoreCase);
						if (fid >= 0)
						{
							_ErrorText = _ErrorText.Remove(fid, _FieldName.Length).Insert(fid, _DisplayName);

							if (e.Exception is PXOverridableException)
							{
								((PXOverridableException)e.Exception).SetMessage(_ErrorText);
							}
						}
					}
				}
				else
				{
					_ErrorValue = null;
					_ErrorText = null;
					_ErrorLevel = PXErrorLevel.Undefined;
					e.Cancel = true;
				}
			}
		}
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update) &&
				!String.IsNullOrEmpty(_ErrorText) && _ErrorLevel != PXErrorLevel.Warning && _ErrorLevel != PXErrorLevel.RowWarning)
			{
				string prefix;
				switch (e.Operation)
				{
					case PXDBOperation.Insert:
						prefix = "Inserting ";
						break;
					case PXDBOperation.Delete:
						prefix = "Deleting ";
						break;
					default:
						prefix = "Updating ";
						break;
				}
				throw new PXOuterException(GetErrors(sender, e.Row), sender.Graph.GetType(), e.Row,
					ErrorMessages.RecordRaisedErrors, prefix, GetItemName(sender));
			}
		}

		public static string GetItemName(PXCache sender)
		{
			var entityType = sender.GetItemType();
			object[] cachename = entityType.GetCustomAttributes(typeof(PXCacheNameAttribute), false);
			if (cachename != null)
				foreach (PXCacheNameAttribute att in cachename)
				{
					var localName = att.GetName();
					if (!string.IsNullOrEmpty(localName)) return localName;
				}
			return entityType.Name;
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (!_ViewRights && !e.ExternalCall)
			{
				e.ReturnValue = null;
			}
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				if (!String.IsNullOrEmpty(_ErrorText) && _ErrorLevel == PXErrorLevel.Error)
				{
					e.ReturnValue = _ErrorValue;
				}

				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, sender.Keys.Contains(_FieldName), null,
					_Required == null ? (e.Row == null && !_Enabled ? -3 : 0) : (_Required == true ? 3 : -3),
					null, null, null, _FieldName, null, _DisplayName, _ErrorText, _ErrorLevel, _Enabled, Visible, _ReadOnly,
				_ViewRights ? _Visibility : PXUIVisibility.Invisible, null, null, null);
			}
		}
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!_EnableRights && e.ExternalCall)
			{
				object valpending = sender.GetValuePending(e.Row, _FieldName);
				object valexisting = sender.GetValue(e.Row, _FieldOrdinal);
				if (e.NewValue == valpending && valpending != valexisting
					&& (valexisting != null || !sender.Keys.Contains(_FieldName)))
				{
					if (_ViewRights)
					{
						throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.NotEnoughRights, _FieldName));
					}
					else
					{
						_RestoredValue = valexisting;
						e.NewValue = _RestoredValue;
					}
				}
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			PXAccess.Secure(sender, this);

			TryLocalize(sender);
		}

		private UIFieldSourceType GetFieldSourceType(PXCache fieldCache)
		{
			UIFieldSourceType sourceType;

			if (fieldCache.Graph != null && FieldOrdinal == -1)
			{
				sourceType = _FieldName.Contains(Localizers.PXUIFieldLocalizer.AUTOMATION_BUTTON_SYMBOL) ? UIFieldSourceType.AutomationButtonName : UIFieldSourceType.ActionName;
			}
			else
			{
				Type classType = fieldCache.GetItemType();
				PropertyInfo prop = classType.GetProperty(_FieldName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

				if (prop != null)
				{
					sourceType = UIFieldSourceType.DacFieldName;
				}
				else
				{
					Type extention = PXPageRipper.GetExtentionWithProperty(fieldCache.GetExtensionTypes(), fieldCache.GetItemType(), FieldName);

					sourceType = extention != null ? UIFieldSourceType.CacheExtensionFieldName : UIFieldSourceType.Undefined;
				}
			}

			return sourceType;
		}

		protected void TryLocalize(PXCache sender)
		{
			if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Items.Contains(PXControlPropertiesCollector.COLLECTION_RESOURCES_KEY))
			{
				PXPageRipper.RipUIField(this, sender, GetFieldSourceType(sender));
			}
			else
			{
				PXLocalizerRepository.UIFieldLocalizer.Localize(this, _BqlTable.FullName, sender, GetFieldSourceType(sender));
			}
		}
		#endregion
	}
	#endregion

	#region PXExtraKeyAttribute
	/// <summary>
	/// Provides base interaction with the database
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXExtraKeyAttribute : PXEventSubscriberAttribute, IPXCommandPreparingSubscriber
	{
		#region Implementation
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			e.IsRestriction = true;
		}
		#endregion
	}
	#endregion

	#region PXDBFieldAttribute
	/// <summary>
	/// Base class for attributes, that implements mapping of DAC field to the database column.
	/// Do not use directly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXDBFieldAttribute))]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXDBFieldAttribute : PXEventSubscriberAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected string _DatabaseFieldName = null;
		protected bool _IsKey = false;
		protected bool _IsImmutable = false;
		public virtual string DatabaseFieldName
		{
			get
			{
				return _DatabaseFieldName;
			}
			set
			{
				_DatabaseFieldName = value;
			}
		}
		/// <summary>
		/// Key fields must uniquely identify a data row. 
		/// Not necessarily to be the same as keys in database.
		/// </summary>
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
				_IsImmutable = IsImmutable || value;
			}
		}
		/// <summary>
		/// Key fields must uniquely identify a data row. 
		/// Not necessarily to be the same as keys in database.
		/// </summary>
		public virtual bool IsImmutable
		{
			get
			{
				return _IsImmutable;
			}
			set
			{
				_IsImmutable = value;
			}
		}
		public virtual Type BqlField
		{
			get
			{
				return null;
			}
			set
			{
				_DatabaseFieldName = char.ToUpper(value.Name[0]) + value.Name.Substring(1);
				if (value.IsNested
					//&& typeof(IBqlTable).IsAssignableFrom(value.DeclaringType)
					)
				{
					BqlTable = value.DeclaringType;
				}
			}
		}
		#endregion

		#region Implementation
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (_DatabaseFieldName != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName;
				//e.FieldName = (e.Table == null ? _DatabaseFieldName : e.Table.Name + '.' + _DatabaseFieldName);
			}
			e.DataValue = e.Value;
			e.IsRestriction = e.IsRestriction || _IsKey;
		}
		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				object dbValue = e.Record.GetValue(e.Position);
				sender.SetValue(e.Row, _FieldOrdinal, dbValue);
			}
			e.Position++;
		}
		public override string ToString()
		{
			return String.Format("{0} {1} {2}", this.GetType().Name, FieldName, this.FieldOrdinal);
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (_DatabaseFieldName == null)
			{
				_DatabaseFieldName = _FieldName;
			}
			if (IsKey /*&& _BqlTable.IsAssignableFrom(sender.GetItemType())*/)
			{
				sender.Keys.Add(_FieldName);
			}
			if (IsImmutable)
			{
				sender.Immutables.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXUnboundKeyAttribute
	/// <summary>
	/// Marks the property as a key one
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public class PXUnboundKeyAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			sender.Keys.Add(FieldName);
		}
		#endregion
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, true, null, null, null, null, null, _FieldName, null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
			}
		}
	}
	#endregion

	#region PXDBStringAttribute
	/// <summary>
	/// This attribute implement mapping of DAC field of string type to the database field of char, varchar, nchar and nvarchar type.<br/>
	/// It also provide an option to specify the input validation mask for the sting.<br/>
	/// This attribute is automatically generated by the system when DAC is generated by DAC class creation wizard from the table properties.<br/>
	/// </summary>
	/// <example>
	/// In this example the RefNbr property of APRegister DAC is mapped to nvarchar(15) APRegister.RefNbr field of APRegister table.
	/// 
	/// <![CDATA[
	///public class APRegister : PX.Data.IBqlTable
	///{
	///...
	///[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
	///[PXDefault]
	///[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
	///[PXSelector(typeof(Search<APRegister.refNbr, Where<APRegister.docType, Equal<Optional<APRegister.docType>>>>), Filterable = true)]
	///public virtual String RefNbr
	///{
	///  get;
	///  set;
	///}
	///...
	///}
	/// ]]>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBStringAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected int _Length = -1;
		protected string _InputMask = null;
		protected bool _IsUnicode = false;
		protected bool _IsFixed = false;

		/// <summary>
		/// Maximum length of string value<br/>
		/// If a value exceeds the Length, it will be trimmed<br/>
		/// if IsFixed=true and value length is less then Length property, value will be extended with spaces.<br/>
		/// Default value: -1
		/// </summary>
		public int Length
		{
			get
			{
				return _Length;
			}
		}

		/// <summary>
		/// Pattern that indicates the allowed characters in string value.<br/>
		/// User interface does not allows the user to enter other characters to input controls.<br/>
		/// Default value: '>aaaaaa' for the key fields<br/>
		/// Control characters:<br/>
		///	'&gt;': following chars to upper case
		/// '&lt;': following chars to lower case
		/// '&amp;', 'C': Any character or a space
		/// 'A', 'a':  Letter or digit
		/// 'L', '?': Letter
		/// '#', '0', '9': Digit
		/// </summary>
		/// <example>
		/// InputMask = "&gt;LLLLL"
		/// InputMask = "&gt;aaaaaaaaaa"
		/// InputMask = "&gt;CC.00.00.00"
		/// </example>
		public string InputMask
		{
			get
			{
				return _InputMask;
			}
			set
			{
				_InputMask = value;
			}
		}

		/// <summary>
		/// This property should be true when database column is unicode string (nchar or nvarchar)<br/>
		/// Default value: false
		/// </summary>
		public bool IsUnicode
		{
			get
			{
				return _IsUnicode;
			}
			set
			{
				_IsUnicode = value;
			}
		}

		/// <summary>
		/// This property should be true when database column has fixed length(char or nchar)<br/>
		/// Default value: false
		/// </summary>
		public bool IsFixed
		{
			get
			{
				return _IsFixed;
			}
			set
			{
				_IsFixed = value;
			}
		}
		protected enum MaskMode
		{
			Manual,
			Auto,
			Foreign
		}
		protected MaskMode _AutoMask = MaskMode.Manual;
		#endregion

		#region Ctor
		public PXDBStringAttribute()
		{
		}

		/// <param name="length">
		/// Maximum length of string value<br/>
		/// If a value exceeds the Length, it will be trimmed<br/>
		/// if IsFixed==true and value length is less then Length property, value will be extended with spaces.<br/>
		/// Default value: -1
		/// </param>
		public PXDBStringAttribute(int length)
		{
			_Length = length;
		}
		#endregion

		#region Runtime
		private static void setLength(PXDBStringAttribute attr, int length)
		{
			attr._Length = length;
		}
		public static void SetLength(PXCache cache, object data, string name, int length)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXDBStringAttribute)
				{
					setLength((PXDBStringAttribute)attr, length);
				}
			}
		}
		public static void SetLength<Field>(PXCache cache, object data, int length)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXDBStringAttribute)
				{
					setLength((PXDBStringAttribute)attr, length);
				}
			}
		}
		public static void SetLength(PXCache cache, string name, int length)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXDBStringAttribute)
				{
					setLength((PXDBStringAttribute)attr, length);
				}
			}
		}
		public static void SetLength<Field>(PXCache cache, int length)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXDBStringAttribute)
				{
					setLength((PXDBStringAttribute)attr, length);
				}
			}
		}
		public static void SetInputMask(PXCache cache, object data, string name, string mask)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXDBStringAttribute)
				{
					((PXDBStringAttribute)attr)._AutoMask = MaskMode.Manual;
					((PXDBStringAttribute)attr)._InputMask = mask;
				}
			}
		}
		public static void SetInputMask<Field>(PXCache cache, object data, string mask)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXDBStringAttribute)
				{
					((PXDBStringAttribute)attr)._AutoMask = MaskMode.Manual;
					((PXDBStringAttribute)attr)._InputMask = mask;
				}
			}
		}
		public static void SetInputMask(PXCache cache, string name, string mask)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXDBStringAttribute)
				{
					((PXDBStringAttribute)attr)._AutoMask = MaskMode.Manual;
					((PXDBStringAttribute)attr)._InputMask = mask;
				}
			}
		}
		public static void SetInputMask<Field>(PXCache cache, string mask)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXDBStringAttribute)
				{
					((PXDBStringAttribute)attr)._AutoMask = MaskMode.Manual;
					((PXDBStringAttribute)attr)._InputMask = mask;
				}
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue != null && !e.Cancel)
			{
				if (!_IsFixed)
				{
					e.NewValue = ((string)e.NewValue).TrimEnd();
				}
				if (_Length >= 0)
				{
					int length = ((string)e.NewValue).Length;
					if (length > _Length)
					{
						e.NewValue = ((string)e.NewValue).Substring(0, _Length);
					}
					else if (_IsFixed && length < _Length)
					{
						StringBuilder bld = new StringBuilder(((string)e.NewValue), _Length);
						for (int i = length; i < _Length; i++)
						{
							bld.Append(' ');
						}
						e.NewValue = bld.ToString();
					}
				}
			}
		}
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (_DatabaseFieldName != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName;
				//e.FieldName = (e.Table == null ? _DatabaseFieldName : e.Table.Name + '.' + _DatabaseFieldName);
			}
			e.DataType = _IsFixed ? (_IsUnicode ? PXDbType.NChar : PXDbType.Char) : (_IsUnicode ? PXDbType.NVarChar : PXDbType.VarChar);
			e.DataValue = e.Value;
			if (_Length >= 0)
			{
				e.DataLength = _Length;
			}
			e.IsRestriction = e.IsRestriction || _IsKey;
		}
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{

			if (e.Row != null)
			{
				string v = e.Record.GetString(e.Position);
				// amazon aws mySQL server won't let us globally change the sql_mode (to include PAD_CHAR_TO_FULL_LENGTH), that's why we need to pad spaces here
				if (v != null) {
					int spacesToPad = IsFixed ? Length - v.Length : 0;
					if (spacesToPad > 0)
						v = string.Concat(v, new String(' ', spacesToPad));
				}

				if (sender.Graph.StringTable != null)
					v = sender.Graph.StringTable.Add(v);

				if (_IsKey && !e.IsReadOnly)
				{
					//string key;
					sender.SetValue(e.Row, _FieldOrdinal, v);
					if ((v == null || sender.IsPresent(e.Row)) && sender.Graph.GetType() != typeof(PXGraph))
					{
						e.Row = null;
					}
				}
				else
				{
					sender.SetValue(e.Row, _FieldOrdinal, v);
				}
			}


			e.Position++;
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				if (_AutoMask == MaskMode.Auto)
				{
					_AutoMask = MaskMode.Manual;
					if (sender.Keys.IndexOf(_FieldName) != sender.Keys.Count - 1)
					{
						_InputMask = null;
					}
				}
				else if (_AutoMask == MaskMode.Foreign && !PXContext.GetSlot<bool>(PXSelectorAttribute.selectorBypassInit))
				{
					_AutoMask = MaskMode.Manual;
					if (!_masks.TryGetValue(_BqlTable.Name + "$" + _FieldName, out _InputMask))
					{
						string Mask = null;
						Type ForeignField = null;
						foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(null, _FieldName))
						{
							if (attr is PXSelectorAttribute)
							{
								ForeignField = ((PXSelectorAttribute)attr).Field;
							}
						}
						if (ForeignField != null)
						{
							PXCache ForeignCache = sender.Graph.Caches[BqlCommand.GetItemType(ForeignField)];
							foreach (PXEventSubscriberAttribute attr in ForeignCache.GetAttributes(null, ForeignField.Name))
							{
								if (attr is PXDBStringAttribute)
								{
									Mask = ((PXDBStringAttribute)attr).InputMask;
								}
							}
						}
						InputMask = Mask;
						lock (((ICollection)_masks).SyncRoot)
						{
							_masks[_BqlTable.Name + "$" + _FieldName] = Mask;
						}
					}
				}
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, _Length, _IsUnicode, _FieldName, _IsKey, null, String.IsNullOrEmpty(_InputMask) ? null : _InputMask, null, null, null, null);
			}
		}
		#endregion

		#region Initialization
		protected static Dictionary<string, string> _masks = new Dictionary<string, string>();
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (_InputMask == null)
			{
				if (IsKey)
				{
					StringBuilder sb = new StringBuilder(">");
					for (int i = 0; i < _Length; i++)
					{
						sb.Append("a");
					}
					InputMask = sb.ToString();
					_AutoMask = MaskMode.Auto;
				}
				else if (!_masks.TryGetValue(_BqlTable.Name + "$" + _FieldName, out _InputMask))
				{
					_AutoMask = MaskMode.Foreign;
				}
			}
		}
		#endregion
	}
	#endregion

	#region PXDBTextAttribute
	/// <summary>
	/// This attribute implements mapping of DAC field of [string] type to the database column of [NVarChar] or [VarChar] type.<br/>
	/// The attribute generates sql statements to read and write field value to database.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBTextAttribute : PXDBStringAttribute
	{
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (_DatabaseFieldName != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = (e.Operation & PXDBOperation.Option) != PXDBOperation.GroupBy ? (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName : BqlCommand.Null;
			}
			e.DataType = _IsUnicode ? PXDbType.NVarChar : PXDbType.VarChar;
			e.DataValue = e.Value;
			e.DataLength = e.Value is string ? ((string)e.Value).Length : 0;
			e.IsRestriction = e.IsRestriction || _IsKey;
		}
	}
	#endregion

	#region PXDBLocalStringAttribute
	/// <summary>
	/// String field stored in the database. Automatically provides localized values.
	/// This attribute takes from and writes values to database fields which have culture information specified in their names.
	/// For example, for field 'Description' english specific field will be 'DescriptionenGB', for russian - 'DescriptionruRU', etc.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBLocalStringAttribute : PXDBStringAttribute
	{
		protected static Dictionary<string, bool> _FieldForCultureExists = new Dictionary<string, bool>();
		private static System.Threading.ReaderWriterLock rwLock = new System.Threading.ReaderWriterLock();

		#region Ctor
		public PXDBLocalStringAttribute()
			: base()
		{
		}

		public PXDBLocalStringAttribute(int Length)
			: base(Length)
		{
		}
		#endregion

		#region Implementation
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			bool fieldexists;
			string culturename = System.Threading.Thread.CurrentThread.CurrentCulture.Name.Replace("-", "");
			if (string.Compare(culturename, "enUS", true) == 0)
			{
				PrepareCommand(e, _DatabaseFieldName);
				return;
			}

			rwLock.AcquireReaderLock(-1);
			try
			{
				if (_FieldForCultureExists.TryGetValue(culturename, out fieldexists))
				{
					if (fieldexists)
					{
						PrepareCommand(e, _DatabaseFieldName + culturename);
					}
					else
					{
						PrepareCommand(e, _DatabaseFieldName);
					}
				}
				else
				{
					System.Threading.LockCookie cookie = rwLock.UpgradeToWriterLock(-1);
					try
					{
						TryPrepareCommand(e, culturename);
					}
					finally
					{
						rwLock.DowngradeFromWriterLock(ref cookie);
					}
				}
			}
			finally
			{
				rwLock.ReleaseReaderLock();
			}
		}

		private bool TryPrepareCommand(PXCommandPreparingEventArgs e, string culturename)
		{
			try
			{
				using (PXDatabase.SelectSingle(BqlTable, new PXDataField(_DatabaseFieldName + culturename)))
				{
				}
			}
			catch (Exception)
			{
				PrepareCommand(e, _DatabaseFieldName);
				_FieldForCultureExists.Add(culturename, false);
				return false;
			}
			PrepareCommand(e, _DatabaseFieldName + culturename);
			_FieldForCultureExists.Add(culturename, true);
			return true;
		}

		private void PrepareCommand(PXCommandPreparingEventArgs e, string dbFieldName)
		{
			if (dbFieldName != null)
			{
				e.BqlTable = _BqlTable;
				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
					e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + dbFieldName;
				else
				{
					string tablename = (e.Table == null ? _BqlTable.Name : e.Table.Name);
					e.FieldName = "COALESCE(" + tablename + '.' + dbFieldName + ", " + tablename + '.' + _DatabaseFieldName + ")";
				}
			}

			e.DataType = _IsFixed ? (_IsUnicode ? PXDbType.NChar : PXDbType.Char) : (_IsUnicode ? PXDbType.NVarChar : PXDbType.VarChar);
			e.DataValue = e.Value;

			if (_Length >= 0)
			{
				e.DataLength = _Length;
			}
			e.IsRestriction = e.IsRestriction || _IsKey;
		}
		#endregion
	}
	#endregion

	#region PXStringAttribute
	/// <summary>
	/// Indicates a string field that is NOT mapped to a database column.<br/>
	/// Also, this attribute controls string length and allowed characters.<br/>
	/// </summary>
	/// <example>
	/// [PXString(8, IsFixed = true, InputMask = "CC.CC.CC.CC")]
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXStringAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected int _Length = -1;
		protected string _InputMask = null;
		protected bool _IsUnicode = false;
		protected bool _IsFixed = false;
		protected bool _IsKey = false;

		public virtual bool IsKey
		{
			get { return _IsKey; }
			set { _IsKey = value; }
		}

		public int Length
		{
			get { return _Length; }
		}

		public string InputMask
		{
			get { return _InputMask; }
			set { _InputMask = value; }
		}

		public bool IsFixed
		{
			get { return _IsFixed; }
			set { _IsFixed = value; }
		}

		public bool IsUnicode
		{
			get { return this._IsUnicode; }
			set { this._IsUnicode = value; }
		}
		#endregion

		#region Ctor
		public PXStringAttribute()
		{
		}
		public PXStringAttribute(int length)
		{
			_Length = length;
		}
		#endregion

		#region Runtime
		private static void setLength(PXStringAttribute attr, int length)
		{
			attr._Length = length;
		}
		public static void SetLength(PXCache cache, object data, string name, int length)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXStringAttribute)
				{
					setLength((PXStringAttribute)attr, length);
				}
			}
		}
		public static void SetLength<Field>(PXCache cache, object data, int length)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXStringAttribute)
				{
					setLength((PXStringAttribute)attr, length);
				}
			}
		}
		public static void SetLength(PXCache cache, string name, int length)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXStringAttribute)
				{
					setLength((PXStringAttribute)attr, length);
				}
			}
		}
		public static void SetLength<Field>(PXCache cache, int length)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXStringAttribute)
				{
					setLength((PXStringAttribute)attr, length);
				}
			}
		}
		public static void SetInputMask(PXCache cache, object data, string name, string mask)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXStringAttribute)
				{
					((PXStringAttribute)attr)._InputMask = mask;
				}
			}
		}
		public static void SetInputMask<Field>(PXCache cache, object data, string mask)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXStringAttribute)
				{
					((PXStringAttribute)attr)._InputMask = mask;
				}
			}
		}
		public static void SetInputMask(PXCache cache, string name, string mask)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXStringAttribute)
				{
					((PXStringAttribute)attr)._InputMask = mask;
				}
			}
		}
		public static void SetInputMask<Field>(PXCache cache, string mask)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXStringAttribute)
				{
					((PXStringAttribute)attr)._InputMask = mask;
				}
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue != null && !e.Cancel)
			{
				if (!_IsFixed)
				{
					e.NewValue = ((string)e.NewValue).TrimEnd();
				}
				if (_Length >= 0)
				{
					int length = ((string)e.NewValue).Length;
					if (length > _Length)
					{
						e.NewValue = ((string)e.NewValue).Substring(0, _Length);
					}
					else if (_IsFixed && length < _Length)
					{
						StringBuilder bld = new StringBuilder(((string)e.NewValue), _Length);
						for (int i = length; i < _Length; i++)
						{
							bld.Append(' ');
						}
						e.NewValue = bld.ToString();
					}
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, _Length, _IsUnicode, _FieldName, _IsKey, null, _InputMask, null, null, null, null);
			}
		}
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = _FieldName;
				e.DataType = _IsFixed ? (_IsUnicode ? PXDbType.NChar : PXDbType.Char) : (_IsUnicode ? PXDbType.NVarChar : PXDbType.VarChar);
				e.DataValue = e.Value;
				if (_Length >= 0)
				{
					e.DataLength = _Length;
				}
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBShortAttribute
	/// <summary>
	/// This attribute implements mapping of DAC field of [short?] type to the database column of [SmallInt] type.<br/>
	/// The attribute generates sql statements to read and write field value to database.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBShortAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected int _MinValue = short.MinValue;
		protected int _MaxValue = short.MaxValue;
		public int MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		public int MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Implementation
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (_DatabaseFieldName != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName;
				//e.FieldName = (e.Table == null ? _DatabaseFieldName : e.Table.Name + '.' + _DatabaseFieldName);
			}
			e.DataType = PXDbType.SmallInt;
			e.DataValue = e.Value;
			e.DataLength = 2;
			e.IsRestriction = e.IsRestriction || _IsKey;
		}
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetInt16(e.Position));
			}
			e.Position++;
		}
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				short val;
				if (short.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _MinValue, _MaxValue, null, null, typeof(short), null);
			}
		}
		#endregion
	}
	#endregion

	#region PXShortAttribute
	/// <summary>
	/// Indicates a DAC field of type [short] that is NOT mapped to a database column.<br/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXShortAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected int _MinValue = short.MinValue;
		protected int _MaxValue = short.MaxValue;
		protected bool _IsKey = false;
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
		public int MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		public int MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				short val;
				if (short.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _MinValue, _MaxValue, null, null, typeof(short), null);
			}
		}
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = _FieldName;
				e.DataType = PXDbType.SmallInt;
				e.DataValue = e.Value;
				e.DataLength = 2;
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBByteAttribute
	/// <summary>
	/// This attribute implements mapping of DAC field of [byte?] type to the database column of [TinyInt] type.<br/>
	/// The attribute generates sql statements to read and write field value to database.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBByteAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected int _MinValue = byte.MinValue;
		protected int _MaxValue = byte.MaxValue;
		public int MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		public int MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Implementation
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (_DatabaseFieldName != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName;
				//e.FieldName = (e.Table == null ? _DatabaseFieldName : e.Table.Name + '.' + _DatabaseFieldName);
			}
			e.DataType = PXDbType.TinyInt;
			e.DataValue = e.Value;
			e.DataLength = 1;
			e.IsRestriction = e.IsRestriction || _IsKey;
		}
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetByte(e.Position));
			}
			e.Position++;
		}
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				byte val;
				if (byte.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _MinValue, _MaxValue, null, null, typeof(byte), null);
			}
		}
		#endregion
	}
	#endregion

	#region PXByteAttribute
	/// <summary>
	/// Indicates a DAC field of type [short?] that is NOT mapped to a database column.<br/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXByteAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected int _MinValue = byte.MinValue;
		protected int _MaxValue = byte.MaxValue;
		protected bool _IsKey = false;
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
		public int MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		public int MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				byte val;
				if (byte.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _MinValue, _MaxValue, null, null, typeof(byte), null);
			}
		}
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = _FieldName;
				e.DataType = PXDbType.TinyInt;
				e.DataValue = e.Value;
				e.DataLength = 1;
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBGuidAttribute
	/// <summary>
	/// This attribute implements mapping of DAC field of [Guid?] type to the database column of [UniqueIdentifier] type.<br/>
	/// The attribute generates sql statements to read and write field value to database.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBGuidAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXFieldDefaultingSubscriber
	{
		private readonly bool _withDefaulting = false;

		public PXDBGuidAttribute() : base() { }

		public PXDBGuidAttribute(bool withDefaulting)
			: this()
		{
			_withDefaulting = withDefaulting;
		}

		#region Implementation
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (_DatabaseFieldName != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = (e.Operation & PXDBOperation.Option) != PXDBOperation.GroupBy ? (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName : BqlCommand.Null;
			}
			e.DataType = PXDbType.UniqueIdentifier;
			e.DataValue = e.Value;
			e.DataLength = 16;
			e.IsRestriction = e.IsRestriction || _IsKey;
		}
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetGuid(e.Position));
			}
			e.Position++;
		}
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				Guid val;
				if (GUID.TryParse((string)e.NewValue, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXGuidState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1);
			}
		}
		#endregion

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (_withDefaulting) e.NewValue = Guid.NewGuid();
		}
	}
	#endregion

	#region PXDBGuidMaintainDeletedAttribute
	/// <summary>
	/// This attribute is equivalent to the PXDBGuidAttribute but doesn't update Guid when restoring deleted record.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBGuidMaintainDeletedAttribute : PXDBGuidAttribute
	{
		public PXDBGuidMaintainDeletedAttribute() : base() { }

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Option) != PXDBOperation.Second)
				base.CommandPreparing(sender, e);
		}
	}
	#endregion

	#region PXGuidAttribute
	/// <summary>
	/// Indicates a DAC field of type [Guid?] that is NOT mapped to a database column.<br/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXGuidAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected bool _IsKey = false;
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				Guid val;
				if (GUID.TryParse((string)e.NewValue, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXGuidState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1);
			}
		}
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = _FieldName;
				e.DataType = PXDbType.UniqueIdentifier;
				e.DataValue = e.Value;
				e.DataLength = 16;
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBBoolAttribute
	/// <summary>
	/// This attribute implements mapping of DAC field of [bool?] type to the database column of [bit] type.<br/>
	/// The attribute generates sql statements to read and write field value to database.
	/// </summary>
	/// <example>
	///	[PXDBBool()]
	///	[PXDefault(false)]
	///	public virtual Boolean? Scheduled{get;set;}
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBBoolAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region Implementation
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (_DatabaseFieldName != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = (e.Operation & PXDBOperation.Option) != PXDBOperation.GroupBy ? (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName : BqlCommand.Null;
			}
			e.DataType = PXDbType.Bit;
			e.DataValue = e.Value;
			e.DataLength = 1;
			e.IsRestriction = e.IsRestriction || _IsKey;
		}
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetBoolean(e.Position));
			}
			e.Position++;
		}
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			PXBoolAttribute.ConvertValue(e);
			//if (e.NewValue is string)
			//{
			//    bool val;
			//    if (bool.TryParse((string)e.NewValue, out val))
			//    {
			//        e.NewValue = val;
			//    }
			//    else
			//    {
			//        string newValue = e.NewValue as string;
			//        if (!string.IsNullOrEmpty(newValue))
			//            switch (newValue.Trim())
			//            {
			//                case "1": e.NewValue = true; break;
			//                case "0": e.NewValue = false; break;
			//                default: e.NewValue = null; break;
			//            }
			//        else e.NewValue = null;
			//    }
			//}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(Boolean), _IsKey, null, -1, null, null, null, _FieldName, null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
			}
		}
		#endregion
	}
	#endregion

	#region PXBoolAttribute
	/// <summary>
	/// Indicates a DAC field of type [bool?] that is NOT mapped to a database column.<br/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXBoolAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected bool _IsKey = false;
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			ConvertValue(e);
		}

		public static void ConvertValue(PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				bool val;
				if (bool.TryParse((string)e.NewValue, out val))
				{
					e.NewValue = val;
				}
				else
				{
					string newValue = e.NewValue as string;
					if (!string.IsNullOrEmpty(newValue))
						switch (newValue.Trim())
						{
							case "1":
								e.NewValue = true;
								break;
							case "0":
								e.NewValue = false;
								break;
							default:
								e.NewValue = null;
								break;
						}
					else e.NewValue = null;
				}
			}
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(Boolean), _IsKey, null, -1, null, null, null, _FieldName, null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
			}
		}
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = _FieldName;
				e.DataType = PXDbType.Bit;
				e.DataValue = e.Value;
				e.DataLength = 1;
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion

		#region Runtime
		public static bool CheckSingleRow(PXCache cache, PXView view, object item, string fieldName)
		{
			bool? val = (bool?)cache.GetValue(item, fieldName);
			if (val == true)
			{
				foreach (object o in cache.Cached)
				{
					bool matching = true;
					foreach (string key in cache.Keys)
						if (!object.Equals(cache.GetValue(o, key), cache.GetValue(item, key)))
						{
							matching = false;
							break;
						}
					if (matching)
						continue;

					val = (bool?)cache.GetValue(o, fieldName);
					if (val == true)
					{
						cache.SetValue(o, fieldName, false);
						cache.Update(o);
					}
				}
				view.RequestRefresh();
				cache.IsDirty = false;
				return true;
			}
			return false;
		}

		public static bool CheckSingleRow<T>(PXCache cache, PXView view, object item) where T : IBqlField
		{
			return CheckSingleRow(cache, view, item, typeof(T).Name);
		}
		#endregion
	}
	#endregion

	#region PXDBIntAttribute
	/// <summary>
	/// This attribute implements mapping of DAC field of [int?] type to the database column of [int] type.<br/>
	/// The attribute generates sql statements to read and write field value to database.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBIntAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected int _MinValue = int.MinValue;
		protected int _MaxValue = int.MaxValue;
		public int MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		public int MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Implementation
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
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				if (_IsKey && !e.IsReadOnly)
				{
					int? key;
					sender.SetValue(e.Row, _FieldOrdinal, (key = e.Record.GetInt32(e.Position)));
					if ((key == null || sender.IsPresent(e.Row)) && sender.Graph.GetType() != typeof(PXGraph))
					{
						e.Row = null;
					}
				}
				else
				{
					sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetInt32(e.Position));
				}
			}
			e.Position++;
		}
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				int val;
				if (int.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _MinValue, _MaxValue, null, null, typeof(int), null);
			}
		}
		#endregion
	}
	#endregion

	#region PXIntAttribute
	/// <summary>
	/// Indicates a DAC field of type [int?] that is NOT mapped to a database column.<br/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXIntAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected int _MinValue = int.MinValue;
		protected int _MaxValue = int.MaxValue;
		protected bool _IsKey = false;
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
		public int MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		public int MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				int val;
				if (int.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _MinValue, _MaxValue, null, null, typeof(int), null);
			}
		}
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = _FieldName;
				e.DataType = PXDbType.Int;
				e.DataValue = e.Value;
				e.DataLength = 4;
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBLongAttribute
	/// <summary>
	/// 8-bytes integer stored in the database
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBLongAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected Int64 _MinValue = Int64.MinValue;
		protected Int64 _MaxValue = Int64.MaxValue;
		public Int64 MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		public Int64 MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Implementation
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (_DatabaseFieldName != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName;
				//e.FieldName = (e.Table == null ? _DatabaseFieldName : e.Table.Name + '.' + _DatabaseFieldName);
			}
			e.DataType = PXDbType.BigInt;
			e.DataValue = e.Value;
			e.DataLength = 8;
			e.IsRestriction = e.IsRestriction || _IsKey;
		}
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetInt64(e.Position));
			}
			e.Position++;
		}
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				Int64 val;
				if (Int64.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXLongState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _MinValue, _MaxValue, typeof(Int64));
			}
		}
		#endregion
	}
	#endregion

	#region PXLongAttribute
	/// <summary>
	/// Indicates a DAC field of type [long?] that is NOT mapped to a database column.<br/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXLongAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected Int64 _MinValue = Int64.MinValue;
		protected Int64 _MaxValue = Int64.MaxValue;
		protected bool _IsKey = false;
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
		public Int64 MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		public Int64 MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				Int64 val;
				if (Int64.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXLongState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _MinValue, _MaxValue, typeof(long));
			}
		}
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = _FieldName;
				e.DataType = PXDbType.BigInt;
				e.DataValue = e.Value;
				e.DataLength = 8;
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBDateAttribute
	/// <summary>
	/// Because of this attribute [DateTime?] field is mapped to a database DateTime or SmallDateTime column.<br/>
	/// UseSmallDateTime flag specifies the type of database column.<br/>
	/// </summary>
	/// <example>
	///	[PXDBDate(MaxValue = "06/06/2079", MinValue = "01/01/1900")]
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBDateAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected static readonly DateTime _MIN_VALUE = new DateTime(1900, 1, 1);

		protected string _InputMask = null;
		protected string _DisplayMask = null;
		protected DateTime? _MinValue;
		protected DateTime? _MaxValue;
		protected bool _PreserveTime;
		protected bool _UseSmallDateTime = true;
		private bool _useTimeZone = true;

		public string InputMask
		{
			get
			{
				return _InputMask;
			}
			set
			{
				_InputMask = value;
			}
		}
		public string DisplayMask
		{
			get
			{
				return _DisplayMask;
			}
			set
			{
				_DisplayMask = value;
			}
		}
		public string MinValue
		{
			get
			{
				if (_MinValue != null)
				{
					return _MinValue.ToString();
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					_MinValue = DateTime.Parse(value);
				}
				else
				{
					_MinValue = null;
				}
			}
		}
		public string MaxValue
		{
			get
			{
				if (_MaxValue != null)
				{
					return _MaxValue.ToString();
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					_MaxValue = DateTime.Parse(value);
				}
				else
				{
					_MaxValue = null;
				}
			}
		}

		/// <summary>
		/// if false, time part is removed 
		/// </summary>
		public virtual bool PreserveTime
		{
			get
			{
				return _PreserveTime;
			}
			set
			{
				_PreserveTime = value;
				if (value && _DisplayMask == null)
				{
					_DisplayMask = "g";
				}
			}
		}

		/// <summary>
		/// DataType = UseSmallDateTime ? PXDbType.SmallDateTime : PXDbType.DateTime
		/// </summary>
		public bool UseSmallDateTime
		{
			get
			{
				return this._UseSmallDateTime;
			}
			set
			{
				this._UseSmallDateTime = value;
			}
		}

		/// <summary>
		/// Convert time to UTC using locale time zone
		/// </summary>
		public virtual bool UseTimeZone
		{
			get { return _useTimeZone; }
			set { _useTimeZone = value; }
		}

		#endregion

		#region Implementation
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (_DatabaseFieldName != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName;
				//e.FieldName = (e.Table == null ? _DatabaseFieldName : e.Table.Name + '.' + _DatabaseFieldName);
			}
			e.DataType = UseSmallDateTime ? PXDbType.SmallDateTime : PXDbType.DateTime;
			if (e.Value == null) e.DataValue = null;
			else
			{
				if (UseTimeZone && _PreserveTime)
				{
					DateTime newDate = PXTimeZoneInfo.ConvertTimeToUtc((DateTime)e.Value, GetTimeZone());
					e.DataValue = newDate;
				}
				else e.DataValue = (DateTime)e.Value;
			}
			if (e.DataValue != null && UseSmallDateTime)
			{
				if ((e.DataValue as DateTime?) > this._MaxValue)
				{
					e.DataValue = this._MaxValue;
				}
				if ((e.DataValue as DateTime?) < this._MinValue)
				{
					e.DataValue = this._MinValue;
				}
			}
			e.DataLength = 4;
			e.IsRestriction = e.IsRestriction || _IsKey;
		}
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				DateTime? dt = e.Record.GetDateTime(e.Position);
				if (dt != null)
				{
					if (_PreserveTime)
					{
						if (UseTimeZone) dt = PXTimeZoneInfo.ConvertTimeFromUtc(dt.Value, GetTimeZone());
					}
					else dt = new DateTime(dt.Value.Year, dt.Value.Month, dt.Value.Day);
				}
				sender.SetValue(e.Row, _FieldOrdinal, dt);
			}
			e.Position++;
		}
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				DateTime val;
				if (DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, DateTimeStyles.None, out val))
				{
					if (!_PreserveTime) val = new DateTime(val.Year, val.Month, val.Day);
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
			else if (!_PreserveTime && e.NewValue != null)
			{
				DateTime val = (DateTime)e.NewValue;
				if (val != null) e.NewValue = new DateTime(val.Year, val.Month, val.Day);
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXDateState.CreateInstance(e.ReturnState, _FieldName, _IsKey, null, _InputMask, _DisplayMask, _MinValue, _MaxValue);
			}
		}
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (_MinValue == null)
			{
				_MinValue = _MIN_VALUE;
			}
			if (_MaxValue == null)
			{
				_MaxValue = new DateTime(2079, 6, 6);
			}
		}
		#endregion

		protected virtual PXTimeZoneInfo GetTimeZone()
		{
			return LocaleInfo.GetTimeZone();
		}
	}
	#endregion

	#region PXDBTimeAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBTimeAttribute : PXDBDateAttribute
	{
		public PXDBTimeAttribute()
		{
			base.PreserveTime = true;
		}

		public override bool PreserveTime
		{
			get
			{
				return base.PreserveTime;
			}
			set
			{
			}
		}

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			base.CommandPreparing(sender, e);

			if (e.DataValue != null) e.DataValue = _MIN_VALUE.AddTicks(((DateTime)e.DataValue).TimeOfDay.Ticks);
		}

		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			base.RowSelecting(sender, e);

			object val = sender.GetValue(e.Row, _FieldOrdinal);
			if (val != null) sender.SetValue(e.Row, _FieldOrdinal, _MIN_VALUE.AddTicks(((DateTime)val).TimeOfDay.Ticks));
		}

		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			base.FieldUpdating(sender, e);

			if (e.NewValue != null) e.NewValue = _MIN_VALUE.AddTicks(((DateTime)e.NewValue).TimeOfDay.Ticks);
		}
	}

	#endregion

	#region PXDateAttribute
	/// <summary>
	/// Indicates a DAC field of type [DateTime?] that is NOT mapped to a database column.<br/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXDateAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected string _InputMask = null;
		protected string _DisplayMask = null;
		protected DateTime? _MinValue;
		protected DateTime? _MaxValue;
		protected bool _IsKey = false;
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
		public string InputMask
		{
			get
			{
				return _InputMask;
			}
			set
			{
				_InputMask = value;
			}
		}
		public string DisplayMask
		{
			get
			{
				return _DisplayMask;
			}
			set
			{
				_DisplayMask = value;
			}
		}
		public string MinValue
		{
			get
			{
				if (_MinValue != null)
				{
					return _MinValue.ToString();
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					_MinValue = DateTime.Parse(value);
				}
				else
				{
					_MinValue = null;
				}
			}
		}
		public string MaxValue
		{
			get
			{
				if (_MaxValue != null)
				{
					return _MaxValue.ToString();
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					_MaxValue = DateTime.Parse(value);
				}
				else
				{
					_MaxValue = null;
				}
			}
		}
		#endregion

		public bool UseTimeZone { get; set; }

		#region Implementation
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				DateTime val;
				if (DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, DateTimeStyles.None, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXDateState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, _InputMask, _DisplayMask, _MinValue, _MaxValue);
			}
		}
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = _FieldName;
				e.DataType = PXDbType.DateTime;
				if (UseTimeZone)
				{
					DateTime newDate = PXTimeZoneInfo.ConvertTimeToUtc((DateTime)e.Value, LocaleInfo.GetTimeZone());
					e.DataValue = newDate;
				}
				else e.DataValue = e.Value;
				e.DataLength = 4;
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
			if (_MinValue == null)
			{
				_MinValue = new DateTime(1900, 1, 1);
			}
			if (_MaxValue == null)
			{
				_MaxValue = new DateTime(2079, 6, 6);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBDoubleAttribute
	/// <summary>
	/// 8-bytes floating point stored in the database
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBDoubleAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected int _Precision = 2;
		protected double _MinValue = double.MinValue;
		protected double _MaxValue = double.MaxValue;
		public double MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		public double MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Ctor
		public PXDBDoubleAttribute()
		{
		}
		public PXDBDoubleAttribute(int precision)
		{
			_Precision = precision;
		}
		#endregion

		#region Runtime
		public static void SetPrecision(PXCache cache, object data, string name, int precision)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXDBDoubleAttribute)
				{
					((PXDBDoubleAttribute)attr)._Precision = precision;
				}
			}
		}
		public static void SetPrecision(PXCache cache, string name, int precision)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXDBDoubleAttribute)
				{
					((PXDBDoubleAttribute)attr)._Precision = precision;
				}
			}
		}
		#endregion

		#region Implementation
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (_DatabaseFieldName != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName;
				//e.FieldName = (e.Table == null ? _DatabaseFieldName : e.Table.Name + '.' + _DatabaseFieldName);
			}
			e.DataType = PXDbType.Float;
			e.DataValue = e.Value;
			e.DataLength = 8;
			e.IsRestriction = e.IsRestriction || _IsKey;
		}
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetDouble(e.Position));
			}
			e.Position++;
		}
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				double val;
				if (double.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
			if (e.NewValue != null)
			{
				e.NewValue = Math.Round((Double)e.NewValue, _Precision);
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXDoubleState.CreateInstance(e.ReturnState, _Precision, _FieldName, _IsKey, -1, _MinValue, _MaxValue);
			}
		}
		#endregion
	}
	#endregion

	#region PXDoubleAttribute
	/// <summary>
	/// Indicates a DAC field of type [double?] that is NOT mapped to a database column.<br/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXDoubleAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected int _Precision = 2;
		protected double _MinValue = double.MinValue;
		protected double _MaxValue = double.MaxValue;
		protected bool _IsKey = false;
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
		public double MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		public double MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Ctor
		public PXDoubleAttribute()
		{
		}
		public PXDoubleAttribute(int precision)
		{
			_Precision = precision;
		}
		#endregion

		#region Runtime
		public static void SetPrecision(PXCache cache, object data, string name, int precision)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXDoubleAttribute)
				{
					((PXDoubleAttribute)attr)._Precision = precision;
				}
			}
		}
		public static void SetPrecision<Field>(PXCache cache, object data, int precision)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXDoubleAttribute)
				{
					((PXDoubleAttribute)attr)._Precision = precision;
				}
			}
		}
		public static void SetPrecision(PXCache cache, string name, int precision)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXDoubleAttribute)
				{
					((PXDoubleAttribute)attr)._Precision = precision;
				}
			}
		}
		public static void SetPrecision<Field>(PXCache cache, int precision)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXDoubleAttribute)
				{
					((PXDoubleAttribute)attr)._Precision = precision;
				}
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				double val;
				if (double.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
			if (e.NewValue != null)
			{
				e.NewValue = Math.Round((Double)e.NewValue, _Precision);
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXDoubleState.CreateInstance(e.ReturnState, _Precision, _FieldName, _IsKey, -1, _MinValue, _MaxValue);
			}
		}
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = _FieldName;
				e.DataType = PXDbType.Float;
				e.DataValue = e.Value;
				e.DataLength = 8;
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBFloatAttribute
	/// <summary>
	/// 4-bytes floating point stored in the database
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBFloatAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected int _Precision = 2;
		protected float _MinValue = float.MinValue;
		protected float _MaxValue = float.MaxValue;
		public float MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		public float MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Ctor
		public PXDBFloatAttribute()
		{
		}
		public PXDBFloatAttribute(int precision)
		{
			_Precision = precision;
		}
		#endregion

		#region Runtime
		public static void SetPrecision(PXCache cache, object data, string name, int precision)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXDBFloatAttribute)
				{
					((PXDBFloatAttribute)attr)._Precision = precision;
				}
			}
		}
		public static void SetPrecision(PXCache cache, string name, int precision)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXDBFloatAttribute)
				{
					((PXDBFloatAttribute)attr)._Precision = precision;
				}
			}
		}
		#endregion

		#region Implementation
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (_DatabaseFieldName != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName;
				//e.FieldName = (e.Table == null ? _DatabaseFieldName : e.Table.Name + '.' + _DatabaseFieldName);
			}
			e.DataType = PXDbType.Real;
			e.DataValue = e.Value;
			e.DataLength = 4;
			e.IsRestriction = e.IsRestriction || _IsKey;
		}
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetFloat(e.Position));
			}
			e.Position++;
		}
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				float val;
				if (float.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
			if (e.NewValue != null)
			{
				e.NewValue = Convert.ToSingle(Math.Round((float)e.NewValue, _Precision));
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXFloatState.CreateInstance(e.ReturnState, _Precision, _FieldName, _IsKey, -1, _MinValue, _MaxValue);
			}
		}
		#endregion
	}
	#endregion

	#region PXFloatAttribute
	/// <summary>
	/// Indicates a DAC field of type [float?] that is NOT mapped to a database column.<br/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXFloatAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected int _Precision = 2;
		protected float _MinValue = float.MinValue;
		protected float _MaxValue = float.MaxValue;
		protected bool _IsKey = false;
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
		public float MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				_MinValue = value;
			}
		}
		public float MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				_MaxValue = value;
			}
		}
		#endregion

		#region Ctor
		public PXFloatAttribute()
		{
		}
		public PXFloatAttribute(int precision)
		{
			_Precision = precision;
		}
		#endregion

		#region Runtime
		public static void SetPrecision(PXCache cache, object data, string name, int precision)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXFloatAttribute)
				{
					((PXFloatAttribute)attr)._Precision = precision;
				}
			}
		}
		public static void SetPrecision<Field>(PXCache cache, object data, int precision)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXFloatAttribute)
				{
					((PXFloatAttribute)attr)._Precision = precision;
				}
			}
		}
		public static void SetPrecision(PXCache cache, string name, int precision)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXFloatAttribute)
				{
					((PXFloatAttribute)attr)._Precision = precision;
				}
			}
		}
		public static void SetPrecision<Field>(PXCache cache, int precision)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXFloatAttribute)
				{
					((PXFloatAttribute)attr)._Precision = precision;
				}
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				float val;
				if (float.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
			if (e.NewValue != null)
			{
				e.NewValue = Convert.ToSingle(Math.Round((float)e.NewValue, _Precision));
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXFloatState.CreateInstance(e.ReturnState, _Precision, _FieldName, _IsKey, -1, _MinValue, _MaxValue);
			}
		}
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = _FieldName;
				e.DataType = PXDbType.Real;
				e.DataValue = e.Value;
				e.DataLength = 4;
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBDecimalAttribute
	/// <summary>
	/// Because of this attribute [Decimal?] field is mapped to a database [Decimal] 16 byte column.<br/>
	/// Allows to specify MinValue, MaxValue, Precision<br/>
	/// Precision can be calculated at runtime with Bql query<br/>
	/// Default precision: 2<br/>
	/// </summary>
	/// <example>
	///[PXDBDecimal(6, MinValue = 0, MaxValue=100)]
	///[PXDBDecimal(typeof(Search&lt;Currency.decimalPlaces, Where&lt;Currency.curyID, Equal&lt;Current&lt;POCreateFilter.vendorID>>>>))]
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBDecimalAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXRowPersistingSubscriber
	{
		#region State

		public class DBDecimalProperties
		{
			public int? _scale;
			public int? _precision;
			public decimal? _maxValue;

			public int? Scale
			{
				get
				{
					_sync.AcquireReaderLock(-1);
					try
					{
						return _scale;
					}
					finally 
					{
						_sync.ReleaseReaderLock();
					}
				}
			}

			public int? Precision
			{
				get
				{
					_sync.AcquireReaderLock(-1);
					try
					{
						return _precision;
					}
					finally
					{
						_sync.ReleaseReaderLock();
					}
				}
			}

			public decimal? MaxValue
			{
				get
				{
					_sync.AcquireReaderLock(-1);
					try
					{
						return _maxValue;
					}
					finally
					{
						_sync.ReleaseReaderLock();
					}
				}
			}


			private ReaderWriterLock _sync = new ReaderWriterLock();

			public void Fill(Type table, string field)
			{
				_sync.AcquireReaderLock(-1);
				try
				{
					if (_scale != null && _precision != null && _maxValue != null)
						return;

					var lc = _sync.UpgradeToWriterLock(-1);
					try
					{
						if (_scale != null && _precision != null && _maxValue != null)
							return;

						var tableHeader = PXDatabase.Provider.GetTableStructure(table.Name);
						if (tableHeader != null)
						{
							var column = tableHeader
								.Columns.FirstOrDefault(c => string.Equals(c.Name, field, StringComparison.OrdinalIgnoreCase));
							if (column != null)
							{
								_scale = column.Scale;
								_precision = column.Precision;
								_maxValue = (decimal?)Math.Pow(10, (double)(column.Precision - column.Scale));
							}
							else
							{
								_scale = 29;
								_precision = 28;
								_maxValue = decimal.MaxValue;
							}
						}
					}
					finally
					{
						_sync.DowngradeFromWriterLock(ref lc);
					}
				}
				finally
				{
					_sync.ReleaseReaderLock();
				}
			}

			public bool IsSet
			{
				get
				{
					_sync.AcquireReaderLock(-1);
					try
					{
						return _scale != null && _precision != null && _maxValue != null;
					}
					finally
					{
						_sync.ReleaseReaderLock();
					}
				}
			}
		}

		// because attributes instantiating by copying there will be only one instance for all instances of field.
		public DBDecimalProperties DBProperties { get; private set; }

		protected int? _Precision = 2;
		protected decimal _MinValue = decimal.MinValue;
		protected decimal _MaxValue = decimal.MaxValue;
		protected Type _Type;
		protected BqlCommand _Select;
		public double MinValue
		{
			get
			{
				return (double)_MinValue;
			}
			set
			{
				_MinValue = (decimal)value;
			}
		}
		public double MaxValue
		{
			get
			{
				return (double)_MaxValue;
			}
			set
			{
				_MaxValue = (decimal)value;
			}
		}
		#endregion

		#region Ctor
		public PXDBDecimalAttribute()
		{
			DBProperties = new DBDecimalProperties();
		}
		public PXDBDecimalAttribute(int precision)
			: this()
		{
			_Precision = precision;
		}

		/// <param name="type">
		/// Bql query for precision, IBqlSearch or IBqlField
		/// </param>
		public PXDBDecimalAttribute(Type type)
			: this()
		{
			if (type == null)
			{
				throw new PXArgumentException("type", ErrorMessages.ArgumentNullException);
			}
			if (typeof(IBqlSearch).IsAssignableFrom(type))
			{
				_Select = BqlCommand.CreateInstance(type);
				_Type = BqlCommand.GetItemType(((IBqlSearch)_Select).GetField());
			}
			else if (type.IsNested && typeof(IBqlField).IsAssignableFrom(type))
			{
				_Type = BqlCommand.GetItemType(type);
				_Select = BqlCommand.CreateInstance(typeof(Search<>), type);
			}
			else
			{
				throw new PXArgumentException("type", ErrorMessages.CantCreateForeignKeyReference, type);
			}
		}

		#endregion

		#region Runtime

		public static void SetPrecision(PXCache cache, object data, string name, int? precision)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXDBDecimalAttribute)
				{
					((PXDBDecimalAttribute)attr)._Precision = precision;
				}
			}
		}
		public static void SetPrecision(PXCache cache, string name, int? precision)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXDBDecimalAttribute)
				{
					((PXDBDecimalAttribute)attr)._Precision = precision;
				}
			}
		}
		#endregion

		#region Implementation

		protected string Check(object value)
		{
			if (value is decimal)
			{
				decimal val = Normalize((decimal)value);

				if (!DBProperties.IsSet)
				{
					DBProperties.Fill(_BqlTable, _DatabaseFieldName);
				}
				// if can`t read properties - ignoring check.
				if (DBProperties.IsSet)
				{
					if (Math.Abs(val) >= DBProperties.MaxValue)
					{
						return PXMessages.LocalizeFormat(ErrorMessages.InvalidDecimalValue, _FieldName);
					}
				}
			}

			return null;
		}

		protected decimal Normalize(decimal value)
		{
			return decimal.Round(value, 28);
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object val = sender.GetValue(e.Row, _FieldOrdinal);
			string error = Check(val);
			if (error != null)
			{
				if (sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyKeepPreviousException(error)))
				{
					throw new PXRowPersistingException(_FieldName, null, error);
				}
			}
		}

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (_DatabaseFieldName != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName;
				//e.FieldName = (e.Table == null ? _DatabaseFieldName : e.Table.Name + '.' + _DatabaseFieldName);
			}

			e.DataType = PXDbType.Decimal;
			e.DataValue = e.Value;
			e.DataLength = 16;
			e.IsRestriction = e.IsRestriction || _IsKey;
		}

		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetDecimal(e.Position));
			}
			e.Position++;
		}
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				decimal val;
				if (decimal.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
			if (e.NewValue != null)
			{
				_ensurePrecision(sender, e.Row);
				if (_Precision != null)
				{
					e.NewValue = Math.Round((decimal)e.NewValue, (int)_Precision, MidpointRounding.AwayFromZero);
				}
				string error = Check(e.NewValue);
				if (error != null)
				{
					throw new PXSetPropertyException(error);
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				_ensurePrecision(sender, e.Row);
				e.ReturnState = PXDecimalState.CreateInstance(e.ReturnState, _Precision, _FieldName, _IsKey, -1, _MinValue, _MaxValue);
			}
		}
		protected void _ensurePrecision(PXCache sender, object row)
		{
			if (_Type != null)
			{
				PXView view = sender.Graph.TypedViews.GetView(_Select, true);
				object item = null;
				try
				{
					List<object> list = view.SelectMultiBound(new object[] { row });
					if (list.Count > 0) item = list[0];
				}
				catch
				{
				}
				if (item != null)
				{
					int? prec = GetItemPrecision(view, item);
					if (prec != null)
						_Precision = prec;
				}
			}
		}

		public static void EnsurePrecision(PXCache cache)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				PXDBDecimalAttribute decattr = attr as PXDBDecimalAttribute;
				if (decattr != null && decattr.AttributeLevel == PXAttributeLevel.Cache)
				{
					int? oldValue = decattr._Precision;
					try
					{
						decattr._Precision = null;
						decattr._ensurePrecision(cache, null);
						oldValue = (int)decattr._Precision;

						cache.SetAltered(decattr._FieldName, true);
						decattr._Type = null;
					}
					catch (InvalidOperationException) { }
					finally
					{
						decattr._Precision = oldValue;
					}
				}
			}
		}
		#endregion

		protected virtual int? GetItemPrecision(PXView view, object item)
		{
			if (item is PXResult) item = ((PXResult)item)[0];
			return item != null ? (short?)view.Cache.GetValue(item, ((IBqlSearch)_Select).GetField().Name) : null;
		}

	}

	#endregion
	#region PXDecimalAttribute
	/// <summary>
	/// Indicates a DAC field of type [decimal?] that is NOT mapped to a database column.<br/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXFieldState))]
	public class PXDecimalAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXCommandPreparingSubscriber
	{
		#region State
		protected int? _Precision = 2;
		protected decimal _MinValue = decimal.MinValue;
		protected decimal _MaxValue = decimal.MaxValue;
		protected bool _IsKey = false;
		protected Type _Type;
		protected BqlCommand _Select;
		public virtual bool IsKey
		{
			get
			{
				return _IsKey;
			}
			set
			{
				_IsKey = value;
			}
		}
		public double MinValue
		{
			get
			{
				return (double)_MinValue;
			}
			set
			{
				_MinValue = (decimal)value;
			}
		}
		public double MaxValue
		{
			get
			{
				return (double)_MaxValue;
			}
			set
			{
				_MaxValue = (decimal)value;
			}
		}
		#endregion

		#region Ctor
		public PXDecimalAttribute()
		{
		}
		public PXDecimalAttribute(int precision)
		{
			_Precision = precision;
		}
		public PXDecimalAttribute(Type type)
		{
			if (type == null)
			{
				throw new PXArgumentException("type", ErrorMessages.ArgumentNullException);
			}
			if (typeof(IBqlSearch).IsAssignableFrom(type))
			{
				_Select = BqlCommand.CreateInstance(type);
				_Type = BqlCommand.GetItemType(((IBqlSearch)_Select).GetField());
			}
			else if (type.IsNested && typeof(IBqlField).IsAssignableFrom(type))
			{
				_Type = type.DeclaringType;
				_Select = BqlCommand.CreateInstance(typeof(Search<>), type);
			}
			else
			{
				throw new PXArgumentException("type", ErrorMessages.CantCreateForeignKeyReference, type);
			}
		}
		#endregion

		#region Runtime
		public static void SetPrecision(PXCache cache, object data, string name, int? precision)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXDecimalAttribute)
				{
					((PXDecimalAttribute)attr)._Precision = precision;
				}
			}
		}
		public static void SetPrecision<Field>(PXCache cache, object data, int? precision)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXDecimalAttribute)
				{
					((PXDecimalAttribute)attr)._Precision = precision;
				}
			}
		}
		public static void SetPrecision(PXCache cache, string name, int? precision)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXDecimalAttribute)
				{
					((PXDecimalAttribute)attr)._Precision = precision;
				}
			}
		}
		public static void SetPrecision<Field>(PXCache cache, int? precision)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXDecimalAttribute)
				{
					((PXDecimalAttribute)attr)._Precision = precision;
				}
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				decimal val;
				if (decimal.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
			if (e.NewValue != null)
			{
				_ensurePrecision(sender, e.Row);
				if (_Precision != null)
				{
					e.NewValue = Math.Round((decimal)e.NewValue, (int)_Precision, MidpointRounding.AwayFromZero);
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				_ensurePrecision(sender, e.Row);
				e.ReturnState = PXDecimalState.CreateInstance(e.ReturnState, _Precision, _FieldName, _IsKey, -1, _MinValue, _MaxValue);
			}
		}
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && e.Value != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = _FieldName;
				e.DataType = PXDbType.Decimal;
				e.DataValue = e.Value;
				e.DataLength = 16;
			}
		}
		protected void _ensurePrecision(PXCache sender, object row)
		{
			if (_Type != null)
			{
				PXView view = sender.Graph.TypedViews.GetView(_Select, true);
				object item = null;
				try
				{
					List<object> list = view.SelectMultiBound(new object[] { row });
					if (list.Count > 0) item = list[0];
				}
				catch
				{
				}
				if (item != null)
				{
					int? prec = GetItemPrecision(view, item);
					if (prec != null)
						_Precision = prec;
				}
			}
		}

		protected virtual int? GetItemPrecision(PXView view, object item)
		{
			if (item is PXResult) item = ((PXResult)item)[0];
			return item != null ? (short?)view.Cache.GetValue(item, ((IBqlSearch)_Select).GetField().Name) : null;
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (IsKey)
			{
				sender.Keys.Add(_FieldName);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBTimestampAttribute
	/// <summary>
	/// 8-bytes timestamp stored in the database
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBTimestampAttribute : PXDBFieldAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXRowPersistedSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected bool _RecordComesFirst;
		public virtual bool RecordComesFirst
		{
			get
			{
				return _RecordComesFirst;
			}
			set
			{
				_RecordComesFirst = value;
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				try
				{
					e.NewValue = Convert.FromBase64String((string)e.NewValue);
				}
				catch
				{
				}
			}
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.ReturnValue is byte[])
			{
				e.ReturnValue = Convert.ToBase64String((byte[])e.ReturnValue);
			}
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, false, _FieldName, false, null, null, null, null, null, null);
				((PXFieldState)e.ReturnState).Visible = false;
				((PXFieldState)e.ReturnState).Enabled = false;
				((PXFieldState)e.ReturnState).Visibility = PXUIVisibility.Invisible;
			}
		}

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Insert && (e.Operation & PXDBOperation.Option) != PXDBOperation.Second)
			{
				object value = null;
				bool datachange = ((e.Operation & PXDBOperation.Command) != PXDBOperation.Select);
				if (_RecordComesFirst)
				{
					value = e.Value;
					if (value == null && datachange)
					{
						value = sender.Graph.TimeStamp;
					}
				}
				else
				{
					if (datachange)
					{
						byte[] gstamp = sender.Graph.TimeStamp;
						value = gstamp;
						byte[] tstamp = null;
						object[] persisted = PXTimeStampScope.GetPersisted(sender, e.Row);
						if (persisted != null && persisted.Length > 0)
						{
							tstamp = persisted[0] as byte[];
						}
						if (tstamp != null)
						{
							if (gstamp != null)
							{
								// Get the maximum timestamp (that is the newest one)
								for (int i = 0; i < tstamp.Length && i < gstamp.Length; i++)
								{
									if (tstamp[i] == gstamp[i])
										continue;
									value = tstamp[i] > gstamp[i] ? tstamp : gstamp;
									break;
								}
							}
							else
							{
								value = tstamp;
							}
						}
						else if (gstamp != null && sender.Graph.PrimaryItemType != null && e.Row != null && sender.Graph.PrimaryItemType.IsAssignableFrom(e.Row.GetType()))
						{
							TimeSpan ts;
							Exception msg;
							if (PXLongOperation.GetStatus(sender.Graph.UID, out ts, out msg) == PXLongRunStatus.Completed)
							{
								tstamp = sender.GetValue(e.Row, _FieldOrdinal) as byte[];
								if (tstamp != null)
								{
									// Get the maximum timestamp (that is the newest one)
									for (int i = 0; i < tstamp.Length && i < gstamp.Length; i++)
									{
										if (tstamp[i] == gstamp[i])
											continue;
										if (tstamp[i] > gstamp[i])
										{
											value = sender.Graph.TimeStamp = tstamp;
											break;
										}
									}
								}
							}
						}
					}
					if (value == null)
					{
						if (e.Value is string)
						{
							try
							{
								value = Convert.FromBase64String((string)e.Value);
							}
							catch
							{
							}
						}
						else
						{
							value = e.Value;
						}
						//if (datachange
						//    && value is byte[]
						//    && (e.Operation & PXDBOperation.Command) == PXDBOperation.Update
						//    || (e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
						//{
						//    sender.Graph.TimeStamp = (byte[])value;
						//}
					}
				}
				if (value != null || !datachange)
				{
					if (_DatabaseFieldName != null)
					{
						e.BqlTable = _BqlTable;
						e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName;
						//e.FieldName = (e.Table == null ? _DatabaseFieldName : e.Table.Name + '.' + _DatabaseFieldName);
					}
					e.DataType = PXDbType.Timestamp;
					e.DataValue = value;
					e.DataLength = 8;
					e.IsRestriction = e.IsRestriction || datachange;
				}
				else
				{
					throw new PXCommandPreparingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
				}
			}
		}
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetTimeStamp(e.Position));
			}
			e.Position++;
		}
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			byte[] gstamp;
			if (e.TranStatus == PXTranStatus.Completed
				&& (e.Operation & PXDBOperation.Command) != PXDBOperation.Delete
				&& (gstamp = sender.Graph.TimeStamp) != null)
			{
				byte[] tstamp = sender.GetValue(e.Row, _FieldOrdinal) as byte[];
				if (tstamp == null)
				{
					sender.SetValue(e.Row, _FieldOrdinal, gstamp);
					PXTimeStampScope.PutPersisted(sender, e.Row, gstamp);
				}
				else
				{
					bool isSet = false;
					for (int i = 0; i < tstamp.Length && i < gstamp.Length; i++)
					{
						if (gstamp[i] > tstamp[i])
						{
							sender.SetValue(e.Row, _FieldOrdinal, gstamp);
							PXTimeStampScope.PutPersisted(sender, e.Row, gstamp);
							isSet = true;
							break;
						}
					}
					if (!isSet)
					{
						PXTimeStampScope.PutPersisted(sender, e.Row, tstamp);
					}
				}
			}
		}
		#endregion
	}
	#endregion

	#region PXDBIdentityAttribute
	/// <summary>
	/// 4-bytes auto increment integer stored in the database
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBIdentityAttribute : PXDBFieldAttribute, IPXFieldDefaultingSubscriber, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXRowPersistedSubscriber, IPXFieldVerifyingSubscriber
	{
		#region State
		protected int? _KeyToAbort;
		protected class LastDefault
		{
			public int Value;
			public List<object> Rows = new List<object>();
		}
		protected LastDefault _MaximumDefaultValue;
		#endregion

		#region Implementation
		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && _MaximumDefaultValue.Value < 0)
			{
				foreach (object row in _MaximumDefaultValue.Rows)
				{
					if (sender.Locate(row) != null)
					{
						_MaximumDefaultValue.Rows.Clear();
						_MaximumDefaultValue.Value++;
						break;
					}
				}
				e.NewValue = _MaximumDefaultValue.Value;
				_MaximumDefaultValue.Rows.Add(e.Row);
			}
			else
			{
				int newId = int.MinValue;
				foreach (object data in sender.Cached)
				{
					object val = sender.GetValue(data, _FieldOrdinal);
					if (val != null)
					{
						if ((int)val > 0)
						{
							foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(data, _FieldName))
							{
								if (attr is PXDBIdentityAttribute)
								{
									if (((PXDBIdentityAttribute)attr)._KeyToAbort < 0 && ((PXDBIdentityAttribute)attr)._KeyToAbort > newId)
									{
										newId = (int)((PXDBIdentityAttribute)attr)._KeyToAbort;
									}
									break;
								}
							}
						}
						else if ((int)val > newId)
						{
							newId = (int)val;
						}
					}
				}
				newId++;
				e.NewValue = newId;
				if (e.Row != null)
				{
					_MaximumDefaultValue.Value = newId;
					_MaximumDefaultValue.Rows.Add(e.Row);
				}
			}
			e.Cancel = true;
		}
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Insert
				&& ((e.Operation & PXDBOperation.Option) != PXDBOperation.Second || _IsKey || e.IsRestriction)
				&& ((e.Operation & PXDBOperation.Command) != PXDBOperation.Update || e.Value is int && ((int)e.Value) >= 0
				|| (e.Operation & PXDBOperation.Option) == PXDBOperation.Second))
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
				e.IsRestriction = true;
			}
		}
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetInt32(e.Position));
			}
			e.Position++;
		}
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				int val;
				if (int.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, null, null, null, null, typeof(int), null);
			}
		}

		protected virtual int getLastInsertedIdentity()
		{
			return Convert.ToInt32(PXDatabase.SelectIdentity(_BqlTable, _FieldName));
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				if (e.TranStatus == PXTranStatus.Open)
				{
					if (_KeyToAbort == null)
						_KeyToAbort = (int?)sender.GetValue(e.Row, _FieldOrdinal);
					if (_KeyToAbort < 0)
					{
						int? id = getLastInsertedIdentity();
						if (id.Value == 0m)
						{
							PXDataField[] pars = new PXDataField[sender.Keys.Count + 1];
							pars[0] = new PXDataField(_DatabaseFieldName);
							for (int i = 0; i < sender.Keys.Count; i++)
							{
								string name = sender.Keys[i];
								PXCommandPreparingEventArgs.FieldDescription description = null;
								sender.RaiseCommandPreparing(name, e.Row, sender.GetValue(e.Row, name), PXDBOperation.Select, _BqlTable, out description);
								if (description != null && !String.IsNullOrEmpty(description.FieldName) && description.IsRestriction)
								{
									pars[i + 1] = new PXDataFieldValue(description.FieldName, description.DataType, description.DataLength, description.DataValue);
								}
							}
							using (PXDataRecord record = PXDatabase.SelectSingle(_BqlTable, pars))
							{
								if (record != null)
								{
									id = record.GetInt32(0);
								}
							}
						}
						sender.SetValue(e.Row, _FieldOrdinal, id);
					}
					else
					{
						_KeyToAbort = null;
					}
				}
				else if (e.TranStatus == PXTranStatus.Aborted && _KeyToAbort != null)
				{
					sender.SetValue(e.Row, _FieldOrdinal, _KeyToAbort);
					_KeyToAbort = null;
				}
			}
		}
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.ExternalCall)
			{
				int? oldValue = (int?)sender.GetValue(e.Row, _FieldOrdinal);

				if (oldValue != null)
				{
					e.NewValue = oldValue;
				}
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			_MaximumDefaultValue = new LastDefault();
			sender._Identity = _FieldName;
			base.CacheAttached(sender);
		}
		#endregion
	}
	#endregion

	#region PXDBLongIdentityAttribute
	/// <summary>
	/// 8-bytes auto increment big integer stored in the database
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBLongIdentityAttribute : PXDBFieldAttribute, IPXFieldDefaultingSubscriber, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber, IPXRowPersistedSubscriber, IPXFieldVerifyingSubscriber
	{
		#region State
		protected long? _KeyToAbort;
		protected class LastDefault
		{
			public long Value;
			public List<object> Rows = new List<object>();
		}
		protected LastDefault _MaximumDefaultValue;
		#endregion

		#region Implementation
		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && _MaximumDefaultValue.Value < 0)
			{
				foreach (object row in _MaximumDefaultValue.Rows)
				{
					if (sender.Locate(row) != null)
					{
						_MaximumDefaultValue.Rows.Clear();
						_MaximumDefaultValue.Value++;
						break;
					}
				}
				e.NewValue = _MaximumDefaultValue.Value;
				_MaximumDefaultValue.Rows.Add(e.Row);
			}
			else
			{
				long newId = int.MinValue;
				foreach (object data in sender.Cached)
				{
					object val = sender.GetValue(data, _FieldOrdinal);
					if (val != null)
					{
						if ((long)val > 0)
						{
							foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(data, _FieldName))
							{
								if (attr is PXDBLongIdentityAttribute)
								{
									if (((PXDBLongIdentityAttribute)attr)._KeyToAbort < 0 && ((PXDBLongIdentityAttribute)attr)._KeyToAbort > newId)
									{
										newId = (long)((PXDBLongIdentityAttribute)attr)._KeyToAbort;
									}
									break;
								}
							}
						}
						else if ((long)val > newId)
						{
							newId = (long)val;
						}
					}
				}
				newId++;
				e.NewValue = newId;
				if (e.Row != null)
				{
					_MaximumDefaultValue.Value = newId;
					_MaximumDefaultValue.Rows.Add(e.Row);
				}
			}
		}
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Insert && ((e.Operation & PXDBOperation.Option) != PXDBOperation.Second || _IsKey || e.IsRestriction))
			{
				if (_DatabaseFieldName != null)
				{
					e.BqlTable = _BqlTable;
					e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName;
					//e.FieldName = (e.Table == null ? _DatabaseFieldName : e.Table.Name + '.' + _DatabaseFieldName);
				}
				e.DataType = PXDbType.BigInt;
				e.DataValue = e.Value;
				e.DataLength = 8;
				e.IsRestriction = true;
			}
			e.Cancel = true;
		}
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetInt64(e.Position));
			}
			e.Position++;
		}
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				long val;
				if (long.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXLongState.CreateInstance(e.ReturnState, _FieldName, _IsKey, -1, null, null, typeof(long));
			}
		}
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				if (e.TranStatus == PXTranStatus.Open)
				{
					_KeyToAbort = (long?)sender.GetValue(e.Row, _FieldOrdinal);
					if (_KeyToAbort < 0)
					{
						long? id = Convert.ToInt64(PXDatabase.SelectIdentity(_BqlTable, _FieldName));
						if ((id ?? 0m) == 0m)
						{
							PXDataField[] pars = new PXDataField[sender.Keys.Count + 1];
							pars[0] = new PXDataField(_DatabaseFieldName);
							for (int i = 0; i < sender.Keys.Count; i++)
							{
								string name = sender.Keys[i];
								PXCommandPreparingEventArgs.FieldDescription description = null;
								sender.RaiseCommandPreparing(name, e.Row, sender.GetValue(e.Row, name), PXDBOperation.Select, _BqlTable, out description);
								if (description != null && !String.IsNullOrEmpty(description.FieldName) && description.IsRestriction)
								{
									pars[i + 1] = new PXDataFieldValue(description.FieldName, description.DataType, description.DataLength, description.DataValue);
								}
							}
							using (PXDataRecord record = PXDatabase.SelectSingle(_BqlTable, pars))
							{
								if (record != null)
								{
									id = record.GetInt64(0);
								}
							}
						}
						sender.SetValue(e.Row, _FieldOrdinal, id);
					}
					else
					{
						_KeyToAbort = null;
					}
				}
				else if (e.TranStatus == PXTranStatus.Aborted && _KeyToAbort != null)
				{
					sender.SetValue(e.Row, _FieldOrdinal, _KeyToAbort);
					_KeyToAbort = null;
				}
			}
		}
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.ExternalCall)
			{
				long? oldValue = (long?)sender.GetValue(e.Row, _FieldOrdinal);

				if (oldValue != null)
				{
					e.NewValue = oldValue;
				}
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			_MaximumDefaultValue = new LastDefault();
			sender._Identity = _FieldName;
			base.CacheAttached(sender);
		}
		#endregion
	}
	#endregion

	#region PXStringListAttribute
	/// <summary>
	/// This attribute specifies a list of allowed values for DAC field.
	/// User can select specified values from combobox in user interface.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXBaseListAttribute))]
	public class PXStringListAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber, IPXLocalizableList, IPXCommandPreparingSubscriber
	{
		#region State
		protected string[] _AllowedValues;
		protected string[] _AllowedLabels;
		protected string[] _NeutralAllowedLabels;
		protected bool _ExclusiveValues = true;
		protected string _DatabaseFieldName;
		protected int _ExplicitCnt;
		public virtual bool IsLocalizable { get; set; }
		public virtual bool MultiSelect { get; set; }
		public bool ExclusiveValues
		{
			get
			{
				return _ExclusiveValues;
			}
			set
			{
				_ExclusiveValues = value;
			}
		}
		public virtual Type BqlField
		{
			get
			{
				return null;
			}
			set
			{
				_DatabaseFieldName = char.ToUpper(value.Name[0]) + value.Name.Substring(1);
				if (value.IsNested
					//&& typeof(IBqlTable).IsAssignableFrom(value.DeclaringType)
					)
				{
					BqlTable = BqlCommand.GetItemType(value);
				}
			}
		}
		public Dictionary<string, string> ValueLabelDic
		{
			get
			{
				Dictionary<string, string> result = new Dictionary<string, string>(_AllowedValues.Length);
				for (int index = 0; index < _AllowedValues.Length; index++)
					result.Add(_AllowedValues[index], _AllowedLabels[index]);
				return result;
			}
		}
		#endregion

		#region Ctor
		public PXStringListAttribute()
		{
			IsLocalizable = true;
			_AllowedValues = new string[] { null };
			_AllowedLabels = new string[] { "" };

			CreateNeutralLabels();
		}
		public PXStringListAttribute(string[] allowedValues, string[] allowedLabels)
		{
			CheckValuesAndLabels(allowedValues, allowedLabels);
			IsLocalizable = true;
			_AllowedValues = allowedValues;
			_AllowedLabels = allowedLabels;
			CreateNeutralLabels();
		}
		public PXStringListAttribute(string list)
		{
			IsLocalizable = true;
			string[] items = list.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			_AllowedValues = new string[items.Length];
			_AllowedLabels = new string[items.Length];
			for (int i = 0; i < items.Length; i++)
			{
				int pos = items[i].IndexOf(';');
				if (pos >= 0)
				{
					_AllowedValues[i] = items[i].Substring(0, pos);
					if (pos + 1 < items[i].Length)
					{
						_AllowedLabels[i] = items[i].Substring(pos + 1);
					}
					else
					{
						_AllowedLabels[i] = _AllowedValues[i];
					}
				}
				else
				{
					_AllowedValues[i] = items[i];
					_AllowedLabels[i] = items[i];
				}
			}
			CreateNeutralLabels();
		}
		#endregion

		#region Runtime
		private static void CheckValuesAndLabels(string[] values, string [] labels)
		{
			if ((values == null && labels != null) ||
				(values != null && labels == null) ||
				(values != null && labels != null && values.Length != labels.Length))
			{
				throw new PXArgumentException("allowedLabels", ErrorMessages.IncorrectValueArrayLength);
			}
		}

		public static void SetList<Field>(PXCache cache, object data, PXStringListAttribute listSource)
				where Field : IBqlField
		{
			SetList<Field>(cache, data, listSource._AllowedValues, listSource._AllowedLabels);
		}

		public static void SetList<Field>(PXCache cache, object data, string[] allowedValues, string[] allowedLabels)
			where Field : IBqlField
		{
			CheckValuesAndLabels(allowedValues, allowedLabels);

			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}

			IEnumerable<PXStringListAttribute> attributes = cache.GetAttributes<Field>(data).OfType<PXStringListAttribute>();
			SetListInternal(attributes, allowedValues, allowedLabels);
		}

		public static void SetList(PXCache cache, object data, string field, string[] allowedValues, string[] allowedLabels)
		{
			CheckValuesAndLabels(allowedValues, allowedLabels);

			if (data == null)
			{
				cache.SetAltered(field, true);
			}

			IEnumerable<PXStringListAttribute> attributes = cache.GetAttributes(data, field).OfType<PXStringListAttribute>();
			SetListInternal(attributes, allowedValues, allowedLabels);
		}

		public static void SetList(PXCache cache, object data, string field, PXStringListAttribute listSource)
		{
			SetList(cache, data, field, listSource._AllowedValues, listSource._AllowedLabels);
		}

		private static void SetListInternal(IEnumerable<PXStringListAttribute> attributes, string[] allowedValues, string[] allowedLabels)
		{
			foreach (PXStringListAttribute attr in attributes)
			{
				attr._AllowedValues = allowedValues;
				attr._AllowedLabels = allowedLabels;
				attr._NeutralAllowedLabels = null;
				attr.CreateNeutralLabels();

				if (attr._AllowedLabels != null)
				{
					for (int i = 0; i < attr._AllowedLabels.Length; i++)
					{
						attr._AllowedLabels[i] = PXMessages.LocalizeNoPrefix(attr._NeutralAllowedLabels[i]);
					}
				}
			}
		}

		public static void AppendList<Field>(PXCache cache, object data, string[] allowedValues, string[] allowedLabels)
			where Field : IBqlField
		{
			CheckValuesAndLabels(allowedValues, allowedLabels);

			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}

			IEnumerable<PXStringListAttribute> attributes = cache.GetAttributes<Field>(data).OfType<PXStringListAttribute>();
			AppendListInternal(attributes, allowedValues, allowedLabels);
		}

		public static void AppendList(PXCache cache, object data, string field, string[] allowedValues, string[] allowedLabels)
		{
			CheckValuesAndLabels(allowedValues, allowedLabels);

			if (data == null)
			{
				cache.SetAltered(field, true);
			}

			IEnumerable<PXStringListAttribute> attributes = cache.GetAttributes(data, field).OfType<PXStringListAttribute>();
			AppendListInternal(attributes, allowedValues, allowedLabels);
		}

		private static void AppendListInternal(IEnumerable<PXStringListAttribute> attributes, string[] allowedValues, string[] allowedLabels)
		{
			foreach (PXStringListAttribute attr in attributes)
			{
				if (allowedValues == null)
				{
					attr._AllowedValues = null;
					attr._AllowedLabels = null;
					attr._NeutralAllowedLabels = null;
				}
				else
				{
					if (attr._AllowedValues == null)
					{
						attr._AllowedValues = allowedValues;
						attr._AllowedLabels = allowedLabels;
						attr._NeutralAllowedLabels = null;
						attr.CreateNeutralLabels();
					}
					else
					{
						int destIndex = attr._AllowedValues.Length;

						Array.Resize(ref attr._AllowedValues, attr._AllowedValues.Length + allowedValues.Length);
						Array.Copy(allowedValues, 0, attr._AllowedValues, destIndex, allowedValues.Length);

						Array.Resize(ref attr._AllowedLabels, attr._AllowedValues.Length);
						Array.Copy(allowedLabels, 0, attr._AllowedLabels, destIndex, allowedLabels.Length);

						Array.Resize(ref attr._NeutralAllowedLabels, attr._AllowedValues.Length);
						Array.Copy(allowedLabels, 0, attr._NeutralAllowedLabels, destIndex, allowedLabels.Length);
					}

					for (int i = 0; i < attr._AllowedLabels.Length; i++)
					{
						attr._AllowedLabels[i] = PXMessages.LocalizeNoPrefix(attr._NeutralAllowedLabels[i]);
					}
				}
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				RemoveDisabledValues(sender.GetType().GetGenericArguments()[0].FullName);
				string[] values = _AllowedValues;
				if (_ExplicitCnt > 0 && sender.Graph.AutomationView == null && !PXGraph.ProxyIsActive && sender.Graph.GetType() != typeof(PXGraph))
				{
					Array.Resize<string>(ref values, values.Length - _ExplicitCnt);
				}
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, -1, null, values, _AllowedLabels, _ExclusiveValues, null);
				((PXStringState)e.ReturnState).MultiSelect = MultiSelect;
			}
		}

		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.External) == PXDBOperation.External)
			{
				e.Cancel = true;
				StringBuilder sb = new StringBuilder();
				sb.Append(" CASE ");
				for (int i = 0; i < _AllowedValues.Length; i++)
				{
					sb.AppendFormat(" WHEN {0}.{1} = '{2}' THEN '{3}' ", e.Table.Name, _FieldName, _AllowedValues[i], _AllowedLabels[i]);
				}
				sb.Append(" END ");
				e.FieldName = sb.ToString();
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);			
			if (_DatabaseFieldName == null)
			{
				_DatabaseFieldName = _FieldName;
			}
			_ExplicitCnt = PXAutomation.AppendCombos(BqlTable, _DatabaseFieldName, ref _AllowedValues, ref _AllowedLabels);
			AppendNeutral();
			TryLocalize(sender);
		}
		#endregion

		internal void RemoveDisabledValues(string cacheName)
		{
			if (_AllowedLabels != null && _AllowedValues != null && _BqlTable != null && _DatabaseFieldName != null)
			{
				var pairs =
					_AllowedValues.Zip(_AllowedLabels, (v, l) => new Tuple<string, string>(v, l))
					.Where(t => !PXAccess.IsStringListValueDisabled(cacheName, _DatabaseFieldName, t.Item1));
				_AllowedValues = pairs.Select(p => p.Item1).ToArray();
				_AllowedLabels = pairs.Select(p => p.Item2).ToArray();
			}
		}

		private void AppendNeutral()
		{
			if (_NeutralAllowedLabels != null && _AllowedLabels != null && _NeutralAllowedLabels.Length != _AllowedLabels.Length)
			{
				_NeutralAllowedLabels = _AllowedLabels;
			}
		}

		protected void TryLocalize(PXCache sender)
		{
			if (IsLocalizable)
			{
				if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Items.Contains(PXControlPropertiesCollector.COLLECTION_RESOURCES_KEY))
				{
					PXPageRipper.RipList(this, sender, _NeutralAllowedLabels);
				}
				else
				{
					PXLocalizerRepository.ListLocalizer.Localize(this, sender, _NeutralAllowedLabels, _AllowedLabels);
				}
			}
		}

		private void CreateNeutralLabels()
		{
			if (_AllowedLabels != null && _AllowedLabels.Length > 0 && _NeutralAllowedLabels == null)
			{
				_NeutralAllowedLabels = new string[_AllowedLabels.Length];
				_AllowedLabels.CopyTo(_NeutralAllowedLabels, 0);
			}
		}
	}
	#endregion

	#region PXImagesListAttribute
	public class PXImagesListAttribute : PXStringListAttribute
	{
		#region State
		protected string[] _AllowedImages;
		public override bool IsLocalizable { get { return true; } }
		#endregion

		#region Ctor
		public PXImagesListAttribute()
		{
		}
		public PXImagesListAttribute(string[] allowedValues, string[] allowedLabels, string[] allowedImages)
			: base(allowedValues, allowedLabels)
		{
			if (allowedValues.Length != allowedImages.Length)
				throw new PXArgumentException("allowedImages", ErrorMessages.IncorrectValueArrayLength);
			_AllowedImages = allowedImages;
		}
		#endregion

		#region Implementation
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			PXStringState state = e.ReturnState as PXStringState;
			if (state != null && _AllowedImages != null) state.AllowedImages = _AllowedImages;
		}
		#endregion
	}
	#endregion

	#region PXAutomationMenuAttribute
	[PXDBString]
	[PXDefault(Undefined)]
	[PXUIField(DisplayName = "Action")]
	[PXStringList(new string[] { Undefined }, new string[] { Undefined })]
	public class PXAutomationMenuAttribute : PXAggregateAttribute, IPXRowSelectedSubscriber, IPXFieldSelectingSubscriber
	{
		#region Ctor
		public PXAutomationMenuAttribute()
			: base()
		{
		}
		#endregion

		#region State
		protected List<string> _AllowedValues;
		public string DisplayName
		{
			get
			{
				return this.GetAttribute<PXUIFieldAttribute>().DisplayName;
			}
			set
			{
				this.GetAttribute<PXUIFieldAttribute>().DisplayName = value;
			}
		}

		public bool Visible
		{
			get
			{
				return this.GetAttribute<PXUIFieldAttribute>().Visible;
			}
			set
			{
				this.GetAttribute<PXUIFieldAttribute>().Visible = value;
			}
		}
		#endregion

		#region Implementation
		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			string val = sender.GetValue(e.Row, this._FieldName) as string;
			if (string.IsNullOrEmpty(val))
				sender.SetValue(e.Row, this._FieldName, Undefined);
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			string val = e.ReturnValue as string;
			if (!String.IsNullOrEmpty(val) && val != Undefined && _AllowedValues != null && !val.Contains('$'))
			{
				val = val + "$";
				string ret = _AllowedValues.Where((s) => !String.IsNullOrEmpty(s) && s.StartsWith(val)).FirstOrDefault();
				if (ret != null)
				{
					e.ReturnValue = ret;
				}
			}
		}
		#endregion

		private class AutomationActionMenuItem
		{
			public class Comparer : IEqualityComparer<AutomationActionMenuItem>
			{
				public bool Equals(AutomationActionMenuItem x, AutomationActionMenuItem y)
				{
					if (x == null && y == null)
					{
						return true;
					}

					if (x == null || y == null)
					{
						return false;
					}

					return (String.Equals(x.Menu, y.Menu, StringComparison.OrdinalIgnoreCase)) &&
						(x.Fills.Count() == y.Fills.Count()) &&
						!(x.Fills.Except(y.Fills, new Fill.Comparer()).Any() || y.Fills.Except(x.Fills, new Fill.Comparer()).Any());
				}

				public int GetHashCode(AutomationActionMenuItem obj)
				{
					var comparer = new Fill.Comparer();
					return obj.Fills.OrderBy(f => f.Name).Select(
						f => string.Concat(f.Name, f.Value, f.Delayed, f.Relative, f.Ignore))
						.JoinToString("").GetHashCode() ^ obj.Menu.GetHashCode();
				}
			}

			public readonly PXProcessingStep ParentStep;
			public readonly PXProcessingAction ParentAction;
			public readonly string Menu;
			public readonly IEnumerable<Fill> Fills;

			public AutomationActionMenuItem(PXProcessingStep parentStep, PXProcessingAction parentAction, string menu, IEnumerable<Fill> fills)
			{
				ParentStep = parentStep;
				ParentAction = parentAction;
				Menu = menu;
				Fills = fills;
			}
		}

		#region Initialization

		private string GetLabel(PXCache sender, IEnumerable<AutomationActionMenuItem> itemGroup)
		{
			string menu = itemGroup.First().Menu;
			var info = itemGroup.SelectMany(item => GraphHelper.GetActions(item.ParentStep.GraphName, item.ParentStep.CacheName));
			var action = info.FirstOrDefault(_ => String.Equals(_.Name, menu, StringComparison.OrdinalIgnoreCase));
			var label = string.Empty;
			if (action != null)
			{
				label = action.DisplayName;
			}
			else
			{
				label = menu;
				PXLocalizerRepository.AutomationLocalizer.Localize(ref label, sender.Graph.GetType().FullName);
			}
			return label;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			List<string> values = new List<string>();
			List<string> labels = new List<string>();
			PXProcessingStep[] steps = PXAutomation.GetProcessingSteps(sender.Graph);

			List<PXProcessingStep> secured = new List<PXProcessingStep>();
			string screenID = null;
			string graphName = null;
			Type cacheType = null;
			PXCacheRights rights;
			List<string> disabled = null;
			List<string> invisible = null;
			foreach (PXProcessingStep s in steps)
			{
				if (!String.Equals(s.ScreenID, screenID, StringComparison.InvariantCultureIgnoreCase)
					|| !String.Equals(s.GraphName, graphName, StringComparison.OrdinalIgnoreCase)
					|| cacheType == null
					|| !String.Equals(cacheType.FullName, s.CacheName, StringComparison.OrdinalIgnoreCase))
				{
					screenID = s.ScreenID;
					graphName = s.GraphName;
					cacheType = System.Web.Compilation.BuildManager.GetType(s.CacheName, false);
					if (!String.IsNullOrEmpty(screenID) && !String.IsNullOrEmpty(graphName) && cacheType != null)
					{
						PXAccess.GetRights(screenID, graphName, cacheType, out rights, out invisible, out disabled);
					}
				}
				if ((invisible == null || invisible.Count == 0) && (disabled == null || disabled.Count == 0))
				{
					secured.Add(s);
				}
				else
				{
					List<PXProcessingAction> actions = new List<PXProcessingAction>();
					foreach (PXProcessingAction a in s.Actions)
					{
						if (String.IsNullOrEmpty(a.Name))
						{
							continue;
						}
						if (a.Menus == null || a.Menus.Length == 0)
						{
							if ((disabled == null || !disabled.Contains(a.Name, StringComparer.InvariantCultureIgnoreCase))
								&& (invisible == null || !invisible.Contains(a.Name, StringComparer.InvariantCultureIgnoreCase)))
							{
								actions.Add(a);
							}
						}
						else
						{
							List<MenuItem> menus = new List<MenuItem>();
							foreach (MenuItem m in a.Menus)
							{
								if (String.IsNullOrEmpty(m.Menu))
								{
									continue;
								}
								if ((disabled == null || !disabled.Contains(m.Menu + "@" + a.Name, StringComparer.InvariantCultureIgnoreCase))
									&& (invisible == null || !invisible.Contains(m.Menu + "@" + a.Name, StringComparer.InvariantCultureIgnoreCase)))
								{
									menus.Add(m);
								}
							}
							if (menus.Count > 0)
							{
								actions.Add(new PXProcessingAction(a.Name, menus.ToArray()));
							}
						}
					}
					if (actions.Count > 0)
					{
						secured.Add(new PXProcessingStep(s.ScreenID, s.Name, s.GraphName, s.CacheName, actions.ToArray(), s.Description));
					}
				}
			}
			steps = secured.ToArray();

			var comparer = new AutomationActionMenuItem.Comparer();
			var menuGroups = steps.SelectMany(
				step => step.Actions.SelectMany(
					action => action.Menus.Select(
						menuItem => new AutomationActionMenuItem(step, action, menuItem.Menu, menuItem.Fills))))
				.GroupBy(comparer.GetHashCode)
				.GroupBy(g => g.First().Menu);

			foreach (var menuGroup in menuGroups)
			{
				bool firstProcessed = false;
				foreach (var menuItem in menuGroup.OrderByDescending(m => m.Count()))
				{
					if (!firstProcessed)
					{
						labels.Add(GetLabel(sender, menuItem));
						values.Add(menuItem.First().Menu + "$" + menuItem.Select(i => i.ParentStep.Name).JoinToString("$"));
						firstProcessed = true;
					}
					else
					{
						labels.Add(GetLabel(sender, menuItem) + " - " + menuItem.Select(i => i.ParentStep.Description).JoinToString(", "));
						values.Add(menuItem.First().Menu + "$" + menuItem.Select(i => i.ParentStep.Name).JoinToString("$"));
					}
				}
			}

			if (values.Count == 0)
				return;
			if (values.Count > 1)
				PXStringListAttribute.AppendList(sender, null, this._FieldName, values.ToArray(), labels.ToArray());
			else
				PXStringListAttribute.SetList(sender, null, this._FieldName, values.ToArray(), labels.ToArray());

			this._AllowedValues = values;
		}
		#endregion

		public const string Undefined = "<SELECT>";

		public class undefinded : Constant<string>
		{
			public undefinded()
				: base(Undefined)
			{
			}
		}
	}
	#endregion

	#region PXIntListAttribute
	/// <summary>
	/// This attribute specifies a list of allowed values for DAC field.
	/// User can select specified values from combobox in user interface.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method)]
	[PXAttributeFamily(typeof(PXBaseListAttribute))]
	public class PXIntListAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber, IPXLocalizableList
	{
		#region State
		protected int[] _AllowedValues;
		protected string[] _AllowedLabels;
		protected string[] _NeutralAllowedLabels;
		public virtual bool IsLocalizable { get; set; }
		public Dictionary<int, string> ValueLabelDic
		{
			get
			{
				Dictionary<int, string> result = new Dictionary<int, string>(_AllowedValues.Length);
				for (int index = 0; index < _AllowedValues.Length; index++)
					result.Add(_AllowedValues[index], _AllowedLabels[index]);
				return result;
			}
		}
		#endregion

		#region Ctor
		public PXIntListAttribute()
		{
			IsLocalizable = true;
		}
		public PXIntListAttribute(string list)
			: this()
		{
			string[] items = list.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			_AllowedValues = new int[items.Length];
			_AllowedLabels = new string[items.Length];
			for (int i = 0; i < items.Length; i++)
			{
				int pos = items[i].IndexOf(';');
				if (pos >= 0)
				{
					_AllowedValues[i] = int.Parse(items[i].Substring(0, pos));
					if (pos + 1 < items[i].Length)
					{
						_AllowedLabels[i] = items[i].Substring(pos + 1);
					}
					else
					{
						_AllowedLabels[i] = items[i].Substring(0, pos);
					}
				}
				else
				{
					_AllowedValues[i] = int.Parse(items[i]);
					_AllowedLabels[i] = items[i];
				}
			}

			CreateNeutralLabels();
		}
		public PXIntListAttribute(int[] allowedValues, string[] allowedLabels)
			: this()
		{
			if (allowedValues.Length != allowedLabels.Length)
				throw new PXArgumentException("allowedValues", ErrorMessages.IncorrectValueArrayLength);
			_AllowedValues = allowedValues;
			_AllowedLabels = allowedLabels;

			CreateNeutralLabels();
		}
		public PXIntListAttribute(Type enumType)
			: this()
		{
			_AllowedValues = (int[])Enum.GetValues(enumType);
			_AllowedLabels = (string[])Enum.GetNames(enumType);

			CreateNeutralLabels();
		}
		#endregion

		#region Runtime
		public static void SetList<Field>(PXCache cache, object data, int[] allowedValues, string[] allowedLabels)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXIntListAttribute)
				{
					PXIntListAttribute intList = (PXIntListAttribute)attr;

					intList._AllowedValues = allowedValues;
					intList._AllowedLabels = allowedLabels;
					intList._NeutralAllowedLabels = null;
					intList.CreateNeutralLabels();

					if (intList._AllowedLabels != null)
					{
						for (int i = 0; i < intList._AllowedLabels.Length; i++)
						{
							intList._AllowedLabels[i] = PXMessages.LocalizeNoPrefix(intList._NeutralAllowedLabels[i]);
						}
					}
				}
			}
		}
		#endregion

		#region Implementation
		public override void CacheAttached(PXCache sender)
		{
			AppendNeutral();
			TryLocalize(sender);
		}

		private void AppendNeutral()
		{
			if (_NeutralAllowedLabels != null && _AllowedLabels != null && _NeutralAllowedLabels.Length != _AllowedLabels.Length)
			{
				_NeutralAllowedLabels = _AllowedLabels;
			}
		}

		private void TryLocalize(PXCache sender)
		{
			if (IsLocalizable)
			{
				if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Items.Contains(PXControlPropertiesCollector.COLLECTION_RESOURCES_KEY))
				{
					PXPageRipper.RipList(this, sender, _AllowedLabels);
				}
				else
				{
					PXLocalizerRepository.ListLocalizer.Localize(this, sender, _NeutralAllowedLabels, _AllowedLabels);
				}
			}
		}

		private void CreateNeutralLabels()
		{
			if (_AllowedLabels != null && _AllowedLabels.Length > 0 && _NeutralAllowedLabels == null)
			{
				_NeutralAllowedLabels = new string[_AllowedLabels.Length];
				_AllowedLabels.CopyTo(_NeutralAllowedLabels, 0);
			}
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, null, null, null, null, _AllowedValues, _AllowedLabels, null, null);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBCalcedAttribute
	/// <summary>
	/// Read-only database-related field. server-side calculation is supported
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class)]
	public class PXDBCalcedAttribute : PXEventSubscriberAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected Type _OperandType;
		protected IBqlCreator _Operand;
		protected Type _Type;
		protected int _DatabaseOrdinal = -1;
		protected bool _Persistent = false;
		protected bool _BypassGroupby;

		public virtual bool Persistent
		{
			get
			{
				return _Persistent;
			}
			set
			{
				this._Persistent = value;
			}
		}
		#endregion

		#region Ctor
		public PXDBCalcedAttribute(Type operand, Type type)
		{
			_OperandType = operand;
			_Type = type;
			foreach (Type t in BqlCommand.Decompose(operand))
			{
				if (typeof(IBqlSearch).IsAssignableFrom(t))
				{
					_BypassGroupby = true;
				}
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (_Persistent)
			{
				sender.Graph.RowPersisted.AddHandler(sender.GetItemType(), RowPersisted);
			}
		}
		#endregion

		#region Implementation
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && !sender.BypassCalced)
			{
				StringBuilder text = new StringBuilder();
				List<Type> tables = new List<Type>();
				bool replace = false;
				if (e.Table != null)
				{
					tables.Add(e.Table);
				}
				else
				{
					tables.Add(_BqlTable);
					if (sender.GetExtensionTables() != null && sender.GetItemType() != _BqlTable)
					{
						replace = true;
					}
				}
				if (!typeof(IBqlCreator).IsAssignableFrom(_OperandType))
				{
					if (String.Compare(_OperandType.Name, _FieldName, StringComparison.OrdinalIgnoreCase) != 0)
					{
						text.Append(" ").Append(BqlCommand.GetSingleField(_OperandType, sender.Graph, tables, null));
					}
					else
					{
						text.Append(tables[0].Name);
						text.Append('.');
						text.Append(_OperandType.Name);
					}
				}
				else
				{
					if (_Operand == null)
					{
						_Operand = Activator.CreateInstance(_OperandType) as IBqlCreator;
					}
					if (_Operand == null)
					{
						throw new PXArgumentException("Operand", ErrorMessages.OperandNotClassFieldAndNotIBqlCreator);
					}
					BqlCommand select = sender.BqlSelect;
					try
					{
						sender.BqlSelect = null;
						_Operand.Parse(sender.Graph, null, tables, null, null, text, null);
					}
					finally
					{
						sender.BqlSelect = select;
					}
				}
				e.BqlTable = _BqlTable;
				if (replace)
				{
					text.Replace(" " + sender.GetItemType().Name + ".", " " + _BqlTable.Name + ".");
				}
				e.FieldName = (e.Operation & PXDBOperation.Option) != PXDBOperation.GroupBy || _Type != typeof(Boolean) && _Type != typeof(Guid) && !_BypassGroupby ? "(" + text.ToString() + ")" : BqlCommand.Null;
			}
		}
		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				object value = e.Record.GetValue(e.Position);
				sender.SetValue(e.Row, _FieldOrdinal, value == null ? null : Convert.ChangeType(value, _Type));
			}
			e.Position++;
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, _Type, false, true, null, null, null, null, _FieldName, null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
			}
		}
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Completed && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				bool? result = null;
				object value = null;

				if (typeof(IBqlField).IsAssignableFrom(_OperandType))
				{
					if (sender.GetItemType() == BqlCommand.GetItemType(_OperandType) && BqlCommand.GetItemType(_OperandType).IsAssignableFrom(sender.GetItemType()))
					{
						value = sender.GetValue(e.Row, _OperandType.Name);
					}
				}
				else
				{
					if (_Operand == null)
					{
						_Operand = Activator.CreateInstance(_OperandType) as IBqlCreator;
					}
					if (_Operand == null)
					{
						throw new PXArgumentException("Operand", ErrorMessages.OperandNotClassFieldAndNotIBqlCreator);
					}
					_Operand.Verify(sender, e.Row, null, ref result, ref value);
				}
				sender.SetValue(e.Row, _FieldName, value);
			}
		}
		#endregion
	}
	#endregion

	#region PXDisplaySelectorAttribute
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	internal sealed class PXDisplaySelectorAttribute : PXSelectorAttribute
	{
		public PXDisplaySelectorAttribute(Type type)
			: base(type)
		{
		}

		public PXDisplaySelectorAttribute(Type type, params Type[] fieldList)
			: base(type, fieldList)
		{
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e) { }

		public override void ReadDeletedFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e) { }

		public override void SubstituteKeyFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (!e.Cancel && e.NewValue != null)
			{
				object item = null;
				Dictionary<object, KeyValuePair<object, bool>> dict = null;
				if (_CacheGlobal)
				{
					dict = PXContext.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName);
					if (dict == null)
					{
						PXContext.SetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName, (dict = PXDatabase.GetSlot<Dictionary<object, KeyValuePair<object, bool>>>(_Type.FullName, _BqlType)));
					}
					lock (((ICollection)dict).SyncRoot)
					{
						KeyValuePair<object, bool> pair;
						if (dict.TryGetValue(e.NewValue, out pair))
						{
							if (pair.Value)
							{
								PXView select = sender.Graph.TypedViews.GetView(_NaturalSelect, !_DirtyRead);
								object ret = select.Cache.GetValue(pair.Key, ((IBqlSearch)_Select).GetField().Name);
								e.NewValue = ret;
								return;
							}
							item = pair.Key;
						}
					}
				}
				if (item == null)
				{
					PXView select = sender.Graph.TypedViews.GetView(_NaturalSelect, !_DirtyRead);
					if (!_CacheGlobal)
					{
						object[] pars = new object[_ParsCount + 1];
						pars[pars.Length - 1] = e.NewValue;
						item = SelectSingleBound(select, new object[] { e.Row }, pars);
					}
					else
					{
						item = SelectSingleBound(select, new object[] { e.Row }, e.NewValue);
					}
					if (item != null)
					{
						object ret = select.Cache.GetValue(item, ((IBqlSearch)_Select).GetField().Name);
						if (dict != null && select.Cache.GetStatus(item) == PXEntryStatus.Notchanged && select.Cache.Keys.Count <= 1)
						{
							lock (((ICollection)dict).SyncRoot)
							{
								dict[e.NewValue] = new KeyValuePair<object, bool>(item, false);
								if (ret != null)
								{
									dict[ret] = new KeyValuePair<object, bool>(item, false);
								}
							}
						}
						e.NewValue = ret;
					}
					else
					{
						var found = false;
						if (PXDatabase.IsReadDeletedSupported(_BqlType))
						{
							using (PXReadDeletedScope rds = new PXReadDeletedScope())
							{
								if (!_CacheGlobal)
								{
									object[] pars = new object[_ParsCount + 1];
									pars[pars.Length - 1] = e.NewValue;
									item = SelectSingleBound(select, new object[] { e.Row }, pars);
								}
								else
								{
									item = SelectSingleBound(select, new object[] { e.Row }, e.NewValue);
								}
								if (item != null)
								{
									object ret = select.Cache.GetValue(item, ((IBqlSearch)_Select).GetField().Name);
									if (dict != null && select.Cache.GetStatus(item) == PXEntryStatus.Notchanged && select.Cache.Keys.Count <= 1)
									{
										lock (((ICollection)dict).SyncRoot)
										{
											dict[e.NewValue] = new KeyValuePair<object, bool>(item, true);
											if (ret != null)
											{
												dict[ret] = new KeyValuePair<object, bool>(item, true);
											}
										}
									}
									e.NewValue = ret;
									found = true;
								}
							}
						}
						if (!found) throwNoItem(hasRestrictedAccess(sender, _NaturalSelect, e.Row), true, e.NewValue);
					}
				}
				else
				{
					PXCache cache = sender.Graph.Caches[_Type];
					object p = e.NewValue;
					e.NewValue = cache.GetValue(item, ((IBqlSearch)_Select).GetField().Name);
					if (e.NewValue == null && !cache.GetItemType().IsAssignableFrom(item.GetType()))
					{
						PXView select = sender.Graph.TypedViews.GetView(_NaturalSelect, !_DirtyRead);
						item = SelectSingleBound(select, new object[] { e.Row }, p);
						if (item != null)
						{
							e.NewValue = select.Cache.GetValue(item, ((IBqlSearch)_Select).GetField().Name);
						}
					}
				}
			}
		}
	}
	#endregion

	#region PXRestrictorAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = true)]
	public class PXRestrictorAttribute : PXEventSubscriberAttribute, IPXFieldVerifyingSubscriber
	{
		protected string _Message;
		protected Type[] _MsgParams;
		protected Type _Where;

		protected BqlCommand _OriginalCmd;
		protected BqlCommand _AlteredCmd;
		protected List<Type> _AlteredParams = null;
		protected Type _ValueField;

		protected int _ParsCount;
		protected bool _DirtyRead;
		protected Type _SubstituteKey;

		protected int _RestrictLevel;
		protected int _ReplaceLevel = -1;

		public bool ReplaceInherited
		{
			get
			{
				return _ReplaceLevel >= 0;
			}
			set
			{
				_ReplaceLevel = 0;
				_RestrictLevel = 1;
			}
		}

		public PXRestrictorAttribute(Type where, string message, params Type[] pars)
		{
			if (where == null)
			{
				throw new PXArgumentException("where", ErrorMessages.ArgumentNullException);
			}
			else if (typeof(IBqlWhere).IsAssignableFrom(where))
			{
				_Where = where;
			}
			else
			{
				throw new PXArgumentException("where", ErrorMessages.ArgumentException);
			}

			_Message = message;

			if (pars.Any(par => !typeof(IBqlField).IsAssignableFrom(par)))
			{
				throw new PXArgumentException("params", ErrorMessages.ArgumentException);
			}
			_MsgParams = pars;
		}

		public override void CacheAttached(PXCache sender)
		{
			foreach (PXRestrictorAttribute restrattr in sender.GetAttributesReadonly(_FieldName).Where(attr => attr is PXRestrictorAttribute).Cast<PXRestrictorAttribute>())
			{
				if (restrattr._ReplaceLevel >= this._RestrictLevel)
				{
					return;
				}
			}

			foreach (PXSelectorAttribute selattr in sender.GetAttributesReadonly(_FieldName).Where(attr => attr is PXSelectorAttribute).Cast<PXSelectorAttribute>())
			{
				_OriginalCmd = selattr.PrimarySelect;
				_AlteredCmd = selattr.WhereAnd(sender, _Where);
				_ValueField = selattr.ValueField;
				_ParsCount = selattr.ParsCount;
				_DirtyRead = selattr.DirtyRead;
				_SubstituteKey = selattr.SubstituteKey;

				AlterCommand(sender);
				break;
			}
		}

		protected Dictionary<Type, KeyValuePair<BqlCommand, List<Type>>> _altered = new Dictionary<Type, KeyValuePair<BqlCommand, List<Type>>>();

		protected virtual void AlterCommand(PXCache sender)
		{
			if (_AlteredCmd == null) return;

			Type key = typeof(PXGraph);
			if (sender.IsGraphSpecificField(_FieldName))
			{
				key = sender.Graph.GetType();
			}

			KeyValuePair<BqlCommand, List<Type>> altered;
			lock (((ICollection)_altered).SyncRoot)
			{
				if (_altered.TryGetValue(key, out altered))
				{
					_AlteredCmd = altered.Key;
					_AlteredParams = altered.Value;

					return;
				}
			}

			PXView view = sender.Graph.TypedViews.GetView(_OriginalCmd, !_DirtyRead);

			Type[] tables = _AlteredCmd.GetTables();
			Type[] bql = BqlCommand.Decompose(_AlteredCmd.GetSelectType());
			IBqlParameter[] parameters = _AlteredCmd.GetParameters();

			int _AlteredParsCount = 0;
			int _ValueParamIndex = 0;
			if (parameters != null)
				foreach (IBqlParameter par in parameters)
				{
					if (par.IsVisible) _AlteredParsCount++;
					if (par.IsVisible && !par.HasDefault && par.GetReferencedType() == _ValueField) _ValueParamIndex = _AlteredParsCount - 1;
				}


			int j = 0;
			_AlteredParams = new List<Type>(new Type[_AlteredParsCount]);
			_AlteredParams[_ValueParamIndex] = _ValueField;
			for (int i = 0; i < bql.Length; i++)
			{
				if (typeof(IBqlParameter).IsAssignableFrom(bql[i])
					&& (bql[i].GetGenericTypeDefinition() == typeof(Optional<>)
					|| bql[i].GetGenericTypeDefinition() == typeof(Optional2<>)
					|| bql[i].GetGenericTypeDefinition() == typeof(Required<>)))
				{
					j++;
				}

				if (typeof(IBqlField).IsAssignableFrom(bql[i]) && !typeof(IBqlParameter).IsAssignableFrom(bql[i - 1]))
				{
					Type currentType = BqlCommand.GetItemType(bql[i]);
					if (!(view.Cache.GetItemType() == currentType || currentType.IsAssignableFrom(view.Cache.GetItemType())))
					{
						if (Array.IndexOf(tables, currentType) > -1)
						{
							_AlteredParams.Insert(j++, bql[i]);
							bql[i] = typeof(Required<>).MakeGenericType(bql[i]);
						}
					}
				}
			}

			_AlteredCmd = BqlCommand.CreateInstance(BqlCommand.Compose(bql));
			lock (((ICollection)_altered).SyncRoot)
			{
				_altered[key] = new KeyValuePair<BqlCommand, List<Type>>(_AlteredCmd, _AlteredParams);
			}
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_AlteredCmd == null || _AlteredParams == null || _OriginalCmd == null || _Message == null) return;

			object[] pars = new object[_ParsCount + 1];
			pars[pars.Length - 1] = e.NewValue;

			PXView view = sender.Graph.TypedViews.GetView(_OriginalCmd, !_DirtyRead);
			object item = view.SelectSingleBound(new[] { e.Row }, pars);
			if (item != null)
			{
				pars = new object[_AlteredParams.Count];
				for (int i = 0; i < _AlteredParams.Count; i++)
				{
					if (_AlteredParams[i] != null)
					{
						Type currentType = BqlCommand.GetItemType(_AlteredParams[i]);
						object current = PXResult.Unwrap(item, currentType);
						if (current != null)
						{
							pars[i] = sender.Graph.Caches[currentType].GetValue(current, _AlteredParams[i].Name);
						}
					}
				}

				view = sender.Graph.TypedViews.GetView(_AlteredCmd, !_DirtyRead);
				pars = view.PrepareParameters(new object[] { e.Row }, pars);

				if (item is PXResult)
				{
					item = ((PXResult)item)[0];
				}

				if (!_AlteredCmd.Meet(view.Cache, item, pars))
				{
					if (_SubstituteKey != null)
					{
						object errorValue = e.NewValue;
						sender.RaiseFieldSelecting(_FieldName, e.Row, ref errorValue, false);
						e.NewValue = (errorValue is PXFieldState) ? ((PXFieldState)errorValue).Value : errorValue;
					}

					throw new PXSetPropertyException(_Message, _MsgParams.Select(param => view.Cache.GetStateExt(item, param.Name)).ToArray());
				}
			}
		}
	}
	#endregion

	#region PXDBCreatedByIDAttribute
	[Serializable]
	public class PXDBCreatedByIDAttribute : PXAggregateAttribute, IPXRowInsertingSubscriber, IPXFieldVerifyingSubscriber
	{
		[PXBreakInheritance]
		[Serializable]
		public sealed class Creator : Users
		{
			public new abstract class pKID : IBqlField { }

			public new abstract class username : IBqlField { }

			[PXDBString]
			[PXUIField(DisplayName = "Created By", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
			public override String Username { get; set; }
		}

		public Type BqlField
		{
			get
			{
				return null;
			}
			set
			{
				((PXDBGuidAttribute)_Attributes[_Attributes.Count - 2]).BqlField = value;
				BqlTable = ((PXDBGuidAttribute)_Attributes[_Attributes.Count - 2]).BqlTable;
			}
		}
		protected Guid GetUserID(PXCache sender)
		{
			return PXAccess.GetTrueUserID();
		}

		public PXDBCreatedByIDAttribute()
			: this(typeof(Creator.pKID), typeof(Creator.username), typeof(Creator.username),
				typeof(Creator.pKID), typeof(Creator.username))
		{
		}

		internal PXDBCreatedByIDAttribute(Type search, Type substituteKey, Type descriptionField, params Type[] fields)
			: base()
		{
			_Attributes.Add(new PXDBGuidAttribute());
			_Attributes.Add(new PXDisplaySelectorAttribute(search, fields)
								{
									DescriptionField = descriptionField
								});
			((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).SubstituteKey = substituteKey;
			((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).CacheGlobal = true;
		}

		public bool DontOverrideValue { get; set; }

		void IPXRowInsertingSubscriber.RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (!DontOverrideValue || sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, GetUserID(sender));
			}
		}

		void IPXFieldVerifyingSubscriber.FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			try
			{
				((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).FieldVerifying(sender, e);
			}
			catch (PXSetPropertyException)
			{
				e.NewValue = GetUserID(sender);
			}
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber))
			{
				subscribers.Remove(_Attributes[_Attributes.Count - 1] as ISubscriber);
			}
		}
	}
	#endregion

	#region PXDBLastModifiedByIDAttribute
	[Serializable]
	public class PXDBLastModifiedByIDAttribute : PXDBCreatedByIDAttribute, IPXRowUpdatingSubscriber
	{
		[PXBreakInheritance]
		[Serializable]
		public sealed class Modifier : Users
		{
			public new abstract class pKID : IBqlField { }

			public new abstract class username : IBqlField { }

			[PXDBString]
			[PXUIField(DisplayName = "Last Modified By", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
			public override String Username { get; set; }
		}

		public PXDBLastModifiedByIDAttribute()
			: base(typeof(Modifier.pKID), typeof(Modifier.username), typeof(Modifier.username),
				typeof(Modifier.pKID), typeof(Modifier.username))
		{
		}

		void IPXRowUpdatingSubscriber.RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			sender.SetValue(e.NewRow, _FieldOrdinal, GetUserID(sender));
		}
	}
	#endregion

	#region PXDBCreatedByScreenIDAttribute
	public class PXDBCreatedByScreenIDAttribute : PXDBStringAttribute, IPXRowInsertingSubscriber
	{
		public PXDBCreatedByScreenIDAttribute()
			: base(10)
		{
			InputMask = "aa.aa.aa.aa";
		}

		protected string GetScreenID(PXCache sender)
		{
			if (sender.Graph.Accessinfo != null && sender.Graph.Accessinfo.ScreenID != null)
			{
				return ((string)sender.Graph.Accessinfo.ScreenID).Replace(".", "");
			}
			else
			{
				return "00000000";
			}
		}

		void IPXRowInsertingSubscriber.RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			sender.SetValue(e.Row, _FieldOrdinal, GetScreenID(sender));
		}
	}
	#endregion

	#region PXDBLastModifiedByScreenIDAttribute
	public class PXDBLastModifiedByScreenIDAttribute : PXDBCreatedByScreenIDAttribute, IPXRowUpdatingSubscriber
	{
		void IPXRowUpdatingSubscriber.RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			sender.SetValue(e.NewRow, _FieldOrdinal, GetScreenID(sender));
		}
	}
	#endregion

	#region PXDBCreatedDateTimeAttribute
	public class PXDBCreatedDateTimeAttribute : PXDBDateAttribute, IPXCommandPreparingSubscriber, IPXRowInsertingSubscriber
	{
		protected virtual DateTime GetDate()
		{
			DateTime? serverDate = PXTransactionScope.GetServerDateTime(false);
			if (serverDate.HasValue) return serverDate.Value;

			return DateTime.Now;
		}

		public PXDBCreatedDateTimeAttribute()
			: base()
		{
			this.UseSmallDateTime = false;
			PreserveTime = true;
			UseTimeZone = false;
		}

		void IPXRowInsertingSubscriber.RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (sender.GetValue(e.Row, _FieldOrdinal) == null) sender.SetValue(e.Row, _FieldOrdinal, GetDate());
		}
		void IPXCommandPreparingSubscriber.CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Insert) == PXDBOperation.Insert)
			{
				e.DataLength = 8;
				e.IsRestriction = e.IsRestriction || _IsKey;
				if (_DatabaseFieldName != null)
				{
					e.BqlTable = _BqlTable;
					e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName;
				}

				e.DataType = PXDbType.DirectExpression;
				e.DataValue = UseTimeZone ? PXDatabase.Provider.SqlDialect.GetUtcDate : PXDatabase.Provider.SqlDialect.GetDate;

				sender.SetValue(e.Row, _FieldOrdinal, GetDate());
			}
			else base.CommandPreparing(sender, e);
		}
	}
	#endregion

	#region PXDBLastModifiedDateTimeAttribute
	public class PXDBLastModifiedDateTimeAttribute : PXDBCreatedDateTimeAttribute, IPXCommandPreparingSubscriber, IPXRowUpdatingSubscriber
	{
		void IPXRowUpdatingSubscriber.RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (sender.GetValue(e.NewRow, _FieldOrdinal) == null) sender.SetValue(e.NewRow, _FieldOrdinal, GetDate());
		}
		void IPXCommandPreparingSubscriber.CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Insert) == PXDBOperation.Insert
				|| (e.Operation & PXDBOperation.Update) == PXDBOperation.Update)
			{
				e.DataLength = 8;
				e.IsRestriction = e.IsRestriction || _IsKey;
				if (_DatabaseFieldName != null)
				{
					e.BqlTable = _BqlTable;
					e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName;
				}

				e.DataType = PXDbType.DirectExpression;
				e.DataValue = UseTimeZone ? PXDatabase.Provider.SqlDialect.GetUtcDate : PXDatabase.Provider.SqlDialect.GetDate;

				sender.SetValue(e.Row, _FieldOrdinal, GetDate());
			}
			else base.CommandPreparing(sender, e);
		}
	}
	#endregion

	#region PXFormulaAttribute
	/// <summary>
	/// Computes formulas
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class, AllowMultiple = true)]
	public class PXUnboundFormulaAttribute : PXFormulaAttribute
	{
		#region State
		public override string FormulaFieldName
		{
			get
			{
				return null;
			}
		}
		#endregion

		#region Ctor

		public PXUnboundFormulaAttribute(Type formulaType, Type aggregateType)
			: base(formulaType, aggregateType)
		{
		}

		protected override void InitDependencies(PXCache sender)
		{
		}

		protected override void InitAggregate(Type aggregateType)
		{
			if (aggregateType != null)
			{
				Type[] args = aggregateType.GetGenericArguments();

				_ParentFieldType = args[0];
				if (!typeof(IBqlField).IsAssignableFrom(_ParentFieldType))
					throw new PXArgumentException("_ParentFieldType", ErrorMessages.CantGetParentField, _ParentFieldType.Name);

				if (!typeof(IBqlUnboundAggregateCalculator).IsAssignableFrom(aggregateType))
					throw new PXArgumentException("_Aggregate", ErrorMessages.CantFindAggregateType, aggregateType.Name);
				_Aggregate = (IBqlUnboundAggregateCalculator)Activator.CreateInstance(aggregateType);
			}
			else
			{
				_ParentFieldType = null;
				_Aggregate = null;
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			ISwitch formula = _Formula as ISwitch;
			if (formula != null)
			{
				formula.OuterField = null;
			}
		}
		#endregion

		#region Implementation

		protected override object CalcAggregate(PXCache cache, object row, object oldrow, int digit)
		{
			return ((IBqlUnboundAggregateCalculator)_Aggregate).Calculate(cache, row, oldrow, _Formula, digit);
		}

		protected override object CalcAggregate(PXCache cache, object row, object[] records, int digit)
		{
			return ((IBqlUnboundAggregateCalculator)_Aggregate).Calculate(cache, row, _Formula, records, digit);
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (_Formula != null)
			{
				EnsureParent(sender, e.Row);
				UpdateParent(sender, e.Row, e.OldRow);
				EnsureChildren(sender, e.OldRow);
			}
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (_Formula != null)
			{
				EnsureParent(sender, e.Row);
				UpdateParent(sender, e.Row, null);
			}
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (_Formula != null)
			{
				UpdateParent(sender, null, e.Row);
				EnsureChildren(sender, e.Row);
			}
		}
		#endregion
	}
	#endregion

	#region PXFormulaAttribute
	/// <summary>
	/// Computes formulas
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class, AllowMultiple = true)]
	public class PXFormulaAttribute : PXEventSubscriberAttribute, IPXRowUpdatedSubscriber, IPXRowInsertedSubscriber, IPXRowDeletedSubscriber
	{
		#region State
		protected IBqlCreator _Formula;
		protected Type _ParentFieldType;
		protected object _Aggregate;

		public virtual string FormulaFieldName
		{
			get
			{
				return _FieldName;
			}
		}

		public virtual Type Formula
		{
			get
			{
				return _Formula.GetType();
			}
			set
			{
				_Formula = InitFormula(value);
			}
		}

		public virtual Type ParentField
		{
			get
			{
				return _ParentFieldType;
			}
			set
			{
				_ParentFieldType = value;
			}
		}

		public virtual Type Aggregate
		{
			get
			{
				return _Aggregate.GetType();
			}
			set
			{
				InitAggregate(value);
			}

		}

		protected bool _Persistent = false;
		public virtual bool Persistent
		{
			get { return _Persistent; }
			set { _Persistent = value; }
		}

		protected ObjectRef<bool> _recursion;

		#endregion

		#region Ctor
		/// <summary>
		/// This variant of constructor defines formula to calculate value of the
		/// field the attribute is atached to.
		/// </summary>
		/// <param name="formulaType">Formula to calculate value of the field
		/// from other fields of the given row.
		/// The following types are allowed: Add, Sub, Mult, Div
		/// </param>
		public PXFormulaAttribute(Type formulaType)
		{
			_Formula = InitFormula(formulaType);
		}

		/// <summary>
		/// This variant of constructor in addition to the first variant
		/// defines aggregate function used to calculate value for the field in
		/// a foreign table.
		/// </summary>
		/// <param name="formulaType">Formula to calculate value of the field
		/// from other fields of the given row.
		/// The following types are allowed: Add, Sub, Mult, Div</param>
		/// <param name="aggregateType">First generic parameter: Field in a foreign table to keep
		/// the value of the field the attribute is attached to. Second generic parameter: 
		/// aggregate function to calculate value of the field in a foreign table.
		/// Following aggregate functions are defined: SumCalc, CountCalc, MinCalc, MaxCalc</param>
		public PXFormulaAttribute(Type formulaType, Type aggregateType)
		{
			_Formula = InitFormula(formulaType);
			InitAggregate(aggregateType);
		}

		public static IBqlCreator InitFormula(Type formulaType)
		{
			if (formulaType != null)
			{
				Type type = formulaType;
				if (typeof(IBqlField).IsAssignableFrom(formulaType))
				{
					type = typeof(Row<>);
					type = type.MakeGenericType(formulaType);
				}
				else if (typeof(Constant).IsAssignableFrom(formulaType))
				{
					type = typeof(Const<>);
					type = type.MakeGenericType(formulaType);
				}
				else if (!typeof(IBqlCreator).IsAssignableFrom(formulaType))
					throw new PXArgumentException("formulaType", ErrorMessages.CantCreateFormula, formulaType.Name);
				return (IBqlCreator)Activator.CreateInstance(type);
			}
			return null;
		}

		protected virtual void InitAggregate(Type aggregateType)
		{
			if (aggregateType != null)
			{
				Type[] args = aggregateType.GetGenericArguments();

				_ParentFieldType = args[0];
				if (!typeof(IBqlField).IsAssignableFrom(_ParentFieldType))
					throw new PXArgumentException("_ParentFieldType", ErrorMessages.CantGetParentField, _ParentFieldType.Name);

				if (!typeof(IBqlAggregateCalculator).IsAssignableFrom(aggregateType))
					throw new PXArgumentException("_Aggregate", ErrorMessages.CantFindAggregateType, aggregateType.Name);
				_Aggregate = (IBqlAggregateCalculator)Activator.CreateInstance(aggregateType);
			}
			else
			{
				_ParentFieldType = null;
				_Aggregate = null;
			}
		}
		#endregion

		#region Runtime
		public static void SetAggregate<Field>(PXCache sender, Type aggregateType)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributes<Field>())
			{
				if (attr is PXFormulaAttribute)
				{
					((PXFormulaAttribute)attr).Aggregate = aggregateType;
				}
			}
		}

		public static void CalcAggregate<Field>(PXCache sender, object parent)
			where Field : IBqlField
		{
			CalcAggregate<Field>(sender, parent, false);
		}

		public static void CalcAggregate<Field>(PXCache sender, object parent, bool IsReadOnly)
			where Field : IBqlField
		{
			List<PXFormulaAttribute> formulas = new List<PXFormulaAttribute>();
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly<Field>())
			{
				if (attr is PXFormulaAttribute && ((PXFormulaAttribute)attr)._Aggregate != null)
				{
					formulas.Add((PXFormulaAttribute)attr);
				}
			}

			Type ParentType = parent.GetType();

			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(null))
			{
				if (attr is PXParentAttribute && (((PXParentAttribute)attr).ParentType == ParentType || ParentType.IsSubclassOf(((PXParentAttribute)attr).ParentType)))
				{
					PXView view = ((PXParentAttribute)attr).GetChildrenSelect(sender);
					if (view.IsReadOnly != IsReadOnly)
					{
						view.Clear();
					}
					view.IsReadOnly = IsReadOnly;
					List<object> records = view.SelectMultiBound(new object[] { parent });
					foreach (object row in records)
					{
						sender.RaiseRowSelected(row);
					}

					foreach (PXFormulaAttribute f in formulas)
					{
						f.CalcAggregate(sender, records.ToArray(), parent);
					}
					return;
				}
			}
		}
		#endregion

		#region Implementation
		protected virtual object EnsureParent(PXCache cache, object Row)
		{
			if (_ParentFieldType == null)
			{
				return null;
			}
			if (!typeof(IBqlField).IsAssignableFrom(_ParentFieldType))
			{
				throw new PXArgumentException("_ParentFieldType", ErrorMessages.InvalidField, _ParentFieldType.Name);
			}

			Type parentType = BqlCommand.GetItemType(_ParentFieldType);
			object parent = PXParentAttribute.SelectParent(cache, Row, parentType);

			if (parent == null)
			{
				PXParentAttribute.CreateParent(cache, Row, parentType);
				return null;
			}
			return parent;
		}

		protected virtual void EnsureChildren(PXCache cache, object Row)
		{
			if (_ParentFieldType == null || !typeof(ICountCalc).IsAssignableFrom(_Aggregate.GetType()))
			{
				return;
			}

			if (!typeof(IBqlField).IsAssignableFrom(_ParentFieldType))
			{
				throw new PXArgumentException("_ParentFieldType", ErrorMessages.InvalidField, _ParentFieldType.Name);
			}

			Type parentType = BqlCommand.GetItemType(_ParentFieldType);
			PXCache parentcache = cache.Graph.Caches[parentType];

			if (PXParentAttribute.GetParentCreate(cache, parentType))
			{
				object parent = PXParentAttribute.SelectParent(cache, Row, parentType);
				if (parent != null)
				{
					object val = parentcache.GetValue(parent, _ParentFieldType.Name);

					if (val is int && (int)val <= 0 || val is short && (short)val <= 0)
					{
						parentcache.Delete(parent);
					}
				}
			}
		}

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			object parent = EnsureParent(sender, e.Row);

			UpdateParent(sender, e.Row, e.OldRow, parent);

			EnsureChildren(sender, e.OldRow);
		}

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			bool? result = null;
			object value = null;

			if (_Formula != null && _CancelDefaulting != true && sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				BqlFormula.Verify(sender, e.Row, _Formula, ref result, ref value);
				if (value != PXCache.NotSetValue)
				{
					sender.RaiseFieldUpdating(_FieldName, e.Row, ref value);
					sender.SetValue(e.Row, _FieldOrdinal, value);
				}
			}

			if (_Formula == null && _CancelDefaulting != true && sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(_FieldName))
				{
					if (attr is PXFormulaAttribute && ((PXFormulaAttribute)attr)._Formula != null && ((PXFormulaAttribute)attr).FormulaFieldName != null)
					{
						BqlFormula.Verify(sender, e.Row, ((PXFormulaAttribute)attr)._Formula, ref result, ref value);
						sender.RaiseFieldUpdating(_FieldName, e.Row, ref value);
						sender.SetValue(e.Row, _FieldOrdinal, value);

						break;
					}
				}
			}

			object parent = EnsureParent(sender, e.Row);

			UpdateParent(sender, e.Row, null, parent);
		}

		public virtual void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			UpdateParent(sender, null, e.Row);

			EnsureChildren(sender, e.Row);
		}

		protected bool _CancelDefaulting = false;

		public virtual void FormulaDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			bool? result = null;
			object value = null;

			if (e.Row != null)
			{
				_CancelDefaulting = e.Cancel;
			}

			if (e.Row != null && e.Cancel != true)
			{
				if (_recursion.Value)
				{
					throw new StackOverflowException(string.Format(ErrorMessages.CircularReferenceInFormula, _BqlTable.FullName, _FieldName, _Formula.GetType().Name));
				}
				_recursion.Value = true;
				try
				{
					BqlFormula.Verify(sender, e.Row, _Formula, ref result, ref value);
				}
				finally
				{
					_recursion.Value = false;
				}

				if ((value != null && value != PXCache.NotSetValue) || result == true)
				{
					sender.RaiseFieldUpdating(_FieldName, e.Row, ref value);
					e.NewValue = value;
					e.Cancel = true;
				}
			}
		}

		protected virtual void dependentFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e, Type dependentField)
		{
			IBqlTrigger trigger = _Formula as IBqlTrigger;
			if (trigger != null)
			{
				trigger.Verify(sender, _FieldName, e.Row);
			}
			else
			{
				bool isex = false;
				bool? result = null;
				object value = null;
				try
				{
					BqlFormula.Verify(sender, e.Row, _Formula, ref result, ref value);
				}
				catch (PXSetPropertyException ex)
				{
					sender.SetValue(e.Row, dependentField.Name, e.OldValue);
					sender.RaiseExceptionHandling(_FieldName, e.Row, sender.GetValue(e.Row, _FieldName), ex);
					isex = true;
				}

				if (!isex && value != PXCache.NotSetValue)
				{
					sender.SetValueExt(e.Row, _FieldName, value);
				}
			}
		}

		protected virtual object CalcAggregate(PXCache cache, object row, object oldrow, int digit)
		{
			return ((IBqlAggregateCalculator)_Aggregate).Calculate(cache, row, oldrow, _FieldOrdinal, -1);
		}

		protected virtual object CalcAggregate(PXCache cache, object row, object[] records, int digit)
		{
			return ((IBqlAggregateCalculator)_Aggregate).Calculate(cache, row, _FieldOrdinal, records, -1);
		}

		protected virtual void UpdateParent(PXCache sender, object row, object oldrow)
		{
			UpdateParent(sender, row, oldrow, null);
		}

		protected virtual void UpdateParent(PXCache sender, object row, object oldrow, object newparent)
		{
			if (_ParentFieldType == null || _Aggregate == null)
				return;

			if (!typeof(IBqlField).IsAssignableFrom(_ParentFieldType))
				throw new PXArgumentException("_ParentFieldType", ErrorMessages.InvalidField, _ParentFieldType.Name);

			Type ParentType = BqlCommand.GetItemType(_ParentFieldType);

			object oldparent = null;

			if (row != null && newparent == null)
			{
				newparent = PXParentAttribute.SelectParent(sender, row, ParentType);
			}

			if (oldrow != null && !object.ReferenceEquals(row, oldrow))
			{
				if (row != null)
					PXParentAttribute.CopyParent(sender, row, oldrow, ParentType);

				try
				{
					oldparent = PXParentAttribute.SelectParent(sender, oldrow, ParentType);
				}
				finally
				{
					PXParentAttribute.SetParent(sender, oldrow, ParentType, null);
				}

				if (!object.ReferenceEquals(oldparent, newparent) && newparent != null)
				{
					UpdateParent(sender, null, oldrow);
					UpdateParent(sender, row, null);

					return;
				}
			}

			if (newparent == null)
			{
				row = null;
			}

			object erow = row ?? oldrow;
			object foreignrecord = newparent ?? oldparent;

			if (foreignrecord != null)
			{
				PXCache parentcache = sender.Graph.Caches[ParentType];
				object val = null;
				bool curyviewstate = sender.Graph.Accessinfo.CuryViewState;
				object state;
				try
				{
					sender.Graph.Accessinfo.CuryViewState = false;
					state = parentcache.GetStateExt(foreignrecord, _ParentFieldType.Name);
				}
				finally
				{
					sender.Graph.Accessinfo.CuryViewState = curyviewstate;
				}
				TypeCode tc = TypeCode.Empty;
				if (state is PXFieldState)
				{
					tc = Type.GetTypeCode(((PXFieldState)state).DataType);
					state = ((PXFieldState)state).Value;
				}
				else if (state != null)
				{
					tc = Type.GetTypeCode(state.GetType());
				}

				if (tc == TypeCode.String)
				{
					state = parentcache.GetValue(foreignrecord, _ParentFieldType.Name);
					if (state != null)
						tc = Type.GetTypeCode(state.GetType());
					if (tc == TypeCode.String) return;
				}

				if (state != null)
				{
					val = CalcAggregate(sender, row, oldrow, -1);
				}
				if (val == null)
				{
					object[] records = PXParentAttribute.SelectSiblings(sender, erow, ParentType);
					val = CalcAggregate(sender, row, records, -1);
				}
				else
				{
					switch (tc)
					{
						case TypeCode.Int16:
							val = Convert.ToInt16(state) + Convert.ToInt16(val);
							break;
						case TypeCode.Int32:
							val = Convert.ToInt32(state) + Convert.ToInt32(val);
							break;
						case TypeCode.Double:
							val = Convert.ToDouble(state) + Convert.ToDouble(val);
							break;
						case TypeCode.Decimal:
							val = Convert.ToDecimal(state) + Convert.ToDecimal(val);
							break;
						case TypeCode.DateTime:
							val = Convert.ToDateTime(state) +
								  new TimeSpan(0, 0, Convert.ToInt32(val), 0);
							break;
						default:
							break;
					}
				}

				object result = ConvertValue(tc, val);

				object copy = parentcache.CreateCopy(foreignrecord);
				if (!object.Equals(foreignrecord, parentcache.Current))
				{
					parentcache.RaiseFieldUpdating(_ParentFieldType.Name, copy, ref result);
					parentcache.SetValue(copy, _ParentFieldType.Name, result);
					parentcache.Update(copy);
				}
				else
				{
					parentcache.SetValueExt(foreignrecord, _ParentFieldType.Name, result);
					if (parentcache.GetStatus(foreignrecord) == PXEntryStatus.Notchanged || parentcache.GetStatus(foreignrecord) == PXEntryStatus.Held)
					{
						parentcache.SetStatus(foreignrecord, PXEntryStatus.Updated);
					}
					parentcache.RaiseRowUpdated(foreignrecord, copy);
				}
			}
		}

		public void CalcAggregate(PXCache sender, object[] records, object parent)
		{
			object val = CalcAggregate(sender, null, records, -1);
			PXCache parentcache = sender.Graph.Caches[BqlCommand.GetItemType(_ParentFieldType)];
			PXFieldState fieldstate = (PXFieldState)parentcache.GetStateExt(null, _ParentFieldType.Name);
			TypeCode tc = Type.GetTypeCode(fieldstate.DataType);
			parentcache.SetValue(parent, _ParentFieldType.Name, ConvertValue(tc, val));
		}

		private static object ConvertValue(TypeCode tc, object val)
		{
			switch (tc)
			{
				case TypeCode.Int16:
					return Convert.ToInt16(val);
				case TypeCode.Int32:
					return Convert.ToInt32(val);
				case TypeCode.Double:
					return Convert.ToDouble(val);
				case TypeCode.Decimal:
					return Convert.ToDecimal(val);
				case TypeCode.String:
					return Convert.ToString(val);
			}
			return val;
		}
		#endregion

		#region Initialization

		protected virtual void InitDependencies(PXCache sender)
		{
			if (_Formula != null)
			{
				List<Type> fields = new List<Type>();
				_Formula.Parse(sender.Graph, null, null, fields, null, null, null);
				HashSet<Type> unique = new HashSet<Type>(fields);
				foreach (Type t in unique)
				{
					if (t.IsNested && (BqlCommand.GetItemType(t) == sender.GetItemType() || sender.GetItemType().IsSubclassOf(BqlCommand.GetItemType(t))))
					{
						if (!t.Name.Equals(_FieldName, StringComparison.OrdinalIgnoreCase))
						{
							Type dependentFld = t;
							sender.FieldUpdatedEvents[t.Name.ToLower()] += delegate(PXCache cache, PXFieldUpdatedEventArgs e)
							{
								dependentFieldUpdated(cache, e, dependentFld);
							};
						}
					}
				}
				if (!(_Formula is IBqlTrigger))
				{
					sender.FieldDefaultingEvents[_FieldName.ToLower()] += FormulaDefaulting;
				}
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_recursion = new ObjectRef<bool>();

			InitDependencies(sender);

			ISwitch formula = _Formula as ISwitch;
			if (formula != null)
			{
				formula.OuterField = sender.GetBqlField(_FieldName);
			}
			if (!sender.GetAttributesReadonly(_FieldName).Exists(attr => attr is PXDBFieldAttribute))
			{
				sender.Graph.RowSelecting.AddHandler(sender.GetItemType(), Graph_RowSelecting);
			}
			if (_Persistent)
			{
				sender.Graph.RowPersisted.AddHandler(sender.GetItemType(), Graph_RowPersisted);
			}

		}

		protected void SetFormulaValue(PXCache sender, object row)
		{
			if (_Formula != null)
				using (new PXConnectionScope())
				{
					PXFieldDefaultingEventArgs de = new PXFieldDefaultingEventArgs(row);
					FormulaDefaulting(sender, de);
					sender.SetValue(row, _FieldName, de.NewValue);
				}
		}

		public virtual void Graph_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Completed &&
				((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				 (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				SetFormulaValue(sender, e.Row);
			}
		}

		public void Graph_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			SetFormulaValue(sender, e.Row);
		}
		#endregion

	}
	#endregion

	#region PXRateSyncAttribute
	/// <summary>
	/// Synchronizes CuryRateID with the field to which this attribute is applied
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXRateSyncAttribute : PXEventSubscriberAttribute, IPXRowInsertingSubscriber, IPXRowSelectedSubscriber
	{
		#region State
		#endregion

		#region Ctor
		#endregion

		#region Implementation
		public void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			sender.SetValue(e.Row, _FieldOrdinal, Convert.ToInt32(sender.Graph.Accessinfo.CuryRateID));
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			sender.Graph.Accessinfo.CuryRateID = Convert.ToInt32(sender.GetValue(e.Row, _FieldOrdinal));
		}
		#endregion
	}
	#endregion

	#region PXDimensionAttribute
	/// <summary>
	/// Provides the segmented field functionality
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method)]
	[Serializable]
	public class PXDimensionAttribute : PXEventSubscriberAttribute,
		IPXFieldSelectingSubscriber,
		IPXFieldVerifyingSubscriber,
		IPXFieldDefaultingSubscriber,
		IPXRowPersistingSubscriber,
		IPXRowPersistedSubscriber,
		IPXFieldUpdatingSubscriber
	{
		#region State
		protected string _Dimension;
		protected bool? _ValidComboRequired;
		public GroupHelper.ParamsPair[][] Restrictions;
		protected Definition _Definition;
		protected Delegate _SegmentDelegate;
		protected string[] _SegmentParameters;
		public virtual void SetSegmentDelegate(Delegate handler)
		{
			_SegmentDelegate = handler;
			ParameterInfo[] pars = _SegmentDelegate.Method.GetParameters();
			_SegmentParameters = new string[pars.Length];
			for (int i = 0; i < pars.Length; i++)
			{
				_SegmentParameters[i] = pars[i].Name;
			}
		}
		private System.Collections.IEnumerable getOuterSegments(PXCache sender, short segment, object row)
		{
			object[] pars = new object[_SegmentParameters.Length];
			for (int i = 0; i < pars.Length; i++)
			{
				if (String.Equals(_SegmentParameters[i], "segment", StringComparison.OrdinalIgnoreCase))
				{
					pars[i] = segment;
				}
				else
				{
					pars[i] = sender.GetValueExt(row, _SegmentParameters[i]);
					if (pars[i] is PXFieldState)
					{
						pars[i] = ((PXFieldState)pars[i]).Value;
					}
				}
			}
			return new PXView(sender.Graph, true, new Select<SegmentValue>(), _SegmentDelegate).SelectMultiBound(new object[] { row }, pars);
		}
		public virtual bool ValidComboRequired
		{
			get
			{
				if (_ValidComboRequired == null)
				{
					if (_Definition == null)
					{
						Definition defs = PXContext.GetSlot<Definition>();
						if (defs == null)
						{
							PXContext.SetSlot<Definition>(defs = PXDatabase.GetSlot<Definition>("Definition", typeof(Dimension), typeof(Segment), typeof(SegmentValue)));
						}
						if (defs != null)
						{
							if (defs.Dimensions.Count == 0)
							{
								return true;
							}
							return defs.ValidCombos.Contains(_Dimension);
						}
						return true;
					}
					else
					{
						return _Definition.ValidCombos.Contains(_Dimension);
					}
				}
				return (bool)_ValidComboRequired;
			}
			set
			{
				_ValidComboRequired = value;
			}
		}
		protected string _Wildcard;
		public virtual string Wildcard
		{
			get
			{
				return _Wildcard;
			}
			set
			{
				_Wildcard = value;
			}
		}
		public static int GetLength(string dimensionID)
		{
			PXSegment[] segs;
			Definition def = PXDatabase.GetSlot<Definition>("Definition", typeof(Dimension), typeof(Segment), typeof(SegmentValue));
			if (def != null && def.Dimensions.TryGetValue(dimensionID, out segs))
			{
				int ret = 0;
				for (int i = 0; i < segs.Length; i++)
				{
					ret += segs[i].Length;
				}
				return ret;
			}
			return 0;
		}
		#endregion

		#region Ctor
		/// <summary>
		/// Creates a segmented field
		/// </summary>
		/// <param name="dimension">Dimension ID</param>
		public PXDimensionAttribute(string dimension)
			: base()
		{
			if (dimension == null)
			{
				throw new PXArgumentException("dimension", ErrorMessages.ArgumentNullException);
			}
			_Dimension = dimension;
		}
		private class SegDescr : PXSegment
		{
			public readonly string DimensionID;
			public readonly short SegmentID;
			public readonly string Descr;
			public readonly bool AutoNumber;
			public readonly string ParentDimensionID;
			public SegDescr(string dimensionID, short? segmentID, char editMask, char fillCharacter, short? length, bool? validate, short? caseConvert, short? align, char separator, bool? readOnly, string descr, string parentDimensionID)
				: base(editMask, fillCharacter, (short)length, (bool)validate, (short)caseConvert, (short)align, separator, false)
			{
				DimensionID = dimensionID;
				SegmentID = (short)segmentID;
				Descr = descr;
				AutoNumber = (bool)readOnly;
				ParentDimensionID = parentDimensionID;
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXSegmentedState.CreateInstance(e.ReturnState, _FieldName, _Definition != null && _Definition.Dimensions.ContainsKey(_Dimension) ? _Definition.Dimensions[_Dimension] : new PXSegment[0],
					!(e.ReturnState is PXFieldState) || String.IsNullOrEmpty(((PXFieldState)e.ReturnState).ViewName) ? "_" + _Dimension + "_Segments_" : null, ValidComboRequired, _Wildcard);
				((PXSegmentedState)e.ReturnState).IsUnicode = true;
				((PXSegmentedState)e.ReturnState).DescriptionName = typeof(SegmentValue.descr).Name;
			}
		}
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!_Definition.Dimensions.ContainsKey(_Dimension))
			{
				throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.DimensionDontExist, _Dimension));
			}
			PXSegment[] segs = _Definition.Dimensions[_Dimension];
			string val = e.NewValue as string;
			if (val == null)
			{
				return;
			}
			int start = 0;
			List<string> errs = new List<string>();
			for (int i = 0; i < segs.Length; i++)
			{
				if (((SegDescr)segs[i]).AutoNumber)
				{
					if (sender.Locate(e.Row) == null)
					{
						string oldValue = sender.GetValue(e.Row, _FieldOrdinal) as string;
						if (oldValue != null && start < oldValue.Length && start < val.Length)
						{
							string def;
							if (start + segs[i].Length <= oldValue.Length)
							{
								def = oldValue.Substring(start, segs[i].Length);
							}
							else
							{
								def = oldValue.Substring(start);
							}
							if (start + segs[i].Length <= val.Length)
							{
								e.NewValue = val.Substring(0, start) + def + val.Substring(start + segs[i].Length);
							}
							else
							{
								e.NewValue = val.Substring(0, start) + def;
							}
						}
					}
				}
				else if (segs[i].Validate)
				{
					string curr;
					if (start < val.Length)
					{
						if (start + segs[i].Length <= val.Length)
						{
							curr = val.Substring(start, segs[i].Length);
						}
						else
						{
							curr = val.Substring(start);
						}
					}
					else
					{
						curr = String.Empty;
					}
					Dictionary<string, ValueDescr> vals = _Definition.Values[_Dimension][((SegDescr)segs[i]).SegmentID];
					if (!vals.ContainsKey(curr) && !vals.ContainsKey(curr = curr.TrimEnd()))
					{
						if (!String.IsNullOrEmpty(_Wildcard))
						{
							bool fullwild = false;
							for (int k = 0; k < _Wildcard.Length; k++)
							{
								if (curr == new String(_Wildcard[k], segs[i].Length))
								{
									fullwild = true;
									break;
								}
							}
							if (fullwild)
							{
								continue;
							}
							bool meet = true;
							foreach (string key in vals.Keys)
							{
								meet = true;
								for (int j = 0; j < curr.Length; j++)
								{
									bool wild = false;
									for (int k = 0; k < _Wildcard.Length; k++)
									{
										if (curr[j] == _Wildcard[k])
										{
											wild = true;
											break;
										}
									}
									if (wild)
									{
										continue;
									}
									if (j >= key.Length || curr[j] != key[j])
									{
										meet = false;
										break;
									}
								}
								if (meet)
								{
									break;
								}
							}
							if (meet)
							{
								continue;
							}
						}
						errs.Add(String.IsNullOrEmpty(((SegDescr)segs[i]).Descr) ? ((SegDescr)segs[i]).SegmentID.ToString() : ((SegDescr)segs[i]).Descr);
					}
					else
					{
						bool match = true;
						if (Restrictions != null)
						{
							byte[] segmask = vals[curr].GroupMask;
							if (segmask != null)
							{
								for (int m = 0; m < Restrictions.Length; m++)
								{
									for (int l = 0; l < Restrictions[m].Length; l++)
									{
										int verified = 0;
										for (int j = l * 4; j < l * 4 + 4; j++)
										{
											verified = verified << 8;
											if (j < segmask.Length)
											{
												verified |= (int)segmask[j];
											}
										}
										if ((verified & Restrictions[m][l].First) != 0 && (verified & Restrictions[m][l].Second) == 0)
										{
											match = false;
											break;
										}
									}
									if (!match)
									{
										break;
									}
								}
							}
						}
						if (match && _SegmentDelegate != null)
						{
							match = false;
							foreach (SegmentValue sv in getOuterSegments(sender, ((SegDescr)segs[i]).SegmentID, e.Row))
							{
								if (String.Equals(sv.Value, curr, StringComparison.OrdinalIgnoreCase))
								{
									match = true;
									break;
								}
							}
						}
						if (!match)
						{
							errs.Add(String.IsNullOrEmpty(((SegDescr)segs[i]).Descr) ? ((SegDescr)segs[i]).SegmentID.ToString() : ((SegDescr)segs[i]).Descr);
						}
					}
				}
				start += segs[i].Length;
			}
			if (errs.Count > 0)
			{
				if (errs.Count == 1)
				{
					throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ElementOfFieldDoesntExist, errs[0], _FieldName));
				}
				errs.Add(_FieldName);
				StringBuilder bld = new StringBuilder();
				StringBuilder bld2 = new StringBuilder();
				int i;
				for (i = 0; i < errs.Count - 1; i++)
				{
					bld.Append('{');
					bld.Append(i);
					bld.Append('}');
					if (i < errs.Count - 2)
					{
						bld.Append(", ");
					}
				}
				bld2.Append('{');
				bld2.Append(i);
				bld2.Append('}');

				string localstring = PXMessages.LocalizeFormat(ErrorMessages.ElementsOfFieldsDontExist, bld.ToString(), bld2.ToString());
				throw new PXSetPropertyException(String.Format(localstring, errs.ToArray()));
			}
		}
		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			string numbering;
			if (e.NewValue == null && (_Definition.Autonumbers.TryGetValue(_Dimension, out numbering) || _SegmentDelegate != null && e.Row != null))
			{
				e.NewValue = prepValue("", "");
				if (_SegmentDelegate != null && e.NewValue is string)
				{
					int start = 0;
					for (int i = 0; i < _Definition.Dimensions[_Dimension].Length; i++)
					{
						PXSegment seg = _Definition.Dimensions[_Dimension][i];
						if (seg.Validate)
						{
							string theOnly = null;
							foreach (SegmentValue sv in getOuterSegments(sender, ((SegDescr)seg).SegmentID, e.Row))
							{
								if (theOnly != null)
								{
									theOnly = null;
									break;
								}
								theOnly = sv.Value;
							}
							if (theOnly != null)
							{
								if (theOnly.Length < seg.Length)
								{
									theOnly = theOnly + new String(' ', seg.Length - theOnly.Length);
								}
								else if (theOnly.Length > seg.Length)
								{
									theOnly = theOnly.Substring(0, seg.Length);
								}
								((string)e.NewValue).Remove(start, seg.Length).Insert(start, theOnly);
							}
						}
					}
				}
			}
		}
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (!_Definition.Dimensions.ContainsKey(_Dimension))
			{
				throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.DimensionDontExist, _Dimension));
			}
			PXSegment[] segs = _Definition.Dimensions[_Dimension];
			string val = e.NewValue as string;
			if (val == null)
			{
				return;
			}
			int grandtotal = 0;
			bool trimRequired = false;
			for (int i = 0; i < segs.Length; i++)
			{
				trimRequired = trimRequired || segs[i].Align == (short)0 && grandtotal < val.Length && val[grandtotal] == segs[i].FillCharacter;
				grandtotal += segs[i].Length;
				if (i == segs.Length - 1 && grandtotal > val.Length)
				{
					e.NewValue = val + new string(segs[i].FillCharacter, grandtotal - val.Length);
				}
			}
			if (trimRequired)
			{
				char[] arr = ((string)e.NewValue).ToCharArray();
				int total = 0;
				for (int i = 0; i < segs.Length; i++)
				{
					if (segs[i].Align == (short)0 && arr[total] == segs[i].FillCharacter)
					{
						int j = total + 1;
						for (; j < total + segs[i].Length; j++)
						{
							if (arr[j] != segs[i].FillCharacter)
							{
								break;
							}
						}
						if (j < total + segs[i].Length)
						{
							for (int k = 0; k < segs[i].Length - j + total; k++)
							{
								arr[total + k] = arr[j + k];
								arr[j + k] = segs[i].FillCharacter;
							}
						}
					}
					total += segs[i].Length;
				}
				e.NewValue = new String(arr);
			}
			if (((string)e.NewValue).Length > grandtotal)
			{
				e.NewValue = ((string)e.NewValue).Substring(0, grandtotal);
			}
			e.Cancel = true;
		}
		public virtual void SelfRowSelecting(PXCache sender, PXRowSelectingEventArgs e, int length)
		{
			object val = sender.GetValue(e.Row, _FieldOrdinal);
			if (val is string)
			{
				string sval = (string)val;
				if (sval.Length < length)
				{
					sender.SetValue(e.Row, _FieldOrdinal, sval + new String(' ', length - sval.Length));
				}
				else if (sval.Length > length)
				{
					sender.SetValue(e.Row, _FieldOrdinal, sval.Substring(0, length));
				}
			}
		}

		private string prepValue(string value, string symbol)
		{
			StringBuilder bld = new StringBuilder();
			int pos = 0;
			for (int i = 0; i < _Definition.Dimensions[_Dimension].Length; i++)
			{
				PXSegment seg = _Definition.Dimensions[_Dimension][i];
				if (!((SegDescr)seg).AutoNumber)
				{
					if (pos + seg.Length == value.Length)
					{
						bld.Append(value.Substring(pos));
					}
					else if (pos + seg.Length < value.Length)
					{
						bld.Append(value.Substring(pos, seg.Length));
					}
					else if (pos < value.Length)
					{
						bld.Append(value.Substring(pos));
						bld.Append(seg.FillCharacter, pos + seg.Length - value.Length);
					}
					else
					{
						bld.Append(seg.FillCharacter, seg.Length);
					}
				}
				else
				{
					if (symbol == "")
					{
						foreach (string s in _Definition.Values[_Dimension][((SegDescr)seg).SegmentID].Keys)
						{
							symbol = s;
							break;
						}
					}
					if (symbol.Length == seg.Length)
					{
						bld.Append(symbol);
					}
					else if (symbol.Length > seg.Length)
					{
						bld.Append(symbol.Substring(0, seg.Length));
					}
					else if (seg.Align == (short)0)
					{
						bld.Append(symbol);
						bld.Append(seg.FillCharacter, seg.Length - symbol.Length);
					}
					else
					{
						bld.Append(seg.FillCharacter, seg.Length - symbol.Length);
						bld.Append(symbol);
					}
				}
				pos += (int)seg.Length;
			}
			return bld.ToString();
		}

		string LastNbr;
		string NewNumber;
		int NumberingSEQ;
		string Numbering;

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Insert)
			{
				return;
			}
			string val = sender.GetValue(e.Row, _FieldOrdinal) as string;
			if (val != null && val.Trim() == "" && sender.Keys.Contains(_FieldName))
			{
				if (sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyKeepPreviousException(PXMessages.LocalizeFormat(ErrorMessages.FieldIsEmpty, _FieldName))))
				{
					throw new PXRowPersistingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
				}
				return;
			}
			if (!_Definition.Autonumbers.TryGetValue(_Dimension, out Numbering))
			{
				return;
			}

			int? NBranchID;
			string StartNbr;
			string EndNbr;
			string WarnNbr;
			int NbrStep;
			DateTime? StartDate;
			Guid? CreatedByID;
			string CreatedByScreenID;
			DateTime? CreatedDateTime;
			Guid? LastModifiedByID;
			string LastModifiedByScreenID;
			DateTime? LastModifiedDateTime;

			using (PXDataRecord record = PXDatabase.SelectSingle<NumberingSequence>(
				new PXDataField("EndNbr"),
				new PXDataField("COALESCE(LastNbr, StartNbr)"),
				new PXDataField("WarnNbr"),
				new PXDataField("NbrStep"),
				new PXDataField("NumberingSEQ"),
				new PXDataField("NBranchID"),
				new PXDataField("StartNbr"),
				new PXDataField("StartDate"),
				new PXDataField("CreatedByID"),
				new PXDataField("CreatedByScreenID"),
				new PXDataField("CreatedDateTime"),
				new PXDataField("LastModifiedByID"),
				new PXDataField("LastModifiedByScreenID"),
				new PXDataField("LastModifiedDateTime"),
				new PXDataFieldValue("NumberingID", PXDbType.VarChar, 10, Numbering),
				new PXDataFieldValue("StartDate", PXDbType.DateTime, 4, sender.Graph.Accessinfo.BusinessDate, PXComp.LE),
				new PXDataFieldValue("NBranchID", PXDbType.Int, 4, sender.Graph.Accessinfo.BranchID, PXComp.EQorISNULL),
				new PXDataFieldOrder("NBranchID", true),
				new PXDataFieldOrder("StartDate", true)
				))
			{
				if (record == null)
				{
					throw new PXException(ErrorMessages.CantAutoNumber);
				}
				EndNbr = record.GetString(0);
				LastNbr = record.GetString(1);
				WarnNbr = record.GetString(2);
				NbrStep = (int)record.GetInt32(3);
				NumberingSEQ = (int)record.GetInt32(4);
				NBranchID = (int?)record.GetInt32(5);
				StartNbr = record.GetString(6);
				StartDate = record.GetDateTime(7);
				CreatedByID = record.GetGuid(8);
				CreatedByScreenID = record.GetString(9);
				CreatedDateTime = record.GetDateTime(10);
				LastModifiedByID = record.GetGuid(11);
				LastModifiedByScreenID = record.GetString(12);
				LastModifiedDateTime = record.GetDateTime(13);
			}
			NewNumber = nextNumber(LastNbr, NbrStep);

			if (NewNumber.CompareTo(WarnNbr) >= 0)
			{
				//throw new PXException(Messages.WarningNumReached);
				PXUIFieldAttribute.SetWarning(sender, e.Row, _FieldName, ErrorMessages.WarningNumReached);
			}

			if (NewNumber.CompareTo(EndNbr) >= 0)
			{
				throw new PXException(ErrorMessages.EndOfNumberingReached);
			}

			try
			{
				PXDatabase.Update<NumberingSequence>(
					new PXDataFieldAssign("LastNbr", NewNumber),
					new PXDataFieldRestrict("NumberingID", Numbering),
					new PXDataFieldRestrict("NumberingSEQ", NumberingSEQ),
					PXDataFieldRestrict.OperationSwitchAllowed);
			}
			catch (PXDatabaseException ex)
			{
				if (ex.ErrorCode == PXDbExceptions.OperationSwitchRequired)
				{
					PXDatabase.Insert<NumberingSequence>(
						new PXDataFieldAssign("EndNbr", PXDbType.VarChar, 15, EndNbr),
						new PXDataFieldAssign("LastNbr", PXDbType.VarChar, 15, NewNumber),
						new PXDataFieldAssign("WarnNbr", PXDbType.VarChar, 15, WarnNbr),
						new PXDataFieldAssign("NbrStep", PXDbType.Int, 4, NbrStep),
						new PXDataFieldAssign("StartNbr", PXDbType.VarChar, 15, StartNbr),
						new PXDataFieldAssign("StartDate", PXDbType.DateTime, StartDate),
						new PXDataFieldAssign("CreatedByID", PXDbType.UniqueIdentifier, 16, CreatedByID),
						new PXDataFieldAssign("CreatedByScreenID", PXDbType.Char, 8, CreatedByScreenID),
						new PXDataFieldAssign("CreatedDateTime", PXDbType.DateTime, 8, CreatedDateTime),
						new PXDataFieldAssign("LastModifiedByID", PXDbType.UniqueIdentifier, 16, LastModifiedByID),
						new PXDataFieldAssign("LastModifiedByScreenID", PXDbType.Char, 8, LastModifiedByScreenID),
						new PXDataFieldAssign("LastModifiedDateTime", PXDbType.DateTime, 8, LastModifiedDateTime),
						new PXDataFieldAssign("NumberingID", PXDbType.VarChar, 10, Numbering),
						new PXDataFieldAssign("NBranchID", PXDbType.Int, 4, NBranchID)
					);
				}
				else
				{
					throw;
				}
			}

			string oldVal = sender.GetValue(e.Row, _FieldOrdinal) as string;
			if (oldVal == null)
			{
				oldVal = "";
			}
			sender.SetValue(e.Row, _FieldOrdinal, prepValue(oldVal, NewNumber));
		}

		private static string nextNumber(string str, int count)
		{
			int i;
			bool j = true;
			int intcount = count;

			StringBuilder bld = new StringBuilder();
			for (i = str.Length; i > 0; i--)
			{
				string c = str.Substring(i - 1, 1);

				if (System.Text.RegularExpressions.Regex.IsMatch(c, "[^0-9]"))
				{
					j = false;
				}

				if (j && System.Text.RegularExpressions.Regex.IsMatch(c, "[0-9]"))
				{
					int digit = Convert.ToInt16(c);

					string s_count = Convert.ToString(intcount);
					int digit2 = Convert.ToInt16(s_count.Substring(s_count.Length - 1, 1));

					bld.Append((digit + digit2) % 10);

					intcount -= digit2;
					intcount += ((digit + digit2) - (digit + digit2) % 10);

					intcount /= 10;

					if (intcount == 0)
					{
						j = false;
					}
				}
				else
				{
					bld.Append(c);
				}
			}

			if (intcount != 0)
			{
				throw new ArithmeticException("");
			}

			char[] chars = bld.ToString().ToCharArray();
			Array.Reverse(chars);
			return new string(chars);
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Aborted)
			{
				object val = sender.GetValue(e.Row, _FieldOrdinal);
				string oldVal = val as string;
				if (oldVal != null || val == null)
				{
					if (oldVal == null)
					{
						oldVal = "";
					}
					try
					{
						sender.SetValue(e.Row, _FieldOrdinal, prepValue(oldVal, ""));
					}
					catch (InvalidCastException)
					{
					}
					if (e.Exception is PXLockViolationException
						&& !String.IsNullOrEmpty(oldVal)
						&& !String.IsNullOrEmpty(NewNumber)
						&& String.Equals(oldVal, prepValue(oldVal, NewNumber)))
					{
						try
						{
							PXDatabase.Update<NumberingSequence>(
								new PXDataFieldAssign("LastNbr", NewNumber),
								new PXDataFieldRestrict("NumberingID", Numbering),
								new PXDataFieldRestrict("NumberingSEQ", NumberingSEQ),
								new PXDataFieldRestrict("LastNbr", LastNbr));
							((PXLockViolationException)e.Exception).Retry = true;
						}
						catch
						{
						}
					}
				}
			}
		}
		#endregion

		#region Initialization
		protected internal sealed class ValueDescr
		{
			public readonly string Descr;
			public readonly bool? IsConsolidatedValue;
			public readonly byte[] GroupMask;
			public ValueDescr(string descr, bool? isConsolidatedValue, byte[] groupMask)
			{
				Descr = descr;
				IsConsolidatedValue = isConsolidatedValue;
				GroupMask = groupMask;
			}
		}
		[Serializable]
		public sealed class SegmentValue : PX.Data.IBqlTable
		{
			public abstract class value : PX.Data.IBqlField
			{
			}
			private String _Value;
			[PXDBString(30, IsKey = true, InputMask = "")]
			[PXUIField(DisplayName = "Value", Visibility = PXUIVisibility.SelectorVisible)]
			public String Value
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
			public abstract class descr : PX.Data.IBqlField
			{
			}
			private String _Descr;
			[PXDBString(50)]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
			public String Descr
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
			#region IsConsolidatedValue
			public abstract class isConsolidatedValue : PX.Data.IBqlField
			{
			}
			private Boolean? _IsConsolidatedValue;
			[PXDBBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Aggregation")]
			public Boolean? IsConsolidatedValue
			{
				get
				{
					return this._IsConsolidatedValue;
				}
				set
				{
					this._IsConsolidatedValue = value;
				}
			}
			#endregion
			public SegmentValue(string value, string descr, bool? isConsolidatedValue)
			{
				_Value = value;
				_Descr = descr;
				_IsConsolidatedValue = isConsolidatedValue;
			}
			public SegmentValue()
			{
			}
		}
		protected class Definition : IPrefetchable
		{
			public Dictionary<string, PXSegment[]> Dimensions = new Dictionary<string, PXSegment[]>();
			public Dictionary<string, Dictionary<string, ValueDescr>[]> Values = new Dictionary<string, Dictionary<string, ValueDescr>[]>();
			public List<string> ValidCombos = new List<string>();
			public Dictionary<string, string> Autonumbers = new Dictionary<string, string>();
			public void Prefetch()
			{
				try
				{
					var dimChilds = new Dictionary<string, string>();
					var dimParents = new List<string>();
					foreach (PXDataRecord record in PXDatabase.SelectMulti<Dimension>(
						new PXDataField("DimensionID"),
						new PXDataField("Validate"),
						new PXDataField("NumberingID"),
						new PXDataField("ParentDimensionID")
						))
					{
						string dimension = record.GetString(0);
						if (record.GetBoolean(1) == true)
						{
							ValidCombos.Add(dimension);
						}
						string numbering = record.GetString(2);
						if (!String.IsNullOrEmpty(numbering))
						{
							Autonumbers.Add(dimension, numbering);
						}
						string parentDimensionId = record.GetString(3);
						if (!string.IsNullOrEmpty(parentDimensionId))
						{
							dimChilds[dimension] = parentDimensionId;
							if (!dimParents.Contains(parentDimensionId))
								dimParents.Add(parentDimensionId);
						}
					}
					foreach (KeyValuePair<string, string> pair in dimChilds)
					{
						var child = pair.Key;
						var parent = pair.Value;
						string parentAutonumbers;
						if (!Autonumbers.ContainsKey(child) && Autonumbers.TryGetValue(parent, out parentAutonumbers))
							Autonumbers.Add(child, parentAutonumbers);
					}
					var childSegments = new Dictionary<string, IList<short>>(dimChilds.Count);
					foreach (KeyValuePair<string, string> pair in dimChilds)
						childSegments.Add(pair.Key, new List<short>());
					var parentSegments = new Dictionary<string, IDictionary<short, SegDescr>>(dimParents.Count);
					foreach (string item in dimParents)
						parentSegments.Add(item, new Dictionary<short, SegDescr>());
					List<SegDescr> list = new List<SegDescr>();
					foreach (PXDataRecord record in PXDatabase.SelectMulti<Segment>(
						new PXDataField("DimensionID"),
						new PXDataField("SegmentID"),
						new PXDataField("EditMask"),
						new PXDataField("FillCharacter"),
						new PXDataField("Length"),
						new PXDataField("Validate"),
						new PXDataField("CaseConvert"),
						new PXDataField("Align"),
						new PXDataField("Separator"),
						new PXDataField("AutoNumber"),
						new PXDataField("Descr"),
						new PXDataField("ParentDimensionID")
						))
					{
						var segDescr = new SegDescr(
							record.GetString(0),
							record.GetInt16(1),
							record.GetString(2)[0],
							' ',
							record.GetInt16(4),
							record.GetBoolean(5),
							record.GetInt16(6),
							record.GetInt16(7),
							record.GetString(8)[0],
							record.GetBoolean(9),
							record.GetString(10),
							record.GetString(11)
							);
						list.Add(segDescr);
						if (dimChilds.ContainsKey(segDescr.DimensionID) && segDescr.ParentDimensionID != null)
							childSegments[segDescr.DimensionID].Add(segDescr.SegmentID);
						if (dimParents.Contains(segDescr.DimensionID))
							parentSegments[segDescr.DimensionID].Add(segDescr.SegmentID, segDescr);
					}
					foreach (KeyValuePair<string, string> item in dimChilds)
					{
						var childDimensionId = item.Key;
						var existSegments = childSegments[childDimensionId];
						var parentDimensionId = item.Value;
						foreach (KeyValuePair<short, SegDescr> pair in parentSegments[parentDimensionId])
						{
							var segmentId = pair.Key;
							if (!existSegments.Contains(segmentId))
							{
								var parentSegDescr = pair.Value;
								list.Add(new SegDescr(childDimensionId,
									segmentId,
									parentSegDescr.EditMask,
									parentSegDescr.FillCharacter,
									parentSegDescr.Length,
									parentSegDescr.Validate,
									parentSegDescr.CaseConvert,
									parentSegDescr.Align,
									parentSegDescr.Separator,
									parentSegDescr.AutoNumber,
									parentSegDescr.Descr,
									parentSegDescr.ParentDimensionID));
							}
						}
					}
					foreach (string auto in new List<String>(Autonumbers.Keys))
					{
						bool found = false;
						for (int i = 0; i < list.Count; i++)
						{
							if (String.Equals(auto, list[i].DimensionID, StringComparison.OrdinalIgnoreCase)
								&& list[i].AutoNumber)
							{
								found = true;
								break;
							}
						}
						if (!found)
						{
							Autonumbers.Remove(auto);
						}
					}
					list.Sort(delegate(SegDescr a, SegDescr b)
					{
						int cmp = String.Compare(a.DimensionID, b.DimensionID, StringComparison.OrdinalIgnoreCase);
						if (cmp != 0)
						{
							return cmp;
						}
						return a.SegmentID.CompareTo(b.SegmentID);
					}
					);
					int start = 0;
					for (int i = 0; i < list.Count; i++)
					{
						if (list[start].DimensionID != list[i].DimensionID)
						{
							PXSegment[] segs = new PXSegment[i - start];
							short max = -1;
							for (int j = 0; j < i - start; j++)
							{
								segs[j] = list[start + j];
								if (list[start + j].SegmentID > max)
								{
									max = list[start + j].SegmentID;
								}
							}
							Dimensions[list[start].DimensionID] = segs;
							max++;
							Dictionary<string, ValueDescr>[] dicts = new Dictionary<string, ValueDescr>[max];
							for (int j = 0; j < max; j++)
							{
								dicts[j] = new Dictionary<string, ValueDescr>();
							}
							Values[list[start].DimensionID] = dicts;
							start = i;
						}
					}
					if (start < list.Count)
					{
						PXSegment[] segs = new PXSegment[list.Count - start];
						short max = -1;
						for (int j = 0; j < list.Count - start; j++)
						{
							segs[j] = list[start + j];
							if (list[start + j].SegmentID > max)
							{
								max = list[start + j].SegmentID;
							}
						}
						Dimensions[list[start].DimensionID] = segs;
						max++;
						Dictionary<string, ValueDescr>[] dicts = new Dictionary<string, ValueDescr>[max];
						for (int j = 0; j < max; j++)
						{
							dicts[j] = new Dictionary<string, ValueDescr>();
						}
						Values[list[start].DimensionID] = dicts;
					}
					foreach (PXDataRecord record in PXDatabase.SelectMulti<SegmentValue>(
						new PXDataField("DimensionID"),
						new PXDataField("SegmentID"),
						new PXDataField("Value"),
						new PXDataField("Descr"),
						new PXDataField(GroupHelper.FieldName),
						new PXDataField("IsConsolidatedValue"),
						new PXDataFieldValue("Active", PXDbType.Bit, 1)
						))
					{
						byte[] mask = record.GetBytes(4);
						string dimension = record.GetString(0);
						PXSegment[] segs;
						if (Dimensions.TryGetValue(dimension, out segs))
						{
							int segment = (int)record.GetInt16(1);
							Dictionary<string, ValueDescr>[] arr = Values[dimension];
							if (arr != null && segment < arr.Length)
							{
								if (mask != null)
								{
									if (segment > segs.Length || !segs[segment - 1].Validate)
									{
										mask = null;
									}
									else
									{
										bool anyNonZero = false;
										for (int i = 0; i < mask.Length; i++)
										{
											if (mask[i] != (byte)0)
											{
												anyNonZero = true;
												break;
											}
										}
										if (!anyNonZero)
										{
											mask = null;
										}
									}
								}
								arr[segment][record.GetString(2)] = new ValueDescr(record.GetString(3), record.GetBoolean(5), mask);
							}
						}
					}
					foreach (KeyValuePair<string, string> item in dimChilds)
					{
						var childDimensionId = item.Key;
						var existSegments = childSegments[childDimensionId];
						var childValues = Values[childDimensionId];
						var parentDimensionId = item.Value;
						var parentValues = Values[parentDimensionId];
						foreach (KeyValuePair<short, SegDescr> pair in parentSegments[parentDimensionId])
						{
							var segmentId = pair.Key;
							if (!existSegments.Contains(segmentId))
							{
								var dic = childValues[segmentId];
								foreach (KeyValuePair<string, ValueDescr> parentValueDescr in parentValues[segmentId])
									if (!dic.ContainsKey(parentValueDescr.Key))
										dic.Add(parentValueDescr.Key, parentValueDescr.Value);
							}
						}
					}
				}
				catch
				{
					Dimensions.Clear();
					Values.Clear();
					ValidCombos.Clear();
					Autonumbers.Clear();
					throw;
				}
			}
		}
		protected class Dimension : IBqlTable
		{
		}
		protected class Segment : IBqlTable
		{
		}
		protected class NumberingSequence : IBqlTable
		{
		}
		public static string[] GetSegmentValues(string dimensionid, int segmentnumber)
		{
			Definition defs = PXDatabase.GetSlot<Definition>("Definition", typeof(Dimension), typeof(Segment), typeof(SegmentValue));
			Dictionary<string, ValueDescr>[] vals;
			if (String.IsNullOrEmpty(dimensionid) || segmentnumber < 0 || !defs.Values.TryGetValue(dimensionid, out vals) || vals.Length <= segmentnumber)
			{
				return new string[0];
			}
			string[] ret = new string[vals[segmentnumber].Keys.Count];
			vals[segmentnumber].Keys.CopyTo(ret, 0);
			return ret;
		}
		public static void Clear()
		{
			PXDatabase.ResetSlot<Definition>("Definition", typeof(Dimension), typeof(Segment), typeof(SegmentValue));
		}
		public override void CacheAttached(PXCache sender)
		{
			_Definition = PXContext.GetSlot<Definition>();
			if (_Definition == null)
			{
				PXContext.SetSlot<Definition>(_Definition = PXDatabase.GetSlot<Definition>("Definition", typeof(Dimension), typeof(Segment), typeof(SegmentValue)));
			}

			sender.Graph.Views["_" + _Dimension + "_Segments_"] =
				new PXView(sender.Graph, true, new Select<SegmentValue>(), GetSegmentDelegate());

			if (_Definition != null)
			{
				if (!_Definition.Dimensions.ContainsKey(_Dimension))
				{
					throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ValueDoesntExist, "Segmented Key", _Dimension));
				}
				PXSegment[] segs = _Definition.Dimensions[_Dimension];
				int total = 0;
				for (int i = 0; i < segs.Length; i++)
				{
					total += segs[i].Length;
				}
				sender.RowSelecting += delegate(PXCache cache, PXRowSelectingEventArgs e)
				{
					SelfRowSelecting(cache, e, total);
				};
			}
		}
		protected internal Delegate GetSegmentDelegate()
		{
			if (_SegmentDelegate != null)
			{
				return _SegmentDelegate;
			}
			return (PXSelectDelegate<short?>)SegmentSelect;
		}
		public System.Collections.IEnumerable SegmentSelect(
			[PXShort]
			short? segment
		)
		{
			if (!_Definition.Dimensions.ContainsKey(_Dimension) || segment == null || segment < 0 || segment >= _Definition.Values[_Dimension].Length)
			{
				yield break;
			}
			foreach (KeyValuePair<string, ValueDescr> pair in _Definition.Values[_Dimension][(int)segment])
			{
				if (Restrictions == null || pair.Value.GroupMask == null)
				{
					yield return new SegmentValue(pair.Key, pair.Value.Descr, pair.Value.IsConsolidatedValue);
				}
				else
				{
					byte[] segmask = pair.Value.GroupMask;
					bool match = true;
					for (int k = 0; k < Restrictions.Length; k++)
					{
						for (int i = 0; i < Restrictions[k].Length; i++)
						{
							int verified = 0;
							for (int j = i * 4; j < i * 4 + 4; j++)
							{
								verified = verified << 8;
								if (j < segmask.Length)
								{
									verified |= (int)segmask[j];
								}
							}
							if ((verified & Restrictions[k][i].First) != 0 && (verified & Restrictions[k][i].Second) == 0)
							{
								match = false;
								break;
							}
						}
						if (!match)
						{
							break;
						}
					}
					if (match)
					{
						yield return new SegmentValue(pair.Key, pair.Value.Descr, pair.Value.IsConsolidatedValue);
					}
				}
			}
		}
		#endregion
	}
	#endregion

	#region PXDBBinaryAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBBinaryAttribute : PXDBFieldAttribute
	{
		#region State
		protected int _Length = -1;
		protected bool _IsFixed = false;
		public bool IsFixed
		{
			get
			{
				return _IsFixed;
			}
			set
			{
				_IsFixed = value;
			}
		}
		public int Length
		{
			get
			{
				return _Length;
			}
		}
		#endregion

		#region Ctor
		public PXDBBinaryAttribute()
		{
		}
		public PXDBBinaryAttribute(int length)
		{
			_Length = length;
		}
		#endregion

		#region Implementation
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (_DatabaseFieldName != null)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName;
				e.FieldName = ((e.Operation & PXDBOperation.Option) == PXDBOperation.GroupBy) ? BqlCommand.Null : e.FieldName;
				//e.FieldName = (e.Table == null ? _DatabaseFieldName : e.Table.Name + '.' + _DatabaseFieldName);
			}
			e.DataType = _IsFixed ? PXDbType.Binary : PXDbType.VarBinary;
			e.DataValue = (e.Value == null ? new byte[0] : e.Value);
			if (_Length > -1)
			{
				e.DataLength = _Length;
			}
			e.IsRestriction = e.IsRestriction || _IsKey;
		}
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				byte[] buff;
				if (_Length > -1)
				{
					buff = new byte[_Length];
					e.Record.GetBytes(e.Position, 0, buff, 0, _Length);
				}
				else
				{
					buff = e.Record.GetBytes(e.Position);
				}
				if (buff == null)
				{
					buff = new byte[0];
				}
				sender.SetValue(e.Row, _FieldOrdinal, buff);
			}
			e.Position++;
		}
		#endregion
	}
	#endregion

	#region PXDBGroupMaskAttribute
	public class PXDBGroupMaskAttribute : PXDBBinaryAttribute
	{
		#region Ctor
		public PXDBGroupMaskAttribute()
			: base()
		{
		}
		public PXDBGroupMaskAttribute(int length)
			: base(length)
		{
		}
		#endregion

		#region Implementation
		public virtual void securedFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnValue = false;
			if (e.Row != null)
			{
				byte[] mask = sender.GetValue(e.Row, _FieldOrdinal) as byte[];
				if (mask != null)
				{
					foreach (byte b in mask)
					{
						if (b != 0x00)
						{
							e.ReturnValue = true;
							break;
						}
					}
				}
			}
			else
			{
				e.ReturnState = PXFieldState.CreateInstance(null, typeof(bool), false, true, null, null, null, null, "Secured", null, PXMessages.Localize(ActionsMessages.Secured), null, PXErrorLevel.Undefined, false, true, null, PXUIVisibility.Invisible, null, null, null);
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (!sender.Fields.Contains("Secured"))
			{
				sender.Fields.Add("Secured");
				sender.FieldSelectingEvents["secured"] += securedFieldSelecting;
			}
		}
		#endregion
	}
	#endregion

	#region PXDBVariantAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBVariantAttribute : PXDBBinaryAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region Ctor
		public PXDBVariantAttribute()
			: base()
		{
		}

		public PXDBVariantAttribute(int length)
			: base(length)
		{
		}
		#endregion

		#region Implementation
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null && e.ReturnValue != null && e.ReturnValue.GetType() == typeof(byte[]))
				e.ReturnValue = GetValue((byte[])e.ReturnValue);
		}

		public static object GetValue(byte[] val)
		{
			if (val.Length > 0)
			{
				using (System.IO.MemoryStream ms = new System.IO.MemoryStream(val, 1, val.Length - 1))
				using (System.IO.BinaryReader br = new System.IO.BinaryReader(ms, Encoding.Unicode))
				{
					switch ((TypeCode)(val[0]))
					{
						case TypeCode.Boolean:
							return br.ReadBoolean();
						case TypeCode.Byte:
							return br.ReadByte();
						case TypeCode.Char:
							return br.ReadChar();
						case TypeCode.DateTime:
							return new DateTime(br.ReadInt64());
						case TypeCode.Decimal:
							return br.ReadDecimal();
						case TypeCode.Double:
							return br.ReadDouble();
						case TypeCode.Int16:
							return br.ReadInt16();
						case TypeCode.Int32:
							return br.ReadInt32();
						case TypeCode.Int64:
							return br.ReadInt64();
						case TypeCode.SByte:
							return br.ReadSByte();
						case TypeCode.Single:
							return br.ReadSingle();
						case TypeCode.String:
							return br.ReadString();
						case TypeCode.UInt16:
							return br.ReadUInt16();
						case TypeCode.UInt32:
							return br.ReadUInt32();
						case TypeCode.UInt64:
							return br.ReadUInt64();
						default:
							throw new PXException(ErrorMessages.CantRestoreValueFromByteArray);
					}
				}
			}
			return null;
		}

		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null)
			{
				if (e.NewValue != null && e.NewValue.GetType() != typeof(byte[]))
				{
					e.NewValue = SetValue(e.NewValue);
				}
			}
		}

		public static byte[] SetValue(object value)
		{
			TypeCode c = Type.GetTypeCode(value.GetType());
			using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
			using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, Encoding.Unicode))
			{
				bw.Write((byte)c);
				switch (c)
				{
					case TypeCode.Boolean:
						bw.Write((Boolean)value);
						return ms.ToArray();
					case TypeCode.Byte:
						bw.Write((Byte)value);
						return ms.ToArray();
					case TypeCode.Char:
						bw.Write((Char)value);
						return ms.ToArray();
					case TypeCode.DateTime:
						bw.Write(((DateTime)value).Ticks);
						return ms.ToArray();
					case TypeCode.Decimal:
						bw.Write((Decimal)value);
						return ms.ToArray();
					case TypeCode.Double:
						bw.Write((Double)value);
						return ms.ToArray();
					case TypeCode.Int16:
						bw.Write((Int16)value);
						return ms.ToArray();
					case TypeCode.Int32:
						bw.Write((Int32)value);
						return ms.ToArray();
					case TypeCode.Int64:
						bw.Write((Int64)value);
						return ms.ToArray();
					case TypeCode.SByte:
						bw.Write((SByte)value);
						return ms.ToArray();
					case TypeCode.Single:
						bw.Write((Single)value);
						return ms.ToArray();
					case TypeCode.String:
						bw.Write((String)value);
						return ms.ToArray();
					case TypeCode.UInt16:
						bw.Write((UInt16)value);
						return ms.ToArray();
					case TypeCode.UInt32:
						bw.Write((UInt32)value);
						return ms.ToArray();
					case TypeCode.UInt64:
						bw.Write((UInt64)value);
						return ms.ToArray();
					default:
						throw new PXException(ErrorMessages.CantConvertValueToByteArray);
				}
			}
		}

		#endregion
	}
	#endregion

	#region PXVariantAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXVariantAttribute : PXEventSubscriberAttribute, IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
	{
		#region Ctor
		public PXVariantAttribute() : base() { }
		#endregion

		#region Implementation
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null && e.ReturnValue != null && e.ReturnValue.GetType() == typeof(byte[]))
				e.ReturnValue = GetValue((byte[])e.ReturnValue);
		}

		public static object GetValue(byte[] val)
		{
			if (val.Length > 0)
			{
				using (System.IO.MemoryStream ms = new System.IO.MemoryStream(val, 1, val.Length - 1))
				using (System.IO.BinaryReader br = new System.IO.BinaryReader(ms, Encoding.Unicode))
				{
					switch ((TypeCode)(val[0]))
					{
						case TypeCode.Boolean:
							return br.ReadBoolean();
						case TypeCode.Byte:
							return br.ReadByte();
						case TypeCode.Char:
							return br.ReadChar();
						case TypeCode.DateTime:
							return new DateTime(br.ReadInt64());
						case TypeCode.Decimal:
							return br.ReadDecimal();
						case TypeCode.Double:
							return br.ReadDouble();
						case TypeCode.Int16:
							return br.ReadInt16();
						case TypeCode.Int32:
							return br.ReadInt32();
						case TypeCode.Int64:
							return br.ReadInt64();
						case TypeCode.SByte:
							return br.ReadSByte();
						case TypeCode.Single:
							return br.ReadSingle();
						case TypeCode.String:
							return br.ReadString();
						case TypeCode.UInt16:
							return br.ReadUInt16();
						case TypeCode.UInt32:
							return br.ReadUInt32();
						case TypeCode.UInt64:
							return br.ReadUInt64();
						default:
							throw new PXException(ErrorMessages.CantRestoreValueFromByteArray);
					}
				}
			}
			return null;
		}

		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null)
			{
				if (e.NewValue != null && e.NewValue.GetType() != typeof(byte[]))
				{
					TypeCode c = Type.GetTypeCode(e.NewValue.GetType());
					using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
					using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, Encoding.Unicode))
					{
						bw.Write((byte)c);
						switch (c)
						{
							case TypeCode.Boolean:
								bw.Write((Boolean)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.Byte:
								bw.Write((Byte)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.Char:
								bw.Write((Char)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.DateTime:
								bw.Write(((DateTime)e.NewValue).Ticks);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.Decimal:
								bw.Write((Decimal)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.Double:
								bw.Write((Double)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.Int16:
								bw.Write((Int16)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.Int32:
								bw.Write((Int32)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.Int64:
								bw.Write((Int64)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.SByte:
								bw.Write((SByte)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.Single:
								bw.Write((Single)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.String:
								bw.Write((String)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.UInt16:
								bw.Write((UInt16)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.UInt32:
								bw.Write((UInt32)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							case TypeCode.UInt64:
								bw.Write((UInt64)e.NewValue);
								e.NewValue = ms.ToArray();
								break;
							default:
								throw new PXException(ErrorMessages.CantConvertValueToByteArray);
						}
					}
				}
			}
		}
		#endregion
	}
	#endregion

	#region PXDimensionSelectorAttribute
	[PXAttributeFamily(typeof(PXSelectorAttribute))]
	public class PXDimensionSelectorAttribute : PXAggregateAttribute, IPXFieldVerifyingSubscriber, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber
	{
		#region State
		protected object _KeyToAbort;
		protected Dictionary<object, object> _Persisted;
		protected Dictionary<object, object> _SubstitutePersisted;
		protected object _JustPersistedKey;
		protected bool _SuppressViewCreation;
		public virtual void SetSegmentDelegate(Delegate handler)
		{
			((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).SetSegmentDelegate(handler);
		}
		public System.Collections.IEnumerable SegmentSelect(
			[PXShort]
			short? segment
		)
		{
			return ((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).SegmentSelect(segment);
		}
		/// <summary>
		/// Field of the referenced table that contains the description
		/// </summary>
		public virtual Type DescriptionField
		{
			get
			{
				return ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).DescriptionField;
			}
			set
			{
				((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).DescriptionField = value;
			}
		}
		public virtual bool CacheGlobal
		{
			get
			{
				return ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).CacheGlobal;
			}
			set
			{
				((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).CacheGlobal = value;
			}
		}
		public virtual bool Filterable
		{
			get
			{
				return ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).Filterable;
			}
			set
			{
				((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).Filterable = value;
			}
		}
		protected Type _SubstituteKey;
		protected bool _DirtyRead;
		public virtual bool DirtyRead
		{
			get
			{
				return ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).DirtyRead;
			}
			set
			{
				_DirtyRead = value;
				((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).DirtyRead = value;
			}
		}
		public virtual Type Field
		{
			get
			{
				return ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).Field;
			}
		}
		public virtual string[] Headers
		{
			get
			{
				return ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).Headers;
			}
			set
			{
				((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).Headers = value;
			}
		}
		public virtual bool ValidComboRequired
		{
			get
			{
				return ((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).ValidComboRequired;
			}
			set
			{
				((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).ValidComboRequired = value;
			}
		}
		public static void SetValidCombo<Field>(PXCache cache, bool isRequired)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXDimensionSelectorAttribute)
				{
					bool oldRequired = ((PXDimensionAttribute)((PXDimensionSelectorAttribute)attr)._Attributes[((PXDimensionSelectorAttribute)attr)._Attributes.Count - 2]).ValidComboRequired;
					if (oldRequired != isRequired)
					{
						((PXDimensionSelectorAttribute)attr).resetValidCombos(cache, oldRequired, isRequired);
						((PXDimensionAttribute)((PXDimensionSelectorAttribute)attr)._Attributes[((PXDimensionSelectorAttribute)attr)._Attributes.Count - 2]).ValidComboRequired = isRequired;
						((PXSelectorAttribute)((PXDimensionSelectorAttribute)attr)._Attributes[((PXDimensionSelectorAttribute)attr)._Attributes.Count - 1]).DirtyRead = !isRequired;
					}
				}
			}
		}

		public static void SetValidCombo(PXCache cache, string name, bool isRequired)
		{
			cache.SetAltered(name, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(name))
			{
				if (attr is PXDimensionSelectorAttribute)
				{
					bool oldRequired = ((PXDimensionAttribute)((PXDimensionSelectorAttribute)attr)._Attributes[((PXDimensionSelectorAttribute)attr)._Attributes.Count - 2]).ValidComboRequired;
					if (oldRequired != isRequired)
					{
						((PXDimensionSelectorAttribute)attr).resetValidCombos(cache, oldRequired, isRequired);
						((PXDimensionAttribute)((PXDimensionSelectorAttribute)attr)._Attributes[((PXDimensionSelectorAttribute)attr)._Attributes.Count - 2]).ValidComboRequired = isRequired;
						((PXSelectorAttribute)((PXDimensionSelectorAttribute)attr)._Attributes[((PXDimensionSelectorAttribute)attr)._Attributes.Count - 1]).DirtyRead = !isRequired;
					}
				}
			}
		}

		public static void SuppressViewCreation(PXCache cache)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
			{
				if (attr is PXDimensionSelectorAttribute)
				{
					((PXDimensionSelectorAttribute)attr)._SuppressViewCreation = true;
				}
			}
		}

		public virtual PXSelectorMode SelectorMode
		{
			get
			{
				return ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).SelectorMode;
			}
			set
			{
				((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).SelectorMode = value;
			}
		}
		#endregion

		#region Ctor
		/// <summary>
		/// Creates a selector
		/// </summary>
		/// <param name="type">Referenced table. Should be either IBqlField or IBqlSearch</param>
		public PXDimensionSelectorAttribute(string dimension, Type type)
			: base()
		{
			PXDimensionAttribute dim = new PXDimensionAttribute(dimension);
			PXSelectorAttribute sel = new PXSelectorAttribute(type);
			_SubstituteKey = null;
			_Attributes.Add(dim);
			_Attributes.Add(sel);
		}
		/// <summary>
		/// Creates a selector
		/// </summary>
		/// <param name="type">Referenced table. Should be either IBqlField or IBqlSearch</param>
		public PXDimensionSelectorAttribute(string dimension, Type type, Type substituteKey)
			: base()
		{
			PXDimensionAttribute dim = new PXDimensionAttribute(dimension);
			PXSelectorAttribute sel = new PXSelectorAttribute(type);
			sel.SubstituteKey = substituteKey;
			_SubstituteKey = substituteKey;
			_Attributes.Add(dim);
			_Attributes.Add(sel);
		}
		/// <summary>
		/// Creates a selector overriding the columns
		/// </summary>
		/// <param name="type">Referenced table. Should be either IBqlField or IBqlSearch</param>
		/// <param name="fieldList">Fields to display in the selector</param>
		/// <param name="headerList">Headers of the selector columns</param>
		public PXDimensionSelectorAttribute(string dimension, Type type, Type substituteKey, params Type[] fieldList)
			: base()
		{
			PXDimensionAttribute dim = new PXDimensionAttribute(dimension);
			PXSelectorAttribute sel = new PXSelectorAttribute(type, fieldList);
			sel.SubstituteKey = substituteKey;
			_SubstituteKey = substituteKey;
			_Attributes.Add(dim);
			_Attributes.Add(sel);
		}
		#endregion

		#region Implementation
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (((PXSelectorAttribute)_Attributes[_Attributes.Count - 1])._BypassFieldVerifying)
			{
				return;
			}
			PXFieldUpdating fu = ((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).FieldUpdating;
			fu(sender, e);
			e.Cancel = false;
			if (_SubstituteKey == null || _BqlTable.IsAssignableFrom(BqlCommand.GetItemType(_SubstituteKey)) && String.Compare(_SubstituteKey.Name, _FieldName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				e.Cancel = true;
				return;
			}
			try
			{
				fu = ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).SubstituteKeyFieldUpdating;
				fu(sender, e);
			}
			catch (Exception exc)
			{
				if (!(exc is PXForeignRecordDeletedException) && !(exc is PXSetPropertyException))
				{
					throw;
				}
				else
				{
					if (((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).ValidComboRequired)
					{
						throw;
					}
					Type field = ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).Field;
					PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(field)];
					Dictionary<string, object> d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
					d[_SubstituteKey.Name] = e.NewValue;
					d[field.Name] = null;
					bool isDirty = cache.IsDirty;
					bool InsertRights = cache.InsertRights;
					bool AllowInsert = cache.AllowInsert;
					cache.InsertRights = true;
					cache.AllowInsert = true;
					try
					{
						if (exc is PXForeignRecordDeletedException && cache.Locate(d) > 0)
						{
							cache.Remove(cache.Current);
						}
						if (cache.Insert(d) > 0)
						{
							cache.IsDirty = isDirty;
							PXFieldState st = d[_SubstituteKey.Name] as PXFieldState;
							if (st != null && !String.IsNullOrEmpty(st.Error))
							{
								object key = d[field.Name] as PXFieldState;
								if (key == null)
								{
									key = d[field.Name];
								}
								foreach (object data in cache.Inserted)
								{
									if (object.Equals(key, cache.GetValue(data, field.Name)))
									{
										cache.Delete(data);
										break;
									}
								}
								throw new PXSetPropertyException(st.Error);
							}
							st = d[field.Name] as PXFieldState;
							if (st != null)
							{
								e.NewValue = st.Value;
							}
							else if (d[field.Name] != null)
							{
								e.NewValue = d[field.Name];
							}
							else
							{
								throw;
							}
						}
						else
						{
							throw;
						}
					}
					catch
					{
						cache.IsDirty = isDirty;
						cache.AllowInsert = AllowInsert;
						cache.InsertRights = InsertRights;
						throw;
					}
				}
			}
		}
		protected sealed class segmentView : PXView
		{
			private PXDimensionAttribute _Attribute;
			public segmentView(PXGraph graph, bool isReadonly, BqlCommand select, PXDimensionAttribute attribute)
				: base(graph, isReadonly, select, attribute.GetSegmentDelegate())
			{
				_Attribute = attribute;
			}
			protected override List<object> InvokeDelegate(object[] parameters)
			{
				List<GroupHelper.ParamsPair[]> list = null;
				IBqlParameter[] cmdpars = BqlSelect.GetParameters();
				for (int i = 0; i < cmdpars.Length; i++)
				{
					if (cmdpars[i].MaskedType != null && !cmdpars[i].IsArgument)
					{
						if (parameters[i] == null)
						{
							if (!cmdpars[i].NullAllowed)
							{
								return new List<object>();
							}
							parameters[i] = new byte[(GroupHelper.Count + 7) / 8];
							for (int j = 0; j < ((byte[])parameters[i]).Length; j++)
							{
								((byte[])parameters[i])[j] = 0xFF;
							}
						}
						if (list == null)
						{
							list = new List<GroupHelper.ParamsPair[]>();
						}
						Type cross = null;
						Type field = cmdpars[i].GetReferencedType();
						if (!field.IsNested || BqlCommand.GetItemType(field) == _CacheType)
						{
							cross = GroupHelper.GetReferencedType(Cache, field.Name);
						}
						else
						{
							PXCache cache = _Graph.Caches[BqlCommand.GetItemType(field)];
							cross = GroupHelper.GetReferencedType(cache, field.Name);
						}
						list.Add(GroupHelper.GetParams(cross, GroupHelper.FindRestricted(cross, "SegmentValue"), parameters[i] as byte[]));
					}
				}
				if (list != null)
				{
					_Attribute.Restrictions = list.ToArray();
				}
				return base.InvokeDelegate(parameters);
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			FieldSelecting(sender, e, _SuppressViewCreation);
		}
		protected virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, bool bypassViewCreation)
		{
			if (!e.IsAltered)
			{
				e.IsAltered = sender.HasAttributes(e.Row);
			}
			PXFieldSelecting fs;
			if (_SubstituteKey != null && (!_BqlTable.IsAssignableFrom(BqlCommand.GetItemType(_SubstituteKey)) || String.Compare(_SubstituteKey.Name, _FieldName, StringComparison.OrdinalIgnoreCase) != 0))
			{
				fs = ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).SubstituteKeyFieldSelecting;
				fs(sender, e);
			}
			fs = ((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).FieldSelecting;
			if (e.ReturnState is PXFieldState)
			{
				((PXFieldState)e.ReturnState).ViewName = null;
			}
			fs(sender, e);
			fs = ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).FieldSelecting;
			if (((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).ValidComboRequired)
			{
				fs(sender, e);
				if (((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).DescriptionField == null && e.ReturnState is PXFieldState)
				{
					((PXFieldState)e.ReturnState).DescriptionName = null;
				}
			}
			else if (e.IsAltered)
			{
				PXFieldSelectingEventArgs a = new PXFieldSelectingEventArgs(null, null, true, true);
				fs(sender, a);
				PXView viewSelector = sender.Graph.Views[((PXFieldState)a.ReturnState).ViewName];
				BqlCommand selectDimension = null;
				PXView viewDimension = null;
				foreach (IBqlParameter par in viewSelector.BqlSelect.GetParameters())
				{
					if (par.MaskedType != null)
					{
						if (selectDimension == null)
						{
							viewDimension = sender.Graph.Views[((PXFieldState)e.ReturnState).ViewName];
							selectDimension = viewDimension.BqlSelect;
						}
						if (par.NullAllowed)
						{
							selectDimension = selectDimension.WhereAnd(BqlCommand.Compose(
								typeof(Where<,,>),
								par.HasDefault ? (par.IsVisible ? typeof(Optional<>) : typeof(Current<>)) : (typeof(Required<>)),
								par.GetReferencedType(),
								typeof(IsNull),
								typeof(Or<>),
								typeof(Match<>),
								par.HasDefault ? (par.IsVisible ? typeof(Optional<>) : typeof(Current<>)) : (typeof(Required<>)),
								par.GetReferencedType()));
						}
						else
						{
							selectDimension = selectDimension.WhereAnd(BqlCommand.Compose(
								typeof(Where<>),
								typeof(Match<>),
								par.HasDefault ? (par.IsVisible ? typeof(Optional<>) : typeof(Current<>)) : (typeof(Required<>)),
								par.GetReferencedType()));
						}
					}
				}
				if (selectDimension != null)
				{
					if (!bypassViewCreation)
					{
						string vn = Guid.NewGuid().ToString();
						sender.Graph.Views[vn] = new segmentView(sender.Graph, true, selectDimension, (PXDimensionAttribute)_Attributes[_Attributes.Count - 2]);
						PXView fv;
						if (sender.Graph.Views.TryGetValue(((PXFieldState)a.ReturnState).ViewName + PXFilterableAttribute.FilterHeaderName, out fv))
						{
							sender.Graph.Views[vn + PXFilterableAttribute.FilterHeaderName] = fv;
							if (sender.Graph.Views.TryGetValue(((PXFieldState)a.ReturnState).ViewName + PXFilterableAttribute.FilterRowName, out fv))
							{
								sender.Graph.Views[vn + PXFilterableAttribute.FilterRowName] = fv;
							}
						}
						((PXFieldState)e.ReturnState).ViewName = vn;
					}
					else
					{
						List<GroupHelper.ParamsPair[]> list = null;
						IBqlParameter[] cmdpars = selectDimension.GetParameters();
						Type cacheType = selectDimension.GetTables()[0];
						for (int i = 0; i < cmdpars.Length; i++)
						{
							if (cmdpars[i].MaskedType != null)
							{
								object val = null;
								Type field = cmdpars[i].GetReferencedType();
								if (cmdpars[i].HasDefault)
								{
									if (field.IsNested)
									{
										Type ct = BqlCommand.GetItemType(field);
										PXCache cache = ct == cacheType ? viewDimension.Cache : sender.Graph.Caches[ct];
										object row;
										if (e.Row != null && (e.Row.GetType() == ct || e.Row.GetType().IsSubclassOf(ct)))
										{
											row = e.Row;
										}
										else
										{
											row = cache.Current;
										}
										if (row != null)
										{
											val = cache.GetValue(row, field.Name);
										}
										if (val == null && cmdpars[i].TryDefault)
										{
											if (cache.RaiseFieldDefaulting(field.Name, null, out val))
											{
												cache.RaiseFieldUpdating(field.Name, null, ref val);
											}
										}
										val = GroupHelper.GetReferencedValue(cache, row, field.Name, val, false);
									}
								}
								if (val == null)
								{
									val = new byte[(GroupHelper.Count + 7) / 8];
									for (int j = 0; j < ((byte[])val).Length; j++)
									{
										((byte[])val)[j] = 0xFF;
									}
								}
								if (list == null)
								{
									list = new List<GroupHelper.ParamsPair[]>();
								}
								Type cross = null;
								if (!field.IsNested || BqlCommand.GetItemType(field) == cacheType)
								{
									cross = GroupHelper.GetReferencedType(viewDimension.Cache, field.Name);
								}
								else
								{
									PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(field)];
									cross = GroupHelper.GetReferencedType(cache, field.Name);
								}
								list.Add(GroupHelper.GetParams(cross, GroupHelper.FindRestricted(cross, "SegmentValue"), val as byte[]));
							}
						}
						if (list != null)
						{
							((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).Restrictions = list.ToArray();
						}
					}
				}
			}
		}
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			List<IPXFieldVerifyingSubscriber> fv = new List<IPXFieldVerifyingSubscriber>();
			_Attributes[_Attributes.Count - 2].GetSubscriber<IPXFieldVerifyingSubscriber>(fv);
			if (fv.Count > 0)
			{
				PXFieldSelectingEventArgs fsa = new PXFieldSelectingEventArgs(e.Row, e.NewValue, !((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).ValidComboRequired, true);
				FieldSelecting(sender, fsa, true);
				try
				{
					bool needsync = object.Equals(fsa.ReturnValue, e.NewValue);
					PXFieldVerifyingEventArgs ver = new PXFieldVerifyingEventArgs(e.Row, fsa.ReturnValue, e.ExternalCall);
					for (int l = 0; l < fv.Count; l++)
					{
						fv[l].FieldVerifying(sender, ver);
					}
					if (needsync)
					{
						e.NewValue = ver.NewValue;
					}
					((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).FieldVerifying(sender, e);
				}
				catch (PXSetPropertyException ex)
				{
					ex.ErrorValue = fsa.ReturnValue;
					throw ex;
				}
			}
			else
			{
				((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).FieldVerifying(sender, e);
			}
		}
		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (_SubstituteKey == null || _BqlTable.IsAssignableFrom(BqlCommand.GetItemType(_SubstituteKey)) && String.Compare(_SubstituteKey.Name, _FieldName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return;
			}
			object key = sender.GetValue(e.Row, _FieldOrdinal);
			if (key != null && Convert.ToInt32(key) < 0)
			{
				PXSelectorAttribute attr = ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]);
				Type field = attr.Field;
				PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(field)];
				object newkey;
				_KeyToAbort = key;
				if (_Persisted.TryGetValue(key, out newkey))
				{
					sender.SetValue(e.Row, _FieldOrdinal, newkey);
				}
				else
				{
					bool found = false;
					foreach (object data in cache.Inserted)
					{
						if (object.Equals(key, cache.GetValue(data, field.Name)))
						{
							try
							{
								cache.PersistInserted(data);
							}
							catch
							{
							}
							newkey = cache.GetValue(data, field.Name);
							if (newkey != null && Convert.ToInt32(newkey) > 0)
							{
								sender.SetValue(e.Row, _FieldOrdinal, newkey);
								if (!object.Equals(_KeyToAbort, newkey))
								{
									_Persisted[_KeyToAbort] = newkey;
								}
								found = true;
							}
							break;
						}
					}
					if (!found)
					{
						if (_SubstitutePersisted.TryGetValue(key, out newkey))
						{
							sender.SetValue(e.Row, _FieldOrdinal, newkey);
						}
						else
						{
							sender.SetValue(e.Row, _FieldOrdinal, null);
						}
					}
				}
			}
		}
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (_KeyToAbort != null && e.TranStatus != PXTranStatus.Open)
			{
				if (e.TranStatus == PXTranStatus.Aborted)
				{
					object newkey;
					if (_Persisted.TryGetValue(_KeyToAbort, out newkey))
					{
						PXSelectorAttribute attr = ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]);
						Type field = attr.Field;
						PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(field)];
						foreach (object data in cache.Inserted)
						{
							if (object.Equals(newkey, cache.GetValue(data, field.Name)))
							{
								try
								{
									cache.RaiseRowPersisted(data, PXDBOperation.Insert, PXTranStatus.Aborted, null);
								}
								catch
								{
								}
								break;
							}
						}
					}
					sender.SetValue(e.Row, _FieldOrdinal, _KeyToAbort);
				}
				_Persisted.Remove(_KeyToAbort);
				_KeyToAbort = null;
			}
		}
		public virtual void SubstituteRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				PXSelectorAttribute attr = ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]);
				Type field = attr.Field;
				_JustPersistedKey = sender.GetValue(e.Row, field.Name);
			}
		}
		public virtual void SubstituteRowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
			{
				PXSelectorAttribute attr = ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]);
				Type field = attr.Field;
				_SubstitutePersisted[_JustPersistedKey] = sender.GetValue(e.Row, field.Name);
			}
		}
		#endregion

		#region Initialization
		protected void resetValidCombos(PXCache sender, bool oldRequired, bool newRequired)
		{
			if (_SubstituteKey == null)
			{
				return;
			}
			if (oldRequired && !newRequired)
			{
				sender.Graph.RowPersisting.AddHandler(BqlCommand.GetItemType(_SubstituteKey), SubstituteRowPersisting);
				sender.Graph.RowPersisted.AddHandler(BqlCommand.GetItemType(_SubstituteKey), SubstituteRowPersisted);
			}
			else if (!oldRequired && newRequired)
			{
				sender.Graph.RowPersisting.RemoveHandler(BqlCommand.GetItemType(_SubstituteKey), SubstituteRowPersisting);
				sender.Graph.RowPersisted.RemoveHandler(BqlCommand.GetItemType(_SubstituteKey), SubstituteRowPersisted);
			}
		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			string name = _FieldName.ToLower();
			sender.FieldUpdatingEvents[name] -= ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).SubstituteKeyFieldUpdating;
			sender.FieldUpdatingEvents[name] += FieldUpdating;
			sender.FieldSelectingEvents[name] -= ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).SubstituteKeyFieldSelecting;
			sender.FieldSelectingEvents[name] += FieldSelecting;
			if (!_DirtyRead)
			{
				((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).DirtyRead = !((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).ValidComboRequired;
			}
			if (((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).DirtyRead)
			{
				resetValidCombos(sender, true, false);
			}
			_Persisted = new Dictionary<object, object>();
			_SubstitutePersisted = new Dictionary<object, object>();
		}
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber))
			{
				if (_SubstituteKey != null)
				{
					subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
					subscribers.Remove(_Attributes[_Attributes.Count - 1] as ISubscriber);
				}
				else
				{
					subscribers.Remove(this as ISubscriber);
				}
			}
			else if (typeof(ISubscriber) == typeof(IPXFieldSelectingSubscriber))
			{
				subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
				subscribers.Remove(_Attributes[_Attributes.Count - 1] as ISubscriber);
			}
			else if (typeof(ISubscriber) == typeof(IPXFieldUpdatingSubscriber))
			{
				subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
			}
			else if (_SubstituteKey == null || String.Compare(_SubstituteKey.Name, _FieldName, StringComparison.OrdinalIgnoreCase) != 0)
			{
				if (typeof(ISubscriber) == typeof(IPXFieldDefaultingSubscriber))
				{
					subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
				}
				else if (typeof(ISubscriber) == typeof(IPXRowPersistingSubscriber))
				{
					subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
				}
				else if (typeof(ISubscriber) == typeof(IPXRowPersistedSubscriber))
				{
					subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
				}
			}
		}
		#endregion
	}
	#endregion

	#region PXDimensionWildcardAttribute
	public class PXDimensionWildcardAttribute : PXAggregateAttribute, IPXFieldSelectingSubscriber
	{
		#region State
		/// <summary>
		/// Field of the referenced table that contains the description
		/// </summary>
		public virtual Type DescriptionField
		{
			get
			{
				return ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).DescriptionField;
			}
			set
			{
				((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).DescriptionField = value;
			}
		}
		public virtual string Wildcard
		{
			get
			{
				return ((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).Wildcard;
			}
			set
			{
				((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).Wildcard = value;
			}
		}
		public virtual string[] Headers
		{
			get
			{
				return ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).Headers;
			}
			set
			{
				((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).Headers = value;
			}
		}
		#endregion

		#region Ctor
		/// <summary>
		/// Creates a selector
		/// </summary>
		/// <param name="type">Referenced table. Should be either IBqlField or IBqlSearch</param>
		public PXDimensionWildcardAttribute(string dimension, Type type)
			: base()
		{
			PXDimensionAttribute da = new PXDimensionAttribute(dimension);
			da.Wildcard = "?";
			_Attributes.Add(da);
			PXSelectorAttribute attr = new PXSelectorAttribute(type);
			_Attributes.Add(attr);
		}
		/// <summary>
		/// Creates a selector overriding the columns
		/// </summary>
		/// <param name="type">Referenced table. Should be either IBqlField or IBqlSearch</param>
		/// <param name="fieldList">Fields to display in the selector</param>
		/// <param name="headerList">Headers of the selector columns</param>
		public PXDimensionWildcardAttribute(string dimension, Type type, params Type[] fieldList)
			: base()
		{
			PXDimensionAttribute da = new PXDimensionAttribute(dimension);
			da.Wildcard = "?";
			_Attributes.Add(da);
			PXSelectorAttribute attr = new PXSelectorAttribute(type, fieldList);
			_Attributes.Add(attr);
		}
		#endregion

		#region Implementation
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (!e.IsAltered)
			{
				e.IsAltered = sender.HasAttributes(e.Row);
			}
			PXFieldSelecting fs = ((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).FieldSelecting;
			fs(sender, e);
			if (((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).ValidComboRequired)
			{
				fs = ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).FieldSelecting;
				fs(sender, e);
			}
		}
		#endregion

		#region Initialization
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber))
			{
				subscribers.Remove(_Attributes[_Attributes.Count - 1] as ISubscriber);
			}
			else if (typeof(ISubscriber) == typeof(IPXFieldSelectingSubscriber))
			{
				subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
				subscribers.Remove(_Attributes[_Attributes.Count - 1] as ISubscriber);
			}
			else if (typeof(ISubscriber) == typeof(IPXFieldDefaultingSubscriber))
			{
				subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
			}
			else if (typeof(ISubscriber) == typeof(IPXRowPersistingSubscriber))
			{
				subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
			}
			else if (typeof(ISubscriber) == typeof(IPXRowPersistedSubscriber))
			{
				subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
			}
		}
		#endregion
	}
	#endregion

	#region PXCustomDimensionSelectorAttribute
	public class PXCustomDimensionSelectorAttribute : PXDimensionSelectorAttribute
	{
		protected class selectorAttribute : PXCustomSelectorAttribute
		{
			private readonly Func<IEnumerable> _delegate;

			public selectorAttribute(Type searchType, Func<IEnumerable> @delegate)
				: base(searchType)
			{
				if (@delegate == null) throw new ArgumentNullException("delegate");
				_delegate = @delegate;
			}

			public selectorAttribute(Type searchType, Type[] fieldList, Func<IEnumerable> @delegate)
				: base(searchType, fieldList)
			{
				if (@delegate == null) throw new ArgumentNullException("delegate");
				_delegate = @delegate;
			}

			public IEnumerable GetRecords()
			{
				return _delegate();
			}
		}
		#region Ctor
		public PXCustomDimensionSelectorAttribute(string dimension, Type type)
			: base(dimension, type)
		{
			PXSelectorAttribute sel = new selectorAttribute(type, GetRecords);
			_Attributes.RemoveAt(_Attributes.Count - 1);
			_Attributes.Add(sel);
		}

		public PXCustomDimensionSelectorAttribute(string dimension, Type type, Type substituteKey)
			: base(dimension, type, substituteKey)
		{
			PXSelectorAttribute sel = new selectorAttribute(type, GetRecords);
			sel.SubstituteKey = substituteKey;
			_Attributes.RemoveAt(_Attributes.Count - 1);
			_Attributes.Add(sel);
		}

		public PXCustomDimensionSelectorAttribute(string dimension, Type type, Type substituteKey, params Type[] fieldList)
			: base(dimension, type, substituteKey, fieldList)
		{
			PXSelectorAttribute sel = new selectorAttribute(type, fieldList, GetRecords);
			sel.SubstituteKey = substituteKey;
			_Attributes.RemoveAt(_Attributes.Count - 1);
			_Attributes.Add(sel);
		}

		public virtual IEnumerable GetRecords()
		{
			yield break;
		}
		#endregion
	}
	#endregion

	#region PXPhoneValidationAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public class PXPhoneValidationAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		#region State
		protected Type _PhoneValidationField;
		protected Type _CountryIdField;
		protected Type _ForeignIdField;
		protected string _PhoneMask = "";
		protected Definition _Definition;

		public virtual Type PhoneValidationField
		{
			get
			{
				return _PhoneValidationField;
			}
			set
			{
				_PhoneValidationField = value;
			}
		}

		public virtual string PhoneMask
		{
			get
			{
				return _PhoneMask;
			}
			set
			{
				_PhoneMask = value;
			}
		}

		public virtual Type CountryIdField
		{
			get
			{
				return _CountryIdField;
			}
			set
			{
				_CountryIdField = value;
			}
		}
		private Type _CacheType;

		public static void Clear<Table>()
			where Table : IBqlTable
		{
			PXDatabase.ResetSlot<Definition>("PhoneDefinitions", typeof(Table));
		}
		#endregion

		#region Ctor
		public PXPhoneValidationAttribute(Type phoneValidationField)
		{
			if (phoneValidationField != null)
			{
				if (!typeof(IBqlField).IsAssignableFrom(phoneValidationField))
					throw new PXArgumentException("PhoneValidationField", ErrorMessages.PhoneValidationFieldNotValid);
				_PhoneValidationField = phoneValidationField;
			}
		}
		#endregion

		#region Implementation
		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			string id, phonenumber;

			if (sender.GetItemType().Name == BqlCommand.GetItemType(_CountryIdField).Name)
				id = (string)sender.GetValue(e.Row, _CountryIdField.Name);
			else
			{
				PXCache othercache = sender.Graph.Caches[BqlCommand.GetItemType(_CountryIdField).Name];
				id = (string)othercache.GetValue(othercache.Current, _CountryIdField.Name);
			}
			phonenumber = (string)sender.GetValue(e.Row, _FieldName);

			if (id == null || !_Definition.CountryIdMask.TryGetValue(id, out _PhoneMask))
				_PhoneMask = "+#(###) ###-####";

			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, null, _PhoneMask, null, null, null, null);
		}
		private void countryIDRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[_CacheType];
			cache.SetAltered(_FieldName, true);
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (_CountryIdField != null)
			{
				PXCache cache;
				if (sender.BqlFields.Contains(_CountryIdField))
				{
					cache = sender;
				}
				else
				{
					cache = sender.Graph.Caches[BqlCommand.GetItemType(_CountryIdField)];
				}
				foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(_CountryIdField.Name))
				{
					if (attr is PXSelectorAttribute)
					{
						_ForeignIdField = ((PXSelectorAttribute)attr).Field;
						break;
					}
				}
			}
			else
			{
				foreach (Type field in sender.BqlFields)
				{
					foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(field.Name))
					{
						if (attr is PXSelectorAttribute)
						{
							_ForeignIdField = ((PXSelectorAttribute)attr).Field;
							_CountryIdField = field;
							break;
						}
					}
				}
			}
			if (_ForeignIdField == null)
			{
				throw new PXException(ErrorMessages.CountryIdWithSelectorNotFound);
			}

			_Definition = PXContext.GetSlot<Definition>();
			if (_Definition == null)
			{
				PXContext.SetSlot<Definition>(_Definition = PXDatabase.GetSlot<Definition, PXPhoneValidationAttribute>("PhoneDefinitions", this, BqlCommand.GetItemType(_ForeignIdField)));
			}

			_CacheType = sender.GetItemType();
			sender.Graph.Caches[BqlCommand.GetItemType(_CountryIdField)].RowSelected += countryIDRowSelected;
		}

		protected class Definition : IPrefetchable<PXPhoneValidationAttribute>
		{
			public Dictionary<string, string> CountryIdMask = new Dictionary<string, string>();
			public void Prefetch(PXPhoneValidationAttribute attr)
			{
				try
				{
					foreach (PXDataRecord record in PXDatabase.SelectMulti(BqlCommand.GetItemType(attr._PhoneValidationField),
																			new PXDataField(attr._ForeignIdField.Name),
																			new PXDataField(attr._PhoneValidationField.Name)))
						CountryIdMask[record.GetString(0)] = record.GetString(1);
				}
				catch
				{
					CountryIdMask.Clear();
					throw;
				}
			}
		}
		#endregion
	}
	#endregion

	#region PXZipValidationAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXZipValidationAttribute : PXEventSubscriberAttribute, IPXFieldVerifyingSubscriber, IPXFieldSelectingSubscriber
	{
		#region State
		protected Type _ZipValidationField;
		protected Type _ZipMaskField;
		protected Type _CountryIdField;
		protected Type _ForeignIdField;
		protected DefinitionZip _DefinitionZip;
		protected DefinitionMask _DefinitionMask;

		public virtual Type ZipValidationField
		{
			get
			{
				return _ZipValidationField;
			}
			set
			{
				_ZipValidationField = value;
			}
		}

		public virtual Type CountryIdField
		{
			get
			{
				return _CountryIdField;
			}
			set
			{
				_CountryIdField = value;
			}
		}

		public static void Clear<Table>()
			where Table : IBqlTable
		{
			PXDatabase.ResetSlot<DefinitionZip>("ZipDefinitions", typeof(Table));
			PXDatabase.ResetSlot<DefinitionMask>("MaskDefinitions", typeof(Table));
		}
		#endregion

		#region Ctor
		public PXZipValidationAttribute(Type zipValidationField)
			: this(zipValidationField, null)
		{

		}
		public PXZipValidationAttribute(Type zipValidationField, Type zipMaskField)
		{
			if (zipValidationField != null)
			{
				if (!typeof(IBqlField).IsAssignableFrom(zipValidationField))
					throw new PXArgumentException("ZipValidationField", ErrorMessages.ZipValidationFieldNotValid);
				_ZipValidationField = zipValidationField;
			}
			if (zipMaskField != null)
			{
				if (!typeof(IBqlField).IsAssignableFrom(zipMaskField))
					throw new PXArgumentException("zipMaskField", ErrorMessages.ZipMaskFieldNotValid);
				_ZipMaskField = zipMaskField;
			}
		}
		#endregion

		#region Implementation
		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			string id, zipcode, regex = null;

			if (sender.GetItemType().Name == BqlCommand.GetItemType(_CountryIdField).Name)
				id = (string)sender.GetValue(e.Row, _CountryIdField.Name);
			else
			{
				PXCache othercache = sender.Graph.Caches[BqlCommand.GetItemType(_CountryIdField).Name];
				object foreignrow = PXSelectorAttribute.Select(othercache, e.Row, _CountryIdField.Name);
				if (foreignrow == null)
					return;
				id = (string)sender.Graph.Caches[foreignrow.GetType()].GetValue(foreignrow, _CountryIdField.Name);
			}
			zipcode = (string)e.NewValue;

			if (id == null || !_DefinitionZip.CountryIdRegex.TryGetValue(id, out regex))
			{
				return;
			}
			if (!ValidateZip(zipcode, regex))
			{
				throw new PXSetPropertyException(ErrorMessages.ZipCodeDoesntMatch);
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				if (_ZipMaskField != null && _DefinitionMask != null)
				{
					string id, mask = null;
					if (sender.GetItemType().Name == BqlCommand.GetItemType(_CountryIdField).Name)
						id = (string)sender.GetValue(e.Row, _CountryIdField.Name);
					else
					{
						PXCache othercache = sender.Graph.Caches[BqlCommand.GetItemType(_CountryIdField).Name];
						object foreignrow = PXSelectorAttribute.Select(othercache, e.Row, _CountryIdField.Name);
						if (foreignrow == null)
							return;
						id = (string)sender.Graph.Caches[foreignrow.GetType()].GetValue(foreignrow, _CountryIdField.Name);
					}

					if (id != null)
						_DefinitionMask.CountryIdMask.TryGetValue(id, out mask);
					if (mask != null)
						e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, null, mask, null, null, null, null);
				}
			}
		}

		protected bool ValidateZip(string val, string regex)
		{
			if (val == null || regex == null)
				return true;

			System.Text.RegularExpressions.Regex regexobject = new System.Text.RegularExpressions.Regex(regex);
			return regexobject.IsMatch(val);
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			if (_CountryIdField != null)
			{
				PXCache cache;
				if (sender.BqlFields.Contains(_CountryIdField))
				{
					cache = sender;
				}
				else
				{
					cache = sender.Graph.Caches[BqlCommand.GetItemType(_CountryIdField)];
				}
				foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(_CountryIdField.Name))
				{
					if (attr is PXSelectorAttribute)
					{
						_ForeignIdField = ((PXSelectorAttribute)attr).Field;
						break;
					}
				}
			}
			else
			{
				foreach (Type field in sender.BqlFields)
				{
					foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(field.Name))
					{
						if (attr is PXSelectorAttribute)
						{
							_ForeignIdField = ((PXSelectorAttribute)attr).Field;
							_CountryIdField = field;
							break;
						}
					}
				}
			}
			if (_ForeignIdField == null)
			{
				throw new PXException(ErrorMessages.CountryIdWithSelectorNotFound);
			}

			_DefinitionZip = PXContext.GetSlot<DefinitionZip>();
			if (_DefinitionZip == null)
			{
				PXContext.SetSlot<DefinitionZip>(_DefinitionZip = PXDatabase.GetSlot<DefinitionZip, PXZipValidationAttribute>("ZipDefinitions", this, BqlCommand.GetItemType(_ForeignIdField)));
			}
			if (_ZipMaskField != null)
			{
				_DefinitionMask = PXContext.GetSlot<DefinitionMask>();
				if (_DefinitionMask == null)
					PXContext.SetSlot<DefinitionMask>(_DefinitionMask = PXDatabase.GetSlot<DefinitionMask, PXZipValidationAttribute>("MaskDefinitions", this, BqlCommand.GetItemType(_ForeignIdField)));
			}

		}


		protected class DefinitionZip : IPrefetchable<PXZipValidationAttribute>
		{
			public Dictionary<string, string> CountryIdRegex = new Dictionary<string, string>();

			public void Prefetch(PXZipValidationAttribute attr)
			{
				try
				{

					foreach (PXDataRecord record in PXDatabase.SelectMulti(BqlCommand.GetItemType(attr._ZipValidationField),
																		   new PXDataField(attr._ForeignIdField.Name),
																		   new PXDataField(attr._ZipValidationField.Name)))
					{
						CountryIdRegex[record.GetString(0)] = record.GetString(1);
					}

				}
				catch
				{
					CountryIdRegex.Clear();
					throw;
				}
			}
		}
		protected class DefinitionMask : IPrefetchable<PXZipValidationAttribute>
		{
			public Dictionary<string, string> CountryIdMask = new Dictionary<string, string>();

			public void Prefetch(PXZipValidationAttribute attr)
			{
				try
				{

					foreach (PXDataRecord record in PXDatabase.SelectMulti(BqlCommand.GetItemType(attr._ZipValidationField),
																																 new PXDataField(attr._ForeignIdField.Name),
																																 new PXDataField(attr._ZipMaskField.Name)))
					{
						CountryIdMask[record.GetString(0)] = record.GetString(1);
					}

				}
				catch
				{
					CountryIdMask.Clear();
					throw;
				}
			}
		}
		#endregion
	}
	#endregion

	#region PXDynamicMaskAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method)]
	public class PXDynamicMaskAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		#region State
		protected BqlCommand _MaskSearch;
		protected string _DefaultMask = "";
		public virtual string DefaultMask
		{
			get
			{
				return _DefaultMask;
			}
			set
			{
				_DefaultMask = value;
			}
		}
		#endregion

		#region Ctor
		public PXDynamicMaskAttribute(Type maskSearch)
		{
			if (maskSearch != null)
			{
				if (maskSearch == null || !typeof(IBqlSearch).IsAssignableFrom(maskSearch))
					throw new PXArgumentException("maskSearch", ErrorMessages.CantCreateForeignKeyReference, maskSearch);
				_MaskSearch = BqlCommand.CreateInstance(maskSearch);
			}
		}
		#endregion

		#region Implementation
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				string mask = DefaultMask;
				PXView view = sender.Graph.TypedViews.GetView(_MaskSearch, true);
				object row = view.SelectSingleBound(new object[] { e.Row });
				if (row != null)
				{
					Type field = ((IBqlSearch)_MaskSearch).GetField();
					if (row is PXResult)
					{
						row = ((PXResult)row)[BqlCommand.GetItemType(field)];
					}
					mask = (PXFieldState.UnwrapValue(view.Cache.GetValueExt(row, field.Name)) as string) ?? DefaultMask;
				}
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, null, mask, null, null, null, null);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBWeblinkAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
	public class PXDBWeblinkAttribute : PXDBStringAttribute, IPXFieldVerifyingSubscriber
	{
		public PXDBWeblinkAttribute()
			: base(255)
		{
			IsUnicode = true;
			IsFixed = false;
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!e.ExternalCall) return;

			Regex regex = new Regex("(http://|https://)?(www\\.)?[\\w-]+(\\.[\\w-]+)+(/[\\w-]+)*(" +
									"\\.(html|htm|cgi|php|aspx|asp|\\w+))?(\\?(\\w+\\=[\\w%+.]+){" +
									"1}(&\\w+\\=[\\w%+.]+)*)?", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);
			if (!string.IsNullOrEmpty(e.NewValue as string) && !regex.IsMatch(e.NewValue as string))
			{
				throw new PXSetPropertyException(ErrorMessages.InvalidWeblink);
			}
		}
	}
	#endregion

	#region PXDBEmailAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
	public class PXDBEmailAttribute : PXDBStringAttribute, IPXFieldVerifyingSubscriber
	{
		public static Dictionary<Type, List<string>> FieldList = new Dictionary<Type, List<string>>();
		public PXDBEmailAttribute()
			: base(255)
		{
			IsUnicode = true;
			IsFixed = false;
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!e.ExternalCall) return;

			const string pattern = @"(?!\.)(""([^""\r\\]|\\[""\r\\])*""|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)@([a-z0-9][\w\.-]*[a-z0-9]|[a-z])\.[a-z][a-z\.]*[a-z]";

			Regex regex = new Regex(string.Format("({0})|(\"[^\"]\"\\s*<{0}>)", pattern), RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);

			foreach (string email in (e.NewValue as string ?? string.Empty).Split(';').Select(str => str.Trim()).Where(str => !string.IsNullOrEmpty(str) && !regex.IsMatch(str)))
			{
				throw new PXSetPropertyException(ErrorMessages.InvalidEmail, email);
			}
		}

		protected internal override void SetBqlTable(Type bqlTable)
		{
			base.SetBqlTable(bqlTable);
			lock (((ICollection)FieldList).SyncRoot)
			{
				List<string> list;
				if (!FieldList.TryGetValue(bqlTable, out list))
				{
					FieldList[bqlTable] = list = new List<string>();
				}
				if (!list.Contains(base.FieldName))
				{
					list.Add(base.FieldName);
				}
			}
		}
		public static List<string> GetEMailFields(Type table)
		{
			if (PX.Api.ServiceManager.EnsureCachesInstatiated(true))
			{
				List<string> list;
				if (FieldList.TryGetValue(table, out list))
				{
					return list;
				}
			}
			return new List<string>();
		}
	}
	#endregion

	#region PXNoteAttribute
	/// <summary>
	/// 8-bytes floating point stored in the database
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXNoteAttribute : PXDBLongAttribute, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber, IPXRowDeletedSubscriber, IPXReportRequiredField
	{
		#region State
		protected long? _KeyToAbort;
		internal const string _NoteTextField = "NoteText";
		internal const string _NoteFilesField = "NoteFiles";
		protected const string _NoteImagesField = "NoteImages";
		protected const string _NoteActivityField = "NoteActivity";
		protected const string _NoteImagesViewPrefix = "$NoteImages$";

		internal const string _NoteTextExistsField = "NoteTextExists";
		internal const string _NoteFilesExistsField = "NoteFilesExists";

		private PXView _noteNoteID;
		private PXView _noteDocNoteID;
		private PXView _noteFileID;
		protected Type[] _Searches;
		protected bool _PassThrough;
		protected Type _ParentNoteID;
		protected Type[] extraSearchResultColumns;
		protected Type[] foreignRelations;
		protected Dictionary<long, KeyValuePair<string, int>> _Selected;
		protected Dictionary<long, bool> _TextExists = new Dictionary<long, bool>();
		protected Dictionary<long, bool> _FilesExists = new Dictionary<long, bool>();

		protected bool _TextRequired;
		protected bool _FilesRequired;
		protected bool _ActivityRequired;
		private string _declaringType;

		public PXNoteAttribute()
		{
		}

		public PXNoteAttribute(params Type[] searches)
		{
			_Searches = searches;
		}

		public virtual Type ParentNoteID
		{
			get
			{
				return this._ParentNoteID;
			}
			set
			{
				this._ParentNoteID = value;
			}
		}

		public bool ShowInReferenceSelector { get; set; }

		/// <summary>
		/// Gets or sets a list of DACs which will be displayed in separate column
		/// when rendering search results.
		/// </summary>
		public Type[] ExtraSearchResultColumns
		{
			get { return this.extraSearchResultColumns; }
			set { this.extraSearchResultColumns = value; }
		}

		public Type[] Searches
		{
			get { return this._Searches; }
		}

		/// <summary>
		/// Gets or sets an array defining which fields in current table connect it with foreign tables specified in search.
		/// </summary>
		public Type[] ForeignRelations
		{
			get { return this.foreignRelations; }
			set { this.foreignRelations = value; }
		}

		public bool ForceFileCorrection { get; set; }

		protected PXView GetView(PXGraph graph)
		{
			if (_noteNoteID == null)
			{
				_noteNoteID = new PXView(graph, false, new Select<Note, Where<Note.noteID, Equal<Required<Note.noteID>>>>());
			}
			return _noteNoteID;
		}

		protected static string[] GetImageExtansions(PXGraph graph)
		{
			var res = PXSelectReadonly<UploadAllowedFileTypes>.Select(graph);
			var list = new List<string>(res.Count);
			foreach (UploadAllowedFileTypes item in res)
				if (item.IsImage == true) list.Add(item.FileExt.TrimStart('.'));
			return list.ToArray();
		}

		protected PXView GetDocView(PXGraph graph)
		{
			if (_noteDocNoteID == null)
			{
				_noteDocNoteID = new PXView(graph, false, new Select<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>());
			}
			return _noteDocNoteID;
		}

		protected PXView GetFileByID(PXGraph graph)
		{
			if (_noteFileID == null)
				_noteFileID = new PXView(graph, false, new Select<PX.SM.UploadFile, Where<PX.SM.UploadFile.fileID, Equal<Required<PX.SM.UploadFile.fileID>>>>());
			return _noteFileID;
		}

		public static long GetNoteID(PXCache cache, object data, string name)
		{
			data = CastRow(cache, data);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXNoteAttribute)
				{
					return ((PXNoteAttribute)attr).GetNoteID(cache, data);
				}
			}
			return 0;
		}

		public static long GetNoteID<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			data = CastRow(cache, data);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXNoteAttribute)
				{
					return ((PXNoteAttribute)attr).GetNoteID(cache, data);
				}
			}
			return 0;
		}

		internal static bool ImportEnsureNewNoteID(PXCache cache, object data, string externalKey)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, "NoteID"))
			{
				if (attr is PXNoteAttribute)
				{
					return ((PXNoteAttribute)attr).EnsureNoteID(cache, data, externalKey) < 0;
				}
			}
			return false;
		}

		public static long? GetNoteIDReadonly(PXCache cache, object data, string name)
		{
			data = CastRow(cache, data);

			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is PXNoteAttribute)
				{
					return (long?)cache.GetValue(data, (attr as PXNoteAttribute)._FieldOrdinal);
				}
			}
			return 0;
		}

		private static object CastRow(PXCache cache, object data)
		{
			var itemType = cache.GetItemType();
			var res = data as PXResult;
			if (res != null) data = DynamicalyChangeType(res[itemType], itemType);
			data = DynamicalyChangeType(data, itemType);
			return data;
		}

		private static Dictionary<string, MethodInfo> _castDic = new Dictionary<string, MethodInfo>();
		private static object _syncObj = new object();

		private static object DynamicalyChangeType(object obj, Type type)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (obj == null) return null;

			var objType = obj.GetType();

			if (type.IsAssignableFrom(objType)) return obj;

			MethodInfo meth = null;
			lock (_syncObj)
			{
				var key = string.Concat(objType.Name, "->", type.Name);
				if (!_castDic.TryGetValue(key, out meth))
				{
					meth = GetMethod(objType, "op_Implicit", type,
						BindingFlags.Static | BindingFlags.Public);
					if (meth == null)
					{
						meth = GetMethod(objType, "op_Explicit", type,
							BindingFlags.Static | BindingFlags.Public);
					}
					_castDic.Add(key, meth);
				}
			}

			if (meth == null) throw new InvalidCastException("Invalid Cast: " + objType.GetLongName() + " to " + type.GetLongName());

			return meth.Invoke(null, new object[] { obj });
		}

		private static MethodInfo GetMethod(Type toSearch, string methodName,
			Type returnType, BindingFlags bindingFlags)
		{
			return Array.Find(
				toSearch.GetMethods(bindingFlags),
					delegate(MethodInfo inf)
					{
						return ((inf.Name == methodName) && (inf.ReturnType == returnType));
					});
		}

		public static long? GetNoteIDReadonly<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			data = CastRow(cache, data);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXNoteAttribute)
				{
					return (long?)cache.GetValue(data, (attr as PXNoteAttribute)._FieldOrdinal);
				}
			}
			return 0;
		}

		public static void UpdateEntityType(PXCache cache, object data, string noteFieldName, Type newEntityType)
		{
			data = CastRow(cache, data);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, noteFieldName))
			{
				if (attr is PXNoteAttribute)
				{
					Note note = PXSelect<Note, Where<Note.noteID, Equal<Required<Note.noteID>>>>.SelectWindowed(cache.Graph, 0, 1, ((PXNoteAttribute)attr).GetNoteID(cache, data));
					if (note != null)
					{
						note.EntityType = ((PXNoteAttribute)attr)._declaringType;
						cache.Graph.Caches[typeof(Note)].Update(note);
					}
				}
			}
		}

		public static long? GetParentNoteID<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			data = CastRow(cache, data);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXNoteAttribute)
				{
					return ((PXNoteAttribute)attr).GetParentNoteID(cache, data);
				}
			}
			return null;
		}


		public static void ForcePassThrow<Field>(PXCache cache)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(typeof(Field).Name))
			{
				if (attr is PXNoteAttribute)
				{
					((PXNoteAttribute)attr)._PassThrough = true;
					break;
				}
			}
		}

		public static void ForcePassThrow(PXCache cache, string name)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(name))
			{
				if (attr is PXNoteAttribute)
				{
					((PXNoteAttribute)attr)._PassThrough = true;
					break;
				}
			}
		}

		public static long? GetNoteIDNow(PXCache cache, object data)
		{
			data = CastRow(cache, data);
			long? id = null;
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(null))
			{
				if (attr is PXNoteAttribute)
				{
					PXNoteAttribute noteattr = (PXNoteAttribute)attr;
					id = (long?)cache.GetValue(data, noteattr.FieldName);
					if (id == null)
					{
						using (PXTransactionScope tran = new PXTransactionScope())
						{
							try
							{
								PXDatabase.Insert<Note>(
									new PXDataFieldAssign(typeof(Note.noteText).Name, PXDbType.NVarChar, 0, noteattr._CreateNoteText(cache, data, "")),
									new PXDataFieldAssign(typeof(Note.entityType).Name, PXDbType.VarChar, cache.GetItemType().FullName),
									new PXDataFieldAssign(typeof(Note.graphType).Name, PXDbType.VarChar, cache.Graph.GetType().FullName),
									PXDataFieldAssign.OperationSwitchAllowed
								);
							}
							catch (PXDatabaseException e)
							{
								if (e.ErrorCode == PXDbExceptions.OperationSwitchRequired)
								{
									PXDatabase.Update<Note>(
										new PXDataFieldAssign(typeof(Note.noteText).Name, PXDbType.NVarChar, 0, noteattr._CreateNoteText(cache, data, "")),
										new PXDataFieldAssign(typeof(Note.entityType).Name, PXDbType.VarChar, cache.GetItemType().FullName),
										new PXDataFieldAssign(typeof(Note.graphType).Name, PXDbType.VarChar, cache.Graph.GetType().FullName),
										new PXDataFieldRestrict(typeof(Note.noteID).Name, PXDbType.BigInt, 8, null)
									);
								}
								else
								{
									throw;
								}
							}
							id = Convert.ToInt64(PXDatabase.SelectIdentity());
							if (id != null)
							{
								cache.SetValue(data, noteattr.FieldName, id);
								noteattr.updateTableWithId(cache, data, id);
								tran.Complete();
							}
							cache.Graph.TimeStamp = PXDatabase.SelectTimeStamp();
						}
					}
					break;
				}
			}
			return id;
		}

		private void updateTableWithId(PXCache sender, object data, long? id)
		{
			List<PXDataFieldParam> pars = new List<PXDataFieldParam>();
			PXDataFieldAssign assign = new PXDataFieldAssign(_DatabaseFieldName, PXDbType.BigInt, 8, id, sender.ValueToString(_FieldName, id));
			pars.Add(assign);
			foreach (string field in sender.Fields)
			{
				PXCommandPreparingEventArgs.FieldDescription description;
				sender.RaiseCommandPreparing(field, data, sender.GetValue(data, field), PXDBOperation.Update, null, out description);
				if (description != null && description.IsRestriction && description.DataType != PXDbType.Timestamp
					&& (sender.BqlSelect == null
					|| !String.IsNullOrEmpty(description.FieldName) && description.FieldName.StartsWith(BqlTable.Name + ".")))
				{
					pars.Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
				}
			}
			PXDatabase.Update(BqlTable, pars.ToArray());
		}

		public static void SetFileNotes(PXCache cache, object data, params Guid[] fileIDs)
		{
			Guid[] oldfiles = GetFileNotes(cache, data);
			if ((oldfiles != null && oldfiles.Length > 0) ||
				(fileIDs != null && fileIDs.Length > 0))
				cache.SetValueExt(data, _NoteFilesField, fileIDs);
		}

		public static Guid[] GetFileNotes(PXCache sender, object data)
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(null))
			{
				if (attr is PXNoteAttribute)
				{
					PXNoteAttribute noteattr = (PXNoteAttribute)attr;
					List<Guid> ret = new List<Guid>();
					long? id = (long?)sender.GetValue(data, noteattr._FieldName);
					foreach (NoteDoc doc in noteattr.GetDocView(sender.Graph).SelectMulti(id))
						if (doc.FileID != null)
							ret.Add(doc.FileID.Value);
					return ret.ToArray();
				}
			}
			return null;
		}
		public static string GetNote(PXCache sender, object data)
		{
			string result = (string)PXFieldState.UnwrapValue(sender.GetValueExt(data, _NoteTextField));
			return string.IsNullOrEmpty(result) ? null : result;
		}

		public static void SetNote(PXCache sender, object data, string note)
		{
			if (GetNote(sender, data) != note)
				sender.SetValueExt(data, _NoteTextField, note);
		}
		#endregion

		#region Implementation
		protected static string GetGraphType(PXCache cache)
		{
			Type graphType = CustomizedTypeManager.GetTypeNotCustomized(cache.Graph);
			return graphType.FullName;
			//graphType.FullName.Contains(".Customization.")
			//    ?
			//        graphType.BaseType.FullName
			//    :
			//        graphType.FullName;
		}
		protected long GetNoteID(PXCache sender, object row)
		{
			return EnsureNoteID(sender, row, null);
		}
		protected long EnsureNoteID(PXCache sender, object row, string externalKey)
		{
			long? id = (long?)sender.GetValue(row, _FieldOrdinal);
			if (id != null) return (long)id;

			Note note = new Note();
			note.NoteText = string.Empty;
			note.EntityType = _declaringType;
			note.GraphType = GetGraphType(sender);
			note.ExternalKey = externalKey;
			note = GetView(sender.Graph).Cache.Insert(note) as Note;
			if (note != null)
			{
				id = note.NoteID;
				sender.SetValue(row, _FieldOrdinal, id);
			}
			if (sender.GetStatus(row) == PXEntryStatus.Notchanged)
			{
				sender.SetStatus(row, PXEntryStatus.Updated);
			}
			sender.IsDirty = true;
			return (long)id;
		}

		protected long? GetParentNoteID(PXCache sender, object row)
		{
			if (_ParentNoteID == null)
				return null;
			PXCache parentCache = sender.Graph.Caches[BqlCommand.GetItemType(_ParentNoteID)];
			if (parentCache.Current == null)
				return null;
			return (long?)parentCache.GetValue(parentCache.Current, _ParentNoteID.Name);
		}

		public virtual void noteTextFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			//Checking for virtual dac
			if (e.Row == null && PXDatabase.IsVirtualTable(sender.BqlTable))
			{
				e.ReturnValue = null;
				e.ReturnState = null;
				e.Cancel = true;
				return;
			}

			long? id = null;
			if (e.Row != null)
			{
				id = (long?)sender.GetValue(e.Row, _FieldOrdinal);
			}
			if (id != null)
			{
				KeyValuePair<string, int> val;
				if (_TextRequired && _Selected.TryGetValue((long)id, out val))
				{
					e.ReturnValue = val.Key;
				}
				else
				{
					Note note = GetView(sender.Graph).SelectSingle(id) as Note;
					if (note != null)
					{
						if (!String.IsNullOrEmpty(note.NoteText))
						{
							string[] split = note.NoteText.Split('\0');
							if (split.Length > 0)
							{
								e.ReturnValue = split[0];
							}
							else
							{
								e.ReturnValue = String.Empty;
							}
						}
						else
						{
							e.ReturnValue = note.NoteText;
						}
					}
				}
				SetNoteTextExists(id, (string)e.ReturnValue);
			}
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXNoteState.CreateInstance(e.ReturnState, _NoteTextField, _FieldName, _NoteTextField, _NoteFilesField, _NoteActivityField, _NoteTextExistsField, _NoteFilesExistsField);
			}
		}
		protected string _CreateNoteText(PXCache sender, object row, string noteText)
		{
			if (_Searches == null)
			{
				return noteText;
			}
			List<string> ret = new List<string>();
			ret.Add(noteText ?? String.Empty);
			Type lastField = null;
			foreach (Type field in _Searches)
			{
				if (BqlCommand.GetItemType(field) != null)
				{
					if (BqlCommand.GetItemType(field).IsAssignableFrom(sender.GetItemType()))
					{
						ret.Add(this.GetValueFromState(sender, row, field));
						lastField = field;
					}
					else if (lastField != null && typeof(IBqlTable).IsAssignableFrom(BqlCommand.GetItemType(field)))
					{
						object foreign = PXSelectorAttribute.Select(sender, row, lastField.Name);
						if (foreign is PXResult)
							foreign = ((PXResult)foreign)[0];
						PXCache fcache;
						if (foreign == null || !(fcache = sender.Graph.Caches[BqlCommand.GetItemType(field)]).GetItemType().IsAssignableFrom(foreign.GetType()))
							ret.Add(String.Empty);
						else
							ret.Add(this.GetValueFromState(fcache, foreign, field));
					}
					else
					{
						ret.Add(String.Empty);
					}
				}
				else
				{
					ret.Add(String.Empty);
				}
			}
			return String.Join("\0", ret.ToArray());
		}

		protected void _UpdateNoteText(PXCache sender, object row, Note note)
		{
			if (!String.IsNullOrEmpty(note.NoteText))
			{
				string[] split = note.NoteText.Split('\0');
				if (split.Length > 0)
				{
					note.NoteText = _CreateNoteText(sender, row, split[0]);
				}
				else
				{
					note.NoteText = _CreateNoteText(sender, row, String.Empty);
				}
			}
			else
			{
				note.NoteText = _CreateNoteText(sender, row, String.Empty);
			}

			if (sender.Graph.Caches[typeof(Note)].GetStatus(note) != PXEntryStatus.Updated)
				note.EntityType = _declaringType;
		}

		private string GetValueFromState(PXCache cache, object row, Type field)
		{
			object val = cache.GetStateExt(row, field.Name);
			PXFieldState state = val as PXFieldState;

			if (state != null)
				val = state.Value;

			if (state is PXIntState)
			{
				PXIntState istate = (PXIntState)state;
				if (istate.AllowedValues != null && istate.AllowedLabels != null)
					for (int i = 0; i < istate.AllowedValues.Length && i < istate.AllowedLabels.Length; i++)
						if (istate.AllowedValues[i] == (int)val)
						{
							val = istate.AllowedLabels[i];
							break;
						}
			}
			else if (state is PXStringState)
			{
				PXStringState sstate = (PXStringState)state;
				if (sstate.AllowedValues != null && sstate.AllowedLabels != null)
					for (int i = 0; i < sstate.AllowedValues.Length && i < sstate.AllowedLabels.Length; i++)
						if (sstate.AllowedValues[i] == (string)val)
						{
							val = sstate.AllowedLabels[i];
							break;
						}
			}

			return val is string ? (string)val : string.Empty;
		}

		public virtual void noteTextFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null)
			{
				long? id = (long?)sender.GetValue(e.Row, _FieldOrdinal);
				SetNoteTextExists(id, (string)e.NewValue);
				if (_TextRequired && id > 0)
				{
					KeyValuePair<string, int> val;
					if (_Selected.TryGetValue((long)id, out val))
					{
						_Selected[(long)id] = new KeyValuePair<string, int>((string)e.NewValue, val.Value);
					}
				}
				if (_PassThrough || !sender.AllowUpdate && sender.GetStatus(e.Row) == PXEntryStatus.Notchanged)
				{
					if (String.IsNullOrEmpty((string)e.NewValue))
					{
						if (id != null)
						{
							using (PXTransactionScope tran = new PXTransactionScope())
							{
								try
								{
									PXDatabase.Update(typeof(Note),
										new PXDataFieldAssign("NoteText", PXDbType.NVarChar, 0, _CreateNoteText(sender, e.Row, String.Empty)),
										new PXDataFieldAssign("EntityType", PXDbType.VarChar, sender.GetItemType().FullName),
										new PXDataFieldRestrict("NoteID", PXDbType.BigInt, 8, id),
										PXDataFieldRestrict.OperationSwitchAllowed
									);
								}
								catch (PXDatabaseException ex)
								{
									if (ex.ErrorCode == PXDbExceptions.OperationSwitchRequired)
									{
										PXDatabase.Insert<Note>(
											new PXDataFieldAssign(typeof(Note.noteText).Name, PXDbType.NVarChar, 0, _CreateNoteText(sender, e.Row, String.Empty)),
											new PXDataFieldAssign(typeof(Note.entityType).Name, PXDbType.VarChar, sender.GetItemType().FullName),
											new PXDataFieldAssign(typeof(Note.graphType).Name, PXDbType.VarChar, sender.Graph.GetType().FullName)
										);
									}
									else
									{
										throw;
									}
								}
								tran.Complete();
							}
						}
					}
					else
					{
						string noteText = _CreateNoteText(sender, e.Row, (string)e.NewValue);
						if (id != null)
						{
							using (PXTransactionScope tran = new PXTransactionScope())
							{
								try
								{
									PXDatabase.Update(typeof(Note),
										new PXDataFieldAssign(typeof(Note.noteText).Name, PXDbType.NVarChar, noteText.Length, noteText),
										new PXDataFieldAssign(typeof(Note.entityType).Name, PXDbType.VarChar, sender.GetItemType().FullName),
										new PXDataFieldAssign(typeof(Note.graphType).Name, PXDbType.VarChar, sender.Graph.GetType().FullName),
										new PXDataFieldRestrict(typeof(Note.noteID).Name, PXDbType.BigInt, 8, id),
										PXDataFieldRestrict.OperationSwitchAllowed
									);
								}
								catch (PXDatabaseException ex)
								{
									if (ex.ErrorCode == PXDbExceptions.OperationSwitchRequired)
									{
										PXDatabase.Insert<Note>(
											new PXDataFieldAssign(typeof(Note.noteText).Name, PXDbType.NVarChar, noteText.Length, noteText),
											new PXDataFieldAssign(typeof(Note.entityType).Name, PXDbType.VarChar, sender.GetItemType().FullName),
											new PXDataFieldAssign(typeof(Note.graphType).Name, PXDbType.VarChar, sender.Graph.GetType().FullName)
										);
									}
									else
									{
										throw;
									}
								}
								tran.Complete();
							}
						}
						else
						{
							using (PXTransactionScope tran = new PXTransactionScope())
							{
								try
								{
									PXDatabase.Insert(typeof(Note),
										new PXDataFieldAssign(typeof(Note.noteText).Name, PXDbType.NVarChar, noteText.Length, noteText),
										new PXDataFieldAssign(typeof(Note.entityType).Name, PXDbType.VarChar, sender.GetItemType().FullName),
										new PXDataFieldAssign(typeof(Note.graphType).Name, PXDbType.VarChar, sender.Graph.GetType().FullName),
										PXDataFieldAssign.OperationSwitchAllowed
									);
								}
								catch (PXDatabaseException ex)
								{
									if (ex.ErrorCode == PXDbExceptions.OperationSwitchRequired)
									{
										PXDatabase.Update<Note>(
											new PXDataFieldAssign(typeof(Note.noteText).Name, PXDbType.NVarChar, noteText.Length, noteText),
											new PXDataFieldAssign(typeof(Note.entityType).Name, PXDbType.VarChar, sender.GetItemType().FullName),
											new PXDataFieldAssign(typeof(Note.graphType).Name, PXDbType.VarChar, sender.Graph.GetType().FullName),
											new PXDataFieldRestrict(typeof(Note.noteID).Name, PXDbType.BigInt, 8, null)
										);
									}
									else
									{
										throw;
									}
								}
								id = Convert.ToInt64(PXDatabase.SelectIdentity());
								if (id != null)
								{
									sender.SetValue(e.Row, _FieldOrdinal, id);
									updateTableWithId(sender, e.Row, id);
									tran.Complete();
								}
							}
						}
					}
				}
				else
				{
					if (id != null)
					{
						Note note = GetView(sender.Graph).SelectSingle(id) as Note;
						if (note != null)
						{
							note.NoteText = _CreateNoteText(sender, e.Row, (string)(e.NewValue ?? String.Empty));
							note.EntityType = _declaringType;
							note.GraphType = GetGraphType(sender);
							GetView(sender.Graph).Cache.Update(note);
						}
					}
					else if (!String.IsNullOrEmpty((string)e.NewValue) || _Searches != null)
					{
						Note note = new Note();
						note.NoteText = _CreateNoteText(sender, e.Row, (string)(e.NewValue ?? String.Empty));
						note.EntityType = _declaringType;
						note.GraphType = GetGraphType(sender);
						note = GetView(sender.Graph).Cache.Insert(note) as Note;
						if (note != null)
						{
							id = note.NoteID;
							SetNoteTextExists(id, note.NoteText);
							sender.SetValue(e.Row, _FieldOrdinal, id);
						}
					}
					if (sender.GetStatus(e.Row) == PXEntryStatus.Notchanged /*&& sender.Locate(e.Row) != null*/)
					{
						sender.SetStatus(e.Row, PXEntryStatus.Modified);
					}
					sender.IsDirty = true;
				}
			}
			else
			{
				_TextRequired = true;
				if (_Selected == null)
				{
					_Selected = new Dictionary<long, KeyValuePair<string, int>>();
				}
				PXCache cache = sender.Graph.Caches[_BqlTable];
				if (cache != sender)
				{
					object val = null;
					cache.RaiseFieldUpdating(_NoteTextField, null, ref val);
				}
			}
		}
		public virtual void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			long? id = (long?)sender.GetValue(e.Row, _FieldOrdinal);
			if (id != null)
			{
				PXCache cache = sender.Graph.Caches[typeof(Note)];
				Note note = new Note();
				note.NoteID = id;
				cache.Delete(note);

				PXCache dcache = sender.Graph.Caches[typeof(NoteDoc)];
				foreach (NoteDoc doc in GetDocView(sender.Graph).SelectMulti(id))
				{
					dcache.Delete(doc);
				}
			}
		}
		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			long? id = (long?)sender.GetValue(e.Row, _FieldOrdinal);
			PXCache cache = sender.Graph.Caches[typeof(Note)];
			PXCache dcache = sender.Graph.Caches[typeof(NoteDoc)];


			if (_Searches != null && id == null)
			{
				Note note = new Note();
				note.NoteText = _CreateNoteText(sender, e.Row, String.Empty);
				note.EntityType = _declaringType;
				note.GraphType = GetGraphType(sender);
				note = GetView(sender.Graph).Cache.Insert(note) as Note;
				if (note != null)
				{
					id = note.NoteID;
					sender.SetValue(e.Row, _FieldOrdinal, id);
				}
			}
			if (id != null)
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					if (id < 0)
					{
						foreach (Note note in cache.Inserted)
						{
							if (note.NoteID == id)
							{
								if (_Searches != null)
								{
									_UpdateNoteText(sender, e.Row, note);
								}
								cache.PersistInserted(note);
								id = Convert.ToInt64(PXDatabase.SelectIdentity());
								_KeyToAbort = (long?)sender.GetValue(e.Row, _FieldOrdinal);
								sender.SetValue(e.Row, _FieldOrdinal, id);
								note.NoteID = id;
								cache.Normalize();

								foreach (NoteDoc doc in dcache.Inserted)
								{
									if (doc.NoteID == _KeyToAbort)
									{
										doc.NoteID = note.NoteID;
										dcache.Normalize();
										string screenID = PX.Common.PXContext.GetScreenID();
										screenID = string.IsNullOrEmpty(screenID) ? screenID : screenID.Replace(".", "");
										if (!String.IsNullOrEmpty(screenID) && doc.FileID != null)
											PX.SM.UploadFileMaintenance.SetAccessSource(doc.FileID.Value, null, screenID.Replace(".", ""));
										else if (screenID == null && doc.FileID != null)
										{
											var sm = (PXSiteMap.Provider as PXDatabaseSiteMapProvider).
												With(_ => _.FindSiteMapNodeByGraphType(sender.Graph.GetType().FullName));

											//SiteMap sm = PXSelect<SiteMap, Where<SiteMap.graphtype, Equal<Required<SiteMap.graphtype>>>>.Select(sender.Graph, sender.Graph.GetType().FullName);
											if (sm != null)
												PX.SM.UploadFileMaintenance.SetAccessSource(doc.FileID.Value, null, sm.ScreenID);
										}
										dcache.PersistInserted(doc);
									}
								}
								break;
							}
						}
					}
					else
					{
						bool notefound = false;
						foreach (Note note in cache.Updated)
						{
							if (note.NoteID == id)
							{
								if (_Searches != null)
								{
									_UpdateNoteText(sender, e.Row, note);
								}
								cache.PersistUpdated(note);
								notefound = true;
								break;
							}
						}
						if (!notefound && _Searches != null)
						{
							Note note = GetView(sender.Graph).SelectSingle(id) as Note;
							if (note != null)
							{
								_UpdateNoteText(sender, e.Row, note);
								cache.SetStatus(note, PXEntryStatus.Updated);
								cache.PersistUpdated(note);
							}
						}
						foreach (NoteDoc doc in dcache.Inserted)
						{
							if (doc.NoteID == id)
							{
								dcache.PersistInserted(doc);
								string screenID = PX.Common.PXContext.GetScreenID();
								screenID = string.IsNullOrEmpty(screenID) ? screenID : screenID.Replace(".", "");
								if (!string.IsNullOrEmpty(screenID) && doc.FileID != null)
									PX.SM.UploadFileMaintenance.SetAccessSource(doc.FileID.Value, null, screenID);
								else if (screenID == null && doc.FileID != null)
								{
									var sm = (PXSiteMap.Provider as PXDatabaseSiteMapProvider).
										With(_ => _.FindSiteMapNodeByGraphType(sender.Graph.GetType().FullName));
									//SiteMap sm = PXSelect<SiteMap, Where<SiteMap.graphtype, Equal<Required<SiteMap.graphtype>>>>.Select(sender.Graph, sender.Graph.GetType().FullName);
									if (sm != null)
										PX.SM.UploadFileMaintenance.SetAccessSource(doc.FileID.Value, null, sm.ScreenID);
								}
							}
						}
					}
					ts.Complete();
				}
			}

			foreach (Note note in cache.Deleted)
			{
				cache.PersistDeleted(note);
			}
			foreach (NoteDoc doc in dcache.Deleted)
			{
				dcache.PersistDeleted(doc);
			}
		}
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(Note)];
			PXCache dcache = sender.Graph.Caches[typeof(NoteDoc)];
			long? id = (long?)sender.GetValue(e.Row, _FieldOrdinal);
			if (id == null)
			{
				if (e.TranStatus == PXTranStatus.Completed)
				{
					foreach (Note note in cache.Deleted)
					{
						cache.SetStatus(note, PXEntryStatus.Notchanged);
					}
					cache.IsDirty = false;
					foreach (NoteDoc doc in dcache.Deleted)
					{
						dcache.SetStatus(doc, PXEntryStatus.Notchanged);
					}
					dcache.IsDirty = false;
				}
				return;
			}
			if (e.TranStatus != PXTranStatus.Open)
			{
				if (e.TranStatus == PXTranStatus.Aborted)
				{
					if (_KeyToAbort != null)
					{
						sender.SetValue(e.Row, _FieldOrdinal, _KeyToAbort);
						foreach (Note note in cache.Inserted)
						{
							if (id == note.NoteID)
							{
								note.NoteID = _KeyToAbort;
								break;
							}
						}
						foreach (NoteDoc doc in dcache.Inserted)
						{
							if (id == doc.NoteID)
							{
								doc.NoteID = _KeyToAbort;
							}
						}
						_KeyToAbort = null;
					}
				}
				else
				{
					if (id > -1)
						foreach (Note note in cache.Inserted)
						{
							if (id == note.NoteID)
							{
								cache.SetStatus(note, PXEntryStatus.Notchanged);
								break;
							}
						}
					foreach (Note note in cache.Updated)
					{
						if (id == note.NoteID)
						{
							cache.SetStatus(note, PXEntryStatus.Notchanged);
							break;
						}
					}
					foreach (Note note in cache.Deleted)
					{
						cache.SetStatus(note, PXEntryStatus.Notchanged);
					}
					cache.IsDirty = false;

					foreach (NoteDoc doc in dcache.Inserted)
					{
						dcache.SetStatus(doc, PXEntryStatus.Notchanged);
					}
					foreach (NoteDoc doc in dcache.Deleted)
					{
						dcache.SetStatus(doc, PXEntryStatus.Notchanged);
					}
					dcache.IsDirty = false;
				}
				cache.Normalize();
				dcache.Normalize();
			}
			else if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				if (_Searches != null)
				{
					foreach (Note note in cache.Inserted)
					{
						if (id == note.NoteID)
						{
							_UpdateNoteText(sender, e.Row, note);
							PXDatabase.Update(typeof(Note),
								new PXDataFieldAssign(typeof(Note.noteText).Name, PXDbType.NVarChar, note.NoteText.Length, note.NoteText),
								new PXDataFieldRestrict(typeof(Note.noteID).Name, PXDbType.BigInt, 8, id));
							break;
						}
					}
				}
				string prefix = null;
				if (!ForceFileCorrection)
					foreach (NoteDoc doc in dcache.Inserted)
					{
						if (doc.FileID != null && doc.NoteID == id)
						{
							if (prefix == null)
							{
								string screenID = PX.Common.PXContext.GetScreenID();
								PXSiteMapNode node;
								if (!String.IsNullOrEmpty(screenID) && (node = PXSiteMap.Provider.FindSiteMapNodeByScreenIDUnsecure(screenID.Replace(".", ""))) != null && !String.IsNullOrEmpty(node.Title))
								{
									StringBuilder bld = new StringBuilder(node.Title);
									bld.Append(" (");
									for (int k = 0; k < sender.Keys.Count; k++)
									{
										if (k > 0)
										{
											bld.Append(", ");
										}
										bld.Append(sender.GetValue(e.Row, sender.Keys[k]));
									}
									bld.Append(")");
									prefix = bld.ToString();
								}
								else
								{
									return;
								}
							}
							string fileName = null;
							using (PXDataRecord record = PXDatabase.SelectSingle<UploadFile>(
								new PXDataField(typeof(UploadFile.name).Name),
								new PXDataFieldValue(typeof(UploadFile.fileID).Name, PXDbType.UniqueIdentifier, 16, doc.FileID)))
							{
								if (record != null)
								{
									fileName = record.GetString(0);
								}
							}
							if (fileName != null)
							{
								int idx = fileName.IndexOf(")\\");
								if (idx > fileName.IndexOf(" ("))
								{
									fileName = prefix + fileName.Substring(idx + 1);
								}
								else
								{
									fileName = prefix + "\\" + fileName;
								}
								PXDatabase.Update<UploadFile>(
									new PXDataFieldAssign(typeof(UploadFile.name).Name, PXDbType.NVarChar, 255, fileName),
									new PXDataFieldRestrict(typeof(UploadFile.fileID).Name, PXDbType.UniqueIdentifier, 16, doc.FileID),
									PXDataFieldRestrict.OperationSwitchAllowed);
							}
						}
					}
			}
		}
		public virtual void noteFilesFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			//Checking for virtual dac
			if (e.Row == null && PXDatabase.IsVirtualTable(sender.BqlTable))
			{
				e.ReturnValue = null;
				e.ReturnState = null;
				e.Cancel = true;
				return;
			}

			long? id = null;
			if (e.Row != null)
			{
				id = (long?)sender.GetValue(e.Row, _FieldOrdinal);
			}
			if (id != null)
			{
				//Set NoteID to the session to allow work with file even if user hasn't enough access rights
				PXContext.Session.SetString(string.Format("{0}+{1}", typeof(Note).FullName, id), id.ToString());
				PXContext.SetSlot(string.Format("{0}+{1}", typeof(Note).FullName, id), id.ToString());

				if (_FilesRequired)
				{
					KeyValuePair<string, int> val;
					if (_Selected.TryGetValue((long)id, out val) && val.Value == 0)
					{
						id = null;
					}
				}
				if (id != null)
				{
					List<string> ret = new List<string>();
					foreach (NoteDoc doc in GetDocView(sender.Graph).SelectMulti(id))
					{
						if (doc.FileID != null)
						{
							var fileId = doc.FileID.Value;
							PX.SM.UploadFile file = GetFileByID(sender.Graph).SelectSingle(fileId) as PX.SM.UploadFile;
							if (file != null && !string.IsNullOrEmpty(file.Name))
								ret.Add(fileId + "$" + file.Name + "$" + file.Comment);
						}
					}
					if (ret.Count > 0)
					{
						e.ReturnValue = ret.ToArray();
					}
					SetFilesExists(id, ret.Count);
				}
			}
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXNoteState.CreateInstance(e.ReturnState, _NoteTextField, _FieldName, _NoteTextField, _NoteFilesField, _NoteActivityField, _NoteTextExistsField, _NoteFilesExistsField);
			}
		}
		public virtual void noteImagesFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			long? id = null;
			if (e.Row != null)
			{
				id = (long?)sender.GetValue(e.Row, _FieldOrdinal);
			}
			if (id != null)
			{
				if (_FilesRequired)
				{
					KeyValuePair<string, int> val;
					if (_Selected.TryGetValue((long)id, out val) && val.Value == 0)
					{
						id = null;
					}
				}
				if (id != null)
				{

					List<Guid> ret = new List<Guid>();
					var docs = GetDocView(sender.Graph).SelectMulti(id);
					if (docs.Count > 0)
					{
						var imageExts = GetImageExtansions(sender.Graph);
						foreach (NoteDoc doc in docs)
							if (doc.FileID != null)
							{
								PX.SM.UploadFile file = GetFileByID(sender.Graph).SelectSingle(doc.FileID.Value) as PX.SM.UploadFile;
								if (file != null && !string.IsNullOrEmpty(file.Name) &&
									(imageExts.Length == 0 ||
										Array.Find(imageExts, (item) => string.Compare(item, file.Extansion, true) == 0) != null))
								{
									ret.Add((Guid)file.FileID);
								}
							}
					}
					if (ret.Count > 0)
					{
						e.ReturnValue = ret.ToArray();
					}
				}
			}
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXNoteState.CreateInstance(e.ReturnState, _NoteTextField, _FieldName, _NoteTextField, _NoteFilesField, _NoteActivityField, _NoteTextExistsField, _NoteFilesExistsField);
			}
		}
		public virtual void noteFilesFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			Guid[] docs = e.NewValue as Guid[];
			if (docs != null && e.Row != null)
			{
				long? id = (long?)sender.GetValue(e.Row, _FieldOrdinal);
				SetFilesExists(id, docs.Count());
				if (_FilesRequired && id > 0)
				{
					KeyValuePair<string, int> val;
					if (_Selected.TryGetValue((long)id, out val))
					{
						_Selected[(long)id] = new KeyValuePair<string, int>(val.Key, val.Value + 1);
					}
				}
				if (id > 0 || _PassThrough || !sender.AllowUpdate && sender.GetStatus(e.Row) == PXEntryStatus.Notchanged)
				{
					using (PXTransactionScope tran = new PXTransactionScope())
					{
						if (id == null)
						{
							try
							{
								PXDatabase.Insert(typeof(Note),
									new PXDataFieldAssign(typeof(Note.noteText).Name, PXDbType.NVarChar, 0, ""),
									new PXDataFieldAssign(typeof(Note.entityType).Name, PXDbType.VarChar, sender.GetItemType().FullName),
									new PXDataFieldAssign(typeof(Note.graphType).Name, PXDbType.VarChar, sender.Graph.GetType().FullName),
									PXDataFieldAssign.OperationSwitchAllowed
								);
							}
							catch (PXDatabaseException ex)
							{
								if (ex.ErrorCode == PXDbExceptions.OperationSwitchRequired)
								{
									PXDatabase.Update<Note>(
										new PXDataFieldAssign(typeof(Note.noteText).Name, PXDbType.NVarChar, 0, ""),
										new PXDataFieldAssign(typeof(Note.entityType).Name, PXDbType.VarChar, sender.GetItemType().FullName),
										new PXDataFieldAssign(typeof(Note.graphType).Name, PXDbType.VarChar, sender.Graph.GetType().FullName),
										new PXDataFieldRestrict(typeof(Note.noteID).Name, PXDbType.BigInt, 8, null)
									);
								}
								else
								{
									throw;
								}
							}
							id = Convert.ToInt64(PXDatabase.SelectIdentity());
							if (id != null)
							{
								sender.SetValue(e.Row, _FieldOrdinal, id);
								updateTableWithId(sender, e.Row, id);
							}
						}
						if (id != null)
						{
							try
							{
								PXDatabase.Update(typeof(Note),
									new PXDataFieldAssign(typeof(Note.entityType).Name, PXDbType.VarChar, sender.GetItemType().FullName),
									new PXDataFieldAssign(typeof(Note.graphType).Name, PXDbType.VarChar, sender.Graph.GetType().FullName),
									new PXDataFieldRestrict(typeof(Note.noteID).Name, PXDbType.BigInt, 8, id),
									PXDataFieldRestrict.OperationSwitchAllowed
									);
							}
							catch (PXDatabaseException ex)
							{
								if (ex.ErrorCode == PXDbExceptions.OperationSwitchRequired)
								{
									PXDatabase.Insert<Note>(
										new PXDataFieldAssign(typeof(Note.noteText).Name, PXDbType.NVarChar, 0, ""),
										new PXDataFieldAssign(typeof(Note.entityType).Name, PXDbType.VarChar, sender.GetItemType().FullName),
										new PXDataFieldAssign(typeof(Note.graphType).Name, PXDbType.VarChar, sender.Graph.GetType().FullName)
									);
								}
								else
								{
									throw;
								}
							}
							foreach (Guid did in docs)
							{
								PXDatabase.Delete(typeof(NoteDoc),
								new PXDataFieldRestrict(typeof(NoteDoc.noteID).Name, PXDbType.BigInt, 8, id),
								new PXDataFieldRestrict(typeof(NoteDoc.fileID).Name, PXDbType.UniqueIdentifier, did));

								try
								{
									PXDatabase.Insert(typeof(NoteDoc),
										new PXDataFieldAssign(typeof(NoteDoc.noteID).Name, PXDbType.BigInt, 8, id),
										new PXDataFieldAssign(typeof(NoteDoc.fileID).Name, PXDbType.UniqueIdentifier, did),
										PXDataFieldAssign.OperationSwitchAllowed
										);
								}
								catch (PXDatabaseException ex)
								{
									if (ex.ErrorCode == PXDbExceptions.OperationSwitchRequired)
									{
										PXDatabase.Update<NoteDoc>(
											new PXDataFieldAssign(typeof(NoteDoc.noteID).Name, PXDbType.BigInt, 8, id),
											new PXDataFieldAssign(typeof(NoteDoc.fileID).Name, PXDbType.UniqueIdentifier, did),
											new PXDataFieldRestrict(typeof(NoteDoc.noteID).Name, PXDbType.BigInt, 8, id),
											new PXDataFieldRestrict(typeof(NoteDoc.fileID).Name, PXDbType.UniqueIdentifier, did)
										);
									}
									else
									{
										throw;
									}
								}

								string screenID = PX.Common.PXContext.GetScreenID();
								if (!String.IsNullOrEmpty(screenID))
									PX.SM.UploadFileMaintenance.SetAccessSource(did, null, screenID.Replace(".", ""));
							}
							if (docs.Length > 0)
							{
								sender.Graph.Caches[typeof(NoteDoc)].ClearQueryCache();
							}
						}
						tran.Complete();
					}
				}
				else
				{
					if (id == null)
					{
						PXCache cache = sender.Graph.Caches[typeof(Note)];
						Note note = cache.Insert() as Note;
						if (note != null)
						{
							note.NoteText = String.Empty;
							note.EntityType = _declaringType;
							note.GraphType = GetGraphType(sender);
							id = note.NoteID;
							sender.SetValue(e.Row, _FieldOrdinal, id);
						}
					}
					if (id != null)
					{
						PXCache dcache = sender.Graph.Caches[typeof(NoteDoc)];
						PXResultset<NoteDoc> resultset = PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>.Select(sender.Graph, id);
						Dictionary<Guid, NoteDoc> existing = new Dictionary<Guid, NoteDoc>(resultset.Count);
						bool washere = false;
						foreach (NoteDoc nd in resultset)
							existing[nd.FileID.Value] = nd;

						foreach (Guid docID in docs)
						{
							if (existing.ContainsKey(docID))
								continue;

							NoteDoc doc = new NoteDoc();
							doc.NoteID = id;
							doc.FileID = docID;
							dcache.Insert(doc);
							washere = true;
						}
						if (washere)
						{
							if (sender.GetStatus(e.Row) == PXEntryStatus.Notchanged)
								sender.SetStatus(e.Row, PXEntryStatus.Updated);
							sender.IsDirty = true;
						}
					}
				}
			}
			else if (e.Row == null)
			{
				_FilesRequired = true;
				if (_Selected == null)
				{
					_Selected = new Dictionary<long, KeyValuePair<string, int>>();
				}
				PXCache cache = sender.Graph.Caches[_BqlTable];
				if (cache != sender)
				{
					object val = null;
					cache.RaiseFieldUpdating(_NoteFilesField, null, ref val);
				}
			}
		}
		public virtual void noteActivityFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			long? id = null;
			if (e.Row != null)
				id = (long?)sender.GetValue(e.Row, _FieldOrdinal);

			e.ReturnValue = null;
			if (id != null && id > 0)
				e.ReturnValue = PX.Data.EP.ActivityService.GetCount(id);

			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXNoteState.CreateInstance(e.ReturnState, _NoteTextField, _FieldName, _NoteTextField, _NoteFilesField, _NoteActivityField, _NoteTextExistsField, _NoteFilesExistsField);
			}
		}

		public virtual void noteActivityFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null)
			{
				long? id = (long?)sender.GetValue(e.Row, _FieldOrdinal);
				if (_ActivityRequired && id > 0)
				{
					KeyValuePair<string, int> val;
					if (_Selected.TryGetValue((long)id, out val))
					{
						_Selected[(long)id] = new KeyValuePair<string, int>(val.Key, val.Value + 1);
					}
				}
				if (_PassThrough || !sender.AllowUpdate && sender.GetStatus(e.Row) == PXEntryStatus.Notchanged)
				{
					using (PXTransactionScope tran = new PXTransactionScope())
					{
						if (id == null)
						{
							try
							{
								PXDatabase.Insert(typeof(Note),
									new PXDataFieldAssign(typeof(Note.noteText).Name, PXDbType.NVarChar, 0, ""),
									new PXDataFieldAssign(typeof(Note.entityType).Name, PXDbType.VarChar, sender.GetItemType().FullName),
									new PXDataFieldAssign(typeof(Note.graphType).Name, PXDbType.VarChar, sender.Graph.GetType().FullName),
									PXDataFieldAssign.OperationSwitchAllowed
								);
							}
							catch (PXDatabaseException ex)
							{
								if (ex.ErrorCode == PXDbExceptions.OperationSwitchRequired)
								{
									PXDatabase.Update<Note>(
										new PXDataFieldAssign(typeof(Note.noteText).Name, PXDbType.NVarChar, 0, ""),
										new PXDataFieldAssign(typeof(Note.entityType).Name, PXDbType.VarChar, sender.GetItemType().FullName),
										new PXDataFieldAssign(typeof(Note.graphType).Name, PXDbType.VarChar, sender.Graph.GetType().FullName),
										new PXDataFieldRestrict(typeof(Note.noteID).Name, PXDbType.BigInt, 8, null)
									);
								}
								else
								{
									throw;
								}
							}
							id = Convert.ToInt64(PXDatabase.SelectIdentity());
							if (id != null)
							{
								sender.SetValue(e.Row, _FieldOrdinal, id);
								updateTableWithId(sender, e.Row, id);
							}
						}
						if (id != null)
						{
							try
							{
								PXDatabase.Update(typeof(Note),
									new PXDataFieldAssign(typeof(Note.entityType).Name, PXDbType.VarChar, sender.GetItemType().FullName),
									new PXDataFieldAssign(typeof(Note.graphType).Name, PXDbType.VarChar, sender.Graph.GetType().FullName),
									new PXDataFieldRestrict(typeof(Note.noteID).Name, PXDbType.BigInt, 8, id),
									PXDataFieldRestrict.OperationSwitchAllowed
									);
							}
							catch (PXDatabaseException ex)
							{
								if (ex.ErrorCode == PXDbExceptions.OperationSwitchRequired)
								{
									PXDatabase.Insert<Note>(
										new PXDataFieldAssign(typeof(Note.noteText).Name, PXDbType.NVarChar, 0, ""),
										new PXDataFieldAssign(typeof(Note.entityType).Name, PXDbType.VarChar, sender.GetItemType().FullName),
										new PXDataFieldAssign(typeof(Note.graphType).Name, PXDbType.VarChar, sender.Graph.GetType().FullName)
									);
								}
								else
								{
									throw;
								}
							}
						}
						tran.Complete();
					}
				}
				else
				{
					if (id == null)
					{
						PXCache cache = sender.Graph.Caches[typeof(Note)];
						Note note = cache.Insert() as Note;
						if (note != null)
						{
							note.NoteText = String.Empty;
							note.EntityType = _declaringType;
							note.GraphType = GetGraphType(sender);
							id = note.NoteID;
							sender.SetValue(e.Row, _FieldOrdinal, id);
						}
					}
					if (id != null)
					{
						if (sender.GetStatus(e.Row) == PXEntryStatus.Notchanged)
							sender.SetStatus(e.Row, PXEntryStatus.Updated);
						sender.IsDirty = true;
					}
				}
			}
			else if (e.Row == null)
			{
				_ActivityRequired = true;
				if (_Selected == null)
				{
					_Selected = new Dictionary<long, KeyValuePair<string, int>>();
				}
				PXCache cache = sender.Graph.Caches[_BqlTable];
				if (cache != sender)
				{
					object val = null;
					cache.RaiseFieldUpdating(_NoteFilesField, null, ref val);
				}
			}
		}

		public Type DescriptionField { get; set; }

		public Type Selector { get; set; }

		public Type[] FieldList { get; set; }

		public static IEnumerable<Type> SearchableTypes
		{
			get
			{
				foreach (Type type in ServiceManager.Tables)
				{
					foreach (PropertyInfo prop in type.GetProperties())
					{
						if (prop.DeclaringType == type && prop.IsDefined(typeof(PXNoteAttribute), false))
						{
							PXNoteAttribute[] attr =
								(PXNoteAttribute[])prop.GetCustomAttributes(typeof(PXNoteAttribute), true);
							if (attr[0].Searches != null && attr[0].Searches.Length > 0)
								yield return type;
						}
					}
				}
			}
		}

		protected virtual void noteTextCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select)
			{
				if (_TextRequired)
				{
					if (!_BqlTable.IsAssignableFrom(sender.BqlTable))
					{
						if (sender.Graph.Caches[_BqlTable].BqlSelect != null &&
							((e.Operation & PXDBOperation.Option) == PXDBOperation.External ||
							(e.Operation & PXDBOperation.Option) == PXDBOperation.Normal && e.Value == null))
						{
							e.Cancel = true;
							e.DataType = PXDbType.NVarChar;
							e.DataValue = e.Value;
							e.BqlTable = _BqlTable;
							e.FieldName = BqlCommand.SubSelect + " NoteText FROM Note WHERE Note.NoteID = " + ((e.Operation & PXDBOperation.Option) == PXDBOperation.External ? sender.GetItemType().Name : _BqlTable.Name) + '.' + _DatabaseFieldName + ')';
						}
						else
						{
							PXCommandPreparingEventArgs.FieldDescription description;
							e.Cancel = !sender.Graph.Caches[_BqlTable].RaiseCommandPreparing(_DatabaseFieldName, e.Row, e.Value, e.Operation, e.Table, out description);
							if (description != null)
							{
								e.DataType = description.DataType;
								e.DataValue = description.DataValue;
								e.BqlTable = _BqlTable;
								e.FieldName = BqlCommand.SubSelect + " NoteText FROM Note WHERE Note.NoteID = " + description.FieldName + ')';
							}
						}
					}
					else if (((e.Operation & PXDBOperation.Option) == PXDBOperation.External ||
							(e.Operation & PXDBOperation.Option) == PXDBOperation.Normal && e.Value == null))
					{
						e.Cancel = true;
						e.DataType = PXDbType.NVarChar;
						e.DataValue = e.Value;
						e.BqlTable = _BqlTable;
						e.FieldName = BqlCommand.SubSelect + " NoteText FROM Note WHERE Note.NoteID = ";
						if ((e.Operation & PXDBOperation.Option) == PXDBOperation.External)
						{
							e.FieldName = e.FieldName + sender.GetItemType().Name;
						}
						else if (e.Table != null)
						{
							e.FieldName = e.FieldName + e.Table.Name;
						}
						else
						{
							e.FieldName = e.FieldName + _BqlTable.Name;
						}
						e.FieldName = e.FieldName + '.' + _DatabaseFieldName + ')';
					}
					else
					{
						e.FieldName = BqlCommand.Null;
					}
				}
				else
				{
					e.FieldName = BqlCommand.Null;
				}
			}
			else
			{
				e.FieldName = BqlCommand.Null;
			}
		}

		protected virtual void noteFilesCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select)
			{
				if (_FilesRequired)
				{
					if (!_BqlTable.IsAssignableFrom(sender.BqlTable))
					{
						if (sender.Graph.Caches[_BqlTable].BqlSelect != null &&
							((e.Operation & PXDBOperation.Option) == PXDBOperation.External ||
							(e.Operation & PXDBOperation.Option) == PXDBOperation.Normal && e.Value == null))
						{
							e.Cancel = true;
							e.DataType = PXDbType.NVarChar;
							e.DataValue = e.Value;
							e.BqlTable = _BqlTable;
							e.FieldName = BqlCommand.SubSelect + " COUNT(*) FROM NoteDoc WHERE NoteDoc.NoteID = " + ((e.Operation & PXDBOperation.Option) == PXDBOperation.External ? sender.GetItemType().Name : _BqlTable.Name) + '.' + _DatabaseFieldName + ')';
						}
						else
						{
							PXCommandPreparingEventArgs.FieldDescription description;
							e.Cancel = !sender.Graph.Caches[_BqlTable].RaiseCommandPreparing(_DatabaseFieldName, e.Row, e.Value, e.Operation, e.Table, out description);
							if (description != null)
							{
								e.DataType = description.DataType;
								e.DataValue = description.DataValue;
								e.BqlTable = _BqlTable;
								e.FieldName = BqlCommand.SubSelect + " COUNT(*) FROM NoteDoc WHERE NoteDoc.NoteID = " + description.FieldName + ')';
							}
						}
					}
					else if (((e.Operation & PXDBOperation.Option) == PXDBOperation.External ||
							(e.Operation & PXDBOperation.Option) == PXDBOperation.Normal && e.Value == null))
					{
						e.Cancel = true;
						e.DataType = PXDbType.NVarChar;
						e.DataValue = e.Value;
						e.BqlTable = _BqlTable;
						e.FieldName = BqlCommand.SubSelect + " COUNT(*) FROM NoteDoc WHERE NoteDoc.NoteID = ";
						if ((e.Operation & PXDBOperation.Option) == PXDBOperation.External)
						{
							e.FieldName = e.FieldName + sender.GetItemType().Name;
						}
						else if (e.Table != null)
						{
							e.FieldName = e.FieldName + e.Table.Name;
						}
						else
						{
							e.FieldName = e.FieldName + _BqlTable.Name;
						}
						e.FieldName = e.FieldName + '.' + _DatabaseFieldName + ')';
					}
					else
					{
						e.FieldName = BqlCommand.Null;
					}
				}
				else
				{
					e.FieldName = BqlCommand.Null;
				}
			}
			else
			{
				e.FieldName = BqlCommand.Null;
			}
		}

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			base.CommandPreparing(sender, e);
			if ((e.Operation & PXDBOperation.Option) == PXDBOperation.Internal && !String.IsNullOrEmpty(e.FieldName))
			{
				e.BqlTable = _BqlTable;
				e.FieldName = e.FieldName + ", NULL, NULL";
			}
		}

		public void SetFilesExists(long? id, int? count)
		{
			if  (id == null)
				return;
			_FilesExists[(long)id] = count != null && count > 0;
		}

		public void SetNoteTextExists(long? id, string text)
		{
			if (id == null)
				return;
			_TextExists[(long)id] = !String.IsNullOrEmpty(text != null ? text.Split('\0')[0] : null);
		}

		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				base.RowSelecting(sender, e);
				long? id = (long?)sender.GetValue(e.Row, _FieldOrdinal);
				string text = e.Record.GetString(e.Position);
				e.Position++;
				int? count = e.Record.GetInt32(e.Position);
				e.Position++;

				if (_Selected != null && id != null && (text != null || count != null))
				{
					if (!String.IsNullOrEmpty(text))
					{
						string[] split = text.Split('\0');
						if (split.Length > 0)
						{
							text = split[0];
						}
					}
					_Selected[(long)id] = new KeyValuePair<string, int>(text, count ?? 0);
					SetNoteTextExists(id, text);
					SetFilesExists(id, count);
				}
			}
			else
			{
				e.Position += 3;
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.Views.Caches.Remove(typeof(Note));

			int fieldIdx = sender.Fields.IndexOf(_FieldName);
			_declaringType = sender.GetItemType().GetProperty(_FieldName).DeclaringType.FullName;

			sender.Fields.Insert(++fieldIdx, _NoteTextField);
			string field = _NoteTextField.ToLower();
			sender.FieldSelectingEvents[field] += noteTextFieldSelecting;
			sender.FieldUpdatingEvents[field] += noteTextFieldUpdating;
			sender.CommandPreparingEvents[field] += noteTextCommandPreparing;
			PXCache cache = sender.Graph.Caches[typeof(Note)];

			sender.Fields.Insert(++fieldIdx, _NoteFilesField);
			field = _NoteFilesField.ToLower();
			sender.FieldSelectingEvents[field] += noteFilesFieldSelecting;
			sender.FieldUpdatingEvents[field] += noteFilesFieldUpdating;
			sender.CommandPreparingEvents[field] += noteFilesCommandPreparing;

			sender.Fields.Insert(++fieldIdx, _NoteImagesField);
			sender.FieldSelectingEvents[_NoteImagesField.ToLower()] += noteImagesFieldSelecting;

			sender.Fields.Add(_NoteActivityField);
			field = _NoteActivityField.ToLower();
			sender.FieldSelectingEvents[field] += noteActivityFieldSelecting;
			sender.FieldUpdatingEvents[field] += noteActivityFieldUpdating;
			cache = sender.Graph.Caches[typeof(NoteDoc)];

			sender.Fields.Insert(++fieldIdx, _NoteTextExistsField);
			sender.FieldSelectingEvents[_NoteTextExistsField.ToLower()] += noteTextExistsFieldSelecting;

			sender.Fields.Insert(++fieldIdx, _NoteFilesExistsField);
			sender.FieldSelectingEvents[_NoteFilesExistsField.ToLower()] += noteFilesExistsFieldSelecting;

			var imagesSelect = new Select<UploadFile>();
			var imagesView = new PXView(sender.Graph, true, new Select<UploadFile>(),
				new PXSelectDelegate(
					() =>
					{
						var list = new List<UploadFile>();
						var state = sender.GetStateExt(sender.Current, _NoteImagesField) as PXNoteState; //TODO: need review 'sender.Current'
						Guid[] fileIDs;
						if (state != null && (fileIDs = state.Value as Guid[]) != null)
						{
							var inWhere = InHelper.Create(typeof(UploadFile.fileID), fileIDs.Length);
							var select = imagesSelect.WhereNew(inWhere);
							var view = new PXView(sender.Graph, true, select);
							var parameters = Array.ConvertAll(fileIDs, input => (object)input);
							foreach (UploadFile file in view.SelectMulti(parameters))
							{
								var index = string.IsNullOrEmpty(file.Name) ? -1 : file.Name.LastIndexOf('\\');
								var copy = file;
								if (index > -1 && index < file.Name.Length - 1)
								{
									copy = (UploadFile)view.Cache.CreateCopy(file);
									copy.Name = file.Name.Substring(index + 1);
								}
								list.Add(copy);
							}
						}
						return list;
					}));
			var imagesViewName = _NoteImagesViewPrefix + sender.GetItemType().Name;
			sender.Graph.Views.Add(imagesViewName, imagesView);
		}

		private void noteFilesExistsFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null && PXDatabase.IsVirtualTable(sender.BqlTable))
			{
				e.ReturnValue = null;
				e.ReturnState = null;
				e.Cancel = true;
				return;
			}

			long? id = null;
			if (e.Row != null)
			{
				id = (long?)sender.GetValue(e.Row, _FieldOrdinal);
			}

			if (id != null)
			{
				if (_FilesExists.ContainsKey((long)id))
				{
					e.ReturnValue = _FilesExists[(long)id];
				}
				else
				{
					bool value = GetDocView(sender.Graph).SelectMulti(id).Count() > 0;
					e.ReturnValue = value;
					_FilesExists[(long)id] = value;
				}
			}
		}

		private void noteTextExistsFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null && PXDatabase.IsVirtualTable(sender.BqlTable))
			{
				e.ReturnValue = null;
				e.ReturnState = null;
				e.Cancel = true;
				return;
			}

			long? id = null;
			if (e.Row != null)
			{
				id = (long?)sender.GetValue(e.Row, _FieldOrdinal);
			}

			if (id != null)
			{
				if (_TextExists.ContainsKey((long)id))
				{
					e.ReturnValue = _TextExists[(long)id];
				}
				else
				{
					Note note = (GetView(sender.Graph).SelectSingle(id) as Note);
					if (note == null)
						_TextExists[(long) id] = false;
					else
						SetNoteTextExists((long) id, note.NoteText);
					e.ReturnValue = _TextExists[(long)id];
				}
			}
		}

		#endregion
	}
	public class PXNoteTextAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			String value = e.ReturnValue as String;
			if (String.IsNullOrWhiteSpace(value)) return;

			String[] parts = value.Split('\0');
			if (parts.Length <= 1) return;

			e.ReturnValue = parts[0];
		}
	}
	#endregion

	#region PXRefNoteAttibute
	public class PXRefNoteAttribute : PXDBLongAttribute
	{
		public bool FullDescription { get; set; }
		public bool LastKeyOnly { get; set; }

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			helper = new EntityHelper(sender.Graph);
		}
		#endregion

		#region Implementation
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			long? refNoteID = (long?)sender.GetValue(e.Row, _FieldOrdinal);
			e.ReturnValue =
				refNoteID != null ?
				(this.FullDescription ?
					helper.GetEntityRowValues(refNoteID.Value) :
					(this.LastKeyOnly ?
						helper.GetEntityRowID(refNoteID.Value, null) :
						helper.GetEntityRowID(refNoteID.Value))) :
				string.Empty;
		}
		protected EntityHelper helper;
		#endregion
	}
	#endregion

	#region PXEntityNameAttribute
	public class PXEntityNameAttribute : PXStringListAttribute
	{

		#region Initialization
		public PXEntityNameAttribute(Type refNoteID)
		{
			this.refNoteID = refNoteID;
		}
		private readonly Type refNoteID;
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			helper = new EntityHelper(sender.Graph);
		}
		#endregion

		#region Implementation
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[refNoteID.DeclaringType];
			long? id = (long?)cache.GetValue(cache == sender ? e.Row : cache.Current, refNoteID.Name);
			e.ReturnValue = helper.GetFriendlyEntityName(id);
		}
		private EntityHelper helper;
		#endregion
	}
	#endregion

	#region PXScalarAttribute
	[PXAttributeFamily(typeof(PXDBFieldAttribute))]
	public class PXDBScalarAttribute : PXDBFieldAttribute
	{
		protected BqlCommand _Search;
		protected Dictionary<CacheKey, string> dict = new Dictionary<CacheKey, string>();
		protected Type typeOfProperty;

		protected sealed class CacheKey
		{
			private Type _GraphType;
			private Type _CacheType;
			private Type _TableType;
			private int? _HashCode;
			public CacheKey(Type graphType, Type cacheType, Type tableType)
			{
				_GraphType = graphType;
				_CacheType = cacheType;
				_TableType = tableType;
			}
			public override bool Equals(object obj)
			{
				CacheKey that = obj as CacheKey;
				if (that == null)
				{
					return false;
				}
				return _GraphType == that._GraphType && _CacheType == that._CacheType && _TableType == that._TableType;
			}
			public override int GetHashCode()
			{
				unchecked
				{
					if (_HashCode == null)
					{
						int result = 13;
						result = 37 * result;
						result += _GraphType.GetHashCode();
						result = 37 * result;
						result += _CacheType.GetHashCode();
						result = 37 * result;
						if (_TableType != null)
						{
							result += _TableType.GetHashCode();
						}
						_HashCode = result;
					}
					return (int)_HashCode;
				}
			}
		}
		public PXDBScalarAttribute(Type search)
		{
			_Search = BqlCommand.CreateInstance(search);
			BqlField = ((IBqlSearch)_Search).GetField();
		}
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && !sender.BypassCalced)
			{
				if (((e.Operation & PXDBOperation.Option) == PXDBOperation.Normal ||
				(e.Operation & PXDBOperation.Option) == PXDBOperation.Internal))
				{
					CacheKey key = new CacheKey(sender.Graph.GetType(), sender.GetItemType(), e.Table);
					lock (((ICollection)dict).SyncRoot)
					{
						string select = null;
						if (dict.TryGetValue(key, out select))
						{
							e.BqlTable = _BqlTable;
							e.FieldName = select;
							return;
						}
					}

					StringBuilder text = new StringBuilder();
					List<Type> tables = null;
					if (sender.GetItemType().IsAssignableFrom(BqlCommand.GetItemType(((IBqlSearch)_Search).GetField())))
					{
						tables = new List<Type>(new Type[] { sender.GetItemType() });
					}
					else if (e.Table != null)
					{
						tables = new List<Type>(new Type[] { e.Table });
					}
					else
					{
						tables = new List<Type>();
					}

					_Search.Parse(sender.Graph, new List<IBqlParameter>(), tables, null, null, text, null);

					Type field = ((IBqlSearch)_Search).GetField();

					string function = "";
					if (_Search is IBqlAggregate)
					{
						IBqlFunction[] functions = ((IBqlAggregate)_Search).GetAggregates();
						for (int i = 0; i < functions.Length; i++)
						{
							if (functions[i].GetField() == field)
							{
								function = functions[i].GetFunction();
								break;
							}
						}
					}

					if (field.DeclaringType.IsAssignableFrom(sender.GetItemType()) &&
						string.Equals(_FieldName, field.Name, StringComparison.OrdinalIgnoreCase))
					{
						e.FieldName = string.Format("{0}.{1}", tables.First().Name, _FieldName);
					}
					else
					{
						var fieldtext = BqlCommand.GetSingleField(field, sender.Graph, new List<Type>(), null);
						e.FieldName = BqlCommand.SubSelect + function + "(" + (fieldtext.Length > 0 ? fieldtext : field.Name) + ") " + text + ")";
					}


					e.BqlTable = _BqlTable;
					lock (((ICollection)dict).SyncRoot)
					{
						dict[key] = e.FieldName;
					}
				}
				else if ((e.Operation & PXDBOperation.Option) == PXDBOperation.GroupBy)
				{
					e.FieldName = BqlCommand.Null;
				}
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			typeOfProperty = sender.GetFieldType(this._FieldName);
		}

		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				object dbValue;

				// mySQL returns LONG from scalar selects, shorts when querying tinyint, so we should take destination field type into consideration.
				if (typeOfProperty == typeof(int) || typeOfProperty == typeof(int?))
					dbValue = e.Record.GetInt32(e.Position);
				else if (typeOfProperty == typeof(short) || typeOfProperty == typeof(short?))
					dbValue = e.Record.GetInt16(e.Position);
				else if (typeOfProperty == typeof(long) || typeOfProperty == typeof(long?))
					dbValue = e.Record.GetInt64(e.Position);
				else if (typeOfProperty == typeof(bool) || typeOfProperty == typeof(bool?))
					dbValue = e.Record.GetBoolean(e.Position);
				else
					dbValue = e.Record.GetValue(e.Position);

				sender.SetValue(e.Row, _FieldOrdinal, dbValue);
			}
			e.Position++;
		}
	}
	#endregion

	#region PXDBCryptStringAttribute
	public class PXDBCryptStringAttribute : PXDBStringAttribute, IPXFieldVerifyingSubscriber, IPXRowUpdatingSubscriber, IPXRowSelectingSubscriber
	{
		#region State
		protected bool isViewDeprypted;
		protected string viewAsString;
		protected Type viewAsField;
		protected bool isEncryptionRequired = true;

		public bool IsViewDecrypted
		{
			get { return isViewDeprypted; }
			set { isViewDeprypted = value; }
		}

		public string ViewAsString
		{
			get { return this.viewAsString; }
			set { this.viewAsField = null; this.viewAsString = value; }
		}

		public Type ViewAsField
		{
			get { return this.viewAsField; }
			set
			{
				this.viewAsField = value; this.viewAsString = null;
			}
		}

		public bool IsEncryptionRequired
		{
			get { return isEncryptionRequired; }
			set { isEncryptionRequired = value; }
		}
		#endregion

		#region Ctor
		public PXDBCryptStringAttribute()
		{
		}
		public PXDBCryptStringAttribute(int length)
			: base(length)
		{
		}
		#endregion

		#region Runtime
		/// <summary>
		/// Overrides the visible state for the particular data item
		/// </summary>
		/// <param name="cache">Cache containing the data item</param>		
		/// <param name="def">Default value</param>
		public static void SetDecrypted(PXCache cache, object data, string field, bool isDecrypted)
		{
			if (data == null)
			{
				cache.SetAltered(field, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, field))
			{
				if (attr is PXDBCryptStringAttribute)
				{
					((PXDBCryptStringAttribute)attr).IsViewDecrypted = isDecrypted;
				}
			}
		}
		/// <summary>
		/// Overrides the visible state for the particular data item
		/// </summary>
		/// <param name="cache">Cache containing the data item</param>		
		/// <param name="def">Default value</param>
		public static void SetDecrypted(PXCache cache, string field, bool isDecrypted)
		{
			cache.SetAltered(field, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(field))
			{
				if (attr is PXDBCryptStringAttribute)
				{
					((PXDBCryptStringAttribute)attr).IsViewDecrypted = isDecrypted;
					break;
				}
			}
		}
		/// <summary>
		/// Overrides the visible state for the particular data item
		/// </summary>
		/// <param name="cache">Cache containing the data item</param>		
		/// <param name="def">Default value</param>
		public static void SetDecrypted<Field>(PXCache cache, object data, bool isDecrypted)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXDBCryptStringAttribute)
				{
					((PXDBCryptStringAttribute)attr).IsViewDecrypted = isDecrypted;
				}
			}
		}
		/// <summary>
		/// Overrides the view as state for the particular data item
		/// </summary>
		/// <typeparam name="Field">Field to set default value for</typeparam>
		/// <param name="cache">Cache containing the data item</param>		
		/// <param name="def">Default value</param>
		public static void SetDecrypted<Field>(PXCache cache, bool isDecrypted)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXDBCryptStringAttribute)
				{
					((PXDBCryptStringAttribute)attr).IsViewDecrypted = isDecrypted;
					break;
				}
			}
		}

		/// <summary>
		/// Overrides the view as state for the particular data item
		/// </summary>
		/// <param name="cache">Cache containing the data item</param>		
		/// <param name="def">Default value</param>
		public static void SetViewAs(PXCache cache, object data, string field, string source)
		{
			if (data == null)
			{
				cache.SetAltered(field, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, field))
			{
				if (attr is PXDBCryptStringAttribute)
				{
					((PXDBCryptStringAttribute)attr).ViewAsString = source;
				}
			}
		}
		/// <summary>
		/// Overrides the view as state for the particular data item
		/// </summary>
		/// <param name="cache">Cache containing the data item</param>		
		/// <param name="def">Default value</param>
		public static void SetViewAs(PXCache cache, string field, string source)
		{
			cache.SetAltered(field, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(field))
			{
				if (attr is PXDBCryptStringAttribute)
				{
					((PXDBCryptStringAttribute)attr).ViewAsString = source;
					break;
				}
			}
		}
		/// <summary>
		/// Overrides the view as state for the particular data item
		/// </summary>
		/// <param name="cache">Cache containing the data item</param>		
		/// <param name="def">Default value</param>
		public static void SetViewAs<Field>(PXCache cache, object data, string source)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXDBCryptStringAttribute)
				{
					((PXDBCryptStringAttribute)attr).ViewAsString = source;
				}
			}
		}
		/// <summary>
		/// Overrides the view as state for the particular data item
		/// </summary>
		/// <typeparam name="Field">Field to set default value for</typeparam>
		/// <param name="cache">Cache containing the data item</param>		
		/// <param name="def">Default value</param>
		public static void SetViewAs<Field>(PXCache cache, string source)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXDBCryptStringAttribute)
				{
					((PXDBCryptStringAttribute)attr).ViewAsString = source;
					break;
				}
			}
		}
		/// <summary>
		/// Overrides the view as state for the particular data item
		/// </summary>
		/// <param name="cache">Cache containing the data item</param>		
		/// <param name="def">Default value</param>
		public static void SetViewAs(PXCache cache, object data, string field, Type sourceField)
		{
			if (data == null)
			{
				cache.SetAltered(field, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, field))
			{
				if (attr is PXDBCryptStringAttribute)
				{
					((PXDBCryptStringAttribute)attr).ViewAsField = sourceField;
				}
			}
		}
		/// <summary>
		/// Overrides the view as state for the particular data item
		/// </summary>
		/// <param name="cache">Cache containing the data item</param>		
		/// <param name="def">Default value</param>
		public static void SetViewAs(PXCache cache, string field, Type sourceField)
		{
			cache.SetAltered(field, true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(field))
			{
				if (attr is PXDBCryptStringAttribute)
				{
					((PXDBCryptStringAttribute)attr).ViewAsField = sourceField;
					break;
				}
			}
		}
		/// <summary>
		/// Overrides the view as state for the particular data item
		/// </summary>
		/// <param name="cache">Cache containing the data item</param>		
		/// <param name="def">Default value</param>
		public static void SetViewAs<Field>(PXCache cache, object data, Type sourceField)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXDBCryptStringAttribute)
				{
					((PXDBCryptStringAttribute)attr).ViewAsField = sourceField;
				}
			}
		}
		/// <summary>
		/// Overrides the view as state for the particular data item
		/// </summary>
		/// <typeparam name="Field">Field to set default value for</typeparam>
		/// <param name="cache">Cache containing the data item</param>		
		/// <param name="def">Default value</param>
		public static void SetViewAs<Field>(PXCache cache, Type sourceField)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>())
			{
				if (attr is PXDBCryptStringAttribute)
				{
					((PXDBCryptStringAttribute)attr).ViewAsField = sourceField;
					break;
				}
			}
		}
		#endregion

		#region Implementation
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (!sender.BypassAuditFields.Contains(this.FieldName))
				sender.BypassAuditFields.Add(this.FieldName);
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!isViewDeprypted && e.Row != null &&
					(string)e.NewValue == ViewString(sender, e.Row))
				e.Cancel = true;
		}

		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			base.RowSelecting(sender, e);
			if (e.Row == null || sender.GetStatus(e.Row) != PXEntryStatus.Notchanged) return;

			string value = (string)sender.GetValue(e.Row, _FieldOrdinal);
			string result = string.Empty;
			if (!string.IsNullOrEmpty(value))
			{
				if (isEncryptionRequired)
				{
					try
					{
						result = Encoding.Unicode.GetString(Decrypt(Convert.FromBase64String(value)));
					}
					catch (Exception)
					{
						try
						{
							result = Encoding.Unicode.GetString(Convert.FromBase64String(value));
						}
						catch (Exception)
						{
							result = value;
						}
					}
				}
				else
				{
					result = value;
				}
			}
			sender.SetValue(e.Row, _FieldOrdinal, result.Replace("\0", string.Empty));
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (!this.isViewDeprypted && e.Row != null)
				e.ReturnValue = ViewString(sender, e.Row);

			base.FieldSelecting(sender, e);
		}
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update) && isEncryptionRequired)
			{
				string value = (string)sender.GetValue(e.Row, _FieldOrdinal);

				e.Value = !string.IsNullOrEmpty(value)
					?
						Convert.ToBase64String(Encrypt(Encoding.Unicode.GetBytes(value)))
					:
						null;
			}
			base.CommandPreparing(sender, e);
		}

		public virtual void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (e.Row == null) return;

			string value = (string)sender.GetValue(e.NewRow, _FieldOrdinal);
			if (!IsViewDecrypted && value == ViewString(sender, e.NewRow))
			{
				object oldValue = sender.GetValue(e.Row, _FieldOrdinal);
				sender.SetValue(e.NewRow, _FieldOrdinal, oldValue);
			}
		}

		private void Encrypt(PXCache sender, object row)
		{
			if (row == null) return;
			string value = (string)sender.GetValue(row, _FieldOrdinal);
			string result = string.Empty;

			if (!string.IsNullOrEmpty(value))
				result = Convert.ToBase64String(Encrypt(Encoding.Unicode.GetBytes(value)));

			sender.SetValue(row, _FieldOrdinal, result);
		}

		protected virtual string DefaultViewAsString
		{
			get
			{
				return new string('*', this.Length > 0 && this.Length < 8 ? this.Length : 8);
			}
		}

		protected virtual byte[] Encrypt(byte[] source)
		{
			return source;
		}

		protected virtual byte[] Decrypt(byte[] source)
		{
			return source;
		}

		private string ViewString(PXCache cache, object data)
		{
			if (ViewAsField != null)
				return cache.GetValue(data, viewAsField.Name).ToString();

			return ViewAsString != null ? ViewAsString : DefaultViewAsString;
		}

		#endregion
	}
	#endregion

	#region PXDB3DesCryphStringAttribute
	public class PXDB3DesCryphStringAttribute : PXDBCryptStringAttribute
	{
		#region Ctor
		public PXDB3DesCryphStringAttribute()
		{
		}
		public PXDB3DesCryphStringAttribute(int length)
			: base(length)
		{
		}
		#endregion

		#region Runtime
		public static string Encrypt(string source)
		{
			return Convert.ToBase64String(SitePolicy.TripleDESCryptoProvider.Encrypt(Encoding.Unicode.GetBytes(source)));
		}
		#endregion

		#region Implementation
		protected override byte[] Encrypt(byte[] source)
		{
			return SitePolicy.TripleDESCryptoProvider.Encrypt(source);
		}

		protected override byte[] Decrypt(byte[] source)
		{
			return SitePolicy.TripleDESCryptoProvider.Decrypt(source);
		}
		#endregion
	}
	#endregion

	#region PXRSACryptStringAttribute
	public class PXRSACryptStringAttribute : PXDBCryptStringAttribute
	{
		#region Ctor
		public PXRSACryptStringAttribute()
		{
		}
		public PXRSACryptStringAttribute(int length)
			: base(length)
		{
		}
		#endregion

		#region Runtime
#if !AZURE
		[System.Security.Permissions.KeyContainerPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
#endif
		public static string Encrypt(string source)
		{
			return SitePolicy.RSACryptoProvider.IsEncryptEnable
					?
						Convert.ToBase64String(SitePolicy.RSACryptoProvider.Encrypt(Encoding.Unicode.GetBytes(source)))
					:
						source;
		}
#if !AZURE
		[System.Security.Permissions.KeyContainerPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Security.Permissions.StorePermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Net.WebPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Net.DnsPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
#endif
		internal static string Decrypt(string source)
		{
			byte[] bSource = null;
			try
			{
				bSource = Convert.FromBase64String(source);
			}
			catch (Exception)
			{
				return source;
			}
			return SitePolicy.RSACryptoProvider.IsDecryptEnable
							?
								Encoding.Unicode.GetString(SitePolicy.RSACryptoProvider.Decrypt(bSource))
							:
								Encoding.Unicode.GetString(bSource);
		}

		public virtual bool EncryptOnCertificateReplacement(PXCache cache, object row)
		{
			return true;
		}
		#endregion

		#region Implementation
		protected internal override void SetBqlTable(Type bqlTable)
		{
			base.SetBqlTable(bqlTable);
		}

#if !AZURE
		[System.Security.Permissions.KeyContainerPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
#endif
		protected override byte[] Encrypt(byte[] source)
		{
			return SitePolicy.RSACryptoProvider.IsEncryptEnable ? SitePolicy.RSACryptoProvider.Encrypt(source) : source;
		}

#if !AZURE
		[System.Security.Permissions.KeyContainerPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Security.Permissions.StorePermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
		[System.Net.WebPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
#endif
		protected override byte[] Decrypt(byte[] source)
		{
			return SitePolicy.RSACryptoProvider.IsDecryptEnable ? SitePolicy.RSACryptoProvider.Decrypt(source) : source;
		}
		#endregion
	}
	#endregion

	#region PXDBLiteDefaultAttribute
	/// <summary>
	/// Provides default value for the property or parameter.
	/// Use for defaulting from the auto generated key field
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class PXDBLiteDefaultAttribute : PXDBDefaultAttribute
	{
		#region Ctor
		/// <summary>
		/// Defines default from the item of the type sourceType
		/// </summary>
		/// <param name="sourceType">Type to get the default value from, if implements IBqlField and is nested, then defines the source field as well</param>
		public PXDBLiteDefaultAttribute(Type sourceType)
			: base(sourceType)
		{
		}
		#endregion
	}
	#endregion

	#region PXDBChildDefaultAttribute
	/// <summary>
	/// Provides default value for the property or parameter.
	/// Use for defaulting from the auto generated key field
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBChildIdentityAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber
	{
		#region State
		protected Type _SourceType;
		protected string _SourceField;
		protected Dictionary<object, object> _Persisted;
		protected Type _SelfType;
		#endregion

		#region Ctor
		/// <summary>
		/// Defines default from the item of the type sourceType
		/// </summary>
		/// <param name="sourceType">Type to get the default value from, if implements IBqlField and is nested, then defines the source field as well</param>
		public PXDBChildIdentityAttribute(Type sourceType)
		{
			if (sourceType == null)
			{
				throw new ArgumentNullException("sourceType");
			}
			if (sourceType.IsNested && typeof(IBqlField).IsAssignableFrom(sourceType))
			{
				_SourceType = BqlCommand.GetItemType(sourceType);
				_SourceField = sourceType.Name;

			}
			else
			{
				throw new ArgumentOutOfRangeException("sourceType", String.Format("Cannot create foreign key reference from the type '{0}'.", sourceType));
			}
		}
		#endregion

		#region Implementation
		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete)
			{
				object key = sender.GetValue(e.Row, _FieldOrdinal);
				if (key != null && Convert.ToInt32(key) < 0)
				{
					_Persisted[e.Row] = key;
					PXCache cache = sender.Graph.Caches[_SourceType];
					{
						object child;
						if (_Persisted.TryGetValue(key, out child) && _SourceType.IsAssignableFrom(child.GetType()))
						{
							sender.SetValue(e.Row, _FieldOrdinal, cache.GetValue(child, _SourceField));
						}
					}
					foreach (object child in cache.Inserted)
					{
						if (object.Equals(key, cache.GetValue(child, _SourceField)))
						{
							object parents;
							if (!_Persisted.TryGetValue(child, out parents))
							{
								_Persisted[child] = new List<object>();
							}
							((List<object>)_Persisted[child]).Add(e.Row);
						}
					}
				}
			}
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete && e.TranStatus == PXTranStatus.Aborted)
			{
				object key;
				if (_Persisted.TryGetValue(e.Row, out key))
				{
					sender.SetValue(e.Row, _FieldOrdinal, key);
				}
			}
		}

		public void SourceRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				object key = sender.GetValue(e.Row, _SourceField);
				if (key != null && Convert.ToInt32(key) < 0)
				{
					_Persisted[key] = e.Row;
				}
			}
		}

		public void SourceRowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert) && e.TranStatus == PXTranStatus.Open)
			{
				object parents;
				if (_Persisted.TryGetValue(e.Row, out parents) && parents != null && parents is List<object>)
				{
					foreach (object parent in (List<object>)parents)
					{
						if (!_SelfType.IsAssignableFrom(parent.GetType()))
						{
							continue;
						}

						object id = sender.GetValue(e.Row, _SourceField);
						if (id != null && Convert.ToInt32(id) > 0)
						{
							if (sender.Graph.TimeStamp == null)
							{
								sender.Graph.SelectTimeStamp();
							}
							PXCache cache = sender.Graph.Caches[_SelfType];
							List<PXDataFieldParam> pars = new List<PXDataFieldParam>();
							{
								PXCommandPreparingEventArgs.FieldDescription description = null;
								cache.RaiseCommandPreparing(_FieldName, parent, id, PXDBOperation.Update, _BqlTable, out description);
								if (description != null && !String.IsNullOrEmpty(description.FieldName))
								{
									PXDataFieldAssign assign = new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue, cache.ValueToString(_FieldName, id));
									pars.Add(assign);
								}
							}
							foreach (string field in cache.Fields)
							{
								PXCommandPreparingEventArgs.FieldDescription description;
								cache.RaiseCommandPreparing(field, parent, cache.GetValue(parent, field), PXDBOperation.Update, _BqlTable, out description);
								if (description != null && !String.IsNullOrEmpty(description.FieldName) && description.IsRestriction && description.DataValue != null && description.DataType != PXDbType.Timestamp)
								{
									pars.Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
								}
							}
							pars.Add(PXDataFieldRestrict.OperationSwitchAllowed);
							PXDatabase.Update(_BqlTable, pars.ToArray());
							cache.SetValue(parent, _FieldOrdinal, id);
						}
					}
				}
			}
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			_Persisted = new Dictionary<object, object>();
			_SelfType = sender.GetItemType();
			sender.Graph.RowPersisting.AddHandler(_SourceType, SourceRowPersisting);
			sender.Graph.RowPersisted.AddHandler(_SourceType, SourceRowPersisted);
		}
		#endregion
	}
	#endregion

	#region PXEnumDescriptionAttribute
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class PXEnumDescriptionAttribute : Attribute
	{
		private string _displayName;
		private string _displayNameKey;
		private Type _enumType;
		private string _field;
		private string _category;

		private static Hashtable _enumsInfo = new Hashtable();
		private static object _syncObj = new object();

		public PXEnumDescriptionAttribute(string displayName, Type keyType)
			: base()
		{
			_displayName = displayName;
			_displayNameKey = keyType.ToString();
		}

		public string Category
		{
			get
			{
				return _category;
			}
			set
			{
				_category = value;
			}
		}

		public Type EnumType
		{
			get
			{
				return _enumType;
			}
			set
			{
				if (_enumType == null || !typeof(Enum).IsAssignableFrom(_enumType))
					throw new PXException(ErrorMessages.BadEnumType);
				_enumType = value;
			}
		}

		public string Field
		{
			get
			{
				return _field;
			}
			set
			{
				if (string.IsNullOrEmpty(_field))
					throw new PXException(ErrorMessages.BadEnumField);
				_field = value;
			}
		}

		public string DisplayName
		{
			get
			{
				string result = null;
				if (_enumType != null && !string.IsNullOrEmpty(_field) &&
					!System.Globalization.CultureInfo.InvariantCulture.Equals(System.Threading.Thread.CurrentThread.CurrentCulture))
				{
					result = PXLocalizer.Localize(_displayName, _displayNameKey);
				}
				return string.IsNullOrEmpty(result) ? _displayName : result;
			}
		}

		public static string[] GetNames(Type @enum)
		{
			return new List<string>(GetValueNamePairs(@enum).Values).ToArray();
		}

		public static IDictionary<object, string> GetValueNamePairs(Type @enum, bool localize = true)
		{
			return GetValueNamePairs(@enum, null, localize);
		}

		public static IDictionary<object, string> GetValueNamePairs(Type @enum, string categoryName, bool localize = true)
		{
			var key = string.Concat(@enum.FullName, "__", categoryName);
			lock (_syncObj)
			{
				if (_enumsInfo.ContainsKey(key)) return _enumsInfo[key] as IDictionary<object, string>;

				var returnAll = string.IsNullOrEmpty(categoryName);
				var list = new Dictionary<object, string>();
				foreach (var info in GetFullInfoUnSafelly(@enum, localize))
				{
					var categories = info.Value.Key.Split(',');
					if (returnAll || Array.Find(categories, s => string.Compare(s, categoryName, true) == 0) != null)
						list.Add(info.Key, info.Value.Value);
				}
				_enumsInfo.Add(key, list);
				return list;
			}
		}

		public static IDictionary<object, KeyValuePair<string, string>> GetFullInfo(Type @enum, bool localize = false)
		{
			lock (_syncObj)
			{
				return GetFullInfoUnSafelly(@enum, localize);
			}
		}

		private static IDictionary<object, KeyValuePair<string, string>> GetFullInfoUnSafelly(Type @enum, bool localize = true)
		{
			var key = @enum.FullName + "_";
			if (_enumsInfo.ContainsKey(key)) return _enumsInfo[key] as IDictionary<object, KeyValuePair<string, string>>;

			var list = new Dictionary<object, KeyValuePair<string, string>>();
			foreach (var field in
				@enum.GetFields(BindingFlags.Static | BindingFlags.GetField |
								BindingFlags.Public))
			{
				var value = Enum.Parse(@enum, field.Name);
				var info = new KeyValuePair<string, string>(null, field.Name);
				var att = GetCustomAttribute(field, typeof(PXEnumDescriptionAttribute)) as PXEnumDescriptionAttribute;
				if (att != null)
				{
					att._enumType = @enum;
					att._field = field.Name;
					info = new KeyValuePair<string, string>(att._category, localize ? att.DisplayName : att._displayName);
				}
				list.Add(value, info);
			}
			_enumsInfo.Add(key, list);
			return list;
		}

		public static KeyValuePair<string, string> GetInfo(Type @enum, object value)
		{
			var name = Enum.GetName(@enum, value);
			if (!string.IsNullOrEmpty(name))
			{
				var att = GetCustomAttribute(@enum.GetField(name), typeof(PXEnumDescriptionAttribute)) as PXEnumDescriptionAttribute;
				if (att != null) return new KeyValuePair<string, string>(att.Category, att.DisplayName);
			}
			return new KeyValuePair<string, string>();
		}
	}
	#endregion

	#region PXDBDataLengthAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBDataLengthAttribute : PXEventSubscriberAttribute, IPXCommandPreparingSubscriber, IPXRowSelectingSubscriber
	{
		#region State
		private string _TargetFieldName;
		#endregion

		#region Ctor
		public PXDBDataLengthAttribute(Type targetField)
		{
			this._TargetFieldName = targetField.Name;
		}

		public PXDBDataLengthAttribute(string targetFieldName)
		{
			this._TargetFieldName = targetFieldName;
		}
		#endregion

		#region Implementation
		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			string tableName = e.Table == null ? this._BqlTable.Name : e.Table.Name;
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select)
			{
				e.BqlTable = _BqlTable;
				e.FieldName = PXDatabase.Provider.SqlDialect.BinaryLength + "(" + tableName + "." + this._TargetFieldName + ")";
			}
		}

		public void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetValue(e.Position));
			}
			e.Position++;
		}
		#endregion
	}
	#endregion

	#region DashboardTypeAttribute
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class DashboardTypeAttribute : Attribute
	{
		public enum Type
		{
			Default = 0,
			WikiArticle = 1,
			Task = 2,
			Announcements = 3,
			Chart = 4,
		}

		public readonly int[] Types;

		public DashboardTypeAttribute(params int[] type)
		{
			Types = type;
		}
	}
	#endregion

	#region DashboardVisibleAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class DashboardVisibleAttribute : PXEventSubscriberAttribute
	{
		private readonly bool _visible;

		public DashboardVisibleAttribute(bool visible)
		{
			_visible = visible;
		}

		public DashboardVisibleAttribute() : this(true) { }

		public bool Visible
		{
			get { return _visible; }
		}
	}

	#endregion

	#region PXEMailAccountIDSelectorAttribute
	public class PXEMailAccountIDSelectorAttribute : PXCustomSelectorAttribute
	{
		private static Boolean needOwner;
		private static Boolean onlyremoveempty;

		public PXEMailAccountIDSelectorAttribute()
			: base(typeof(EMailAccount.emailAccountID))
		{
			needOwner = false;
			onlyremoveempty = false;
			base.DescriptionField = typeof(EMailAccount.address);
		}

		public PXEMailAccountIDSelectorAttribute(Boolean _needOwner)
			: base(typeof(EMailAccount.emailAccountID))
		{
			needOwner = _needOwner;
			base.DescriptionField = typeof(EMailAccount.address);
		}

		public PXEMailAccountIDSelectorAttribute(Boolean _needOwner, Boolean _onlyremoveempty)
			: base(typeof(EMailAccount.emailAccountID))
		{
			needOwner = _needOwner;
			onlyremoveempty = _onlyremoveempty;
			base.DescriptionField = typeof(EMailAccount.address);
		}

		public override Type DescriptionField
		{
			get
			{
				return base.DescriptionField;
			}
			set
			{
			}
		}

		internal IEnumerable GetRecords()
		{
			return PXEMailAccountIDSelectorAttribute.GetRecords(this._Graph);
		}

		public static IEnumerable GetRecords(PXGraph graph)
		{
			if (onlyremoveempty)
			{
				foreach (EMailAccount account in PXSelectJoinOrderBy<EMailAccount,
						LeftJoin<PreferencesEmail, On<PreferencesEmail.defaultEMailAccountID,
							Equal<EMailAccount.emailAccountID>>>,
							OrderBy<
								Desc<PreferencesEmail.defaultEMailAccountID,
								Asc<EMailAccount.address>>>>
						.Select(graph))
				{
					if (account.Address != null)
					{
						if (!String.IsNullOrEmpty(account.Address.Trim()))
						{
							yield return account;
						}
					}
				}
			}
			else
			{
				if (needOwner)
				{
					foreach (EMailAccount account in PXSelectJoin<EMailAccount,
						LeftJoin<PreferencesEmail, On<PreferencesEmail.defaultEMailAccountID, Equal<EMailAccount.emailAccountID>>>,
							Where<EMailAccount.userName, IsNull,
								Or<EMailAccount.userName, Equal<Current<AccessInfo.userName>>,
								And<Match<Optional<Users.username>>>>>,
							OrderBy<
								Desc<PreferencesEmail.defaultEMailAccountID,
								Asc<EMailAccount.address>>>>
						.Select(graph, PXAccess.GetUserName()))
					{
						if (account.Address != null)
						{
							if (!String.IsNullOrEmpty(account.Address.Trim()))
							{
								yield return account;
							}
						}
					}
				}
				else
				{
					foreach (EMailAccount account in PXSelectJoin<EMailAccount,
						LeftJoin<PreferencesEmail, On<PreferencesEmail.defaultEMailAccountID, Equal<EMailAccount.emailAccountID>>>,
							Where<EMailAccount.userName, IsNull,
								And<Match<Optional<Users.username>>>>,
							OrderBy<
								Desc<PreferencesEmail.defaultEMailAccountID,
								Asc<EMailAccount.address>>>>
						.Select(graph, PXAccess.GetUserName()))
					{
						if (account.Address != null)
						{
							if (!String.IsNullOrEmpty(account.Address.Trim()))
							{
								yield return account;
							}
						}
					}
				}
			}
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (PXSelect<EMailAccount, Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>
				.SelectWindowed(sender.Graph, 0, 1, e.NewValue) != null)
				e.Cancel = true;
			else
				base.FieldVerifying(sender, e);
		}
	}
	#endregion
	 
	#region PXCustomizationAttribute
	[AttributeUsage(AttributeTargets.Class)]
	public class PXCustomizationAttribute : Attribute { }
	#endregion

	#region PXDBDateAndTimeAttribute

	public class PXDBDateAndTimeAttribute : PXDBDateAttribute
	{
		public const string DATE_FIELD_POSTFIX = "_Date";
		public const string TIME_FIELD_POSTFIX = "_Time";

		public string DateDisplayNamePostfix
		{
			get
			{
				return string.Format(" ({0})", PXLocalizer.Localize(Messages.Date, typeof(Messages).FullName));
			}
		}

		public string TimeDisplayNamePostfix
		{
			get
			{
				return string.Format(" ({0})", PXLocalizer.Localize(Messages.Time, typeof(Messages).FullName));
			}
		}

		protected string _TimeInputMask = "t";
		protected string _TimeDisplayMask = "t";

		protected string _DateInputMask = "d";
		protected string _DateDisplayMask = "d";

		private string _displayNameDate = null;
		private string _displayNameTime = null;

		private bool? _isEnabledDate = null;
		private bool? _isEnabledTime = null;

		private bool? _isVisibleDate = null;
		private bool? _isVisibleTime = null;

		#region Ctor
		public PXDBDateAndTimeAttribute()
		{
			this.PreserveTime = true;
		}
		#endregion

		#region Initialization

		public virtual bool WithoutDisplayNames { get; set; }

		public string DisplayNameDate
		{
			get { return _displayNameDate; }
			set { _displayNameDate = value; }
		}

		public string DisplayNameTime
		{
			get { return _displayNameTime; }
			set { _displayNameTime = value; }
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Fields.Add(_FieldName + DATE_FIELD_POSTFIX);
			sender.Fields.Add(_FieldName + TIME_FIELD_POSTFIX);

			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _FieldName + DATE_FIELD_POSTFIX, Date_FieldSelecting);
			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName + DATE_FIELD_POSTFIX, Date_FieldUpdating);

			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _FieldName + TIME_FIELD_POSTFIX, Time_FieldSelecting);
			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName + TIME_FIELD_POSTFIX, Time_FieldUpdating);
		}
		#endregion

		#region Implementation
		public void Date_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			object val = e.ReturnValue = sender.GetValue(e.Row, _FieldOrdinal);

			if (sender.HasAttributes(e.Row) || e.Row == null || e.IsAltered)
			{
				sender.RaiseFieldSelecting(_FieldName, e.Row, ref val, true);
				PXFieldState state = PXDateState.CreateInstance(val, _FieldName + DATE_FIELD_POSTFIX, _IsKey, null, _DateInputMask, _DateDisplayMask, null, null);
				if (!WithoutDisplayNames) state.DisplayName += DateDisplayNamePostfix;
				PXDBDateAndTimeAttribute attr = GetAttribute(sender, e.Row, _FieldName);
				if (attr != null)
				{
					if (attr._displayNameDate != null)
						state.DisplayName = attr._displayNameDate;
					if (attr._isEnabledDate.HasValue)
						state.Enabled = attr._isEnabledDate.Value;
					if (attr._isVisibleDate.HasValue)
						state.Visible = attr._isVisibleDate.Value;
				}
				e.ReturnState = state;

			}

			if (e.ReturnValue != null)
			{
				e.ReturnValue = new DateTime(((DateTime)e.ReturnValue).Year, ((DateTime)e.ReturnValue).Month, ((DateTime)e.ReturnValue).Day);
			}
		}

		public virtual void Time_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			object val = e.ReturnValue = sender.GetValue(e.Row, _FieldOrdinal);

			if (sender.HasAttributes(e.Row) || e.Row == null || e.IsAltered)
			{
				sender.RaiseFieldSelecting(_FieldName, e.Row, ref val, true);
				PXFieldState state = PXDateState.CreateInstance(val, _FieldName + TIME_FIELD_POSTFIX, _IsKey, null, _TimeInputMask, _TimeDisplayMask, null, null);
				if (!WithoutDisplayNames) state.DisplayName += TimeDisplayNamePostfix;
				PXDBDateAndTimeAttribute attr = GetAttribute(sender, e.Row, _FieldName);
				if (attr != null)
				{
					if (attr._displayNameTime != null)
						state.DisplayName = attr._displayNameTime;
					if (attr._isEnabledTime.HasValue)
						state.Enabled = attr._isEnabledTime.Value;
					if (attr._isVisibleTime.HasValue)
						state.Visible = attr._isVisibleTime.Value;
				}
				e.ReturnState = state;
			}

			if (e.ReturnValue != null)
			{
				e.ReturnValue = new DateTime(1900, 1, 1, ((DateTime)e.ReturnValue).Hour, ((DateTime)e.ReturnValue).Minute, 0);
			}
		}

		public virtual void Date_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Cancel) return;

			if (e.NewValue is string)
			{
				DateTime val;
				DateTime? oldval = (DateTime?)sender.GetValue(e.Row, _FieldOrdinal);

				if (DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, DateTimeStyles.None, out val))
				{
					object fieldval = CombineDateTime(val, oldval);
					sender.SetValue(e.Row, _FieldOrdinal, fieldval);
					sender.RaiseFieldUpdated(_FieldName + DATE_FIELD_POSTFIX, e.Row, oldval);
					if (sender.GetValuePending(e.Row, _FieldName + TIME_FIELD_POSTFIX) != null)
					{
						sender.RaiseFieldUpdated(_FieldName, e.Row, oldval);
					}
				}
				else
				{
					e.NewValue = null;
				}
			}
			else if (e.NewValue is DateTime)
			{
				DateTime? oldval = (DateTime?)sender.GetValue(e.Row, _FieldOrdinal);

				object fieldval = CombineDateTime((DateTime)e.NewValue, oldval);
				if (sender.RaiseFieldVerifying(FieldName + DATE_FIELD_POSTFIX, e.Row, ref fieldval))
				{
					sender.SetValue(e.Row, _FieldOrdinal, fieldval);
					sender.RaiseFieldUpdated(_FieldName + DATE_FIELD_POSTFIX, e.Row, oldval);
					if (sender.GetValuePending(e.Row, _FieldName + TIME_FIELD_POSTFIX) != null)
					{
						sender.RaiseFieldUpdated(_FieldName, e.Row, oldval);
					}
				}
			}
		}

		public void Time_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Cancel) return;

			if (e.NewValue is string)
			{
				DateTime val;
				DateTime? fieldval = (DateTime?)sender.GetValue(e.Row, _FieldOrdinal);

				if (fieldval != null && DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, DateTimeStyles.None, out val))
				{
					fieldval = CombineDateTime(fieldval, val);
					sender.SetValueExt(e.Row, _FieldName, fieldval);
				}
				else
				{
					e.NewValue = null;
				}
			}
			else if (e.NewValue is DateTime)
			{
				DateTime? fieldval = (DateTime?)sender.GetValue(e.Row, _FieldOrdinal);
				fieldval = CombineDateTime(fieldval, (DateTime)e.NewValue);
				sender.SetValueExt(e.Row, _FieldName, fieldval);
			}
		}

        public static DateTime? CombineDateTime(DateTime? date, DateTime? time)
		{
			if (date != null)
			{
				if (time != null)
				{
					return new DateTime(((DateTime)date).Year, ((DateTime)date).Month, ((DateTime)date).Day, ((DateTime)time).Hour, ((DateTime)time).Minute, 0);
				}
				else
				{
					return new DateTime(((DateTime)date).Year, ((DateTime)date).Month, ((DateTime)date).Day);
				}
			}
			return null;
		}
		#endregion

		#region SetEnabled
		public static void SetDateEnabled<Field>(PXCache cache, object data, bool isEnabled)
			where Field : IBqlField
		{
			SetDateEnabled(cache, data, cache.GetField(typeof(Field)), isEnabled);
		}

		public static void SetDateEnabled(PXCache cache, object data, string name, bool isEnabled)
		{
			PXDBDateAndTimeAttribute attr = GetAttribute(cache, data, name);
			if (attr != null) attr._isEnabledDate = isEnabled;
		}

		public static void SetTimeEnabled<Field>(PXCache cache, object data, bool isEnabled)
			where Field : IBqlField
		{
			SetTimeEnabled(cache, data, cache.GetField(typeof(Field)), isEnabled);
		}

		public static void SetTimeEnabled(PXCache cache, object data, string name, bool isEnabled)
		{
			PXDBDateAndTimeAttribute attr = GetAttribute(cache, data, name);
			if (attr != null) attr._isEnabledTime = isEnabled;
		}
		#endregion

		#region SetDisplayName
		public static void SetDateDisplayName<Field>(PXCache cache, object data, string displayName)
			where Field : IBqlField
		{
			SetDateDisplayName(cache, data, cache.GetField(typeof(Field)), displayName);
		}

		public static void SetDateDisplayName(PXCache cache, object data, string name, string displayName)
		{
			PXDBDateAndTimeAttribute attr = GetAttribute(cache, data, name);
			if (attr != null) attr._displayNameDate = displayName;
		}

		public static void SetTimeDisplayName<Field>(PXCache cache, object data, string displayName)
			where Field : IBqlField
		{
			SetTimeDisplayName(cache, data, cache.GetField(typeof(Field)), displayName);
		}

		public static void SetTimeDisplayName(PXCache cache, object data, string name, string displayName)
		{
			PXDBDateAndTimeAttribute attr = GetAttribute(cache, data, name);
			if (attr != null) attr._displayNameTime = displayName;
		}
		#endregion

		#region SetVisible
		public static void SetDateVisible<Field>(PXCache cache, object data, bool isVisible)
			where Field : IBqlField
		{
			SetDateVisible(cache, data, cache.GetField(typeof(Field)), isVisible);
		}

		public static void SetDateVisible(PXCache cache, object data, string name, bool isVisible)
		{
			PXDBDateAndTimeAttribute attr = GetAttribute(cache, data, name);
			if (attr != null) attr._isVisibleDate = isVisible;
		}

		public static void SetTimeVisible<Field>(PXCache cache, object data, bool isVisible)
			where Field : IBqlField
		{
			SetTimeVisible(cache, data, cache.GetField(typeof(Field)), isVisible);
		}

		public static void SetTimeVisible(PXCache cache, object data, string name, bool isVisible)
		{
			PXDBDateAndTimeAttribute attr = GetAttribute(cache, data, name);
			if (attr != null) attr._isVisibleTime = isVisible;
		}
		#endregion

		#region Private Methods
		public static PXDBDateAndTimeAttribute GetAttribute(PXCache cache, object data, string name)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
				if (attr is PXDBDateAndTimeAttribute) return attr as PXDBDateAndTimeAttribute;
			return null;
		}
		#endregion
	}
	#endregion

	#region PXDateAndTimeAttribute

	public class PXDateAndTimeAttribute : PXDateAttribute
	{
		public const string DATE_FIELD_POSTFIX = "_Date";
		public const string TIME_FIELD_POSTFIX = "_Time";

		protected string _TimeInputMask = "t";
		protected string _TimeDisplayMask = "t";

		protected string _DateInputMask = "d";
		protected string _DateDisplayMask = "d";

		private bool? _isEnabledDate = null;
		private bool? _isEnabledTime = null;

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Fields.Add(_FieldName + DATE_FIELD_POSTFIX);
			sender.Fields.Add(_FieldName + TIME_FIELD_POSTFIX);

			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _FieldName + DATE_FIELD_POSTFIX, Date_FieldSelecting);
			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName + DATE_FIELD_POSTFIX, Date_FieldUpdating);

			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _FieldName + TIME_FIELD_POSTFIX, Time_FieldSelecting);
			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName + TIME_FIELD_POSTFIX, Time_FieldUpdating);
		}
		#endregion
		#region Implementation
		public void Date_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			object val = e.ReturnValue = sender.GetValue(e.Row, _FieldOrdinal);

			if (sender.HasAttributes(e.Row) || e.Row == null || e.IsAltered)
			{
				sender.RaiseFieldSelecting(_FieldName, e.Row, ref val, true);
				PXFieldState state = PXDateState.CreateInstance(val, _FieldName + DATE_FIELD_POSTFIX, _IsKey, null, _DateInputMask, _DateDisplayMask, null, null);
				PXDateAndTimeAttribute attr = GetAttribute(sender, e.Row, _FieldName);
				if (attr != null && attr._isEnabledDate.HasValue) state.Enabled = attr._isEnabledDate.Value;
				e.ReturnState = state;

			}

			if (e.ReturnValue != null)
			{
				e.ReturnValue = new DateTime(((DateTime)e.ReturnValue).Year, ((DateTime)e.ReturnValue).Month, ((DateTime)e.ReturnValue).Day);
			}
		}

		public void Time_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			object val = e.ReturnValue = sender.GetValue(e.Row, _FieldOrdinal);

			if (sender.HasAttributes(e.Row) || e.Row == null || e.IsAltered)
			{
				sender.RaiseFieldSelecting(_FieldName, e.Row, ref val, true);
				PXFieldState state = PXDateState.CreateInstance(val, _FieldName + TIME_FIELD_POSTFIX, _IsKey, null, _TimeInputMask, _TimeDisplayMask, null, null);
				PXDateAndTimeAttribute attr = GetAttribute(sender, e.Row, _FieldName);
				if (attr != null && attr._isEnabledTime.HasValue) state.Enabled = attr._isEnabledTime.Value;
				e.ReturnState = state;
			}

			if (e.ReturnValue != null)
			{
				e.ReturnValue = new DateTime(1900, 1, 1, ((DateTime)e.ReturnValue).Hour, ((DateTime)e.ReturnValue).Minute, 0);
			}
		}

		public virtual void Date_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				DateTime val;
				DateTime? oldval = (DateTime?)sender.GetValue(e.Row, _FieldOrdinal);

				if (DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, DateTimeStyles.None, out val))
				{
					object fieldval = CombineDateTime(val, oldval);
					sender.SetValue(e.Row, _FieldOrdinal, fieldval);
					if (sender.GetValuePending(e.Row, _FieldName + TIME_FIELD_POSTFIX) != null)
					{
						sender.RaiseFieldUpdated(_FieldName, e.Row, oldval);
					}
				}
				else
				{
					e.NewValue = null;
				}
			}
			else if (e.NewValue is DateTime)
			{
				DateTime? oldval = (DateTime?)sender.GetValue(e.Row, _FieldOrdinal);

				object fieldval = CombineDateTime((DateTime)e.NewValue, oldval);
				sender.SetValue(e.Row, _FieldOrdinal, fieldval);
				if (sender.GetValuePending(e.Row, _FieldName + TIME_FIELD_POSTFIX) != null)
				{
					sender.RaiseFieldUpdated(_FieldName, e.Row, oldval);
				}
			}
		}

		public void Time_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				DateTime val;
				DateTime? fieldval = (DateTime?)sender.GetValue(e.Row, _FieldOrdinal);

				if (fieldval != null && DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, DateTimeStyles.None, out val))
				{
					fieldval = CombineDateTime(fieldval, val);
					sender.SetValueExt(e.Row, _FieldName, fieldval);
				}
				else
				{
					e.NewValue = null;
				}
			}
			else if (e.NewValue is DateTime)
			{
				DateTime? fieldval = (DateTime?)sender.GetValue(e.Row, _FieldOrdinal);
				fieldval = CombineDateTime(fieldval, (DateTime)e.NewValue);
				sender.SetValueExt(e.Row, _FieldName, fieldval);
			}
		}

		protected DateTime? CombineDateTime(DateTime? date, DateTime? time)
		{
			if (date != null)
			{
				if (time != null)
				{
					return new DateTime(((DateTime)date).Year, ((DateTime)date).Month, ((DateTime)date).Day, ((DateTime)time).Hour, ((DateTime)time).Minute, 0);
				}
				else
				{
					return new DateTime(((DateTime)date).Year, ((DateTime)date).Month, ((DateTime)date).Day);
				}
			}
			return null;
		}
		#endregion
		#region SetEnabled
		public static void SetDateEnabled<Field>(PXCache cache, object data, bool isEnabled)
			where Field : IBqlField
		{
			SetDateEnabled(cache, data, cache.GetField(typeof(Field)), isEnabled);
		}

		public static void SetDateEnabled(PXCache cache, object data, string name, bool isEnabled)
		{
			PXDateAndTimeAttribute attr = GetAttribute(cache, data, name);
			if (attr != null) attr._isEnabledDate = isEnabled;
		}

		public static void SetTimeEnabled<Field>(PXCache cache, object data, bool isEnabled)
			where Field : IBqlField
		{
			SetTimeEnabled(cache, data, cache.GetField(typeof(Field)), isEnabled);
		}

		public static void SetTimeEnabled(PXCache cache, object data, string name, bool isEnabled)
		{
			PXDateAndTimeAttribute attr = GetAttribute(cache, data, name);
			if (attr != null) attr._isEnabledTime = isEnabled;
		}
		#endregion

		#region Private Methods
		public static PXDateAndTimeAttribute GetAttribute(PXCache cache, object data, string name)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				var dtAttr = attr as PXDateAndTimeAttribute;
				if (dtAttr != null) return dtAttr;
			}
			return null;
		}
		#endregion

		public class now : Constant<DateTime>
		{
			public now() : base(DateTime.MinValue) { }

			public override object Value
			{
				get
				{
					return PXTimeZoneInfo.Now;
				}
			}
		}

	}
	#endregion

	#region PXVerifyEndDateAttribute

	public class PXVerifyEndDateAttribute : PXEventSubscriberAttribute, IPXFieldVerifyingSubscriber, IPXRowInsertedSubscriber, IPXRowPersistingSubscriber
	{
		private readonly string _startDateField;

		public bool AllowAutoChange { get; set; }
		public bool AutoChangeWarning { get; set; }

		#region Ctor

		public PXVerifyEndDateAttribute(Type startDateField)
		{
			_startDateField = startDateField.Name;
			AllowAutoChange = true;
			AutoChangeWarning = false;
		}

		#endregion

		#region Implementation

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler(sender.GetItemType(), _startDateField, StartDateVerifyning);
		}

		void IPXFieldVerifyingSubscriber.FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			DateTime? startDate = (DateTime?)sender.GetValue(e.Row, _startDateField);
			DateTime? endDate = (DateTime?)e.NewValue;

			Verifying(sender, e.Row, startDate, endDate, _startDateField, endDate);
		}

		private void StartDateVerifyning(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			DateTime? startDate = (DateTime?)e.NewValue;
			DateTime? endDate = (DateTime?)sender.GetValue(e.Row, _FieldName);

			Verifying(sender, e.Row, startDate, endDate, _FieldName, startDate);
		}

		void IPXRowInsertedSubscriber.RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			DateTime? startDate = (DateTime?)sender.GetValue(e.Row, _startDateField);
			DateTime? endDate = (DateTime?)sender.GetValue(e.Row, _FieldName);

			try
			{
				Verifying(sender, e.Row, startDate, endDate, _startDateField, endDate);
			}
			catch (PXSetPropertyException ex)
			{
				sender.RaiseExceptionHandling(_FieldName, e.Row, endDate, ex);
			}
		}

		void IPXRowPersistingSubscriber.RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			DateTime? startDate = (DateTime?)sender.GetValue(e.Row, _startDateField);
			DateTime? endDate = (DateTime?)sender.GetValue(e.Row, _FieldName);

			try
			{
				Verifying(sender, e.Row, startDate, endDate, _FieldName, startDate);
			}
			catch (PXSetPropertyException ex)
			{
				sender.RaiseExceptionHandling(_FieldName, e.Row, endDate, ex);
			}
		}

		private void Verifying(PXCache sender, object row, DateTime? startDate, DateTime? endDate, string fieldName, DateTime? newValue)
		{
			if (startDate != null && endDate != null && startDate > endDate)
			{
				if (AllowAutoChange)
				{
					sender.SetValueExt(row, fieldName, newValue);
					if (AutoChangeWarning)
						sender.RaiseExceptionHandling(fieldName, row, endDate, new PXSetPropertyException(InfoMessages.ChangedAutomatically, PXErrorLevel.Warning, fieldName, newValue));
				}
				else if (fieldName == _FieldName) // start date changed
				{
					throw new PXSetPropertyException(ErrorMessages.StartDateGreaterThanEndDate, _startDateField, endDate);
				}
				else
				{
					throw new PXSetPropertyException(ErrorMessages.EndDateLessThanStartDate, _FieldName, startDate);
				}
			}
		}

		#endregion

	}
	#endregion

	#region TimeSpanFormatType

	public enum TimeSpanFormatType
	{
		DaysHoursMinites = 0,
		DaysHoursMinitesCompact,
		LongHoursMinutes,
		ShortHoursMinutes,
		ShortHoursMinutesCompact,
	}

	#endregion

	#region PXTimeSpanLongAttribute
	public class PXTimeSpanLongAttribute : PXIntAttribute
	{
		#region State
		protected string[] _inputMasks = new string[] { ActionsMessages.DurationInputMask, "### d 00:00", ActionsMessages.TimeSpanLongHM, ActionsMessages.TimeSpanHM, "00:00" };
		protected string[] _outputFormats = new string[] { "{0,3}{1:00}{2:00}", "{0,3}{1:00}{2:00}", "{1,4}{2:00}", "{1,2}{2:00}", "{1:00}{2:00}" };
		protected int[] _lengths = new int[] { 7, 7, 6, 4, 4 };
		protected bool _NullIsZero = false;

		protected TimeSpanFormatType _Format = TimeSpanFormatType.DaysHoursMinites;
		public TimeSpanFormatType Format
		{
			get
			{
				return _Format;
			}
			set
			{
				_Format = value;
			}
		}

		public string InputMask
		{
			get { return inputMask; }
			set
			{
				inputMask = value;
				_maskLenght = 0;

				foreach (char c in value)
				{
					if (c == '#' || c == '0')
						_maskLenght += 1;
				}
			}
		}

		private string inputMask;
		private int _maskLenght;

		#endregion

		#region Ctor
		public PXTimeSpanLongAttribute()
		{
			inputMask = null;
			_maskLenght = 0;
		}
		#endregion

		#region Initialization
		#endregion

		#region Implementation

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				string inputMask = this.inputMask ?? _inputMasks[(int)this._Format];
				int lenght = this.inputMask != null ? _maskLenght : _lengths[(int)this._Format];
				inputMask = PXMessages.LocalizeNoPrefix(inputMask);
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, lenght, null, _FieldName, _IsKey, null, String.IsNullOrEmpty(inputMask) ? null : inputMask, null, null, null, null);
			}

			if (e.ReturnValue != null)
			{
				TimeSpan span = new TimeSpan(0, 0, (int)e.ReturnValue, 0);
				int hours = (this._Format == TimeSpanFormatType.LongHoursMinutes) ? span.Days * 24 + span.Hours : span.Hours;
				e.ReturnValue = string.Format(_outputFormats[(int)this._Format], span.Days, hours, span.Minutes);
			}
		}

		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				int length = ((string)e.NewValue).Length;
				int maxLength = this._lengths[(int)this._Format];
				if (length < maxLength)
				{
					StringBuilder bld = new StringBuilder(maxLength);
					for (int i = length; i < maxLength; i++)
					{
						bld.Append('0');
					}
					bld.Append((string)e.NewValue);
					e.NewValue = bld.ToString();
				}

				int val = 0;
				if (!string.IsNullOrEmpty((string)e.NewValue) && int.TryParse(((string)e.NewValue).Replace(" ", "0"), out val))
				{
					int minutes = val % 100;
					int hours = ((val - minutes) / 100) % 100;
					int days = (((val - minutes) / 100) - hours) / 100;
					if (Format == TimeSpanFormatType.LongHoursMinutes)
					{
						hours = (val - minutes) / 100;
						days = 0;
					}
					TimeSpan span = new TimeSpan(days, hours, minutes, 0);
					e.NewValue = (int)span.TotalMinutes;
				}
				else
				{
					e.NewValue = null;
				}
			}
			if (e.NewValue == null && this._NullIsZero)
				e.NewValue = (int)0;
		}
		#endregion
	}
	#endregion

	#region PXDBTimeSpanLongAttribute
	public class PXDBTimeSpanLongAttribute : PXDBIntAttribute
	{
		#region State

		protected string[] _inputMasks = new string[] { ActionsMessages.TimeSpanMaskDHM, "### d 00:00", ActionsMessages.TimeSpanLongHM, ActionsMessages.TimeSpanHM, "00:00" };
		protected string[] _outputFormats = new string[] { "{0,3}{1:00}{2:00}", "{0,3}{1:00}{2:00}", "{1,4}{2:00}", "{1,2}{2:00}", "{1:00}{2:00}" };
		protected int[] _lengths = new int[] { 7, 7, 6, 4, 4 };
		protected bool _NullIsZero = false;

		protected TimeSpanFormatType _Format = TimeSpanFormatType.DaysHoursMinites;
		public TimeSpanFormatType Format
		{
			get
			{
				return _Format;
			}
			set
			{
				_Format = value;
			}
		}
		public string InputMask
		{
			get { return inputMask; }
			set
			{
				inputMask = value;
				_maskLenght = 0;
				foreach (char c in value)
				{
					if (c == '#' || c == '0')
						_maskLenght += 1;
				}
			}
		}

		private string inputMask;
		private int _maskLenght;


		#endregion

		#region Ctor
		public PXDBTimeSpanLongAttribute()
		{
		}
		#endregion

		#region Initialization
		#endregion

		#region Implementation

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				string inputMask = this.inputMask ?? _inputMasks[(int)this._Format];
				inputMask = PXMessages.LocalizeNoPrefix(inputMask);
				int lenght = this.inputMask != null ? _maskLenght : _lengths[(int)this._Format];
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, lenght, null, _FieldName, _IsKey, null, String.IsNullOrEmpty(inputMask) ? null : inputMask, null, null, null, null);
			}

			if (e.ReturnValue != null)
			{
				TimeSpan span = new TimeSpan(0, 0, (int)e.ReturnValue, 0);
				int hours = (this._Format == TimeSpanFormatType.LongHoursMinutes) ? span.Days * 24 + span.Hours : span.Hours;
				e.ReturnValue = string.Format(_outputFormats[(int)this._Format], span.Days, hours, span.Minutes);
			}
		}

		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string)
			{
				int length = ((string)e.NewValue).Length;
				int maxLength = this._lengths[(int)this._Format];
				if (length < maxLength)
				{
					StringBuilder bld = new StringBuilder(maxLength);
					for (int i = length; i < maxLength; i++)
					{
						bld.Append('0');
					}
					bld.Append((string)e.NewValue);
					e.NewValue = bld.ToString();
				}

				int val = 0;
				if (!string.IsNullOrEmpty((string)e.NewValue) && int.TryParse(((string)e.NewValue).Replace(" ", "0"), out val))
				{
					int minutes = val % 100;
					int hours = ((val - minutes) / 100) % 100;
					int days = (((val - minutes) / 100) - hours) / 100;
					if (Format == TimeSpanFormatType.LongHoursMinutes)
					{
						hours = (val - minutes) / 100;
						days = 0;
					}
					TimeSpan span = new TimeSpan(days, hours, minutes, 0);
					e.NewValue = (int)span.TotalMinutes;
				}
				else
				{
					e.NewValue = null;
				}
			}
			if (e.NewValue == null && this._NullIsZero)
				e.NewValue = (int)0;
		}
		#endregion
	}
	#endregion

	#region PXTimeSpanAttribute
	public class PXTimeSpanAttribute : PXIntAttribute
	{
		#region State
		protected string _InputMask = "HH:mm";
		protected string _DisplayMask = "HH:mm";
		protected new DateTime? _MinValue;
		protected new DateTime? _MaxValue;
		public string InputMask
		{
			get
			{
				return _InputMask;
			}
			set
			{
				_InputMask = value;
			}
		}
		public string DisplayMask
		{
			get
			{
				return _DisplayMask;
			}
			set
			{
				_DisplayMask = value;
			}
		}
		public new string MinValue
		{
			get
			{
				if (_MinValue != null)
				{
					return _MinValue.ToString();
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					_MinValue = DateTime.Parse(value);
				}
				else
				{
					_MinValue = null;
				}
			}
		}
		public new string MaxValue
		{
			get
			{
				if (_MaxValue != null)
				{
					return _MaxValue.ToString();
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					_MaxValue = DateTime.Parse(value);
				}
				else
				{
					_MaxValue = null;
				}
			}
		}
		#endregion

		#region Ctor
		public PXTimeSpanAttribute()
		{
		}
		#endregion

		#region Initialization
		#endregion

		#region Implementation
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXDateState.CreateInstance(e.ReturnState, _FieldName, _IsKey, null, _InputMask, _DisplayMask, _MinValue, _MaxValue);
			}

			if (e.ReturnValue != null)
			{
				TimeSpan span = new TimeSpan(0, 0, (int)e.ReturnValue, 0);
				e.ReturnValue = new DateTime(1900, 1, 1, span.Hours, span.Minutes, 0);
			}
		}

		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue == null || e.NewValue is int)
			{
			}
			else if (e.NewValue is string)
			{
				DateTime val;
				if (DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, DateTimeStyles.None, out val))
				{
					TimeSpan span = new TimeSpan(val.Hour, val.Minute, 0);
					e.NewValue = (int)span.TotalMinutes;
				}
				else
				{
					e.NewValue = null;
				}
			}
			else if (e.NewValue is DateTime)
			{
				DateTime val = (DateTime)e.NewValue;
				TimeSpan span = new TimeSpan(val.Hour, val.Minute, 0);
				e.NewValue = (int)span.TotalMinutes;
			}
		}
		#endregion
	}
	#endregion

	#region PXDBTimeSpanAttribute
	public class PXDBTimeSpanAttribute : PXDBIntAttribute
	{
		#region State
		protected string _InputMask = "HH:mm";
		protected string _DisplayMask = "HH:mm";
		protected new DateTime? _MinValue;
		protected new DateTime? _MaxValue;

		public string InputMask
		{
			get
			{
				return _InputMask;
			}
			set
			{
				_InputMask = value;
			}
		}
		public string DisplayMask
		{
			get
			{
				return _DisplayMask;
			}
			set
			{
				_DisplayMask = value;
			}
		}
		public new string MinValue
		{
			get
			{
				if (_MinValue != null)
				{
					return _MinValue.ToString();
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					_MinValue = DateTime.Parse(value);
				}
				else
				{
					_MinValue = null;
				}
			}
		}
		public new string MaxValue
		{
			get
			{
				if (_MaxValue != null)
				{
					return _MaxValue.ToString();
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					_MaxValue = DateTime.Parse(value);
				}
				else
				{
					_MaxValue = null;
				}
			}
		}

		public const string Zero = "00:00";
		public sealed class zero : Constant<string> { public zero() : base(Zero) { } }
		#endregion

		#region Ctor
		public PXDBTimeSpanAttribute()
		{
		}
		#endregion

		#region Initialization
		#endregion

		#region Implementation
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXDateState.CreateInstance(e.ReturnState, _FieldName, _IsKey, null, _InputMask, _DisplayMask, _MinValue, _MaxValue);
			}

			if (e.ReturnValue != null && (e.ReturnValue is int || e.ReturnValue is int?))
			{
				TimeSpan span = new TimeSpan(0, 0, (int)e.ReturnValue, 0);
				e.ReturnValue = new DateTime(1900, 1, 1).Add(span);
				//e.ReturnValue = new DateTime(1900, 1, 1, span.Hours, span.Minutes, 0);
			}
		}

		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue == null || e.NewValue is int)
			{
			}
			else if (e.NewValue is string)
			{
				DateTime val;
				if (DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, DateTimeStyles.None, out val))
				{
					TimeSpan span = new TimeSpan(val.Hour, val.Minute, 0);
					e.NewValue = (int)span.TotalMinutes;
				}
				else
				{
					e.NewValue = null;
				}
			}
			else if (e.NewValue is DateTime)
			{
				DateTime val = (DateTime)e.NewValue;
				TimeSpan span = new TimeSpan(val.Hour, val.Minute, 0);
				e.NewValue = (int)span.TotalMinutes;
			}
		}
		#endregion

		public static DateTime FromMinutes(int minutes)
		{
			TimeSpan span = new TimeSpan(0, 0, minutes, 0);
			return new DateTime(1900, 1, 1).Add(span);
		}
	}
	#endregion

	#region PXImageAttribute
	public class PXImageAttribute : PXStringAttribute
	{
		public string HeaderImage { get; set; }

		#region Implementation
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXImageState.CreateInstance(e.ReturnState, _Length, _IsUnicode, _FieldName, _IsKey, null, _InputMask, null, null, null, null, HeaderImage);
			}
		}
		#endregion
	}
	#endregion

	#region PXDBImageAttribute
	public class PXDBImageAttribute : PXDBStringAttribute
	{
		public string HeaderImage { get; set; }

		#region Implementation
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				if (_AutoMask == MaskMode.Auto)
				{
					_AutoMask = MaskMode.Manual;
					if (sender.Keys.IndexOf(_FieldName) != sender.Keys.Count - 1)
					{
						_InputMask = null;
					}
				}
				e.ReturnState = PXImageState.CreateInstance(e.ReturnState, _Length, _IsUnicode, _FieldName, _IsKey, null, String.IsNullOrEmpty(_InputMask) ? null : _InputMask, null, null, null, null, HeaderImage);
			}
		}
		#endregion
	}
	#endregion

	#region ReadOnlyScope
	public class ReadOnlyScope : IDisposable
	{
		private PXCache[] _caches;
		private bool[] _isDirty;

		public ReadOnlyScope(params PXCache[] caches)
		{
			_caches = caches;
			_isDirty = new bool[_caches.Length];

			for (int i = 0; i < _caches.Length; i++)
			{
				_isDirty[i] = _caches[i].IsDirty;
			}
		}

		void IDisposable.Dispose()
		{
			for (int i = 0; i < _caches.Length; i++)
			{
				_caches[i].IsDirty = _isDirty[i];
			}
		}
	}
	#endregion

	#region PXSuppressUIUpdateAttribute
	[AttributeUsage(AttributeTargets.Property)]
	public class PXNoUpdateAttribute : PXEventSubscriberAttribute
	{
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.FieldVerifyingEvents[_FieldName.ToLower()] += delegate(PXCache cache, PXFieldVerifyingEventArgs e)
			{
				if (e.Row != null && e.ExternalCall)
				{
					e.NewValue = cache.GetValue(e.Row, _FieldOrdinal);
					e.Cancel = true;
				}
			};
		}
	}
	#endregion

	#region PXCheckUnique
	public class PXCheckUnique : PXEventSubscriberAttribute, IPXRowInsertingSubscriber, IPXRowUpdatingSubscriber, IPXRowPersistingSubscriber
	{
		public Type Where;

		protected string[] _UniqueFields;
		protected PXView _View;


		public PXCheckUnique(params Type[] fields)
		{
			_UniqueFields = new string[fields.Length + 1];
			for (int i = 0; i < fields.Length; i++)
				_UniqueFields[i] = fields[i].Name;
		}

		private bool _ignoreNulls = true;
		public bool IgnoreNulls
		{
			get { return _ignoreNulls; }
			set { _ignoreNulls = value; }
		}

		private bool _clearOnDuplicate = true;
		public bool ClearOnDuplicate
		{
			get { return _clearOnDuplicate; }
			set { _clearOnDuplicate = value; }
		}


		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, OnFieldDefaulting);
			_UniqueFields[_UniqueFields.Length - 1] = _FieldName;
			Type sourceType = sender.GetItemType();
			Type where = Where ?? typeof(Where<True, Equal<True>>);

			for (int i = 0; i < _UniqueFields.Length; i++)
			{
				Type field = sender.GetBqlField(_UniqueFields[i]);
				where = BqlCommand.Compose(
					typeof(Where2<,>),
						typeof(Where<,,>),
						field, typeof(IsNull), typeof(And<,,>),
						typeof(Current<>), field, typeof(IsNull), typeof(Or<,>),
						field, typeof(Equal<>), typeof(Current<>), field,
					typeof(And<>), where
					);
			}
			Type command = BqlCommand.Compose(typeof(Select<,>), sourceType, where);

			_View = new PXView(sender.Graph, false,
				BqlCommand.CreateInstance(command));
		}

		#region IPXRowInsertingSubscriber Members

		public void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (e.Row != null)
			{
				if (!IgnoreNulls && _UniqueFields.Any(field => sender.GetValue(e.Row, field) == null))
					return;

				e.Cancel = !ValidateDuplicates(sender, e.Row, null);
			}
		}

		#endregion

		#region IPXRowUpdatingSubscriber Members

		public void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			ClearErrors(sender, e.NewRow);
			if (e.Row != null && e.NewRow != null && CheckUpdated(sender, e.Row, e.NewRow))
				e.Cancel = !ValidateDuplicates(sender, e.NewRow, e.Row);

			if (ClearOnDuplicate && CheckEquals(sender.GetValue(e.Row, _FieldName), sender.GetValue(e.NewRow, _FieldName)) && e.Cancel)
			{
				ClearErrors(sender, e.NewRow);
				sender.SetValue(e.NewRow, _FieldName, null);
				e.Cancel = !ValidateDuplicates(sender, e.NewRow, e.Row);
			}
		}

		#endregion

		#region IPXRowPersistingSubscriber Members

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null)
				e.Cancel = !ValidateDuplicates(sender, e.Row, null);
		}

		#endregion

		private void ClearErrors(PXCache sender, object row)
		{
			foreach (string field in _UniqueFields)
			{
				string errorText = PXUIFieldAttribute.GetError(sender, row, field);
				if (!string.IsNullOrEmpty(errorText) && PXMessages.Localize(ErrorMessages.DuplicateEntryAdded).EndsWith(errorText))
				{
					PXUIFieldAttribute.SetError(sender, row, field, null);
				}
			}
		}

		protected virtual void OnFieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (callInprocess) return;

			if (e.Cancel != true)
			{
				callInprocess = true;
				object value = null;
				sender.RaiseFieldDefaulting(_FieldName, e.Row, out value);
				if (value != null)
				{
					object copy = sender.CreateCopy(e.Row);
					sender.SetValue(copy, _FieldName, value);
					e.NewValue = ValidateDuplicates(sender, copy, null) ? value : null;
					e.Cancel = true;
				}
				callInprocess = false;
			}
		}

		private bool callInprocess;

		private bool CheckUpdated(PXCache sender, object row, object newRow)
		{
			foreach (string field in _UniqueFields)
			{
				if (!CheckEquals(sender.GetValue(row, field), sender.GetValue(newRow, field)))
					return true;
			}
			return false;
		}

		public static bool CheckEquals(object v1, object v2)
		{
			return (v1 is string || v2 is string) ?
				string.Compare((string)v1, (string)v2, true) == 0 :
				Object.Equals(v1, v2);
		}

		private bool CheckDefaults(PXCache sender, object row)
		{
			foreach (string field in _UniqueFields)
			{
				bool isnull = false;
				foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(row, field))
				{
					if (attr is PXDefaultAttribute && ((PXDefaultAttribute)attr).PersistingCheck != PXPersistingCheck.Nothing)
					{
						isnull = sender.GetValue(row, field) == null;
						break;
					}
				}
				if (isnull)
					return false;
			}
			return true;
		}

		private bool ValidateDuplicates(PXCache sender, object row, object oldRow)
		{
			if (!IgnoreNulls || CheckDefaults(sender, row) && sender.GetValue(row, _FieldOrdinal) != null)
				foreach (object sibling in _View.SelectMultiBound(new object[] { row }))
				{
					if (!sender.ObjectsEqual(sibling, row))
					{
						foreach (string field in _UniqueFields)
						{
							if (oldRow == null ||
								!CheckEquals(sender.GetValue(row, field), sender.GetValue(oldRow, field)))
							{
								PXFieldState state = sender.GetValueExt(row, field) as PXFieldState;
								sender.RaiseExceptionHandling(field,
									row, state != null ? state.Value : sender.GetValue(row, field),
								  new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded)
									);
							}
						}
						return false;
					}
				}
			return true;
		}
	}
	#endregion

	#region PXShortCutAttribute

	public struct HotKeyInfo
	{
		private readonly bool _ctrlKey;
		private readonly bool _shiftKey;
		private readonly bool _altKey;
		private readonly int[] _charCodes;
		private readonly int _keyCode;

		private string _toString;

		public static readonly HotKeyInfo Empty;

		static HotKeyInfo()
		{
			Empty = new HotKeyInfo();
		}

		public HotKeyInfo(bool ctrl, bool shift, bool alt, int keyCode, int[] charCodes)
		{
			if (!ctrl && !shift && !alt)
				throw new ArgumentException("One of special keys (Ctrl, Shift, Alt) must be set at least.");
			if (keyCode == 0 && (charCodes == null || charCodes.Length == 0))
				throw new ArgumentException("Shortcut must contain functional key or one char at least.");
			_ctrlKey = ctrl;
			_shiftKey = shift;
			_altKey = alt;
			_charCodes = charCodes ?? new int[0];
			_keyCode = keyCode;

			_toString = null;
		}

		public bool CtrlKey
		{
			get { return _ctrlKey; }
		}

		public bool ShiftKey
		{
			get { return _shiftKey; }
		}

		public bool AltKey
		{
			get { return _altKey; }
		}

		public int[] CharCodes
		{
			get { return _charCodes; }
		}

		public int KeyCode
		{
			get { return _keyCode; }
		}

		public override string ToString()
		{
			if (_toString == null)
			{
				var sb = new StringBuilder(_charCodes.Length * 4 + 5);
				if (CtrlKey) sb.Append("Ctrl");
				if (AltKey)
				{
					if (sb.Length > 0) sb.Append(" + ");
					sb.Append("Alt");
				}
				if (ShiftKey)
				{
					if (sb.Length > 0) sb.Append(" + ");
					sb.Append("Shift");
				}
				if (KeyCode > 0)
				{
					if (sb.Length > 0) sb.Append(" + ");
					var name = Enum.GetName(typeof(PX.Export.KeyCodes), KeyCode);
					sb.Append(string.IsNullOrEmpty(name) ? KeyCode.ToString() : name);
				}
				foreach (char c in _charCodes)
				{
					sb.Append(" + ");
					sb.Append(c);
				}
				_toString = sb.ToString();
			}
			return _toString;
		}

		public static string ConvertCharCodes(string str)
		{
			var sb = new StringBuilder(str.Length * 4);
			foreach (char c in str)
			{
				if (sb.Length > 0) sb.Append(" + ");
				sb.Append(c);
			}
			return sb.ToString();
		}

		public bool Equals(HotKeyInfo other)
		{
			return other._ctrlKey.Equals(_ctrlKey) && other._shiftKey.Equals(_shiftKey) &&
				other._altKey.Equals(_altKey) && Equals(other._charCodes, _charCodes) &&
				other._keyCode == _keyCode && Equals(other._toString, _toString);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (obj.GetType() != typeof(HotKeyInfo)) return false;
			return Equals((HotKeyInfo)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = _ctrlKey.GetHashCode();
				result = (result * 397) ^ _shiftKey.GetHashCode();
				result = (result * 397) ^ _altKey.GetHashCode();
				result = (result * 397) ^ (_charCodes != null ? _charCodes.GetHashCode() : 0);
				result = (result * 397) ^ _keyCode;
				result = (result * 397) ^ (_toString != null ? _toString.GetHashCode() : 0);
				return result;
			}
		}

		public static bool operator ==(HotKeyInfo left, HotKeyInfo right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(HotKeyInfo left, HotKeyInfo right)
		{
			return !left.Equals(right);
		}

		public static int[] ConvertChars(char[] data)
		{
			var result = new int[data.Length];
			for (int i = 0; i < data.Length; i++)
				result[i] = (int)data[i];
			return result;
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class PXShortCutAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		private static readonly IDictionary<Type, IDictionary<string, PXShortCutAttribute>> _commands;

		private readonly HotKeyInfo _shortcut;

		static PXShortCutAttribute()
		{
			_commands = new Dictionary<Type, IDictionary<string, PXShortCutAttribute>>();

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					// ignore some assemlies, prevent from raising exceptions on dynamic assemblies as well
					if (!PXSubstManager.IsSuitableTypeExportAssembly(assembly) || assembly.FullName.StartsWith("App_SubCode_"))
						continue;

					Type[] types = null;
					try
					{
						if (!assembly.IsDynamic)
							types = assembly.GetExportedTypes();
					}
					catch (ReflectionTypeLoadException te)
					{
						types = te.Types;
					}
					if (types != null)
					{
						foreach (Type type in types)
						{
							if (type != null && (type.IsGenericType || type.IsAbstract || !typeof(PXGraph).IsAssignableFrom(type))) continue;

							IDictionary<string, PXShortCutAttribute> typeCommands = null;
							foreach (MethodInfo method in type.GetMethods())
							{
								ParameterInfo[] parameters;
								if (!typeof(IEnumerable).IsAssignableFrom(method.ReturnType) ||
									(parameters = method.GetParameters()) == null ||
									parameters.Length == 0 ||
									!typeof(PXAdapter).IsAssignableFrom(parameters[0].ParameterType))
								{
									continue;
								}
								var atts = method.GetCustomAttributes(typeof(PXShortCutAttribute), false);
								if (atts == null || atts.Length == 0) continue;
								if (typeCommands == null)
								{
									if (!_commands.TryGetValue(type, out typeCommands))
										_commands.Add(type, typeCommands = new Dictionary<string, PXShortCutAttribute>());
									typeCommands.Clear();
								}
								foreach (PXShortCutAttribute attribute in atts)
								{
									typeCommands.Remove(method.Name);
									typeCommands.Add(method.Name, attribute);
								}
							}
						}
					}
				}
				catch (StackOverflowException) { throw; }
				catch (OutOfMemoryException) { throw; }
				catch { }
			}
		}

		private PXShortCutAttribute(bool ctrl, bool shift, bool alt, int keyCode, int[] charCodes)
		{
			_shortcut = new HotKeyInfo(ctrl, shift, alt, keyCode, charCodes);
		}

		public PXShortCutAttribute(bool ctrl, bool shift, bool alt, PX.Export.KeyCodes key)
			: this(ctrl, shift, alt, (int)key, null) { }

		public PXShortCutAttribute(bool ctrl, bool shift, bool alt, params char[] chars)
			: this(ctrl, shift, alt, 0, HotKeyInfo.ConvertChars(chars)) { }

		public static IEnumerable<KeyValuePair<string, PXShortCutAttribute>> GetDeclared(string graphName)
		{
			IDictionary<string, PXShortCutAttribute> typeCommands;
			var type = System.Web.Compilation.BuildManager.GetType(graphName, false);
			if (type != null && _commands.TryGetValue(type, out typeCommands))
				foreach (KeyValuePair<string, PXShortCutAttribute> command in typeCommands)
					yield return command;
		}

		#region IPXFieldSelectingSubscriber Members

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnState = PXButtonState.CreateInstance(e.ReturnState, null, null, null, null, null, null,
				PXConfirmationType.Unspecified, null, null, null, null, null, null, null, null, null, null, this, null);
		}

		#endregion

		public HotKeyInfo HotKey
		{
			get { return _shortcut; }
		}
	}

	#endregion

	#region PXDBCreatedDateTimeUtcAttribute
	public class PXDBCreatedDateTimeUtcAttribute : PXDBCreatedDateTimeAttribute
	{
		protected override DateTime GetDate()
		{
			return PXTimeZoneInfo.Now;
		}

		public PXDBCreatedDateTimeUtcAttribute()
			: base()
		{
			UseTimeZone = true;
		}
	}
	#endregion

	#region PXDBLastModifiedDateTimeUtcAttribute
	public class PXDBLastModifiedDateTimeUtcAttribute : PXDBLastModifiedDateTimeAttribute
	{
		protected override DateTime GetDate()
		{
			return PXTimeZoneInfo.Now;
		}

		public PXDBLastModifiedDateTimeUtcAttribute()
		{
			UseTimeZone = true;
		}
	}
	#endregion

	#region PXDefaultValidateAttribute
	public class PXDefaultValidateAttribute : PXDefaultAttribute
	{
		public PXDefaultValidateAttribute(Type sourceType, Type validateExists)
			: base(sourceType)
		{
			this.validateExists = BqlCommand.CreateInstance(validateExists);
		}
		private readonly BqlCommand validateExists;

		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			base.FieldDefaulting(sender, e);
			if (e.NewValue != null)
			{
				PXView view = sender.Graph.TypedViews.GetView(validateExists, false);
				int startRow = -1;
				int totalRows = 0;
				List<object> source = view.Select(
					new object[] { e.Row },
					new object[] { e.NewValue },
					null,
					null,
					null,
					null,
					ref startRow,
					1,
					ref totalRows);
				if (source != null && source.Count > 0)
				{
					e.NewValue = null;
					e.Cancel = true;
				}
			}
		}
	}
	#endregion

	#region PXCustomStringListAttribute
	public class PXCustomStringListAttribute : PXStringListAttribute
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

		public PXCustomStringListAttribute(string[] AllowedValues, string[] AllowedLabels)
			: base(AllowedValues, AllowedLabels)
		{
		}
	}
	#endregion

	#region PXDependsOnFieldsAttribute
	/// <summary>
	/// Used for calculated DAC fields, that contains referenses to other fields in their property getters.
	/// This attribute allows to such fields properly works in reports.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
	public sealed class PXDependsOnFieldsAttribute : Attribute
	{
		private Type[] _fields;
		public PXDependsOnFieldsAttribute(params Type[] fields)
		{
			_fields = fields;
		}

		internal static HashSet<string> GetDependsRecursive(PXCache table, string field)
		{
			var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			AddDependsRecursive(table, field, result);
			return result;

		}
		static void AddDependsRecursive(PXCache table, string field, HashSet<string> result)
		{
			if (result.Contains(field))
				return;

			var p = table.GetItemType().GetProperty(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
			if (p == null)
			{
				foreach (Type extension in table.GetExtensionTypes())
				{
					p = p ?? extension.GetProperty(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
				}
				if (p == null)
					return;
			}

			result.Add(p.Name);
			var attr = (PXDependsOnFieldsAttribute)Attribute.GetCustomAttribute(p, typeof(PXDependsOnFieldsAttribute));
			if (attr == null)
			{

				var getter = p.GetGetMethod();
				attr = (PXDependsOnFieldsAttribute)Attribute.GetCustomAttribute(getter, typeof(PXDependsOnFieldsAttribute));

			}

			if (attr == null)
				return;

			foreach (Type fType in attr._fields)
			{
				AddDependsRecursive(table, fType.Name, result);

			}

		}

	}

	#endregion

	#region PXCompositeSegmentedKeyAttribute
	public class PXCompositeKeyAttribute : PXEventSubscriberAttribute, IPXRowSelectingSubscriber//, IPXFieldVerifyingSubscriber
	{
		protected PXSegment[] _Segments;
		protected virtual void _EnsureSegments(PXCache sender)
		{
			if (_Segments == null)
			{
				_Segments = new PXSegment[0];
				PXSegment[] segments = new PXSegment[sender.Keys.Count];
				int offset = 0;
				for (int i = 0; i < sender.Keys.Count; i++)
				{
					PXStringState state = sender.GetStateExt(null, sender.Keys[i]) as PXStringState;
					if (state != null)
					{
						char mask = 'C';
						short caseconvert = 0;
						if (state.InputMask == null)
						{
							if (state.AllowedValues != null)
							{
								mask = '?';
								caseconvert = 1;
							}
						}
						else if (state.InputMask.Length > 0)
						{
							mask = state.InputMask[0];
							if (!char.IsLetterOrDigit(mask))
							{
								if (state.InputMask.Length > 1)
								{
									mask = state.InputMask[1];
								}
								else
								{
									mask = 'C';
								}
							}
							if (mask == 'A')
							{
								mask = 'a';
							}
							else if (mask == '#')
							{
								mask = '9';
							}
							else if (mask == 'L')
							{
								mask = '?';
							}
							if (state.InputMask[0] == '>')
							{
								caseconvert = 1;
							}
							else if (state.InputMask[0] == '<')
							{
								caseconvert = 2;
							}

						}
						segments[i + offset] = new PXSegment(mask, ' ', (short)state.Length, true, caseconvert, 0, '-', !state.Enabled);
					}
					else
					{
						PXFieldState fs = sender.GetStateExt(null, sender.Keys[i]) as PXFieldState;
						if (fs != null && !String.IsNullOrEmpty(fs.ViewName))
						{
							PXView view = sender.Graph.Views[fs.ViewName];
							if (!object.ReferenceEquals(sender, view.Cache))
							{
								PXSegmentedState comp = view.Cache.GetStateExt(null, "CompositeKey") as PXSegmentedState;
								if (comp != null)
								{
									Array.Resize(ref segments, segments.Length + comp.Segments.Length - 1);
									for (int j = 0; j < comp.Segments.Length; j++)
									{
										segments[i + offset + j] = new PXSegment(comp.Segments[j].EditMask, ' ', comp.Segments[j].Length, true, comp.Segments[j].CaseConvert, 0, comp.Segments[j].Separator, comp.Segments[j].ReadOnly);
									}
									offset += comp.Segments.Length - 1;
									continue;
								}
							}
						}
						segments[i + offset] = new PXSegment('C', ' ', 0, true, (short)0, 0, '-', false);
					}
				}
				_Segments = segments;
			}
		}

		public virtual void CompositeKeyFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			_EnsureSegments(sender);
			List<string> errs = new List<string>();
			if (e.Row != null)
			{
				StringBuilder bld = new StringBuilder();
				int offset = 0;
				for (int i = 0; i < sender.Keys.Count; i++)
				{
					if (_Segments[i + offset].Length > 0)
					{
						PXFieldState fs = sender.GetStateExt(e.Row, sender.Keys[i]) as PXFieldState;
						if (fs != null && !String.IsNullOrEmpty(fs.ViewName))
						{
							PXView view = sender.Graph.Views[fs.ViewName];
							if (!object.ReferenceEquals(sender, view.Cache))
							{
								object foreign = PXSelectorAttribute.Select(sender, e.Row, sender.Keys[i]);
								PXSegmentedState comp = view.Cache.GetStateExt(foreign, "CompositeKey") as PXSegmentedState;
								if (comp != null)
								{
									int length = 0;
									for (int j = 0; j < comp.Segments.Length; j++)
									{
										length += comp.Segments[j].Length;
									}
									if (!String.IsNullOrEmpty(comp.Value as string))
									{
										bld.Append((string)comp.Value);
										length -= ((string)comp.Value).Length;
									}
									if (length > 0)
									{
										bld.Append(' ', length);
									}
									if (!String.IsNullOrEmpty(comp.Error) && (comp.ErrorLevel == PXErrorLevel.Error || comp.ErrorLevel == PXErrorLevel.RowError))
									{
										errs.Add(comp.DisplayName);
									}
									offset += comp.Segments.Length - 1;
									continue;
								}
							}
						}
						object state = sender.GetValueExt(e.Row, sender.Keys[i]);
						if (state is PXFieldState && !String.IsNullOrEmpty(((PXFieldState)state).Error) && (((PXFieldState)state).ErrorLevel == PXErrorLevel.Error || ((PXFieldState)state).ErrorLevel == PXErrorLevel.RowError))
						{
							errs.Add(((PXFieldState)state).DisplayName);
						}
						string s = PXFieldState.UnwrapValue(state) as string;
						if (i < sender.Keys.Count - 1)
						{
							if (s == null)
							{
								s = new string(' ', _Segments[i + offset].Length);
							}
							else if (s.Length < _Segments[i + offset].Length)
							{
								s = s + new string(' ', _Segments[i + offset].Length - s.Length);
							}
							else if (s.Length > _Segments[i + offset].Length)
							{
								s = s.Substring(0, _Segments[i + offset].Length);
							}
						}
						else if (s == null)
						{
							s = "";
						}
						if (_Segments[i + offset].CaseConvert == 1)
						{
							s = s.ToUpper();
						}
						if (_Segments[i + offset].CaseConvert == 2)
						{
							s = s.ToLower();
						}
						bld.Append(s);
					}
				}
				e.ReturnValue = bld.ToString();
			}
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXSegmentedState.CreateInstance(
					PXStringState.CreateInstance(e.ReturnState, null, true, "CompositeKey", true, (short)1, null, null, null, null, null),
					"CompositeKey", _Segments, "_" + sender.GetItemType().Name + "_Composite_", false, null);
				((PXFieldState)e.ReturnState).Visibility = PXUIVisibility.Visible;
				PXFieldState state = sender.GetStateExt(e.Row, sender.Keys[sender.Keys.Count - 1]) as PXFieldState;
				if (state != null)
				{
					((PXFieldState)e.ReturnState).DisplayName = state.DisplayName;
				}
				if (errs.Count > 0)
				{
					((PXFieldState)e.ReturnState).ErrorLevel = PXErrorLevel.Error;
					if (errs.Count == 1)
					{
						((PXFieldState)e.ReturnState).Error = PXMessages.LocalizeFormat(ErrorMessages.ElementOfFieldDoesntExist, errs[0], state != null ? state.DisplayName : _FieldName);
					}
					else
					{
						errs.Add(_FieldName);
						StringBuilder bld = new StringBuilder();
						StringBuilder bld2 = new StringBuilder();
						int i;
						for (i = 0; i < errs.Count - 1; i++)
						{
							bld.Append('{');
							bld.Append(i);
							bld.Append('}');
							if (i < errs.Count - 2)
							{
								bld.Append(", ");
							}
						}
						bld2.Append('{');
						bld2.Append(i);
						bld2.Append('}');
						((PXFieldState)e.ReturnState).Error = String.Format(PXMessages.LocalizeFormat(ErrorMessages.ElementsOfFieldsDontExist, bld.ToString(), bld2.ToString()), errs.ToArray());
					}
				}
			}
		}

		public virtual void CompositeKeyFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			string s = e.NewValue as string;
			if (e.Row != null && s != null)
			{
				_EnsureSegments(sender);
				int start = 0;
				int offset = 0;
				for (int i = 0; i < sender.Keys.Count; i++)
				{
					if (_Segments[i + offset].Length > 0)
					{
						PXFieldState fs = sender.GetStateExt(e.Row, sender.Keys[i]) as PXFieldState;
						if (fs != null && !String.IsNullOrEmpty(fs.ViewName))
						{
							PXView view = sender.Graph.Views[fs.ViewName];
							if (!object.ReferenceEquals(sender, view.Cache))
							{
								PXSegmentedState comp = view.Cache.GetStateExt(null, "CompositeKey") as PXSegmentedState;
								if (comp != null)
								{
									int sublength = 0;
									for (int j = 0; j < comp.Segments.Length; j++)
									{
										sublength += comp.Segments[j].Length;
									}
									string val = null;
									if (start < s.Length)
									{
										if (start + sublength < s.Length)
										{
											val = s.Substring(start, sublength);
										}
										else
										{
											val = s.Substring(start);
										}
										val = val.TrimEnd();
										if (val == "")
										{
											val = null;
										}
										start += sublength;
									}
									int first = 0;
									int total = 0;
									List<object> ret = view.Select(new object[] { e.Row }, null, new object[] { val }, new string[] { "CompositeKey" }, null, null, ref first, 1, ref total);
									object key = null;
									if (ret.Count > 0)
									{
										key = PXFieldState.UnwrapValue(view.Cache.GetValueExt(ret[0], fs.ValueField));
									}
									sender.SetValueExt(e.Row, sender.Keys[i], key);
									offset += comp.Segments.Length - 1;
									continue;
								}
							}
						}
						{
							string val = null;
							if (start < s.Length)
							{
								if (start + _Segments[i + offset].Length - 1 < s.Length)
								{
									val = s.Substring(start, _Segments[i + offset].Length);
								}
								else
								{
									val = s.Substring(start);
								}
								val = val.TrimEnd();
								if (val == "")
								{
									val = null;
								}
							}
							start += _Segments[i + offset].Length;
							sender.SetValueExt(e.Row, sender.Keys[i], val);
						}
					}
				}
			}
			e.NewValue = null;
		}

		public virtual void CompositeKeyCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select)
			{
				if (e.Value == null)
				{
					PXCommandPreparingEventArgs.FieldDescription descr;
					sender.RaiseCommandPreparing(_FieldName, e.Row, e.Value, e.Operation, e.Table, out descr);
					if (descr != null)
					{
						e.BqlTable = descr.BqlTable;
						e.FieldName = descr.FieldName;
						e.DataType = descr.DataType;
						e.DataLength = descr.DataLength;
						e.DataValue = descr.DataValue;
						e.IsRestriction = descr.IsRestriction;
					}
				}
				else
				{
					_EnsureSegments(sender);
					System.Text.StringBuilder concatField = new System.Text.StringBuilder();
					System.Text.StringBuilder concatValue = new System.Text.StringBuilder();
					string s = e.Value as string ?? "";
					int start = 0;
					int offset = 0;
					for (int i = 0; i < sender.Keys.Count; i++)
					{
						if (_Segments[i + offset].Length > 0)
						{
							string val = null;
							PXCommandPreparingEventArgs.FieldDescription descr;
							PXFieldState fs = sender.GetStateExt(e.Row, sender.Keys[i]) as PXFieldState;
							if (fs != null && !String.IsNullOrEmpty(fs.ViewName))
							{
								PXView view = sender.Graph.Views[fs.ViewName];
								if (!object.ReferenceEquals(sender, view.Cache))
								{
									PXSegmentedState comp = view.Cache.GetStateExt(null, "CompositeKey") as PXSegmentedState;
									if (comp != null)
									{
										int sublength = 0;
										for (int j = 0; j < comp.Segments.Length; j++)
										{
											sublength += comp.Segments[j].Length;
										}
										val = null;
										if (start < s.Length)
										{
											if (start + sublength < s.Length)
											{
												val = s.Substring(start, sublength);
											}
											else
											{
												val = s.Substring(start);
											}
											if (val == "")
											{
												val = null;
											}
											start += sublength;
										}
										view.Cache.RaiseCommandPreparing("CompositeKey", null, val, e.Operation, view.Cache.GetItemType(), out descr);
										if (descr != null && !String.IsNullOrEmpty(descr.FieldName))
										{
											string subselect = descr.FieldName;
											object subvalue = descr.DataValue;
											sender.RaiseCommandPreparing(sender.Keys[i], e.Row, null, e.Operation, e.Table, out descr);
											string fieldname = descr.FieldName;
											view.Cache.RaiseCommandPreparing(fs.ValueField, null, null, e.Operation, view.Cache.GetItemType(), out descr);
											subselect = BqlCommand.SubSelect + subselect + " FROM " + view.Cache.GetItemType().Name + " WHERE " + descr.FieldName + " = " + fieldname + ")";
											if (concatField.Length > 0)
											{
												concatField.Append(" + ");
											}
											if (i < sender.Keys.Count - 1)
											{
												concatField.Append("SUBSTRING(CONVERT(NVARCHAR(");
												concatField.Append(sublength);
												concatField.Append("), ");
												concatField.Append(subselect);
												concatField.Append(") + '");
												concatField.Append(' ', sublength);
												concatField.Append("', 1, ");
												concatField.Append(sublength);
												concatField.Append(")");
											}
											else
											{
												concatField.Append(subselect);
											}
											if (subvalue is string)
											{
												concatValue.Append((string)subvalue);
											}
										}
										offset += comp.Segments.Length - 1;
										continue;
									}
								}
							}
							if (start < s.Length)
							{
								if (start + _Segments[i + offset].Length - 1 < s.Length)
								{
									val = s.Substring(start, _Segments[i + offset].Length);
								}
								else
								{
									val = s.Substring(start);
								}
							}
							start += _Segments[i + offset].Length;
							sender.RaiseCommandPreparing(sender.Keys[i], e.Row, val, e.Operation, e.Table, out descr);
							if (descr != null && !String.IsNullOrEmpty(descr.FieldName))
							{
								if (concatField.Length > 0)
								{
									concatField.Append(" + ");
								}
								if (i < sender.Keys.Count - 1)
								{
									concatField.Append("SUBSTRING(CONVERT(NVARCHAR(");
									concatField.Append(_Segments[i + offset].Length);
									concatField.Append("), ");
									concatField.Append(descr.FieldName);
									concatField.Append(") + '");
									concatField.Append(' ', _Segments[i + offset].Length);
									concatField.Append("', 1, ");
									concatField.Append(_Segments[i + offset].Length);
									concatField.Append(")");
								}
								else
								{
									concatField.Append(descr.FieldName);
								}
								if (descr.DataValue is string)
								{
									concatValue.Append((string)descr.DataValue);
								}
							}
						}
					}
					e.DataType = PXDbType.NVarChar;
					e.DataLength = concatValue.Length;
					e.BqlTable = _BqlTable;
					e.FieldName = concatField.ToString();
					if (e.Value is string)
					{
						e.DataValue = concatValue.ToString();
					}
					e.IsRestriction = true;
				}
			}
		}

		//public virtual void CompositeKeyRowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		//{
		//    if (sender.BqlSelect != null)
		//    {
		//        sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetValue(sender.GetItemType().Name + "." + _FieldName));
		//    }
		//    else
		//    {
		//        PXCommandPreparingEventArgs.FieldDescription descr;
		//        sender.RaiseCommandPreparing(_FieldName, null, null, PXDBOperation.Select, sender.GetItemType(), out descr);
		//        if (descr != null && !String.IsNullOrEmpty(descr.FieldName))
		//        {
		//            sender.SetValue(e.Row, _FieldOrdinal, e.Record.GetValue(descr.FieldName));
		//        }
		//    }
		//}

		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			e.Position++;
		}

		protected class CompositeView : PXView
		{
			protected PXCache _Sender;
			public CompositeView(PXCache sender)
				: base(sender.Graph, true, new Select<PXDimensionAttribute.SegmentValue>())
			{
				_Sender = sender;
				_Delegate = (PXSelectDelegate<string, short?>)GetRecords;
			}
			public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
			{
				if (parameters != null && parameters.Length > 1 && parameters[1] != null)
				{
					short segment = 0;
					if (parameters[1] is short)
					{
						segment = (short)parameters[1];
					}
					else if (parameters[1] is string)
					{
						short.TryParse((string)parameters[1], out segment);
					}
					if (segment != 0)
					{
						int offset = 0;
						for (int i = 0; i < _Sender.Keys.Count; i++)
						{
							PXFieldState fs = _Sender.GetStateExt(_Sender.Current, _Sender.Keys[i]) as PXFieldState;
							if (fs != null && !String.IsNullOrEmpty(fs.ViewName))
							{
								PXView view = _Sender.Graph.Views[fs.ViewName];
								if (!object.ReferenceEquals(_Sender, view.Cache))
								{
									object foreign = PXSelectorAttribute.Select(_Sender, _Sender.Current, _Sender.Keys[i]);
									PXSegmentedState comp = view.Cache.GetStateExt(foreign, "CompositeKey") as PXSegmentedState;
									if (comp != null)
									{
										if ((int)segment - 1 - offset < comp.Segments.Length)
										{
											parameters[1] = (short?)(segment - offset);
											return _Sender.Graph.Views[comp.ViewName].Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
										}
										offset += comp.Segments.Length - 1;
									}
								}
							}
							if (i + offset == (int)segment - 1)
							{
								parameters[1] = (short?)(segment - offset);
								break;
							}
						}
					}
				}
				return base.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
			}
			protected IEnumerable GetRecords(
				[PXDBString]
				string value,
				[PXDBShort]
				short? segment
				)
			{
				List<PXDimensionAttribute.SegmentValue> list = new List<PXDimensionAttribute.SegmentValue>();
				if (segment >= 1 && segment <= _Sender.Keys.Count)
				{
					PXFieldState state = _Sender.GetStateExt(null, _Sender.Keys[(int)segment - 1]) as PXFieldState;
					if (state != null)
					{
						if (!String.IsNullOrEmpty(state.ViewName))
						{
							PXView view = _Sender.Graph.Views[state.ViewName];
							string valueField = state.ValueField ?? (view.Cache.Keys.Count > 0 ? view.Cache.Keys[view.Cache.Keys.Count - 1] : null);
							if (valueField != null)
							{
								object instance = _Sender.CreateInstance();
								try
								{
									_Sender.SetValueExt(instance, "CompositeKey", value);
								}
								catch
								{
								}
								foreach (object result in view.SelectMultiBound(new object[] { instance }))
								{
									PXDimensionAttribute.SegmentValue ret = new PXDimensionAttribute.SegmentValue();
									object item = result;
									if (item is PXResult)
									{
										item = ((PXResult)item)[0];
									}
									ret.Value = PXFieldState.UnwrapValue(view.Cache.GetValueExt(item, valueField)) as string;
									if (state.DescriptionName != null)
									{
										ret.Descr = PXFieldState.UnwrapValue(view.Cache.GetValueExt(item, state.DescriptionName)) as string;
									}
									list.Add(ret);
								}
							}
						}
						else if (state is PXStringState)
						{
							PXStringState sstate = (PXStringState)state;
							if (sstate.AllowedValues != null && sstate.AllowedValues.Length > 0)
							{
								for (int i = 0; i < sstate.AllowedValues.Length; i++)
								{
									PXDimensionAttribute.SegmentValue ret = new PXDimensionAttribute.SegmentValue();
									ret.Value = sstate.AllowedValues[i];
									if (sstate.AllowedLabels != null && i < sstate.AllowedLabels.Length)
									{
										ret.Descr = sstate.AllowedLabels[i];
									}
									list.Add(ret);
								}
							}
						}
					}
				}
				return list.ToArray();
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.Views["_" + sender.GetItemType().Name + "_Composite_"] = new CompositeView(sender);
			if (!sender.Fields.Contains("CompositeKey"))
			{
				sender.Fields.Insert(sender.Fields.IndexOf(_FieldName) + 1, "CompositeKey");
				sender.FieldSelectingEvents["compositekey"] += CompositeKeyFieldSelecting;
				sender.FieldUpdatingEvents["compositekey"] += CompositeKeyFieldUpdating;
				sender.CommandPreparingEvents["compositekey"] += CompositeKeyCommandPreparing;
			}
		}
	}
	#endregion

	#region IPXReportRequiredField
	public interface IPXReportRequiredField { }

	#endregion

	public class PXDBDecimalStringAttribute : PXDBDecimalAttribute
	{
		public PXDBDecimalStringAttribute()
			: base()
		{
		}

		public PXDBDecimalStringAttribute(int precision)
			: base(precision)
		{
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 10, false, _FieldName, _IsKey, null, null, null, null, null, null);
			}

			if (e.ReturnValue != null && e.ReturnValue is decimal)
			{
				e.ReturnValue = ((decimal)e.ReturnValue).ToString("F2", sender.Graph.Culture);
			}
		}

		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue != null && e.NewValue is string)
			{
				decimal val;
				if (decimal.TryParse((string)e.NewValue, NumberStyles.Any, sender.Graph.Culture, out val))
				{
					e.NewValue = val;
				}
				else
				{
					e.NewValue = null;
				}
			}
		}
	}

	public class PXDecimalListAttribute : PXStringListAttribute
	{
		public PXDecimalListAttribute(string[] allowedValues, string[] allowedLabels)
			: base(allowedValues, allowedLabels)
		{

		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			//if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				string[] values = Array.ConvertAll<string, string>(_AllowedValues, delegate(string a)
				{
					decimal val = Decimal.Parse(a, NumberStyles.Any, CultureInfo.InvariantCulture);
					return val.ToString("F2", sender.Graph.Culture);
				}
				);

				string[] labels = Array.ConvertAll<string, string>(_AllowedLabels, delegate(string a)
				{
					decimal val = Decimal.Parse(a, NumberStyles.Any, CultureInfo.InvariantCulture);
					return val.ToString("F2", sender.Graph.Culture);
				}
				);

				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, -1, null, values, labels, _ExclusiveValues, null);
			}
		}
	}


	#region PXAttributeFamilyAttribute
	/// <summary>
	/// Allows to specify rules, which attributes can not be combined together.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class PXAttributeFamilyAttribute : Attribute
	{
		public PXAttributeFamilyAttribute(Type rootType)
		{
			RootType = rootType;

		}
		public readonly Type RootType;
		private static readonly Type[] Empty = new Type[0];

		public static Type[] GetRoots(Type t)
		{


			foreach (var parent in t.CreateList(_ => _.BaseType))
			{
				var list = (PXAttributeFamilyAttribute[])Attribute.GetCustomAttributes(parent, typeof(PXAttributeFamilyAttribute));
				if (list.Any())
					return list.Select(_ => _.RootType).ToArray();

			}



			return Empty;

		}

		public static PXAttributeFamilyAttribute FromType(Type t)
		{
			foreach (var parent in t.CreateList(_ => _.BaseType))
			{
				var a = (PXAttributeFamilyAttribute)Attribute.GetCustomAttribute(parent, typeof(PXAttributeFamilyAttribute));
				if (a != null)
					return a;

			}

			return null;

		}

		public static void CheckAttributes(PropertyInfo prop, PXEventSubscriberAttribute[] attributes)
		{
			var groups = (from a in attributes
						  from t in GetRoots(a.GetType())
						  select new { t, a })
				.ToLookup(_ => _.t, _ => _.a);

			//var groups = attributes.ToLookup(a => GetFamilyRoot(a.GetType()));
			foreach (var g in groups)
			{
				if (g.Key == null)
					continue;

				if (g.Count() > 1)
				{
					PXValidationWriter.AddTypeError(prop.DeclaringType, "Not compatible attributes detected. Family: {0}, Property {1}::{2}, Attributes: {3}", g.Key.Name, prop.DeclaringType, prop.Name, g.Select(_ => _.GetType().Name).JoinToString(","));
				}


			}
		}
	}


	public class PXValidationWriter
	{
		public static readonly bool? PageValidation = GetPageValidation();

		static bool? GetPageValidation()
		{
			string ValidationPolicyConfig = System.Web.Configuration.WebConfigurationManager.AppSettings["PageValidation"];
			if (String.IsNullOrEmpty(ValidationPolicyConfig))
				return null;

			return Convert.ToBoolean(ValidationPolicyConfig);

		}

		//public static readonly PXValidationWriter Current = new PXValidationWriter();



		public static readonly Dictionary<Type, HashSet<string>> TypeErrors = new Dictionary<Type, HashSet<string>>();
		public static void AddTypeError(Type t, string format, params object[] args)
		{
			if (PageValidation == false)
				return;
			if (!TypeErrors.ContainsKey(t))
				TypeErrors.Add(t, new HashSet<string>());

			TypeErrors[t].Add(String.Format(format, args));

		}

	}
	#endregion

	#region PXFeatureAttribute
	public class PXFeatureAttribute : Attribute
	{
		public readonly Type Feature;
		public PXFeatureAttribute(Type feature)
		{
			this.Feature = feature;
		}
	}
	#endregion

	#region PXDBLastChangeDateTimeAttribute
	public class PXDBLastChangeDateTimeAttribute : PXDBDateAttribute, IPXRowUpdatingSubscriber
	{
		[Serializable]
		public class SelectedValue : IBqlTable
		{
			[PXString(IsKey = true)]
			public virtual string FieldName { get; set; }

			public virtual object Value { get; set; }
		}

		private readonly Type _MonitoredField;

		public PXDBLastChangeDateTimeAttribute(Type monitoredField)
		{
			UseSmallDateTime = false;
			base.PreserveTime = true;
			base.UseTimeZone = true;

			if (monitoredField == null)
			{
				throw new PXArgumentException("monitoredField", ErrorMessages.ArgumentNullException);
			}
			if (typeof(IBqlField).IsAssignableFrom(monitoredField))
			{
				_MonitoredField = monitoredField;
			}
			else
			{
				throw new PXArgumentException("monitoredField", ErrorMessages.ArgumentException);
			}
		}

		public DateTime GetDate()
		{
			if (UseTimeZone)
			{
				return PXTimeZoneInfo.Now;
			}
			DateTime? serverDate = PXTransactionScope.GetServerDateTime(false);
			return serverDate.HasValue ? serverDate.Value : DateTime.Now;
		}

		void IPXRowUpdatingSubscriber.RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (sender.GetValue(e.NewRow, _FieldOrdinal) == null) sender.SetValue(e.NewRow, _FieldOrdinal, GetDate());
		}


		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			base.CommandPreparing(sender, e);
			if ((e.Operation & PXDBOperation.Update) == PXDBOperation.Update ||
				(e.Operation & PXDBOperation.Insert) == PXDBOperation.Insert)
			{
				e.DataLength = 8;
				e.IsRestriction = e.IsRestriction || _IsKey;
				if (_DatabaseFieldName != null)
				{
					e.BqlTable = _BqlTable;
					e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName;
				}

				var origValue = sender.GetValueOriginal(e.Row, _MonitoredField.Name);
				if (origValue != null && !Equals(origValue, sender.GetValue(e.Row, _MonitoredField.Name)))
				{
					e.DataValue = UseTimeZone ? PXDatabase.Provider.SqlDialect.GetUtcDate : PXDatabase.Provider.SqlDialect.GetDate;
					e.DataType = PXDbType.DirectExpression;
					sender.SetValue(e.Row, _FieldOrdinal, GetDate());
				}
				else
				{
					e.DataValue = sender.GetValue(e.Row, _FieldOrdinal);
					e.DataType = UseSmallDateTime ? PXDbType.SmallDateTime : PXDbType.DateTime;
				}
			}
		}


	}
	#endregion

	#region PXDBRevision
	public class PXDBRevision : PXDBIntAttribute, IPXRowUpdatingSubscriber
	{
		private readonly Type _MonitoredField;

		public PXDBRevision(Type monitoredField)
		{
			if (monitoredField == null)
			{
				throw new PXArgumentException("monitoredField", ErrorMessages.ArgumentNullException);
			}
			if (typeof(IBqlField).IsAssignableFrom(monitoredField))
			{
				_MonitoredField = monitoredField;
			}
			else
			{
				throw new PXArgumentException("monitoredField", ErrorMessages.ArgumentException);
			}
		}


		void IPXRowUpdatingSubscriber.RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (sender.GetValue(e.NewRow, _FieldOrdinal) == null) sender.SetValue(e.NewRow, _FieldOrdinal, 0);
		}

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			base.CommandPreparing(sender, e);

			if ((e.Operation & PXDBOperation.Update) == PXDBOperation.Update ||
				(e.Operation & PXDBOperation.Insert) == PXDBOperation.Insert)
			{
				e.DataLength = 4;
				e.IsRestriction = e.IsRestriction || _IsKey;
				if (_DatabaseFieldName != null)
				{
					e.BqlTable = _BqlTable;
					e.FieldName = (e.Table == null ? _BqlTable.Name : e.Table.Name) + '.' + _DatabaseFieldName;
				}

				e.DataType = PXDbType.Int;

				int revision = (int)sender.GetValue(e.Row, _FieldOrdinal);
				var origValue = sender.GetValueOriginal(e.Row, _MonitoredField.Name);
				if (origValue != null && !Equals(origValue, sender.GetValue(e.Row, _MonitoredField.Name)))
				{
					e.DataValue = revision + 1;
					sender.SetValue(e.Row, _FieldOrdinal, revision + 1);
				}
				else
				{
					e.DataValue = revision;
				}
			}
		}
	}
	#endregion

	#region PXDynamicButtonAttribute
	[AttributeUsage(AttributeTargets.Class)]
	public class PXDynamicButtonAttribute : Attribute
	{
		public string[] buttonNames;
		public string[] displayNames;
		public Type TranslationKeyType { get; set; }

		public PXDynamicButtonAttribute(string[] dynamicButtonNames, string[] dynamicButtonDisplayNames)
		{
			buttonNames = dynamicButtonNames;
			displayNames = dynamicButtonDisplayNames;
		}

		public List<PXActionInfo> DynamicActions
		{
			get
			{
				List<PXActionInfo> actions = new List<PXActionInfo>();

				if (buttonNames != null)
				{
					for (int i = 0; i < buttonNames.Length; i++)
					{
						string actionName = buttonNames[i];

						if (!string.IsNullOrEmpty(actionName))
						{
							string displayName = GetActionDisplayName(i);

							PXActionInfo newAction = new PXActionInfo(actionName, displayName);
							actions.Add(newAction);
						}
					}
				}

				return actions;
			}
		}

		private string GetActionDisplayName(int buttonNameIndex)
		{
			string displayName = buttonNames[buttonNameIndex];

			if (displayNames != null && buttonNameIndex <= displayNames.Length - 1 && !string.IsNullOrEmpty(displayNames[buttonNameIndex]))
			{
				displayName = TranslationKeyType == null ? PXMessages.Localize(displayNames[buttonNameIndex]) : PXLocalizer.Localize(displayNames[buttonNameIndex], TranslationKeyType.FullName);
			}

			return displayName;
		}
	}
	#endregion
}

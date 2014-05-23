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
using System.Reflection.Emit;
using System.Text;

namespace PX.Data
{
	public abstract class PXEventSubscriberAttribute : Attribute
	{
		internal PXGraphExtension[] Extensions;
		
		protected PXGraphExtension[] GraphExtensions
		{
			get { return Extensions; }
		}



		public override int GetHashCode()
		{
			return _FieldOrdinal;
		}
		public override bool Equals(object obj)
		{
			return object.ReferenceEquals(this, obj);
		}
		protected Type _BqlTable = null;
		protected string _FieldName = null;
		protected int _FieldOrdinal = -1;
		protected PXEventSubscriberAttribute()
		{
			PXExtensionManager.InitExtensions(this);
		}
		protected PXAttributeLevel _AttributeLevel = PXAttributeLevel.Type;
		public PXAttributeLevel AttributeLevel
		{
			get
			{
				return _AttributeLevel;
			}
		}
		public virtual PXEventSubscriberAttribute Clone(PXAttributeLevel attributeLevel)
		{
			PXEventSubscriberAttribute attr = (PXEventSubscriberAttribute)MemberwiseClone();
			attr._AttributeLevel = attributeLevel;
			return attr;
		}
		public virtual Type BqlTable
		{
			get
			{
				return _BqlTable;
			}
			set
			{
				if (_BqlTable != null && _AttributeLevel != PXAttributeLevel.Type)
				{
					throw new PXException(ErrorMessages.BaseAttributeStateOverride);
				}
				_BqlTable = value;
			}
		}
		protected virtual internal void SetBqlTable(Type bqlTable)
		{
			if (!typeof(PXCacheExtension).IsAssignableFrom(bqlTable))
			{
				_BqlTable = bqlTable;
				Type baseType;
				while ((typeof(IBqlTable).IsAssignableFrom(baseType = _BqlTable.BaseType)
					|| baseType.IsDefined(typeof(PXTableAttribute), false)
					|| baseType.IsDefined(typeof(PXTableNameAttribute), false))
					&& ((_FieldName != null
					&& baseType.GetProperty(_FieldName) != null
					|| !_BqlTable.IsDefined(typeof(PXTableAttribute), false))
					&& !_BqlTable.IsDefined(typeof(PXTableNameAttribute), false)))
				{
					_BqlTable = baseType;
				}
			}
			else
			{
				_BqlTable = bqlTable;
			}
		}
		public virtual string FieldName
		{
			get
			{
				return _FieldName;
			}
			set
			{
				if (_AttributeLevel != PXAttributeLevel.Type)
				{
					throw new PXException(ErrorMessages.BaseAttributeStateOverride);
				}
				_FieldName = value;
			}
		}
		public virtual int FieldOrdinal
		{
			get
			{
				return _FieldOrdinal;
			}
			set
			{
				if (_AttributeLevel != PXAttributeLevel.Type)
				{
					throw new PXException(ErrorMessages.BaseAttributeStateOverride);
				}
				_FieldOrdinal = value;
			}
		}
		public void InvokeCacheAttached(PXCache cache)
		{
			this.CacheAttached(cache);
		}
		public virtual void CacheAttached(PXCache sender)
		{
		}
        public virtual void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
            where ISubscriber : class
		{
            if (this is ISubscriber)
            {
                subscribers.Add(this as ISubscriber);
            }
		}

		public static T CreateInstance<T>( params object[] constructorArgs)
			where T: PXEventSubscriberAttribute
		{
			return (T) CreateInstance(typeof (T), constructorArgs);
		}

		public static PXEventSubscriberAttribute CreateInstance(Type t, params object[] constructorArgs)
		{
			t = PXExtensionManager.GetWrapperType(t);
			if (constructorArgs == null || constructorArgs.Length == 0)
				return (PXEventSubscriberAttribute)Activator.CreateInstance(t);

			var c = t.GetConstructor(constructorArgs.Select(_ => _.GetType()).ToArray());
			return (PXEventSubscriberAttribute)c.Invoke(constructorArgs);
		}


		protected class ObjectRef<T>
		{
			public T Value;

			public ObjectRef()
			{
			}

			public ObjectRef(T defaultValue)
			{
				Value = defaultValue;
			}
		}
	}

	public interface IPXCommandPreparingSubscriber
	{
		void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e);
	}
	public interface IPXRowSelectingSubscriber
	{
		void RowSelecting(PXCache sender, PXRowSelectingEventArgs e);
	}
	public interface IPXRowSelectedSubscriber
	{
		void RowSelected(PXCache sender, PXRowSelectedEventArgs e);
	}
	public interface IPXRowInsertingSubscriber
	{
		void RowInserting(PXCache sender, PXRowInsertingEventArgs e);
	}
	public interface IPXRowInsertedSubscriber
	{
		void RowInserted(PXCache sender, PXRowInsertedEventArgs e);
	}
	public interface IPXRowUpdatingSubscriber
	{
		void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e);
	}
	public interface IPXRowUpdatedSubscriber
	{
		void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e);
	}
	public interface IPXRowDeletingSubscriber
	{
		void RowDeleting(PXCache sender, PXRowDeletingEventArgs e);
	}
	public interface IPXRowDeletedSubscriber
	{
		void RowDeleted(PXCache sender, PXRowDeletedEventArgs e);
	}
	public interface IPXRowPersistingSubscriber
	{
		void RowPersisting(PXCache sender, PXRowPersistingEventArgs e);
	}
	public interface IPXRowPersistedSubscriber
	{
		void RowPersisted(PXCache sender, PXRowPersistedEventArgs e);
	}
	public interface IPXFieldDefaultingSubscriber
	{
		void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e);
	}
	public interface IPXFieldUpdatingSubscriber
	{
		void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e);
	}
	public interface IPXFieldVerifyingSubscriber
	{
		void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e);
	}
	public interface IPXFieldUpdatedSubscriber
	{
		void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e);
	}
	public interface IPXFieldSelectingSubscriber
	{
		void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e);
	}
	public interface IPXExceptionHandlingSubscriber
	{
		void ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e);
	}
	public interface IPXInterfaceField
	{
		string DisplayName
		{
			get;
			set;
		}
		PXUIVisibility Visibility
		{
			get;
			set;
		}
		bool Enabled
		{
			get;
			set;
		}
		bool Visible
		{
			get;
			set;
		}
		string ErrorText
		{
			get;
			set;
		}
		object ErrorValue
		{
			get;
			set;
		}
		int TabOrder
		{
			get;
			set;
		}
		PXCacheRights MapEnableRights
		{
			get;
			set;
		}
		PXCacheRights MapViewRights
		{
			get;
			set;
		}
		bool ViewRights
		{
			get;
		}
		void ForceEnabled();
	}
	public class PXAggregateAttribute : PXEventSubscriberAttribute
	{
		protected List<PXEventSubscriberAttribute> _Attributes;
		public PXAggregateAttribute()
		{
			_Attributes = new List<PXEventSubscriberAttribute>();
			foreach(PXEventSubscriberAttribute attr in this.GetType().GetCustomAttributes(typeof(PXEventSubscriberAttribute), true))
			{
				_Attributes.Add(attr);
			}
		}
		public PXEventSubscriberAttribute[] GetAttributes()
		{
			return _Attributes.ToArray();
		}
		public T GetAttribute<T>() where T : class
		{
			foreach (PXEventSubscriberAttribute attr in _Attributes)
				if (attr is T)
					return attr as T;
			return null;
		}
		public override PXEventSubscriberAttribute Clone(PXAttributeLevel attributeLevel)
		{
			PXAggregateAttribute attr = (PXAggregateAttribute)base.Clone(attributeLevel);
			attr._Attributes = new List<PXEventSubscriberAttribute>();
			foreach (PXEventSubscriberAttribute subscr in _Attributes)
			{
				attr._Attributes.Add(subscr.Clone(attributeLevel));
			}
			return attr;
		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			foreach (PXEventSubscriberAttribute subscr in _Attributes)
			{
				subscr.CacheAttached(sender);
			}
		}
		public override Type BqlTable
		{
			get
			{
				return base.BqlTable;
			}
			set
			{
				base.BqlTable = value;
				foreach (PXEventSubscriberAttribute subscr in _Attributes)
				{
					subscr.BqlTable = value;
				}
			}
		}
		protected internal override void SetBqlTable(Type bqlTable)
		{
			base.SetBqlTable(bqlTable);
			foreach (PXEventSubscriberAttribute subscr in _Attributes)
			{
				subscr.SetBqlTable(bqlTable);
			}
		}
		public override string FieldName
		{
			get
			{
				return base.FieldName;
			}
			set
			{
				base.FieldName = value;
				foreach (PXEventSubscriberAttribute subscr in _Attributes)
				{
					subscr.FieldName = value;
				}
			}
		}
		public override int FieldOrdinal
		{
			get
			{
				return base.FieldOrdinal;
			}
			set
			{
				base.FieldOrdinal = value;
				foreach (PXEventSubscriberAttribute subscr in _Attributes)
				{
					subscr.FieldOrdinal = value;
				}
			}
		}
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			foreach (PXEventSubscriberAttribute subscr in _Attributes)
			{
				subscr.GetSubscriber<ISubscriber>(subscribers);
			}
		}
	}

	public abstract class PXDBInterceptorAttribute : Attribute
	{
		protected static object[] getKeys(PXCache sender, object node)
		{
			object[] ret = new object[sender.Keys.Count];
			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = sender.GetValue(node, sender.Keys[i]);
			}
			return ret;
		}
		public PXDBInterceptorAttribute Child
		{
			get;
			set;
		}
		internal virtual List<string> Keys
		{
			get
			{
				return null;
			}
		}
		public abstract BqlCommand GetRowCommand();
		public abstract BqlCommand GetTableCommand();
		internal virtual BqlCommand GetTableCommand(PXCache sender)
		{
			return GetTableCommand();
		}
		public virtual void CacheAttached(PXCache sender)
		{
		}

		public virtual Type[] GetTables()
		{
			BqlCommand cmd = GetTableCommand();
			return cmd.GetTables();
		}

		public virtual bool PersistInserted(PXCache sender, object row)
		{
			return false;
		}
		public virtual bool PersistUpdated(PXCache sender, object row)
		{
			return false;
		}
		public virtual bool PersistDeleted(PXCache sender, object row)
		{
			return false;
		}
        public virtual object Insert(PXCache sender, object row)
        {
            return sender.Insert(row, true);
        }
        public virtual object Update(PXCache sender, object row)
        {
            return sender.Update(row, true);
        }
        public virtual object Delete(PXCache sender, object row)
        {
            return sender.Delete(row, true);
        }
		public virtual bool CacheSelected
		{
			get
			{
				return true;
			}
		}
    }

	public interface IPXExtensibleTableAttribute
	{
		string[] Keys
		{
			get;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class PXTableAttribute : PXDBInterceptorAttribute
	{
		protected BqlCommand rowselection;
		protected BqlCommand tableselection;
		protected List<string> keys;
		protected Type[] _bypassOnDelete; // tables that should be bypassed on delete operation
		public bool IsOptional
		{
			get;
			set;
		}
		public override void CacheAttached(PXCache sender)
		{
			rowselection = new BqlRowSelection(sender, true, keys);
			tableselection = new BqlRowSelection(sender, false, keys);
			if (keys != null && keys.Count == 1 && sender.Keys.Count == 1)
			{
				sender.CommandPreparingEvents[sender.Keys[0].ToLower()] += KeysCommandPreparing;
			}
		}
		public override BqlCommand GetRowCommand()
		{
			return rowselection;
		}
		public override BqlCommand GetTableCommand()
		{
			return tableselection;
		}
		internal override BqlCommand GetTableCommand(PXCache sender)
		{
			if (tableselection == null)
			{
				tableselection = new BqlRowSelection(sender, false, keys);
			}
			return tableselection;
		}
		public virtual void KeysCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Update || (e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) && 
				e.Table != null && e.Row != null
				)
			{
				Type[] tables = GetTables();
				if (tables[tables.Length - 1] != e.Table &&
					object.Equals(sender.GetValue(e.Row, sender.Keys[0]), e.Value)
					)
				{
					PXCommandPreparingEventArgs.FieldDescription description;
					sender.RaiseCommandPreparing(keys[0], e.Row, sender.GetValue(e.Row, keys[0]), PXDBOperation.Select, e.Table, out description);
					if (description != null && !String.IsNullOrEmpty(description.FieldName))
					{
						e.BqlTable = description.BqlTable;
						e.FieldName = description.FieldName;
						e.DataType = description.DataType;
						e.DataLength = description.DataLength;
						e.DataValue = description.DataValue;
						e.IsRestriction = true;
						e.Cancel = true;
					}
				}
			}
		}
		public PXTableAttribute()
		{
		}
		public PXTableAttribute(params Type[] links)
			: this()
		{
			keys = new List<string>();
			foreach (Type key in links)
			{
				keys.Add(char.ToUpper(key.Name[0]) + key.Name.Substring(1));
			}
		}

		/// <param name="tables">Tables that should be bypassed on delete operation.</param>
		public void BypassOnDelete(params Type[] tables)
		{
			_bypassOnDelete = tables;
		}

		internal override List<string> Keys
		{
			get
			{
				return keys;
			}
		} 
		public override bool PersistInserted(PXCache sender, object row)
		{
			Type[] tables = GetTables();
			List<Type> extensions = sender.GetExtensionTables();
			if (extensions != null)
			{
				extensions = new List<Type>(extensions);
				extensions.AddRange(tables);
				tables = extensions.ToArray();
			}
			if (keys == null)
			{
				keys = new List<string>(sender.Keys);
			}
			List<PXDataFieldAssign>[] pars = new List<PXDataFieldAssign>[tables.Length];
			for (int i = 0; i < tables.Length; i++)
			{
				pars[i] = new List<PXDataFieldAssign>();
			}
			bool audit = false;
			Type table = sender.BqlTable;
			while (!(audit = PXDatabase.AuditRequired(table)) && table.BaseType != typeof(object))
			{
				table = table.BaseType;
			}
			foreach (string field in sender.Fields)
			{
				object val = sender.GetValue(row, field);
				PXCommandPreparingEventArgs.FieldDescription description;
				sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Insert, null, out description);
				if (description != null && !String.IsNullOrEmpty(description.FieldName))
				{
					for (int j = 0; j < tables.Length; j++)
					{
						if (description.FieldName.StartsWith(tables[j].Name + "."))
						{
							PXDataFieldAssign assign = new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue, null);
							if (audit && val != null)
							{
								assign.IsChanged = true;
								assign.NewValue = sender.ValueToString(field, val);
							}
							else assign.IsChanged = false;
							pars[j].Add(assign);
							break;
						}
					}
				}
			}
			try
			{
				pars[tables.Length - 1].Add(PXDataFieldAssign.OperationSwitchAllowed);
				sender.Graph.ProviderInsert(tables[tables.Length - 1], pars[tables.Length - 1].ToArray());
			}
			catch (PXDatabaseException e)
			{
				if (e.ErrorCode == PXDbExceptions.OperationSwitchRequired)
				{
					List<PXDataFieldParam> upd = new List<PXDataFieldParam>();
					foreach (string field in sender.Fields)
					{
						PXCommandPreparingEventArgs.FieldDescription description;
						sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update | PXDBOperation.Second, null, out description);
						if (description != null && !String.IsNullOrEmpty(description.FieldName))
						{
							if (description.FieldName.StartsWith(tables[tables.Length - 1].Name + "."))
							{
								if (description.IsRestriction)
								{
									upd.Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
								}
								else
								{
									upd.Add(new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue));
								}
							}
						}
					}
					sender.Graph.ProviderUpdate(tables[tables.Length - 1], upd.ToArray());
				}
				else
				{
					throw;
				}
			}
			try
			{
				sender.RaiseRowPersisted(row, PXDBOperation.Insert, PXTranStatus.Open, null);
				lock (((System.Collections.ICollection)sender._Originals).SyncRoot)
				{
					sender._Originals.Remove((IBqlTable)row);
				}
			}
			catch (PXRowPersistedException e)
			{
				sender.RaiseExceptionHandling(e.Name, row, e.Value, e);
				throw;
			}
			for (int j = 0; j < tables.Length - 1; j++)
			{
				foreach (string field in keys)
				{
					object val = sender.GetValue(row, field);
					PXCommandPreparingEventArgs.FieldDescription description;
					sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Insert, tables[j], out description);
					if (description == null || String.IsNullOrEmpty(description.FieldName))
					{
						sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Select, tables[j], out description);
					}
					if (description != null && !String.IsNullOrEmpty(description.FieldName))
					{
						PXDataFieldAssign assign = new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue, null);
						if (audit && val != null)
						{
							assign.IsChanged = true;
							assign.NewValue = sender.ValueToString(field, val);
						}
						else assign.IsChanged = false;
						pars[j].Add(assign);
					}
				}
			}
			for (int i = tables.Length - 2; i >= 0; i--)
			{
				try
				{
					pars[i].Add(PXDataFieldAssign.OperationSwitchAllowed);
					sender.Graph.ProviderInsert(tables[i], pars[i].ToArray());
				}
				catch (PXDatabaseException e)
				{
					if (e.ErrorCode == PXDbExceptions.OperationSwitchRequired)
					{
						List<PXDataFieldParam> upd = new List<PXDataFieldParam>();
						foreach (string field in sender.Fields)
						{
							PXCommandPreparingEventArgs.FieldDescription description;
							sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update | PXDBOperation.Second, null, out description);
							if (description != null && !String.IsNullOrEmpty(description.FieldName))
							{
								if (description.FieldName.StartsWith(tables[i].Name + "."))
								{
									if (description.IsRestriction)
									{
										upd.Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
									}
									else
									{
										upd.Add(new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue));
									}
								}
							}
						}
						foreach (string field in keys)
						{
							PXCommandPreparingEventArgs.FieldDescription description;
							sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update, tables[i], out description);
							if (description == null || String.IsNullOrEmpty(description.FieldName))
							{
								sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Select, tables[i], out description);
							}
							if (description != null && !String.IsNullOrEmpty(description.FieldName))
							{
								upd.Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
							}
						}
						sender.Graph.ProviderUpdate(tables[i], upd.ToArray());
					}
					else
					{
						throw;
					}
				}
			}
			return true;
		}
		public override bool PersistUpdated(PXCache sender, object row)
		{
			Type[] tables = GetTables();
			List<Type> extensions = sender.GetExtensionTables();
			if (extensions != null)
			{
				extensions = new List<Type>(extensions);
				extensions.AddRange(tables);
				tables = extensions.ToArray();
			}
			if (keys == null)
			{
				keys = new List<string>(sender.Keys);
			}
			object unchanged = null;
			try
			{
				lock (((System.Collections.ICollection)sender._Originals).SyncRoot)
				{
					BqlTablePair orig;
					if (sender._Originals.TryGetValue((IBqlTable)row, out orig))
					{
						unchanged = orig.Unchanged;
					}
				}
			}
			catch
			{
			}
			bool audit = false;
			Type table = sender.BqlTable;
			while (!(audit = PXDatabase.AuditRequired(table)) && table.BaseType != typeof(object))
			{
				table = table.BaseType;
			}
			List<PXDataFieldParam>[] pars = new List<PXDataFieldParam>[tables.Length];
			for (int i = 0; i < tables.Length; i++)
			{
				pars[i] = new List<PXDataFieldParam>();
			}
			foreach (string field in sender.Fields)
			{
				object val = sender.GetValue(row, field);
				PXCommandPreparingEventArgs.FieldDescription description;
				sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Update, null, out description);
				if (description != null && !String.IsNullOrEmpty(description.FieldName))
				{
					for (int j = 0; j < tables.Length; j++)
					{
						if (description.FieldName.StartsWith(tables[j].Name + "."))
						{
							if (description.IsRestriction)
							{
								object origval;
								if (unchanged != null && description.DataType != PXDbType.Timestamp
									&& sender.Keys.Contains(field) && !object.Equals((origval = sender.GetValue(unchanged, field)), val)
									&& origval != null)
								{
									PXCommandPreparingEventArgs.FieldDescription origdescription;
									sender.RaiseCommandPreparing(field, row, origval, PXDBOperation.Update, null, out origdescription);
									if (origdescription != null && !String.IsNullOrEmpty(origdescription.FieldName))
									{
										PXDataFieldAssign assign = new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue, sender.ValueToString(field, val));
										pars[j].Add(assign);
										pars[j].Add(new PXDataFieldRestrict(origdescription.FieldName, origdescription.DataType, origdescription.DataLength, origdescription.DataValue));
									}
									else
									{
										pars[j].Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
									}
								}
								else
								{
									pars[j].Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
								}
							}
							else
							{
								PXDataFieldAssign assign = new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue, null);
								if (unchanged != null)
								{
									if (assign.IsChanged = !object.Equals(val, sender.GetValue(unchanged, field)))
									{
										assign.NewValue = sender.ValueToString(field, val);
									}
								}
								else assign.IsChanged = false;
								pars[j].Add(assign);
							}
							break;
						}
					}
				}
			}
			for (int j = 0; j < tables.Length - 1; j++)
			{
				foreach (string field in keys)
				{
					PXCommandPreparingEventArgs.FieldDescription description;
					sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update, tables[j], out description);
					if (description == null || String.IsNullOrEmpty(description.FieldName))
					{
						sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Select, tables[j], out description);
					}
					if (description != null && !String.IsNullOrEmpty(description.FieldName))
					{
						pars[j].Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
					}
				}
			}
			for (int i = tables.Length - 1; i >= 0 ; i--)
			{
				bool success;
				try
				{
					pars[i].Add(PXDataFieldRestrict.OperationSwitchAllowed);
					success = sender.Graph.ProviderUpdate(tables[i], pars[i].ToArray());
				}
				catch (PXDatabaseException e)
				{
					if (e.ErrorCode == PXDbExceptions.OperationSwitchRequired)
					{
						List<PXDataFieldAssign> ins = new List<PXDataFieldAssign>();
						foreach (string field in sender.Fields)
						{
							PXCommandPreparingEventArgs.FieldDescription description;
							sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Insert, null, out description);
							if (description != null && !String.IsNullOrEmpty(description.FieldName))
							{
								if (description.FieldName.StartsWith(tables[i].Name + "."))
								{
									ins.Add(new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue));
								}
							}
						}
						if (i < tables.Length - 1)
						{
							foreach (string field in keys)
							{
								PXCommandPreparingEventArgs.FieldDescription description;
								sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update, tables[i], out description);
								if (description == null || String.IsNullOrEmpty(description.FieldName))
								{
									sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Select, tables[i], out description);
								}
								if (description != null && !String.IsNullOrEmpty(description.FieldName))
								{
									ins.Add(new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue));
								}
							}
						}
						sender.Graph.ProviderInsert(tables[i], ins.ToArray());
						success = true;
					}
					else
					{
						throw;
					}
				}
				if (!success)
				{
					if (i == tables.Length - 1)
					{
						throw new PXLockViolationException(tables[i], PXDBOperation.Update, getKeys(sender, row));
					}
					else
					{
						List<PXDataFieldAssign> ins = new List<PXDataFieldAssign>();
						foreach (string field in sender.Fields)
						{
							object val = sender.GetValue(row, field);
							PXCommandPreparingEventArgs.FieldDescription description;
							sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Insert, null, out description);
							if (description != null && !String.IsNullOrEmpty(description.FieldName))
							{
								if (description.FieldName.StartsWith(tables[i].Name + "."))
								{
									PXDataFieldAssign assign = new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue, null);
									if (audit && val != null)
									{
										assign.IsChanged = true;
										assign.NewValue = sender.ValueToString(field, val);
									}
									else assign.IsChanged = false;
									ins.Add(assign);
								}
							}
						}
						foreach (string field in keys)
						{
							PXCommandPreparingEventArgs.FieldDescription description;
							sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update, tables[i], out description);
							if (description == null || String.IsNullOrEmpty(description.FieldName))
							{
								sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Select, tables[i], out description);
							}
							if (description != null && !String.IsNullOrEmpty(description.FieldName))
							{
								ins.Add(new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue));
							}
						}
						try
						{
							ins.Add(PXDataFieldAssign.OperationSwitchAllowed);
							sender.Graph.ProviderInsert(tables[i], ins.ToArray());
						}
						catch (PXDatabaseException e)
						{
							if (e.ErrorCode == PXDbExceptions.OperationSwitchRequired)
							{
								List<PXDataFieldParam> upd = new List<PXDataFieldParam>();
								foreach (string field in sender.Fields)
								{
									PXCommandPreparingEventArgs.FieldDescription description;
									sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update | PXDBOperation.Second, null, out description);
									if (description != null && !String.IsNullOrEmpty(description.FieldName))
									{
										if (description.FieldName.StartsWith(tables[i].Name + "."))
										{
											if (description.IsRestriction)
											{
												upd.Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
											}
											else
											{
												upd.Add(new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue));
											}
										}
									}
								}
								foreach (string field in keys)
								{
									PXCommandPreparingEventArgs.FieldDescription description;
									sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update, tables[i], out description);
									if (description == null || String.IsNullOrEmpty(description.FieldName))
									{
										sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Select, tables[i], out description);
									}
									if (description != null && !String.IsNullOrEmpty(description.FieldName))
									{
										upd.Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
									}
								}
								sender.Graph.ProviderUpdate(tables[i], upd.ToArray());
							}
							else
							{
								throw;
							}
						}
					}
				}
			}
			try
			{
				sender.RaiseRowPersisted(row, PXDBOperation.Update, PXTranStatus.Open, null);
				lock (((System.Collections.ICollection)sender._Originals).SyncRoot)
				{
					sender._Originals.Remove((IBqlTable)row);
				}
			}
			catch (PXRowPersistedException e)
			{
				sender.RaiseExceptionHandling(e.Name, row, e.Value, e);
				throw;
			}
			return true;
		}
		public override bool PersistDeleted(PXCache sender, object row)
		{
			Type[] tables = GetTables();
			List<Type> extensions = sender.GetExtensionTables();
			if (extensions != null)
			{
				extensions = new List<Type>(extensions);
				extensions.AddRange(tables);
				tables = extensions.ToArray();
			}

			if (keys == null)
			{
				keys = new List<string>(sender.Keys);
			}
			List<PXDataFieldRestrict>[] pars = new List<PXDataFieldRestrict>[tables.Length];
			for (int i = 0; i < tables.Length; i++)
			{
				pars[i] = new List<PXDataFieldRestrict>();
			}
			// Preparing restrictions
			foreach (string field in sender.Fields)
			{
				PXCommandPreparingEventArgs.FieldDescription description;
				sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Delete, null, out description);
				if (description != null && !String.IsNullOrEmpty(description.FieldName) && description.IsRestriction)
				{
					for (int j = 0; j < tables.Length; j++)
					{
						if (description.FieldName.StartsWith(tables[j].Name + "."))
						{
							pars[j].Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
							break;
						}
					}
				}
			}
			for (int j = 0; j < tables.Length - 1; j++)
			{
				foreach (string field in keys)
				{
					PXCommandPreparingEventArgs.FieldDescription description;
					sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Delete, tables[j], out description);
					if (description == null || String.IsNullOrEmpty(description.FieldName))
					{
						sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Select, tables[j], out description);
					}
					if (description != null && !String.IsNullOrEmpty(description.FieldName))
					{
						pars[j].Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
					}
				}
			}
			// Removing bypassed tables from the list
			if (_bypassOnDelete != null)
				tables = tables.Except(_bypassOnDelete).ToArray();
			for (int i = 0; i < tables.Length; i++)
			{
				try
				{
					if (!sender.Graph.ProviderDelete(tables[i], pars[i].ToArray()) && i == tables.Length - 1)
					{
						throw new PXLockViolationException(tables[i], PXDBOperation.Delete, getKeys(sender, row));
					}
				}
				catch (PXDatabaseException e)
				{
					if (e.ErrorCode == PXDbExceptions.OperationSwitchRequired)
					{
						List<PXDataFieldAssign> ins = new List<PXDataFieldAssign>();
						foreach (string field in sender.Fields)
						{
							PXCommandPreparingEventArgs.FieldDescription description;
							sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Insert, null, out description);
							if (description != null && !String.IsNullOrEmpty(description.FieldName))
							{
								if (description.FieldName.StartsWith(tables[i].Name + "."))
								{
									ins.Add(new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue));
								}
							}
						}
						if (i < tables.Length - 1)
						{
							foreach (string field in keys)
							{
								PXCommandPreparingEventArgs.FieldDescription description;
								sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update, tables[i], out description);
								if (description == null || String.IsNullOrEmpty(description.FieldName))
								{
									sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Select, tables[i], out description);
								}
								if (description != null && !String.IsNullOrEmpty(description.FieldName))
								{
									ins.Add(new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue));
								}
							}
						}
						sender.Graph.ProviderInsert(tables[i], ins.ToArray());
					}
					else
					{
						throw;
					}
				}
			}
			try
			{
				sender.RaiseRowPersisted(row, PXDBOperation.Delete, PXTranStatus.Open, null);
			}
			catch (PXRowPersistedException e)
			{
				sender.RaiseExceptionHandling(e.Name, row, e.Value, e);
				throw;
			}
			return true;
		}
		private sealed class BqlRowSelection : BqlCommand, IPXExtensibleTableAttribute
		{
			private void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e, Type table)
			{
				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select)
				{
					e.FieldName = (e.Table == null ? table.Name : e.Table.Name) + "." + this.links[0];
				}
			}
			private void RowSelecting(PXCache sender, PXRowSelectingEventArgs e, Type table)
			{
				if (!object.ReferenceEquals(sender, attributecache))
					return;
				if (e.Record != null && !(e.Record is PXDataRecordMap) && e.Record.IsDBNull(e.Position))
				{
					using (new PXConnectionScope())
					{
						string fieldProcessed = null;
						foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(e.Row, null))
						{
							if (fieldProcessed == attr.FieldName)
							{
								continue;
							}
							if (attr.BqlTable == table)
							{
								object newValue;
								if (sender.RaiseFieldDefaulting(attr.FieldName, e.Row, out newValue))
								{
									sender.RaiseFieldUpdating(attr.FieldName, e.Row, ref newValue);
								}
								sender.SetValue(e.Row, attr.FieldName, newValue);
								fieldProcessed = attr.FieldName;
							}
						}
					}
				}
				e.Position++;
			}
			string[] IPXExtensibleTableAttribute.Keys
			{
				get
				{
					return links.ToArray();
				}
			}
			public BqlRowSelection(PXCache cache, bool single, List<string> links)
			{
				this.attributecache = cache;
				this.single = single;
				this.alias = cache.GetItemType();
				if (links != null)
				{
					this.links = new List<string>(links);
				}
				else
				{
					this.links = new List<string>(cache.Keys);
				}
				this.tables = new List<Type>();
				Type table = alias;
				while (table != typeof(object))
				{
					if ((table.BaseType == typeof(object)
						|| !typeof(IBqlTable).IsAssignableFrom(table.BaseType))
						&& typeof(IBqlTable).IsAssignableFrom(table)
						|| table.IsDefined(typeof(PXTableAttribute), false))
					{
						this.tables.Add(table);
						if (table.IsDefined(typeof(PXTableAttribute), false))
						{
							foreach (PXTableAttribute attr in table.GetCustomAttributes(typeof(PXTableAttribute), false))
							{
								if (attr.IsOptional)
								{
									this.optional.Add(table);
								}
							}
						}
					}
					table = table.BaseType;
				}
				List<Type> extensions = cache.GetExtensionTables();
				if (extensions != null)
				{
					foreach (Type ext in extensions)
					{
						if (ext.IsDefined(typeof(PXTableAttribute), false))
						{
							foreach (PXTableAttribute attr in ext.GetCustomAttributes(typeof(PXTableAttribute), false))
							{
								if (attr.IsOptional)
								{
									this.optional.Add(ext);
								}
							}
						}
					}
				}
				if (single && optional.Count > 0 && this.links.Count > 0)
				{
					foreach (Type t in optional)
					{
						string fn = t.Name + "_" + this.links[0];
						if (!cache.Fields.Contains(fn))
						{
							cache.Fields.Add(fn);
							cache.Graph.CommandPreparing.AddHandler(cache.GetItemType(), fn, (sender, e) => CommandPreparing(sender, e, t));
							cache.Graph.RowSelecting.AddHandler(cache.GetItemType(), (sender, e) => RowSelecting(sender, e, t));
						}
					}
				}
			}
			private List<Type> tables;
			private List<Type> optional = new List<Type>();
			private List<string> links;
			private Type alias;
			private bool single;
			private PXCache attributecache;
			public override void Parse(PXGraph graph, List<IBqlParameter> pars, List<Type> tables, List<Type> fields, List<IBqlSortColumn> sortColumns, StringBuilder text, Selection selection)
			{
				if (tables != null)
				{
					if (single)
					{
						tables.Add(alias);
					}
					else
					{
						tables.AddRange(this.tables);
					}
				}
				if (graph != null && text != null)
				{
					if (single)
					{
						text.Append(" FROM (SELECT ");
					}
					bool firstfield = true;
					PXCache cache = graph.Caches[alias];
					var sqlDialect = PXDatabase.Provider.SqlDialect;
					foreach (string field in cache.Fields)
					{
						PXCommandPreparingEventArgs.FieldDescription description;
						cache.RaiseCommandPreparing(field, null, null, PXDBOperation.Select, null, out description);
						if (description != null && !String.IsNullOrEmpty(description.FieldName))
						{
							string fieldName;
							if (!description.FieldName.StartsWith("(")
								&& !description.FieldName.EndsWith(")")
								&& description.BqlTable != null && !description.BqlTable.IsAssignableFrom(alias)
								&& (!typeof(PXCacheExtension).IsAssignableFrom(description.BqlTable) || !description.BqlTable.BaseType.IsGenericType || !description.BqlTable.BaseType.GetGenericArguments().Last().IsAssignableFrom(alias)))
							{
								fieldName = BqlCommand.Null;
							}
							else
							{
								fieldName = description.FieldName;
							}
							if (single)
							{
								selection.Add(alias.Name + "." + field);
								if (!firstfield)
								{
									text.Append(", ");
								}
								else
								{
									firstfield = false;
								}
								text.Append(fieldName);
								text.Append(" AS ");
								text.Append(sqlDialect.quoteDbIdentifier(field));
							}
							else
							{
								selection.Add(fieldName);
							}
						}
					}
					text.Append(" FROM ");
					List<Type> bound = new List<Type>();
					for (int i = 0; i < this.tables.Count; i++)
					{
						if (i > 0)
						{
							if (!optional.Contains(this.tables[i]))
							{
								text.Append(" CROSS JOIN ");
							}
							else
							{
								text.Append(" LEFT JOIN ");
							}
						}
						text.Append(this.tables[i].Name);
						text.Append(" ");
						text.Append(this.tables[i].Name);
						bound.Add(this.tables[i]);
						if (i > 0 && optional.Contains(this.tables[i]))
						{
							if (this.links.Count > 0)
							{
								AppendLinks(bound, cache, text, " ON ");
							}
							else
							{
								text.Append(" ON 1 = 1 ");
							}
							bound.Remove(this.tables[i]);
						}
					}
					List<Type> extensions = cache.GetExtensionTables();
					if (extensions != null)
					{
						foreach (Type ext in extensions)
						{
							if (!optional.Contains(ext))
							{
								text.Append(" CROSS JOIN ");
							}
							else
							{
								text.Append(" LEFT JOIN ");
							}
							text.Append(ext.Name);
							text.Append(" ");
							text.Append(ext.Name);
							bound.Add(ext);
							if (optional.Contains(ext))
							{
								if (this.links.Count > 0)
								{
									AppendLinks(bound, cache, text, " ON ");
								}
								else
								{
									text.Append(" ON 1 = 1 ");
								}
								bound.Remove(ext);
							}
						}
					}
					if (bound.Count > 1)
					{
						AppendLinks(bound, cache, text, " WHERE ");
					}
					if (single)
					{
						text.Append(") ");
						text.Append(alias.Name);
						bool first = true;
						for (int i = 0; i < cache.Keys.Count; i++)
						{
							if (first)
							{
								text.Append(" WHERE ");
								first = false;
							}
							else
							{
								text.Append(" AND ");
							}
							text.Append(alias.Name + "." + cache.Keys[i]);
							text.Append(" = ");
							text.Append(PXDatabase.CreateParameterName(i));
						}
					}
				}
			}
			private void AppendLinks(List<Type> bound, PXCache cache, StringBuilder text, string first)
			{
				for (int i = 0; i < links.Count; i++)
				{
					for (int j = 0; j < bound.Count; j++)
					{
						PXCommandPreparingEventArgs.FieldDescription pdescription;
						cache.RaiseCommandPreparing(links[i], null, null, PXDBOperation.Select, bound[j], out pdescription);
						if (pdescription != null && !String.IsNullOrEmpty(pdescription.FieldName))
						{
							for (int k = j + 1; k < bound.Count; k++)
							{
								PXCommandPreparingEventArgs.FieldDescription description;
								cache.RaiseCommandPreparing(links[i], null, null, PXDBOperation.Select, bound[k], out description);
								if (description != null && !String.IsNullOrEmpty(description.FieldName))
								{
									if (first != null)
									{

										text.Append(first);
										first = null;
									}
									else
									{
										text.Append(" AND ");
									}
									text.Append(pdescription.FieldName);
									text.Append(" = ");
									text.Append(description.FieldName);
								}
							}
						}
					}
				}
			}
			public override void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
			{
			}
			public override BqlCommand OrderByNew<newOrderBy>()
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand OrderByNew(Type newOrderBy)
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereAnd<where>()
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereAnd(Type where)
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereNew<newWhere>()
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereNew(Type newWhere)
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereNot()
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereOr<where>()
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereOr(Type where)
			{
				throw new PXException("The method or operation is not implemented.");
			}
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class PXOfflineAttribute : PXDBInterceptorAttribute
	{
		protected class int0 : Constant<int>
		{
			public int0()
				: base(0)
			{
			}
		}
		protected BqlCommand _Command;
		protected List<object> _Inserted;
		protected internal virtual List<object> Inserted
		{
			get
			{
				return _Inserted;
			}
		}
		protected List<object> _Updated;
		protected internal virtual List<object> Updated
		{
			get
			{
				return _Updated;
			}
		}
		protected List<object> _Deleted;
		protected internal virtual List<object> Deleted
		{
			get
			{
				return _Deleted;
			}
		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_Command = BqlCommand.CreateInstance(typeof(Select<,>), sender.BqlTable, typeof(Where<int0, NotEqual<int0>>));
		}
		public override BqlCommand GetRowCommand()
		{
			return _Command;
		}
		public override BqlCommand GetTableCommand()
		{
			return _Command;
		}
		public override bool PersistDeleted(PXCache sender, object row)
		{
			sender.SetStatus(row, PXEntryStatus.Notchanged);
			return true;
		}
		public override bool PersistInserted(PXCache sender, object row)
		{
			sender.SetStatus(row, PXEntryStatus.Notchanged);
			return true;
		}
		public override bool PersistUpdated(PXCache sender, object row)
		{
			sender.SetStatus(row, PXEntryStatus.Notchanged);
			return true;
		}
		public override object Delete(PXCache sender, object row)
		{
			sender.Current = null;
			if (sender.Graph.Defaults.ContainsKey(sender.GetItemType()))
			{
				sender.Graph.Defaults.Remove(sender.GetItemType());
			}
			if (_Deleted == null)
			{
				_Deleted = new List<object>();
			}
			_Deleted.Add(row);
			return row;
		}
		public override object Insert(PXCache sender, object row)
		{
			sender.Current = null;
			sender.Graph.Defaults[sender.GetItemType()] = delegate() { return row; };
			if (_Inserted == null)
			{
				_Inserted = new List<object>();
			}
			_Inserted.Add(row);
			return row;
		}
		public override object Update(PXCache sender, object row)
		{
			sender.Current = null;
			sender.Graph.Defaults[sender.GetItemType()] = delegate() { return row; };
			if (_Updated == null)
			{
				_Updated = new List<object>();
			}
			_Updated.Add(row);
			return row;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class PXProjectionAttribute : PXDBInterceptorAttribute
	{
		protected Type _select;
		internal Type Select
		{
			get { return _select; }
		}
		protected Type[] _tables = null;
		protected bool _persistent;
		public bool Persistent
		{
			get
			{
				return _persistent;
			}
			set
			{
				_persistent = value;
			}
		}
		
		public PXProjectionAttribute(Type select)
		{
			_select = select;
		}

		public PXProjectionAttribute(Type select, Type[] persistent)
			: this(select)
		{
			_tables = persistent;
			Persistent = true;
		}

		protected BqlCommand rowselection;
		protected BqlCommand tableselection;

		public override Type[] GetTables()
		{
			Type[] tables = base.GetTables();
			return Array.FindAll(tables, a => _tables == null || Array.IndexOf(_tables, a) >= 0);
		}

		public override void CacheAttached(PXCache sender)
		{
			tableselection = (BqlCommand)Activator.CreateInstance(_select);
			Type where = null;
			foreach (Type key in sender.BqlKeys)
			{
				@where = @where == null 
					? BqlCommand.Compose(typeof(Where<,>), key, typeof(Equal<>), typeof(Required<>), key) 
					: BqlCommand.Compose(typeof(Where2<,>), @where, typeof(And<>), typeof(Where<,>), key, typeof(Equal<>), typeof(Required<>), key);
			}
			rowselection = @where == null 
				? BqlCommand.CreateInstance(typeof(Select<>), sender.GetItemType()) 
				: BqlCommand.CreateInstance(typeof(Select<,>), sender.GetItemType(), @where);
			
			if (Child == null) return;

			tableselection = new BqlRowSelection(sender, Child, tableselection);
			rowselection = new BqlRowSelection(sender, Child, rowselection);
		}
		protected class BqlRowSelection : BqlCommand, IPXExtensibleTableAttribute
		{
			protected BqlCommand _Command;
			protected string[] _Links;
			protected Type _Alias;
			protected PXDBInterceptorAttribute _Child;
			string[] IPXExtensibleTableAttribute.Keys
			{
				get
				{
					return _Links;
				}
			}
			public BqlRowSelection(PXCache cache, PXDBInterceptorAttribute child, BqlCommand command)
			{
				_Child = child;
				_Command = command;
				_Alias = cache.GetItemType();
				BqlCommand cmd = _Child.GetTableCommand(cache);
				if (cmd is IPXExtensibleTableAttribute)
				{
					_Links = ((IPXExtensibleTableAttribute)cmd).Keys;
				}
				if (_Links == null)
				{
					_Links = cache.Keys.ToArray();
				}
			}
			public override void Parse(PXGraph graph, List<IBqlParameter> pars, List<Type> tables, List<Type> fields, List<IBqlSortColumn> sortColumns, StringBuilder text, Selection selection)
			{
				if (graph != null && text != null && _Child != null)
				{
					PXCache cache = graph.Caches[_Alias];
					BqlCommand cmd = cache.BqlSelect;
					cache.BqlSelect = _Child.GetTableCommand(cache);
					cache.BypassCalced = true;
					_Command.Parse(graph, pars, tables, fields, sortColumns, text, selection);
					cache.BqlSelect = cmd;
					cache.BypassCalced = false;
				}
				else
				{
					_Command.Parse(graph, pars, tables, fields, sortColumns, text, selection);
				}
			}
			public override void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
			{
				Verify(cache, item, pars, ref result, ref value);
			}
			public override BqlCommand OrderByNew<newOrderBy>()
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand OrderByNew(Type newOrderBy)
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereAnd<where>()
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereAnd(Type where)
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereNew<newWhere>()
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereNew(Type newWhere)
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereNot()
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereOr<where>()
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereOr(Type where)
			{
				throw new PXException("The method or operation is not implemented.");
			}
		}
		public override BqlCommand GetRowCommand()
		{
			return rowselection;
		}
		public override BqlCommand GetTableCommand()
		{
			return tableselection;
		}
		public override bool PersistInserted(PXCache sender, object row)
		{
			if (!_persistent)
			{
				return base.PersistInserted(sender, row);
			}
			Type[] tables = GetTables();
			List<PXDataFieldAssign>[] pars = new List<PXDataFieldAssign>[tables.Length];
			List<string>[] keys = new List<string>[tables.Length];
			for (int i = 0; i < tables.Length; i++)
			{
				pars[i] = new List<PXDataFieldAssign>();
				keys[i] = new List<string>();
			}
			int length = tables.Length;
			for (int i = 0; i < length; i++)
			{
				Type t = tables[i];
				PXCache tc = sender.Graph.Caches[tables[i]];
				tables[i] = tc.BqlTable;
				List<Type> extensions = tc.GetExtensionTables();
				if (extensions != null)
				{
					int oldlength = extensions.Count;
					extensions = new List<Type>(extensions);
					extensions.InsertRange(0, tables);
					tables = extensions.ToArray();
					Array.Resize<List<string>>(ref keys, extensions.Count);
					Array.Resize<List<PXDataFieldAssign>>(ref pars, extensions.Count);
					if (tc.BqlSelect is IPXExtensibleTableAttribute)
					{
						for (int j = keys.Length - oldlength; j < keys.Length; j++)
						{
							keys[j] = new List<string>(((IPXExtensibleTableAttribute)tc.BqlSelect).Keys);
							pars[j] = new List<PXDataFieldAssign>();
						}
					}
					else
					{
						for (int j = keys.Length - oldlength; j < keys.Length; j++)
						{
							keys[j] = new List<string>(tc.Keys);
							pars[j] = new List<PXDataFieldAssign>();
						}
					}
				}
			}
			bool audit = false;
			Type table = sender.BqlTable;
			while (!(audit = PXDatabase.AuditRequired(table)) && table.BaseType != typeof(object))
			{
				table = table.BaseType;
			}
			foreach (string field in sender.Fields)
			{
				object val = sender.GetValue(row, field);
				PXCommandPreparingEventArgs.FieldDescription description;
				sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Insert, null, out description);
				if (description != null && !String.IsNullOrEmpty(description.FieldName))
				{
					for (int j = 0; j < tables.Length; j++)
					{
						if (description.FieldName.StartsWith(tables[j].Name + "."))
						{
							if (pars[j] != null)
							{
								if (j > 0 && description.IsRestriction)
								{
									if (description.DataValue == null)
									{
										pars[j] = null;
										for (int k = length; k < tables.Length; k++)
										{
											if (tables[k].BaseType.GetGenericArguments()[tables[k].BaseType.GetGenericArguments().Length - 1].IsAssignableFrom(tables[j]))
											{
												pars[k] = null;
											}
										}
									}
									else
									{
										keys[j].Add(field);
									}
								}
								else
								{
									PXDataFieldAssign assign = new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue, null);
									if (audit && val != null)
									{
										assign.IsChanged = true;
										assign.NewValue = sender.ValueToString(field, val);
									}
									else assign.IsChanged = false;
									pars[j].Add(assign);
								}
							}
							break;
						}
					}
				}
			}
			try
			{
				pars[0].Add(PXDataFieldAssign.OperationSwitchAllowed);
				sender.Graph.ProviderInsert(tables[0], pars[0].ToArray());
			}
			catch (PXDatabaseException e)
			{
				if (e.ErrorCode == PXDbExceptions.OperationSwitchRequired)
				{
					List<PXDataFieldParam> upd = new List<PXDataFieldParam>();
					foreach (string field in sender.Fields)
					{
						PXCommandPreparingEventArgs.FieldDescription description;
						sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update | PXDBOperation.Second, null, out description);
						if (description != null && !String.IsNullOrEmpty(description.FieldName))
						{
							if (description.FieldName.StartsWith(tables[0].Name + "."))
							{
								if (description.IsRestriction)
								{
									upd.Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
								}
								else
								{
									upd.Add(new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue));
								}
							}
						}
					}
					sender.Graph.ProviderUpdate(tables[0], upd.ToArray());
				}
				else
				{
					throw;
				}
			}
			try
			{
				sender.RaiseRowPersisted(row, PXDBOperation.Insert, PXTranStatus.Open, null);
				lock (((System.Collections.ICollection)sender._Originals).SyncRoot)
				{
					sender._Originals.Remove((IBqlTable)row);
				}
			}
			catch (PXRowPersistedException e)
			{
				sender.RaiseExceptionHandling(e.Name, row, e.Value, e);
				throw;
			}
			for (int i = 1; i < tables.Length; i++)
			{
				if (pars[i] != null && pars[i].Count > 0)
				{
					try
					{
						foreach (string field in keys[i])
						{
							object val = sender.GetValue(row, field);
							PXCommandPreparingEventArgs.FieldDescription description;
							if (i < length)
							{
								sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Insert, null, out description);
							}
							else
							{
								sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Update, tables[i], out description);
							}
							if (description != null && !String.IsNullOrEmpty(description.FieldName))
							{
								PXDataFieldAssign assign = new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue, null);
								if (audit && val != null)
								{
									assign.IsChanged = true;
									assign.NewValue = sender.ValueToString(field, val);
								}
								else assign.IsChanged = false;
								pars[i].Add(assign);
							}
						}

						pars[i].Add(PXDataFieldAssign.OperationSwitchAllowed);
						sender.Graph.ProviderInsert(tables[i], pars[i].ToArray());
					}
					catch (PXDatabaseException e)
					{
						if (e.ErrorCode == PXDbExceptions.OperationSwitchRequired)
						{
							List<PXDataFieldParam> upd = new List<PXDataFieldParam>();
							foreach (string field in sender.Fields)
							{
								PXCommandPreparingEventArgs.FieldDescription description;
								sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Update | PXDBOperation.Second, null, out description);
								if (description != null && !String.IsNullOrEmpty(description.FieldName))
								{
									for (int j = 0; j < tables.Length; j++)
									{
										if (description.FieldName.StartsWith(tables[j].Name + "."))
										{
											if (description.IsRestriction)
											{
												upd.Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
											}
											else
											{
												upd.Add(new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue));
											}
											break;
										}
									}
								}
							}
							sender.Graph.ProviderUpdate(tables[0], upd.ToArray());
						}
						else
						{
							throw;
						}
					}
				}
			}
			return true;
		}
		public override bool PersistUpdated(PXCache sender, object row)
		{
			if (!_persistent)
			{
				return base.PersistUpdated(sender, row);
			}
			Type[] tables = GetTables();
			string[][] links = new string[tables.Length][];
			List<PXDataFieldParam>[] pars = new List<PXDataFieldParam>[tables.Length];
			for (int i = 0; i < tables.Length; i++)
			{
				pars[i] = new List<PXDataFieldParam>();
			}
			int length = tables.Length;
			for (int i = 0; i < length; i++)
			{
				Type t = tables[i];
				PXCache tc = sender.Graph.Caches[tables[i]];
				tables[i] = tc.BqlTable;
				List<Type> extensions = tc.GetExtensionTables();
				if (extensions != null)
				{
					int oldlength = extensions.Count;
					extensions = new List<Type>(extensions);
					extensions.InsertRange(0, tables);
					tables = extensions.ToArray();
					Array.Resize<string[]>(ref links, extensions.Count);
					Array.Resize<List<PXDataFieldParam>>(ref pars, extensions.Count);
					if (tc.BqlSelect is IPXExtensibleTableAttribute)
					{
						for (int j = pars.Length - oldlength; j < pars.Length; j++)
						{
							pars[j] = new List<PXDataFieldParam>();
							links[j] = ((IPXExtensibleTableAttribute)tc.BqlSelect).Keys;
						}
					}
					else
					{
						for (int j = pars.Length - oldlength; j < pars.Length; j++)
						{
							pars[j] = new List<PXDataFieldParam>();
							links[j] = tc.Keys.ToArray();
						}
					}
				}
			}
			object unchanged = null;
			try
			{
				lock (((System.Collections.ICollection)sender._Originals).SyncRoot)
				{
					BqlTablePair orig;
					if (sender._Originals.TryGetValue((IBqlTable)row, out orig))
					{
						unchanged = orig.Unchanged;
					}
				}
			}
			catch
			{
			}
			bool audit = false;
			Type table = sender.BqlTable;
			while (!(audit = PXDatabase.AuditRequired(table)) && table.BaseType != typeof(object))
			{
				table = table.BaseType;
			}
			foreach (string field in sender.Fields)
			{
				object val = sender.GetValue(row, field);
				PXCommandPreparingEventArgs.FieldDescription description;
				sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Update, null, out description);
				if (description != null && !String.IsNullOrEmpty(description.FieldName))
				{
					for (int j = 0; j < tables.Length; j++)
					{
						if (description.FieldName.StartsWith(tables[j].Name + "."))
						{
							if (description.IsRestriction)
							{
								if (pars[j] != null)
								{
									if (j > 0 && description.DataValue == null)
									{
										pars[j] = null;
									}
									else
									{
										pars[j].Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
									}
								}
							}
							else if (pars[j] != null)
							{
								PXDataFieldAssign assign = new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue, null);
								if (unchanged != null)
								{
									if (assign.IsChanged = !object.Equals(val, sender.GetValue(unchanged, field)))
									{
										assign.NewValue = sender.ValueToString(field, val);
									}
								}
								else assign.IsChanged = false;
								pars[j].Add(assign);
							}
							break;
						}
					}
				}
			}
			for (int i = length; i < tables.Length; i++)
			{
				if (pars[i] != null && pars[i].Count > 0)
				{
					foreach (string field in links[i])
					{
						object val = sender.GetValue(row, field);
						PXCommandPreparingEventArgs.FieldDescription description;
						sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Update, tables[i], out description);
						if (description != null && !String.IsNullOrEmpty(description.FieldName))
						{
							if (description.DataValue == null)
							{
								pars[i] = null;
								break;
							}
							PXDataFieldRestrict p = new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue);
							pars[i].Add(p);
						}
					}
				}
			}
			for (int i = 0; i < tables.Length; i++)
			{
				if (pars[i] == null || pars[i].Count == 0)
				{
					continue;
				}
				bool success;
				try
				{
					pars[i].Add(PXDataFieldRestrict.OperationSwitchAllowed);
					success = sender.Graph.ProviderUpdate(tables[i], pars[i].ToArray());
				}
				catch (PXDatabaseException e)
				{
					if (e.ErrorCode == PXDbExceptions.OperationSwitchRequired)
					{
						List<PXDataFieldAssign> ins = new List<PXDataFieldAssign>();
						foreach (string field in sender.Fields)
						{
							PXCommandPreparingEventArgs.FieldDescription description;
							sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Insert, null, out description);
							if (description != null && !String.IsNullOrEmpty(description.FieldName))
							{
								if (description.FieldName.StartsWith(tables[i].Name + "."))
								{
									ins.Add(new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue));
								}
							}
						}
						sender.Graph.ProviderInsert(tables[i], ins.ToArray());
						success = true;
					}
					else
					{
						throw;
					}
				}
				if (!success)
				{
					if (i == 0)
					{
						throw new PXLockViolationException(tables[i], PXDBOperation.Update, getKeys(sender, row));
					}
					else
					{
						List<PXDataFieldAssign> ins = new List<PXDataFieldAssign>();
						foreach (string field in sender.Fields)
						{
							object val = sender.GetValue(row, field);
							PXCommandPreparingEventArgs.FieldDescription description;
							sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Insert, null, out description);
							if (description != null && !String.IsNullOrEmpty(description.FieldName))
							{
								if (description.FieldName.StartsWith(tables[i].Name + "."))
								{
									if (ins != null)
									{
										if (description.IsRestriction && description.DataValue == null)
										{
											ins = null;
										}
										else
										{
											PXDataFieldAssign assign = new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue, null);
											if (audit && val != null)
											{
												assign.IsChanged = true;
												assign.NewValue = sender.ValueToString(field, val);
											}
											else assign.IsChanged = false;
											ins.Add(assign);
										}
									}
								}
							}
						}
						if (ins != null && i >= length && links[i] != null)
						{
							foreach (string field in links[i])
							{
								object val = sender.GetValue(row, field);
								PXCommandPreparingEventArgs.FieldDescription description;
								sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Update, tables[i], out description);
								if (description != null && !String.IsNullOrEmpty(description.FieldName))
								{
									if (description.DataValue == null)
									{
										ins = null;
										break;
									}
									PXDataFieldAssign assign = new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue);
									ins.Add(assign);
								}
							}
						}
						if (ins != null)
						{
							sender.Graph.ProviderInsert(tables[i], ins.ToArray());
						}
					}
				}
			}
			try
			{
				sender.RaiseRowPersisted(row, PXDBOperation.Update, PXTranStatus.Open, null);
				lock (((System.Collections.ICollection)sender._Originals).SyncRoot)
				{
					sender._Originals.Remove((IBqlTable)row);
				}
			}
			catch (PXRowPersistedException e)
			{
				sender.RaiseExceptionHandling(e.Name, row, e.Value, e);
				throw;
			}
			return true;
		}
		public override bool PersistDeleted(PXCache sender, object row)
		{
			if (!_persistent)
			{
				return base.PersistDeleted(sender, row);
			}
			Type[] tables = GetTables();
			string[][] links = new string[tables.Length][];
			List<PXDataFieldRestrict>[] pars = new List<PXDataFieldRestrict>[tables.Length];
			for (int i = 0; i < tables.Length; i++)
			{
				pars[i] = new List<PXDataFieldRestrict>();
			}
			int length = tables.Length;
			for (int i = 0; i < length; i++)
			{
				Type t = tables[i];
				PXCache tc = sender.Graph.Caches[tables[i]];
				tables[i] = tc.BqlTable;
				List<Type> extensions = tc.GetExtensionTables();
				if (extensions != null)
				{
					int oldlength = extensions.Count;
					extensions = new List<Type>(extensions);
					extensions.InsertRange(0, tables);
					tables = extensions.ToArray();
					Array.Resize<string[]>(ref links, extensions.Count);
					Array.Resize<List<PXDataFieldRestrict>>(ref pars, extensions.Count);
					if (tc.BqlSelect is IPXExtensibleTableAttribute)
					{
						for (int j = pars.Length - oldlength; j < pars.Length; j++)
						{
							pars[j] = new List<PXDataFieldRestrict>();
							links[j] = ((IPXExtensibleTableAttribute)tc.BqlSelect).Keys;
						}
					}
					else
					{
						for (int j = pars.Length - oldlength; j < pars.Length; j++)
						{
							pars[j] = new List<PXDataFieldRestrict>();
							links[j] = tc.Keys.ToArray();
						}
					}
				}
			}
			foreach (string field in sender.Fields)
			{
				PXCommandPreparingEventArgs.FieldDescription description;
				sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Delete, null, out description);
				if (description != null && !String.IsNullOrEmpty(description.FieldName) && description.IsRestriction)
				{
					for (int j = 0; j < tables.Length; j++)
					{
						if (description.FieldName.StartsWith(tables[j].Name + "."))
						{
							if (pars[j] != null)
							{
								if (j > 0 && description.DataValue == null)
								{
									pars[j] = null;
								}
								else
								{
									pars[j].Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
								}
							}
							break;
						}
					}
				}
			}
			for (int i = length; i < tables.Length; i++)
			{
				if (pars[i] != null)
				{
					foreach (string field in links[i])
					{
						object val = sender.GetValue(row, field);
						PXCommandPreparingEventArgs.FieldDescription description;
						sender.RaiseCommandPreparing(field, row, val, PXDBOperation.Update, tables[i], out description);
						if (description != null && !String.IsNullOrEmpty(description.FieldName))
						{
							if (description.DataValue == null)
							{
								pars[i] = null;
								break;
							}
							PXDataFieldRestrict p = new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue);
							pars[i].Add(p);
						}
					}
				}
			}
			for (int i = tables.Length - 1; i >= 0; i--)
			{
				if (pars[i] == null || pars[i].Count == 0)
				{
					continue;
				}
				try
				{
					if (!sender.Graph.ProviderDelete(tables[i], pars[i].ToArray()) && i == 0)
					{
						throw new PXLockViolationException(tables[i], PXDBOperation.Delete, getKeys(sender, row));
					}
				}
				catch (PXDatabaseException e)
				{
					if (e.ErrorCode == PXDbExceptions.OperationSwitchRequired)
					{
						List<PXDataFieldAssign> ins = new List<PXDataFieldAssign>();
						foreach (string field in sender.Fields)
						{
							PXCommandPreparingEventArgs.FieldDescription description;
							sender.RaiseCommandPreparing(field, row, sender.GetValue(row, field), PXDBOperation.Insert, null, out description);
							if (description != null && !String.IsNullOrEmpty(description.FieldName))
							{
								if (description.FieldName.StartsWith(tables[i].Name + "."))
								{
									ins.Add(new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue));
								}
							}
						}
						sender.Graph.ProviderInsert(tables[i], ins.ToArray());
					}
					else
					{
						throw;
					}
				}
			}
			try
			{
				sender.RaiseRowPersisted(row, PXDBOperation.Delete, PXTranStatus.Open, null);
			}
			catch (PXRowPersistedException e)
			{
				sender.RaiseExceptionHandling(e.Name, row, e.Value, e);
				throw;
			}
			return true;
		}		
	}

    [AttributeUsage(AttributeTargets.Class)]
    public class PXAccumulatorAttribute : PXDBInterceptorAttribute
    {
        protected BqlCommand rowselection;
        protected Type[] _Source;
        protected Type[] _Destination;
        protected bool _SingleRecord;
		public override bool CacheSelected
		{
			get
			{
				return false;
			}
		}
        public virtual bool SingleRecord
        {
            get
            {
                return _SingleRecord;
            }
            set
            {
                _SingleRecord = value;
            }
        }
        public PXAccumulatorAttribute()
        {
        }
        public PXAccumulatorAttribute(Type[] source, Type[] destination)
        {
            _Source = source;
            _Destination = destination;
        }
        public override void CacheAttached(PXCache sender)
        {
            Type[] pars;
            if (sender.BqlKeys.Count == 0)
            {
                pars = new Type[2];
                pars[0] = typeof(Select<>);
                pars[1] = sender.GetItemType();
            }
            else if (sender.BqlKeys.Count == 1)
            {
                pars = new Type[7];
                pars[0] = typeof(Select<,>);
                pars[1] = sender.GetItemType();
                pars[2] = typeof(Where<,>);
                pars[3] = sender.BqlKeys[0];
                pars[4] = typeof(Equal<>);
                pars[5] = typeof(Required<>);
                pars[6] = sender.BqlKeys[0];
            }
            else
            {
                pars = new Type[7 + (sender.BqlKeys.Count - 1) * 5];
                pars[0] = typeof(Select<,>);
                pars[1] = sender.GetItemType();
                pars[2] = typeof(Where<,,>);
                for (int i = 0; i < sender.BqlKeys.Count; i++)
                {
                    pars[3 + 5 * i] = sender.BqlKeys[i];
                    pars[3 + 5 * i + 1] = typeof(Equal<>);
                    pars[3 + 5 * i + 2] = typeof(Required<>);
                    pars[3 + 5 * i + 3] = sender.BqlKeys[i];
                    if (i < sender.BqlKeys.Count - 2)
                    {
                        pars[3 + 5 * i + 4] = typeof(And<,,>);
                    }
                    else if (i < sender.BqlKeys.Count - 1)
                    {
                        pars[3 + 5 * i + 4] = typeof(And<,>);
                    }
                }
            }
            rowselection = BqlCommand.CreateInstance(pars);
        }
        public override BqlCommand GetRowCommand()
        {
            return rowselection;
        }
        public override BqlCommand GetTableCommand()
        {
            return null;
        }
        protected virtual bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            foreach (string descr in sender.Fields)
            {
                columns.Add(descr);
                object val = sender.GetValue(row, descr);
                if (val == null)
                {
                    continue;
                }
                int idx = sender.Keys.IndexOf(descr);
                if (idx == sender.Keys.Count - 1)
                {
                    columns.Restrict(descr, PXComp.EQ, val);
                    if (!_SingleRecord)
                    {
                        columns.RestrictPast(descr, PXComp.LT, val);
                        columns.RestrictFuture(descr, PXComp.GT, val);
                        columns.OrderBy(descr, false);
                    }
                    else
                    {
                        columns.RestrictPast(descr, PXComp.EQ, val);
                    }
                    columns.InitializeWith(descr, val);
                }
                else if (idx != -1)
                {
                    columns.Restrict(descr, PXComp.EQ, val);
                    columns.RestrictPast(descr, PXComp.EQ, val);
                    if (!_SingleRecord)
                    {
                        columns.RestrictFuture(descr, PXComp.EQ, val);
                    }
                    columns.InitializeWith(descr, val);
                }
                else if (val.GetType() == typeof(decimal))
                {
                    columns.InitializeWith(descr, val);
                    if ((decimal)val != 0m)
                    {
                        columns.Update(descr, val);
                    }
                }
                else if (val.GetType() == typeof(double))
                {
                    columns.InitializeWith(descr, val);
                    if ((double)val != 0.0)
                    {
                        columns.Update(descr, val);
                    }
                }
                else
                {
                    columns.InitializeWith(descr, val);
                }
            }
            if (!_SingleRecord && _Source != null && _Destination != null)
            {
                for (int i = 0; i < _Source.Length && i < _Destination.Length; i++)
                {
                    columns.InitializeFrom(_Destination[i], _Source[i]);
                    columns.UpdateFuture(_Destination[i], sender.GetValue(row, _Source[i].Name));
                }
            }
            return true;
        }
        public override bool PersistInserted(PXCache sender, object row)
        {
            PXAccumulatorCollection columns = new PXAccumulatorCollection();
            if (!PrepareInsert(sender, row, columns))
            {
                return false;
            }
            List<PXAccumulatorItem> list = new List<PXAccumulatorItem>();
            int keyspos = 0;
            int initpos = 0;
            foreach (PXAccumulatorItem item in columns.Values)
            {
                if (item.OrderBy != null || item.CurrentComparison.Length > 0 || item.PastComparison.Length > 0 || item.FutureComparison.Length > 0)
                {
                    list.Add(item);
                }
                else if (item.Initializer != null && !String.IsNullOrEmpty(item.Initializer.Value.Key))
                {
                    list.Insert(initpos, item);
                    keyspos++;
                }
                else
                {
                    list.Insert(0, item);
                    keyspos++;
                    initpos++;
                }
            }
            List<PXDataFieldAssign> values = new List<PXDataFieldAssign>();
            List<PXDataField> pars = new List<PXDataField>();
			List<PXDataFieldParam> single = new List<PXDataFieldParam>();
			List<PXDataFieldParam> mass = new List<PXDataFieldParam>();
			List<PXDataField> checkfield = null;
			List<PXDataFieldParam> checkparam = null;
			List<KeyValuePair<int, KeyValuePair<int, int>>> checkindex = null;
            bool anyfuture = false;
            try
            {
                int lastkey = -1;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    PXAccumulatorItem item = list[i];
                    if (i >= keyspos)
                    {
						if (!columns.UpdateOnly)
						{
							{
								PXCommandPreparingEventArgs.FieldDescription description = null;
								sender.RaiseCommandPreparing(item.Field, row, item.Initializer != null ? item.Initializer.Value.Value : null, PXDBOperation.Insert, null, out description);
								if (description != null && !String.IsNullOrEmpty(description.FieldName))
								{
									int pos = description.FieldName.IndexOf('.');
									string name = (pos == -1 ? description.FieldName : description.FieldName.Substring(pos + 1));
									values.Add(new PXDataFieldAssign(name, description.DataType, description.DataLength, description.DataValue));
								}
							}
							bool orderset = false;
							foreach (KeyValuePair<PXComp, object> restr in item.PastComparison)
							{
								PXCommandPreparingEventArgs.FieldDescription description = null;
								sender.RaiseCommandPreparing(item.Field, row, restr.Value, PXDBOperation.Select, null, out description);
								if (description != null && !String.IsNullOrEmpty(description.FieldName))
								{
									int pos = description.FieldName.IndexOf('.');
									string name = (pos == -1 ? description.FieldName : description.FieldName.Substring(pos + 1));
									pars.Add(new PXDataFieldValue(name, description.DataType, description.DataLength, description.DataValue, restr.Key));
									if (item.OrderBy != null && !orderset)
									{
										pars.Add(new PXDataFieldOrder(name, !((bool)item.OrderBy)));
										orderset = true;
									}
								}
							}
						}
                        foreach (KeyValuePair<PXComp, object> restr in item.CurrentComparison)
                        {
                            PXCommandPreparingEventArgs.FieldDescription description = null;
                            sender.RaiseCommandPreparing(item.Field, row, restr.Value, PXDBOperation.Update, null, out description);
                            if (description != null && !String.IsNullOrEmpty(description.FieldName))
                            {
                                int pos = description.FieldName.IndexOf('.');
                                string name = (pos == -1 ? description.FieldName : description.FieldName.Substring(pos + 1));
                                single.Add(new PXDataFieldRestrict(name, description.DataType, description.DataLength, description.DataValue, restr.Key));
                            }
                        }
                        foreach (KeyValuePair<PXComp, object> restr in item.FutureComparison)
                        {
                            PXCommandPreparingEventArgs.FieldDescription description = null;
                            sender.RaiseCommandPreparing(item.Field, row, restr.Value, PXDBOperation.Update, null, out description);
                            if (description != null && !String.IsNullOrEmpty(description.FieldName))
                            {
                                int pos = description.FieldName.IndexOf('.');
                                string name = (pos == -1 ? description.FieldName : description.FieldName.Substring(pos + 1));
                                mass.Add(new PXDataFieldRestrict(name, description.DataType, description.DataLength, description.DataValue, restr.Key));
                            }
                        }
                    }
					else if (!columns.UpdateOnly && item.Initializer != null)
                    {
                        if (lastkey == -1)
                        {
                            lastkey = values.Count;
                        }
                        PXCommandPreparingEventArgs.FieldDescription description = null;
                        sender.RaiseCommandPreparing(item.Field, row, item.Initializer.Value.Value, PXDBOperation.Insert, null, out description);
                        if (description != null && !String.IsNullOrEmpty(description.FieldName))
                        {
                            int pos = description.FieldName.IndexOf('.');
                            string name = (pos == -1 ? description.FieldName : description.FieldName.Substring(pos + 1));
                            if (!description.IsRestriction)
                            {
                                values.Insert(lastkey, new PXDataFieldAssign(name, description.DataType, description.DataLength, description.DataValue));
                                if (i >= initpos)
                                {
                                    description = null;
                                    sender.RaiseCommandPreparing(item.Initializer.Value.Key, row, item.Initializer.Value.Value, PXDBOperation.Insert, null, out description);
                                    if (description != null && !String.IsNullOrEmpty(description.FieldName))
                                    {
                                        pos = description.FieldName.IndexOf('.');
                                        name = (pos == -1 ? description.FieldName : description.FieldName.Substring(pos + 1));
                                        pars.Insert(0, new PXDataField(name));
										if (item.CurrentUpdate != null || item.CurrentUpdateBehavior == PXDataFieldAssign.AssignBehavior.Nullout)
                                        {
                                            values[lastkey].Behavior = item.CurrentUpdateBehavior;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (!columns.InsertOnly && (item.CurrentUpdate != null || item.CurrentUpdateBehavior == PXDataFieldAssign.AssignBehavior.Nullout))
                    {
                        PXCommandPreparingEventArgs.FieldDescription description = null;
                        sender.RaiseCommandPreparing(item.Field, row, item.CurrentUpdate, PXDBOperation.Update, null, out description);
                        if (description != null && !String.IsNullOrEmpty(description.FieldName))
                        {
                            int pos = description.FieldName.IndexOf('.');
                            string name = (pos == -1 ? description.FieldName : description.FieldName.Substring(pos + 1));
                            if (!description.IsRestriction)
                            {
                                PXDataFieldAssign s = new PXDataFieldAssign(name, description.DataType, description.DataLength, description.DataValue);
                                s.Behavior = item.CurrentUpdateBehavior;
                                single.Add(s);
                            }
                        }
                    }
                    if (item.FutureUpdate != null)
                    {
                        PXCommandPreparingEventArgs.FieldDescription description = null;
                        sender.RaiseCommandPreparing(item.Field, row, item.FutureUpdate, PXDBOperation.Update, null, out description);
                        if (description != null && !String.IsNullOrEmpty(description.FieldName))
                        {
                            int pos = description.FieldName.IndexOf('.');
                            string name = (pos == -1 ? description.FieldName : description.FieldName.Substring(pos + 1));
                            if (!description.IsRestriction)
                            {
                                PXDataFieldAssign m = new PXDataFieldAssign(name, description.DataType, description.DataLength, description.DataValue);
                                m.Behavior = item.FutureUpdateBehavior;
                                mass.Add(m);
                                anyfuture = true;
                            }
                        }
                    }
                }
				if (columns.Exceptions != null && columns.Exceptions.Count > 0)
				{
					checkparam = new List<PXDataFieldParam>();
					checkfield = new List<PXDataField>();
					checkindex = new List<KeyValuePair<int, KeyValuePair<int, int>>>();
					int start = 0;
					for (int i = 0; i < columns.Exceptions.Count; i++)
					{
						int finish = start;
						for (int j = 0; j < columns.Exceptions[i].Value.Length; j++)
						{
							PXCommandPreparingEventArgs.FieldDescription description = null;
							sender.RaiseCommandPreparing(columns.Exceptions[i].Value[j].FieldName, row, columns.Exceptions[i].Value[j].Value, PXDBOperation.Update, null, out description);
							if (description != null && !String.IsNullOrEmpty(description.FieldName))
							{
								int pos = description.FieldName.IndexOf('.');
								string name = (pos == -1 ? description.FieldName : description.FieldName.Substring(pos + 1));
								PXDataFieldRestrict r = new PXDataFieldRestrict(name, description.DataType, description.DataLength, description.DataValue, columns.Exceptions[i].Value[j].Comp);
								if (j == 0)
								{
									r.OpenBrackets++;
								}
								else
								{
									r.OrOperator = true;
								}
								if (j == columns.Exceptions[i].Value.Length - 1)
								{
									r.CloseBrackets++;
								}
								r.CheckResultOnly = true;
								checkparam.Add(r);
								PXDataFieldValue v = new PXDataFieldValue(r.FieldName, r.ValueType, r.ValueLength, r.Value, r.Comp);
								v.OpenBrackets = r.OpenBrackets;
								v.CloseBrackets = r.CloseBrackets;
								v.OrOperator = r.OrOperator;
								v.CheckResultOnly = true;
								checkfield.Add(v);
								finish++;
							}
						}
						if (start != finish)
						{
							checkindex.Add(new KeyValuePair<int, KeyValuePair<int, int>>(i, new KeyValuePair<int, int>(start, finish - 1)));
						}
						start = finish;
					}
					if (checkparam.Count == 0)
					{
						checkparam = null;
					}
				}
            }
            catch (PXCommandPreparingException e)
            {
                if (sender.RaiseExceptionHandling(e.Name, row, e.Value, e))
                {
                    throw;
                }
                return false;
            }
			PXDBOperation oper = PXDBOperation.Insert;
            try
            {
				int parscount = pars.Count;
				int singlecount = single.Count;
				if (checkparam != null)
				{
					pars.AddRange(checkfield);
				}
				if ((parscount == 0 || !sender.Graph.ProviderEnsure(sender.BqlTable, values.ToArray(), pars.ToArray())) && single.Count > 0)
                {
					oper = PXDBOperation.Update;
					if (checkparam != null)
					{
						single.AddRange(checkparam);
					}
					if (!sender.Graph.ProviderUpdate(sender.BqlTable, single.ToArray()))
                    {
						if (checkparam != null)
						{
							for (int i = 1; i <= checkindex.Count; i++)
							{
								for (int j = 0; j < Math.Pow(2.0, checkindex.Count) && j < int.MaxValue - 1; j++)
								{
									int m = j;
									int count = 0;
									{
										for (int k = 0; k < checkindex.Count; k++)
										{
											if ((m & 1) == 0)
											{
												count++;
											}
											m = m >> 1;
										}
									}
									if (count == i)
									{
										pars = pars.GetRange(0, parscount);
										single = single.GetRange(0, singlecount);
										int errornbr = -1;
										m = j;
										for (int k = 0; k < checkindex.Count; k++)
										{
											if ((m & 1) == 1)
											{
												pars.AddRange(checkfield.GetRange(checkindex[k].Value.Key, checkindex[k].Value.Value - checkindex[k].Value.Key + 1));
												single.AddRange(checkparam.GetRange(checkindex[k].Value.Key, checkindex[k].Value.Value - checkindex[k].Value.Key + 1));
											}
											else if (errornbr == -1)
											{
												errornbr = k;
											}
											m = m >> 1;
										}
										if (sender.Graph.ProviderEnsure(sender.BqlTable, values.ToArray(), pars.ToArray())
											|| single.Count > 0 && sender.Graph.ProviderUpdate(sender.BqlTable, single.ToArray()))
										{
											throw new PXRestrictionViolationException(columns.Exceptions[checkindex[errornbr].Key].Key, getKeys(sender, row), checkindex[errornbr].Key);
										}
									}
								}
							}
						}
						throw new PXLockViolationException(sender.BqlTable, PXDBOperation.Update, getKeys(sender, row));
					}
                }
				sender.RaiseRowPersisted(row, oper, PXTranStatus.Open, null);
				if (anyfuture)
                {
                    sender.Graph.ProviderUpdate(sender.BqlTable, mass.ToArray());
                }
            }
            catch (PXDatabaseException e)
            {
                e.Keys = getKeys(sender, row);
                throw;
            }
            return true;
        }
        public override bool PersistUpdated(PXCache sender, object row)
        {
            List<PXDataFieldParam> pars = new List<PXDataFieldParam>();
            try
            {
                foreach (string descr in sender.Fields)
                {
                    PXCommandPreparingEventArgs.FieldDescription description = null;
                    sender.RaiseCommandPreparing(descr, row, sender.GetValue(row, descr), PXDBOperation.Update, null, out description);
                    if (description != null && !String.IsNullOrEmpty(description.FieldName))
                    {
                        if (description.IsRestriction)
                        {
                            pars.Add(new PXDataFieldRestrict(description.FieldName, description.DataType, description.DataLength, description.DataValue));
                        }
                        else
                        {
                            pars.Add(new PXDataFieldAssign(description.FieldName, description.DataType, description.DataLength, description.DataValue));
                        }
                    }
                }
            }
            catch (PXCommandPreparingException e)
            {
                if (sender.RaiseExceptionHandling(e.Name, row, e.Value, e))
                {
                    throw;
                }
                return false;
            }
            try
            {
                if (!sender.Graph.ProviderUpdate(sender.BqlTable, pars.ToArray()))
                {
					throw new PXLockViolationException(sender.BqlTable, PXDBOperation.Update, getKeys(sender, row));
				}
            }
            catch (PXDatabaseException e)
            {
                e.Keys = getKeys(sender, row);
                throw;
            }
            return true;
        }
        public override bool PersistDeleted(PXCache sender, object row)
        {
            List<PXDataFieldRestrict> pars = new List<PXDataFieldRestrict>();
            try
            {
                foreach (string descr in sender.Fields)
                {
                    PXCommandPreparingEventArgs.FieldDescription description = null;
                    sender.RaiseCommandPreparing(descr, row, sender.GetValue(row, descr), PXDBOperation.Delete, null, out description);
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
                if (sender.RaiseExceptionHandling(e.Name, row, e.Value, e))
                {
                    throw;
                }
                return false;
            }
            try
            {
                if (!sender.Graph.ProviderDelete(sender.BqlTable, pars.ToArray()))
                {
                    throw new PXLockViolationException(sender.BqlTable, PXDBOperation.Delete, getKeys(sender, row));
                }
                sender.RaiseRowPersisted(row, PXDBOperation.Delete, PXTranStatus.Open, null);
            }
            catch (PXDatabaseException e)
            {
                e.Keys = getKeys(sender, row);
                throw;
            }
            return true;
        }
        public override object Insert(PXCache sender, object row)
        {
            object existing = sender.Locate(row);
            if (existing != null)
            {
                if (sender.GetStatus(existing) == PXEntryStatus.Inserted)
                {
								    sender.Current = existing;
                    return existing;
                }
                sender.Remove(existing);
                return base.Insert(sender, row);
            }
            return base.Insert(sender, row);
        }
    }

    public sealed class PXAccumulatorItem
    {
        private string _Field;
        private List<KeyValuePair<PXComp, object>> _PastComparison;
        private List<KeyValuePair<PXComp, object>> _CurrentComparison;
        private List<KeyValuePair<PXComp, object>> _FutureComparison;
        private string _InitializeFrom;
        private object _InitializeWith;
        private bool? _OrderPast;
        private object _CurrentUpdate;
        private PXDataFieldAssign.AssignBehavior _CurrentUpdateBehavior;
        private object _FutureUpdate;
        private PXDataFieldAssign.AssignBehavior _FutureUpdateBehavior;
        public PXAccumulatorItem(string field)
        {
            _Field = field;
        }
        public void RestrictPast(PXComp comparison, object value)
        {
            if (_PastComparison == null)
            {
                _PastComparison = new List<KeyValuePair<PXComp, object>>();
            }
            _PastComparison.Add(new KeyValuePair<PXComp, object>(comparison, value));
        }
        public void OrderPast(bool ascending)
        {
            _OrderPast = ascending;
        }
        public void RestrictCurrent(PXComp comparison, object value)
        {
            if (_CurrentComparison == null)
            {
                _CurrentComparison = new List<KeyValuePair<PXComp, object>>();
            }
            _CurrentComparison.Add(new KeyValuePair<PXComp, object>(comparison, value));
        }
        public void RestrictFuture(PXComp comparison, object value)
        {
            if (_FutureComparison == null)
            {
                _FutureComparison = new List<KeyValuePair<PXComp, object>>();
            }
            _FutureComparison.Add(new KeyValuePair<PXComp, object>(comparison, value));
        }
        public void InitializeFrom(string field)
        {
            _InitializeFrom = field;
        }
        public void InitializeWith(object value)
        {
            _InitializeWith = value;
        }
        public void UpdateCurrent(object value)
        {
            UpdateCurrent(value, PXDataFieldAssign.AssignBehavior.Summarize);
        }
        public void UpdateCurrent(object value, PXDataFieldAssign.AssignBehavior behavior)
        {
            _CurrentUpdate = value;
			_CurrentUpdateBehavior = value != null || behavior != PXDataFieldAssign.AssignBehavior.Replace ? behavior : PXDataFieldAssign.AssignBehavior.Nullout;
        }
        public void UpdateFuture(object value)
        {
            _FutureUpdate = value;
            _FutureUpdateBehavior = PXDataFieldAssign.AssignBehavior.Summarize;
        }
        public void UpdateFuture(object value, PXDataFieldAssign.AssignBehavior behavior)
        {
            _FutureUpdate = value;
			_FutureUpdateBehavior = value != null || behavior != PXDataFieldAssign.AssignBehavior.Replace ? behavior : PXDataFieldAssign.AssignBehavior.Nullout;
		}
        public string Field
        {
            get
            {
                return _Field;
            }
        }
        public KeyValuePair<PXComp, object>[] PastComparison
        {
            get
            {
                if (_PastComparison == null || _PastComparison.Count == 0)
                {
                    return new KeyValuePair<PXComp, object>[0];
                }
                return _PastComparison.ToArray();
            }
        }
        public KeyValuePair<PXComp, object>[] CurrentComparison
        {
            get
            {
                if (_CurrentComparison == null || _CurrentComparison.Count == 0)
                {
                    return new KeyValuePair<PXComp, object>[0];
                }
                return _CurrentComparison.ToArray();
            }
        }
        public KeyValuePair<PXComp, object>[] FutureComparison
        {
            get
            {
                if (_FutureComparison == null || _FutureComparison.Count == 0)
                {
                    return new KeyValuePair<PXComp, object>[0];
                }
                return _FutureComparison.ToArray();
            }
        }
        public KeyValuePair<string, object>? Initializer
        {
            get
            {
                if (_InitializeFrom == null && _InitializeWith == null)
                {
                    return null;
                }
                return new KeyValuePair<string, object>(_InitializeFrom, _InitializeWith);
            }
        }
        public bool? OrderBy
        {
            get
            {
                return _OrderPast;
            }
        }
        public object CurrentUpdate
        {
            get
            {
                return _CurrentUpdate;
            }
        }
        public object FutureUpdate
        {
            get
            {
                return _FutureUpdate;
            }
        }
        public PXDataFieldAssign.AssignBehavior CurrentUpdateBehavior
        {
            get
            {
                return _CurrentUpdateBehavior;
            }
        }
        public PXDataFieldAssign.AssignBehavior FutureUpdateBehavior
        {
            get
            {
                return _FutureUpdateBehavior;
            }
        }
    }

	public class PXAccumulatorRestriction
	{
		public readonly string FieldName;
		public readonly PXComp Comp;
		public readonly object Value;
		public PXAccumulatorRestriction(string fieldName, PXComp comp, object value)
		{
			FieldName = fieldName;
			Comp = comp;
			Value = value;
		}
	}

	public class PXAccumulatorRestriction<Field> : PXAccumulatorRestriction
		where Field : IBqlField
	{
		public PXAccumulatorRestriction(PXComp comp, object value)
			: base(typeof(Field).Name, comp, value)
		{
		}
	}

    public sealed class PXAccumulatorCollection : Dictionary<string, PXAccumulatorItem>
    {
		private List<KeyValuePair<string, PXAccumulatorRestriction[]>> _Exceptions;
		public List<KeyValuePair<string, PXAccumulatorRestriction[]>> Exceptions
		{
			get
			{
				return _Exceptions;
			}
		}
        private bool _InsertOnly;
        public bool InsertOnly
        {
            get
            {
                return _InsertOnly;
            }
            set
            {
                _InsertOnly = value;
            }
        }
		private bool _UpdateOnly;
		public bool UpdateOnly
		{
			get
			{
				return _UpdateOnly;
			}
			set
			{
				_UpdateOnly = value;
			}
		}
        public PXAccumulatorCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
        public void Add<Field>()
            where Field : IBqlField
        {
            Add(typeof(Field).Name);
        }
        public void Add(Type bqlField)
        {
            Add(bqlField.Name);
        }
        public void Add(string field)
        {
            base.Add(field, new PXAccumulatorItem(field));
        }
        public void Remove<Field>()
            where Field : IBqlField
        {
            Remove(typeof(Field).Name);
        }
        public void Remove(Type bqlField)
        {
            Remove(bqlField.Name);
        }
        public new void Remove(string field)
        {
            base.Remove(field);
        }
        public void Update<Field>(object value)
            where Field : IBqlField
        {
            Update(typeof(Field).Name, value, PXDataFieldAssign.AssignBehavior.Summarize);
        }
        public void Update<Field>(object value, PXDataFieldAssign.AssignBehavior behavior)
            where Field : IBqlField
        {
            Update(typeof(Field).Name, value, behavior);
        }
        public void Update(Type bqlField, object value)
        {
            Update(bqlField.Name, value, PXDataFieldAssign.AssignBehavior.Summarize);
        }
        public void Update(Type bqlField, object value, PXDataFieldAssign.AssignBehavior behavior)
        {
            Update(bqlField.Name, value, behavior);
        }
        public void Update(string field, object value)
        {
            Update(field, value, PXDataFieldAssign.AssignBehavior.Summarize);
        }
        public void Update(string field, object value, PXDataFieldAssign.AssignBehavior behavior)
        {
            PXAccumulatorItem item = base[field];
            item.UpdateCurrent(value, behavior);
        }
        public void UpdateFuture<Field>(object value)
            where Field : IBqlField
        {
            UpdateFuture(typeof(Field).Name, value, PXDataFieldAssign.AssignBehavior.Summarize);
        }
        public void UpdateFuture<Field>(object value, PXDataFieldAssign.AssignBehavior behavior)
            where Field : IBqlField
        {
            UpdateFuture(typeof(Field).Name, value, behavior);
        }
        public void UpdateFuture(Type bqlField, object value)
        {
            UpdateFuture(bqlField.Name, value, PXDataFieldAssign.AssignBehavior.Summarize);
        }
        public void UpdateFuture(Type bqlField, object value, PXDataFieldAssign.AssignBehavior behavior)
        {
            UpdateFuture(bqlField.Name, value, behavior);
        }
        public void UpdateFuture(string field, object value)
        {
            UpdateFuture(field, value, PXDataFieldAssign.AssignBehavior.Summarize);
        }
        public void UpdateFuture(string field, object value, PXDataFieldAssign.AssignBehavior behavior)
        {
            PXAccumulatorItem item = base[field];
            item.UpdateFuture(value, behavior);
        }
        public void Restrict<Field>(PXComp comparison, object value)
            where Field : IBqlField
        {
            Restrict(typeof(Field).Name, comparison, value);
        }
        public void Restrict(Type bqlField, PXComp comparison, object value)
        {
            Restrict(bqlField.Name, comparison, value);
        }
        public void Restrict(string field, PXComp comparison, object value)
        {
            PXAccumulatorItem item = base[field];
            item.RestrictCurrent(comparison, value);
        }
        public void RestrictPast<Field>(PXComp comparison, object value)
            where Field : IBqlField
        {
            RestrictPast(typeof(Field).Name, comparison, value);
        }
        public void RestrictPast(Type bqlField, PXComp comparison, object value)
        {
            RestrictPast(bqlField.Name, comparison, value);
        }
        public void RestrictPast(string field, PXComp comparison, object value)
        {
            PXAccumulatorItem item = base[field];
            item.RestrictPast(comparison, value);
        }
        public void RestrictFuture<Field>(PXComp comparison, object value)
            where Field : IBqlField
        {
            RestrictFuture(typeof(Field).Name, comparison, value);
        }
        public void RestrictFuture(Type bqlField, PXComp comparison, object value)
        {
            RestrictFuture(bqlField.Name, comparison, value);
        }
        public void RestrictFuture(string field, PXComp comparison, object value)
        {
            PXAccumulatorItem item = base[field];
            item.RestrictFuture(comparison, value);
        }
        public void InitializeFrom<Field, Source>()
            where Field : IBqlField
            where Source : IBqlField
        {
            InitializeFrom(typeof(Field).Name, typeof(Source).Name);
        }
        public void InitializeFrom<Field>(Type source)
            where Field : IBqlField
        {
            InitializeFrom(typeof(Field).Name, source.Name);
        }
        public void InitializeFrom(Type bqlField, Type source)
        {
            InitializeFrom(bqlField.Name, source.Name);
        }
        public void InitializeFrom(string field, string source)
        {
            PXAccumulatorItem item = base[field];
            item.InitializeFrom(source);
        }
        public void InitializeWith<Field>(object value)
            where Field : IBqlField
        {
            InitializeWith(typeof(Field).Name, value);
        }
        public void InitializeWith(Type bqlField, object value)
        {
            InitializeWith(bqlField.Name, value);
        }
        public void InitializeWith(string field, object value)
        {
            PXAccumulatorItem item = base[field];
            item.InitializeWith(value);
        }
        public void OrderBy<Field>(bool ascending)
            where Field : IBqlField
        {
            OrderBy(typeof(Field).Name, ascending);
        }
        public void OrderBy(Type bqlField, bool ascending)
        {
            OrderBy(bqlField.Name, ascending);
        }
        public void OrderBy(string field, bool ascending)
        {
            PXAccumulatorItem item = base[field];
            item.OrderPast(ascending);
        }
		public void AppendException(string message, params PXAccumulatorRestriction[] exception)
		{
			if (exception.Length > 0)
			{
				if (_Exceptions == null)
				{
					_Exceptions = new List<KeyValuePair<string, PXAccumulatorRestriction[]>>();
				}
				_Exceptions.Add(new KeyValuePair<string, PXAccumulatorRestriction[]>(message, exception));
			}
		}
    }

	[AttributeUsage(AttributeTargets.Class)]
	public class PXSplitRowAttribute : PXDBInterceptorAttribute
	{
		protected BqlCommand rowselection;
		protected BqlCommand tableselection;
		protected Type[] mergeFields;
		public override void CacheAttached(PXCache sender)
		{
			tableselection = new BqlRowSelection(sender, mergeFields);
			sender.CommandPreparingEvents[mergeFields[0].Name.ToLower()] += CaseCommandPreparing;
			sender.Keys.Add(char.ToUpper(mergeFields[0].Name[0]) + mergeFields[0].Name.Substring(1));
			for (int i = 1; i < mergeFields.Length; i++)
			{
				sender.Keys.Add(char.ToUpper(mergeFields[i].Name[0]) + mergeFields[i].Name.Substring(1));
				int j = i;
				sender.CommandPreparingEvents[mergeFields[j].Name.ToLower()] += delegate(PXCache cache, PXCommandPreparingEventArgs e)
				{
					WhenCommandPreparing(cache, e, j);
				};
			}
		}
		bool bypassevent;
		public virtual void CaseCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (bypassevent)
			{
				return;
			}
			bypassevent = true;
			StringBuilder bld = new StringBuilder("CASE SplitRowVector.Value WHEN 1 THEN ");
			{
				PXCommandPreparingEventArgs.FieldDescription description;
				sender.RaiseCommandPreparing(mergeFields[0].Name, null, e.Value, PXDBOperation.Select, e.Table, out description);
				if (description != null && !String.IsNullOrEmpty(description.FieldName))
				{
					e.DataType = description.DataType;
					e.DataLength = description.DataLength;
					e.DataValue = description.DataValue;
					e.Cancel = true;
					bld.Append(description.FieldName);
				}
			}
			for (int i = 1; i < mergeFields.Length; i++)
			{
				PXCommandPreparingEventArgs.FieldDescription description;
				sender.RaiseCommandPreparing(mergeFields[i].Name, null, null, PXDBOperation.Select, e.Table, out description);
				if (description != null && !String.IsNullOrEmpty(description.FieldName))
				{
					bld.Append(" WHEN ");
					bld.Append(i + 1);
					bld.Append(" THEN ");
					bld.Append(description.FieldName);
					e.BqlTable = description.BqlTable;
				}
			}
			bld.Append(" END");
			e.FieldName = bld.ToString();
			bypassevent = false;
		}
		public virtual void WhenCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e, int sub)
		{
			if (bypassevent)
			{
				return;
			}
			StringBuilder bld = new StringBuilder("CASE SplitRowVector.Value WHEN ");
			bld.Append(sub + 1);
			bld.Append(" THEN ");
			{
				bypassevent = true;
				PXCommandPreparingEventArgs.FieldDescription description;
				sender.RaiseCommandPreparing(mergeFields[sub].Name, null, e.Value, PXDBOperation.Select, e.Table, out description);
				if (description != null && !String.IsNullOrEmpty(description.FieldName))
				{
					e.DataType = description.DataType;
					e.DataLength = description.DataLength;
					e.DataValue = description.DataValue;
					e.Cancel = true;
					bld.Append(description.FieldName);
					e.BqlTable = description.BqlTable;
				}
				bypassevent = false;
			}
			bld.Append(" ELSE NULL END");
			e.FieldName = bld.ToString();
		}
		public override BqlCommand GetRowCommand()
		{
			throw new PXException("The method or operation is not implemented.");
		}
		public override BqlCommand GetTableCommand()
		{
			return tableselection;
		}
		public PXSplitRowAttribute(params Type[] fields)
		{
			mergeFields = fields;
		}
		public override bool PersistInserted(PXCache sender, object row)
		{
			throw new PXException("The method or operation is not implemented.");
		}
		public override bool PersistUpdated(PXCache sender, object row)
		{
			throw new PXException("The method or operation is not implemented.");
		}
		public override bool PersistDeleted(PXCache sender, object row)
		{
			throw new PXException("The method or operation is not implemented.");
		}
		private sealed class BqlRowSelection : BqlCommand
		{
			public BqlRowSelection(PXCache cache, Type[] fields)
			{
				this.lastKey = cache.Keys.Count - 1;
				this.alias = cache.GetItemType();
				this.keys = cache.Keys.ToArray();
				this.tables = new List<Type>();
				this.fields = fields;
				Type table = alias;
				while (table != typeof(object))
				{
					if ((table.BaseType == typeof(object)
						|| !typeof(IBqlTable).IsAssignableFrom(table.BaseType))
						&& typeof(IBqlTable).IsAssignableFrom(table)
						|| table.IsDefined(typeof(PXTableAttribute), false))
					{
						this.tables.Add(table);
					}
					table = table.BaseType;
				}
			}
			private List<Type> tables;
			private string[] keys;
			private Type alias;
			private Type[] fields;
			private int lastKey;
			public override void Parse(PXGraph graph, List<IBqlParameter> pars, List<Type> tables, List<Type> fields, List<IBqlSortColumn> sortColumns, StringBuilder text, Selection selection)
			{
				if (tables != null)
				{
					tables.AddRange(this.tables);
				}
				if (graph != null && text != null)
				{
					PXCache cache = graph.Caches[alias];
					foreach (string field in cache.Fields)
					{
						PXCommandPreparingEventArgs.FieldDescription description;
						cache.RaiseCommandPreparing(field, null, null, PXDBOperation.Select, null, out description);
						if (description != null && !String.IsNullOrEmpty(description.FieldName))
						{
							selection.Add(description.FieldName);
						}
					}
					text.Append(" FROM ");
					for (int i = 0; i < this.tables.Count; i++)
					{
						if (i > 0)
						{
							text.Append(" CROSS JOIN ");
						}
						text.Append(this.tables[i].Name);
						text.Append(" ");
						text.Append(this.tables[i].Name);
					}
					text.Append(" CROSS JOIN (SELECT 1 AS Value");
					for (int i = 1; i < this.fields.Length; i++)
					{
						text.Append(" UNION ALL SELECT ");
						text.Append(i + 1);
					}
					text.Append(") SplitRowVector ");
					if (this.tables.Count > 1)
					{
						bool first = true;
						for (int i = 0; i <= lastKey; i++)
						{
							for (int j = 0; j < this.tables.Count; j++)
							{
								PXCommandPreparingEventArgs.FieldDescription pdescription;
								cache.RaiseCommandPreparing(cache.Keys[i], null, null, PXDBOperation.Select, this.tables[j], out pdescription);
								if (pdescription != null && !String.IsNullOrEmpty(pdescription.FieldName))
								{
									for (int k = j + 1; k < this.tables.Count; k++)
									{
										PXCommandPreparingEventArgs.FieldDescription description;
										cache.RaiseCommandPreparing(cache.Keys[i], null, null, PXDBOperation.Select, this.tables[k], out description);
										if (description != null && !String.IsNullOrEmpty(description.FieldName))
										{
											if (first)
											{

												text.Append(" WHERE ");
												first = false;
											}
											else
											{
												text.Append(" AND ");
											}
											text.Append(pdescription.FieldName);
											text.Append(" = ");
											text.Append(description.FieldName);
										}
									}
								}
							}
						}
					}
				}
			}
			public override void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
			{
			}
			public override BqlCommand OrderByNew<newOrderBy>()
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand OrderByNew(Type newOrderBy)
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereAnd<where>()
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereAnd(Type where)
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereNew<newWhere>()
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereNew(Type newWhere)
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereNot()
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereOr<where>()
			{
				throw new PXException("The method or operation is not implemented.");
			}
			public override BqlCommand WhereOr(Type where)
			{
				throw new PXException("The method or operation is not implemented.");
			}
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class PXDynamicAggregateAttribute : PXEventSubscriberAttribute, IPXRowSelectingSubscriber, IPXRowSelectedSubscriber, 
		IPXRowUpdatingSubscriber, IPXRowUpdatedSubscriber, IPXRowInsertingSubscriber, IPXRowInsertedSubscriber, 
		IPXRowDeletingSubscriber, IPXRowDeletedSubscriber, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber, 
		IPXFieldDefaultingSubscriber, IPXFieldSelectingSubscriber, IPXFieldUpdatingSubscriber, IPXFieldUpdatedSubscriber, 
		IPXFieldVerifyingSubscriber
	{
		public delegate IEnumerable<PXEventSubscriberAttribute> GetAttributes(string fieldName);

		private readonly GetAttributes _getAttributesHandler;

		public PXDynamicAggregateAttribute(GetAttributes handler)
		{
			if (handler == null) throw new ArgumentNullException("handler");

			_getAttributesHandler = handler;
		}

		public void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowSelectingSubscriber>())
				attribute.RowSelecting(sender, e);
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowSelectedSubscriber>())
				attribute.RowSelected(sender, e);
		}

		public void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowUpdatingSubscriber>())
				attribute.RowUpdating(sender, e);
		}

		public void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowUpdatedSubscriber>())
				attribute.RowUpdated(sender, e);
		}

		public void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowInsertingSubscriber>())
				attribute.RowInserting(sender, e);
		}

		public void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowInsertedSubscriber>())
				attribute.RowInserted(sender, e);
		}

		public void RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowDeletingSubscriber>())
				attribute.RowDeleting(sender, e);
		}

		public void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowDeletedSubscriber>())
				attribute.RowDeleted(sender, e);
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowPersistingSubscriber>())
				attribute.RowPersisting(sender, e);
		}

		public void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			foreach (var attribute in Attributes<IPXRowPersistedSubscriber>())
				attribute.RowPersisted(sender, e);
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXFieldDefaultingSubscriber>())
				attribute.FieldDefaulting(sender, e);
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXFieldSelectingSubscriber>())
				attribute.FieldSelecting(sender, e);
		}

		public void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXFieldUpdatingSubscriber>())
				attribute.FieldUpdating(sender, e);
		}

		public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			foreach (var attribute in Attributes<IPXFieldUpdatedSubscriber>())
				attribute.FieldUpdated(sender, e);
		}

		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			foreach (var attribute in Attributes<IPXFieldVerifyingSubscriber>())
				attribute.FieldVerifying(sender, e);
		}

		private IEnumerable<T> Attributes<T>() 
			where T : class
		{
			foreach (var attribute in _getAttributesHandler(_FieldName))
			{
				var typedAtt = attribute as T;
				if (typedAtt != null) yield return typedAtt;
			}
		}
    }

    #region PXClassAttribute
    public abstract class PXClassAttribute : Attribute
    {
        public virtual void CacheAttached(PXCache sender)
        { 
        }
    }
    #endregion

    #region PXDisableCloneAttributesAttribute
    [AttributeUsage(AttributeTargets.Class)]
    public class PXDisableCloneAttributesAttribute : PXClassAttribute
    {
        public override void CacheAttached(PXCache sender)
        {
            sender.DisableCloneAttributes = true;
        }
    }
    #endregion

	#region PXTableNameAttribute
	[AttributeUsage(AttributeTargets.Class)]
	public class PXTableNameAttribute : Attribute
	{
	}
	#endregion

	#region IPXLocalizableList
	//Can be inherited by attributes which are used on properties of IBqlTable class
	//If IsLocalizable is false list values won't be collected for localization
	public interface IPXLocalizableList
	{
		bool IsLocalizable { get; set; }
	}
	#endregion
}

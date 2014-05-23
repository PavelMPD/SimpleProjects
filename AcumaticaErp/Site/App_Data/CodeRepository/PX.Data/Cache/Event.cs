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
using System.ComponentModel;


namespace PX.Data
{
	public delegate void PXRowUpdating(PXCache sender, PXRowUpdatingEventArgs e);

    /// <summary>
    /// Occurs before updating the row in the cache.<br/>
    /// This occurs when cache Update method is invoked from API or from user interface.<br/>
    /// Update method searches the row in the cache and in the database, if row is found then RowUpdating event is raised, else RowInserting.
    /// The event allows to cancel Update procedure.
    /// </summary>
	public sealed class PXRowUpdatingEventArgs : CancelEventArgs
	{
		private readonly object _Row;
		private readonly object _NewRow;
		private readonly bool _ExternalCall;

		public PXRowUpdatingEventArgs(object row, object newrow, bool externalCall)
		{
			_Row = row;
			_NewRow = newrow;
			_ExternalCall = externalCall;
		}

		public object Row
		{
			get
			{
				return _Row;
			}
		}
		public object NewRow
		{
			get
			{
				return _NewRow;
			}
		}

        /// <summary>
        /// Means that transaction has been initiated from the UI.<br/>
        /// More precisely, the method Update(IDictionary, IDictionary) has been invoked.<br/>
        /// </summary>
		public bool ExternalCall
		{
			get
			{
				return _ExternalCall;
			}
		}
	}

	public delegate void PXRowUpdated(PXCache sender, PXRowUpdatedEventArgs e);

    /// <summary>
    /// Occurs after the row in the cache has been updated successfully.<br/>
    /// This occurs when cache Update method is invoked from API or from user interface.<br/>
    /// Update method searches the row in the cache and in the database, if row is found then RowUpdated event is raised, else RowInserted.
    /// </summary>
	public sealed class PXRowUpdatedEventArgs : EventArgs
	{
		private readonly object _Row;
		private readonly object _OldRow;
		private readonly bool _ExternalCall;

		public PXRowUpdatedEventArgs(object row, object oldRow, bool externalCall)
		{
			_Row = row;
			_OldRow = oldRow;
			_ExternalCall = externalCall;
		}

		public object Row
		{
			get
			{
				return _Row;
			}
		}
		public object OldRow
		{
			get
			{
				return _OldRow;
			}
		}

        /// <summary>
        /// Means that this event has been initiated from the UI.<br/>
        /// More precisely, the method Update(IDictionary, IDictionary) has been invoked.<br/>
        /// </summary>
		public bool ExternalCall
		{
			get
			{
				return _ExternalCall;
			}
		}
	}

	public delegate void PXRowInserting(PXCache sender, PXRowInsertingEventArgs e);

    /// <summary>
    /// Occurs before inserting the row into the cache.<br/>
    /// This occurs when cache Insert method is invoked from API or from user interface.<br/>
    /// This event also is raised when Update method is invoked for the row, that does not exists in the cache and database.<br/>
    /// The event allows to cancel Insert procedure.
    /// </summary>
	public sealed class PXRowInsertingEventArgs : CancelEventArgs
	{
		private readonly object _Row;
		private readonly bool _ExternalCall;

		public PXRowInsertingEventArgs(object row, bool externalCall)
		{
			_Row = row;
			_ExternalCall = externalCall;
		}

		public object Row
		{
			get
			{
				return _Row;
			}
		}

        /// <summary>
        /// Means that transaction has been initiated from the UI.<br/>
        /// More precisely, the method Insert(IDictionary) has been invoked.<br/>
        /// </summary>
		public bool ExternalCall
		{
			get
			{
				return _ExternalCall;
			}
		}
	}

	public delegate void PXRowInserted(PXCache sender, PXRowInsertedEventArgs e);

    /// <summary>
    /// Occurs after the row has been inserted successfully into the cache.<br/>
    /// This occurs when cache Insert method is invoked from API or from user interface.<br/>
    /// This event also is raised when Update method is invoked for the row, that does not exists in the cache and database.<br/>
    /// </summary>
	public sealed class PXRowInsertedEventArgs : EventArgs
	{
		private readonly object _Row;
		private readonly bool _ExternalCall;

		public PXRowInsertedEventArgs(object row, bool externalCall)
		{
			_Row = row;
			_ExternalCall = externalCall;
		}

		public object Row
		{
			get
			{
				return _Row;
			}
		}

        /// <summary>
        /// Means that transaction has been initiated from the UI.<br/>
        /// More precisely, the method Insert(IDictionary) has been invoked.<br/>
        /// </summary>
		public bool ExternalCall
		{
			get
			{
				return _ExternalCall;
			}
		}
	}

	public delegate void PXRowDeleting(PXCache sender, PXRowDeletingEventArgs e);

    /// <summary>
    /// Occcurs when cache Delete method is invoked from API or from user interface.<br/>
    /// Delete method searches the row in the cache and in the database, if row is found - it marked with status Deleted or InsertedDeleted.
    /// This event allows to cancel Delete procedure.
    /// </summary>
	public sealed class PXRowDeletingEventArgs : CancelEventArgs
	{
		private readonly object _Row;
		private readonly bool _ExternalCall;

		public PXRowDeletingEventArgs(object row, bool externalCall)
		{
			_Row = row;
			_ExternalCall = externalCall;
		}

		public object Row
		{
			get
			{
				return _Row;
			}
		}

        /// <summary>
        /// Means that transaction has been initiated from the UI.<br/>
        /// More precisely, the method Delete(IDictionary) has been invoked.<br/>
        /// </summary>
		public bool ExternalCall
		{
			get
			{
				return _ExternalCall;
			}
		}
	}

	public delegate void PXRowDeleted(PXCache sender, PXRowDeletedEventArgs e);

    /// <summary>
    /// Occurs after the row has been marked as deleted in the cache.<br/>
    /// This occcurs when cache Delete method is invoked from API or from user interface.<br/>
    /// Delete method searches the row in the cache and in the database, if row is found - it marked with status Deleted or InsertedDeleted.
    /// </summary>
	public sealed class PXRowDeletedEventArgs : EventArgs
	{
		private readonly object _Row;
		private readonly bool _ExternalCall;

		public PXRowDeletedEventArgs(object row, bool externalCall)
		{
			_Row = row;
			_ExternalCall = externalCall;
		}

		public object Row
		{
			get
			{
				return _Row;
			}
		}

        /// <summary>
        /// Means that transaction has been initiated from the UI.<br/>
        /// More precisely, the method Delete(IDictionary) has been invoked.<br/>
        /// </summary>
		public bool ExternalCall
		{
			get
			{
				return _ExternalCall;
			}
		}
	}

	public delegate void PXRowSelected(PXCache sender, PXRowSelectedEventArgs e);
    /// <summary>
    /// Occurs every time a value is assigned to PXCache::Current property.<br/>
    /// This occurs for each row selected by the user interface.<br/>
    /// API methods Inser and Update also causes this event.
    /// </summary>
	public sealed class PXRowSelectedEventArgs : EventArgs
	{
		private readonly object _Row;

		public PXRowSelectedEventArgs(object row)
		{
			_Row = row;
		}

		public object Row
		{
			get
			{
				return _Row;
			}
		}
	}

	public delegate void PXCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e);

    /// <summary>
    /// Occurs each time when the system generates SQL statement for SELECT, INSERT, UPDATE, and DELETE operations.<br/>
    /// This event occurs for each cache field.<br/>
    /// Event subscribers should provide field description<br/>
    /// which allows create sql statements related to this field.<br/>
    /// </summary>
	public sealed class PXCommandPreparingEventArgs : CancelEventArgs
	{
		public sealed class FieldDescription
		{
			public readonly Type BqlTable;
			public readonly string FieldName;
			public PXDbType DataType;
			public readonly int? DataLength;
			public readonly object DataValue;
			public readonly bool IsRestriction;
			internal FieldDescription(Type bqlTable, string fieldName, PXDbType dataType, int? dataLength, object dataValue, bool isRestriction)
			{
				BqlTable = bqlTable;
				FieldName = fieldName;
				DataType = dataType;
				DataLength = dataLength;
				DataValue = dataValue;
				IsRestriction = isRestriction;
			}
		}
		public FieldDescription GetFieldDescription()
		{
			return new FieldDescription(_BqlTable, _FieldName, _DataType, _DataLength, _DataValue, _IsRestriction);
		}
		private readonly object _Row;
		private object _Value;
		private readonly PXDBOperation _Operation;
		private readonly Type _Table;
		private Type _BqlTable;
		private string _FieldName;
		private PXDbType _DataType = PXDbType.Unspecified;
		private int? _DataLength;
		private object _DataValue;
		private bool _IsRestriction;

		public PXCommandPreparingEventArgs(object row, object value, PXDBOperation operation, Type table)
		{
			_Row = row;
			_Value = value;
			_Operation = operation;
			_Table = table;
		}

		public object Row
		{
			get
			{
				return _Row;
			}
		}
		public object Value
		{
			get
			{
				return _Value;
			}
			set
			{
				_Value = value;
			}
		}
		public PXDBOperation Operation
		{
			get
			{
				return _Operation;
			}
		}
		public Type Table
		{
			get
			{
				return _Table;
			}
		}
		public Type BqlTable
		{
			get
			{
				return _BqlTable;
			}
			set
			{
				_BqlTable = value;
			}
		}
		public string FieldName
		{
			get
			{
				return _FieldName;
			}
			set
			{
				_FieldName = value;
			}
		}
		public PXDbType DataType
		{
			get
			{
				return _DataType;
			}
			set
			{
				_DataType = value;
			}
		}
		public int? DataLength
		{
			get
			{
				return _DataLength;
			}
			set
			{
				_DataLength = value;
			}
		}
		public object DataValue
		{
			get
			{
				return _DataValue;
			}
			set
			{
				_DataValue = value;
			}
		}
		public bool IsRestriction
		{
			get
			{
				return _IsRestriction;
			}
			set
			{
				_IsRestriction = value;
			}
		}
	}

	public delegate void PXRowSelecting(PXCache sender, PXRowSelectingEventArgs e);
    /// <summary>
    /// Occurs after the database select command is completed and the data row need to be created from the data reader.<br />
    /// For example, this event is raised for each row when any PXSelect command is executed. 
    /// </summary>
	public sealed class PXRowSelectingEventArgs : CancelEventArgs
	{
		private object _Row;
		private readonly PXDataRecord _Record;
		private int _Position;
		private readonly bool _IsReadOnly;

		public PXRowSelectingEventArgs(object row, PXDataRecord record, int position, bool isReadOnly)
		{
			_Row = row;
			_Record = record;
			_Position = position;
			_IsReadOnly = isReadOnly;
		}

		public object Row
		{
			get
			{
				return _Row;
			}
			internal set
			{
				_Row = value;
			}
		}
		public PXDataRecord Record
		{
			get
			{
				return _Record;
			}
		}
		public int Position
		{
			get
			{
				return _Position;
			}
			set
			{
				_Position = value;
			}
		}
		public bool IsReadOnly
		{
			get
			{
				return _IsReadOnly;
			}
		}
	}

	public delegate void PXRowPersisting(PXCache sender, PXRowPersistingEventArgs e);

    /// <summary>
    /// Occurs before saving the row to the database.<br/>
    /// When this event occurs, the database transaction is opened and some other rows may already be persisted.<br/>
    /// This event allows to cancel row persisting.<br/>
    /// Rows persisted in the following order: first Inserted rows from all caches, then Updated rows, then Deleted rows.<br/>
    /// </summary>
	public sealed class PXRowPersistingEventArgs : CancelEventArgs
	{
		private readonly object _Row;
		private readonly PXDBOperation _Operation;

		public PXRowPersistingEventArgs(PXDBOperation operation, object row)
		{
			_Row = row;
			_Operation = operation;
		}

		public object Row
		{
			get
			{
				return _Row;
			}
		}
		public PXDBOperation Operation
		{
			get
			{
				return _Operation;
			}
		}
	}

	public delegate void PXRowPersisted(PXCache sender, PXRowPersistedEventArgs e);

    /// <summary>
    /// Occurs two times for the same row in different contexts.<br/>
    /// First time within opened database transaction just after related sql statement fineshed (this event occurs with flag PXTranStatus.Open)<br/>
    /// Rows persisted in the following order: first Inserted rows from all caches, then Updated rows, then Deleted rows.<br/>
    /// Second time after transaction is completed or aborted this event occurs for all persisted rows in the same order with flag  PXTranStatus.Aborted or PXTranStatus.Completed<br/>
    /// </summary>
	public sealed class PXRowPersistedEventArgs : EventArgs
	{
		private readonly object _Row;
		private readonly PXDBOperation _Operation;
		private readonly PXTranStatus _TranStatus;
		private readonly Exception _Exception;

		public PXRowPersistedEventArgs(object row, PXDBOperation operation, PXTranStatus tranStatus, Exception exception)
		{
			_Row = row;
			_Operation = operation;
			_TranStatus = tranStatus;
			_Exception = exception;
		}

		public object Row
		{
			get
			{
				return _Row;
			}
		}
		public PXTranStatus TranStatus
		{
			get
			{
				return _TranStatus;
			}
		}
		public PXDBOperation Operation
		{
			get
			{
				return _Operation;
			}
		}
		public Exception Exception
		{
			get
			{
				return _Exception;
			}
		}
	}

	public delegate void PXFieldSelecting(PXCache sender, PXFieldSelectingEventArgs args);

    /// <summary>
    /// Occurs when the field value is retrieving by the external accessor.<br/>
    /// This event is raised by methods GetValueExt, GetStateExt, and when row is selected to user interface.<br/>
    /// </summary>
	public sealed class PXFieldSelectingEventArgs : CancelEventArgs
	{
		private readonly object _Row;
		private object _ReturnValue;
		private bool _IsAltered;
		private readonly bool _ExternalCall;

		public PXFieldSelectingEventArgs(object row, object returnValue, bool isAltered, bool externalCall)
		{
			_Row = row;
			_ReturnValue = returnValue;
			_IsAltered = isAltered;
			_ExternalCall = externalCall;
		}
		public object Row
		{
			get
			{
				return _Row;
			}
		}
		public object ReturnState
		{
			get
			{
				return _ReturnValue;
			}
			set
			{
				_ReturnValue = value;
			}
		}
		public bool IsAltered
		{
			get
			{
				return _IsAltered;
			}
			set
			{
				_IsAltered = value;
			}
		}
		public object ReturnValue
		{
			get
			{
				PXFieldState state = _ReturnValue as PXFieldState;
				if (state == null)
				{
					return _ReturnValue;
				}
				else
				{
					return state.Value;
				}
			}
			set
			{
				PXFieldState state = _ReturnValue as PXFieldState;
				if (state == null)
				{
					_ReturnValue = value;
				}
				else
				{
					state.Value = value;
				}
			}
		}
		public bool ExternalCall
		{
			get
			{
				return _ExternalCall;
			}
		}
	}

	public delegate void PXFieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs args);

    /// <summary>
    /// Event is raised to obtain default value for the field.<br/>
    /// Event is raised by methods SetDefaultExt, and when a new row inserted from API or from UI.<br/>
    /// Update and Delete methods does not triggers this event.<br/>
    /// OnFieldUpdating event is raised after this event.<br/>
    /// </summary>
	public sealed class PXFieldDefaultingEventArgs : CancelEventArgs
	{
		private readonly object _Row;
		private object _NewValue;

		public PXFieldDefaultingEventArgs(object row)
		{
			_Row = row;
		}
		public object Row
		{
			get
			{
				return _Row;
			}
		}
		public object NewValue
		{
			get
			{
				return _NewValue;
			}
			set
			{
				_NewValue = value;
			}
		}
	}

	public delegate void PXFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs args);

    /// <summary>
    /// Occurs when a new value of the field needs to be formated<br/>
    /// This event is raised to convert the value received from an external source, for example from user interface.<br/>
    /// Type of value may differ from the type of data field<br/>
    /// For example, it can be of type string while the field has a boolean type<br/>
    /// This event is raised by methods SetValueExt, SetDefaultExt,<br/> 
    /// and for each value received from the user interface when user inserts or updates rows.<br/>
    /// OnFieldVerifying and OnFieldUpdated events are raised after this event.<br/> 
    /// </summary>
	public sealed class PXFieldUpdatingEventArgs : CancelEventArgs
	{
		private readonly object _Row;
		private object _NewValue;

		public PXFieldUpdatingEventArgs(object row, object newValue)
		{
			_Row = row;
			_NewValue = newValue;
		}
		public object Row
		{
			get
			{
				return _Row;
			}
		}
		public object NewValue
		{
			get
			{
				return _NewValue;
			}
			set
			{
				_NewValue = value;
			}
		}
	}

	public delegate void PXFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs args);

    /// <summary>
    /// Occurs when a new value of the field needs to be verified.<br/>
    /// This event is raised before a new value is assigned to data field.<br/>
    /// This event is raised by methods SetValueExt, SetDefaultExt,<br/> 
    /// and for each new value when Insert or Update  method is invoked.<br/>
    /// OnFieldUpdated event is raised after this event.<br/> 
    /// </summary>
	public sealed class PXFieldVerifyingEventArgs : CancelEventArgs
	{
		private readonly object _Row;
		private object _NewValue;
		private readonly bool _ExternalCall;

		public PXFieldVerifyingEventArgs(object row, object newValue, bool externalCall)
		{
			_Row = row;
			_NewValue = newValue;
			_ExternalCall = externalCall;
		}
		public object Row
		{
			get
			{
				return _Row;
			}
		}
		public object NewValue
		{
			get
			{
				return _NewValue;
			}
			set
			{
				_NewValue = value;
			}
		}
		public bool ExternalCall
		{
			get
			{
				return _ExternalCall;
			}
		}
	}

	public delegate void PXFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs args);

    /// <summary>
    /// Occurs after the field has been successfully updated.<br/>
    /// This event is raised each time after OnFieldVerifying event is raised.<br/>
    /// This event is raised by methods SetValueExt, SetDefaultExt,<br/> 
    /// and for each new value when Insert or Update method is invoked.<br/>
    /// </summary>
	public sealed class PXFieldUpdatedEventArgs : EventArgs
	{
		private readonly object _Row;
		private readonly object _OldValue;
		private readonly bool _ExternalCall;

		public PXFieldUpdatedEventArgs(object row, object oldValue, bool externalCall)
		{
			_Row = row;
			_OldValue = oldValue;
			_ExternalCall = externalCall;
		}

		public object Row
		{
			get
			{
				return _Row;
			}
		}
		public object OldValue
		{
			get
			{
				return _OldValue;
			}
		}
		public bool ExternalCall
		{
			get
			{
				return _ExternalCall;
			}
		}
	}

	public delegate void PXExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs args);

    /// <summary>
    /// Occurs when an exception needs to be handled.<br/>
    /// This event is raised in two scenarios.<br/>
    /// First, when PXSetPropertyException is raised while field updating workflow.<br/>
    /// Second, when data row is to be saved to database and PXCommandPreparingException or PXRowPersistingException is thrown.<br/>
    /// </summary>
	public sealed class PXExceptionHandlingEventArgs : CancelEventArgs
	{
		private readonly object _Row;
		private object _NewValue;
		private readonly Exception _Exception;

		public PXExceptionHandlingEventArgs(object row, object newValue, Exception exception)
		{
			_Row = row;
			_NewValue = newValue;
			_Exception = exception;
		}
		public object Row
		{
			get
			{
				return _Row;
			}
		}
		public object NewValue
		{
			get
			{
				return _NewValue;
			}
			set
			{
				_NewValue = value;
			}
		}
		public Exception Exception
		{
			get
			{
				return _Exception;
			}
		}
	}
}

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
using System.Runtime.Serialization;
using PX.Common;

namespace PX.Data
{
	public class PXOverridableException : PXException
	{
		// Field name to map exception to
		protected string _MapErrorTo = null;

		/// <summary>
		/// Gets oe Set fieldname to map exception to
		/// </summary>
		public string MapErrorTo
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

		public PXOverridableException(string message, Exception inner)
			: base(message, inner)
		{
			_Message = base.MessageNoPrefix;
		}
		// Formats exception message like String.Format() does
		public PXOverridableException(Exception inner, string format, params object[] args)
			: base(inner, format, args)
		{
			_Message = base.MessageNoPrefix;
		}
		public PXOverridableException(string message)
			: base(message)
		{
			_Message = base.MessageNoPrefix;
		}
		// Formats exception message like String.Format() does
		public PXOverridableException(string format, params object[] args)
			: base(format, args)
		{
			_Message = base.MessageNoPrefix;
		}
		public override string Message
		{
			get
			{
				if (MessagePrefix != null)
					return string.Format("{0} #{1}: {2}", MessagePrefix, ExceptionNumber, _Message);

				if (ExceptionNumber == 0)
					return _Message;
				else
					return String.Format("{0}: {1}", ExceptionNumber, _Message);
			}
		}
		public override string MessageNoNumber
		{
			get
			{
				if (MessagePrefix != null)
					return string.Format("{0}: {1}", MessagePrefix, _Message);
				return _Message;
			}
		}
		public virtual void SetMessage(string message)
		{
			if (MessagePrefix != null)
			{
				string strtemp = string.Format("{0} #{1}:", MessagePrefix, ExceptionNumber);
				int idx = message.IndexOf(strtemp);
				if (idx != -1)
				{
					message = message.Substring(message.IndexOf(strtemp) + strtemp.Length);
				}
			}
			_Message = PXMessages.Localize(message);
		}

		public PXOverridableException(SerializationInfo info, StreamingContext context)
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

	[Serializable]
	public class PXOperationCompletedException : PXOverridableException
	{ 
		public PXOperationCompletedException(string message, Exception inner)
			: base(message, inner)
		{
		}

		public PXOperationCompletedException(Exception inner, string format, params object[] args)
			: base(inner, format, args)
		{
		}
		public PXOperationCompletedException(string message)
			: base(message)
		{
		}

		public PXOperationCompletedException(string format, params object[] args)
			: base(format, args)
		{
		}

		public PXOperationCompletedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
					
		}
	}

    /// <summary>
    /// Exception type which is used to indicate that required data isn't entered into setup screen
    /// </summary>
    [Serializable]
    public class PXSetupNotEnteredException : PXSetPropertyException
    {
        //DAC which doesn't containt required data
        private Type _DAC;
        //Store page name where user can navigate to enter required data
        private string _navigateTo;

        public PXSetupNotEnteredException(string format, Type inpDAC, params object[] args)
            : base(format, args)
        {
            _DAC = inpDAC;            
            _navigateTo = args[0].ToString();
        }

        public Type DAC
        {
            get
            {
                return _DAC;
            }
        }

        public string NavigateTo
        {
            get
            {
                return _navigateTo;
            }
        }

		public PXSetupNotEnteredException(SerializationInfo info, StreamingContext context)
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

	[Serializable]
	public class PXSetPropertyException : PXOverridableException
	{
		protected PXErrorLevel _ErrorLevel = PXErrorLevel.Error;
		protected object _ErrorValue;

		public object ErrorValue
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

		public PXErrorLevel ErrorLevel
		{
			get
			{
				return this._ErrorLevel;
			}
		}

		public override string Message
		{
			get
			{
				return base.MessageNoNumber;
			}
		}

		public override void SetMessage(string message)
		{
			if (MessagePrefix != null)
			{
				string strtemp = string.Format("{0}:", MessagePrefix, ExceptionNumber);
				int idx = message.IndexOf(strtemp);
				if (idx != -1)
				{
					message = message.Substring(message.IndexOf(strtemp) + strtemp.Length);
				}
			}
			_Message = PXMessages.LocalizeNoPrefix(message);
		}

		public PXSetPropertyException(string message)
			: base(message)
		{
		}

		public PXSetPropertyException(string message, PXErrorLevel errorLevel)
			: this(message)
		{
			this._ErrorLevel = errorLevel;
		}

		public PXSetPropertyException(string format, params object[] args)
			: base(format, args)
		{
		}

		public PXSetPropertyException(string format, PXErrorLevel errorLevel, params object[] args)
			: base(format, args)
		{
			this._ErrorLevel = errorLevel;
		}

        public PXSetPropertyException(Exception inner, PXErrorLevel errorLevel, string format, params object[] args)
			: base(inner, format, args)
		{
            this._ErrorLevel = errorLevel;
        }

		public PXSetPropertyException(SerializationInfo info, StreamingContext context)
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

	public class PXFieldProcessingException : PXSetPropertyException
	{
		public readonly string FieldName;

        protected PXFieldProcessingException(string fieldName, Exception inner, PXErrorLevel errorLevel, string format, params object[] args)
			: base(inner, errorLevel, format, args)
		{
			FieldName = fieldName;
		}

		public PXFieldProcessingException(string fieldName, Exception inner, PXErrorLevel errorLevel, params object[] args)
			:this(fieldName, inner, errorLevel, ErrorMessages.ErrorFieldProcessing, args)
		{
		}

		public PXFieldProcessingException(SerializationInfo info, StreamingContext context)
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

	public class PXFieldValueProcessingException : PXFieldProcessingException
	{
		public PXFieldValueProcessingException(string fieldName, Exception inner, PXErrorLevel errorLevel, params object[] args)
			: base(fieldName, inner, errorLevel, ErrorMessages.ErrorFieldValueProcessing, args)
		{
		}

		public PXFieldValueProcessingException(SerializationInfo info, StreamingContext context)
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
	
	public class PXSetPropertyKeepPreviousException : PXSetPropertyException
	{
		public PXSetPropertyKeepPreviousException(string message)
			: base(message)
		{
		}

		public PXSetPropertyKeepPreviousException(string message, PXErrorLevel errorLevel)
			: base(message, errorLevel)
		{
		}

		public PXSetPropertyKeepPreviousException(string format, PXErrorLevel errorLevel, params object[] args)
			: base(format, errorLevel, args)
		{
		}

        public PXSetPropertyKeepPreviousException(Exception inner, PXErrorLevel errorLevel, string format, params object[] args)
			: base(inner, errorLevel, format, args)
		{
        }

		public PXSetPropertyKeepPreviousException(SerializationInfo info, StreamingContext context)
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

	public class PXSetPropertyException<Field> : PXSetPropertyException
		where Field : IBqlField 
	{
		public PXSetPropertyException(string message)
			: base(message)
		{
			this._MapErrorTo = typeof(Field).Name;
		}

		public PXSetPropertyException(string message, PXErrorLevel errorLevel)
			: base(message, errorLevel)
		{
			this._MapErrorTo = typeof(Field).Name;
		}

		public PXSetPropertyException(string format, params object[] args)
			: base(format, args)
		{
			this._MapErrorTo = typeof(Field).Name;
		}

		public PXSetPropertyException(string format, PXErrorLevel errorLevel, params object[] args)
			: base(format, errorLevel, args)
		{
			this._MapErrorTo = typeof(Field).Name;
		}

		public PXSetPropertyException(Exception inner, PXErrorLevel errorLevel, string format, params object[] args)
			: base(inner, errorLevel, format, args)
		{
			this._MapErrorTo = typeof(Field).Name;
		}

		public PXSetPropertyException(SerializationInfo info, StreamingContext context)
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

	public class PXForeignRecordDeletedException : PXSetPropertyException
	{
		public PXForeignRecordDeletedException()
			: base(ErrorMessages.ForeignRecordDeleted)
		{
		}

		public PXForeignRecordDeletedException(SerializationInfo info, StreamingContext context)
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

	public class PXLockViolationException : PXOverridableException
	{
		private PXDBOperation _Operation;
		public PXDBOperation Operation
		{
			get
			{
				return _Operation;
			}
			set
			{
				_Operation = value;
			}
		}
		private Type _Table;
		public Type Table
		{
			get
			{
				return _Table;
			}
			set
			{
				_Table = value;
			}
		}
		private bool _Retry;
		public bool Retry
		{
			get
			{
				return _Retry;
			}
			set
			{
				_Retry = value;
			}
		}
		private object[] _Keys;
		public object[] Keys
		{
			get 
			{ 
				return _Keys; 
			}
			set 
			{ 
				_Keys = value;
			}
		}
		public override string Message
		{
			get
			{
				switch (_Operation)
				{
					case PXDBOperation.Insert:
						_Message = PXMessages.LocalizeFormat(ErrorMessages.RecordAddedByAnotherProcess, out _ExceptionNumber, out _MessagePrefix, _Table.Name, _Retry ? ErrorMessages.RetrySavingRecord : ErrorMessages.ChangesWillBeLost);
						break;
					case PXDBOperation.Update:
						_Message = PXMessages.LocalizeFormat(ErrorMessages.RecordUpdatedByAnotherProcess, out _ExceptionNumber, out _MessagePrefix, _Table.Name);
						break;
					case PXDBOperation.Delete:
						_Message = PXMessages.LocalizeFormat(ErrorMessages.RecordDeletedByAnotherProcess, out _ExceptionNumber, out _MessagePrefix, _Table.Name);
						break;
				}
				return base.Message;
			}
		}

		public PXLockViolationException(Type table, PXDBOperation operation, object[] keys)
			: base("")
		{
			_Table = table;
			_Operation = operation;
			_Keys = keys;
		}

		public PXLockViolationException(SerializationInfo info, StreamingContext context)
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

	public class PXCommandPreparingException : PXOverridableException
	{
		public readonly string Name;
		public readonly object Value;
		public PXCommandPreparingException(string name, object value, string message)
			: base(message)
		{
			Name = name;
			Value = value;
		}
		public PXCommandPreparingException(string name, object value, string format, params object[] args)
			: base(format, args)
		{
			Name = name;
			Value = value;
		}

		public PXCommandPreparingException(SerializationInfo info, StreamingContext context)
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

	public class PXRowPersistingException : PXOverridableException
	{
		public readonly string Name;
		public readonly object Value;
		public PXRowPersistingException(string name, object value, string message)
			: base(message)
		{
			Name = name;
			Value = value;
		}
		public PXRowPersistingException(string name, object value, string format, params object[] args)
			: base(format, args)
		{
			Name = name;
			Value = value;
		}

		public PXRowPersistingException(SerializationInfo info, StreamingContext context)
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

	public class PXRowPersistedException : PXOverridableException
	{
		public readonly string Name;
		public readonly object Value;
		public PXRowPersistedException(string name, object value, string message)
			: base(message)
		{
			Name = name;
			Value = value;
		}
		public PXRowPersistedException(string name, object value, string format, params object[] args)
			: base(format, args)
		{
			Name = name;
			Value = value;
		}

		public PXRowPersistedException(SerializationInfo info, StreamingContext context)
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

	public class PXRestrictionViolationException : PXOverridableException
	{
		public readonly int Index;
		public PXRestrictionViolationException(string message, object[] keys, int index)
			: base(message, keys)
		{
			Index = index;
		}
		public PXRestrictionViolationException(SerializationInfo info, StreamingContext context)
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

	public class PXDatabaseException : PXOverridableException
	{
		protected string _Table;
		public string Table
		{
			get { return _Table; }
		}

		protected object[] _Keys;
		public object[] Keys
		{
			get	{ return _Keys;	}
			set { _Keys = value; }
		}

		protected PXDbExceptions _ErrorCode;
		public PXDbExceptions ErrorCode
		{
			get { return _ErrorCode; }
		}

		protected bool _IsFriendlyMessage;
		public bool IsFriendlyMessage
		{
			get { return _IsFriendlyMessage; }
			set { _IsFriendlyMessage = value; }
		}

		public PXDatabaseException(string table, object[] keys, PXDbExceptions errCode, string message, Exception inner)
			: base(message, inner)
		{
			_Table = table;
			_Keys = keys;
			_ErrorCode = errCode;
		}

		public PXDatabaseException(string table, object[] keys, string message, Exception inner)
			: base(message, inner)
		{
			_Table = table;
			_Keys = keys;
			_ErrorCode = PXDbExceptions.Unknown;
		}

		public PXDatabaseException(string table, object[] keys, PXDbExceptions errCode, string message)
			: base(message)
		{
			_Table = table;
			_Keys = keys;
			_ErrorCode = errCode;
		}

		public PXDatabaseException(string table, object[] keys, string message)
			: base(message)
		{
			_Table = table;
			_Keys = keys;
			_ErrorCode = PXDbExceptions.Unknown;
		}

		public PXDatabaseException(SerializationInfo info, StreamingContext context)
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

	public class PXVisibiltyUpdateRequiredException : PXDatabaseException
	{
		public PXVisibiltyUpdateRequiredException(String tableName)
			: base(tableName, null, PXDbExceptions.OperationSwitchRequired, "Visibility update of the shared record required.") {}
	}
	public class PXInsertSharedRecordRequiredException : PXDatabaseException 
	{
		public PXInsertSharedRecordRequiredException(String tableName)
			: base(tableName, null, PXDbExceptions.OperationSwitchRequired, "Insert of a shared record required.") {}
	}

    public class PXUnderMaintenanceException : Exception
    {
        public PXUnderMaintenanceException()
            : base(ErrorMessages.SiteUnderMaintenance)
        {
        }

		public PXUnderMaintenanceException(SerializationInfo info, StreamingContext context)
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
	public class PXLicenseExceededException : Exception
	{
		public String Url { get; private set; }
		public String Title { get; private set; }

		public PXLicenseExceededException(PXExeededReason reason)
			: base(GetRedirectMessage(reason))
		{
			Url = GetLoginUrl(reason);
			Title = PXMessages.LocalizeFormatNoPrefix(ActionsMessages.LogoutReason, reason.ToString());
		}

		public PXLicenseExceededException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			PXReflectionSerializer.RestoreObjectProps(this, info);
		}
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			PXReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}

		private static String GetLoginUrl(PXExeededReason reason)
		{
			String result = String.Concat(PX.Export.Authentication.FormsAuthenticationModule.LoginUrl,
				"?returnUrl=",
				System.Web.VirtualPathUtility.ToAbsolute("~/" + PX.Export.Authentication.FormsAuthenticationModule.DefaultUrl),
				"&licenseexceeded="
				,reason.ToString());

			if (System.Web.HttpContext.Current == null) return result;
			return PXSessionStateStore.GetSessionUrl(System.Web.HttpContext.Current, result);
		}
		private static String GetRedirectMessage(PXExeededReason reason)
		{
			return "Refresh|" + GetLoginUrl(reason) + "|";
		}
	}
	public class PXUndefinedCompanyException : Exception
	{
		public PXUndefinedCompanyException()
			: base(ErrorMessages.UndefinedCompany)
		{
		}
		public PXUndefinedCompanyException(SerializationInfo info, StreamingContext context)
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

	public class PXOuterException : PXException
	{
		protected Dictionary<string, string> _InnerExceptions;
		public virtual string[] InnerMessages
		{
			get
			{
				string[] ret = new string[_InnerExceptions.Count];
				_InnerExceptions.Values.CopyTo(ret, 0);
				return ret;
			}
		}
		public virtual string[] InnerFields
		{
			get
			{
				string[] ret = new string[_InnerExceptions.Count];
				_InnerExceptions.Keys.CopyTo(ret, 0);
				return ret;
			}
		}
		public virtual void InnerRemove(string fieldName)
		{
			_InnerExceptions.Remove(fieldName);
		}

		protected Type _GraphType;
		public virtual Type GraphType
		{
			get
			{
				return _GraphType;
			}
		}

		protected object _Row;
		public virtual object Row
		{
			get
			{
				return _Row;
			}
		}

		public PXOuterException(Dictionary<string, string> innerExceptions, Type graphType, object row, string message)
			: base(message)
		{
			_InnerExceptions = innerExceptions;
			_GraphType = graphType;
			_Row = row;
		}

		// Formats exception message like String.Format() does
		public PXOuterException(Dictionary<string, string> innerExceptions, Type graphType, object row, string format, params object[] args)
			: base(format, args)
		{
			_InnerExceptions = innerExceptions;
			_GraphType = graphType;
			_Row = row;
		}

		public PXOuterException(SerializationInfo info, StreamingContext context)
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

	public class PXNotEnoughRightsException : PXException
	{
		private PXCacheRights rightsMissing;

		public PXNotEnoughRightsException(PXCacheRights rightsMissing)
			: base(ErrorMessages.NotEnoughRights, rightsMissing.ToString())
		{
			this.rightsMissing = rightsMissing;
		}

		public PXNotEnoughRightsException(PXCacheRights rightsMissing, string message)
			: base(message)
		{
			this.rightsMissing = rightsMissing;
		}

		public PXCacheRights RightsMissing
		{
			get { return this.rightsMissing; }
		}
		public PXNotEnoughRightsException(SerializationInfo info, StreamingContext context)
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
}

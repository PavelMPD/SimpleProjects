namespace PX.Objects.GL
{
	using System;
	using PX.Data;
    using PX.Data.EP;
	using System.Collections;
	using System.Collections.Generic;
	[System.SerializableAttribute()]
	public partial class GLTranCode : PX.Data.IBqlTable
	{
		#region Module
		public abstract class module : PX.Data.IBqlField
		{
		}
		protected String _Module;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault()]
        [PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.SelectorVisible)]
		[VoucherModule.List()]
        [PXFieldDescription]		
		public virtual String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
		#region TranType
		public abstract class tranType : PX.Data.IBqlField
		{
		}
		protected String _TranType;
		[PXDBString(3, IsKey = true, IsFixed = true)]        
		[PXDefault()]
        [PXUIField(DisplayName = "Module Tran. Type",Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region TranCode
		public abstract class tranCode : PX.Data.IBqlField
		{
		}
		protected String _TranCode;
		[PXDBString(5, IsUnicode = true,InputMask = ">aaaaa")]
		[PXDefault()]
        [PXCheckUnique()]
        [PXUIField(DisplayName = "Unique Tran. Code", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String TranCode
		{
			get
			{
				return this._TranCode;
			}
			set
			{
				this._TranCode = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
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
		#endregion
        #region Active
        public abstract class active : PX.Data.IBqlField
        {
        }
        protected bool? _Active;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Active")]
        public virtual bool? Active
        {
            get
            {
                return _Active;
            }
            set
            {
                _Active = value;
            }
        }
        #endregion
	}

	public static class VoucherModule 
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
					new string[] { GL.BatchModule.GL, GL.BatchModule.AP, GL.BatchModule.AR, GL.BatchModule.CA},
					new string[] { Messages.ModuleGL, Messages.ModuleAP, Messages.ModuleAR, Messages.ModuleCA }) { }
		}		
	}	
}

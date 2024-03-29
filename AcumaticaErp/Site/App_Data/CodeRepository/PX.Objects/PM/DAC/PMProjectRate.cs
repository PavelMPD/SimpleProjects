namespace PX.Objects.PM
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public partial class PMProjectRate : PX.Data.IBqlTable
	{
		#region RateDefinitionID
		public abstract class rateDefinitionID : PX.Data.IBqlField
		{
		}
		protected int? _RateDefinitionID;
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(PMRateSequence.rateDefinitionID))]
		[PXParent(typeof(Select<PMRateSequence, Where<PMRateSequence.rateDefinitionID, Equal<Current<PMProjectRate.rateDefinitionID>>>>))]
		public virtual int? RateDefinitionID
		{
			get
			{
				return this._RateDefinitionID;
			}
			set
			{
				this._RateDefinitionID = value;
			}
		}
		#endregion
		#region RateCodeID
		public abstract class rateCodeID : PX.Data.IBqlField
		{
		}
		protected String _RateCodeID;

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(PMRateSequence.rateCodeID))]
		public virtual String RateCodeID
		{
			get
			{
				return this._RateCodeID;
			}
			set
			{
				this._RateCodeID = value;
			}
		}
		#endregion
		#region ProjectCD
		public abstract class projectCD : PX.Data.IBqlField
		{
		}
		protected String _ProjectCD;
		[PXDimensionWildcardAttribute(ProjectAttribute.DimensionName, 
			typeof(Search<PMProject.contractCD, Where<PMProject.baseType, Equal<PMProject.ProjectBaseType>, And<PMProject.isTemplate, Equal<False>>>>),
			typeof(PMProject.contractCD), typeof(PMProject.description))]
		[PXDBString(IsKey = true, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName="Project")]
		public virtual String ProjectCD
		{
			get
			{
				return this._ProjectCD;
			}
			set
			{
				this._ProjectCD = value;
			}
		}
		#endregion
	}
}

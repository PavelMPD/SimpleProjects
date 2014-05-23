using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using System.Diagnostics;
using PX.Objects.CS;

namespace PX.Objects.PM
{
    [Serializable]
	public class ProjectAttributeGroupMaint : PXGraph<ProjectAttributeGroupMaint>
	{

		#region Actions/Buttons
		public PXSave<GroupTypeFilter> Save;
		public PXCancel<GroupTypeFilter> Cancel;
		#endregion

		#region Views/Selects

		public PXFilter<GroupTypeFilter> Filter;
		public PXSelect<CSAttributeGroup,
			Where<CSAttributeGroup.entityClassID, Equal<Current<GroupTypeFilter.classID>>,
			And<CSAttributeGroup.type, Equal<Current<GroupTypeFilter.entityType>>>>> Mapping;

		#endregion
		
		#region Event Handlers

		protected virtual void CSAttributeGroup_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CSAttributeGroup row = e.Row as CSAttributeGroup;

			if (row != null && Filter.Current != null)
			{
				row.EntityClassID = Filter.Current.ClassID;
				row.Type = Filter.Current.EntityType;
			}
		}

		#endregion

		#region Local Types
        [Serializable]
		public partial class GroupTypeFilter : IBqlTable
		{
			#region ClassID
			public abstract class classID : PX.Data.IBqlField
			{
			}
			protected string _ClassID;
			[PXUIField(DisplayName = "Name", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDBString(10, IsUnicode = true)]
			[GroupTypes.List()]
			[PXDefault(GroupTypes.Project)]
			public virtual String ClassID
			{
				get
				{
					return this._ClassID;
				}
				set
				{
					this._ClassID = value;
				}
			}
			#endregion
			#region EntityType
			public abstract class entityType : PX.Data.IBqlField
			{
			}
			[PXString(2, IsFixed = true)]
			public virtual String EntityType
			{
				get
				{
					switch (ClassID)
					{
						case GroupTypes.AccountGroup:
							return CSAnswerType.AccountGroup;
						case GroupTypes.Transaction:
							return CSAnswerType.ProjectTran;
						case GroupTypes.Task:
							return CSAnswerType.ProjectTask;
						case GroupTypes.Project:
							return CSAnswerType.Project;
						case GroupTypes.Equipment:
							return CSAnswerType.Equipment;

						default:
							return null;
					}
				}
				set
				{
				}
			}
			#endregion
		}

		#endregion

	}
}

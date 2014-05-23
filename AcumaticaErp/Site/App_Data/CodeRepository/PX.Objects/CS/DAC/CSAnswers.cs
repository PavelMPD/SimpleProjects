namespace PX.Objects.CS
{
	using System;
	using PX.Data;
	using System.Diagnostics;
	using PX.Objects.CA;
	using System.Collections.Generic;

	public class CSAttributeSelectorAttribute : PXSelectorAttribute
	{
		protected class Definition : IPrefetchable
		{
			public Dictionary<string, string> IDbyDescription = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			public Dictionary<string, string> DescriptionByID = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			public void Prefetch()
			{
				foreach (PXDataRecord record in PXDatabase.SelectMulti<CSAttribute>(
					new PXDataField<CSAttribute.attributeID>(),
					new PXDataField<CSAttribute.description>(),
					new PXDataFieldOrder<CSAttribute.attributeID>()
					))
				{
					string id = record.GetString(0);
					string description = record.GetString(1);
					IDbyDescription[description] = id;
					DescriptionByID[id] = description;
				}
			}
		}

		protected Definition _Definition;

		public CSAttributeSelectorAttribute()
			: base(typeof(CSAttribute.attributeID))
		{
			DescriptionField = typeof(CSAttribute.description);
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);
			if ((sender.Graph.IsImport || sender.Graph.IsExport) && e.ReturnValue is string && _Definition != null)
			{
				string description;
				if (!_Definition.IDbyDescription.ContainsKey((string)e.ReturnValue) && _Definition.DescriptionByID.TryGetValue((string)e.ReturnValue, out description))
				{
					e.ReturnValue = description;
				}
			}
		}

		public virtual void AttributeIDFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if ((sender.Graph.IsImport || sender.Graph.IsExport) && e.NewValue is string && _Definition != null)
			{
				string id;
				if (_Definition.IDbyDescription.TryGetValue((string)e.NewValue, out id) && !_Definition.DescriptionByID.ContainsKey((string)e.NewValue))
				{
					e.NewValue = id;
				}
				else if (id != null && String.Equals((string)e.NewValue, id, StringComparison.InvariantCultureIgnoreCase))
				{
					e.NewValue = id;
				}
				////
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_Definition = PX.Common.PXContext.GetSlot<Definition>();
			if (_Definition == null)
			{
				PX.Common.PXContext.SetSlot(_Definition = PXDatabase.GetSlot<Definition>(typeof(Definition).FullName, typeof(CSAttribute)));
			}

			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, AttributeIDFieldUpdating);
		}
	}

	[System.SerializableAttribute]
	[DebuggerDisplay("AttributeID={AttributeID}")]
	[PXCacheName(Messages.CSAnswers)]
	[PXPossibleRowsList(typeof(CSAttribute.description), typeof(CSAnswers.attributeID), typeof(CSAnswers.value))]
	public partial class CSAnswers : PX.Data.IBqlTable
	{
		#region EntityType
		public abstract class entityType : PX.Data.IBqlField
		{
		}
		protected String _EntityType;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Entity Type")]
		[CSAnswerType.List()]
		public virtual String EntityType
		{
			get
			{
				return this._EntityType;
			}
			set
			{
				this._EntityType = value;
			}
		}
		#endregion
		#region EntityID
		public abstract class entityID : PX.Data.IBqlField
		{
		}
		protected Int32? _EntityID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? EntityID
		{
			get
			{
				return this._EntityID;
			}
			set
			{
				this._EntityID = value;
			}
		}
		#endregion
		#region AttributeID
		public abstract class attributeID : PX.Data.IBqlField
		{
		}
		protected String _AttributeID;
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Attribute")]
		[CSAttributeSelector]
		public virtual String AttributeID
		{
			get
			{
				return this._AttributeID;
			}
			set
			{
				this._AttributeID = value;
			}
		}
		#endregion
		#region Value
		public abstract class value : PX.Data.IBqlField
		{
		}
		protected String _Value;
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Value")]
		[DynamicValueValidation(typeof(Search<CSAttribute.regExp, Where<CSAttribute.attributeID, Equal<Current<CSAnswers.attributeID>>>>))]
		public virtual String Value
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
		#endregion
		
		//#region EntityClassID
		//public abstract class entityClassID : PX.Data.IBqlField
		//{
		//}
		//protected String _EntityClassID;
		//[PXDBString(30, IsUnicode = true, IsKey = true)]
		//[PXDefault()]
		//[PXUIField(DisplayName = "Entity Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		//public virtual String EntityClassID
		//{
		//	get
		//	{
		//		return this._EntityClassID;
		//	}
		//	set
		//	{
		//		this._EntityClassID = value;
		//	}
		//}
		//#endregion

		#region IsRequired
		public abstract class isRequired : PX.Data.IBqlField
		{
		}
		protected bool? _IsRequired;
		[PXBool()]
		[PXUIField(DisplayName = "Required", IsReadOnly = true)]
		public virtual bool? IsRequired
		{
			get
			{
				return this._IsRequired;
			}
			set
			{
				this._IsRequired = value;
			}
		}
		#endregion

		#region Order
		public class order : IBqlField { }

		[PXShort]
		public Int16? Order { get; set; }
		#endregion
	}

	[Serializable]
	public class CSAnswers2 : CSAnswers
	{
		public new abstract class attributeID : IBqlField { }
		public new abstract class entityType : IBqlField { }
		public new abstract class entityID : IBqlField { }
		public new abstract class value : IBqlField { }
	}

	public static class CSAnswerType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Lead, Account, Case, Opportunity, InventoryItem, Project, ProjectTask, AccountGroup, ProjectTran, Equipment, Contract },
				new string[] { CR.Messages.Lead, CR.Messages.Account, CR.Messages.Case, CR.Messages.Opportunity, CR.Messages.InventoryItem, PM.Messages.GroupTypes_Project, PM.Messages.GroupTypes_Task, PM.Messages.GroupTypes_AccountGroup, PM.Messages.GroupTypes_Transaction, PM.Messages.GroupTypes_Equipment, CT.Messages.GroupTypes_Contract }) { ; }
		}

		public const string Lead = "LD";
		public const string Account = "AT";
		public const string Case = "CS";
		public const string Opportunity = "OP";
		public const string InventoryItem = "IN";
		public const string Project = "PR";
		public const string ProjectTask = "PT";
		public const string AccountGroup = "AG";
		public const string ProjectTran = "TR";
		public const string Equipment = "EQ";
		public const string Contract = "CT";

		public class caseAnswerType : Constant<string>
		{
			public caseAnswerType() : base(Case) { ;}
		}

		public class opportunityAnswerType : Constant<string>
		{
			public opportunityAnswerType() : base(Opportunity) { ;}
		}

		public class leadAnswerType : Constant<string>
		{
			public leadAnswerType() : base(CSAnswerType.Lead) { ;}
		}

		public class accountAnswerType : Constant<string>
		{
			public accountAnswerType() : base(CSAnswerType.Account) { ;}
		}

		public class inventoryAnswerType : Constant<string>
		{
			public inventoryAnswerType() : base(CSAnswerType.InventoryItem) { ;}
		}

		public class projectAnswerType : Constant<string>
		{
			public projectAnswerType() : base(CSAnswerType.Project) { ;}
		}

		public class projectTaskAnswerType : Constant<string>
		{
			public projectTaskAnswerType() : base(CSAnswerType.ProjectTask) { ;}
		}

		public class accountGroupAnswerType : Constant<string>
		{
			public accountGroupAnswerType() : base(CSAnswerType.AccountGroup) { ;}
		}

		public class projectTranAnswerType : Constant<string>
		{
			public projectTranAnswerType() : base(CSAnswerType.ProjectTran) { ;}
		}

		public class equipmentAnswerType : Constant<string>
		{
			public equipmentAnswerType() : base(CSAnswerType.Equipment) { ;}
		}

		public class contractAnswerType : Constant<string>
		{
			public contractAnswerType() : base(CSAnswerType.Contract) { ;}
		}

		public static string GetAnswerType(Type entityType)
		{
			if (typeof (CR.Contact).IsAssignableFrom(entityType)) return Lead;
			if (typeof (CR.BAccount).IsAssignableFrom(entityType)) return Account;
			if (typeof (CR.CRCase).IsAssignableFrom(entityType)) return Case;
			if (typeof (CR.CROpportunity).IsAssignableFrom(entityType)) return Opportunity;
			if (typeof (IN.InventoryItem).IsAssignableFrom(entityType)) return InventoryItem;
			if (typeof (PM.PMProject).IsAssignableFrom(entityType)) return Project;
			if (typeof(PM.PMTask).IsAssignableFrom(entityType)) return ProjectTask;
			if (typeof(PM.PMAccountGroup).IsAssignableFrom(entityType)) return AccountGroup;
			if (typeof (PM.PMTran).IsAssignableFrom(entityType)) return ProjectTran;
			if (typeof (EP.EPEquipment).IsAssignableFrom(entityType)) return Equipment;
			
			return null;
		}

		public static string GetEntityID(Type entityType)
		{
			if (typeof(CR.Contact).IsAssignableFrom(entityType)) return typeof(CR.Contact.contactID).Name;
			if (typeof(CR.BAccount).IsAssignableFrom(entityType)) return typeof(CR.BAccount.bAccountID).Name;
			if (typeof(CR.CRCase).IsAssignableFrom(entityType)) return typeof(CR.CRCase.caseID).Name;
			if (typeof(CR.CROpportunity).IsAssignableFrom(entityType)) return typeof(CR.CROpportunity.opportunityNumber).Name;
			if (typeof(IN.InventoryItem).IsAssignableFrom(entityType)) return typeof(IN.InventoryItem.inventoryID).Name;
			if (typeof(PM.PMProject).IsAssignableFrom(entityType)) return typeof(PM.PMProject.contractID).Name;
			if (typeof(PM.PMTask).IsAssignableFrom(entityType)) return typeof(PM.PMTask.taskID).Name;
			if (typeof(PM.PMAccountGroup).IsAssignableFrom(entityType)) return typeof(PM.PMAccountGroup.groupID).Name;
			if (typeof(PM.PMTran).IsAssignableFrom(entityType)) return typeof(PM.PMTran.tranID).Name;
			if (typeof(EP.EPEquipment).IsAssignableFrom(entityType)) return typeof(EP.EPEquipment.equipmentID).Name;

			return null;
		}
		public static string GetEntityClassID(Type entityType)
		{
			if (typeof(CR.Contact).IsAssignableFrom(entityType)) return typeof(CR.Contact.classID).Name;
			if (typeof(CR.BAccount).IsAssignableFrom(entityType)) return typeof(CR.BAccount.classID).Name;
			if (typeof(CR.CRCase).IsAssignableFrom(entityType)) return typeof(CR.CRCase.caseClassID).Name;
			if (typeof(CR.CROpportunity).IsAssignableFrom(entityType)) return typeof(CR.CROpportunity.cROpportunityClassID).Name;
			if (typeof(IN.InventoryItem).IsAssignableFrom(entityType)) return typeof(IN.InventoryItem.itemClassID).Name;
			if (typeof(PM.PMProject).IsAssignableFrom(entityType)) return typeof(PM.PMProject.classID).Name;
			if (typeof(PM.PMTask).IsAssignableFrom(entityType)) return typeof(PM.PMTask.classID).Name;
			if (typeof(PM.PMAccountGroup).IsAssignableFrom(entityType)) return typeof(PM.PMAccountGroup.classID).Name;
			if (typeof(PM.PMTran).IsAssignableFrom(entityType)) return typeof(PM.PMTran.projectID).Name;
			if (typeof(EP.EPEquipment).IsAssignableFrom(entityType)) return typeof(EP.EPEquipment.classID).Name;

			return null;
		}
	}
}

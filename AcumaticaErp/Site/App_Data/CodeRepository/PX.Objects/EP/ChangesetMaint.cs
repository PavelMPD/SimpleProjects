using System;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	public class ChangesetMaint : PXGraph<ChangesetMaint>
	{
		private const string _NAME_FIELD = "name";
		private const string _OLDVALUE_FIELD = "oldValueString";
		private const string _NEWVALUE_FIELD = "newValueString";

		[PXViewName(Messages.Changeset)]
		public PXSelect<EPActivity, 
			Where<EPActivity.classID, Equal<CRActivityClass.history>>>
			Changeset;

		[PXViewName(Messages.ChangesetDetails)]
		public PXSelect<EPChangesetDetail, 
			Where<EPChangesetDetail.changesetID, Equal<Current<EPActivity.taskID>>>>
			Details;

		public ChangesetMaint()
		{
			AttacheNewFields();
			CorrectUI();
		}

		private void CorrectUI()
		{
			PXUIFieldAttribute.SetDisplayName<EPActivity.taskID>(Changeset.Cache, Messages.Number);
			PXUIFieldAttribute.SetDisplayName<EPActivity.startDate>(Changeset.Cache, Messages.Date);
			PXUIFieldAttribute.SetEnabled<EPActivity.taskID>(Changeset.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<EPActivity.startDate>(Changeset.Cache, null, false);
		}

		public PXSave<EPActivity> Save;

		public PXCancel<EPActivity> Cancel;

		private void AttacheNewFields()
		{
			var cache = Details.Cache;
			cache.Fields.Add(_NAME_FIELD);
			var nameState = PXFieldState.CreateInstance(null, null, null, null, null,
				null, null, null, _NAME_FIELD, null, Messages.Name, null, PXErrorLevel.Undefined, true, true, null,
				PXUIVisibility.Undefined, null, null, null);
			FieldSelecting.AddHandler(cache.GetItemType(), _NAME_FIELD, 
				(sender, args) =>
				{
					args.ReturnState = nameState;

					var sourceField = ((EPChangesetDetail)args.Row).With(_ => _.SourceField);
					if (string.IsNullOrEmpty(sourceField)) return;

					var entityType = GetCurrentEntityType();
					if (entityType == null) return;

					var displayName = (Caches[entityType].GetStateExt(null, sourceField) as PXFieldState).With(_ => _.DisplayName);
					args.ReturnValue = string.IsNullOrEmpty(displayName) ? sourceField : displayName;
				});
			cache.Fields.Add(_OLDVALUE_FIELD);
			var oldValueState = PXFieldState.CreateInstance(null, null, null, null, null,
				null, null, null, _OLDVALUE_FIELD, null, Messages.OldValue, null, PXErrorLevel.Undefined, true, true, null,
				PXUIVisibility.Undefined, null, null, null);
			FieldSelecting.AddHandler(cache.GetItemType(), _OLDVALUE_FIELD,
				(sender, args) =>
				{
					args.ReturnState = oldValueState;
					args.ReturnValue = GetValue(args.Row as EPChangesetDetail, typeof(EPChangesetDetail.oldValue).Name);
				});
			cache.Fields.Add(_NEWVALUE_FIELD);
			var newValueState = PXFieldState.CreateInstance(null, null, null, null, null,
				null, null, null, _NEWVALUE_FIELD, null, Messages.NewValue, null, PXErrorLevel.Undefined, true, true, null,
				PXUIVisibility.Undefined, null, null, null);
			FieldSelecting.AddHandler(cache.GetItemType(), _NEWVALUE_FIELD,
				(sender, args) =>
				{
					args.ReturnState = newValueState;
					args.ReturnValue = GetValue(args.Row as EPChangesetDetail, typeof(EPChangesetDetail.newValue).Name);
				});
		}

		private object GetValue(EPChangesetDetail row, string field)
		{
			if (row == null) return null;

			var sourceField = row.With(_ => _.SourceField);
			if (string.IsNullOrEmpty(sourceField)) return null;

			var valArr = row.With(_ => Details.Cache.GetValue(row, field) as byte[]).
				With(_ => PXDatabase.Deserialize(_));
			if (valArr == null || valArr.Length == 0) return null;
			var val = valArr[0];

			var state = GetCurrentEntityType().With(_ => Caches[_].GetStateExt(null, sourceField) as PXFieldState);
			if (state == null) return null;

			var intState = state as PXIntState;
			int intIndex;
			if (intState != null && intState.AllowedValues != null && 
				(intIndex = Array.IndexOf(intState.AllowedValues, val)) > -1)
			{
				return intState.AllowedLabels[intIndex];
			}

			var stringState = state as PXStringState;
			int stringIndex;
			if (stringState != null && stringState.AllowedValues != null && 
				(stringIndex = Array.IndexOf(stringState.AllowedValues, val)) > -1)
			{
				return stringState.AllowedLabels[stringIndex];
			}

			//TODO: need implement mask convertion

			return val;
		}

		private Type GetCurrentEntityType()
		{
			var refNoteID = Changeset.Current.With(_ => _.RefNoteID);
			if (refNoteID.HasValue) 
				return new EntityHelper(this).GetEntityRow((long)refNoteID).With(_ => _.GetType());
			return null;
		}
	}
}

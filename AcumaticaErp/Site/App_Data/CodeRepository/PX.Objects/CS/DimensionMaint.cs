using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using PX.Common;
using PX.Data;
using System.Text;
using PX.Objects.GL;
using System.Text.RegularExpressions;
using PX.Objects.IN;

namespace PX.Objects.CS
{
	public class DimensionMaint : PXGraph<DimensionMaint, Dimension>
	{				
		#region Selects
		public PXSelect<Dimension, Where<Dimension.dimensionID, InFieldClassActivated, Or<Dimension.dimensionID, IsNull>>> Header;

		public PXSelect<Segment,
			Where<Segment.dimensionID, Equal<Current<Dimension.dimensionID>>>>
			Detail;

        public PXSelect<SegmentValue> Values;
		#endregion

		#region Data Handlers

		protected virtual IEnumerable detail()
		{
			if (Header.Current == null) return new Segment[0];
			return Header.Current.ParentDimensionID != null
				? GetComplexDetails(Header.Current)
				: GetSimpleDetails(Header.Current);
		}

		protected virtual IEnumerable GetDetails(string dimId)
		{
			Dimension dim;
			if (string.IsNullOrEmpty(dimId) ||
				(dim = (Dimension)PXSelect<Dimension>.Search<Dimension.dimensionID>(this, dimId)) == null)
			{
				return new Segment[0];
			}
			return dim.ParentDimensionID != null ? GetComplexDetails(dim) : GetSimpleDetails(dim);
		}

		private IEnumerable GetComplexDetails(Dimension dim)
		{
			var hashtable = new Hashtable();
			foreach (Segment row in PXSelect<Segment,
					Where<Segment.dimensionID, Equal<Required<Segment.dimensionID>>>>
					.Select(this, dim.ParentDimensionID))
			{
				var item = (Segment)Detail.Cache.CreateCopy(row);
				var key = (short)item.SegmentID;
                item.ParentDimensionID = dim.ParentDimensionID;
				item.Inherited = true;
				item.DimensionID = dim.DimensionID;
				hashtable.Add(key, item);
			}
			foreach (Segment item in PXSelect<Segment,
				 Where<Segment.dimensionID, Equal<Required<Segment.dimensionID>>>>.
				 Select(this, dim.DimensionID))
			{
				if (item.ParentDimensionID != null && hashtable.ContainsKey(item.SegmentID))
				{
					hashtable.Remove(item.SegmentID);
                    yield return item;
				}
			}
		    foreach (Segment item in hashtable.Values)
		    {
                if (Detail.Cache.GetStatus(item) == PXEntryStatus.Notchanged)
                {
                    Detail.Cache.SetStatus(item, PXEntryStatus.Inserted);
                }
                yield return item;
		    }
		}

		private IEnumerable GetSimpleDetails(Dimension dim)
		{
			foreach (Segment item in
				PXSelect<Segment,
					Where<Segment.dimensionID, Equal<Required<Segment.dimensionID>>>>
				.Select(this, dim.DimensionID))
			{
				yield return item;
			}
		}

		#endregion

		#region Actions

		public PXAction<Dimension> ViewSegment;
        public PXAction<Dimension> RebuildValues;

		[PXButton]
		[PXUIField(DisplayName = Messages.ViewSegment)]
		protected virtual IEnumerable viewSegment(PXAdapter adapter)
		{
			var current = Detail.Current;
			if (current != null)
			{
				Segment row;
				if (current.Inherited == true)
				{
					row = (Segment)PXSelectReadonly2<Segment,
					InnerJoin<Dimension, On<Dimension.parentDimensionID, Equal<Segment.dimensionID>>>,
					Where<Dimension.dimensionID, Equal<Required<Dimension.dimensionID>>,
						And<Segment.segmentID, Equal<Required<Segment.segmentID>>>>>.
						Select(this, current.DimensionID, current.SegmentID);
				}
				else
				{
					row = (Segment)PXSelectReadonly<Segment>.
						Search<Segment.dimensionID, Segment.segmentID>(this, current.DimensionID, current.SegmentID);
				}
				PXRedirectHelper.TryRedirect(Caches[typeof(Segment)], row, string.Empty);
			}
			return adapter.Get();
		}

        /*IEnumerable<Type> GetTypesWith<TAttribute>(bool inherit)
                              where TAttribute : System.Attribute
        {
            return from a in AppDomain.CurrentDomain.GetAssemblies()
                   from t in a.GetTypes()
                   where t.IsDefined(typeof(TAttribute), inherit)
                   select t;
        }*/

        //private void SelectG<TTable>()
        //{
        //    vat m = PXSelect<TTable>;
        //}


        [PXButton]
        [PXUIField(DisplayName = "Rebuild segment values", MapEnableRights = PXCacheRights.Update)]
        public virtual IEnumerable rebuildValues(PXAdapter adapter)
        {
            ////var t = PXDatabase.SelectMulti<Ledger>(
            ////                    new PXDataField(Header.Current.FieldName));
            //Type selectedType = Type.GetType(Header.Current.TableName);
            
            //var assemblyT = Assembly.GetAssembly(selectedType);
            ////var f = selectedType.
            ////IBqlTable tp = (IBqlTable)selectedType;


            //List<string> values = new List<string>();
            //foreach (PXDataRecord line in PXDatabase.SelectMulti<INSubItem>(
            //                    new PXDataField(Header.Current.FieldName)))
            //{
            //    values.Add(line.GetString(0));
            //}

            ////PXResultset<Segment> segments = Detail.Select();
            //int currentPosition = 0;
            //PXGraph d = PXGraph.CreateInstance(typeof(SegmentMaint));
            //foreach (Segment segment in Detail.Select())
            //{
            //    List<SegmentValue> newSegmentValues = new List<SegmentValue>();
            //    HashSet<string> segmentValues = new HashSet<string>();
            //    foreach (string cd in values)
            //    {
            //        SegmentValue newSegmentValue = new SegmentValue();
            //        newSegmentValue.SegmentID = segment.SegmentID;
            //        newSegmentValue.DimensionID = segment.DimensionID;
            //        if (currentPosition < cd.Length)
            //        {
            //            newSegmentValue.Value = cd.Substring(currentPosition, (int)segment.Length);
            //            if (!string.IsNullOrWhiteSpace(newSegmentValue.Value) && !segmentValues.Contains(newSegmentValue.Value))
            //            {
            //                segmentValues.Add(cd.Substring(currentPosition, (int)segment.Length));
            //                newSegmentValues.Add(newSegmentValue);
            //            }
            //        }
            //    }
            //    PXResultset<SegmentValue> existingSegmentValues = PXSelect<SegmentValue,
            //    Where<SegmentValue.dimensionID, Equal<Required<Segment.dimensionID>>,
            //    And<SegmentValue.segmentID, Equal<Required<Segment.segmentID>>>>>.Select(d, segment.DimensionID, segment.SegmentID);
            //    foreach (SegmentValue existingSegmentValue in existingSegmentValues)
            //    { 
                
            //    }
            //    currentPosition += (int)segment.Length;
            //}

            
            ////PXView m = d.Views["Values"];
            
            ///*if (Header.Current.TableName != null)
            //{
            //    Type selectedType = Type.GetType(Header.Current.TableName);
            //    string dimensionName = "";
            //    PropertyInfo[] properties = selectedType.GetProperties();
            //    foreach (PropertyInfo property in properties)
            //    {
            //        Object[] customAttributes = property.GetCustomAttributes(true);
            //        foreach (Object customAttribute in customAttributes)
            //        {
            //            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            //            FieldInfo[] attributeFields = customAttribute.GetType().GetFields(bindFlags);
            //            foreach (FieldInfo attributeField in attributeFields)
            //            {
            //                if (attributeField.Name.Contains("DimensionName"))
            //                {
            //                    dimensionName = attributeField.GetValue(customAttribute).ToString();
            //                    if (dimensionName == Header.Current.DimensionID)
            //                    {
            //                        Header.Ask("Table found", "Table: " + Header.Current.TableName + " Field: " + property.Name, MessageButtons.OK);
            //                        return adapter.Get();
            //                    }
            //                        //break;
            //                }
            //            }
            //        }
            //    }
            //    if (!dimensionName.Equals(string.Empty))
            //    {
            //        Header.Ask("Line", dimensionName, MessageButtons.OK);
            //    }
            //}*/
            return adapter.Get();
        }

		public override void Persist()
		{

			if (Header.Cache.IsDirty && Detail.Select().Count == 0 && Header.Current != null)
			{
				throw new PXException(Messages.DimensionIsEmpty);
			}

			var current = Header.Current;
			if (current == null) base.Persist();
			else
				try
				{
					if (Header.Cache.GetStatus(current) != PXEntryStatus.Deleted)
					{
						InsertNumberingValue(Header.Current);
						CorrectChildDimensions();
					}

					if (Header.Current.DimensionID == IN.SubItemAttribute.DimensionName)
					{
						ValidateSubItems();
					}


					PXDimensionAttribute.Clear();
					base.Persist();
					Header.Current = current;
					PXDimensionAttribute.Clear();
				}
				catch (PXDatabaseException e)
				{
					if (e.ErrorCode == PXDbExceptions.DeleteForeignKeyConstraintViolation)
						throw new PXException(Messages.SegmentHasValues, e.Keys[1]);
					throw;
				}
		    PXPageCacheUtils.InvalidateCachedPages();
		}


		[PXProjection(typeof(Select4<INSubItemDup, Aggregate<GroupBy<INSubItemDup.subItemCD>>>))]
		public class INSubItemDup : INSubItem
		{
			#region SubItemCD
			public new abstract class subItemCD : IBqlField { }
			//[PXDBString(30, IsUnicode = true)]
			[PXDBCalced(typeof(Left<INSubItem.subItemCD, CurrentValue<Dimension.segments>>), typeof(string))]
			public override string SubItemCD
			{
				get;
				set;
			}
			#endregion
		}

		public class NoSort : IBqlSortColumn
		{
			public Type GetReferencedType()
			{
				return null;
			}
			public bool IsDescending
			{
				get
				{
					return false;
				}
			}

			public void Parse(PXGraph graph, List<IBqlParameter> pars, List<Type> tables, List<Type> fields, List<IBqlSortColumn> sortColumns, StringBuilder text, BqlCommand.Selection selection)
			{
				if (sortColumns != null)
				{
					sortColumns.Add(this);
				}
			}

			public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
			{
			}
		}


		protected virtual void ValidateSubItems()
		{
			//1.Validate that trimming segmented key will not cause duplicate values
			int? length = Header.Current.Length;

			foreach(PXResult<INSubItem> record in PXSelectGroupBy<INSubItem, Aggregate<GroupBy<Left<INSubItem.subItemCD, CurrentValue<Dimension.length>>, Count>>>.Select(this))
			{
				if (record.RowCount > 1)
				{
                    throw new PXRowPersistingException(typeof(Dimension.length).Name, length, Messages.InSubitemDuplicate);
				}
			}
		}

		#endregion

		#region Dimenstion Event Handlers

		protected virtual void Dimension_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            Dimension dim = (Dimension)e.Row;
			PXUIFieldAttribute.SetEnabled<Dimension.length>(cache, dim, false);
			PXUIFieldAttribute.SetEnabled<Dimension.segments>(cache, dim, false);

			bool numberingIDEnabled = true;
			bool specificModule = true;
			bool validateEnabled = false;

			Detail.Cache.AllowInsert = true;
			Detail.Cache.AllowUpdate = true;
			Detail.Cache.AllowDelete = true;

			switch (dim.DimensionID)
			{
				case SubItemAttribute.DimensionName:
					PXUIFieldAttribute.SetEnabled<Segment.autoNumber>(Detail.Cache, null, false);
					break;

				case SubAccountAttribute.DimensionName:
					validateEnabled = true;
					break;

				case AccountAttribute.DimensionName:
					break;

				case AR.CustomerAttribute.DimensionName:
				case AR.SalesPersonAttribute.DimensionName:
				case IN.InventoryAttribute.DimensionName:
				case IN.SiteAttribute.DimensionName:
				case IN.LocationAttribute.DimensionName:
				case AP.VendorAttribute.DimensionName:
				case AR.CustomerRawAttribute.DimensionName:
				case EP.EmployeeRawAttribute.DimensionName:
				case CR.LeadRawAttribute.DimensionName:
					validateEnabled = dim.Validate == false;
					PXUIFieldAttribute.SetEnabled<Segment.autoNumber>(Detail.Cache, null, true);
					PXUIFieldAttribute.SetVisible<Segment.isCosted>(Detail.Cache, null, false);
					break;

				default:
					validateEnabled = true;
					break;
			}

		    cache.AllowDelete = dim.Internal != true;
            //RebuildValues.SetEnabled(dim.TableName != null);

			PXUIFieldAttribute.SetVisible<Segment.consolOrder>(Detail.Cache, null, dim.DimensionID == SubAccountAttribute.DimensionName);
			PXUIFieldAttribute.SetVisible<Segment.consolNumChar>(Detail.Cache, null, dim.DimensionID == SubAccountAttribute.DimensionName);

			PXUIFieldAttribute.SetVisible<Segment.isCosted>(Detail.Cache, null, dim.DimensionID == SubItemAttribute.DimensionName);

            bool hasParent = dim.ParentDimensionID != null;
            Detail.Cache.AllowInsert &= !hasParent;
			PXUIFieldAttribute.SetVisible<Segment.inherited>(Detail.Cache, null, hasParent);
			PXUIFieldAttribute.SetVisible<Segment.isOverrideForUI>(Detail.Cache, null, hasParent);

			PXUIFieldAttribute.SetEnabled<Dimension.numberingID>(cache, e.Row, numberingIDEnabled);
			PXUIFieldAttribute.SetEnabled<Dimension.specificModule>(cache, e.Row, specificModule);
			PXUIFieldAttribute.SetEnabled<Dimension.validate>(cache, e.Row, validateEnabled);
		}

		protected virtual void Dimension_Validate_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
		{
			if (e.ReturnValue != null)
			{
				e.ReturnValue = !((bool)e.ReturnValue);
			}
		}

		protected virtual void Dimension_Validate_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
		{
			PXBoolAttribute.ConvertValue(e);
			if (e.NewValue != null)
			{
				e.NewValue = !((bool)e.NewValue);
			}
		}

		protected virtual void Dimension_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			CheckLength(cache, e.Row);
		}

		#endregion

		#region Segment Event Handlers

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(Dimension.dimensionID))]
		[PXUIField(DisplayName = "Dimension ID", Visibility = PXUIVisibility.Invisible, Visible = false)]
		[PXSelector(typeof(Dimension.dimensionID), DirtyRead = true)]
		protected virtual void Segment_DimensionID_CacheAttached(PXCache sender)
		{

		}

		[PXDBShort(IsKey = true)]
		[PXUIField(DisplayName = "Segment ID", Visibility = PXUIVisibility.Visible, Enabled = false)]
		protected virtual void Segment_SegmentID_CacheAttached(PXCache sender)
		{

		}

		protected virtual void Segment_SegmentID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			Segment currow = e.Row as Segment;
			if (currow == null) return;

			short maxsegid = 0;

			foreach (Segment segrow in GetDetails(currow.DimensionID))
			{
				if (segrow.SegmentID > maxsegid)
				{
					maxsegid = (short)segrow.SegmentID;
				}
			}
			e.NewValue = ++maxsegid;
			currow.ConsolOrder = maxsegid;
			e.Cancel = true;

			PXUIFieldAttribute.SetEnabled<Segment.segmentID>(cache, currow, false);
		}

        protected virtual void Segment_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            Segment seg = (Segment) e.Row;
            if (seg == null) return;

            e.Cancel = seg.ParentDimensionID != null && seg.Inherited == true &&
                       cache.GetStatus(seg) == PXEntryStatus.Inserted;
        }
        
        protected virtual void Segment_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			UpdateHeader(e.Row);
			CheckLength(Header.Cache, Header.Current);
		}

		protected virtual void Segment_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
		    Segment seg = (Segment) e.Row;
		    if (seg == null) return;

		    if (seg.ParentDimensionID != null) seg.Inherited = false;

			UpdateHeader(e.Row);
			CheckLength(Header.Cache, Header.Current);
		}

		protected virtual void Segment_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			UpdateHeader(e.Row);
		}

		protected virtual void Segment_DimensionID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void Segment_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            Segment row = e.Row as Segment;
            if (row == null)
                return;

            if (sender.GetStatus(row) == PXEntryStatus.Inserted)
            {
                string message = Messages.SegmentedKeyAddingNewSegmentRequiresUpdating;
                if (Header.Current != null && Header.Current.DimensionID == SubAccountAttribute.DimensionName)
                {
                    message = Messages.SubaccountAddingNewSegmentRequiresUpdating;
                }
                sender.RaiseExceptionHandling<Segment.segmentID>(row, row.SegmentID,
                    new PXSetPropertyException(message, PXErrorLevel.RowWarning));
            }

            if (row.ParentDimensionID != null)
            {
                Segment parent = row;
                if (row.Inherited != true)
                {
                    parent = PXSelectReadonly<Segment, Where<Segment.dimensionID, Equal<Current<Segment.parentDimensionID>>,
                                                        And<Segment.segmentID, Equal<Current<Segment.segmentID>>>>>.SelectSingleBound(this, new object[]{row});
                }
                PXUIFieldAttribute.SetEnabled(sender, row, false);
                PXUIFieldAttribute.SetEnabled<Segment.descr>(sender, row, true);
                PXUIFieldAttribute.SetEnabled<Segment.editMask>(sender, row, true);
                PXUIFieldAttribute.SetEnabled<Segment.autoNumber>(sender, row, true);
                PXUIFieldAttribute.SetEnabled<Segment.validate>(sender, row, parent != null && parent.Validate != true);
            }
		}

		protected virtual void Segment_Length_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Segment currow = e.Row as Segment;
			if (currow == null) return;
			currow.ConsolNumChar = currow.Length;
		}

		protected virtual void Segment_AutoNumber_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Segment currow = e.Row as Segment;

			if (currow.Validate != null && (bool)currow.Validate && currow.AutoNumber != (bool)e.NewValue && (bool)e.NewValue)
			{
				currow.Validate = false;
				cache.Update(currow);
			}
		}

		protected virtual void Segment_Validate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Segment currow = e.Row as Segment;

			if (currow.AutoNumber != null && (bool)currow.AutoNumber && currow.Validate != (bool)e.NewValue && (bool)e.NewValue)
			{
				currow.AutoNumber = false;
				cache.Update(currow);
			}
		}

		protected virtual void Segment_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			var row = (Segment)e.Row;
			var dimensionID = row.DimensionID;
			var segmentID = row.SegmentID;

            if (Header.Current.ParentDimensionID != null && row.Inherited == true)
            {
                throw new PXException(Messages.SegmentNotOverridden, segmentID);
            }
			if (PXSelectReadonly<Segment, Where<Segment.parentDimensionID, Equal<Current<Segment.dimensionID>>, 
                                            And<Segment.segmentID, Equal<Current<Segment.segmentID>>>>>.SelectMultiBound(this, new object[]{row}).Count > 0 )
			{
				throw new PXException(Messages.SegmentHasChilds, segmentID);
			}
			Segment lastSegmeent = PXSelect<Segment, Where<Segment.dimensionID, Equal<Current<Segment.dimensionID>>>,
											OrderBy<Desc<Segment.segmentID>>>.SelectSingleBound(this, new object[]{row});
			if (lastSegmeent != null && lastSegmeent.SegmentID > segmentID)
			{
				throw new PXException(Messages.SegmentIsNotLast, segmentID);
			}
			if (((SegmentValue)PXSelect<SegmentValue,
				Where<SegmentValue.dimensionID, Equal<Optional<Segment.dimensionID>>,
					And<SegmentValue.segmentID, Equal<Optional<Segment.segmentID>>>>>.
				Select(this, dimensionID, segmentID)) != null)
			{
				if (row.ParentDimensionID == null) throw new PXException(Messages.SegmentHasValues, segmentID);

				var answer = Header.Ask(Messages.Warning,
					string.Format(Messages.SegmentHasValuesQuestion, segmentID),
					MessageButtons.YesNoCancel,
					MessageIcon.Warning);
				switch (answer)
				{
					case WebDialogResult.Yes:
						break;
					case WebDialogResult.Cancel:
						e.Cancel = true;
						break;
					case WebDialogResult.No:
					default:
						throw new PXException(Messages.SegmentHasValues, segmentID);
				}
			}
		}

		protected virtual void Segment_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			var row = (Segment)e.Row;
			var newRow = ((Segment)e.NewRow);
			if (newRow.AutoNumber == false && (row.Length != newRow.Length || row.EditMask != newRow.EditMask))
			{
				foreach (SegmentValue val in PXSelect<SegmentValue, Where<SegmentValue.dimensionID, Equal<Optional<Segment.dimensionID>>, And<SegmentValue.segmentID, Equal<Optional<Segment.segmentID>>>>>.Select(this, row.DimensionID, row.SegmentID))
				{
					string mask = Regex.Replace(Regex.Replace(val.Value, "[0-9]", "9"), "[^0-9]", "?");

					if (newRow.Length != val.Value.Length)
					{
						e.Cancel = true;
						cache.RaiseExceptionHandling<Segment.length>(e.NewRow, newRow.Length, new PXSetPropertyException(Messages.SegmentHasValuesFailedUpdate, row.SegmentID));
					    return;
					}

					if (newRow.EditMask == "?" && mask.Contains("9") || newRow.EditMask == "9" && mask.Contains("?"))
					{
						e.Cancel = true;
						cache.RaiseExceptionHandling<Segment.editMask>(e.NewRow, newRow.EditMask, new PXSetPropertyException(Messages.SegmentHasValuesFailedUpdate, row.SegmentID));
                        return;
                    }


				}
			}
            var childs = PXSelect<Segment, Where<Segment.parentDimensionID, Equal<Current<Segment.dimensionID>>,
                                            And<Segment.segmentID, Equal<Current<Segment.segmentID>>>>>.SelectMultiBound(this, new object[] { row });
			//bool editMask = false;
			bool separator = false;
			bool align = false;
			bool validate = false;
			bool caseConvert = false;
			if (/*(editMask = row.EditMask != newRow.EditMask) ||*/
				(separator = row.Separator != newRow.Separator) || (align = row.Align != newRow.Align) ||
				(validate = row.Validate != newRow.Validate) || (caseConvert = row.CaseConvert != newRow.CaseConvert))
			{
				if (childs != null && childs.Count > 0)
				{
					e.Cancel = true;

					var error = new PXSetPropertyException(Messages.SegmentHasChildsFailedUpdate, row.SegmentID);
                    /*if (editMask)
                        cache.RaiseExceptionHandling<Segment.editMask>(e.NewRow, newRow.EditMask, error);
					else*/ if (separator)
						cache.RaiseExceptionHandling<Segment.separator>(e.NewRow, newRow.Separator, error);
					else if (align)
						cache.RaiseExceptionHandling<Segment.align>(e.NewRow, newRow.Align, error);
					else if (validate)
						cache.RaiseExceptionHandling<Segment.validate>(e.NewRow, newRow.Validate, error);
					else cache.RaiseExceptionHandling<Segment.caseConvert>(e.NewRow, newRow.CaseConvert, error);
                    
                    return;
                }
			}

            if (row.Length != newRow.Length && childs != null)
            {
                foreach (Segment child in childs)
                {
                    Segment copy = (Segment)cache.CreateCopy(child);
                    copy.Length = newRow.Length;
                    copy = Detail.Update(copy);
                    if (copy == null)
                    {
                        e.Cancel = true;
                        cache.RaiseExceptionHandling<Segment.length>(e.NewRow, newRow.Length, new PXSetPropertyException(Messages.SegmentHasValuesFailedUpdate, row.SegmentID));
                        return;
                    }
                }
            }
		}

		#endregion

		#region Private Methods

		private void CorrectChildDimensions()
		{
			var header = Header.Current;
			if (header != null)
			{
				var oldNumbering = header.DimensionID.
					With(id => (Dimension)PXSelectReadonly<Dimension>.
						Search<Dimension.dimensionID>(this, id)).
					With(dim => dim.NumberingID);
				foreach (Dimension item in
					PXSelect<Dimension, Where<Dimension.parentDimensionID, Equal<Required<Dimension.parentDimensionID>>>>.
						Select(this, header.DimensionID))
				{
					item.Length = header.Length;
					item.Segments = header.Segments;

					if (string.IsNullOrEmpty(item.NumberingID)) item.NumberingID = header.NumberingID;
					var numbErrors = new List<Exception>();
					var oldNumbID = item.NumberingID;
					if (oldNumbID == oldNumbering)
					{
						item.NumberingID = header.NumberingID;
						CheckLength(Header.Cache, item, numbErrors);
						if (numbErrors.Count > 0)
						{
							numbErrors.Clear();
							item.NumberingID = oldNumbID;
						}
						else oldNumbID = item.NumberingID;
					}
					CheckLength(Header.Cache, item, numbErrors);
					if (numbErrors.Count > 0 && oldNumbID != header.NumberingID)
					{
						var oldNumbErrorsCount = numbErrors.Count;
						item.NumberingID = header.NumberingID;
						CheckLength(Header.Cache, item, numbErrors);
						if (numbErrors.Count == oldNumbErrorsCount) numbErrors.Clear();
						else item.NumberingID = oldNumbID;
					}
					if (numbErrors.Count > 0)
					{
						var sb = new StringBuilder();
						foreach (var error in numbErrors)
							sb.AppendLine(error.Message);
						throw new Exception(sb.ToString());
					}

					Header.Cache.Update(item);

					InsertNumberingValue(item);
				}
			}
		}

		private void InsertNumberingValue(Dimension dimension)
		{
			foreach (Segment segrow in GetDetails(dimension.DimensionID))
			{
				if (segrow.Inherited == true) continue;
				if (segrow.AutoNumber == true)
				{
					segrow.EditMask = "C";
					segrow.Validate = false;
					//segrow.CaseConvert = 0;
					segrow.FillCharacter = " ";
					Detail.Update(segrow);

					Numbering numb = PXSelect<Numbering,
										Where<Numbering.numberingID, Equal<Required<Numbering.numberingID>>>>.
									 Select(this, dimension.NumberingID);
					bool numvalueexists = false;
					if (numb == null)
					{
						throw new PXException(Messages.NumberingIDRequired);
					}

					var valuesCache = Caches[typeof (SegmentValue)];
					foreach (SegmentValue val in 
						PXSelect<SegmentValue,
						Where<SegmentValue.dimensionID, Equal<Required<SegmentValue.dimensionID>>,
							And<SegmentValue.segmentID, Equal<Required<SegmentValue.segmentID>>>>>.
						Select(this, segrow.DimensionID, segrow.SegmentID))
					{
						if (!object.Equals(val.Value, numb.NewSymbol))
						{
							valuesCache.Delete(val);
						}
						else
						{
							numvalueexists = true;
						}
					}

					if (!numvalueexists)
					{
						SegmentValue val = new SegmentValue();
						val.DimensionID = segrow.DimensionID;
						val.SegmentID = segrow.SegmentID;
						val.Value = numb.NewSymbol;
						valuesCache.Insert(val);
					}
				}
			}
		}

		private void UpdateHeader(object row)
		{
			//
			short segcnt = 0;
			short seglen = 0;

			Segment currow = row as Segment;
			bool IsRefreshNeeded = false;

			if ((bool)currow.AutoNumber)
			{
				foreach (Segment segrow in GetDetails(currow.DimensionID))
				{
					if (segrow.Inherited == true) continue;
					if ((bool)segrow.AutoNumber && segrow.SegmentID != currow.SegmentID)
					{
						segrow.AutoNumber = (bool)false;
						Detail.Cache.Update(segrow);
						IsRefreshNeeded = true;
					}
				}
			}

			foreach (Segment segrow in GetDetails(currow.DimensionID))
			{
				if (segrow.Inherited != true && currow.DimensionID == IN.SubItemAttribute.DimensionName)
				{
					if ((bool)((Segment)row).IsCosted)
					{
						if (segrow.SegmentID < ((Segment)row).SegmentID)
						{
							segrow.IsCosted = true;
							Detail.Cache.Update(segrow);
							IsRefreshNeeded = true;
						}
					}
					else
					{
						if (segrow.SegmentID > ((Segment)row).SegmentID)
						{
							segrow.IsCosted = false;
							Detail.Cache.Update(segrow);
							IsRefreshNeeded = true;
						}
					}
				}

				segcnt++;
				seglen += (short)segrow.Length;
			}

			Dimension dim = Header.Current as Dimension;
			dim.Segments = segcnt;
			dim.Length = seglen;
			Header.Cache.Update(dim);

			if (IsRefreshNeeded)
			{
				Detail.View.RequestRefresh();
			}
		}
		
		private void CheckLength(PXCache cache, object row)
		{
			CheckLength(cache, row, null);
		}

		private void CheckLength(PXCache cache, object row, ICollection<Exception> errors)
		{
			Dimension currow = row as Dimension;

			short seglen = 0;

			foreach (Segment segrow in GetDetails(currow.DimensionID))
			{
				seglen += (short)segrow.Length;

				if ((bool)segrow.AutoNumber)
				{
					if (currow.NumberingID == null)
					{
						if (errors == null)
						{
							cache.RaiseExceptionHandling<Dimension.numberingID>(currow, currow.NumberingID,
								new PXSetPropertyException(Messages.NumberingIDRequired));
						}
						else
						{
							errors.Add(new PXSetPropertyException(Messages.NumberingIDRequiredCustom, currow.DimensionID));
						}
					}
					else
					{
						foreach (NumberingSequence num in PXSelect<NumberingSequence, Where<NumberingSequence.numberingID, Equal<Required<Dimension.numberingID>>>>.Select(this, currow.NumberingID))
						{
							if (num.StartNbr.Length != segrow.Length)
							{
								if (errors == null)
								{
									cache.RaiseExceptionHandling<Dimension.numberingID>(currow, currow.NumberingID,
                                        new PXSetPropertyException(Messages.NumberingIDCannotBeUsedWithSegment + Messages.EnsureSegmentLength, segrow.SegmentID.ToString()));
								}
								else
								{
                                    errors.Add(new PXSetPropertyException(Messages.NumberingIDCannotBeUsedWithSegmentCustom + Messages.EnsureSegmentLength,
										currow.DimensionID.ToString(), currow.NumberingID.ToString(), segrow.SegmentID.ToString()));
								}
							}

							string mask = Regex.Replace(Regex.Replace(num.StartNbr, "[0-9]", "9"), "[^0-9]", "?");
							if (segrow.EditMask == "?" && mask.Contains("9") || segrow.EditMask == "9" && mask.Contains("?"))
							{
								if (errors == null)
								{
									cache.RaiseExceptionHandling<Dimension.numberingID>(currow, currow.NumberingID,
                                        new PXSetPropertyException(Messages.NumberingIDCannotBeUsedWithSegment + Messages.EnsureSegmentMask, segrow.SegmentID.ToString()));
								}
								else
								{
                                    errors.Add(new PXSetPropertyException(Messages.NumberingIDCannotBeUsedWithSegmentCustom + Messages.EnsureSegmentMask,
										currow.DimensionID.ToString(), currow.NumberingID.ToString(), segrow.SegmentID.ToString()));
								}
							}
						}
					}
				}
			}
		}

		#endregion
	}
}

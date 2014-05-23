using System;
using PX.Data;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;

namespace PX.Objects.CS
{
	public class NumberingMaint : PXGraph<NumberingMaint, Numbering>
	{
		public PXSelectReadonly<Numbering> Numbering;
		public PXSelect<Numbering> Header;
		public PXSelect<NumberingSequence,Where<NumberingSequence.numberingID,Equal<Current<Numbering.numberingID>>>> Sequence;

		public NumberingMaint()
		{
			PXUIFieldAttribute.SetRequired<NumberingSequence.nbrStep>(Sequence.Cache, true);
		}

		private string MakeMask(string str, ref string nbr)
		{
			int i;
			bool j = true;

			StringBuilder bld = new StringBuilder();
			StringBuilder bldNbr = new StringBuilder();
			for (i = str.Length; i > 0; i--)
			{
				if (Regex.IsMatch(str.Substring(i - 1, 1), "[^0-9]"))
				{
					j = false;
				}

				if (j)
				{
					bld.Append(Regex.Replace(str.Substring(i - 1, 1), "[0-9]", "9"));
					bldNbr.Append(str.Substring(i - 1, 1));
				}
				else
				{
					bld.Append(str[i - 1]);
				}
			}

			char[] dig = bldNbr.ToString().ToCharArray();
			Array.Reverse(dig);
			nbr = new string(dig);

			char[] c = bld.ToString().ToCharArray();
			Array.Reverse(c);
			return new string(c); 
		}

		private void CheckNumbers(object row)
		{
			NumberingSequence currow = row as NumberingSequence;

			string nbr=null;
			string s_mask = (currow.StartNbr == null) ? null : MakeMask(currow.StartNbr, ref nbr);
			string s_nbr = nbr;
			string e_mask = (currow.EndNbr == null) ? null : MakeMask(currow.EndNbr, ref nbr);
			string e_nbr = nbr;
			string l_mask = (currow.LastNbr == null) ? null : MakeMask(currow.LastNbr, ref nbr);
			string l_nbr = nbr;
			string w_mask = (currow.WarnNbr == null) ? null : MakeMask(currow.WarnNbr, ref nbr);
			string w_nbr = nbr;

			if (s_mask != e_mask || l_mask != null && s_mask != l_mask || w_mask != null && s_mask != w_mask)
			{
				throw new PXException(Messages.SameNumberingMask);
			}
			if (currow.StartNbr.CompareTo(currow.EndNbr) >= 0) 
			{
				throw new PXException(Messages.StartNumMustBeGreaterEndNum);
			}
			if (currow.WarnNbr!= null && currow.WarnNbr.CompareTo(currow.EndNbr) >= 0)
			{
				throw new PXException(Messages.WarnNumMustBeLessEndNum);
			}
			if (currow.WarnNbr != null && currow.WarnNbr.CompareTo(currow.StartNbr) <= 0)
			{
				throw new PXException(Messages.WarnNumMustBeGreaterStartNum);
			}
			if (currow.LastNbr != null && currow.LastNbr.CompareTo(currow.EndNbr) >= 0)
			{
				throw new PXException(Messages.LastNumMustBeLessEndNum);
			}
			if (currow.LastNbr != null && currow.LastNbr.CompareTo(currow.StartNbr) < 0 && !EqualLastAndStartMinusOne(s_nbr, l_nbr))
			{
			    throw new PXException(Messages.LastNumMustBeGreaterOrEqualStartNum);
			}

		}

		private bool EqualLastAndStartMinusOne(string start, string last)
		{
			char[] charStart = start.ToCharArray();
			for (int i = charStart.Length - 1; i >= 0; i--)
			{
				if (charStart[i] == '0')
					charStart[i] = '9';
				else
				{
					charStart[i] = Convert.ToChar((Convert.ToInt16(charStart[i]) - 1));
					break;
				}
			}
			return String.Equals(new string(charStart), last);
		}

		protected virtual void NumberingSequence_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			//
			CheckNumbers(e.Row);
		}


		protected virtual void NumberingSequence_EndNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			NumberingSequence row = e.Row as NumberingSequence;
			if (row != null)
			{
				
				if (!string.IsNullOrEmpty(row.StartNbr) && string.IsNullOrEmpty(row.EndNbr))

				{
					char[] result = row.StartNbr.ToCharArray();

					for (int i = result.Length-1; i >= 0; i--)
					{
						if ( char.IsDigit(result[i]))
						{
							result[i] = '9';
						}
						else
							break;
					}

					row.EndNbr = new string(result);
				}
			}
		}


		protected virtual void NumberingSequence_StartDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			NumberingSequence row = e.Row as NumberingSequence;
			if (row != null)
			{
				if (row.StartDate == null)
				{
					PXResultset<NumberingSequence> resultset = PXSelect<NumberingSequence, Where<NumberingSequence.numberingID, Equal<Current<Numbering.numberingID>>>>.SelectWindowed(this, 0, 2);
					if (resultset.Count < 1)
					{
						row.StartDate = new DateTime(1900,1,1);
					}
				}
			}
		}


		protected virtual void NumberingSequence_LastNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			NumberingSequence row = e.Row as NumberingSequence;
			if (row != null)
			{
				if (string.IsNullOrEmpty(row.LastNbr) && !string.IsNullOrEmpty(row.StartNbr))
				{
					char[] startNumber = row.StartNbr.ToCharArray();
					char lastChar = startNumber[startNumber.GetUpperBound(0)];

					if (char.IsDigit(lastChar))
					{
						int digit = int.Parse(new string(new char[1] { lastChar }));

						if (digit > 0)
						{
							startNumber[startNumber.GetUpperBound(0)] = (digit - 1).ToString().ToCharArray()[0];
						}

					}

					row.LastNbr = new string(startNumber);
				}
			}
		}


		protected virtual void NumberingSequence_WarnNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			NumberingSequence row = e.Row as NumberingSequence;
			if (row != null)
			{
				if (!string.IsNullOrEmpty(row.EndNbr) && string.IsNullOrEmpty(row.WarnNbr))
				{
					if (row.EndNbr.Length >= 3)
					{
						if (char.IsDigit(row.EndNbr[row.EndNbr.Length - 1]) &&
							char.IsDigit(row.EndNbr[row.EndNbr.Length - 2]) &&
							char.IsDigit(row.EndNbr[row.EndNbr.Length - 3]) 
							)
						{
							int number = int.Parse( row.EndNbr.Substring(row.EndNbr.Length - 3, 3));
							
							if ( number > 100 )
							{
								int warningNumber = number - 100;

								string str = string.Format("{0:000}", warningNumber);
								row.WarnNbr = row.EndNbr.Substring(0, row.EndNbr.Length - 3) + str;
							}

						}
					}
				}
			}
		}



		protected virtual void NumberingSequence_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			//
			CheckNumbers(e.NewRow);
		}

		protected virtual void Numbering_NewSymbol_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue != null)
			{
				foreach (NumberingSequence seq in Sequence.Select(((Numbering)e.Row).NumberingID))
				{
					if (seq.StartNbr.Length < ((string)e.NewValue).Length)
					{
						throw new PXSetPropertyException(Messages.NewSymbolLength);
					}
				}
			}
		}

		protected virtual void Numbering_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			//
			if (((Numbering) e.Row).UserNumbering != true && string.IsNullOrEmpty(((Numbering) e.Row).NewSymbol))
			{
				throw new PXException(Messages.OneFieldMustBeFilled);
			}

			if (((Numbering)e.Row).NewSymbol != null )
			{
				foreach (NumberingSequence num in Sequence.Select(((Numbering)e.Row).NumberingID))
				{
					if (num.StartNbr.Length < ((Numbering)e.Row).NewSymbol.Length)
					{
						throw new PXException(Messages.NewSymbolLength);
					}
				}
			}
		}

		protected virtual void NumberingSequence_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			//
			NumberingSequence num = (NumberingSequence)e.Row;			
			foreach (PXResult<Dimension, Segment> r in PXSelectJoin<Dimension, InnerJoin<Segment, On<Segment.dimensionID, Equal<Dimension.dimensionID>>>, Where<Dimension.numberingID, Equal<Optional<Numbering.numberingID>>, And<Segment.autoNumber, Equal<Optional<Segment.autoNumber>>>>>.Select(this, ((NumberingSequence)e.Row).NumberingID, (bool)true))
			{
				Dimension dim = r;
				Segment segrow = r;

				if (num.StartNbr.Length != segrow.Length)
				{
					cache.RaiseExceptionHandling<NumberingSequence.startNbr>(num, num.StartNbr, new PXSetPropertyException(Messages.NumberingViolatesSegmentDef, segrow.DimensionID, segrow.SegmentID.ToString()));
				}

				string mask = Regex.Replace(Regex.Replace(num.StartNbr, "[0-9]", "9"), "[^0-9]", "?");
				if (segrow.EditMask == "?" && mask.Contains("9") || segrow.EditMask == "9" && mask.Contains("?"))
				{
					cache.RaiseExceptionHandling<NumberingSequence.startNbr>(num, num.StartNbr, new PXSetPropertyException(Messages.NumberingViolatesSegmentDef, segrow.DimensionID, segrow.SegmentID.ToString()));
				}


			}
		}

		protected virtual void Numbering_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			//
			Numbering num = (Numbering)e.Row;
			foreach (PXResult<Dimension, Segment> r in PXSelectJoin<Dimension, InnerJoin<Segment, On<Segment.dimensionID, Equal<Dimension.dimensionID>>>, Where<Dimension.numberingID, Equal<Optional<Numbering.numberingID>>, And<Segment.autoNumber, Equal<Optional<Segment.autoNumber>>>>>.Select(this, ((Numbering)e.Row).NumberingID, (bool)true))
			{
				Dimension dim = r;
				Segment segrow = r;

				cache.RaiseExceptionHandling<Numbering.numberingID>(num, num.NumberingID, new PXSetPropertyException(Messages.NumberingIsUsedFailedDelete, segrow.DimensionID, segrow.SegmentID.ToString()));
				e.Cancel = true;
			}
		}

		private struct NumPair
		{
			public int? BranchID;
			public DateTime? StartDate;
			
			public NumPair(int? BranchID, DateTime? StartDate)
			{
				this.BranchID = BranchID;
				this.StartDate = StartDate;
			}
		}

		public override void Persist()
		{
			HashSet<NumPair> uniqueStartDates = new HashSet<NumPair>();
			foreach (NumberingSequence ns in Sequence.Select())
			{
				if (Sequence.Cache.GetStatus(ns) == PXEntryStatus.Notchanged && ns.StartDate != null)
				{
					uniqueStartDates.Add(new NumPair(ns.NBranchID, ns.StartDate));
				}
			}
			foreach(NumberingSequence ns in Sequence.Cache.Inserted)
			{
				if (ns.StartDate != null && !uniqueStartDates.Add(new NumPair(ns.NBranchID, ns.StartDate)))
				{
					Sequence.Cache.RaiseExceptionHandling<NumberingSequence.startDate>(ns, ns.StartDate, new PXSetPropertyException(Messages.StartDateNotUnique));
				}
			}
			foreach (NumberingSequence ns in Sequence.Cache.Updated)
			{
				if (ns.StartDate != null && !uniqueStartDates.Add(new NumPair(ns.NBranchID, ns.StartDate)))
				{
					Sequence.Cache.RaiseExceptionHandling<NumberingSequence.startDate>(ns, ns.StartDate, new PXSetPropertyException(Messages.StartDateNotUnique));
				}
			}
			
			base.Persist();
		}
	}
}

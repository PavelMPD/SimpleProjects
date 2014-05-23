using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.EP;
using PX.SM;
using PX.TM;

namespace PX.Objects.CR
{
	#region SimpleCase

    [Serializable]
	public partial class SimpleCase : CRCase
	{
		#region CaseID

		public new abstract class caseID : IBqlField { }

		#endregion

		#region CustomerID

		public new abstract class customerID : IBqlField { }

		#endregion

		#region ContactID

		public new abstract class contactID : IBqlField { }

		#endregion

		#region Hold

		public abstract class hold : IBqlField { }

		#endregion

		#region Closed

		public abstract class closed : IBqlField { }

		#endregion

		#region Released

		public new abstract class released : IBqlField { }

		#endregion

		#region Status

		public new abstract class status : IBqlField { }

		#endregion

		#region StatusDescription

		public abstract class statusDescription : IBqlField { }

		[PXString]
		public virtual String StatusDescription { get; set; }

		#endregion

		#region Resolution

		public new abstract class resolution : IBqlField { }

		#endregion

		#region ResolutionDescription

		public abstract class resolutionDescription : IBqlField { }

		[PXString]
		public virtual String ResolutionDescription { get; set; }

		#endregion

		#region NoteID

		public new abstract class noteID : IBqlField { }

		#endregion

		#region InitResponse
		public new abstract class initResponse : IBqlField { }

		[PXUIField(DisplayName = "Init. Response", Enabled = false)]
		[PXTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
		public override Int32? InitResponse
		{
			get { return base.InitResponse; }
			set { base.InitResponse = value; }
		}

		#endregion

		#region LastActivity

		public new abstract class lastActivity : IBqlField { }

		[PXDate]
		public override DateTime? LastActivity
		{
			get { return base.LastActivity; } 
			set { base.LastActivity = value; }
		}

		#endregion

		#region LastModifiedDateTime

		public new abstract class lastModifiedDateTime : IBqlField { }

		#endregion
	}

	#endregion

	public class MySupportMaint : PXGraph<MySupportMaint>
	{
		#region Selects

		public PXSelectJoin<BAccount,
			InnerJoin<Contact, On<Contact.bAccountID, Equal<BAccount.bAccountID>>>,
			Where<Contact.userID, Equal<Current<AccessInfo.userID>>>>
			CasesByCustomer;

		#endregion

		#region Data Handlers

		public virtual IEnumerable casesByCustomer()
		{
			var command = CasesByCustomer.View.BqlSelect;
			foreach(PXResult<BAccount, Contact> row in this.QuickSelect(command))
			{
				var customer = (BAccount)row[typeof(BAccount)];
				customer.Count = CalcCountOfCases(customer.BAccountID);
				customer.LastActivity = CalcLastActivity(customer.BAccountID);
				yield return customer;
			}
		}

		#endregion

		#region Protected Methods

		protected virtual BqlCommand AddCaseLimitations(BqlCommand command)
		{
			return command;
		}

		#endregion

		#region Private Methods

		private DateTime? CalcLastActivity(int? bAccountId)
		{
			DateTime? res = null;
			if (bAccountId != null)
			{
				BqlCommand command = new Select5<EPActivity,
					InnerJoin<SimpleCase, On<SimpleCase.noteID, Equal<EPActivity.refNoteID>>,
						InnerJoin<Contact, On<Contact.contactID, Equal<SimpleCase.contactID>>>>,
					Where<SimpleCase.customerID, Equal<Required<SimpleCase.customerID>>,
						And<Contact.userID, Equal<Current<AccessInfo.userID>>>>,
					Aggregate<Max<EPActivity.lastModifiedDateTime>>>();
				command = AddCaseLimitations(command);
				var activities = new PXView(this, true, command).SelectMulti(bAccountId);
				if (activities != null && activities.Count > 0)
				{
					res = ((EPActivity)((PXResult)activities[0])[typeof(EPActivity)]).LastModifiedDateTime;
				}
				command = new Select5<SimpleCase,
					InnerJoin<Contact, On<Contact.contactID, Equal<SimpleCase.contactID>>>,
					Where<SimpleCase.customerID, Equal<Required<SimpleCase.customerID>>,
						And<Contact.userID, Equal<Current<AccessInfo.userID>>>>,
					Aggregate<Max<SimpleCase.lastModifiedDateTime>>>();
				command = AddCaseLimitations(command);
				var cases = new PXView(this, true, command).SelectMulti(bAccountId);
				if (cases != null && cases.Count > 0)
				{
					var casesLastModified = ((SimpleCase)((PXResult)cases[0])[typeof(SimpleCase)]).LastModifiedDateTime;
					if (casesLastModified != null && (res == null || res < casesLastModified)) 
						res = casesLastModified;
				}
			}
			return res;
		}

		private int CalcCountOfCases(int? bAccountId)
		{
			if (bAccountId != null)
			{
				BqlCommand command = new Select4<SimpleCase,
					Where<SimpleCase.customerID, Equal<Required<SimpleCase.customerID>>>,
					Aggregate<Count<SimpleCase.caseID>>>();
				command = AddCaseLimitations(command);
				var res = new PXView(this, true, command).SelectMulti(bAccountId);
				if (res != null && res.Count > 0)
				{
					var count = ((PXResult)res[0]).RowCount;
					if (count != null) return (int)count;
				}
			} 
			return 0;
		}

		#endregion
	}

	public class MyOpenCasesMaint : MySupportMaint
	{
		protected override BqlCommand AddCaseLimitations(BqlCommand command)
		{
			var res = base.AddCaseLimitations(command);
			return res.WhereAnd(typeof(Where2<Where<SimpleCase.closed, Equal<False>, Or<SimpleCase.closed, IsNull>>, 
				And<Where<SimpleCase.released, Equal<False>, Or<SimpleCase.released, IsNull>>>>));
		}
	}

	public class MyClosedCasesMaint : MySupportMaint
	{
		protected override BqlCommand AddCaseLimitations(BqlCommand command)
		{
			var res = base.AddCaseLimitations(command);
			return res.WhereAnd(typeof(Where<SimpleCase.closed, Equal<True>, Or<SimpleCase.released, Equal<True>>>));
		}
	}

    [Serializable]
	public class MyCasesByCustomerMaint : PXGraph<MyCasesByCustomerMaint>
	{
		#region Filter
        [Serializable]
		public partial class Filter : IBqlTable
		{
			#region IsOpen

			public abstract class isOpen : IBqlField { }

			[PXDBBool]
			public virtual Boolean? IsOpen { get; set; }

			#endregion
		}
		#endregion

		#region CaseInfo

		[Serializable]
		[PXProjection(typeof(Select2<CRCase, 
			LeftJoin<CRCaseClass, On<CRCaseClass.caseClassID, Equal<CRCase.caseClassID>>,
			LeftJoin<Users, On<Users.pKID, Equal<CRCase.ownerID>>, 
			LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<Users.pKID>>, 
			LeftJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>>>>>>))]
		public partial class CaseInfo : CRCase
		{
			#region CustomerID

			public new abstract class customerID : IBqlField { }

			#endregion

			#region Hold

			public abstract class hold : IBqlField { }

			#endregion

			#region Closed

			public abstract class closed : IBqlField { }

			#endregion

			#region Released

			public new abstract class released : IBqlField { }

			#endregion

			#region Status

			public new abstract class status : IBqlField { }

			#endregion

			#region ClassName

			public abstract class className : IBqlField { }

			[PXDBString(IsUnicode = true, BqlField = typeof(CRCaseClass.description))]
			public virtual String ClassName { get; set; }

			#endregion

			#region UserDisplayName

			public abstract class userDisplayName : IBqlField { }

			[PXDBString(IsUnicode = true, BqlField = typeof(Users.fullName))]
			public virtual String UserDisplayName { get; set; }

			#endregion

			#region EmployeeDisplayName

			public abstract class employeeDisplayName : IBqlField { }

			[PXDBString(IsUnicode = true, BqlField = typeof(Contact.displayName))]
			public virtual String EmployeeDisplayName { get; set; }

			#endregion

			#region Technician

			public abstract class technician : IBqlField { }

			[PXString]
			public virtual String Technician
			{
				[PXDependsOnFields(typeof(employeeDisplayName),typeof(userDisplayName))]
				get { return string.IsNullOrEmpty(EmployeeDisplayName) ? UserDisplayName : EmployeeDisplayName; }
			}

			#endregion
		}

		#endregion

		#region Selects

		public PXSelect<BAccount,
			Where<BAccount.bAccountID, Equal<Optional<BAccount.bAccountID>>,
				And<Where<Optional<Filter.isOpen>, Equal<True>,
					Or<Optional<Filter.isOpen>, Equal<False>>>>>>
			Customer;

		public PXSelect<CaseInfo,
			Where<CaseInfo.customerID, Equal<Current<BAccount.bAccountID>>,
				And<Where2<Where<Optional<Filter.isOpen>, Equal<True>,
							And<Where2<Where<CaseInfo.closed, Equal<False>, Or<CaseInfo.closed, IsNull>>,
									And<Where<CaseInfo.released, Equal<False>, Or<CaseInfo.released, IsNull>>>>>>,
							Or<Where<Optional<Filter.isOpen>, Equal<False>,
								And<Where<CaseInfo.closed, Equal<True>, Or<CaseInfo.released, Equal<True>>>>>>>>>>
			Cases;

		#endregion

		#region Data Handlers

		protected virtual IEnumerable customer()
		{
			var userId = PXAccess.GetUserID();
			var currentContact = (Contact)PXSelect<Contact,
				Where<Contact.userID, Equal<Required<Contact.userID>>>>.
				Select(this, userId);

			foreach (BAccount row in this.QuickSelect(Customer.View.BqlSelect))
			{
				if (currentContact == null ||
					row.BAccountID != currentContact.BAccountID)
				{
					throw new PXException(Messages.AccountNotFound);
				}

				var isOpenStr = PXView.Parameters[1].With(p => p.ToString());
				bool isOpen;
				if (bool.TryParse(isOpenStr, out isOpen))
					row.CasesCount = CalcAccountCases(row.BAccountID, isOpen);
				else row.CasesCount = 0;
				yield return row;
			}
		}

		protected virtual IEnumerable cases()
		{
			var userId = PXAccess.GetUserID();
			var currentContact = (Contact)PXSelect<Contact,
				Where<Contact.userID, Equal<Required<Contact.userID>>>>.
				Select(this, userId);
			foreach (CaseInfo row in this.QuickSelect(Cases.View.BqlSelect))
			{
				if (row.CreatedByID != userId && 
					(currentContact == null || 
						(row.ContactID != currentContact.ContactID && row.CustomerID != currentContact.BAccountID)))
				{
					continue;
				}
				yield return row;
			}
		}

		private int CalcAccountCases(int? bAccountId, bool isOpen)
		{
			if (bAccountId != null)
			{
				BqlCommand command = new Select4<SimpleCase,
					Where<SimpleCase.customerID, Equal<Required<SimpleCase.customerID>>>,
					Aggregate<Count<SimpleCase.caseID>>>();
				if (isOpen)
					command = command.WhereAnd(
						typeof(Where2<Where<SimpleCase.closed, Equal<False>, Or<SimpleCase.closed, IsNull>>, 
							And<Where<SimpleCase.released, Equal<False>, Or<SimpleCase.released, IsNull>>>>));
				else
				{
					command = command.WhereAnd(
						typeof(Where<SimpleCase.closed, Equal<True>, Or<SimpleCase.released, Equal<True>>>));
				}
				var res = new PXView(this, true, command).SelectMulti(bAccountId);
				if (res != null && res.Count > 0)
				{
					var count = ((PXResult)res[0]).RowCount;
					if (count != null) return (int)count;
				}
			} 
			return 0;
		}

		#endregion
	}

    [Serializable]
	public class MyCaseDetailsMaint : PXGraph<MyCaseDetailsMaint>
	{
		#region EPActivityWithOwner

		[PXProjection(typeof(Select2<EPActivity,
			LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<EPActivity.owner>>,
			LeftJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>,
			LeftJoin<Users, On<Users.pKID, Equal<EPActivity.owner>>>>>>))]
        [Serializable]
		public partial class EPActivityWithOwner : EPActivity
		{
			#region RefNoteID

			public new abstract class refNoteID : IBqlField { }

			#endregion

			#region IsPrivate

			public new abstract class isPrivate : IBqlField { }

			#endregion

			#region EmployeeDisplayName

			public abstract class employeeDisplayName : IBqlField { }

			[PXDBString(BqlField = typeof(Contact.displayName))]
			public virtual String EmployeeDisplayName { get; set; }

			#endregion

			#region UserDisplayName

			public abstract class userDisplayName : IBqlField { }

			[PXDBString(BqlField = typeof(Users.fullName))]
			public virtual String UserDisplayName { get; set; }

			#endregion

			#region OwnerName

			public abstract class ownerName : IBqlField { }

			[PXString]
			public virtual String OwnerName
			{
				[PXDependsOnFields(typeof(employeeDisplayName),typeof(userDisplayName))]
				get
				{
					return string.IsNullOrEmpty(EmployeeDisplayName) 
						? UserDisplayName 
						: EmployeeDisplayName;
				}
			}

			#endregion

			#region Summary

			public abstract class summary : IBqlField { }

			[PXString]
			public virtual String Summary
			{
				[PXDependsOnFields(typeof(body),typeof(subject))]
				get
				{
					var text = Tools.ConvertHtmlToSimpleText(Body).
						With(_ => Regex.Replace(_, "(" + Environment.NewLine + "){2,}", "<br /><br />", RegexOptions.Compiled).
						Replace(Environment.NewLine, "<br />"));
					return Subject + "<br />" + text;
				}
			}

			#endregion
		}

		#endregion

		#region Selects

		[PXHidden]
		public PXSelect<EPActivity>
			BaseEPActivity;

		public PXSelect<SimpleCase,
			Where<SimpleCase.caseID, Equal<Optional<SimpleCase.caseID>>>>
			Case;

		public PXSelect<EPActivityWithOwner,
			Where<EPActivityWithOwner.refNoteID, Equal<Current<SimpleCase.noteID>>,
				And<Where<EPActivityWithOwner.isPrivate, Equal<False>, 
					Or<EPActivityWithOwner.isPrivate, IsNull>>>>>
			History;

		public PXSelect<EPActivity,
			Where<EPActivity.refNoteID, Equal<Current<SimpleCase.noteID>>>>
			Activities;

		#endregion

		#region Ctors

		public MyCaseDetailsMaint()
		{
			CorrectUI();

			AttachDataHandlers();
		}

		private void AttachDataHandlers()
		{
			var oldCaseView = Case.View;
			var newCaseView = new PXView(oldCaseView.Graph, oldCaseView.IsReadOnly, oldCaseView.BqlSelect, 
				new PXSelectDelegate(CaseHandler));
			Case.View = newCaseView;
			Views["Case"] = newCaseView;
		}

		private void CorrectUI()
		{
			PXUIFieldAttribute.SetDisplayName<EPActivity.subject>(Activities.Cache, Messages.Subject);
			PXUIFieldAttribute.SetDisplayName<EPActivity.body>(Activities.Cache, Messages.Description);
		}

		#endregion

		#region Actions

		public PXAction<SimpleCase> AddNote;

		[PXButton]
		[PXUIField(Visible = false)]
		public virtual IEnumerable addNote(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<SimpleCase> Close;

		[PXButton]
		[PXUIField(Visible = false)]
		public virtual IEnumerable close(PXAdapter adapter)
		{
			var res = new List<object>();
			foreach(SimpleCase @case in adapter.Get())
				if (IsOpen(@case))
				{
					@case.MajorStatus = CRCaseMajorStatusesAttribute._CLOSED;
					@case.Status = CRCaseStatusesAttribute._CLOSED;
					@case.Resolution = CRCaseResolutionsAttribute._RESOLVED;
					@case.ResolutionDate = PXTimeZoneInfo.Now;
					Case.Cache.Update(@case);
					Actions.PressSave();
					SelectTimeStamp();
					res.Add(@case);
					break;
				}
			if (res.Count == 0)
				throw new PXException(Messages.CannotCloseClosedCase);
			return res;
		}

		#endregion

		#region Data Handlers

		public IEnumerable CaseHandler()
		{
			var @case = (SimpleCase)PXSelect<SimpleCase,
				Where<SimpleCase.caseID, Equal<Optional<SimpleCase.caseID>>>>.
				Select(this, PXView.Parameters);
			var userId = PXAccess.GetUserID();
			if (@case != null && @case.CreatedByID != userId)
			{
				var currentContact = (Contact)PXSelect<Contact,
					Where<Contact.userID, Equal<Required<Contact.userID>>>>.
					Select(this, userId);
				if (currentContact == null ||
					(@case.ContactID != currentContact.ContactID && @case.CustomerID != currentContact.BAccountID))
				{
					@case = null;
				}
			}
			if (@case == null)
				throw new PXException(Messages.CaseNotFound);
			yield return @case;
		}

		#endregion

		#region Event Handlers

		protected virtual void SimpleCase_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = e.Row as SimpleCase;
			if (row == null) return;

			row.StatusDescription = "unknown";
			if (!string.IsNullOrEmpty(row.Status))
			{
				var statusState = cache.GetStateExt(row, typeof(SimpleCase.status).Name) as PXStringState;
				if (statusState != null && statusState.AllowedLabels != null && statusState.AllowedValues != null)
				{
					var index = Array.FindIndex(statusState.AllowedValues, 0, s => string.Compare(s, row.Status, StringComparison.OrdinalIgnoreCase) == 0);
					if (index > -1) row.StatusDescription = statusState.AllowedLabels[index];
				}
			}

			row.ResolutionDescription = string.Empty;
			if (!string.IsNullOrEmpty(row.Resolution))
			{
				var resolutionState = cache.GetStateExt(row, typeof(SimpleCase.resolution).Name) as PXStringState;
				if (resolutionState != null && resolutionState.AllowedLabels != null && resolutionState.AllowedValues != null)
				{
					var index = Array.FindIndex(resolutionState.AllowedValues, 0, s => string.Compare(s, row.Status, StringComparison.OrdinalIgnoreCase) == 0);
					if (index > -1) row.ResolutionDescription = resolutionState.AllowedLabels[index];
				}
			}
		}

		protected virtual void EPActivity_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			if (Case.Current == null) e.Cancel = true;
			else
			{
				var row = e.Row as EPActivity;
				if (row != null)
				{
					row.StartDate = PXTimeZoneInfo.Now;
					row.RefNoteID = Case.Current.NoteID;
					row.ClassID = CRActivityClass.Activity;
					row.IsExternal = true;
				}
			}
		}

		protected virtual void EPActivity_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			var row = e.Row as EPActivity;
			var @case = Case.Current;
			if (row == null || @case == null) return;

			if (row.IsExternal == true && 
				@case.MajorStatus != CRCaseMajorStatusesAttribute._CLOSED && 
				@case.MajorStatus != CRCaseMajorStatusesAttribute._RELEASED && 
				@case.Released != true && @case.Status == CRCaseStatusesAttribute._PENDING_CUSTOMER)
			{
				@case.Status = CRCaseStatusesAttribute._OPEN;
				@case.Resolution = CRCaseResolutionsAttribute._CUSTOMER_REPLIED;
				Case.Cache.Update(@case);
			}
		}

		#endregion

		protected virtual bool IsOpen(SimpleCase @case)
		{
			return @case == null || 
				@case.MajorStatus != CRCaseMajorStatusesAttribute._CLOSED && 
				@case.MajorStatus != CRCaseMajorStatusesAttribute._RELEASED && 
				@case.Released != true;
		}
	}
}

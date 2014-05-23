using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.TM;

namespace PX.Objects.EP
{
	[System.SerializableAttribute()]
	public partial class EmployeeClaimsEnqFilter : PX.Data.IBqlTable
	{
		#region EmployeeID
		public abstract class employeeID : PX.Data.IBqlField
		{
		}
		protected Int32? _EmployeeID;
		[PXDBInt()]
		[PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>))]
		[PXSubordinateSelector]
		[PXUIField(DisplayName = "Employee ID", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? EmployeeID
		{
			get
			{
				return this._EmployeeID;
			}
			set
			{
				this._EmployeeID = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _EndDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
		#endregion
		#region OnHold
		public abstract class onHold : PX.Data.IBqlField
		{
		}
		protected bool? _OnHold;
		[PXDefault(true)]
		[PXDBBool()]
		[PXUIField(DisplayName = "On Hold", Visibility = PXUIVisibility.Visible)]
		public virtual bool? OnHold
		{
			get
			{
				return this._OnHold;
			}
			set
			{
				this._OnHold = value;
			}
		}
		#endregion
		#region Pending
		public abstract class pending : PX.Data.IBqlField
		{
		}
		protected bool? _Pending;
		[PXDefault(true)]
		[PXDBBool()]
		[PXUIField(DisplayName = "Pending Approval", Visibility = PXUIVisibility.Visible)]
		public virtual bool? Pending
		{
			get
			{
				return this._Pending;
			}
			set
			{
				this._Pending = value;
			}
		}
		#endregion
		#region Approved
		public abstract class approved : PX.Data.IBqlField
		{
		}
		protected bool? _Approved;
		[PXDBBool()]
		[PXUIField(DisplayName = "Approved", Visibility = PXUIVisibility.Visible)]
		public virtual bool? Approved
		{
			get
			{
				return this._Approved;
			}
			set
			{
				this._Approved = value;
			}
		}
		#endregion		
		#region Released
		public abstract class released : PX.Data.IBqlField
		{
		}
		protected bool? _Released;
		[PXDBBool()]
		[PXUIField(DisplayName = "Released", Visibility = PXUIVisibility.Visible)]
		public virtual bool? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion
		#region Voided
		public abstract class voided : PX.Data.IBqlField
		{
		}
		protected bool? _Voided;
		[PXDBBool()]
		[PXUIField(DisplayName = "Rejected", Visibility = PXUIVisibility.Visible)]
		public virtual bool? Voided
		{
			get
			{
				return this._Voided;
			}
			set
			{
				this._Voided = value;
			}
		}
		#endregion
	}

	[TableDashboardType]
	public class EmployeeClaimsEnq : PXGraph<EmployeeClaimsEnq>
	{
        public PXCancel<EmployeeClaimsEnqFilter> cancel;
        public PXFilter<EmployeeClaimsEnqFilter> Filter;
		[PXFilterable]
		public PXSelect<EPExpenseClaim> ExpenseClaimRecords;

		public PXSetup<EPSetup> EPSetup;

		public EmployeeClaimsEnq()
		{
			EPSetup setup = EPSetup.Current;
			if(Filter.Current != null)
			{
				Filter.Current.OnHold = true;
				Filter.Current.Pending = true;
			}
			PXUIFieldAttribute.SetRequired<EPExpenseClaim.docDate>(this.ExpenseClaimRecords.Cache, false);
			PXUIFieldAttribute.SetRequired<EPExpenseClaim.docDesc>(this.ExpenseClaimRecords.Cache, false);
			PXUIFieldAttribute.SetRequired<EPExpenseClaim.curyID>(this.ExpenseClaimRecords.Cache, false);
		}

		protected virtual IEnumerable expenseClaimRecords()
		{
			EmployeeClaimsEnqFilter filter = Filter.Current as EmployeeClaimsEnqFilter;

			this.ExpenseClaimRecords.Cache.AllowInsert = false;
			this.ExpenseClaimRecords.Cache.AllowDelete = false;
			this.ExpenseClaimRecords.Cache.AllowUpdate = false;

			ExpenseClaimRecords.Cache.Clear();

			PXSelectBase<EPExpenseClaim> select =
				new PXSelectJoin<EPExpenseClaim,
					InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPExpenseClaim.employeeID>>>,
					Where<EPExpenseClaim.employeeID, Equal<Current<EmployeeClaimsEnqFilter.employeeID>>>,
					OrderBy<Desc<EPExpenseClaim.docDate,
						Asc<EPExpenseClaim.refNbr>>>>(this);

			if(filter.StartDate.HasValue)
				select.WhereAnd<Where<EPExpenseClaim.docDate,GreaterEqual<Current<EmployeeClaimsEnqFilter.startDate>>>>();
			if(filter.EndDate.HasValue)
				select.WhereAnd<Where<EPExpenseClaim.docDate,LessEqual<Current<EmployeeClaimsEnqFilter.endDate>>>>();

			foreach (EPExpenseClaim claim in select.Select())				
			{
				if ((((filter.Released??false) && claim.Status == EPClaimStatus.Released) ||
                    ((filter.Approved ?? false) && claim.Status == EPClaimStatus.Approved) ||
                    ((filter.Pending ?? false) && claim.Status == EPClaimStatus.Balanced) ||
                    ((filter.Voided ?? false) && claim.Status == EPClaimStatus.Voided) ||
                    ((filter.OnHold ?? false) && claim.Status == EPClaimStatus.Hold)))
				{
					yield return claim;
				}
			}
		}

		public PXAction<EmployeeClaimsEnqFilter> claimDetails;
		[PXUIField(DisplayName = Messages.ClaimDetails, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ClaimDetails(PXAdapter adapter)
		{
			if (ExpenseClaimRecords.Current != null && Filter.Current != null)
			{
				ExpenseClaimEntry graph = PXGraph.CreateInstance<ExpenseClaimEntry>();
				graph.ExpenseClaim.Current = ExpenseClaimRecords.Current;
				throw new PXRedirectRequiredException(graph, true, Messages.ClaimDetails){Mode = PXBaseRedirectException.WindowMode.NewWindow};
			}
			return adapter.Get();
		}
	}
}

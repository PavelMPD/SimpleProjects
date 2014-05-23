using System.Collections.Generic;
using PX.Data.EP;
using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.TM;
using PX.Objects.TX;
using PX.Objects.AR;
using PX.Objects.PM;
using PX.Objects.GL;

namespace PX.Objects.CR
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.Opportunity)]
	[PXPrimaryGraph(typeof(OpportunityMaint))]
	[CREmailContactsView(typeof(Select2<Contact,
		LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
		Where2<Where<Optional<CROpportunity.bAccountID>, IsNull, And<Contact.contactID, Equal<Optional<CROpportunity.contactID>>>>, 
			  Or2<Where<Optional<CROpportunity.bAccountID>, IsNotNull, And<Contact.bAccountID, Equal<Optional<CROpportunity.bAccountID>>>>,
				Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>>))]
	[PXEMailSource]//NOTE: for assignment map
	public partial class CROpportunity : PX.Data.IBqlTable, IAssign, IAttributeSupport, IPXSelectable
	{
		#region Selected
		public abstract class selected : IBqlField { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Service)]
		public virtual bool? Selected { get; set; }
		#endregion

		#region OpportunityNumber
		public abstract class opportunityNumber : PX.Data.IBqlField { }

		[PXDBIdentity]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual Int32? OpportunityNumber { get; set; }
		#endregion

		#region OpportunityID
		public abstract class opportunityID : PX.Data.IBqlField { }

		public const int OpportunityIDLength = 10;

		[PXDBString(OpportunityIDLength, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Opportunity ID", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(CRSetup.opportunityNumberingID), typeof(AccessInfo.businessDate))]
		[PXSelector(typeof(Search3<CROpportunity.opportunityID,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CROpportunity.bAccountID>>,
			LeftJoin<Contact, On<Contact.contactID, Equal<CROpportunity.contactID>>>>, 
			OrderBy<Desc<CROpportunity.opportunityID>>>),
			new[] { typeof(CROpportunity.opportunityID), 
				typeof(CROpportunity.opportunityName),
				typeof(CROpportunity.status), 
				typeof(CROpportunity.curyAmount),
				typeof(CROpportunity.curyID), 
				typeof(CROpportunity.closeDate),
				typeof(CROpportunity.stageID),
				typeof(CROpportunity.cROpportunityClassID),
				typeof(BAccount.acctName),
				typeof(Contact.displayName) },
				Filterable = true)]
		[PXFieldDescription]
		public virtual String OpportunityID { get; set; }
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected Int32? _BranchID;
		[Branch(typeof(Coalesce<
			Search<Location.cBranchID, Where<Location.bAccountID, Equal<Current<CROpportunity.bAccountID>>, And<Location.locationID, Equal<Current<CROpportunity.locationID>>>>>,
			Search<Branch.branchID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>), IsDetail = false)]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.IBqlField { }

		private int? _BAccountID;
		[CustomerAndProspect(DisplayName = "Business Account")]
		public virtual Int32? BAccountID
		{
			get
			{
				return _BAccountID;
			}
			set
			{
				_BAccountID = value;
			}
		}
		#endregion

		#region LocationID
		public abstract class locationID : PX.Data.IBqlField { }

		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<CROpportunity.bAccountID>>>),
			DisplayName = "Location", DescriptionField = typeof(Location.descr))]
		public virtual Int32? LocationID { get; set; }
		#endregion

		#region ContactID
		public abstract class contactID : PX.Data.IBqlField { }

		[PXDBInt]
		[PXUIField(DisplayName = "Contact")]
		[PXSelector(typeof(Search2<Contact.contactID,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
			Where2<Where2<
						 Where<Contact.contactType, Equal<ContactTypesAttribute.person>,
								Or<Contact.contactType, Equal<ContactTypesAttribute.lead>>>,
							And<
								Where<BAccount.type, IsNull,
									Or<BAccount.type, Equal<BAccountType.customerType>,
										Or<BAccount.type, Equal<BAccountType.prospectType>,
										Or<BAccount.type, Equal<BAccountType.combinedType>>>>>>>,
				And<Where<BAccount.bAccountID, Equal<Current<CROpportunity.bAccountID>>,
						Or<Current<CROpportunity.bAccountID>, IsNull>>>>>),
			DescriptionField = typeof(Contact.displayName), Filterable = true, DirtyRead = true)]
		[PXRestrictor(typeof(Where<Contact.isActive, Equal<True>>), Messages.ContactInactive, typeof(Contact.displayName))]
		public virtual Int32? ContactID { get; set; }
		#endregion

		#region CROpportunityClassID
		public abstract class cROpportunityClassID : PX.Data.IBqlField { }

		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Class ID")]
		[PXDefault]
		[PXSelector(typeof(CROpportunityClass.cROpportunityClassID),
			DescriptionField = typeof(CROpportunityClass.description), CacheGlobal = true)]
		[PXMassUpdatableField]
		public virtual String CROpportunityClassID { get; set; }
		#endregion

		#region OpportunityName
		public abstract class opportunityName : PX.Data.IBqlField { }

		[PXDBString(255, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String OpportunityName { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.IBqlField { }

		[PXDBText(IsUnicode = true)]
		[PXUIField(DisplayName = "Details")]
		public virtual String Description { get; set; }
		#endregion

		#region ParentBAccountID
		public abstract class parentBAccountID : PX.Data.IBqlField { }

		[CustomerAndProspect(DisplayName = "Parent Account")]
		public virtual Int32? ParentBAccountID { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.IBqlField { }

		[ProjectDefault(BatchModule.CR,
			typeof(Search<Location.cDefProjectID,
				Where<Location.bAccountID, Equal<Current<CROpportunity.bAccountID>>,
					And<Location.locationID, Equal<Current<CROpportunity.locationID>>>>>))]
		[PM.ActiveProjectForModule(BatchModule.CR, typeof(CROpportunity.bAccountID), false, true)]
		public virtual Int32? ProjectID { get; set; }
		#endregion

		#region CloseDate
		public abstract class closeDate : PX.Data.IBqlField { }

		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Estimation", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? CloseDate { get; set; }
		#endregion

		#region StageID
		public abstract class stageID : PX.Data.IBqlField { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Stage")]
		[CROpportunityStages]
		[PXDefault(CROpportunityStagesAttribute._PROSPECT)]
		[PXMassUpdatableField]
		public virtual String StageID { get; set; }
		#endregion

		#region CampaignSourceID
		public abstract class campaignSourceID : PX.Data.IBqlField { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Campaign")]
		[PXSelector(typeof(Search3<CRCampaign.campaignID, OrderBy<Desc<CRCampaign.campaignID>>>),
			DescriptionField = typeof(CRCampaign.campaignName), Filterable = true)]
		public virtual String CampaignSourceID { get; set; }
		#endregion

		#region MajorStatus
		public abstract class majorStatus : IBqlField { }

		private int? _MajorStatus;
		[PXDBInt]
		[OpportunityMajorStatusesAttribute]
		[PXDefault(OpportunityMajorStatusesAttribute._JUSTCREATED, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Visible = false)]
		public virtual int? MajorStatus
		{
			get
			{
				return _MajorStatus;
			} 
			set
			{
				_MajorStatus = value;
			} 
		}

		#endregion

		#region Status
		public abstract class status : PX.Data.IBqlField { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
		[PXStringList(new string[0], new string[0])]
		[PXDefault()] 
		[PXMassUpdatableField]
		public virtual string Status { get; set; }

		#endregion

		#region Resolution
		public abstract class resolution : PX.Data.IBqlField { }

		[PXDBString(2, IsFixed = true)]
		[PXStringList(new string[0], new string[0])]
		[PXUIField(DisplayName = "Reason")]
		[PXMassUpdatableField]
		public virtual String Resolution { get; set; }
		#endregion

		#region AssignDate
		public abstract class assignDate : IBqlField { }

		[PXDBDate(PreserveTime = true)]
		[PXUIField(DisplayName = "Assignment Date")]
		public virtual DateTime? AssignDate { get; set; }
		#endregion

		#region ClosingDate
		public abstract class closingDate : IBqlField { }

		[PXDBDate(PreserveTime = true)]
		[PXUIField(DisplayName = "Closing Date")]
		public virtual DateTime? ClosingDate { get; set; }
		#endregion

		#region WorkgroupID
		public abstract class workgroupID : PX.Data.IBqlField { }

		[PXDBInt]
		[PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Workgroup")]
		[PXMassUpdatableField]
		public virtual int? WorkgroupID { get; set; }
		#endregion

		#region OwnerID
		public abstract class ownerID : IBqlField { }

		[PXDBGuid()]
		[PXOwnerSelector(typeof(CROpportunity.workgroupID))]
		[PXUIField(DisplayName = "Owner")]
		[PXMassUpdatableField]
		public virtual Guid? OwnerID { get; set; }
		#endregion

		#region CuryID
		public abstract class curyID : PX.Data.IBqlField { }

		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXDefault(typeof(Search<CRSetup.defaultCuryID>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Currency.curyID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CuryID { get; set; }
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.IBqlField { }

		[PXDBLong()]
		[CurrencyInfo(ModuleCode = "CR")]
		public virtual Int64? CuryInfoID { get; set; }
		#endregion

		#region LineTotal
		public abstract class lineTotal : PX.Data.IBqlField { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineTotal { get; set; }
		#endregion

		#region CuryLineTotal
		public abstract class curyLineTotal : PX.Data.IBqlField { }

		[PXDBCurrency(typeof(CROpportunity.curyInfoID), typeof(CROpportunity.lineTotal))]
		[PXUIField(DisplayName = "Detail Total", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryLineTotal { get; set; }
		#endregion

		#region IsTaxValid
		public abstract class isTaxValid : PX.Data.IBqlField
		{
		}
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Tax is up to date", Enabled = false)]
		public virtual Boolean? IsTaxValid
		{
			get;
			set;
		}
		#endregion

		#region TaxTotal
		public abstract class taxTotal : PX.Data.IBqlField { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TaxTotal { get; set; }
		#endregion

		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.IBqlField { }

		[PXDBCurrency(typeof(CROpportunity.curyInfoID), typeof(CROpportunity.taxTotal))]
		[PXUIField(DisplayName = "Tax Total", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTaxTotal { get; set; }
		#endregion

		#region ManualTotal
		public abstract class manualTotalEntry : PX.Data.IBqlField { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Manual Amount")]
		public virtual Boolean? ManualTotalEntry { get; set; }
		#endregion

		#region Amount
		public abstract class amount : PX.Data.IBqlField { }

		private decimal? _amount;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBBaseCury()]
		[PXUIField(DisplayName = "Amount")]
		public virtual Decimal? Amount
		{
			[PXDependsOnFields(typeof(lineTotal))]
			get { return ManualTotalEntry == true ? _amount : LineTotal; }
			set { _amount = value; }
		}

		#endregion

		#region RawAmount
		public abstract class rawAmount : PX.Data.IBqlField { }

		[PXBaseCury]
		[PXDBCalced(typeof(Switch<Case<Where<CROpportunity.manualTotalEntry, Equal<True>>, CROpportunity.amount>, CROpportunity.lineTotal>), typeof(decimal))]
		public virtual Decimal? RawAmount
		{
			get;
			set;
		}

		#endregion
		#region CuryAmount
		public abstract class curyAmount : PX.Data.IBqlField { }

		private decimal? _curyAmount;

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(CROpportunity.curyInfoID), typeof(CROpportunity.amount))]
		[PXDependsOnFields(typeof(manualTotalEntry), typeof(curyLineTotal))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? CuryAmount
		{
			get { return ManualTotalEntry == true ? _curyAmount : CuryLineTotal; }
			set { _curyAmount = value; }
		}

		#endregion

		#region DiscTot
		public abstract class discTot : PX.Data.IBqlField { }

		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DiscTot { get; set; }
		#endregion

		#region CuryDiscTot
		public abstract class curyDiscTot : PX.Data.IBqlField { }

		[PXDBCurrency(typeof(CROpportunity.curyInfoID), typeof(CROpportunity.discTot))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Discount")]
		public virtual Decimal? CuryDiscTot { get; set; }
		#endregion

		#region ProductsAmount
		public abstract class productsAmount : PX.Data.IBqlField { }
		[PXDependsOnFields(typeof(amount), typeof(discTot), typeof(manualTotalEntry), typeof(lineTotal))]
		[PXDBDecimal(4)]
		public virtual Decimal? ProductsAmount
		{
			get
			{
				var res = (Amount ?? 0) - (DiscTot ?? 0);
				return res < 0 ? 0 : res;
			}
		}
		#endregion

		#region CuryProductsAmount
		public abstract class curyProductsAmount : PX.Data.IBqlField { }
		[PXDependsOnFields(typeof(curyAmount), typeof(curyDiscTot))]
		[PXDBCurrency(typeof(CROpportunity.curyInfoID), typeof(CROpportunity.productsAmount))]
		[PXUIField(DisplayName = "Total", Enabled = false)]
		public virtual Decimal? CuryProductsAmount
		{
			get
			{
				var res = (CuryAmount ?? 0m) - (CuryDiscTot ?? 0m);
				return res < 0m ? 0m : res;
			}
		}
		#endregion

		#region CuryWgtAmount
		public abstract class curyWgtAmount : PX.Data.IBqlField { }

		[PXDecimal]
		[PXUIField(DisplayName = "Wgt. Total", Enabled = false)]
		public virtual Decimal? CuryWgtAmount { get; set; }
		#endregion


		#region CuryVatExemptTotal
		public abstract class curyVatExemptTotal : PX.Data.IBqlField { }

		[PXDBCurrency(typeof(CROpportunity.curyInfoID), typeof(CROpportunity.vatExemptTotal))]
		[PXUIField(DisplayName = "VAT Exempt Total", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatExemptTotal { get; set; }
		#endregion

		#region VatExemptTaxTotal
		public abstract class vatExemptTotal : PX.Data.IBqlField { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatExemptTotal { get; set; }
		#endregion

		#region CuryVatTaxableTotal
		public abstract class curyVatTaxableTotal : PX.Data.IBqlField { }

		[PXDBCurrency(typeof(CROpportunity.curyInfoID), typeof(CROpportunity.vatTaxableTotal))]
		[PXUIField(DisplayName = "VAT Taxable Total", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryVatTaxableTotal { get; set; }
		#endregion

		#region VatTaxableTotal
		public abstract class vatTaxableTotal : PX.Data.IBqlField { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? VatTaxableTotal { get; set; }
		#endregion

		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.IBqlField { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone")]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		public virtual String TaxZoneID { get; set; }
		#endregion


		#region ARRefNbr
		public abstract class aRRefNbr : IBqlField { }

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "AR Reference Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<ARInvoice.refNbr, Where<ARInvoice.docType, Equal<ARDocType.invoice>>>))]
		public virtual String ARRefNbr { get; set; }
		#endregion

		#region OrderType
		public abstract class orderType : PX.Data.IBqlField { }

		[PXDBString(2, IsFixed = true)]
		public virtual String OrderType { get; set; }
		#endregion

		#region OrderNbr
		public abstract class orderNbr : PX.Data.IBqlField { }

		[PXDBString(15, IsUnicode = true)]
		public virtual String OrderNbr { get; set; }
		#endregion


		#region NoteID
		public abstract class noteID : PX.Data.IBqlField { }

		[PXNote(typeof(CROpportunity),
			typeof(CRCase.status),
			typeof(CRCase.subject),
			typeof(BAccount.acctCD),
			typeof(BAccount.acctName),
			typeof(Contact.displayName),
			DescriptionField = typeof(CROpportunity.opportunityID),
			Selector = typeof(CROpportunity.opportunityID),
			ShowInReferenceSelector = true)]
		public virtual Int64? NoteID { get; set; }
		#endregion

		#region LastActivity
		public abstract class lastActivity : IBqlField { }
		[PXDBScalar(typeof(Search<CRActivityStatistics.lastActivityDate, Where<CRActivityStatistics.noteID, Equal<noteID>>>))]
		[PXDate]
		[PXUIField(DisplayName = "Last Activity Date", Enabled = false)]
		public virtual DateTime? LastActivity { get; set; }
		#endregion

		#region Source
		public abstract class source : IBqlField { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Source", Visibility = PXUIVisibility.Visible, Visible = true)]
		[CRMSources]
		[PXMassUpdatableField]
		[PXFormula(typeof(Selector<CROpportunity.contactID, Contact.source>))]
		public virtual String Source { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.IBqlField { }

		[PXDBTimestamp()]
		public virtual Byte[] tstamp { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.IBqlField { }

		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.IBqlField { }

		[PXDBCreatedByID()]
		[PXUIField(DisplayName = "Created By")]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.IBqlField { }

		[PXDBCreatedDateTimeUtc]
		[PXUIField(DisplayName = "Date Created", Enabled = false)]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.IBqlField { }

		[PXDBLastModifiedByID()]
		[PXUIField(DisplayName = "Last Modified By")]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.IBqlField { }

		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.IBqlField { }

		[PXDBLastModifiedDateTimeUtc]
		[PXUIField(DisplayName = "Last Modified Date", Enabled = false)]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion

		#region Attributes
		public abstract class attributes : IBqlField { }

		[CRAttributesField(typeof(CSAnswerType.opportunityAnswerType), 
			typeof(CROpportunity.opportunityNumber), 
			typeof(CROpportunity.cROpportunityClassID))]
		public virtual string[] Attributes { get; set; }
		#endregion

		#region IAttributeSupport Members

		public int? ID
		{
			get { return OpportunityNumber; }
		}

		public string EntityType
		{
			get { return CSAnswerType.Opportunity; }
		}

		public string ClassID
		{
			get { return CROpportunityClassID; }
		}

		#endregion
	}

	public class Allowed<StringlistValue> : BqlFormula<StringlistValue>, ISwitch
		where StringlistValue : IBqlOperand
	{
		public override void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			if (!cache.GetAttributesReadonly(item, OuterField.Name).Exists(attr => attr is PXStringListAttribute)) return;

			string val = (string) Calculate<StringlistValue>(cache, item);
			PXStringState state = (PXStringState) cache.GetStateExt(item, OuterField.Name);
			value = new List<string>(state.AllowedValues).Exists(str => String.CompareOrdinal(str, val) == 0) ? val : null;
		}

		public Type OuterField { get; set; }
	}
}

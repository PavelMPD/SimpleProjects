using System;
using System.Collections.Specialized;
using System.Linq;
using Avalara.AvaTax.Adapter;
using Avalara.AvaTax.Adapter.TaxService;
using PX.Common;
using PX.Data;
using System.Collections;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.Objects.EP;
using PX.Objects.SO;
using System.Collections.Generic;
using System.Diagnostics;
using PX.Objects.GL;
using PX.SM;
using AvaAddress = Avalara.AvaTax.Adapter.AddressService;

namespace PX.Objects.CR
{
	public class OpportunityMaint : PXGraph<OpportunityMaint>
	{
		public partial class ConvertedFilter : IBqlTable
		{
			#region OpportunityID
			public abstract class opportunityID : PX.Data.IBqlField { }
			[PXDBInt]
			[PXUIField(DisplayName = "Opportunity ID", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDefault(typeof(CROpportunity.opportunityID))]
			public virtual String OpportunityID { get; set; }
			#endregion
		}

		#region CreateSalesOrderFilter

		[Serializable()]
		public partial class CreateSalesOrderFilter : IBqlTable
		{
			#region OrderType

			public abstract class orderType : IBqlField
			{
			}

			[PXDBString(2, IsFixed = true, InputMask = ">aa")]
			[PXDefault(SOOrderTypeConstants.SalesOrder)]
			[PXSelector(typeof (Search<SOOrderType.orderType, Where<SOOrderType.active, Equal<boolTrue>>>),
				DescriptionField = typeof (SOOrderType.descr))]
			[PXUIField(DisplayName = "Order Type")]
			public virtual String OrderType { get; set; }

			#endregion

			#region RecalcDiscounts

			public abstract class recalcDiscount : IBqlField
			{
			}

			[PXDBBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Recalculate Prices and Discounts")]
			public virtual Boolean? RecalcDiscounts { get; set; }

			#endregion
		}

		#endregion

		#region Selects / Views
        [PXHidden()]
        public PXSelect<BAccount> BAccounts;

		//TODO: need review
		[PXHidden]
		public PXSelect<BAccount>
			bAccountBasic;

		[PXHidden]
		public PXSetupOptional<SOSetup>
			sosetup;

		[PXHidden]
		public PXSetup<CRSetup>
			Setup;

		public PXSelect<CurrencyInfo,
			Where<CurrencyInfo.curyInfoID, Equal<Current<CROpportunity.curyInfoID>>>>
			currencyinfo;

		public ToggleCurrency<CROpportunity>
			CurrencyView;

		[PXViewName(Messages.Opportunity)]
		public PXSelect<CROpportunity> 
			Opportunity;

		[PXHidden]
		public PXSelect<CROpportunity,
			Where<CROpportunity.opportunityID, Equal<Current<CROpportunity.opportunityID>>>>
			OpportunityCurrent;

		[PXHidden]
		public PXSelect<CROpportunityProbability,
			Where<CROpportunityProbability.stageCode, Equal<Current<CROpportunity.stageID>>>>
			ProbabilityCurrent;

		[PXHidden]
		public PXSelect<Address>
			Address;

		[PXHidden]
		public PXSetup<Contact, Where<Contact.contactID, Equal<Optional<CROpportunity.contactID>>>> Contacts;

		[PXViewName(Messages.Answers)]
		public CRAttributeSourceList<CROpportunity, CROpportunity.contactID>
			Answers;

		[PXViewName(Messages.Answers)]
		public CRAttributeList<Contact> ContactAnswers;

		[PXViewName(Messages.Activities)]
		[PXFilterable]
		public OpportunityActivities Activities;

		[PXViewName(Messages.Relations)]
		[PXFilterable]
		public CRRelationsList<CROpportunity.noteID>
			Relations;

		[PXViewName(Messages.OpportunityProducts)]
		public PXSelect<CROpportunityProducts,
			Where<CROpportunityProducts.cROpportunityID, Equal<Current<CROpportunity.opportunityID>>>>
			Products;

		[PXViewName(Messages.OpportunityTax)]
		public PXSelectJoin<CRTaxTran,
			InnerJoin<Tax, On<Tax.taxID, Equal<CRTaxTran.taxID>>>,
			Where<CRTaxTran.opportunityID, Equal<Current<CROpportunity.opportunityID>>>>
			Taxes;

		[PXViewName(Messages.DiscountDetails)]
		public PXSelect<CROpportunityDiscountDetail,
			Where<CROpportunityDiscountDetail.opportunityID, Equal<Current<CROpportunity.opportunityID>>>>
			DiscountDetails;

		[PXViewName(Messages.CreateSalesOrder)]
		public PXFilter<CreateSalesOrderFilter>
			CreateOrderParams;

		#region Create Account
		public PXFilter<ConvertedFilter> ConvertedInfo;

		[PXViewName(Messages.CreateAccount)]
		public PXFilter<LeadMaint.AccountsFilter> AccountInfo;

		[PXViewName(Messages.Attributes)]
		public PXSelect<CSAnswers,
			Where<CSAnswers.entityType, Equal<CSAnswerType.accountAnswerType>,
				And<CSAnswers.entityID, Equal<Current<LeadMaint.AccountsFilter.bAccountID>>>>> 
			AccountAnswers;

		#endregion

		#endregion

		protected bool isAutoNumberOn;

		#region Ctors

		public OpportunityMaint()
		{
			var crsetup = Setup.Current;

			if (string.IsNullOrEmpty(Setup.Current.OpportunityNumberingID))
			{
				throw new PXSetPropertyException(Messages.NumberingIDIsNull, Messages.CRSetup);
			}

			Activities.GetNewEmailAddress =
				() =>
				{
					var current = Opportunity.Current;
					if (current != null)
					{
						var contact = current.ContactID.
							With(_ => (Contact)PXSelect<Contact,
								Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
							Select(this, _.Value));
						if (contact != null && !string.IsNullOrWhiteSpace(contact.EMail))
							return new Email(contact.DisplayName, contact.EMail);

						var customerContact = current.BAccountID.
							With(_ => (PXResult<Contact, BAccount>)PXSelectJoin<Contact,
								InnerJoin<BAccount, On<BAccount.defContactID, Equal<Contact.contactID>>>,
								Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
							Select(this, _.Value)).
							With(_ => (Contact)_);
						if (customerContact != null && !string.IsNullOrWhiteSpace(customerContact.EMail))
							return new Email(customerContact.DisplayName, customerContact.EMail);
					}
					return Email.Empty;
				};

			foreach (Segment segment in PXSelect<Segment,
				Where<Segment.dimensionID, Equal<Required<Segment.dimensionID>>>>.Select(this, "BIZACCT"))
			{
				if (segment.AutoNumber == true)
				{
					isAutoNumberOn = true;
					break;
				}
			}
			if (isAutoNumberOn)
			{
				FieldDefaulting.AddHandler<LeadMaint.AccountsFilter.bAccountID>(delegate(PXCache sender, PXFieldDefaultingEventArgs e)
				{
					object cd;
					Caches[typeof(BAccount)].RaiseFieldDefaulting<BAccount.acctCD>(null, out cd);
					e.NewValue = (string)cd;
					e.Cancel = true;
				});
			}
			// Kesha insert it but.....  its bad for Is Dirty ask him
			//this.Views.Caches.Remove(typeof (Contact));
			var bAccountCache = Caches[typeof(BAccount)];
			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(bAccountCache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(bAccountCache, Messages.BAccountName);
		}

		#endregion

		#region Actions

		public PXSave<CROpportunity> Save;
		public PXCancel<CROpportunity> Cancel;
		public PXInsert<CROpportunity> Insert;
		public PXCopyPasteAction<CROpportunity> CopyPaste;
		public PXDelete<CROpportunity> Delete;
		public PXFirst<CROpportunity> First;
		public PXPrevious<CROpportunity> Previous;
		public PXNext<CROpportunity> Next;
		public PXLast<CROpportunity> Last;

		public PXMenuAction<CROpportunity> Action;
		public PXMenuInquiry<CROpportunity> Inquiry;


		public PXAction<CROpportunity> createInvoice;
		[PXUIField(DisplayName = Messages.CreateInvoice, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable CreateInvoice(PXAdapter adapter)
		{
			CROpportunity opportunity = this.Opportunity.Current;
			Customer customer = (Customer)PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<CROpportunity.bAccountID>>>>.Select(this);

			if (customer == null)
			{
				throw new PXException(Messages.ProspectNotCustomer);
			}

			var stokItems = PXSelectJoinGroupBy<InventoryItem,
				InnerJoin<CROpportunityProducts, On<CROpportunityProducts.inventoryID, Equal<InventoryItem.inventoryID>>>,
				Where<InventoryItem.stkItem, Equal<True>,
					And<CROpportunityProducts.cROpportunityID, Equal<Required<CROpportunityProducts.cROpportunityID>>>>,
				Aggregate<Count<InventoryItem.inventoryID>>>.
				Select(this, opportunity.OpportunityID);
			if (stokItems.RowCount > 0)
				throw new PXException(Messages.NeedOrderInsteadOfInvoice);

			if (opportunity.ARRefNbr != null)
			{
				ARInvoice doc = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<ARDocType.invoice>, And<ARInvoice.refNbr, Equal<Current<CROpportunity.aRRefNbr>>>>>.Select(this);

				if (doc == null)
				{
					WebDialogResult result = Opportunity.View.Ask(opportunity, Messages.AskConfirmation, Messages.InvoiceAlreadyCreatedDeleted, MessageButtons.YesNo, MessageIcon.Question);
					if (result == WebDialogResult.Yes)
					{
						opportunity.ARRefNbr = null;
					}
				}
				else
				{
					throw new PXException(Messages.InvoiceAlreadyCreated);
				}
			}

			if (customer != null && opportunity.ARRefNbr == null)
			{
				ARInvoiceEntry docgraph = PXGraph.CreateInstance<ARInvoiceEntry>();

				docgraph.customer.Current = customer;

				CurrencyInfo info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<CROpportunity.curyInfoID>>>>.Select(this);
				info.CuryInfoID = null;
				info = docgraph.currencyinfo.Insert(info);

				ARInvoice invoice = new ARInvoice();
				invoice.DocType = ARDocType.Invoice;
				invoice.CuryID = info.CuryID;
				invoice.CuryInfoID = info.CuryInfoID;
				invoice.DocDate = opportunity.CloseDate;
				invoice.Hold = true;
				invoice.BranchID = opportunity.BranchID;
				invoice = PXCache<ARInvoice>.CreateCopy(docgraph.Document.Insert(invoice));

				invoice.TermsID = customer.TermsID;
				invoice.InvoiceNbr = opportunity.OpportunityID;
				invoice.DocDesc = opportunity.OpportunityName;
				invoice.CustomerID = opportunity.BAccountID;
				invoice.CustomerLocationID = customer.DefLocationID;
				invoice.TaxZoneID = opportunity.TaxZoneID;
				invoice.ProjectID = opportunity.ProjectID;
				invoice = docgraph.Document.Update(invoice);

				TaxAttribute.SetTaxCalc<ARTran.taxCategoryID, ARTaxAttribute>(docgraph.Transactions.Cache, null, TaxCalc.ManualCalc);

				foreach (PXResult<CROpportunityProducts, InventoryItem> res in PXSelectJoin<CROpportunityProducts, LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<CROpportunityProducts.inventoryID>>>, Where<CROpportunityProducts.cROpportunityID, Equal<Current<CROpportunity.opportunityID>>>>.Select(this))
				{
					CROpportunityProducts product = (CROpportunityProducts)res;
					InventoryItem item = (InventoryItem)res;

					ARTran tran = new ARTran();
					tran.InventoryID = product.InventoryID;
					tran.TranDesc = product.TransactionDescription;
					tran.Qty = product.Quantity;
					tran.UOM = product.UOM;
					tran.CuryUnitPrice = product.CuryUnitPrice;
					tran.CuryTranAmt = product.CuryAmount;
					tran.TaxCategoryID = product.TaxCategoryID;
					tran.ProjectID = product.ProjectID;
					tran.TaskID = product.TaskID;
					tran.Commissionable = item.Commisionable;

					if (product.ManualDisc == true)
					{
						tran.ManualDisc = true;
						tran.DiscAmt = product.DiscAmt;
						tran.DiscPct = product.DiscPct;
					}

					tran = docgraph.Transactions.Insert(tran);

					if (Setup.Current.CopyNotes == true)
					{
						PXNoteAttribute.SetNote(docgraph.Transactions.Cache, tran, PXNoteAttribute.GetNote(Products.Cache, product));
					}

					if (Setup.Current.CopyFiles == true)
					{
						PXNoteAttribute.SetFileNotes(docgraph.Transactions.Cache, tran, PXNoteAttribute.GetFileNotes(Products.Cache, product));
					}
				}

				if (opportunity.CuryDiscTot > 0)
				{
					//Add Document Discount Tran:
					ARTran tran = new ARTran();
					tran.LineType = SOLineType.Discount;
					tran.TranDesc = Messages.OpportunityDocDisc;
					tran.CuryTranAmt = opportunity.CuryDiscTot;
					tran.DrCr = (invoice.DrCr == "D") ? "C" : "D";
					tran.BatchNbr = "";
					tran.Commissionable = false;
					tran = docgraph.Transactions.Insert(tran);

					Location loc = PXSelect<Location, Where<Location.locationID, Equal<Current<CROpportunity.locationID>>>>.Select(this);
					if (loc != null && loc.CDiscountAcctID != null)
					{
						tran.AccountID = loc.CDiscountAcctID;
						tran.SubID = loc.CDiscountSubID;
					}
					else
					{
						CROpportunityClass oc = PXSelect<CROpportunityClass, Where<CROpportunityClass.cROpportunityClassID, Equal<Current<CROpportunity.cROpportunityClassID>>>>.Select(this);
						if (oc != null && oc.DiscountAcctID != null)
						{
							tran.AccountID = oc.DiscountAcctID;
							tran.SubID = oc.DiscountSubID;
						}
					}

					docgraph.Transactions.Cache.SetDefaultExt<ARTran.taskID>(tran);
					if (tran.TaskID == null && !PM.ProjectDefaultAttribute.IsNonProject(this, tran.ProjectID))
					{
						Account ac = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, tran.AccountID);
						throw new PXException(SO.Messages.TaskWasNotAssigned, ac.AccountCD);
					}
				}

				foreach (CRTaxTran tax in PXSelect<CRTaxTran, Where<CRTaxTran.opportunityID, Equal<Current<CROpportunity.opportunityID>>>>.Select(this))
				{
					ARTaxTran new_artax = new ARTaxTran();
					new_artax.TaxID = tax.TaxID;

					new_artax = docgraph.Taxes.Insert(new_artax);

					if (new_artax != null)
					{
						new_artax = PXCache<ARTaxTran>.CreateCopy(new_artax);
						new_artax.TaxRate = tax.TaxRate;
						new_artax.CuryTaxableAmt = tax.CuryTaxableAmt;
						new_artax.CuryTaxAmt = tax.CuryTaxAmt;
						new_artax = docgraph.Taxes.Update(new_artax);
					}
				}

				invoice.CuryOrigDocAmt = invoice.CuryDocBal;
				invoice.Hold = true;
				docgraph.Document.Update(invoice);

				using (PXConnectionScope cs = new PXConnectionScope())
				{
					using (PXTransactionScope ts = new PXTransactionScope())
					{
						docgraph.Save.Press();

						opportunity.ARRefNbr = docgraph.Document.Current.RefNbr;
						if (this.Opportunity.Cache.GetStatus(opportunity) == PXEntryStatus.Notchanged)
						{
							this.Opportunity.Cache.SetStatus(opportunity, PXEntryStatus.Updated);
						}
						this.Save.Press();
						ts.Complete();
					}
				}
				throw new PXRedirectRequiredException(docgraph, "");
			}
			return adapter.Get();
		}


		public PXAction<CROpportunity> createAccount;
		[PXUIField(DisplayName = Messages.CreateAccount, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable CreateAccount(PXAdapter adapter)
		{

			List<CROpportunity> opportunities = new List<CROpportunity>(adapter.Get().Cast<CROpportunity>());
			foreach (CROpportunity opp in opportunities)
			{
				if (AccountInfo.AskExt() != WebDialogResult.OK) return opportunities;
				bool empty_required = !AccountInfo.VerifyRequired();
				BAccount existing = PXSelect<BAccount, Where<BAccount.acctCD, Equal<Required<BAccount.acctCD>>>>.SelectSingleBound(this, null, AccountInfo.Current.BAccountID);
				if (existing != null)
				{
					AccountInfo.Cache.RaiseExceptionHandling<LeadMaint.AccountsFilter.bAccountID>(AccountInfo.Current, AccountInfo.Current.BAccountID, new PXSetPropertyException(Messages.BAccountAlreadyExists, AccountInfo.Current.BAccountID));
					return opportunities;
				}
				if (empty_required) return opportunities;

				Save.Press();
				PXLongOperation.StartOperation(this, () => ConvertToAccount(opp, AccountInfo.Current));
			}
			return opportunities;
		}

		public static void ConvertToAccount(CROpportunity opportunity, LeadMaint.AccountsFilter param)
		{
			BusinessAccountMaint accountMaint = CreateInstance<BusinessAccountMaint>();
			object cd = param.BAccountID;
			accountMaint.BAccount.Cache.RaiseFieldUpdating<BAccount.acctCD>(null, ref cd);
			BAccount account = new BAccount
			{
				AcctCD = (string) cd,
				AcctName = param.AccountName,
				Type = BAccountType.ProspectType,
				ParentBAccountID = opportunity.ParentBAccountID,
				ClassID = param.AccountClass,
				WorkgroupID = opportunity.WorkgroupID,
				OwnerID = opportunity.OwnerID
			};

			try
			{
				object newValue = account.OwnerID;
				accountMaint.BAccount.Cache.RaiseFieldVerifying<BAccount.ownerID>(account, ref newValue);
			}
			catch (PXSetPropertyException)
			{
				account.OwnerID = null;
			}

			account = accountMaint.BAccount.Insert(account);
			accountMaint.Answers.CopyAllAttributes(account, opportunity);

			#region Set Contact and Address fields
			Contact contact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<CROpportunity.contactID>>>>.Select(accountMaint, opportunity.ContactID);
			Contact defContact = PXCache<Contact>.CreateCopy(PXSelect<Contact, Where<Contact.contactID, Equal<Current<BAccount.defContactID>>>>.SelectSingleBound(accountMaint, new object[] { account }));
			if (contact != null)
			{
				PXCache<Contact>.RestoreCopy(defContact, contact);
			}
			defContact.ContactType = ContactTypesAttribute.BAccountProperty;
			defContact.Title = null;
			defContact.FirstName = null;
			defContact.LastName = null;
			defContact.MidName = null;
			defContact.FullName = account.AcctName;
			defContact.ContactID = account.DefContactID;
			defContact.DefAddressID = account.DefAddressID;
			defContact.BAccountID = account.BAccountID;
			defContact = accountMaint.DefContact.Update(defContact);

			
			Address defAddress = PXCache<Address>.CreateCopy(PXSelect<Address, Where<Address.addressID, Equal<Current<BAccount.defAddressID>>>>.SelectSingleBound(accountMaint, new object[] { account }));
			if (contact != null)
			{
				Address contactAddress = PXSelect<Address, Where<Address.addressID, Equal<Current<Contact.defAddressID>>>>.SelectSingleBound(accountMaint, new object[] { contact });
				PXCache<Address>.RestoreCopy(defAddress, contactAddress);
			}
			
			defAddress.AddressID = account.DefAddressID;
			defAddress.BAccountID = account.BAccountID;
			defAddress = accountMaint.AddressCurrent.Update(defAddress);

			opportunity.BAccountID = account.BAccountID;
			if (contact != null)
			{
				contact.BAccountID = account.BAccountID;
				accountMaint.Contacts.Update(contact);
			}
			accountMaint.OpportunityLink.Update(opportunity);
			#endregion

			throw new PXRedirectRequiredException(accountMaint, "Business Account");
		}

		public PXAction<CROpportunity> viewInvoice;
		[PXUIField(DisplayName = Messages.ViewInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Inquiry)]
		public virtual IEnumerable ViewInvoice(PXAdapter adapter)
		{
			if (Opportunity.Current != null && !string.IsNullOrEmpty(Opportunity.Current.ARRefNbr))
			{
				ARInvoiceEntry target = PXGraph.CreateInstance<ARInvoiceEntry>();
				target.Clear();
				target.Document.Current = target.Document.Search<ARInvoice.refNbr>(Opportunity.Current.ARRefNbr);
				throw new PXRedirectRequiredException(target, "ViewInvoice");
			}

			return adapter.Get();
		}

		public PXAction<CROpportunity> createSalesOrder;
		[PXUIField(DisplayName = Messages.CreateSalesOrder, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable CreateSalesOrder(PXAdapter adapter)
		{
			foreach (CROpportunity opportunity in adapter.Get())
			{
				Customer customer = (Customer)PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<CROpportunity.bAccountID>>>>.Select(this);
				if (customer == null)
				{
					throw new PXException(Messages.ProspectNotCustomer);
				}

				if (!string.IsNullOrEmpty(opportunity.OrderNbr))
				{
					SOOrder doc = PXSelect<SOOrder,
						Where<SOOrder.orderType, Equal<Current<CROpportunity.orderType>>,
							And<SOOrder.orderNbr, Equal<Current<CROpportunity.orderNbr>>>>>.
						Select(this);

					if (doc == null)
					{
						WebDialogResult result = Opportunity.View.Ask(opportunity, Messages.AskConfirmation, Messages.OrderAlreadyCreatedDeleted, MessageButtons.YesNo, MessageIcon.Question);
						if (result == WebDialogResult.Yes)
						{
							opportunity.OrderNbr = null;
						}
					}
					else
					{
						WebDialogResult result = Opportunity.View.Ask(opportunity, Messages.AskConfirmation, Messages.OrderView, MessageButtons.YesNo, MessageIcon.Question);
						if (result == WebDialogResult.Yes)
						{
							SOOrderEntry docgraph = PXGraph.CreateInstance<SOOrderEntry>();
							docgraph.Document.Current = doc;
							throw new PXRedirectRequiredException(docgraph, "");
						}
						//throw new PXException(Messages.OrderAlreadyCreated);
					}
				}

				if (customer != null && opportunity.OrderNbr == null &&
					CreateOrderParams.AskExt() == WebDialogResult.OK)
				{
					var recalcDiscounts = CreateOrderParams.Current.RecalcDiscounts == true;

					SOOrderEntry docgraph = PXGraph.CreateInstance<SOOrderEntry>();

					CurrencyInfo info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<CROpportunity.curyInfoID>>>>.Select(this);
					info.CuryInfoID = null;
					info = docgraph.currencyinfo.Insert(info);

					SOOrder doc = new SOOrder();
					doc.OrderType = CreateOrderParams.Current.OrderType ?? SOOrderTypeConstants.SalesOrder;
					doc = docgraph.Document.Insert(doc);
					doc = PXCache<SOOrder>.CreateCopy(docgraph.Document.Search<SOOrder.orderNbr>(doc.OrderNbr));

					doc.CuryInfoID = info.CuryInfoID;
					doc = PXCache<SOOrder>.CreateCopy(docgraph.Document.Update(doc));

					doc.CuryID = info.CuryID;
					doc.OrderDate = Accessinfo.BusinessDate;
					doc.OrderDesc = opportunity.OpportunityName;
					doc.TermsID = customer.TermsID;
					doc.CustomerID = opportunity.BAccountID;
					doc.CustomerLocationID = customer.DefLocationID;
					doc.TaxZoneID = opportunity.TaxZoneID;
					doc.ProjectID = opportunity.ProjectID;
					doc.BranchID = opportunity.BranchID;
					doc = docgraph.Document.Update(doc);

					bool failed = false;
					SOTaxAttribute.SetTaxCalc<SOLine.taxCategoryID>(docgraph.Transactions.Cache, null, TaxCalc.ManualCalc);
					foreach (CROpportunityProducts product in SelectProducts(opportunity.OpportunityID))
					{
						if (product.SiteID == null)
						{
							Products.Cache.RaiseExceptionHandling<CROpportunityProducts.siteID>(product, null,
								new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(CROpportunityProducts.siteID).Name));
							failed = true;
						}

						if (product.IsFree == true && product.ManualDisc == false && recalcDiscounts)
						{
							continue;
						}

						SOLine tran = new SOLine();
						tran.InventoryID = product.InventoryID;
						tran.SubItemID = product.SubItemID;
						tran.TranDesc = product.TransactionDescription;
						tran.Qty = product.Quantity;
						tran.UOM = product.UOM;
						tran.CuryUnitPrice = product.CuryUnitPrice;
						tran.CuryLineAmt = product.CuryAmount;
						tran.TaxCategoryID = product.TaxCategoryID;
						tran.SiteID = product.SiteID;
						tran.IsFree = product.IsFree;
						tran.ProjectID = product.ProjectID;
						tran.TaskID = product.TaskID;

						if (recalcDiscounts)
						{
							//tran.CuryUnitPrice = null;
							//tran.CuryExtPrice = null;
							tran.ManualDisc = false;
							tran.ManualPrice = false;
						}
						else
						{
							tran.ManualDisc = true;
							tran.ManualPrice = true;
						}

						if (tran.ManualDisc == true)
						{
							tran.DiscAmt = product.DiscAmt;
							tran.DiscPct = product.DiscPct;
						}

						tran = docgraph.Transactions.Insert(tran);

						if (Setup.Current.CopyNotes == true)
						{
							PXNoteAttribute.SetNote(docgraph.Transactions.Cache, tran, PXNoteAttribute.GetNote(Products.Cache, product));
						}

						if (Setup.Current.CopyFiles == true)
						{
							PXNoteAttribute.SetFileNotes(docgraph.Transactions.Cache, tran, PXNoteAttribute.GetFileNotes(Products.Cache, product));
						}

					}
					if (failed)
						throw new PXException(Messages.SiteNotDefined);

					foreach (CRTaxTran tax in PXSelect<CRTaxTran, Where<CRTaxTran.opportunityID, Equal<Current<CROpportunity.opportunityID>>>>.Select(this))
					{
						for (short i = 0; i < 3; i++)
						{
							SOTaxTran newtax = new SOTaxTran();
							newtax.LineNbr = int.MaxValue;
							newtax.TaxID = tax.TaxID;
							newtax.BONbr = i;

							newtax = docgraph.Taxes.Insert(newtax);

							if (newtax != null)
							{
								newtax = PXCache<SOTaxTran>.CreateCopy(newtax);
								newtax.TaxRate = tax.TaxRate;
								newtax.CuryTaxableAmt = tax.CuryTaxableAmt;
								newtax.CuryTaxAmt = tax.CuryTaxAmt;
								newtax = docgraph.Taxes.Update(newtax);
							}
						}
					}

					docgraph.OpportunityBackReference.Update(opportunity);

					throw new PXRedirectRequiredException(docgraph, "");
				}
				yield return opportunity;
			}

		}

		public PXAction<CROpportunity> viewSalesOrder;
		[PXUIField(DisplayName = Messages.ViewSalesOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Inquiry)]
		public virtual IEnumerable ViewSalesOrder(PXAdapter adapter)
		{
			var row = Opportunity.Current;
			if (row != null && !string.IsNullOrEmpty(row.OrderNbr))
			{
				SOOrderEntry target = PXGraph.CreateInstance<SOOrderEntry>();
				target.Clear();
				target.Document.Current = target.Document.Search<SOOrder.orderNbr>(row.OrderNbr, row.OrderType);
				throw new PXRedirectRequiredException(target, "ViewSalesOrder");
			}

			return adapter.Get();
		}

		public PXAction<CROpportunity> updateClosingDate;
		[PXUIField(Visible = false)]
		[PXButton]
		public virtual IEnumerable UpdateClosingDate(PXAdapter adapter)
		{
			var opportunity = Opportunity.Current;
			if (opportunity != null)
			{
				opportunity.ClosingDate = Accessinfo.BusinessDate;
				Opportunity.Cache.Update(opportunity);
				Save.Press();
			}
			return adapter.Get();
		}

		#endregion

		#region Data Handlers

		protected virtual IEnumerable accountAnswers()
		{
			foreach (CSAnswers row in AccountAnswers.Cache.Cached)
				if (row.EntityType == CSAnswerType.Account && row.EntityID == int.MinValue)
					yield return row;
		}

		#endregion

		#region Event Handlers

		#region Contacts

		[CustomerProspectVendor(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual void Contact_BAccountID_CacheAttached(PXCache sender)
		{

		}

		#endregion

		#region CROpportunity

		[PXDBString(2, IsFixed = true)]
		[PXStringList(new string[0], new string[0])]
		[PXUIField(DisplayName = "Reason")]
		[CRDropDownAutoValue(typeof(CROpportunity.status))]
		public virtual void CROpportunity_Resolution_CacheAttached(PXCache sender)
		{

		}

		protected virtual void CROpportunity_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = e.Row as CROpportunity;
			if (row == null) return;

			var customerLocation = (Location)PXSelect<Location,
				Where<Location.bAccountID, Equal<Required<CROpportunity.bAccountID>>,
					And<Location.locationID, Equal<Required<CROpportunity.locationID>>>>>.
				Select(this, row.BAccountID, row.LocationID);
			if (customerLocation == null) return;

			if (!string.IsNullOrEmpty(customerLocation.CTaxZoneID))
			{
				e.NewValue = customerLocation.CTaxZoneID;
			}
			else
			{
				var address = (Address)PXSelect<Address, 
					Where<Address.addressID, Equal<Required<Address.addressID>>>>.
					Select(this, customerLocation.DefAddressID);
				if (address != null && !string.IsNullOrEmpty(address.PostalCode))
				{
					e.NewValue = TaxBuilderEngine.GetTaxZoneByZip(this, address.PostalCode);
				}
			}
		}

		protected virtual void CROpportunity_CROpportunityClassID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Setup.Current.DefaultOpportunityClassID;
		}

		protected virtual void CROpportunity_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = e.Row as CROpportunity;
			if (row == null || row.BAccountID == null) return;

			var baccount = (BAccount)PXSelect<BAccount, 
				Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(this, row.BAccountID);

			if (baccount != null)
			{
				e.NewValue = baccount.DefLocationID;
				e.Cancel = true;
			}
		}

		protected virtual void CROpportunity_ContactID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = e.Row as CROpportunity;
			if (row == null || row.BAccountID == null) return;

			var contactsSet = PXSelectJoin<Contact,
				InnerJoin<BAccount,
					On<BAccount.bAccountID, Equal<Contact.bAccountID>,
						And<BAccount.defContactID, NotEqual<Contact.contactID>>>>,
				Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				SelectWindowed(this, 0, 2, row.BAccountID);

			if (contactsSet != null && contactsSet.Count == 1)
			{
				e.NewValue = ((Contact)contactsSet[0]).ContactID;
				e.Cancel = true;
			}
		}

		protected virtual void CROpportunity_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled<CROpportunity.opportunityID>(cache, e.Row, true);

			CROpportunity opportunity = e.Row as CROpportunity;

			if (opportunity == null) return;

			PXUIFieldAttribute.SetVisible<CROpportunity.curyID>(cache, opportunity, IsMultyCurrency);

			CROpportunityClass source = PXSelectorAttribute.Select<CROpportunity.cROpportunityClassID>(cache, e.Row) as CROpportunityClass;
			if (source != null)
			{
				Activities.DefaultEMailAccountId = source.DefaultEMailAccountID;
			}

			// The code	inside this	if Allows CuryID if	BAccount of	current	Opportunity	is Customer	and	AllowOverrideCury is enables/disables CuryID
			if (opportunity.BAccountID != null && IsMultyCurrency)
			{
				bool allowCuryID = true;
				BAccount bAccount = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.Select(this, opportunity.BAccountID);
				if (bAccount != null)
				{
					if (bAccount.Type == BAccountType.CustomerType)
					{
						Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, opportunity.BAccountID);
						if (customer != null)
						{
							allowCuryID = (customer.AllowOverrideCury ?? false);
						}
					}
				}
				PXUIFieldAttribute.SetEnabled<CROpportunity.curyID>(cache, opportunity, allowCuryID);
			}

			if (opportunity.BAccountID != null)
			{
				if (SelectProducts(opportunity.OpportunityID).GetEnumerator().MoveNext()) //Count > 0
				{
					PXUIFieldAttribute.SetEnabled<CROpportunity.bAccountID>(cache, opportunity, false);
				}
			}

			viewInvoice.SetEnabled(SelectInvoice(opportunity.ARRefNbr) != null);
			viewSalesOrder.SetEnabled(SelectOrder(opportunity.OrderNbr) != null);

			PXUIFieldAttribute.SetEnabled<CROpportunity.curyAmount>(cache, e.Row, opportunity.ManualTotalEntry == true);
			createAccount.SetEnabled(opportunity.BAccountID == null);

			decimal? curyWgtAmount = null;
			var oppProbability = opportunity.StageID.
				With(_ => (CROpportunityProbability)PXSelect<CROpportunityProbability,
					Where<CROpportunityProbability.stageCode, Equal<Required<CROpportunityProbability.stageCode>>>>.
				Select(this, _));
			if (oppProbability != null && oppProbability.Probability != null)
				curyWgtAmount = oppProbability.Probability * opportunity.CuryProductsAmount / 100;
			opportunity.CuryWgtAmount = curyWgtAmount;
		}

		private object SelectInvoice(string arRefNbr)
		{
			if (string.IsNullOrEmpty(arRefNbr)) return null;
			return (ARInvoice)PXSelectJoin<ARInvoice,
				LeftJoin<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>>,
				Where2<Where<ARInvoice.origModule, Equal<BatchModule.moduleAR>, Or<ARInvoice.released, Equal<True>>>,
					And2<Where<Customer.bAccountID, IsNull, Or<Match<Customer, Current<AccessInfo.userName>>>>,
					And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>>.
				Select(this, arRefNbr);
		}

		private object SelectOrder(string orderNbr)
		{
			if (string.IsNullOrEmpty(orderNbr)) return null;
			return (SOOrder)PXSelectJoin<SOOrder,
				LeftJoin<Customer, On<Customer.bAccountID, Equal<SOOrder.customerID>>>,
				Where2<Where<Customer.bAccountID, IsNull, Or<Match<Customer, Current<AccessInfo.userName>>>>,
					And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>.
				Select(this, orderNbr);
		}

		protected virtual void CROpportunity_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			var row = e.Row as CROpportunity;

			object newContactId = row.ContactID;
			if (newContactId != null && !VerifyField<CROpportunity.contactID>(row, newContactId))
				row.ContactID = null;

			if (row.ContactID != null)
			{
				object newCustomerId = row.BAccountID;
				if (newCustomerId == null)
					row.BAccountID = GetDefaultBAccountID(row);
			}

			if (row.BAccountID != null)
			{
				var bAccount = (BAccount)PXSelect<BAccount, 
					Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(this, row.BAccountID);
				if (bAccount != null && IsMultyCurrency)
				{
					if (bAccount.Type == BAccountType.CustomerType || bAccount.Type == BAccountType.CombinedType)
					{
						Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, row.BAccountID);
						if (customer != null && customer.CuryID != null)
						{
							row.CuryID = customer.CuryID;
						}
					}
				}
			}

			object newLocationId = row.LocationID;
			if (newLocationId == null || !VerifyField<CROpportunity.locationID>(row, newLocationId))
			{
				cache.SetDefaultExt<CROpportunity.locationID>(row);
			}

			if (row.ContactID == null)
				cache.SetDefaultExt<CROpportunity.contactID>(row);

			if (row.TaxZoneID == null) 
				cache.SetDefaultExt<CROpportunity.taxZoneID>(row);
		}

		protected virtual void CROpportunity_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var oldRow = e.Row as CROpportunity;
			var row = e.NewRow as CROpportunity;
			if (oldRow == null || row == null) return;

			if (oldRow.Status != row.Status && oldRow.Resolution == row.Resolution)
				row.Resolution = null;
		}

		protected virtual void CROpportunity_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CROpportunity row = e.Row as CROpportunity;
			CROpportunity oldRow = e.OldRow as CROpportunity;
			if (row == null || oldRow == null) return;

			object newContactId = row.ContactID;
			if (newContactId != null && !VerifyField<CROpportunity.contactID>(row, newContactId))
				row.ContactID = null;

			if (row.ContactID != null && row.ContactID != oldRow.ContactID)
			{
				object newCustomerId = row.BAccountID;
				if (newCustomerId == null)
					row.BAccountID = GetDefaultBAccountID(row);
			}

			var customerChanged = row.BAccountID != oldRow.BAccountID;
			if (customerChanged && row.BAccountID != null)
			{
				var bAccount = (BAccount)PXSelect<BAccount,
					Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(this, row.BAccountID);
				if (bAccount != null && IsMultyCurrency)
				{
					if (bAccount.Type == BAccountType.CustomerType || bAccount.Type == BAccountType.CombinedType)
					{
						Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, row.BAccountID);
						if (customer != null && customer.CuryID != null)
						{
							row.CuryID = customer.CuryID;
						}
					}
				}
			}

			object newLocationId = row.LocationID;
			if (newLocationId == null || !VerifyField<CROpportunity.locationID>(row, newLocationId))
			{
				sender.SetDefaultExt<CROpportunity.locationID>(row);
			}

			if (row.ContactID == null)
				sender.SetDefaultExt<CROpportunity.contactID>(row);

			if (customerChanged)
				sender.SetDefaultExt<CROpportunity.taxZoneID>(row);

			var locationChanged = row.LocationID != oldRow.LocationID;
			var closeDateChanged = row.CloseDate != oldRow.CloseDate;
			var projectChanged = row.ProjectID != oldRow.ProjectID;
			if (locationChanged || closeDateChanged || projectChanged || customerChanged)
			{
				var productsCache = Products.Cache;
				foreach (CROpportunityProducts line in SelectProducts(row.OpportunityID))
				{
					var lineCopy = (CROpportunityProducts)productsCache.CreateCopy(line);
					if (locationChanged || closeDateChanged) lineCopy.ManualDisc = false;
					if (locationChanged) lineCopy.OpportunityLocationIDChanged = true;
					if (closeDateChanged) lineCopy.OpportunityCloseDateChanged = true;
					lineCopy.ProjectID = row.ProjectID;
					lineCopy.CustomerID = row.BAccountID;
					productsCache.Update(lineCopy);
				}
			}

			if (row.OwnerID == null)
			{
				row.AssignDate = null;
			}
			else if (oldRow.OwnerID == null)
			{
				row.AssignDate = PXTimeZoneInfo.Now;
			}
		}

		protected virtual void CROpportunity_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = (CROpportunity)e.Row;
			if (row == null) return;

			if (row.BAccountID != null && row.LocationID == null)
			{
				sender.RaiseExceptionHandling<CROpportunity.locationID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
				e.Cancel = true;
			}
		}

		protected virtual void CROpportunity_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			CROpportunity row = e.Row as CROpportunity;
			if (row == null) return;

			if (row.ContactID != null)
			{
				Contact contact = PXSelect<Contact, Where<Contact.contactID, Equal<Required<CROpportunity.contactID>>>>.Select(this, row.ContactID);
				if (contact != null && contact.ContactType == ContactTypesAttribute.Lead)
				{
					contact.ContactType = ContactTypesAttribute.Person;
					if (contact.DuplicateStatus == DuplicateStatusAttribute.PossibleDuplicated)
					{
						contact.DuplicateStatus = DuplicateStatusAttribute.NotValidated;
					}
					Contacts.Update(contact);
					Save.Press();
				}
			}
		}

		#endregion

		#region CROpportunityProducts

		protected virtual void CROpportunityProducts_DiscPct_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = e.Row as CROpportunityProducts;
			if (row == null) return;

			decimal? newValue = (decimal?)e.NewValue;
			SODiscountEngine<CROpportunityProducts>.ValidateMinGrossProfitPct(sender, row, row.UnitPrice, ref newValue);

			e.NewValue = newValue;
		}

		protected virtual void CROpportunityProducts_CuryDiscAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = e.Row as CROpportunityProducts;
			if (row == null) return;

			decimal? newValue = (decimal?)e.NewValue;
			SODiscountEngine<CROpportunityProducts>.ValidateMinGrossProfitAmt(sender, row, row.UnitPrice, ref newValue);
			e.NewValue = newValue;
		}

		protected virtual void CROpportunityProducts_CuryUnitPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CROpportunityProducts row = e.Row as CROpportunityProducts;
			if (row != null)
			{
				if (row.InventoryID != null)
				{
					if (row.UOM != null && SODiscountEngine<CROpportunityProducts>.IsFlatPriceApplicable(sender, row))
					{
						e.NewValue = SODiscountEngine<CROpportunityProducts>.CalculateFlatPrice(sender, row, Opportunity.Current.LocationID, Opportunity.Current.CloseDate.Value);
					}
					else
					{
						e.NewValue = ARSalesPriceMaint.CalculateSalesPrice(sender, AR.ARPriceClass.EmptyPriceClass, row.InventoryID, currencyinfo.Select(), row.UOM, Accessinfo.BusinessDate.Value);
					}
				}
			}
		}

		protected virtual void CROpportunityProducts_IsFree_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CROpportunityProducts row = e.Row as CROpportunityProducts;
			if (row != null)
			{
				if (row.IsFree == true)
				{
					row.CuryUnitPrice = 0;
				}
				else
				{
					sender.SetDefaultExt<CROpportunityProducts.curyUnitPrice>(row);
				}
			}
		}

		protected virtual void CROpportunityProducts_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CROpportunityProducts row = e.Row as CROpportunityProducts;
			if (row == null) return;

			InventoryItem item = PXSelectorAttribute.Select<CROpportunityProducts.inventoryID>(cache, row) as InventoryItem;
			if (item != null)
			{
				SetDiscounts(cache, row);
				row.TransactionDescription = item.Descr;
				cache.SetDefaultExt<CROpportunityProducts.uOM>(e.Row);
				cache.SetDefaultExt<CROpportunityProducts.curyUnitPrice>(e.Row);
				cache.SetDefaultExt<CROpportunityProducts.siteID>(e.Row);
			}
		}

		protected virtual void CROpportunityProducts_UOM_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			cache.SetDefaultExt<CROpportunityProducts.curyUnitPrice>(e.Row);
		}

		protected virtual void CROpportunityProducts_Quantity_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CROpportunityProducts row = e.Row as CROpportunityProducts;
			SetFlatUnitPrice(sender, row);
		}


		protected virtual void CROpportunityProducts_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CROpportunityProducts;
			if (row == null) return;

			PXUIFieldAttribute.SetEnabled<CROpportunityProducts.curyUnitPrice>(sender, row, row.IsFree != true);

			bool autoFreeItem = row.ManualDisc != true && row.IsFree == true;

			if (autoFreeItem)
			{
				PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
				PXUIFieldAttribute.SetEnabled<CROpportunityProducts.taxCategoryID>(sender, e.Row);
				PXUIFieldAttribute.SetEnabled<CROpportunityProducts.transactionDescription>(sender, e.Row);
				PXUIFieldAttribute.SetEnabled<CROpportunityProducts.siteID>(sender, e.Row);
			}
			PXUIFieldAttribute.SetEnabled<CROpportunityProducts.manualDisc>(sender, e.Row, !autoFreeItem);
			PXUIFieldAttribute.SetEnabled<CROpportunityProducts.quantity>(sender, e.Row, !autoFreeItem);
			PXUIFieldAttribute.SetEnabled<CROpportunityProducts.isFree>(sender, e.Row, !autoFreeItem && row.InventoryID != null);
		}

		protected virtual void CROpportunityProducts_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var row = e.Row as CROpportunityProducts;
			if (row == null) return;

			if (row.InventoryID != null)
			{
				var item = PXSelectorAttribute.Select<CROpportunityProducts.inventoryID>(sender, row) as InventoryItem;
				if (item != null)
				{
					row.UOM = item.SalesUnit;
					sender.SetValueExt<CROpportunityProducts.curyUnitPrice>(row, item.BasePrice);
					row.TransactionDescription = item.Descr;
					sender.SetDefaultExt<CROpportunityProducts.siteID>(row);
				}
			}

			if (row.Quantity == null) sender.SetValueExt<CROpportunityProducts.quantity>(row, 0m);

			if (row.IsFree == true)
			{
				sender.SetValueExt<CROpportunityProducts.curyUnitPrice>(row, 0m);
				if (row.ManualDisc != true) ResetQtyOnFreeItem(row);
			}
			else
			{
				decimal? curyUnitPrice = null;
				CROpportunity opportunity;
				if (row.InventoryID != null && row.UOM != null && (opportunity = SelectOpportunity(row.CROpportunityID)) != null && opportunity.CloseDate != null)
				{
					if (SODiscountEngine<CROpportunityProducts>.IsFlatPriceApplicable(sender, row))
					{
						curyUnitPrice = SODiscountEngine<CROpportunityProducts>.CalculateFlatPrice(sender, row, opportunity.LocationID, (DateTime)opportunity.CloseDate);
					}
					if (curyUnitPrice == null)
					{
						curyUnitPrice = ARSalesPriceMaint.CalculateSalesPrice(sender, ARPriceClass.EmptyPriceClass,
							row.InventoryID, currencyinfo.Select(), row.UOM, (DateTime)opportunity.CloseDate) ?? 0;
					}
				}
				sender.SetValueExt<CROpportunityProducts.curyUnitPrice>(row, curyUnitPrice);
			}

			if (row.ManualDisc == true)
			{
				sender.SetValueExt<CROpportunityProducts.curyDiscAmt>(row, (row.CuryUnitPrice ?? 0m) * row.Quantity * (row.DiscPct ?? 0m) * 0.01m);
			}
			else
			{
				sender.SetValueExt<CROpportunityProducts.detDiscIDC1>(row, null);
				sender.SetValueExt<CROpportunityProducts.detDiscSeqIDC1>(row, null);
				sender.SetValueExt<CROpportunityProducts.detDiscIDC2>(row, null);
				sender.SetValueExt<CROpportunityProducts.detDiscSeqIDC2>(row, null);

				CROpportunity opportunity;
				if (row.IsFree != true && row.InventoryID != null && row.CustomerID != null && 
					(opportunity = SelectOpportunity(row.CROpportunityID)) != null && opportunity.CloseDate != null)
				{
					sender.SetValueExt<CROpportunityProducts.curyAmount>(row, (row.CuryUnitPrice ?? 0m) * row.Quantity);

					SODiscountEngine<CROpportunityProducts>.SetDiscounts(sender, row, opportunity.LocationID, (DateTime)opportunity.CloseDate);

					SODiscountEngine<CROpportunityProducts>.CalculateLineDiscounts(sender, row, opportunity.LocationID, (DateTime)opportunity.CloseDate);

					SODiscountEngine<CROpportunityProducts>.UpdateDiscountDetails<CROpportunityDiscountDetail>(sender, Products, DiscountDetails,
						row, opportunity.LocationID, (DateTime)opportunity.CloseDate, ProrateDiscount);

					SODiscountEngine<CROpportunityProducts>.RemoveUnappliableDiscountDetails<CROpportunityDiscountDetail>(Products, DiscountDetails);

					RefreshFreeItemLines(row.CROpportunityID);
				}
				else
				{
					sender.SetValueExt<CROpportunityProducts.curyDiscAmt>(row, 0m);
				}
			}

			if (row.CuryDiscAmt == null) sender.SetValueExt<CROpportunityProducts.curyDiscAmt>(row, 0m);

			sender.SetValueExt<CROpportunityProducts.curyAmount>(row, (row.CuryUnitPrice ?? 0m) * row.Quantity - row.CuryDiscAmt);

			TaxAttribute.Calculate<CROpportunityProducts.taxCategoryID>(sender, e);
		}

		protected virtual void CROpportunityProducts_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Debug.WriteLine("CROpportunityProducts_RowUpdated");

			CROpportunityProducts row = e.Row as CROpportunityProducts;
			CROpportunityProducts oldRow = e.OldRow as CROpportunityProducts;
			
			if (row.ManualDisc != true)
			{
				if (oldRow.ManualDisc == true)
				{
					SetDiscounts(sender, (CROpportunityProducts)e.Row);
					SetFlatUnitPrice(sender, row);

					if (row.IsFree == true)
					{
						ResetQtyOnFreeItem(row);
					}
				}

				RecalculateDiscounts(sender, row);
			}
			else
			{
				SODiscountEngine<CROpportunityProducts>.RemoveUnappliableDiscountDetails<CROpportunityDiscountDetail>(Products, DiscountDetails);
			}
			TaxAttribute.Calculate<CROpportunityProducts.taxCategoryID>(sender, e);
		}

		protected virtual void CROpportunityProducts_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			var row = e.Row as CROpportunityProducts;
			if (row == null) return;

			bool autoFreeItem = row.ManualDisc != true && row.IsFree == true;
			if (autoFreeItem) return;

			var opportunity = SelectOpportunity(row.CROpportunityID);
			if (opportunity != null && opportunity.CloseDate != null && row.InventoryID != null && row.Quantity != null)
			{
				sender.SetValueExt<CROpportunityProducts.detDiscIDC1>(row, null);
				sender.SetValueExt<CROpportunityProducts.detDiscSeqIDC1>(row, null);
				sender.SetValueExt<CROpportunityProducts.detDiscIDC2>(row, null);
				sender.SetValueExt<CROpportunityProducts.detDiscSeqIDC2>(row, null);

				SODiscountEngine<CROpportunityProducts>.UpdateDiscountDetails<CROpportunityDiscountDetail>(sender, Products, DiscountDetails,
					row, opportunity.LocationID, (DateTime)opportunity.CloseDate, ProrateDiscount);

				SODiscountEngine<CROpportunityProducts>.RemoveUnappliableDiscountDetails<CROpportunityDiscountDetail>(Products, DiscountDetails);

				RefreshFreeItemLines(row.CROpportunityID);
			}
		}

		#endregion

		#region CRRelation
		[PXDBLong]
		[PXDBDefault(typeof(CROpportunity.noteID))]
		public virtual void CRRelation_RefNoteID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region CurrencyInfo

		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!IsMultyCurrency) return;

			if (Setup.Current == null || string.IsNullOrEmpty(Setup.Current.DefaultCuryID)) return;

			e.NewValue = Setup.Current.DefaultCuryID;
			e.Cancel = true;
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!IsMultyCurrency) return;

			if (Setup.Current == null || string.IsNullOrEmpty(Setup.Current.DefaultRateTypeID)) return;

			e.NewValue = Setup.Current.DefaultRateTypeID;
			e.Cancel = true;
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Accessinfo.BusinessDate;
			e.Cancel = true;
		}

		#endregion

		

		protected virtual void AccountsFilter_BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.IsDimensionAutonumbered(CustomerAttribute.DimensionName))
			{
				e.NewValue = this.GetDimensionAutonumberingNewValue(CustomerAttribute.DimensionName);
			}
		}
		protected virtual void AccountsFilter_AccountName_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			Contact contact = PXSelectorAttribute.Select<CROpportunity.contactID>(this.Opportunity.Cache, this.Opportunity.Current) as Contact;
			if (contact == null) return;
			e.NewValue = contact.FullName;
			e.Cancel = true;
		}
		protected virtual void AccountsFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled<LeadMaint.AccountsFilter.bAccountID>(sender, e.Row, !this.IsDimensionAutonumbered(CustomerAttribute.DimensionName));
		}
		#endregion

		#region Private Methods

		private BAccount SelectAccount(string acctCD)
		{
			if (string.IsNullOrEmpty(acctCD)) return null;
			return (BAccount)PXSelectReadonly<BAccount,
				Where<BAccount.acctCD, Equal<Required<BAccount.acctCD>>>>.
				Select(this, acctCD);
		}

		private bool VerifyField<TField>(object row, object newValue)
			where TField : IBqlField
		{
			if (row == null) return true;

			var result = false;
			var cache = Caches[row.GetType()];
			try
			{
				result = cache.RaiseFieldVerifying<TField>(row, ref newValue);
			}
			catch (StackOverflowException) { throw; }
			catch (OutOfMemoryException) { throw; }
			catch (Exception) { }

			return result;
		}

		private int? GetDefaultBAccountID(CROpportunity row)
		{
			if (row == null) return null;

			if (row.ContactID != null)
			{
				var contact = (Contact)PXSelectReadonly<Contact,
					Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
					Select(this, row.ContactID);
				if (contact != null)
					return contact.BAccountID;
			}

			return null;
		}

		private void FillAnswers(string entityType, object entityID, object entityClassId)
		{
			var cache = Caches[typeof(CSAnswers)];
			var oldDirty = cache.IsDirty;

			var attMap = new HybridDictionary();
			if (entityClassId != null)
				foreach (CSAttribute attribute in
					PXSelectJoin<CSAttribute,
						InnerJoin<CSAttributeGroup, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
						Where<CSAttributeGroup.type, Equal<CSAnswerType.accountAnswerType>,
							And<CSAttributeGroup.entityClassID, Equal<Required<CSAttributeGroup.entityClassID>>>>>.
						Select(this, entityClassId))
				{
					attMap.Add(attribute.AttributeID, false);
				}

			foreach (CSAnswers answer in cache.Cached)
			{
				if (answer.EntityType != CSAnswerType.Account) continue;

				if (!attMap.Contains(answer.AttributeID))
					cache.Remove(answer);
				else
				{
					if (!string.IsNullOrEmpty(answer.Value))
						attMap[answer.AttributeID] = true;
				}
			}

			if (entityID != null && entityType != null)
				foreach (CSAnswers answer in
					PXSelect<CSAnswers,
						Where<CSAnswers.entityID, Equal<Required<CSAnswers.entityID>>,
							And<CSAnswers.entityType, Equal<Required<CSAnswers.entityType>>>>>.
						Select(this, entityID, entityType))
				{
					if (!attMap.Contains(answer.AttributeID) || (bool)attMap[answer.AttributeID]) continue;

					if (answer.Value != null)
					{
						var newAnswer = (CSAnswers)cache.CreateInstance();
						newAnswer.EntityID = int.MinValue;
						newAnswer.EntityType = CSAnswerType.Account;
						newAnswer.AttributeID = answer.AttributeID;
						newAnswer.Value = answer.Value;
						cache.Update(newAnswer);
						attMap[answer.AttributeID] = true;
					}
				}

			foreach (DictionaryEntry entry in attMap)
			{
				if (!(bool)entry.Value)
				{
					var newAnswer = (CSAnswers)cache.CreateInstance();
					newAnswer.EntityID = int.MinValue;
					newAnswer.EntityType = CSAnswerType.Account;
					newAnswer.AttributeID = (string)entry.Key;
					cache.Insert(newAnswer);
				}
			}

			cache.IsDirty = oldDirty;
		}

		private bool IsMultyCurrency
		{
			get
			{
				CMSetup cmsetup = PXSelect<CMSetup>.Select(this);

				if (cmsetup == null)
					return false;

				return cmsetup.MCActivated == true;
			}
		}

		private bool ProrateDiscount
		{
			get
			{
				SOSetup sosetup = PXSelect<SOSetup>.Select(this);

				if (sosetup == null)
					return true; //default true

				if (sosetup.ProrateDiscounts == null)
					return true;

				return sosetup.ProrateDiscounts == true;
			}
		}

		private CROpportunity SelectOpportunity(string opportunityId)
		{
			if (opportunityId == null) return null;

			var opportunity = (CROpportunity)PXSelect<CROpportunity,
				Where<CROpportunity.opportunityID, Equal<Required<CROpportunity.opportunityID>>>>.
				Select(this, opportunityId);
			return opportunity;
		}

		private IEnumerable SelectProducts(object opportunityId)
		{
			if (opportunityId == null)
				return new CROpportunityProducts[0];

			return PXSelect<CROpportunityProducts,
				Where<CROpportunityProducts.cROpportunityID, Equal<Required<CROpportunity.opportunityID>>>>.
				Select(this, opportunityId).
				RowCast<CROpportunityProducts>();
		}

		private IEnumerable SelectDiscountDetails(object opportunityId)
		{
			if (opportunityId == null)
				return new CROpportunityDiscountDetail[0];

			return PXSelect<CROpportunityDiscountDetail,
				Where<CROpportunityDiscountDetail.opportunityID, Equal<Required<CROpportunity.opportunityID>>>>.
				Select(this, opportunityId).
				RowCast<CROpportunityDiscountDetail>();
		}

		private void RefreshFreeItemLines(object opportunityId)
		{
			var groupedByInventory = new Dictionary<int, decimal>();

			foreach (CROpportunityDiscountDetail item in SelectDiscountDetails(opportunityId))
			{
				if (item.FreeItemID == null) continue;
				
				var itemId = (int)item.FreeItemID;
				if (groupedByInventory.ContainsKey(itemId))
				{
					groupedByInventory[itemId] += item.FreeItemQty ?? 0;
				}
				else
				{
					groupedByInventory.Add(itemId, item.FreeItemQty ?? 0);
				}
			}

			bool freeItemChanged = false;
			foreach (CROpportunityProducts line in SelectProducts(opportunityId))
			{
				if (line.IsFree != true || line.ManualDisc == true || line.InventoryID == null) continue;
					
				var inventoryId = (int)line.InventoryID;
				decimal itemsQty;
				var isFinded = false;
				if (!(isFinded = groupedByInventory.TryGetValue(inventoryId, out itemsQty)) || itemsQty == 0)
				{
					Products.Delete(line);
					freeItemChanged = true;
					if (isFinded) groupedByInventory.Remove(inventoryId);
				}
				else 
				{
					if (line.Quantity != itemsQty)
					{
						var rowCopy = (CROpportunityProducts)Products.Cache.CreateCopy(line);
						rowCopy.Quantity = itemsQty;
						Products.Update(rowCopy);
						freeItemChanged = true;
					}
					groupedByInventory.Remove(inventoryId);
				}
			}

			foreach (KeyValuePair<int, decimal> kv in groupedByInventory)
			{
				var inventoryId = kv.Key;
				var quantity = kv.Value;
				if (quantity > 0m)
				{
					var line = (CROpportunityProducts)Products.Cache.CreateInstance();
					line.InventoryID = inventoryId;
					line.IsFree = true;
					line.Quantity = quantity;

					Products.Insert(line);
					freeItemChanged = true;
				}
			}

			if (freeItemChanged)
			{
				Products.View.RequestRefresh();
			}
		}

		private void ResetQtyOnFreeItem(CROpportunityProducts line)
		{
			decimal? qtyTotal = 0;
			foreach (CROpportunityDiscountDetail item in
				PXSelect<CROpportunityDiscountDetail,
				Where<CROpportunityDiscountDetail.opportunityID, Equal<Required<CROpportunityDiscountDetail.opportunityID>>,
					And<CROpportunityDiscountDetail.freeItemID, Equal<Required<CROpportunityDiscountDetail.freeItemID>>>>>.
				Select(this, line.CROpportunityID, line.InventoryID))
			{
				if (item.FreeItemID != null && item.FreeItemQty != null && item.FreeItemQty > 0)
					qtyTotal += item.FreeItemQty;
			}

			Caches[typeof(CROpportunityDiscountDetail)].SetValueExt<CROpportunityProducts.quantity>(line, qtyTotal);
		}

		private void SetDiscounts(PXCache sender, CROpportunityProducts row)
		{
			if (row != null && row.InventoryID != null
							&& row.IsFree != true)
			{
				SODiscountEngine<CROpportunityProducts>.SetDiscounts(sender, row, Opportunity.Current.LocationID, Opportunity.Current.CloseDate.Value);
				SODiscountEngine<CROpportunityProducts>.RemoveUnappliableDiscountDetails<CROpportunityDiscountDetail>(Products, DiscountDetails);
			}
		}

		/// <summary>
		/// Finds Flat price in Discounts and sets UnitPrice.
		/// True if Flat price is applicable for the given line; otherwise false.
		/// </summary>
		private bool SetFlatUnitPrice(PXCache sender, CROpportunityProducts row)
		{
			decimal? curyFlatPrice = GetFlatUnitPrice(sender, row);
			if (curyFlatPrice != null)
			{
				sender.SetValueExt<CROpportunityProducts.curyUnitPrice>(row, curyFlatPrice);
				return true;
			}

			return false;
		}

		private decimal? GetFlatUnitPrice(PXCache sender, CROpportunityProducts row)
		{
			if (row.InventoryID != null && row.UOM != null && SODiscountEngine<CROpportunityProducts>.IsFlatPriceApplicable(sender, row))
			{
				return SODiscountEngine<CROpportunityProducts>.CalculateFlatPrice(sender, row, Opportunity.Current.LocationID, Opportunity.Current.CloseDate.Value);
			}

			return null;
		}

		private void RecalculateDiscounts(PXCache sender, CROpportunityProducts line)
		{
			if (line.InventoryID != null && line.Qty != null && line.CuryLineAmt != null && line.IsFree != true && line.InventoryID != null)
			{
				if (line.ManualDisc == false)
					SODiscountEngine<CROpportunityProducts>.CalculateLineDiscounts(sender, line, Opportunity.Current.LocationID, Opportunity.Current.CloseDate.Value);

				SODiscountEngine<CROpportunityProducts>.UpdateDiscountDetails<CROpportunityDiscountDetail>(sender, Products, DiscountDetails, line, Opportunity.Current.LocationID, Opportunity.Current.CloseDate.Value, ProrateDiscount);

				RefreshFreeItemLines(line.CROpportunityID);
			}
		}

		#endregion

		#region Avalara Tax

		protected bool IsExternalTax
		{
			get
			{
				if (Opportunity.Current == null)
					return false;

				return AvalaraMaint.IsExternalTax(this, Opportunity.Current.TaxZoneID);
			}
		}

		public bool SkipAvalaraTaxProcessing { get; set; }

		#endregion

		public override void Persist()
		{
			base.Persist();

			if (Opportunity.Current != null && IsExternalTax == true && !SkipAvalaraTaxProcessing && Opportunity.Current.IsTaxValid != true)
			{
				PXLongOperation.StartOperation(this, delegate()
				{
					CROpportunity doc = new CROpportunity();
					doc.OpportunityID = Opportunity.Current.OpportunityID;
					CRExternalTaxCalc.Process(doc);
				});
			}
		}
	}
}

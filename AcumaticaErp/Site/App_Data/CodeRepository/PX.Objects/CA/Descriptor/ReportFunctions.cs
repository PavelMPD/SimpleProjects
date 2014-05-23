using PX.Data;
using PX.Objects.CR;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.CM;
using System;
using System.Collections;
using System.Collections.Generic;


namespace PX.Objects.CA.Descriptor
{
    public class ReportFunctions
    {
        //=Payments.GetAPPaymentInfo('102000', 'FEDWRE', '1' , 'V000213')
        //                           ^-Cash Account
        //                                     ^-Payment Method
        //                                               ^- Detail ID
        //                                                     ^- Vendror ID
        public string GetAPPaymentInfo(
            string accountCD, 
            string paymentMethodID,
            string detailID,
            string acctCD)
        {
            return GetAPPaymentInfo(accountCD, paymentMethodID, detailID, acctCD, null);
        }

        public string GetAPPaymentInfo(
            string accountCD, 
            string paymentMethodID,
            string detailID,
            string acctCD,
            string locationCD)
        {
            VendorPaymentMethodDetail detail = PXSelectJoin<VendorPaymentMethodDetail,
                                                            InnerJoin<BAccount, On<BAccount.bAccountID, Equal<VendorPaymentMethodDetail.bAccountID>>,
                                                            InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<VendorPaymentMethodDetail.paymentMethodID>>,
                                                            InnerJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<VendorPaymentMethodDetail.paymentMethodID>,
                                                                And<PaymentMethodDetail.detailID, Equal<VendorPaymentMethodDetail.detailID>,
                                                                And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
                                                                    Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>,
                                                            InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>,
                                                            InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>,
															InnerJoin<Account, On<Account.accountID, Equal<CashAccount.accountID>>,
                                                            LeftJoin<Location, On<Location.locationCD, Equal<Required<Location.locationCD>>>>>>>>>>,
                                                            Where<Account.accountCD, Equal<Required<Account.accountCD>>,
                                                                And<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>,
                                                                And<VendorPaymentMethodDetail.detailID, Equal<Required<VendorPaymentMethodDetail.detailID>>,
                                                                And<BAccount.acctCD, Equal<Required<BAccount.acctCD>>,
                                                                And<Where2<Where<Location.locationID, IsNotNull>, And<VendorPaymentMethodDetail.locationID, Equal<Location.locationID>,
                                                                    Or2<Where<Location.locationID, IsNull>, And<VendorPaymentMethodDetail.locationID, Equal<BAccount.defLocationID>>>>>>>>>>>
                                               .Select(PXGraph.CreateInstance<BusinessAccountMaint>(),
                                                       locationCD,        
                                                       accountCD,
                                                       paymentMethodID,
                                                       detailID,
                                                       acctCD);

            return detail != null ? detail.DetailValue : string.Empty;
        }

        //=Payments.GetARPaymentInfo('102000', 'FEDWRE', '1', 'C000213')
        //                           ^-Cash Account
        //                                     ^-Payment Method
        //                                               ^- Detail ID
        //                                                    ^- Customer ID
        public string GetARPaymentInfo(
            string accountCD,
            string paymentMethodID,
            string detailID,
            string pMInstanceID)
        {
            int actualPMInstanceID;
            if (!int.TryParse(pMInstanceID, out actualPMInstanceID))
                throw new PXArgumentException("pMInstanceID", @"Value of parameter ""pMInstanceID"" cannot be represented as an Integer");

            CustomerPaymentMethodDetail detail = PXSelectJoin<CustomerPaymentMethodDetail,
                                                              InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethodDetail.paymentMethodID>>,
                                                              InnerJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<CustomerPaymentMethodDetail.paymentMethodID>,
                                                                  And<PaymentMethodDetail.detailID, Equal<CustomerPaymentMethodDetail.detailID>,
                                                                  And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
                                                              InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>,
                                                              InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>,
															  InnerJoin<Account, On<Account.accountID, Equal<CashAccount.accountID>>>>>>>,
                                                              Where<Account.accountCD, Equal<Required<Account.accountCD>>,
                                                                  And<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>,
                                                                  And<CustomerPaymentMethodDetail.detailID, Equal<Required<CustomerPaymentMethodDetail.detailID>>,
                                                                  And<CustomerPaymentMethodDetail.pMInstanceID, Equal<Required<CustomerPaymentMethodDetail.pMInstanceID>>>>>>>
                                                 .Select(PXGraph.CreateInstance<CustomerPaymentMethodMaint>(),
                                                         accountCD,
                                                         paymentMethodID,
                                                         detailID,
                                                         pMInstanceID);

            return detail != null ? detail.Value : string.Empty;
        }

        //=Payments.GetRemitPaymentInfo('102000', 'FEDWRE', '1')
        //                              ^-Cash Account
        //                                        ^-Payment Method
        //                                                  ^- Detail ID
        public string GetRemitPaymentInfo(
            string accountCD,
            string paymentMethodID,
            string detailID)
        {
            CashAccountPaymentMethodDetail detail = PXSelectJoin<CashAccountPaymentMethodDetail,
                                                                 InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CashAccountPaymentMethodDetail.paymentMethodID>>,
                                                                 InnerJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<CashAccountPaymentMethodDetail.paymentMethodID>,
                                                                     And<PaymentMethodDetail.detailID, Equal<CashAccountPaymentMethodDetail.detailID>,
                                                                     And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
                                                                         Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>,
                                                                 InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>,
                                                                 InnerJoin<CashAccount, On<CashAccount.accountID, Equal<CashAccountPaymentMethodDetail.accountID>,
                                                                    And<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>>>>>>,
                                                                 Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>,
                                                                     And<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>,
                                                                     And<CashAccountPaymentMethodDetail.detailID, Equal<Required<CashAccountPaymentMethodDetail.detailID>>>>>>
                                                    .Select(PXGraph.CreateInstance<CashAccountMaint>(),
                                                            accountCD,
                                                            paymentMethodID,
                                                            detailID);

            return detail != null ? detail.DetailValue : string.Empty;
        }

        private List<CurrencyRate> GetCachedCurrencyRates()
        {
            List<CurrencyRate> rates = PX.Common.PXContext.GetSlot<List<CurrencyRate>>();
            if (rates == null)
            {
                rates = PX.Common.PXContext.SetSlot<List<CurrencyRate>>(PXDatabase.GetSlot<List<CurrencyRate>>(typeof(CM.CurrencyRate).FullName, typeof(CurrencyRate)));
            }
            return rates;
        }

        private CurrencyRate FindCurrencyRate(List<CurrencyRate> rates, string fromCury, string toCury, string rateType, DateTime effectiveDate)
        {
            CM.CurrencyRate foundRate = null;
            if (rates.Count != 0)
            {
                foreach (CurrencyRate rate in rates)
                {
                    if (rate.CuryRateType == (string)rateType &&
                        rate.FromCuryID == (string)fromCury &&
                        rate.ToCuryID == (string)toCury)
                    {
                        if (rate.CuryEffDate == (DateTime)effectiveDate)
                        {
                            foundRate = rate;
                            break;
                        }
                        else if (rate.CuryEffDate < (DateTime)effectiveDate && ((foundRate != null && rate.CuryEffDate > foundRate.CuryEffDate) || foundRate == null))
                        {
                            foundRate = rate;
                        }
                    }
                }
            }
            return foundRate;
        }

        private CurrencyRate FindCurrencyRate(string fromCury, string toCury, string rateType, DateTime effectiveDate)
        {
            List<CurrencyRate> rates = GetCachedCurrencyRates();

            CM.CurrencyRate foundRate = FindCurrencyRate(rates, fromCury, toCury, rateType, (DateTime)effectiveDate);
            if (foundRate == null)
            {
                PXGraph graph = new PXGraph();
                PXResultset<CurrencyRate> currencyRates = PXSelect<CurrencyRate, Where<CurrencyRate.fromCuryID, Equal<Required<CurrencyRate.fromCuryID>>,
                And<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>, 
                And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>, 
                And<CurrencyRate.curyEffDate, GreaterEqual<Required<CurrencyRate.curyEffDate>>>>>>, 
                OrderBy<Asc<CM.CurrencyRate.curyEffDate>>>.SelectWindowed(graph, 0, 100, fromCury, toCury, rateType, effectiveDate);
                if (currencyRates.Count == 0)
                {
                    currencyRates = PXSelect<CurrencyRate, Where<CurrencyRate.fromCuryID, Equal<Required<CurrencyRate.fromCuryID>>,
                    And<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
                    And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,  
                    And<CurrencyRate.curyEffDate, Less<Required<CurrencyRate.curyEffDate>>>>>>, 
                    OrderBy<Desc<CurrencyRate.curyEffDate>>>.SelectWindowed(graph, 0, 1, fromCury, toCury, rateType, effectiveDate);
                }
                foreach (CM.CurrencyRate rate in currencyRates)
                {
                    if (!rates.Contains(rate))
                        rates.Add(rate);
                }
                foundRate = FindCurrencyRate(rates, fromCury, toCury, rateType, (DateTime)effectiveDate);
            }
            return foundRate;
        }

        public object CuryConvCury(object fromCury, object toCury, object rateType, object baseval, object effectiveDate)
        {
            CM.CurrencyRate foundRate = FindCurrencyRate((string)fromCury, (string)toCury, (string)rateType, (DateTime)effectiveDate);
            decimal curyval;
            if (foundRate != null)
            {
                decimal rate;
                try
                {
                    rate = (decimal)foundRate.CuryRate;
                }
                catch (InvalidOperationException)
                {
                    throw new PXRateNotFoundException();
                }
                if (rate == 0.0m)
                {
                    rate = 1.0m;
                }
                bool mult = foundRate.CuryMultDiv != "D";
                curyval = mult ? (decimal)baseval * rate : (decimal)baseval / rate;
            }
            else
            {
                curyval = baseval == null ? 0m : (decimal)baseval;
            }
            return curyval;
        }

        public object CuryConvBase(object fromCury, object rateType, object curyval, object effectiveDate)
        {
            Company company = PXSelect<Company>.Select(new PXGraph());
            object baseval = CuryConvCury(fromCury, company.BaseCuryID, rateType, curyval, effectiveDate);
            return baseval != null ? (decimal)baseval : 0m;
        }
    }
}

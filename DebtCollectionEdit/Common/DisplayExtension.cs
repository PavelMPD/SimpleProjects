using System;

using DebtCollection.Common.Constants;

namespace DebtCollection.Common
{
    public static class DisplayExtension
    {
        public static string DisplayDate(this DateTime? dateTime, string format = SystemConstants.StandartDate)
        {
            return dateTime==null ? String.Empty : ((DateTime) dateTime).ToString(format);
        }

        public static string DisplayCurrency(this long? currency)
        {
            return currency==null ? String.Empty : ((long)currency).ToString("### ### ### ### ### ##0");
        }

        public static string DisplayCurrency(this decimal currency)
        {
            return ((long?) Math.Round(currency, MidpointRounding.AwayFromZero)).DisplayCurrency();
        }

        public static string DisplayCurrency(this decimal? currency)
        {
            return currency==null ? String.Empty : DisplayCurrency((decimal) currency);
        }

        public static string DisplayCurrencyWithFractional(this decimal currency)
        {
            return DisplayCurrencyWithFractional((decimal?) currency);
        }

        public static string DisplayCurrencyWithFractional(this decimal? currency)
        {
            return currency==null ? String.Empty : (((double)currency).ToString("### ### ### ### ### ##0.#####"));
        }
    }
}

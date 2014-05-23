using System;
using System.Globalization;
using System.Text;
using DebtCollection.Common.Constants;

namespace DebtCollection.Common
{
    public class ConvertHelper
    {
        /// <summary>
        /// Round double to long with mathematical rounding
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static long ConvertDoubleToLong(double number)
        {
            return (long)Math.Round(number, 0, MidpointRounding.AwayFromZero);
        }

        public static long ConvertDecimalToLong(decimal number)
        {
            return (long)Math.Round(number, 0, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Round double to long with mathematical rounding
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static long? ConvertDoubleToLong(double? number)
        {
            return number.HasValue ? (long?)ConvertDoubleToLong((double)number) : null;
        }

        public static long? ConvertDecimalToLong(decimal? number)
        {
            return number.HasValue ? (long?)ConvertDecimalToLong((decimal)number) : null;
        }

        public static string ConvertToFormattedMoneyString(decimal? amount)
        {
            var formatInfo = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            formatInfo.NumberGroupSeparator = " ";
            formatInfo.NumberDecimalDigits = 0;

            return amount.HasValue ? amount.Value.ToString("N", formatInfo) : "0";
        }

        public static string ConvertToFormattedMoneyStringForDocx(decimal? amount)
        {
            var formatInfo = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            formatInfo.NumberGroupSeparator = Encoding.UTF8.GetString(new byte[] { 0xC2, 0xA0 });
            formatInfo.NumberDecimalDigits = 0;

            return amount.HasValue ? amount.Value.ToString("N", formatInfo) : "0";
        }

        public static string ConvertToFormattedDateString(DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.ToString(SystemConstants.StandartDate) : "";
        }

        public static string ConvertToNumberByWordsString(decimal val, bool male)
        {
            bool minus = false;
            if (val < 0) { val = -val; minus = true; }

            var n = (int)val;

            var r = new StringBuilder();

            if (0 == n) r.Append("ноль ");
            if (n % 1000 != 0)
                r.Append(NumToStr(n, male, "", "", ""));
            else
                r.Append("");

            n /= 1000;

            r.Insert(0, NumToStr(n, false, "тысяча", "тысячи", "тысяч"));
            n /= 1000;

            r.Insert(0, NumToStr(n, true, "миллион", "миллиона", "миллионов"));
            n /= 1000;

            r.Insert(0, NumToStr(n, true, "миллиард", "миллиарда", "миллиардов"));
            n /= 1000;

            r.Insert(0, NumToStr(n, true, "триллион", "триллиона", "триллионов"));
            n /= 1000;

            r.Insert(0, NumToStr(n, true, "триллиард", "триллиарда", "триллиардов"));
            if (minus) r.Insert(0, "минус ");

            return r.ToString().Trim();
        }

        public static string ConvertToUpperCaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        #region for ConvertToNumberByWordsString method

        private static string[] hunds =
        {
            "", "сто ", "двести ", "триста ", "четыреста ",
            "пятьсот ", "шестьсот ", "семьсот ", "восемьсот ", "девятьсот "
        };

        private static string[] tens =
        {
            "", "десять ", "двадцать ", "тридцать ", "сорок ", "пятьдесят ",
            "шестьдесят ", "семьдесят ", "восемьдесят ", "девяносто "
        };

        private static string[] frac20 =
            {
                "", "один ", "два ", "три ", "четыре ", "пять ", "шесть ",
                "семь ", "восемь ", "девять ", "десять ", "одиннадцать ",
                "двенадцать ", "тринадцать ", "четырнадцать ", "пятнадцать ",
                "шестнадцать ", "семнадцать ", "восемнадцать ", "девятнадцать "
            };

        private static string NumToStr(int val, bool male, string one, string two, string five)
        {
            // TODO: UnitTest this one
            int num = val % 1000;
            if (0 == num) return "";
            if (num < 0) throw new ArgumentOutOfRangeException("val", "Параметр не может быть отрицательным");
            if (!male)
            {
                frac20[1] = "одна ";
                frac20[2] = "две ";
            }
            else
            {
                frac20[1] = "один ";
                frac20[2] = "два ";
            }

            var r = new StringBuilder(hunds[num / 100]);

            if (num % 100 < 20)
            {
                r.Append(frac20[num % 100]);
            }
            else
            {
                r.Append(tens[num % 100 / 10]);
                r.Append(frac20[num % 10]);
            }

            r.Append(Case(num, one, two, five));

            if (r.Length != 0) r.Append(" ");
            return r.ToString();
        }

        private static string Case(int val, string one, string two, string five)
        {
            int t = (val % 100 > 20) ? val % 10 : val % 20;

            switch (t)
            {
                case 1: return one;
                case 2:
                case 3:
                case 4: return two;
                default: return five;
            }
        }

        #endregion for ConvertToNumberByWordsString method

        public static string ConvertToDateByWordsString(DateTime date, GrammaticalCase grammaticalCase)
        {
            string[] days = DayNominative;
            switch (grammaticalCase)
            {
                case GrammaticalCase.Genitive:
                    days = DayGenitive;
                    break;
                case GrammaticalCase.Nominative:
                    days = DayNominative;
                    break;
            }

            var dayAsString = days[date.Day - 1];

            var monthAsString = MonthGenitive[date.Month - 1];

            var yearAsString = ConvertToNumberByWordsString(date.Year, true);

            int indexOfLastWordInDateStr = yearAsString.LastIndexOf(" ", StringComparison.Ordinal);
            if (indexOfLastWordInDateStr == -1)
            {
                yearAsString = string.Empty;
            }
            else
            {
                yearAsString = yearAsString.Substring(0, indexOfLastWordInDateStr + 1);
            }

            int year = date.Year % 10;
            int decade = date.Year % 100;
            int century = date.Year % 1000;
            if (year != 0 && decade < 20) // year like 1914
            {
                yearAsString += DayGenitive[decade - 1];
            }
            else if (year != 0 && decade > 20) // year like 1926
            {
                yearAsString += DayGenitive[year - 1];
            }
            else // year like 1930 || 1900 || 2000
            {
                if (decade != 0) // year like 1930
                {
                    yearAsString += DecadeGenitive[decade / 10 - 1];
                }
                else if (century != 0) // year like 1930
                {
                    yearAsString += CenturyGenitive[century / 100 - 1];
                }
                else
                {
                    indexOfLastWordInDateStr = yearAsString.Trim().LastIndexOf(" ", StringComparison.Ordinal);
                    yearAsString = yearAsString.Substring(0, indexOfLastWordInDateStr + 1);
                    yearAsString += ThousandGenitive[date.Year / 1000 - 1];
                }
            }

            return string.Format("{0} {1} {2} года", dayAsString, monthAsString, yearAsString);
        }

        #region for ConvertToDateByWordsString

        public enum GrammaticalCase
        {
            Nominative,
            Genitive
        }

        private static readonly string[] DayNominative = new[]
            {
                "первое", "второе", "третье", "четвертое", "пятое", "шестое", "седьмое", "восьмое", "девятое",
                "десятое", "одиннадцатое", "двенадцатое",  "тринадцатое", "четырнадцатое","пятнадцатое",
                "шестнадцатое","семнадцатое","восемнадцатое","девятнадцатое","двадцатое","двадцать первое",
                "двадцать второе","двадцать третье","двадцать четвертое","двадцать пятое","двадцать шестое",
                "двадцать седьмое","двадцать восьмое","двадцать девятое","тридцатое","тридцать первое"
            };

        private static readonly string[] DayGenitive = new[]
            {
                "первого", "второго", "третьего","четвертого","пятого","шестого","седьмого","восьмого",
                "девятого","десятого","одиннадцатого","двенадцатого","тринадцатого","четырнадцатого",
                "пятнадцатого","шестнадцатого","семнадцатого","восемнадцатого","девятнадцатого","двадцатого",
                "двадцать первого","двадцать второго","двадцать третьего","двадцать четвертого","двадцать пятого",
                "двадцать шестого","двадцать седьмого","двадцать восьмого","двадцать девятого","тридцатого",
                "тридцать первого"
            };

        private static readonly string[] ThousandGenitive = new[] 
            {
                "тысячного", "двухтысячного", "трехтысячного", "пятитысячного", "шеститысячного", "семитысячного", 
                "восьмитысячного", "девятитысячного" // максимальный год в .NET 9999
            };

        private static readonly string[] CenturyGenitive = new[]
            {
                "сотого", "двухсотого", "трёхсотого", "четырёхстого", "пятисотого", "шестисотого", "семисотого", 
                "восьмисотого", "девятисотого"
            };

        private static readonly string[] DecadeGenitive = new[]
            {
                "десятого", "двадцатого", "тридцатого", "согокового", "пятидесятого", "шестидесятого", "семидесятого", 
                "восьмидесятого", "девяностого"
            };

        private static readonly string[] MonthGenitive = new[]
            {
                "января", "февраля", "марта","апреля","мая","июня","июля","августа", "сентября","октября",
                "ноября","декабря"
            };

        #endregion
    }
}
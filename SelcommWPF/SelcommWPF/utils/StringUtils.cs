using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SelcommWPF.utils
{
    class StringUtils
    {
        public static string Remove_Escape_Sequences(string sText, string sReplace = "")
        {

            sText = sText.Replace("\\\\r\\\\n", Environment.NewLine);
            sText = sText.Replace("\\r\\n", Environment.NewLine);
            sText = sText.Replace("\r\n", Environment.NewLine);
            sText = sText.Replace("\\n", Environment.NewLine);
            sText = sText.Replace("\\\\", ""); // Backslash
            sText = sText.Replace("\\", ""); // Backslash
            sText = Remove_Escape_Trigraph(sText);

            return sText;


        }

        public static string Remove_Escape_Trigraph(string sText, string sReplace = "")
        {

            sText = sText.Replace("??=", sReplace); //#

            sText = sText.Replace("??(" , sReplace); // [

            sText = sText.Replace("??/", sReplace); // \

            sText = sText.Replace("??)\" " , sReplace); // ]

            sText = sText.Replace("??'\" )" , sReplace); // ^

            sText = sText.Replace("?\") < \"(", sReplace); // {

            sText = sText.Replace("??!" , sReplace); // |

            sText = sText.Replace("?\") > \"(", sReplace); // }

            sText = sText.Replace("??-", sReplace); // ~

            return sText;

        }

        public static string ConvertCurrency(double value, string currencyCode = "en-AU")
        {
            bool isNegative = value < 0;
            string result = value.ToString("C", CultureInfo.CreateSpecificCulture(currencyCode));

            switch (currencyCode)
            {
                case "en-AU":
                    result = isNegative ? string.Format("-A{0}", result.Substring(1)) : string.Format("A{0}", result);
                    break;
            }

            return result;
        }

        public static string ConvertDateTime(string date)
        {
            return date == null ? "" : DateTime.Parse(date).ToString("yyyy-MM-dd hh:mm:ss tt");
        }

        public static string ConvertDate(string date)
        {
            return date == null ? "" : DateTime.Parse(date).ToString("dd/MM/yyyy");
        }

        public static bool IsPhoneNbr(string number)
        {
            return Regex.Match(number, @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$").Success;
        }

        public static string FormatBytes(long bytes)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0 Bytes";
        }

    }
}

using ProductionApp.Models;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace ProductionApp.Helpers {
    public static class Utilities {
        private static String logPath = @"~\log\";

        public static void WriteLog(String str = "" ,String module = "") {
            var sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
            if((UserModels)HttpContext.Current.Session["SignedInUser"] != null)
                sb.Append(((UserModels)HttpContext.Current.Session["SignedInUser"]).Username + "\r\n");
            if(module != "")
                sb.Append(module + "\r\n");
            sb.Append(str);
            sb.Append("\r\n-----------------------\r\n\r\n");
            File.AppendAllText(
                Path.Combine(HttpContext.Current.Server.MapPath(logPath) ,
                    "error" + GetYYYYMMDDLog(DateTime.Now) + ".txt") ,sb.ToString());
            sb.Clear();
            Debug.WriteLine(sb);
        }

        public static void WriteLogException(Exception e ,String mess = "") {
            var sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n");
            if((UserModels)HttpContext.Current.Session["SignedInUser"] != null)
                sb.Append(((UserModels)HttpContext.Current.Session["SignedInUser"]).Username + "\r\n");
            if(mess != "")
                sb.Append(mess + "\r\n");

            sb.Append("Message:" + e.Message + "\r\n");
            sb.Append("Data:" + e.Data + "\r\n");
            sb.Append("HResult:" + e.HResult + "\r\n");
            sb.Append("HelpLink:" + e.HelpLink + "\r\n");
            sb.Append("Source:" + e.Source + "\r\n");
            sb.Append("InnerException:" + e.InnerException + "\r\n");
            sb.Append("StackTrace:" + e.StackTrace + "\r\n");
            sb.Append("TargetSite:" + e.TargetSite + "\r\n");


            var st = new StackTrace(e ,true);
            for(var i=0; i < st.FrameCount; i++) {
                var frame = st.GetFrame(i);
                sb.Append("Line:" + frame.GetFileLineNumber() + " in Method " + frame.GetMethod().Name + ", File " + frame.GetFileName() + ", Column " + frame.GetFileColumnNumber() + "\r\n");

            }

            sb.Append("\r\n-----------------------\r\n\r\n");
            File.AppendAllText(
                Path.Combine(HttpContext.Current.Server.MapPath(logPath) ,
                    "error" + GetYYYYMMDDLog(DateTime.Now) + ".txt") ,sb.ToString());
            sb.Clear();
            Debug.WriteLine(sb);
        }

        public static double DateDiff(DateTime s1 ,DateTime s2) {

            return (s2 - s1).TotalDays;
        }
        public static string GetYYYYMMDDLog(DateTime d) {

            try {
                return d.ToString("yyyyMMdd");
            } catch(Exception e) {
                WriteLogException(e ,"GetYYYYMMDDLog");
                return "error";
            }
        }

        public static string GetMMDDYYYY(String dateTime) {
            try {
                return Convert.ToDateTime(dateTime).ToString("MM/dd/yyyy");
            } catch(Exception e) {
                WriteLogException(e ,"GetMMDDYYYY");
                return "error";
            }

        }
        public static string GetDDMMYYYY(String dateTime) {
            try {
                return Convert.ToDateTime(dateTime).ToString("MM/dd/yyyy");
            } catch(Exception e) {
                WriteLogException(e ,"GetDDMMYYYY");
                return "error";
            }

        }
        public static string DateFormat(String dateTime ,string format) {
            try {
                return Convert.ToDateTime(dateTime).ToString(format);
            } catch(Exception e) {
                WriteLogException(e ,"DateFormat");
                return "error";
            }

        }

        public static String GetTT(String dateTime) {
            try {
                return Convert.ToDateTime(dateTime).ToString("tt" ,CultureInfo.InvariantCulture);
            } catch(Exception e) {
                WriteLogException(e ,"GetTT");
                return "error";
            }
        }
        public static DateTime GetDate_VietNam(DateTime dateTime) {
            TimeZoneInfo hwZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTime(dateTime ,hwZone);
            //,TimeZoneInfo.Local
        }

        public static String NumberFormat(float? number1) {
            var number = number1 ?? 0;
            try {
                return number.ToString("N1" ,CultureInfo.InvariantCulture);
            } catch(Exception e) {
                WriteLogException(e ,"NumberFormat");
                return "error";
            }
        }
        public static string NumberFormat(double? number1) {
            var number = number1 ?? 0;
            try {
                return number.ToString("N1" ,CultureInfo.InvariantCulture);
            } catch(Exception e) {
                WriteLogException(e ,"NumberFormat");
                return "error";
            }
        }
        public static string NumberFormat(int? number1) {
            var number = number1 ?? 0;
            var kq = "";
            try {
                kq = number.ToString("N1" ,CultureInfo.InvariantCulture);
                return kq.Substring(0 ,kq.Length - 2);
            } catch(Exception e) {
                WriteLogException(e ,"NumberFormat");
                return "error";
            }
        }

        public static double ValidDouble(String numberString) {
            double number;
            var success = double.TryParse(numberString ,out number);
            return success ? double.Parse(numberString) : 0;
        }



        public static bool SendEmail(string subject ,string from ,string to ,string cc ,string body) {
            //return true;
            //cc += (cc == "" ? "" : ";") + "hieu.hoang@hanes.com";
            //to = "hieu.hoang@hanes.com";
            //cc = "hieu.hoang@hanes.com";
            var mail = new TBL_MailLog {
                mTitle = subject ,
                mFrom = from ,
                mTo = to ,
                mCC = cc ,
                mDate = DateTime.Now ,
                mContent = body
            };
            var db = new MyContext();
            try {

                var message = new MailMessage();
                var toMails = to.Split(';');
                foreach(var toMail in toMails) {
                    if(toMail != "") {
                        message.To.Add(new MailAddress(toMail)); // replace with valid value 
                    }
                }

                var toCCs = cc.Split(';');
                foreach(var tocc in toCCs) {
                    if(tocc != "") {
                        message.CC.Add(new MailAddress(tocc)); // replace with valid value 
                    }

                }

                var signature = "<br/><br/><br/><br/>----------------------------------------------<br/>";
                signature += "Welcome to HYC Application Center</br>";
                var ip = db.TBL_SYSTEM.SingleOrDefault(a => a.id == "website");
                if(ip != null)
                    signature += "<a href='" + ip.value + "'>" + ip.value + "</a>";

                message.Subject = subject;
                message.Body = body + signature;
                message.IsBodyHtml = true;
                using(var smtp = new SmtpClient()) {

                    message.From = new MailAddress(from); // replace with valid value
                    smtp.Port = 25;
                    smtp.Host = "hbiexchsmtp-vip.res.hbi.net";
                    smtp.Send(message);

                }

                mail.mstatus = 1;
                db.TBL_MailLog.Add(mail);
                db.SaveChanges();
            } catch(Exception e) {
                mail.mstatus = 2;
                db.TBL_MailLog.Add(mail);
                db.SaveChanges();
                WriteLogException(e ,"SendEmail ID " + mail.id);

            }



            return mail.mstatus == 1;
        }

        public static int GetIso8601WeekOfYear(DateTime time) {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if(day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday) {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time ,CalendarWeekRule.FirstFourDayWeek ,DayOfWeek.Monday);
        }
        public static DataTable ListToDataTable<T>(this IList<T> data) {
            DataTable dataTable = new DataTable(typeof(T).Name);
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach(PropertyInfo prop in props) {
                dataTable.Columns.Add(prop.Name ,Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            foreach(T item in data) {
                var values = new object[props.Length];
                for(int i = 0; i < props.Length; i++) {
                    values[i] = props[i].GetValue(item ,null);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }
        public static int ConvertStringToInt(string strConvertToInt) {
            try {
                int numberReturn = 0;
                bool isConvertOk = false;
                isConvertOk = int.TryParse(strConvertToInt ,out numberReturn);
                if(isConvertOk)
                    return numberReturn;
                else
                    return 0;
            } catch {
                return 0;
            }
        }

        public static double ConvertStringToDouble(string strConvertToDouble) {
            try {
                double numberReturn = 0;
                bool isConvertOk = false;
                isConvertOk = double.TryParse(strConvertToDouble ,out numberReturn);
                if(isConvertOk)
                    return numberReturn;
                else
                    return 0;
            } catch {
                return 0;
            }
        }

        public static string ValidEmpID(string empid) {
            empid = empid.Trim();
            if(empid.Length < 6)
                return "000000".Substring(empid.Length) + empid;

            return empid.ToUpper();
        }

        public static string MD5Hash(string input) {
            // Step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < hashBytes.Length; i++) {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// Convert a object to Decimal
        /// </summary>
        /// <param name="input"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static decimal NullSafeDecimal(object input, decimal defaultValue)
        {
            if (input != null)
            {
                if (!string.IsNullOrEmpty(input.ToString()))
                {
                    decimal outValue;
                    bool success = decimal.TryParse(input.ToString(), out outValue);
                    if (success)
                        return outValue;
                }
            }
            return defaultValue;
        }

        public static string NonUnicode(this string text)
        {
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",  
                "đ",  
                "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",  
                "í","ì","ỉ","ĩ","ị",  
                "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",  
                "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",  
                "ý","ỳ","ỷ","ỹ","ỵ",};
            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",  
                "d",  
                "e","e","e","e","e","e","e","e","e","e","e",  
                "i","i","i","i","i",  
                "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",  
                "u","u","u","u","u","u","u","u","u","u","u",  
                "y","y","y","y","y",};
            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
                text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }
            return text;
        }  
    }
}
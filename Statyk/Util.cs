using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Statyk
{
    public static class Util
    {
        public static string GetFullError(Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(ex.Message);

            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                sb.AppendLine(ex.Message);
            }
            return sb.ToString();
        }

        public static string GetFullStackTraceError(Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                var fullStack = ex.InnerException.StackTrace;
                sb.AppendLine(fullStack);
            }
            return sb.ToString();
        }

        public static string AssemblyDirectory
        {
            get
            {
                string location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(location))
                    location = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

                return location;
            }
        }

        public static bool IsValidJson(this string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {

                try
                {

                    var obj =  JsonSerializer.Serialize(strInput);

                    return true;
                }

                catch (JsonException jex)
                {
                    //Exception in parsing json
                    Debug.WriteLine(jex.Message);
                    return false;
                }

                catch (Exception ex) //some other exception
                {
                    Debug.WriteLine(ex.ToString());
                    return false;
                }

                }
            else
            {
                return false;
            }
        }
    }
}

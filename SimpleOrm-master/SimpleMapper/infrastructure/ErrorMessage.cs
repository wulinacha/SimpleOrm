using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    public class ErrorMessage
    {
        internal static string ConnectionConfigIsNull
        {
            get
            {
                return GetThrowMessage("ConnectionConfig can't be null",
                                       "ConnectionConfig不能为null。");
            }
        }
        internal static string DateProviderIsNull
        {
            get
            {
                return GetThrowMessage("DateProvider cant't be null",
                    "DataProvider不能为null");
            }
        }
        internal static string NoRegisterDataProvider
        {
            get
            {
                return GetThrowMessage("DateProvider is no register",
                    "未注册数据库提供者");
            }
        }
        internal static string GetThrowMessage(string enMessage, string cnMessage, params string[] args)
        {
            List<string> formatArgs = new List<string>() { enMessage, cnMessage };
            formatArgs.AddRange(args);
            return string.Format("\r\n English Message : {0}\r\n Chinese Message : {1}", formatArgs.ToArray());
        }
    }
}

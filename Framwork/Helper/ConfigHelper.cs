using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framwork
{
    public class ConfigHelper
    {
        public static string GetConfigurationManagerStr(string name) {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

        public static string GetAppSettingStr(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }

    }
}

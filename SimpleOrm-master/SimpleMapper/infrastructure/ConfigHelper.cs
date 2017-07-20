using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper.infrastructure
{
    public class ConfigHelper
    {
        public static string GetConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }
        public static ConnectionStringSettings GetConnectionSettings(string name)
        {
            return ConfigurationManager.ConnectionStrings[name];
        }

        public static string GetAppSettingStr(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    public class Check
    {
        public static void ConnectionConfig(string config,string message=null)
        {
            if (config == null || config.IsNullOrSpace())
            {
                throw new SimpleException("SimpleException.ArgumentNullException：" + ErrorMessage.ConnectionConfigIsNull);
            }
        }
        public static void ArgumentNullException(object checkobject, string message=null)
        {
            if (checkobject.IsNullOrSpace()) {
                throw new SimpleException("SimpleException.ArgumentNullException：" + ErrorMessage.DateProviderIsNull);
            }
        }
        public static void ThrowDataException(Exception ex)
        {
            SimpleException exception;
            try
            {
                exception = new SimpleException("SimpleException.SqlDataException:" + ex.Message);
            }
            catch
            {
                exception = new SimpleException("SimpleException.SqlDataException:" + ex);
            }
            throw exception;
        }
        public static void ProviderIsNoRegister(string connstr,Func<string, bool> express, string message = null) {
            if (!express(connstr))
            {
                throw new SimpleException("SimpleException.NotRegisterException:" + ErrorMessage.NoRegisterDataProvider);
            }
        }
    }
}

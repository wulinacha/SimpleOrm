using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framwork
{
    public class ServiceResult
    {
        public ServiceResult() { }
        public ServiceResult(bool success,string message) {
            this.Success = success;
            this.Message = message;
        }
        public bool Success { get; set; }

        public string Message { get; set; }

        public ServiceResult SetSuccess(string msg = "成功")
        {
            this.Message = msg;
            this.Success = true;
            return this;
        }

        public ServiceResult SetFaild(string msg = "失败")
        {
            this.Message = msg;
            this.Success = false;
            return this;
        }
    }
}

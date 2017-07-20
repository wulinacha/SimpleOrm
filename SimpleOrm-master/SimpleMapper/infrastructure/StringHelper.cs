using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper.infrastructure
{
    public class StringHelper
    {
        /// <summary>
        /// 去掉最后一个元素
        /// </summary>
        /// <param name="targ"></param>
        /// <returns></returns>
        public static string RemoveLastElment(string targ) {
            return targ.ToString().Substring(0, targ.Length - 1);
        }
        /// <summary>
        /// 去掉第一个元素和逗号
        /// </summary>
        /// <returns></returns>
        public static string RemoveDotFirstElment(string targ) {
            string[] strlist=targ.Split(',');
            return strlist.Skip(1).ToString();
        }

    }
}

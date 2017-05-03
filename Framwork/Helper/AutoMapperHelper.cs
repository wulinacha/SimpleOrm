using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framwork
{
    public class AutoMapperHelper<T, TResult> where T : class where TResult:class
    {
        public static TResult Change(T model) {
            return AutoMapper.Mapper.DynamicMap<TResult>(model);
        }
    }
}

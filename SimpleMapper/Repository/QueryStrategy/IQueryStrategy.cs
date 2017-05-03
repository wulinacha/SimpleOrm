using SimpleMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper.Rpository
{
    public interface IQueryStrategy
    {
        string Excute();
        IQueryStrategy Equal(Condition condition);
        IQueryStrategy Equal(List<Condition> condition);
    }
}

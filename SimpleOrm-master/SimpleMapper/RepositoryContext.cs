using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace SimpleMapper
{
    public class RepositoryContext
    {
        public string connectionString { get; set; }
        public RepositoryContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public Repository<T> GetRepository<T>()where T:new() {
                return new Repository<T>(connectionString);
        } 
    }
}

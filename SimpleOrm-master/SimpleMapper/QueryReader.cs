using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    public class QueryReader:IDisposable
    {
        public QueryReader(IDataReader reader, IDbCommand command, string connctionStr)
        {
            this.reader = reader;
            this.command = command;
            this.connctionStr = connctionStr;
        }
        private IDataReader reader { get; set; }
        private IDbCommand command { get; set; }
        private string connctionStr { get; set; }
        public IEnumerable<T> ReadList<T>()
        {
            Type type = typeof(T);
            Func<IDataReader, object> act = null;
            object obj = null;
            int cachekey = SqlMapper.GetHashKey(connctionStr, type);
            if (SqlMapper.QueryMultiReaderActionCache.ContainsKey(cachekey))
            {
                act = SqlMapper.QueryMultiReaderActionCache[cachekey];
            }
            else
            {
                act = this.ReadImpl<T>(type, reader);
            }
            while (reader.Read())
            {
                obj=act(reader);
                if (obj == null) continue;
                yield return (T)obj;
            }
            ReadNextResult();
        }
        public T Read<T>()
        {
            Type type = typeof(T);
            Func<IDataReader, object> act = null;
            object obj = null;
            int cachekey = SqlMapper.GetHashKey(connctionStr, type);
            if (SqlMapper.QueryMultiReaderActionCache.ContainsKey(cachekey))
            {
                act = SqlMapper.QueryMultiReaderActionCache[cachekey];
            }
            else
            {
                act = this.ReadImpl<T>(type, reader);
            }
            reader.Read();
            obj = act(reader);
            ReadNextResult();
            return (T)obj;
        }
        public Func<IDataReader, object> ReadImpl<T>(Type type, IDataReader reader)
        {
            return SqlMapper.GetInitModelByReader(type, reader);
        }
        protected void ReadNextResult()
        {
            if (!this.reader.NextResult())
            {
                this.reader.Close();
                this.reader.Dispose();
                this.reader = null;
                var conn = this.command.Connection;
                conn.Close();
                conn.Dispose();
                this.command.Dispose();
                this.command = null;
            }
        }
        public void Dispose()
        {
            if (this.reader != null)
            {
                if (!reader.IsClosed)
                {
                    this.reader.Close();
                }
                this.reader.Dispose();
                this.reader = null;
            }
            if (this.command != null)
            {
                this.command.Dispose();
                this.command = null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    public class ColumnMap
    {
        private readonly string _columnName;
        private readonly PropertyInfo _property;
        private readonly PrimaryKeyAttribute _primaryKeyAttribute;
        private readonly ColumnAttribute _columnAttribute;

        public ColumnMap(string columnName, PropertyInfo propertyInfo, PrimaryKeyAttribute primaryKeyAttribute,ColumnAttribute columnAttribute)
        {
            this._columnName = columnName;
            this._property = propertyInfo;
            this._primaryKeyAttribute = primaryKeyAttribute;
            this._columnAttribute = columnAttribute;
        }

        public Type MemberType
        {
            get
            {
                if (this._property != null)
                {
                    return this._property.PropertyType;
                }
                return null;
            }
        }

        public MethodInfo GetMethodInfo
        {
            get
            {
                return this._property.GetGetMethod(true);
            }
        }
        
        public MethodInfo SetMethodInfo { 
            get{
                return this._property.GetSetMethod(true);
            }
        }

        public string MemberName
        {
            get
            {
                if (_columnAttribute.IsNullOrSpace())
                    return this._columnName;
                else
                    return this._columnAttribute.columnName;
            }
        }

        public bool isPrimaryKey
        {
            get
            {
                return this._primaryKeyAttribute!=null;
            }
        }
        public bool isAutoIncrement
        {
            get
            {
                if (this._primaryKeyAttribute== null)
                    return false;
                return this._primaryKeyAttribute.autoIncrement;
            }
        }
    }
}

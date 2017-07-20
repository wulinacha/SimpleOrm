using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapper
{
    public class DataMap
    {
        public DataMap(string _tableName, string _className, Type type)
        {
            this.tableName = _tableName;
            this.className = _className;
            this._fields = GetSettableFields(type);
            this._properties = GetSettableProps(type);
            this._type = type;
            this._primaryKeyAttributes = GetPrimaryKeyAttribute(type);
            this._columnAttributes = GetColumnAttribute(type);
        }
        public string tableName { get; set; }
        public string className { get; set; }

        public readonly List<FieldInfo> _fields;
        public readonly List<PropertyInfo> _properties;
        public readonly Type _type;
        public readonly List<PrimaryKeyAttribute> _primaryKeyAttributes;
        public readonly List<ColumnAttribute> _columnAttributes;

        internal static List<FieldInfo> GetSettableFields(Type t)
        {
            return t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).ToList<FieldInfo>();
        }

        internal static List<PropertyInfo> GetSettableProps(Type t)
        {
            return (from p in t.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    where GetPropertySetter(p, t) != null && !p.PropertyType.IsGenericType&&!p.PropertyType.IsArray
                    select p).ToList<PropertyInfo>();
        }
        public static List<PrimaryKeyAttribute> GetPrimaryKeyAttribute(Type t)
        {
            return (from p in t.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    where p.GetCustomAttribute<PrimaryKeyAttribute>()!=null
                    select p.GetCustomAttribute<PrimaryKeyAttribute>()).ToList();
        }
        public static List<ColumnAttribute> GetColumnAttribute(Type t)
        {
            return (from p in t.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    where p.GetCustomAttribute<ColumnAttribute>() != null
                    select new ColumnAttribute(p.GetCustomAttribute<ColumnAttribute>().columnName) 
                    { fieldName = p.Name }).ToList();
        }
        internal static MethodInfo GetPropertySetter(PropertyInfo propertyInfo, Type type)
        {
            return propertyInfo.GetSetMethod(true);
        }
        public string GetColumnMapListStr()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in (from p in _properties select GetMember(p.Name)))
            {
                sb.Append(item.MemberName+ ",");
            }
            return sb.ToString().Substring(0, sb.Length - 1);
        }

        public FieldInfo GetColumnMapByFileName(string fileName)
        {
            return _fields.Where(e => e.Name == fileName).FirstOrDefault();
        }

        //构造函数-当个构造函数
        private ConstructorInfo FindExplicitConstructor()
        {
            List<ConstructorInfo> list = this._type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).ToList<ConstructorInfo>();//(<>c.<>9__8_0 ?? (<>c.<>9__8_0 = new Func<ConstructorInfo, bool>(<>c.<>9.<FindExplicitConstructor>b__8_0)))
            if (list.Count == 1)
                return list[0];

            throw new NullReferenceException("暂不支持多个构造函数的实体");
        }
         public ConstructorInfo FindConstructor()
         {
             ConstructorInfo info = FindExplicitConstructor();
             ParameterInfo[] parameters = info.GetParameters();
             if (parameters.Length == 0)
                 return info;

             throw new NullReferenceException("暂时支持默认无参构造函数");
         }

         public ColumnMap GetMember(string columnName)
         {
             var primaryKeyAttribute = this._primaryKeyAttributes.FirstOrDefault(a => a.fieldName.ToLower() == columnName.ToLower());
             var columnAttribute = this._columnAttributes.FirstOrDefault(a => string.Equals(a.fieldName, columnName,StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(a.columnName, columnName,StringComparison.OrdinalIgnoreCase));
             var propertyName=columnAttribute.IsNullOrSpace()?null:columnAttribute.fieldName;
             PropertyInfo property = this._properties.FirstOrDefault<PropertyInfo>(p => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase)
                 || string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
             if (!property.IsNullOrSpace())
                 return new ColumnMap(columnName, property, primaryKeyAttribute, columnAttribute);
             return null;
         }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using SimpleMapper.Providers;

namespace SimpleMapper
{
    public class SqlMapper
    {
        private static Dictionary<Type, string> typeMap;
        static SqlMapper()
        {
            typeMap = new Dictionary<Type, string>();
            typeMap[typeof(byte)] = "ToByte";
            typeMap[typeof(sbyte)] = "ToSByte";
            typeMap[typeof(short)] = "ToInt16";
            typeMap[typeof(ushort)] = "ToUInt16";
            typeMap[typeof(int)] = "ToInt32";
            typeMap[typeof(uint)] = "ToUInt32";
            typeMap[typeof(long)] = "ToInt64";
            typeMap[typeof(ulong)] = "ToUInt64";
            typeMap[typeof(float)] = "ToSingle";
            typeMap[typeof(double)] = "ToDouble";
            typeMap[typeof(decimal)] = "ToDecimal";
            typeMap[typeof(bool)] = "ToBoolean";
            typeMap[typeof(string)] = "ToString";
            typeMap[typeof(char)] = "ToStringFixedLength";
            typeMap[typeof(Guid)] = "ToGuid";
            typeMap[typeof(DateTime)] = "ToDateTime";
            typeMap[typeof(DateTimeOffset)] = "ToDateTimeOffset";
            typeMap[typeof(TimeSpan)] = "ToTime";
            typeMap[typeof(byte[])] = "ToBinary";
            typeMap[typeof(byte?)] = "ToByte";
            typeMap[typeof(sbyte?)] = "ToSByte";
            typeMap[typeof(short?)] = "ToInt16";
            typeMap[typeof(ushort?)] = "ToUInt16";
            typeMap[typeof(int?)] = "ToInt32";
            typeMap[typeof(uint?)] = "ToUInt32";
            typeMap[typeof(long?)] = "ToInt64";
            typeMap[typeof(ulong?)] = "ToUInt64";
            typeMap[typeof(float?)] = "ToSingle";
            typeMap[typeof(double?)] = "ToDouble";
            typeMap[typeof(decimal?)] = "ToDecimal";
            typeMap[typeof(bool?)] = "ToBoolean";
            typeMap[typeof(char?)] = "ToStringFixedLength";
            typeMap[typeof(Guid?)] = "ToGuid";
            typeMap[typeof(DateTime?)] = "ToDateTime";
            typeMap[typeof(DateTimeOffset?)] = "ToDateTimeOffset";
            typeMap[typeof(TimeSpan?)] = "ToTime";
            typeMap[typeof(object)] = "ToObject";

            getItem = typeof(IDataRecord).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where<PropertyInfo>(new Func<PropertyInfo, bool>(n => isHasParameter(n))).Select<PropertyInfo, MethodInfo>(new Func<PropertyInfo, MethodInfo>(n => GetPropertyInfoMethod(n))).First<MethodInfo>();
            Type[] typeArray3 = new Type[] { typeof(Type), typeof(string), typeof(bool) };
            enumParse = typeof(Enum).GetMethod("Parse", typeArray3);
        }

        private static readonly MethodInfo getItem;
        private static readonly MethodInfo enumParse;
        private static ConcurrentDictionary<int, Func<IDataReader, object>> readerActionCache = new ConcurrentDictionary<int, Func<IDataReader, object>>();
        private static ConcurrentDictionary<int, List<Func<IDataReader, object>>> multiReaderActionCache = new ConcurrentDictionary<int, List<Func<IDataReader, object>>>();
        public static ConcurrentDictionary<int, Func<IDataReader, object>> QueryMultiReaderActionCache = new ConcurrentDictionary<int, Func<IDataReader, object>>();
        private static ConcurrentDictionary<int, Func<object, string>> handleActionCache = new ConcurrentDictionary<int, Func<object, string>>();
        private static ConcurrentDictionary<int, Func<object,BaseProvider, List<DbParameter>>> paramsActionCache = new ConcurrentDictionary<int, Func<object,BaseProvider, List<DbParameter>>>();
        /// <summary>
        /// 判断是否存在属性
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        internal static bool isHasParameter(PropertyInfo p)
        {
            return (p.GetIndexParameters().Any<ParameterInfo>() && (p.GetIndexParameters()[0].ParameterType == typeof(int)));
        }
        /// <summary>
        /// 获取Get方法
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        internal static MethodInfo GetPropertyInfoMethod(PropertyInfo p)
        {
            return p.GetGetMethod();//返回Get方法
        }

        #region 数据加载
        public static TResult Load<TResult>(Type type, IDataReader reader, int ReaderActionKey) 
        {
            if (!readerActionCache.ContainsKey(ReaderActionKey))
            {
                Func<IDataReader, object> arc = GetInitModelByReader(type, reader);
                readerActionCache.TryAdd(ReaderActionKey, arc);
            }
            return (TResult)((Func<IDataReader, object>)readerActionCache[ReaderActionKey])(reader);
        }
        public static Func<IDataReader, object> GetInitModelByReader(Type type, IDataReader reader)
        {
            Type[] parameterTypes = new Type[] { typeof(IDataReader) };
            DynamicMethod method = new DynamicMethod(string.Format("Deserialize{0}", Guid.NewGuid()), typeof(object), parameterTypes, true);
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.DeclareLocal(typeof(int));
            iLGenerator.DeclareLocal(type);
            iLGenerator.Emit(OpCodes.Ldc_I4_0);
            iLGenerator.Emit(OpCodes.Stloc_0);
            DataMap dataMap = Metadata.GetDataMap(type);
            string[] names = (from i in Enumerable.Range(0, reader.FieldCount) select reader.GetName(i))
                .Where(e => e != "RN" && (dataMap._properties.Where(p => p.Name == e).Count() > 0 
                    || dataMap._columnAttributes.Where(p => p.columnName == e).Count() > 0)).Distinct().ToArray<string>();
            ConstructorInfo info = dataMap.FindConstructor();
  
            iLGenerator.Emit(OpCodes.Newobj, info);
            iLGenerator.Emit(OpCodes.Stloc_1);
            iLGenerator.BeginExceptionBlock();
            int num = 0;
            int localIndex = iLGenerator.DeclareLocal(typeof(object)).LocalIndex;
        
            foreach (ColumnMap map in (from n in names select dataMap.GetMember(n)).ToList<ColumnMap>())
            {
                //var labelElse = iLGenerator.DefineLabel();
                //var labelEnd = iLGenerator.DefineLabel();

                iLGenerator.Emit(OpCodes.Ldarg_0);//加载reader
                iLGenerator.Emit(OpCodes.Ldstr,map.MemberName);
                MethodInfo getValueMethod = typeof(IDataRecord).GetMethod("get_Item", new[] { typeof(string) });
                iLGenerator.Emit(OpCodes.Callvirt, getValueMethod);
                iLGenerator.Emit(OpCodes.Stloc, (short)localIndex);

                iLGenerator.Emit(OpCodes.Ldloc_1);//压入对象
                iLGenerator.Emit(OpCodes.Ldloc, localIndex);
                iLGenerator.Emit(OpCodes.Dup);
                Label notNullLabel = iLGenerator.DefineLabel();
                iLGenerator.Emit(OpCodes.Isinst, typeof(DBNull));//是1，否0
                iLGenerator.Emit(OpCodes.Brfalse_S, notNullLabel);//如果非空、非0则控制权转到label
                iLGenerator.Emit(OpCodes.Pop);
                iLGenerator.Emit(OpCodes.Ldstr,"0");
                iLGenerator.Emit(OpCodes.Stloc, localIndex);
                iLGenerator.Emit(OpCodes.Ldloc, localIndex);
                iLGenerator.Emit(OpCodes.Br_S, notNullLabel);
                iLGenerator.MarkLabel(notNullLabel);
                iLGenerator.Emit(OpCodes.Call, typeof(Convert).GetMethod(typeMap[map.MemberType], new Type[] { typeof(object) }));
                iLGenerator.Emit(OpCodes.Callvirt, map.SetMethodInfo);
                num++;
            }
            iLGenerator.BeginCatchBlock(typeof(Exception));
            iLGenerator.EmitCall(OpCodes.Call, typeof(Check).GetMethod("ThrowDataException"), null);
            iLGenerator.EndExceptionBlock();
            //iLGenerator.Emit(OpCodes.Ldloc, localIndex);
            iLGenerator.Emit(OpCodes.Ldloc_1);
            iLGenerator.Emit(OpCodes.Ret);
            return (Func<IDataReader, object>)method.CreateDelegate(typeof(Func<IDataReader, object>));
        }

        #endregion

        #region 多结果集数据加载

        public static TReturn MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(Delegate map, IDataReader reader, int ReaderActionKey)
        { 
            List<Func<IDataReader,object>> list=new List<Func<IDataReader,object>>();
            
            if (multiReaderActionCache.ContainsKey(ReaderActionKey)){
                list = multiReaderActionCache[ReaderActionKey];
            }
            else
            {
                list = MultiMapImpl<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(reader);
                multiReaderActionCache.TryAdd(ReaderActionKey,list);
            }

            return GenerateMapper<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(list.ToArray(), map)(reader);
        }
        public static List<Func<IDataReader, object>> MultiMapImpl<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(IDataReader reader)
        {
            List<Func<IDataReader, object>> list = new List<Func<IDataReader, object>>();
            Type[] types = new Type[] { typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh) };
            foreach (var type in types)
            {
                if (type == typeof(DontMap))
                    break;
               list.Add(GetInitModelByReader(type,reader));
            }
            return list;
        }
        private static Func<IDataReader, TReturn> GenerateMapper<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(Func<IDataReader, object>[] deserializer,object map)
        {
            switch (deserializer.Length)
            {
                case 2:
                    return r => ((System.Func<TFirst, TSecond, TReturn>)map)((TFirst)deserializer[0](r), (TSecond)deserializer[1](r));
                case 3:
                    return r => ((Func<TFirst, TSecond, TThird, TReturn>)map)((TFirst)deserializer[0](r), (TSecond)deserializer[1](r), (TThird)deserializer[2](r));

                case 4:
                    return r => ((Func<TFirst, TSecond, TThird, TFourth, TReturn>)map)((TFirst)deserializer[0](r), (TSecond)deserializer[1](r), (TThird)deserializer[2](r), (TFourth)deserializer[3](r));

                case 5:
                    return r => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>)map)((TFirst)deserializer[0](r), (TSecond)deserializer[1](r), (TThird)deserializer[2](r), (TFourth)deserializer[3](r), (TFifth)deserializer[3](r));

                case 6:
                    return r => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>)map)((TFirst)deserializer[0](r), (TSecond)deserializer[1](r), (TThird)deserializer[2](r), (TFourth)deserializer[3](r), (TFifth)deserializer[4](r), (TSixth)deserializer[5](r));

                case 7:
                    return r => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>)map)((TFirst)deserializer[0](r), (TSecond)deserializer[1](r), (TThird)deserializer[2](r), (TFourth)deserializer[3](r), (TFifth)deserializer[4](r), (TSixth)deserializer[5](r), (TSeventh)deserializer[7](r));
            }
            throw new NotSupportedException();
        }
        #endregion

        #region 获取参数
        public static List<DbParameter> GetParams(object paramters, Type parametersType, BaseProvider provider, int paramsActionKey)
        {
            if (!paramsActionCache.ContainsKey(paramsActionKey))
            {
                Func<object, BaseProvider, List<DbParameter>> arc = GetParamsImpl(paramters, parametersType);
                paramsActionCache.TryAdd(paramsActionKey, arc);
            }
            return paramsActionCache[paramsActionKey](paramters, provider);
            
        }
        public static Func<object, BaseProvider, List<DbParameter>> GetParamsImpl(object paramters, Type parametersType)
        {
            DynamicMethod method = new DynamicMethod(string.Format("Deserialize{0}", Guid.NewGuid()), typeof(List<DbParameter>), new Type[]{typeof(object),typeof(BaseProvider)}, true);
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.DeclareLocal(typeof(List<DbParameter>));
            iLGenerator.Emit(OpCodes.Newobj, typeof(List<DbParameter>).GetConstructor(new Type[] { }));
            iLGenerator.Emit(OpCodes.Stloc_0);

            List<PropertyInfo> items=parametersType.GetProperties().ToList();
            iLGenerator.BeginExceptionBlock();
            var indexlocal = iLGenerator.DeclareLocal(typeof(object));
            var indexlocalDbParameter=iLGenerator.DeclareLocal(typeof(DbParameter));
            foreach (PropertyInfo item in items)
            {
               iLGenerator.Emit(OpCodes.Ldarg_0);
               iLGenerator.Emit(OpCodes.Unbox_Any, parametersType);
               iLGenerator.Emit(OpCodes.Callvirt, item.GetGetMethod(true));

               if (item.PropertyType.IsValueType)
               {
                   iLGenerator.Emit(OpCodes.Box, item.PropertyType);
               }
               iLGenerator.Emit(OpCodes.Box,typeof(object));
               iLGenerator.Emit(OpCodes.Stloc,indexlocal);

               iLGenerator.Emit(OpCodes.Ldarg_1);
               iLGenerator.Emit(OpCodes.Ldstr,item.Name);
               iLGenerator.Emit(OpCodes.Ldloc,indexlocal);
               iLGenerator.Emit(OpCodes.Callvirt, typeof(BaseProvider).GetMethod("CreateEmitParameter", new Type[] { typeof(string), typeof(object) }));
               iLGenerator.Emit(OpCodes.Stloc, indexlocalDbParameter);

               iLGenerator.Emit(OpCodes.Ldloc_0);
               iLGenerator.Emit(OpCodes.Ldloc, indexlocalDbParameter);
               iLGenerator.Emit(OpCodes.Callvirt, typeof(List<DbParameter>).GetMethod("Add", new Type[] { typeof(DbParameter) }));
            }
            iLGenerator.BeginCatchBlock(typeof(Exception));
            iLGenerator.EmitCall(OpCodes.Call, typeof(SqlMapper).GetMethod("ThrowDataException"), null);
            iLGenerator.EndExceptionBlock();
            iLGenerator.Emit(OpCodes.Ldloc_0);
            iLGenerator.Emit(OpCodes.Ret);
            return (Func<object, BaseProvider, List<DbParameter>>)method.CreateDelegate(typeof(Func<object, BaseProvider, List<DbParameter>>));
        }

        #endregion

        #region 生成更新语句
        public static string CreateUpdateMethod(object model, int updateActionKey)
        {
            if (!handleActionCache.ContainsKey(updateActionKey))
            {
                Type type = model.GetType();
                Func<object, string> arc = CreateUpdateSqlMethodImpl(model);
                if (!handleActionCache.TryAdd(updateActionKey, arc))
                    throw new NullReferenceException();
            }
            return ((Func<object, string>)handleActionCache[updateActionKey])(model);
        }
        public static Func<object, string> CreateUpdateSqlMethodImpl(object model)
        {
            Type type = model.GetType();
            DynamicMethod method = new DynamicMethod(string.Format("Update{0}", Guid.NewGuid()), typeof(string), new Type[] { typeof(object) }, true);
            var iLGenerator = method.GetILGenerator();

            iLGenerator.DeclareLocal(typeof(StringBuilder));
            iLGenerator.Emit(OpCodes.Newobj, typeof(StringBuilder).GetConstructor(new Type[] { }));
            iLGenerator.Emit(OpCodes.Stloc_0);

            DataMap map = Metadata.GetDataMap(model.GetType());

            iLGenerator.Emit(OpCodes.Ldloc_0);
            iLGenerator.Emit(OpCodes.Ldstr, "update " + map.tableName + " set ");
            iLGenerator.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }));
            iLGenerator.Emit(OpCodes.Stloc_0);
            iLGenerator.BeginExceptionBlock();
            var indexlocal = iLGenerator.DeclareLocal(typeof(string));
            var indexlocalInt32 = iLGenerator.DeclareLocal(typeof(Int32));
            foreach (ColumnMap item in (from p in map._properties select (map.GetMember(p.Name))))
            {
                if (!item.isPrimaryKey)
                {
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Unbox_Any, type);
                    iLGenerator.Emit(OpCodes.Callvirt, item.GetMethodInfo);
                    if (item.MemberType == typeof(Int32))
                    {
                        iLGenerator.Emit(OpCodes.Stloc, indexlocalInt32);
                        iLGenerator.Emit(OpCodes.Ldloca_S, indexlocalInt32);
                    }
                    if (item.MemberType == typeof(Boolean))
                    {
                        iLGenerator.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToInt32", new Type[] { typeof(Boolean) }));
                        iLGenerator.Emit(OpCodes.Stloc, indexlocalInt32);
                        iLGenerator.Emit(OpCodes.Ldloca_S, indexlocalInt32);
                    }
                    if (item.MemberType == typeof(Boolean))
                        iLGenerator.Emit(OpCodes.Call, typeof(Int32).GetMethod("ToString", new Type[] { }));
                    else
                        iLGenerator.Emit(OpCodes.Call, item.MemberType.GetMethod("ToString", new Type[] { }));
                    iLGenerator.Emit(OpCodes.Stloc, indexlocal);
                    iLGenerator.Emit(OpCodes.Ldstr, item.MemberName + (item.MemberType==typeof(Int32)||item.MemberType==typeof(Boolean)?"=":"='"));
                    iLGenerator.Emit(OpCodes.Ldloc, indexlocal);
                    iLGenerator.Emit(OpCodes.Ldstr, item.MemberType==typeof(Int32)||item.MemberType==typeof(Boolean)?",":"',");
                    iLGenerator.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new Type[] { typeof(object), typeof(object), typeof(object) }));

                    iLGenerator.Emit(OpCodes.Stloc, indexlocal);

                    iLGenerator.Emit(OpCodes.Ldloc_0);
                    iLGenerator.Emit(OpCodes.Ldloc, indexlocal);
                    iLGenerator.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }));
                    iLGenerator.Emit(OpCodes.Stloc_0);
                }
            }
            iLGenerator.BeginCatchBlock(typeof(Exception));
            iLGenerator.EmitCall(OpCodes.Call, typeof(Check).GetMethod("ThrowDataException"), null);
            iLGenerator.EndExceptionBlock();
            iLGenerator.Emit(OpCodes.Ldloc_0);
            iLGenerator.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("ToString", new Type[] { }));
            iLGenerator.Emit(OpCodes.Call, typeof(SqlMapper).GetMethod("RemoveComma", new Type[] { typeof(string) }));
            iLGenerator.Emit(OpCodes.Ret);
            return (Func<object, string>)method.CreateDelegate(typeof(Func<object, string>));
        }
        public static string RemoveComma(string str)
        {
            return str.Substring(0, str.Length - 1);
        }

        #endregion

        #region 生成插入语句
        public static string CreateInsertMethod(object model, int updateActionKey)
        {
            if (!handleActionCache.ContainsKey(updateActionKey))
            {
                Type type = model.GetType();
                Func<object, string> arc = CreateInsertMethodImpl(model);
                handleActionCache.TryAdd(updateActionKey, arc);
            }
            return ((Func<object, string>)handleActionCache[updateActionKey])(model);
        }
        private static Func<object, string> CreateInsertMethodImpl(object model)
        {
            Type type = model.GetType();
            DynamicMethod method = new DynamicMethod(string.Format("Update{0}", Guid.NewGuid()), typeof(string), new Type[] { typeof(object) }, true);
            var iLGenerator = method.GetILGenerator();

            iLGenerator.DeclareLocal(typeof(StringBuilder));
            iLGenerator.Emit(OpCodes.Newobj, typeof(StringBuilder).GetConstructor(new Type[] { }));
            iLGenerator.Emit(OpCodes.Stloc_0);

            iLGenerator.DeclareLocal(typeof(StringBuilder));
            iLGenerator.Emit(OpCodes.Newobj, typeof(StringBuilder).GetConstructor(new Type[] { }));
            iLGenerator.Emit(OpCodes.Stloc_1);

            DataMap map = Metadata.GetDataMap(model.GetType());

            iLGenerator.BeginExceptionBlock();
            var indexlocal = iLGenerator.DeclareLocal(typeof(string));
            var indexlocalInt32 = iLGenerator.DeclareLocal(typeof(Int32));
            var indexlocalSecond = iLGenerator.DeclareLocal(typeof(string));
            foreach (ColumnMap item in (from p in map._properties select (map.GetMember(p.Name))))
            {
                if (!item.isAutoIncrement)
                {
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Unbox_Any, type);
                    iLGenerator.Emit(OpCodes.Callvirt, item.GetMethodInfo);
                    if (item.MemberType == typeof(Int32))
                    {
                        iLGenerator.Emit(OpCodes.Stloc, indexlocalInt32);
                        iLGenerator.Emit(OpCodes.Ldloca_S, indexlocalInt32);
                    }
                    if (item.MemberType == typeof(Boolean))
                    {
                        iLGenerator.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToInt32", new Type[] { typeof(Boolean) }));
                        iLGenerator.Emit(OpCodes.Stloc, indexlocalInt32);
                        iLGenerator.Emit(OpCodes.Ldloca_S, indexlocalInt32);
                    }
                    if (item.MemberType == typeof(Boolean))
                        iLGenerator.Emit(OpCodes.Call, typeof(Int32).GetMethod("ToString", new Type[] { }));
                    else
                        iLGenerator.Emit(OpCodes.Call, item.MemberType.GetMethod("ToString", new Type[] { }));
                    iLGenerator.Emit(OpCodes.Stloc, indexlocal);

                    iLGenerator.Emit(OpCodes.Ldstr, CreateFormatParamByType(item.MemberType));
                    iLGenerator.Emit(OpCodes.Ldloc, indexlocal);
                    iLGenerator.Emit(OpCodes.Call, typeof(string).GetMethod("Format", new Type[] { typeof(string), item.MemberType }));
                    iLGenerator.Emit(OpCodes.Stloc, indexlocal);

                    iLGenerator.Emit(OpCodes.Ldloc_0);
                    iLGenerator.Emit(OpCodes.Ldloc, indexlocal);
                    iLGenerator.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }));
                    iLGenerator.Emit(OpCodes.Stloc_0);

                    iLGenerator.Emit(OpCodes.Ldstr, "{0},");
                    iLGenerator.Emit(OpCodes.Ldstr, item.MemberName);
                    iLGenerator.Emit(OpCodes.Call, typeof(string).GetMethod("Format", new Type[] { typeof(string), item.MemberType }));
                    iLGenerator.Emit(OpCodes.Stloc, indexlocalSecond);

                    iLGenerator.Emit(OpCodes.Ldloc_1);
                    iLGenerator.Emit(OpCodes.Ldloc, indexlocalSecond);
                    iLGenerator.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }));
                    iLGenerator.Emit(OpCodes.Stloc_1);
                }
            }
            iLGenerator.BeginCatchBlock(typeof(Exception));
            iLGenerator.EmitCall(OpCodes.Call, typeof(Check).GetMethod("ThrowDataException"), null);
            iLGenerator.EndExceptionBlock();
            iLGenerator.Emit(OpCodes.Ldloc_0);
            iLGenerator.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("ToString", new Type[] { }));
            iLGenerator.Emit(OpCodes.Call, typeof(SqlMapper).GetMethod("RemoveComma", new Type[] { typeof(string) }));
            iLGenerator.Emit(OpCodes.Stloc, indexlocal);

            iLGenerator.Emit(OpCodes.Ldloc_1);
            iLGenerator.Emit(OpCodes.Callvirt, typeof(StringBuilder).GetMethod("ToString", new Type[] { }));
            iLGenerator.Emit(OpCodes.Call, typeof(SqlMapper).GetMethod("RemoveComma", new Type[] { typeof(string) }));
            iLGenerator.Emit(OpCodes.Stloc, indexlocalSecond);
            iLGenerator.Emit(OpCodes.Ldstr, "INSERT INTO {0} ({1}) VALUES ({2})");
            iLGenerator.Emit(OpCodes.Ldstr, map.tableName);
            iLGenerator.Emit(OpCodes.Ldloc, indexlocalSecond);
            iLGenerator.Emit(OpCodes.Ldloc, indexlocal);
            iLGenerator.Emit(OpCodes.Call, typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) }));

            iLGenerator.Emit(OpCodes.Ret);
            return (Func<object, string>)method.CreateDelegate(typeof(Func<object, string>));
        }

        public static string CreateFormatParamByType(Type type)
        {
            if (type == typeof(Int32) || type == typeof(Boolean))
                return "{0},";
            else
                return "'{0}',";
        }
        #endregion

        #region 数据键生成
        public static int GetHashKey(string connString, string sqlKey)
        {
            int hashCode = 23;
            unchecked
            {
                hashCode = hashCode * 17 + connString.GetHashCode();
                hashCode = hashCode * 17 + sqlKey.GetHashCode();
            }
            return hashCode;
        }
        public static int GetHashKey(string connString, Type sqlKey)
        {
            int hashCode = 23;
            unchecked
            {
                hashCode = hashCode * 17 + connString.GetHashCode();
                hashCode = hashCode * 17 + sqlKey.GetHashCode();
            }
            return hashCode;
        }
        #endregion

    }
}

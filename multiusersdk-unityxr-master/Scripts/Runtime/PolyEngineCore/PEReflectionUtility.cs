using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Reflection;
using System;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Unity.Collections;
using System.Runtime.InteropServices;
namespace Ximmerse.XR.Reflection
{
    /// <summary>
    /// Polyengine reflection utility 
    /// </summary>
    public static class PEReflectionUtility
    {
        /// <summary>
        /// Copy src object's serializable fields to target object.
        /// </summary>
        /// <param name="srcObj"></param>
        /// <param name="targetObj"></param>
        public static void CopyAndPasteSerializableFields(this System.Object srcObj, System.Object targetObj)
        {
            if (!srcObj.GetType().IsAssignableFrom(targetObj.GetType()))
            {
                Debug.LogErrorFormat("Type: {0} is not assignable from {1}", srcObj.GetType().Name, targetObj.GetType().Name);
                return;
            }

            var fields01 = GetAllSerializableFields(srcObj.GetType());
            var fields02 = GetAllSerializableFields(targetObj.GetType());
            for (int i = 0; i < fields01.Length; i++)
            {
                FieldInfo field01 = fields01[i];
                foreach (var field02 in fields02)
                {
                    if (field01.Name == field02.Name && field01.FieldType == field02.FieldType)
                    {
                        field02.SetValue(targetObj, field01.GetValue(srcObj));
                    }
                }
            }
        }

        /// <summary>
        /// Get the debug string of the method info of the type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static string GetMethodInfoText(this System.Type type, BindingFlags binding = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            StringBuilder buffer1 = new StringBuilder();
            StringBuilder buffer2 = new StringBuilder();
            var methods = type.GetMethods(binding);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];
                buffer2.Clear();
                var Params = m.GetParameters();
                if (Params.Length > 0)
                {
                    for (int j = 0; j < Params.Length; j++)
                    {
                        ParameterInfo param = Params[j];
                        string paramInfo = string.Format("Name: {0}, type: {1}, is In: {2}, is Out: {3}, is Optional: {4}, has Default Value; {5}, Default Value: {6}", param.Name, param.ParameterType, param.IsIn, param.IsOut, param.IsOptional, param.HasDefaultValue, param.DefaultValue);
                        buffer2.AppendLine(paramInfo);
                    }
                }

                //string.Format ("Method: {0}")
                if (buffer2.Length > 0)
                {
                    buffer1.AppendFormattedLine("{0}, is static: {1}, is public: {2}", m, m.IsStatic, m.IsPublic)
                   .AppendLine("--- Parameters Start ---")
                   .Append(buffer2)
                   .AppendLine("--- Parameters End ---").AppendLine();
                }
                else
                {
                    buffer1.AppendFormattedLine("{0}, is static: {1}, is public: {2}", m, m.IsStatic, m.IsPublic)
                    .AppendLine();
                }

            }
            return buffer1.ToString();
        }


        /// <summary>
        /// Get the debug string of the property info of the type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static string GetPropertiesInfoText(this System.Type type, BindingFlags binding = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            StringBuilder buffer1 = new StringBuilder();
            var properties = type.GetProperties(binding);
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo p = properties[i];
                var methodInfos_Get = p.GetGetMethod();
                var methodInfo_Set = p.GetSetMethod();
                buffer1.AppendFormattedLine("{0}, is static: {1}, can read: {2}, can write : {3}", p, p.IsPropertyStatic(), p.CanRead, p.CanWrite);
            }
            return buffer1.ToString();
        }


        /// <summary>
        /// Get the debug string of the field info of the type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static string GetFieldsInfoText(this System.Type type, BindingFlags binding = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            StringBuilder buffer1 = new StringBuilder();
            var fields = type.GetFields(binding);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo f = fields[i];
                buffer1.AppendFormattedLine("{0}, is static: {1}, is public {2}", f, f.IsStatic, f.IsPublic);
            }
            return buffer1.ToString();
        }


        /// <summary>
        /// Return enumerations in array
        /// </summary>
        /// <returns>The enumerations.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T[] GetEnumerations<T>() where T : struct, System.IConvertible
        {
            System.Type targetType = typeof(T);
            if (targetType.IsEnum == false)
            {
                throw new UnityException(string
                    .Format("{0} is not an enum type.", targetType.FullName));
            }
            System.Array array = System.Enum.GetValues(targetType);
            T[] ret = new T[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                ret[i] = (T)array.GetValue(i);
            }
            return ret;
        }

        /// <summary>
        /// 获取在枚举队列中的下一个枚举值。
        /// </summary>
        /// <returns>The enumerations.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T GetNextEnumeration<T>(this T enumVal) where T : struct, IConvertible
        {
            Type targetType = typeof(T);
            if (targetType.IsEnum == false)
            {
                throw new UnityException(string
                    .Format("{0} is not an enum type.", targetType.FullName));
            }
            Array array = Enum.GetValues(targetType);
            for (int i = 0; i < array.Length; i++)
            {
                T enumT = (T)array.GetValue(i);
                if (enumT.Equals(enumVal))
                {
                    if (i == array.Length - 1)
                    {
                        return (T)array.GetValue(0);
                    }
                    else
                    {
                        return (T)array.GetValue(i + 1);
                    }
                }
            }

            return default(T);
        }

        /// <summary>
        /// 获取在枚举队列中的上一个枚举值。
        /// </summary>
        /// <returns>The enumerations.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T GetPrevEnumeration<T>(this T enumVal) where T : struct, IConvertible
        {
            Type targetType = typeof(T);
            if (targetType.IsEnum == false)
            {
                throw new UnityException(string
                    .Format("{0} is not an enum type.", targetType.FullName));
            }
            Array array = Enum.GetValues(targetType);
            for (int i = 0; i < array.Length; i++)
            {
                T enumT = (T)array.GetValue(i);
                if (enumT.Equals(enumVal))
                {
                    if (i == 0)
                    {
                        return (T)array.GetValue(array.Length - 1);
                    }
                    else
                    {
                        return (T)array.GetValue(i - 1);
                    }
                }
            }

            return default(T);
        }

        /// <summary>
        /// Return enumerations in array
        /// </summary>
        /// <returns>The enumerations.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T[] GetEnumerationsExcept<T>(params T[] except) where T : struct, System.IConvertible
        {
            System.Type targetType = typeof(T);
            if (targetType.IsEnum == false)
            {
                throw new UnityException(string
                    .Format("{0} is not an enum type.", targetType.FullName));
            }
            int length = 0;
            System.Array array = System.Enum.GetValues(targetType);
            for (int i = 0; i < array.Length; i++)
            {
                bool exceptThis = false;
                for (int j = 0; j < except.Length; j++)
                {
                    if (except[j].Equals((T)array.GetValue(i)))
                    {
                        exceptThis = true;
                        break;
                    }
                }
                if (!exceptThis)
                {
                    length++;
                }
            }
            T[] ret = new T[length];
            int index = 0;
            for (int i = 0; i < array.Length; i++)
            {
                bool exceptThis = false;
                for (int j = 0; j < except.Length; j++)
                {
                    if (except[j].Equals((T)array.GetValue(i)))
                    {
                        exceptThis = true;
                        break;
                    }
                }
                if (!exceptThis)
                {
                    ret[index++] = (T)array.GetValue(i);
                }
            }

            return ret;
        }

        /// <summary>
        /// Gets the field value.
        /// </summary>
        /// <returns>The field value.</returns>
        /// <param name="_object">Object.</param>
        /// <param name="FieldName">Field name.</param>
        /// <param name="bindingAttribute">binding flag.</param>
        /// <param name="SearchUpwards">If set to <c>true</c> search upwards.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T GetFieldValue<T>(this System.Object _object, string FieldName,
                                          BindingFlags bindingAttribute = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                                          bool SearchUpwards = true)
        {
            System.Type type = _object.GetType();
            do
            {
                var fieldInfo = type.GetField(FieldName, bindingAttribute);
                if (fieldInfo != null)
                {
                    System.Object value = fieldInfo.GetValue(_object);
                    if (value != null && value is T)
                    {
                        return (T)value;
                    }
                    else
                        return default(T);
                }
                else
                {
                    type = type.BaseType;
                }
            }
            while (SearchUpwards && type.BaseType != null && type.BaseType != type);

            return default(T);
        }

        /// <summary>
        /// Sets the field value to the _object .
        /// Return true for setting successfully.
        /// </summary>
        /// <param name="_object">Object.</param>
        /// <param name="FieldName">Field name.</param>
        /// <param name="FieldValue">Field value.</param>
        /// <param name="bindingAttribute">Binding attribute.</param>
        /// <param name="SearchUpwards">If set to <c>true</c> search upwards.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static bool SetFieldValue<T>(this System.Object _object, string FieldName, T FieldValue,
                                             BindingFlags bindingAttribute = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                                             bool SearchUpwards = true)
        {
            System.Type type = _object.GetType();
            do
            {
                var fieldInfo = type.GetField(FieldName, bindingAttribute);
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(_object, FieldValue);
                    return true;
                }
                else
                {
                    type = type.BaseType;
                }
            }
            while (SearchUpwards && type.BaseType != null && type.BaseType != type);
            return false;
        }


        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <returns>The field value.</returns>
        /// <param name="_object">Object.</param>
        /// <param name="PropertyName">Field name.</param>
        /// <param name="bindingAttribute">B flag.</param>
        /// <param name="SearchUpwards">If set to <c>true</c> search upwards.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T GetPropertyValue<T>(this System.Object _object, string PropertyName,
                                             BindingFlags bindingAttribute = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                                             bool SearchUpwards = true)
        {
            System.Type type = _object.GetType();
            do
            {
                var propertyInfo = type.GetProperty(PropertyName, bindingAttribute);
                if (propertyInfo != null && propertyInfo.CanRead)
                {
                    System.Object value = propertyInfo.GetValue(_object, null);
                    if (value != null && value is T)
                    {
                        return (T)value;
                    }
                    else
                        return default(T);
                }
                else
                {
                    type = type.BaseType;
                }
            }
            while (SearchUpwards && type.BaseType != null && type.BaseType != type);

            return default(T);
        }

        /// <summary>
        /// Invoke a method with parameters and cast to type then return.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static T InvokeMethod<T>(this MethodInfo method, string target, params object[] param)
        {
            var obj = method.Invoke(target, param);
            if (obj != null)
            {
                return (T)obj;
            }
            return default(T);
        }

        /// <summary>
        /// Invoke a method with parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static void InvokeMethod(this MethodInfo method, string target, params object[] param)
        {
            method.Invoke(target, param);
        }

        public static bool IsFieldContainsAttribute<T>(FieldInfo fieldInfo) where T : System.Attribute
        {
            object[] attributes = fieldInfo.GetCustomAttributes(true);
            for (int i = 0; i < attributes.Length; i++)
            {
                object item = attributes[i];
                if (item is T)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Is the property info declared as static ?
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static bool IsPropertyStatic(this PropertyInfo propertyInfo)
        {
            try
            {
                var mRead = propertyInfo.GetGetMethod(true);
                var mWrite = propertyInfo.GetSetMethod(true);
                return propertyInfo.CanRead ? mRead.IsStatic : mWrite.IsStatic;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <returns>The field value.</returns>
        /// <param name="_object">Object.</param>
        /// <param name="PropertyName">Field name.</param>
        /// <param name="bindingAttribute">B flag.</param>
        /// <param name="SearchUpwards">If set to <c>true</c> search upwards.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static bool SetPropertyValue<T>(this System.Object _object, string PropertyName, T Value,
                                                BindingFlags bindingAttribute = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                                                bool SearchUpwards = true)
        {
            System.Type type = _object.GetType();
            do
            {
                var propertyInfo = type.GetProperty(PropertyName, bindingAttribute);
                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(_object, Value, null);
                    return true;
                }
                else
                {
                    type = type.BaseType;
                }
            }
            while (SearchUpwards && type.BaseType != null && type.BaseType != type);
            return false;
        }


        /// <summary>
        /// Searchs the method in the given type and its base type.
        /// </summary>
        /// <returns>The method upwards.</returns>
        /// <param name="MethodName">Method name.</param>
        public static System.Reflection.MethodInfo SearchMethodUpwards(this System.Type type,
                                                                        string MethodName,
                                                                        System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
        {
            var searchType = type;
            System.Reflection.MethodInfo method = null;
            do
            {
                method = searchType.GetMethod(MethodName, bindingFlags);
                if (method != null)
                {
                    return method;
                }
                else
                {
                    searchType = searchType.BaseType;
                    if (searchType.BaseType == null || searchType.BaseType == searchType)
                    {
                        break;
                    }
                }
            }
            while (searchType != null);
            return default(MethodInfo);
        }

        /// <summary>
        /// Searchs the property in the given type. If the property is not found ,will try searching upwards along base type.
        /// </summary>
        /// <returns>The property.</returns>
        /// <param name="type">Type.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="bindingFlags">Binding flags.</param>
        public static System.Reflection.PropertyInfo SearchProperty(this System.Type type, string propertyName,
            System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
        {
            var searchType = type;
            System.Reflection.PropertyInfo propertyInfo = null;
            do
            {
                propertyInfo = searchType.GetProperty(propertyName, bindingFlags);
                if (propertyInfo != null)
                {
                    return propertyInfo;
                }
                else
                {
                    searchType = searchType.BaseType;
                    if (searchType.BaseType == null || searchType.BaseType == searchType)
                    {
                        break;
                    }
                }
            }
            while (searchType != null);
            return default(PropertyInfo);
        }

        /// <summary>
        /// Searchs the field in the given type. If the field is not found ,will try searching upwards along base type.
        /// </summary>
        /// <returns>The property.</returns>
        /// <param name="type">Type.</param>
        /// <param name="fieldName">Property name.</param>
        /// <param name="bindingFlags">Binding flags.</param>
        public static System.Reflection.FieldInfo SearchField(this System.Type type, string fieldName,
            System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
        {
            var searchType = type;
            System.Reflection.FieldInfo fieldInfo = null;
            do
            {
                fieldInfo = searchType.GetField(fieldName, bindingFlags);
                if (fieldInfo != null)
                {
                    return fieldInfo;
                }
                else
                {
                    searchType = searchType.BaseType;
                    if (searchType.BaseType == null || searchType.BaseType == searchType)
                    {
                        break;
                    }
                }
            }
            while (searchType != null);
            return default(FieldInfo);
        }

        /// <summary>
        /// 给出 type, 查找所有类型等于（或者是 isAssignableFrom) 目标类型的字段。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fields"></param>
        /// <param name="restrictMatch">要求字段类型严格匹配， 否则就是使用 IsAssignableFrom </param>
        /// <param name="FilterFunc">过滤方法， 返回true代表此field info被保留， 返回false 代表抛弃。</param>
        /// <param name="bindingFlags"></param>
        /// <returns></returns>
        public static void SearchFieldsOfType<T>(this Type type, List<FieldInfo> fields, bool restrictMatch = false,
            Func<FieldInfo, bool> FilterFunc = null,
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var targetType = typeof(T);
            fields.Clear();
            type.GetAllFields(fields, bindingFlags);
            for (int i = fields.Count - 1; i >= 0; i--)
            {
                var field = fields[i];
                if (restrictMatch == false)
                {
                    if (!targetType.IsAssignableFrom(field.FieldType))
                    {
                        fields.RemoveAt(i);
                    }
                }
                else
                {
                    if (!(field.FieldType == (targetType)))
                    {
                        fields.RemoveAt(i);
                    }
                }
            }

            //过滤方法:
            if (FilterFunc != null)
            {
                for (int i = fields.Count - 1; i >= 0; i--)
                {
                    if (!FilterFunc(fields[i]))
                    {
                        fields.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Search for types that state with attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeList"></param>
        /// <returns>The search result count.</returns>
        public static int SearchForTypesWithAttribute<T>(IList<Type> typeList) where T : Attribute
        {
            var domain = AppDomain.CurrentDomain;
            Assembly[] assemblies = domain.GetAssemblies();
            int searchCount = 0;
            for (int i = 0; i < assemblies.Length; i++)
            {
                try
                {
                    var assembly = assemblies[i];
                    var types = assembly.GetTypes();
                    for (int j = 0; j < types.Length; j++)
                    {
                        var type = types[j];
                        System.Object[] attributes = type.GetCustomAttributes(true);
                        for (int k = 0, attributesLength = attributes.Length; k < attributesLength; k++)
                        {
                            var item = attributes[k];
                            if (item.GetType() == typeof(T))
                            {
                                typeList.Add(type);
                                searchCount++;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

            }
            return searchCount;
        }

        /// <summary>
        /// Search for member info with custom attribute attached.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="memberInfos">用于接收输出的对象。</param>
        /// <param name="inherit">是否搜索基类型的成员.</param>
        /// <returns>The search result count.</returns>
        public static int SearchForMembersWithAttribute<T>(this Type type, IList<MemberInfo> memberInfos, bool inherit = false, BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance) where T : Attribute
        {
            int searchCount = 0;
            MemberInfo[] members = type.GetMembers();
            for (int i = 0, membersLength = members.Length; i < membersLength; i++)
            {
                var member = members[i];
                if (member.HasAttribute<T>())
                {
                    memberInfos.Add(member);
                    searchCount++;
                }
            }
            if (inherit && type != typeof(System.Object))
            {
                type = type.BaseType;
                searchCount += SearchForMembersWithAttribute<T>(type, memberInfos, true, bindingFlags);
            }
            return searchCount;
        }

        /// <summary>
        /// 判断 field info 是否有自定义属性标签
        /// </summary>
        /// <returns><c>true</c> if has attribute the specified field info; otherwise, <c>false</c>.</returns>
        /// <param name="fieldInfo">Field info.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static bool HasAttribute<T>(this System.Reflection.FieldInfo fieldInfo) where T : System.Attribute
        {
            if (fieldInfo == null)
            {
                return false;
            }
            var attrs = fieldInfo.GetCustomAttributes(true);
            for (int i = 0; i < attrs.Length; i++)
            {
                object att = attrs[i];
                if (att.GetType().IsAssignableFrom(typeof(T)))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断 field info 是否有自定义属性标签
        /// </summary>
        /// <returns><c>true</c> if has attribute the specified field info; otherwise, <c>false</c>.</returns>
        /// <param name="memberInfo">Field info.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static bool HasAttribute<T>(this MemberInfo memberInfo, out T attribute) where T : System.Attribute
        {
            attribute = null;
            if (memberInfo == null)
            {
                return false;
            }
            var attrs = memberInfo.GetCustomAttributes(true);
            for (int i = 0; i < attrs.Length; i++)
            {
                object att = attrs[i];
                if (att.GetType().IsAssignableFrom(typeof(T)))
                {
                    attribute = (T)att;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断 member info 是否有自定义属性标签
        /// </summary>
        /// <returns><c>true</c> if has attribute the specified memeberInfo; otherwise, <c>false</c>.</returns>
        /// <param name="memeberInfo">Memeber info.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static bool HasAttribute<T>(this MemberInfo memeberInfo) where T : System.Attribute
        {
            if (memeberInfo == null)
            {
                return false;
            }
            var attrs = memeberInfo.GetCustomAttributes(true);
            for (int i = 0; i < attrs.Length; i++)
            {
                object att = attrs[i];
                if (att.GetType().IsAssignableFrom(typeof(T)))
                {
                    return true;
                }
            }
            return false;
        }



        /// <summary>
        /// 判断 member info 是否有自定义属性标签.
        /// Warning : Mass memory allocation.
        /// Use HasCustomAttribute for less memory allocation.
        /// </summary>
        /// <returns><c>true</c> if has attribute the specified memeberInfo; otherwise, <c>false</c>.</returns>
        /// <param name="type">Type info.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static bool HasAttribute<T>(this Type type) where T : System.Attribute
        {
            if (type == null)
            {
                return false;
            }
            var attrs = type.GetCustomAttributes(true);
            for (int i = 0; i < attrs.Length; i++)
            {
                object att = attrs[i];
                if (att.GetType().IsAssignableFrom(typeof(T)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If the type has custom attribute ? Only works for custom attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool HasCustomAttribute<T>(this Type type) where T : System.Attribute
        {
            if (type == null)
            {
                return false;
            }
            var attrs = type.CustomAttributes;
            foreach (var att in attrs)
            {
                if (att.AttributeType.IsAssignableFrom(typeof(T)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断 member info 是否有自定义属性标签
        /// </summary>
        /// <returns><c>true</c> if has attribute the specified memeberInfo; otherwise, <c>false</c>.</returns>
        /// <param name="type">Type info.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static bool HasAttribute<T>(this Type type, out T attribute) where T : System.Attribute
        {
            attribute = null;
            if (type == null)
            {
                return false;
            }
            var attrs = type.GetCustomAttributes(true);
            for (int i = 0; i < attrs.Length; i++)
            {
                object att = attrs[i];
                if (att.GetType().IsAssignableFrom(typeof(T)))
                {
                    attribute = (T)att;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets all fields - include base type.
        /// 获取所有的字段 （包括BaseType)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fields"></param>
        /// <returns>Field count</returns>
        public static int GetAllFields(this Type type, IList<FieldInfo> fields,
            BindingFlags bindingFlags = (BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
        {
            var t = type;
            int typeCount = 0;
            while (true)
            {
                var fieldInfos = t.GetFields(bindingFlags);
                if (fieldInfos.Length > 0)
                {
                    fields.AddMany(fieldInfos);
                    typeCount += fieldInfos.Length;
                }
                if (t == typeof(System.Object))
                {
                    break;
                }
                t = t.BaseType;
            }
            return typeCount;
        }

        /// <summary>
        /// Finds the type in given fullname, case sensitive..
        /// </summary>
        /// <returns>The type.</returns>
        public static System.Type SearchTypeByName(string fullTypeName)
        {
            var domain = System.AppDomain.CurrentDomain;
            Assembly[] assemblies = domain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                try
                {
                    var assembly = assemblies[i];
                    var types = assembly.GetTypes();
                    for (int j = 0; j < types.Length; j++)
                    {
                        var type = types[j];
                        if (type.FullName == fullTypeName)
                        {
                            return type;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }

            }
            return default(System.Type);
        }

        /// <summary>
        /// Finds the type in given full name and full assembly name, case sensitive.
        /// </summary>
        /// <param name="fullTypeName"></param>
        /// <param name="fullAssemblyName"></param>
        /// <returns></returns>
        public static Type SearchTypeByName(string fullTypeName, string fullAssemblyName)
        {
            var domain = System.AppDomain.CurrentDomain;
            Assembly[] assemblies = domain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                try
                {
                    var assembly = assemblies[i];
                    if (assembly.FullName == fullAssemblyName)
                    {
                        var types = assembly.GetTypes();
                        for (int j = 0; j < types.Length; j++)
                        {
                            var type = types[j];
                            if (type.FullName == fullTypeName)
                            {
                                return type;
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }

            }
            return default(System.Type);
        }

        /// <summary>
        /// 查找所有继承了 baseType 的子类型。
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="subTypes"></param>
        public static void SearchForChildrenTypes(System.Type baseType, IList<System.Type> subTypes)
        {
            if (subTypes.IsReadOnly)
            {
                Debug.LogErrorFormat("SearchForChildrenTypes: {0} the list is readonly !", baseType.Name);
                return;
            }
            subTypes.Clear();
            var domain = AppDomain.CurrentDomain;
            Assembly[] assemblies = domain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                try
                {
                    var types = assembly.GetTypes();
                    for (int j = 0; j < types.Length; j++)
                    {
                        var type = types[j];
                        if (type != baseType && baseType.IsAssignableFrom(type))
                        {
                            subTypes.Add(type);
                        }
                    }
                }
                catch (System.Exception exc)
                {
                    Debug.LogErrorFormat("Error search types on assembly: {0}", assembly.FullName);
                    Debug.LogException(exc);
                }
            }
        }

        /// <summary>
        /// Finds all children types to the target type.
        /// Return true if found.
        /// If OnlyProjectScripts = true, search only the scripts presents in this project. 
        /// Else search for all DLL loaded.
        /// </summary>
        /// <param name="baseType">基础类</param>
        /// <param name="typeList">输出类型列表</param>
        public static bool FindAllAssignableTypes(Type baseType, IList<Type> typeList)
        {
            if (typeList.IsReadOnly)
            {
                Debug.LogErrorFormat("FindAllAssignableTypes: {0} the list is readonly !", baseType.Name);
                return false;
            }
            typeList.Clear();
            var domain = AppDomain.CurrentDomain;
            Assembly[] assemblies = domain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                try
                {
                    var assembly = assemblies[i];
                    var types = assembly.GetTypes();
                    for (int j = 0; j < types.Length; j++)
                    {
                        var type = types[j];
                        if (baseType.IsAssignableFrom(type))
                        {
                            typeList.Add(type);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }
            return typeList.Count > 0;
        }

        /// <summary>
        /// Get attribute of type from class type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this System.Type type, bool inherit = false) where T : System.Attribute
        {
            object[] attrs = type.GetCustomAttributes(inherit);
            for (int i = 0; i < attrs.Length; i++)
            {
                object item = attrs[i];
                if (item.GetType() == typeof(T))
                {
                    return (T)item;
                }
            }
            return null;
        }

        /// <summary>
        /// Get attribute of type from class type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this FieldInfo field, bool inherit = false) where T : System.Attribute
        {
            object[] attrs = field.GetCustomAttributes(inherit);
            for (int i = 0; i < attrs.Length; i++)
            {
                object item = attrs[i];
                if (item.GetType() == typeof(T))
                {
                    return (T)item;
                }
            }
            return null;
        }

        /// <summary>
        /// Return the fields in array which has target attribute attach to.
        /// </summary>
        /// <returns>The fields with attribute.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static FieldInfo[] GetFieldsWithAttribute<T>(this System.Type type,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) where T : System.Attribute
        {
            FieldInfo[] fields = type.GetFields(flags);
            List<FieldInfo> ret = new List<FieldInfo>();
            for (int i = 0, fieldsLength = fields.Length; i < fieldsLength; i++)
            {
                FieldInfo field = fields[i];
                var attrs = field.GetCustomAttributes(true);
                for (int i1 = 0; i1 < attrs.Length; i1++)
                {
                    object attr = attrs[i1];
                    if (attr.GetType() == typeof(T) || attr.GetType().IsAssignableFrom(typeof(T)))
                    {
                        ret.Add(field);
                    }
                }
            }
            return ret.ToArray();
        }

        /// <summary>
        /// Return the properties in array which has target attribute attach to.
        /// </summary>
        /// <returns>The fields with attribute.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static PropertyInfo[] GetPropertiesWithAttribute<T>(this System.Type type,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) where T : System.Attribute
        {
            PropertyInfo[] props = type.GetProperties(flags);
            List<PropertyInfo> ret = new List<PropertyInfo>();
            for (int i = 0, length = props.Length; i < length; i++)
            {
                PropertyInfo property = props[i];
                var attrs = property.GetCustomAttributes(true);
                for (int i1 = 0; i1 < attrs.Length; i1++)
                {
                    object attr = attrs[i1];
                    if (attr.GetType() == typeof(T) || attr.GetType().IsAssignableFrom(typeof(T)))
                    {
                        ret.Add(property);
                    }
                }
            }
            return ret.ToArray();
        }

        static List<FieldInfo> sSerializeFieldCollect = new List<FieldInfo>();

        /// <summary>
        /// Gets all serializable fields.
        /// </summary>
        /// <returns>The all serializable fields.</returns>
        /// <param name="type">Target type.</param>
        public static FieldInfo[] GetAllSerializableFields(this System.Type type)
        {
            sSerializeFieldCollect.Clear();
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            System.Type targetType = type;
            while (targetType != null)
            {
                FieldInfo[] fields = targetType.GetFields(flags);
                foreach (var item in fields)
                {
                    if (item.DeclaringType != targetType)
                    {
                        continue;
                    }
                    if (IsFieldSerializable(item))
                    {
                        //Debug.LogFormat("Field: {0} declared in : {1}", item.Name, item.DeclaringType.Name);
                        sSerializeFieldCollect.AddUnduplicate(item);
                    }
                }

                targetType = targetType.BaseType;
                if (targetType == typeof(UnityEngine.Object) || targetType == typeof(System.Object))
                {
                    break;
                }
            }
            return sSerializeFieldCollect.ToArray();
        }

        /// <summary>
        /// Gets all serializable fields.
        /// </summary>
        /// <returns>The all serializable fields.</returns>
        /// <param name="type">Target type.</param>
        public static void GetAllSerializableFields(this System.Type type, IList<FieldInfo> fieldList)
        {
            if (fieldList.IsReadOnly)
            {
                Debug.LogErrorFormat("GetAllSerializableFields: {0} the list is readonly !", type.Name);
                return;
            }
            fieldList.Clear();
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            System.Type targetType = type;
            while (targetType != null)
            {
                FieldInfo[] fields = targetType.GetFields(flags);
                foreach (var item in fields)
                {
                    if (item.DeclaringType != targetType)
                    {
                        continue;
                    }
                    if (IsFieldSerializable(item))
                    {
                        fieldList.AddUnduplicate(item);
                    }
                }

                targetType = targetType.BaseType;
                if (targetType == typeof(UnityEngine.Object) || targetType == typeof(System.Object))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Is the field serializable.
        /// </summary>
        /// <returns><c>true</c>, if field serializable was ised, <c>false</c> otherwise.</returns>
        /// <param name="field">Field.</param>
        static bool IsFieldSerializable(FieldInfo field)
        {
            if (field.IsPublic)
            {
                return !IsFieldContainsAttribute<System.NonSerializedAttribute>(field);
            }
            else
            {
                return IsFieldContainsAttribute<SerializeField>(field);
            }
        }

        /// <summary>
        /// is the type a static type ?
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsStatic(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) == null && type.IsAbstract && type.IsSealed;
        }


    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CSharp.Utils.Reflection.Internal;

namespace CSharp.Utils.Reflection
{
    public static class ReflectionHelper
    {
        #region Public Methods and Operators

        public static PropertyInfo[] GetAllInstancePropertiesOfClass(Type type)
        {
            if (!type.IsClass)
            {
                throw new ArgumentException("type must be a class", "type");
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Array.Sort(properties, ColumnOrderComparer.Instance);
            return properties;
        }

        public static object ConvertUsingConvertTo(Type type, string s)
        {
            object convertedValue = null;
            if (!type.IsEnum)
            {
                if ((type == typeof(long) || type == typeof(int)) && char.ToUpper(s[s.Length - 1]) == 'B')
                {
                    try
                    {
                        long l = long.Parse(s.Substring(0, s.Length - 2));
                        string size = s.Substring(s.Length - 2).ToUpper();
                        bool isMatched = false;
                        switch (size)
                        {
                            case "KB":
                                l = l * 1024;
                                isMatched = true;
                                break;

                            case "MB":
                                l = l * 1024 * 1024;
                                isMatched = true;
                                break;

                            case "GB":
                                l = l * 1024 * 1024 * 1024;
                                isMatched = true;
                                break;
                        }

                        if (isMatched)
                        {
                            if (type == typeof(int))
                            {
                                convertedValue = (int)l;
                            }
                            else
                            {
                                convertedValue = l;
                            }

                            return convertedValue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }

                MethodInfo method;
                if (SharedReflectionInfo.ConvertToMethods.TryGetValue(type, out method))
                {
                    try
                    {
                        convertedValue = method.Invoke(null, new object[] { s });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
                else
                {
                    MethodInfo mi = type.GetMethod("Parse");

                    if (mi != null && mi.IsStatic && mi.ReflectedType == type)
                    {
                        ParameterInfo[] parameters = mi.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(String))
                        {
                            SharedReflectionInfo.ConvertToMethods.Add(mi.ReturnType, mi);
                            return mi.Invoke(null, new object[] { s });
                        }
                    }
                }
            }
            else
            {
                convertedValue = Enum.Parse(type, s);
            }

            return convertedValue;
        }

        public static bool CreateListIfPropertyTypeIsACollection(Type type, out Type listType, out Type[] genericparameters)
        {
            genericparameters = null;
            listType = null;
            if (typeof(ICollection).IsAssignableFrom(type))
            {
                if (type.IsGenericType)
                {
                    Type[] generics = type.GetGenericArguments();
                    Type t = typeof(List<>).MakeGenericType(generics);
                    if (type.IsAssignableFrom(t))
                    {
                        genericparameters = generics;
                        listType = t;
                        return true;
                    }
                }
                else
                {
                    Type t = typeof(List<object>);
                    if (type.IsAssignableFrom(t))
                    {
                        listType = t;
                        return true;
                    }
                }

                listType = type;
                genericparameters = new[] { typeof(object) };
                return true;
            }

            return false;
        }

        public static T CreateOrGetObject<T>(string typeName) where T : class
        {
            bool isSingleton = false;
            Type type = null;
            return CreateOrGetObject<T>(typeName, out isSingleton, out type);
        }

        public static T CreateOrGetObject<T>(string typeName, out bool isSingleton, out Type type) where T : class
        {
            isSingleton = false;
            type = GetType(typeName);
            if (type == null)
            {
                return null;
            }

            return CreateOrGetObject<T>(type, out isSingleton);
        }

        public static T CreateOrGetObject<T>(Type t, out bool isSingleton) where T : class
        {
            isSingleton = false;
            if (t != null)
            {
                object[] attributes = t.GetCustomAttributes(typeof(SingletonAttribute), false);
                if (attributes.Length == 0)
                {
                    ConstructorInfo ci = t.GetConstructor(Type.EmptyTypes);
                    if (ci != null && !ci.IsStatic)
                    {
                        if (ci.IsPublic)
                        {
                            return Activator.CreateInstance(t) as T;
                        }
                    }
                    else
                    {
                        object o = tryGettingSingleton(t, "Instance");
                        if (o != null)
                        {
                            isSingleton = true;
                            return o as T;
                        }

                        try
                        {
                            return Activator.CreateInstance(t, true) as T;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }

                        return null;
                    }
                }
                else
                {
                    var att = (SingletonAttribute)attributes[0];
                    object o = tryGettingSingleton(t, att.InstancePropertyName);
                    if (o != null)
                    {
                        isSingleton = true;
                        return o as T;
                    }
                }
            }

            return null;
        }

        public static IEnumerable<TTo> DoCopyConvert<TFrom, TTo>(this IEnumerable<TFrom> items) where TTo : new()
        {
            foreach (TFrom v in items)
            {
                var t = new TTo();
                DynamicCopyHelper<TFrom, TTo>.Copy(v, t);
                yield return t;
            }
        }

        public static List<PropertyInfo> GetAllPropertiesOfInterfaceHierarchy(Type interfaceType)
        {
            var properties = new List<PropertyInfo>();
            getAllPropertiesOfInterfaceHierarchy(interfaceType, properties);
            return properties;
        }

        public static T GetEffectiveAttribute<T>(Type type, bool considerInterfaces) where T : Attribute
        {
            if (Attribute.IsDefined(type, typeof(T), true))
            {
            }

            return null;
        }

        public static List<KeyValuePair<PropertyInfo, T>> GetMandatoryPropertyCustomAttributes<T>(Type t, bool isInherit) where T : Attribute
        {
            PropertyInfo[] properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            Array.Sort(properties, ColumnOrderComparer.Instance);
            return (from property in properties let customAttributes = property.GetCustomAttributes(typeof(T), isInherit) where customAttributes.Length > 0 let attribute = customAttributes[0] as T select new KeyValuePair<PropertyInfo, T>(property, attribute)).ToList();
        }

        public static object GetPropertyValue(object obj, string propertyName)
        {
            return GetPropertyValue(obj, propertyName, null);
        }

        public static object GetPropertyValue(object obj, string propertyName, object[] indexes)
        {
            Type type = obj.GetType();
            PropertyInfo pi = type.GetProperty(propertyName);
            if (pi != null)
            {
                return pi.GetValue(obj, indexes);
            }

            return null;
        }

        public static T GetPropertyValue<T>(object obj, string propertyName)
        {
            return GetPropertyValue<T>(obj, propertyName, null);
        }

        public static T GetPropertyValue<T>(object obj, string propertyName, object[] indexes)
        {
            return (T)GetPropertyValue(obj, propertyName, indexes);
        }

        public static List<PropertyInfo> GetPublicProperties<T>(bool sortOnPropertyOrder)
        {
            return GetPublicProperties(typeof(T), sortOnPropertyOrder);
        }

        public static List<PropertyInfo> GetPublicProperties(Type type, bool sortOnPropertyOrder)
        {
            if (type.IsInterface)
            {
                var propertyInfos = new List<PropertyInfo>();

                var considered = new List<Type>();
                var queue = new Queue<Type>();
                considered.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    Type subType = queue.Dequeue();
                    foreach (Type subInterface in subType.GetInterfaces())
                    {
                        if (considered.Contains(subInterface))
                        {
                            continue;
                        }

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

                    PropertyInfo[] typeProperties = subType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);

                    List<PropertyInfo> newPropertyInfos = typeProperties.Where(propertyInfos.Contains).ToList();

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                if (sortOnPropertyOrder)
                {
                    propertyInfos.Sort(ColumnOrderComparer.Instance);
                }

                return propertyInfos;
            }

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            var dictionary = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo property in properties)
            {
                PropertyInfo p;
                if (dictionary.TryGetValue(property.Name, out p))
                {
                    if (p.DeclaringType.IsAssignableFrom(property.DeclaringType))
                    {
                        dictionary[property.Name] = property;
                    }
                }
                else
                {
                    dictionary.Add(property.Name, property);
                }
            }

            List<PropertyInfo> v = dictionary.Values.ToList();
            if (sortOnPropertyOrder)
            {
                v.Sort(ColumnOrderComparer.Instance);
            }

            return v;
        }

        public static Type GetType(string typeName, bool reflectionOnly = false)
        {
            Type t = Type.GetType(typeName);
            if (t != null)
            {
                return t;
            }

            var parser = new TypeParser(typeName);

            if (parser.FullyQualifiedTypeName != null)
            {
                Assembly assembly = null;
                try
                {
                    assembly = LoadAssembly(parser.AssemblyDescriptionString, reflectionOnly);
                }
                catch (Exception ex)
                {
                    assembly = LoadAssemblyFromFileByShortAssemblyName(parser.ShortAssemblyName, reflectionOnly);
                    Debug.WriteLine(ex.ToString());
                }

                if (assembly != null)
                {
                    t = assembly.GetType(parser.ShortTypeName);

                    if (t != null)
                    {
                        return t;
                    }
                }
            }

            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = a.GetType(parser.FullyQualifiedTypeName);
                if (t != null)
                {
                    return t;
                }
            }

            return t;
        }

        public static bool IsNumericType(this Type type)
        {
            if (type == null)
            {
                return false;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;

                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        return IsNumericType(Nullable.GetUnderlyingType(type));
                    }

                    return false;
            }

            return false;
        }

        public static List<Assembly> LoadAssemblies(string assembliesDirectory)
        {
            string[] files = Directory.GetFiles(assembliesDirectory, "*.dll", SearchOption.AllDirectories);
            return files.Select(Assembly.LoadFile).ToList();
        }

        public static Assembly LoadAssembly(string fullyQualifiedAssemblyName, bool reflectionOnly = false)
        {
            Assembly assembly = null;
            if (reflectionOnly)
            {
                assembly = Assembly.ReflectionOnlyLoad(fullyQualifiedAssemblyName);
            }

            return assembly;
        }

        public static Assembly LoadAssemblyFromFile(string fileName, bool reflectionOnly = false)
        {
            if (File.Exists(fileName))
            {
                return reflectionOnly ? Assembly.ReflectionOnlyLoadFrom(fileName) : Assembly.LoadFrom(fileName);
            }

            return null;
        }

        public static Assembly LoadAssemblyFromFileByShortAssemblyName(string shortAssemblyName, bool reflectionOnly = false)
        {
            string fileFormat = @"{0}\" + shortAssemblyName + ".dll";
            string dllFileName = string.Format(fileFormat, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            Assembly assembly = LoadAssemblyFromFile(dllFileName, reflectionOnly);
            if (assembly != null)
            {
                return assembly;
            }

            dllFileName = string.Format(fileFormat, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"bin");
            assembly = LoadAssemblyFromFile(dllFileName, reflectionOnly);
            if (assembly != null)
            {
                return assembly;
            }

            dllFileName = string.Format(fileFormat, AppDomain.CurrentDomain.BaseDirectory);
            assembly = LoadAssemblyFromFile(dllFileName, reflectionOnly);
            if (assembly != null)
            {
                return assembly;
            }

            dllFileName = string.Format(fileFormat, AppDomain.CurrentDomain.BaseDirectory + @"bin");
            assembly = LoadAssemblyFromFile(dllFileName, reflectionOnly);
            if (assembly != null)
            {
                return assembly;
            }

            dllFileName = string.Format(fileFormat, AppDomain.CurrentDomain.BaseDirectory + @"bin\CodeShare");
            assembly = LoadAssemblyFromFile(dllFileName, reflectionOnly);
            return assembly;
        }

        [CautionUsedByReflection]
        public static object ParseEnum<T>(string s)
        {
            return Enum.Parse(typeof(T), s);
        }

        public static void SetPropertyValue(object obj, string propertyName, object value)
        {
            SetPropertyValue(obj, propertyName, value, null);
        }

        public static void SetPropertyValue(object obj, string propertyName, object value, object[] indexes)
        {
            Type type = obj.GetType();
            PropertyInfo pi = type.GetProperty(propertyName);
            if (pi != null)
            {
                pi.SetValue(obj, value, indexes);
            }
        }

        public static bool TryConvertToValueTypeOrStringOrType(Type type, string s, out object value)
        {
            if (type == typeof(Type))
            {
                value = Type.GetType(s);
                return true;
            }

            if (type == typeof(string))
            {
                value = s;
                return true;
            }

            if (s != null && type == typeof(char))
            {
                switch (s.ToUpper())
                {
                    case "TAB":
                        value = '\t';
                        return true;

                    case "SPACE":
                        value = ' ';
                        return true;

                    case "NEWLINE":
                        value = '\n';
                        return true;
                }
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                value = ConvertUsingConvertTo(type.GetGenericArguments()[0], s);
                if (value != null)
                {
                    return true;
                }
            }

            if (type.IsValueType)
            {
                value = ConvertUsingConvertTo(type, s);
                if (value != null)
                {
                    return true;
                }
            }
            else
            {
                return TryParseToStaticPropertyValue(s, out value);
            }

            return false;
        }

        public static bool TryParseToStaticPropertyValue(string s, out object value)
        {
            value = null;
            string[] splits = s.Split(new[] { ',' });
            if (splits.Length == 3)
            {
                string s1 = splits[0] + ", " + splits[1];
                try
                {
                    Type staticType = Type.GetType(s1);
                    if (staticType != null)
                    {
                        PropertyInfo p = staticType.GetProperty(splits[2].Trim());
                        if (p != null)
                        {
                            value = p.GetValue(null, null);
                            return true;
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

            return false;
        }

        #endregion Public Methods and Operators

        #region Methods

        private static void getAllPropertiesOfInterfaceHierarchy(Type interfaceType, List<PropertyInfo> properties)
        {
            PropertyInfo[] props = interfaceType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            properties.AddRange(props);
            Type[] interfacetypes = interfaceType.GetInterfaces();
            foreach (Type t in interfacetypes)
            {
                getAllPropertiesOfInterfaceHierarchy(t, properties);
            }
        }

        public static TAttribute GetAttributeFromEnum<TAttribute>(this Enum value)
        where TAttribute : Attribute
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            return type.GetField(name)
                .GetCustomAttributes(false)
                .OfType<TAttribute>()
                .SingleOrDefault();
        }

        private static object tryGettingSingleton(Type t, string instancePropertyName)
        {
            PropertyInfo property = t.GetProperty(instancePropertyName);
            if (property != null && property.PropertyType == t)
            {
                return property.GetValue(null, null);
            }

            return null;
        }

        public static List<T> GetAttributes<T>(MethodInfo method, bool inherit, bool checkInterfaces)
            where T : Attribute
        {
            var attributeType = typeof (T);
            var customAttributes = method.GetCustomAttributes(attributeType, inherit);
            var list = new List<T>();
            Copy(list, customAttributes);

            if (!checkInterfaces)
            {
                return list;
            }

            var interfaces = method.DeclaringType.GetInterfaces();
            foreach (var iface in interfaces)
            {
                var interfaceMap = method.ReflectedType.GetInterfaceMap(iface);
                var targetMethods = interfaceMap.TargetMethods;
                if (targetMethods.Contains(method))
                {
                    var interfaceMethods = interfaceMap.InterfaceMethods;

                    foreach (var im in interfaceMethods)
                    {
                        if (im.Name == method.Name)
                        {
                            if (DoesSignatureEqual(im, method))
                            {
                                customAttributes = im.GetCustomAttributes(attributeType, inherit);
                                Copy(list, customAttributes);
                                break;
                            }
                        }
                    }
                }
            }

            return list;
        }

        public static bool IsDefined<T>(MethodInfo method, bool inherit, bool checkInterfaces)
            where T : Attribute
        {
            var attributeType = typeof(T);
            var customAttributes = method.GetCustomAttributes(attributeType, inherit);
            if (customAttributes.Length > 0)
            {
                return true;
            }

            if (!checkInterfaces)
            {
                return false;
            }

            var interfaces = method.DeclaringType.GetInterfaces();
            foreach (var iface in interfaces)
            {
                var interfaceMap = method.ReflectedType.GetInterfaceMap(iface);
                var targetMethods = interfaceMap.TargetMethods;
                if (targetMethods.Contains(method))
                {
                    var interfaceMethods = interfaceMap.InterfaceMethods;

                    foreach (var im in interfaceMethods)
                    {
                        if (im.Name == method.Name)
                        {
                            if (DoesSignatureEqual(im, method))
                            {
                                customAttributes = im.GetCustomAttributes(attributeType, inherit);
                                if (customAttributes.Length > 0)
                                {
                                    return true;
                                }

                                break;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static void Copy<T>(IList<T> list, IEnumerable attributes)
        {
            foreach (var v in attributes)
            {
                list.Add((T)v);
            }
        }

        public static bool DoesSignatureEqual(MethodInfo method1, MethodInfo method2)
        {
            if (method1.ReturnType != method2.ReturnType)
            {
                return false;
            }

            var method1parameters = method1.GetParameters();
            var method2parameters = method2.GetParameters();
            if (method1parameters.Length != method2parameters.Length)
            {
                return false;
            }

            for (int i = 0; i < method1parameters.Length; i++)
            {
                if (method1parameters[i].ParameterType != method2parameters[i].ParameterType)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion Methods
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CSharp.Utils.Reflection.Internal;

namespace CSharp.Utils.Data.Helpers
{
    public sealed class CharSeparatedDataReaderStrategy : AbstractDataReaderStrategy<string>
    {
        #region Fields

        private readonly bool _allStringColumns = true;

        private readonly IList<KeyValuePair<string, Type>> _columns;

        private readonly char _delimeter;

        private readonly IEnumerator<string> _enumerator;

        private PopulateItemArrayCallback _callback;

        #endregion Fields

        #region Constructors and Finalizers

        public CharSeparatedDataReaderStrategy(IEnumerator<string> enumerator, char delimiter)
            : this(enumerator, delimiter, readColumns(enumerator, delimiter))
        {
        }

        public CharSeparatedDataReaderStrategy(IEnumerator<string> enumerator, char delimiter, IEnumerable<string> columnNames)
        {
            this._columns = new List<KeyValuePair<string, Type>>();
            this._enumerator = enumerator;
            this._delimeter = delimiter;

            foreach (string columnName in columnNames)
            {
                this._columns.Add(new KeyValuePair<string, Type>(columnName, typeof(string)));
            }
        }

        public CharSeparatedDataReaderStrategy(IEnumerator<string> enumerator, char delimiter, IList<KeyValuePair<string, Type>> columns)
        {
            this._columns = new List<KeyValuePair<string, Type>>();
            this._enumerator = enumerator;
            this._delimeter = delimiter;
            this._allStringColumns = false;
        }

        #endregion Constructors and Finalizers

        #region Delegates

        private delegate void PopulateItemArrayCallback(string[] items, object[] objectArray);

        #endregion Delegates

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this._enumerator.Dispose();
        }

        protected override IList<KeyValuePair<string, Type>> getColumns()
        {
            return this._columns;
        }

        protected override void InitializeProtected()
        {
            base.InitializeProtected();
            if (this._allStringColumns)
            {
                var dynamicMethod = new DynamicMethod("populateItemArray" + GeneralHelper.Identity, typeof(void), new[] { typeof(string[]), typeof(object[]) }, typeof(CharSeparatedDataReaderStrategy));
                ILGenerator ilGen = dynamicMethod.GetILGenerator();
                int index = -1;
                int convertMode = -1;
                MethodInfo convertMethod = null;
                foreach (var kvp in this._columns)
                {
                    Type propertyType = kvp.Value;
                    index++;
                    if (kvp.Value == typeof(string))
                    {
                        convertMode = 0;
                    }

                    if (convertMode == -1 && propertyType.IsValueType)
                    {
                        if (SharedReflectionInfo.ConvertToMethods.TryGetValue(propertyType, out convertMethod))
                        {
                            convertMode = 1;
                        }
                        else
                        {
                            MethodInfo mi = propertyType.GetMethod("Parse", new[] { typeof(string) });
                            if (mi != null && mi.IsStatic && mi.ReflectedType == propertyType)
                            {
                                ParameterInfo[] parameters = mi.GetParameters();
                                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                                {
                                    convertMode = 2;
                                    convertMethod = mi;
                                }
                            }
                        }
                    }

                    if (convertMode != -1)
                    {
                        ilGen.Emit(OpCodes.Ldarg_1);
                        ilGen.Emit(OpCodes.Ldc_I4, index);
                        ilGen.Emit(OpCodes.Ldarg_0);
                        ilGen.Emit(OpCodes.Ldc_I4, index);
                        ilGen.Emit(OpCodes.Callvirt, SharedReflectionInfo.ArrayIndexerGetMethod);
                        if (convertMode != 0)
                        {
                            ilGen.Emit(OpCodes.Call, SharedReflectionInfo.ObjectToStringMethod);
                            ilGen.Emit(OpCodes.Call, convertMethod);
                        }

                        if (kvp.Value.IsValueType)
                        {
                            ilGen.Emit(OpCodes.Box, kvp.Value);
                        }

                        ilGen.Emit(OpCodes.Callvirt, SharedReflectionInfo.ArrayIndexerSetMethod);
                    }
                }

                ilGen.Emit(OpCodes.Ret);
                this._callback = (PopulateItemArrayCallback)dynamicMethod.CreateDelegate(typeof(PopulateItemArrayCallback));
            }
        }

        protected override void populateItemArray(string obj, object[] itemArray)
        {
            string[] items = obj.Split(this._delimeter);
            int min = Math.Min(items.Length, itemArray.Length);
            if (this._allStringColumns)
            {
                for (int i = 0; i < min; i++)
                {
                    itemArray[i] = items[i];
                }
            }
            else
            {
                this._callback(items, itemArray);
            }

            for (int i = min; i < itemArray.Length; i++)
            {
                itemArray[i] = null;
            }
        }

        protected override string readNextObject()
        {
            return getNextLine(this._enumerator);
        }

        private static string getNextLine(IEnumerator<string> enumerator)
        {
            if (enumerator.MoveNext())
            {
                return enumerator.Current;
            }

            return null;
        }

        private static IEnumerable<string> readColumns(IEnumerator<string> enumerator, char delimiter)
        {
            string line = getNextLine(enumerator);
            if (line != null)
            {
                string[] items = line.Split(delimiter);
                return items;
            }

            return new string[0];
        }

        #endregion Methods
    }
}

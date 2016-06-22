using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CSharp.Utils.Reflection.CodeSerialization
{
    public delegate TReturnType DynamicMethodDeligate<out TReturnType, in TParameterType>(TParameterType obj);

    [Serializable]
    public sealed class MethodInformation : AbstractMethodInformation
    {
        #region Constructors and Finalizers

        internal MethodInformation()
        {
            this.Instructions = new List<MsilInstruction>();
        }

        #endregion Constructors and Finalizers

        #region Properties

        internal List<MsilInstruction> Instructions { get; set; }

        internal string[] LocalVariableTypes { get; set; }

        internal string ReturnType { get; set; }

        #endregion Properties

        #region Public Methods and Operators

        public static MethodInformation BuildMethodInformation(MethodInfo method)
        {
            return MsilReader.BuildMethodInformation(method);
        }

        public void ApplyTypeMappings(string serverType, string clientType, bool ignoreAssemblyVersion)
        {
            Dictionary<string, string> mappings = ignoreAssemblyVersion ? new Dictionary<string, string>(IgnoreAssemblyVersionEqualityComparer.Instance) : new Dictionary<string, string>();
            mappings.Add(serverType, clientType);
            this.ApplyTypeMappings(mappings);
        }

        public void ApplyTypeMappings(IDictionary<string, string> mappings)
        {
            List<string> splits = splitTypeNames(this.ReturnType);
            this.ReturnType = applyMapping(this.ReturnType, splits, mappings);
            applyMapping(this.ParameterTypes, mappings);
            applyMapping(this.LocalVariableTypes, mappings);
            foreach (MsilInstruction ins in this.Instructions)
            {
                if (ins.TypeName != null)
                {
                    splits = splitTypeNames(ins.TypeName);
                    ins.TypeName = applyMapping(ins.TypeName, splits, mappings);
                }

                if (ins.MethodCallInfo != null)
                {
                    applyMapping(ins.MethodCallInfo.ParameterTypes, mappings);
                }
            }
        }

        public DynamicMethod BuildDynamicMethod()
        {
            return MsilWriter.BuildDynamicMethod(this);
        }

        public DynamicMethodDeligate<TReturnType, TParameterType> BuildMethodDeligate<TReturnType, TParameterType>()
        {
            DynamicMethod method = MsilWriter.BuildDynamicMethod(this);
            var del = (DynamicMethodDeligate<TReturnType, TParameterType>)method.CreateDelegate(typeof(DynamicMethodDeligate<TReturnType, TParameterType>));
            return del;
        }

        #endregion Public Methods and Operators

        #region Methods

        private static void applyMapping(IList<string> serverTypes, IDictionary<string, string> mappings)
        {
            for (int i = 0; i < serverTypes.Count; i++)
            {
                List<string> splits = splitTypeNames(serverTypes[i]);
                serverTypes[i] = applyMapping(serverTypes[i], splits, mappings);
            }
        }

        private static string applyMapping(string mainString, IEnumerable<string> children, IDictionary<string, string> mappings)
        {
            foreach (string v in children)
            {
                string mappingString = getMapping(v, mappings);
                mainString = mainString.Replace(v, mappingString);
            }

            return mainString;
        }

        private static string getMapping(string serverTypeName, IDictionary<string, string> mappings)
        {
            string clientTypeName = null;
            if (!mappings.TryGetValue(serverTypeName, out clientTypeName))
            {
                clientTypeName = serverTypeName;
            }

            return clientTypeName;
        }

        private static List<string> splitTypeNames(string typeName)
        {
            var list = new List<string>();
            var parser = new TypeParser(typeName);

            splitTypeNamesRecursively(parser, list);
            return list;
        }

        private static void splitTypeNamesRecursively(TypeParser parser, ICollection<string> list)
        {
            if (parser.GenericParameters == null || parser.GenericParameters.Count == 0)
            {
                list.Add(parser.FullyQualifiedTypeName);
                return;
            }

            foreach (TypeParser innerParser in parser.GenericParameters)
            {
                splitTypeNamesRecursively(innerParser, list);
            }
        }

        #endregion Methods
    }
}

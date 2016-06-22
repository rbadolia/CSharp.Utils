using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace CSharp.Utils.Reflection
{
    public class AssemblyBuilder
    {
        #region Public Methods and Operators

        public static CompilerResults BuildAssembly(string code, string programmingLanguage, List<string> assembliesToInclude, bool generateInMemory)
        {
            CodeDomProvider domProvider = CodeDomProvider.CreateProvider(programmingLanguage);
            var parameters = new CompilerParameters();
            foreach (string assembly in assembliesToInclude)
            {
                parameters.ReferencedAssemblies.Add(assembly);
            }

            parameters.GenerateInMemory = generateInMemory;
            return domProvider.CompileAssemblyFromSource(parameters, code);
        }

        public static CompilerResults BuildAssembly(string code, string programmingLanguage, CompilerParameters parameters)
        {
            CodeDomProvider domProvider = CodeDomProvider.CreateProvider(programmingLanguage);
            return domProvider.CompileAssemblyFromSource(parameters, code);
        }

        #endregion Public Methods and Operators
    }
}

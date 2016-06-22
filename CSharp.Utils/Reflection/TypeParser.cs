using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CSharp.Utils.Reflection
{
    public class TypeParser
    {
        #region Static Fields

        private static readonly Lazy<Assembly[]> Assemblies = new Lazy<Assembly[]>(() => AppDomain.CurrentDomain.GetAssemblies());

        #endregion Static Fields

        #region Fields

        public readonly string AssemblyDescriptionString;

        public readonly string Culture;

        public readonly string FullyQualifiedTypeName;

        public readonly List<TypeParser> GenericParameters = new List<TypeParser>();

        public readonly string PublicKeyToken;

        public readonly string ShortAssemblyName;

        public readonly string ShortTypeName;

        public readonly string Version;

        #endregion Fields

        #region Constructors and Finalizers

        public TypeParser(string assemblyQualifiedName)
        {
            this.FullyQualifiedTypeName = assemblyQualifiedName;
            int index = -1;
            var rootBlock = new block();
            {
                int bcount = 0;
                block currentBlock = rootBlock;
                for (int i = 0; i < assemblyQualifiedName.Length; ++i)
                {
                    char c = assemblyQualifiedName[i];
                    if (c == '[')
                    {
                        ++bcount;
                        var b = new block { iStart = i + 1, level = bcount, parentBlock = currentBlock };
                        currentBlock.innerBlocks.Add(b);
                        currentBlock = b;
                    }
                    else if (c == ']')
                    {
                        currentBlock.iEnd = i - 1;
                        if (assemblyQualifiedName[currentBlock.iStart] != '[')
                        {
                            currentBlock.parsedAssemblyQualifiedName = new TypeParser(assemblyQualifiedName.Substring(currentBlock.iStart, i - currentBlock.iStart));
                            if (bcount == 2)
                            {
                                this.GenericParameters.Add(currentBlock.parsedAssemblyQualifiedName);
                            }
                        }

                        currentBlock = currentBlock.parentBlock;
                        --bcount;
                    }
                    else if (bcount == 0 && c == ',')
                    {
                        index = i;
                        break;
                    }
                }
            }

            this.ShortTypeName = assemblyQualifiedName.Substring(0, index);

            this.AssemblyDescriptionString = assemblyQualifiedName.Substring(index + 2);
            {
                List<string> parts = this.AssemblyDescriptionString.Split(',').Select(x => x.Trim()).ToList();
                this.Version = LookForPairThenRemove(parts, "Version");
                this.Culture = LookForPairThenRemove(parts, "Culture");
                this.PublicKeyToken = LookForPairThenRemove(parts, "PublicKeyToken");
                if (parts.Count > 0)
                {
                    this.ShortAssemblyName = parts[0];
                }
            }
        }

        #endregion Constructors and Finalizers

        #region Methods

        internal string LanguageStyle(string prefix, string suffix)
        {
            if (this.GenericParameters.Count > 0)
            {
                var sb = new StringBuilder(this.ShortTypeName.Substring(0, this.ShortTypeName.IndexOf('`')));
                sb.Append(prefix);
                bool pendingElement = false;
                foreach (TypeParser param in this.GenericParameters)
                {
                    if (pendingElement)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(param.LanguageStyle(prefix, suffix));
                    pendingElement = true;
                }

                sb.Append(suffix);
                return sb.ToString();
            }

            return this.ShortTypeName;
        }

        private static string LookForPairThenRemove(IList<string> strings, string Name)
        {
            for (int istr = 0; istr < strings.Count; istr++)
            {
                string s = strings[istr];
                int i = s.IndexOf(Name, StringComparison.Ordinal);
                if (i == 0)
                {
                    int i2 = s.IndexOf('=');
                    if (i2 > 0)
                    {
                        string ret = s.Substring(i2 + 1);
                        strings.RemoveAt(istr);
                        return ret;
                    }
                }
            }

            return null;
        }

        #endregion Methods

        private sealed class block
        {
            #region Fields

            internal readonly List<block> innerBlocks = new List<block>();

            internal int iEnd;

            internal int iStart;

            internal int level;

            internal block parentBlock;

            internal TypeParser parsedAssemblyQualifiedName;

            #endregion Fields
        }
    }
}

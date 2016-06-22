using System;
using System.Reflection;
using System.Reflection.Emit;

namespace CSharp.Utils.Reflection.CodeSerialization
{
    [Serializable]
    internal sealed class MsilInstruction
    {
        #region Constructors and Finalizers

        internal MsilInstruction(short opCodeValue, object data)
        {
            this.OpCodeValue = opCodeValue;
            if (data != null)
            {
                var mb = data as MethodBase;
                if (mb != null)
                {
                    this.TypeName = mb.DeclaringType.AssemblyQualifiedName;
                    this.MethodCallInfo = new MethodCallInformation();
                    AbstractMethodInformation.PopulateMethodBaseInformation(mb, this.MethodCallInfo);
                    this.MethodCallInfo.MethodName = mb.Name;
                }
                else
                {
                    var t = data as Type;
                    if (t != null)
                    {
                        this.TypeName = t.AssemblyQualifiedName;
                    }
                    else
                    {
                        this.Data = data;
                    }
                }
            }
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public object Data { get; private set; }

        #endregion Public Properties

        #region Properties

        internal MethodCallInformation MethodCallInfo { get; private set; }

        internal OpCode OpCode
        {
            get
            {
                return OpCodeLookupHelper.GetOpCodeByValue(this.OpCodeValue);
            }
        }

        internal short OpCodeValue { get; private set; }

        internal string TypeName { get; set; }

        #endregion Properties
    }
}

using System;
using System.Reflection;
using System.Reflection.Emit;
using CSharp.Utils.Algorithms;
using CSharp.Utils.Contracts;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Diagnostics.Performance
{
    public class PropertyPerfCounterInjector : AbstractInjectingComponent<PerfCounterCategoryAttribute>
    {
        #region Static Fields

        private static readonly PropertyPerfCounterInjector InstanceObject = new PropertyPerfCounterInjector();

        #endregion Static Fields

        #region Fields

        private readonly MethodInfo _addInstanceUnTypedMethodInfo = typeof(PerfCounterManager).GetMethod("AddInstanceUnTyped");

        private readonly MethodInfo _getInstanceMethod = typeof(PerfCounterManager).GetProperty("Instance").GetMethod;

        private readonly MethodInfo _getUniqueStringMethodInfo = typeof(RandomHelper).GetMethod("GenerateUniqueString");

        private readonly MethodInfo _nameMethodInfo = typeof(IUnique).GetProperty("Id").GetMethod;

        #endregion Fields

        #region Constructors and Finalizers

        private PropertyPerfCounterInjector()
            : base(PropertyPerfCounterFilterMethodsStrategy.Instance)
        {
            this.CounterStorageType = CounterStorageType.Memory;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static PropertyPerfCounterInjector Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        public CounterStorageType CounterStorageType { get; set; }

        #endregion Public Properties

        #region Methods

        protected override DynamicMethod BuildDynamicMethod(InjectableMethodInfo<PerfCounterCategoryAttribute> methodInfo)
        {
            Guid guid = Guid.NewGuid();
            string newMethodName = methodInfo.MethodName + "_" + guid.ToString().Replace("-", string.Empty);
            var dynamicMethod = new DynamicMethod(newMethodName, methodInfo.Method.ReturnType, new[] { methodInfo.Method.DeclaringType }, methodInfo.Method.DeclaringType);
            ILGenerator ilGen = dynamicMethod.GetILGenerator();

            ilGen.DeclareLocal(typeof(Exception));
            Label exBlock = ilGen.BeginExceptionBlock();

            ilGen.Emit(OpCodes.Ldarg, 0);

            ilGen.Emit(OpCodes.Callvirt, methodInfo.Method);
            ilGen.Emit(OpCodes.Leave, exBlock);

            ilGen.BeginCatchBlock(typeof(Exception));
            ilGen.Emit(OpCodes.Stloc, 0);
            ilGen.Emit(OpCodes.Rethrow);

            ilGen.BeginFinallyBlock();

            ilGen.Emit(OpCodes.Call, this._getInstanceMethod);

            ilGen.Emit(OpCodes.Ldarg_0);

            if (typeof(IUnique).IsAssignableFrom(methodInfo.Method.DeclaringType))
            {
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Callvirt, this._nameMethodInfo);
            }
            else
            {
                ilGen.Emit(OpCodes.Call, this._getUniqueStringMethodInfo);
            }

            ilGen.Emit(OpCodes.Ldc_I4, (int)this.CounterStorageType);
            ilGen.Emit(OpCodes.Callvirt, this._addInstanceUnTypedMethodInfo);
            ilGen.EndExceptionBlock();

            ilGen.Emit(OpCodes.Ret);
            return dynamicMethod;
        }

        #endregion Methods
    }
}

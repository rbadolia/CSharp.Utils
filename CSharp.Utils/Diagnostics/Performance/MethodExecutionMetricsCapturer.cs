using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Diagnostics.Performance
{
    public sealed class MethodExecutionMetricsCapturer : AbstractInjectingComponent<MethodPerfCounterAttribute>
    {
        #region Static Fields

        private static readonly MethodExecutionMetricsCapturer InstanceObject = new MethodExecutionMetricsCapturer();

        #endregion Static Fields

        #region Fields

        private static readonly MethodInfo ElapsedTicksMethod = typeof(SharedStopWatch).GetProperty("ElapsedTicks").GetMethod;

        private readonly MethodInfo _getInstanceMethod = typeof(MethodExecutionMetricsCapturer).GetProperty("Instance").GetMethod;

        private readonly MethodInfo _incrementMethodMetricsMethod = typeof(MethodExecutionMetricsCapturer).GetMethod("IncrementMethodMetrics", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        private readonly ConcurrentDictionary<string, MethodExecutionMetrics> _methodMetrics = new ConcurrentDictionary<string, MethodExecutionMetrics>();

        #endregion Fields

        #region Constructors and Finalizers

        private MethodExecutionMetricsCapturer()
            : base(GenericFilterMethodStrategy<MethodPerfCounterAttribute>.Instance)
        {
            GenericFilterMethodStrategy<MethodPerfCounterAttribute>.Instance.DoNotInjectAttributeType = typeof(DoNotMethodPerfCounterAttribute);
            this.CounterStorageType = CounterStorageType.Memory;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public static MethodExecutionMetricsCapturer Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        public CounterStorageType CounterStorageType { get; set; }

        #endregion Public Properties

        #region Public Methods and Operators

        [CautionUsedByReflection]
        public void IncrementMethodMetrics(long ticksTaken, string methodId)
        {
            MethodExecutionMetrics metrics;
            if (this._methodMetrics.TryGetValue(methodId, out metrics))
            {
                metrics.Increment(ticksTaken);
            }
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override DynamicMethod BuildDynamicMethod(InjectableMethodInfo<MethodPerfCounterAttribute> methodInfo)
        {
            string className = methodInfo.Method.DeclaringType.FullName;
            string actualMethodName = methodInfo.Method.Name;
            string uniqueMethodName = methodInfo.Method.DeclaringType.FullName + "." + methodInfo.MethodName;
            ParameterInfo[] methodParameters = methodInfo.Method.GetParameters();
            Type[] parameterTypes;
            int destIndex = 0;
            if (!methodInfo.Method.IsStatic)
            {
                parameterTypes = new Type[methodParameters.Length + 1];
                parameterTypes[0] = methodInfo.Method.DeclaringType;
                destIndex = 1;
            }
            else
            {
                parameterTypes = new Type[methodParameters.Length];
            }

            for (int i = 0; i < methodParameters.Length; i++)
            {
                parameterTypes[i + destIndex] = methodParameters[i].ParameterType;
            }

            Guid guid = Guid.NewGuid();
            string newMethodName = methodInfo.MethodName + "_" + guid.ToString().Replace("-", string.Empty);
            var dynamicMethod = new DynamicMethod(newMethodName, methodInfo.Method.ReturnType, parameterTypes, methodInfo.Method.DeclaringType);
            ILGenerator ilGen = dynamicMethod.GetILGenerator();
            ilGen.DeclareLocal(typeof(long));

            if (methodInfo.Method.ReturnType != typeof(void))
            {
                ilGen.DeclareLocal(methodInfo.Method.ReturnType);
            }

            ilGen.Emit(OpCodes.Call, ElapsedTicksMethod);
            ilGen.Emit(OpCodes.Stloc_0);
            Label exBlock = ilGen.BeginExceptionBlock();

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                ilGen.Emit(OpCodes.Ldarg, i);
            }

            ilGen.Emit(methodInfo.Method.IsStatic ? OpCodes.Call : OpCodes.Callvirt, methodInfo.Method);
            if (methodInfo.Method.ReturnType != typeof(void))
            {
                ilGen.Emit(OpCodes.Stloc_1);
            }

            ilGen.Emit(OpCodes.Leave, exBlock);

            ilGen.BeginFinallyBlock();

            ilGen.Emit(OpCodes.Call, this._getInstanceMethod);
            ilGen.Emit(OpCodes.Call, ElapsedTicksMethod);
            ilGen.Emit(OpCodes.Ldloc_0);
            ilGen.Emit(OpCodes.Sub);
            ilGen.Emit(OpCodes.Ldstr, guid.ToString());
            ilGen.Emit(OpCodes.Callvirt, this._incrementMethodMetricsMethod);

            ilGen.EndExceptionBlock();

            if (methodInfo.Method.ReturnType != typeof(void))
            {
                ilGen.Emit(OpCodes.Ldloc_1);
            }

            ilGen.Emit(OpCodes.Ret);

            var metrics = new MethodExecutionMetrics { ClassName = className, MethodName = actualMethodName, UniqueMethodName = methodInfo.MethodName };
            this._methodMetrics.TryAdd(guid.ToString(), metrics);

            PerfCounterManager.Instance.AddInstance(metrics, uniqueMethodName, this.CounterStorageType);
            return dynamicMethod;
        }

        #endregion Methods
    }
}

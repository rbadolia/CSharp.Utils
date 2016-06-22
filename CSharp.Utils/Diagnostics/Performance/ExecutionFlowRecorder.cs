using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CSharp.Utils.Collections.Generic;
using CSharp.Utils.Reflection;

namespace CSharp.Utils.Diagnostics.Performance
{
    public sealed class ExecutionFlowRecorder : AbstractInjectingComponent<RecordExecutionFlowAttribute>
    {
        #region Static Fields

        private static readonly ExecutionFlowRecorder InstanceObject = new ExecutionFlowRecorder();

        #endregion Static Fields

        #region Fields

        private static readonly MethodInfo ListAddMethod = typeof(List<Pair<string, object>>).GetMethod("Add");

        private static readonly ConstructorInfo ListConstructorInfo = typeof(List<Pair<string, object>>).GetConstructor(Type.EmptyTypes);

        private static readonly MethodInfo MethodCalledMethod = typeof(RecordExecutionFlowHelper).GetMethod("MethodCalled");

        private static readonly MethodInfo MethodExecutedMethod = typeof(RecordExecutionFlowHelper).GetMethod("MethodExecuted");

        private static readonly ConstructorInfo PairConstructorInfo = typeof(Pair<string, object>).GetConstructor(new[] { typeof(string), typeof(object) });

        #endregion Fields

        #region Constructors and Finalizers

        private ExecutionFlowRecorder()
            : base(GenericFilterMethodStrategy<RecordExecutionFlowAttribute>.Instance)
        {
            GenericFilterMethodStrategy<RecordExecutionFlowAttribute>.Instance.DoNotInjectAttributeType = typeof(DoNotRecordExecutionFlowAttribute);
            this.Enabled = true;
        }

        #endregion Constructors and Finalizers

        #region Public Events

        public event EventHandler<ExecutionFlowRecordedEventArgs> OnExecutionFlowRecorded;

        #endregion Public Events

        #region Public Properties

        public static ExecutionFlowRecorder Instance
        {
            get
            {
                return InstanceObject;
            }
        }

        #endregion Public Properties

        #region Methods

        internal void RaiseFlowRecordedEvent(ExecutionFlow flow)
        {
            if (this.OnExecutionFlowRecorded != null)
            {
                this.OnExecutionFlowRecorded(this, new ExecutionFlowRecordedEventArgs(flow));
            }
        }

        protected override DynamicMethod BuildDynamicMethod(InjectableMethodInfo<RecordExecutionFlowAttribute> methodInfo)
        {
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
            string newMethodName = methodInfo.Method.Name + "_" + guid.ToString().Replace("-", string.Empty);
            var dynamicMethod = new DynamicMethod(newMethodName, methodInfo.Method.ReturnType, parameterTypes, methodInfo.Method.DeclaringType);
            ILGenerator ilGen = dynamicMethod.GetILGenerator();
            ilGen.DeclareLocal(typeof(List<Pair<string, object>>));
            ilGen.DeclareLocal(typeof(Pair<string, object>));
            ilGen.DeclareLocal(typeof(Exception));
            ilGen.DeclareLocal(typeof(ExecutionFlow));

            if (methodInfo.Method.ReturnType != typeof(void))
            {
                ilGen.DeclareLocal(methodInfo.Method.ReturnType);
            }

            if (!methodInfo.Attribute.IgnoreArguments)
            {
                ilGen.Emit(OpCodes.Newobj, ListConstructorInfo);
                ilGen.Emit(OpCodes.Stloc_0);

                for (int i = 0; i < methodParameters.Length; i++)
                {
                    ilGen.Emit(OpCodes.Ldstr, methodParameters[i].Name);
                    ilGen.Emit(OpCodes.Ldarg, i + destIndex);
                    if (methodParameters[i].ParameterType.IsValueType)
                    {
                        ilGen.Emit(OpCodes.Box, methodParameters[i].ParameterType);
                    }

                    ilGen.Emit(OpCodes.Newobj, PairConstructorInfo);
                    ilGen.Emit(OpCodes.Stloc_1);
                    ilGen.Emit(OpCodes.Ldloc_0);
                    ilGen.Emit(OpCodes.Ldloc_1);
                    ilGen.Emit(OpCodes.Callvirt, ListAddMethod);
                }
            }

            ilGen.Emit(OpCodes.Ldstr, methodInfo.Method.DeclaringType.FullName);
            ilGen.Emit(OpCodes.Ldstr, methodInfo.MethodName);
            ilGen.Emit(OpCodes.Ldloc_0);
            ilGen.Emit(OpCodes.Call, MethodCalledMethod);
            ilGen.Emit(OpCodes.Stloc, 3);
            Label exBlock = ilGen.BeginExceptionBlock();

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                ilGen.Emit(OpCodes.Ldarg, i);
            }

            ilGen.Emit(methodInfo.Method.IsStatic ? OpCodes.Call : OpCodes.Callvirt, methodInfo.Method);
            if (methodInfo.Method.ReturnType != typeof(void))
            {
                ilGen.Emit(OpCodes.Stloc, 4);
            }

            ilGen.Emit(OpCodes.Leave, exBlock);

            ilGen.BeginCatchBlock(typeof(Exception));
            ilGen.Emit(OpCodes.Stloc_2);
            ilGen.Emit(OpCodes.Rethrow);

            ilGen.BeginFinallyBlock();

            ilGen.Emit(OpCodes.Ldloc, 3);
            ilGen.Emit(OpCodes.Ldloc, 2);
            ilGen.Emit(OpCodes.Call, MethodExecutedMethod);

            ilGen.EndExceptionBlock();

            if (methodInfo.Method.ReturnType != typeof(void))
            {
                ilGen.Emit(OpCodes.Ldloc, 4);
            }

            ilGen.Emit(OpCodes.Ret);
            return dynamicMethod;
        }

        #endregion Methods
    }
}

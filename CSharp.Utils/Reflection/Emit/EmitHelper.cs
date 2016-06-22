using System;
using System.Reflection.Emit;
using CSharp.Utils.Reflection.Internal;

namespace CSharp.Utils.Reflection.Emit
{
    public static class EmitHelper
    {
        public static void ReplaceArrayElement(this ILGenerator ilGen, OpCode codeToPlaceArrayOnStack, int index, OpCode codeToPlaceElementOnStack)
        {
            ReplaceArrayElement(ilGen, codeToPlaceArrayOnStack, index, () => ilGen.Emit(codeToPlaceElementOnStack));
        }

        public static void ReplaceArrayElement(this ILGenerator ilGen, OpCode codeToPlaceArrayOnStack, int index, Action actionToPlaceElementOnStack)
        {
            ilGen.Emit(codeToPlaceArrayOnStack);
            ilGen.Emit(OpCodes.Ldc_I4, index);
            actionToPlaceElementOnStack();
            ilGen.Emit(OpCodes.Stelem_Ref);
        }

        public static void CreateArray(this ILGenerator ilGen, Type arrayType, int size, int localVariableIndexToSave)
        {
            ilGen.Emit(OpCodes.Ldc_I4, size);
            ilGen.Emit(OpCodes.Newarr, arrayType);
            ilGen.Emit(OpCodes.Stloc, localVariableIndexToSave);
        }

        public static void CallSafeInvoke(this ILGenerator ilGen, OpCode codeToPlaceDelegateOnStack, OpCode codeToPlaceArgumentArrayOnStack)
        {
            ilGen.Emit(codeToPlaceDelegateOnStack);
            ilGen.Emit(codeToPlaceArgumentArrayOnStack);
            ilGen.Emit(OpCodes.Call, SharedReflectionInfo.SafeInvokeMethod);
        }
    }
}

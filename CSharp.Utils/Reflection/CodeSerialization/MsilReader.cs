using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace CSharp.Utils.Reflection.CodeSerialization
{
    internal sealed class MsilReader : AbstractDisposable
    {
        #region Fields

        private readonly BinaryReader _methodReader;

        private readonly Module _module;

        private MsilInstruction _current;

        #endregion Fields

        #region Constructors and Finalizers

        internal MsilReader(MethodBase method)
        {
            if (method == null)
            {
                throw new ArgumentException(@"method is null.", "method");
            }

            this._module = method.Module;
            this._methodReader = new BinaryReader(new MemoryStream(method.GetMethodBody().GetILAsByteArray()));
        }

        #endregion Constructors and Finalizers

        #region Properties

        internal MsilInstruction Current
        {
            get
            {
                return this._current;
            }
        }

        #endregion Properties

        #region Methods

        internal static MethodInformation BuildMethodInformation(MethodInfo method)
        {
            var info = new MethodInformation();
            AbstractMethodInformation.PopulateMethodBaseInformation(method, info);
            info.ReturnType = method.ReturnType.AssemblyQualifiedName;
            MethodBody body = method.GetMethodBody();
            int implicitParametersCount = method.IsStatic ? 0 : 1;
            info.LocalVariableTypes = new string[body.LocalVariables.Count + implicitParametersCount];
            for (int i = 0; i < body.LocalVariables.Count; i++)
            {
                info.LocalVariableTypes[body.LocalVariables[i].LocalIndex + implicitParametersCount] = body.LocalVariables[i].LocalType.AssemblyQualifiedName;
            }

            if (implicitParametersCount == 1)
            {
                info.LocalVariableTypes[0] = method.DeclaringType.AssemblyQualifiedName;
            }

            var reader = new MsilReader(method);
            while (reader.Read())
            {
                info.Instructions.Add(reader.Current);
            }

            return info;
        }

        internal bool Read()
        {
            if (this._methodReader.BaseStream.Length == this._methodReader.BaseStream.Position)
            {
                return false;
            }

            var index = (int)this._methodReader.BaseStream.Position;
            int instructionValue;

            if (this._methodReader.BaseStream.Length - 1 == this._methodReader.BaseStream.Position)
            {
                instructionValue = this._methodReader.ReadByte();
            }
            else
            {
                instructionValue = this._methodReader.ReadUInt16();
                if ((instructionValue & OpCodes.Prefix1.Value) != OpCodes.Prefix1.Value)
                {
                    instructionValue &= 0xff;
                    this._methodReader.BaseStream.Position--;
                }
                else
                {
                    instructionValue = ((0xFF00 & instructionValue) >> 8) | ((0xFF & instructionValue) << 8);
                }
            }

            OpCode code;
            if (!OpCodeLookupHelper.TryGetOpCodeByValue((short)instructionValue, out code))
            {
                throw new InvalidProgramException();
            }

            int dataSize = GetSize(code.OperandType);
            var data = new byte[dataSize];
            this._methodReader.Read(data, 0, dataSize);

            object objData = GetData(this._module, code, data);

            this._current = new MsilInstruction(code.Value, objData);
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            this._methodReader.Dispose();
        }

        private static object GetData(Module module, OpCode code, byte[] rawData)
        {
            if (code.OperandType == OperandType.InlineNone)
            {
                return null;
            }

            object data = null;
            switch (code.OperandType)
            {
                case OperandType.InlineField:
                    data = module.ResolveField(BitConverter.ToInt32(rawData, 0));
                    break;

                case OperandType.InlineBrTarget:
                case OperandType.InlineSwitch:
                case OperandType.InlineI:
                    data = BitConverter.ToInt32(rawData, 0);
                    break;

                case OperandType.InlineI8:
                    data = BitConverter.ToInt64(rawData, 0);
                    break;

                case OperandType.InlineMethod:
                    data = module.ResolveMethod(BitConverter.ToInt32(rawData, 0));
                    break;

                case OperandType.InlineR:
                    data = BitConverter.ToDouble(rawData, 0);
                    break;

                case OperandType.InlineSig:
                    data = module.ResolveSignature(BitConverter.ToInt32(rawData, 0));
                    break;

                case OperandType.InlineString:
                    data = module.ResolveString(BitConverter.ToInt32(rawData, 0));
                    break;

                case OperandType.InlineTok:
                case OperandType.InlineType:
                    data = module.ResolveType(BitConverter.ToInt32(rawData, 0));
                    break;

                case OperandType.InlineVar:
                    data = BitConverter.ToInt16(rawData, 0);
                    break;

                case OperandType.ShortInlineVar:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineBrTarget:
                    data = rawData[0];
                    break;

                case OperandType.ShortInlineR:
                    data = BitConverter.ToSingle(rawData, 0);
                    break;
            }

            return data;
        }

        private static int GetSize(OperandType opType)
        {
            switch (opType)
            {
                case OperandType.InlineNone:
                    return 0;

                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                    return 1;

                case OperandType.InlineVar:
                    return 2;

                case OperandType.InlineBrTarget:
                case OperandType.InlineField:
                case OperandType.InlineI:
                case OperandType.InlineMethod:
                case OperandType.InlineSig:
                case OperandType.InlineString:
                case OperandType.InlineSwitch:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                case OperandType.ShortInlineR:

                    return 4;

                case OperandType.InlineI8:
                case OperandType.InlineR:

                    return 8;

                default:
                    return 0;
            }
        }

        #endregion Methods
    }
}

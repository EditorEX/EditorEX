using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace EditorEX.Tests.Harness
{
    // Harmony can read a method body itself, but only by round-tripping it through MonoMod's
    // dynamic method backend, which needs ILGenerator.MarkSequencePoint and therefore does not
    // run on .NET 8. Tests that want to feed real game IL to a transpiler read it here instead.
    internal static class MethodIlReader
    {
        private static readonly Dictionary<short, OpCode> _opCodes = typeof(OpCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(n => n.FieldType == typeof(OpCode))
            .Select(n => (OpCode)n.GetValue(null)!)
            .ToDictionary(n => n.Value);

        internal static List<CodeInstruction> Read(MethodBase method)
        {
            byte[] il =
                method.GetMethodBody()?.GetILAsByteArray()
                ?? throw new ArgumentException($"{method.Name} has no body.", nameof(method));

            Module module = method.Module;
            Type[]? typeArguments = method.DeclaringType is { IsGenericType: true } declaringType
                ? declaringType.GetGenericArguments()
                : null;
            Type[]? methodArguments = method.IsGenericMethod
                ? method.GetGenericArguments()
                : null;

            List<CodeInstruction> instructions = new();
            int position = 0;
            while (position < il.Length)
            {
                short code = il[position++];
                if (code == 0xFE)
                {
                    code = (short)((code << 8) | il[position++]);
                }

                OpCode opCode = _opCodes[code];
                object? operand = null;

                switch (opCode.OperandType)
                {
                    case OperandType.InlineNone:
                        break;

                    case OperandType.ShortInlineBrTarget:
                    case OperandType.ShortInlineI:
                    case OperandType.ShortInlineVar:
                        operand = (int)il[position];
                        position += 1;
                        break;

                    case OperandType.InlineVar:
                        operand = (int)BitConverter.ToUInt16(il, position);
                        position += 2;
                        break;

                    case OperandType.InlineI8:
                    case OperandType.InlineR:
                        position += 8;
                        break;

                    case OperandType.InlineSwitch:
                        position += 4 + (4 * BitConverter.ToInt32(il, position));
                        break;

                    case OperandType.InlineMethod:
                    case OperandType.InlineField:
                    case OperandType.InlineType:
                    case OperandType.InlineString:
                    case OperandType.InlineTok:
                        operand = Resolve(
                            module,
                            opCode.OperandType,
                            BitConverter.ToInt32(il, position),
                            typeArguments,
                            methodArguments
                        );
                        position += 4;
                        break;

                    default:
                        position += 4;
                        break;
                }

                instructions.Add(new CodeInstruction(opCode, operand));
            }

            return instructions;
        }

        private static object? Resolve(
            Module module,
            OperandType operandType,
            int token,
            Type[]? typeArguments,
            Type[]? methodArguments
        )
        {
            try
            {
                return operandType switch
                {
                    OperandType.InlineMethod => module.ResolveMethod(
                        token,
                        typeArguments,
                        methodArguments
                    ),
                    OperandType.InlineField => module.ResolveField(
                        token,
                        typeArguments,
                        methodArguments
                    ),
                    OperandType.InlineType => module.ResolveType(
                        token,
                        typeArguments,
                        methodArguments
                    ),
                    OperandType.InlineString => module.ResolveString(token),
                    _ => module.ResolveMember(token, typeArguments, methodArguments),
                };
            }
            catch (Exception)
            {
                // Signature tokens and the like are not resolvable as members; nothing inspects
                // them, so the raw token is a good enough stand-in.
                return token;
            }
        }
    }
}

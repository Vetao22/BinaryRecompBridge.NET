using System;
using System.Collections.Generic;
using System.Text;
using AsmCPlusPlusDecodingEngine.Engine;
using AsmSharpDecodingEngine.Engine;
using BinaryDecoded.NET.Core;
using BinaryDecoded.NET.Engine;

namespace BinaryRecompBridge.NET.Core
{
    public enum TargetLanguage
    {
        CSharp,
        CPlusPlus
    }

    /// <summary>
    /// Converts a streamed binary file into C# or C++ source code using BinaryDecoded.NET
    /// and the AsmSharp / AsmCPlusPlus code generators.
    /// </summary>
    public sealed class BinaryTranspiler
    {
        private readonly IArchitectureDecoder _decoder;

        public BinaryTranspiler(IArchitectureDecoder decoder)
        {
            _decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
        }

        public string ToCSharp(string binaryFilePath, long baseAddress, string className, string namespaceName = "RecompiledBinary")
        {
            string asm = ToAssembly(binaryFilePath, baseAddress);
            var instructions = new AssemblyParser().ParseCode(asm, isFilePath: false);
            return new CSharpCodeGenerator().GenerateClass(className, namespaceName, instructions);
        }

        public string ToCPlusPlus(string binaryFilePath, long baseAddress, string functionName)
        {
            string asm = ToAssembly(binaryFilePath, baseAddress);
            var instructions = new AssemblyParser().ParseCode(asm, isFilePath: false);
            return new CppCodeGenerator().GenerateCppSource(functionName, instructions);
        }

        public string Transpile(string binaryFilePath, long baseAddress, TargetLanguage target, string name, string namespaceName = "RecompiledBinary") =>
            target switch
            {
                TargetLanguage.CSharp => ToCSharp(binaryFilePath, baseAddress, name, namespaceName),
                TargetLanguage.CPlusPlus => ToCPlusPlus(binaryFilePath, baseAddress, name),
                _ => throw new ArgumentOutOfRangeException(nameof(target))
            };

        private string ToAssembly(string binaryFilePath, long baseAddress)
        {
            List<Instruction> instructions = StreamingDisassemblerEngine.ExtractInstructions(binaryFilePath, _decoder, baseAddress);
            var sb = new StringBuilder();

            foreach (var instr in instructions)
            {
                if (!instr.IsValid || string.IsNullOrWhiteSpace(instr.Mnemonic))
                    continue;

                string ops = instr.Operands.Length > 0 ? $" {string.Join(", ", instr.Operands)}" : string.Empty;
                sb.AppendLine($"{instr.Mnemonic}{ops}");
            }

            return sb.ToString();
        }
    }
}
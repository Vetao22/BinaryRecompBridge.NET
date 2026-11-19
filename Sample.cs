using System;
using System.IO;
using BinaryDecoded.NET.Decoders;
using BinaryRecompBridge.NET.Core;

namespace BinaryRecompBridge.NET.Demo
{
    class Program
    {
        static void Main()
        {
            string tempBin = Path.GetTempFileName();

            // Sample x86-64 stream: SUB RSP, 0x20 | MOV RCX, RAX | RET
            File.WriteAllBytes(tempBin, new byte[]
            {
                0x48, 0x83, 0xEC, 0x20,
                0x48, 0x8B, 0xC8,
                0xC3
            });

            var transpiler = new BinaryTranspiler(new XboxOneDecoder());

            // Generate C# class and save to disk
            string csCode = transpiler.ToCSharp(tempBin, baseAddress: 0x140001000, className: "JaguarRoutine", namespaceName: "XboxOne.Recomp");
            File.WriteAllText("JaguarRoutine.cs", csCode);

            // Generate C++ source and save to disk
            string cppCode = transpiler.ToCPlusPlus(tempBin, baseAddress: 0x140001000, functionName: "ExecuteJaguarRoutine");
            File.WriteAllText("ExecuteJaguarRoutine.cpp", cppCode);

            Console.WriteLine("C# and C++ outputs generated successfully.");

            File.Delete(tempBin);
        }
    }
}
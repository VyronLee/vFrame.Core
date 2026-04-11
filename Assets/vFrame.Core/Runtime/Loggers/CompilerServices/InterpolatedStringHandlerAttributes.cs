// ------------------------------------------------------------
//         File: InterpolatedStringHandlerAttributes.cs
//        Brief: Compiler-recognized attributes for zero-GC
//               interpolated string logging.
//
//              Unity 2022.3 targets .NET Standard 2.1 which
//              lacks these attributes. Defined here so the
//              Roslyn compiler recognizes them by name.
//
//      Created: 2026-04-12
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Marks a ref struct as an interpolated string handler.
    /// The compiler generates handler construction calls instead
    /// of string concatenation, enabling short-circuit evaluation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    internal sealed class InterpolatedStringHandlerAttribute : Attribute { }

    /// <summary>
    /// Specifies which method parameters should be forwarded to
    /// the interpolated string handler constructor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class InterpolatedStringHandlerArgumentAttribute : Attribute
    {
        public string[] Arguments { get; }

        public InterpolatedStringHandlerArgumentAttribute(string argument) {
            Arguments = new[] { argument };
        }

        public InterpolatedStringHandlerArgumentAttribute(params string[] arguments) {
            Arguments = arguments;
        }
    }
}

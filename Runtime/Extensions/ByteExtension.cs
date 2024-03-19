//------------------------------------------------------------
//        File:  ByteExtension.cs
//       Brief:  Byte type extension.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-09-09 16:56
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Text;
using vFrame.Core.ObjectPools.Builtin;

namespace vFrame.Core.Extensions
{
    public static class ByteExtension
    {
        public static string ToHex(this byte b) {
            return b.ToString("X2");
        }

        public static string ToHex(this byte[] bytes) {
            var builder = StringBuilderPool.Shared.Get();
            foreach (var b in bytes) {
                builder.Append(b.ToString("X2"));
            }
            var str = builder.ToString();
            StringBuilderPool.Shared.Return(builder);
            return str;
        }

        public static string ToHex(this byte[] bytes, string format) {
            var builder = StringBuilderPool.Shared.Get();
            foreach (var b in bytes) {
                builder.Append(b.ToString(format));
            }
            var str = builder.ToString();
            StringBuilderPool.Shared.Return(builder);
            return str;
        }

        public static string ToHex(this byte[] bytes, int offset, int count) {
            var builder = StringBuilderPool.Shared.Get();
            for (var i = offset; i < offset + count; ++i) {
                builder.Append(bytes[i].ToString("X2"));
            }
            var str = builder.ToString();
            StringBuilderPool.Shared.Return(builder);
            return str;
        }

        public static string ToStr(this byte[] bytes) {
            return Encoding.Default.GetString(bytes);
        }

        public static string ToStr(this byte[] bytes, int index, int count) {
            return Encoding.Default.GetString(bytes, index, count);
        }

        public static string Utf8ToStr(this byte[] bytes) {
            return Encoding.UTF8.GetString(bytes);
        }

        public static string Utf8ToStr(this byte[] bytes, int index, int count) {
            return Encoding.UTF8.GetString(bytes, index, count);
        }
    }
}
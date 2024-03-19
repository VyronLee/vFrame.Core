//------------------------------------------------------------
//        File:  StringExtension.cs
//       Brief:  String type extension.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-09-09 16:59
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace vFrame.Core.Extensions
{
    public static class StringExtension
    {
        public static IEnumerable<byte> ToBytes(this string str) {
            var byteArray = Encoding.Default.GetBytes(str);
            return byteArray;
        }

        public static byte[] ToByteArray(this string str) {
            var byteArray = Encoding.Default.GetBytes(str);
            return byteArray;
        }

        public static byte[] ToUtf8ByteArray(this string str) {
            var byteArray = Encoding.UTF8.GetBytes(str);
            return byteArray;
        }

        public static byte[] HexToBytes(this string hexString) {
            if (hexString.Length % 2 != 0) {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            var hexAsBytes = new byte[hexString.Length / 2];
            for (var index = 0; index < hexAsBytes.Length; index++) {
                var byteValue = "";
                byteValue += hexString[index * 2];
                byteValue += hexString[index * 2 + 1];
                hexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return hexAsBytes;
        }
    }
}
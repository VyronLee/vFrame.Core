﻿//------------------------------------------------------------
//        File:  MessageDigestUtils.cs
//       Brief:  Message digest utils
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-09-09 17:20
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.IO;
using System.Security.Cryptography;
using vFrame.Core.Extensions;

namespace vFrame.Core.Utils
{
    public static class MessageDigestUtils
    {
        public static string MD5(byte[] data) {
            var md5 = new MD5CryptoServiceProvider();
            var ret = md5.ComputeHash(data);
            return ret.ToHex("x2");
        }

        public static string MD5(Stream data) {
            var md5 = new MD5CryptoServiceProvider();
            var ret = md5.ComputeHash(data);
            return ret.ToHex("x2");
        }

        public static byte[] MD5b(byte[] data) {
            var md5 = new MD5CryptoServiceProvider();
            return md5.ComputeHash(data);
        }

        public static byte[] MD5b(Stream data) {
            var md5 = new MD5CryptoServiceProvider();
            return md5.ComputeHash(data);
        }

        public static string SHA256(byte[] data) {
            var sha = new SHA256CryptoServiceProvider();
            var ret = sha.ComputeHash(data);
            return ret.ToHex("x2");
        }

        public static byte[] SHA256b(byte[] data) {
            var sha = new SHA256CryptoServiceProvider();
            return sha.ComputeHash(data);
        }

        public static string SHA512(byte[] data) {
            var sha = new SHA512CryptoServiceProvider();
            var ret = sha.ComputeHash(data);
            return ret.ToHex("x2");
        }

        public static byte[] SHA512b(byte[] data) {
            var sha = new SHA512CryptoServiceProvider();
            return sha.ComputeHash(data);
        }

        public static string FileMD5(string filePath) {
            byte[] retVal;
            using (var file = new FileStream(filePath, FileMode.Open)) {
                var md5 = new MD5CryptoServiceProvider();
                retVal = md5.ComputeHash(file);
            }

            return retVal.ToHex("x2");
        }

        public static byte[] FileMD5b(string filePath) {
            using (var file = new FileStream(filePath, FileMode.Open)) {
                var md5 = new MD5CryptoServiceProvider();
                return md5.ComputeHash(file);
            }
        }
    }
}
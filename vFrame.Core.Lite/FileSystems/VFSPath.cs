﻿using System;
using System.IO;
using vFrame.Core.Extensions;
using vFrame.Core.FileSystems.Exceptions;
using vFrame.Core.Utils;

namespace vFrame.Core.FileSystems
{
    public struct VFSPath
    {
        private string _value;

        private VFSPath(string value) {
            _value = string.Empty;
            _value = Normalize(value);
        }

        public static VFSPath GetPath(string value) {
            return new VFSPath(value);
        }

        private static string Normalize(string value) {
            return value.Replace("\\", "/");
        }

        private VFSPath EnsureDirectoryPath() {
            if (!_value.EndsWith("/")) _value += "/";
            return this;
        }

        public VFSPath AsDirectory() {
            return EnsureDirectoryPath();
        }

        public string GetValue() {
            return _value;
        }

        public string GetHash() {
            return MessageDigestUtils.MD5(_value.ToUtf8ByteArray());
        }

        public string GetFileName() {
            return Path.GetFileName(_value);
        }

        public VFSPath GetDirectory() {
            return new VFSPath(GetDirectoryName());
        }

        public string GetDirectoryName() {
            return Path.GetDirectoryName(_value);
        }

        public string GetExtension() {
            return Path.GetExtension(_value);
        }

        public VFSPath GetRelative(VFSPath target) {
            if (_value.Contains(target.GetValue())) {
                return _value.Substring(target._value.Length);
            }
            throw new PathNotRelativeException();
        }

        public bool IsAbsolute() {
            // unix style: "/user/data"
            // window style: "c:/user/data"
            return _value.Length > 0 && _value[0] == '/'
                   || _value.Length >= 3 && _value[1] == ':' && _value[2] == '/';
        }

        public override string ToString() {
            return _value;
        }

        public override bool Equals(object obj) {
            switch (obj) {
                case VFSPath path:
                    return path == this;
                case string str:
                    return str == this;
                default:
                    return false;
            }
        }

        public override int GetHashCode() {
            return _value.GetHashCode();
        }

        public static bool operator ==(VFSPath p1, VFSPath p2) {
            return p1._value == p2._value;
        }

        public static bool operator !=(VFSPath p1, VFSPath p2) {
            return !(p1 == p2);
        }

        public static VFSPath operator +(VFSPath dir, string fileName) {
            return dir + GetPath(fileName);
        }

        public static VFSPath operator +(VFSPath dir, VFSPath fileName) {
            return GetPath(dir.EnsureDirectoryPath().GetValue() + fileName.GetValue());
        }

        public static explicit operator string(VFSPath vfsPath) {
            return vfsPath.GetValue();
        }

        public static implicit operator VFSPath(string value) {
            return GetPath(value);
        }
    }
}
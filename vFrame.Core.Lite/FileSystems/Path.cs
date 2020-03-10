using System;
using vFrame.Core.FileSystems.Exceptions;

namespace vFrame.Core.FileSystems
{
    public struct Path
    {
        private string _value;

        public Path(string value) {
            _value = string.Empty;
            _value = Normalize(value);
        }

        public static Path GetPath(string value) {
            return new Path(value);
        }

        private string Normalize(string value) {
            return value.Replace("\\", "/");
        }

        private Path EnsureDirectoryPath() {
            if (!_value.EndsWith("/")) {
                _value += "/";
            }
            return this;
        }

        public string GetValue() {
            return _value;
        }

        public string GetFileName() {
            return System.IO.Path.GetFileName(_value);
        }

        public Path GetDirectory() {
            return new Path(GetDirectoryName());
        }

        public string GetDirectoryName() {
            return System.IO.Path.GetDirectoryName(_value);
        }

        public string GetExtension() {
            return System.IO.Path.GetExtension(_value);
        }

        public Path GetRelative(Path target) {
            var targetValue = target.GetValue();
            var begin = targetValue.IndexOf(_value, StringComparison.Ordinal);
            if (begin < 0) {
                throw new PathNotRelativeException();
            }
            return new Path(targetValue.Substring(begin));
        }

        public bool IsAbsolute() {
            // unix style: "/user/data"
            // window style: "c:/user/data"
            return (_value.Length > 0 && _value[0] == '/')
                   || (_value.Length >= 3 && _value[1] == ':' && _value[2] == '/');
        }

        public static Path operator +(Path dir, string fileName) {
            return dir + GetPath(fileName);
        }

        public static Path operator +(Path dir, Path fileName) {
            return GetPath(dir.EnsureDirectoryPath().GetValue() + fileName.GetValue());
        }
    }
}
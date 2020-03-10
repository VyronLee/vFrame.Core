using System;

namespace vFrame.Core.FileSystems.Exceptions
{
    public class FileSystemException : Exception {}

    public class PathNotRelativeException : FileSystemException {}

    public class PackageStreamNotOpenedException : FileSystemException {}
    public class PackageStreamClosedException : FileSystemException {}
    public class PackageStreamSeekOutOfRangeException : FileSystemException {}
}
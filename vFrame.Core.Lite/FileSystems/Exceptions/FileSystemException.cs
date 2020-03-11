using System;

namespace vFrame.Core.FileSystems.Exceptions
{
    public class FileSystemException : Exception
    {
        protected FileSystemException() {}
        protected FileSystemException(string message) : base(message) { }
    }

    public class PathNotRelativeException : FileSystemException {}

    public class FileSystemAlreadyOpenedException : FileSystemException {}
    public class FileSystemAlreadyClosedException : FileSystemException {}

    public class PackageFileSystemHeaderDataError : FileSystemException {}
    public class PackageFileSystemFileListDataError : FileSystemException {}
    public class PackageFileSystemBlockTableDataError : FileSystemException {}
    public class PackageFileSystemFileNotFound : FileSystemException {}
    public class PackageStreamOpenFailedException : FileSystemException {}

    public class PackageStreamNotOpenedException : FileSystemException {}
    public class PackageStreamClosedException : FileSystemException {}
    public class PackageBlockDisposedException : FileSystemException {}
    public class PackageBlockOffsetErrorException : FileSystemException {
        public PackageBlockOffsetErrorException(long offset, long need) : base($"At least: {need}, got: {offset}") {}
    }
    public class PackageStreamDataErrorException : FileSystemException {
        public PackageStreamDataErrorException(long size, long need) : base($"At least: {need}, got: {size}") {}
    }
}
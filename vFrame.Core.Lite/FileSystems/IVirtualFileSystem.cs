using System.Collections.Generic;
using System.IO;

namespace vFrame.Core.FileSystems
{
    public interface IVirtualFileSystem
    {
        /// <summary>
        ///     Open file system.
        /// </summary>
        /// <param name="streamVfsPath">Working directory or package file path.</param>
        /// <returns></returns>
        void Open(VFSPath streamVfsPath);

        /// <summary>
        ///     Close file system.
        /// </summary>
        /// <returns></returns>
        void Close();

        /// <summary>
        ///     Is file with relative path exist?
        /// </summary>
        /// <param name="relativeVfsPath"></param>
        /// <returns></returns>
        bool Exist(VFSPath relativeVfsPath);

        /// <summary>
        ///     Get file stream of specified name.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="mode"></param>
        /// <param name="access"></param>
        /// <param name="share"></param>
        /// <returns></returns>
        IVirtualFileStream GetStream(VFSPath fileName, FileMode mode = FileMode.Open,
            FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.Read);

        /// <summary>
        ///     Get readonly file stream async of specified name.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        IReadonlyVirtualFileStreamRequest GetReadonlyStreamAsync(VFSPath fileName);

        /// <summary>
        ///     List all files in this file system.
        /// </summary>
        IList<VFSPath> List();

        /// <summary>
        ///     List all files in this file system.
        /// </summary>
        IList<VFSPath> List(IList<VFSPath> refs);
    }
}
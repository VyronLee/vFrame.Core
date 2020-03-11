using System.Collections.Generic;
using System.IO;

namespace vFrame.Core.FileSystems
{
    public interface IFileSystem
    {
        /// <summary>
        /// Open file system.
        /// </summary>
        /// <param name="streamPath">Working directory or package file path.</param>
        /// <returns></returns>
        void Open(Path streamPath);

        /// <summary>
        /// Close file system.
        /// </summary>
        /// <returns></returns>
        void Close();

        /// <summary>
        /// Is file with relative path exist?
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        bool Exist(Path relativePath);

        /// <summary>
        /// Get file stream of specified name.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        Stream GetStream(Path fileName, FileMode mode = FileMode.Open);

        /// <summary>
        /// List all files in this file system.
        /// </summary>
        IList<Path> List();

        /// <summary>
        /// List all files in this file system.
        /// </summary>
        IList<Path> List(IList<Path> refs);
    }
}
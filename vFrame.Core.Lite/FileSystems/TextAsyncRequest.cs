using System;
using vFrame.Core.FileSystems.Constants;
using vFrame.Core.Loggers;
using vFrame.Core.MultiThreading;

namespace vFrame.Core.FileSystems
{
    public class TextAsyncRequest : ThreadedAsyncRequest<string, string>, ITextAsyncRequest
    {
        internal IFileSystemManager _fileSystemManager { get; set; }

        protected override string OnThreadedHandle(string arg) {
            if (null == _fileSystemManager) {
                throw new ArgumentNullException("FileSystemManager cannot be null");
            }
            return _fileSystemManager.ReadAllText(arg);
        }

        protected override void ErrorHandler(Exception e) {
            Logger.Error(FileSystemConst.LogTag, "Exception occurred while reading: {0}", Arg);
        }
    }
}
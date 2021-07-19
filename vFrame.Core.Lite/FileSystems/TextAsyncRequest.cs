using System;
using vFrame.Core.FileSystems.Constants;
using vFrame.Core.Loggers;
using vFrame.Core.ThreadPools;

namespace vFrame.Core.FileSystems
{
    public class TextAsyncRequest : ThreadedAsyncRequest<string, string>, ITextAsyncRequest
    {
        internal IFileSystemManager _fileSystemManager { get; set; }

        public TextAsyncRequest() {
            ParallelCount = 4;
        }

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
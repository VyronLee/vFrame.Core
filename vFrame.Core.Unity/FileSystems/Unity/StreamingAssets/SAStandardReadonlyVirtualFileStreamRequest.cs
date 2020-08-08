﻿using System.IO;
using UnityEngine.Networking;
using vFrame.Core.FileSystems.Constants;
using vFrame.Core.Loggers;
using vFrame.Core.Utils;

namespace vFrame.Core.FileSystems.Unity.StreamingAssets
{
    internal class SAStandardReadonlyVirtualFileStreamRequest : ReadonlyVirtualFileStreamRequest
    {
        private UnityWebRequest _request;
        private bool _failed;
        private bool _finished;

        public SAStandardReadonlyVirtualFileStreamRequest(string path) {
            var absolutePath = PathUtils.RelativeStreamingAssetsPathToAbsolutePath(path);
            _request = UnityWebRequest.Get(absolutePath);
        }

        public override bool MoveNext() {
            if (_finished || _failed) {
                return false;
            }

            if (_request.isDone) {
                Stream = new SAStandardVirtualFileStream(
                    new MemoryStream(_request.downloadHandler.data));

                _request.Dispose();
                _request = null;
                _finished = true;
                return false;
            }

            if (_request.isNetworkError || _request.isHttpError) {
                Logger.Error(FileSystemConst.LogTag, "Error occurred while reading file: {0}", _request.error);
                _failed = true;
                return false;
            }

            return true;
        }
    }
}
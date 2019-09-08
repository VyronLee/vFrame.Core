//------------------------------------------------------------
//        File:  FileReaderRequest.cs
//       Brief:  FileReaderRequest
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-05-14 10:48
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System.IO;
using vFrame.Core.Crypto;
using vFrame.Core.Loggers;
using vFrame.Core.ThreadPools;

namespace vFrame.Core.FileReader
{
    public class FileReaderRequest : FileReader, IFileReaderRequest
    {
        private const int MaxThread = 5;

        private static ThreadPool _threadPool;
        private static ThreadPool ThreadPool
        {
            get
            {
                if (_threadPool == null)
                {
                    _threadPool = new ThreadPool();
                    _threadPool.Create(MaxThread);
                }
                return _threadPool;
            }
        }

        private readonly object _lockObject = new object();
        private readonly string _path;

        private byte[] _buffer;
        private bool _finished;

        public FileReaderRequest(string path)
        {
            _path = path;
            _finished = false;

            ReadInternal();
        }

        public FileReaderRequest(string path, ICryptoService crypto, byte[] key, int keyLength)
            :base(crypto, key, keyLength)
        {
            _path = path;
            _finished = false;

            ReadInternal();
        }

        private void ReadInternal()
        {
            if (!IsFileExist(_path))
                throw new FileNotFoundException(_path);

            ThreadPool.AddTask(ReadBytesAndDecrypt, null, OnException);
        }

        private void OnException(System.Exception e)
        {
            Logger.Error("!!! Read file failed: {0}, exception: {1}", _path, e);
        }

        private void ReadBytesAndDecrypt(object stateInfo)
        {
            Logger.Info("Read buffer started: {0}", _path);

            _buffer = ReadAllBytes(_path);

            lock (_lockObject)
                _finished = true;

            Logger.Info("Read buffer ended: {0}", _path);
        }

        public byte[] GetBytes()
        {
            return _buffer;
        }

        public bool MoveNext()
        {
            lock (_lockObject)
                return !_finished;
        }

        public void Reset()
        {

        }

        public object Current { get; private set; }

        public bool IsDone
        {
            get { return !MoveNext(); }
        }

        public float Progress
        {
            get
            {
                return _finished ? 1f : 0f;
            }
        }
    }
}
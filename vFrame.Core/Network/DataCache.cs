//------------------------------------------------------------
//        File:  DataCache.cs
//       Brief:  数据缓冲池
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-03-14 15:29
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System;
using vFrame.Core.Log;
using Logger = vFrame.Core.Log.Logger;

namespace vFrame.Core.Network
{
    public class DataCache
    {
        public int Head { private set; get; }
        private int Tail { set; get; }

        public byte[] Buffer { private set; get; }
        private int _buffSize;

        public DataCache()
        {
            _buffSize = sizeof(byte) * NetworkDef.DATA_CACHE_INIT_SIZE;
            Buffer = new byte[NetworkDef.DATA_CACHE_INIT_SIZE];

            Head = Tail = 0;
        }

        public void AppendData(byte[] data, int len)
        {
            // buffer 大小不足，首先尝试重新布局
            if (Tail + len > _buffSize)
                Rearrange();

            // 仍然不足，重新分配内存
            if (Tail + len > _buffSize)
                Resize(Tail + len);

            Array.Copy(data, 0, Buffer, Tail, len);

            Tail += len;

            Logger.Verbose("Network", "DataCache appendData, size: {0}, total: {1}",len, Tail - Head);
        }

        public void Skip(int size)
        {
            Head += size;

            Logger.Verbose("Network", "DataCache skip: {0}, total left: {1}", size, Tail - Head);
        }

        private void Rearrange()
        {
            if (Head <= 0)
                return;

            Array.Copy(Buffer, Head, Buffer, 0, Tail - Head);

            Tail = Tail - Head;
            Head = 0;

            Logger.Verbose("Network", "Buffer rearrange, new tail: {0}", Tail);
        }

        private void Resize(int sizeRequired)
        {
            // Resize策略：每次重分配都是之前的2倍大小
            var newBuffSize = _buffSize;
            while (newBuffSize < sizeRequired)
            {
                newBuffSize *= 2;
            }

            byte[] newBuffer;
            try
            {
                newBuffer = new byte[newBuffSize];
            }
            catch (Exception e)
            {
                Logger.Fatal("Network",
                    "Resize data cache buffer failed, new size: {0}, exception: {1}", newBuffSize, e);
                throw;
            }

            Array.Copy(Buffer, 0, newBuffer, 0, _buffSize);

            Logger.Verbose("Network", "Buffer resize, old size: {0}, new size: {1}", _buffSize, newBuffSize);

            Buffer = newBuffer;
            _buffSize = newBuffSize;
        }

        public int DataSize
        {
            get { return Tail - Head;}
        }
    }
}
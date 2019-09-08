//------------------------------------------------------------
//        File:  MessageQueue.cs
//       Brief:  消息队列
//
//      Author:  VyronLee, lwz_jz@Hotmail.com
//
//     Modified:  2019-04-01 20:12
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System;
using System.Collections.Generic;
using vFrame.Core.Log;
using Logger = vFrame.Core.Log.Logger;

namespace vFrame.Core.Network
{
    public class MessageQueue : IDisposable
    {
        public class STMessage : IDisposable
        {
            public int Size { protected internal set; get; }
            public byte[] Body { internal set; get; }

            internal MessageQueue Queue;

            public void Dispose()
            {
                Queue.RecycleNode(this);
            }
        }

        private readonly Dictionary<int, LinkedList<STMessage>> _nodes;
        private readonly Queue<STMessage> _message;

        private readonly object _messageQueueLock = new object();
        private readonly object _messageSTLock = new object();

        public MessageQueue()
        {
            _nodes = new Dictionary<int, LinkedList<STMessage>>(256);
            _message = new Queue<STMessage>(64);
        }

        public void Dispose()
        {
            _nodes.Clear();
            _message.Clear();
        }

        public void PushMessage(byte[] buff, int begin, int size)
        {
            var index = GetSlotIndex(size);

            STMessage stMessage = null;
            lock (_messageSTLock)
            {
                LinkedList<STMessage> msgList;
                if (!_nodes.TryGetValue(index, out msgList))
                    _nodes.Add(index, msgList = new LinkedList<STMessage>());

                if (msgList.Count > 0)
                {
                    stMessage = msgList.Last.Value;
                    msgList.RemoveLast();

                    Logger.Verbose("Network",
                        "PushMessage, size: {0}, slot index: {1}, pop from cache!", size, index);
                }
            }

            if (null == stMessage)
            {
                stMessage = new STMessage {Size = index};
                stMessage.Body = new byte[stMessage.Size];
                stMessage.Queue = this;

                Logger.Verbose("Network",
                    "PushMessage, size: {0}, slot index: {1}, no struct in cache, generate new one!", size, index);
            }
            stMessage.Size = size;

            Array.Copy(buff, begin, stMessage.Body, 0, size);

            lock (_messageQueueLock)
            {
                _message.Enqueue(stMessage);

                Logger.Verbose("Network",
                    "PushMessage, size: {0}, slot index: {1}, total in queue: {2}", size, index, _message.Count);
            }
        }

        public STMessage PopMessage()
        {
            STMessage msg;
            lock (_messageQueueLock)
            {
                msg = _message.Count > 0 ? _message.Dequeue() : null;

                if (null != msg)
                    Logger.Verbose("Network", "PopMessage, size: {0}, total in queue: {1}",
                        msg.Size, _message.Count);
            }

            return msg;
        }

        private void RecycleNode(STMessage node)
        {
            var index = GetSlotIndex(node.Size);

            lock (_messageSTLock)
            {
                LinkedList<STMessage> msgList;
                if (!_nodes.TryGetValue(index, out msgList))
                    _nodes.Add(index, msgList = new LinkedList<STMessage>());

                msgList.AddLast(node);
            }

            Logger.Verbose("Network", "Recycle Message, slot index: {0}", index);
        }

        private static int GetSlotIndex(int size)
        {
            var i = 1;
            while (i < size)
                i *= 2;
            return i;
        }
    }
}
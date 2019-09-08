using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using vFrame.Core.Log;
using Logger = vFrame.Core.Log.Logger;

namespace vFrame.Core.Network
{
    public class TcpClient
    {
        private class StateObject
        {
            public readonly byte[] buffer = new byte[NetworkDef.STREAM_BUFF_SIZE];
            public Socket socket;
        }

        private enum ConnectStatus
        {
            CLOSED,
            CONNECTING,
            CONNECTED
        }

        private volatile ConnectStatus _status;
        private readonly StateObject _stateObject;
        private readonly MemoryStream _sendStream;

        private Socket _socket;
        private DataCache _dataCache;

        public event Action OnClose;
        public event Action OnServerClose;
        public event Action<byte[], int, int> OnMessage;

        public bool Connected
        {
            get { return _status == ConnectStatus.CONNECTED; }
        }

        public TcpClient()
        {
            _status = ConnectStatus.CLOSED;
            _stateObject = new StateObject();

            _sendStream = new MemoryStream(NetworkDef.SEND_STREAM_BUFF_SIZE);
        }

        //会堵塞当前线程
        public void Connect(string host, int port)
        {
            if (_status != ConnectStatus.CLOSED)
            {
                Logger.Warning("Network", "_status != ConnectStatus.CLOSED");
                return;
            }

            _status = ConnectStatus.CONNECTING;
            _dataCache = new DataCache();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {NoDelay = true};

            try
            {
                var result = _socket.BeginConnect(host, port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(NetworkDef.CONNECT_TIME_OUT, true);
                if (success)
                {
                    _socket.EndConnect(result);
                    _status = ConnectStatus.CONNECTED;

                    Receive();
                }
                else
                {
                    throw new TimeoutException("Connect TimeOut");
                }
            }
            catch (Exception e)
            {
                Logger.Warning("Network", "TcpClient:Connect exception: {0}", e);
                _socket.Close();
                _status = ConnectStatus.CLOSED;
            }
        }

        public void SendMessage(byte[] data, int dataSize)
        {
            if (_status != ConnectStatus.CONNECTED)
            {
                Logger.Warning("Network", "Not connected");
                return;
            }

            SerializeDataToSend(data, dataSize);
            try
            {
                Logger.Verbose("Network", "Send message, length: {0}", _sendStream.Length);
                _socket.BeginSend(_sendStream.ToArray(), 0, (int)_sendStream.Length, SocketFlags.None, EndSend, _socket);
            }
            catch (Exception e)
            {
                Logger.Warning("Network", "TcpClient:SendMessage exception: {0}", e);
                NotifyClose();
            }
        }

        private void EndSend(IAsyncResult ar)
        {
            var socket = (Socket) ar.AsyncState;
            if (!socket.Connected)
                return;

            try
            {
                socket.EndSend(ar);
            }
            catch (Exception e)
            {
                Logger.Warning("Network", "TcpClient:EndSend exception: {0}", e);
                NotifyClose();
            }
        }

        private void Receive()
        {
            if (_status != ConnectStatus.CONNECTED)
            {
                return;
            }

            try
            {
                _stateObject.socket = _socket;
                _socket.BeginReceive(_stateObject.buffer, 0, NetworkDef.STREAM_BUFF_SIZE, 0, EndReceive,
                    _stateObject);
            }
            catch (Exception e)
            {
                Logger.Warning("Network", "TcpClient:Receive exception: {0}", e);
                NotifyClose();
            }
        }

        private void EndReceive(IAsyncResult ar)
        {
            var state = (StateObject) ar.AsyncState;
            var socket = state.socket;
            if (!socket.Connected)
                return;

            try
            {
                var bytesRead = socket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    ProcessReceivedBytes(state.buffer, bytesRead);
                    Receive();
                }
                else
                {
                    Logger.Warning("Network", "Receive data length: 0, disconnect by server.");
                    NotifyServerClose();
                }
            }
            catch (Exception e)
            {
                Logger.Warning("Network", "TcpClient:EndReceive exception: {0}", e);
                // NotifyClose();
            }
        }

        public void Close()
        {
            if (_status == ConnectStatus.CLOSED)
            {
                return;
            }

            _status = ConnectStatus.CLOSED;

            _socket.Close();
            _socket = null;
        }

        private void ProcessReceivedBytes(byte[] buffer, int bytesRead)
        {
            // 添加到消息缓存中
            _dataCache.AppendData(buffer, bytesRead);

            // 可能会有多个包，需要循环处理，直至无法粘包
            while (true)
            {
                // 获取包体长度数据，4字节，不足则等待
                var dataSize = _dataCache.DataSize;
                if (dataSize < NetworkDef.DATA_BODY_FLAG_SIZE)
                    break;

                // 包体长度解析
                var bodySize = BitConverter.ToInt32(_dataCache.Buffer, _dataCache.Head);
                bodySize = IPAddress.NetworkToHostOrder(bodySize);
                if (bodySize < 0)
                {
                    Logger.Error("Network", "Invalid body size: " + bodySize);
                    break;
                }

                // 获取包体数据，不足则等待
                if (dataSize < NetworkDef.DATA_BODY_FLAG_SIZE + bodySize)
                    break;

                // 包体数据足够，进行解析
                if (OnMessage != null)
                    OnMessage(_dataCache.Buffer, _dataCache.Head + NetworkDef.DATA_BODY_FLAG_SIZE, bodySize);

                // 数据包使用完后，指向下一个包的位置
                _dataCache.Skip(NetworkDef.DATA_BODY_FLAG_SIZE + bodySize);
            }
        }

        private void SerializeDataToSend(byte[] data, int dataSize)
        {
            // 消息体长度转化
            var lenNet = IPAddress.HostToNetworkOrder(dataSize);
            var lenByte = BitConverter.GetBytes(lenNet);

            _sendStream.SetLength(0);
            _sendStream.Seek(0, SeekOrigin.Begin);
            _sendStream.Write(lenByte, 0, lenByte.Length);
            _sendStream.Write(data, 0, dataSize);

            //var destinationArray = new byte[dataSize];
            //Array.Copy(data, 0, destinationArray, 0, dataSize);
            //Logger.Info("Network",
            //    "local order size: {0}, network order size: {1}, size bytes: {2}, data bytes: {3}",
            //    dataSize,
            //    lenByte.Length,
            //    BitConverter.ToString(lenByte).Replace("-", string.Empty),
            //    BitConverter.ToString(destinationArray).Replace("-", string.Empty));
        }

        private void NotifyClose()
        {
            // 手动关闭连接的时候也会触发到这里
            if (_status == ConnectStatus.CLOSED)
            {
                return;
            }

            Close();

            if (OnClose != null)
            {
                OnClose();
            }
        }

        private void NotifyServerClose()
        {
            if (_status == ConnectStatus.CLOSED)
            {
                return;
            }

            Close();

            if (OnServerClose != null)
            {
                OnServerClose();
            }
        }
    }
}
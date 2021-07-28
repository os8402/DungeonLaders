using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{

    public class PacketSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
    
        }

        public override void OnDisconnected(EndPoint endPoint)
        {

        }

        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int n = 0;
            return n;
        }

        public override void OnSend(int numOfBytes)
        {
       
        }
    }

    public abstract class Session
    {

        Socket _socket;
        int _disConnected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(65535); 

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();


        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnSend(int numOfBytes);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnDisconnected(EndPoint endPoint);


        public void Start(Socket socket)
        {
            _socket = socket;

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            RegisterRecv();

        }

        void Clear()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }


        public void DisConnect()
        {
            if (_disConnected == 1)
                return;

            if (Interlocked.Exchange(ref _disConnected, 1) == 1)
                return;


            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();

        }
        public void Send(List<ArraySegment<byte>> sendBufferList)
        {
            lock(_lock)
            {
                if (sendBufferList.Count == 0)
                    return;

                foreach (ArraySegment<byte> buffer in sendBufferList)
                    _sendQueue.Enqueue(buffer);


                if (_sendQueue.Count == 0)
                    RegisterSend();
            }
       
        }

        public void Send(ArraySegment<byte> sendBuffer)
        {
            lock(_lock)
            {
                _sendQueue.Enqueue(sendBuffer);
                if (_sendQueue.Count == 0)
                    RegisterSend();
            }

      
        }


        void RegisterSend()
        {

            if (_disConnected == 1)
                return;


            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buffer = _sendQueue.Dequeue();
                _pendingList.Add(buffer);

            }

            _sendArgs.BufferList = _pendingList;

            try
            {
                bool pending = _socket.SendAsync(_sendArgs);
                if (pending == false)
                    OnSendCompleted(null, _sendArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"OnSendCompleted Failed {e}");
            }


        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock(_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {

                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(args.BytesTransferred);

                        if (_sendQueue.Count > 0)
                            RegisterSend();

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }

                }
                else
                {
                    DisConnect();
                }
            }
           
        }

        void RegisterRecv()
        {
            if (_disConnected == 1)
                return;

            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array , segment.Offset ,segment.Count);

            try
            {
                bool pending = _socket.ReceiveAsync(_recvArgs);
                if (pending == false)
                    OnRecvCompleted(null, _recvArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterRecv Failed {e}");
            }

        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.BytesTransferred > 0  && args.SocketError == SocketError.Success)
            {
                try
                {
                    if(_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        DisConnect();
                        return;
                    }

                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if (processLen < 0 ||  _recvBuffer.DataSize < processLen)
                    {
                        DisConnect();
                        return;
                    }

                    if(_recvBuffer.OnRead(processLen) == false)
                    {
                        DisConnect();
                        return; 
                    }

                    RegisterRecv(); 

               
                }
                catch(Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }


            }
            else
            {
                DisConnect();
            }
        }
    }
}

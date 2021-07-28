using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    public class Listener
    {
        Socket _socket;
        Func<Session> _sessionFactory;

        public void Init(IPEndPoint endPoint , Func<Session> sessionFactory , int register = 10 ,  int  backlog = 100)
        {

            _socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            _socket.Bind(endPoint);

            _socket.Listen(backlog);


            for(int i = 0; i < register; i++)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += OnAcceptCompleted;
                RegisterAccept(args); 
            }

        }
        
        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null; 

            try
            {
                bool pending = _socket.AcceptAsync(args);
                if (pending == false)
                    OnAcceptCompleted(null, args); 

            }
            catch (Exception e)
            {
                Console.WriteLine(e) ;
            }
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                if(args.SocketError == SocketError.Success)
                {
                    Session session = _sessionFactory.Invoke();
                    session.Start(args.AcceptSocket);
                    session.OnConnected(args.AcceptSocket.RemoteEndPoint);

                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            RegisterAccept(args); 


        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using GameServer.Game;
using GameServer.Data;

namespace GameServer
{
	public partial class ClientSession : PacketSession
	{
		public PlayerServerState ServerState { get; private set; } = PlayerServerState.ServerStateLogin;

		public Player MyPlayer { get; set; }
		public int SessionId { get; set; }

		object _lock = new object();
		List<ArraySegment<byte>> _reserveQueue = new List<ArraySegment<byte>>();

		long _pingpongTick = 0; 
		public void Ping()
        {
			if(_pingpongTick > 0)
            {
				long delta = (System.Environment.TickCount64 - _pingpongTick);
				if(delta > 30 * 1000)
                {
                    Console.WriteLine("Disconnected By PingCheck");
					Disconnect();
					return;
                }
            }

			S_Ping pingPacket = new S_Ping();
			Send(pingPacket);

			GameLogic.Instance.PushAfter(5000, Ping);
        }
		public void HandlePong()
        {
			_pingpongTick = System.Environment.TickCount64;

		}


		//예약만 
        #region Network
        public void Send(IMessage packet)
        {

			string msgName =  packet.Descriptor.Name.Replace("_", string.Empty);
			MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId) , msgName);
			
			ushort size = (ushort)packet.CalculateSize();
			byte[] sendBuffer = new byte[size + 4];
			Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
			Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
			Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

			_reserveQueue.Add(sendBuffer);
		}

		//실제 네트워크 IO 처리
		public void FlushSend()
        {
			List<ArraySegment<byte>> sendList = null;

			lock(_lock)
            {
				if (_reserveQueue.Count == 0)
					return;

				sendList = _reserveQueue;
				_reserveQueue = new List<ArraySegment<byte>>();

			}

			Send(sendList);
		}

		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");

            {
				S_Connected connectedPacket = new S_Connected();
				Send(connectedPacket);
            }

			GameLogic.Instance.PushAfter(5000, Ping);

		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
            GameLogic.Instance.Push(() =>
            {
				if (MyPlayer == null)
					return;

				GameRoom room = GameLogic.Instance.Find(1);
                room.Push(room.LeaveGame, MyPlayer.Info.ObjectId);
            });

			Console.WriteLine($"OnDisconnected : {endPoint}");

			SessionManager.Instance.Remove(this);


		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
        #endregion
    }
}

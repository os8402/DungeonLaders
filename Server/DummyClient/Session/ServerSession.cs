using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

public class ServerSession : PacketSession
{
    public int DummyId { get; set; }

    // AccountServer 로그인으로 받은 입장 자격 (SessionManager.Generate 에서 채운다)
    public int AccountDbId { get; set; }
    public int Token { get; set; }

    // [2026-09 추가] 왕복 시간 측정용 송신 시각 (Environment.TickCount64)
    public long LoginSentTick { get; set; }
    public long EnterSentTick { get; set; }
    public void Send(IMessage packet)
    {

        string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
        MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);

        ushort size = (ushort)packet.CalculateSize();
        byte[] sendBuffer = new byte[size + 4];
        Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
        Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
        Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

        Send(new ArraySegment<byte>(sendBuffer));
    }
    public override void OnConnected(EndPoint endPoint)
    {
       // Console.WriteLine($"OnConnected : {endPoint}");
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        // 원래는 아무것도 하지 않아 "Connected (N)" 수가 끊겨도 줄지 않았다
        DummyClient.Session.SessionManager.Instance.Remove(this);
        DummyClient.LoadStats.RecordDisconnect();
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketManager.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnSend(int numOfBytes)
    {
        //Console.WriteLine($"Transferred bytes: {numOfBytes}");
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace DummyClient.Session
{
    public class SessionManager
    {
        public static SessionManager Instance { get; } = new SessionManager();

        HashSet<ServerSession> _sessions = new HashSet<ServerSession>();
        object _lock = new object();

        int _dummyId = 1;

        public int Count { get { lock (_lock) { return _sessions.Count; } } }

        public ServerSession Generate()
        {
            lock (_lock)
            {
                ServerSession session = new ServerSession();
                session.DummyId = _dummyId;
                _dummyId++;

                // AccountServer 에서 받아둔 (AccountDbId, Token) 을 DummyId 순서로 배정한다
                int infoIdx = session.DummyId - 1;
                if (infoIdx < Program.LoginInfos.Length && Program.LoginInfos[infoIdx] != null)
                {
                    session.AccountDbId = Program.LoginInfos[infoIdx].AccountDbId;
                    session.Token = Program.LoginInfos[infoIdx].Token;
                }

                _sessions.Add(session);
                Console.WriteLine($"Connected ({_sessions.Count}) Players");
                return session;
            }
        }

        public void Remove(ServerSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);
                Console.WriteLine($"Connected ({_sessions.Count}) Players");

            }

        }

    }

}



using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game
{
    public class GameObject
    {
        public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
        public int Id
        {
            get { return Info.ObjectId; }
            set { Info.ObjectId = value; }
        }
        public GameRoom Room { get; set; }

        public ObjectInfo Info { get; set; } = new ObjectInfo();

        public PositionInfo PosInfo { get; private set; } = new PositionInfo();
        public WeaponInfo WeaponInfo { get; private set; } = new WeaponInfo();
        public EquipWeapon EquipWeapon { get; set; }
        public StatInfo Stat { get; private set; } = new StatInfo();

        public float Speed
        {
            get { return Stat.Speed; }
            set { Stat.Speed = value; }
        }

        public ControllerState State
        {
            get { return PosInfo.State; }
            set { PosInfo.State = value; }
        }
        public DirState Dir
        {
            get { return PosInfo.Dir; }
            set { PosInfo.Dir = value; }

        }

        public GameObject()
        {
            Info.PosInfo = PosInfo;
            Info.WeaponInfo = WeaponInfo;
            Info.StatInfo = Stat;

        }

        public Vector2Int CellPos
        {
            get
            {
                return new Vector2Int(PosInfo.PosX, PosInfo.PosY);
            }
            set
            {
                PosInfo.PosX = value.x;
                PosInfo.PosY = value.y;

            }
        }
        public DirState GetDirState(Vector2Int dir)
        {
            return GetDirState(dir.x, dir.y);
        }

        public DirState GetDirState(int posX, int posY)
        {
            DirState state = DirState.Up;

            if (posX == 0 && posY == 1)
                state = DirState.Up;
            else if (posX == 0 && posY == -1)
                state = DirState.Down;
            else if (posX == -1 && posY == 0)
                state = DirState.Left;
            else if (posX == 1 && posY == 0)
                state = DirState.Right;
            else if (posX == -1 && posY == 1)
                state = DirState.UpLeft;
            else if (posX == -1 && posY == -1)
                state = DirState.DownLeft;
            else if (posX == 1 && posY == -1)
                state = DirState.DownRight;
            else if (posX == 1 && posY == 1)
                state = DirState.UpRight;

            return state;
        }

        long _nextDeadTick = 0;
        public virtual void Update()
        {
            //if(_deadFlag)
            //{
            //    if (_nextDeadTick > Environment.TickCount64)
            //        return;

            //        ReviveGameObject();

            //}
        }

        public virtual void OnDamaged(GameObject attacker, int damage)
        {
            if (Stat.Hp <= 0)
                return;

            Stat.Hp = Math.Max(Stat.Hp - damage, 0);

            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = Id;
            changePacket.Hp = Stat.Hp;
            changePacket.AttackerId = attacker.Id;
            Room.BroadCast(changePacket);

            if (Stat.Hp <= 0)
            {
                OnDead(attacker);
            }

        }

        bool _deadFlag = false;

        public virtual void OnDead(GameObject attacker)
        {
            Console.WriteLine($"Player_{attacker.Id} -> Player{Id} Kill!");

            S_Die diePacket = new S_Die();
            diePacket.ObjectId = Id;
            diePacket.AttackerId = attacker.Id;
            Room.BroadCast(diePacket);

            GameRoom room = Room;
            room.LeaveGame(Id);

            Stat.Hp = Stat.MaxHp;
            PosInfo.State = ControllerState.Idle;
            PosInfo.Dir = DirState.Right;
            PosInfo.PosX = 0;
            PosInfo.PosY = 0;

            room.EnterGame(this);

        }

        void ReviveGameObject()
        {
            _deadFlag = false;

            GameRoom room = Room;
            room.LeaveGame(Id);

            Stat.Hp = Stat.MaxHp;
            PosInfo.State = ControllerState.Idle;
            PosInfo.Dir = DirState.Right;
            PosInfo.PosX = 0;
            PosInfo.PosY = 0;

            room.EnterGame(this);


        }
    }
}

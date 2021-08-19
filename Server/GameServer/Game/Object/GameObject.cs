
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
        public PositionInfo PosInfo { get; private set; } = new PositionInfo { Target = new TargetInfo() };
        public WeaponInfo WeaponInfo { get; private set; } = new WeaponInfo();
        public EquipWeapon EquipWeapon { get; set; }
        public StatInfo Stat { get; private set; } = new StatInfo();

        public virtual int TotalAttack { get { return Stat.Attack; } }
        public virtual int TotalDefence { get { return 0; } }
            

        public float Speed
        {
            get { return Stat.Speed; }
            set { Stat.Speed = value; }
        }
        public int HP
        {
            get { return Stat.Hp; }
            set { Stat.Hp = Math.Clamp(value, 0 , Stat.MaxHp); }
        }

        public ControllerState State
        {
            get { return PosInfo.State; }
            set { PosInfo.State = value; }
        }
        public TargetInfo TargetInfo
        {
            get { return PosInfo.Target; }
            set { PosInfo.Target = value;  }
        }
        public Vector2Int TargetPos
        {
            get { return new Vector2Int((int)PosInfo.Target.TargetX , (int)PosInfo.Target.TargetY); }
            set 
            { 
                PosInfo.Target.TargetX = value.x; 
                PosInfo.Target.TargetY = value.y; 
            }
        }

        public DirState Dir
        {
            get { return PosInfo.Target.Dir; }
            set { PosInfo.Target.Dir = value; }
        }

        public GameObject()
        {
            Info.PosInfo = PosInfo;
            Info.PosInfo.Target = TargetInfo; 
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
            int x = 0;
            int y = 0;

            if (dir.x != 0)
                x = dir.x / Math.Abs(dir.x);
            if (dir.y != 0)
                y = dir.y / Math.Abs(dir.y);

            return GetDirState(x, y);
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

        public virtual void Update()
        {
        }

        public virtual void OnDamaged(GameObject attacker, int damage)
        {
            if (Room == null)
                return;

            if (Stat.Hp <= 0)
                return;

            damage = Math.Max((damage - TotalDefence) , 0);

            Stat.Hp = Math.Max(Stat.Hp - damage, 0);

            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = Id;
            changePacket.Hp = Stat.Hp;
            changePacket.AttackerId = attacker.Id;
            Room.Broadcast(CellPos, changePacket);

            if (Stat.Hp <= 0)
            {
                OnDead(attacker);
            }
        }


        public virtual void OnDead(GameObject attacker)
        {
            if (Room == null)
                return;
                    
            Console.WriteLine($"{attacker.GetOwner().GetType().Name}_{attacker.Id} -> {GetType().Name}_{Id} Kill");

            State = ControllerState.Dead;

            S_Die diePacket = new S_Die();
            diePacket.ObjectId = Id;
            diePacket.AttackerId = attacker.Id;
            Room.Broadcast(CellPos, diePacket);
            GameRoom room = Room;
            room.PushAfter(2000, ReviveGameObject);
        }

        void ReviveGameObject()
        {
            if (Room == null)
                return;

            GameRoom room = Room;
            room.LeaveGame(Id);

            Stat.Hp = Stat.MaxHp;
            State = ControllerState.Idle;
            room.EnterGame(this , randomPos : true);

        }

        public virtual GameObject GetOwner()
        {
            return this; 
        }
    }
}

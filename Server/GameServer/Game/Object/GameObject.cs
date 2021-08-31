
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
     
        private EquipWeapon _equipWeapon; 
        public EquipWeapon EquipWeapon 
        {
            get { return _equipWeapon; }
            set 
            {
                _equipWeapon = value;
                if (_equipWeapon == null)
                    return;
                _equipWeapon.Owner = this;
                Info.WeaponId = _equipWeapon.Id;


            }
        }
      
        public StatInfo Stat { get; private set; } = new StatInfo();

        public virtual int TotalAttack { get { return Stat.Attack; } }
        public virtual int TotalDefence { get { return 0; } }
        public virtual int TotalHp
        {
            get { return Info.TotalHp; }           
            set { Info.TotalHp = value; }
        }


        public virtual float TotalSpeed
        {
            get { return Stat.Speed; }
            set { Stat.Speed = value; }
        }
        public int HP
        {
            get { Stat.Hp = Math.Clamp(Stat.Hp , 0 , TotalHp); return Stat.Hp; }
            set { Stat.Hp = Math.Clamp(value, 0 , TotalHp); }
        }
        public int Mp
        {
            get {  return Stat.Mp; }
            set { Stat.Mp = Math.Clamp(value, 0, Stat.MaxMp); }
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

        public virtual void Update() { }
        

        public virtual void OnDamaged(GameObject attacker, int damage)
        {
            if (Room == null)
                return;

            if (Stat.Hp <= 0)
                return;

            damage = Math.Max((damage - TotalDefence) , 0);

            HP = Math.Max(Stat.Hp - damage, 0);

            S_Damaged damagedPacket = new S_Damaged();
            damagedPacket.ObjectId = Id;
            damagedPacket.Hp = Stat.Hp;
            damagedPacket.Damage = damage;
            damagedPacket.AttackerId = attacker.Id;
            damagedPacket.TotalHp = TotalHp;
            Room.Broadcast(CellPos, damagedPacket);

            if (Stat.Hp <= 0)
            {
                OnDead(attacker);
            }
        }


        public virtual void OnDead(GameObject attacker)
        {
            if (Room == null)
                return;
                    
          //  Console.WriteLine($"{attacker.GetOwner().GetType()}_{attacker.Id} -> {GetType().Name}_{Id} Kill");
            Console.WriteLine($"{attacker.GetOwner().Info.Name}_{attacker.Id} -> {Info.Name}_{Id} Kill");

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

            Stat.Hp = TotalHp;

            State = ControllerState.Idle;
            room.EnterGame(this , randomPos : true);

        }

        public virtual GameObject GetOwner()
        {
            return this; 
        }
    }
}

using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class Player : GameObject
    {
     
        public ClientSession Session { get; set; }

        public Action<Player> _checkDeadTarget;

        public Player()
        {
            ObjectType = GameObjectType.Player;

        }

        public override void Update()
        {
           // base.Update();
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
       
        }
        public override void OnDead(GameObject attacker)
        {

            _checkDeadTarget.Invoke(this);
            base.OnDead(attacker);
        }

    }
}

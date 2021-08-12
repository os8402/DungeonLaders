using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public class Projectile : GameObject
    {
        public Projectile()
        {
            ObjectType = GameObjectType.Projectile;
        }

        public AttackPos AttackPos { get; set; }

        public virtual void Update() { } 
     
        public Vector2Int GetFrontCellPos()
        {
            Vector2Int cellPos = CellPos;

            cellPos += new Vector2Int(AttackPos.AttkPosX, AttackPos.AttkPosY);

            return cellPos;
        }

       
    }
}

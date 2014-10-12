using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CollisionManager;
using Microsoft.Xna.Framework;

namespace OmegaRace
{
    enum MsgType
    {
        ship,
        update,
        Collision,
        Missile,
        Bomb,
    }

    abstract class Message
    {

        public MsgType type;

        protected Message(MsgType type)
        {
            this.type = type;
        }

        public abstract void execute(Game1 g);
    }

    class ShipInputMsg : Message
    {
        public Vector2 dir;
        public float rot;
        public PlayerID pID;
        public FireMissile missile;
        public DropBomb bomb;


        public ShipInputMsg(PlayerID pID, float rot, Vector2 direction, FireMissile missile, DropBomb bomb)
            : base(MsgType.ship)
        {
            this.pID = pID;
            this.rot = rot;
            this.dir = direction;
            this.missile = missile;
            this.bomb = bomb;
        }

        public ShipInputMsg()
            : base(MsgType.ship)
        {
            this.dir = new Vector2(0,0);
            this.rot = -666;
            this.bomb = DropBomb.none;
            this.missile = FireMissile.none;
        }

        public override void execute(Game1 g)
        {
            Player p = PlayerManager.Instance().getPlayer(pID);
            Ship pShip = p.playerShip;

            if (rot != -666)
            {
                pShip.physicsObj.body.Rotation += rot;
            }
            pShip.physicsObj.body.ApplyLinearImpulse(dir, pShip.physicsObj.body.GetWorldCenter());

            if (bomb != DropBomb.none)
            {
                if (g.gamer.IsHost)
                {
                    GameObjManager.Instance().createBomb(pID);
                    CreateBombMsg msg = new CreateBombMsg(pID, -99);
                    OutputQueue.Instance.add(msg);
                }
                else
                {
                }
            }

            if (missile != FireMissile.none)
            {
                p.createMissile(g);
            }
        }
    }


    class UpdateMsg : Message
    {
        public Vector2 vel;
        public Vector2 loc;
        public PlayerID pID;
        public float rot;
        public float vRot;
        public int goid;

        public UpdateMsg()
            : base(MsgType.update)
        {
        }

        public UpdateMsg(int id, Vector2 loc, float rot, PlayerID pID, Vector2 vel, float vRot)
            : base(MsgType.update)
        {
            this.rot = rot;
            this.loc = loc;
            this.goid = id;
            this.pID = pID;
            this.vel = vel;
            this.vRot = vRot;
        }

        public override void execute(Game1 g)
        {
            GameObject go = GameObjManager.Instance().getGameObject(goid);
            if(go == null) return;
            go.MsgUpdate(loc, rot, vel, vRot, g);
        }
    }

    class CollisionMsg : Message
    {
        public Vector2 point;
        public int a;
        public int b;
        


        public CollisionMsg(int a, int b, Vector2 point)
            : base(MsgType.Collision)
        {
            this.a = a;
            this.b = b;
            this.point = point;
        }

        public override void execute(Game1 g)
        {
            GameObject A = GameObjManager.Instance().getGameObject(a);
            GameObject B = GameObjManager.Instance().getGameObject(b);
            if (A == null && B == null) return;
            A.Accept(B, point);
        }
    }

    class CreateMissileMsg : Message
    {
        public PlayerID pID;
        public int id;

        public CreateMissileMsg(PlayerID pID, int id)
            : base(MsgType.Missile)
        {
            this.id = id;
            this.pID = pID;
        }

        public override void execute(Game1 g)
        {
            Player player = PlayerManager.Instance().getPlayer(pID);
            player.createMissile(g);

        }
    }

    class CreateBombMsg : Message
    {
        public PlayerID pID;
        public int id;

        public CreateBombMsg(PlayerID pID, int id)
            : base(MsgType.Bomb)
        {
            this.pID = pID;
            this.id = id;
        }

        public override void execute(Game1 g)
        {
            GameObjManager.Instance().createBomb(pID);
        }
    }
}

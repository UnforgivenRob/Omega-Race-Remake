using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpriteAnimation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;
using OmegaRace;

namespace CollisionManager
{

    enum GameObjState
    {
        alive,
        dead
    }

    abstract class GameObject : Visitor
    {
        //public GOID id;
        public int id;
        private static int cnt = 0;

        public Sprite_Proxy spriteRef;

        public GameObjType type;

        public PhysicsObj physicsObj;

        public PlayerID pID;

        public bool CollideAvailable;

        // Speed is m/s 
        // Note the max speed of any object is 120m/s  /////////
        public static float MaxSpeed = 50;

        public Vector2 objSpeed;

        public float rotation;
        public Vector2 location;
        private UpdateMsg msg;

        //Prediction Stuff
        protected Vector2 oldLocation;
        protected float oldRotation;
        protected Vector2 velocity;
        protected float vRot;
        protected int frameSinceUpdate = 0;
        protected int framesDiff = 0;
        protected float currentSmoothing = 0;
        protected Vector2 newLocation;
        protected float newRotation;

        public GameObject(PlayerID pID)
        {
            rotation = 0;
            location = new Vector2();
            oldLocation = location;
            oldRotation = rotation;

            msg = new UpdateMsg();
            id = cnt;
            cnt++;
            this.pID = pID;
            this.CollideAvailable = true;
        }

        public static void resetCnt()
        {
            cnt = 0;
        }

        public virtual void Update()
        {
            this.spriteRef.pos = location;
            this.spriteRef.rotation = rotation;
        }

        public void setPhysicsObj(PhysicsObj _physObj)
        {
            physicsObj = _physObj;
        }

        public virtual void pushPhysics(float rot, Vector2 loc)
        {
            if (rot != rotation || loc != location)
            {
                oldLocation = location;
                oldRotation = rotation;
                rotation = rot;
                location = loc;

                msg.goid = id;
                msg.pID = pID;
                msg.loc = loc;
                msg.rot = rot;
                msg.vel = loc - oldLocation;
                msg.vRot = rot - oldRotation;
                OutputQueue.Instance.add(msg);
            }
        }

        public void MsgUpdate(Vector2 loc, float rot, Vector2 vel, float vRot, Game1 g)
        {
            oldLocation = location;
            oldRotation = rotation;

            velocity = vel;
            this.vRot = vRot;
            framesDiff = frameSinceUpdate;
            frameSinceUpdate = 0;
            currentSmoothing = 1;

            if (g.netPred.smoothing == OnOff.off)
            {
                location = loc;
                rotation = rot;
                physicsObj.body.Rotation = rotation;
                physicsObj.body.Position = location;
            }
            else
            {
                newRotation = rot;
                newLocation = loc;
            }
        }
    }
}

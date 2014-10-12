using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SpriteAnimation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Box2D.XNA;
using OmegaRace;

namespace CollisionManager
{

    enum ShipOp
    {
        rotate,
        accelerate,
        Update,
    }

    struct Ship_Data
    {
        public Vector2 pos;
        public ShipOp op;
        public float input;
        public float rotation;
        public GameObjType GOType;

        public Ship_Data(Vector2 pos, ShipOp op, float input, float rot, GameObjType GOtype)
        {
            this.pos = pos;
            this.op = op;
            this.input = input;
            this.rotation = rot;
            this.GOType = GOtype;

        }
    }



    class Ship: GameObject
    {

        WaveBank waveBank;
        SoundBank soundBank;
        UpdateMsg msg;

        public Ship(GameObjType _type, Sprite_Proxy _spriteRef)
            : base((_type == GameObjType.p1ship) ? PlayerID.one : PlayerID.two)
        {
            type = _type;
            spriteRef = _spriteRef;
            msg = new UpdateMsg();
            
            waveBank = WaveBankManager.WaveBank();
            soundBank = SoundBankManager.SoundBank();
            currentSmoothing = 1;
        }


        public override void Update()
        {
            Vector2 velocity = physicsObj.body.GetLinearVelocity();
            if (velocity.Length() > MaxSpeed)
                physicsObj.body.SetLinearVelocity((MaxSpeed / velocity.Length() * velocity));

            base.Update();
        }



        public override void Accept(GameObject other, Vector2 _man)
        {
            if (other == null) return;
            other.VisitShip(this, _man);
        }


        public override void VisitMissile(Missile m, Vector2 _man)
        {
            reactionToShip(this, m, _man);
        }

        public override void VisitBomb(Bomb b, Vector2 _man)
        {
            reactionToShip(this, b, _man);
        }

        public override void VisitWall(Wall w, Vector2 _man)
        {
            w.hit();
        }


        private void reactionToShip(Ship s, Missile m, Vector2 _man)
        {

            if (s.type == GameObjType.p1ship && m.type == GameObjType.p2missiles)
            {

                GameObjManager.Instance().addExplosion(s.spriteRef.pos, s.spriteRef.color);
                GameObjManager.Instance().remove(batchEnum.ships, s);
                GameObjManager.Instance().remove(batchEnum.missiles, m);

                hit(PlayerID.one);

                ScoreManager.Instance().p2Kill();

                playMissileHitSound();
                playShipHitSound();

                PlayerManager.Instance().getPlayer(m.owner).increaseNumMissiles();
            }

            else if (s.type == GameObjType.p2ship && m.type == GameObjType.p1missiles)
            {

                GameObjManager.Instance().addExplosion(s.spriteRef.pos, s.spriteRef.color);

                GameObjManager.Instance().remove(batchEnum.ships, s);
                GameObjManager.Instance().remove(batchEnum.missiles, m);

                hit(PlayerID.two);

                ScoreManager.Instance().p1Kill();

                playMissileHitSound();
                playShipHitSound();

                PlayerManager.Instance().getPlayer(m.owner).increaseNumMissiles();
            }
            else { }

            
            
        }


        private void reactionToShip(Ship s, Bomb b, Vector2 _man)
        {

            if (s.type == GameObjType.p1ship)
            {
                GameObjManager.Instance().remove(batchEnum.ships, s);
                BombManager.Instance().removeBomb(b, s.spriteRef.pos, s.spriteRef.color);

                hit(PlayerID.one);

                ScoreManager.Instance().p2Kill();

                playShipHitSound();
                playBombHitSound();
            }

            else if (s.type == GameObjType.p2ship)
            {
                GameObjManager.Instance().remove(batchEnum.ships, s);
                BombManager.Instance().removeBomb(b, s.spriteRef.pos, s.spriteRef.color);

                hit(PlayerID.two);

                ScoreManager.Instance().p1Kill();

                playShipHitSound();
                playBombHitSound();
            }
            else { }

            playShipHitSound();
        }

        public void hit(PlayerID _id)
        {
            PlayerManager.Instance().getPlayer(_id).state = PlayerState.dead;

            TimeSpan currentTime = Timer.GetCurrentTime();
            TimeSpan t_1 = currentTime.Add(new TimeSpan(0, 0, 0, 0, 600));
            CallBackData nodeData = new CallBackData(3, TimeSpan.Zero);
            nodeData.playerID = _id;

            Timer.Add(t_1, nodeData, PlayerManager.Instance().respawn);
        }

        private void playMissileHitSound()
        {
            Cue hit_Cue = soundBank.GetCue("Laser_Hit_Cue");
            hit_Cue.Play();
        }

        private void playShipHitSound()
        {
            Cue hit_Cue = soundBank.GetCue("Ship_Pop_Cue");
            hit_Cue.Play();
        }

        private void playBombHitSound()
        {
            Cue hit_Cue = soundBank.GetCue("Mine_Pop_Cue");
            hit_Cue.Play();
        }

        public void doWork(Ship_Data data)
        {
            switch (data.op)
            {
                case ShipOp.rotate:
                    physicsObj.body.Rotation += data.input;
                    break;
                case ShipOp.accelerate:
                    Vector2 direction = new Vector2((float)(Math.Cos(physicsObj.body.GetAngle())), (float)(Math.Sin(physicsObj.body.GetAngle())));
                    direction.Normalize();
                    direction *= data.input;
                    physicsObj.body.ApplyLinearImpulse(direction, physicsObj.body.GetWorldCenter());
                    break;
                case ShipOp.Update:
                    rotation = data.rotation;
                    location = data.pos;
                    break;
            }
        }

        public void UpdatePredSmooth(Game1 g)
        {
            if (frameSinceUpdate != 0)
            {

                if (g.netPred.prediction == OnOff.on)
                {
                    updatePrediction();
                }

                if (g.netPred.smoothing == OnOff.on)
                {
                    float smoothingdecay = 1.0f;
                    smoothingdecay = 1.0f / framesDiff;
                    currentSmoothing -= smoothingdecay;
                    if (currentSmoothing < 0) currentSmoothing = 0;
                    updateSmoothing();
                }
            }
            frameSinceUpdate++;
        }

        private void updatePrediction()
        {
            location += velocity;
            rotation += vRot;
            physicsObj.body.Rotation = rotation;
            physicsObj.body.Position = location;
            Debug.Assert(!float.IsNaN(location.X), "NaN");
            Debug.Assert(!float.IsNaN(location.Y), "NaN");
            Debug.Assert(!float.IsNaN(rotation), "NaN");
        }

        private void updateSmoothing()
        {
            location = Vector2.Lerp(newLocation, oldLocation, currentSmoothing);
            rotation = MathHelper.Lerp(newRotation, oldRotation, currentSmoothing);
            physicsObj.body.Rotation = rotation;
            physicsObj.body.Position = location;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmegaRace;
using CollisionManager;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework;

namespace OmegaRace
{


    class InputQueue
    {
        private PacketReader packetReader;
        private static readonly InputQueue instance = new InputQueue();
        private Queue<Queue_Data> inQueue;
        private int SequenceNumber;
        private int FramesSincePacket;

        private InputQueue()
        {
            packetReader = new PacketReader();
            inQueue = new Queue<Queue_Data>();
            SequenceNumber = 9000;
            FramesSincePacket = 0;
        }

        public static InputQueue Instance
        {
            get
            {
                return instance;
            }
        }

        public void add(Queue_Data data)
        {
            data.inSeqNum = SequenceNumber;
            inQueue.Enqueue(data);
            SequenceNumber++;
        }

        public void process(Game1 g)
        {
            int cnt = inQueue.Count;
            while (cnt > 0)
            {
                Queue_Data data = inQueue.Dequeue();
                if (g.gamer.IsHost)
                {
                    ((Message)data.obj).execute(g);
                    cnt--;
                }
                else
                {
                    switch (data.type)
                    {
                        case MsgType.ship:
                            if (g.getCurrentPlayerID() == ((ShipInputMsg)data.obj).pID)
                            {
                                ((Message)data.obj).execute(g);

                            }
                            else
                            {
                                OutputQueue.Instance.add((ShipInputMsg)data.obj);
                            }
                            cnt--;
                            break;
                        case MsgType.update:
                            if (g.gamer.IsHost)
                            {
                                 ((Message)data.obj).execute(g);
                            }
                            else
                            {   //client
                                FramesSincePacket++;
                                if (FramesSincePacket >= g.netPred.framesBetweenPacket)
                                {
                                    FramesSincePacket = 0;
                                    ((Message)data.obj).execute(g);
                                }
                            }
                            cnt--;
                            break;
                        default:
                            ((Message)data.obj).execute(g);
                            cnt--;
                            break;

                    }
                }
            }
        }

        public void readData(LocalNetworkGamer gamer)
        {
            while (gamer.IsDataAvailable)
            {
                NetworkGamer sender;
                gamer.ReceiveData(packetReader, out sender);

                Queue_Data data;

                data.inSeqNum = packetReader.ReadInt32();
                data.outSeqNum = packetReader.ReadInt32();
                data.type = (MsgType)packetReader.ReadInt32();

                switch (data.type)
                {
                    case MsgType.ship:
                        Vector2 dir = packetReader.ReadVector2();
                        float rot = packetReader.ReadSingle();
                        PlayerID pID = (PlayerID)packetReader.ReadInt32();
                        FireMissile missile = (FireMissile)packetReader.ReadInt32();
                        DropBomb bomb = (DropBomb)packetReader.ReadInt32();
                        ShipInputMsg sMsg = new ShipInputMsg(pID, rot, dir, missile, bomb);
                        data.obj = sMsg;
                        inQueue.Enqueue(data);
                        break;
                    case MsgType.update:
                        Vector2 loc = packetReader.ReadVector2();
                        Vector2 vel = packetReader.ReadVector2();
                        float vrot = packetReader.ReadSingle();
                        float rota = packetReader.ReadSingle();
                        int gid = packetReader.ReadInt32();
                        PlayerID pId = (PlayerID)packetReader.ReadInt32();
                        UpdateMsg uMsg = new UpdateMsg(gid, loc, rota, pId, vel, vrot);
                        data.obj = uMsg;
                        inQueue.Enqueue(data);
                        break;
                    case MsgType.Collision:
                        Vector2 point = packetReader.ReadVector2();
                        int a = packetReader.ReadInt32();
                        int b = packetReader.ReadInt32();
                        CollisionMsg cMsg = new CollisionMsg(a, b, point);
                        data.obj = cMsg;
                        inQueue.Enqueue(data);
                        break;
                    case MsgType.Missile:
                        PlayerID pid = (PlayerID)packetReader.ReadInt32();
                        int id = packetReader.ReadInt32();
                        CreateMissileMsg mMsg = new CreateMissileMsg(pid, id);
                        data.obj = mMsg;
                        inQueue.Enqueue(data);
                        break;
                    case MsgType.Bomb:
                        PlayerID PID = (PlayerID)packetReader.ReadInt32();
                        int ID = packetReader.ReadInt32();
                        CreateBombMsg bMsg = new CreateBombMsg(PID, ID);
                        data.obj = bMsg;
                        inQueue.Enqueue(data);
                        break;
                }
            }
        }

        public void clear()
        {
            inQueue.Clear();
        }
    }


}

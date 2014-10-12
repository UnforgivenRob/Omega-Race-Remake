using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmegaRace;
using CollisionManager;
using Microsoft.Xna.Framework.Net;

namespace OmegaRace
{
    enum QueueType
    {
        Player,
        Ship,
    }

    struct Queue_Data
    {
        public int inSeqNum;
        public int outSeqNum;
        public MsgType type;
        public Object obj;
    }

    class OutputQueue
    {
        private PacketWriter packetWriter;
        private static readonly OutputQueue instance = new OutputQueue();
        private Queue<Queue_Data> outQueue;
        private int SequenceNumber;
        private Queue_Data data;

        private OutputQueue()
        {
            packetWriter = new PacketWriter();
            outQueue = new Queue<Queue_Data>();
            data = new Queue_Data();
            SequenceNumber = 9000;
        }

        public static OutputQueue Instance
        {
            get
            {
                return instance;
            }
        }

        public void add(Message msg)
        {
            data.type = msg.type;
            data.obj = msg;
            data.outSeqNum = SequenceNumber;
            outQueue.Enqueue(data);
            SequenceNumber++;
        }

        public void send(LocalNetworkGamer gamer, Game1 g)
        {
            int count = outQueue.Count;
            while (count > 0)
            {
                Queue_Data data = outQueue.Dequeue();

                //InputQueue.Instance.add(data);

                if (data.type == MsgType.ship)
                {
                    packetWriter.Write(data.inSeqNum);
                    packetWriter.Write(data.outSeqNum);
                    packetWriter.Write((int)data.type);
                    packetWriter.Write(((ShipInputMsg)data.obj).dir);
                    packetWriter.Write(((ShipInputMsg)data.obj).rot);
                    packetWriter.Write((int)((ShipInputMsg)data.obj).pID);
                    packetWriter.Write((int)((ShipInputMsg)data.obj).missile);
                    packetWriter.Write((int)((ShipInputMsg)data.obj).bomb);
                    gamer.SendData(packetWriter, SendDataOptions.ReliableInOrder, g.netSession.RemoteGamers[0]);
                }
                else if (data.type == MsgType.update)
                {
                    packetWriter.Write(data.inSeqNum);
                    packetWriter.Write(data.outSeqNum);
                    packetWriter.Write((int)data.type);
                    packetWriter.Write(((UpdateMsg)data.obj).loc);
                    packetWriter.Write(((UpdateMsg)data.obj).vel);
                    packetWriter.Write(((UpdateMsg)data.obj).vRot);
                    packetWriter.Write(((UpdateMsg)data.obj).rot);
                    packetWriter.Write((int)((UpdateMsg)data.obj).goid);
                    packetWriter.Write((int)((UpdateMsg)data.obj).pID);
                    gamer.SendData(packetWriter, SendDataOptions.InOrder, g.netSession.RemoteGamers[0]);
                }
                else if (data.type == MsgType.Collision)
                {
                    packetWriter.Write(data.inSeqNum);
                    packetWriter.Write(data.outSeqNum);
                    packetWriter.Write((int)data.type);
                    packetWriter.Write(((CollisionMsg)data.obj).point);
                    packetWriter.Write((int)((CollisionMsg)data.obj).a);
                    packetWriter.Write((int)((CollisionMsg)data.obj).b);
                    gamer.SendData(packetWriter, SendDataOptions.ReliableInOrder);
                }
                else if (data.type == MsgType.Missile)
                {
                    packetWriter.Write(data.inSeqNum);
                    packetWriter.Write(data.outSeqNum);
                    packetWriter.Write((int)data.type);
                    packetWriter.Write((int)((CreateMissileMsg)data.obj).pID);
                    packetWriter.Write(((CreateMissileMsg)data.obj).id);
                    gamer.SendData(packetWriter, SendDataOptions.ReliableInOrder, g.netSession.RemoteGamers[0]);
                }
                else if (data.type == MsgType.Bomb)
                {
                    packetWriter.Write(data.inSeqNum);
                    packetWriter.Write(data.outSeqNum);
                    packetWriter.Write((int)data.type);
                    packetWriter.Write((int)((CreateBombMsg)data.obj).pID);
                    packetWriter.Write(((CreateBombMsg)data.obj).id);
                    gamer.SendData(packetWriter, SendDataOptions.ReliableInOrder, g.netSession.RemoteGamers[0]);
                }
                count--;
            }
        }

        public void clear()
        {
            outQueue.Clear();
        }
    }
}

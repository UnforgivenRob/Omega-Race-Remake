//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using CollisionManager;

//namespace OmegaRace
//{
//    class PlayerQueue
//    {
//        public static void add(Player_Data data)
//        {
//            Queue_Data qData;
//            qData.type = QueueType.Player;
//            qData.obj = data;
//            qData.inSeqNum = -1;
//            qData.outSeqNum = -1;
//            OutputQueue.Instance.add(qData);
//        }
//    }

//    class ShipQueue
//    {
//        public static void add(Ship_Data data)
//        {
//            Queue_Data qData;
//            qData.type = QueueType.Ship;
//            qData.obj = data;
//            qData.inSeqNum = -1;
//            qData.outSeqNum = -1;
//            OutputQueue.Instance.add(qData);
//        }
//    }
//}

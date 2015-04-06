using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trinity;
using Trinity.Storage;
using Trinity.Network.Messaging;

namespace GraphTest
{
    class Graph
    {
        public static NodeSet[] Q = new NodeSet[2];
        //public static object LockThis = new object();
        //public static List<NodeSet> AllSameLabel = new List<NodeSet>();
        public static long GraphSize = 10;





        public void Initialize()
        {
            Q[0] = new NodeSet();
            Q[1] = new NodeSet();
            Q[0].headid = 102;
            Q[0].set.Add(101);
            Q[0].set.Add(102);
            Q[0].set.Add(103);
            Q[1].headid = 104;
            Q[1].set.Add(103);
            Q[1].set.Add(104);
            Q[1].set.Add(101);
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trinity;
using Trinity.Storage;
using Trinity.Network.Messaging;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.IO;


namespace FacebookTest
{
    class GraphProxy : GraphProxyBase
    {
        public SameLabel sl = new SameLabel();

        public override void BeginProxyHandler(BeginSignalReader bsr)
        {
            NodeSet[] Q = new NodeSet[2];
            Q[0].set = new List<long>();
            Q[1].set = new List<long>();

            Q[0].headid = 5001;
            Q[0].set.Add(5002);
            Q[0].set.Add(5005);

            Q[1].headid = 5003;
            Q[1].set.Add(5004);
            Q[1].set.Add(5005);

            int slave_count = Global.SlaveCount;
            sl.set = new List<NodeSet>();
            Console.WriteLine(slave_count);
            //Console.ReadLine();
            ResultReader[] rr = new ResultReader[slave_count];
            for (int q = 0; q < 2; q++)
            {
                for (int i = 0; i < slave_count; i++)
                {
                    Global.CloudStorage.TellSlaveIDToGraphSlave(i, new SlaveIDWriter(i));
                    //Console.WriteLine("slave id : {0}", i);
                    Global.CloudStorage.GetSameLabelSetToGraphSlave(i, new SameLabelWriter(sl.set));
                    Console.WriteLine("q{0} to slave{1}",q,i);

                    rr[i] = Global.CloudStorage.QueryToGraphSlave(i, new NodeSetWriter(Q[q].headid, Q[q].headname, Q[q].headlabel, Q[q].set.ToList()));


                    rr[i].samelabel.ForEach((tmp) =>
                    {
                        sl.set.Add(tmp);
                    });
                    Console.WriteLine("Done!!");
                    Console.WriteLine();
                    Console.WriteLine();


                //    rr[i].set.ForEach((tmp1) =>
                //{
                //    Console.WriteLine(tmp1.headid);
                //    tmp1.set.ForEach((tmp2) =>
                //    {
                //        Console.Write("{0} ", tmp2);
                //    }
                //    );
                //    Console.WriteLine();
                //}
                //    );




                    //Console.ReadLine();

                }
            }
            Console.WriteLine("finished");
        }
        public override void UpdateSameLabelSetHandler(SameLabelReader slr)
        {

        }
        public override void QueryHandler(NodeSetReader nsr, ResultWriter result)
        {
            int slave_count = Global.SlaveCount;
            sl.set = new List<NodeSet>();
            Console.WriteLine(slave_count);
            Console.ReadLine();
            ResultReader[] rr = new ResultReader[slave_count];
            for (int i = 0; i < slave_count; i++)
            {
                Global.CloudStorage.TellSlaveIDToGraphSlave(i, new SlaveIDWriter(i));
                Console.WriteLine("slave id : {0}", i);
                Global.CloudStorage.GetSameLabelSetToGraphSlave(i, new SameLabelWriter(sl.set));
                rr[i] = Global.CloudStorage.QueryToGraphSlave(i, new NodeSetWriter(nsr.headid, nsr.headname, nsr.headlabel, nsr.set));
                rr[i].samelabel.ForEach((tmp) =>
                    {
                        sl.set.Add(tmp);
                    });
            }
            for (int i = 0; i < slave_count; i++)
            {
                rr[i].set.ForEach((tmp) =>
                {
                    result.set.Add(tmp);
                });
            }
        }
    }
}

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
    class Program
    {

        public static List<NodeSet> AllSameLabel = new List<NodeSet>();
        public static HashSet<int> flag = new HashSet<int>();
        static void Main(string[] args)
        {
            long graphsize = 0;
             
                if (args.Length >= 1 && args[0].StartsWith("-s"))
                {
                    GraphSlave slave = new GraphSlave();
                    slave.Start(true);
                }
                if (args.Length >= 1 && args[0].StartsWith("-p"))
                {
                    GraphProxy proxy = new GraphProxy();
                    proxy.Start(true);
                }

            if (args.Length >= 1 && args[0].StartsWith("-g"))
            {
                TrinityConfig.CurrentRunningMode = RunningMode.Client;
                for (int i = 1; i <= 5000; i++)
                {
                    Global.CloudStorage.SaveNode(i, id: i, neighbors: new List<long>(), name: Convert.ToString(i), label: Convert.ToString(i % 10), flag: 0);
                }
                Global.CloudStorage.BeginToGraphSlave(0, new BeginSignalWriter(signal: 1));
                HashSet<long> a = new HashSet<long>();
                HashSet<long> b = new HashSet<long>();
                HashSet<long> c = new HashSet<long>();
                HashSet<long> d = new HashSet<long>();
                HashSet<long> e = new HashSet<long>();

                a.Add(5002);
                a.Add(5005);
                Global.CloudStorage.SaveNode(5001, id: 5001, neighbors: a.ToList(), name: "5001", label: "1", flag: 0);

                b.Add(5001);
                b.Add(5003);
                Global.CloudStorage.SaveNode(5002, id: 5002, neighbors: b.ToList(), name: "5002", label: "2", flag: 0);

                c.Add(5002);
                c.Add(5004);
                c.Add(5005);
                Global.CloudStorage.SaveNode(5003, id: 5003, neighbors: c.ToList(), name: "5003", label: "3", flag: 0);

                d.Add(5005);
                d.Add(5003);
                Global.CloudStorage.SaveNode(5004, id: 5004, neighbors: d.ToList(), name: "5004", label: "4", flag: 0);

                e.Add(5001);
                e.Add(5003);
                e.Add(5004);
                Global.CloudStorage.SaveNode(5005, id: 5005, neighbors: e.ToList(), name: "5005", label: "5", flag: 0);


                Global.CloudStorage.SaveNode(10000, id: graphsize, neighbors: new List<long>(), name: Convert.ToString(graphsize), label: Convert.ToString(graphsize % 10), flag: 0);
            }
            if (args.Length >= 1 && args[0].StartsWith("-c"))
            {
                TrinityConfig.CurrentRunningMode = RunningMode.Client;
                //var tmpsize = Global.CloudStorage.LoadNode(10000);

                //graphsize = tmpsize.id;
                //Console.WriteLine("graphsize: {0}", graphsize);
                //NodeSet[] Q = new NodeSet[2];
                //Q[0].set = new List<long>();
                //Q[1].set = new List<long>();

                //Q[0].headid = 5005;
                //Q[0].set.Add(5001);
                //Q[0].set.Add(5003);
                //Q[0].set.Add(5004);

                //Q[1].headid = 5004;
                //Q[1].set.Add(5003);
                //Q[1].set.Add(5001);

                //NodeSet tmp = new NodeSet();
                //tmp = Q[0];
                //ResultReader r = Global.CloudStorage.QueryToGraphProxy(0, new NodeSetWriter(tmp.headid, tmp.headname, tmp.headlabel, tmp.set.ToList()));
                //r.set.ForEach((tmp1) =>
                //{
                //    Console.WriteLine(tmp1.headid);
                //    tmp1.set.ForEach((tmp2) =>
                //    {
                //        Console.Write("{0} ", tmp2);
                //    }
                //    );
                //    Console.WriteLine();
                //    //String s = Console.ReadLine();
                //}
                //    );
                Global.CloudStorage.BeginProxyToGraphProxy(0, new BeginSignalWriter(1));
                Console.ReadLine();
            }
        }
       
    }
}

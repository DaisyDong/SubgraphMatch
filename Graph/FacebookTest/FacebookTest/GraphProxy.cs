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
        public int slave_count = Global.SlaveCount;
        public ResultReader[][] rr = new ResultReader[2][];
        public HashSet<long> leafSet = new HashSet<long>(); //保存每个匹配结果中的叶子节点 
        public List<NodeSet> result = new List<NodeSet>();
        public int STwigNum = 2;    //分好的STwig的个数

        public override void BeginProxyHandler(BeginSignalReader bsr)
        {
            NodeSet[] Q = new NodeSet[2];
            Q[0].set = new List<long>();
            Q[1].set = new List<long>();

            Q[0].headid = 5001;
            Q[0].set.Add(5002);
            Q[0].set.Add(5005);

            Q[1].headid = 5002;
            Q[1].set.Add(5004);
            Q[1].set.Add(5005);

            int slave_count = Global.SlaveCount;
            sl.set = new List<NodeSet>();
            Console.WriteLine(slave_count);
            //Console.ReadLine();
            // ResultReader[][] rr = new ResultReader[2][slave_count];
            for (int q = 0; q < 2; q++)
            {
                rr[q] = new ResultReader[slave_count];
                for (int i = 0; i < slave_count; i++)
                {
                    Global.CloudStorage.TellSlaveIDToGraphSlave(i, new SlaveIDWriter(i));
                    //Console.WriteLine("slave id : {0}", i);
                    Global.CloudStorage.GetSameLabelSetToGraphSlave(i, new SameLabelWriter(sl.set));
                    Console.WriteLine("q{0} to slave{1}", q, i);

                    rr[q][i] = Global.CloudStorage.QueryToGraphSlave(i, new NodeSetWriter(Q[q].headid, Q[q].headname, Q[q].headlabel, Q[q].set.ToList()));

                    rr[q][i].samelabel.ForEach((tmp) =>
                    {
                        Console.WriteLine("{0}", tmp.headid);
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
            ReportHandler();
        }
        public override void UpdateSameLabelSetHandler(SameLabelReader slr)
        {

        }
        /* public void QueryHandler(NodeSetReader nsr, ResultWriter result)
         {
             int slave_count = Global.SlaveCount;
             sl.set = new List<NodeSet>();
             Console.WriteLine(slave_count);
             Console.ReadLine();
             //ResultReader[] rr = new ResultReader[slave_count];
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
          }*/
        //处理每个slave的结果,rr[i][j] 表示第i个STwig在第j个机器中的结果
        public void ReportHandler()
        {
            for (int j = 0; j < Global.SlaveCount; j++)
            {  
                int n = 1;
                rr[0][j].samelabel.ForEach((tmp) =>
                    {
                        tmp.set.ForEach((num) =>
                            { 
                                leafSet.Add(num);
                            });
                        if (n == STwigNum)
                        {
                            Console.WriteLine("match subGraph is:");
                            Console.WriteLine("{0}:{1}", tmp.headid, tmp.set);
                        }
                        else findResult(n,j); 
                        //  Console.WriteLine("{0}", tmp.headid);
                        //result.Add(tmp);
                         
                    });
            }
        }
    public void findResult(int n,int m){
        HashSet<long> leafSetS = leafSet;
        rr[n][m].samelabel.ForEach((tem) =>
            {
                if (leafSetS.Contains(tem.headid))
                { 
                    tem.set.ForEach((num) =>
                        { 
                            if(num != tem.headid)
                                leafSet.Add(num);
                        });
                    result.Add(tem);
                    if (++n == STwigNum)
                    {
                        Console.WriteLine("match subgraph is:");
                        result.ForEach((tmp) =>
                            {
                                Console.WriteLine("{0}:{1}", tmp.headid, tmp.set);
                            });
                    }
                    else
                        findResult(n, m);
                }
            });
        return;
    }
    }       
}

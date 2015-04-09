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
        //public HashSet<long> leafSet = new HashSet<long>(); //保存每个匹配结果中的叶子节点  
        public int STwigNum = 2;    //分好的STwig的个数
        public int resultNum = 0;   //用于记录找到的匹配子图的个数，因为一般情况下我们不需要所有的也看不完，所以这里我们设定超过1024就不找了

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
                        //Console.WriteLine(tmp.headid);
                        sl.set.Add(tmp);
                    });
                    Console.WriteLine("Done!!");
                    Console.WriteLine();
                    Console.WriteLine();


                    //  rr[q][i].set.ForEach((tmp1) =>
                    //{
                    //   Console.WriteLine(tmp1.headid);
                    //   tmp1.set.ForEach((tmp2) =>
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
                int n = 0;
                //int count = 0;
                rr[n][j].set.ForEach((tmp) =>
                    {
                        if (j == 1) return; 
                        //Console.WriteLine(tmp.headid);
                        HashSet<long> leafSet = new HashSet<long>(); //保存每个匹配结果中的叶子节点 
                        List<NodeSet> result = new List<NodeSet>();
                        tmp.set.ForEach((son) =>
                            { 
                                leafSet.Add(son);       //形成每个stwig连接后留下来的叶子节点用来寻找下一个stwig
                             
                            });
                        result.Add(tmp);
                        //Console.WriteLine();
                        if (n + 1 == STwigNum)  //已经连接到了所有stwig匹配的点，即表示构成了匹配的子图
                        {
                            Console.Write("{0}: ", tmp.headid);
                            tmp.set.ForEach((s) =>
                            {
                                Console.Write("{0} ", s);
                            }
                            );
                            Console.WriteLine();
                            if (++resultNum > 1024) return;
                        }
                        else
                        {
                            findResult(n + 1, j, leafSet, result);
                            leafSet = null; 
                        }
                        //  Console.WriteLine("{0}", tmp.headid);
                        //result.Add(tmp);
                         
                    });
            }
        }
        public void findResult(int n, int m, HashSet<long> leafSetS,List<NodeSet> result)
        {
        //Console.WriteLine("findResult:{0} ge STwig,{1} ge Slave",n,m);
        HashSet<long> temp = leafSetS;
        NodeSet note = new NodeSet();
        if (n >= STwigNum || resultNum > 1024)
        {
            //Console.WriteLine("{0},{1}", n, STwigNum);
            return;
        } 
        rr[n][m].set.ForEach((tem) =>
            {
                Boolean flag = false;
                //Console.WriteLine("headid:{0}", tem.headid);
                if (leafSetS.Contains(tem.headid))
                {
                    //Console.WriteLine("you pi pei de jie dian {0}", tem.headid);
                    tem.set.ForEach((num) =>
                        {
                            if (num != tem.headid)
                            {  //每次加新的子结构都继续添加多出来的儿子节点，同时
                                leafSetS.Add(num);
                                HashSet<long> find = new HashSet<long>();
                                find.Add(tem.headid);
                                leafSetS.ExceptWith(find);
                            }//将新结构的父节点从儿子节点表中去除防止重复选择
                        });
                    result.Add(tem);
                    note = tem;
                    flag = true;    //i所对应的子结构已经在集合中加入过，下次再找同构的子结构时就要将之前的结果去除
                    if (n + 1 == STwigNum)
                    {
                        Console.WriteLine("match subgraph is:");
                        result.ForEach((t) =>
                            {
                                Console.Write("{0}: ", t.headid);
                                t.set.ForEach((s) =>
                                    {
                                        Console.Write("{0} ", s);
                                    }
                                );
                                Console.WriteLine();
                            });
                        if (++resultNum > 1024) return;
                        leafSetS = temp;    //因为别的结构也可能有链接，所以恢复叶子节点集合
                        result.Remove(note);  //输出结果就将结果删除S
                    }
                    else //再去找下一个STWig
                    {
                        Console.WriteLine("findResult:{0}ge STwig,{1} ge Slave", n, m);
                        findResult(n + 1, m, leafSetS,result);
                    }
                }
            });                         
        return;
    }
    }       
}

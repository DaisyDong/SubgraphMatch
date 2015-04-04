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
    class GraphSlave : GraphSlaveBase
    {
        public static object LockThis = new object();
        public long graphsize;
        public int myid;
        public bool updatesl;
        public SameLabel sl = new SameLabel();
        static string getnodelabel;

        public override void GetSameLabelSetHandler(SameLabelReader slr,BeginSignalWriter bsw)
        {
            sl.set = new List<NodeSet>();
            slr.set.ForEach((tmp) =>
                {
                    sl.set.Add(tmp);
                });
            bsw.signal = 1;
        }
        public override void ShowNodeSetHandler(NodeIndexReader result)
        {
        }
        public override void TellSlaveIDHandler(SlaveIDReader id, BeginSignalWriter bsw)
        {
            myid = id.id;
            Console.WriteLine("my id : {0}",myid);
            bsw.signal = 1;
        }
        public override void QueryHandler(NodeSetReader nsr, ResultWriter result)
        {
            //Console.WriteLine("begin here.");
            NodeSet ns = new NodeSet();
            updatesl = false;
            ns = FindSameLabel(nsr.headid);
            if (updatesl == true)
            {
                result.samelabel.Add(ns);//将结果加入到总的Samelabel中
            }
            for (int i = 0; i < ns.set.Count;i++)//对头的匹配节点做处理，只有id为当前slaveid的节点留下
            {
                if (Global.CloudStorage.GetSlaveIdByCellId(ns.set[i]) != myid)
                {
                    //Console.WriteLine("{0}'s id : {1}", ns.set[i], Global.CloudStorage.GetSlaveIdByCellId(ns.set[i]));
                    ns.set.Remove(ns.set[i]);
                    i--;
                    //Console.WriteLine("-----{0}'s id : {1}", ns.set[i],Global.CloudStorage.GetSlaveIdByCellId(ns.set[i]));
                }
            }
            Boolean notFound = false;
            List<NodeSet> tmpResult = new List<NodeSet>();
            List<NodeSet> tmpResult2 = new List<NodeSet>();
            //Console.WriteLine("shit1");
            foreach (long tmp in ns.set)//每次去取一个具有相同label的根结点
            {
                //Console.WriteLine("God bless me! {0}",tmp);
                //Console.WriteLine(tmp.id);
                tmpResult.Clear();
                tmpResult2.Clear();
                NodeSet tmpns = new NodeSet();//一个暂时变量，暂时保存某一个查到的匹配到的子图
                tmpns.set = new List<long>();
                tmpns.headid = tmp;
                tmpns.headlabel = nsr.headlabel;
                tmpns.set.Add(tmp);
                tmpResult2.Add(tmpns);
                //Console.ReadLine();
                //tmpns.addNode(tmp);
                notFound = false;
                nsr.set.ForEach((qn) =>//取Q中每一个儿子节点与上一行取出的节点的儿子节点作比对（nsr为proxy传过来的需要查询的图）
                {
                    if (notFound == true)
                    {
                        return;
                    }
                    notFound = true;
                    tmpResult.Clear();
                    //Console.WriteLine("query:{0}", qn);
                    updatesl = false;
                    NodeSet fsl = new NodeSet();
                    fsl = FindSameLabel(qn);
                    if (updatesl == true)
                    {
                        result.samelabel.Add(fsl);//将结果加入到总的Samelabel中
                    }
                    using (var testhead = Global.LocalStorage.UseNode(tmp))//tmp为一个具有相同label的根结点----------注意这里需要给对应的slave处理，记得改
                    {
                        testhead.neighbors.ForEach((n) =>//搜索一下取出的根节点的儿子中是否存在与qn其中一个儿子节点相匹配的节点
                        {
                            //Console.WriteLine("test n{0}", n);
                            foreach (int i in fsl.set)
                            {
                                if (i == n)
                                {
                                    notFound = false;
                                    foreach (NodeSet tmpns2 in tmpResult2)
                                    {
                                        NodeSet tmpns3 = NodeSetCopy(tmpns2);
                                        tmpns3.set.Add(n);
                                        tmpResult.Add(tmpns3);
                                    }
                                }
                            }

                        }
                        );
                    }

                    if (notFound == true)
                    {
                        //if (tmp == 3932)
                        //{
                        //    Console.WriteLine("not found");
                        //}
                        return;
                    }
                    tmpResult2.Clear();
                    foreach (NodeSet tmpns4 in tmpResult)
                    {
                        // tmpns4.show();
                        tmpResult2.Add(tmpns4);
                    }
                }
                );
                //Console.WriteLine("************************************************");
                if (notFound == false)
                {
                    foreach (NodeSet tmpns5 in tmpResult)
                    {
                        //Console.WriteLine("find one");
                        result.set.Add(tmpns5);
                        //Q[i].showResult();
                    }
                }
            }
            //result.set.ForEach((tmp1) =>
            //{
            //    Console.WriteLine(tmp1.headid);
            //    tmp1.set.ForEach((tmp2) =>
            //    {
            //        Console.Write("{0} ", tmp2);
            //    }
            //    );
            //    Console.WriteLine();
            //}
            //       );
            //result.samelabel.ForEach((tmp1) =>
            //{
            //    Console.WriteLine(tmp1.headid);
            //    tmp1.set.ForEach((tmp2) =>
            //    {
            //        Console.Write("{0} ", tmp2);
            //    }
            //    );
            //    Console.WriteLine();
            //}
            //       );
            // Console.ReadLine();
            //Global.CloudStorage.UpdateSameLabelSetToGraphProxy(0, new SameLabelWriter(result.samelabel));
        }



        public NodeSet NodeSetCopy(NodeSet ns)
        {
            NodeSet tmp = new NodeSet();
            tmp.set = new List<long>();
            tmp.headid = ns.headid;
            ns.set.ForEach((n) =>
            {
                tmp.set.Add(n);
            });
            return tmp;
        }
        public NodeSet FindSameLabel(long num)
        {

            NodeLabelReader l = Global.CloudStorage.GetLabelToGraphSlave(Global.CloudStorage.GetSlaveIdByCellId(num), new NodeIndexWriter(num));
            // Console.WriteLine(l.label);
            //Console.WriteLine(num);
            //Console.WriteLine("graphsize:{0}", this.graphsize);
            NodeSet ns = new NodeSet();
            ns.set = new List<long>();
            ns.headlabel = l.label;
            bool isfound = false;
            sl.set.ForEach((tmp) =>
                {
                    if (isfound == true)
                    {
                        return;
                    }
                    if (tmp.headlabel == ns.headlabel)
                    {
                        tmp.set.ForEach((n) =>
                            {
                                ns.set.Add(n);
                            });
                        isfound = true;
                    }
                });
            if (isfound == true)
            {
                //Console.WriteLine("find samelabel");
                return ns;
            }
            updatesl = true;
            for (long i = 1; i <= this.graphsize; i++)
            {
                NodeLabelReader l2 = Global.CloudStorage.GetLabelToGraphSlave(Global.CloudStorage.GetSlaveIdByCellId(i), new NodeIndexWriter(i));
                if (l2.label == l.label)
                {
                    ns.set.Add(i);
                }
            }
            return ns;
        }


        public override void GetLabelHandler(NodeIndexReader nodeindex, NodeLabelWriter l)
        {
            using (var tmp = Global.LocalStorage.UseNode(nodeindex.num))
            {
                l.label = tmp.label;
                l.name = tmp.name;
            }
        }

        public override void BeginHandler(BeginSignalReader beginsignal)
        {
            string strLine;
            graphsize = 0;
            try
            {
                FileStream aFile = new FileStream("E:\\trinity\\Graph\\facebook_combined.txt", FileMode.Open);
                StreamReader sr = new StreamReader(aFile);
                strLine = sr.ReadLine();
                HashSet<long> tmp = new HashSet<long>();
                long head, neighbor, lasthead;
                lasthead = -1;
                HashSet<long> tmpset;
                tmpset = new HashSet<long>();
                while (strLine != null)
                {
                    String[] number = strLine.Split(' ');
                    head = Convert.ToInt32(number[0]);
                    //Console.WriteLine(head);
                    neighbor = Convert.ToInt32(number[1]);
                    using (var tmpn = Global.LocalStorage.UseNode(head))
                    {
                        // Console.WriteLine(tmpn.name);
                        if (tmpn.flag == 0)
                        {
                            tmpn.flag = 1;
                        }
                        tmpn.neighbors.Add(neighbor);
                    }
                    using (var tmpn = Global.LocalStorage.UseNode(neighbor))
                    {
                        if (tmpn.flag == 0)
                        {
                            tmpn.flag = 1;
                        }
                        tmpn.neighbors.Add(head);
                    }
                    //Console.WriteLine(strLine);
                    strLine = sr.ReadLine();
                }
                graphsize = 5000;
                sr.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("An IOException has been thrown!");
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }
        }

    }
}

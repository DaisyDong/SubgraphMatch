/*Trinity Proxy:
 *     1、dispatching graph search queries to slaves；
 *     2、gathering the partial results from slaves,finally giveing
 *        back clients the final results; 
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.ExceptionServices;
using System.Security;

using Trinity; 
using Trinity.Storage;

namespace SubgraphMatch
{
    class Proxy : MyProxyBase
    { 
        static int report_num = 0;
        static int reports_total = 0;
        static object lock_o = new object();
        static int slave_count = Global.SlaveCount; //slave的数目
        static long STnum; //= STwig.num;    待划分好stwig后就能得到
        static HashSet<node>[] matches = new HashSet<node>[STnum]; //保存得到的STwig集合
        static long[] STwigOrder = new long[STnum];//保存有划分好的STwig的id，以STwig的id为下标，里面存的值表示顺序
        public List<node> match = new List<node>();//最后匹配好的答案
        //register a handler to deal with query requests from the client
        [HandleProcessCorruptedStateExceptionsAttribute, SecurityCriticalAttribute]
        public override void QueryHandler(QueryRequestReader request, ResultWriter responseBuff)
        {
             try
            {
               Global.CloudStorage.SearchToMySlave(Global.CloudStorage.GetSlaveIdByCellId(1),
                   new QueryRequestWriter(/*具体参数自己定义request.hop, request.name, request.neighbours*/));//分给slave的
                 reports_total = slave_count;  //计算总共收到的report的数目
                //Console.WriteLine("Waiting for {0} reports", reports_total); 
                while (report_num != reports_total) //等待所有结果都出来
                {
                    Thread.Sleep(1000);
                }
  
           }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        //处理从各个slaves中得到的结果
        //register a handler to deal with partial result reports from slaves
        public override void ReportHandler(ResultReader request)
        {
            lock (lock_o)   //开启多线程
            {
                request.STwig.ForEach(stwig =>
                { 
                    matches[STwigOrder[stwig.id]].Add(stwig); //以id所对应的order为下标，将匹配的每一个子结构按类分好;; 
                }
                );
                ++report_num;   //可以读取下一个slave的report了 
                //Console.WriteLine("This is the {0}th report", ++report_num); 
                int num = 0; //输出数目统计，控制在1024之内，太多了也没用
                if (report_num == reports_total)//收到所有slave中的匹配的子结构，按照order进行组合
                {
                    int order = 0; //代表现在加入的order的顺序
                    combSTwig(matches,order,num);
                }
            }
        } 
        private void combSTwig(HashSet<node>[] matches,int order,int num)
        {
            node preSTwig;
            if (order == 0) preSTwig = null;
            else preSTwig = match[order-1];//先得到上一个子结构看现在的和上一个之间是否存在连接关系
            for (int i = 0; i < matches[order].Count; i++)
            {
                node temp = matches[order].ElementAt(i);
                if (defineConnect(preSTwig,temp))  //判断这两个子结构是否能够连接
                {
                    match.Add(temp);
                    if (order == STnum - 1) //找到一个完整的子图就输出在控制台
                    {
                        Console.WriteLine(match);
                        num++;
                    }
                    else
                         combSTwig(matches, ++order, num);   //查找下一个子结构
                }
                if (num > 1024) return;
            }
        }
        private bool defineConnect(node pre,node next){
            STnode preA = pre.detail;
            STnode nextB = next.detail;
            for (int i = 0; i < preA.child.Count; i++)
            {
                if (nextB.father == preA.child.ElementAt(i)) return true; //下一个子结构的根节点和上一个的子节点相同则表明可以连接
            }
            return false;
        }
    }
}

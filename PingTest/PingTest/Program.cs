using System ;
using System . Collections . Generic ;
using System . Linq ;
using System . Text ; 
using Trinity . Storage ;
using Trinity ;
using System . Threading ;
namespace TestTrinity
{
    class MySlave : MySlaveBase
    {
        public override void SynPingHandler(MyMessageReader request)
        {
            Console.WriteLine(" Received SynPing , sn ={0} ", request.sn);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var my_slave = new MySlave();
            my_slave.Start(false);
            var synReq = new MyMessageWriter(1);
            Global.CloudStorage.SynPingToMySlave(0, synReq);
            Console.WriteLine();
            Console.ReadKey();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trinity;
using Trinity.Storage;

namespace PeopleSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (args[0])
            {
                case "-s"://start a slave   
                    var my_slave = new MySlave();
                    my_slave.Start(true);
                    break;
                case "-m"://start a proxy   
                    var my_proxy = new MyProxy();
                    my_proxy.Start(true);
                    break;
                case "-c": // start a client for issuing queries
                    TrinityConfig.CurrentRunningMode = RunningMode.Client;
                    CreateGraph();
                        
                    Console.WriteLine("==========================");
                    Console.WriteLine("Done with loading generated graph to Trinity!");
                    Console.WriteLine();

                    LaunchQueries();
                    break;
            }
        }

        static void CreateGraph()
        {
            //140 is the average friends number of any person
            GraphCreator graphCreator = new GraphCreator(140);
            graphCreator.CreateGraph();//this'll dump person cells into trinity storage
        }

        static void LaunchQueries()
        {
            string yourname = Global.CloudStorage.LoadPerson(1).name;
            Console.WriteLine("CENTRE's name is: " +yourname );
            while (true)
            {
                Console.WriteLine("Write a name you want to find in CENTRE's three hop friends");
                string name = Console.ReadLine();
                //the cellId of CENTRE is set to 1 on purpose
                ResultReader result = Global.CloudStorage.QueryToMyProxy(0, new NameRequestWriter(0, name, new List<long> { 1 }));
                Console.WriteLine("There are {0} 3 hop friends whose name is {1}", result.matchPersons.Count, name);
            }
        }

    }
}

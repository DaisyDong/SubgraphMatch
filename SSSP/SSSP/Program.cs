using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trinity;
using Trinity.Storage;
using Trinity.Network.Messaging;

namespace SSSP
{
    class SSSPSlave : SSSPSlaveBase
    {
        public override void DistanceUpdatingHandler(DistanceUpdatingMessageReader request)
        {
            List<DistanceUpdatingMessage> DistanceUpdatingMessageList = new List<DistanceUpdatingMessage>();
            request.recipients.ForEach((cellId) =>
                {
                    using (var cell = Global.LocalStorage.UseSSSPCell(cellId))
                    {
                        if (cell.distance > request.distance + 1)
                        {
                            cell.distance = request.distance + 1;
                            cell.parent = request.senderId;
                            Console.Write(cell.distance + " ");
                            MessageSorter sorter = new MessageSorter(cell.neighbors);

                            for (int i = 0; i < Global.SlaveCount; i++)
                            {
                                DistanceUpdatingMessageWriter msg = new DistanceUpdatingMessageWriter(cell.CellID.Value,
                                    cell.distance, sorter.GetCellRecipientList(i));
                                Global.CloudStorage.DistanceUpdatingToSSSPSlave(i, msg);
                            }

                        }
                    }
                });
        }

        public override void StartSSSPHandler(StartSSSPMessageReader request)
        {
            if (Global.CloudStorage.IsLocalCell(request.root))
            {
                using (var rootCell = Global.LocalStorage.UseSSSPCell(request.root))
                {
                    rootCell.distance = 0;
                    rootCell.parent = -1;
                    MessageSorter sorter = new MessageSorter(rootCell.neighbors);

                    for (int i = 0; i < Global.SlaveCount; i++)
                    {
                        DistanceUpdatingMessageWriter msg = new DistanceUpdatingMessageWriter(rootCell.CellID.Value, 0, sorter.GetCellRecipientList(i));
                        Global.CloudStorage.DistanceUpdatingToSSSPSlave(i, msg);
                    }
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length >= 1 && args[0].StartsWith("-s"))
            {
                SSSPSlave slave = new SSSPSlave();
                slave.Start(true);
            }

            //SSSP.exe -c startcell
            if (args.Length >= 2 && args[0].StartsWith("-c"))
            {
                TrinityConfig.CurrentRunningMode = RunningMode.Client;
                for (int i = 0; i < Global.SlaveCount; i++)
                {
                    Global.CloudStorage.StartSSSPToSSSPSlave(i, new StartSSSPMessageWriter(long.Parse(args[1].Trim())));
                }
            }

            //SSSP.exe -q cellID
            if (args.Length >= 2 && args[0].StartsWith("-q"))
            {
                TrinityConfig.CurrentRunningMode = RunningMode.Client;
                var cell = Global.CloudStorage.LoadSSSPCell(int.Parse(args[1]));
                Console.WriteLine("Current Node's id is {0}, The distance to the centre node is {1}.",
                    cell.CellID, cell.distance);
                while (cell.distance > 0)
                {
                    cell = Global.CloudStorage.LoadSSSPCell(cell.parent);
                    Console.WriteLine("Current Node's id is {0}, The distance to the centre node is {1}.",
                        cell.CellID, cell.distance);
                }

            }

            //SSSP.exe -g node count
            if (args.Length >= 2 && args[0].StartsWith("-g"))
            {
                TrinityConfig.CurrentRunningMode = RunningMode.Client;

                Random rand = new Random();
                int nodeCount = int.Parse(args[1].Trim());
                for (int i = 0; i < nodeCount; i++)
                {
                    HashSet<long> neighbors = new HashSet<long>();
                    for (int j = 0; j < 10; j++)
                    {
                        long neighor = rand.Next(0, nodeCount);
                        if (neighor != i)
                        {
                            neighbors.Add(neighor);
                        }
                    }
                    Global.CloudStorage.SaveSSSPCell(i, distance: int.MaxValue, parent: -1, neighbors: neighbors.ToList());
                }
            }
        }
    }
}

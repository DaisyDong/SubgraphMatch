using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity;

namespace CellTransformation
{
    class Program
    {
        static void Main(string[] args)
        {
            TestCellTransformation();
        }

        static void TestCellTransformation()
        {
            TrinityConfig.CurrentRunningMode = RunningMode.Embedded;

            Console.WriteLine("Insert some cells for testing");

            for (int i = 0; i < 10000000; i++)
            {
                if (i % 256 == 1)
                    Global.LocalStorage.SaveMyCellA(i, i);
            }
            Console.WriteLine("Cell Count: {0}", Global.LocalStorage.CellCount);

            CellConverter<MyCellA, MyCellB> cc = new CellConverter<MyCellA, MyCellB>((oldCell) =>
            {
                MyCellB B = new MyCellB(oldCell.A, "Expanded Field");
                return B;
            });

            Console.WriteLine("Start cell transformation ...");
            Global.LocalStorage.Transform(cc);
            Console.WriteLine("Transformation is done.");


            Console.WriteLine("Check transformed cells ");

            int count = 0;
            for (int i = 0; i < 10000000; i++)
            {
                if (i % 256 == 1)
                {
                    Console.WriteLine("CellId: {0} \t Name: {1}", i, Global.LocalStorage.LoadMyCellB(i).Name);
                    if (++count >= 10)
                        break;
                }
            }

            Console.WriteLine("Cell Count: {0}", Global.LocalStorage.CellCount);
        }
    }
}

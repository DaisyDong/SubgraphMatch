using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trinity;
using Trinity.Storage;
using System.Collections;
using System.IO;

namespace PeopleSearch
{
    class GraphCreator
    {
        ArrayList firstnameList = new ArrayList();
        ArrayList surnameList = new ArrayList();

        Person[] persons;

        int neighbourCount = 0;

        int slaveCount = 1;

        int hop = 3;

        Random random = new Random(Environment.TickCount);

        public GraphCreator(int node_degree)
        {
            neighbourCount = node_degree;
            LoadNames();
        }

        public void GenerateGraph(int avg_degree)
        {
            GraphCreator psg = new GraphCreator(avg_degree);
            Console.WriteLine("Degree: {0}", avg_degree);
            psg.CreateGraph();
            Console.WriteLine("Created {0}", avg_degree);
        }

        public void CreateGraph()
        {
            int cellCount = 0;
            for (int i = 0; i <= hop; i++)
            {
                cellCount += (int)Math.Pow(neighbourCount, i);
            }
            Console.WriteLine("Total cell count: {0}", cellCount);

            persons = new Person[cellCount];

            for (int i = 0; i < cellCount; i++)
            {
                persons[i] = new Person(RandomName(), new List<long>());
            }

            persons[0].CellID = 1;
            Console.WriteLine(persons[0].name);

            for (int i = 0; i < hop; i++)
            {
                int start = IndexOfNthHop(i);
                int a = IndexOfNthHop(i + 1);
                for (int startindex = start; startindex < a; startindex++)
                {
                    for (int j = a + (startindex - start) * neighbourCount; j < a + (startindex - start) * neighbourCount + neighbourCount; j++)
                    {
                        persons[startindex].friends.Add(persons[j].CellID);
                    }
                }
            }

            long[] person_id_list = new long[cellCount];
            byte[][] person_bytes_list = new byte[cellCount][];

            for (int i = 0; i < cellCount; i++)
            {
                person_id_list[i] = persons[i].CellID;
                person_bytes_list[i] = ((Person_Accessor)persons[i]).ToByteArray();
            }
            Console.WriteLine("Start batch saving ...");
            Console.WriteLine("slave cout " + Global.SlaveCount);
            Console.WriteLine("the slave id is " + Global.CloudStorage.GetSlaveIdByCellId(1));
            Global.CloudStorage.BatchSaveCell(person_id_list, person_bytes_list);
            Console.WriteLine("Batch Saving done!");
            Console.WriteLine("Start saving stoarge ...");
            Global.CloudStorage.SaveStorage();
            Console.WriteLine("ALL DONE!");
        }

        int IndexOfNthHop(int n)
        {
            int index = 0;
            for (int i = 0; i < n; i++)
            {
                index += (int)Math.Pow(neighbourCount, i);
            }

            Console.WriteLine("index of {0}th hop is {1}", n, index);
            return index;
        }

        string RandomName()
        {
            int a, b;
            a = (int)(random.NextDouble() * firstnameList.Count);
            b = (int)(random.NextDouble() * surnameList.Count);

            string name = firstnameList[a] + " " + (string)surnameList[b];

            return name;
        }

        void LoadNames()
        {
            StreamReader sm_firstname = new StreamReader(@"firstname5494.txt");
            StreamReader sm_surname = new StreamReader(@"surname16000.txt");

            string line;
            while ((line = sm_firstname.ReadLine()) != null)
            {
                string[] frags = line.Split(new char[] { '\t', ' ' });
                firstnameList.Add(frags[0]);
            }

            while ((line = sm_surname.ReadLine()) != null)
            {
                string[] frags = line.Split(new char[] { '\t', ' ' });
                surnameList.Add(frags[0]);
            }

            Console.WriteLine("{0} first names have been loaded.", firstnameList.Count);
            Console.WriteLine("{0} surnames have been loaded.", surnameList.Count);
        }
    }
}
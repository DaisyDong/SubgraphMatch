using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Trinity;
using Trinity.Storage;

namespace PeopleSearch
{
    class MySlave : MySlaveBase
    {
        public override void SearchHandler(NameRequestReader request)
        {
            string queryname = request.name;
            int hop = request.hop;
            int hop_next = hop + 1;
            int slave_count = Global.SlaveCount;
            HashSet<long>[] buckets = new HashSet<long>[slave_count];
            for (int i = 0; i < slave_count; ++i)
            {
                buckets[i] = new HashSet<long>();
            }

            HashSet<long> matches = new HashSet<long>();
            request.neighbours.ForEach(candidate =>
                {
                    using (var person = Global.LocalStorage.UsePerson(candidate))
                    {
                        //find partial results
                        string friendname = person.name;
                        if (friendname.ToLower().Contains(queryname.ToLower()))
                        {
                            matches.Add(candidate);
                        }
                        //group each candidate's friends by their hosting slave
                        if (hop < 3)
                        {
                            person.friends.ForEach(neighbour =>
                                {
                                    buckets[Global.CloudStorage.GetSlaveIdByCellId(neighbour)].Add(neighbour);
                                }
                            );
                        }
                    }
                }
            );

            //send partial result first
            Global.CloudStorage.ReportToMyProxy(0, new ResultWriter(matches.ToList()));

            //propagate
            if (hop < 3)
            {
                for (int i = 0; i < slave_count; ++i)
                {
                    Global.CloudStorage.SearchToMySlave(i, new NameRequestWriter(hop_next, queryname, buckets[i].ToList()));
                }
            }

        }
    }
}

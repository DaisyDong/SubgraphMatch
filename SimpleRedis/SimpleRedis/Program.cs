using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trinity.Core;
using Trinity;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SimpleRedis
{
    class MySlave : MySlaveBase
    {
        public override void GetValueHandler(StringRequestReader request, ValueResponseWriter response)
        {
            long md5 = MD5Utility.GetMD5(request.key);
            if (Global.LocalStorage.Contains(md5))
            {
                //the bucket exists, very likely the key is in the bucket 
                using (var bucket = Global.LocalStorage.UseBucket(md5))
                {
                    response.isFound = false;
                    bucket.elements.ForEach(element_accessor =>
                        {
                            //make sure one of the element matches the key
                            if (element_accessor.key == request.key)
                            {
                                response.isFound = true;
                                response.value = element_accessor.value;
                                return;
                            }
                        });
                    return;
                }
            }
            else
            {
                //no entry for this key exist
                response.isFound = false;
                return;
            }
        }

        public override void SetValueHandler(ElementRequestReader request)
        {
            long md5 = MD5Utility.GetMD5(request.element.key);
            if (Global.LocalStorage.Contains(md5))
            {
                //the bucket already exists
                using (var bucket = Global.LocalStorage.UseBucket(md5))
                {
                    //different keys may end up in the same bucket(Hash Collision)
                    bucket.elements.ForEach(element_accessor =>
                        {
                            if (element_accessor.key == request.element.key)
                            {
                                //if already in the bucket, update the value
                                element_accessor.value = request.element.value;
                                return;
                            }
                        });
                    //upon here,no entry for this key exist
                    bucket.elements.Add(request.element);
                }
            }
            else
            {
                //no bucket is ever exist, create for my own
                Bucket newbucket = new Bucket();
                Global.LocalStorage.SaveBucket(md5, newbucket);
                using (var bucket = Global.LocalStorage.UseBucket(md5))
                {
                    bucket.elements.Add(request.element);
                }
            }
        }
    }
    class Program
    {
        static int slave_count = 1;

        static void Main(string[] args)
        {
            switch (args[0])
            {
                case "-s"://slave mode
                    var my_slave = new MySlave();
                    my_slave.Start(true);
                    break;
                case "-c"://client mode
                    DealWithCommand();
                    break;
            }

        }

        static void DealWithCommand()
        {
            TrinityConfig.CurrentRunningMode = RunningMode.Client;
            slave_count = Global.SlaveCount;
            Console.WriteLine("You know you're playing with {0} slave machines", slave_count);
            Console.WriteLine("Begin Playing with the toy Redis:");
            while (true)
            {
                Console.Write(" >");
                string command = Console.ReadLine();
                Regex setregex = new Regex(@"set\s+(\w*)\s+(.*)");
                Regex getregex = new Regex(@"get\s+(\w*)");
                Match setmatch = setregex.Match(command);
                Match getmatch = getregex.Match(command);
                #region it is a set command
                if (setmatch.Length > 0)
                {
                    string key_raw = setmatch.Groups[1].ToString().Trim();

                    string value_raw = setmatch.Groups[2].ToString().Trim();
                    if (value_raw.StartsWith("[") && value_raw.EndsWith("]"))
                    {
                        Console.WriteLine("setting the value under \"{0}\" to a List", key_raw);
                        value_raw = value_raw.TrimStart("[".ToCharArray());
                        value_raw = value_raw.TrimEnd("]".ToCharArray());
                        string[] numbers = value_raw.Split(",".ToCharArray());
                        List<int> list_value = new List<int>();
                        foreach (string s in numbers)
                            list_value.Add(int.Parse(s.Trim()));

                        //notice! since it's a list value, set the other two arguments to be null, so they won't take extra space
                        Element e = new Element(key_raw, new Value(null, null, list_value));
                        long md5 = MD5Utility.GetMD5(key_raw);
                        Global.CloudStorage.SetValueToMySlave(MD5Utility.GetSlaveID(md5, slave_count), 
                            new ElementRequestWriter(e));

                    }
                    else if (value_raw.StartsWith("\"") && value_raw.EndsWith("\""))
                    {
                        Console.WriteLine("setting the value under \"{0}\" to a string", key_raw);
                        value_raw = value_raw.TrimStart("\"".ToCharArray());
                        value_raw = value_raw.TrimEnd("\"".ToCharArray());
                        string string_value = value_raw;

                        //Notice: since it's a string value, set the other two arguments to be null, so they won't take extra space
                        Element e = new Element(key_raw, new Value(null, string_value, null));
                        long md5 = MD5Utility.GetMD5(key_raw);
                        Global.CloudStorage.SetValueToMySlave(MD5Utility.GetSlaveID(md5, slave_count), 
                            new ElementRequestWriter(e));
                    }
                    else
                    {
                        double double_value = 0;
                        if (!double.TryParse(value_raw.Trim(), out double_value))
                        {
                            Console.WriteLine("don't understand what you're talking about.");
                            continue;
                        }
                        Console.WriteLine("setting the value under \"{0}\" to a double", key_raw);

                        //notice! since it's a double value, set the other two arguments to be null, so they won't take extra space
                        Element e = new Element(key_raw, new Value(double_value, null, null));
                        long md5 = MD5Utility.GetMD5(key_raw);
                        Global.CloudStorage.SetValueToMySlave(MD5Utility.GetSlaveID(md5, slave_count), 
                            new ElementRequestWriter(e));
                    }
                }
                #endregion
                #region it is a get command
                else if (getmatch.Length > 0)
                {
                    string key_raw = getmatch.Groups[1].ToString().Trim();

                    long md5 = MD5Utility.GetMD5(key_raw);
                    ValueResponseReader reader = Global.CloudStorage.GetValueToMySlave(
                        MD5Utility.GetSlaveID(md5, slave_count), new StringRequestWriter(key_raw));
                    if (reader.isFound)
                    {
                        if (reader.value.Contains_int_list_value)
                        {
                            Console.WriteLine("the value under \"{0}\" is a List", key_raw);
                            Console.Write("List of Int: [");
                            reader.value.int_list_value.ForEach(i => { Console.Write(i + ","); });
                            Console.WriteLine("]");
                        }
                        else if (reader.value.Contains_string_value)
                        {
                            Console.WriteLine("the value under \"{0}\" is a string", key_raw);
                            Console.WriteLine(reader.value.string_value);
                        }
                        else if (reader.value.Contains_double_value)
                        {
                            Console.WriteLine("the value under \"{0}\" is a double", key_raw);
                            Console.WriteLine(reader.value.double_value);
                        }
                    }
                    else
                    {
                        Console.WriteLine("no such object exist under the key: \"{0}\"", key_raw);
                    }
                }
                #endregion
                else
                    Console.WriteLine("don't understand what you're talking about.");
            }
        }

    }

    class MD5Utility
    {
        internal static MD5 md5 = System.Security.Cryptography.MD5.Create();

        internal static unsafe long GetMD5(string s)
        {
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(s);
            byte[] hash = md5.ComputeHash(inputBytes);
            fixed (byte* p = hash)
            {
                return *(long*)p;
            }
        }

        internal static int GetSlaveID(long md5, int slave_count)
        {
            return (int)(((ulong)md5) % (uint)slave_count);
        }
    }
}

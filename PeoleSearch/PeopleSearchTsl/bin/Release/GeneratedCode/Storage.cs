using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using System.Security;
using Trinity;
using Trinity.Storage;
using Trinity.Utilities;
using Trinity.TSL.Lib;

using Trinity.Network;
using Trinity.Index;
using Trinity.Network.Sockets;
using Trinity.Network.Messaging;
using Trinity.Index.SQL;
using Trinity.TSL;
namespace Trinity
{

    static public class StorageExtension_UDT
    {
        public static Type[] GetCellStructureTypeArray(this Trinity.Storage.LocalMemoryStorage storage)
        {
            return new Type[] {
typeof(StorageExtension_node),
            };
        }
    }


    static public class StorageExtension_node
    {
		
        public unsafe static bool Savenode(this Trinity.Storage.LocalMemoryStorage storage,long CellID, string name=null,List<long> friends=null)
		{

        byte* targetPtr = null;

        if(name!= null)
        {
            targetPtr += name.Length*2+sizeof(int);
        }else
        {
            targetPtr += sizeof(int);
        }

if(friends!= null)
{
    targetPtr += friends.Count*8+sizeof(int);
}else
{
    targetPtr += sizeof(int);
}


        byte[] tmpcell = new byte[(int)(targetPtr)];
        fixed(byte* tmpcellptr = tmpcell)
        {
            targetPtr = tmpcellptr;

        if(name!= null)
        {
            *(int*)targetPtr = name.Length*2;
            targetPtr += sizeof(int);
            for(int iterator_0 = 0;iterator_0<name.Length;++iterator_0)
            {
*(char*)targetPtr = name[iterator_0];
            targetPtr += 2;

            }
        }else
        {
            *(int*)targetPtr = 0;
            targetPtr += sizeof(int);
        }

if(friends!= null)
{
    *(int*)targetPtr = friends.Count*8;
    targetPtr += sizeof(int);
    for(int iterator_0 = 0;iterator_0<friends.Count;++iterator_0)
    {
*(long*)targetPtr = friends[iterator_0];
            targetPtr += 8;

    }

}else
{
    *(int*)targetPtr = 0;
    targetPtr += sizeof(int);
}

        }

        return storage.SaveCell(CellID, tmpcell, (ushort)CellType.node);
    }

        public unsafe static bool Savenode(this Trinity.Storage.LocalMemoryStorage storage, long CellID, node CellContent)
        {
            return Savenode(storage,CellID, CellContent.name, CellContent.friends);
        }
        public unsafe static bool Savenode(this Trinity.Storage.LocalMemoryStorage storage, node CellContent)
        {
            return Savenode(storage,CellContent.CellID, CellContent.name, CellContent.friends);
        }
        public unsafe static node_Accessor Usenode(this Trinity.Storage.LocalMemoryStorage storage,long CellID, ActionOnCellNotFound action)
        {

            return node_Accessor.New(CellID,action);
        }
        public unsafe static node_Accessor Usenode(this Trinity.Storage.LocalMemoryStorage storage,long CellID)
        {

            return node_Accessor.New(CellID,ActionOnCellNotFound.ThrowException);
        }
        public unsafe static node Loadnode(this Trinity.Storage.LocalMemoryStorage storage, long CellID)
        {
            using (var cell = new node_Accessor(CellID, ActionOnCellNotFound.ThrowException))
            {
                node ret = cell;
                ret.CellID = CellID;
                return ret;
            }
        }
				public unsafe static bool Savenode(this Trinity.Storage.MemoryCloud storage,long CellID, string name=null,List<long> friends=null)
		{

        byte* targetPtr = null;

        if(name!= null)
        {
            targetPtr += name.Length*2+sizeof(int);
        }else
        {
            targetPtr += sizeof(int);
        }

if(friends!= null)
{
    targetPtr += friends.Count*8+sizeof(int);
}else
{
    targetPtr += sizeof(int);
}


        byte[] tmpcell = new byte[(int)(targetPtr)];
        fixed(byte* tmpcellptr = tmpcell)
        {
            targetPtr = tmpcellptr;

        if(name!= null)
        {
            *(int*)targetPtr = name.Length*2;
            targetPtr += sizeof(int);
            for(int iterator_0 = 0;iterator_0<name.Length;++iterator_0)
            {
*(char*)targetPtr = name[iterator_0];
            targetPtr += 2;

            }
        }else
        {
            *(int*)targetPtr = 0;
            targetPtr += sizeof(int);
        }

if(friends!= null)
{
    *(int*)targetPtr = friends.Count*8;
    targetPtr += sizeof(int);
    for(int iterator_0 = 0;iterator_0<friends.Count;++iterator_0)
    {
*(long*)targetPtr = friends[iterator_0];
            targetPtr += 8;

    }

}else
{
    *(int*)targetPtr = 0;
    targetPtr += sizeof(int);
}

        }

        return storage.SaveCell(CellID, tmpcell,  (ushort)CellType.node);
    }

        public unsafe static bool Savenode(this Trinity.Storage.MemoryCloud storage, long CellID, node CellContent)
        {
            return Savenode(storage,CellID, CellContent.name, CellContent.friends);
        }

        public unsafe static bool Savenode(this Trinity.Storage.MemoryCloud storage, node CellContent)
        {
            return Savenode(storage,CellContent.CellID, CellContent.name, CellContent.friends);
        }

        public unsafe static node Loadnode(this Trinity.Storage.MemoryCloud storage, long CellID)
        {
            byte[] content = Global.CloudStorage.LoadCell(CellID);
            fixed(byte* ptr = content)
            {
                using (var cell = new node_Accessor(ptr))
                {
                    node ret = cell;
                    ret.CellID = CellID;
                    return ret;
                }
            }
        }
}

    public static class Storage_RelationalIndexing_Extension
    {
        public static void BuildIndex(this LocalMemoryStorage storage)
        {
        }

        public static void CloseSearcher(this LocalMemoryStorage storage)
        {
        }

        public static void InitializeSearcher(this LocalMemoryStorage storage)
        {
        }

        public static void RegisterIndexer(this LocalMemoryStorage storage)
        {
        }

        public static long[] Search(this LocalMemoryStorage storage, string pattern, int max = -1)
        {
            return new long[0];
        }
    }


    public abstract class CellConverter
    {
        internal static Dictionary<Type, CellType> CellTypeMap = new Dictionary<Type, CellType>
        {
            { typeof(node), CellType.node} ,
        };
        internal CellConverter(CellType source, CellType ptr1)
        {
            SourceCellType = source;
            TargetCellType = ptr1;
        }
        public readonly CellType SourceCellType;
        public readonly CellType TargetCellType;
        internal CellTransformAction<long, int, ushort> _action;
    }
    public class CellConverter<TInput, TOutput>
        : CellConverter
    {
        static internal CellType _SourceCellType;
        static internal CellType _TargetCellType;
        static internal Func<long, long, object> InputFunction = null;
        static internal Func<object, byte[]> OutputFunction = null;
        static internal bool InvalidConverter;
        [ThreadStatic]
        static internal node_Accessor Static_node_Accessor;
        static unsafe CellConverter()
        {
            Type tIn = typeof(TInput);
            Type tOut = typeof(TOutput);
            try
            {
                _SourceCellType = CellConverter.CellTypeMap[tIn];
                _TargetCellType = CellConverter.CellTypeMap[tOut];
            }
            catch (Exception)
            {
                InvalidConverter = true;
            }
            switch(_SourceCellType)
            {
                case CellType.node:
                    InputFunction = (CellID, long_ptr) =>
                        {
                            if (Static_node_Accessor == default(node_Accessor))
                                Static_node_Accessor = new node_Accessor(null);
                            Static_node_Accessor.CellPtr = (byte*)long_ptr; // + 1;
                            return (node)Static_node_Accessor;
                        };
                    break;
            }
            switch(_TargetCellType)
            {
                case CellType.node:
                    OutputFunction = (cell) =>
                        {
                            Static_node_Accessor = (node)cell;
                            return Static_node_Accessor.ToByteArray();
                        };
                    break;
            }
        }
        public unsafe CellConverter(Func<TInput, TOutput> ConvertFunction) :
            base(_SourceCellType, _TargetCellType)
        {
            if (InvalidConverter)
            {
                throw new Exception(string.Format(
                    "This type of converter ({0} to {1}) is not supported, make sure you pass in a cell-type-to-cell-type converter.",
                    typeof(TInput), typeof(TOutput)));
            }
            _action = (byte* ptr_long, long id, int count, ref ushort cellType) =>
                {
                    TInput input = (TInput)InputFunction(id, (long)ptr_long);
                    TOutput output = ConvertFunction(input);
                    cellType = (ushort)_TargetCellType;
                    return OutputFunction(output);
                };
        }
    }
    public class CellAccessorAction
    {
        public unsafe CellAccessorAction(Action<node_Accessor> action)
        {
            CellType = CellType.node;
            _action = (cellID, long_ptr, CellEntryIndex) =>
                {
                    node_Accessor accessor = new node_Accessor((byte*)long_ptr);
                    accessor.CellID = cellID;
                    accessor.CellEntryIndex = CellEntryIndex;
                    action(accessor);
                };
        }
        internal Action<long, long, int> _action;
        internal CellType CellType;
    }
    static public class Storage_ForEach_Extension
    {
        /// <summary>
        /// Perform actions to each corresponding cell in Trinity local storage. The actions are performed in parallel.
        /// </summary>
        /// <param name="actions">A list of CellAccessorActions.</param>
        public static unsafe void ForEach(this Trinity.Storage.LocalMemoryStorage storage, params CellAccessorAction[] actions)
        {
            Dictionary<ushort, Action<long, long, int>> actionMap = new Dictionary<ushort, Action<long, long, int>>();
            foreach (var action in actions)
            {
                actionMap[(ushort)action.CellType] = action._action;
            }
            storage.ForEach(
                new CellAction<long, int, int, ushort>(              
                    (byte* ptr, long id, int size, int idx, ushort cellType) =>
                    {
                    if (actionMap.ContainsKey(cellType))
                        actionMap[cellType](id, (long)ptr, idx);
                    }
                ));
        }
        /// <summary>
        /// Perform actions to each corresponding cell in Trinity local storage.
        /// </summary>
        /// <param name="parallel">true indicates the actions are performed in parallel; otherwise, the actions are performed sequentially.</param>
        /// <param name="actions">A list of CellAccessorActions.</param>
        public static unsafe void ForEach(this Trinity.Storage.LocalMemoryStorage storage, bool parallel, params CellAccessorAction[] actions)
        {
            Dictionary<ushort, Action<long, long, int>> actionMap = new Dictionary<ushort, Action<long, long, int>>();
            foreach (var action in actions)
            {
                actionMap[(ushort)action.CellType] = action._action;
            }
            storage.ForEach(
                new CellAction<long, int, int, ushort>(
                (byte* ptr, long id, int size, int idx, ushort cellType) =>
                {
                    if (actionMap.ContainsKey(cellType))
                        actionMap[cellType](id, (long)ptr, idx);
                }), parallel);
        }
        
        /// <summary>
        /// Perform actions to each corresponding cell in Trinity local storage.
        /// </summary>
        /// <param name="dispose">true indicates that current memory trunk will be disposed once it is processed; otherwise, the memory is kept in-memory.</param>
        /// <param name="parallel">true indicates the actions are performed in parallel; otherwise, the actions are performed sequentially.</param>
        /// <param name="actions">A list of CellAccessorActions.</param>
        public static unsafe void ForEach(this Trinity.Storage.LocalMemoryStorage storage, bool dispose, bool parallel, params CellAccessorAction[] actions)
        {
            Dictionary<ushort, Action<long, long, int>> actionMap = new Dictionary<ushort, Action<long, long, int>>();
            foreach (var action in actions)
            {
                actionMap[(ushort)action.CellType] = action._action;
            }

            if (parallel)
            {
                Parallel.ForEach<MemoryTrunk>(storage.memory_trunks, mem_trunk =>
                {
                    mem_trunk.ForEach(
                    new CellAction<long, int, int, ushort>(
                    (byte* ptr, long id, int size, int idx, ushort cellType) =>
                    {
                	    if (actionMap.ContainsKey(cellType))
                	        actionMap[cellType](id, (long)ptr, idx);
                	}));
                    if(dispose)
                    {
                        mem_trunk.Save();
                        mem_trunk.Dispose();
                    }
                });
            }
            else
            {
                for (int i = 0; i < storage.memory_trunks.Length; i++)
                {
                    storage.memory_trunks[i].ForEach(
                    new CellAction<long, int, int, ushort>(
                    (byte* ptr, long id, int size, int idx, ushort cellType) =>
                    {
                	    if (actionMap.ContainsKey(cellType))
                	        actionMap[cellType](id, (long)ptr, idx);
                	}));
                    if(dispose)
                	{
                	    storage.memory_trunks[i].Save();
                	    storage.memory_trunks[i].Dispose();
                	}
                }
            }
        }

    }

    static public class Storage_CellType_Extension
    {
        public unsafe static bool Isnode(this Trinity.Storage.LocalMemoryStorage storage, long CellID)
        {
            //int idx;
            //int size;
            //byte* ptr = storage.GetLockedUnsafeCellLocation(CellID, out size, out idx);
            //if (ptr == null)
            //    throw new Exception(string.Format("Cell #{0} doesn't exist.", CellID));
            //bool ret = *ptr == (byte)CellType.node;
            //storage.ReleaseCellLock(CellID, idx);
            //return ret;
            return storage.GetCellType(CellID) == (ushort)CellType.node;
        }
        public unsafe static CellType GetCellType(this Trinity.Storage.LocalMemoryStorage storage, long CellID)
        {
            //int idx;
            //int size;
            //byte* ptr = storage.GetLockedUnsafeCellLocation(CellID, out size, out idx);
            //if (ptr == null)
            //    throw new Exception(string.Format("Cell #{0} doesn't exist.", CellID));
            //CellType ret = (CellType)(*ptr);
            //storage.ReleaseCellLock(CellID, idx);
            //return ret;
            return (CellType)storage.GetCellType(CellID);
        }
        public unsafe static void Transform(this Trinity.Storage.LocalMemoryStorage storage, params CellConverter[] converters)
        {
            Dictionary<ushort, CellTransformAction<long,int,ushort>> converterMap = new Dictionary<ushort, CellTransformAction<long,int,ushort>>();
            foreach (var converter in converters)
            {
                converterMap[(ushort)converter.SourceCellType] = converter._action;
            }
            storage.TransformCells((byte* ptr, long id,  int count, ref ushort cellType) =>
                {
                    //ushort cellType = *(ushort*)ptr;
                    if (converterMap.ContainsKey(cellType))
                    {
                        return converterMap[cellType](ptr, id, count, ref cellType);
                    }
                    else
                    {
                        byte[] ret = new byte[count];
                        Memory.Copy(ptr, 0, ret, 0, count);
                        return ret;
                    }
                });
        }
    }
}

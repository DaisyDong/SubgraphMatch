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

    public struct node
    {

        public long CellID;		
        public node(long cell_id, string name=null,List<long> friends=null)
		{
            this.name = name;
            this.friends = friends;
		
CellID = cell_id;
		}
		
        public node(string name=null,List<long> friends=null)
		{
            this.name = name;
            this.friends = friends;

		CellID = CellIDFactory.NewCellID();
		}

        public unsafe node(AccessorBuffer buffer)
        {
            byte[] cellContent = buffer.Buffer;
            fixed(byte* ptr = cellContent)
            {
                using(var accessor = new node_Accessor(ptr))
                {
                    this.name = accessor.name;this.friends = accessor.friends;
                }
				CellID = CellIDFactory.NewCellID();
            }
        }
        public unsafe node(long cell_id, AccessorBuffer buffer)
        {
            byte[] cellContent = buffer.Buffer;
            fixed(byte* ptr = cellContent)
            {
                using(var accessor = new node_Accessor(ptr))
                {
                    this.name = accessor.name;this.friends = accessor.friends;
                }
                CellID = cell_id;
            }
        }
        public string name;

        public List<long> friends;

        public static implicit operator Tuple<string,List<long>>(node FormatStruct)
        {
            return new Tuple<string,List<long>>(FormatStruct.name,FormatStruct.friends);
        }

        public static implicit operator node (Tuple<string,List<long>>tuple)
        {
            return new node(tuple.Item1,tuple.Item2);
        }

        public static implicit operator KeyValuePair<string,List<long>>(node FormatStruct)
        {
            return new KeyValuePair<string,List<long>>(FormatStruct.name,FormatStruct.friends);
        }

        public static implicit operator node (KeyValuePair<string,List<long>>tuple)
        {
                return new node(tuple.Key,tuple.Value);
        }

        public static bool operator == (node a, node b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }
            // Return true if the fields match:
            return a.name == b.name && a.friends == b.friends;
        }

        public static bool operator != (node a, node b)
        {
            return !(a == b);
        }

    }

    public unsafe class node_Accessor: IDisposable
    {
        internal byte* CellPtr;
        internal int CellEntryIndex;
        public long? CellID;
        private GCHandle handle;
        internal unsafe byte* ResizeFunction(byte* ptr, int ptr_offset, int delta)
        {
            int offset = (int)(ptr - CellPtr) + ptr_offset;
            CellPtr = Global.LocalStorage.ResizeCell((long)CellID,CellEntryIndex,offset,delta);
            return CellPtr + (offset - ptr_offset);
        }

        public node_Accessor(long cellId, byte[] buffer)
        {
            this.CellID = cellId;
            handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            this.CellPtr = (byte*)handle.AddrOfPinnedObject().ToPointer();
        name_Accessor_Field = new BlobString(null,
                (ptr,ptr_offset,delta)=>
                {
                    int substructure_offset = (int)(ptr - this.CellPtr);
                    this.ResizeFunction(this.CellPtr, ptr_offset + substructure_offset, delta);
                    return this.CellPtr + substructure_offset;
                });

        friends_Accessor_Field = new longList(null,
                (ptr,ptr_offset,delta)=>
                {
                    int substructure_offset = (int)(ptr - this.CellPtr);
                    this.ResizeFunction(this.CellPtr, ptr_offset + substructure_offset, delta);
                    return this.CellPtr + substructure_offset;
                });

            this.CellEntryIndex = -1;
        }

        public ushort GetCellType()
        {
            if(!CellID.HasValue)
            {
                throw new Exception("The cell you're trying to get cell type must have a cellID.");
            } 
            return Global.LocalStorage.GetCellType(CellID.Value);
        }

        internal static string[] optional_field_names;
        public static string[] GetOptionalFieldNames()
        {
            if(optional_field_names == null)
                optional_field_names = new string[] {};
            return optional_field_names;   
        }

        internal List<string> GetNotNullOptionalFields()
        {
            List<string> list = new List<string>();
            BitArray ba = new BitArray(GetOptionalFieldMap());
            string[] optional_fields = GetOptionalFieldNames();
            for (int i = 0; i < ba.Count; i++)
            {
                if(ba[i])
                    list.Add(optional_fields[i]);
            }
            return list;
        }

        internal unsafe byte[] GetOptionalFieldMap()
        {
            return new byte[0];
        }

        public byte[] ToByteArray()
        {
            byte* targetPtr = CellPtr;
            targetPtr += 4 + *(int*)targetPtr;
            targetPtr += 4 + *(int*)targetPtr;

            int size = (int)(targetPtr - CellPtr);
            byte[] ret = new byte[size];
            Memory.Copy(CellPtr,0,ret,0,size);
            return ret;
        }
        [ThreadStatic]
        internal static Stack<node_Accessor> accessorPool;
        internal static node_Accessor New(long CellID, ActionOnCellNotFound action)
        {
            if(accessorPool == null)
            {
                accessorPool = new Stack<node_Accessor>();
            }
            node_Accessor ret = null;
            if(accessorPool.Count != 0)
            {
                ret = accessorPool.Pop();
                int tmp;
                ret.CellPtr = Global.LocalStorage.GetLockedUnsafeCellLocation(CellID, out tmp, out ret.CellEntryIndex);
                if (ret.CellPtr == null)
                {
                    if(action == ActionOnCellNotFound.ThrowException)
                        throw new CellNotFoundException("Cell with ID=" + CellID + " not found!");
                    else if (action == ActionOnCellNotFound.CreateNew)
                    {
                        int size;
                        byte[] defaultContent = construct(CellID);
                        ret.CellPtr = Global.LocalStorage.AddOrUse(CellID,out size,out ret.CellEntryIndex,defaultContent, (ushort)CellType.node);
                    }else
                    {
                        accessorPool.Push(ret);
                        return null;
                    }
                }
                ret.CellID = CellID;
                return ret;
            }
            ret = new node_Accessor(CellID, action);
            if(ret.CellID == -1 && CellID != -1)
                return null;
            else
                return ret;
        }

        internal unsafe node_Accessor(long CellID, ActionOnCellNotFound action)
        {
            int tmp;
            CellPtr = Global.LocalStorage.GetLockedUnsafeCellLocation(CellID, out tmp, out CellEntryIndex);
            if (CellPtr == null)
            {
                if(action == ActionOnCellNotFound.ThrowException)
                    throw new CellNotFoundException("Cell with ID=" + CellID + " not found!");
                else if (action == ActionOnCellNotFound.CreateNew)
                {
                    int size;
                    byte[] defaultContent = construct(CellID);
                    CellPtr = Global.LocalStorage.AddOrUse(CellID,out size,out CellEntryIndex,defaultContent,(ushort)CellType.node);
                }else
                {
                    this.CellID = -1;
                    return;
                }
            }

        name_Accessor_Field = new BlobString(null,
                (ptr,ptr_offset,delta)=>
                {
                    int substructure_offset = (int)(ptr - this.CellPtr);
                    this.ResizeFunction(this.CellPtr, ptr_offset + substructure_offset, delta);
                    return this.CellPtr + substructure_offset;
                });

        friends_Accessor_Field = new longList(null,
                (ptr,ptr_offset,delta)=>
                {
                    int substructure_offset = (int)(ptr - this.CellPtr);
                    this.ResizeFunction(this.CellPtr, ptr_offset + substructure_offset, delta);
                    return this.CellPtr + substructure_offset;
                });

            this.CellID = CellID;
        }
        internal unsafe node_Accessor(byte* _CellPtr)
        {
            CellPtr = _CellPtr;
        name_Accessor_Field = new BlobString(null,
                (ptr,ptr_offset,delta)=>
                {
                    int substructure_offset = (int)(ptr - this.CellPtr);
                    this.ResizeFunction(this.CellPtr, ptr_offset + substructure_offset, delta);
                    return this.CellPtr + substructure_offset;
                });

        friends_Accessor_Field = new longList(null,
                (ptr,ptr_offset,delta)=>
                {
                    int substructure_offset = (int)(ptr - this.CellPtr);
                    this.ResizeFunction(this.CellPtr, ptr_offset + substructure_offset, delta);
                    return this.CellPtr + substructure_offset;
                });

            this.CellEntryIndex = -1;
        }
        public void Dispose()
        {
            if(CellEntryIndex >= 0)
            {
                Global.LocalStorage.ReleaseCellLock((long)CellID, CellEntryIndex);
                if(accessorPool != null && accessorPool.Count < 128)
                {
                    accessorPool.Push(this);
                }
            }
            
            if(handle != null && handle.IsAllocated)
                handle.Free();
        }
		internal static unsafe byte[] construct(long CellID, string name=null,List<long> friends=null)
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

            return tmpcell;
        }
BlobString name_Accessor_Field;
        public unsafe BlobString name
        {
            get
            {
                
                byte* targetPtr = CellPtr;

                name_Accessor_Field.CellPtr = targetPtr + 4;
                name_Accessor_Field.CellID = this.CellID;
                return name_Accessor_Field;
            }

            set
            {
                if ((object)value == null) throw new ArgumentNullException("The assigned variable is null.");
                byte* targetPtr = CellPtr;

                name_Accessor_Field.CellID = this.CellID;
                
              int length = *(int*)(value.CellPtr - 4);

                //senario: cell_a.inlinks = cell_b.inlinks,
                //the later part will invoke the Get, filling cell_b.inlinks(a inlink_accessor_fiedld)'storage CellID
                int oldlength = *(int*)targetPtr;
                if (value.CellID != name_Accessor_Field.CellID)
                {
                    //if not in the same Cell
                    name_Accessor_Field.CellPtr = name_Accessor_Field.ResizeFunction(targetPtr, 0, length - oldlength);
                    Memory.Copy(value.CellPtr - 4, name_Accessor_Field.CellPtr, length + 4);
                }
                else
                {
                    byte[] tmpcell = new byte[length + 4];
                    fixed (byte* tmpcellptr = tmpcell)
                    {                        
                        Memory.Copy(value.CellPtr - 4, tmpcellptr, length + 4);
                        name_Accessor_Field.CellPtr = name_Accessor_Field.ResizeFunction(targetPtr, 0, length - oldlength);
                        Memory.Copy(tmpcellptr, name_Accessor_Field.CellPtr, length + 4);
                    }
                }
                name_Accessor_Field.CellPtr += 4;
                
            }

    }
longList friends_Accessor_Field;
        public unsafe longList friends
        {
            get
            {
                
                byte* targetPtr = CellPtr;
            targetPtr += 4 + *(int*)targetPtr;

                friends_Accessor_Field.CellPtr = targetPtr + 4;
                friends_Accessor_Field.CellID = this.CellID;
                return friends_Accessor_Field;
            }

            set
            {
                if ((object)value == null) throw new ArgumentNullException("The assigned variable is null.");
                byte* targetPtr = CellPtr;
            targetPtr += 4 + *(int*)targetPtr;

                friends_Accessor_Field.CellID = this.CellID;
                
              int length = *(int*)(value.CellPtr - 4);

                //senario: cell_a.inlinks = cell_b.inlinks,
                //the later part will invoke the Get, filling cell_b.inlinks(a inlink_accessor_fiedld)'storage CellID
                int oldlength = *(int*)targetPtr;
                if (value.CellID != friends_Accessor_Field.CellID)
                {
                    //if not in the same Cell
                    friends_Accessor_Field.CellPtr = friends_Accessor_Field.ResizeFunction(targetPtr, 0, length - oldlength);
                    Memory.Copy(value.CellPtr - 4, friends_Accessor_Field.CellPtr, length + 4);
                }
                else
                {
                    byte[] tmpcell = new byte[length + 4];
                    fixed (byte* tmpcellptr = tmpcell)
                    {                        
                        Memory.Copy(value.CellPtr - 4, tmpcellptr, length + 4);
                        friends_Accessor_Field.CellPtr = friends_Accessor_Field.ResizeFunction(targetPtr, 0, length - oldlength);
                        Memory.Copy(tmpcellptr, friends_Accessor_Field.CellPtr, length + 4);
                    }
                }
                friends_Accessor_Field.CellPtr += 4;
                
            }

    }

        public static unsafe implicit operator node(node_Accessor accessor)
        {
            
            if(accessor.CellID != null)
            return new node(accessor.CellID.Value,accessor.name,accessor.friends);
            else
            return new node(accessor.name,accessor.friends);
        }

        public unsafe static implicit operator node_Accessor(node guid)
        {  
            byte* targetPtr = null;
            
        {
        if(guid.name!= null)
        {
            targetPtr += guid.name.Length*2+sizeof(int);
        }else
        {
            targetPtr += sizeof(int);
        }

if(guid.friends!= null)
{
    targetPtr += guid.friends.Count*8+sizeof(int);
}else
{
    targetPtr += sizeof(int);
}


        }
            byte* tmpcellptr = BufferAllocator.AllocBuffer((int)targetPtr);
            targetPtr = tmpcellptr;
        
        {
        if(guid.name!= null)
        {
            *(int*)targetPtr = guid.name.Length*2;
            targetPtr += sizeof(int);
            for(int iterator_1 = 0;iterator_1<guid.name.Length;++iterator_1)
            {
*(char*)targetPtr = guid.name[iterator_1];
            targetPtr += 2;

            }
        }else
        {
            *(int*)targetPtr = 0;
            targetPtr += sizeof(int);
        }

if(guid.friends!= null)
{
    *(int*)targetPtr = guid.friends.Count*8;
    targetPtr += sizeof(int);
    for(int iterator_1 = 0;iterator_1<guid.friends.Count;++iterator_1)
    {
*(long*)targetPtr = guid.friends[iterator_1];
            targetPtr += 8;

    }

}else
{
    *(int*)targetPtr = 0;
    targetPtr += sizeof(int);
}

        }
            node_Accessor ret = new node_Accessor(tmpcellptr);
            ret.CellID = guid.CellID;
            return ret;
        }

        public static bool operator == (node_Accessor a, node b)
        {
            node_Accessor bb = b;
            return (a == bb);
        }

        public static bool operator != (node_Accessor a, node b)
        {
            return !(a == b);
        }

        public static bool operator == (node_Accessor a, node_Accessor b)
        {
            // If both are null, return true.
            if ((object)a == null && (object)b == null) return true;
            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;
            // If both are same instance, return true.
            if (a.CellPtr == b.CellPtr) return true;
            byte* targetPtr = a.CellPtr;
            targetPtr += 4 + *(int*)targetPtr;
            targetPtr += 4 + *(int*)targetPtr;

            int lengthA = (int)(targetPtr - a.CellPtr);
            targetPtr = b.CellPtr;
            targetPtr += 4 + *(int*)targetPtr;
            targetPtr += 4 + *(int*)targetPtr;

            int lengthB = (int)(targetPtr - b.CellPtr);
            if(lengthA != lengthB) return false;
            return Memory.Compare(a.CellPtr,b.CellPtr,lengthA);
        }

        public static bool operator != (node_Accessor a, node_Accessor b)
        {
            return !(a == b);
        }
	}

}

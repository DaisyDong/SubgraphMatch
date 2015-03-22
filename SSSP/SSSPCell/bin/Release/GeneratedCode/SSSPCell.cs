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

    public struct SSSPCell
    {

        public long CellID;		
        public SSSPCell(long cell_id, int distance=default(int),long parent=default(long),List<long> neighbors=null)
		{
            this.distance = distance;
            this.parent = parent;
            this.neighbors = neighbors;
		
CellID = cell_id;
		}
		
        public SSSPCell(int distance=default(int),long parent=default(long),List<long> neighbors=null)
		{
            this.distance = distance;
            this.parent = parent;
            this.neighbors = neighbors;

		CellID = CellIDFactory.NewCellID();
		}

        public unsafe SSSPCell(AccessorBuffer buffer)
        {
            byte[] cellContent = buffer.Buffer;
            fixed(byte* ptr = cellContent)
            {
                using(var accessor = new SSSPCell_Accessor(ptr))
                {
                    this.distance = accessor.distance;this.parent = accessor.parent;this.neighbors = accessor.neighbors;
                }
				CellID = CellIDFactory.NewCellID();
            }
        }
        public unsafe SSSPCell(long cell_id, AccessorBuffer buffer)
        {
            byte[] cellContent = buffer.Buffer;
            fixed(byte* ptr = cellContent)
            {
                using(var accessor = new SSSPCell_Accessor(ptr))
                {
                    this.distance = accessor.distance;this.parent = accessor.parent;this.neighbors = accessor.neighbors;
                }
                CellID = cell_id;
            }
        }
        public int distance;

        public long parent;

        public List<long> neighbors;

        public static implicit operator Tuple<int,long,List<long>>(SSSPCell FormatStruct)
        {
            return new Tuple<int,long,List<long>>(FormatStruct.distance,FormatStruct.parent,FormatStruct.neighbors);
        }

        public static implicit operator SSSPCell (Tuple<int,long,List<long>>tuple)
        {
            return new SSSPCell(tuple.Item1,tuple.Item2,tuple.Item3);
        }

        public static bool operator == (SSSPCell a, SSSPCell b)
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
            return a.distance == b.distance && a.parent == b.parent && a.neighbors == b.neighbors;
        }

        public static bool operator != (SSSPCell a, SSSPCell b)
        {
            return !(a == b);
        }

    }

    public unsafe class SSSPCell_Accessor: IDisposable
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

        public SSSPCell_Accessor(long cellId, byte[] buffer)
        {
            this.CellID = cellId;
            handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            this.CellPtr = (byte*)handle.AddrOfPinnedObject().ToPointer();
        neighbors_Accessor_Field = new longList(null,
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
            targetPtr += 12;
            targetPtr += 4 + *(int*)targetPtr;

            int size = (int)(targetPtr - CellPtr);
            byte[] ret = new byte[size];
            Memory.Copy(CellPtr,0,ret,0,size);
            return ret;
        }
        [ThreadStatic]
        internal static Stack<SSSPCell_Accessor> accessorPool;
        internal static SSSPCell_Accessor New(long CellID, ActionOnCellNotFound action)
        {
            if(accessorPool == null)
            {
                accessorPool = new Stack<SSSPCell_Accessor>();
            }
            SSSPCell_Accessor ret = null;
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
                        ret.CellPtr = Global.LocalStorage.AddOrUse(CellID,out size,out ret.CellEntryIndex,defaultContent, (ushort)CellType.SSSPCell);
                    }else
                    {
                        accessorPool.Push(ret);
                        return null;
                    }
                }
                ret.CellID = CellID;
                return ret;
            }
            ret = new SSSPCell_Accessor(CellID, action);
            if(ret.CellID == -1 && CellID != -1)
                return null;
            else
                return ret;
        }

        internal unsafe SSSPCell_Accessor(long CellID, ActionOnCellNotFound action)
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
                    CellPtr = Global.LocalStorage.AddOrUse(CellID,out size,out CellEntryIndex,defaultContent,(ushort)CellType.SSSPCell);
                }else
                {
                    this.CellID = -1;
                    return;
                }
            }

        neighbors_Accessor_Field = new longList(null,
                (ptr,ptr_offset,delta)=>
                {
                    int substructure_offset = (int)(ptr - this.CellPtr);
                    this.ResizeFunction(this.CellPtr, ptr_offset + substructure_offset, delta);
                    return this.CellPtr + substructure_offset;
                });

            this.CellID = CellID;
        }
        internal unsafe SSSPCell_Accessor(byte* _CellPtr)
        {
            CellPtr = _CellPtr;
        neighbors_Accessor_Field = new longList(null,
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
		internal static unsafe byte[] construct(long CellID, int distance=default(int),long parent=default(long),List<long> neighbors=null)
		{

        byte* targetPtr = null;
targetPtr += 4;
targetPtr += 8;

if(neighbors!= null)
{
    targetPtr += neighbors.Count*8+sizeof(int);
}else
{
    targetPtr += sizeof(int);
}


        byte[] tmpcell = new byte[(int)(targetPtr)];
        fixed(byte* tmpcellptr = tmpcell)
        {
            targetPtr = tmpcellptr;
*(int*)targetPtr = distance;
            targetPtr += 4;
*(long*)targetPtr = parent;
            targetPtr += 8;

if(neighbors!= null)
{
    *(int*)targetPtr = neighbors.Count*8;
    targetPtr += sizeof(int);
    for(int iterator_0 = 0;iterator_0<neighbors.Count;++iterator_0)
    {
*(long*)targetPtr = neighbors[iterator_0];
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
int distance_Accessor_Field;
        public unsafe int distance
        {
            get
            {
                
                byte* targetPtr = CellPtr;

                return *(int*)(targetPtr);
            }

            set
            {
                byte* targetPtr = CellPtr;

                *(int*)(targetPtr) = value;
            }

        }
long parent_Accessor_Field;
        public unsafe long parent
        {
            get
            {
                
                byte* targetPtr = CellPtr;
            targetPtr += 4;

                return *(long*)(targetPtr);
            }

            set
            {
                byte* targetPtr = CellPtr;
            targetPtr += 4;

                *(long*)(targetPtr) = value;
            }

        }
longList neighbors_Accessor_Field;
        public unsafe longList neighbors
        {
            get
            {
                
                byte* targetPtr = CellPtr;
            targetPtr += 12;

                neighbors_Accessor_Field.CellPtr = targetPtr + 4;
                neighbors_Accessor_Field.CellID = this.CellID;
                return neighbors_Accessor_Field;
            }

            set
            {
                if ((object)value == null) throw new ArgumentNullException("The assigned variable is null.");
                byte* targetPtr = CellPtr;
            targetPtr += 12;

                neighbors_Accessor_Field.CellID = this.CellID;
                
              int length = *(int*)(value.CellPtr - 4);

                //senario: cell_a.inlinks = cell_b.inlinks,
                //the later part will invoke the Get, filling cell_b.inlinks(a inlink_accessor_fiedld)'storage CellID
                int oldlength = *(int*)targetPtr;
                if (value.CellID != neighbors_Accessor_Field.CellID)
                {
                    //if not in the same Cell
                    neighbors_Accessor_Field.CellPtr = neighbors_Accessor_Field.ResizeFunction(targetPtr, 0, length - oldlength);
                    Memory.Copy(value.CellPtr - 4, neighbors_Accessor_Field.CellPtr, length + 4);
                }
                else
                {
                    byte[] tmpcell = new byte[length + 4];
                    fixed (byte* tmpcellptr = tmpcell)
                    {                        
                        Memory.Copy(value.CellPtr - 4, tmpcellptr, length + 4);
                        neighbors_Accessor_Field.CellPtr = neighbors_Accessor_Field.ResizeFunction(targetPtr, 0, length - oldlength);
                        Memory.Copy(tmpcellptr, neighbors_Accessor_Field.CellPtr, length + 4);
                    }
                }
                neighbors_Accessor_Field.CellPtr += 4;
                
            }

    }

        public static unsafe implicit operator SSSPCell(SSSPCell_Accessor accessor)
        {
            
            if(accessor.CellID != null)
            return new SSSPCell(accessor.CellID.Value,accessor.distance,accessor.parent,accessor.neighbors);
            else
            return new SSSPCell(accessor.distance,accessor.parent,accessor.neighbors);
        }

        public unsafe static implicit operator SSSPCell_Accessor(SSSPCell guid)
        {  
            byte* targetPtr = null;
            
        {targetPtr += 4;
targetPtr += 8;

if(guid.neighbors!= null)
{
    targetPtr += guid.neighbors.Count*8+sizeof(int);
}else
{
    targetPtr += sizeof(int);
}


        }
            byte* tmpcellptr = BufferAllocator.AllocBuffer((int)targetPtr);
            targetPtr = tmpcellptr;
        
        {*(int*)targetPtr = guid.distance;
            targetPtr += 4;
*(long*)targetPtr = guid.parent;
            targetPtr += 8;

if(guid.neighbors!= null)
{
    *(int*)targetPtr = guid.neighbors.Count*8;
    targetPtr += sizeof(int);
    for(int iterator_1 = 0;iterator_1<guid.neighbors.Count;++iterator_1)
    {
*(long*)targetPtr = guid.neighbors[iterator_1];
            targetPtr += 8;

    }

}else
{
    *(int*)targetPtr = 0;
    targetPtr += sizeof(int);
}

        }
            SSSPCell_Accessor ret = new SSSPCell_Accessor(tmpcellptr);
            ret.CellID = guid.CellID;
            return ret;
        }

        public static bool operator == (SSSPCell_Accessor a, SSSPCell b)
        {
            SSSPCell_Accessor bb = b;
            return (a == bb);
        }

        public static bool operator != (SSSPCell_Accessor a, SSSPCell b)
        {
            return !(a == b);
        }

        public static bool operator == (SSSPCell_Accessor a, SSSPCell_Accessor b)
        {
            // If both are null, return true.
            if ((object)a == null && (object)b == null) return true;
            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;
            // If both are same instance, return true.
            if (a.CellPtr == b.CellPtr) return true;
            byte* targetPtr = a.CellPtr;
            targetPtr += 12;
            targetPtr += 4 + *(int*)targetPtr;

            int lengthA = (int)(targetPtr - a.CellPtr);
            targetPtr = b.CellPtr;
            targetPtr += 12;
            targetPtr += 4 + *(int*)targetPtr;

            int lengthB = (int)(targetPtr - b.CellPtr);
            if(lengthA != lengthB) return false;
            return Memory.Compare(a.CellPtr,b.CellPtr,lengthA);
        }

        public static bool operator != (SSSPCell_Accessor a, SSSPCell_Accessor b)
        {
            return !(a == b);
        }
	}

}

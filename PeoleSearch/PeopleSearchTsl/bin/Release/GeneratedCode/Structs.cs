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

    public enum CellType : ushort 
    {
        Undefined = 0,
        node,
    }
    public struct NameRequest
    {
		
        public NameRequest(int hop=default(int),string name=null,List<long> neighbours=null)
		{
            this.hop = hop;
            this.name = name;
            this.neighbours = neighbours;

		}

        public int hop;

        public string name;

        public List<long> neighbours;

        public static implicit operator Tuple<int,string,List<long>>(NameRequest FormatStruct)
        {
            return new Tuple<int,string,List<long>>(FormatStruct.hop,FormatStruct.name,FormatStruct.neighbours);
        }

        public static implicit operator NameRequest (Tuple<int,string,List<long>>tuple)
        {
            return new NameRequest(tuple.Item1,tuple.Item2,tuple.Item3);
        }

        public static bool operator == (NameRequest a, NameRequest b)
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
            return a.hop == b.hop && a.name == b.name && a.neighbours == b.neighbours;
        }

        public static bool operator != (NameRequest a, NameRequest b)
        {
            return !(a == b);
        }

    }

    public unsafe class NameRequest_Accessor
    {
        internal byte* CellPtr;
        internal long? CellID;
        internal ResizeFunctionPrototype ResizeFunction;
        internal unsafe NameRequest_Accessor(byte* _CellPtr, ResizeFunctionPrototype func)
        {
            CellPtr = _CellPtr;
            ResizeFunction = func;
            
        name_Accessor_Field = new BlobString(null,
                (ptr,ptr_offset,delta)=>
                {
                    int substructure_offset = (int)(ptr - this.CellPtr);
                    this.CellPtr = this.ResizeFunction(this.CellPtr, ptr_offset + substructure_offset, delta);
                    return this.CellPtr + substructure_offset;
                });

        neighbours_Accessor_Field = new longList(null,
                (ptr,ptr_offset,delta)=>
                {
                    int substructure_offset = (int)(ptr - this.CellPtr);
                    this.CellPtr = this.ResizeFunction(this.CellPtr, ptr_offset + substructure_offset, delta);
                    return this.CellPtr + substructure_offset;
                });

        }

        public static implicit operator NameRequest_Accessor(NameRequest_Accessor_ReadOnly accessor )
        {
            return new NameRequest_Accessor(accessor.CellPtr, accessor.ResizeFunction);
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
            targetPtr += 4;
            targetPtr += 4 + *(int*)targetPtr;
            targetPtr += 4 + *(int*)targetPtr;

            int size = (int)(targetPtr - CellPtr);
            byte[] ret = new byte[size];
            Memory.Copy(CellPtr,0,ret,0,size);
            return ret;
        }
int hop_Accessor_Field;
        public unsafe int hop
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
BlobString name_Accessor_Field;
        public unsafe BlobString name
        {
            get
            {
                
                byte* targetPtr = CellPtr;
            targetPtr += 4;

                name_Accessor_Field.CellPtr = targetPtr + 4;
                name_Accessor_Field.CellID = this.CellID;
                return name_Accessor_Field;
            }

            set
            {
                if ((object)value == null) throw new ArgumentNullException("The assigned variable is null.");
                byte* targetPtr = CellPtr;
            targetPtr += 4;

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
longList neighbours_Accessor_Field;
        public unsafe longList neighbours
        {
            get
            {
                
                byte* targetPtr = CellPtr;
            targetPtr += 4;
            targetPtr += 4 + *(int*)targetPtr;

                neighbours_Accessor_Field.CellPtr = targetPtr + 4;
                neighbours_Accessor_Field.CellID = this.CellID;
                return neighbours_Accessor_Field;
            }

            set
            {
                if ((object)value == null) throw new ArgumentNullException("The assigned variable is null.");
                byte* targetPtr = CellPtr;
            targetPtr += 4;
            targetPtr += 4 + *(int*)targetPtr;

                neighbours_Accessor_Field.CellID = this.CellID;
                
              int length = *(int*)(value.CellPtr - 4);

                //senario: cell_a.inlinks = cell_b.inlinks,
                //the later part will invoke the Get, filling cell_b.inlinks(a inlink_accessor_fiedld)'storage CellID
                int oldlength = *(int*)targetPtr;
                if (value.CellID != neighbours_Accessor_Field.CellID)
                {
                    //if not in the same Cell
                    neighbours_Accessor_Field.CellPtr = neighbours_Accessor_Field.ResizeFunction(targetPtr, 0, length - oldlength);
                    Memory.Copy(value.CellPtr - 4, neighbours_Accessor_Field.CellPtr, length + 4);
                }
                else
                {
                    byte[] tmpcell = new byte[length + 4];
                    fixed (byte* tmpcellptr = tmpcell)
                    {                        
                        Memory.Copy(value.CellPtr - 4, tmpcellptr, length + 4);
                        neighbours_Accessor_Field.CellPtr = neighbours_Accessor_Field.ResizeFunction(targetPtr, 0, length - oldlength);
                        Memory.Copy(tmpcellptr, neighbours_Accessor_Field.CellPtr, length + 4);
                    }
                }
                neighbours_Accessor_Field.CellPtr += 4;
                
            }

    }

        public static unsafe implicit operator NameRequest(NameRequest_Accessor accessor)
        {
            
            return new NameRequest(accessor.hop,accessor.name,accessor.neighbours);
        }

        public unsafe static implicit operator NameRequest_Accessor(NameRequest guid)
        {  
            byte* targetPtr = null;
            
        {targetPtr += 4;

        if(guid.name!= null)
        {
            targetPtr += guid.name.Length*2+sizeof(int);
        }else
        {
            targetPtr += sizeof(int);
        }

if(guid.neighbours!= null)
{
    targetPtr += guid.neighbours.Count*8+sizeof(int);
}else
{
    targetPtr += sizeof(int);
}


        }
            byte* tmpcellptr = BufferAllocator.AllocBuffer((int)targetPtr);
            targetPtr = tmpcellptr;
        
        {*(int*)targetPtr = guid.hop;
            targetPtr += 4;

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

if(guid.neighbours!= null)
{
    *(int*)targetPtr = guid.neighbours.Count*8;
    targetPtr += sizeof(int);
    for(int iterator_1 = 0;iterator_1<guid.neighbours.Count;++iterator_1)
    {
*(long*)targetPtr = guid.neighbours[iterator_1];
            targetPtr += 8;

    }

}else
{
    *(int*)targetPtr = 0;
    targetPtr += sizeof(int);
}

        }
            NameRequest_Accessor ret = new NameRequest_Accessor(tmpcellptr,null);
            ret.CellID = null;
            return ret;
        }

        public static bool operator == (NameRequest_Accessor a, NameRequest b)
        {
            NameRequest_Accessor bb = b;
            return (a == bb);
        }

        public static bool operator != (NameRequest_Accessor a, NameRequest b)
        {
            return !(a == b);
        }

        public static bool operator == (NameRequest_Accessor a, NameRequest_Accessor b)
        {
            // If both are null, return true.
            if ((object)a == null && (object)b == null) return true;
            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;
            // If both are same instance, return true.
            if (a.CellPtr == b.CellPtr) return true;
            byte* targetPtr = a.CellPtr;
            targetPtr += 4;
            targetPtr += 4 + *(int*)targetPtr;
            targetPtr += 4 + *(int*)targetPtr;

            int lengthA = (int)(targetPtr - a.CellPtr);
            targetPtr = b.CellPtr;
            targetPtr += 4;
            targetPtr += 4 + *(int*)targetPtr;
            targetPtr += 4 + *(int*)targetPtr;

            int lengthB = (int)(targetPtr - b.CellPtr);
            if(lengthA != lengthB) return false;
            return Memory.Compare(a.CellPtr,b.CellPtr,lengthA);
        }

        public static bool operator != (NameRequest_Accessor a, NameRequest_Accessor b)
        {
            return !(a == b);
        }

    }

    public unsafe class NameRequest_Accessor_ReadOnly
    {
        internal byte* CellPtr;
        internal long? CellID;
        internal ResizeFunctionPrototype ResizeFunction;
        internal unsafe NameRequest_Accessor_ReadOnly(byte* _CellPtr, ResizeFunctionPrototype func)
        {
            CellPtr = _CellPtr;
            ResizeFunction = func;
            
        name_Accessor_Field = new BlobString(null,
                (ptr,ptr_offset,delta)=>
                {
                    int substructure_offset = (int)(ptr - this.CellPtr);
                    this.CellPtr = this.ResizeFunction(this.CellPtr, ptr_offset + substructure_offset, delta);
                    return this.CellPtr + substructure_offset;
                });

        neighbours_Accessor_Field = new longList(null,
                (ptr,ptr_offset,delta)=>
                {
                    int substructure_offset = (int)(ptr - this.CellPtr);
                    this.CellPtr = this.ResizeFunction(this.CellPtr, ptr_offset + substructure_offset, delta);
                    return this.CellPtr + substructure_offset;
                });

        }

        public static implicit operator NameRequest_Accessor_ReadOnly(NameRequest_Accessor accessor )
        {
            return new NameRequest_Accessor_ReadOnly(accessor.CellPtr, accessor.ResizeFunction);
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
            targetPtr += 4;
            targetPtr += 4 + *(int*)targetPtr;
            targetPtr += 4 + *(int*)targetPtr;

            int size = (int)(targetPtr - CellPtr);
            byte[] ret = new byte[size];
            Memory.Copy(CellPtr,0,ret,0,size);
            return ret;
        }
int hop_Accessor_Field;
        public unsafe int hop
        {
            get
            {
                
                byte* targetPtr = CellPtr;

                return *(int*)(targetPtr);
            }

        }
BlobString name_Accessor_Field;
        public unsafe BlobString name
        {
            get
            {
                
                byte* targetPtr = CellPtr;
            targetPtr += 4;

                name_Accessor_Field.CellPtr = targetPtr + 4;
                name_Accessor_Field.CellID = this.CellID;
                return name_Accessor_Field;
            }

    }
longList neighbours_Accessor_Field;
        public unsafe longList neighbours
        {
            get
            {
                
                byte* targetPtr = CellPtr;
            targetPtr += 4;
            targetPtr += 4 + *(int*)targetPtr;

                neighbours_Accessor_Field.CellPtr = targetPtr + 4;
                neighbours_Accessor_Field.CellID = this.CellID;
                return neighbours_Accessor_Field;
            }

    }

        public static unsafe implicit operator NameRequest(NameRequest_Accessor_ReadOnly accessor)
        {
            
            return new NameRequest(accessor.hop,accessor.name,accessor.neighbours);
        }

        public unsafe static implicit operator NameRequest_Accessor_ReadOnly(NameRequest guid)
        {  
            byte* targetPtr = null;
            
        {targetPtr += 4;

        if(guid.name!= null)
        {
            targetPtr += guid.name.Length*2+sizeof(int);
        }else
        {
            targetPtr += sizeof(int);
        }

if(guid.neighbours!= null)
{
    targetPtr += guid.neighbours.Count*8+sizeof(int);
}else
{
    targetPtr += sizeof(int);
}


        }
            byte* tmpcellptr = BufferAllocator.AllocBuffer((int)targetPtr);
            targetPtr = tmpcellptr;
        
        {*(int*)targetPtr = guid.hop;
            targetPtr += 4;

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

if(guid.neighbours!= null)
{
    *(int*)targetPtr = guid.neighbours.Count*8;
    targetPtr += sizeof(int);
    for(int iterator_1 = 0;iterator_1<guid.neighbours.Count;++iterator_1)
    {
*(long*)targetPtr = guid.neighbours[iterator_1];
            targetPtr += 8;

    }

}else
{
    *(int*)targetPtr = 0;
    targetPtr += sizeof(int);
}

        }
            NameRequest_Accessor_ReadOnly ret = new NameRequest_Accessor_ReadOnly(tmpcellptr,null);
            ret.CellID = null;
            return ret;
        }

        public static bool operator == (NameRequest_Accessor_ReadOnly a, NameRequest b)
        {
            NameRequest_Accessor_ReadOnly bb = b;
            return (a == bb);
        }

        public static bool operator != (NameRequest_Accessor_ReadOnly a, NameRequest b)
        {
            return !(a == b);
        }

        public static bool operator == (NameRequest_Accessor_ReadOnly a, NameRequest_Accessor_ReadOnly b)
        {
            // If both are null, return true.
            if ((object)a == null && (object)b == null) return true;
            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;
            // If both are same instance, return true.
            if (a.CellPtr == b.CellPtr) return true;
            byte* targetPtr = a.CellPtr;
            targetPtr += 4;
            targetPtr += 4 + *(int*)targetPtr;
            targetPtr += 4 + *(int*)targetPtr;

            int lengthA = (int)(targetPtr - a.CellPtr);
            targetPtr = b.CellPtr;
            targetPtr += 4;
            targetPtr += 4 + *(int*)targetPtr;
            targetPtr += 4 + *(int*)targetPtr;

            int lengthB = (int)(targetPtr - b.CellPtr);
            if(lengthA != lengthB) return false;
            return Memory.Compare(a.CellPtr,b.CellPtr,lengthA);
        }

        public static bool operator != (NameRequest_Accessor_ReadOnly a, NameRequest_Accessor_ReadOnly b)
        {
            return !(a == b);
        }

    }

    public struct Result
    {
		
        public Result(List<long> matchPersons=null)
		{
            this.matchPersons = matchPersons;

		}

        public List<long> matchPersons;

        public static implicit operator Tuple<List<long>>(Result FormatStruct)
        {
            return new Tuple<List<long>>(FormatStruct.matchPersons);
        }

        public static implicit operator Result (Tuple<List<long>>tuple)
        {
            return new Result(tuple.Item1);
        }

        public static bool operator == (Result a, Result b)
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
            return a.matchPersons == b.matchPersons;
        }

        public static bool operator != (Result a, Result b)
        {
            return !(a == b);
        }

    }

    public unsafe class Result_Accessor
    {
        internal byte* CellPtr;
        internal long? CellID;
        internal ResizeFunctionPrototype ResizeFunction;
        internal unsafe Result_Accessor(byte* _CellPtr, ResizeFunctionPrototype func)
        {
            CellPtr = _CellPtr;
            ResizeFunction = func;
            
        matchPersons_Accessor_Field = new longList(null,
                (ptr,ptr_offset,delta)=>
                {
                    int substructure_offset = (int)(ptr - this.CellPtr);
                    this.CellPtr = this.ResizeFunction(this.CellPtr, ptr_offset + substructure_offset, delta);
                    return this.CellPtr + substructure_offset;
                });

        }

        public static implicit operator Result_Accessor(Result_Accessor_ReadOnly accessor )
        {
            return new Result_Accessor(accessor.CellPtr, accessor.ResizeFunction);
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

            int size = (int)(targetPtr - CellPtr);
            byte[] ret = new byte[size];
            Memory.Copy(CellPtr,0,ret,0,size);
            return ret;
        }
longList matchPersons_Accessor_Field;
        public unsafe longList matchPersons
        {
            get
            {
                
                byte* targetPtr = CellPtr;

                matchPersons_Accessor_Field.CellPtr = targetPtr + 4;
                matchPersons_Accessor_Field.CellID = this.CellID;
                return matchPersons_Accessor_Field;
            }

            set
            {
                if ((object)value == null) throw new ArgumentNullException("The assigned variable is null.");
                byte* targetPtr = CellPtr;

                matchPersons_Accessor_Field.CellID = this.CellID;
                
              int length = *(int*)(value.CellPtr - 4);

                //senario: cell_a.inlinks = cell_b.inlinks,
                //the later part will invoke the Get, filling cell_b.inlinks(a inlink_accessor_fiedld)'storage CellID
                int oldlength = *(int*)targetPtr;
                if (value.CellID != matchPersons_Accessor_Field.CellID)
                {
                    //if not in the same Cell
                    matchPersons_Accessor_Field.CellPtr = matchPersons_Accessor_Field.ResizeFunction(targetPtr, 0, length - oldlength);
                    Memory.Copy(value.CellPtr - 4, matchPersons_Accessor_Field.CellPtr, length + 4);
                }
                else
                {
                    byte[] tmpcell = new byte[length + 4];
                    fixed (byte* tmpcellptr = tmpcell)
                    {                        
                        Memory.Copy(value.CellPtr - 4, tmpcellptr, length + 4);
                        matchPersons_Accessor_Field.CellPtr = matchPersons_Accessor_Field.ResizeFunction(targetPtr, 0, length - oldlength);
                        Memory.Copy(tmpcellptr, matchPersons_Accessor_Field.CellPtr, length + 4);
                    }
                }
                matchPersons_Accessor_Field.CellPtr += 4;
                
            }

    }

        public static unsafe implicit operator Result(Result_Accessor accessor)
        {
            
            return new Result(accessor.matchPersons);
        }

        public unsafe static implicit operator Result_Accessor(Result guid)
        {  
            byte* targetPtr = null;
            
        {
if(guid.matchPersons!= null)
{
    targetPtr += guid.matchPersons.Count*8+sizeof(int);
}else
{
    targetPtr += sizeof(int);
}


        }
            byte* tmpcellptr = BufferAllocator.AllocBuffer((int)targetPtr);
            targetPtr = tmpcellptr;
        
        {
if(guid.matchPersons!= null)
{
    *(int*)targetPtr = guid.matchPersons.Count*8;
    targetPtr += sizeof(int);
    for(int iterator_1 = 0;iterator_1<guid.matchPersons.Count;++iterator_1)
    {
*(long*)targetPtr = guid.matchPersons[iterator_1];
            targetPtr += 8;

    }

}else
{
    *(int*)targetPtr = 0;
    targetPtr += sizeof(int);
}

        }
            Result_Accessor ret = new Result_Accessor(tmpcellptr,null);
            ret.CellID = null;
            return ret;
        }

        public static bool operator == (Result_Accessor a, Result b)
        {
            Result_Accessor bb = b;
            return (a == bb);
        }

        public static bool operator != (Result_Accessor a, Result b)
        {
            return !(a == b);
        }

        public static bool operator == (Result_Accessor a, Result_Accessor b)
        {
            // If both are null, return true.
            if ((object)a == null && (object)b == null) return true;
            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;
            // If both are same instance, return true.
            if (a.CellPtr == b.CellPtr) return true;
            byte* targetPtr = a.CellPtr;
            targetPtr += 4 + *(int*)targetPtr;

            int lengthA = (int)(targetPtr - a.CellPtr);
            targetPtr = b.CellPtr;
            targetPtr += 4 + *(int*)targetPtr;

            int lengthB = (int)(targetPtr - b.CellPtr);
            if(lengthA != lengthB) return false;
            return Memory.Compare(a.CellPtr,b.CellPtr,lengthA);
        }

        public static bool operator != (Result_Accessor a, Result_Accessor b)
        {
            return !(a == b);
        }

    }

    public unsafe class Result_Accessor_ReadOnly
    {
        internal byte* CellPtr;
        internal long? CellID;
        internal ResizeFunctionPrototype ResizeFunction;
        internal unsafe Result_Accessor_ReadOnly(byte* _CellPtr, ResizeFunctionPrototype func)
        {
            CellPtr = _CellPtr;
            ResizeFunction = func;
            
        matchPersons_Accessor_Field = new longList(null,
                (ptr,ptr_offset,delta)=>
                {
                    int substructure_offset = (int)(ptr - this.CellPtr);
                    this.CellPtr = this.ResizeFunction(this.CellPtr, ptr_offset + substructure_offset, delta);
                    return this.CellPtr + substructure_offset;
                });

        }

        public static implicit operator Result_Accessor_ReadOnly(Result_Accessor accessor )
        {
            return new Result_Accessor_ReadOnly(accessor.CellPtr, accessor.ResizeFunction);
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

            int size = (int)(targetPtr - CellPtr);
            byte[] ret = new byte[size];
            Memory.Copy(CellPtr,0,ret,0,size);
            return ret;
        }
longList matchPersons_Accessor_Field;
        public unsafe longList matchPersons
        {
            get
            {
                
                byte* targetPtr = CellPtr;

                matchPersons_Accessor_Field.CellPtr = targetPtr + 4;
                matchPersons_Accessor_Field.CellID = this.CellID;
                return matchPersons_Accessor_Field;
            }

    }

        public static unsafe implicit operator Result(Result_Accessor_ReadOnly accessor)
        {
            
            return new Result(accessor.matchPersons);
        }

        public unsafe static implicit operator Result_Accessor_ReadOnly(Result guid)
        {  
            byte* targetPtr = null;
            
        {
if(guid.matchPersons!= null)
{
    targetPtr += guid.matchPersons.Count*8+sizeof(int);
}else
{
    targetPtr += sizeof(int);
}


        }
            byte* tmpcellptr = BufferAllocator.AllocBuffer((int)targetPtr);
            targetPtr = tmpcellptr;
        
        {
if(guid.matchPersons!= null)
{
    *(int*)targetPtr = guid.matchPersons.Count*8;
    targetPtr += sizeof(int);
    for(int iterator_1 = 0;iterator_1<guid.matchPersons.Count;++iterator_1)
    {
*(long*)targetPtr = guid.matchPersons[iterator_1];
            targetPtr += 8;

    }

}else
{
    *(int*)targetPtr = 0;
    targetPtr += sizeof(int);
}

        }
            Result_Accessor_ReadOnly ret = new Result_Accessor_ReadOnly(tmpcellptr,null);
            ret.CellID = null;
            return ret;
        }

        public static bool operator == (Result_Accessor_ReadOnly a, Result b)
        {
            Result_Accessor_ReadOnly bb = b;
            return (a == bb);
        }

        public static bool operator != (Result_Accessor_ReadOnly a, Result b)
        {
            return !(a == b);
        }

        public static bool operator == (Result_Accessor_ReadOnly a, Result_Accessor_ReadOnly b)
        {
            // If both are null, return true.
            if ((object)a == null && (object)b == null) return true;
            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;
            // If both are same instance, return true.
            if (a.CellPtr == b.CellPtr) return true;
            byte* targetPtr = a.CellPtr;
            targetPtr += 4 + *(int*)targetPtr;

            int lengthA = (int)(targetPtr - a.CellPtr);
            targetPtr = b.CellPtr;
            targetPtr += 4 + *(int*)targetPtr;

            int lengthB = (int)(targetPtr - b.CellPtr);
            if(lengthA != lengthB) return false;
            return Memory.Compare(a.CellPtr,b.CellPtr,lengthA);
        }

        public static bool operator != (Result_Accessor_ReadOnly a, Result_Accessor_ReadOnly b)
        {
            return !(a == b);
        }

    }

}

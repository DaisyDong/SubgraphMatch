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
        SSSPCell,
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct StartSSSPMessage
    {
		
        public StartSSSPMessage(long root=default(long))
		{
            this.root = root;

		}

        [FieldOffset(0)]

        public long root;

        public static implicit operator Tuple<long>(StartSSSPMessage FormatStruct)
        {
            return new Tuple<long>(FormatStruct.root);
        }

        public static implicit operator StartSSSPMessage (Tuple<long>tuple)
        {
            return new StartSSSPMessage(tuple.Item1);
        }

        public static bool operator == (StartSSSPMessage a, StartSSSPMessage b)
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
            return a.root == b.root;
        }

        public static bool operator != (StartSSSPMessage a, StartSSSPMessage b)
        {
            return !(a == b);
        }

    }

    public unsafe class StartSSSPMessage_Accessor
    {
        internal byte* CellPtr;
        internal long? CellID;
        
        internal unsafe StartSSSPMessage_Accessor(byte* _CellPtr)
        {
            CellPtr = _CellPtr;
        }

        public static implicit operator StartSSSPMessage_Accessor(StartSSSPMessage_Accessor_ReadOnly accessor )
        {
            return new StartSSSPMessage_Accessor(accessor.CellPtr);
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
            targetPtr += 8;

            int size = (int)(targetPtr - CellPtr);
            byte[] ret = new byte[size];
            Memory.Copy(CellPtr,0,ret,0,size);
            return ret;
        }
long root_Accessor_Field;
        public unsafe long root
        {
            get
            {
                
                byte* targetPtr = CellPtr;

                return *(long*)(targetPtr);
            }

            set
            {
                byte* targetPtr = CellPtr;

                *(long*)(targetPtr) = value;
            }

        }

        public static unsafe implicit operator StartSSSPMessage(StartSSSPMessage_Accessor accessor)
        {
            
            return new StartSSSPMessage(accessor.root);
        }

        public unsafe static implicit operator StartSSSPMessage_Accessor(StartSSSPMessage guid)
        {  
            byte* targetPtr = null;
            targetPtr += 8;

            byte* tmpcellptr = BufferAllocator.AllocBuffer((int)targetPtr);
            targetPtr = tmpcellptr;
        
        {*(long*)targetPtr = guid.root;
            targetPtr += 8;

        }
            StartSSSPMessage_Accessor ret = new StartSSSPMessage_Accessor(tmpcellptr);
            ret.CellID = null;
            return ret;
        }

        public static bool operator == (StartSSSPMessage_Accessor a, StartSSSPMessage b)
        {
            StartSSSPMessage_Accessor bb = b;
            return (a == bb);
        }

        public static bool operator != (StartSSSPMessage_Accessor a, StartSSSPMessage b)
        {
            return !(a == b);
        }

        public static bool operator == (StartSSSPMessage_Accessor a, StartSSSPMessage_Accessor b)
        {
            // If both are null, return true.
            if ((object)a == null && (object)b == null) return true;
            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;
            // If both are same instance, return true.
            if (a.CellPtr == b.CellPtr) return true;
            byte* targetPtr = a.CellPtr;
            targetPtr += 8;

            int lengthA = (int)(targetPtr - a.CellPtr);
            targetPtr = b.CellPtr;
            targetPtr += 8;

            int lengthB = (int)(targetPtr - b.CellPtr);
            if(lengthA != lengthB) return false;
            return Memory.Compare(a.CellPtr,b.CellPtr,lengthA);
        }

        public static bool operator != (StartSSSPMessage_Accessor a, StartSSSPMessage_Accessor b)
        {
            return !(a == b);
        }

    }

    public unsafe class StartSSSPMessage_Accessor_ReadOnly
    {
        internal byte* CellPtr;
        internal long? CellID;
        
        internal unsafe StartSSSPMessage_Accessor_ReadOnly(byte* _CellPtr)
        {
            CellPtr = _CellPtr;
        }

        public static implicit operator StartSSSPMessage_Accessor_ReadOnly(StartSSSPMessage_Accessor accessor )
        {
            return new StartSSSPMessage_Accessor_ReadOnly(accessor.CellPtr);
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
            targetPtr += 8;

            int size = (int)(targetPtr - CellPtr);
            byte[] ret = new byte[size];
            Memory.Copy(CellPtr,0,ret,0,size);
            return ret;
        }
long root_Accessor_Field;
        public unsafe long root
        {
            get
            {
                
                byte* targetPtr = CellPtr;

                return *(long*)(targetPtr);
            }

        }

        public static unsafe implicit operator StartSSSPMessage(StartSSSPMessage_Accessor_ReadOnly accessor)
        {
            
            return new StartSSSPMessage(accessor.root);
        }

        public unsafe static implicit operator StartSSSPMessage_Accessor_ReadOnly(StartSSSPMessage guid)
        {  
            byte* targetPtr = null;
            targetPtr += 8;

            byte* tmpcellptr = BufferAllocator.AllocBuffer((int)targetPtr);
            targetPtr = tmpcellptr;
        
        {*(long*)targetPtr = guid.root;
            targetPtr += 8;

        }
            StartSSSPMessage_Accessor_ReadOnly ret = new StartSSSPMessage_Accessor_ReadOnly(tmpcellptr);
            ret.CellID = null;
            return ret;
        }

        public static bool operator == (StartSSSPMessage_Accessor_ReadOnly a, StartSSSPMessage b)
        {
            StartSSSPMessage_Accessor_ReadOnly bb = b;
            return (a == bb);
        }

        public static bool operator != (StartSSSPMessage_Accessor_ReadOnly a, StartSSSPMessage b)
        {
            return !(a == b);
        }

        public static bool operator == (StartSSSPMessage_Accessor_ReadOnly a, StartSSSPMessage_Accessor_ReadOnly b)
        {
            // If both are null, return true.
            if ((object)a == null && (object)b == null) return true;
            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;
            // If both are same instance, return true.
            if (a.CellPtr == b.CellPtr) return true;
            byte* targetPtr = a.CellPtr;
            targetPtr += 8;

            int lengthA = (int)(targetPtr - a.CellPtr);
            targetPtr = b.CellPtr;
            targetPtr += 8;

            int lengthB = (int)(targetPtr - b.CellPtr);
            if(lengthA != lengthB) return false;
            return Memory.Compare(a.CellPtr,b.CellPtr,lengthA);
        }

        public static bool operator != (StartSSSPMessage_Accessor_ReadOnly a, StartSSSPMessage_Accessor_ReadOnly b)
        {
            return !(a == b);
        }

    }

    public struct DistanceUpdatingMessage
    {
		
        public DistanceUpdatingMessage(long senderId=default(long),int distance=default(int),List<long> recipients=null)
		{
            this.senderId = senderId;
            this.distance = distance;
            this.recipients = recipients;

		}

        public long senderId;

        public int distance;

        public List<long> recipients;

        public static implicit operator Tuple<long,int,List<long>>(DistanceUpdatingMessage FormatStruct)
        {
            return new Tuple<long,int,List<long>>(FormatStruct.senderId,FormatStruct.distance,FormatStruct.recipients);
        }

        public static implicit operator DistanceUpdatingMessage (Tuple<long,int,List<long>>tuple)
        {
            return new DistanceUpdatingMessage(tuple.Item1,tuple.Item2,tuple.Item3);
        }

        public static bool operator == (DistanceUpdatingMessage a, DistanceUpdatingMessage b)
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
            return a.senderId == b.senderId && a.distance == b.distance && a.recipients == b.recipients;
        }

        public static bool operator != (DistanceUpdatingMessage a, DistanceUpdatingMessage b)
        {
            return !(a == b);
        }

    }

    public unsafe class DistanceUpdatingMessage_Accessor
    {
        internal byte* CellPtr;
        internal long? CellID;
        internal ResizeFunctionPrototype ResizeFunction;
        internal unsafe DistanceUpdatingMessage_Accessor(byte* _CellPtr, ResizeFunctionPrototype func)
        {
            CellPtr = _CellPtr;
            ResizeFunction = func;
            
        recipients_Accessor_Field = new longList(null,
                (ptr,ptr_offset,delta)=>
                {
                    int substructure_offset = (int)(ptr - this.CellPtr);
                    this.CellPtr = this.ResizeFunction(this.CellPtr, ptr_offset + substructure_offset, delta);
                    return this.CellPtr + substructure_offset;
                });

        }

        public static implicit operator DistanceUpdatingMessage_Accessor(DistanceUpdatingMessage_Accessor_ReadOnly accessor )
        {
            return new DistanceUpdatingMessage_Accessor(accessor.CellPtr, accessor.ResizeFunction);
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
long senderId_Accessor_Field;
        public unsafe long senderId
        {
            get
            {
                
                byte* targetPtr = CellPtr;

                return *(long*)(targetPtr);
            }

            set
            {
                byte* targetPtr = CellPtr;

                *(long*)(targetPtr) = value;
            }

        }
int distance_Accessor_Field;
        public unsafe int distance
        {
            get
            {
                
                byte* targetPtr = CellPtr;
            targetPtr += 8;

                return *(int*)(targetPtr);
            }

            set
            {
                byte* targetPtr = CellPtr;
            targetPtr += 8;

                *(int*)(targetPtr) = value;
            }

        }
longList recipients_Accessor_Field;
        public unsafe longList recipients
        {
            get
            {
                
                byte* targetPtr = CellPtr;
            targetPtr += 12;

                recipients_Accessor_Field.CellPtr = targetPtr + 4;
                recipients_Accessor_Field.CellID = this.CellID;
                return recipients_Accessor_Field;
            }

            set
            {
                if ((object)value == null) throw new ArgumentNullException("The assigned variable is null.");
                byte* targetPtr = CellPtr;
            targetPtr += 12;

                recipients_Accessor_Field.CellID = this.CellID;
                
              int length = *(int*)(value.CellPtr - 4);

                //senario: cell_a.inlinks = cell_b.inlinks,
                //the later part will invoke the Get, filling cell_b.inlinks(a inlink_accessor_fiedld)'storage CellID
                int oldlength = *(int*)targetPtr;
                if (value.CellID != recipients_Accessor_Field.CellID)
                {
                    //if not in the same Cell
                    recipients_Accessor_Field.CellPtr = recipients_Accessor_Field.ResizeFunction(targetPtr, 0, length - oldlength);
                    Memory.Copy(value.CellPtr - 4, recipients_Accessor_Field.CellPtr, length + 4);
                }
                else
                {
                    byte[] tmpcell = new byte[length + 4];
                    fixed (byte* tmpcellptr = tmpcell)
                    {                        
                        Memory.Copy(value.CellPtr - 4, tmpcellptr, length + 4);
                        recipients_Accessor_Field.CellPtr = recipients_Accessor_Field.ResizeFunction(targetPtr, 0, length - oldlength);
                        Memory.Copy(tmpcellptr, recipients_Accessor_Field.CellPtr, length + 4);
                    }
                }
                recipients_Accessor_Field.CellPtr += 4;
                
            }

    }

        public static unsafe implicit operator DistanceUpdatingMessage(DistanceUpdatingMessage_Accessor accessor)
        {
            
            return new DistanceUpdatingMessage(accessor.senderId,accessor.distance,accessor.recipients);
        }

        public unsafe static implicit operator DistanceUpdatingMessage_Accessor(DistanceUpdatingMessage guid)
        {  
            byte* targetPtr = null;
            
        {targetPtr += 8;
targetPtr += 4;

if(guid.recipients!= null)
{
    targetPtr += guid.recipients.Count*8+sizeof(int);
}else
{
    targetPtr += sizeof(int);
}


        }
            byte* tmpcellptr = BufferAllocator.AllocBuffer((int)targetPtr);
            targetPtr = tmpcellptr;
        
        {*(long*)targetPtr = guid.senderId;
            targetPtr += 8;
*(int*)targetPtr = guid.distance;
            targetPtr += 4;

if(guid.recipients!= null)
{
    *(int*)targetPtr = guid.recipients.Count*8;
    targetPtr += sizeof(int);
    for(int iterator_1 = 0;iterator_1<guid.recipients.Count;++iterator_1)
    {
*(long*)targetPtr = guid.recipients[iterator_1];
            targetPtr += 8;

    }

}else
{
    *(int*)targetPtr = 0;
    targetPtr += sizeof(int);
}

        }
            DistanceUpdatingMessage_Accessor ret = new DistanceUpdatingMessage_Accessor(tmpcellptr,null);
            ret.CellID = null;
            return ret;
        }

        public static bool operator == (DistanceUpdatingMessage_Accessor a, DistanceUpdatingMessage b)
        {
            DistanceUpdatingMessage_Accessor bb = b;
            return (a == bb);
        }

        public static bool operator != (DistanceUpdatingMessage_Accessor a, DistanceUpdatingMessage b)
        {
            return !(a == b);
        }

        public static bool operator == (DistanceUpdatingMessage_Accessor a, DistanceUpdatingMessage_Accessor b)
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

        public static bool operator != (DistanceUpdatingMessage_Accessor a, DistanceUpdatingMessage_Accessor b)
        {
            return !(a == b);
        }

    }

    public unsafe class DistanceUpdatingMessage_Accessor_ReadOnly
    {
        internal byte* CellPtr;
        internal long? CellID;
        internal ResizeFunctionPrototype ResizeFunction;
        internal unsafe DistanceUpdatingMessage_Accessor_ReadOnly(byte* _CellPtr, ResizeFunctionPrototype func)
        {
            CellPtr = _CellPtr;
            ResizeFunction = func;
            
        recipients_Accessor_Field = new longList(null,
                (ptr,ptr_offset,delta)=>
                {
                    int substructure_offset = (int)(ptr - this.CellPtr);
                    this.CellPtr = this.ResizeFunction(this.CellPtr, ptr_offset + substructure_offset, delta);
                    return this.CellPtr + substructure_offset;
                });

        }

        public static implicit operator DistanceUpdatingMessage_Accessor_ReadOnly(DistanceUpdatingMessage_Accessor accessor )
        {
            return new DistanceUpdatingMessage_Accessor_ReadOnly(accessor.CellPtr, accessor.ResizeFunction);
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
long senderId_Accessor_Field;
        public unsafe long senderId
        {
            get
            {
                
                byte* targetPtr = CellPtr;

                return *(long*)(targetPtr);
            }

        }
int distance_Accessor_Field;
        public unsafe int distance
        {
            get
            {
                
                byte* targetPtr = CellPtr;
            targetPtr += 8;

                return *(int*)(targetPtr);
            }

        }
longList recipients_Accessor_Field;
        public unsafe longList recipients
        {
            get
            {
                
                byte* targetPtr = CellPtr;
            targetPtr += 12;

                recipients_Accessor_Field.CellPtr = targetPtr + 4;
                recipients_Accessor_Field.CellID = this.CellID;
                return recipients_Accessor_Field;
            }

    }

        public static unsafe implicit operator DistanceUpdatingMessage(DistanceUpdatingMessage_Accessor_ReadOnly accessor)
        {
            
            return new DistanceUpdatingMessage(accessor.senderId,accessor.distance,accessor.recipients);
        }

        public unsafe static implicit operator DistanceUpdatingMessage_Accessor_ReadOnly(DistanceUpdatingMessage guid)
        {  
            byte* targetPtr = null;
            
        {targetPtr += 8;
targetPtr += 4;

if(guid.recipients!= null)
{
    targetPtr += guid.recipients.Count*8+sizeof(int);
}else
{
    targetPtr += sizeof(int);
}


        }
            byte* tmpcellptr = BufferAllocator.AllocBuffer((int)targetPtr);
            targetPtr = tmpcellptr;
        
        {*(long*)targetPtr = guid.senderId;
            targetPtr += 8;
*(int*)targetPtr = guid.distance;
            targetPtr += 4;

if(guid.recipients!= null)
{
    *(int*)targetPtr = guid.recipients.Count*8;
    targetPtr += sizeof(int);
    for(int iterator_1 = 0;iterator_1<guid.recipients.Count;++iterator_1)
    {
*(long*)targetPtr = guid.recipients[iterator_1];
            targetPtr += 8;

    }

}else
{
    *(int*)targetPtr = 0;
    targetPtr += sizeof(int);
}

        }
            DistanceUpdatingMessage_Accessor_ReadOnly ret = new DistanceUpdatingMessage_Accessor_ReadOnly(tmpcellptr,null);
            ret.CellID = null;
            return ret;
        }

        public static bool operator == (DistanceUpdatingMessage_Accessor_ReadOnly a, DistanceUpdatingMessage b)
        {
            DistanceUpdatingMessage_Accessor_ReadOnly bb = b;
            return (a == bb);
        }

        public static bool operator != (DistanceUpdatingMessage_Accessor_ReadOnly a, DistanceUpdatingMessage b)
        {
            return !(a == b);
        }

        public static bool operator == (DistanceUpdatingMessage_Accessor_ReadOnly a, DistanceUpdatingMessage_Accessor_ReadOnly b)
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

        public static bool operator != (DistanceUpdatingMessage_Accessor_ReadOnly a, DistanceUpdatingMessage_Accessor_ReadOnly b)
        {
            return !(a == b);
        }

    }

}

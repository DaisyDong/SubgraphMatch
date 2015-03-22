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

    public abstract class SSSPSlaveBase : TrinitySlave
    {
        internal override void RegisterMessageHandler()
        {
        	MessageRegistry.RegisterMessageHandler((byte)Trinity.TSL.TrinitySlave.SSSPSlave.AsynReqMessageType.StartSSSP, _StartSSSPHandler);
	MessageRegistry.RegisterMessageHandler((byte)Trinity.TSL.TrinitySlave.SSSPSlave.AsynReqMessageType.DistanceUpdating, _DistanceUpdatingHandler);

        }
        private unsafe void _StartSSSPHandler(AsynReqArgs args)
        {
             fixed (byte* p = &args.Buffer[args.Offset])
             {
            StartSSSPHandler( new StartSSSPMessageReader(p));
             }
        }
public abstract void StartSSSPHandler(StartSSSPMessageReader request);

        private unsafe void _DistanceUpdatingHandler(AsynReqArgs args)
        {
             fixed (byte* p = &args.Buffer[args.Offset])
             {
            DistanceUpdatingHandler( new DistanceUpdatingMessageReader(p));
             }
        }
public abstract void DistanceUpdatingHandler(DistanceUpdatingMessageReader request);

    }
    namespace TSL.TrinitySlave.SSSPSlave
    {
        
        public enum SynReqMessageType : byte
        {
        
    
        }
        
        public enum SynReqRspMessageType : byte
        {
        
    
        }
        
        public enum AsynReqMessageType : byte
        {
        
            StartSSSP,
            DistanceUpdating,
    
        }
    }

    public static class MessagePassingExtension
    {
            
        public unsafe static void StartSSSPToSSSPSlave(this Trinity.Storage.MemoryCloud storage, int slaveId, StartSSSPMessageWriter msg) 
        {
            byte* bufferPtr = (byte*)msg.handle.AddrOfPinnedObject().ToPointer();
            *(int*)(bufferPtr) = msg.Length + TrinityProtocol.ProtocolHeaderLength;
            *(bufferPtr + TrinityProtocol.MsgTypeOffset) = (byte) TrinityMessageType.ASYNC;
            *(bufferPtr + TrinityProtocol.MsgIdOffset) = (byte)Trinity.TSL.TrinitySlave.SSSPSlave.AsynReqMessageType.StartSSSP;
            Global.CloudStorage.SendMessage(
                slaveId, 
                msg.buffer,
                0,
                msg.Length + TrinityProtocol.AsyncRequestWithoutResponseHeaderLength);
        }

        public unsafe static void DistanceUpdatingToSSSPSlave(this Trinity.Storage.MemoryCloud storage, int slaveId, DistanceUpdatingMessageWriter msg) 
        {
            byte* bufferPtr = (byte*)msg.handle.AddrOfPinnedObject().ToPointer();
            *(int*)(bufferPtr) = msg.Length + TrinityProtocol.ProtocolHeaderLength;
            *(bufferPtr + TrinityProtocol.MsgTypeOffset) = (byte) TrinityMessageType.ASYNC;
            *(bufferPtr + TrinityProtocol.MsgIdOffset) = (byte)Trinity.TSL.TrinitySlave.SSSPSlave.AsynReqMessageType.DistanceUpdating;
            Global.CloudStorage.SendMessage(
                slaveId, 
                msg.buffer,
                0,
                msg.Length + TrinityProtocol.AsyncRequestWithoutResponseHeaderLength);
        }

    }

    public unsafe class StartSSSPMessageReader : StartSSSPMessage_Accessor_ReadOnly
    {
        internal GCHandle handle;
        internal byte[] buffer = null;
        internal unsafe StartSSSPMessageReader(byte[] buffer, int offset)
            : base(null)
        {
            handle = GCHandle.Alloc(buffer,GCHandleType.Pinned);
            this.buffer = buffer;
            this.CellPtr = ((byte*)handle.AddrOfPinnedObject().ToPointer())+offset;
        }
        
        internal unsafe StartSSSPMessageReader(byte* _CellPtr)
            : base(_CellPtr)
        {
        }
        
        ~StartSSSPMessageReader()
        {
            if(buffer != null)
            {
                handle.Free();
                buffer = null;
            }
        }
    }


    public unsafe class StartSSSPMessageWriter: StartSSSPMessage_Accessor
    {
        //For a writer, the buffer pointer is not passed in, instead, the writer itself will allocate a buffer
        //Also, we should add a finalizer for the writer so that we can free that buffer when the writer is GCed.
        

        internal GCHandle handle;
        internal byte[] buffer = null;
        internal int Length;

		public unsafe StartSSSPMessageWriter(long root=default(long))
: base(null)
        {

int preservedHeaderLength = 6;
        byte* targetPtr = (byte*) preservedHeaderLength;targetPtr += 8;

        byte[] tmpcell = new byte[(int)(targetPtr)];
        fixed(byte* tmpcellptr = tmpcell)
        {
            targetPtr = tmpcellptr;

            targetPtr += preservedHeaderLength;
*(long*)targetPtr = root;
            targetPtr += 8;

        }

        //return new StartSSSPMessageWriter(tmpcell, preservedHeaderLength);
        buffer = tmpcell;
        handle = GCHandle.Alloc(buffer,GCHandleType.Pinned);
        this.CellPtr = ((byte*)handle.AddrOfPinnedObject().ToPointer()) + preservedHeaderLength;
        Length = tmpcell.Length - preservedHeaderLength;
    }
        private unsafe StartSSSPMessageWriter(byte[] targetBuffer,int cellPtrOffset)
            : base(null)
        {
            buffer = targetBuffer;
            handle = GCHandle.Alloc(buffer,GCHandleType.Pinned);
            this.CellPtr = ((byte*)handle.AddrOfPinnedObject().ToPointer()) + cellPtrOffset;
            Length = targetBuffer.Length - cellPtrOffset;
        }
        ~StartSSSPMessageWriter()
        {
            handle.Free();
            buffer = null;
        }
    }


    public unsafe class DistanceUpdatingMessageReader : DistanceUpdatingMessage_Accessor_ReadOnly
    {
        internal GCHandle handle;
        internal byte[] buffer = null;
        internal unsafe DistanceUpdatingMessageReader(byte[] buffer, int offset)
            : base(null,null)
        {
            handle = GCHandle.Alloc(buffer,GCHandleType.Pinned);
            this.buffer = buffer;
            this.CellPtr = ((byte*)handle.AddrOfPinnedObject().ToPointer())+offset;
        }
        
        internal unsafe DistanceUpdatingMessageReader(byte* _CellPtr)
            : base(_CellPtr,null)
        {
        }
        
        ~DistanceUpdatingMessageReader()
        {
            if(buffer != null)
            {
                handle.Free();
                buffer = null;
            }
        }
    }


    public unsafe class DistanceUpdatingMessageWriter: DistanceUpdatingMessage_Accessor
    {
        //For a writer, the buffer pointer is not passed in, instead, the writer itself will allocate a buffer
        //Also, we should add a finalizer for the writer so that we can free that buffer when the writer is GCed.
        

        internal GCHandle handle;
        internal byte[] buffer = null;
        internal int Length;

		public unsafe DistanceUpdatingMessageWriter(long senderId=default(long),int distance=default(int),List<long> recipients=null)
: base(null,null)
        {

int preservedHeaderLength = 6;
        byte* targetPtr = (byte*) preservedHeaderLength;targetPtr += 8;
targetPtr += 4;

if(recipients!= null)
{
    targetPtr += recipients.Count*8+sizeof(int);
}else
{
    targetPtr += sizeof(int);
}


        byte[] tmpcell = new byte[(int)(targetPtr)];
        fixed(byte* tmpcellptr = tmpcell)
        {
            targetPtr = tmpcellptr;

            targetPtr += preservedHeaderLength;
*(long*)targetPtr = senderId;
            targetPtr += 8;
*(int*)targetPtr = distance;
            targetPtr += 4;

if(recipients!= null)
{
    *(int*)targetPtr = recipients.Count*8;
    targetPtr += sizeof(int);
    for(int iterator_0 = 0;iterator_0<recipients.Count;++iterator_0)
    {
*(long*)targetPtr = recipients[iterator_0];
            targetPtr += 8;

    }

}else
{
    *(int*)targetPtr = 0;
    targetPtr += sizeof(int);
}

        }

        //return new DistanceUpdatingMessageWriter(tmpcell, preservedHeaderLength);
        buffer = tmpcell;
        handle = GCHandle.Alloc(buffer,GCHandleType.Pinned);
        this.CellPtr = ((byte*)handle.AddrOfPinnedObject().ToPointer()) + preservedHeaderLength;
        Length = tmpcell.Length - preservedHeaderLength;
            this.ResizeFunction = (ptr, ptr_offset, delta)=>
            {
                if(delta >= 0)
                {
                    byte* currentBufferPtr = (byte*)handle.AddrOfPinnedObject().ToPointer();
                    int required_length = (int)(this.Length + delta + (this.CellPtr - currentBufferPtr));
                    if(required_length <= this.buffer.Length)
                    {
                        Memory.memmove(
                            ptr + ptr_offset + delta,
                            ptr + ptr_offset,
                            (ulong)(Length - (ptr + ptr_offset - this.CellPtr)));
                        Length += delta;
                        return ptr;
                    }else
                    {
                        int target_length = this.buffer.Length * 2;
                        while(target_length < required_length)
                            target_length *= 2;
                        byte[] tmpBuffer = new byte[target_length];
                        GCHandle newHandle = GCHandle.Alloc(tmpBuffer, GCHandleType.Pinned);
                        byte* tmpPtr = (byte*)newHandle.AddrOfPinnedObject().ToPointer();
                        Memory.memcpy(
                            tmpPtr,
                            currentBufferPtr,
                            (ulong)(ptr + ptr_offset - currentBufferPtr));
                        byte* newCellPtr = tmpPtr + (this.CellPtr - currentBufferPtr);
                        Memory.memcpy(
                            //newCellPtr + (ptr + ptr_offset + delta - currentBufferPtr), // bug fix remove
                            newCellPtr + (ptr_offset + delta),
                            ptr + ptr_offset,
                            (ulong)(Length - (ptr + ptr_offset - this.CellPtr)));
                        Length += delta;
                        this.CellPtr = newCellPtr;
                        this.buffer = tmpBuffer;
                        handle.Free();
                        handle = newHandle;
                        return tmpPtr + (ptr - currentBufferPtr);
                    }
                }else
                {
                    Memory.memmove(
                        ptr + ptr_offset,
                        ptr + ptr_offset - delta,
                        (ulong)(Length - (ptr + ptr_offset - delta - this.CellPtr)));
                    Length += delta;
                    return ptr;
                }
            };
    }
        private unsafe DistanceUpdatingMessageWriter(byte[] targetBuffer,int cellPtrOffset)
            : base(null,null)
        {
            buffer = targetBuffer;
            handle = GCHandle.Alloc(buffer,GCHandleType.Pinned);
            this.CellPtr = ((byte*)handle.AddrOfPinnedObject().ToPointer()) + cellPtrOffset;
            Length = targetBuffer.Length - cellPtrOffset;
            this.ResizeFunction = (ptr, ptr_offset, delta)=>
            {
                if(delta >= 0)
                {
                    byte* currentBufferPtr = (byte*)handle.AddrOfPinnedObject().ToPointer();
                    int required_length = (int)(this.Length + delta + (this.CellPtr - currentBufferPtr));
                    if(required_length <= this.buffer.Length)
                    {
                        Memory.memmove(
                            ptr + ptr_offset + delta,
                            ptr + ptr_offset,
                            (ulong)(Length - (ptr + ptr_offset - this.CellPtr)));
                        Length += delta;
                        return ptr;
                    }else
                    {
                        int target_length = this.buffer.Length * 2;
                        while(target_length < required_length)
                            target_length *= 2;
                        byte[] tmpBuffer = new byte[target_length];
                        GCHandle newHandle = GCHandle.Alloc(tmpBuffer, GCHandleType.Pinned);
                        byte* tmpPtr = (byte*)newHandle.AddrOfPinnedObject().ToPointer();
                        Memory.memcpy(
                            tmpPtr,
                            currentBufferPtr,
                            (ulong)(ptr + ptr_offset - currentBufferPtr));
                        byte* newCellPtr = tmpPtr + (this.CellPtr - currentBufferPtr);
                        Memory.memcpy(
                            //newCellPtr + (ptr + ptr_offset + delta - currentBufferPtr), // bug fix remove
                            newCellPtr + (ptr_offset + delta),
                            ptr + ptr_offset,
                            (ulong)(Length - (ptr + ptr_offset - this.CellPtr)));
                        Length += delta;
                        this.CellPtr = newCellPtr;
                        this.buffer = tmpBuffer;
                        handle.Free();
                        handle = newHandle;
                        return tmpPtr + (ptr - currentBufferPtr);
                    }
                }else
                {
                    Memory.memmove(
                        ptr + ptr_offset,
                        ptr + ptr_offset - delta,
                        (ulong)(Length - (ptr + ptr_offset - delta - this.CellPtr)));
                    Length += delta;
                    return ptr;
                }
            };
        }
        ~DistanceUpdatingMessageWriter()
        {
            handle.Free();
            buffer = null;
        }
    }


}

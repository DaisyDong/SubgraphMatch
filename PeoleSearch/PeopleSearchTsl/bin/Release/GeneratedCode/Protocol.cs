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

    public abstract class MySlaveBase : TrinitySlave
    {
        internal override void RegisterMessageHandler()
        {
        	MessageRegistry.RegisterMessageHandler((byte)Trinity.TSL.TrinitySlave.MySlave.AsynReqMessageType.Search, _SearchHandler);

        }
        private unsafe void _SearchHandler(AsynReqArgs args)
        {
             fixed (byte* p = &args.Buffer[args.Offset])
             {
            SearchHandler( new NameRequestReader(p));
             }
        }
public abstract void SearchHandler(NameRequestReader request);

    }
    namespace TSL.TrinitySlave.MySlave
    {
        
        public enum SynReqMessageType : byte
        {
        
    
        }
        
        public enum SynReqRspMessageType : byte
        {
        
    
        }
        
        public enum AsynReqMessageType : byte
        {
        
            Search,
    
        }
    }

    public abstract class MyProxyBase : TrinityProxy
    {
        internal override void RegisterMessageHandler()
        {
        			MessageRegistry.RegisterMessageHandler((byte)Trinity.TSL.TrinityProxy.MyProxy.SynReqRspMessageType.Query, _QueryHandler);
			MessageRegistry.RegisterMessageHandler((byte)Trinity.TSL.TrinityProxy.MyProxy.AsynReqMessageType.Report, _ReportHandler);

        }
        private unsafe void _QueryHandler(SynReqRspArgs args)
        {
             fixed (byte* p = &args.Buffer[args.Offset])
             {
                var responseBuff = new ResultWriter();
            QueryHandler( new NameRequestReader(p), responseBuff);
                *(int*)(responseBuff.CellPtr - TrinityProtocol.MsgHeader) = responseBuff.Length + TrinityProtocol.TrinityMsgHeader;
                args.Response = new TrinityMessage( responseBuff.buffer, responseBuff.Length + TrinityProtocol.MsgHeader);
             }
        }
public abstract void QueryHandler(NameRequestReader request, ResultWriter response);

        private unsafe void _ReportHandler(AsynReqArgs args)
        {
             fixed (byte* p = &args.Buffer[args.Offset])
             {
            ReportHandler( new ResultReader(p));
             }
        }
public abstract void ReportHandler(ResultReader request);

    }
    namespace TSL.TrinityProxy.MyProxy
    {
        
        public enum SynReqMessageType : byte
        {
        
    
        }
        
        public enum SynReqRspMessageType : byte
        {
        
            Query,
    
        }
        
        public enum AsynReqMessageType : byte
        {
        
            Report,
    
        }
    }

    public static class MessagePassingExtension
    {
            
        public unsafe static void SearchToMySlave(this Trinity.Storage.MemoryCloud storage, int slaveId, NameRequestWriter msg) 
        {
            byte* bufferPtr = (byte*)msg.handle.AddrOfPinnedObject().ToPointer();
            *(int*)(bufferPtr) = msg.Length + TrinityProtocol.ProtocolHeaderLength;
            *(bufferPtr + TrinityProtocol.MsgTypeOffset) = (byte) TrinityMessageType.ASYNC;
            *(bufferPtr + TrinityProtocol.MsgIdOffset) = (byte)Trinity.TSL.TrinitySlave.MySlave.AsynReqMessageType.Search;
            Global.CloudStorage.SendMessage(
                slaveId, 
                msg.buffer,
                0,
                msg.Length + TrinityProtocol.AsyncRequestWithoutResponseHeaderLength);
        }

        public unsafe static void ReportToMyProxy(this Trinity.Storage.MemoryCloud storage, int proxyId, ResultWriter msg) 
        {
            byte* bufferPtr = (byte*)msg.handle.AddrOfPinnedObject().ToPointer();
            *(int*)(bufferPtr) = msg.Length + TrinityProtocol.ProtocolHeaderLength;
            *(bufferPtr + TrinityProtocol.MsgTypeOffset) = (byte) TrinityMessageType.ASYNC;
            *(bufferPtr + TrinityProtocol.MsgIdOffset) = (byte)Trinity.TSL.TrinityProxy.MyProxy.AsynReqMessageType.Report;
            Global.GetProxy(proxyId).SendMessage(
                msg.buffer,
                0,
                msg.Length + TrinityProtocol.AsyncRequestWithoutResponseHeaderLength);
        }
 
        public unsafe static ResultReader QueryToMyProxy(this Trinity.Storage.MemoryCloud storage, int proxyId, NameRequestWriter msg)
        {
            byte* bufferPtr = (byte*)msg.handle.AddrOfPinnedObject().ToPointer();
            *(int*)(bufferPtr) = msg.Length + TrinityProtocol.ProtocolHeaderLength;
            *(bufferPtr + TrinityProtocol.MsgTypeOffset) = (byte) TrinityMessageType.SYNC_WITH_RSP;
            *(bufferPtr + TrinityProtocol.MsgIdOffset) = (byte)Trinity.TSL.TrinityProxy.MyProxy.SynReqRspMessageType.Query;
            TrinityMessage response;
            Global.GetProxy(proxyId).SendMessage(
                msg.buffer, 
                0, 
                msg.Length + TrinityProtocol.MsgHeader, out response);
            return new ResultReader(response.Buffer, TrinityMessage.Offset); 
        }

    }

    public unsafe class NameRequestReader : NameRequest_Accessor_ReadOnly
    {
        internal GCHandle handle;
        internal byte[] buffer = null;
        internal unsafe NameRequestReader(byte[] buffer, int offset)
            : base(null,null)
        {
            handle = GCHandle.Alloc(buffer,GCHandleType.Pinned);
            this.buffer = buffer;
            this.CellPtr = ((byte*)handle.AddrOfPinnedObject().ToPointer())+offset;
        }
        
        internal unsafe NameRequestReader(byte* _CellPtr)
            : base(_CellPtr,null)
        {
        }
        
        ~NameRequestReader()
        {
            if(buffer != null)
            {
                handle.Free();
                buffer = null;
            }
        }
    }


    public unsafe class NameRequestWriter: NameRequest_Accessor
    {
        //For a writer, the buffer pointer is not passed in, instead, the writer itself will allocate a buffer
        //Also, we should add a finalizer for the writer so that we can free that buffer when the writer is GCed.
        

        internal GCHandle handle;
        internal byte[] buffer = null;
        internal int Length;

		public unsafe NameRequestWriter(int hop=default(int),string name=null,List<long> neighbours=null)
: base(null,null)
        {

int preservedHeaderLength = 6;
        byte* targetPtr = (byte*) preservedHeaderLength;targetPtr += 4;

        if(name!= null)
        {
            targetPtr += name.Length*2+sizeof(int);
        }else
        {
            targetPtr += sizeof(int);
        }

if(neighbours!= null)
{
    targetPtr += neighbours.Count*8+sizeof(int);
}else
{
    targetPtr += sizeof(int);
}


        byte[] tmpcell = new byte[(int)(targetPtr)];
        fixed(byte* tmpcellptr = tmpcell)
        {
            targetPtr = tmpcellptr;

            targetPtr += preservedHeaderLength;
*(int*)targetPtr = hop;
            targetPtr += 4;

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

if(neighbours!= null)
{
    *(int*)targetPtr = neighbours.Count*8;
    targetPtr += sizeof(int);
    for(int iterator_0 = 0;iterator_0<neighbours.Count;++iterator_0)
    {
*(long*)targetPtr = neighbours[iterator_0];
            targetPtr += 8;

    }

}else
{
    *(int*)targetPtr = 0;
    targetPtr += sizeof(int);
}

        }

        //return new NameRequestWriter(tmpcell, preservedHeaderLength);
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
        private unsafe NameRequestWriter(byte[] targetBuffer,int cellPtrOffset)
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
        ~NameRequestWriter()
        {
            handle.Free();
            buffer = null;
        }
    }


    public unsafe class ResultReader : Result_Accessor_ReadOnly
    {
        internal GCHandle handle;
        internal byte[] buffer = null;
        internal unsafe ResultReader(byte[] buffer, int offset)
            : base(null,null)
        {
            handle = GCHandle.Alloc(buffer,GCHandleType.Pinned);
            this.buffer = buffer;
            this.CellPtr = ((byte*)handle.AddrOfPinnedObject().ToPointer())+offset;
        }
        
        internal unsafe ResultReader(byte* _CellPtr)
            : base(_CellPtr,null)
        {
        }
        
        ~ResultReader()
        {
            if(buffer != null)
            {
                handle.Free();
                buffer = null;
            }
        }
    }


    public unsafe class ResultWriter: Result_Accessor
    {
        //For a writer, the buffer pointer is not passed in, instead, the writer itself will allocate a buffer
        //Also, we should add a finalizer for the writer so that we can free that buffer when the writer is GCed.
        

        internal GCHandle handle;
        internal byte[] buffer = null;
        internal int Length;

		public unsafe ResultWriter(List<long> matchPersons=null)
: base(null,null)
        {

int preservedHeaderLength = 6;
        byte* targetPtr = (byte*) preservedHeaderLength;
if(matchPersons!= null)
{
    targetPtr += matchPersons.Count*8+sizeof(int);
}else
{
    targetPtr += sizeof(int);
}


        byte[] tmpcell = new byte[(int)(targetPtr)];
        fixed(byte* tmpcellptr = tmpcell)
        {
            targetPtr = tmpcellptr;

            targetPtr += preservedHeaderLength;

if(matchPersons!= null)
{
    *(int*)targetPtr = matchPersons.Count*8;
    targetPtr += sizeof(int);
    for(int iterator_0 = 0;iterator_0<matchPersons.Count;++iterator_0)
    {
*(long*)targetPtr = matchPersons[iterator_0];
            targetPtr += 8;

    }

}else
{
    *(int*)targetPtr = 0;
    targetPtr += sizeof(int);
}

        }

        //return new ResultWriter(tmpcell, preservedHeaderLength);
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
        private unsafe ResultWriter(byte[] targetBuffer,int cellPtrOffset)
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
        ~ResultWriter()
        {
            handle.Free();
            buffer = null;
        }
    }


}

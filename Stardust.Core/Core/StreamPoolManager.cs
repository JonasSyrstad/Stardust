//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.IO;

//namespace Stardust.Core
//{
//    public static class StreamPoolManager
//    {
//        private const int BlockSize = 1024;

//        private const int LargeBufferMultiple = 1024 * 1024;

//        private const int MaxBufferSize = 16 * LargeBufferMultiple;

//        private const string StardustTagName = "stardust.memoryStream";

//        private static RecyclableMemoryStreamManager manager = new RecyclableMemoryStreamManager(BlockSize, 
//                                                LargeBufferMultiple, 
//                                                MaxBufferSize);

//        public static MemoryStream GetStream()
//        {
//           return  manager.GetStream();
//        }

//        public static MemoryStream GetStream(string debugTag)
//        {
//            return manager.GetStream(debugTag);
//        }

//        public static MemoryStream GetStream(string debugTag, int capacity)
//        {
//            return manager.GetStream(debugTag,  capacity);
//        }

//        public static MemoryStream GetStream(string debugTag, int capacity, bool asContinuousBuffer)
//        {
//            return manager.GetStream(debugTag, capacity,asContinuousBuffer);
//        }

//        public static MemoryStream GetStream(int capacity)
//        {
//            return manager.GetStream(StardustTagName, capacity);
//        }

//        public static MemoryStream GetStream(int capacity, bool asContinuousBuffer)
//        {
//            return manager.GetStream(StardustTagName, capacity, asContinuousBuffer);
//        }


//        public static MemoryStream GetStream(byte[] buffer, int offset, int count)
//        {
//            return manager.GetStream(StardustTagName,buffer,offset,count);
//        }

//        public static MemoryStream GetStream(byte[] buffer)
//        {
//            return manager.GetStream(StardustTagName, buffer, 0,buffer.Length);
//        }

//        public static MemoryStream GetStream(string debugTag, byte[] buffer, int offset, int count)
//        {
//            return manager.GetStream(debugTag, buffer, offset, count);
//        }

//        public static MemoryStream GetStream(string debugTag, byte[] buffer)
//        {
//            return GetStream(debugTag, buffer, 0, buffer.Length);
//        }
//    }
//}

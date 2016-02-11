using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.ServiceBus.Messaging;

namespace Stardust.Core.Azure
{
    public interface INotificationService
    {
        void Initialize(EventHubSender sender);

    }

    internal static class SerializationHelpers
    {
        public static MemoryStream SerializeToMemoryStream<T>(T obj)
        {
            var memStream = new MemoryStream();
            Serialize(obj, memStream);
            memStream.Position = 0;
            return memStream;
        }


        public static T Deserializer<T>(Stream source)
        {
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(source);
        }


        public static T TryDeserializer<T>(Stream source) where T : class
        {
            try
            {
                return Deserializer<T>(source);
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static void Serialize<T>(T source, Stream destination)
        {
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(destination, source);
        }

    }
}

using Stardust.Core.FactoryHelpers;

namespace Stardust.Core.Sandbox.Serializer
{
    /// <summary>
    /// 
    /// </summary>
    public static class FlatFileSerializer
    {
        static FlatFileSerializer()
        {
            Resolver.Bind<IFlatFileSerializer>().To<FlatFileSerializerImp>();
            Resolver.Bind<IFlatFileDeserializer>().To<FlatFileDeserializerImp>();
        }

        /// <summary>
        /// Serializes an array of objects to the specified file path.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">The file path.</param>
        /// <param name="objectsToSerialize">The objects automatic serialize.</param>
        public static void Serialize<T>(string filePath, T[] objectsToSerialize)
        {
            Resolver.Resolve<IFlatFileSerializer>().Activate().Serialize(filePath,objectsToSerialize,typeof(T));
        }

        /// <summary>
        /// Deserializes the specified serialized objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObjects">The serialized objects.</param>
        /// <returns></returns>
        public static T[] Deserialize<T>(string serializedObjects) where T : new()
        {
            return Resolver.Resolve<IFlatFileDeserializer>().Activate().Deserialize<T>(serializedObjects);
        }
    }
}

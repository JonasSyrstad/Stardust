namespace Stardust.Core.Sandbox.Serializer
{
    public interface IFlatFileDeserializer
    {
        T[] Deserialize<T>(string serializedObjects) where T : new();
    }
}
using System;

namespace Stardust.Core.Sandbox.Serializer
{
    public interface IFlatFileSerializer
    {
        void Serialize<T>(string filePath, T[] objectsToSerialize, Type type);
    }
}
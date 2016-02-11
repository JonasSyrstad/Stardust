using System;
using System.Collections.Concurrent;
using System.IO;
using ProtoBuf.Meta;

namespace Stardust.Core.Default.Implementations
{
    public static class ProtobufSerializerHelpers
    {
        private static ConcurrentDictionary<Type, RuntimeTypeModel> protobufModel = new ConcurrentDictionary<Type, RuntimeTypeModel>();
        private static ConcurrentDictionary<Type, TypeModel> compiledModels = new ConcurrentDictionary<Type, TypeModel>();

        public static IModelConfigurationContext<T> CreateModel<T>()
        {
            return new ModelConfigurationContext<T>(DoCreateModel<T>());
        }
        private static RuntimeTypeModel DoCreateModel<T>()
        {
            RuntimeTypeModel model;
            if (protobufModel.TryGetValue(typeof(T), out model)) return model;
            model = TypeModel.Create();
            if (protobufModel.TryAdd(typeof(T), model)) return model;
            if (!protobufModel.TryGetValue(typeof(T), out model)) throw new InvalidDataException("Unable to create cached model");
            return model;
        }

        internal static void CompileAndCache<T>(this RuntimeTypeModel model)
        {
            TypeModel compileModel;
            if (compiledModels.TryGetValue(typeof(T), out compileModel)) return;
            var cModel = model.Compile();
            if (compiledModels.TryGetValue(typeof(T), out compileModel)) return;
            compiledModels.TryAdd(typeof(T), cModel);
        }

        public static MemoryStream SerializeToStream<T>(this T instance)
        {
            TypeModel model;
            if (compiledModels.TryGetValue(typeof(T), out model))
            {
                var stream = new MemoryStream();
                {
                    model.Serialize(stream, instance);
                    stream.Position = 0;
                    return stream;
                }
            }
            throw new IndexOutOfRangeException(string.Format("There is no model for {0}", typeof(T)));
        }

        public static T Deserialize<T>(this string protobufString)
        {
            using (var reader = new MemoryStream(Convert.FromBase64String(protobufString)))
            {
                return Deserialize<T>(reader);
            }
        }

        public static T Deserialize<T>(this Stream reader)
        {
            TypeModel model;
            if (compiledModels.TryGetValue(typeof(T), out model))
            {
                return (T)model.Deserialize(reader, null, typeof(T));
            }
            throw new IndexOutOfRangeException(string.Format("There is no model for {0}", typeof(T)));
        }

        public static void SerializeIntoStream<T>(this Stream stream, T message)
        {
            TypeModel model;
            if (!compiledModels.TryGetValue(typeof (T), out model)) throw new IndexOutOfRangeException(string.Format("There is no model for {0}", typeof (T)));
            model.Serialize(stream, message);
        }

        public static string Serialize<T>(this T instance)
        {
            using (var stream = instance.SerializeToStream())
            {
                return ReadStringFromStream(stream);
            }
        }
        private static string ReadStringFromStream(MemoryStream stream)
        {
            stream.Position = 0;
            var buffer = stream.GetBuffer();
            return Convert.ToBase64String(buffer, 0, (int)stream.Length);
        }
    }
}

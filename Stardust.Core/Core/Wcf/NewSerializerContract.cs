using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;

namespace Stardust.Core.Wcf
{
    /// <summary>
    /// https://code.msdn.microsoft.com/windowsdesktop/WCF-Custom-Serialization-43b3ee7a
    /// </summary>
    public class MyNewSerializerContractBehaviorAttribute : Attribute, IContractBehavior
    {
        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            this.ReplaceSerializerOperationBehavior(contractDescription);
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            this.ReplaceSerializerOperationBehavior(contractDescription);
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
            foreach (OperationDescription operation in contractDescription.Operations)
            {
                foreach (MessageDescription message in operation.Messages)
                {
                    this.ValidateMessagePartDescription(message.Body.ReturnValue);
                    foreach (MessagePartDescription part in message.Body.Parts)
                    {
                        this.ValidateMessagePartDescription(part);
                    }

                    foreach (MessageHeaderDescription header in message.Headers)
                    {
                        this.ValidateCustomSerializableType(header.Type);
                    }
                }
            }
        }

        private void ValidateMessagePartDescription(MessagePartDescription part)
        {
            if (part != null)
            {
                this.ValidateCustomSerializableType(part.Type);
            }
        }

        private void ValidateCustomSerializableType(Type type)
        {
            if (typeof(ICustomSerializable).IsAssignableFrom(type))
            {
                if (!type.IsPublic)
                {
                    throw new InvalidOperationException("Custom serialization is supported in public types only");
                }

                ConstructorInfo defaultConstructor = type.GetConstructor(new Type[0]);
                if (defaultConstructor == null)
                {
                    throw new InvalidOperationException("Custom serializable types must have a public, parameterless constructor");
                }
            }
        }

        private void ReplaceSerializerOperationBehavior(ContractDescription contract)
        {
            foreach (OperationDescription od in contract.Operations)
            {
                for (int i = 0; i < od.Behaviors.Count; i++)
                {
                    DataContractSerializerOperationBehavior dcsob = od.Behaviors[i] as DataContractSerializerOperationBehavior;
                    if (dcsob != null)
                    {
                        od.Behaviors[i] = new MyNewSerializerOperationBehavior(od);
                    }
                }
            }
        }

        class MyNewSerializerOperationBehavior : DataContractSerializerOperationBehavior
        {
            public MyNewSerializerOperationBehavior(OperationDescription operation) : base(operation) { }
            public override XmlObjectSerializer CreateSerializer(Type type, string name, string ns, IList<Type> knownTypes)
            {
                return new MyNewSerializer(type, base.CreateSerializer(type, name, ns, knownTypes));
            }

            public override XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
            {
                return new MyNewSerializer(type, base.CreateSerializer(type, name, ns, knownTypes));
            }
        }

        public class MyNewSerializer : XmlObjectSerializer
        {
            const string MyPrefix = "new";
            Type type;
            bool isCustomSerialization;
            XmlObjectSerializer fallbackSerializer;
            public MyNewSerializer(Type type, XmlObjectSerializer fallbackSerializer)
            {
                this.type = type;
                this.isCustomSerialization = typeof(ICustomSerializable).IsAssignableFrom(type);
                this.fallbackSerializer = fallbackSerializer;
            }

            public override bool IsStartObject(XmlDictionaryReader reader)
            {
                if (this.isCustomSerialization)
                {
                    return reader.LocalName == MyPrefix;
                }
                else
                {
                    return this.fallbackSerializer.IsStartObject(reader);
                }
            }

            public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
            {
                if (this.isCustomSerialization)
                {
                    object result = Activator.CreateInstance(this.type);
                    MemoryStream ms = new MemoryStream(reader.ReadElementContentAsBase64());
                    ((ICustomSerializable)result).InitializeFrom(ms);
                    return result;
                }
                else
                {
                    return this.fallbackSerializer.ReadObject(reader, verifyObjectName);
                }
            }

            public override void WriteEndObject(XmlDictionaryWriter writer)
            {
                if (this.isCustomSerialization)
                {
                    writer.WriteEndElement();
                }
                else
                {
                    this.fallbackSerializer.WriteEndObject(writer);
                }
            }

            public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
            {
                if (this.isCustomSerialization)
                {
                    var ms = new MemoryStream();
                    ((ICustomSerializable)graph).WriteTo(ms);
                    byte[] bytes = ms.ToArray();
                    writer.WriteBase64(bytes, 0, bytes.Length);
                }
                else
                {
                    this.fallbackSerializer.WriteObjectContent(writer, graph);
                }
            }

            public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
            {
                if (this.isCustomSerialization)
                {
                    writer.WriteStartElement(MyPrefix);
                }
                else
                {
                    this.fallbackSerializer.WriteStartObject(writer, graph);
                }
            }
        }

    }
    public interface ICustomSerializable
    {
        void WriteTo(Stream stream);
        void InitializeFrom(Stream stream);
    }
}

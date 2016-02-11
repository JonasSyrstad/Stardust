using System.Runtime.Serialization;
using System.ServiceModel;
using Stardust.Interstellar;
using Stardust.Interstellar.Messaging;

namespace Stardust.Core.ServicesTest.Mocks
{
    

    public class Message:DuplexMessageBase
    {
    }

    [ServiceContract]
    [ServiceName("ContactsService")]
    public interface IContacts
    {

        [OperationContract]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class CompositeType : DuplexMessageBase
    {
        bool boolValue = true;
        string stringValue = "Runtime: ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}

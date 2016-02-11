using System.ServiceModel;

namespace Stardust.Interstellar.Messaging
{
    [MessageContract]
    public abstract class RequestMessageContractBase<TBody> : IRequestBase
    {
        private RequestHeader requestHeader;

        protected RequestMessageContractBase()
        {
            requestHeader=new RequestHeader();
        }

        protected RequestMessageContractBase(TBody messageBody):this()
        {
            Body = messageBody;
        }

        [MessageHeader(Name = "reqHeader")]
        RequestHeader IRequestBase.RequestHeader
        {
            get { return requestHeader; }
            set { requestHeader = value; }
        }

        [MessageBodyMember(Name = "RequestBody")]
        public TBody Body { get; set; }
    }

    [MessageContract]
    public abstract class ResponseMessageContractBase<TBody>:IResponseBase
    {
        private ResponseHeader responseHeader;

        protected ResponseMessageContractBase()
        {
            responseHeader=new ResponseHeader();
        }

        protected ResponseMessageContractBase(TBody messageBody):this()
        {
            Body = messageBody;
        }

        [MessageHeader(Name = "resHeader")]
        ResponseHeader IResponseBase.ResponseHeader
        {
            get { return responseHeader; }
            set { responseHeader = value; }
        }

        [MessageBodyMember(Name = "ResponseBody")]
        public TBody Body { get; set; }
    }
}

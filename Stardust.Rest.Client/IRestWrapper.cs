using System;
using System.Threading.Tasks;

namespace Stardust.Rest.Client
{
    public interface IRestWrapper
    {
        Task<object> InvokeMemberPostAsync(object request, Type resultType);

        object InvokeMemberPost(object request, Type resultType);

        Task<TOut> RunTypedPostAsync<TOut>(object request);

       
        Task<object> InvokeMemberGetAsync(object request, Type resultType);

        object InvokeMemberGet(object request, Type resultType);

        Task<TOut> RunTypedGetAsync<TOut>(object request);

        Task<object> InvokeMemberPutAsync(object request, Type resultType);

        object InvokeMemberPut(object request, Type resultType);

        Task<TOut> RunTypedPutAsync<TOut>(object request);

        Task<object> InvokeMemberDeleteAsync(object request, Type resultType);

        object InvokeMemberDelete(object request, Type resultType);

        Task<TOut> RunTypedDeleteAsync<TOut>(object request);
    }
}
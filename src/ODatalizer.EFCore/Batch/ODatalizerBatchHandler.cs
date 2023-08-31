using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace ODatalizer.EFCore.Batch
{
    public class ODatalizerBatchHandler : DefaultODataBatchHandler
    {
        public override async Task<IList<ODataBatchResponseItem>> ExecuteRequestMessagesAsync(IEnumerable<ODataBatchRequestItem> requests, RequestDelegate handler)
        {
            var tran = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled);

            var responses = await base.ExecuteRequestMessagesAsync(requests, handler);

            if (responses.All(r =>
            {
                if (r is ChangeSetResponseItem c)
                    return c.Contexts.All(c => c.Response.IsSuccessStatusCode());
                if (r is OperationResponseItem o)
                    return o.Context.Response.IsSuccessStatusCode();

                return false;
            }))
            {
                tran.Complete();
            }

            tran.Dispose();

            return responses;
        }
    }
}

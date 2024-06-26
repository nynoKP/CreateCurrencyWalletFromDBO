using Camunda.Worker;
using Camunda.Worker.Variables;
using CreateCurrencyWalletFromDBO.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCurrencyWalletFromDBO.Workers
{
    [HandlerTopics("GetClientAccounts")]
    public class AccountGetHandler : IExternalTaskHandler
    {
        private readonly ILogger<AccountGetHandler> _logger;
        public AccountGetHandler(ILogger<AccountGetHandler> logger)
        {
            _logger = logger;
        }
        public async Task<IExecutionResult> HandleAsync(ExternalTask externalTask, CancellationToken cancellationToken)
        {
            try
            {
                var accGetRequest = new Extensions.AnorHubModels.NciAccountGet.NciAccountGetRequestDTO
                {
                    id = 1,
                    jsonrpc = "2.0",
                    method = "nci.account.get",
                    @params = new Extensions.AnorHubModels.NciAccountGet.Params()
                    {
                        agentRequestId = new Guid().ToString(),
                        account = new Extensions.AnorHubModels.NciAccountGet.Account
                        {
                            clientId = externalTask.Variables.GetValue<string>("NciClientId") ?? "",
                            accBal = "22616",
                            currency = GetCurrency(externalTask.Variables.GetValue<string>("Currency"))
                        }
                    }
                };
                var accGetResult = await AnorHubService.Instance.NciAccountGet(accGetRequest);
                return await Task.FromResult<IExecutionResult>(new CompleteResult()
                {
                    //тут криво и временно
                    Variables = new Dictionary<string, VariableBase>
                    {
                        ["IsSuccess"] = new BooleanVariable(!accGetResult.IsSuccess && accGetResult.Object.result.status == "SUCCESS"),
                        ["IsClientHasWallet"] = new BooleanVariable(/*accGetResult.IsSuccess && accGetResult.Object.result.status == "SUCCESS" && accGetResult.Object.result.data.Any()*/ false),
                        ["AccountGetObject"] = JsonVariable.Create(accGetResult)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Script Task Error: " + ex.ToString());
                return await Task.FromResult<IExecutionResult>(new FailureResult(ex));
            }
        }

        private static string GetCurrency(string? currency)
        {
            if (currency == null) throw new ArgumentNullException("MessageContent.products.currency");

            if (currency == "UZS") return "000";
            if (currency == "USD") return "840";
            if (currency == "EUR") return "978";
            if (currency == "RUB") return "643";

            return string.Empty;
        }
    }
}

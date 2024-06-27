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
    [HandlerTopics("CreateClientAccount")]
    public class AccountCreateHandler : IExternalTaskHandler
    {
        private readonly ILogger<AccountCreateHandler> _logger;
        public AccountCreateHandler(ILogger<AccountCreateHandler> logger)
        {
            _logger = logger;
        }
        public async Task<IExecutionResult> HandleAsync(ExternalTask externalTask, CancellationToken cancellationToken)
        {
            try
            {
                var accCreateRequest = new Extensions.AnorHubModels.NciAccountCreate.NciAccountCreateRequestDTO
                {
                    id = 1,
                    jsonrpc = "2.0",
                    method = "nci.account.create",
                    @params = new Extensions.AnorHubModels.NciAccountCreate.Params()
                    {
                        agentRequestId = Guid.NewGuid().ToString(),
                        account = new Extensions.AnorHubModels.NciAccountCreate.Account
                        {
                            accBal = "22616",
                            clientId = externalTask.Variables.GetValue<string>("NciClientId") ?? "",
                            currency = GetCurrency(externalTask.Variables.GetValue<string>("Currency")),
                            idOrder = "000",
                            orderBetween = "001;199",
                            name = string.Format("{0} {1}", GetCurrency(externalTask.Variables.GetValue<string>("Currency")) ?? "", "Электронный кошелек"),
                            signOpen = "0"
                        }
                    }
                };
                var accCreateResult = await AnorHubService.Instance.NciAccountCreate(accCreateRequest);
                return await Task.FromResult<IExecutionResult>(new CompleteResult()
                {
                    //тут криво и временно
                    Variables = new Dictionary<string, VariableBase>
                    {
                        ["AccountCreateObject"] = JsonVariable.Create(accCreateResult),
                        ["IsSuccess"] = new BooleanVariable(accCreateResult.IsSuccess),
                        ["IsAccountCreated"] = new BooleanVariable(accCreateResult.IsSuccess && accCreateResult?.Object?.result?.status == "SUCCESS")
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
            if (currency == null) throw new ArgumentNullException("Currency");

            if (currency == "UZS") return "000";
            if (currency == "USD") return "840";
            if (currency == "EUR") return "978";
            if (currency == "RUB") return "643";

            return string.Empty;
        }
    }
}

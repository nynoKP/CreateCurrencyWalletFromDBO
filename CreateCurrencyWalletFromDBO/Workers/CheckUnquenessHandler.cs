using Camunda.Api.Client;
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
    [HandlerTopics("CheckClientUniqueness")]
    public class CheckUnquenessHandler : IExternalTaskHandler
    {
        private readonly ILogger<CheckUnquenessHandler> _logger;
        public CheckUnquenessHandler(ILogger<CheckUnquenessHandler> logger)
        {
            _logger = logger;
        }
        public async Task<IExecutionResult> HandleAsync(ExternalTask externalTask, CancellationToken cancellationToken)
        {
            try
            {
                CamundaClient camunda = CamundaClient.Create(ConfigurationManager.Configuration.GetSection("CamundaConnection")["url"] ?? "");
                var processList = await camunda.ProcessInstances.Query(new Camunda.Api.Client.ProcessInstance.ProcessInstanceQuery()
                {
                    ProcessDefinitionKey = externalTask.ProcessDefinitionKey,
                    Active = true,
                    Variables = new List<VariableQueryParameter>
                    {
                        new VariableQueryParameter()
                        {
                            Name = "Pinfl",
                            Operator = ConditionOperator.Equals,
                            Value = externalTask.Variables.GetValue<string>("Pinfl")
                        },
                        new VariableQueryParameter()
                        {
                            Name = "Currency",
                            Operator = ConditionOperator.Equals,
                            Value = externalTask.Variables.GetValue<string>("Currency")
                        },
                        new VariableQueryParameter()
                        {
                            Name = "Method",
                            Operator = ConditionOperator.Equals,
                            Value = externalTask.Variables.GetValue<string>("Method")
                        }
                    }
                }).List();
                var processCount = processList.Where(c=>c.Id != externalTask.ProcessInstanceId).Count();
                return await Task.FromResult<IExecutionResult>(new CompleteResult()
                {
                    Variables = new Dictionary<string, VariableBase>
                    {
                        ["IsUniqueness"] = new BooleanVariable(processCount == 0)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Script Task Error: " + ex.ToString());
                return await Task.FromResult<IExecutionResult>(new FailureResult(ex));
            }
        }
    }
}

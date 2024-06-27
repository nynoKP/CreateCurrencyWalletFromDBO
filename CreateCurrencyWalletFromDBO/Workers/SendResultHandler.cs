using Camunda.Worker;
using Camunda.Worker.Variables;
using CreateCurrencyWalletFromDBO.Extensions;
using CreateCurrencyWalletFromDBO.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCurrencyWalletFromDBO.Workers
{
    [HandlerTopics("SendResult")]
    public class SendResultHandler : IExternalTaskHandler
    {
        private readonly ILogger<SendResultHandler> _logger;
        public SendResultHandler(ILogger<SendResultHandler> logger)
        {
            _logger = logger;
        }
        public async Task<IExecutionResult> HandleAsync(ExternalTask externalTask, CancellationToken cancellationToken)
        {
            try
            {
                var isAccountCreated = externalTask.Variables.GetValue<bool?>("IsAccountCreated");
                var uniqueId = externalTask.Variables.GetValue<string>("UniqueId");
                if (uniqueId != null)
                {
                    SendResultToDBO resultObj = null;
                    if (isAccountCreated != null || isAccountCreated == true)
                    {
                        //4 успех
                        resultObj = new SendResultToDBO(externalTask.ProcessInstanceId, 4, uniqueId);
                        QueueSenderService.SendMessage(resultObj.ToJson());
                    }
                    else
                    {
                        //5 отказ
                        resultObj = new SendResultToDBO(externalTask.ProcessInstanceId, 5, uniqueId);
                        QueueSenderService.SendMessage(resultObj.ToJson());
                    }
                    return await Task.FromResult<IExecutionResult>(new CompleteResult()
                    {
                        Variables = new Dictionary<string, VariableBase>
                        {
                            ["QueueResultMessage"] = new StringVariable(resultObj.ToJson())
                        }
                    });
                }
                else
                {
                    return await Task.FromResult<IExecutionResult>(new CompleteResult()
                    {
                        Variables = new Dictionary<string, VariableBase>
                        {
                            ["Error"] = new StringVariable("Message for queue not sended! UniqueIdNotFound!")
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Script Task Error: " + ex.ToString());
                return await Task.FromResult<IExecutionResult>(new FailureResult(ex));
            }
        }
    }
}

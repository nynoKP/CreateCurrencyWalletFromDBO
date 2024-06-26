using Camunda.Worker;
using Camunda.Worker.Variables;
using CreateCurrencyWalletFromDBO.Extensions;
using CreateCurrencyWalletFromDBO.Models.MessageContentDTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static CreateCurrencyWalletFromDBO.Extensions.CamundaClientHelper;

namespace CreateCurrencyWalletFromDBO.Workers
{
    [HandlerTopics("ParseAndCheckData")]
    public class ParseMessageHandler : IExternalTaskHandler
    {
        private readonly ILogger<ParseMessageHandler> _logger;
        public ParseMessageHandler(ILogger<ParseMessageHandler> logger)
        {
            _logger = logger;
        }
        public Task<IExecutionResult> HandleAsync(ExternalTask externalTask, CancellationToken cancellationToken)
        {
            try
            {
                var messageContent = externalTask.Variables.GetValue<string>("MessageContent");
                var messageObject = JsonConvert.DeserializeObject<MessageContentDTO>(messageContent);
                if (messageContent != null && messageObject.IsValid())
                {
                    return Task.FromResult<IExecutionResult>(new CompleteResult()
                    {
                        Variables = new Dictionary<string, VariableBase>
                        {
                            ["IsParsed"] = new BooleanVariable(true),
                            ["Pinfl"] = new StringVariable(messageObject.MessageContent.client_info.pinfl),
                            ["Method"] = new StringVariable(messageObject.Method),
                            ["Currency"] = new StringVariable(messageObject.MessageContent.products.currency),
                            ["NciClientId"] = new StringVariable(messageObject.MessageContent.client_info.nci_number)
                        }
                    });
                }
                else
                {
                    return Task.FromResult<IExecutionResult>(new CompleteResult()
                    {
                        Variables = new Dictionary<string, VariableBase>
                        {
                            ["IsParsed"] = new BooleanVariable(false)
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Script Task Error: " + ex.ToString());
                return Task.FromResult<IExecutionResult>(new FailureResult(ex));
            }
        }
    }
}

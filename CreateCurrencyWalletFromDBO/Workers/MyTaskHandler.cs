
using Camunda.Worker.Client;
using Camunda.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
using Camunda.Worker.Execution;
using Camunda.Worker.Variables;

namespace CreateCurrencyWalletFromDBO.Workers
{
    [HandlerTopics("MyTaskHandler")]
    public class MyTaskHandler : IExternalTaskHandler
    {
        private readonly ILogger<MyTaskHandler> _logger;

        public MyTaskHandler(ILogger<MyTaskHandler> logger)
        {
            _logger = logger;
        }

        public Task<IExecutionResult> HandleAsync(ExternalTask externalTask, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling task {TaskId}", externalTask.Id);

            // Обработка задачи
            var variables = externalTask.Variables;
            var someVariable = variables.GetValueOrDefault("someVariable");
            _logger.LogInformation("Received variable: {SomeVariable}", someVariable);

            // Завершение задачи
            return Task.FromResult<IExecutionResult>(new CompleteResult
            {
                Variables = new Dictionary<string, VariableBase>
                {
                    ["result"] = new StringVariable("Task completed successfully")
                }
            });
        }
    }
}

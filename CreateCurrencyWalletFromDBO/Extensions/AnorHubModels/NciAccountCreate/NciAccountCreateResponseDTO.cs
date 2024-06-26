using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCurrencyWalletFromDBO.Extensions.AnorHubModels.NciAccountCreate
{
    public class AccountResult
    {
        public string errorCode { get; set; }
        public string errorNote { get; set; }
        public string tranId { get; set; }
    }

    public class Data
    {
        public AccountResult accountResult { get; set; }
        public string checkAccountStatusId { get; set; }
    }

    public class Result
    {
        public string agentRequestId { get; set; }
        public string method { get; set; }
        public string message { get; set; }
        public string status { get; set; }
        public Data data { get; set; }
    }

    public class NciAccountCreateResponseDTO
    {
        public string jsonrpc { get; set; }
        public int id { get; set; }
        public Result result { get; set; }
    }
}

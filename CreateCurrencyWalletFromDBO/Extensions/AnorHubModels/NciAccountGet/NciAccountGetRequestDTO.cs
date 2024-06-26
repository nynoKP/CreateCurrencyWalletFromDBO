using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCurrencyWalletFromDBO.Extensions.AnorHubModels.NciAccountGet
{
    public class Account
    {
        public string transId { get; set; }
        public string name { get; set; }
        public string clientId { get; set; }
        public string accBal { get; set; }
        public string currency { get; set; }
        public string idOrder { get; set; }
    }

    public class Params
    {
        public string agentRequestId { get; set; }
        public Account account { get; set; }
    }

    public class NciAccountGetRequestDTO
    {
        public int id { get; set; }
        public string jsonrpc { get; set; }
        public string method { get; set; }
        public Params @params { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCurrencyWalletFromDBO.Extensions.AnorHubModels.NciAccountGet
{
    public class Datum
    {
        public string errorCode { get; set; }
        public string errorNote { get; set; }
        public string transId { get; set; }
        public string branch { get; set; }
        public string id { get; set; }
        public string accBal { get; set; }
        public string currency { get; set; }
        public string client { get; set; }
        public string idOrder { get; set; }
        public string name { get; set; }
        public string sgn { get; set; }
        public string bal { get; set; }
        public string signRegistr { get; set; }
        public string dt { get; set; }
        public string ct { get; set; }
        public string dtTmp { get; set; }
        public string ctTmp { get; set; }
        public DateTime dateOpen { get; set; }
        public string accGroupId { get; set; }
        public string state { get; set; }
        public string kodErr { get; set; }
        public DateTime dateOpenNibbd { get; set; }
        public string subBranch { get; set; }
        public string nameCl { get; set; }
        public string sin { get; set; }
        public string sout { get; set; }
        public string sinTmp { get; set; }
        public string soutTmp { get; set; }
        public DateTime ldate { get; set; }
    }

    public class Result
    {
        public string agentRequestId { get; set; }
        public string method { get; set; }
        public int hubId { get; set; }
        public string message { get; set; }
        public string status { get; set; }
        public List<Datum> data { get; set; }
    }

    public class NciAccountGetResponseDTO
    {
        public string jsonrpc { get; set; }
        public int id { get; set; }
        public Result result { get; set; }
    }
}

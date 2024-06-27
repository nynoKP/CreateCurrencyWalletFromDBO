using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCurrencyWalletFromDBO.Models
{
    public class MessageContent
    {
        public System system { get; set; }
    }

    public class SendResultToDBO
    {
        public SendResultToDBO(string InstId, int status, string uniqueId)
        {
            TransactionID = InstId;
            InstanceID = InstId;
            MessageType = "DataRequest";
            Action = "Dbo";
            Method = "LoanProposal";
            MessageContent = new MessageContent();
            MessageContent.system = new System();
            MessageContent.system.unique_id = uniqueId;
            MessageContent.system.claim_id = null;
            MessageContent.system.application_status = status;
            MessageContent.system.process_name = "CurrencyWallet";
            MessageContent.system.product_id = null;
            MessageContent.system.approved_amount = null;
            MessageContent.system.approved_term = null;
        }
        public string TransactionID { get; set; }
        public string InstanceID { get; set; }
        public string MessageType { get; set; }
        public string Action { get; set; }
        public string Method { get; set; }
        public MessageContent MessageContent { get; set; }

        public string ToJson() => JsonConvert.SerializeObject(this);
    }

    public class System
    {
        public string unique_id { get; set; }
        public object? claim_id { get; set; }
        public int application_status { get; set; }
        public string process_name { get; set; }
        public object? product_id { get; set; }
        public object? approved_amount { get; set; }
        public object? approved_term { get; set; }
    }
}

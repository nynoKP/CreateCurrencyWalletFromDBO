using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCurrencyWalletFromDBO.Models.MessageContentDTO
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ClientInfo
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string name { get; set; }

        [JsonProperty("surname", NullValueHandling = NullValueHandling.Ignore)]
        public string surname { get; set; }

        [JsonProperty("pantronomic", NullValueHandling = NullValueHandling.Ignore)]
        public string pantronomic { get; set; }

        [JsonProperty("pinfl", NullValueHandling = NullValueHandling.Ignore)]
        public string pinfl { get; set; }

        [JsonProperty("passport_series", NullValueHandling = NullValueHandling.Ignore)]
        public string passport_series { get; set; }

        [JsonProperty("passport_number", NullValueHandling = NullValueHandling.Ignore)]
        public string passport_number { get; set; }

        [JsonProperty("phone_number", NullValueHandling = NullValueHandling.Ignore)]
        public string phone_number { get; set; }

        [JsonProperty("birth_date", NullValueHandling = NullValueHandling.Ignore)]
        public string birth_date { get; set; }

        [JsonProperty("nci_number", NullValueHandling = NullValueHandling.Ignore)]
        public string nci_number { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(surname) && !string.IsNullOrWhiteSpace(pantronomic) && !string.IsNullOrWhiteSpace(pinfl) && !string.IsNullOrWhiteSpace(passport_series) && !string.IsNullOrWhiteSpace(passport_number) && !string.IsNullOrWhiteSpace(phone_number) && !string.IsNullOrWhiteSpace(birth_date) && !string.IsNullOrWhiteSpace(nci_number);
        }
    }

    public class Content
    {
        [JsonProperty("system", NullValueHandling = NullValueHandling.Ignore)]
        public System system { get; set; }

        [JsonProperty("products", NullValueHandling = NullValueHandling.Ignore)]
        public Products products { get; set; }

        [JsonProperty("client_info", NullValueHandling = NullValueHandling.Ignore)]
        public ClientInfo client_info { get; set; }

        public bool IsValid()
        {
            return products != null && products.currency != null && system !=null && system.unique_id != null && client_info.IsValid();
        }
    }

    public class Products
    {
        [JsonProperty("currency", NullValueHandling = NullValueHandling.Ignore)]
        public string currency { get; set; }
    }

    public class MessageContentDTO
    {
        [JsonProperty("Action", NullValueHandling = NullValueHandling.Ignore)]
        public string Action { get; set; }

        [JsonProperty("Method", NullValueHandling = NullValueHandling.Ignore)]
        public string Method { get; set; }

        [JsonProperty("MessageContent", NullValueHandling = NullValueHandling.Ignore)]
        public Content MessageContent { get; set; }

        public bool IsValid()
        {
            return Method != null && Method == "CurrencyWallet" && MessageContent.IsValid();
        }
    }

    public class System
    {
        [JsonProperty("language", NullValueHandling = NullValueHandling.Ignore)]
        public string language { get; set; }

        [JsonProperty("unique_id", NullValueHandling = NullValueHandling.Ignore)]
        public string unique_id { get; set; }

        [JsonProperty("loan_date", NullValueHandling = NullValueHandling.Ignore)]
        public string loan_date { get; set; }

        [JsonProperty("application_status", NullValueHandling = NullValueHandling.Ignore)]
        public string application_status { get; set; }
    }


}

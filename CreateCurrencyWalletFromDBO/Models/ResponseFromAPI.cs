using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCurrencyWalletFromDBO.Models
{
    public class ResponseFromAPI<T>
    {
        public RequestToApi RequestData { get; set; }
        public T Object { get { try { return JsonConvert.DeserializeObject<T>(Response); } catch { return default(T); } } }
        public string Response { get; set; }
        public bool IsSuccess { get; set; }
        public Exception Exception { get; set; }
    }
}

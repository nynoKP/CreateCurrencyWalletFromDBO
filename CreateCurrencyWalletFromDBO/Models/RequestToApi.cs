using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCurrencyWalletFromDBO.Models
{
    public class RequestToApi
    {
        public string Url { get; set; }
        public HttpMethod Method { get; set; } = HttpMethod.Post;
        public string Request { get; set; }
        public string JwtToken { get; set; }
        public string ContentType { get; set; } = "application/json";
    }
}

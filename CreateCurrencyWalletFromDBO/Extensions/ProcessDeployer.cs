using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CreateCurrencyWalletFromDBO.Extensions
{
    public class ProcessDeployer
    {
        public string Deploy(string deploymentName, List<object> files)
        {
            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add("deployment-name", deploymentName);
            postParameters.Add("deployment-source", "C# Process Application");
            postParameters.Add("enable-duplicate-filtering", "true");
            postParameters.Add("data", files);

            var helper = new CamundaClientHelper(new Uri(ConfigurationManager.Configuration.GetSection("CamundaConnection")["url"] ?? ""), null, null);

            // Create request and receive response
            string postURL = helper.RestUrl + "deployment/create";
            HttpWebResponse webResponse = FormUpload.MultipartFormDataPost(postURL, helper.RestUsername, helper.RestPassword, postParameters);

            using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
            {
                var deployment = JsonConvert.DeserializeObject<Deployment>(reader.ReadToEnd());
                return deployment.Id;
            }
        }

        public void AutoDeploy()
        {
            Assembly thisExe = Assembly.GetEntryAssembly();
            string[] resources = thisExe.GetManifestResourceNames();

            if (resources.Length == 0)
            {
                return;
            }

            List<object> files = new List<object>();
            foreach (string resource in resources)
            {
                // TODO Check if Camunda relevant (BPMN, DMN, HTML Forms)

                // Read and add to Form for Deployment                
                files.Add(FileParameter.FromManifestResource(thisExe, resource));

                Log.ForContext<ProcessDeployer>().Information("Adding resource to deployment: " + resource);
            }
            Deploy(thisExe.GetName().Name, files);

            Log.ForContext<ProcessDeployer>().Information("Deployment to Camunda BPM succeeded.");

        }
    }

    public class CamundaClientHelper
    {
        public Uri RestUrl { get; }
        public const string CONTENT_TYPE_JSON = "application/json";
        public string RestUsername { get; }
        public string RestPassword { get; }

        private static HttpClient client;

        public CamundaClientHelper(Uri restUrl, string username, string password)
        {
            this.RestUrl = restUrl;
            this.RestUsername = username;
            this.RestPassword = password;
        }

        public HttpClient HttpClient()
        {
            if (client == null)
            {
                if (RestUsername != null)
                {
                    var credentials = new NetworkCredential(RestUsername, RestPassword);
                    client = new HttpClient(new HttpClientHandler() { Credentials = credentials });
                }
                else
                {
                    client = new HttpClient();
                    client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite); // Infinite / really?
                }
                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(CONTENT_TYPE_JSON));
                client.BaseAddress = RestUrl;
            }

            return client;
        }

        public static Dictionary<string, Variable> ConvertVariables(Dictionary<string, object> variables)
        {
            // report successful execution
            var result = new Dictionary<string, Variable>();
            if (variables == null)
            {
                return result;
            }
            foreach (var variable in variables)
            {
                Variable camundaVariable = new Variable
                {
                    Value = variable.Value
                };
                result.Add(variable.Key, camundaVariable);
            }
            return result;
        }

        public class Variable
        {
            // lower case to generate JSON we need
            public string Type { get; set; }
            public object Value { get; set; }
            public object ValueInfo { get; set; }
        }
    }

    public class FileParameter
    {
        public byte[] File { get; }
        public string FileName { get; }
        public string ContentType { get; }
        public FileParameter(byte[] file) : this(file, null) { }
        public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
        public FileParameter(byte[] file, string filename, string contenttype)
        {
            File = file;
            FileName = filename;
            ContentType = contenttype;
        }

        public static FileParameter FromManifestResource(Assembly assembly, string resourcePath)
        {
            Stream resourceAsStream = assembly.GetManifestResourceStream(resourcePath);
            byte[] resourceAsBytearray;
            using (MemoryStream ms = new MemoryStream())
            {
                resourceAsStream.CopyTo(ms);
                resourceAsBytearray = ms.ToArray();
            }

            // TODO: Verify if this is the correct way of doing it:
            string assemblyBaseName = assembly.GetName().Name;
            string fileLocalName = resourcePath.Replace(assemblyBaseName + ".", "");

            return new FileParameter(resourceAsBytearray, fileLocalName);
        }
    }

    public class Deployment
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }

        public override string ToString() => $"Deployment [Id={Id}, Name={Name}]";
    }

    public static class FormUpload
    {
        private static readonly Encoding encoding = Encoding.UTF8;

        public static HttpWebResponse MultipartFormDataPost(string postUrl, string username, string password, Dictionary<string, object> postParameters)
        {
            string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
            string contentType = "multipart/form-data; boundary=" + formDataBoundary;

            byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

            return PostForm(postUrl, username, password, contentType, formData);
        }

        private static HttpWebResponse PostForm(string postUrl, string username, string password, string contentType, byte[] formData)
        {
            HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

            if (request == null)
            {
                throw new Exception("request is not a HTTP request");
            }

            // Set up the request properties.
            request.Method = "POST";
            request.ContentType = contentType;
            //request.UserAgent = userAgent;
            request.CookieContainer = new CookieContainer();
            request.ContentLength = formData.Length;

            // You could add authentication here as well if needed:
            if (username != null)
            {
                request.PreAuthenticate = true;
                request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
                request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(username + ":" + password)));
            }

            // Send the form data to the request.
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
                requestStream.Dispose();
            }

            return request.GetResponse() as HttpWebResponse;
        }

        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            Stream formDataStream = new System.IO.MemoryStream();

            // Thanks to feedback from commenter's, add a CRLF to allow multiple parameters to be added.
            // Skip it on the first parameter, add it to subsequent parameters.
            bool needsCLRF = false;

            foreach (var param in postParameters)
            {
                if (param.Value is List<object>)
                {
                    // list of files
                    foreach (var value in (List<object>)param.Value)
                    {
                        if (needsCLRF)
                            formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));
                        AddFormData(boundary, formDataStream, param.Key, value);
                        needsCLRF = true;
                    }
                }
                else
                {
                    // only a single file
                    if (needsCLRF)
                        formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                    AddFormData(boundary, formDataStream, param.Key, param.Value);
                    needsCLRF = true;
                }
            }

            // Add the end of the request.  Start with a newline
            string footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

            // Dump the Stream into a byte[]
            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();

            formDataStream.Dispose();

            return formData;
        }

        private static void AddFormData(string boundary, Stream formDataStream, String key, object value)
        {
            var fileToUpload = value as FileParameter;
            if (fileToUpload != null)
            {
                // Add just the first part of this parameter, since we will write the file data directly to the Stream
                string header = string.Format(
                    CultureInfo.InvariantCulture, "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                boundary,
                fileToUpload.FileName ?? key,
                fileToUpload.FileName ?? key,
                fileToUpload.ContentType ?? "application/octet-stream");

                formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                // Write the file data directly to the Stream, rather than serializing it to a string.
                formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
            }
            else
            {
                string postData = string.Format(
                    CultureInfo.InvariantCulture,
                    "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                    boundary,
                    key,
                    value);
                formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
            }
        }
    }
}

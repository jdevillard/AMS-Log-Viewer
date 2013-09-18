using AzureMobileServiceLogViewer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AzureMobileServiceLogViewer.Data
{
    public static class Helper
    {
        //public const string mobileServiceRoot = "services/mobileservices";
        public const string mobileServiceRoot = "mobileservices";
        public const string mobileServiceRegion = mobileServiceRoot + "/regions";
        public const string mobileServiceList = mobileServiceRoot + "/mobileservices";
        public const string mobileServiceGet = mobileServiceList + "/{0}";

        public const string mobileServicePreview = mobileServiceGet + "/previewfeatures";

        public const string mobileServiceLog = mobileServiceGet + "/logs";

        private static String GetSubscriptionUri(string subscriptionId)
        {
            // Build a URI for https://management.core.windows.net/<subscription-id>/services/<operation-type>
            String requestUri = @"https://management.core.windows.net/"
                                    + subscriptionId
                                    + "/services/";

            return requestUri;
        }

        public static Models.Logs GetPrivateLog(string mobileServiceName, string continuationToken)
        {
            MobileDbContext ctx = new MobileDbContext();
            var sub = ctx.MobileServices.Where(m => m.Name == mobileServiceName).First().subscription;
            string subscriptionId = sub.Id.ToString();
            string certString64Encoded = sub.Cert;

            var topCount = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["topCount"]);

            Uri requestUri = new Uri(GetSubscriptionUri(subscriptionId)
                                + getLogUri(mobileServiceName, null, null, continuationToken, topCount));
            
            HttpWebRequest request = GetRequest(requestUri, certString64Encoded);

            var resultType = typeof(Models.Logs);
            try
            {
                return getResponseRequest<Models.Logs>(request);
            }
            catch (Exception)
            {

                return null;
            }

            

        }

        private static HttpWebRequest GetRequest(Uri requestUri, string certString64Encoded)
        {

            // Create the request and specify attributes of the request.
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUri);

            // Define the requred headers to specify the API version and operation type.
            //request.Headers.Add("x-ms-version", "2010-10-28");
            request.Headers.Add("x-ms-version", "2012-03-01");
            request.Method = "GET";
            request.ContentType = "application/json";

            var cert = new X509Certificate2(Convert.FromBase64String(certString64Encoded));
            // Attach the certificate to the request.
            request.ClientCertificates.Add(cert);
            return request;

        }

        internal static Models.Logs GetLog(string serviceName, Result lastResult)
        {
            Models.Logs logsToReturn;

            var results = GetLog(serviceName);
            var continuationToken = string.Empty;
            while (results != null && !String.IsNullOrEmpty(results.continuationToken) && continuationToken != results.continuationToken)
            {
                GetPrivateLog(serviceName, results.continuationToken);
                continuationToken = results.continuationToken;
            }


            return GetLog(serviceName);
        }

        public static Models.Logs GetLog(string mobileServiceName)
        {
            return GetPrivateLog(mobileServiceName, null);
        }

        public static string getLogUri(string serviceName, string source, string type, string continuationToken, int top = 5)
        {
            var uriQuery = String.Format(mobileServiceLog, serviceName);

            List<String> queryParameters = new List<string>();

            queryParameters.Add("$top=" + top);

            if (!string.IsNullOrEmpty(continuationToken))
            {
                queryParameters.Add("continuationToken=" + continuationToken);
            }

            List<String> filter = new List<string>();
            if (!String.IsNullOrEmpty(type))
            {
                filter.Add("Type eq '" + type + "'");
            }
            if (!String.IsNullOrEmpty(source))
            {
                filter.Add("Source eq '" + source + "'");
            }

            if (filter.Count > 0)
            {
                string filterString = "$filter=" + filter.Aggregate((a, b) => { return a + " and " + b; });
                queryParameters.Add(filterString);
            }

            if (queryParameters.Count > 0)
            {
                uriQuery = uriQuery + "?" + queryParameters.Aggregate((a, b) => { return a + "&" + b; });
            }

            return uriQuery;
        }

        private static T getResponseRequest<T>(HttpWebRequest request) where T: class
        {
            try
            {
                // Make the call using the web request.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                
                // Parse the web response.
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);

                // Display the raw response.
                Console.WriteLine("Response output:");
                var stringResult = reader.ReadToEnd();
                var obj = JsonConvert.DeserializeObject<T>(stringResult);

                // Close the resources no longer needed.
                response.Close();
                responseStream.Close();
                reader.Close();

                return obj;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        public static List<MobileServiceManagement> GetMobileServiceInSubscription(string subscriptionId)
        {
            Uri requestUri = new Uri(GetSubscriptionUri(subscriptionId)
                + mobileServiceList);
            var request = GetRequest(requestUri, GetCertFromSubscription(subscriptionId));
            try
            {

                var response = getResponseRequest<List<MobileServiceManagement>>(request);
                return response;
            }
            catch (Exception)
            {
                return null;
            }
            
            
        }


        private static String GetCertFromSubscription(string subscriptionId)
        {
            MobileDbContext ctx = new MobileDbContext();
            var subId = Guid.Parse(subscriptionId);
            var sub = ctx.Subscriptions.Where(s => s.Id == subId).First();
            string certString64Encoded = sub.Cert;
            return certString64Encoded;
        }
        
    }
}

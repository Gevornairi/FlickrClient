using FlickrClient.Enums;
using FlickrClient.Exceptions;
using FlickrClient.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FlickrClient.FlickrConnect
{
    public static partial class FlickrResponder
    {
        private const string PostContentType = "application/x-www-form-urlencoded";

        /// <summary>
        /// Gets a data response for the given base url and parameters, 
        /// either using OAuth or not depending on which parameters were passed in.
        /// </summary>
        /// <param name="flickr">The current instance of the <see cref="Flickr"/> class.</param>
        /// <param name="baseUrl">The base url to be called.</param>
        /// <param name="parameters">A dictionary of parameters.</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static void GetDataResponseAsync(Flickr flickr, string baseUrl, Dictionary<string, string> parameters, Action<FlickrResult<string>> callback)
        {
            bool oAuth = parameters.ContainsKey("oauth_consumer_key");

            if (oAuth)
                GetDataResponseOAuthAsync(flickr, baseUrl, parameters, callback);
            else
                GetDataResponseNormalAsync(flickr, baseUrl, parameters, callback);
        }

        public static void GetDataResponseAsync(Flickr flickr, string baseUrl, Dictionary<string, string> parameters, string url, Action<FlickrResult<string>> callback)
        {
            bool oAuth = parameters.ContainsKey("oauth_consumer_key");

            if (oAuth)
                GetDataResponseOAuthAsync(flickr, baseUrl, parameters, url, callback);
            else
                GetDataResponseNormalAsync(flickr, baseUrl, parameters, callback);
        }

        private static void GetDataResponseNormalAsync(Flickr flickr, string baseUrl, Dictionary<string, string> parameters, Action<FlickrResult<string>> callback)
        {
            var method = flickr.CurrentService == SupportedService.Zooomr ? "GET" : "POST";

            var data = string.Empty;

            foreach (var k in parameters)
            {
                data += k.Key + "=" + UtilityMethods.EscapeDataString(k.Value) + "&";
            }

            if (method == "GET" && data.Length > 2000) method = "POST";

            if (method == "GET")
                DownloadDataAsync(method, baseUrl + "?" + data, null, null, null, callback);
            else
                DownloadDataAsync(method, baseUrl, data, PostContentType, null, callback);
        }

        private static void GetDataResponseOAuthAsync(Flickr flickr, string baseUrl, Dictionary<string, string> parameters, Action<FlickrResult<string>> callback)
        {
            const string method = "";

            // Remove api key if it exists.
            if (parameters.ContainsKey("api_key")) parameters.Remove("api_key");
            if (parameters.ContainsKey("api_sig")) parameters.Remove("api_sig");

            // If OAuth Access Token is set then add token and generate signature.
            if (!string.IsNullOrEmpty(flickr.OAuthAccessToken) && !parameters.ContainsKey("oauth_token"))
            {
                parameters.Add("oauth_token", flickr.OAuthAccessToken);
            }
            if (!string.IsNullOrEmpty(flickr.OAuthAccessTokenSecret) && !parameters.ContainsKey("oauth_signature"))
            {
                string sig = flickr.OAuthCalculateSignature(method, baseUrl, parameters, flickr.OAuthAccessTokenSecret);
                parameters.Add("oauth_signature", sig);
            }

            // Calculate post data, content header and auth header
            string data = OAuthCalculatePostData(parameters);
            string authHeader = OAuthCalculateAuthHeader(parameters);

            // Download data.
            try
            {
                DownloadDataAsync(method, baseUrl, data, PostContentType, authHeader, callback);
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                if (response == null) throw;

                if (response.StatusCode != HttpStatusCode.BadRequest && response.StatusCode != HttpStatusCode.Unauthorized) throw;

                using (var responseReader = new StreamReader(response.GetResponseStream()))
                {
                    string responseData = responseReader.ReadToEnd();
                    throw new OAuthException(responseData, ex);
                }
            }
        }

        private static void GetDataResponseOAuthAsync(Flickr flickr, string baseUrl, Dictionary<string, string> parameters, string url, Action<FlickrResult<string>> callback)
        {
            const string method = "";

            // Remove api key if it exists.
            if (parameters.ContainsKey("api_key")) parameters.Remove("api_key");
            if (parameters.ContainsKey("api_sig")) parameters.Remove("api_sig");

            // If OAuth Access Token is set then add token and generate signature.
            if (!string.IsNullOrEmpty(flickr.OAuthAccessToken) && !parameters.ContainsKey("oauth_token"))
            {
                parameters.Add("oauth_token", flickr.OAuthAccessToken);
            }
            if (!string.IsNullOrEmpty(flickr.OAuthAccessTokenSecret) && !parameters.ContainsKey("oauth_signature"))
            {
                string sig = flickr.OAuthCalculateSignature(method, baseUrl, parameters, flickr.OAuthAccessTokenSecret);
                parameters.Add("oauth_signature", sig);
            }

            // Calculate post data, content header and auth header
            string data = OAuthCalculatePostData(parameters);
            string authHeader = OAuthCalculateAuthHeader(parameters);

            // Download data.
            try
            {
                DownloadDataAsync(method, baseUrl, data, PostContentType, authHeader, url, callback);
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                if (response == null) throw;

                if (response.StatusCode != HttpStatusCode.BadRequest && response.StatusCode != HttpStatusCode.Unauthorized) throw;

                using (var responseReader = new StreamReader(response.GetResponseStream()))
                {
                    string responseData = responseReader.ReadToEnd();
                    throw new OAuthException(responseData, ex);
                }
            }
        }

        private static void DownloadDataAsync(string method, string baseUrl, string data, string contentType, string authHeader, Action<FlickrResult<string>> callback)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            if (!string.IsNullOrEmpty(contentType)) client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", contentType); //Keeps returning false
            if (!string.IsNullOrEmpty(authHeader)) client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authHeader);

            var result = new FlickrResult<string>();

            if (method == "POST")
            {
                HttpContent content = new StringContent(data, Encoding.UTF8);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = client.PostAsync(baseUrl, content).Result;
                string statusCode = response.StatusCode.ToString();

                response.EnsureSuccessStatusCode();
                Task<string> responseBody = response.Content.ReadAsStringAsync();
                result.Result = responseBody.Result;
                callback(result);

            }
            else
            {
                String response = client.GetStringAsync(baseUrl).Result;                
                result.Result = response;
                callback(result);
            }
        }

        private static void DownloadDataAsync(string method, string baseUrl, string data, string contentType, string authHeader, string url, Action<FlickrResult<string>> callback)
        {
            var client = new HttpClient();
            //client.DefaultRequestHeaders.Accept.Clear();
            //if (!string.IsNullOrEmpty(contentType)) client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", contentType); //Keeps returning false
            //if (!string.IsNullOrEmpty(authHeader)) client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authHeader);

            var result = new FlickrResult<string>();

            if (method == "POST")
            {
                HttpContent content = new StringContent(data, Encoding.UTF8);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = client.PostAsync(baseUrl, content).Result;
                string statusCode = response.StatusCode.ToString();

                response.EnsureSuccessStatusCode();
                Task<string> responseBody = response.Content.ReadAsStringAsync();
                result.Result = responseBody.Result;
                callback(result);

            }
            else
            {
                String response = client.GetStringAsync(url).Result;
                result.Result = response;
                callback(result);
            }
        }

        /// <summary>
        /// Returns the string for the Authorisation header to be used for OAuth authentication.
        /// Parameters other than OAuth ones are ignored.
        /// </summary>
        /// <param name="parameters">OAuth and other parameters.</param>
        /// <returns></returns>
        public static string OAuthCalculateAuthHeader(Dictionary<string, string> parameters)
        {
            var sb = new StringBuilder("OAuth ");
            foreach (KeyValuePair<string, string> pair in parameters)
            {
                if (pair.Key.StartsWith("oauth", StringComparison.Ordinal))
                {
                    sb.Append(pair.Key + "=\"" + Uri.EscapeDataString(pair.Value) + "\",");
                }
            }
            return sb.Remove(sb.Length - 1, 1).ToString();
        }

        /// <summary>
        /// Calculates for form encoded POST data to be included in the body of an OAuth call.
        /// </summary>
        /// <remarks>This will include all non-OAuth parameters. The OAuth parameter will be included in the Authentication header.</remarks>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string OAuthCalculatePostData(Dictionary<string, string> parameters)
        {
            string data = string.Empty;
            foreach (KeyValuePair<string, string> pair in parameters)
            {
                if (!pair.Key.StartsWith("oauth", StringComparison.Ordinal))
                {
                    data += pair.Key + "=" + UtilityMethods.EscapeDataString(pair.Value) + "&";
                }
            }
            return data;
        }
    }
}

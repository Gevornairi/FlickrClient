using FlickrClient.Enums;
using FlickrClient.Exceptions;
using FlickrClient.Helpers;
using FlickrClient.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FlickrClient.FlickrConnect
{

    public class Flickr
    {

        private static SupportedService defaultService = SupportedService.Flickr;

        private SupportedService service = SupportedService.Flickr;

        /// <summary>
        /// The base URL for all Flickr REST method calls.
        /// </summary>
        public Uri BaseUri
        {
            get { return baseUri[(int)service]; }
        }

        private readonly Uri[] baseUri = {
                                                     new Uri("https://api.flickr.com/services/rest/"),
                                                     new Uri("http://beta.zooomr.com/bluenote/api/rest"),
                                                     new Uri("http://www.23hq.com/services/rest/")
        };


        private string ReplaceUrl
        {
            get { return replaceUrl[(int)service]; }
        }
        private static string[] replaceUrl = {
                                                               "https://up.flickr.com/services/replace/",
                                                               "http://beta.zooomr.com/bluenote/api/replace",
                                                               "http://www.23hq.com/services/replace/"
        };

        private string AuthUrl
        {
            get { return authUrl[(int)service]; }
        }
        private static string[] authUrl = {
                                                            "https://www.flickr.com/services/auth/",
                                                            "http://beta.zooomr.com/auth/",
                                                            "http://www.23hq.com/services/auth/"
        };

        private string apiKey;
        private string apiToken;
        private string sharedSecret;
        // private int timeout = 100000;  // original value
        private int timeout = 3600000;  // (Andrew Keil) Changed to 1 hour in milliseconds to avoid timeout issues when uploading picture & videos

        private string lastRequest;
        private string lastResponse;

        /// <summary>
        /// Get or set the API Key to be used by all calls. API key is mandatory for all 
        /// calls to Flickr.
        /// </summary>
        public string ApiKey
        {
            get { return apiKey; }
            set
            {
                apiKey = value == null || value.Length == 0 ? null : value;
            }
        }

        /// <summary>
        /// API shared secret is required for all calls that require signing, which includes
        /// all methods that require authentication, as well as the actual flickr.auth.* calls.
        /// </summary>
        public string ApiSecret
        {
            get { return sharedSecret; }
            set
            {
                sharedSecret = value == null || value.Length == 0 ? null : value;
            }
        }

        /// <summary>
        /// The authentication token is required for all calls that require authentication.
        /// A <see cref="FlickrApiException"/> will be raised by Flickr if the authentication token is
        /// not set when required.
        /// </summary>
        /// <remarks>
        /// It should be noted that some methods will work without the authentication token, but
        /// will return different results if used with them (such as group pool requests, 
        /// and results which include private pictures the authenticated user is allowed to see
        /// (their own, or others).
        /// </remarks>
        [Obsolete("Use OAuthToken and OAuthTokenSecret now.")]
        public string AuthToken
        {
            get { return apiToken; }
            set
            {
                apiToken = value == null || value.Length == 0 ? null : value;
            }
        }

        /// <summary>
        /// OAuth Access Token. Needed for authenticated access using OAuth to Flickr.
        /// </summary>
        public string OAuthAccessToken { get; set; }

        /// <summary>
        /// OAuth Access Token Secret. Needed for authenticated access using OAuth to Flickr.
        /// </summary>
        public string OAuthAccessTokenSecret { get; set; }


        /// <summary>
        /// Override if the cache is disabled for this particular instance of <see cref="Flickr"/>.
        /// </summary>
        public bool InstanceCacheDisabled { get; set; }


        /// <summary>
        /// The default service to use for new Flickr instances
        /// </summary>
        public static SupportedService DefaultService
        {
            get
            {
                return defaultService;
            }
            set
            {
                defaultService = value;
            }
        }

        /// <summary>
        /// The current service that the Flickr API is using.
        /// </summary>
        public SupportedService CurrentService
        {
            get
            {
                return service;
            }
            set
            {
                service = value;
            }
        }

        /// <summary>
        /// Internal timeout for all web requests in milliseconds. Defaults to 30 seconds.
        /// </summary>
        public int HttpTimeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        /// <summary>
        /// Checks to see if a shared secret and an api token are stored in the object.
        /// Does not check if these values are valid values.
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                return sharedSecret != null && apiToken != null;
            }
        }

        /// <summary>
        /// Returns the raw XML returned from the last response.
        /// Only set it the response was not returned from cache.
        /// </summary>
        public string LastResponse
        {
            get { return lastResponse; }
        }

        /// <summary>
        /// Returns the last URL requested. Includes API signing.
        /// </summary>
        public string LastRequest
        {
            get { return lastRequest; }
        }


        /// <summary>
        /// Constructor loads configuration settings from app.config or web.config file if they exist.
        /// </summary>
        public Flickr()
        {
            CurrentService = DefaultService;
        }

        /// <summary>
        /// Create a new instance of the <see cref="Flickr"/> class with no authentication.
        /// </summary>
        /// <param name="apiKey">Your Flickr API Key.</param>
        public Flickr(string apiKey)
            : this(apiKey, null, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Flickr"/> class with an API key and a Shared Secret.
        /// This is only useful really useful for calling the Auth functions as all other
        /// authenticationed methods also require the API Token.
        /// </summary>
        /// <param name="apiKey">Your Flickr API Key.</param>
        /// <param name="sharedSecret">Your Flickr Shared Secret.</param>
        public Flickr(string apiKey, string sharedSecret)
            : this(apiKey, sharedSecret, null)
        {
        }

        /// <summary>
        /// Create a new instance of the <see cref="Flickr"/> class with the email address and password given
        /// </summary>
        /// <param name="apiKey">Your Flickr API Key</param>
        /// <param name="sharedSecret">Your FLickr Shared Secret.</param>
        /// <param name="token">The token for the user who has been authenticated.</param>
        public Flickr(string apiKey, string sharedSecret, string token)
            : this()
        {
            ApiKey = apiKey;
            ApiSecret = sharedSecret;
            AuthToken = token;
        }

        internal void CheckApiKey()
        {
            if (string.IsNullOrEmpty(ApiKey))
                throw new ApiKeyRequiredException();
        }

        internal void CheckSigned()
        {
            CheckApiKey();

            if (string.IsNullOrEmpty(ApiSecret))
                throw new SignatureRequiredException();
        }

        internal void CheckRequiresAuthentication()
        {
            CheckSigned();

            if (!string.IsNullOrEmpty(OAuthAccessToken) && !string.IsNullOrEmpty(OAuthAccessTokenSecret))
            {
                return;
            }

            if (!string.IsNullOrEmpty(AuthToken))
            {
                return;
            }

            throw new AuthenticationRequiredException();
        }

        /// <summary>
        /// Calculates the Flickr method cal URL based on the passed in parameters, and also generates the signature if required.
        /// </summary>
        /// <param name="parameters">A Dictionary containing a list of parameters to add to the method call.</param>
        /// <param name="includeSignature">Boolean use to decide whether to generate the api call signature as well.</param>
        /// <returns>The <see cref="Uri"/> for the method call.</returns>
        public string CalculateUri(Dictionary<string, string> parameters, bool includeSignature)
        {
            if (includeSignature)
            {
                string signature = CalculateAuthSignature(parameters);
                parameters.Add("api_sig", signature);
            }

            var url = new StringBuilder();
            url.Append("?");
            foreach (KeyValuePair<string, string> pair in parameters)
            {
                var escapedValue = UtilityMethods.EscapeDataString(pair.Value ?? "");
                url.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0}={1}&", pair.Key, escapedValue);
            }

            return BaseUri.AbsoluteUri + url.ToString();
        }

        private string CalculateAuthSignature(Dictionary<string, string> parameters)
        {
            var sorted = new SortedList<string, string>();
            foreach (var pair in parameters) { sorted.Add(pair.Key, pair.Value); }

            var sb = new StringBuilder(ApiSecret);
            foreach (var pair in sorted)
            {
                sb.Append(pair.Key);
                sb.Append(pair.Value);
            }
            return UtilityMethods.MD5Hash(sb.ToString());
        }

        private static Stream ConvertNonSeekableStreamToByteArray(Stream nonSeekableStream)
        {
            if (nonSeekableStream.CanSeek)
            {
                nonSeekableStream.Position = 0;
                return nonSeekableStream;
            }

            var ms = new MemoryStream();
            var buffer = new byte[1024];
            int bytes;
            while ((bytes = nonSeekableStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, bytes);
            }
            ms.Position = 0;
            return ms;
        }


        /// <summary>
        /// Gets the context of the photo in the users photostream.
        /// </summary>
        /// <param name="photoId">The ID of the photo to return the context for.</param>
        /// <param name="callback">Callback method to call upon return of the response from Flickr.</param>
        public void PhotosGetContextAsync(string photoId, Action<FlickrResult<Context>> callback)
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add("method", "flickr.photos.getContext");
            parameters.Add("photo_id", photoId);

            GetResponseAsync<Context>(parameters, callback);
        }


        /// <summary>
        /// Get information about a photo. The calling user must have permission to view the photo.
        /// </summary>
        /// <param name="photoId">The id of the photo to fetch information for.</param>
        /// <param name="callback">Callback method to call upon return of the response from Flickr.</param>
        public void PhotosGetInfoAsync(string photoId, Action<FlickrResult<PhotoInfo>> callback)
        {
            PhotosGetInfoAsync(photoId, null, callback);
        }

        /// <summary>
        /// Get information about a photo. The calling user must have permission to view the photo.
        /// </summary>
        /// <param name="photoId">The id of the photo to fetch information for.</param>
        /// <param name="secret">The secret for the photo. If the correct secret is passed then permissions checking is skipped. 
        /// This enables the 'sharing' of individual photos by passing around the id and secret.</param>
        /// <param name="callback">Callback method to call upon return of the response from Flickr.</param>
        public void PhotosGetInfoAsync(string photoId, string secret, Action<FlickrResult<PhotoInfo>> callback)
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add("method", "flickr.photos.getInfo");
            parameters.Add("photo_id", photoId);
            if (secret != null) parameters.Add("secret", secret);

            GetResponseAsync<PhotoInfo>(parameters, callback);
        }


        /// <summary>
        /// Search for a set of photos, based on the value of the <see cref="PhotoSearchOptions"/> parameters.
        /// </summary>
        /// <param name="options">The parameters to search for.</param>
        /// <param name="callback">Callback method to call upon return of the response from Flickr.</param>
        public  void PhotosSearchAsync(PhotoSearchOptions options, Action<FlickrResult<PhotoCollection>> callback)
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add("method", "flickr.photos.search");

            options.AddToDictionary(parameters);

            GetResponseAsync<PhotoCollection>(parameters, callback);
        }

        /// <summary>
        /// Returns the location data for a give photo.
        /// </summary>
        /// <param name="photoId">The ID of the photo to return the location information for.</param>
        /// <param name="callback">Callback method to call upon return of the response from Flickr.</param>
        public void PhotosGeoGetLocationAsync(string photoId, Action<FlickrResult<PlaceInfo>> callback)
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add("method", "flickr.photos.geo.getLocation");
            parameters.Add("photo_id", photoId);

            GetResponseAsync<PhotoInfo>(
                parameters,
                r =>
                {
                    var result = new FlickrResult<PlaceInfo>();
                    result.HasError = r.HasError;
                    if (result.HasError)
                    {
                        if (result.ErrorCode == 2)
                        {
                            result.HasError = false;
                            result.Result = null;
                            result.Error = null;
                        }
                        else
                        {
                            result.Error = r.Error;
                        }
                    }
                    else
                    {
                        result.Result = r.Result.Location;
                    }
                    callback(result);
                });
        }

        private void GetResponseAsync<T>(Dictionary<string, string> parameters, Action<FlickrResult<T>> callback) where T : IFlickrParsable, new()
        {
            CheckApiKey();

            parameters["api_key"] = ApiKey;

            // If performing one of the old 'flickr.auth' methods then use old authentication details.
            string method = parameters["method"];

            if (method.StartsWith("flickr.auth", StringComparison.Ordinal) && !method.EndsWith("oauth.checkToken", StringComparison.Ordinal))
            {
                if (!string.IsNullOrEmpty(AuthToken)) parameters["auth_token"] = AuthToken;
            }
            else
            {
                // If OAuth Token exists or no authentication required then use new OAuth
                if (!string.IsNullOrEmpty(OAuthAccessToken) || string.IsNullOrEmpty(AuthToken))
                {
                    OAuthGetBasicParameters(parameters);
                    if (!string.IsNullOrEmpty(OAuthAccessToken)) parameters["oauth_token"] = OAuthAccessToken;
                }
                else
                {
                    parameters["auth_token"] = AuthToken;
                }
            }


            var url = CalculateUri(parameters, !string.IsNullOrEmpty(sharedSecret));

            lastRequest = url;

            try
            {
                FlickrResponder.GetDataResponseAsync(this, BaseUri.AbsoluteUri, parameters, url, (r)
                    =>
                {
                    var result = new FlickrResult<T>();
                    if (r.HasError)
                    {
                        result.Error = r.Error;
                    }
                    else
                    {
                        try
                        {
                            lastResponse = r.Result;

                            var settings = new XmlReaderSettings();
                            settings.IgnoreWhitespace = true;
                            XmlReader reader = XmlReader.Create(new StringReader(r.Result), settings);

                            if (!reader.ReadToDescendant("rsp"))
                            {
                                throw new XmlException("Unable to find response element 'rsp' in Flickr response");
                            }
                            while (reader.MoveToNextAttribute())
                            {
                                if (reader.LocalName == "stat" && reader.Value == "fail")
                                {
                                    throw ExceptionHandler.CreateResponseException(reader);
                                }
                                continue;
                            }

                            reader.MoveToElement();
                            reader.Read();

                            var t = new T();
                            ((IFlickrParsable)t).Load(reader);
                            result.Result = t;
                            result.HasError = false;
                        }
                        catch (Exception ex)
                        {
                            result.Error = ex;
                        }
                    }

                    if (callback != null) callback(result);

                });
            }
            catch (Exception ex)
            {
                var result = new FlickrResult<T>();
                result.Error = ex;
                if (null != callback) callback(result);
            }

        }

        // <summary>
        /// Calculates the signature for an OAuth call.
        /// </summary>
        /// <param name="method">POST or GET method.</param>
        /// <param name="url">The URL the request will be sent to.</param>
        /// <param name="parameters">Parameters to be added to the signature.</param>
        /// <param name="tokenSecret">The token secret (either request or access) for generating the SHA-1 key.</param>
        /// <returns>Base64 encoded SHA-1 hash.</returns>
        public string OAuthCalculateSignature(string method, string url, Dictionary<string, string> parameters, string tokenSecret)
        {
            string baseString = "";
            string key = ApiSecret + "&" + tokenSecret;
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);

#if !SILVERLIGHT
            var sorted = new SortedList<string, string>();
            foreach (KeyValuePair<string, string> pair in parameters) { sorted.Add(pair.Key, pair.Value); }
#else
                var sorted = parameters.OrderBy(p => p.Key);
#endif

            var sb = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in sorted)
            {
                sb.Append(pair.Key);
                sb.Append("=");
                sb.Append(UtilityMethods.EscapeOAuthString(pair.Value));
                sb.Append("&");
            }

            sb.Remove(sb.Length - 1, 1);

            baseString = method + "&" + UtilityMethods.EscapeOAuthString(url) + "&" + UtilityMethods.EscapeOAuthString(sb.ToString());

            var httpHelper = new HttpHelper();

            byte[] hashBytes = HttpHelper.HmacSha1(keyBytes, System.Text.Encoding.UTF8.GetBytes(baseString)); 

            string hash = Convert.ToBase64String(hashBytes);

            return hash;
        }

        /// <summary>
        /// Returns the authorization URL for OAuth authorization, based off the request token and permissions provided.
        /// </summary>
        /// <param name="requestToken">The request token to include in the authorization url.</param>
        /// <param name="perms">The permissions being requested.</param>
        /// <returns></returns>
        public string OAuthCalculateAuthorizationUrl(string requestToken, AuthLevel perms)
        {
            return OAuthCalculateAuthorizationUrl(requestToken, perms, false);
        }

        /// <summary>
        /// Returns the authorization URL for OAuth authorization, based off the request token and permissions provided.
        /// </summary>
        /// <param name="requestToken">The request token to include in the authorization url.</param>
        /// <param name="perms">The permissions being requested.</param>
        /// <param name="mobile">Should the url be generated be the mobile one or not.</param>
        /// <returns></returns>
        public string OAuthCalculateAuthorizationUrl(string requestToken, AuthLevel perms, bool mobile)
        {
            string permsString = (perms == AuthLevel.None) ? "" : "&perms=" + UtilityMethods.AuthLevelToString(perms);

            return "https://" + (mobile ? "m" : "www") + ".flickr.com/services/oauth/authorize?oauth_token=" + requestToken + permsString;
        }

        /// <summary>
        /// Populates the given dictionary with the basic OAuth parameters, oauth_timestamp, oauth_noonce etc.
        /// </summary>
        /// <param name="parameters">Dictionary to be populated with the OAuth parameters.</param>
        private void OAuthGetBasicParameters(Dictionary<string, string> parameters)
        {
            var oAuthParameters = OAuthGetBasicParameters();
            foreach (var k in oAuthParameters)
            {
                parameters.Add(k.Key, k.Value);
            }
        }

        /// <summary>
        /// Returns a new dictionary containing the basic OAuth parameters.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> OAuthGetBasicParameters()
        {
            string oauthtimestamp = UtilityMethods.DateToUnixTimestamp(DateTime.UtcNow);
            string oauthnonce = Guid.NewGuid().ToString("N");

            var parameters = new Dictionary<string, string>();
            parameters.Add("oauth_nonce", oauthnonce);
            parameters.Add("oauth_timestamp", oauthtimestamp);
            parameters.Add("oauth_version", "1.0");
            parameters.Add("oauth_signature_method", "HMAC-SHA1");
            parameters.Add("oauth_consumer_key", ApiKey);
            return parameters;
        }

    }

}

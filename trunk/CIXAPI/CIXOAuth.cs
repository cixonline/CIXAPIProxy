using System;
using System.Web;

namespace CIXAPI
{
    public static class CIXOAuth
    {
        public static string APIServer = "http://api.cixonline.com/v1.0/";

        static public string ConsumerKey { private get; set; }

        static public string ConsumerSecret { private get; set; }

        static public string OAuthToken { private get; set; }

        static public string OAuthTokenSecret { private get; set; }

        /// <summary>
        /// Construct a URL for a given API call using the tokens stored in the settings.
        /// </summary>
        /// <param name="baseUrl">The fully qualified base URL</param>
        /// <returns>A URL string with the requested parameters</returns>
        static public string GetUri(string baseUrl)
        {
            return Uri(baseUrl, "GET", OAuthToken, OAuthTokenSecret);
        }

        /// <summary>
        /// Construct a URL for a given API call using the tokens stored in the settings.
        /// </summary>
        /// <param name="baseUrl">The fully qualified base URL</param>
        /// <returns>A URL string with the requested parameters</returns>
        static public string PostUri(string baseUrl)
        {
            return Uri(baseUrl, "POST", OAuthToken, OAuthTokenSecret);
        }

        /// <summary>
        /// Construct a URL for a given API call with the tokens returned from the last
        /// authorization or authentication call. Passing through the tokens is required in
        /// order to generate a signature that takes their values into account.
        /// </summary>
        /// <param name="baseUrl">The fully qualified base URL</param>
        /// <param name="requestType">A GET or PUT string</param>
        /// <param name="tokenString">OAuth token string</param>
        /// <param name="tokenStringSecret">OAuth token secret string</param>
        /// <returns>A URL string with the requested parameters</returns>
        static public string Uri(string baseUrl, string requestType, string tokenString, string tokenStringSecret)
        {
            string webUrl;
            string requestParam;

            string fullUrl = APIServer + baseUrl;

            OAuthBase oAuth = new OAuthBase();
            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string sig = oAuth.GenerateSignature(new Uri(fullUrl),
                ConsumerKey, ConsumerSecret,
                tokenString, tokenStringSecret,
                requestType, timeStamp, nonce,
                out webUrl,
                out requestParam);
            sig = HttpUtility.UrlEncode(sig);
            return webUrl + "?" + requestParam + "&oauth_signature=" + sig;
        }
    }
}

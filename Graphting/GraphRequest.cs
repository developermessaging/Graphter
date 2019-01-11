using System;
using System.Net;
using System.Net.Http;

namespace Graphting
{
    public class GraphRequest
    {
        private Uri _targetUri;
        private HttpMethod _httpMethod = HttpMethod.Get;  // Default to GET
        private string _requestData = String.Empty;
        private GraphResponse _response;
        private Logger _logger = null;

        public GraphRequest(Uri TargetUri)
        {
            _targetUri = TargetUri;
        }

        public GraphRequest(string TargetUri): this(new Uri(TargetUri))
        {
        }

        private void Log(string Details, string Description = "")
        {
            try
            {
                if (_logger == null) return;
                _logger.Log(Details, Description);
            }
            catch {}
        }

        private void LogCookies(CookieCollection Cookies, string Description)
        {
            // Log cookies
            try
            {
                if (Cookies.Count == 0) return;
                string sCookies = "";
                foreach (Cookie cookie in Cookies)
                {
                    sCookies += cookie.ToString() + Environment.NewLine;
                }
                Log(sCookies, Description);
            }
            catch { }
        }


    }
}

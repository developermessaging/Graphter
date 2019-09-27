using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Graphting
{
    public class GraphRequest
    {
        private Uri _targetUri;
        private HttpMethod _httpMethod = HttpMethod.Get;  // Default to GET
        private string _requestData = String.Empty;
        private string _contentType = "application/json;charset=\"utf-8\"";
        private bool _bypassWebProxy = false;
        private GraphResponse _response = null;
        private Logger _logger = null;
        private int _page; // Used to keep track of paged calls
        private Auth.OAuthHelper _oAuthHelper = null;
        private List<string[]> _httpHeaders = new List<string[]>();
        private CookieCollection _cookies = new CookieCollection();

        public GraphRequest(Uri TargetUri, Auth.OAuthHelper oAuthHelper)
        {
            _oAuthHelper = oAuthHelper;
            _targetUri = TargetUri;
        }

        public GraphRequest(string TargetUri, Auth.OAuthHelper oAuthHelper) : this(new Uri(TargetUri), oAuthHelper)
        {
        }

        public GraphRequest(string TargetUri, Auth.OAuthHelper oAuthHelper, string POSTData) : this(TargetUri, oAuthHelper)
        {
            _requestData = POSTData;
            _httpMethod = HttpMethod.Post;
        }

        public bool BypassWebProxy
        {
            get { return _bypassWebProxy; }
            set { _bypassWebProxy = value; }
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

        private void LogHeaders(WebHeaderCollection Headers, string Description, string Url = "", HttpWebResponse Response = null)
        {
            // Log request headers
            StringBuilder sHeaders = new StringBuilder();
            if (Response != null)
            {
                sHeaders.AppendLine($"{(int)Response.StatusCode} {Response.StatusDescription}");
            }
            if (!String.IsNullOrEmpty(Url))
            {
                sHeaders.AppendLine($"POST URL: {Url}");
                sHeaders.AppendLine();
            }
            try
            {
                foreach (string sHeader in Headers.AllKeys)
                {
                    sHeaders.AppendLine($"{sHeader}:{Headers[sHeader]}");
                }
                Log(sHeaders.ToString(), Description);
            }
            catch { }
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

        private HttpWebRequest CreateBasicRequest()
        {
            HttpWebRequest httpWebRequest = null;
            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(_targetUri);
            }
            catch (Exception ex)
            {
                _response = new GraphResponse(this, ex);
                return null;
            }

            httpWebRequest.UserAgent = String.Format("{1}/{0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            if (_bypassWebProxy)
                httpWebRequest.Proxy = null;

            // Set authentication
            httpWebRequest.UseDefaultCredentials = false;
            if (!String.IsNullOrEmpty(_oAuthHelper.AuthHttpHeader))
            {
                // Add authorization header
                httpWebRequest.Headers["Authorization"] = _oAuthHelper.AuthHttpHeader;
            }

            httpWebRequest.ContentType = _contentType;
            httpWebRequest.Accept = _contentType;
            if (_httpHeaders.Count > 0)
            {
                foreach (string[] header in _httpHeaders)
                {
                    try
                    {
                        if (header[0].ToLower() == "content-type")
                        {
                            httpWebRequest.ContentType = header[1];
                        }
                        else if (header[0].ToLower() == "accept")
                        {
                            httpWebRequest.Accept = header[1];
                        }
                        else
                            httpWebRequest.Headers[header[0]] = header[1];
                    }
                    catch { }
                }
                LogHeaders(httpWebRequest.Headers, "Request Headers");
            }

            httpWebRequest.CookieContainer = new CookieContainer();
            if (!(_cookies == null))
            {
                // Add cookies to the request
                foreach (Cookie cookie in _cookies)
                {
                    try
                    {
                        httpWebRequest.CookieContainer.Add(cookie);
                    }
                    catch { }
                }
                LogCookies(_cookies, "Request Cookies");
            }


            return httpWebRequest;
        }


        public GraphResponse GetResponse()
        {
            // Send the request and return the response

            if (_response != null)
                return _response; // We've already got a response

            HttpWebRequest oWebRequest = CreateBasicRequest();
            if (oWebRequest == null)
                return _response;

            if (!String.IsNullOrEmpty(_requestData))
            {
                try
                {
                    using (StreamWriter streamWriter = new StreamWriter(oWebRequest.GetRequestStream(), Encoding.UTF8))
                        streamWriter.Write(_requestData);
                }
                catch (Exception ex)
                {
                    // Failed to set request data
                    _response = new GraphResponse(this, ex);
                    return _response;
                }
                Log(_requestData, "Request Payload");
            }

            oWebRequest.Method = _httpMethod.ToString();

            oWebRequest.Expect = "";

            IAsyncResult asyncResult = oWebRequest.BeginGetResponse(null, null);
            asyncResult.AsyncWaitHandle.WaitOne();

            WebResponse oWebResponse = null;
            try
            {
                oWebResponse = oWebRequest.EndGetResponse(asyncResult);
                _response = GraphResponse.CreateFromWebResponse(this, oWebResponse);

            }
            catch (Exception ex)
            {
                _response = new GraphResponse(this, ex);
            }

            return _response;

        }
    }
}

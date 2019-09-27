using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Graphting
{
    public class GraphResponse
    {
        private bool _responseComplete = false;
        private GraphRequest _graphRequest = null;
        private string _payload = String.Empty;
        private WebHeaderCollection _httpHeaders;
        private CookieCollection _cookies = new CookieCollection();
        private Exception _error = null;
        
        public GraphResponse(GraphRequest Request)
        {
            _graphRequest = Request;
        }

        public GraphResponse(GraphRequest Request, Exception Error)
        {
            _graphRequest = Request;
            _error = Error;
            if (Error is WebException)
            {
                WebException wex = Error as WebException;
                if (!(wex.Response == null))
                {
                    using (StreamReader streamReader = new StreamReader(wex.Response.GetResponseStream()))
                        _payload = streamReader.ReadToEnd();
                    _httpHeaders = wex.Response.Headers;
                    //LogHeaders(wex.Response.Headers, "Response Headers", "", (wex.Response as HttpWebResponse));
                }
            }
        }

        public GraphResponse(GraphRequest Request, WebResponse WebResponse)
        {
            _graphRequest = Request;

            _httpHeaders = WebResponse.Headers;
            //LogHeaders(oWebResponse.Headers, "Response Headers", "", (oWebResponse as HttpWebResponse));
            try
            {
                _cookies = (WebResponse as HttpWebResponse).Cookies;
            }
            catch { }
            //LogCookies(_responseCookies, "Response Cookies");
            if (WebResponse.ContentLength>0)
            {
                using (StreamReader streamReader = new StreamReader(WebResponse.GetResponseStream()))
                    _payload = streamReader.ReadToEnd();
                //LogHeaders(wex.Response.Headers, "Response Headers", "", (wex.Response as HttpWebResponse));
            }

            try
            {
                WebResponse.Close();
            }
            catch { }
        }


        public static GraphResponse CreateFromWebResponse(GraphRequest graphRequest, WebResponse webResponse)
        {
            return new GraphResponse(graphRequest, webResponse);
        }

        public bool IsErrorResponse
        {
            get { return (_error != null); }
        }

        public Exception Exception
        {
            get { return _error; }
        }
    }
}

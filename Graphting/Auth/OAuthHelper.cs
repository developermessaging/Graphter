using System;
using System.Collections.Generic;
using System.Text;

namespace Graphting.Auth
{
    public class OAuthHelper
    {


        public string AccessToken
        {
            get { return null; }
        }

        public string AuthHttpHeader
        {
            get { return "Bearer: {0}"; }
        }
    }
}

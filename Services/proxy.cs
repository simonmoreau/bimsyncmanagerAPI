using System;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace bimsyncManagerAPI.Services
{
    
    public class MyProxy : IWebProxy
    {
        private IConfiguration Configuration { get; set; }
        public MyProxy(string proxyUri,IConfiguration configuration)
        : this(new Uri(proxyUri))
        {
            Configuration = configuration;
            ICredentials credentials = new NetworkCredential(Configuration["user"], Configuration["password"]);
            this.Credentials = credentials;
        }

        public MyProxy(Uri proxyUri)
        {
            this.ProxyUri = proxyUri;
        }

        public Uri ProxyUri { get; set; }

        public ICredentials Credentials { get; set; }

        public Uri GetProxy(Uri destination)
        {
            return this.ProxyUri;
        }

        public bool IsBypassed(Uri host)
        {
            return false; /* Proxy all requests */
        }
    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace CSharp.Utils.Web
{
    public class ReverseProxyHttpModule : AbstractDisposable, IHttpModule
    {
        #region Constants

        private const string PROXY_MARKER_HEADER_NAME = "Via";

        private const string PROXY_MARKER_HEADER_VALUE = "MMS Proxy 1.0";

        private const string REQUEST_HEADERS = "Accept,Accept-Charset,Accept-Encoding,Accept-Language,Accept-Datetime,Cache-Control,Connection,Cookie,Content-Length,Content-MD5,Content-Type,Date,Expect,From,Host,If-Match,If-Modified,If-None-Match,If-Range,If-Unmodified-Since,Max-Forwards,Proxy-Authorization,Range,TE,Upgrade,Via,Warning,Proxy-Connection";

        private const string RESPONSE_HEADERS = "Accept-Ranges,Access-Control-Allow-Origin,Age,Allow,Connection,Content-Encoding,Content-Language,Content-Length,Content-Location,Content-MD5,Content-Disposition,Content-Range,Content-Type,Date,ETag,Sever,Set-Cookie,Strict-Transport-Security,Trailer,Transfer-Encoding";

        #endregion Constants

        #region Fields

        protected string _redirectUrl = null;

        protected HashSet<string> _requestHeaders = null;

        protected string _requestUrl = null;

        protected HashSet<string> _responseHeaders = null;

        #endregion Fields

        #region Constructors and Finalizers

        public ReverseProxyHttpModule()
        {
            this._requestUrl = ConfigurationManager.AppSettings["requestUrl"];
            this._redirectUrl = ConfigurationManager.AppSettings["redirectUrl"];
            if (this._requestUrl != null)
            {
                this._requestUrl = this._requestUrl.ToUpper();
            }

            this._requestHeaders = REQUEST_HEADERS.BuildHashSet();
            this._responseHeaders = RESPONSE_HEADERS.BuildHashSet();
        }

        #endregion Constructors and Finalizers

        #region Public Methods and Operators

        public virtual void Init(HttpApplication context)
        {
            context.BeginRequest += this.context_BeginRequest;
        }

        #endregion Public Methods and Operators

        #region Methods

        protected override void Dispose(bool disposing)
        {
        }

        protected virtual void context_BeginRequest(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;
            this.processRequest(application.Context);
            application.CompleteRequest();
        }

        protected virtual void doTunnel(HttpContext context, string newUrl)
        {
            Stream requestStream = context.Request.InputStream;
            var postData = new byte[context.Request.InputStream.Length];
            requestStream.Read(postData, 0, (int)context.Request.InputStream.Length);
            var proxyRequest = (HttpWebRequest)WebRequest.Create(newUrl);

            proxyRequest.Method = context.Request.HttpMethod;
            proxyRequest.UserAgent = context.Request.UserAgent;
            if (proxyRequest.Headers == null)
            {
                proxyRequest.Headers = new WebHeaderCollection();
            }

            foreach (string headerKey in context.Request.Headers.AllKeys)
            {
                if (!this._requestHeaders.Contains(headerKey))
                {
                    try
                    {
                        proxyRequest.Headers.Add(headerKey, context.Request.Headers[headerKey]);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }

            proxyRequest.Headers[PROXY_MARKER_HEADER_NAME] = PROXY_MARKER_HEADER_VALUE;
            var proxyCookieContainer = new CookieContainer();
            proxyRequest.CookieContainer = new CookieContainer();
            proxyRequest.CookieContainer.Add(proxyCookieContainer.GetCookies(new Uri(newUrl)));
            proxyRequest.KeepAlive = false;
            if (proxyRequest.Method == "POST")
            {
                proxyRequest.ContentType = context.Request.ContentType;
                proxyRequest.ContentLength = postData.Length;
                Stream proxyRequestStream = proxyRequest.GetRequestStream();
                proxyRequestStream.Write(postData, 0, postData.Length);
                proxyRequestStream.Close();
            }

            var proxyResponse = (HttpWebResponse)proxyRequest.GetResponse();

            try
            {
                context.Response.ContentEncoding = Encoding.GetEncoding(proxyResponse.ContentEncoding);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            context.Response.ContentType = proxyResponse.ContentType;
            context.Response.StatusCode = (int)proxyResponse.StatusCode;

            if (proxyRequest.HaveResponse)
            {
                foreach (Cookie returnCookie in proxyResponse.Cookies)
                {
                    bool cookieFound = false;
                    foreach (Cookie oldCookie in proxyCookieContainer.GetCookies(new Uri(newUrl)))
                    {
                        if (returnCookie.Name.Equals(oldCookie.Name))
                        {
                            oldCookie.Value = returnCookie.Value;
                            cookieFound = true;
                        }
                    }

                    if (!cookieFound)
                    {
                        proxyCookieContainer.Add(returnCookie);
                    }
                }

                foreach (string headerkey in proxyResponse.Headers.AllKeys)
                {
                    if (!this._responseHeaders.Contains(headerkey))
                    {
                        string[] values = proxyResponse.Headers.GetValues(headerkey);
                        if (values != null)
                        {
                            foreach (string value in values)
                            {
                                try
                                {
                                    context.Response.Headers[headerkey] = value;
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex);
                                }
                            }
                        }
                    }
                }

                context.Response.Headers[PROXY_MARKER_HEADER_NAME] = ((context.Response.Headers[PROXY_MARKER_HEADER_NAME] ?? string.Empty) + "," + PROXY_MARKER_HEADER_VALUE).TrimStart(',');
            }

            Stream streamResponse = proxyResponse.GetResponseStream();
            const int responseReadBufferSize = 256;
            var responseReadBuffer = new byte[responseReadBufferSize];
            var memoryStreamResponse = new MemoryStream();

            int responseCount = streamResponse.Read(responseReadBuffer, 0, responseReadBufferSize);
            while (responseCount > 0)
            {
                memoryStreamResponse.Write(responseReadBuffer, 0, responseCount);
                responseCount = streamResponse.Read(responseReadBuffer, 0, responseReadBufferSize);
            }

            byte[] responseData = memoryStreamResponse.ToArray();
            context.Response.OutputStream.Write(responseData, 0, responseData.Length);

            memoryStreamResponse.Close();
            streamResponse.Close();
            proxyResponse.Close();
        }

        protected virtual void processRequest(HttpContext context)
        {
            if (context.Request.Headers[PROXY_MARKER_HEADER_NAME] != null && context.Request.Headers[PROXY_MARKER_HEADER_NAME].Contains(PROXY_MARKER_HEADER_VALUE))
            {
                return;
            }

            string newUrl = context.Request.Url.ToString();
            if (this._requestUrl != null)
            {
                string urlUpper = newUrl.ToUpper();
                int index = urlUpper.IndexOf(this._requestUrl, StringComparison.Ordinal);
                if (index > -1)
                {
                    newUrl = newUrl.Substring(0, index) + this._redirectUrl + newUrl.Substring(index + this._requestUrl.Length);
                }
            }

            try
            {
                this.doTunnel(context, newUrl);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                context.Response.StatusCode = 500;
            }
        }

        #endregion Methods
    }
}

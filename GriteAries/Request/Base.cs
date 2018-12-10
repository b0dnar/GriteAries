using System;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace GriteAries.Request
{
    public static class Base
    {
        public static string HttpGet(string uri)
        {
            WebRequest req = WebRequest.Create(uri);
           // req.Proxy = new System.Net.WebProxy(ProxyString, true); //true means no proxy
            WebResponse resp = req.GetResponse();
            StreamReader sr = new StreamReader(resp.GetResponseStream());
            return sr.ReadToEnd().Trim();
        }

        public static string HttpPost(string uri, string parameters)
        {
            WebRequest req = WebRequest.Create(uri);
            //req.Proxy = new System.Net.WebProxy(ProxyString, true);
            
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(parameters);
            req.ContentLength = bytes.Length;
            Stream os = req.GetRequestStream();
            os.Write(bytes, 0, bytes.Length); //Push it out there
            os.Close();
            WebResponse resp = req.GetResponse();
            if (resp == null) return null;
            StreamReader sr = new StreamReader(resp.GetResponseStream());
            return sr.ReadToEnd().Trim();
        }
    }
}
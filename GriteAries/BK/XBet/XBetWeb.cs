using System;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using GriteAries.SystemLogging;

namespace GriteAries.BK.XBet
{
    public class XBetWeb
    {
        private string UserAgent { get; set; }
        private string Accept { get; set; }
        private string AcceptEncoding { get; set; }
        private string AcceptLanguage { get; set; }

        private Logging _logging;

        public XBetWeb(Logging log, string userAgent)
        {
            _logging = log;

            UserAgent = userAgent;
            Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            AcceptEncoding = "gzip, deflate, br";
            AcceptLanguage = "uk-UA,uk;q=0.9,ru;q=0.8,en-US;q=0.7,en;q=0.6";
        }

        public async Task<string> GetPageLiveSport(string nameSport)
        {
            var request = (HttpWebRequest)WebRequest.Create($"https://1xbetua.com/en/live/{nameSport}/");

            request.Method = "GET";
            request.UserAgent = UserAgent;
            request.Accept = Accept;
            request.Referer = "https://1xbetua.com/en/";
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.Headers.Add("Accept-Encoding", AcceptEncoding);
            request.Headers.Add("Accept-Language", AcceptLanguage);
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            try
            {
                var response = await request.GetResponseAsync();

                var stream = response.GetResponseStream();
                StreamReader responseReader = new StreamReader(stream, Encoding.UTF8);
                var kodPage = await responseReader.ReadToEndAsync();

                return kodPage;
            }
            catch (Exception e)
            {
                string log = String.Format("ERROR in GetPageEvent\n{0}", e.ToString());
                await _logging.WriteLog(log);

                return null;
            }
        }

        public async Task<string> GetInfoEvent(int idEvent)
        {
            var request = (HttpWebRequest)WebRequest.Create($"https://1xbetua.com/LiveFeed/GetGameZip?id={idEvent}&lng=en&cfview=0&isSubGames=true&GroupEvents=true&countevents=250");

            request.Method = "GET";
            request.UserAgent = UserAgent;
            request.Accept = Accept;
            request.Referer = "https://1xbetua.com/en/";
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.Headers.Add("Accept-Encoding", AcceptEncoding);
            request.Headers.Add("Accept-Language", AcceptLanguage);
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            try
            {
                var response = await request.GetResponseAsync();

                var stream = response.GetResponseStream();
                StreamReader responseReader = new StreamReader(stream, Encoding.UTF8);
                var kodPage = await responseReader.ReadToEndAsync();

                return kodPage;
            }
            catch (Exception e)
            {
                string log = String.Format("ERROR in GetPageEvent\n{0}", e.ToString());
                await _logging.WriteLog(log);

                return null;
            }
        }
    }
}
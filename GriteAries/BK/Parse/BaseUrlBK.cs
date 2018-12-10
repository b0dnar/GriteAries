namespace GriteAries.BK.Parse
{
    public static class BaseUrlBK
    {
        public static string GetBaseUrl(BaseUrl baseUrl)
        {
            switch(baseUrl)
            {
                case BaseUrl.MarathonBase:
                    return "https://www.marathonbet.com";
                case BaseUrl.MarathonLive:
                    return "https://www.marathonbet.com/en/live/";
                default:
                    return "";
            }
        }

    }

    public enum BaseUrl
    {
        MarathonBase,
        MarathonLive,
        XBet
    }

}
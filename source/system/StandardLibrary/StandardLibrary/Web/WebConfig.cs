namespace StandardLibrary.Web
{
    static class WebConfig
    {
        public static string PostHeader =>
            "Content-Type: application/x-www-form-urlencoded; charset=UTF-8";

        public static string UserAgentIE =>
            "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
        public static string UserAgentChrome =>
            "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.101 Safari/537.36";
    }
}
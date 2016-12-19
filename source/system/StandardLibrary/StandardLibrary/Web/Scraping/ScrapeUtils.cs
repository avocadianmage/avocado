﻿using System;

namespace StandardLibrary.Web
{
    enum WebBrowserType
    {
        IE,
        Chrome,
    }

    static class ScrapeUtils
    {
        public static string BaseUrl(this Uri url)
        {
            return url.GetLeftPart(UriPartial.Authority);
        }

        public static string GetUserAgent(WebBrowserType browser)
        {
            switch (browser)
            {
                case WebBrowserType.IE: return 
                    "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";

                case WebBrowserType.Chrome: return
                    "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.101 Safari/537.36";

                default: throw new NotSupportedException(
                    "The specified browser is not supported.");
            }
        }
    }
}
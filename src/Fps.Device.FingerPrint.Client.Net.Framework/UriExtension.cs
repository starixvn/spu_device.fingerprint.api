using System;

namespace Fps.Device.FingerPrint.Client.Net.Framework
{
    public static class UriExtension
    {
        public static Uri Combine(this Uri baseUri, string relativeUrl)
        {
            if (baseUri == null)
                throw new ArgumentNullException(nameof(baseUri));

            return new Uri(UrlCombine.Combine(baseUri.AbsoluteUri, relativeUrl));
        }

        public static Uri Combine(this Uri baseUri, params string[] relativePaths)
        {
            if (baseUri == null)
                throw new ArgumentNullException(nameof(baseUri));

            return new Uri(UrlCombine.Combine(baseUri.AbsoluteUri, relativePaths));
        }
    }
}

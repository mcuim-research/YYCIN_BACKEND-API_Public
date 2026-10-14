namespace test3.Configuration;

public static class PublicUrls
{
    public static Uri Create(IConfiguration configuration, string relativePath)
    {
        var configuredUrl = configuration["App:PublicBaseUrl"];
        if (!Uri.TryCreate(configuredUrl, UriKind.Absolute, out var baseUri) ||
            (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps) ||
            !string.IsNullOrEmpty(baseUri.UserInfo) ||
            !string.IsNullOrEmpty(baseUri.Query) || !string.IsNullOrEmpty(baseUri.Fragment))
        {
            throw new InvalidOperationException(
                "Set App:PublicBaseUrl to the public HTTP(S) application URL before sending mail.");
        }

        return new Uri(new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/"), relativePath);
    }
}

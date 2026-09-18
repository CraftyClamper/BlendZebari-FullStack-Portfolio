using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BookmarkManager.Server
{
    public class MetadataScraper
    {
        private readonly HttpClient _httpClient;

        public MetadataScraper(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }

        public async Task<string> ExtractSiteSummaryAsync(string url)
        {
            try
            {
                var htmlContent = await _httpClient.GetStringAsync(url);

                // 1. YouTube Deep Parse Guard: If it's a YouTube link, snatch the raw unstructured description array block
                if (url.Contains("youtube.com") || url.Contains("youtu.be"))
                {
                    // Looks for the long-form raw text block inside YouTube's inner script configurations
                    var ytDescriptionRegex = new Regex("\"shortDescription\":\"([^\"]*)\"", RegexOptions.IgnoreCase);
                    var ytMatch = ytDescriptionRegex.Match(htmlContent);

                    if (ytMatch.Success && !string.IsNullOrWhiteSpace(ytMatch.Groups[1].Value))
                    {
                        // Clean out the raw unicode newline text formatting blocks (\n, \r)
                        string cleanedText = ytMatch.Groups[1].Value
                            .Replace("\\n", "\n")
                            .Replace("\\r", "\r")
                            .Replace("\\\"", "\"");

                        return System.Net.WebUtility.HtmlDecode(cleanedText.Trim());
                    }
                }

                // 2. UNIVERSAL DEEP PARSE: Look for full structured JSON-LD data schema blocks
                var jsonLdRegex = new Regex("<script[^>]*type=[\"']application/ld\\+json[\"'][^>]*>([\\s\\S]*?)</script>", RegexOptions.IgnoreCase);
                var jsonMatches = jsonLdRegex.Matches(htmlContent);
                foreach (Match jsonMatch in jsonMatches)
                {
                    var descPattern = new Regex("\"description\"\\s*:\\s*\"([^\"]*)\"", RegexOptions.IgnoreCase);
                    var descMatch = descPattern.Match(jsonMatch.Groups[1].Value);
                    if (descMatch.Success && !string.IsNullOrWhiteSpace(descMatch.Groups[1].Value))
                    {
                        return System.Net.WebUtility.HtmlDecode(descMatch.Groups[1].Value.Trim());
                    }
                }

                // 3. BASELINE FALLBACKS: Used for standard websites
                var ogRegex = new Regex("<meta[^>]*property=[\"']og:description[\"'][^>]*content=[\"']([^\"']*)[\"']", RegexOptions.IgnoreCase);
                var ogMatch = ogRegex.Match(htmlContent);
                if (ogMatch.Success && !string.IsNullOrWhiteSpace(ogMatch.Groups[1].Value))
                {
                    return System.Net.WebUtility.HtmlDecode(ogMatch.Groups[1].Value.Trim());
                }

                var standardRegex = new Regex("<meta[^>]*name=[\"']description[\"'][^>]*content=[\"']([^\"']*)[\"']", RegexOptions.IgnoreCase);
                var standardMatch = standardRegex.Match(htmlContent);
                if (standardMatch.Success && !string.IsNullOrWhiteSpace(standardMatch.Groups[1].Value))
                {
                    return System.Net.WebUtility.HtmlDecode(standardMatch.Groups[1].Value.Trim());
                }

                return "No public description metadata provided by this webpage. Click 'Open Site' to browse its layout.";
            }
            catch (Exception)
            {
                return "Unable to parse web summary headers. Site content may be private or protected behind a server firewall.";
            }
        }
    }
}

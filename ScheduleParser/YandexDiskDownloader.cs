using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace ScheduleParser;

public static class YandexDiskDownloader
{
    private struct Response
    {
        [JsonPropertyName("href")] public required string Href { get; set; }
    }

    /// <summary>
    /// Downloads a public file from Yandex.Disk.
    /// </summary>
    /// <returns>Stream to open a file.</returns>
    public static async Task<Stream> DownloadFile(string fileUrl)
    {
        var uriBuilder = new UriBuilder("https://cloud-api.yandex.net/v1/disk/public/resources/download");
        var parameters = HttpUtility.ParseQueryString(string.Empty);
        parameters["public_key"] = fileUrl;
        uriBuilder.Query = parameters.ToString();

        using var client = new HttpClient();
        using var response = await client.GetAsync(uriBuilder.Uri);

        if (!response.IsSuccessStatusCode)
        {
            throw new ArgumentException($"Неверная ссылка: {fileUrl}");
        }
        
        var body = JsonSerializer.Deserialize<Response>(await response.Content.ReadAsStreamAsync());
        var responseUri = body.Href;

        return await client.GetStreamAsync(responseUri);
    }

}
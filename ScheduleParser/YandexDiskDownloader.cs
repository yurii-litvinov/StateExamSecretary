// <copyright file="YandexDiskDownloader.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ScheduleParser;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

/// <summary>
/// Class for downloading a file from Yandex.Disk.
/// </summary>
public static class YandexDiskDownloader
{
    /// <summary>
    /// Downloads a public file from Yandex.Disk.
    /// </summary>
    /// <param name="fileUrl">File reference.</param>
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

    private struct Response
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }
    }
}
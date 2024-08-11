// <copyright file="YandexDiskWorker.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace StateExamSecretaryEngine;

using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Web;
using YandexDisk.Client;
using YandexDisk.Client.Http;
using YandexDisk.Client.Protocol;

/// <summary>
/// Class for working with Yandex.Disk.
/// </summary>
public class YandexDiskWorker
{
    private string? token;
    private IDiskApi? diskApi;

    /// <summary>
    /// Uploads a file to disk using the specified path.
    /// </summary>
    /// <param name="stream">File as a stream.</param>
    /// <param name="path">Absolute path with the file name.</param>
    public async Task UploadFile(Stream stream, string path)
    {
        if (this.diskApi != null)
        {
            var link = await this.diskApi.Files.GetUploadLinkAsync($"/{path}", true);
            await this.diskApi.Files.UploadAsync(link, stream);
        }
    }

    /// <summary>
    /// Creates a folder on a disk.
    /// </summary>
    /// <param name="path">Absolute path with the folder name.</param>
    public async Task CreateFolder(string path)
    {
        if (this.diskApi != null)
        {
            var folderPath = "/";
            var folderName = path;
            var end = path.LastIndexOf('/');
            if (end != -1)
            {
                folderPath += path[.. end];
                folderName = path[(end + 1) ..];
            }

            var folderData = await this.diskApi.MetaInfo.GetInfoAsync(
                new ResourceRequest
                {
                    Path = folderPath,
                });

            if (!folderData.Embedded.Items.Any(item => item.Type == ResourceType.Dir && item.Name.Equals(folderName)))
            {
                await this.diskApi.Commands.CreateDictionaryAsync($"/{path}");
            }
        }
    }

    /// <summary>
    /// Gets information about a folder on the disk.
    /// </summary>
    /// <param name="path">Absolute path with the folder name.</param>
    public async Task<Resource> GetFolder(string path)
    {
        if (this.diskApi != null)
        {
            return await this.diskApi.MetaInfo.GetInfoAsync(
                new ResourceRequest
                {
                    Path = $"{path}",
                });
        }

        throw new UnauthorizedAccessException("Не удалось получить доступ к папке.");
    }

    /// <summary>
    /// Gets an OAuth token to work with the disk.
    /// </summary>
    public async Task GetToken(string clientId, string clientSecret, string redirectUri)
    {
        var code = await this.GetConfirmationCode(clientId, redirectUri);
        this.token = await this.ExchangeCodeForToken(
            clientId,
            clientSecret,
            code ?? throw new InvalidOperationException("Неверный код подтверждения."));

        this.diskApi = new DiskHttpApi(this.token);
    }

    private async Task<string?> GetConfirmationCode(string clientId, string redirectUri)
    {
        var codeUrl = $"https://oauth.yandex.ru/authorize?response_type=code&client_id={clientId}";

        Process.Start(
            new ProcessStartInfo
            {
                FileName = codeUrl,
                UseShellExecute = true,
            });

        using var listener = new HttpListener();
        listener.Prefixes.Add(redirectUri);
        listener.Start();

        var context = await listener.GetContextAsync();

        if (context.Request.Url == null)
        {
            throw new HttpRequestException("Произошла ошибка при попытке получить код подтверждения.");
        }

        var query = context.Request.Url.Query;
        var parameters = HttpUtility.ParseQueryString(query);
        var code = parameters["code"];

        await using (var writer = new StreamWriter(context.Response.OutputStream))
        {
            context.Response.ContentType = "text/plain; charset=utf-8";
            await writer.WriteAsync("Код успешно получен! Вы можете закрыть это окно.");
        }

        listener.Stop();

        return code;
    }

    private async Task<string?> ExchangeCodeForToken(string clientId, string clientSecret, string code)
    {
        const string tokenUrl = "https://oauth.yandex.ru/token";

        using var httpClient = new HttpClient();
        var parameters = new FormUrlEncodedContent(
            new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
            });

        var response = await httpClient.PostAsync(tokenUrl, parameters);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(jsonResponse);
        return doc.RootElement.TryGetProperty("access_token", out var tokenElement) ? tokenElement.GetString() : null;
    }
}
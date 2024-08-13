// <copyright file="DiskWorker.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace StateExamSecretaryEngine;

/// <summary>
/// Interface for working with the disk.
/// </summary>
public interface IDiskWorker
{
    /// <summary>
    /// Uploads a file to disk using the specified path.
    /// </summary>
    /// <param name="stream">File as a stream.</param>
    /// <param name="path">Absolute path with the file name.</param>
    Task UploadFile(Stream stream, string path);

    /// <summary>
    /// Creates a folder on a disk.
    /// </summary>
    /// <param name="path">Absolute path with the folder name.</param>
    Task CreateFolder(string path);

    /// <summary>
    /// Gets the file paths of the specified folder.
    /// </summary>
    /// <param name="path">Absolute path with the folder name.</param>
    Task<List<string>> GetFolderFiles(string path);

    /// <summary>
    /// Gets an OAuth token to work with the disk.
    /// </summary>
    Task GetOAuthToken(string clientId, string clientSecret, string redirectUri);
}
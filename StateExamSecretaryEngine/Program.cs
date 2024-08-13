// <copyright file="Program.cs" company="Maria Myasnikova">
// Copyright (c) Maria Myasnikova. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

#pragma warning disable SA1200
using StateExamSecretaryEngine;

#pragma warning restore SA1200

var engine = new Engine();

if (engine.TryCreateConfig())
{
    Console.WriteLine(
        "В рабочей директории была создана папка Config. " +
        "Перейдите в неё, настройте файл config.json и перезапустите программу.");
    return;
}

try
{
    if (engine.CheckDiskParameters())
    {
        Console.WriteLine("Создание папки на Яндекс.Диске...");
        await engine.CreateMainFolder();
    }

    Console.WriteLine("Парсинг расписания...");
    engine.ParseSchedule();

    Console.WriteLine("Генерация порядков дня...");
    await engine.GenerateDayOrders();

    if (engine.CheckUploadFilesToDisk())
    {
        Console.WriteLine("Загрузка файлов на Яндекс.Диск...");
        await engine.UploadFilesToDisk();

        Console.WriteLine("Проверка наличия файлов...\n");
        var report = engine.CheckForFilesOnDisk();
        if (report != string.Empty)
        {
            Console.WriteLine("⚠️ Не хватает некоторых файлов\n");
            Console.WriteLine(report);
        }
        else
        {
            Console.WriteLine("✅ Все необходимые файлы на месте");
        }
    }
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}
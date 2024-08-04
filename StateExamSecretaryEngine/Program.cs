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
    Console.WriteLine("Парсинг расписания...");
    engine.ParseSchedule();
    Console.WriteLine("Готово!\n");

    Console.WriteLine("Генерация порядков дня...");
    engine.GenerateDayOrders();
    Console.WriteLine("Готово!");
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}
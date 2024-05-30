namespace ScheduleParserTest;

using System.Text.Json;
using ScheduleParser.Config;
using ScheduleParser.Models;
using FluentAssertions;

[TestFixture]
public class Tests
{
    private List<DaySchedule> expectedDays = [];
    private Config? config;

    [OneTimeSetUp]
    public void Setup()
    {
        GetExpectedData();

        var projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        Directory.SetCurrentDirectory(projectDirectory ?? throw new InvalidOperationException());

        var jsonString = File.ReadAllText("test.json");
        config = JsonSerializer.Deserialize<Config>(jsonString);
    }

    private void GetExpectedData()
    {
        var studentWorks = new List<StudentWork>
        {
            new(1, "Власов Илья Максимович",
                "Разработка платформы для комплексного анализа использования языков программирования",
                "Михайлова Елена Георгиевна", "Мазур Юрий Александрович"),
            new(2, "Говорова Диана Игоревна",
                "Система анализа двигательной активности по показаниям датчиков", "Графеева Наталья Генриховна",
                "Егорова Ольга Борисовна"),
            new(1, "Панарин Павел Михайлович",
                "Кластеризация случаев медицинского обслуживания для формирования шаблонов медицинской документации",
                "Сысоев Сергей Сергеевич", "Велюхов Юрий Геннадьевич"),
            new(2, "Плужникова Анастасия Дмитриевна",
                "Автоматическая генерация заданий на основе регулярных выражений", "Графеева Наталья Генриховна",
                "Егорова Ольга Борисовна"),
            new(1, "Бабич Никита Викторович",
                "Логистический портал Cargotime: разработка платформы для взаимодействия клиентов и логистических компаний",
                "Абрамов Максим Викторович", "Захаров Валерий Вячеславович"),
            new(2, "Вяткин Артём Андреевич",
                "Алгебраические байесовские сети: логико-вероятностный вывод с использованием третичной структуры и подходы к применению",
                "Абрамов Максим Викторович", "Фильченков Андрей Александрович")
        };

        studentWorks[0].Consultant = "Спирин Егор Сергеевич, программист-разработчик, ООО “В Контакте”";
        studentWorks[1].Consultant = string.Empty;
        studentWorks[2].Consultant = string.Empty;
        studentWorks[3].Consultant = string.Empty;
        studentWorks[4].Consultant = "Анастасия Андреевна Корепанова, аспирант СПбГУ";
        studentWorks[5].Consultant = "Харитонов Никита Алексеевич, аспирант СПбГУ";

        var meetings = new List<CommissionMeeting>
        {
            new("11:00, ауд. 3381", "ИАС, бакалавры техпрога, ГЭК 5006-02",
                [studentWorks[0], studentWorks[1]]),
            new("16:00, ауд. 3381", "ИАС, бакалавры техпрога, ГЭК 5006-03",
                [studentWorks[2], studentWorks[3]]),
            new("10:00, ауд. 3381", "Информатика/ПА, бакалавры техпрога, ГЭК 5006-04",
                [studentWorks[4], studentWorks[5]])
        };

        var members1 = new List<string>
        {
            "Председатель: Лисс Александр Рудольфович",
            "Секретарь: Хохулина Виктория Александровна",
            "Луцив Дмитрий Вадимович",
            "Назаренко Артём Александрович",
            "Чижова Ангелина Сергеевна",
            "Самарин Алексей Владимирович"
        };
        var members2 = new List<string>
        {
            "Председатель: Лисс Александр Рудольфович",
            "Секретарь: Хохулина Виктория Александровна",
            "Ковалев Владимир Сергеевич",
            "Пащенко Антон Евгеньевич",
            "Раковская Юлия Александровна",
            "Тесля Николай Николаевич",
            "Яковлев Павел Юрьевич"
        };

        expectedDays.Add(new DaySchedule("26 мая", members1, [meetings[0], meetings[1]]));
        expectedDays.Add(new DaySchedule("29 мая", members2, [meetings[2]]));
    }

    [Test]
    public void ParserTest()
    {
        var parser = new ScheduleParser.ScheduleParser(config ?? throw new InvalidOperationException());
        var days = parser.Parse();
        days.Should().BeEquivalentTo(expectedDays);
    }
}
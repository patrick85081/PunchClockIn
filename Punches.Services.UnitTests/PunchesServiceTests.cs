using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Punches.Models;
using Punches.Services.GoogleSheet;

namespace Punches.Services.UnitTests;

public class PunchesServiceTests
{
    private PunchSheetService punchSheetService;
    private ISpreadsheetsApi? spreadsheetsApi;

    [SetUp]
    public void Setup()
    {
        spreadsheetsApi = Substitute.For<ISpreadsheetsApi>();
        var spreadsheetsServiceFactory = Substitute.For<ISpreadsheetsServiceFactory>();
        spreadsheetsServiceFactory.GetSpreadsheetsRepository(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(spreadsheetsApi));
        
        punchSheetService = new PunchSheetService(
            spreadsheetsServiceFactory,
            Substitute.For<ILogger<PunchSheetService>>(),
            new GoogleSheetConfig()
        );
    }

    [Test]
    public async Task GetEmployee_Test()
    {
        spreadsheetsApi.GetSheetValue(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<IList<object>>>(
                new List<IList<object>>()
                {
                    new List<object>() { "老闆", "Boss" },
                    new List<object>() { "管理部" },
                    new List<object>() { "小白", "White" },
                    new List<object>() { "工程部" },
                    new List<object>() { "攻城獅", "AttachLions" },
                    new List<object>() { "小明", "Today" },
                }));
        var actual = (await punchSheetService.GetEmployee(CancellationToken.None));
        
        actual
            .Should()
            .BeEquivalentTo(new List<Employee>()
            {
                new Employee(){ Index = 1, ChineseName = "老闆", EnglishName = "Boss", Department = null},
                new Employee(){ Index = 3, ChineseName = "小白", EnglishName = "White", Department = "管理部"},
                new Employee(){ Index = 5, ChineseName = "攻城獅", EnglishName = "AttachLions", Department = "工程部"},
                new Employee(){ Index = 6, ChineseName = "小明", EnglishName = "Today", Department = "工程部"},
            });
        Assert.Pass();
    }

    [Test]
    public async Task GetTitleAndMonth_Test()
    {
        spreadsheetsApi.GetSpreadsheetsInfo(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SpreadsheetsInfo>(
                new SpreadsheetsInfo()
                {
                    Title = "這是標題",
                    SheetProperties = new []
                    {
                        new SheetProperty(){ Title = "11102", SheetId = 24680},
                        new SheetProperty(){ Title = "11103", SheetId = 24681}
                    }
                }
));
        var actual = (await punchSheetService.GetTitleAndMonth(CancellationToken.None));
        
        actual.title.Should().Be("這是標題");
        actual.month
            .Should()
            .BeEquivalentTo(new List<ClockMonth>()
            {
                new ClockMonth("11102", 24680),
                new ClockMonth("11103", 24681),
            });
        Assert.Pass();
    }
}
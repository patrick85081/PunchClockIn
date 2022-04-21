using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
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
        SetupGetSheetValueReturn(new List<IList<object>>()
        {
            new List<object>() { "老闆", "Boss" },
            new List<object>() { "管理部" },
            new List<object>() { "小白", "White" },
            new List<object>() { "工程部" },
            new List<object>() { "攻城獅", "AttachLions" },
            new List<object>() { "小明", "Today" },
        });
        
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
    }

    [Test]
    public async Task GetTitleAndMonth_Test()
    {
        SetupGetSpreadsheetsInfoReturn(new SpreadsheetsInfo()
        {
            Title = "這是標題",
            SheetProperties = new[]
            {
                new SheetProperty() { Title = "11102", SheetId = 24680 },
                new SheetProperty() { Title = "11103", SheetId = 24681 }
            }
        });
        
        var actual = (await punchSheetService.GetTitleAndMonth(CancellationToken.None));
        
        actual.title.Should().Be("這是標題");
        actual.month
            .Should()
            .BeEquivalentTo(new List<ClockMonth>()
            {
                new ClockMonth("11102", 24680),
                new ClockMonth("11103", 24681),
            });
    }

    [Test]
    public async Task QueryMonth_Test()
    {
        SetupGetSheetValueReturn(
            new List<IList<object>>()
            {
                new List<object>() { "2022/2/24", "業務部", "小明 Min", "08:29", "18:30", null, "公司" },
                new List<object>() { "2022/2/25", "業務部", "小明 Min", null, null, null, null, "特休", "08:30~12:00" },
                new List<object>() { "2022/2/26", "業務部", "小明 Min", null, null, null, null, }
            }
        );
        
        var actual = (await punchSheetService.QueryMonthAsync("11102", CancellationToken.None));
        
        actual.Should()
            .BeEquivalentTo(new List<ClockIn>()
            {
                new ClockIn()
                {
                    RowNumber = 2,
                    Id = "1110224小明 Min",
                    Date = new DateTime(2022, 2, 24), 
                    Department = "業務部", 
                    Name = "小明 Min", 
                    WorkOn = new TimeSpan(08, 29, 0),
                    WorkOff = new TimeSpan(18, 30, 0),
                    Location = "公司"
                },
                new ClockIn()
                {
                    RowNumber = 3,
                    Id = "1110225小明 Min",
                    Date = new DateTime(2022, 2, 25), 
                    Department = "業務部", 
                    Name = "小明 Min", 
                    WorkOn = null,
                    WorkOff = null,
                    Location = null,
                    Status = "特休",
                    Reason = "08:30~12:00"
                },
                new ClockIn()
                {
                    RowNumber = 4,
                    Id = "1110226小明 Min",
                    Date = new DateTime(2022, 2, 26), 
                    Department = "業務部", 
                    Name = "小明 Min", 
                    WorkOn = null,
                    WorkOff = null,
                    Location = null,
                },
            });
    }

    [Test]
    public async Task WriteWorkOnTime_SheetNoData_Test()
    {
        SetupGetSheetValueReturn(
            new List<IList<object>>()
            {
                new List<object>() { "2022/2/24", "業務部", "小明 Min", "08:29", "18:30", null, "公司" },
                new List<object>() { "2022/2/25", "業務部", "小明 Min", null, null, null, null, "特休", "08:30~12:00" },
                new List<object>() { "2022/2/26", "業務部", "小明 Min", null, null, null, null, }
            }
        );

        await punchSheetService.WriteWorkOnTime(
            new DateTime(2022, 2, 27),
            "業務部", "小明 Min",
            new TimeSpan(08, 23, 0), "公司", "Test");

        spreadsheetsApi.Received(1)
            .AppendRequest(Arg.Any<string>(), Arg.Any<string>(),
                "2022/2/27", "業務部", "小明 Min", "08:23");
        spreadsheetsApi.Received(1)
            .WriteRequest(Arg.Any<string>(), Arg.Any<string>(), "公司", "", "Test");
    }

    [Test]
    public async Task WriteWorkOnTime_SheetHaveData_Test()
    {
        SetupGetSheetValueReturn(
            new List<IList<object>>()
            {
                new List<object>() { "2022/2/24", "業務部", "小明 Min", "08:29", "18:30", null, "公司" },
                new List<object>() { "2022/2/25", "業務部", "小明 Min", null, null, null, null, "特休", "08:30~12:00" },
                new List<object>() { "2022/2/26", "業務部", "小明 Min", null, null, null, null, }
            }
        );

        var action = () => punchSheetService.WriteWorkOnTime(
                new DateTime(2022, 2, 26),
                "業務部", "小明 Min",
                new TimeSpan(08, 23, 0), "公司", "")
            .GetAwaiter()
            .GetResult();

        spreadsheetsApi.DidNotReceive()
            .AppendRequest(Arg.Any<string>(), Arg.Any<string>(), data: Arg.Any<object[]>());
        spreadsheetsApi.DidNotReceive()
            .WriteRequest(Arg.Any<string>(), Arg.Any<string>(), data: Arg.Any<object[]>());
        action.Should()
            .Throw<Exception>()
            .WithMessage("2022/02/26 小明 Min already Work On");
    }

    [Test]
    public async Task WriteWorkOffTime_SheetNoData_Test()
    {
        SetupGetSheetValueReturn(
            new List<IList<object>>()
            {
                new List<object>() { "2022/2/24", "業務部", "小明 Min", "08:29", "18:30", null, "公司" },
                new List<object>() { "2022/2/25", "業務部", "小明 Min", null, null, null, null, "特休", "08:30~12:00" },
                new List<object>() { "2022/2/26", "業務部", "小明 Min", null, null, null, null, }
            }
        );

        var action = () => punchSheetService.WriteWorkOffTime(
                new DateTime(2022, 2, 27),
                "小明 Min",
                new TimeSpan(18, 23, 0))
            .GetAwaiter()
            .GetResult();

        spreadsheetsApi.DidNotReceive()
            .AppendRequest(Arg.Any<string>(), Arg.Any<string>(), data: Arg.Any<object[]>());
        spreadsheetsApi.DidNotReceive()
            .WriteRequest(Arg.Any<string>(), Arg.Any<string>(), data: Arg.Any<object[]>());
        action.Should()
            .Throw<Exception>()
            .WithMessage("2022/02/27 小明 Min Clock In is not found");
    }

    [Test]
    public async Task WriteWorkOffTime_SheetHaveData_Test()
    {
        SetupGetSheetValueReturn(
            new List<IList<object>>()
            {
                new List<object>() { "2022/2/24", "業務部", "小明 Min", "08:29", "18:30", null, "公司" },
                new List<object>() { "2022/2/25", "業務部", "小明 Min", null, null, null, null, "特休", "08:30~12:00" },
                new List<object>() { "2022/2/26", "業務部", "小明 Min", null, null, null, null, }
            }
        );

        await punchSheetService.WriteWorkOffTime(
            new DateTime(2022, 2, 24),
            "小明 Min",
            new TimeSpan(18, 23, 0));

        spreadsheetsApi.Received(1)
            .WriteRequest(Arg.Any<string>(), Arg.Any<string>(),
                "18:23");
    }

    private void SetupGetSpreadsheetsInfoReturn(SpreadsheetsInfo spreadsheetsInfo)
    {
        spreadsheetsApi.GetSpreadsheetsInfo(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SpreadsheetsInfo>(
                spreadsheetsInfo
            ));
    }

    private void SetupGetSheetValueReturn(List<IList<object>> result)
    {
        spreadsheetsApi.GetSheetValue(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IList<IList<object>>>(
                result));
    }
}
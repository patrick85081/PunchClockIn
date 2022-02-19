using System.Reactive.Linq;
using System.Text;
using Punches.Models;
using Punches.Repository.Extensions;

namespace Punches.Repository
{
    public class HolidayRepository : IHolidayRepository
    {
        // private static readonly string url = "https://data.ntpc.gov.tw/api/datasets/308DCD75-6434-45BC-A95F-584DA4FED251/csv/file";
        static SemaphoreSlim _lock = new(1, 1);
        static HolidayRepository()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private readonly DataContext dataContext;
        private static Dictionary<DateTime, Holiday> holidayMap = null;


        public static bool HasData => holidayMap != null;
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public HolidayRepository(DataContext dataContext)
        {
            this.dataContext = dataContext;
            _ = Init();
        }

        public async Task Init()
        {
            try
            {
                await _lock.WaitAsync();
                var count = dataContext.Holiday.Count(h => h.Id.StartsWith(DateTime.Today.Year.ToString()));
                if (count == 0)
                {
                    if (DateTime.Today.Year == 2022)
                    {
                        var year111 = 
                            "https://www.dgpa.gov.tw/FileConversion?filename=dgpa/files/202106/9112cdab-5954-4dfc-b476-2e38009ad039.csv&nfix=&name=111年中華民國政府行政機關辦公日曆表.csv";
                        var holiday = await GetHolidayFormNetwork(year111);
                        dataContext.Holiday.Upsert(holiday);
                    }
                    if (DateTime.Today.Year == 2021)
                    {
                        var year110 =
                            "https://www.dgpa.gov.tw/FileConversion?filename=dgpa/files/202007/08c0add8-7b90-4504-8de5-829e8240a282.csv&nfix=&name=110中華民國政府行政機關辦公日曆表.csv";
                        var holiday = await GetHolidayFormNetwork(year110);
                        dataContext.Holiday.Upsert(holiday);
                    }
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public bool IsHoliday(DateTime date)
        {
            var day = dataContext.Holiday.FindById(date.ToString("yyyyMMdd"));
            if (day != null)
            {
                return day.IsHoliday;
            }
            else
            {
                return date is { DayOfWeek: DayOfWeek.Saturday or DayOfWeek.Sunday };
            }
        }
        public Holiday GetHoliday(DateTime date)
        {
            var day = dataContext.Holiday.FindById(date.ToString("yyyyMMdd"));
            return day;
        }

        private static async Task<List<Holiday>> GetHolidayFormNetwork(string url)
        {
            // var holidayCsv = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Holiday.csv");
            var httpClient = new HttpClient();
            // var uri = new Uri(HttpUtility.HtmlEncode(url));
            var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers = {{"User-Agent", "PostmanRuntime/7.28.4"}, {"Accept", "*/*"}}
            });

            var stream = await response.Content.ReadAsStreamAsync();
            var list = await Observable.Using(
                    () => new StreamReader(stream, Encoding.GetEncoding("big5")),
                    sr => sr.ToObservable())
                .Skip(1)
                .Select(x => x.Split(','))
                .Select(column => new Holiday()
                {
                    Id = column.ElementAtOrDefault(0),
                    Date = DateTime.ParseExact(column.ElementAtOrDefault(0), "yyyyMMdd", null),
                    Name = column.ElementAtOrDefault(3),
                    IsHoliday = column.ElementAtOrDefault(2) == "2",
                })
                .ToList();
            return list.ToList();
        }

    }
}

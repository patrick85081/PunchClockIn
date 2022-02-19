using Punches.Models;

namespace Punches.Repository;

public class ClockInRepository : IClockInRepository
{
    private readonly DataContext dataContext;

    public ClockInRepository(DataContext dataContext)
    {
        this.dataContext = dataContext;
    }

    public IEnumerable<ClockIn> GetMonth(string month) =>
        dataContext.ClockIn
            .Find(clock => clock.Id.StartsWith(month))
            .OrderBy(clock => clock.Date);

    public bool HasMonth(string month)
    {
        return dataContext.ClockIn
            .Count(clockIn => clockIn.Id.StartsWith(month)) > 0;
    }

    public IEnumerable<ClockIn> Get(QueryParameter query) =>
        Get(query.Month, query.Name);

    public IEnumerable<ClockIn> Get(string month, string name)
    {
        return dataContext.ClockIn
            .Find(clock => clock.Id.StartsWith(month) && clock.Id.EndsWith(name))
            .OrderBy(clock => clock.Date);
    }

    public ClockIn GetByDate(DateTime date, string name)
    {
        var key = date.ToClockInMonth();
        var clockIn = dataContext.ClockIn
            .FindOne(clock => clock.Date == date.Date && clock.Name == name);
        return clockIn;
    }

    public void Set(IEnumerable<ClockIn> data)
    {
        dataContext.ClockIn.Upsert(data);
    }
}
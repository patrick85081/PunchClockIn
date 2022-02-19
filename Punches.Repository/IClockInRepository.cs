using Punches.Models;

namespace Punches.Repository;

public interface IClockInRepository
{
    IEnumerable<ClockIn> Get(string month, string name);
    void Set(IEnumerable<ClockIn> data);
    IEnumerable<ClockIn> Get(QueryParameter query);
    ClockIn GetByDate(DateTime date, string name);
    IEnumerable<ClockIn> GetMonth(string month);
    bool HasMonth(string month);
}
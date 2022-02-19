using Punches.Models;

namespace Punches.Repository;

public interface IClockMonthRepository
{
    IEnumerable<ClockMonth> GetAll();
    void Set(IEnumerable<ClockMonth> data);
}
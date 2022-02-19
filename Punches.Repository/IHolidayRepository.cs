using Punches.Models;

namespace Punches.Repository;

public interface IHolidayRepository
{
    bool IsHoliday(DateTime date);
    Holiday GetHoliday(DateTime date);
}
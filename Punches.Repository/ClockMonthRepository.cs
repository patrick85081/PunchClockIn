using Punches.Models;

namespace Punches.Repository
{
    public class ClockMonthRepository : IClockMonthRepository
    {
        private readonly DataContext dataContext;

        public ClockMonthRepository(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public IEnumerable<ClockMonth> GetAll() =>
            dataContext.Month
                .FindAll()
                .OrderBy(m => m.Id);

        public void Set(IEnumerable<ClockMonth> data)
        {
            dataContext.Month
                .Upsert(data);
        }
    }
}

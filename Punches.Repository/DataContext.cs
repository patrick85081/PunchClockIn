using LiteDB;
using Punches.Models;

namespace Punches.Repository;

public class DataContext : IDisposable
{
    private readonly LiteDatabase db;

    public DataContext() : this("ClockIn.db")
    {
        
    }
    public DataContext(string databasePath)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, databasePath);
        db = new LiteDatabase($"Filename={path};Connection=shared;");
        ClockIn = db.GetCollection<ClockIn>("ClockIn");
        Employee = db.GetCollection<Employee>("Employee");
        Month = db.GetCollection<ClockMonth>("Month");
        Holiday = db.GetCollection<Holiday>("Holiday");
        DailyType = db.GetCollection<DailyType>("DailyType");
        KeyValue = db.GetCollection<KeyValue>("KeyValue");

        db.Mapper.Entity<ClockIn>()
            .Id(x => x.Id);
        db.Mapper.Entity<Employee>()
            .Id(x => x.Id);
        db.Mapper.Entity<ClockMonth>()
            .Id(x => x.Id);
        db.Mapper.Entity<Holiday>()
            .Id(x => x.Id);
        db.Mapper.Entity<DailyType>()
            .Id(x => x.Id);
        db.Mapper.Entity<KeyValue>()
            .Id(x => x.Key);
    }
    public ILiteCollection<ClockIn> ClockIn { get; }
    public ILiteCollection<Employee> Employee { get; }
    public ILiteCollection<ClockMonth> Month { get; }
    public ILiteCollection<Holiday> Holiday { get; }
    public ILiteCollection<DailyType> DailyType { get; }
    public ILiteCollection<KeyValue> KeyValue { get; }

    public void Dispose()
    {
        db.Dispose();
    }
}
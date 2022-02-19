using Punches.Models;

namespace Punches.Repository;

public class KeyValueRepository : IKeyValueRepository
{
    private readonly DataContext dataContext;

    public KeyValueRepository(DataContext dataContext)
    {
        this.dataContext = dataContext;
    }

    public string Get(string key) => dataContext.KeyValue.FindOne(kv => kv.Key == key)?.Value;
    public void Set(string key, string value, string note = null)
    {
        if (dataContext.KeyValue.Exists(kv => kv.Key == key))
        {
            var keyValue = dataContext.KeyValue.FindOne(kv => kv.Key == key);
            keyValue.Value = value;
            dataContext.KeyValue.Update(keyValue);
        }
        else
        {
            dataContext.KeyValue
                .Upsert(new KeyValue() { Key = key, Value = value, Note = note });
        }
    }
}
namespace Punches.Repository;

public interface IKeyValueRepository
{
    string Get(string key);
    void Set(string key, string value, string note = null);
}
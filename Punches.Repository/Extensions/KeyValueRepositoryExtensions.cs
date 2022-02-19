namespace Punches.Repository.Extensions;

public static class KeyValueRepositoryExtensions
{
    public static string Get(this IKeyValueRepository keyValueRepository, KeyValueType type) =>
        keyValueRepository.Get(type.ToString());
    public static void Set(
        this IKeyValueRepository keyValueRepository, KeyValueType type, string value, string note = null) =>
        keyValueRepository.Set(type.ToString(), value, note);

    public static string GetTitle(this IKeyValueRepository keyValueRepository)
        => keyValueRepository.Get(KeyValueType.Title);
    public static void SetTitle(this IKeyValueRepository keyValueRepository, string value)
        => keyValueRepository.Set(KeyValueType.Title, value, "標題");
}
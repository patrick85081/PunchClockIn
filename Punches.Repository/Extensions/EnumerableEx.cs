namespace Punches.Repository.Extensions;

public static class EnumerableEx
{
    public static IEnumerable<(T Item, int Index)> WithIndex<T>(this IEnumerable<T> source)
    {
        int i = 0;
        foreach (var item in source)
        {
            yield return (item, i);
            i++;
        }
    }
}
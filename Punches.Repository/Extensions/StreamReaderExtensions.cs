using System.Reactive.Linq;

namespace Punches.Repository.Extensions;

public static class StreamReaderExtensions
{
    public static IObservable<string> ToReadLinesObservable(this StreamReader sr) =>
        Observable.Create<string>(async (observer, token) =>
        {
            try
            {
                while (!sr.EndOfStream && !token.IsCancellationRequested)
                {
                    var line = await sr.ReadLineAsync();
                    observer.OnNext(line);
                }

                observer.OnCompleted();
            }
            catch (Exception e)
            {
                observer.OnError(e);
            }

            return Task.CompletedTask;
        });
}
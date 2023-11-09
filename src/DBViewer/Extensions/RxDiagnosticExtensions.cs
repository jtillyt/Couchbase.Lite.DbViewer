using System;
using System.Diagnostics;
using System.Reactive.Linq;

namespace DbViewer.Extensions
{
    public static class RxDiagnosticExtensions
    {
        public static IObservable<TSource> LogManagedThread<TSource>(this IObservable<TSource> source, string logName)
        {
            return source.Do(
                _ => Debug.WriteLine($"{logName}: OnNext - ThreadId - {Environment.CurrentManagedThreadId}"),
                exception =>
                    Debug.WriteLine(
                        $"{logName}: OnException - ThreadId - {Environment.CurrentManagedThreadId}. Exception: {exception} "),
                () => Debug.WriteLine($"{logName}: OnCompleted - ThreadId - {Environment.CurrentManagedThreadId}"));
        }
    }
}
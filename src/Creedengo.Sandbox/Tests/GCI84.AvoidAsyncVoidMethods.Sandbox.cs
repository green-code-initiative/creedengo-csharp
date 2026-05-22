using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Creedengo.Sandbox.Tests;

// Mirrors GCI84.AvoidAsyncVoidMethods.Tests.cs.
// Members marked "warns" should light up GCI84 in the IDE; "ok" members should stay clean.
internal static class GCI84Sandbox
{
    public static async void Positive_AsyncVoidMethod() // warns
    {
        await Task.Delay(1000);
        Console.WriteLine();
    }

    public static async void Positive_AsyncVoidWithMissingUsing() // warns
    {
        using var httpClient = new HttpClient();
        _ = await httpClient.GetAsync(new Uri("https://example.com"));
    }

    public static async Task Negative_AsyncTaskMethod() // ok
    {
        await Task.Delay(1000);
        Console.WriteLine();
    }

    public static async Task<int> Negative_AsyncGenericTaskMethod() // ok
    {
        await Task.Delay(1000);
        return 1;
    }

    public static void Positive_LocalAsyncVoidFunction()
    {
        async void Inner() // warns
        {
            await Task.Delay(1000);
        }
        Inner();
    }

    public static void Negative_LocalAsyncTaskFunction()
    {
        async Task Inner() // ok
        {
            await Task.Delay(1000);
        }
        _ = Inner();
    }
}

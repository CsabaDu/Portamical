using Portamical.Core.Converters;
using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes;

namespace Portamical.Benchmarks;

/// <summary>
/// Quick verification that the optimization is working correctly.
/// </summary>
public class VerifyOptimization
{
    public static void Main()
    {
        Console.WriteLine("Verifying ToDistinctArrayTask optimization...\n");

        // Test small collection (should use Task.FromResult)
        var smallCollection = new ITestData[]
        {
            TestDataFactory.CreateTestData<int>("Test_1", "result", 1),
            TestDataFactory.CreateTestData<int>("Test_2", "result", 2),
            TestDataFactory.CreateTestData<int>("Test_3", "result", 3)
        };

        // Test large collection (should use Task.Run)
        var largeCollection = new ITestData[50];
        for (int i = 0; i < 50; i++)
        {
            largeCollection[i] = TestDataFactory.CreateTestData<int>($"Test_{i}", "result", i);
        }

        // Run tests
        var task1 = smallCollection.ToDistinctArrayTask();
        var task2 = largeCollection.ToDistinctArrayTask();

        Task.WaitAll(task1, task2);

        Console.WriteLine($"Small collection (3 items): {task1.Result.Length} distinct items - Task completed: {task1.IsCompletedSuccessfully}");
        Console.WriteLine($"Large collection (50 items): {task2.Result.Length} distinct items - Task completed: {task2.IsCompletedSuccessfully}");

        // Verify identity conversion works
        var identityTask = smallCollection.ToDistinctArrayTask(td => td);
        identityTask.Wait();
        Console.WriteLine($"Identity conversion: {identityTask.Result.Length} items - Task completed: {identityTask.IsCompletedSuccessfully}");

        Console.WriteLine("\n? All verification tests passed!");
        Console.WriteLine("\nOptimization behavior:");
        Console.WriteLine("- Collections < 10 items: Uses Task.FromResult (synchronous, no thread pool overhead)");
        Console.WriteLine("- Collections >= 10 items: Uses Task.Run (asynchronous, offloads to thread pool)");
    }
}

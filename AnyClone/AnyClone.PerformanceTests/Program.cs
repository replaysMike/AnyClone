using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AnyClone.Tests.TestObjects;
using Force.DeepCloner;

namespace AnyClone.PerformanceTests
{
    class Program
    {
        private const int TestObjects = 50000;

        static void Main(string[] args)
        {
            var results = new Dictionary<string, TimeSpan>();
            Console.WriteLine("AnyClone Performance Testing");

            var buyObject = new BuyObject() {
                AcceptedFrom = Guid.NewGuid(),
                ActualSnapshot = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 },
                AdId = 100,
                BuyTypeId = BuyType.NoFee,
                Id = 999,
                RowVersion = 111,
                SkipVersionIncrement = true,
                ExternalOrderVersion = 2,
                ChildHeaderId = 123234234
            };
            var clonedObjects = new List<BuyObject>();

            // clone using AnyClone
            var timer = new Stopwatch();
            Console.WriteLine($"Measuring AnyClone.Clone() X {TestObjects:N0}...");
            timer.Start();
            for (var i = 0; i < TestObjects; i++)
            {
                clonedObjects.Add(buyObject.Clone(CloneOptions.DisableIgnoreAttributes));
            }
            timer.Stop();
            results.Add("AnyClone", timer.Elapsed);
            Console.WriteLine($"AnyClone took {timer.Elapsed}");
            clonedObjects.Clear();

            Console.WriteLine($"Measuring DeepCloner.DeepClone() X {TestObjects:N0}...");
            timer.Restart();
            for (var i = 0; i < TestObjects; i++)
            {
                clonedObjects.Add(buyObject.DeepClone());
            }
            timer.Stop();
            results.Add("DeepCloner", timer.Elapsed);
            Console.WriteLine($"DeepCloner took {timer.Elapsed}");

            Console.WriteLine($"\r\nFinished performance tests!\r\n");
            Console.WriteLine($"Results: \r\n");

            var resultNumber = 1;
            foreach(var result in results.OrderBy(x => x.Value))
            {
                Console.WriteLine($"#{resultNumber}: {result.Key} took {result.Value}, {(result.Value.TotalMilliseconds / TestObjects):N2}ms per clone operation");
                resultNumber++;
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yasks
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create cancelleation token source and obtain token
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;

            //Create and run long running task
            var taskCT = Task.Run(() => doWork(ct), ct);

            //Cancell this task
            cts.Cancel();

            //Handle the TaskCanceledException
            try
            {
                taskCT.Wait();
            }
            catch (AggregateException ae)
            {
                foreach (var inner in ae.InnerExceptions)
                {
                    if (inner is TaskCanceledException)
                    {
                        Console.WriteLine("The task has been canceled.");
                        Console.ReadLine();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            Task<string> task1 = Task.Run<string>(() => DateTime.Now.DayOfWeek.ToString());
            Console.WriteLine(task1.Result);

            Task task2 = Task.Run(() => { Console.WriteLine("The time is now {0}", DateTime.Now); });
            // task2.Wait();

            var task3 = Task.Factory.StartNew(() => Console.WriteLine("Task 3 has completed"));
            // task3.Wait();

            var task4 = Task.Run(() => Console.WriteLine("Task 4 has completed"));
            // task4.Wait();

            Parallel.Invoke(() => task2.Wait(),
                () => task3.Wait(),
                () => task4.Wait());

            int from = 0;
            int to = 400;
            double[] array = new double[100000];

            Parallel.For(from, to, index =>
           {
               array[index] = Math.Sqrt(index);
           }).ToString();

            Task<string> firstTask = new Task<string>(() => "Hello");

            Task<string> secondTask = firstTask.ContinueWith((antecedent) => String.Format("{0}, World!", antecedent.Result));

            firstTask.Start();

            Console.WriteLine(secondTask.Result);

            var outer = Task.Run(() =>
            {
                Console.WriteLine("Outer task is starting...");
                var inner = Task.Run(() =>
                {
                    Console.WriteLine("Nested task has started...");
                    Thread.SpinWait(5000);
                    Console.WriteLine("Nested task is completing...");
                });
                //outer.Wait();
                Console.WriteLine("Outer task completed.");
            });

            //Console.ReadKey();

            var parent = Task.Run(() =>
            {
                Console.WriteLine("Parent task is starting...");
                var child = Task.Run(() =>
                {
                    Console.WriteLine("Child task has started...");
                    Thread.SpinWait(5000);
                    Console.WriteLine("Child task is completing...");
                });
            });
            parent.Wait();
            Console.WriteLine("Parent task completed.");
            Console.ReadKey();
        }

        private static void doWork(CancellationToken token)
        {
            for (int i = 0; i < 100; i++)
            {
                Thread.SpinWait(500000);
                token.ThrowIfCancellationRequested();
            }
        }
    }
}

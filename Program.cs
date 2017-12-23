using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TableStorageLoad
{
	class Program
	{
		private const string _connection = "DefaultEndpointsProtocol=https;AccountName=devresolverusnc1;AccountKey=TvF7yglliobGm4cYzH1JSihxJKDqYew8yWFFQsU6gip1DOetK3jw+EZPopl8QzZCV1y9HW0Nhbj63jgF6ed4lQ==";

		static void Main(string[] args)
		{{
			int threadCount = 5;
			int durationSec = 5;
			bool useService = false;

			if (args.Length >= 1) threadCount = Convert.ToInt16(args[0]);
			if (args.Length >= 2) durationSec = Convert.ToInt16(args[1]);
			if (args.Length >= 3) useService = args[2] == "s";

			Console.WriteLine("Thread Count: {0}, Duration Seconds: {1}", threadCount, durationSec);

			//int minWorker, minIOC;
			//ThreadPool.GetMinThreads(out minWorker, out minIOC);
			//ThreadPool.SetMinThreads(threadCount, threadCount);

			// spin up some threads to do the work
			var tasks = new List<Task>();
			for (int loop = 1; loop <= threadCount; loop++)
			{
				int loop1 = loop;
				if (useService)
					tasks.Add(Task.Run(async () => { await BlastTableService(loop1, durationSec); }));
				else
					tasks.Add(Task.Run(async () => { await BlastTableUser(loop1, durationSec); }));
			}

			Task.WaitAll(tasks.ToArray());
			Logging.WriteLogData(useService ? "LoadServ.csv" : "LoadUser.csv", threadCount);
		}}

		private static async Task BlastTableUser(int instance, int durationSec)
		{
			var table = new TableStorage();
			table.InitTableClient(_connection);

			var timer = Stopwatch.StartNew();
			int loop = 1;
			var logData = new List<Logging.LogData>();

			while (timer.Elapsed < TimeSpan.FromSeconds(durationSec))
			{
				var sw = Stopwatch.StartNew();
				var user = await table.GetSdkUser("resolver.client");
				sw.Stop();

				if (user == null)
				{
					var data = new Logging.LogData() {Thread = instance, Iteration = loop++, ElapsedMs = 99999};
					logData.Add(data);
					Console.WriteLine("Thread: {0:d2}, Iteration: {1:d4}, Table: Error, Elapsed: {2}ms", instance, data.Iteration, data.ElapsedMs);
				}
				else
				{
					var data = new Logging.LogData() {Thread = instance, Iteration = loop++, ElapsedMs = sw.ElapsedMilliseconds};
					logData.Add(data);
					Console.WriteLine("Thread: {0:d2}, Iteration: {1:d4}, Table: {2}, Elapsed: {3}ms", data.Thread, data.Iteration, user.RowKey, data.ElapsedMs);
				}
			}

			Logging.AddLogData(logData);
			Console.WriteLine("Thread: {0:d2} exiting, duration elapsed", instance);
		}

		private static async Task BlastTableService(int instance, int durationSec)
		{
			var table = new TableStorage();
			table.InitTableClient(_connection);

			var timer = Stopwatch.StartNew();
			int loop = 1;
			var logData = new List<Logging.LogData>();

			while (timer.Elapsed < TimeSpan.FromSeconds(durationSec))
			{
				var sw = Stopwatch.StartNew();
				var service = await table.GetService("12", "5");
				sw.Stop();

				if (service == null)
				{
					var data = new Logging.LogData() { Thread = instance, Iteration = loop++, ElapsedMs = 99999 };
					logData.Add(data);
					Console.WriteLine("Thread: {0:d2}, Iteration: {1:d4}, Table: Error, Elapsed: {2}ms", instance, data.Iteration, data.ElapsedMs);
				}
				else
				{
					var data = new Logging.LogData() { Thread = instance, Iteration = loop++, ElapsedMs = sw.ElapsedMilliseconds };
					logData.Add(data);
					Console.WriteLine("Thread: {0:d2}, Iteration: {1:d4}, Table: {2}, Elapsed: {3}ms", data.Thread, data.Iteration, service.PartitionKey, data.ElapsedMs);
				}
			}

			Logging.AddLogData(logData);
			Console.WriteLine("Thread: {0:d2} exiting, duration elapsed", instance);
		}
	}
}

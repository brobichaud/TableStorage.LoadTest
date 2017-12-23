using System;
using System.Threading.Tasks;
using Digimarc.Data.ResolverDal;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;

namespace TableStorageLoad
{
	internal class TableStorage
	{
		private CloudStorageAccount _account;

		/// <summary>
		/// Gets an sdkuser by username
		/// </summary>
		/// <param name="username">Username to get</param>
		public async Task<SdkUserEntity> GetSdkUser(string username)
		{
			var table = GetTableClient().GetTableReference("sdkuser");

			// query for lowercase rowkey for a case insensitive query, table storage does not support that natively
			var operation = TableOperation.Retrieve<SdkUserEntity>("", username.ToLower());
			var result = await table.ExecuteAsync(operation);
			return (SdkUserEntity)result.Result;
		}

		/// <summary>
		/// Gets a service by payload and type
		/// </summary>
		/// <param name="payload">Payload logList associated with the service</param>
		/// <param name="type">Payload type associated with the service</param>
		public async Task<ServiceEntity> GetService(string payload, string type)
		{
			var table = GetTableClient().GetTableReference("service");

			var operation = TableOperation.Retrieve<ServiceEntity>(payload, type);
			var result = await table.ExecuteAsync(operation);
			return (ServiceEntity)result.Result;
		}

		public void InitTableClient(string connection, IRetryPolicy retryPolicy = null, int timeoutSec = 0)
		{
			_account = CloudStorageAccount.Parse(connection);
			CloudTableClient client = GetTableClient();

			// set custom exponential retry policy to limit how long we wait
			client.DefaultRequestOptions.RetryPolicy = retryPolicy ?? new ExponentialRetry(TimeSpan.FromSeconds(4), 3);

			// set custom timeout if specified
			if (timeoutSec > 0)
				client.DefaultRequestOptions.ServerTimeout = TimeSpan.FromSeconds(timeoutSec);
		}

		/// <summary>
		/// Creates a table client from the passed connection string
		/// </summary>
		private CloudTableClient GetTableClient()
		{
			return _account.CreateCloudTableClient();
		}
	}
}

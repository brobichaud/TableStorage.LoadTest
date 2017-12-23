using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Digimarc.Data.ResolverDal
{
	/// <summary>
	/// Resolver sdkuser credential details
	/// </summary>
	/// <remarks>
	/// PartitionKey is empty, RowKey is the UserName (always lowercase, enforced by TableStorageRepository)
	/// </remarks>
	[Serializable]
	public class SdkUserEntity : TableEntity, ISdkUser
	{
		/// <summary>
		/// Version of the schema this row adheres to
		/// </summary>
		public int SchemaVersion { get; set; }

		/// <summary>
		/// Whether user is currently enabled
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// Database record id from the Portal main database
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Users security key (password)
		/// </summary>
		public string SecurityKey { get; set; }

		/// <summary>
		/// When the user expires
		/// </summary>
		public DateTime Expires { get; set; }

		/// <summary>
		/// Default ctor
		/// </summary>
		public SdkUserEntity()
		{
			PartitionKey = string.Empty;
			RowKey = string.Empty;
			SchemaVersion = 1;
		}

		/// <summary>
		/// Initializing ctor
		/// </summary>
		/// <param name="pk">PartitionKey - always empty</param>
		/// <param name="rk">RowKey - UserName</param>
		public SdkUserEntity(string pk, string rk)
		{
			PartitionKey = pk;
			RowKey = rk;
			SchemaVersion = 1;
		}
	}
}

using System;

namespace Digimarc.Data.ResolverDal
{
	/// <summary>
	/// Resolver sdkuser credential details
	/// </summary>
	public interface ISdkUser
	{
		bool Enabled { get; set; }
		int Id { get; set; }
		string SecurityKey { get; set; }
		DateTime Expires { get; set; }
	}
}

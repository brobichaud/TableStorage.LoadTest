using System;
using System.Xml.Serialization;

namespace Digimarc.Data.ResolverDal
{
	/// <summary>
	/// Represents a single payoff record for a service
	/// </summary>
	[Serializable]
	[XmlType(TypeName = "Payoff")]
	public class PayoffRecord
	{
		/// <summary>
		/// Id of the payoff record
		/// </summary>
		[XmlAttribute(AttributeName = "id")]
		public int Id { get; set; }

		/// <summary>
		/// Url of payoff content
		/// </summary>
		[XmlAttribute(AttributeName = "url")]
		public string Url { get; set; }

		/// <summary>
		/// Url of payoff content
		/// </summary>
		[XmlAttribute(AttributeName = "desc")]
		public string Description { get; set; }
	}
}

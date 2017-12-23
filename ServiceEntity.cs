using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.WindowsAzure.Storage.Table;

namespace Digimarc.Data.ResolverDal
{
	/// <summary>
	/// MediaService details needed to handle payload resolve requests
	/// </summary>
	/// <remarks>
	/// PartitionKey is the Payload Data, RowKey is the Payload Type
	/// </remarks>
	[Serializable]
	public class ServiceEntity : TableEntity
	{
		/// <summary>
		/// Version of the schema this row adheres to
		/// </summary>
		public int SchemaVersion { get; set; }

		/// <summary>
		/// Whether this service should return a payoff
		/// </summary>
		/// <remarks>
		/// A combination of all database fields that apply to whether a service is enabled.
		/// Excludes Active and preview period calculations.
		/// </remarks>
		public bool Enabled { get; set; }

		/// <summary>
		/// MediaService database record id from the main Portal database
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		/// Time the service becomes active (UTC)
		/// </summary>
		public DateTime ActiveStart { get; set; }

		/// <summary>
		/// Time the service stops being active (UTC)
		/// </summary>
		public DateTime ActiveEnd { get; set; }

		/// <summary>
		/// Time the service is no longer available for preview (UTC)
		/// </summary>
		public DateTime? PreviewEnd { get; set; }

		/// <summary>
		/// Common title of the payoff
		/// </summary>
		public string CommonTitle { get; set; }

		/// <summary>
		/// Common subtitle of the payoff
		/// </summary>
		public string CommonSubtitle { get; set; }

		/// <summary>
		/// Common image of the payoff
		/// </summary>
		public string CommonImageUrl { get; set; }

		/// <summary>
		/// Common image anchor of the payoff, indicates where the area of interest is
		/// </summary>
		public string CommonImageAnchor { get; set; }

		/// <summary>
		/// List of payoff records
		/// </summary>
		[XmlIgnore]
		public List<PayoffRecord> PayoffRecords { get; set; }

		/// <summary>
		/// String representation of PayoffRecords
		/// </summary>
		/// <remarks>
		/// This is used for serialization support. To access the data use the PayoffRecords property.
		/// </remarks>
		public string PayoffRecordsData
		{
			get { return ToXml(PayoffRecords); }
			set { PayoffRecords = FromXml<List<PayoffRecord>>(value); }
		}

		/// <summary>
		/// Default ctor
		/// </summary>
		public ServiceEntity()
		{
			SchemaVersion = _schemaVersionCurrent;
		}

		/// <summary>
		/// Initializing ctor
		/// </summary>
		/// <param name="pk">PartitionKey - Payload Data</param>
		/// <param name="rk">RowKey - Payload Type</param>
		public ServiceEntity(string pk, string rk) : base(pk, rk)
		{
			SchemaVersion = _schemaVersionCurrent;
		}

		/// <summary>
		/// Determines if the service is currently active
		/// </summary>
		public bool IsActive()
		{
			var now = DateTime.UtcNow;

			// check enabled status and whether we are in the active period
			bool active = (Enabled && (now >= ActiveStart) && (now < ActiveEnd));

			// check if service is in preview mode
			active |= (now < (PreviewEnd ?? DateTime.MinValue));

			return active;
		}

		/// <summary>
		/// Serializes the object to an xml string
		/// </summary>
		/// <param name="obj">object to write</param>
		/// <returns>string representation</returns>
		private string ToXml(object obj)
		{
			var buffer = new StringBuilder();
			var settings = new XmlWriterSettings() { Encoding = Encoding.UTF8, OmitXmlDeclaration = true };
			using (var writer = XmlWriter.Create(buffer, settings))
			{
				// remove all namespace declarations
				var ns = new XmlSerializerNamespaces();
				ns.Add("", "");

				var ser = XmlSerializerCache.Create(obj.GetType(), new XmlRootAttribute("Payoffs"));
				ser.Serialize(writer, obj, ns);
			}

			return buffer.ToString();
		}

		/// <summary>
		/// Creates an object by deserializing from an xml string
		/// </summary>
		/// <param name="objData">string representation</param>
		/// <returns>object</returns>
		private T FromXml<T>(string objData)
		{
			using (var reader = new StringReader(objData))
			{
				var ser = XmlSerializerCache.Create(typeof(T), new XmlRootAttribute("Payoffs"));
				return (T)ser.Deserialize(reader);
			}
		}

		private const int _schemaVersionCurrent = 3;
	}
	
	/// <summary>
	/// Caches serializers to avoid an obscure issue with using an XmlSerializer that takes a root attribute
	/// </summary>
	public static class XmlSerializerCache
	{
		private static readonly Dictionary<string, XmlSerializer> Cache = new Dictionary<string, XmlSerializer>();

		public static XmlSerializer Create(Type type, XmlRootAttribute root)
		{
			var key = String.Format(CultureInfo.InvariantCulture, "{0}:{1}", type, root.ElementName);

			XmlSerializer ser;
			if (!Cache.TryGetValue(key, out ser))
			{
				ser = new XmlSerializer(type, root);

				try
				{
					Cache.Add(key, ser);
				}
				catch (ArgumentException e)
				{
					// swallow duplicate key exceptions
					if (!e.Message.Contains("item with the same key has already been added"))
						throw;
				}
			}

			return ser;
		}
	}
}

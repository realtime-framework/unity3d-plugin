// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
using System;
using Realtime.LITJson;
using Realtime.Storage.Models;

namespace Realtime.Storage.DataAccess
{
    /// <summary>
    /// Exporters and Importers for LITJson conversion
    /// </summary>
    internal class StorageConverters
    {
        private static bool _isInit;

        /// <summary>
        /// Initializes
        /// </summary>
        public static void Initialize()
        {
            if (_isInit)
                return;
            _isInit = true;
            JsonMapper.RegisterExporter((ExporterFunc<BetweenFilter>)BetweenFilterExporter);
            JsonMapper.RegisterExporter((ExporterFunc<Filter>)FilterExporter);
            JsonMapper.RegisterExporter((ExporterFunc<DateTime>)DateTimeExporter);
            JsonMapper.RegisterExporter((ExporterFunc<Key.DataType>)DataTypeExporter);
            JsonMapper.RegisterImporter((ImporterFunc<long, DateTime>)DateTimeImporter);
            JsonMapper.RegisterImporter((ImporterFunc<double, DateTime>)DateTimeImporter);
            JsonMapper.RegisterImporter((ImporterFunc<string, Key.DataType>)DataTypeImporter);
            JsonMapper.RegisterImporter((ImporterFunc<string, TableMetadata.Status>)StatusImporter);
            JsonMapper.RegisterImporter((ImporterFunc<JsonData, TableMetadata>)ReadMetadata);
        }


        /// <summary>
        /// serialization DTO
        /// </summary>
        protected class ProvisionMeta
        {
            /// <summary>
            /// id
            /// </summary>
            public int id { get; set; }
            /// <summary>
            /// Name
            /// </summary>
            public string name { get; set; }
        }

        static public TableMetadata ReadMetadata(JsonData jObject)
        {
            var target = new TableMetadata();

            if (jObject["table"] != null)
                target.name = jObject["table"].ToString();
            if (jObject["name"] != null)
                target.name = jObject["name"].ToString();
            if (jObject["creationDate"] != null)
                target.creationDate = DateTime.FromFileTimeUtc((long)jObject["creationDate"]);
            if (jObject["status"] != null)
                target.status = (TableMetadata.Status)Enum.Parse(typeof(TableMetadata.Status), jObject["status"].ToString().ToUpper());
            if (jObject["itemCount"] != null)
                target.itemCount = (int)jObject["itemCount"];
            if (jObject["key"] != null)
                target.key = JsonMapper.ToObject<TableKey>(jObject["key"].ToJson());
            if (jObject["size"] != null)
                target.size = (int)jObject["size"];
            if (jObject["throughput"] != null)
                target.throughput = JsonMapper.ToObject<TableThroughput>(jObject["throughput"].ToJson());
            //
            if (jObject["provisionType"] != null)
            {
                var p1 = JsonMapper.ToObject<ProvisionMeta>(jObject["provisionType"].ToJson());
                target.provisionType = (ProvisionType)p1.id;
            }
            if (jObject["provisionLoad"] != null)
            {
                var p2 = JsonMapper.ToObject<ProvisionMeta>(jObject["provisionLoad"].ToJson());
                target.provisionLoad = (ProvisionLoad)p2.id;
            }

            return target;

        }


        public static TableMetadata.Status StatusImporter(string value)
        {
            return (TableMetadata.Status)Enum.Parse(typeof(TableMetadata.Status), value.ToUpper());
        }

        public static Key.DataType DataTypeImporter(string value)
        {
            return (Key.DataType)Enum.Parse(typeof(Key.DataType), value.ToUpper());
        }

        public static void DataTypeExporter(Key.DataType value, JsonWriter writer)
        {
            writer.Write(value.ToString());
        }

        public static DateTime DateTimeImporter(double value)
        {
            return DateTime.FromFileTimeUtc((long)value);
        }

        public static DateTime DateTimeImporter(long value)
        {
            return DateTime.FromFileTimeUtc(value);
        }

        public static void DateTimeExporter(DateTime value, JsonWriter writer)
        {
            writer.Write(value.ToFileTimeUtc());
        }

        public static void FilterExporter(Filter value, JsonWriter writer)
        {
            writer.WriteObjectStart();

            writer.WritePropertyName("operator");
            writer.Write(value.op.ToString());

            if (!string.IsNullOrEmpty(value.item))
            {
                writer.WritePropertyName("item");
                writer.Write(value.item);
            }

            writer.WritePropertyName("value");
            WriteValue(value.value, writer);

            writer.WriteObjectEnd();
        }

        public static void BetweenFilterExporter(BetweenFilter value, JsonWriter writer)
        {
            writer.WriteObjectStart();

            writer.WritePropertyName("operator");
            writer.Write("between");

            if (!string.IsNullOrEmpty(value.item))
            {
                writer.WritePropertyName("item");
                writer.Write(value.item);
            }

            writer.WritePropertyName("value");
            writer.WriteArrayStart();
            WriteValue(value.value, writer);
            WriteValue(value.endvalue, writer);
            writer.WriteArrayEnd();

            writer.WriteObjectEnd();
        }

        static void WriteValue(object value, JsonWriter writer)
        {
            if (value is int)
            {
                writer.Write((int)value);
            }
            else if (value is long)
            {
                writer.Write((long)value);
            }
            else if (value is double)
            {
                writer.Write((double)value);
            }
            else if (value is short)
            {
                writer.Write((short)value);
            }
            else if (value is uint)
            {
                writer.Write((uint)value);
            }
            else if (value is ulong)
            {
                writer.Write((ulong)value);
            }
            else if (value is ushort)
            {
                writer.Write((ushort)value);
            }
            else if (value is DateTime)
            {
                writer.Write(((DateTime)value).ToFileTimeUtc());
            }
            else
            {
                writer.Write(value.ToString());
            }
        }
    }
}

// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
using System;
using System.Linq;
using System.Reflection;

namespace Realtime.Storage.Models
{
    /// <summary>
    /// Metadata for the resolving of an objects TableName and DataKey.
    /// </summary>
    /// <remarks>
    /// Uses Reflection. Depends on StorageKeyAttribute
    /// </remarks>
    public class ItemMetadata
    {
        /// <summary>
        /// Type of Object
        /// </summary>
        public Type ObjecType { get; set; }

        /// <summary>
        /// Name of the table
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Name of the primary key.
        /// </summary>
        public string Primary { get; set; }

        /// <summary>
        /// Name of the secondary key.
        /// </summary>
        public string Secondary { get; set; }

        /// <summary>
        /// Secondary is not null
        /// </summary>
        public bool HasSecondary { get; set; }

        /// <summary>
        /// Type for Primary Key
        /// </summary>
        public Type PrimaryType { get; set; }

        /// <summary>
        /// Type for Secondary Key
        /// </summary>
        public Type SecondaryType { get; set; }

        /// <summary>
        /// Is the primary member a field or property
        /// </summary>
        public bool PrimaryIsField { get; set; }

        /// <summary>
        /// Is the secondary member a field or property
        /// </summary>
        public bool SecondaryIsField { get; set; }

        /// <summary>
        /// creates a new Metadata
        /// </summary>
        /// <param name="t"></param>
        /// <param name="a"></param>
        public ItemMetadata(Type t, StorageKeyAttribute a)
            : this(t, a.Table, a.Primary, a.Secondary)
        {

        }

        /// <summary>
        /// Creates a new ItemMetadta
        /// </summary>
        /// <param name="t"></param>
        /// <param name="tableName"></param>
        /// <param name="primaryName"></param>
        /// <param name="secondaryName"></param>
        public ItemMetadata(Type t, string tableName, string primaryName, string secondaryName)
        {
            Table = tableName;
            Primary = primaryName;
            Secondary = secondaryName;
            HasSecondary = !string.IsNullOrEmpty(secondaryName);
            ObjecType = t;

            //

            var p = ObjecType.GetMember(Primary).FirstOrDefault();
            if (p == null)
                throw new Exception("Invalid PrimaryKey");

            if (p is FieldInfo)
            {
                PrimaryIsField = true;
                PrimaryType = ((FieldInfo)p).FieldType;
            }
            else if (p is PropertyInfo)
            {
                PrimaryIsField = false;
                PrimaryType = ((PropertyInfo)p).PropertyType;

            }
            else
                throw new Exception("Invalid PrimaryKey");

            if (!IsValidKeyType(PrimaryType))
                throw new Exception("Invalid Key. Secondary key must be a string or a number.");
            //

            if (HasSecondary)
            {
                var s = ObjecType.GetMember(Secondary).FirstOrDefault();
                if (s == null)
                    throw new Exception("Invalid SecondaryKey");

                if (s is FieldInfo)
                {
                    SecondaryIsField = true;
                    SecondaryType = ((FieldInfo)s).FieldType;
                }
                else if (s is PropertyInfo)
                {
                    SecondaryIsField = false;
                    SecondaryType = ((PropertyInfo)s).PropertyType;
                }
                else
                    throw new Exception("Invalid SecondaryKey");

                if (!IsValidKeyType(PrimaryType))
                    throw new Exception("Invalid Key. Secondary key must be a string or a number.");
            }
        }

        /// <summary>
        /// returns the primary key value
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object GetPrimaryKey(object context)
        {
            return PrimaryIsField ? ObjecType.GetField(Primary).GetValue(context) : ObjecType.GetProperty(Primary).GetValue(context, null);
        }

        /// <summary>
        /// returns the secondary key value
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object GetSecondaryKey(object context)
        {
            return SecondaryIsField ? ObjecType.GetField(Secondary).GetValue(context) : ObjecType.GetProperty(Secondary).GetValue(context, null);
        }

        /// <summary>
        /// returns a Datakey for the object
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public DataKey GetKey(object context)
        {
            return new DataKey
            {
                primary = GetPrimaryKey(context),
                secondary = HasSecondary ? GetSecondaryKey(context) : null
            };
        }

        /// <summary>
        /// Is the type valid for use as a column
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        bool IsValidKeyType(Type t)
        {
            return t == typeof(string) || t == typeof(int) || t == typeof(double) || t == typeof(short) || t == typeof(uint) || t == typeof(long) || t == typeof(float);
        }
    }
}
// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
using System;

namespace Realtime.Storage.Models
{
    /// <summary>
    /// Security Policy
    /// </summary>
    [Serializable]
    public class Policy
    {
     
        /// <summary>
        /// database this policy is for
        /// </summary>
        public Database database;
        /// <summary>
        /// tables this policy is for
        /// </summary>
        public Tables tables;

        /// <summary>
        /// describes the database policy
        /// </summary>
        [Serializable]
        public class Database
        {
            /// <summary>
            /// permission to list all tables
            /// </summary>
            public string[] listTables;
            /// <summary>
            /// permission only to update (throughput) table named 'SampleUser'
            /// </summary>
            public string[] updateTable;
            /// <summary>
            /// permission to create tables
            /// </summary>
            public bool createTable;
        }

        /// <summary>
        /// describes the table policy
        /// </summary>
        [Serializable]
        public class Tables
        {
            /// <summary>
            /// Admin allowance
            /// </summary>
            public AdminInfo Admin;
            /// <summary>
            /// Sample User
            /// </summary>
            public AdminInfo SampleUser;

            /// <summary>
            /// Allowed commands
            /// </summary>
            [Serializable]
            public class AdminInfo
            {
                /// <summary>
                /// C,R, ect
                /// </summary>
                public string allow;
            }

            /// <summary>
            /// Allowed commands
            /// </summary>
            [Serializable]
            public class SampleUserInfo
            {
                /// <summary>
                /// C,R, ect
                /// </summary>
                public string allow;
            }
        }
    }
}
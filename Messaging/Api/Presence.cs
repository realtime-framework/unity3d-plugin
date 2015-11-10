using System;
using System.Collections.Generic;

namespace Realtime.Messaging
{


    /// <summary>
    /// Presence info, such as total subscriptions and metadata.
    /// </summary>
    public class Presence
    {
        /// <summary>
        /// Gets the subscriptions value.
        /// </summary>
        public long Subscriptions { get; set; }

        /// <summary>
        /// Gets the first 100 unique metadata.
        /// </summary>
        public Dictionary<String, long> Metadata { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Presence"/> class.
        /// </summary>
        public Presence()
        {
            Subscriptions = 0;
            Metadata = new Dictionary<String, long>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharpnote.Json;
using Sharpnote.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Sharpnote
{
    /// <summary>
    /// Simple implementation of INote interface
    /// </summary>
    [JsonObject]
    public class Note: INote
    {
         [JsonProperty("deleted", NullValueHandling = NullValueHandling.Ignore)]
        private int? _deleted {get;set;}

        /// <summary>
        /// Note identifier created by Simplenote server
        /// </summary>
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key { get; private set; }

        /// <summary>
        /// Whether or not note is in Simplenote trash
        /// </summary>       
        [JsonIgnore]
        public bool? Deleted 
        {
            get
            {
                if (_deleted.HasValue)
                    return _deleted == 1;

                return null;
            }
            set
            {
                if (!value.HasValue) 
                    _deleted = null;

                _deleted = value.Value ? 1 : 0;
            }
        }

        /// <summary>
        /// Whether or not the note is pinned
        /// </summary>
        [JsonIgnore]
        public bool Pinned 
        {
            get
            {
                return SystemTags != null && SystemTags.Contains("pinned");
            }
            set
            {
                if (value)
                {
                    if (SystemTags == null)
                        SystemTags = new List<string>();

                    if (!SystemTags.Contains("pinned"))
                        SystemTags.Add("pinned");
                }
                else
                {
                    if (SystemTags != null)
                        SystemTags.Remove("pinned");
                }
            }
        }

        /// <summary>
        /// Whether or not the note has been read
        /// </summary>
        [JsonIgnore]
        public bool Unread
        {
            get
            {
                return SystemTags != null && SystemTags.Contains("unread");
            }
            set
            {
                if (value)
                {
                    if (SystemTags == null)
                        SystemTags = new List<string>();

                    if (!SystemTags.Contains("unread"))
                        SystemTags.Add("unread");
                }
                else
                {
                    if (SystemTags != null)
                        SystemTags.Remove("unread");
                }

            }
        }

        /// <summary>
        /// Last modified date
        /// </summary>
        [JsonProperty("modifydate", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(Sharpnote.Json.DateTimeEpochConverter))]
        public DateTimeOffset? Modified
        {
            get;
            set;
        }

        /// <summary>
        /// Created date
        /// </summary>
        [JsonProperty("createdate", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(Sharpnote.Json.DateTimeEpochConverter))]
        public DateTimeOffset? Created
        {
            get;
            set;
        }

        /// <summary>
        /// Tracks syncs to Simplenote server
        /// </summary>
        [JsonProperty("syncnum", NullValueHandling = NullValueHandling.Ignore)]
        public int? SyncNum { get; private set; }

        /// <summary>
        /// Tracks changes to note content
        /// </summary>
        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public int? Version { get; private set; }

        /// <summary>
        /// Minimum version of note available from Simplenote server
        /// </summary>
        [JsonProperty("minversion", NullValueHandling = NullValueHandling.Ignore)]
        public int? MinVersion { get; private set; }

        /// <summary>
        /// Shared note identifier
        /// </summary>
        [JsonProperty("sharekey", NullValueHandling = NullValueHandling.Ignore)]
        public string ShareKey { get; private set; }

        /// <summary>
        /// Published note identifier
        /// </summary>
        [JsonProperty("publishkey", NullValueHandling = NullValueHandling.Ignore)]
        public string PublishKey { get; private set; }

        /// <summary>
        /// All tags for note.  Some set by Simplenote server
        /// </summary>
        [JsonProperty("systemtags", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> SystemTags { get; private set; }

        /// <summary>
        /// Tags for note
        /// </summary>
        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Tags { get; set; }

        /// <summary>
        /// Note content
        /// </summary>
        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; set; }
    }    
}

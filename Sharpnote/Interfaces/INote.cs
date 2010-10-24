using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpnote.Interfaces
{
    /// <summary>
    /// Interface encasing properties of a note from Simplenote. 
    /// </summary>
    public interface INote
    {
        /// <summary>
        /// Note identifier created by Simplenote server
        /// </summary>
        string Key { get; }
        /// <summary>
        /// Whether or not note is in Simplenote trash
        /// </summary>
        bool? Deleted { get; set; }
        /// <summary>
        /// Last modified date
        /// </summary>
        DateTimeOffset? Modified { get; set; }
        /// <summary>
        /// Created date
        /// </summary>
        DateTimeOffset? Created { get; set; }    
        /// <summary>
        /// Tracks syncs to Simplenote server
        /// </summary>
        int? SyncNum { get; }
        /// <summary>
        /// Tracks changes to note content
        /// </summary>
        int? Version { get; }
        /// <summary>
        /// Minimum version of note available from Simplenote server
        /// </summary>
        int? MinVersion { get; }
        /// <summary>
        /// Shared note identifier
        /// </summary>
        string ShareKey { get; }
        /// <summary>
        /// Published note identifier
        /// </summary>
        string PublishKey { get; }
        /// <summary>
        /// All tags for note.  Some set by Simplenote server
        /// </summary>
        List<string> SystemTags { get; }
        /// <summary>
        /// Tags for note
        /// </summary>
        List<string> Tags { get; set; }
        /// <summary>
        /// Note content
        /// </summary>
        string Content { get; set; }
    }
}

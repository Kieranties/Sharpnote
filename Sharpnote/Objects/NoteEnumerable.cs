using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharpnote.Interfaces;
using Newtonsoft.Json;

namespace Sharpnote.Objects
{
    /// <summary>
    /// Simple implementation of INoteEnumerable interface
    /// </summary>
    [JsonObject]
    public class NoteEnumerable<T> : INoteEnumerable<T> where T: INote, new()
    {
        /// <summary>
        /// The key of the note for the next set of items in this collection
        /// </summary>
        [JsonProperty("mark", NullValueHandling = NullValueHandling.Ignore)]
        public string Mark { get; private set; }
        /// <summary>
        /// The date at which the items in this collection are fetched from
        /// </summary>
        [JsonProperty("since", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Since { get; private set; }
        /// <summary>
        /// The private collection of items
        /// </summary>
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        private T[] Items { get; set; }


        #region IEnumerable<T> methods
        public IEnumerator<T> GetEnumerator()
        {
            return Items.AsEnumerable().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }  
        #endregion      
    }
}

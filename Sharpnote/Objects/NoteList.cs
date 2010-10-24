using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharpnote.Interfaces;

namespace Sharpnote.Objects
{
    /// <summary>
    /// Simple implementation of INoteList interface
    /// </summary>
    /// <typeparam name="INote">An implementation of INote to return</typeparam>
    public class NoteList<INote> : List<INote>, INoteList<INote>
    {
        /// <summary>
        /// The key of the note for the next set of items in this collection
        /// </summary>
        public string Mark { get; private set; }
        /// <summary>
        /// The date at which the items in this collection are fetched from
        /// </summary>
        public DateTimeOffset? Since { get; private set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpnote.Interfaces
{
    /// <summary>
    /// A collection object of T notes
    /// </summary>
    /// <typeparam name="INote"></typeparam>
    public interface INoteList<INote>: IList<INote>
    {
        /// <summary>
        /// The key of the note for the next set of items in this collection
        /// </summary>
        string Mark { get; }
        /// <summary>
        /// The date at which the items in this collection are fetched from
        /// </summary>
        DateTimeOffset? Since { get; }
    }
}

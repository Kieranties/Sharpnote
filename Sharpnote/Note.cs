using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharpnote.Interfaces;

namespace Sharpnote
{
    /// <summary>
    /// Simple implementation of INote interface
    /// </summary>
    public class Note: INote
    {
        public string Key { get; set; }
        public string Content { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
    }
}

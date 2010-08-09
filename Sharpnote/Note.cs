using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpnote
{
    public class Note: Interfaces.INote
    {
        public string Key { get; set; }
        public string Content { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
    }
}

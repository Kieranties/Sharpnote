using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpnote.Interfaces
{
    public interface INote
    {
        string Key { get; set; }
        string Content { get; set; }
        DateTime Modified { get; set; }
        DateTime Created { get; set; }
    }
}

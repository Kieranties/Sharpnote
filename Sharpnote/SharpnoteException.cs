using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpnote
{
    public class SharpnoteException: Exception  {}

    public class SharpnoteAuthorisationException : SharpnoteException
    {
        public override string Message
        {
            get
            {
                return "Simplenote authroisation key has expired, is invalid or has not been set.  Re-authenticate the user";
            }
        }
    }

    public class SharpnoteNonExistentNoteException : SharpnoteException
    {
        private string _key;
        public override string Message
        {
            get
            {
                return string.Format("No note found for key: {0}", _key);
            }
        }

        public SharpnoteNonExistentNoteException (string key)
	    {
            _key = key;
	    }
    }
}

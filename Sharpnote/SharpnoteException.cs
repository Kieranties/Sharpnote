using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpnote
{
    /// <summary>
    /// Base Exception class for errors thrown by Sharpnote
    /// </summary>
    public class SharpnoteException : Exception
    {
        /// <summary>
        /// Creates an instance of SharpnoteException
        /// </summary>
        public SharpnoteException(){}
        /// <summary>
        /// Creates an instance of SharpnoteException
        /// </summary>
        /// <param name="message">The message for the exception</param>
        /// <param name="ex">A previous exception</param>
        public SharpnoteException(string message , Exception ex) : base(message, ex) { }
    }

    /// <summary>
    /// Thrown in the event of failing to authorise a user
    /// </summary>
    public class SharpnoteAuthorisationException : SharpnoteException
    {
        public override string Message
        {
            get
            {
                return "Simplenote authroisation key has expired, is invalid or has not been set.  Re-authenticate the user";
            }
        }

        public SharpnoteAuthorisationException(Exception ex = null):base(null, ex)
        {

        }
    }

    /// <summary>
    /// Thrown in the event a non existent key is referenced
    /// </summary>
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

        public SharpnoteNonExistentNoteException (string key, Exception ex = null):base(null, ex)
	    {
            
            _key = key;
	    }

    }
}

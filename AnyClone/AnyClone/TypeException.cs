using System;
using System.Collections.Generic;
using System.Text;

namespace AnyClone
{
    /// <summary>
    /// Type exception
    /// </summary>
    public class TypeException : Exception
    {
        /// <summary>
        /// Type exception
        /// </summary>
        /// <param name="message">Exception message to display</param>
        public TypeException(string message) : base(message)
        {

        }
    }
}

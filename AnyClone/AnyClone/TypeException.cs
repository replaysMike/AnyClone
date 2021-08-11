using System;

namespace AnyClone
{
    /// <summary>
    /// Type exception
    /// </summary>
    [Serializable]
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

using System;

namespace AnyClone
{
    /// <summary>
    /// Cloning exception
    /// </summary>
    [Serializable]
    public class CloneException : Exception
    {
        /// <summary>
        /// The current recursion path
        /// </summary>
        public string Path { get; set;}

        /// <summary>
        /// Create a new cloning exception
        /// </summary>
        /// <param name="message"></param>
        public CloneException(string message) : base(message)
        {

        }

        /// <summary>
        /// Create a new cloning exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="path">The current recursion path</param>
        public CloneException(string message, string path) : base(message)
        {
            Path = path;
        }
    }
}

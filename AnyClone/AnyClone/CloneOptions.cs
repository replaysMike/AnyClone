using System;

namespace AnyClone
{
    /// <summary>
    /// The cloning options
    /// </summary>
    [Flags]
    public enum CloneOptions
    {
        /// <summary>
        /// No options specified
        /// </summary>
        None = 0,

        /// <summary>
        /// Specify if you want disable the ignore by attribute feature
        /// </summary>
        DisableIgnoreAttributes = 1,
    }
}

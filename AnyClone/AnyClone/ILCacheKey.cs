using System;

namespace AnyClone
{
    public struct ILCacheKey
    {
        /// <summary>
        /// The name of the type
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// The name of the type's member field
        /// </summary>
        public string MemberName { get; set; }

        public ILCacheKey(Type type, string memberName)
        {
            Type = type;
            MemberName = memberName;
        }
    }
}

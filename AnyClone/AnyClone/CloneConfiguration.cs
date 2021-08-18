using System;
using System.Collections.Generic;
using System.Linq;

namespace AnyClone
{
    /// <summary>
    /// Configuration options for configuring cloning behaviors
    /// </summary>
    public class CloneConfiguration
    {
        /// <summary>
        /// Set the maximum recursion depth
        /// </summary>
        public const int DefaultMaxDepth = 64;

        public const ReferenceTrackingType DefaultReferenceTrackingType = ReferenceTrackingType.ObjectIdGenerator;

        /// <summary>
        /// Ignore properties decorated with a custom list of attribute names
        /// Default: ["IgnoreDataMemberAttribute", "NonSerializedAttribute", "JsonIgnoreAttribute"]
        /// </summary>
        public ICollection<string> IgnorePropertiesWithAttributes { get; set; } = new List<string> {
            "IgnoreDataMemberAttribute",
            "NonSerializedAttribute",
            "JsonIgnoreAttribute",
        };

        /// <summary>
        /// True to allow cloning of readonly fields and properties
        /// Default: true
        /// </summary>
        public bool AllowCloningOfReadOnlyEntities { get; set; } = true;

        /// <summary>
        /// True to use custom hashcode implementations for reference tracking, false to use the system generated hashcode
        /// Default: true
        /// </summary>
        public bool AllowUseCustomHashCodes { get; set; } = true;

        /// <summary>
        /// True to respect custom implementations of IClonable
        /// Default: false
        /// </summary>
        public bool AllowIClonableImplementations { get; set; } = false;

        /// <summary>
        /// Specify the type of object instance reference tracking used
        /// Default: <seealso cref="DefaultReferenceTrackingType"/>
        /// </summary>
        public ReferenceTrackingType ReferenceTrackingType { get; set; } = DefaultReferenceTrackingType;

        /// <summary>
        /// Set the maximum recursion depth for cloning fields and properties
        /// Default: 32
        /// </summary>
        public int MaxDepth { get; set; } = DefaultMaxDepth;

        /// <summary>
        /// Create a clone configuration using a list of attributes to ignore
        /// </summary>
        /// <param name="attributesToIgnore"></param>
        /// <returns></returns>
        public static CloneConfiguration UsingAttributesToIgnore(params Type[] attributesToIgnore)
            => new() {
                IgnorePropertiesWithAttributes = attributesToIgnore.Select(x => x.Name).ToList()
            };

        /// <summary>
        /// Create a clone configuration using a list of attribute names to ignore
        /// </summary>
        /// <param name="attributeNamesToIgnore"></param>
        /// <returns></returns>
        public static CloneConfiguration UsingAttributeNamesToIgnore(params string[] attributeNamesToIgnore)
            => new() {
                IgnorePropertiesWithAttributes = attributeNamesToIgnore
            };

        /// <summary>
        /// Create a clone configuration that does not allow readonly fields and properties to be cloned
        /// </summary>
        /// <param name="attributeNamesToIgnore"></param>
        /// <returns></returns>
        public static CloneConfiguration SkipReadOnlyMembers()
            => new() {
                AllowCloningOfReadOnlyEntities = false
            };

        internal static CloneConfiguration CreateFromOptions(CloneOptions options)
            => options switch {
                CloneOptions.DisableIgnoreAttributes => new CloneConfiguration {
                    IgnorePropertiesWithAttributes = new List<string>()
                },
                CloneOptions.None => new CloneConfiguration(),
                _ => new CloneConfiguration()
            };
    }
}

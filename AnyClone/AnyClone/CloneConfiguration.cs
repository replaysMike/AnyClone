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
        /// Ignore properties decorated with a custom list of attribute names
        /// Default: ["IgnoreDataMemberAttribute", "NonSerializedAttribute", "JsonIgnoreAttribute"]
        /// </summary>
        public ICollection<string> IgnorePropertiesWithAttributes { get; set; } = new List<string> {
            "IgnoreDataMemberAttribute",
            "NonSerializedAttribute",
            "JsonIgnoreAttribute",
        };

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

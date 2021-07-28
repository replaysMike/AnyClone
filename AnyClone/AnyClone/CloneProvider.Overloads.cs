using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AnyClone
{
    public partial class CloneProvider<T>
    {
        /// <summary>
        /// Clone any objects
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <returns></returns>
        public T Clone(T sourceObject)
            => (T)InspectAndCopy(sourceObject, 0, DefaultMaxDepth, new CloneConfiguration(), new Dictionary<int, object>(), string.Empty);

        /// <summary>
        /// Clone any objects
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="options">The cloning options</param>
        /// <returns></returns>
        public T Clone(T sourceObject, CloneOptions options)
            => (T)InspectAndCopy(sourceObject, 0, DefaultMaxDepth, CloneConfiguration.CreateFromOptions(options), new Dictionary<int, object>(), string.Empty);

        /// <summary>
        /// Clone any objects
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="configuration">Configure custom cloning options</param>
        /// <returns></returns>
        public T Clone(T sourceObject, CloneConfiguration configuration)
            => (T)InspectAndCopy(sourceObject, 0, DefaultMaxDepth, configuration, new Dictionary<int, object>(), string.Empty);

        /// <summary>
        /// Clone any objects
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="options">The cloning options</param>
        /// <param name="maxTreeDepth">The maximum tree depth</param>
        /// <returns></returns>
        public T Clone(T sourceObject, CloneOptions options, int maxTreeDepth)
            => (T)InspectAndCopy(sourceObject, 0, maxTreeDepth, CloneConfiguration.CreateFromOptions(options), new Dictionary<int, object>(), string.Empty);

        /// <summary>
        /// Clone any objects
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="options">The cloning options</param>
        /// <param name="maxTreeDepth">The maximum tree depth</param>
        /// <returns></returns>
        public T Clone(T sourceObject, CloneOptions options, int maxTreeDepth, params string[] ignorePropertiesOrPaths)
            => (T)InspectAndCopy(sourceObject, 0, maxTreeDepth, CloneConfiguration.CreateFromOptions(options), new Dictionary<int, object>(), string.Empty, ignorePropertiesOrPaths);

        /// <summary>
        /// Clone any objects
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="configuration">Configure custom cloning options</param>
        /// <param name="maxTreeDepth">The maximum tree depth</param>
        /// <returns></returns>
        public T Clone(T sourceObject, CloneConfiguration configuration, int maxTreeDepth, params string[] ignorePropertiesOrPaths)
            => (T)InspectAndCopy(sourceObject, 0, maxTreeDepth, configuration, new Dictionary<int, object>(), string.Empty, ignorePropertiesOrPaths);

        /// <summary>
        /// Clone any objects
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="options">The cloning options</param>
        /// <param name="maxTreeDepth">The maximum tree depth</param>
        /// <returns></returns>
        public T Clone(T sourceObject, CloneOptions options, int maxTreeDepth, params Expression<Func<T, object>>[] ignoreProperties)
            => (T)InspectAndCopy(sourceObject, 0, maxTreeDepth, CloneConfiguration.CreateFromOptions(options), new Dictionary<int, object>(), string.Empty, ConvertToPropertyNameList(ignoreProperties));

        /// <summary>
        /// Clone any objects
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="configuration">Configure custom cloning options</param>
        /// <param name="maxTreeDepth">The maximum tree depth</param>
        /// <returns></returns>
        public T Clone(T sourceObject, CloneConfiguration configuration, int maxTreeDepth, params Expression<Func<T, object>>[] ignoreProperties)
            => (T)InspectAndCopy(sourceObject, 0, maxTreeDepth, configuration, new Dictionary<int, object>(), string.Empty, ConvertToPropertyNameList(ignoreProperties));

        /// <summary>
        /// (Recursive) Recursive function that inspects an object and its properties/fields and clones it
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="currentDepth">The current tree depth</param>
        /// <param name="maxDepth">The max tree depth</param>
        /// <param name="configuration">The cloning options</param>
        /// <param name="objectTree">The object tree to prevent cyclical references</param>
        /// <param name="path">The current path being traversed</param>
        /// <returns></returns>
        private object InspectAndCopy(object sourceObject, int currentDepth, int maxDepth, CloneConfiguration configuration, IDictionary<int, object> objectTree, string path)
            => InspectAndCopy(sourceObject, currentDepth, maxDepth, configuration, objectTree, path, null);
    }
}

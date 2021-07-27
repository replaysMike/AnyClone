using System;
using System.Linq.Expressions;

namespace AnyClone
{
    /// <summary>
    /// Clone any object
    /// </summary>
    public static class Cloner
    {
        /// <summary>
        /// Clone any object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject">The object to clone</param>
        /// <returns></returns>
        public static T Clone<T>(this T sourceObject)
            => Clone(sourceObject, CloneOptions.None);

        /// <summary>
        /// Clone any object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="options">Specify the Clone options</param>
        /// <returns></returns>
        public static T Clone<T>(this T sourceObject, CloneOptions options)
        {
            var cloneEngine = new CloneProvider<T>();
            return cloneEngine.Clone(sourceObject, options);
        }

        /// <summary>
        /// Clone any object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="ignorePropertiesOrPaths">A list of property names or fully qualified paths to ignore</param>
        /// <returns></returns>
        public static T Clone<T>(this T sourceObject, params string[] ignorePropertiesOrPaths)
            => Clone(sourceObject, CloneOptions.None, ignorePropertiesOrPaths);

        /// <summary>
        /// Clone any object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="ignoreProperties">A list of property names to ignore</param>
        /// <returns></returns>
        public static T Clone<T>(this T sourceObject, params Expression<Func<T, object>>[] ignoreProperties)
            => Clone(sourceObject, CloneOptions.None, ignoreProperties);

        /// <summary>
        /// Clone any object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="onErrorCallback">An error callback to receive cloning errors</param>
        /// <returns></returns>
        public static T Clone<T>(this T sourceObject, Action<Exception, string, object, object> onErrorCallback)
            => Clone(sourceObject, CloneOptions.None, onErrorCallback);

        /// <summary>
        /// Clone any object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="options">Specify the Clone options</param>
        /// <param name="ignorePropertiesOrPaths">A list of property names or fully qualified paths to ignore</param>
        /// <returns></returns>
        public static T Clone<T>(this T sourceObject, CloneOptions options, params string[] ignorePropertiesOrPaths)
        {
            var cloneEngine = new CloneProvider<T>();
            return cloneEngine.Clone(sourceObject, options, CloneProvider<T>.DefaultMaxDepth, ignorePropertiesOrPaths);
        }

        /// <summary>
        /// Clone any object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="options">Specify the Clone options</param>
        /// <param name="ignoreProperties">A list of property names to ignore</param>
        /// <returns></returns>
        public static T Clone<T>(this T sourceObject, CloneOptions options, params Expression<Func<T, object>>[] ignoreProperties)
        {
            var cloneEngine = new CloneProvider<T>();
            return cloneEngine.Clone(sourceObject, options, CloneProvider<T>.DefaultMaxDepth, ignoreProperties);
        }

        /// <summary>
        /// Clone any object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="options"></param>
        /// <param name="onErrorCallback">An error callback to receive cloning errors</param>
        /// <returns></returns>
        public static T Clone<T>(this T sourceObject, CloneOptions options, Action<Exception, string, object, object> onErrorCallback)
        {
            var cloneEngine = new CloneProvider<T>
            {
                OnCloneError = onErrorCallback
            };
            return cloneEngine.Clone(sourceObject, options);
        }
    }
}

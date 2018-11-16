using System;

namespace AnyClone
{
    public static class CloneExtensions
    {
        /// <summary>
        /// Clone any object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject">The object to clone</param>
        /// <returns></returns>
        public static T Clone<T>(this T sourceObject)
        {
            var cloneEngine = new CloneProvider<T>();
            return cloneEngine.Clone(sourceObject);
        }

        /// <summary>
        /// Clone any object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="onErrorCallback">An error callback to receive cloning errors</param>
        /// <returns></returns>
        public static T Clone<T>(this T sourceObject, Action<Exception, string, object, object> onErrorCallback)
        {
            var cloneEngine = new CloneProvider<T>();
            cloneEngine.OnCloneError = onErrorCallback;
            var result = cloneEngine.Clone(sourceObject);
            return result;
        }
    }
}

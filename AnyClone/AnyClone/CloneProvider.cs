using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TypeSupport;
using TypeSupport.Extensions;

namespace AnyClone
{
    /// <summary>
    /// Provider for cloning objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CloneProvider<T>
    {
        private const int DefaultMaxDepth = 1000;
        private readonly Type _type;
        private readonly ObjectFactory _objectFactory;
        private readonly ICollection<Type> _ignoreAttributes = new List<Type> {
            typeof(IgnoreDataMemberAttribute),
            typeof(NonSerializedAttribute),
            typeof(JsonIgnoreAttribute),
        };

        public Action<Exception, string, object, object> OnCloneError { get; set; }

        /// <summary>
        /// Provider for cloning objects
        /// </summary>
        public CloneProvider()
        {
            _type = typeof(T);
            _objectFactory = new ObjectFactory();
        }

        /// <summary>
        /// Clone any objects
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="options">The cloning options</param>
        /// <param name="maxTreeDepth">The maximum tree depth</param>
        /// <returns></returns>
        public T Clone(T sourceObject, CloneOptions options = CloneOptions.None, int maxTreeDepth = DefaultMaxDepth)
        {
            return (T)InspectAndCopy(sourceObject, 0, maxTreeDepth, options, new Dictionary<int, object>(), string.Empty);
        }

        /// <summary>
        /// (Recursive) Recursive function that inspects an object and its properties/fields and clones it
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="currentDepth">The current tree depth</param>
        /// <param name="maxDepth">The max tree depth</param>
        /// <param name="options">The cloning options</param>
        /// <param name="objectTree">The object tree to prevent cyclical references</param>
        /// <param name="path">The current path being traversed</param>
        /// <returns></returns>
        private object InspectAndCopy(object sourceObject, int currentDepth, int maxDepth, CloneOptions options, IDictionary<int, object> objectTree, string path)
        {
            if (sourceObject == null)
                return null;

            // ensure we don't go too deep if specified
            if (maxDepth > 0 && currentDepth >= maxDepth)
                return null;

            var typeSupport = new ExtendedType(sourceObject.GetType());

            // drop any objects we are ignoring by attribute
            if (typeSupport.Attributes.Any(x => _ignoreAttributes.Contains(x)) && options.BitwiseHasFlag(CloneOptions.DisableIgnoreAttributes))
                return null;

            // for delegate types, copy them by reference rather than returning null
            if (typeSupport.IsDelegate)
                return sourceObject;

            object newObject = null;
            // create a new empty object of the desired type
            if (typeSupport.IsArray)
            {
                var length = 0;
                if(typeSupport.IsArray)
                    length = (sourceObject as Array).Length;
                newObject = _objectFactory.CreateEmptyObject(typeSupport.Type, length: length);
            }
            else
            {
                newObject = _objectFactory.CreateEmptyObject(typeSupport.Type);
            }

            // increment the current recursion depth
            currentDepth++;

            // construct a hashtable of objects we have already inspected (simple recursion loop preventer)
            // we use this hashcode method as it does not use any custom hashcode handlers the object might implement
            if (sourceObject != null)
            {
                var hashCode = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(sourceObject);
                if (objectTree.ContainsKey(hashCode))
                    return objectTree[hashCode];

                // ensure we can refer back to the reference for this object
                objectTree.Add(hashCode, newObject);
            }
            try
            {
                // clone a dictionary's key/values
                if (typeSupport.IsDictionary && typeSupport.IsGeneric)
                {
                    var genericType = typeSupport.Type.GetGenericArguments().ToList();
                    Type[] typeArgs = { genericType[0], genericType[1] };

                    var listType = typeof(Dictionary<,>).MakeGenericType(typeArgs);
                    var newDictionary = Activator.CreateInstance(listType) as IDictionary;
                    newObject = newDictionary;
                    var enumerator = (IDictionary)sourceObject;
                    foreach (DictionaryEntry item in enumerator)
                    {
                        var key = InspectAndCopy(item.Key, currentDepth, maxDepth, options, objectTree, path);
                        var value = InspectAndCopy(item.Value, currentDepth, maxDepth, options, objectTree, path);
                        newDictionary.Add(key, value);
                    }
                    return newObject;
                }

                // clone an enumerables' elements
                if (typeSupport.IsEnumerable && typeSupport.IsGeneric)
                {
                    var genericType = typeSupport.Type.GetGenericArguments().First();
                    var genericExtendedType = new ExtendedType(genericType);
                    var addMethod = typeSupport.Type.GetMethod("Add");
                    var enumerator = (IEnumerable)sourceObject;
                    foreach (var item in enumerator)
                    {
                        var element = InspectAndCopy(item, currentDepth, maxDepth, options, objectTree, path);
                        addMethod.Invoke(newObject, new object[] { element });
                    }
                    return newObject;
                }

                // clone an arrays' elements
                if (typeSupport.IsArray)
                {
                    var sourceArray = sourceObject as Array;
                    var newArray = newObject as Array;
                    newObject = newArray;
                    for (var i = 0; i < sourceArray.Length; i++)
                    {
                        var element = sourceArray.GetValue(i);
                        var newElement = InspectAndCopy(element, currentDepth, maxDepth, options, objectTree, path);
                        newArray.SetValue(newElement, i);
                    }
                    return newArray;
                }

                var fields = sourceObject.GetFields(FieldOptions.AllWritable);

                var rootPath = path;
                // clone and recurse fields
                foreach (var field in fields)
                {
                    path = $"{rootPath}.{field.Name}";
#if FEATURE_CUSTOM_ATTRIBUTES
                    if (field.CustomAttributes.Any(x => _ignoreAttributes.Contains(x.AttributeType)) && options.BitwiseHasFlag(CloneOptions.DisableIgnoreAttributes))
#else
                    if (field.CustomAttributes.Any(x => _ignoreAttributes.Contains(x.Constructor.DeclaringType)) && options.BitwiseHasFlag(CloneOptions.DisableIgnoreAttributes))
#endif
                        continue;
                    var fieldTypeSupport = new ExtendedType(field.Type);
                    var fieldValue = sourceObject.GetFieldValue(field);
                    if (fieldTypeSupport.IsValueType || fieldTypeSupport.IsImmutable)
                        newObject.SetFieldValue(field, fieldValue);
                    else if (fieldValue != null)
                    {
                        var clonedFieldValue = InspectAndCopy(fieldValue, currentDepth, maxDepth, options, objectTree, path);
                        newObject.SetFieldValue(field, clonedFieldValue);
                    }
                }

                return newObject;
            }
            finally
            {

            }
        }
    }
}

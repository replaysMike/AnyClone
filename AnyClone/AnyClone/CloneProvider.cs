using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using TypeSupport;
using TypeSupport.Extensions;

namespace AnyClone
{
    /// <summary>
    /// Provider for cloning objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CloneProvider<T> : CloneProvider
    {
        private readonly ObjectFactory _objectFactory;
		private const TypeSupportOptions DefaultTypeSupportOptions = TypeSupportOptions.Attributes | TypeSupportOptions.Collections | TypeSupportOptions.Enums | TypeSupportOptions.Generics | TypeSupportOptions.Caching;

		private readonly ICollection<object> _ignoreAttributes = new List<object> {
            typeof(IgnoreDataMemberAttribute),
            typeof(NonSerializedAttribute),
            "JsonIgnoreAttribute",
        };

        public Action<Exception, string, object, object> OnCloneError { get; set; }

        /// <summary>
        /// Provider for cloning objects
        /// </summary>
        public CloneProvider()
        {
            _objectFactory = new ObjectFactory();
        }

        /// <summary>
        /// Clone any objects
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <returns></returns>
        public T Clone(T sourceObject)
        {
            return (T)InspectAndCopy(sourceObject, 0, DefaultMaxDepth, CloneOptions.None, new Dictionary<int, object>(), string.Empty);
        }

        /// <summary>
        /// Clone any objects
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="options">The cloning options</param>
        /// <returns></returns>
        public T Clone(T sourceObject, CloneOptions options)
        {
            return (T)InspectAndCopy(sourceObject, 0, DefaultMaxDepth, options, new Dictionary<int, object>(), string.Empty);
        }

        /// <summary>
        /// Clone any objects
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="options">The cloning options</param>
        /// <param name="maxTreeDepth">The maximum tree depth</param>
        /// <returns></returns>
        public T Clone(T sourceObject, CloneOptions options, int maxTreeDepth)
        {
            return (T)InspectAndCopy(sourceObject, 0, maxTreeDepth, options, new Dictionary<int, object>(), string.Empty);
        }

        /// <summary>
        /// Clone any objects
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="options">The cloning options</param>
        /// <param name="maxTreeDepth">The maximum tree depth</param>
        /// <returns></returns>
        public T Clone(T sourceObject, CloneOptions options, int maxTreeDepth, params string[] ignorePropertiesOrPaths)
        {
            return (T)InspectAndCopy(sourceObject, 0, maxTreeDepth, options, new Dictionary<int, object>(), string.Empty, ignorePropertiesOrPaths);
        }

        /// <summary>
        /// Clone any objects
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="options">The cloning options</param>
        /// <param name="maxTreeDepth">The maximum tree depth</param>
        /// <returns></returns>
        public T Clone(T sourceObject, CloneOptions options, int maxTreeDepth, params Expression<Func<T, object>>[] ignoreProperties)
        {
            return (T)InspectAndCopy(sourceObject, 0, maxTreeDepth, options, new Dictionary<int, object>(), string.Empty, ConvertToPropertyNameList(ignoreProperties));
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
            return InspectAndCopy(sourceObject, currentDepth, maxDepth, options, objectTree, path, null);
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
        /// <param name="ignorePropertiesOrPaths">A list of properties or paths to ignore</param>
        /// <returns></returns>
        private object InspectAndCopy(object sourceObject, int currentDepth, int maxDepth, CloneOptions options, IDictionary<int, object> objectTree, string path, ICollection<string> ignorePropertiesOrPaths)
        {
            if (IgnoreObjectName(null, path, options, ignorePropertiesOrPaths))
                return null;

            if (sourceObject == null)
                return null;

            // ensure we don't go too deep if specified
            if (maxDepth > 0 && currentDepth >= maxDepth)
                return null;

            var type = sourceObject.GetType();
            var typeSupport = type.GetExtendedType();

            // always return the original value on value types
            if (typeSupport.IsValueType)
            {
                return sourceObject;
            }

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
                var sourceArray = sourceObject as Array;
                // calculate the dimensions of the array
                var arrayRank = sourceArray.Rank;
                // get the length of each dimension
                var arrayDimensions = new List<int>();
                for (var dimension = 0; dimension < arrayRank; dimension++)
                    arrayDimensions.Add(sourceArray.GetLength(dimension));
                newObject = _objectFactory.CreateEmptyObject(typeSupport.Type, default(TypeRegistry), arrayDimensions.ToArray());
            }
            else if (typeSupport.Type == typeof(string))
            {
                // copy the item directly
                newObject = String.Copy((string)sourceObject);
                return newObject;
            }
            else
            {
                newObject = _objectFactory.CreateEmptyObject(typeSupport.Type);
            }

            if (newObject == null)
                return newObject;

            // increment the current recursion depth
            currentDepth++;

            // construct a hashtable of objects we have already inspected (simple recursion loop preventer)
            // we use this hashcode method as it does not use any custom hashcode handlers the object might implement
            if (sourceObject != null && !typeSupport.IsValueType)
            {
                var hashCode = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(sourceObject);
                if (objectTree.ContainsKey(hashCode))
                {
                    if (objectTree[hashCode].GetType() == type)
                        return objectTree[hashCode];

                }
                else
                {
                    // ensure we can refer back to the reference for this object
                    objectTree.Add(hashCode, newObject);
                }
            }

            // clone a dictionary's key/values
            if (typeSupport.IsDictionary && typeSupport.IsGeneric)
            {
                var genericType = typeSupport.Type.GetGenericArguments().ToList();
                Type[] typeArgs = { genericType[0], genericType[1] };

                var listType = typeof(Dictionary<,>).MakeGenericType(typeArgs);
                var newDictionary = Activator.CreateInstance(listType) as IDictionary;
                newObject = newDictionary;
                var iDictionary = (IDictionary)sourceObject;
                var success = false;
                var retryCount = 0;
                while (!success && retryCount < 10)
                {
                    try
                    {
                        foreach (DictionaryEntry item in iDictionary)
                        {
                            var key = InspectAndCopy(item.Key, currentDepth, maxDepth, options, objectTree, path, ignorePropertiesOrPaths);
                            var value = InspectAndCopy(item.Value, currentDepth, maxDepth, options, objectTree, path, ignorePropertiesOrPaths);
                            newDictionary.Add(key, value);
                        }
                        success = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // if the collection was modified during enumeration, stop re-initialize and retry
                        success = false;
                        retryCount++;
                        newDictionary.Clear();
                    }
                }
                return newObject;
            }
            else if (typeSupport.IsEnumerable && !typeSupport.IsArray)
            {
                // clone an enumerables' elements
                var addMethod = typeSupport.Type.GetMethod("Add");
                if (addMethod == null)
                    addMethod = typeSupport.Type.GetMethod("Enqueue");
                if (addMethod == null)
                    addMethod = typeSupport.Type.GetMethod("Push");
                if (addMethod == null)
                    throw new TypeException($"Unsupported IEnumerable type: {typeSupport.Type.Name}");
                var enumerator = (IEnumerable)sourceObject;
                var success = false;
                var retryCount = 0;
                while (!success && retryCount < 10)
                {
                    try
                    {
                        foreach (var item in enumerator)
                        {
                            var element = InspectAndCopy(item, currentDepth, maxDepth, options, objectTree, path, ignorePropertiesOrPaths);
                            addMethod.Invoke(newObject, new[] { element });
                        }
                        success = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // if the collection was modified during enumeration, stop re-initialize and retry
                        success = false;
                        retryCount++;
                        var clearMethod = typeSupport.Type.GetMethod("Clear");
                        clearMethod?.Invoke(newObject, null);
                    }
                }
                return newObject;
            }

            // clone an arrays' elements
            if (typeSupport.IsArray)
            {
                var sourceArray = sourceObject as Array;
                var newArray = newObject as Array;
                var arrayRank = newArray.Rank;
                var arrayDimensions = new List<int>();
                for (var dimension = 0; dimension < arrayRank; dimension++)
                    arrayDimensions.Add(newArray.GetLength(dimension));
                var flatRowIndex = 0;
                foreach(var row in sourceArray)
                {
                    var newElement = InspectAndCopy(row, currentDepth, maxDepth, options, objectTree, path, ignorePropertiesOrPaths);
                    // performance optimization, skip dimensional processing if it's a 1d array
                    if (arrayRank > 1)
                    {
                        // this is an optimized multi-dimensional array reconstruction
                        // based on the formula: indicies.Add((i / (arrayDimensions[arrayRank - 1] * arrayDimensions[arrayRank - 2] * arrayDimensions[arrayRank - 3] * arrayDimensions[arrayRank - 4] * arrayDimensions[arrayRank - 5])) % arrayDimensions[arrayRank - 6]);
                        var indicies = new List<int>();
                        for (var r = 1; r <= arrayRank; r++)
                        {
                            var multi = 1;
                            for (var p = 1; p < r; p++)
                            {
                                multi *= arrayDimensions[arrayRank - p];
                            }
                            var b = (flatRowIndex / multi) % arrayDimensions[arrayRank - r];
                            indicies.Add(b);
                        }
                        indicies.Reverse();
                        // set element of multi-dimensional array
                        newArray.SetValue(newElement, indicies.ToArray());
                    }
                    else
                    {
                        // set element of 1d array
                        newArray.SetValue(newElement, flatRowIndex);
                    }
                    flatRowIndex++;
                }
                return newArray;
            }

            var fields = sourceObject.GetFields(FieldOptions.AllWritable);

            var rootPath = path;
            var localPath = string.Empty;
            // clone and recurse fields
            if (newObject != null)
            {
                foreach (var field in fields)
                {
                    localPath = $"{rootPath}.{field.Name}";
                    if (IgnoreObjectName(field.Name, localPath, options, ignorePropertiesOrPaths, field.CustomAttributes))
                        continue;
                    // also check the property for ignore, if this is a auto-backing property
                    if (field.BackedProperty != null && IgnoreObjectName(field.BackedProperty.Name, $"{rootPath}.{field.BackedPropertyName}", options, ignorePropertiesOrPaths, field.BackedProperty.CustomAttributes))
                        continue;
#if FEATURE_DISABLE_SET_INITONLY
                    // we can't duplicate init-only fields since .net core 3.0+
                    if (field.FieldInfo.IsInitOnly)
                        continue;
#endif
                    var fieldTypeSupport = field.Type;
                    var fieldValue = sourceObject.GetFieldValue(field);
                    if (fieldTypeSupport.IsValueType || fieldTypeSupport.IsImmutable)
                        newObject.SetFieldValue(field, fieldValue);
                    else if (fieldValue != null)
                    {
                        var clonedFieldValue = InspectAndCopy(fieldValue, currentDepth, maxDepth, options, objectTree, localPath, ignorePropertiesOrPaths);
                        newObject.SetFieldValue(field, clonedFieldValue);
                    }
                }
            }

            return newObject;
        }

        /// <summary>
        /// Returns true if object name should be ignored
        /// </summary>
        /// <param name="name">Property or field name</param>
        /// <param name="path">Full path to object</param>
        /// <param name="options">Comparison options</param>
        /// <param name="ignorePropertiesOrPaths">List of names or paths to ignore</param>
        /// <returns></returns>
        private bool IgnoreObjectName(string name, string path, CloneOptions options, ICollection<string> ignorePropertiesOrPaths, IEnumerable<CustomAttributeData> attributes = null)
        {
            var ignoreByNameOrPath = ignorePropertiesOrPaths?.Contains(name) == true || ignorePropertiesOrPaths?.Contains(path) == true;
            if (ignoreByNameOrPath)
                return true;
#if FEATURE_CUSTOM_ATTRIBUTES
            if (attributes?.Any(x => !options.BitwiseHasFlag(CloneOptions.DisableIgnoreAttributes) && (_ignoreAttributes.Contains(x.AttributeType) || _ignoreAttributes.Contains(x.AttributeType.Name))) == true)
#else
            if (attributes?.Any(x => !options.BitwiseHasFlag(CloneOptions.DisableIgnoreAttributes) && (_ignoreAttributes.Contains(x.Constructor.DeclaringType) || _ignoreAttributes.Contains(x.Constructor.DeclaringType.Name))) == true)
#endif
                return true;
            return false;
        }

        /// <summary>
        /// Convert an expression of properties to a list of property names
        /// </summary>
        /// <param name="ignoreProperties"></param>
        /// <returns></returns>
        private static ICollection<string> ConvertToPropertyNameList(Expression<Func<T, object>>[] ignoreProperties)
        {
            var ignorePropertiesList = new List<string>();
            foreach (var expression in ignoreProperties)
            {
                var name = "";
                switch (expression.Body)
                {
                    case MemberExpression m:
                        name = m.Member.Name;
                        break;
                    case UnaryExpression u when u.Operand is MemberExpression m:
                        name = m.Member.Name;
                        break;
                    default:
                        throw new NotImplementedException(expression.GetType().ToString());
                }
                ignorePropertiesList.Add(name);
            }
            return ignorePropertiesList;
        }
    }

    public class CloneProvider
    {
        public static readonly int DefaultMaxDepth = 32;

        protected CloneProvider() { }
    }
}

﻿using System;
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
    public class CloneProvider<T>
    {
        public const int DefaultMaxDepth = 32;
        private readonly Type _type;
        private readonly ObjectFactory _objectFactory;
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
        private object InspectAndCopy(object sourceObject, int currentDepth, int maxDepth, CloneOptions options, IDictionary<int, object> objectTree, string path, ICollection<string> ignorePropertiesOrPaths = null)
        {
            if (IgnoreObjectName(null, path, options, ignorePropertiesOrPaths))
                return null;

            if (sourceObject == null)
                return null;

            // ensure we don't go too deep if specified
            if (maxDepth > 0 && currentDepth >= maxDepth)
                return null;

            if (ignorePropertiesOrPaths == null)
                ignorePropertiesOrPaths = new List<string>();

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
                if (typeSupport.IsArray)
                    length = (sourceObject as Array).Length;
                newObject = _objectFactory.CreateEmptyObject(typeSupport.Type, length: length);
            }
            else if(typeSupport.Type == typeof(string))
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
                        var key = InspectAndCopy(item.Key, currentDepth, maxDepth, options, objectTree, path, ignorePropertiesOrPaths);
                        var value = InspectAndCopy(item.Value, currentDepth, maxDepth, options, objectTree, path, ignorePropertiesOrPaths);
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
                        var element = InspectAndCopy(item, currentDepth, maxDepth, options, objectTree, path, ignorePropertiesOrPaths);
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
                        var newElement = InspectAndCopy(element, currentDepth, maxDepth, options, objectTree, path, ignorePropertiesOrPaths);
                        newArray.SetValue(newElement, i);
                    }
                    return newArray;
                }

                var fields = sourceObject.GetFields(FieldOptions.AllWritable);

                var rootPath = path;
                // clone and recurse fields
                if (newObject != null)
                {
                    foreach (var field in fields)
                    {
                        path = $"{rootPath}.{field.Name}";
                        if (IgnoreObjectName(field.Name, path, options, ignorePropertiesOrPaths, field.CustomAttributes))
                            continue;
                        // also check the property for ignore, if this is a auto-backing property
                        if (field.BackedProperty != null && IgnoreObjectName(field.BackedProperty.Name, $"{rootPath}.{field.BackedPropertyName}", options, ignorePropertiesOrPaths, field.BackedProperty.CustomAttributes))
                            continue;
                        var fieldTypeSupport = new ExtendedType(field.Type);
                        var fieldValue = sourceObject.GetFieldValue(field);
                        if (fieldTypeSupport.IsValueType || fieldTypeSupport.IsImmutable)
                            newObject.SetFieldValue(field, fieldValue);
                        else if (fieldValue != null)
                        {
                            var clonedFieldValue = InspectAndCopy(fieldValue, currentDepth, maxDepth, options, objectTree, path, ignorePropertiesOrPaths);
                            newObject.SetFieldValue(field, clonedFieldValue);
                        }
                    }
                }

                return newObject;
            }
            finally
            {

            }
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
        private ICollection<string> ConvertToPropertyNameList(Expression<Func<T, object>>[] ignoreProperties)
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
}

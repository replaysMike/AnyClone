using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

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
        }

        /// <summary>
        /// Clone any objects
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="maxTreeDepth">The maximum tree depth</param>
        /// <returns></returns>
        public T Clone(T sourceObject, int maxTreeDepth = DefaultMaxDepth)
        {
            return (T)InspectAndCopy(sourceObject, 0, maxTreeDepth, new Dictionary<int, object>(), string.Empty);
        }

        /// <summary>
        /// (Recursive) Recursive function that inspects an object and its properties/fields and clones it
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="currentDepth">The current tree depth</param>
        /// <param name="maxDepth">The max tree depth</param>
        /// <param name="objectTree">The object tree to prevent cyclical references</param>
        /// <param name="path">The current path being traversed</param>
        /// <returns></returns>
        private object InspectAndCopy(object sourceObject, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path)
        {
            if (sourceObject == null)
                return null;

            // ensure we don't go too deep if specified
            if (maxDepth > 0 && currentDepth >= maxDepth)
                return null;

            var typeSupport = new TypeSupport(sourceObject.GetType());

            // drop any objects we are ignoring by attribute
            if (typeSupport.Attributes.Any(x => _ignoreAttributes.Contains(x)))
                return null;

            // for delegate types, copy them by reference rather than returning null
            if (typeSupport.IsDelegate)
                return sourceObject;

            // create a new empty object of the desired type
            var newObject = CreateEmptyObject(typeSupport.Type, sourceObject);

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
                        var key = InspectAndCopy(item.Key, currentDepth, maxDepth, objectTree, path);
                        var value = InspectAndCopy(item.Value, currentDepth, maxDepth, objectTree, path);
                        newDictionary.Add(key, value);
                    }
                    return newObject;
                }

                // clone an enumerables' elements
                if (typeSupport.IsEnumerable && typeSupport.IsGeneric)
                {
                    var genericType = typeSupport.Type.GetGenericArguments().First();

                    var listType = typeof(List<>).MakeGenericType(genericType);
                    var newList = (IList)Activator.CreateInstance(listType);
                    newObject = newList;
                    var enumerator = (IEnumerable)sourceObject;
                    foreach (var item in enumerator)
                    {
                        var newItem = InspectAndCopy(item, currentDepth, maxDepth, objectTree, path);
                        newList.Add(newItem);
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
                        var newElement = InspectAndCopy(element, currentDepth, maxDepth, objectTree, path);
                        newArray.SetValue(newElement, i);

                    }
                    return newArray;
                }

                var properties = GetProperties(sourceObject);
                var fields = GetFields(sourceObject);

                // clone and recurse properties
                var rootPath = path;
                foreach (var property in properties)
                {
                    path = $"{rootPath}.{property.Name}";
                    if (property.CustomAttributes.Any(x => _ignoreAttributes.Contains(x.AttributeType)))
                        continue;
                    var propertyTypeSupport = new TypeSupport(property.PropertyType);
                    var propertyValue = property.GetValue(sourceObject);
                    if (property.PropertyType.IsValueType)
                        SetPropertyValue(property, newObject, propertyValue, path);
                    else if (propertyTypeSupport.IsImmutable)
                    {
                        SetPropertyValue(property, newObject, propertyValue, path);
                    }
                    else if (propertyValue != null)
                    {
                        var clonedPropertyValue = InspectAndCopy(propertyValue, currentDepth, maxDepth, objectTree, path);
                        SetPropertyValue(property, newObject, clonedPropertyValue, path);
                    }
                }

                // clone and recurse fields
                foreach (var field in fields)
                {
                    path = $"{rootPath}.{field.Name}";
                    if (field.CustomAttributes.Any(x => _ignoreAttributes.Contains(x.AttributeType)))
                        continue;
                    var fieldTypeSupport = new TypeSupport(field.FieldType);
                    var fieldValue = field.GetValue(sourceObject);
                    if (field.FieldType.IsValueType)
                        SetFieldValue(field, newObject, fieldValue, path);
                    else if (fieldTypeSupport.IsImmutable)
                        SetFieldValue(field, newObject, fieldValue, path);
                    else if (fieldValue != null && !fieldTypeSupport.IsImmutable)
                    {
                        var clonedFieldValue = InspectAndCopy(fieldValue, currentDepth, maxDepth, objectTree, path);
                        SetFieldValue(field, newObject, clonedFieldValue, path);
                    }
                }

                return newObject;
            }
            finally
            {

            }
        }

        private void SetPropertyValue(PropertyInfo property, object obj, object valueToSet, string path)
        {
            try
            {
                if (property.SetMethod != null)
                {
                    property.SetValue(obj, valueToSet);
                }
                else
                {
                    // if this is an auto-property with a backing field, set it
                    var field = obj.GetType().GetField($"<{property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field != null)
                        field.SetValue(obj, valueToSet);
                }
            }
            catch (Exception ex)
            {
                OnCloneError?.Invoke(ex, path, property, obj);
                if (OnCloneError == null)
                    throw ex;
            }
        }

        private void SetFieldValue(FieldInfo field, object obj, object valueToSet, string path)
        {
            try
            {
                field.SetValue(obj, valueToSet);
            }
            catch (Exception ex)
            {
                OnCloneError?.Invoke(ex, path, field, obj);
                if (OnCloneError == null)
                    throw ex;
            }
        }

        /// <summary>
        /// Create a new, empty object of a given type
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        private TValue CreateEmptyObject<TValue>()
        {
            var typeSupport = new TypeSupport<TValue>();
            if (typeSupport.HasEmptyConstructor)
                return Activator.CreateInstance<TValue>();
            return (TValue)FormatterServices.GetUninitializedObject(typeSupport.Type);
        }

        /// <summary>
        /// Create a new, empty object of a given type
        /// </summary>
        /// <param name="type">The type of object to construct</param>
        /// <param name="sourceObject">For array types, the sourceObject is needed to determine length</param>
        /// <returns></returns>
        private object CreateEmptyObject(Type type, object sourceObject = null)
        {
            var typeSupport = new TypeSupport(type);
            if (typeSupport.HasEmptyConstructor)
                return Activator.CreateInstance(typeSupport.Type);
            else if (typeSupport.IsArray)
                return Activator.CreateInstance(typeSupport.Type, new object[] { ((Array)sourceObject)?.Length ?? 0 });
            else if (typeSupport.IsImmutable)
                return null;
            return FormatterServices.GetUninitializedObject(typeSupport.Type);
        }

        /// <summary>
        /// Get all of the properties of an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private ICollection<PropertyInfo> GetProperties(object obj)
        {
            if (obj != null)
            {
                var t = obj.GetType();
                return t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
            return new PropertyInfo[0];
        }

        /// <summary>
        /// Get all of the fields of an object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="includeAutoPropertyBackingFields">True to include the compiler generated backing fields for auto-property getters/setters</param>
        /// <returns></returns>
        private ICollection<FieldInfo> GetFields(object obj, bool includeAutoPropertyBackingFields = false)
        {
            if (obj != null)
            {
                var t = obj.GetType();
                var allFields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (!includeAutoPropertyBackingFields)
                {
                    var allFieldsExcludingAutoPropertyFields = allFields.Where(x => !x.Name.Contains("k__BackingField")).ToList();
                    return allFieldsExcludingAutoPropertyFields;
                }
                return allFields;
            }
            return new FieldInfo[0];
        }
    }
}

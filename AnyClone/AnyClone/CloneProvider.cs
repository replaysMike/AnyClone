using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using TypeSupport;
using TypeSupport.Extensions;

namespace AnyClone
{
    /// <summary>
    /// Provider for cloning objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class CloneProvider<T>
    {
        private delegate void _memberUpdaterByRef(ref object source, object value);
        private readonly ObjectFactory _objectFactory;
        private MethodInfo _memberwiseCloneMethod = typeof(object)
            .GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
        private const TypeSupportOptions DefaultExtendedTypeOptions = TypeSupportOptions.Fields | TypeSupportOptions.Collections | TypeSupportOptions.Generics | TypeSupportOptions.Constructors | TypeSupportOptions.Caching;

        // maintain a static cache of IL dynamic methods for a given type/field pair
        private static readonly Dictionary<ILCacheKey, _memberUpdaterByRef> _memberUpdaterCache = new Dictionary<ILCacheKey, _memberUpdaterByRef>();
        private bool? _hasIgnoreConfiguration;

        /// <summary>
        /// Cloning error event
        /// </summary>
        public Action<Exception, string, object, object> OnCloneError { get; set; }

        /// <summary>
        /// Provider for cloning objects
        /// </summary>
        public CloneProvider()
        {
            _objectFactory = new ObjectFactory();
        }

        /// <summary>
        /// Recursive function that inspects an object and its properties/fields and clones it to a new or existing type
        /// </summary>
        /// <param name="sourceObject">The object to clone</param>
        /// <param name="destinationObject">An existing object to clone values to</param>
        /// <param name="destinationObjectType">The type of the destination object to clone to</param>
        /// <param name="currentDepth">The current tree depth</param>
        /// <param name="maxDepth">The max tree depth</param>
        /// <param name="configuration">Configure custom cloning options</param>
        /// <param name="objectTree">The object tree to prevent cyclical references</param>
        /// <param name="path">The current path being traversed</param>
        /// <param name="ignorePropertiesOrPaths">A list of properties or paths to ignore</param>
        /// <returns></returns>
        private object InspectAndCopy(object sourceObject, object destinationObject, Type destinationObjectType, int currentDepth, int maxDepth, CloneConfiguration configuration, ObjectTreeReferenceTracker objectTree, string path, ICollection<string> ignorePropertiesOrPaths)
        {
            if (_hasIgnoreConfiguration == null)
                _hasIgnoreConfiguration = HasIgnoreConfiguration(configuration, ignorePropertiesOrPaths);

            if (_hasIgnoreConfiguration.Value && IgnoreObjectName(null, path, configuration, ignorePropertiesOrPaths))
                return null;

            if (sourceObject == null)
                return null;

            // ensure we don't go too deep if specified
            if (maxDepth > 0 && currentDepth >= maxDepth)
                throw new CloneException($"The maximum clone recursion depth has exceeded maxDepth of '{maxDepth}'. Try setting the configuration option {nameof(CloneConfiguration.AllowUseCustomHashCodes)} to true or increase the {nameof(CloneConfiguration.MaxDepth)}. Check the Path in the exception for diagnosing the cause.", path);

            var type = sourceObject.GetType();
            ExtendedType typeSupport;
            try
            {
                typeSupport = type.GetExtendedType(DefaultExtendedTypeOptions);
            }
            // certain attributes such as .net remoting SoapTypeAttribute can cause issues trying to inspect. Possibly a framework bug with reflection
            catch (CustomAttributeFormatException)
            {
                return sourceObject;
            }

            ExtendedType destinationTypeSupport;
            if (destinationObjectType == null || destinationObjectType == type)
                destinationTypeSupport = typeSupport;
            else
                destinationTypeSupport = destinationObjectType.GetExtendedType(DefaultExtendedTypeOptions);

            // always return the original value on value types
            if (typeSupport.IsValueType)
            {
                return sourceObject;
            }

            // drop any objects we are ignoring by attribute
            if (_hasIgnoreConfiguration.Value && typeSupport.Attributes.Any(x => configuration.IgnorePropertiesWithAttributes.Contains(x.Name)))
                return null;

            // for delegate types, copy them by reference rather than returning null
            if (typeSupport.IsDelegate)
                return sourceObject;

            object newObject = null;
            // create a new empty object of the desired type
            if (typeSupport.IsArray)
            {
                if (!(sourceObject is Array sourceArray))
                    throw new NullReferenceException($"{nameof(sourceArray)} cannot be null!");
                // calculate the dimensions of the array
                var arrayRank = sourceArray.Rank;
                // get the length of each dimension
                var arrayDimensions = new List<int>();
                for (var dimension = 0; dimension < arrayRank; dimension++)
                    arrayDimensions.Add(sourceArray.GetLength(dimension));
                newObject = _objectFactory.CreateEmptyObject(destinationTypeSupport, default(TypeRegistry), arrayDimensions.ToArray());
            }
            else if (typeSupport.Type == typeof(string))
            {
                // copy the item directly
                newObject = string.Copy((string)sourceObject);
                return newObject;
            }
            else
            {
                newObject = destinationObject ?? _objectFactory.CreateEmptyObject(destinationTypeSupport);
            }

            if (newObject == null)
                return null;

            // increment the current recursion depth
            currentDepth++;

            // construct a hashtable of objects we have already inspected (recursion loop preventer)
            if (!typeSupport.IsValueType)
            {
                if (!objectTree.Contains(sourceObject))
                    objectTree.Add(sourceObject);
                else
                {
                    // object has already been traversed
                    return sourceObject;
                }
            }

            // clone using IClonable interface if exists
            if (configuration.AllowIClonableImplementations && typeSupport.Interfaces.Contains(typeof(ICloneable)))
            {
                var iClonable = sourceObject as ICloneable;
                return iClonable?.Clone();
            }

            // clone a dictionary's key/values
            else if (typeSupport.IsDictionary && typeSupport.IsGeneric)
            {
                var genericType = typeSupport.Type.GetGenericArguments().ToList();
                Type[] typeArgs = { genericType[0], genericType[1] };

                var listType = typeof(Dictionary<,>).MakeGenericType(typeArgs);
                var newDictionary = Activator.CreateInstance(listType) as IDictionary;
                newObject = newDictionary ?? throw new NullReferenceException($"{nameof(newDictionary)} cannot be null");
                var iDictionary = sourceObject as IDictionary;
                var success = false;
                var retryCount = 0;
                while (!success && retryCount < 10)
                {
                    try
                    {
                        foreach (DictionaryEntry item in iDictionary)
                        {
                            var key = InspectAndCopy(item.Key, null, null, currentDepth, maxDepth, configuration, objectTree, path, ignorePropertiesOrPaths);
                            var value = InspectAndCopy(item.Value, null, null, currentDepth, maxDepth, configuration, objectTree, path, ignorePropertiesOrPaths);
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
                if (!success)
                    throw new CloneException($"Error cloning Dictionary<,> at path '{path}'. Ensure the object is not modified while cloning data utilizing thread-safe access.");
                return newObject;
            }
            else if (typeSupport.IsHashtable && !typeSupport.IsGeneric)
            {
                var newHashtable = new Hashtable();
                newObject = newHashtable ?? throw new NullReferenceException($"{nameof(newHashtable)} cannot be null");
                var hashtable = (Hashtable)sourceObject;
                var success = false;
                var retryCount = 0;
                while (!success && retryCount < 10)
                {
                    try
                    {
                        foreach (DictionaryEntry item in hashtable)
                        {
                            var key = InspectAndCopy(item.Key, null, null, currentDepth, maxDepth, configuration, objectTree, path, ignorePropertiesOrPaths);
                            var value = InspectAndCopy(item.Value, null, null, currentDepth, maxDepth, configuration, objectTree, path, ignorePropertiesOrPaths);
                            newHashtable.Add(key, value);
                        }
                        success = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // if the collection was modified during enumeration, stop re-initialize and retry
                        success = false;
                        retryCount++;
                        newHashtable.Clear();
                    }
                }
                if (!success)
                    throw new CloneException($"Error cloning Hashtable at path '{path}'. Ensure the object is not modified while cloning data utilizing thread-safe access.");
                return newObject;
            }
            else if (typeSupport.IsEnumerable && !typeSupport.IsArray)
            {
                // clone enumerable elements
                var enumerable = (IEnumerable)sourceObject;
                bool hasEntries;
                if (typeSupport.IsCollection)
                    hasEntries = ((ICollection)sourceObject).Count > 0;
                else
                {
                    hasEntries = enumerable.Cast<object>().Any();
                }
                if (hasEntries)
                {
#if NET45_OR_GREATER || NETSTANDARD1_0_OR_GREATER
                    var readOnlyCollectionTypes = new[] { typeof(ReadOnlyCollection<>), typeof(ReadOnlyDictionary<,>) };
                    if (type.IsGenericType && readOnlyCollectionTypes.Contains(type.GetGenericTypeDefinition()))
                    {
                        // return as-is, since they can't be modified anyways
                        return sourceObject;
                    }
#endif

                    var addMethod = typeSupport.Type.GetMethod("Add");
                    if (addMethod == null)
                    {
                        addMethod = typeSupport.Type.GetMethod("Enqueue");
                        if (addMethod == null)
                        {
                            addMethod = typeSupport.Type.GetMethod("Push");
                            if (addMethod == null)
                            {
                                addMethod = typeSupport.Methods.FirstOrDefault(x => x.Name.StartsWith("Add"));
                                if (addMethod == null)
                                {
                                    // as a backup, try utilizing memberwise clone
                                    return _memberwiseCloneMethod.Invoke(sourceObject, null);
                                }
                            }
                        }
                    }

                    var success = false;
                    var retryCount = 0;
                    while (!success && retryCount < 10)
                    {
                        try
                        {
                            foreach (var item in enumerable)
                            {
                                var element = InspectAndCopy(item, null, null, currentDepth, maxDepth, configuration,
                                    objectTree, path, ignorePropertiesOrPaths);
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
                    if (!success)
                        throw new CloneException($"Error cloning IEnumerable at path '{path}'. Ensure the object is not modified while cloning data utilizing thread-safe access.");
                }

                return newObject;
            }

            // clone an arrays' elements
            if (typeSupport.IsArray)
            {
                var sourceArray = sourceObject as Array;
                var newArray = newObject as Array;

                // performance optimization, value typed primitive arrays can be block copied
                if (typeSupport.ElementType.IsPrimitive)
                {
                    var bytesPerValue = GetBytesPerValue(typeSupport.ElementType);
                    try
                    {
                        Buffer.BlockCopy(sourceArray, 0, newArray, 0, sourceArray.Length * bytesPerValue);
                    }
                    catch (Exception ex)
                    {
                        throw new CloneException($"Error block copying array at path '{path}'", ex);
                    }
                }
                else
                {
                    try
                    {
                        // copy each array element and clone the value
                        var arrayRank = newArray.Rank;
                        var arrayDimensions = new List<int>();
                        for (var dimension = 0; dimension < arrayRank; dimension++)
                            arrayDimensions.Add(newArray.GetLength(dimension));
                        var flatRowIndex = 0;
                        foreach (var row in sourceArray)
                        {
                            var newElement = InspectAndCopy(row, null, null, currentDepth, maxDepth, configuration, objectTree, path, ignorePropertiesOrPaths);
                            // performance optimization, skip dimensional processing if it's a 1d array
                            if (arrayRank > 1)
                            {
                                // this is an optimized multi-dimensional array reconstruction
                                // based on the formula: indices.Add((i / (arrayDimensions[arrayRank - 1] * arrayDimensions[arrayRank - 2] * arrayDimensions[arrayRank - 3] * arrayDimensions[arrayRank - 4] * arrayDimensions[arrayRank - 5])) % arrayDimensions[arrayRank - 6]);
                                var indices = new List<int>();
                                for (var r = 1; r <= arrayRank; r++)
                                {
                                    var multi = 1;
                                    for (var p = 1; p < r; p++)
                                    {
                                        multi *= arrayDimensions[arrayRank - p];
                                    }
                                    var b = (flatRowIndex / multi) % arrayDimensions[arrayRank - r];
                                    indices.Add(b);
                                }
                                indices.Reverse();
                                // set element of multi-dimensional array
                                newArray.SetValue(newElement, indices.ToArray());
                            }
                            else
                            {
                                // set element of 1d array
                                newArray.SetValue(newElement, flatRowIndex);
                            }
                            flatRowIndex++;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new CloneException($"Error cloning array elements at path '{path}'", ex);
                    }
                }
                return newArray;
            }

            if (typeSupport.IsExpression)
            {
                // utilize MemberwiseClone for expressions
                try
                {
                    var newExpression = _memberwiseCloneMethod.Invoke(sourceObject, null);
                    return newExpression;
                }
                catch (Exception ex)
                {
                    throw new CloneException($"Error cloning expression with type '{typeSupport.FullName}' at path '{path}'", ex);
                }
            }

            var fields = typeSupport.Fields.Where(x => !x.IsConstant);
            var rootPath = path;
            var localPath = string.Empty;
            // clone and recurse fields
            foreach (var field in fields)
            {
                localPath = $"{rootPath}.{field.Name}";
                // optimization to disable ignores by attribute if configured
                //System.Diagnostics.Debug.WriteLine($"Copying {localPath}");
                if (_hasIgnoreConfiguration.Value)
                {
                    if (IgnoreObjectName(field.Name, localPath, configuration, ignorePropertiesOrPaths, configuration.IgnorePropertiesWithAttributes.Count > 0 ? field.CustomAttributes : null))
                        continue;
                    // also check the property for ignore, if this is a auto-backing property
                    if (field.BackedProperty != null && IgnoreObjectName(field.BackedProperty.Name, $"{rootPath}.{field.BackedPropertyName}", configuration, ignorePropertiesOrPaths, configuration.IgnorePropertiesWithAttributes.Count > 0 ? field.BackedProperty.CustomAttributes : null))
                        continue;
                }
#if FEATURE_DISABLE_SET_INITONLY
                // we can't duplicate init-only fields since .net core 3.0+
                // make use of IL to get around this limitation
                if (field.FieldInfo.IsInitOnly && configuration.AllowCloningOfReadOnlyEntities)
                {
                    try
                    {
                        var updateFieldValue = sourceObject.GetFieldValue(field);
                        var updater = GetWriterForField(field);
                        updater(ref newObject, updateFieldValue);
                    }
                    catch (Exception ex)
                    {
                        throw new CloneException($"Failed to set field value named '{field.Name}' at path '{path}' using IL DynamicMethod", ex);
                    }
                    continue;
                }
#endif
                // only copy readonly fields if we allow as such
                if (field.FieldInfo.IsInitOnly && !configuration.AllowCloningOfReadOnlyEntities)
                    continue;

                // utilize reflection
                var fieldTypeSupport = field.Type;
                var fieldValue = sourceObject.GetFieldValue(field);
                var destinationField = newObject.GetField(field.Name, true);
                // does this field exist on the destination object with the same type?
                if (destinationField != null && destinationField.FieldType == field.FieldInfo.FieldType)
                {
                    if (fieldTypeSupport.IsValueType || fieldTypeSupport.IsImmutable)
                    {
                        SetFieldValue(newObject, destinationField, fieldValue, localPath);
                    }
                    else if (fieldValue != null)
                    {
                        var clonedFieldValue = InspectAndCopy(fieldValue, null, null, currentDepth, maxDepth,
                            configuration, objectTree, localPath, ignorePropertiesOrPaths);
                        SetFieldValue(newObject, destinationField, clonedFieldValue, localPath);
                    }
                }
            }

            return newObject;
        }

        private void SetFieldValue(object newObject, FieldInfo fieldInfo, object value, string path)
        {
            try
            {
                // use IL to set values
                //var updater = GetWriterForField(fieldInfo);
                //updater(ref newObject, value);

                // use reflection to set values
                newObject.SetFieldValue(fieldInfo, value);
            }
            catch (Exception ex)
            {
                throw new CloneException($"Error setting value of field named '{fieldInfo.Name}' at path '{path}' using reflection", ex);
            }
        }

        /// <summary>
        /// Set a readonly field value
        /// Not possible via reflection since .net standard 2.0 / .net core 3.0
        /// </summary>
        /// <param name="field"></param>
        private _memberUpdaterByRef GetWriterForField(FieldInfo field)
        {
            // Partial credit to https://www.productiverage.com/trying-to-set-a-readonly-autoproperty-value-externally-plus-a-little-benchmarkdotnet
            // dynamically generate a new method that will emit IL to set a field value
            var type = field.DeclaringType;
            var methodName = $"Set{field.Name}";
            var cacheKey = new ILCacheKey(type, methodName);
            if (_memberUpdaterCache.ContainsKey(cacheKey))
            {
                return _memberUpdaterCache[cacheKey];
            }

            var dynamicMethod = new DynamicMethod(
                methodName,
                typeof(void),
                new Type[] { typeof(object).MakeByRefType(), typeof(object) },
                type.Module,
                true
            );

            var gen = dynamicMethod.GetILGenerator();

            // because arg 0 is a ref, ldarg pushes arg0's address to the stack instead of the object/value
            gen.Emit(OpCodes.Ldarg_0);

            // in order to set the value of a field on on the object we cant use it's address
            // pop the address and push the instance to the stack
            gen.Emit(OpCodes.Ldind_Ref);

            // push the value that the field should be set to, to the stack
            gen.Emit(OpCodes.Ldarg_1);

            // because the value may be either a reference or value type, unbox it
            gen.Emit(OpCodes.Unbox_Any, field.FieldType);

            // pop the instance of the object, pop the value, and set the value of the field
            gen.Emit(OpCodes.Stfld, field);

            // no remaining objects on the stack, ret to exit
            gen.Emit(OpCodes.Ret);


            // create a delegate matching the parameter types
            var memberUpdater = (_memberUpdaterByRef)dynamicMethod.CreateDelegate(typeof(_memberUpdaterByRef));

            //System.Diagnostics.Debug.WriteLine($"Created writer for {type.Name} => {methodName}");
            _memberUpdaterCache.Add(cacheKey, memberUpdater);
            return memberUpdater;
        }

        /// <summary>
        /// Returns true if an ignore configuration is configured
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="ignorePropertiesOrPaths"></param>
        /// <returns></returns>
        private bool HasIgnoreConfiguration(CloneConfiguration configuration, ICollection<string> ignorePropertiesOrPaths)
        {
            if (configuration.IgnorePropertiesWithAttributes.Count == 0 && (ignorePropertiesOrPaths == null || ignorePropertiesOrPaths.Count == 0))
                return false;
            return true;
        }

        /// <summary>
        /// Returns true if object name should be ignored
        /// </summary>
        /// <param name="name">Property or field name</param>
        /// <param name="path">Full path to object</param>
        /// <param name="configuration">Configure custom cloning options</param>
        /// <param name="ignorePropertiesOrPaths">List of names or paths to ignore</param>
        /// <returns></returns>
        private bool IgnoreObjectName(string name, string path, CloneConfiguration configuration, ICollection<string> ignorePropertiesOrPaths, IEnumerable<CustomAttributeData> attributes = null)
        {
            var ignoreByNameOrPath = ignorePropertiesOrPaths?.Contains(name) == true || ignorePropertiesOrPaths?.Contains(path) == true;
            if (ignoreByNameOrPath)
                return true;
#if FEATURE_CUSTOM_ATTRIBUTES
            if (attributes?.Any(x => configuration.IgnorePropertiesWithAttributes?.Contains(x.AttributeType.Name) == true) == true)
#else
            if (attributes?.Any(x => configuration.IgnorePropertiesWithAttributes?.Contains(x.Constructor.DeclaringType.Name) == true) == true)
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
                    case UnaryExpression { Operand: MemberExpression m }:
                        name = m.Member.Name;
                        break;
                    default:
                        throw new NotImplementedException(expression.GetType().ToString());
                }
                ignorePropertiesList.Add(name);
            }
            return ignorePropertiesList;
        }

        private static int GetBytesPerValue(Type type)
        {
            // this is more performant than a switch
            if (type == typeof(bool))
                return sizeof(bool);
            if (type == typeof(char))
                return sizeof(char);
            if (type == typeof(byte) || type == typeof(sbyte))
                return sizeof(byte);
            else if (type == typeof(short) || type == typeof(ushort))
                return sizeof(short);
            else if (type == typeof(int) || type == typeof(uint))
                return sizeof(int);
            else if (type == typeof(nint) || type == typeof(nuint))
                return sizeof(int); // varies per platform, use int
            else if (type == typeof(long) || type == typeof(ulong))
                return sizeof(long);
            else if (type == typeof(float))
                return sizeof(float);
            else if (type == typeof(double))
                return sizeof(double);
            else if (type == typeof(decimal))
                return sizeof(decimal);

            throw new NotSupportedException($"Type {type} is not supported.");
        }
    }
}

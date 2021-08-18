using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace AnyClone
{
    public class ObjectTreeReferenceTracker : IDisposable
    {
        private IDictionary<long, object> _objectTree;
        private ReferenceTrackingType _referenceTrackingType;
        private bool _useCustomHashCodes;
        private ObjectIDGenerator _objects;

        public int Count => _objectTree.Count;

        public ObjectTreeReferenceTracker(ReferenceTrackingType referenceTrackingType) : this(referenceTrackingType, true)
        {
        }

        public ObjectTreeReferenceTracker(ReferenceTrackingType referenceTrackingType, bool useCustomHashCodes)
        {
            _objectTree = new Dictionary<long, object>();
            _objects = new ObjectIDGenerator();
            _referenceTrackingType = referenceTrackingType;
            _useCustomHashCodes = useCustomHashCodes;
        }

        /// <summary>
        /// Add an object to the tracked tree if it doesn't exist
        /// </summary>
        /// <param name="instance"></param>
        public void Add(object instance)
        {
            var referenceId = GetReferenceId(instance);
            if (!_objectTree.ContainsKey(referenceId))
                _objectTree.Add(referenceId, instance);
        }

        /// <summary>
        /// True if reference to instance has been tracked
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool Contains(object instance)
        {
            var referenceId = GetReferenceId(instance);
            var contains = _objectTree.ContainsKey(referenceId);
            return contains;
        }

        private long GetObjectIdFromGenerator(object instance) => _objects.GetId(instance, out var _);

        private IntPtr GetAddress(object instance)
        {
            var pointer = IntPtr.Zero;
            ManagedReferenceHelper.GetPinnedPtr(instance, ptr => { pointer = ptr; });
            return pointer;
        }

        private int GetInstanceHashCode(object instance)
        {
            var chosenHashcode = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(instance);
            try
            {
                if (_useCustomHashCodes)
                    chosenHashcode = instance.GetHashCode();
            }
            catch (Exception)
            {
                // error occured inside an object's custom GetHashCode implementation
            }
            return chosenHashcode;
        }

        private long GetReferenceId(object instance)
        {
            switch (_referenceTrackingType)
            {
                case ReferenceTrackingType.Hashcode:
                    return GetInstanceHashCode(instance);
                default:
                case ReferenceTrackingType.Reference:
                    return GetAddress(instance).ToInt64();
                case ReferenceTrackingType.ObjectIdGenerator:
                    return GetObjectIdFromGenerator(instance);
            }
        }

        protected virtual void Dispose(bool isDisposing)
        {
            _objectTree.Clear();
            _objectTree = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public enum ReferenceTrackingType
    {
        Reference,
        Hashcode,
        ObjectIdGenerator
    }
}

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace AnyClone.Tests.TestObjects
{
    [Category("CategoryName")]
    [Description("Description value")]
    public class BasicObjectWithParamAttributes
    {
        [IgnoreDataMember]
        public int Id { get; set; }

        [AttributeWithParams("Value1 / (Value2 / {0})", SortKeyInputType.UniversePerK)]
        public decimal Number => 3.1415m;
    }

    public class AttributeWithParamsAttribute : Attribute
    {
        private string _dynamicClause;
        private SortKeyInputType[] _inputTypes;

        public LambdaExpression SortExpression { get; set; }
        public AttributeWithParamsAttribute(string dynamicClause, params SortKeyInputType[] inputTypes)
        {
            _dynamicClause = dynamicClause;
            _inputTypes = inputTypes;
        }

        public static T CallNotSupported<T>() => throw new NotSupportedException();
    }

    public enum SortKeyInputType
    {
        None,
        UniversePerK,
    }
}

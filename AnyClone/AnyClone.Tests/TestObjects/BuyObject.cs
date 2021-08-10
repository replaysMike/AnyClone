#if NET45_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using NUnit.Framework;

namespace AnyClone.Tests.TestObjects
{
    /// <summary>
    /// Represents a very complex type
    /// </summary>
    public partial class BuyObject : BuyObjectBase
    {
        public BuyObject()
        {
            _actualUnits = null;
            _originalUnits = null;
            InternalUnits = new Lazy<ICollection<object>>(() => GetLegacyCollectionWithListener());
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [NotMapped]
        public override long Identity => Id;

        [SqlDefaultValue(1)]
        public int RowVersion { get; set; }

        [NotMapped]
        public bool SkipVersionIncrement { get; set; }

        public int? ExternalOrderVersion { get; set; }

        public ExternalOrderStatus? ExternalOrderStatusId { get; private set; }
        public void SetExternalOrderStatus(ExternalOrderStatus externalOrderStatusId, int externalOrderVersion)
        {
            ExternalOrderStatusId = externalOrderStatusId;
            ExternalOrderVersion = externalOrderVersion;
        }

        public long? ChildHeaderId { get; set; }
        public virtual object ChildHeader { get; set; }

        public virtual object SpotsPerWeekSet { get; set; }

        public virtual ICollection<object> LocalizedNotes { get; set; } = new HashSet<object>();
        public virtual ICollection<object> TagAssignments { get; set; } = new HashSet<object>();
        public virtual ICollection<object> ProgramAssignments { get; set; } = new HashSet<object>();
        public virtual ICollection<object> Snapshots { get; set; } = new HashSet<object>();
        public virtual ICollection<object> Offers { get; set; } = new HashSet<object>();
        public virtual ICollection<object> ChildSnapshots { get; set; } = new HashSet<object>();
        public virtual ICollection<object> PriceEstimates { get; set; } = new HashSet<object>();
        public virtual ICollection<object> Mappings { get; set; } = new HashSet<object>();
        public virtual ICollection<object> MatchedUnits { get; set; } = new HashSet<object>();
        public virtual ICollection<BuyObject> BuyObjects { get; set; } = new HashSet<BuyObject>();
        public virtual ICollection<object> Synchronizations { get; set; } = new HashSet<object>();

        private IList<int> _actualUnits;
        private IList<int> _originalUnits;

        public object AggregateParentEntity => PurchaseHeader;

        [NotMapped]
        public IList<int> ActualSnapshot
        {
            get => _actualUnits;
            set => _actualUnits = value;
        }

        [NotMapped]
        public IList<int> OriginalSnapshot
        {
            get => _originalUnits;
            set => _originalUnits = value;
        }

        public override void SoftDelete()
        {
            if (ExternalOrderStatusId.HasValue)
            {
                throw new Exception("Error not allowed");
            }

            base.SoftDelete();
        }

        protected virtual ICollection<object> GetLegacyCollectionWithListener()
        {
            var spots = new List<object>();
            return spots;
        }

        protected Lazy<ICollection<object>> InternalUnits;
        public decimal ThirdPartyDataIndex { get; set; }
        public override object GetSet() => SpotsPerWeekSet;
        public override IEnumerable<object> GetEstimates() => PriceEstimates;
        public override object GetPackage() => BuyPackage;

        [NotMapped]
        public override Guid? AcceptedFrom { get; set; }

        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<object> SpotsPerWeekAllocations => GetLegacyCollectionWithListener();

        [NotMapped]
        public GenericType<object> Target => typeof(BuyObject);

        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [IgnoreDataMember]
        public string UnitsTableName => "UnitsPerWeek";

        [NotMapped]
        public override IEnumerable<int> CategoryIds => ProgramAssignments.Select(x => 0);

        [NotMapped]
        public override IEnumerable<object> Notes => Notes;

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!_hasCalledSetDiscount && Id == 0)
                yield return new ValidationResult($"Cannot create new");

            if (!ReferenceId.HasValue || ReferenceId.Value <= 0)
                yield return new ValidationResult($"Active must have a {nameof(ReferenceId)} set");

            if (PurchaseHeader == null)
                yield return new ValidationResult($"Buy must be loaded to validate");

            foreach (var result in base.Validate(validationContext))
                yield return result;
        }

        [ExpressionSortKey]
        public Expression<Func<BuyObject, int>> Units { get; internal set; } = i => 0;

        [ExpressionSortKey]
        public Expression<Func<BuyObject, double>> Duration { get; } = i => i.EndTime.Subtract(i.StartTime).TotalSeconds;
    }

    public class ExpressionSortKeyAttribute : NotMappedAttribute
    {
    }

    public abstract class BuyObjectBase : BuyBase
    {
        public int Id { get; set; }
        public long ParentId { get; set; }

        [Index]
        public int AdId { get; set; }

        [Index]
        public int ProductTypeId { get; set; }

        [NotMapped]
        public decimal? NetCost => 0;

        [System.ComponentModel.DataAnnotations.Range(0.0, 1.0)]
        public decimal? NetCalcDiscountPercentage { get; private set; }

        [Index]
        public long? ParentBuyId { get; set; }

        public virtual object PurchaseHeader { get; set; }
        public virtual object BaseInventory { get; set; }
        public virtual object AdvertiserOrganization { get; set; }
        public virtual object BrandOrganization { get; set; }
        public virtual object BuyPackage { get; set; }

        public override object GetBaseInventory() => BaseInventory;

        public override int GetMarketId() => 0;

        public override object GetHeader() => PurchaseHeader;

        public override void AssignHeader(object source)
        {
            PurchaseHeader = source;
            Id = 1;
        }

        [NotMapped]
        public BuyObject ParentBuy { get; set; }

        public void SetDiscount(decimal? percentage)
        {
            _hasCalledSetDiscount = true;
            NetCalcDiscountPercentage = percentage;
        }

        protected bool _hasCalledSetDiscount;

        [ExpressionSortKey]
        public Expression<Func<BuyObjectBase, decimal>> InventoryCost { get; } = b => 0;
    }

    public abstract class BuyBase : DbBase
    {
        [NotMapped]
        public virtual long Identity =>
            throw new NotImplementedException();

        [SqlDefaultValue("uuid_generate_v4()")]
        [Index]
        public Guid ChangesetItemHash { get; set; } = Guid.NewGuid();

        [Index]
        public object BuylineStateId { get; set; }

        public int? ReferenceId { get; set; }

        [EncryptionSource(typeof(decimal?), "OriginalCost")]
        public byte[] OriginalCostEncrypted { get; set; }

        [EncryptionSource(typeof(decimal?), "Cost")]
        public byte[] CostEncrypted { get; set; }

        [EncryptionDestination]
        public decimal? Cost { get; set; }

        [EncryptionDestination]
        public decimal? OriginalCost { get; set; }

        [Obsolete("No longer used")]
        [DecimalPrecision(19, 4)]
        public decimal PerThousand { get; set; }

        [Obsolete("No longer used")]
        [DecimalPrecision(19, 4)]
        public decimal Viewership { get; set; }

        public int DayOfWeekMask { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        [SqlDefaultValue(5)]
        public int LengthId { get; set; }

        [NotMapped]
        public object Length { get; set; }

        public long? ClonedFromRecordId { get; set; }

        public string PackageName { get; set; }

        public long? PackageId { get; set; }

        [SqlDefaultValue(1)]
        public object PaymentType { get; set; } = new object();

        [SqlDefaultValue((int)BuyType.Regular)]
        public BuyType BuyTypeId { get; set; } = BuyType.Regular;

        public Guid? LatestRejectId { get; set; }

        public abstract Guid? AcceptedFrom { get; set; }

        public int? CustomDayId { get; set; }

        public long? SplitFromBuyId { get; set; }

        public abstract object GetBaseInventory();

        public abstract object GetHeader();

        public abstract void AssignHeader(object source);

        public abstract object GetSet();

        public abstract IEnumerable<object> GetEstimates();

        public abstract int GetMarketId();

        public abstract object GetPackage();

        [NotMapped]
        public abstract IEnumerable<int> CategoryIds { get; }

        [NotMapped]
        public virtual object DeltaSpots { get; set; }

        [NotMapped]
        public virtual IEnumerable<object> Notes { get; }

        public void UpdateActualAndDeltaAllocation(int weekNumber, int newActual)
        {
        }

        public void UpdateActualAndDeltaAllocations(IList<int> newActualSpots)
        {
        }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Enum.IsDefined(typeof(BuyType), BuyTypeId))
                yield return new ValidationResult($"Invalid value of {BuyTypeId} for {nameof(BuyTypeId)}");

            if (BuyTypeId == BuyType.NoFee && Cost > 0)
                yield return new ValidationResult($"A {nameof(BuyTypeId)} of type {BuyTypeId} must have a {nameof(Cost)} of zero");
        }

        [DynamicSortKey("Cost / Impressions")]
        public decimal Cpm => DynamicSortKeyAttribute.CallNotSupported<decimal>();

        [DynamicSortKey("Cost / (Impressions / {0})", SortKeyInputType.UniversePerK)]
        public decimal Cpp => DynamicSortKeyAttribute.CallNotSupported<decimal>();

        [ExpressionSortKey]
        public Expression<Func<BuyBase, decimal>> CpmExpression => b => (decimal)b.Cost / b.PerThousand;
    }

    [SoftDelete(nameof(IsDeleted))]
    [Serializable]
    public abstract class DbBase
    {
        [Index]
        [Column]
        protected bool IsDeleted { get; set; }

        [Column]
        protected DateTime? DateDeletedUtc { get; set; }

        public DbBase()
        {
            IsDeleted = false;
        }

	    public virtual void SoftDelete()
        {
            if (IsDeleted)
                return;

            IsDeleted = true;
            DateDeletedUtc = DateTime.UtcNow;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SqlDefaultValueAttribute : Attribute
    {
        public string DefaultValue { get; private set; }

        public SqlDefaultValueAttribute(string defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public SqlDefaultValueAttribute(int defaultValue)
        {
            DefaultValue = defaultValue.ToString();
        }

        public SqlDefaultValueAttribute(bool defaultValue)
        {
            DefaultValue = defaultValue ? "true" : "false";
        }
    }

    public enum ExternalOrderStatus
    {
        New = 1,
        Rejected = 2,
        Revised = 3,
        Confirmed = 4,
        Cancelled = 5,
    }

    public sealed class GenericType<T>
    {
        public Type BaseType { get; private set; }

        private GenericType() { }

        public GenericType(Type type)
        {
            BaseType = type;
        }

        public static implicit operator Type(GenericType<T> x) => typeof(T);

        public static implicit operator GenericType<T>(Type x) => new GenericType<T>(x);
    }

    public class IndexAttribute : Attribute
    {
        public IndexAttribute()
            : this(string.Empty)
        { }

        public IndexAttribute(string name)
            : this(name, 0)
        { }

        public IndexAttribute(string name, int order)
        {
            Name = name;
            Order = order;
        }

        public virtual bool IsUnique { get; set; }
        public virtual string Name { get; set; }
        public virtual int Order { get; set; }
    }

    public class EncryptionSourceAttribute : Attribute
    {
        public Type DataType { get; }
        public string DestinationName { get; }

        public EncryptionSourceAttribute(Type dataType, string destinationName)
        {
            DataType = dataType;
            DestinationName = destinationName;
        }

        private EncryptionSourceAttribute() { }
    }

    public class EncryptionDestinationAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class DecimalPrecisionAttribute : Attribute
    {
        public DecimalPrecisionAttribute(byte precision, byte scale)
        {
            Precision = precision;
            Scale = scale;
        }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
    }

    public enum BuyType
    {
        [System.ComponentModel.Description("Regular")]
        Regular = 1,
        [System.ComponentModel.Description("Non Regular")]
        NonRegular = 2,
        [System.ComponentModel.Description("No fee")]
        NoFee
    }

    public class DynamicSortKeyAttribute : NotMappedAttribute
    {
        private string _dynamicClause;
        private SortKeyInputType[] _inputTypes;

        public LambdaExpression SortExpression { get; set; }
        public DynamicSortKeyAttribute(string dynamicClause, params SortKeyInputType[] inputTypes)
        {
            _dynamicClause = dynamicClause;
            _inputTypes = inputTypes;
        }

        public static T CallNotSupported<T>() => throw new NotSupportedException();
    }

    public class SoftDeleteAttribute : Attribute
    {
        public SoftDeleteAttribute(string column)
        {
            ColumnName = column;
        }

        public string ColumnName { get; set; }
    }
}
#endif

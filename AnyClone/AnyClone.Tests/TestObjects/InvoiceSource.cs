using System;
using System.Collections.Generic;
using System.Linq;

namespace AnyClone.Tests.TestObjects
{
    [Serializable]
    public class InvoiceSource
    {
        public string Identifier { get; set; }
        public InvoiceGroup[] InvoiceGroups { get; set; }
        public InvoiceOutlet[] Outlets { get; set; }
        public InvoiceSourceMediaTypes MediaType { get; set; }

        public DateTime StartDate =>
            InvoiceGroups?.Select(x => x.StartDate).Min() ?? default(DateTime);
        public DateTime EndDate =>
            InvoiceGroups?.Select(x => x.EndDate).Max() ?? default(DateTime);

        private DateTime _originalProposalStartDate;
        public DateTime OriginalProposalStartDate
        {
            get => _originalProposalStartDate;
            set {
                _originalProposalStartDate = value;
                AdjustedProposalStartDate = new DateTime(2021, 1, 1);
            }
        }

        private DateTime _originalProposalEndDate;
        public DateTime OriginalProposalEndDate
        {
            get => _originalProposalEndDate;
            set {
                _originalProposalEndDate = value;
                AdjustedProposalEndDate = new DateTime(2021, 3, 1);
            }
        }

        public DateTime AdjustedProposalStartDate { get; private set; }
        public DateTime AdjustedProposalEndDate { get; private set; }
    }

    [Serializable]
    public class InvoiceOutlet
    {
        public string Identifier { get; set; }
        public string Name { get; set; }
        public InvoiceSourceMediaTypes MediaType { get; set; }
        public short? MarketZone { get; set; }
    }

    [Serializable]
    public class InvoiceGroup
    {
        public string Identifier { get; set; }
        public Invoice[] Invoices { get; set; }
        public InvoiceDemoCategory[] DemoCategories { get; set; }
        public bool IsPackage { get; set; }

        public DateTime StartDate =>
            Invoices?.Select(x => x.StartDate).Min() ?? default(DateTime);
        public DateTime EndDate =>
            Invoices?.Select(x => x.EndDate).Max() ?? default(DateTime);
    }

    [Serializable]
    public class InvoiceDemoCategory
    {
        public string Identifier { get; set; }
        public short AgeFrom { get; set; }
        public short AgeTo { get; set; }
        public string Group { get; set; }
        public DemographicValueTypes DemoValueType { get; set; }
    }

    [Serializable]
    public class Invoice
    {
        public string UUID { get; set; }
        public string Name { get; set; }
        public string[] ProgramNames { get; set; }
        public InvoicePeriod[] Periods { get; set; }
        public InvoiceDayTime[] DayTimes { get; set; }
        public string SpotLength { get; set; }
        public string OutletName { get; set; }
        public SourceModelType SourceModelType { get; set; } = SourceModelType.SpotCableProposal;
        public object SourceModel { get; set; }
        public int SourceModelLineNumber { get; set; }

        public DateTime StartDate =>
            Periods?.Select(x => x.AdjustedStartDate).Min() ?? default(DateTime);
        public DateTime EndDate =>
            Periods?.Select(x => x.AdjustedEndDate).Max() ?? default(DateTime);
    }

    [Serializable]
    public class InvoicePeriod
    {
        public DateTime AdjustedStartDate { get; set; }
        public DateTime OriginalStartDate { get; set; }
        public DateTime AdjustedEndDate { get; set; }
        public DateTime OriginalEndDate { get; set; }
        public decimal Rate { get; set; }
        public DemoValue[] DemoValues { get; set; }
        public string SpotsPerWeek { get; set; }
        public ItemsPerDayOfWeek OriginalSpotsPerDayOfWeek { get; set; }
        public DayOfWeek AllocatedDayOfWeek { get; set; }
        public InvoicePeriodType InvoicePeriodType { get; set; } = InvoicePeriodType.DetailedPeriodType;
    }

    [Serializable]
    public class InvoiceDayTime
    {
        public TimeSpanRange BroadcastTimeRange { get; set; }
        public CustomDaysOfWeek Days { get; set; }
        public string ProgramName { get; set; }

        public override bool Equals(object obj) =>
            obj is InvoiceDayTime time &&
            EqualityComparer<TimeSpanRange>.Default.Equals(BroadcastTimeRange, time.BroadcastTimeRange) &&
            Days == time.Days &&
            ProgramName == time.ProgramName;

        public override int GetHashCode()
        {
            var hashCode = 1869100972;
            hashCode = hashCode * -1521134295 + EqualityComparer<TimeSpanRange>.Default.GetHashCode(BroadcastTimeRange);
            hashCode = hashCode * -1521134295 + Days.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProgramName);
            return hashCode;
        }
    }

    [Serializable]
    public class TimeSpanRange
    {
        public TimeSpanRange(TimeSpan startTime, TimeSpan endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }

        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }

        public override bool Equals(object obj) =>
            obj is TimeSpanRange timeSpan &&
            StartTime.Equals(timeSpan.StartTime) &&
            EndTime.Equals(timeSpan.EndTime);

        public override int GetHashCode()
        {
            var hashCode = -445957783;
            hashCode = hashCode * -1521134295 + StartTime.GetHashCode();
            hashCode = hashCode * -1521134295 + EndTime.GetHashCode();
            return hashCode;
        }
    }

    [Serializable]
    public class DemoValue
    {
        public string DemoCategoryRef { get; set; }
        public string Value { get; set; }
        public DemographicValueTypes DemoValueType { get; set; }

        public override bool Equals(object obj)
        {
            var x = this;
            var y = obj as DemoValue;
            return object.ReferenceEquals(x, y) ||
                   (x != null && y != null &&
                    x.DemoCategoryRef.Equals(y.DemoCategoryRef, StringComparison.InvariantCultureIgnoreCase) &&
                    x.DemoValueType == y.DemoValueType &&
                    x.Value.Equals(y.Value, StringComparison.InvariantCultureIgnoreCase));
        }

        public override int GetHashCode() =>
            (DemoCategoryRef ?? string.Empty).GetHashCode() ^
            DemoValueType.GetHashCode() ^
            (Value ?? string.Empty).GetHashCode();
    }

    [Serializable]
    public class ItemsPerDayOfWeek
    {
        public int Monday { get; set; }
        public int Tuesday { get; set; }
        public int Wednesday { get; set; }
        public int Thursday { get; set; }
        public int Friday { get; set; }
        public int Saturday { get; set; }
        public int Sunday { get; set; }

        public int GetAllocatedItemsForDayOfWeek(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return Monday;
                case DayOfWeek.Tuesday:
                    return Tuesday;
                case DayOfWeek.Wednesday:
                    return Wednesday;
                case DayOfWeek.Thursday:
                    return Thursday;
                case DayOfWeek.Friday:
                    return Friday;
                case DayOfWeek.Saturday:
                    return Saturday;
                case DayOfWeek.Sunday:
                    return Sunday;
                default:
                    throw new ArgumentException("The value of dayOfWeek must be a valid day of week.");
            }
        }
    }

    [Flags]
    public enum CustomDaysOfWeek
    {
        None = 0x0000,
        Monday = 0x0001,
        Tuesday = 0x0002,
        Wednesday = 0x0004,
        Thursday = 0x0008,
        Friday = 0x0010,
        Saturday = 0x0020,
        Sunday = 0x0040,
        MondayToFriday = Monday | Tuesday | Wednesday | Thursday | Friday,
        Weekend = Saturday | Sunday,
        All = MondayToFriday | Weekend,
    }

    public enum SourceModelType
    {
        SpotCableProposal = 1,
        SpotCableOrder = 2
    }

    public enum DemographicValueTypes
    {
        None = 0,
        Ratings = 1,
        Impressions = 2,
        CPP = 3,
        CPM = 4,
    }

    public enum InvoicePeriodType
    {
        DetailedPeriodType = 1,
        DayDetailedPeriodType = 2
    }

    public enum InvoiceSourceMediaTypes
    {
        MediaType1,
        MediaType2
    }
}

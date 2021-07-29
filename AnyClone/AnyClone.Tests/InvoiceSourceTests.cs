using System;
using AnyClone;
using AnyClone.Tests.TestObjects;
using NUnit.Framework;

namespace AnyClone.Tests
{
    [TestFixture]
    public class InvoiceSourceTests
    {
        [Test]
        public void Should_Clone_AvailSource()
        {
            var availSource = new InvoiceSource() {
                Identifier = "TestAvailSource",
                InvoiceGroups = new [] {
                    new InvoiceGroup {
                        Identifier = "TestGroupIdentifier",
                        Invoices = new [] {
                            new Invoice {
                                DayTimes = new [] {
                                    new InvoiceDayTime {
                                        ProgramName = "TestProgram",
                                        Days = CustomDaysOfWeek.MondayToFriday,
                                        BroadcastTimeRange = new TimeSpanRange(TimeSpan.FromHours(0), TimeSpan.FromHours(12))
                                    }
                                },
                                Name = "TestAvail",
                                OutletName = "TestOutlet",
                                Periods = new [] {
                                    new InvoicePeriod {
                                        AdjustedStartDate = new DateTime(2021, 1, 1),
                                        AdjustedEndDate = new DateTime(2021, 3, 1),
                                        AllocatedDayOfWeek = DayOfWeek.Monday | DayOfWeek.Tuesday | DayOfWeek.Wednesday,
                                        InvoicePeriodType = InvoicePeriodType.DayDetailedPeriodType,
                                        OriginalStartDate = new DateTime(2021, 1, 1),
                                        OriginalEndDate = new DateTime(2021, 3, 1),
                                        Rate = 100.0m,
                                        SpotsPerWeek = "10",
                                        DemoValues = new [] {
                                            new DemoValue {
                                                DemoCategoryRef = "TestDemoCategoryRef",
                                                DemoValueType = DemographicValueTypes.CPM,
                                                Value = "value"
                                            }
                                        },
                                        OriginalSpotsPerDayOfWeek = new ItemsPerDayOfWeek {
                                            Monday = 5,
                                            Tuesday = 3,
                                            Wednesday = 2,
                                            Thursday = 1
                                        }
                                    }
                                },
                                ProgramNames = new [] { "Program name" },
                                SourceModel = new TimeSpan(),
                                SourceModelLineNumber = 10,
                                SourceModelType = SourceModelType.SpotCableOrder,
                                SpotLength = "30",
                                UUID = "fake uuid"
                            }
                        },
                        IsPackage = true,
                        DemoCategories = new [] {
                            new InvoiceDemoCategory {
                                AgeFrom = 18,
                                AgeTo = 56,
                                DemoValueType = DemographicValueTypes.CPM,
                                Group = "TestGroup",
                                Identifier = "TestIdentifier"
                            }
                        }
                    }
                },
                MediaType = InvoiceSourceMediaTypes.MediaType2,
                OriginalProposalStartDate = new DateTime(2021, 1, 1),
                OriginalProposalEndDate = new DateTime(2021, 3, 1),
                Outlets = new [] {
                    new InvoiceOutlet {
                        Identifier = "TestOutletIdentifier",
                        MarketZone = 100,
                        MediaType = InvoiceSourceMediaTypes.MediaType2,
                        Name = "TestOutletName"
                    }
                }
            };

            // clone the original object
            var cloned = availSource.Clone();

            Assert.AreEqual(availSource.Identifier, cloned.Identifier);
            Assert.AreEqual(availSource.MediaType, cloned.MediaType);
            Assert.AreEqual(availSource.OriginalProposalStartDate, cloned.OriginalProposalStartDate);
            Assert.AreEqual(availSource.OriginalProposalEndDate, cloned.OriginalProposalEndDate);
            Assert.AreEqual(availSource.InvoiceGroups.Length, cloned.InvoiceGroups.Length);
            Assert.AreEqual(availSource.InvoiceGroups[0].Invoices.Length, cloned.InvoiceGroups[0].Invoices.Length);
            Assert.AreEqual(availSource.InvoiceGroups[0].Invoices[0].Name, cloned.InvoiceGroups[0].Invoices[0].Name);
            Assert.AreEqual(availSource.InvoiceGroups[0].Invoices[0].OutletName, cloned.InvoiceGroups[0].Invoices[0].OutletName);
            Assert.AreEqual(availSource.InvoiceGroups[0].Invoices[0].UUID, cloned.InvoiceGroups[0].Invoices[0].UUID);
            Assert.AreEqual(availSource.InvoiceGroups[0].Invoices[0].SourceModelType, cloned.InvoiceGroups[0].Invoices[0].SourceModelType);
            Assert.AreEqual(availSource.InvoiceGroups[0].Invoices[0].SourceModelLineNumber, cloned.InvoiceGroups[0].Invoices[0].SourceModelLineNumber);
            Assert.AreEqual(availSource.InvoiceGroups[0].Invoices[0].SpotLength, cloned.InvoiceGroups[0].Invoices[0].SpotLength);
            Assert.AreEqual(availSource.InvoiceGroups[0].Invoices[0].OutletName, cloned.InvoiceGroups[0].Invoices[0].OutletName);
            Assert.AreEqual(availSource.InvoiceGroups[0].Invoices[0].DayTimes[0].Days, cloned.InvoiceGroups[0].Invoices[0].DayTimes[0].Days);
            Assert.AreEqual(availSource.InvoiceGroups[0].Invoices[0].DayTimes[0].ProgramName, cloned.InvoiceGroups[0].Invoices[0].DayTimes[0].ProgramName);
            Assert.AreEqual(availSource.InvoiceGroups[0].Invoices[0].Periods[0].AdjustedStartDate, cloned.InvoiceGroups[0].Invoices[0].Periods[0].AdjustedStartDate);
            Assert.AreEqual(availSource.InvoiceGroups[0].Invoices[0].Periods[0].AdjustedEndDate, cloned.InvoiceGroups[0].Invoices[0].Periods[0].AdjustedEndDate);
            Assert.AreEqual(availSource.InvoiceGroups[0].StartDate, cloned.InvoiceGroups[0].StartDate);
            Assert.AreEqual(availSource.InvoiceGroups[0].EndDate, cloned.InvoiceGroups[0].EndDate);
            Assert.AreEqual(availSource.InvoiceGroups[0].DemoCategories[0].Identifier, cloned.InvoiceGroups[0].DemoCategories[0].Identifier);
            Assert.AreEqual(availSource.InvoiceGroups[0].DemoCategories[0].AgeFrom, cloned.InvoiceGroups[0].DemoCategories[0].AgeFrom);
            Assert.AreEqual(availSource.InvoiceGroups[0].DemoCategories[0].AgeTo, cloned.InvoiceGroups[0].DemoCategories[0].AgeTo);
            Assert.AreEqual(availSource.InvoiceGroups[0].DemoCategories[0].DemoValueType, cloned.InvoiceGroups[0].DemoCategories[0].DemoValueType);
            Assert.AreEqual(availSource.Outlets[0].Identifier, cloned.Outlets[0].Identifier);
            Assert.AreEqual(availSource.Outlets[0].Name, cloned.Outlets[0].Name);
            Assert.AreEqual(availSource.Outlets[0].MediaType, cloned.Outlets[0].MediaType);
            Assert.AreEqual(availSource.Outlets[0].MarketZone, cloned.Outlets[0].MarketZone);

            // modify some properties
            availSource.Identifier = "TestAvailSourceModified";
            availSource.InvoiceGroups[0].Identifier = "TestGroupIdentifierModified";
            availSource.InvoiceGroups[0].Invoices[0].Name = "TestAvailModified";
            availSource.InvoiceGroups[0].Invoices[0].ProgramNames[0] = "Program name modified";
            availSource.InvoiceGroups[0].Invoices[0].SourceModelType = SourceModelType.SpotCableProposal;
            availSource.InvoiceGroups[0].Invoices[0].DayTimes[0].Days = CustomDaysOfWeek.Weekend;
            availSource.Outlets[0].MarketZone = 200;
            availSource.Outlets[0].Name = "TestOutletNameModified";

            // ensure values changed are different
            Assert.AreNotEqual(availSource.Identifier, cloned.Identifier);
            Assert.AreNotEqual(availSource.InvoiceGroups[0].Identifier, cloned.InvoiceGroups[0].Identifier);
            Assert.AreNotEqual(availSource.InvoiceGroups[0].Invoices[0].Name, cloned.InvoiceGroups[0].Invoices[0].Name);
            Assert.AreNotEqual(availSource.InvoiceGroups[0].Invoices[0].ProgramNames[0], cloned.InvoiceGroups[0].Invoices[0].ProgramNames[0]);
            Assert.AreNotEqual(availSource.InvoiceGroups[0].Invoices[0].SourceModelType, cloned.InvoiceGroups[0].Invoices[0].SourceModelType);
            Assert.AreNotEqual(availSource.InvoiceGroups[0].Invoices[0].DayTimes[0].Days, cloned.InvoiceGroups[0].Invoices[0].DayTimes[0].Days);
            Assert.AreNotEqual(availSource.Outlets[0].MarketZone, cloned.Outlets[0].MarketZone);
            Assert.AreNotEqual(availSource.Outlets[0].Name, cloned.Outlets[0].Name);
        }
    }
}

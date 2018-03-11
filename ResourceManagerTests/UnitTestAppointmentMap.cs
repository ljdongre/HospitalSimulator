using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ResourceManager;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace ResourceManagerTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class UnitTestAppointmentMap
    {
        [TestMethod]
        public void TestAppointmentMapInitialization()
        {
            var po = new PrivateObject(new AppointmentMap(DateTime.Parse("3/7/2018"), 1));
            /*
             *  byte[] _availabilityMap;
                DateTime _referenceDate;
                uint _size;
                long _head;
                uint _count;
             */

            var map = (byte[])po.GetField("_availabilityMap");
            Assert.IsTrue(map.Length == 1);
            Assert.IsTrue((DateTime)po.GetField("_referenceDate") == DateTime.Parse("3/5/2018"));
            Assert.IsTrue((uint)po.GetField("_size") == 1);
            Assert.IsTrue((long)po.GetField("_head") == 0);          
        }

        [TestMethod]
        public void TestAppointmentBookFull_1()
        {
            const uint appointmentBookSize = 1;

            var a = new AppointmentMap(DateTime.Parse("3/7/2018"), appointmentBookSize);
            var po = new PrivateObject(a);

            var map = (byte[])po.GetField("_availabilityMap");
            for (int i = 0; i < map.Length; i++)
            {
                map[i] = 0xFF;
            }
            po.SetField("_availabilityMap", map);

            Assert.IsTrue(map.Length == appointmentBookSize);
            Assert.IsTrue(map[0] == 0xFF);

            var result = a.ReserveFirstAvailableExcludeRequestedDate();
            Assert.IsFalse(result.Item1);
        }

        [TestMethod]
        public void TestAppointmentBookFull_2()
        {
            const uint appointmentBookSize = 24;

            var a = new AppointmentMap(DateTime.Parse("3/7/2018"), appointmentBookSize);
            var po = new PrivateObject(a);
            var map = (byte[])po.GetField("_availabilityMap");
            for(int i =0; i < map.Length; i++)
            {
                map[i] = 0xFF;
            }
            po.SetField("_availabilityMap", map);
           
            Assert.IsTrue(map.Length == appointmentBookSize);
            Assert.IsTrue(map[0] == 0xFF);
            Assert.IsTrue(map[map.Length-1] == 0xFF);

            var result = a.ReserveFirstAvailableExcludeRequestedDate();
            Assert.IsFalse(result.Item1);
        }

        [TestMethod]
        public void TestAppointment2MonthIntoFuture()
        {
            const uint appointmentBookSize = 24;

            var a = new AppointmentMap(DateTime.Parse("12/7/2017"), appointmentBookSize);
            var po = new PrivateObject(a);
            var map = (byte[])po.GetField("_availabilityMap");
            
            Assert.IsTrue(map.Length == appointmentBookSize);
            Assert.IsTrue((DateTime)po.GetField("_referenceDate") == DateTime.Parse("12/4/2017"));

            DateTime firstAllocationExpectedDate = GetExpectedReservationDate();
            DateTime referenceDate = firstAllocationExpectedDate;

            var result = a.ReserveFirstAvailableExcludeRequestedDate();
            Assert.IsTrue(result.Item1);
            Assert.IsTrue(result.Item2.Date == referenceDate);
        }

        [TestMethod]
        public void TestMultipleAppointment2MonthIntoFuture()
        {
            const uint appointmentBookSize = 24;

            var a = new AppointmentMap(DateTime.Parse("12/7/2017"), appointmentBookSize);
            var po = new PrivateObject(a);
            var map = (byte[])po.GetField("_availabilityMap");

            Assert.IsTrue(map.Length == appointmentBookSize);
            Assert.IsTrue((DateTime)po.GetField("_referenceDate") == DateTime.Parse("12/4/2017"));

            DateTime firstAllocationExpectedDate = GetExpectedReservationDate();
            DateTime referenceDate = firstAllocationExpectedDate;

            for (int i = 0; i < 100; i++)
            {
                 var result = a.ReserveFirstAvailableExcludeRequestedDate();
                Assert.IsTrue(result.Item1);
                Assert.IsTrue(result.Item2.Date == referenceDate);
                referenceDate = GetNextExpectedReservationDate(referenceDate);
            }
        }

        [TestMethod]
        public void TestBookMultipleAppointmentsInTheFuture_Cancel_One_And_BookAgainShouldReturnTheCancelledOne()
        {
            const uint appointmentBookSize = 24;

            var a = new AppointmentMap(DateTime.Parse("12/7/2017"), appointmentBookSize);
            var po = new PrivateObject(a);
            var map = (byte[])po.GetField("_availabilityMap");

            Assert.IsTrue(map.Length == appointmentBookSize);
            Assert.IsTrue((DateTime)po.GetField("_referenceDate") == DateTime.Parse("12/4/2017"));

            var firstAppointmentDate = DateTime.Now.Date;

            Dictionary<int, DateTime> allocatedDates = new Dictionary<int, DateTime>();

            DateTime firstAllocationExpectedDate = GetExpectedReservationDate();
            DateTime referenceDate = firstAllocationExpectedDate;

            for (var i = 0; i < 100; i++)
            {                
                var result = a.ReserveFirstAvailableExcludeRequestedDate();
                Assert.IsTrue(result.Item1);
                Assert.IsTrue(result.Item2.Date == referenceDate);
                referenceDate = GetNextExpectedReservationDate(referenceDate);

                allocatedDates.Add(i, result.Item2.Date);
            }


            // Run Cancel/allocation loop multiple time...
            for (int i =0; i < 10; i++)
            {
                var j = new Random().Next(0, allocatedDates.Keys.Count);
                if (allocatedDates.ContainsKey(j))
                {
                    a.CancelAppointment(allocatedDates[j]);
                    var result = a.ReserveFirstAvailableExcludeRequestedDate();

                    Assert.IsTrue(result.Item1);
                    Assert.IsTrue(result.Item2.Date == allocatedDates[j]);
                }
            }
 
        }


        [TestMethod]
        public void TestCancelMultipleinARowAndBookAppointShouldGetTheSmallestDateInTheCancelledSet()
        {
            const uint appointmentBookSize = 24;

            var a = new AppointmentMap(DateTime.Parse("12/7/2017"), appointmentBookSize);
            var po = new PrivateObject(a);
            var map = (byte[])po.GetField("_availabilityMap");

            Assert.IsTrue(map.Length == appointmentBookSize);
            Assert.IsTrue((DateTime)po.GetField("_referenceDate") == DateTime.Parse("12/4/2017"));

            var firstAppointmentDate = DateTime.Now.Date;

            Dictionary<int, DateTime> allocatedDates = new Dictionary<int, DateTime>();

            DateTime firstAllocationExpectedDate = GetExpectedReservationDate();
            DateTime referenceDate = firstAllocationExpectedDate;

            for (var i = 0; i < 100; i++)
            {
                var result = a.ReserveFirstAvailableExcludeRequestedDate();
                Assert.IsTrue(result.Item1);

                Assert.IsTrue(result.Item2.Date == referenceDate);
                referenceDate = GetNextExpectedReservationDate(referenceDate);

                allocatedDates.Add(i, result.Item2.Date);
            }

            List<Tuple<int, DateTime>> cancelledList = new List<Tuple<int, DateTime>>();
            // Run Cancel/allocation loop multiple time...

            var random = new Random();

            for (int i = 0; i < 20; i++)
            {
                var j = random.Next(0, allocatedDates.Keys.Count);
                if (allocatedDates.ContainsKey(j))
                {
                    a.CancelAppointment(allocatedDates[j]);
                    cancelledList.Add(new Tuple<int, DateTime>(j, allocatedDates[j]));
                }
            }

            cancelledList.Sort();
            var r = a.ReserveFirstAvailableExcludeRequestedDate();

            Assert.IsTrue(r.Item1);
            Assert.IsTrue(r.Item2.Date == cancelledList[0].Item2);

        }

        [TestMethod]
        public void TestRangeQuery()
        {
            const uint appointmentBookSize = 24;

            var a = new AppointmentMap(DateTime.Parse("03/1/2018"), appointmentBookSize);

            SortedSet<DateTime> allocatedDates = new SortedSet<DateTime>();

            for (var i = 0; i < 100; i++)
            {
                var result = a.ReserveFirstAvailableExcludeRequestedDate();
                Assert.IsTrue(result.Item1);
                allocatedDates.Add(result.Item2);
            }

            var maxDate = allocatedDates.Max;
            var refDate = maxDate;

            const int rangeSize = 10;

            var expectedRange = Enumerable.Range(1, rangeSize).Aggregate(new List<DateTime>(), (l, inex) =>
             {
                 refDate = GetNextExpectedReservationDate(refDate);
                 l.Add(refDate);
                 return l;
             });


            var availableRange = a.RetrieveFirstAvailableDateRangeExcludeRequestedDate(rangeSize);

            for(int i =0; i < rangeSize; i++)
            {
                Assert.IsTrue(expectedRange[i] == availableRange[i]);
            }

        }

        [TestMethod]
        public void TestRangeQueryWhereAppointmentHasHolesinReservedSpots()
        {
            const uint appointmentBookSize = 24;

            var a = new AppointmentMap(DateTime.Parse("03/1/2018"), appointmentBookSize);

            Dictionary<int, DateTime> allocatedDates = new Dictionary<int, DateTime>();

            for (var i = 0; i < 100; i++)
            {
                var result = a.ReserveFirstAvailableExcludeRequestedDate();
                Assert.IsTrue(result.Item1);
                allocatedDates.Add(i, result.Item2);
            }

            const int rangeSize = 10;

            var cancelledAppointments = new SortedSet<DateTime>();
           
            var random = new Random();
            for (int i = 0; i < rangeSize; i++)
            {
                var j = random.Next(0, allocatedDates.Count);
                if (allocatedDates.ContainsKey(j))
                {
                    a.CancelAppointment(allocatedDates[j]);
                    cancelledAppointments.Add(allocatedDates[j]);
                }
            }

            var tempList = cancelledAppointments.ToList();


            var availableAppointmentRange = a.RetrieveFirstAvailableDateRangeExcludeRequestedDate(rangeSize);

            for (int i = 0; i < Math.Min(availableAppointmentRange.Count, tempList.Count); i++)
            {
                Assert.IsTrue(tempList[i] == availableAppointmentRange[i], 
                    string.Format("failed Comarison for {0}: {1}, {2}", 
                    i, tempList[i], availableAppointmentRange[i]));
            }

        }

        // If we hade a rule Engine we could have asked the Rules engine to get the expected date
        // for comparison.
        // For now us eth efollowing logic
        //
        DateTime  GetExpectedReservationDate()
        {
            // If scheduled data is Friday/Saturday or Sunday -- The next available date returned is
            // 3, 2, or 1 day away.
            //
            var increment = 1;
            switch(DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Friday:
                    increment = 3;
                    break;
                case DayOfWeek.Saturday:
                    increment = 2;
                    break;
                default:
                    increment = 1;
                    break;
            }
            return DateTime.Now.AddDays(increment).Date;
        }

        // when we are doing consecutive reservations ...
        DateTime GetNextExpectedReservationDate(DateTime refdate)
        {
            // If scheduled data is Friday/Saturday or Sunday -- The next available date returned is
            // 3, 2, or 1 day away.
            //
            var increment = 1;
            switch (refdate.DayOfWeek)
            {
                case DayOfWeek.Friday:
                    increment = 3;
                    break;
                case DayOfWeek.Saturday:
                    increment = 2;
                    break;
                default:
                    increment = 1;
                    break;
            }
            return refdate.AddDays(increment).Date;
        }
    }
}

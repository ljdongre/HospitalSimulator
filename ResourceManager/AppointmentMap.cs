using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceManager
{
    /// <summary>
    /// Keeps appointment mask for 6 months in the future.
    /// Each week of the appointment space is modeled as a Byte.
    /// Days Monday-friday are modelled as MSB to LSB, 3 bits of bytes marked unusable.
    /// 
    /// Initial Condition:
    ///     Reference Date = Current Date.
    ///     CurrentWeekOffset = 0
    /// During Execution:
    ///     Adjust CurrentWeekOffset  (CurrentDate - ReferenceDate).Days/7
    /// </summary>
    public class AppointmentMap
    {
        // we allow appointments 24 weeks in advance.

        const int FutureAppointmentSpanInWeeks = 24;

        public AppointmentMap(DateTime refDate, uint futureAppointmentSpanInWeek = FutureAppointmentSpanInWeeks)
        {
            var noOfWeeks = DateTime.Now + new TimeSpan(days:(int)futureAppointmentSpanInWeek * 7, hours:0, minutes:0, seconds:0);
            _availabilityMap = new byte[futureAppointmentSpanInWeek];

            // By design the LSB 2 bits are unavailable for allocation.
            // appointments can only be booked from Monday-Friday ( first 5 bits of 8 bit byte)
            //
            for (int i = 0; i < _availabilityMap.Length; i++)
            {
                _availabilityMap[i] = 0x07;
            }

            _referenceDate = GetReferenceDateForMondayOfCurrentWeek(refDate);
            _size = futureAppointmentSpanInWeek;
            _head = 0;
            Advance(refDate);
        }

        // MSB => LSB Monday/Tuesday ... etc.)
        const byte Mondaymask = 0x80;

        public Tuple<bool, DateTime> ReserveFirstAvailableExcludeRequestedDate()
        {
            AdjustReferenceDate(DateTime.Now);

            var firstAvailableSlot = GetFirstAvailableWeekSlotAndDayOffset();
            if (!firstAvailableSlot.Item1)
                return new Tuple<bool, DateTime>(false, DateTime.MinValue);

            var currentHead = firstAvailableSlot.Item2.Item1;
            var weekOffset = firstAvailableSlot.Item2.Item2;

            var appointmentDate = ConvertSlotAndOffsetToDate(currentHead, weekOffset);

            MarkSlotUnAvailable(appointmentDate);

            //_availabilityMap[currentHead] |= (byte)(Mondaymask >> weekOffset);

            return new Tuple<bool, DateTime>(true, appointmentDate);
        }

        public void MarkSlotUnAvailable(DateTime date)
        {
            var weekAndItsOfset = ConvertDateToSlotAndOffset(date);
            _availabilityMap[weekAndItsOfset.Item1] |= (byte)(Mondaymask >> weekAndItsOfset.Item2);
        }

        public void MarkSlotAvailable(DateTime date)
        {
            var weekAndItsOfset = ConvertDateToSlotAndOffset(date);
            byte mask = Mondaymask;
            mask >>= weekAndItsOfset.Item2;
            _availabilityMap[weekAndItsOfset.Item1] &= (byte)~mask;
        }


        public void CancelAppointment(DateTime date)
        {
            AdjustReferenceDate(DateTime.Now);
            if (date < DateTime.Now) return;
            MarkSlotAvailable(date);

            //var weekOffset = (date.Date - _referenceDate.Date).Days / 7;

            //var currentHead = (_head + weekOffset) % _size;
            //byte mask = Mondaymask;
            //mask >>= ((date.Date - _referenceDate.Date).Days % 7);
            //_availabilityMap[currentHead] &= (byte)~mask;
        }


        public List<DateTime> RetrieveFirstAvailableDateRangeExcludeRequestedDate(int rangeSize)
        {
            AdjustReferenceDate(DateTime.Now);

            var currentHead = _head;
            var firstAvailableSlot = GetFirstAvailableWeekSlotAndDayOffset();

            if (!firstAvailableSlot.Item1)
                return new List<DateTime>();

            var availableRange = new List<DateTime>();

            availableRange.Add(ConvertSlotAndOffsetToDate(firstAvailableSlot.Item2.Item1,
                firstAvailableSlot.Item2.Item2));

            return Enumerable.Range(1, rangeSize).Aggregate(availableRange,
                (l, e) =>
                {
                    currentHead = firstAvailableSlot.Item2.Item1;
                    firstAvailableSlot = GetNextAvailableWeekSlotAndDayOffsetStartingFromAGivenSlot(firstAvailableSlot.Item2.Item1,
                        firstAvailableSlot.Item2.Item2);
                    if (firstAvailableSlot.Item1)
                    {
                        l.Add(ConvertSlotAndOffsetToDate(firstAvailableSlot.Item2.Item1,
                                firstAvailableSlot.Item2.Item2));
                    }
                    return l;
                });
        }

        /// <summary>
        /// <TODO>
        ///     Think of better name.
        ///     Should we aggrgate the rules in some rule engine?
        /// </TODO> 
        ///     
        /// </summary>
        bool IsScheduedOnSameDayAsRequestedExceptForSaturdayOrSunday(DateTime requestDate)
        {
            var currentHeadDate = ConvertSlotAndOffsetToDate(_head, ConvertDayofWeekTodayOffset(requestDate.DayOfWeek));

            // when we are allocating a reservation date which is in future, because the current slots are full,
            // we will never use the current day as possible reservation date.

            if (requestDate > currentHeadDate) return false;

            if (requestDate.Date.DayOfWeek == DayOfWeek.Saturday ||
                requestDate.Date.DayOfWeek == DayOfWeek.Sunday)
                return false;
            return true;
        }

        Tuple<bool, Tuple<long, int>> GetFirstAvailableWeekSlotAndDayOffset()
        {
            var currentHead = _head;
            var dayOffset = (7 + (DateTime.Now.DayOfWeek - DayOfWeek.Monday)) % 7;

            // We do not reserve patient on the same day of the reservation request.
            // But no work is done onsaturay
            if (IsScheduedOnSameDayAsRequestedExceptForSaturdayOrSunday(DateTime.Now.Date)) dayOffset++;

            var weekOffset = 0;
            while ((weekOffset = GetAvailableSlotinWeek(currentHead, dayOffset)) == -1)
            {
                currentHead = (currentHead + 1) % _size;
                if (currentHead == _head)
                {
                    SimpleLogger.SimpleLogger.Instance.Log(SimpleLoggerContract.LogLevel.Info,
                        string.Format("Failed to book appointment, calender is full for next six month, Allocation Map: [{0}]",
                        _availabilityMap.Aggregate(new StringBuilder(), (sb, b) =>
                        {
                            sb.Append(string.Format("{0:X2} ", b));
                            return sb;
                        })));
                    return new Tuple<bool, Tuple<long, int>>(false, new Tuple<long, int>( 0, 0 ));
                }
                dayOffset = 0;
            }

            return new Tuple<bool, Tuple<long, int>>(true, new Tuple<long, int>(currentHead, weekOffset));
        }

        Tuple<bool, Tuple<long, int>> GetNextAvailableWeekSlotAndDayOffsetStartingFromAGivenSlot(long startHead,
                int dayOffset)
        {
            var currentHead = startHead;
            dayOffset++;
            var weekOffset = 0;
            while ((weekOffset = GetAvailableSlotinWeek(currentHead, dayOffset)) == -1)
            {
                currentHead = (currentHead + 1) % _size;
                if (currentHead == _head)
                {
                    SimpleLogger.SimpleLogger.Instance.Log(SimpleLoggerContract.LogLevel.Info,
                        string.Format("Failed to book appointment, calender is full for next six month, Allocation Map: [{0}]",
                        _availabilityMap.Aggregate(new StringBuilder(), (sb, b) =>
                        {
                            sb.Append(string.Format("{0:X2} ", b));
                            return sb;
                        })));
                    return new Tuple<bool, Tuple<long, int>>(false, new Tuple<long, int>(0, 0));
                }
                dayOffset = 0;
            }

            return new Tuple<bool, Tuple<long, int>>(true, new Tuple<long, int>(currentHead, weekOffset));
        }


        DateTime ConvertSlotAndOffsetToDate(long currentHead, int weekOffset)
        {
            var days = currentHead >= _head
                 ? (currentHead - _head) * 7
                 : (_size - _head + currentHead) * 7;
            days += weekOffset;
            return _referenceDate.Date.AddDays(days);
        }

        Tuple<long, int> ConvertDateToSlotAndOffset(DateTime date)
        {
            var days = (date.Date - _referenceDate.Date).Days;
            var weekSlot = (_head + (days / 7)) % _size;
            var weekOffset = days % 7;
            return new Tuple<long, int>(weekSlot, weekOffset);
        }

        void AdjustReferenceDate(DateTime date)
        {
            Advance(date);
            _referenceDate = GetReferenceDateForMondayOfCurrentWeek(date);
        }
        /// <summary>
        /// if week is full return -1;
        /// 
        /// </summary>
        /// <returns></returns>
        int GetAvailableSlotinWeek(long currentWeek, int dayOffset)
        {
            int offset = dayOffset;
            byte mask = (byte)(Mondaymask >> dayOffset);
            while ((_availabilityMap[currentWeek] & mask) == mask && offset < 8)
            {
                mask = (byte)(mask >> 1);
                offset++;
            }
            return offset == 8 ? -1 : offset;
        }

        void Advance(DateTime current)
        {
            var weekOffset = (current.Date - _referenceDate.Date).Days / 7;
            _head = (_head + weekOffset) % _size;
            _referenceDate.AddDays(weekOffset * 7);
        }

        /// <summary>
        /// Note: 
        /// The reference day of the week is related to prior week day and thus
        /// for current day which smaller the saturday, the diff would be negetive.
        /// </summary>
        DateTime GetReferenceDateForMondayOfCurrentWeek(DateTime currentdate)
        {
            var dayOffset = (7 + (currentdate.DayOfWeek - DayOfWeek.Monday)) % 7;
            return currentdate.AddDays(-1 * dayOffset);
        }

        enum AppointMapDayOfWeek
        {
            Monday = 0,
            Tuesday,
            Wednesday,
            Thursday,
            Friday,
            Saturday,
            Sunday,
        };

        int ConvertDayofWeekTodayOffset(DayOfWeek day)
        {
            AppointMapDayOfWeek dayOffset = AppointMapDayOfWeek.Monday;
            switch (day)
            {
                case DayOfWeek.Monday:
                    dayOffset = AppointMapDayOfWeek.Monday;
                    break;
                case DayOfWeek.Tuesday:
                    dayOffset = AppointMapDayOfWeek.Tuesday;
                    break;
                case DayOfWeek.Wednesday:
                    dayOffset = AppointMapDayOfWeek.Wednesday;
                    break;
                case DayOfWeek.Thursday:
                    dayOffset = AppointMapDayOfWeek.Thursday;
                    break;
                case DayOfWeek.Friday:
                    dayOffset = AppointMapDayOfWeek.Friday;
                    break;
                case DayOfWeek.Saturday:
                    dayOffset = AppointMapDayOfWeek.Saturday;
                    break;
                case DayOfWeek.Sunday:
                    dayOffset = AppointMapDayOfWeek.Sunday;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("DayOfWeek");
            }
            return (int)dayOffset;
        }

        byte[] _availabilityMap;
        DateTime _referenceDate;
        uint _size;
        long _head;
    }
}

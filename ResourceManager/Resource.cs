using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceManager
{
    public abstract class Resource : IResource
    {
        public string ID { get; private set; }
        AppointmentMap _appointments;
        const uint appointmentBookSize = 24;

        public Resource(string id)
        {
            ID = id;
            _appointments = new AppointmentMap(DateTime.Parse("03/01/2018"), 
                appointmentBookSize);
        }

        public List<DateTime> AvailableDateRange(int rangeSize)
        {
            return _appointments.RetrieveFirstAvailableDateRangeExcludeRequestedDate(rangeSize);
        }

        public void ReserverDate(DateTime date)
        {
            _appointments.MarkSlotUnAvailable(date);
        }

        public void UnreserveDate(DateTime date)
        {
            _appointments.MarkSlotUnAvailable(date);
        }
    }
}

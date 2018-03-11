using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceManager
{
    public interface IResource
    {
        string ID { get; }
        List<DateTime> AvailableDateRange(int rangeSize);
        void ReserverDate(DateTime date);
        void UnreserveDate(DateTime date);
    }
}

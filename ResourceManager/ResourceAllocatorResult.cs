using HospitalSimulatorService.Contract.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceManager
{
    public class ResourceAllocatorResult
    {
        public bool Status { get; private set; }
        public ConsultationRecord  Consultation { get; private set; }

        public ResourceAllocatorResult()
        {
            Status = false;
            Consultation = new ConsultationRecord();
        }
        public ResourceAllocatorResult(bool status, ConsultationRecord consulation) : this()
        {
            Status = status;
            Consultation = consulation;
        }

    }
}

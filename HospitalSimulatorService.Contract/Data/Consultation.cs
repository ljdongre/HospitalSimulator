using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HospitalSimulatorService.Contract.Data
{
    [DataContract]
    public class Consultation
    {
        [DataMember]
        public string Patient { get; private set; }
        [DataMember]
        public string TreatmentRoom { get; private set; }

        [DataMember]
        public string Doctor { get; private set; }

        [DataMember]
        public DateTime DateRegistered { get; private set; }
        [DataMember]
        public DateTime ConsulationDate { get; private set; }

        public Consultation(string patientName, string roomName, 
            string doctorName, DateTime registrationdate, 
            DateTime consultationDate)
        {
            Patient = patientName;
            TreatmentRoom = roomName;
            Doctor = doctorName;
            DateRegistered = registrationdate;
            ConsulationDate = consultationDate;
        }
    }
}

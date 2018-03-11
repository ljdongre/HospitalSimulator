using HospitalSimulatorService.Contract.Data;
using System;

namespace ResourceManager
{
    public class ConsultationRecord
    {
        public Patient Patient { get; private set; }
        public TreatmentRoom TreatmentRoom { get; private set; }

        public Doctor Doctor { get; private set; }

        public DateTime DateRegistered { get; private set; }
        public DateTime ConsulatationDate { get; private set; }

        public ConsultationRecord()
        {
            Patient = new Patient(string.Empty, PatientCondition.None, ConditionTopography.None);
            TreatmentRoom = new TreatmentRoom(string.Empty, 
                new TreatmentMachine(string.Empty, TreatmentMachineCapabilities.None));
            Doctor = new Doctor(string.Empty, new DoctorRole[] { });
            DateRegistered = DateTime.MinValue;
            ConsulatationDate = DateTime.MinValue;
        }

        public ConsultationRecord(Patient patient, TreatmentRoom room,
            Doctor doctor, DateTime registrationdate,
            DateTime consultationDate) : this()
        {
            Patient = patient;
            TreatmentRoom = room;
            Doctor = doctor;
            DateRegistered = registrationdate;
            ConsulatationDate = consultationDate;
        }
    }
}


using System;
using System.Collections.Generic;
using System.ServiceModel;
using HospitalSimulatorService.Contract.Data;
using HospitalSimulatorService.Contract.Service;

namespace HospitalSimulatorService.Contract.Proxy
{
    public class HospitalSimulatorProxy : ClientBase<IHospitalSimulator>, IHospitalSimulator
    {
        public bool IsAlive()
        {
            return Channel.IsAlive();
        }

        public void RegisterPatient(Patient patient)
        {
            Channel.RegisterPatient(patient);
        }

        public Tuple<bool, Consultation> IsRegistrationSuccessful(string patientName)
        {
            return Channel.IsRegistrationSuccessful(patientName);
        }

        public IEnumerable<Patient> RegisteredPatients()
        {
            return Channel.RegisteredPatients();
        }

        public IEnumerable<Consultation> ScheduledConsultations()
        {
            return Channel.ScheduledConsultations();
        }
    }
}

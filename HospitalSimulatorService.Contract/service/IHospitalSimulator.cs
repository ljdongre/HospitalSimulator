
using System.Collections.Generic;
using HospitalSimulatorService.Contract.Data;
using System.ServiceModel;
using System;

namespace HospitalSimulatorService.Contract.Service
{
    [ServiceContract(Name = "IHospitalSimulator")]
    public interface IHospitalSimulator
    {
        [OperationContract]
        bool IsAlive();

        [OperationContract(IsOneWay = true)]
        void RegisterPatient(Patient patient);

        [OperationContract]
        Tuple<bool, Consultation> IsRegistrationSuccessful(string patientName);

        [OperationContract]
        IEnumerable<Patient> RegisteredPatients();

        [OperationContract]
        IEnumerable<Consultation> ScheduledConsultations();

    }
}

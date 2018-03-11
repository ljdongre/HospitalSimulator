using HospitalSimulatorService.Contract.Data;
using HospitalSimulatorService.Contract.Service;
using ResourceManager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace HospitalSimulatorService
{

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
    public class HSService : IHospitalSimulator, IDisposable
    {

        static ResourceAllocator resourceAllocator = new ResourceAllocator();
        /// <summary>
        /// Default constructor
        /// </summary>
        public HSService()
        {
            // read and initialize the resources.
        }
       

        /// <summary>
        /// Release resource 
        /// </summary>
        public void Dispose()
        {
            //resourceAllocator.Dispose();
        }

        public bool IsAlive()
        {
            return true;
        }

        void IHospitalSimulator.RegisterPatient(Patient patient)
        {
            resourceAllocator.RequestResources(patient, (result) =>
            {
                if (result.Status)
                {
                    _consultations.Add(result.Consultation);
                }
            });
        }

        public Tuple<bool, Consultation> IsRegistrationSuccessful(string patientName)
        {
            var result = _consultations.ToArray().Where(c => c.Patient.Name == patientName).ToList();
            return new Tuple<bool, Consultation>(result.Any(),
                result.Any() 
                    ? new Consultation(result.First().Patient.Name,
                            result.First().TreatmentRoom.ID, 
                            result.First().Doctor.ID, 
                            result.First().DateRegistered,
                            result.First().ConsulatationDate)
                    : new Consultation(string.Empty, string.Empty, string.Empty, DateTime.MinValue,
                        DateTime.MaxValue));
        }

        public IEnumerable<Patient> RegisteredPatients()
        {
            return _consultations.ToArray().Select(c => c.Patient).ToList();
        }

        private static ConcurrentBag<ConsultationRecord> _consultations =
                new ConcurrentBag<ConsultationRecord>();
        public IEnumerable<Consultation> ScheduledConsultations()
        {
            return _consultations.ToArray().Select(c =>
            new Consultation(c.Patient.Name, c.TreatmentRoom.ID, c.Doctor.ID, c.DateRegistered, c.ConsulatationDate)).ToList();
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalSimulatorService.Contract.Data;

namespace ResourceManager
{
    public class DoctorResourceRule : IResourceRule
    {
        public bool IsResourceValid(IResource resource, Patient patient)
        {
            var doctorResource = resource as Doctor;

            bool resourceAvailable = false;
            if (doctorResource == null)
                return resourceAvailable;

            switch(patient.Condition)
            {
                case PatientCondition.Cancer:
                    resourceAvailable = doctorResource.Roles.Any(r => r == DoctorRole.Oncologist);
                    break;
                case PatientCondition.Flu:
                    resourceAvailable = doctorResource.Roles.Any(r => r == DoctorRole.GeneralPractitioner);
                    break;
                default:
                    resourceAvailable = false;
                    break;
            }
            return resourceAvailable;
        }
    }
}

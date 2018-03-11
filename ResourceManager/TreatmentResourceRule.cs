using HospitalSimulatorService.Contract.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceManager
{
    public class TreatmentResourceRule : IResourceRule
    {
        public bool IsResourceValid(IResource resource, Patient patient)
        {
            var roomResource = resource as TreatmentRoom;
            if (roomResource == null)
                return false;
            // NOTE: we can use TreatmentRoom which has advanced capability machine to treat breat cancer.
            // For this implementation we care excluding this implementation.
            // The decison for now arbitrary.

            if (patient.Condition == PatientCondition.Cancer)
            {
                if (!roomResource.HasMachine) return false;
                if (patient.PatientConditionTopography == ConditionTopography.HeadAndNeck)
                    return roomResource.Machine.Capabilities == TreatmentMachineCapabilities.Advanced;
                else
                    return (roomResource.Machine.Capabilities == TreatmentMachineCapabilities.Simple);
            }
            else
            {
                return !roomResource.HasMachine;
            }
        }
    }
}

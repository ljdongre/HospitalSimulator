using HospitalSimulatorService.Contract.Data;

namespace ResourceManager
{
    public class TreatmentMachine : Resource
    {
        public TreatmentMachineCapabilities Capabilities { get; private set; }
        public TreatmentMachine(string id, TreatmentMachineCapabilities capabilities) : base(id)
        {
            Capabilities = capabilities;
        }
    }
}

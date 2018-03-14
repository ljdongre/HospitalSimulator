using HospitalSimulatorService.Contract.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceManager
{
    public class TreatmentRoom : Resource
    {
        public TreatmentMachine Machine { get; private set; }
        public TreatmentRoom(string id, TreatmentMachine machine) : base(id)
        {
            Machine = machine;
        }

        public bool HasAdvancedCapabilities { get
            {
                return HasMachine && Machine.Capabilities == TreatmentMachineCapabilities.Advanced;
            } }

        public bool HasSimpleCapabilities
        {
            get
            {
                return HasMachine && Machine.Capabilities == TreatmentMachineCapabilities.Simple;
            }
        }

        public bool HasMachine
        {
            get
            {
                return Machine != null;
            }
        }

        public override string ToString()
        {
            return String.Format("Room: {0}, Machine: {1}{2}", 
                ID, HasMachine ? Machine.ID : string.Empty, 
                HasMachine ? HasAdvancedCapabilities ? ": Advanced" : ": Simple" : string.Empty);
        }
    }
}

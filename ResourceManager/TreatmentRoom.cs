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

    }
}

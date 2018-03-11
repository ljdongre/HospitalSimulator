using HospitalSimulatorService.Contract.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceManager
{
    public class Doctor : Resource
    {
        public DoctorRole[] Roles { get; private set; }
        public Doctor(string id, DoctorRole[] roles) : base(id)
        {
            Roles = roles;
        }
    }
}

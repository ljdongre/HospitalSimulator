using HospitalSimulatorService.Contract.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceManager
{
    public interface IResourceRule
    {
        bool IsResourceValid(IResource resource, Patient patient);
    }
}

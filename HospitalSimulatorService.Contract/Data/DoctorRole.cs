using System.Runtime.Serialization;

namespace HospitalSimulatorService.Contract.Data
{
    [DataContract]
    public enum  DoctorRole
    {
        [EnumMember]
        Oncologist,
        [EnumMember]
        GeneralPractitioner
    }
}

using System.Runtime.Serialization;

namespace HospitalSimulatorService.Contract.Data
{
    [DataContract]
    public enum PatientCondition
    {
        [EnumMember]
        Cancer,

        [EnumMember]
        Flu,

        [EnumMember]
        None
    }
}

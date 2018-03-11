using System.Runtime.Serialization;

namespace HospitalSimulatorService.Contract.Data
{
    [DataContract]
    public enum  ConditionTopography
    {
        [EnumMember]
        HeadAndNeck,

        [EnumMember]
        Breast,

        [EnumMember]
        None
    }
}

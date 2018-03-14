using System;
using System.Runtime.Serialization;

namespace HospitalSimulatorService.Contract.Data
{
    [DataContract]
    public class Patient
    {
        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public PatientCondition Condition { get; private set; }

        [DataMember]
        public ConditionTopography PatientConditionTopography { get; private set; }
        public Patient(string name, PatientCondition condition, ConditionTopography topography = ConditionTopography.None)
        {
            Name = name;
            Condition = condition;
            if (Condition == PatientCondition.Cancer && topography == ConditionTopography.None)
            {
                throw new ArgumentException("Missing Condition Topography...");
            }
            PatientConditionTopography = topography;
        }

        public override string ToString()
        {
            return string.Format("Name: '{0}', {1}{2}",
                Name, Condition.ToString(),
                PatientConditionTopography == ConditionTopography.None ?
                    string.Empty : string.Format(":{0}", PatientConditionTopography.ToString()));
        }
    }
}

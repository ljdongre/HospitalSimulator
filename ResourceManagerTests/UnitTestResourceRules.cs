using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HospitalSimulatorService.Contract.Data;
using ResourceManager;
using System.Diagnostics.CodeAnalysis;

namespace ResourceManagerTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class UnitTestResourceRules
    {
        [TestMethod]
        public void TestMethodPatientHasHeadAndNeckCancerAndResourceIsTreatmentRoomWithSimpleCapabilitiesReturnsFalse()
        {
            var p = new Patient("A Patient", PatientCondition.Cancer, ConditionTopography.HeadAndNeck);
            var room = new TreatmentRoom("Simple Room", new TreatmentMachine("machine 1", TreatmentMachineCapabilities.Simple));

            var resourceRule = new TreatmentResourceRule();
            Assert.IsFalse(resourceRule.IsResourceValid(room, p));
        }

        [TestMethod]
        public void TestMethodPatientHasBreastCancerAndResourceIsTreatmentRoomWithSimpleCapabilitiesReturnsTrue()
        {
            var p = new Patient("A Patient", PatientCondition.Cancer, ConditionTopography.Breast);
            var room = new TreatmentRoom("Simple Room", new TreatmentMachine("machine 1", TreatmentMachineCapabilities.Simple));

            var resourceRule = new TreatmentResourceRule();
            Assert.IsTrue(resourceRule.IsResourceValid(room, p));
        }

        [TestMethod]
        public void TestMethodPatientHasBreastCancerAndDoctorResourceIsGeneralPractitionerDoctorReturnsFalse()
        {
            var p = new Patient("A Patient", PatientCondition.Cancer, ConditionTopography.Breast);
            var doctor = new Doctor("GP", new[] { DoctorRole.GeneralPractitioner });
           
            var resourceRule = new DoctorResourceRule();
            Assert.IsFalse(resourceRule.IsResourceValid(doctor, p));
        }


        [TestMethod]
        public void TestMethodPatientHasBreastCancerAndDoctorResourceIsBothGeneralPractitionerAndOncologistDoctorReturnsTrue()
        {
            var p = new Patient("A Patient", PatientCondition.Cancer, ConditionTopography.Breast);
            var doctor = new Doctor("GP", new[] { DoctorRole.GeneralPractitioner, DoctorRole.Oncologist });

            var resourceRule = new DoctorResourceRule();
            Assert.IsTrue(resourceRule.IsResourceValid(doctor, p));
        }

        [TestMethod]
        public void TestMethodPatientHasFlueAndDoctorResourceIsOncologistDoctorReturnsFalse()
        {
            var p = new Patient("A Patient", PatientCondition.Flu);
            var doctor = new Doctor("GP", new[] { DoctorRole.Oncologist });

            var resourceRule = new DoctorResourceRule();
            Assert.IsFalse(resourceRule.IsResourceValid(doctor, p));
        }


        [TestMethod]
        public void TestMethodPatientHasFlueAndDoctorResourceIsBothGeneralPractitionerAndOncologistDoctorReturnsTrue()
        {
            var p = new Patient("A Patient", PatientCondition.Flu);
            var doctor = new Doctor("GP", new[] { DoctorRole.GeneralPractitioner, DoctorRole.Oncologist });

            var resourceRule = new DoctorResourceRule();
            Assert.IsTrue(resourceRule.IsResourceValid(doctor, p));
        }

    }
}

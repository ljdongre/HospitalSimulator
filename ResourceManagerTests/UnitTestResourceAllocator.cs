using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using HospitalSimulatorService.Contract.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ResourceManager;

namespace ResourceManagerTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class UnitTestResourceAllocator
    {
        [TestMethod]
        public void TestMethodResourceForPatientWthFlu()
        {
            var patient = new Patient("Test Patient", PatientCondition.Flu);
            var resourceAllocator = new ResourceAllocator();

            var consultation = 
                new ConsultationRecord(patient, null, null, DateTime.Now.Date, DateTime.MinValue);
            ResourceAllocatorResult r = new ResourceAllocatorResult(false, consultation);

            ManualResetEvent done = new ManualResetEvent(false);
            resourceAllocator.RequestResources(patient, (result) =>
            {
                r = result;
                done.Set();
            });

            done.WaitOne();
            resourceAllocator.Dispose();

            Assert.IsTrue(r.Status, "Failed to get Resource..");
            Assert.IsTrue(r.Consultation.TreatmentRoom != null, "Failed to get Treatment Room..");
            Assert.IsTrue(r.Consultation.Doctor != null, "Failed to get Doctor..");
            Assert.IsTrue(r.Consultation.DateRegistered == consultation.DateRegistered, "Registration date is incorrect.");
            Assert.IsTrue(r.Consultation.ConsulatationDate != DateTime.MinValue, "Consultation date is incorrect.");
        }
    }
}

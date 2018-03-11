using HospitalSimulatorService.Contract.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManager
{
    public class ResourceAllocator : IDisposable
    {
        List<Doctor> _doctors = new List<Doctor>
        {
            new Doctor ("John", new DoctorRole[] {DoctorRole.Oncologist }),
            new Doctor ("Anna", new DoctorRole[] {DoctorRole.GeneralPractitioner }),
            new Doctor ("Laura", new DoctorRole[] {DoctorRole.Oncologist, DoctorRole.GeneralPractitioner }),
        };


        List<TreatmentRoom> _treatmentRooms = new List<TreatmentRoom>
        {
            new TreatmentRoom ("RoomOne", null),
            new TreatmentRoom ("RoomTwo", null),
            new TreatmentRoom ("RoomThree", new TreatmentMachine ("MachineA", TreatmentMachineCapabilities.Advanced)),
            new TreatmentRoom ("RoomFour", new TreatmentMachine ("MachineB", TreatmentMachineCapabilities.Advanced)),
            new TreatmentRoom ("RoomFive", new TreatmentMachine ("MachineC", TreatmentMachineCapabilities.Simple)),
        };

        private Task _allocateResourceTask;

        private BlockingCollection<Tuple<ConsultationRecord, Action<ResourceAllocatorResult>>> _resourceRequests = 
            new BlockingCollection<Tuple<ConsultationRecord, Action<ResourceAllocatorResult>>>();

        public CancellationTokenSource Cancel { get; private set; }

        public ResourceAllocator()
        {
            _allocateResourceTask = Task.Factory.StartNew(() => ReserveResources());
            Cancel = new CancellationTokenSource();
        }

        public void RequestResources(Patient p, Action<ResourceAllocatorResult> resultAction)
        {
            var consultation = new ConsultationRecord(p, null, null, DateTime.Now.Date, DateTime.MinValue);
             _resourceRequests.Add(new Tuple<ConsultationRecord, Action<ResourceAllocatorResult>>(consultation, resultAction));
        }
        private void ReserveResources()
        {
            while (true)
            {

                Tuple<ConsultationRecord, Action<ResourceAllocatorResult>> request;
                try
                {
                    if (!_resourceRequests.TryTake(out request, -1, Cancel.Token))
                    {
                        SimpleLogger.SimpleLogger.Instance.Log(SimpleLoggerContract.LogLevel.Info,
                            string.Format("ResourceAllocator: ReserveResources - Thread cancelled..."));
                        return;
                    }
                }
                catch(OperationCanceledException)
                {
                    return;
                }
 

                var patient = request.Item1.Patient;

                // TODO: Following LOG not HIPPA compliant.
                SimpleLogger.SimpleLogger.Instance.Log(SimpleLoggerContract.LogLevel.Info,
                    string.Format("ResourceAllocator: Reserving Resources for Patient: {0}", patient.Name));

                var doctorRule = new DoctorResourceRule();
                var availableDoctors = _doctors.Where(d => doctorRule.IsResourceValid(d, patient)).ToList();

                if (!availableDoctors.Any())
                {
                    SimpleLogger.SimpleLogger.Instance.Log(SimpleLoggerContract.LogLevel.Error,
                        string.Format("ResourceAllocator: Failed to find any Doctors for Patient: {0}", patient.Name));
                    Task.Factory.StartNew(() =>
                        request.Item2(new ResourceAllocatorResult(false, new ConsultationRecord())));
                }

                var treatmentRoomRule = new TreatmentResourceRule();
                var availableRooms = _treatmentRooms.Where(r => treatmentRoomRule.IsResourceValid(r, patient)).ToList();

                if (!availableRooms.Any())
                {
                    SimpleLogger.SimpleLogger.Instance.Log(SimpleLoggerContract.LogLevel.Error,
                        string.Format("ResourceAllocator: Failed to find any Treatment Rooms for Patient: {0}", patient.Name));

                    Task.Factory.StartNew(() =>
                    request.Item2(new ResourceAllocatorResult(false, new ConsultationRecord())));
                }

                const int rangeSize = 10;
                SortedDictionary<DateTime, List<IResource>> availableResourceDictionary =
                    new SortedDictionary<DateTime, List<IResource>>();

                //TODO: Make use of Parallel for after getting performance measurements

                availableDoctors.ForEach(doctor =>
                {
                    var dates = doctor.AvailableDateRange(rangeSize);
                    dates.ForEach(date =>
                    {
                        if (!availableResourceDictionary.ContainsKey(date))
                        {
                            availableResourceDictionary[date] = new List<IResource>();
                        }
                        availableResourceDictionary[date].Add(doctor);
                    });
                });

                //TODO: Make use of Parallel for after getting performance measurements
                availableRooms.ForEach(room =>
                {
                    var dates = room.AvailableDateRange(rangeSize);
                    dates.ForEach(date =>
                    {
                        if (!availableResourceDictionary.ContainsKey(date))
                        {
                            availableResourceDictionary[date] = new List<IResource>();
                        }
                        availableResourceDictionary[date].Add(room);
                    });
                });

                var firstDateWhichSatisfiesPatienTResourceConstraint = availableResourceDictionary.Keys.Where(d =>
                {
                    return (availableResourceDictionary[d].Any(r => r is Doctor) &&
                    availableResourceDictionary[d].Any(r => r is TreatmentRoom));
                }).FirstOrDefault();

                if (!availableResourceDictionary.ContainsKey(firstDateWhichSatisfiesPatienTResourceConstraint))
                    request.Item2(new ResourceAllocatorResult(false, request.Item1));

                var availableDoctor = availableResourceDictionary[firstDateWhichSatisfiesPatienTResourceConstraint]
                    .FirstOrDefault(r => r is Doctor);
                var availableRoom = availableResourceDictionary[firstDateWhichSatisfiesPatienTResourceConstraint]
                    .FirstOrDefault(r => r is TreatmentRoom);

                availableDoctor.ReserverDate(firstDateWhichSatisfiesPatienTResourceConstraint);
                availableRoom.ReserverDate(firstDateWhichSatisfiesPatienTResourceConstraint);

                var consultation = new ConsultationRecord(patient, (TreatmentRoom)availableRoom,
                    (Doctor)availableDoctor, request.Item1.DateRegistered, firstDateWhichSatisfiesPatienTResourceConstraint);

                Task.Factory.StartNew(() =>
                    request.Item2(new ResourceAllocatorResult(true, consultation)));
            }
        }

              
        public void Dispose()
        {
            if (_allocateResourceTask != null)
            {
                Cancel.Cancel();
                _allocateResourceTask.Wait();
                _allocateResourceTask = null;
            }                
        }

    }
}

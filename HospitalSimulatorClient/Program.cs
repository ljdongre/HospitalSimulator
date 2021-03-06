﻿using HospitalSimulatorService.Contract.Data;
using HospitalSimulatorService.Contract.Proxy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalSimulatorClient
{
    class Program
    {

        static Dictionary<int, Tuple<string, Action>> _commandMap = new Dictionary<int, Tuple<string, Action>>
        {
            {1, new Tuple<string, Action>("Register Patient", RegisterPatientRequest)},
            {2, new Tuple<string, Action>("Patient Registered", IsRegistrationSuccessful)},
            {3, new Tuple<string, Action>("Get Registered Patients", GetRegisteredPatients)},
            {4, new Tuple<string, Action>("Get Scheduled Consultations", GetScheduledConsultations)},
            {5, new Tuple<string, Action>("Stress Test", StressTest)},
            {99, new Tuple<string, Action>("Exit", () => { }) }
        };

        static void GetScheduledConsultations()
        {
            GetScheduledConsultations(null);
        }

        static void GetScheduledConsultations(StreamWriter logFile)
        {
            HospitalSimulatorProxy proxy = null;
            try
            {
                proxy = new HospitalSimulatorProxy();
                var consulations = proxy.ScheduledConsultations();
                WriteLine("Consulations ...", logFile);
                foreach (var c in consulations)
                {
                    WriteLine(string.Format("Scheduled {0}: with '{1}' at '{2}' on '{3}'",
                        c.Patient, c.Doctor, c.TreatmentRoom, c.ConsulationDate), logFile);
                }
                WriteLine(string.Empty, logFile);

            }
            finally
            {
                try
                {
                    proxy.Close();
                }
                catch (Exception e)
                {
                    WriteLine(string.Format("Erroe [{0}] Executing 'GetScheduledConsultations'", e), logFile);
                    proxy.Abort();
                }
            }
        }

        static void GetRegisteredPatients()
        {
            GetRegisteredPatients(null);
        }
        static void GetRegisteredPatients(StreamWriter logFile)
        {
            HospitalSimulatorProxy proxy = null;
            try
            {
                proxy = new HospitalSimulatorProxy();
                var patients = proxy.RegisteredPatients();
                WriteLine("Registered Patient List...", logFile);

                foreach (var p in patients)
                {
                    WriteLine(string.Format("{0}", p), logFile);
                }
                Console.WriteLine();
            }
            finally
            {
                try
                {
                    proxy.Close();
                }
                catch (Exception e)
                {
                    WriteLine(string.Format("Erroe [{0}] Executing 'GetRegisteredPatients'", e), logFile);
                    proxy.Abort();
                }
            }
        }

        static void RegisterPatient(Patient p)
        {
            HospitalSimulatorProxy proxy = null;
            try
            {
                proxy = new HospitalSimulatorProxy();
                proxy.RegisterPatient(p);
            }
            finally
            {
                try
                {
                    proxy.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error [{0}] Executing 'RegisterPatient'", e);
                    proxy.Abort();
                }
            }
        }

        private static void RegisterPatientRequest()
        {
            RegisterPatient(GetPatientInfo());
        }

        static void IsRegistrationSuccessful()
        {
            var result = IsRegistrationSuccessful(GetPatientName());
            if (result.Item1)
            {
                Console.WriteLine("Registered '{0}': with '{1}' at '{2}' on '{3}'",
                    result.Item2.Patient, result.Item2.Doctor, result.Item2.TreatmentRoom, result.Item2.ConsulationDate.ToShortDateString());
            }
        }
        static Tuple<bool, Consultation> IsRegistrationSuccessful(string patientName)
        {
            HospitalSimulatorProxy proxy = null;
            try
            {
                proxy = new HospitalSimulatorProxy();
                return proxy.IsRegistrationSuccessful(patientName);
 
            }
            finally
            {
                try
                {
                    proxy.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error [{0}] Executing 'RegisterPatientRequest'", e);
                    proxy.Abort();
                }
            }
        }

        const int noOfTasks = 10;
        const int noOfOperations = 20;

        private static SemaphoreSlim ss = new SemaphoreSlim(noOfTasks);
        private static List<Task> tasks = new List<Task>();
        private static ConcurrentDictionary<string, string> patientNames = 
            new ConcurrentDictionary<string, string>();

        private static void RunTask(int id, StreamWriter logFile, CancellationToken cancelToken)
        {
            ss.Release();
            WriteLine(string.Format("Starting task ID: {0}", id), logFile);
            Thread.Sleep(1000);

            var optionRandom = new Random();
            var conditionRandom = new Random();
            var topographyRandom = new Random();
            var nameRandom = new Random();

            for (int j = 1; j < noOfOperations; j++)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    WriteLine(string.Format("Task [{0}] cancelled", id), logFile);
                    break;
                }
                var option = optionRandom.Next(1, 5);
                switch (option)
                {
                    case 1:
                        var cr = conditionRandom.Next(1, 19) % conditionRandom.Next(1, 19);
                        PatientCondition pc = PatientCondition.Flu;
                        if (cr % 2 == 0)
                            pc = PatientCondition.Cancer;

                        ConditionTopography ct = ConditionTopography.None;
                        
                        if (pc == PatientCondition.Cancer)
                        {
                            var t = topographyRandom.Next(1, 29) % topographyRandom.Next(1, 29);
                            ct = ConditionTopography.HeadAndNeck;
                            if (t % 3 == 0)
                                ct = ConditionTopography.Breast;
                        }
  
                        var p = new Patient(string.Format("Test_{0} Patient_{1}",
                            id * j, id * j + 1), pc, ct);
                        patientNames.TryAdd(p.Name, p.Name);
                        Program.RegisterPatient(p);
                        WriteLine(string.Format("Initiated Registration for Patient '{0}'", p), logFile);
                        break;
                    case 2:
                        var names = patientNames.ToList();
                        if (!names.Any())
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        var nameIndex = nameRandom.Next(0, names.Count);

                        var result = Program.IsRegistrationSuccessful(names[nameIndex].Key);
                        if (!result.Item1)
                        {
                            WriteLine(string.Format("Failed to Register patient: '{0}' will check later",
                                   names[nameIndex].Key), logFile);
                        }
                        
                        break;
                    case 3:
                        Program.GetRegisteredPatients(logFile);
                        break;
                    case 4:
                        Program.GetScheduledConsultations(logFile);
                        break;
                    default:
                        break;
                }
            }
            WriteLine(string.Format("Task [{0}] End", id), logFile);
        }

        private static void StressTest()
        {
            var cancelToken = new CancellationTokenSource();

            const string logFileName = "StressTestLog.txt";
            const string logFileSearchPattern = "StressTestLog_";

            if (File.Exists(logFileName))
            {
                var oldLogFiles = Directory.GetFiles(".", logFileSearchPattern + "*", SearchOption.TopDirectoryOnly)
                    .Select(f => int.Parse(Path.GetFileNameWithoutExtension(f).Split('_')[1])).ToList();

                var currentFileIndex = oldLogFiles.Any() ? oldLogFiles.Max() + 1 : 1;

                File.Move(logFileName, logFileSearchPattern + currentFileIndex + ".txt");
            }

            var logFile = new StreamWriter(logFileName);

            for(int i =1; i < noOfTasks; i++)
            {
                int k = i;
                tasks.Add(Task.Factory.StartNew(() => RunTask(k, logFile, cancelToken.Token), cancelToken.Token));
            }

            ss.Wait();
            WriteLine("Waiting for all tasks to end ...", logFile);
            Task.WaitAll(tasks.ToArray());
            WriteLine("All tasks have finished .. See the result of registration requests...", logFile);
            var names = patientNames.ToList();
            foreach(var name in names)
            {
                var result = Program.IsRegistrationSuccessful(name.Key);
                if (result.Item1)
                    WriteLine(string.Format("Patient successfully Registered: '{0}'",
                                   name.Key), logFile);
                else 
                    WriteLine(string.Format("Failed to Register patient: '{0}', seems like registration takes time",
                                   name.Key), logFile);
            }
            WriteLine("------------------Aggregate Consultations For the Run (Start)---------------------", logFile);
            Program.GetScheduledConsultations(logFile);
            WriteLine("------------------Aggregate Consultations For the Run (End)---------------------", logFile);
            logFile.Flush();
            logFile.Close();
        }


        private static void Main()
        {
            if (!Process.GetProcessesByName("HospitalSimulator.Host").Any())
            {
                Console.WriteLine("server process 'HospitalSimulator.Host.exe' is not running. Do start the same, before starting the client");
                Environment.Exit(-1);
            }

            var exit = false;

            while (!exit)
            {
                int option = DisplayUsage();
                if (option == 99)
                {
                    exit = true;
                }
                _commandMap[option].Item2();
            }
        }

        private static int DisplayUsage()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;

            var keys = _commandMap.Keys.ToList() ;
            keys.Sort();
            foreach (var key in keys)
            {
                Console.WriteLine("{0}: {1}", key, _commandMap[key].Item1);
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            while (true)
            {
                WriteLine(" Select Option: <hit Enter>");
                var input = Console.ReadLine();
                int option;
                if (!Int32.TryParse(input, out option))
                {
                    WriteLine("only digits is considered a valid input");
                    continue;
                }
                if (option > 0 && _commandMap.ContainsKey(option)) return option;
            }
        }

        private static Patient GetPatientInfo()
        {
            
            Console.Write("Patient Name: ");
            var patientName = Console.ReadLine();
            WriteLine(string.Empty);

            var pc = GetPatientCondition();
            var ct = ConditionTopography.None;

            if (pc == PatientCondition.Cancer)
            {
                ct = GetCancerTopography();
            }

            return new Patient(patientName, pc, ct);
        }

        private static string  GetPatientName()
        {

            Console.Write("Query For Registration, Enter Patient Name: ");
            var patientName = Console.ReadLine();
            WriteLine(string.Empty);

            return patientName;
        }

        static PatientCondition GetPatientCondition()
        {
            Console.Write("Condition: Flu (1), Cancer (2)");
            int conditionId;
            while (!int.TryParse(Console.ReadLine(), out conditionId) &&
                (conditionId < 1 || conditionId > 2))
            {
                WriteLine(string.Empty);
            }

            PatientCondition pc;
            switch (conditionId)
            {
                case 1:
                    pc = PatientCondition.Flu;
                    break;
                case 2:
                    pc = PatientCondition.Cancer;
                    break;
                default:
                    pc = PatientCondition.Flu;
                    break;
            }
            return pc;
        }

        static ConditionTopography GetCancerTopography()
        {
            Console.Write("Topography: Head&Neck (1), Breast (2)");
            int id;
            while (!int.TryParse(Console.ReadLine(), out id) &&
                (id < 1 || id > 2))
            {
                WriteLine(string.Empty);
            }

            ConditionTopography ct;
            switch (id)
            {
                case 1:
                    ct = ConditionTopography.HeadAndNeck;
                    break;
                case 2:
                    ct = ConditionTopography.Breast;
                    break;
                default:
                    ct = ConditionTopography.None;
                    break;
            }
            return ct;
        }

        static object consoleLock = new object();
        static void WriteLine(string msg, StreamWriter logstream = null)
        {
            lock (consoleLock)
            {
                Console.WriteLine(msg);
                if (logstream != null) logstream.WriteLine(msg);
            }
        }
    }
}

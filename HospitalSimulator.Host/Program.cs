using System;
using System.Reflection;
using System.Threading.Tasks;
using Topshelf;

namespace HospitalSimulator.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var method = MethodBase.GetCurrentMethod().Name;
            try
            {
                var h = HostFactory.New(x =>
                {
                    x.Service<ApplicationHost>(s =>
                    {
                        s.ConstructUsing(name => new ApplicationHost());
                        s.WhenStarted(tc =>
                        {

                            tc.Start();
                        });

                        s.AfterStartingService(VerifyWcfService);

                        s.WhenStopped(tc =>
                        {
                            tc.Stop();
                        });


                    });

                    x.RunAsLocalSystem();

                    x.SetDescription("Hospital Simulator service");
                    x.SetDisplayName("Hospital Simulator service");
                    x.SetServiceName("HospitalSimulator");

                    x.EnableServiceRecovery(r =>
                    {
                        SimpleLogger.SimpleLogger.Instance.Log(SimpleLoggerContract.LogLevel.Error, "Topshelf Restarting");
                        r.RestartService(0);
                    }
                     );
                });

                h.Run();

            }
            catch (Exception exp)
            {
                SimpleLogger.SimpleLogger.Instance.Log(SimpleLoggerContract.LogLevel.Error, string.Format("Topshelf hosting Exception: {0}", exp));
                throw;
            }
        }

        private static void VerifyWcfService()
        {
            var method = MethodBase.GetCurrentMethod().Name;

            var task = Task.Factory.StartNew(() =>
            {
                var proxy = new HospitalSimulatorService.Contract.Proxy.HospitalSimulatorProxy();
                var msg = string.Format("WcfService HospitalSimulator Proxy Status: {0}", proxy.IsAlive());

                Console.WriteLine(msg);
                SimpleLogger.SimpleLogger.Instance.Log(SimpleLoggerContract.LogLevel.Info,  msg);
            });
            task.ContinueWith((t) =>
            {
                SimpleLogger.SimpleLogger.Instance.Log(SimpleLoggerContract.LogLevel.Error, "HospitalSimulator verification complete");
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            task.ContinueWith((t) =>
            {
                SimpleLogger.SimpleLogger.Instance.Log(SimpleLoggerContract.LogLevel.Error, string.Format("Unable to verify the Hospital Simulator Service: {0}",
                    t.Exception));
            }, TaskContinuationOptions.OnlyOnFaulted);

        }
    }

}

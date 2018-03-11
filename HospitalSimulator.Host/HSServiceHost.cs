using HospitalSimulatorService;
using System.Reflection;
using System.ServiceModel;

namespace HospitalSimulator.Host
{
    /// <summary>
    /// The class for hosting the WCF service
    /// </summary>
    internal class HSServiceHost : ServiceHost
    {
               
        public HSServiceHost() : base(typeof(HSService))
        {
        }

        public bool Start()
        {
            var method = MethodBase.GetCurrentMethod().Name;
           
            Open();
            foreach (var address in this.BaseAddresses)
            {
                SimpleLogger.SimpleLogger.Instance.Log(SimpleLoggerContract.LogLevel.Info, string.Format("Hosting address :{0}", address));
            }
            return true;
        }

        public bool Stop()
        {
            Close();
            return true;
        }
    }
}

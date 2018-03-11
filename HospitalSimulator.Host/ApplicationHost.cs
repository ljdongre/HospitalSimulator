
namespace HospitalSimulator.Host
{
    internal class ApplicationHost
    {
        private readonly HSServiceHost _hospitalSimulatorHost;

        public ApplicationHost()
        {
            _hospitalSimulatorHost = new HSServiceHost();
        }

        public bool Start()
        {
            return _hospitalSimulatorHost.Start();
        }

        public bool Stop()
        {
           return _hospitalSimulatorHost.Stop();
        }
    }
}
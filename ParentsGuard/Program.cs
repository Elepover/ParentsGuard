using ParentsGuard.Services;
using System.ServiceProcess;

namespace ParentsGuard
{
    static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new BlockingService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}

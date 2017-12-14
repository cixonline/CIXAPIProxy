using System.ServiceProcess;

namespace CIXService {

    static class Program {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main() {

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new CIXService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}

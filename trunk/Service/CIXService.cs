using CIXAPIProxy;
using System.ServiceProcess;

namespace CIXService {

    public partial class CIXService : ServiceBase {

        CoSyServer _server;

        public CIXService() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            _server = new CoSyServer(23);
            _server.Start();
        }

        protected override void OnStop() {
            if (_server != null) {
                _server.Stop();
            }
        }
    }
}

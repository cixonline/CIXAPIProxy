using CIXAPIProxy;

namespace Console {

    public class Program {

        static void Main(string[] args) {

            CoSyServer _server;
            _server = new CoSyServer(23);
            _server.Start();
        }
    }
}

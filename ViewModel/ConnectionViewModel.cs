using PettingZoo.Model;

namespace PettingZoo.ViewModel
{
    public class ConnectionViewModel
    {
        public ConnectionInfo ConnectionInfo { get; set; }
        public string Exchange { get; set; }
        public string RoutingKey { get; set;  }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectivePlatformApp.Models
{
    public class Offers
    {
        public int Id { get; set; }
        public int Price { get; set; }

        public int ClientId { get; set; }
        public Clients Client { get; set; } = null!;

        public int AgentId { get; set; }
        public Agents Agent { get; set; } = null!;

        public Deal? Deal { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectivePlatformApp.Models
{
    public class Agents
    {

        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string MiddleName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public int Commision { get; set; }

        public ICollection<Demands> Demands { get; set; } = new List<Demands>();
        public ICollection<Offers> Offers { get; set; } = new List<Offers>();
    }
}

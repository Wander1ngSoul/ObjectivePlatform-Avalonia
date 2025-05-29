using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectivePlatformApp.Models
{
    public class Districts
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Area { get; set; }

        public ICollection<RealEstates> RealEstates { get; set; } = new List<RealEstates>();
        public ICollection<Demands> Demands { get; set; } = new List<Demands>();
    }

}

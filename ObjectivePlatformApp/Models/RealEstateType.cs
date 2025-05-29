using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectivePlatformApp.Models
{
    public class RealEstateType
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<RealEstates> RealEstates { get; set; } = new List<RealEstates>();
        public ICollection<Demands> Demands { get; set; } = new List<Demands>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectivePlatformApp.Models
{
    public class HouseDemands
    {
        public int Id { get; set; }
        public int DemandId { get; set; }
        public Demands Demand { get; set; } = null!;

        public double MinArea { get; set; }
        public double MaxArea { get; set; }
        public int MinRoomsCount { get; set; }
        public int MaxRoomsCount { get; set; }
        public int MinFloorCount { get; set; }
        public int MaxFloorCount { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectivePlatformApp.Models
{
    public class Demands
    {
        public int Id { get; set; }
        public int MinPrice { get; set; }
        public int MaxPrice { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? House { get; set; }
        public string? Flat { get; set; }

        public int? DistrictId { get; set; }
        public Districts? District { get; set; }

        public int ClientId { get; set; }
        public Clients Client { get; set; } = null!;

        public int AgentId { get; set; }
        public Agents Agent { get; set; } = null!;

        public int RealEstateTypeId { get; set; }
        public RealEstateType RealEstateType { get; set; } = null!;

        public HouseDemands? HouseDemands { get; set; }
        public FlatDemands? FlatDemands { get; set; }
        public LandDemands? LandDemands { get; set; }
        public Deal? Deal { get; set; }
    }

}

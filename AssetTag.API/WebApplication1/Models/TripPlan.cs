using System;
using System.Collections.Generic;

namespace ForEveryAdventure.Models
{
    public class TripPlan
    {
        public Guid TripIdentifier { get; set; }
        public string? TripRoutePreference { get; set; }
        public string? TripRoute { get; set; }
        public DateTime TripStartDate { get; set; }
        public DateTime TripEndDate { get; set; }
        public int TripDurationDays { get; set; }
        public List<ForEveryAdventure.Models.LocationCoordinates> TripLocationStart { get; set; } = new();
        //public List<LocationCoordinates> TripLocationStart { get; set; } = new();
        public List<ForEveryAdventure.Models.LocationCoordinates> TripLocationEnd { get; set; } = new();
        //public List<LocationCoordinates> TripLocationEnd { get; set; } = new();
        public List<ForEveryAdventure.Models.LocationCoordinates> TripFeaturedLocation { get; set; } = new();
        //public List<LocationCoordinates> TripFeaturedLocation { get; set; } = new();
    }
}
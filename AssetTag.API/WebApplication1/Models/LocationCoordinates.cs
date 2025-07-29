using System;
using System.Collections.Generic;

namespace ForEveryAdventure.Models
{
    public class LocationCoordinates
    {
        public Guid LocationIdentifier { get; set; }
        public string? LocationName { get; set; }
        public string? LocationGPSformat01 { get; set; }
        public string? LocationGPSformat02 { get; set; }
        public string? LocationWhatThreeWords { get; set; }
        public string? LocationAppleMap { get; set; }
        public string? LocationGoogleMap { get; set; }
        public string? LocationAddressCriteria { get; set; }
    }
}
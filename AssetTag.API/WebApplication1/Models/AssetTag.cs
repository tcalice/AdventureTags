using System;
using System.Collections.Generic;

namespace ForEveryAdventure.Models
{
    public class AssetTag
    {
        public Guid Id { get; set; }
        public string? TagCode { get; set; }
        public Guid UserId { get; set; }
        public List<EmergencyContact> EmergencyContacts { get; set; } = new();
        public List<TripPlan> TripPlans { get; set; } = new();

    }
}
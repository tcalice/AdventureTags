using ForEveryAdventure.Models;
using System;

namespace ForEveryAdventure.Services
{
    public static class ForAdventureLogic
    {
        public static string generateTripPlanNarrative(TripPlan tripPlan)
        {
            if (tripPlan == null) return "No trip plan provided.";

            var duration = (tripPlan.TripEndDate - tripPlan.TripStartDate).Days;
            var notes = !string.IsNullOrWhiteSpace(tripPlan.TripRoutePreference) ? $"Notes: {tripPlan.TripRoutePreference}" : "No additional notes provided.";

            return $"Trip planned to {tripPlan.TripLocationStart} from {tripPlan.TripStartDate:MMMM dd, yyyy} to {tripPlan.TripEndDate:MMMM dd, yyyy}. " +
                   $"Duration: {duration} days. {notes}";
        }
    }
}
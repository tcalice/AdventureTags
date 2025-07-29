using ForEveryAdventure.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;

namespace ForEveryAdventure.Services
{
    public class AdventureAPIService
    {
        private readonly HttpClient _client = new HttpClient();
        private const string BaseUrl = "http://localhost:5034/api";

        // Fixes CS0721 and IDE0060: Remove JsonSerializer parameter, use static JsonSerializer directly
        public async Task<AssetTag> CreateAssetTagAsync(Guid userId)
        {
            var content = new StringContent(JsonSerializer.Serialize(userId), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{BaseUrl}/MakeAssetTag", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var assetTag = JsonSerializer.Deserialize<AssetTag>(json);
            if (assetTag is null)
                throw new InvalidOperationException("Failed to deserialize AssetTag from API response.");
            return assetTag;
        }

        public async Task<TripPlan> AddTripPlanAsync(TripPlan plan)
        {
            var content = new StringContent(JsonSerializer.Serialize(plan), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{BaseUrl}/AddPlanLogistics", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TripPlan>(json);
        }

        public async Task<AssetTag> GetAssetTagAsync(string tagCode)
        {
            var response = await _client.GetAsync($"{BaseUrl}/ShareAssetTag/{tagCode}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var assetTag = JsonSerializer.Deserialize<AssetTag>(json);
            if (assetTag is null)
                throw new InvalidOperationException("Failed to deserialize AssetTag from API response.");
            return assetTag;
        }

        public async Task<string> SendEmergencyAlertAsync(string tagCode)
        {
            var response = await _client.PostAsync($"{BaseUrl}/Emergency/{tagCode}/alert", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }


public static class AssetTagEndpoints
{
	public static void MapAssetTagEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/AssetTag").WithTags(nameof(AssetTag));

        group.MapGet("/", () =>
        {
            return new [] { new AssetTag() };
        })
        .WithName("GetAllAssetTags")
        .WithOpenApi();

        group.MapGet("/{id}", (int id) =>
        {
            //return new AssetTag { ID = id };
        })
        .WithName("GetAssetTagById")
        .WithOpenApi();

        group.MapPut("/{id}", (int id, AssetTag input) =>
        {
            return TypedResults.NoContent();
        })
        .WithName("UpdateAssetTag")
        .WithOpenApi();

        group.MapPost("/", (AssetTag model) =>
        {
            //return TypedResults.Created($"/api/AssetTags/{model.ID}", model);
        })
        .WithName("CreateAssetTag")
        .WithOpenApi();

        group.MapDelete("/{id}", (int id) =>
        {
            //return TypedResults.Ok(new AssetTag { ID = id });
        })
        .WithName("DeleteAssetTag")
        .WithOpenApi();
    }
}

public static class TripPlanEndpoints
{
	public static void MapTripPlanEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/TripPlan").WithTags(nameof(TripPlan));

        group.MapGet("/", () =>
        {
            return new [] { new TripPlan() };
        })
        .WithName("GetAllTripPlans")
        .WithOpenApi();

        group.MapGet("/{id}", (int id) =>
        {
            //return new TripPlan { ID = id };
        })
        .WithName("GetTripPlanById")
        .WithOpenApi();

        group.MapPut("/{id}", (int id, TripPlan input) =>
        {
            return TypedResults.NoContent();
        })
        .WithName("UpdateTripPlan")
        .WithOpenApi();

        group.MapPost("/", (TripPlan model) =>
        {
            //return TypedResults.Created($"/api/TripPlans/{model.ID}", model);
        })
        .WithName("CreateTripPlan")
        .WithOpenApi();

        group.MapDelete("/{id}", (int id) =>
        {
            //return TypedResults.Ok(new TripPlan { ID = id });
        })
        .WithName("DeleteTripPlan")
        .WithOpenApi();
    }
}}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ForEveryAdventure.Models;

namespace ForEveryAdventure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetTagController : ControllerBase
    {
        private readonly IAssetTagStore _store;
        private static readonly List<AssetTag> _assetTags = [];

        public ILogger<AssetTagController> Logger { get; }

        public AssetTagController(IAssetTagStore store, ILogger<AssetTagController> logger)
        {
            _store = store;
            Logger = logger;
        }

        [HttpPost("MakeAssetTag")]
        public IActionResult MakeAssetTag([FromBody] AssetTag assetTag)
        {
            var newTag = new AssetTag
            {
                Id = Guid.NewGuid(),
                TagCode = assetTag.TagCode,
                UserId = assetTag.UserId,
                EmergencyContacts = assetTag.EmergencyContacts,
                TripPlans = assetTag.TripPlans
            };

            if (_store.AssetTags == null)
                throw new InvalidOperationException("Asset tag store is not initialized.");
            _store.AssetTags.Add(newTag);

            var response = new
            {
                Message = "Retrieve your Asset Sticker with this Unique Asset Tag ID",
                AssetTagId = newTag.Id
            };
            return Ok(response);
        }
        // Controller actions go here
    }
}

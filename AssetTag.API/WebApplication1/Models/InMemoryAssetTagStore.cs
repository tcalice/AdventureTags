
namespace ForEveryAdventure.Models
{
    public class InMemoryAssetTagStore : IAssetTagStore
    {
        public List<AssetTag> AssetTags { get; set; }
        public InMemoryAssetTagStore()
        {
            AssetTags = new List<AssetTag>();
        }
    }
}

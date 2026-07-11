using CommunityFoodCharityInventory.API.DTOs;

namespace CommunityFoodCharityInventory.API.Hubs
{
    public interface IDonorHubClient
    {
        Task ReceiveInventoryUpdate(InventoryItemDto update);
    }
    public class DonorHub
    {
        //No Groups required because clients just subscribe to all web sockets broadcast
    }
}

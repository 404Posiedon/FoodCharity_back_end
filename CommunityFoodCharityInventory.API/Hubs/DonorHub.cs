using CommunityFoodCharityInventory.API.DTOs;
using Microsoft.AspNetCore.SignalR;
namespace CommunityFoodCharityInventory.API.Hubs
{
    public interface IDonorHubClient
    {
        Task ReceiveInventoryUpdate(InventoryItemDto update);
        Task ReceiveInventoryDelete(Guid id);
    }
    public class DonorHub :Hub<IDonorHubClient>
    {
        //No Groups required because clients just subscribe to all web sockets broadcast
    }
}

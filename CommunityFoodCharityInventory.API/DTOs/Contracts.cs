namespace CommunityFoodCharityInventory.API.DTOs;

public record InventoryItemDto(Guid Id, string Name, double EffectiveQuantity, string Status);
public record PledgeRequestDto(double Quantity);
public record DeductRequestDto(double Quantity);
public record RestockRequestDto(double Quantity);
public record CreateItemRequestDto(string Name, double CurrentQuantity, double TargetCap, double MinThreshold, double CritThreshold);

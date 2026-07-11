namespace CommunityFoodCharityInventory.API.DTOs;

public record InventoryItemDto(Guid Id, string Name, double EffectiveQuantity, string Status);
public record PledgeRequestDto(double Quality);
public record DeductRequestDto(double Quality);
public record RestockRequestDto(double Quality);
public record CreateItemRequestDto(string Name, double CurrentQuantity, double TargetCap, double MinThreshold, double CritThreshold);

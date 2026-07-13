namespace CommunityFoodCharityInventory.API.DTOs;

public record InventoryItemRequestDto(Guid Id, string Name, double EffectiveQuantity, double MinimumThreshol,double MaximumThreshold,double TargetCap,string Status);
public record InventoryItemDto(Guid Id, string Name, double MinimumThreshold, double MaximumThreshold, double CurrentQuantity, double EffectiveQuantity, double TargetCap, string Status);
public record PledgeRequestDto(double Quantity);
public record DeductRequestDto(double Quantity);
public record RestockRequestDto(double Quantity);
public record CreateItemRequestDto(string Name, double CurrentQuantity, double TargetCap, double MinThreshold,double MaxThreshold, double CritThreshold);
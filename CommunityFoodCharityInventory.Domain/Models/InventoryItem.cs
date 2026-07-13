using CommunityFoodCharityInventory.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CommunityFoodCharityInventory.Domain.Models
{
   public class InventoryItem
    {
        public Guid Id { get;private set; }
        public string Name { get; private set; }
        public double CurrentQuantity { get; private set; }
        public double PledgedQuantity { get; private set; }
        public double TargetCap { get; private set; }
        public double MinimumThreshold { get; private set; }
        public double MaximumThreshold { get; private set; }
        public double CriticalThreshold { get; private set; }

        //Concurrency Timestamp
        [Timestamp]
        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

        //Computed fields
        public double  EffectiveQuantity => CurrentQuantity + PledgedQuantity;
        public UrgencyLevel Status => EffectiveQuantity switch
        {
            var quantity when quantity >= TargetCap => UrgencyLevel.Stable,
            var quantity when quantity <= CriticalThreshold => UrgencyLevel.Critical,
            _ => UrgencyLevel.LowStock
        };

        //Constructors
        public InventoryItem()
        {
            
        }
        public InventoryItem(Guid id, string name, double currentQuantity, double targetCap, double minThreshold,double maxThreshold, double critThreshold)
        {
            Id = id;
            Name = name;
            CurrentQuantity = currentQuantity;
            TargetCap = targetCap;
            MaximumThreshold = maxThreshold; 
            MinimumThreshold = minThreshold;
            CriticalThreshold = critThreshold;
        }

        //State Management Methods
        public void AdjustQuantity(double quantity)
        {
            if (CurrentQuantity + quantity < 0)
                throw new InvalidOperationException("Cannot reduce inventory quantity below zero.");
            CurrentQuantity += quantity;
        }
        public bool TryAddPledge(double quantity)
        {
            if(EffectiveQuantity + quantity > TargetCap)
                return false;
            PledgedQuantity += quantity;
            return true;
        }
        public void ReleasePledge(double quantity)
        {
            PledgedQuantity = Math.Max(0, PledgedQuantity - quantity);
        }

    }
}

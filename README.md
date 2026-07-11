# Food Charity Pulse 

Food Charity Pulse is a reactive, real-time inventory optimization engine designed for community soup kitchens and food charity networks. It tracks current physical stock along with upcoming donor drop-off commitments (pledges), allowing administrators to see actual vs. anticipated stock availability without over-allocating warehouse storage capacities.

## 1. Architectural Overview
   
The system balances data consistency with immediate user-interface reactivity using a decoupled, event-driven design.

* **Domain-Driven Foundations:** Business logic rules are tightly contained inside the `InventoryItem` rich domain entity to avoid a weak domain design.
* **Persistent Infrastructure:** Relational mappings and concurrent transaction management are handled via Entity Framework Core over Microsoft SQL Server.
* **Real-time Synchronization:** Outbound notifications use a strongly typed ASP.NET Core SignalR hub to stream inventory changes to all active consumer dashboards instantly.

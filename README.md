# Food Charity Pulse 

Food Charity Pulse is a reactive, real-time inventory optimization engine designed for community soup kitchens and food charity networks. It tracks current physical stock along with upcoming donor drop-off commitments (pledges), allowing administrators to see actual vs. anticipated stock availability without over-allocating warehouse storage capacities.

## 1. Architectural Overview
   
The system balances data consistency with immediate user-interface reactivity using a decoupled, event-driven design.

* **Domain-Driven Foundations:** Business logic rules are tightly contained inside the `InventoryItem` rich domain entity to avoid a weak domain design.
* **Persistent Infrastructure:** Relational mappings and concurrent transaction management are handled via Entity Framework Core over Microsoft SQL Server.
* **Real-time Synchronization:** Outbound notifications use a strongly typed ASP.NET Core SignalR hub to stream inventory changes to all active consumer dashboards instantly.

## 2. Domain Layer & Core Business Rules

### `UrgencyLevel` (Enum)
Defines the immediate operational priority of a given food resource category.

* **`Stable`**: The item resource meets or exceeds storage baseline capacity targets.
* **`LowStock`**: The resource is running below ideal levels but is not yet critical.
* **`Critical`**: Stock levels have hit dangerous minimum distributions requiring immediate attention.

### `InventoryItem` (Aggregate Root)
Contains the core fields, state formulas, and state mutation methods.

#### Key Metrics Logic

* **Effective Quantity:** The sum of what is physically on hand plus what has been promised by off-site donors.
  ```text
  EffectiveQuantity = CurrentQuantity + PledgedQuantity

## 3. Communication & Data Contracts (DTOs)

The system uses lightweight record contracts to pass data cleanly through network boundaries:

| DTO Contract Name | Fields | Purpose |
| :--- | :--- | :--- |
| `InventoryItemDto` | `Guid Id`, `string Name`, `double EffectiveQuantity`, `string Status` | Flattens state information for public consumption. |
| `PledgeRequestDto` | `double Amount` | Encapsulates requested donor pledge adjustments. |
| `DeductRequestDto` | `double Amount` | Passes requested stock consumption values from the kitchen. |
| `RestockRequestDto` | `double Amount` | Captures explicit unpledged warehouse restock counts. |
| `CreateItemRequestDto` | `string Name`, `double CurrentQuantity`, `double TargetCap`, `double MinThreshold`, `double CritThreshold` | Schema template used for administrative item creations. |

---

## 4. RESTful API Reference

All write operations update the underlying database engine and broadcast the updated `InventoryItemDto` message package automatically across the active SignalR channel.

### `GET /api/inventory`
* **Summary:** Retrieves a complete array snapshot of all current inventory entities.
* **Response:** `200 OK` with a `List<InventoryItemDto>` payload.

### `POST /api/inventory`
* **Summary:** Provisions a brand-new trackable resource group type in the platform.
* **Input Body:** `CreateItemRequestDto` JSON object.
* **Response:** `201 Created` with the newly assigned entity ID.

### `POST /api/inventory/{id}/pledge`
* **Summary:** Registers a donor commitment to drop off items.
* **Input Body:** `PledgeRequestDto`
* **Responses:**
  * `200 OK`: Pledge successfully registered.
  * `400 BadRequest`: Target capacity limits reached.
  * `449 Conflict`: `DbUpdateConcurrencyException` occurred (retry recommended).

### `POST /api/inventory/{id}/fulfill-pledge`
* **Summary:** Processes a pledge arriving physically at the facility kitchen.
* **Logic:** Decrements `PledgedQuantity` buffer and increments `CurrentQuantity` by the matching volume.
* **Input Body:** `PledgeRequestDto`

### `POST /api/inventory/{id}/cancel-pledge`
* **Summary:** Releases a donor pledge allocation if a commitment falls through.
* **Logic:** Subtracts value from `PledgedQuantity` safely using a lower limit of zero.

### `POST /api/inventory/{id}/restock`
* **Summary:** Direct physical replenishment (e.g., direct purchases).
* **Input Body:** `RestockRequestDto`

### `POST /api/inventory/{id}/deduct`
* **Summary:** Deducts inventory items as they are consumed by kitchen operations.
* **Input Body:** `DeductRequestDto`

---

## 5. Real-Time Streaming Subsystem (DonorHub)

The application exposes a lightweight, push-based messaging pipeline running over WebSockets using ASP.NET Core SignalR.

* **Hub Route:** `/donorHub`
* **Outbound Interface Protocol (`IDonorHubClient`):** `Task ReceiveInventoryUpdate(InventoryItemDto update)`
* **Subscription Model:** Subscription uses a global fan-out broadcast pattern. Clients do not sort themselves into granular rooms or groups; any state modification executed anywhere across the REST API pipeline streams an isolated payload update to all connected clients, allowing client UIs to re-render in real-time.
  

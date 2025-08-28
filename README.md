# AmbientTransaction

A .NET library that provides ambient transaction scoping for database operations, enabling automatic transaction management across multiple database calls without explicit transaction passing.

## What is Ambient Transaction For?

Ambient transactions solve the common problem of managing database transactions across multiple methods and classes without explicitly passing transaction objects around. This library provides:

- **Automatic Transaction Management**: Transactions are automatically created and shared across all database operations within a scope
- **Nested Scope Support**: Inner scopes automatically join existing transactions
- **Lazy Connection Management**: Database connections are only opened when actually needed
- **Nested Scope Voting**: All nested scopes must complete successfully for the transaction to commit - actual commit/rollback happens when the root scope is disposed
- **Connection Wrapper**: Prevents accidental transaction misuse by intercepting direct transaction operations

## How to Use

### Basic Usage

```csharp
// Basic transaction scope with commit
await using (var scope = AmbientTransactionScope.Create(connectionString))
{
    var repository = new Repository(new DbConnectionFactory(connectionString));
    await repository.DoWork("data");
    
    scope.Complete(); // Mark for commit
} // Transaction commits here

// Basic transaction scope with rollback
await using (var scope = AmbientTransactionScope.Create(connectionString))
{
    var repository = new Repository(new DbConnectionFactory(connectionString));
    await repository.DoWork("data");
    
    // No scope.Complete() call - transaction rolls back
} // Transaction rolls back here
```

### Repository Implementation

```csharp
public class Repository
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    public Repository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    public async Task DoWork(string data)
    {
        // GetConnection automatically uses ambient transaction if available
        await using var connection = _connectionFactory.GetConnection(out var transaction);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.Transaction = transaction; // Use the ambient transaction
        command.CommandText = "INSERT INTO table_1 (data) VALUES (@data)";
        command.Parameters.Add(new SqlParameter("@data", data));
        
        await command.ExecuteNonQueryAsync();
    }
}
```

### Nested Scopes

```csharp
await using (var outerScope = AmbientTransactionScope.Create(connectionString))
{
    await repository.DoWork("outer");
    
    await using (var innerScope = AmbientTransactionScope.Create(connectionString))
    {
        await repository.DoWork("inner");
        innerScope.Complete(); // Inner scope must complete
    }
    
    outerScope.Complete(); // Outer scope must complete
} // Both operations commit together
```

### Mixed Usage (With and Without Ambient Transactions)

```csharp
// This works without ambient transaction
var repository = new Repository(new DbConnectionFactory(connectionString));
await repository.DoWork("standalone"); // Creates its own connection and commits immediately

// This works within ambient transaction
await using (var scope = AmbientTransactionScope.Create(connectionString))
{
    await repository.DoWork("transactional"); // Uses ambient transaction
    scope.Complete();
}
```

## Key Components

### AmbientTransactionScope
- Main entry point for creating transaction scopes
- Manages transaction lifecycle and voting
- Supports nested scopes with automatic joining
- Implements async disposal pattern

### DbConnectionFactory
- Creates database connections that automatically participate in ambient transactions
- Returns wrapped connections when ambient transaction exists
- Returns regular connections when no ambient transaction is active

### DbConnectionWrapper
- Prevents direct transaction operations on wrapped connections
- Automatically handles connection state for ambient transactions
- Throws exceptions if you try to manually start transactions, and ignores close/dispose operations

### ConnectionInformation
- Manages the actual database connection and transaction
- Implements lazy initialization - only opens connections when needed
- Handles proper disposal of connections and transactions

## What the Tests Demonstrate

The test suite (`UnitTestTake2.cs`) demonstrates several key scenarios:

### 1. **Transaction Commit Test** (`TestAmbientConnectionScopeDoMultipleWorkInTransactionCommit`)
- Creates ambient transaction scope
- Performs multiple database operations
- Calls `scope.Complete()` to vote for commit
- Verifies data was actually committed to database

### 2. **Transaction Rollback Test** (`TestAmbientConnectionScopeDoMultipleWorkInTransactionRollBack`)
- Creates ambient transaction scope
- Performs multiple database operations
- Does NOT call `scope.Complete()` 
- Verifies data was rolled back (not found in database)

### 3. **Connection String Validation** (`TestCnStringMismatchRaiseException`)
- Tests that using different connection strings in the same scope throws an exception
- Ensures transaction integrity by preventing mixed connections

### 4. **No Ambient Transaction Test** (`TestNoAmbientConnectionScopeDoMultipleWorkInTransactionCommit`)
- Demonstrates that repository code works without ambient transactions
- Each operation creates its own connection and commits immediately

### 5. **Nested Scope Test** (`TestAmbientConnectionScopeTwoLevelScopeDoSingleWorkInTransactionCommit`)
- Tests two-level nested scopes
- Both inner and outer scopes must call `Complete()` for commit
- Demonstrates transaction joining behavior

### 6. **Single Operation Tests**
- `TestAmbientConnectionScopeDoSingleWorkInTransactionCommit`: Single operation with commit
- `TestAmbientConnectionScopeDoSingleWorkRollBack`: Single operation with rollback

## Example Usage Scenarios

### Scenario 1: Service Layer Transaction

```csharp
public class OrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly IInventoryRepository _inventoryRepo;
    private readonly IDbConnectionFactory _connectionFactory;
    
    public async Task ProcessOrder(Order order)
    {
        await using var scope = AmbientTransactionScope.Create(_connectionString);
        
        // All these operations share the same transaction
        await _orderRepo.SaveOrder(order);
        await _inventoryRepo.UpdateStock(order.Items);
        await _orderRepo.UpdateOrderStatus(order.Id, OrderStatus.Processed);
        
        scope.Complete(); // Commit all operations together
    }
}
```

### Scenario 2: Nested Business Operations

```csharp
public class AccountService
{
    public async Task TransferFunds(int fromAccount, int toAccount, decimal amount)
    {
        await using var scope = AmbientTransactionScope.Create(_connectionString);
        
        await DebitAccount(fromAccount, amount);  // Nested scope
        await CreditAccount(toAccount, amount);   // Nested scope
        
        scope.Complete();
    }
    
    private async Task DebitAccount(int accountId, decimal amount)
    {
        await using var scope = AmbientTransactionScope.Create(_connectionString);
        // This automatically joins the parent transaction
        
        await _accountRepo.UpdateBalance(accountId, -amount);
        await _auditRepo.LogTransaction(accountId, TransactionType.Debit, amount);
        
        scope.Complete();
    }
}
```

### Scenario 3: Repository with Optional Transactions

```csharp
public class UserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    // This method works both with and without ambient transactions
    public async Task CreateUser(User user)
    {
        await using var connection = _connectionFactory.GetConnection(out var transaction);
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.Transaction = transaction; // Will be null if no ambient transaction
        command.CommandText = "INSERT INTO Users (Name, Email) VALUES (@name, @email)";
        
        // Add parameters and execute
        await command.ExecuteNonQueryAsync();
    }
}

// Usage without ambient transaction (auto-commit)
await userRepository.CreateUser(newUser);

// Usage with ambient transaction
await using var scope = AmbientTransactionScope.Create(connectionString);
await userRepository.CreateUser(newUser);
await userRepository.UpdateUserPreferences(newUser.Id, preferences);
scope.Complete(); // Both operations commit together
```

## Installation

Add the following NuGet packages to your project:
```xml
<PackageReference Include="Architect.AmbientContexts" Version="2.0.1" />
<PackageReference Include="Microsoft.Data.SqlClient" Version="6.1.0" />
```

## Requirements

- .NET 9.0 or later
- SQL Server (uses Microsoft.Data.SqlClient)
- Nullable reference types enabled

## Important Notes

- Always call `scope.Complete()` to commit transactions
- Connection strings must match across all operations in the same ambient scope
- Nested scopes automatically join parent transactions
- The library prevents direct transaction manipulation on wrapped connections
- Connections are lazily initialized only when first accessed
- All nested scopes must complete for the transaction to commit successfully
# Database Migration Instructions

## Initial Migration Created

The project now includes an initial EF Core migration that will create the **Products** table in your SQL Server database.

### Migration Files Created:
- `Migrations/20240101000000_InitialCreate.cs` - Main migration file
- `Migrations/20240101000000_InitialCreate.Designer.cs` - Migration metadata
- `Migrations/AppDbContextModelSnapshot.cs` - Current model snapshot

---

## Database Schema

The migration creates the following table:

### Products Table
| Column Name | Data Type | Constraints |
|------------|-----------|-------------|
| ProductId | int | Primary Key, Identity(1,1) |
| ProductName | nvarchar(200) | NOT NULL, Indexed |
| Description | nvarchar(1000) | NULL |
| UnitPrice | decimal(18,2) | NOT NULL |
| SKU | nvarchar(50) | NOT NULL, Unique Index |
| Quantity | int | NOT NULL |
| CreatedOn | datetime2 | NOT NULL, Default: GETUTCDATE() |
| CreatedBy | nvarchar(100) | NOT NULL |
| ModifiedOn | datetime2 | NULL |
| ModifiedBy | nvarchar(100) | NULL |

---

## How to Apply the Migration

### Option 1: Using dotnet CLI (Recommended)
```bash
cd Products
dotnet ef database update
```

### Option 2: Using Package Manager Console in Visual Studio
```powershell
Update-Database
```

### Option 3: Programmatically at Application Startup
Add the following to `Program.cs` before `app.Run()`:

```csharp
// Apply migrations automatically at startup (Development only)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }
}
```

---

## Connection String

Ensure your `appsettings.json` has a valid connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ProductsDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

Or for SQL Server:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ProductsDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

---

## Verifying the Migration

After applying the migration, verify the table was created:

```sql
-- Check if table exists
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products'

-- View table structure
EXEC sp_help 'Products'

-- Check indexes
EXEC sp_helpindex 'Products'
```

---

## Future Migrations

When you make changes to your entities, create new migrations:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

To rollback a migration:
```bash
dotnet ef database update <PreviousMigrationName>
dotnet ef migrations remove
```

---

## Troubleshooting

### Issue: "Build failed"
- Ensure all required packages are installed
- Run `dotnet restore`
- Check for compilation errors

### Issue: "Cannot connect to database"
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure you have permissions to create databases

### Issue: "A network-related or instance-specific error"
- For LocalDB: Install SQL Server Express LocalDB
- For SQL Server: Verify server name and authentication

---

**Note:** The migration is already created and ready to apply. You don't need to run `dotnet ef migrations add` again unless you make changes to your models.

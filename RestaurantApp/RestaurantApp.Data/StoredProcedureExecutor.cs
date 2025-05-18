using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Core.Interfaces.Data;
using RestaurantApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace RestaurantApp.Data
{
    public class StoredProcedureExecutor : IStoredProcedureExecutor
    {
        private readonly RestaurantDbContext _context;

        public StoredProcedureExecutor(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<List<Dish>> SearchDishesByNameAsync(string searchTerm)
        {
            var dishes = new List<Dish>();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "[dbo].[SearchDishesByName]";
                command.CommandType = CommandType.StoredProcedure;

                var parameter = new SqlParameter("@SearchTerm", SqlDbType.NVarChar, 100)
                {
                    Value = searchTerm
                };
                command.Parameters.Add(parameter);

                await _context.Database.OpenConnectionAsync();

                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        dishes.Add(new Dish
                        {
                            Id = result.GetInt32(result.GetOrdinal("Id")),
                            Name = result.GetString(result.GetOrdinal("Name")),
                            Price = result.GetDecimal(result.GetOrdinal("Price")),
                            PortionQuantity = result.GetDecimal(result.GetOrdinal("PortionQuantity")),
                            TotalQuantity = result.GetDecimal(result.GetOrdinal("TotalQuantity")),
                            CategoryId = result.GetInt32(result.GetOrdinal("CategoryId")),
                            Category = new Category
                            {
                                Id = result.GetInt32(result.GetOrdinal("Category.Id")),
                                Name = result.GetString(result.GetOrdinal("Category.Name")),
                                Description = !result.IsDBNull(result.GetOrdinal("Category.Description"))
                                    ? result.GetString(result.GetOrdinal("Category.Description"))
                                    : null
                            }
                        });
                    }
                }
            }

            return dishes;
        }

        public async Task<List<Dish>> SearchDishesByAllergenAsync(int allergenId, bool includeAllergen)
        {
            var dishes = new List<Dish>();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "[dbo].[SearchDishesByAllergen]";
                command.CommandType = CommandType.StoredProcedure;

                var allergenParam = new SqlParameter("@AllergenId", SqlDbType.Int)
                {
                    Value = allergenId
                };
                command.Parameters.Add(allergenParam);

                var includeParam = new SqlParameter("@IncludeAllergen", SqlDbType.Bit)
                {
                    Value = includeAllergen
                };
                command.Parameters.Add(includeParam);

                await _context.Database.OpenConnectionAsync();

                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        dishes.Add(new Dish
                        {
                            Id = result.GetInt32(result.GetOrdinal("Id")),
                            Name = result.GetString(result.GetOrdinal("Name")),
                            Price = result.GetDecimal(result.GetOrdinal("Price")),
                            PortionQuantity = result.GetDecimal(result.GetOrdinal("PortionQuantity")),
                            TotalQuantity = result.GetDecimal(result.GetOrdinal("TotalQuantity")),
                            CategoryId = result.GetInt32(result.GetOrdinal("CategoryId")),
                            Category = new Category
                            {
                                Id = result.GetInt32(result.GetOrdinal("Category.Id")),
                                Name = result.GetString(result.GetOrdinal("Category.Name")),
                                Description = !result.IsDBNull(result.GetOrdinal("Category.Description"))
                                    ? result.GetString(result.GetOrdinal("Category.Description"))
                                    : null
                            }
                        });
                    }
                }
            }

            return dishes;
        }

        public async Task<List<Dish>> GetPopularDishesAsync(int topCount = 10)
        {
            var dishes = new List<Dish>();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "[dbo].[GetPopularDishes]";
                command.CommandType = CommandType.StoredProcedure;

                var parameter = new SqlParameter("@TopCount", SqlDbType.Int)
                {
                    Value = topCount
                };
                command.Parameters.Add(parameter);

                await _context.Database.OpenConnectionAsync();

                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        dishes.Add(new Dish
                        {
                            Id = result.GetInt32(result.GetOrdinal("Id")),
                            Name = result.GetString(result.GetOrdinal("Name")),
                            Price = result.GetDecimal(result.GetOrdinal("Price")),
                            PortionQuantity = result.GetDecimal(result.GetOrdinal("PortionQuantity")),
                            TotalQuantity = result.GetDecimal(result.GetOrdinal("TotalQuantity")),
                            CategoryId = result.GetInt32(result.GetOrdinal("CategoryId"))
                        });
                    }
                }
            }

            return dishes;
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "[dbo].[UpdateOrderStatus]";
                command.CommandType = CommandType.StoredProcedure;

                var orderIdParam = new SqlParameter("@OrderId", SqlDbType.Int)
                {
                    Value = orderId
                };
                command.Parameters.Add(orderIdParam);

                var statusParam = new SqlParameter("@NewStatus", SqlDbType.Int)
                {
                    Value = (int)newStatus
                };
                command.Parameters.Add(statusParam);

                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        // Method to create the stored procedures in the database
        public async Task CreateStoredProceduresAsync()
        {
            // You would execute SQL scripts to create the stored procedures
            // This would typically be done during application setup or as part of migrations

            string[] procedureScripts = new string[]
            {
                GetSearchDishesByNameScript(),
                GetSearchDishesByAllergenScript(),
                GetPopularDishesScript(),
                GetOrdersReportScript(),
                GetUpdateOrderStatusScript()
            };

            foreach (var script in procedureScripts)
            {
                await _context.Database.ExecuteSqlRawAsync(script);
            }
        }

        private string GetSearchDishesByNameScript()
        {
            // Return the SQL script for creating the SearchDishesByName stored procedure
            return @"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SearchDishesByName]') AND type in (N'P', N'PC'))
                    DROP PROCEDURE [dbo].[SearchDishesByName]
                GO

                CREATE PROCEDURE [dbo].[SearchDishesByName]
                    @SearchTerm NVARCHAR(100)
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    SELECT d.Id, d.Name, d.Price, d.PortionQuantity, d.TotalQuantity, d.CategoryId,
                           c.Id AS 'Category.Id', c.Name AS 'Category.Name', c.Description AS 'Category.Description'
                    FROM Dishes d
                    INNER JOIN Categories c ON d.CategoryId = c.Id
                    WHERE d.Name LIKE '%' + @SearchTerm + '%'
                    ORDER BY d.Name;
                END
            ";
        }

        private string GetSearchDishesByAllergenScript()
        {
            // Return the SQL script for creating the SearchDishesByAllergen stored procedure
            return @"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SearchDishesByAllergen]') AND type in (N'P', N'PC'))
                    DROP PROCEDURE [dbo].[SearchDishesByAllergen]
                GO

                CREATE PROCEDURE [dbo].[SearchDishesByAllergen]
                    @AllergenId INT,
                    @IncludeAllergen BIT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    IF @IncludeAllergen = 1
                    BEGIN
                        -- Dishes that contain the allergen
                        SELECT DISTINCT d.Id, d.Name, d.Price, d.PortionQuantity, d.TotalQuantity, d.CategoryId,
                               c.Id AS 'Category.Id', c.Name AS 'Category.Name', c.Description AS 'Category.Description'
                        FROM Dishes d
                        INNER JOIN Categories c ON d.CategoryId = c.Id
                        INNER JOIN DishAllergens da ON d.Id = da.DishId
                        WHERE da.AllergenId = @AllergenId
                        ORDER BY d.Name;
                    END
                    ELSE
                    BEGIN
                        -- Dishes that do NOT contain the allergen
                        SELECT d.Id, d.Name, d.Price, d.PortionQuantity, d.TotalQuantity, d.CategoryId,
                               c.Id AS 'Category.Id', c.Name AS 'Category.Name', c.Description AS 'Category.Description'
                        FROM Dishes d
                        INNER JOIN Categories c ON d.CategoryId = c.Id
                        WHERE d.Id NOT IN (
                            SELECT DishId FROM DishAllergens WHERE AllergenId = @AllergenId
                        )
                        ORDER BY d.Name;
                    END
                END
            ";
        }

        private string GetPopularDishesScript()
        {
            // Return the SQL script for creating the GetPopularDishes stored procedure
            return @"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPopularDishes]') AND type in (N'P', N'PC'))
                    DROP PROCEDURE [dbo].[GetPopularDishes]
                GO

                CREATE PROCEDURE [dbo].[GetPopularDishes]
                    @TopCount INT = 10
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    SELECT TOP (@TopCount) d.Id, d.Name, d.Price, d.PortionQuantity, d.TotalQuantity, d.CategoryId,
                           COUNT(od.Id) AS OrderCount
                    FROM Dishes d
                    INNER JOIN OrderDetails od ON d.Id = od.DishId
                    INNER JOIN Orders o ON od.OrderId = o.Id
                    WHERE o.Status <> 5 -- Not cancelled
                    GROUP BY d.Id, d.Name, d.Price, d.PortionQuantity, d.TotalQuantity, d.CategoryId
                    ORDER BY OrderCount DESC;
                END
            ";
        }

        private string GetOrdersReportScript()
        {
            // Return the SQL script for creating the GetOrdersReport stored procedure
            return @"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetOrdersReport]') AND type in (N'P', N'PC'))
                    DROP PROCEDURE [dbo].[GetOrdersReport]
                GO

                CREATE PROCEDURE [dbo].[GetOrdersReport]
                    @StartDate DATETIME,
                    @EndDate DATETIME
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    -- Orders summary
                    SELECT 
                        COUNT(*) AS TotalOrders,
                        SUM(o.FoodCost) AS TotalFoodRevenue,
                        SUM(o.ShippingCost) AS TotalShippingRevenue,
                        SUM(o.Discount) AS TotalDiscounts,
                        SUM(o.FoodCost + o.ShippingCost - o.Discount) AS TotalRevenue,
                        (SELECT COUNT(*) FROM Orders WHERE Status = 5 AND OrderDate BETWEEN @StartDate AND @EndDate) AS CancelledOrders
                    FROM Orders o
                    WHERE o.OrderDate BETWEEN @StartDate AND @EndDate;
                    
                    -- Orders by date
                    SELECT 
                        CAST(o.OrderDate AS DATE) AS OrderDate,
                        COUNT(*) AS OrderCount,
                        SUM(o.FoodCost + o.ShippingCost - o.Discount) AS DailyRevenue
                    FROM Orders o
                    WHERE o.OrderDate BETWEEN @StartDate AND @EndDate
                    GROUP BY CAST(o.OrderDate AS DATE)
                    ORDER BY CAST(o.OrderDate AS DATE);
                    
                    -- Top selling dishes
                    SELECT TOP 10
                        d.Name AS DishName,
                        SUM(od.Quantity) AS TotalQuantitySold,
                        SUM(od.Quantity * od.UnitPrice) AS TotalRevenue
                    FROM OrderDetails od
                    INNER JOIN Orders o ON od.OrderId = o.Id
                    INNER JOIN Dishes d ON od.DishId = d.Id
                    WHERE o.OrderDate BETWEEN @StartDate AND @EndDate
                    AND o.Status <> 5 -- Not cancelled
                    AND od.DishId IS NOT NULL
                    GROUP BY d.Name
                    ORDER BY TotalQuantitySold DESC;
                    
                    -- Top selling menus
                    SELECT TOP 10
                        m.Name AS MenuName,
                        SUM(od.Quantity) AS TotalQuantitySold,
                        SUM(od.Quantity * od.UnitPrice) AS TotalRevenue
                    FROM OrderDetails od
                    INNER JOIN Orders o ON od.OrderId = o.Id
                    INNER JOIN Menus m ON od.MenuId = m.Id
                    WHERE o.OrderDate BETWEEN @StartDate AND @EndDate
                    AND o.Status <> 5 -- Not cancelled
                    AND od.MenuId IS NOT NULL
                    GROUP BY m.Name
                    ORDER BY TotalQuantitySold DESC;
                END
            ";
        }

        private string GetUpdateOrderStatusScript()
        {
            // Return the SQL script for creating the UpdateOrderStatus stored procedure
            return @"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateOrderStatus]') AND type in (N'P', N'PC'))
                    DROP PROCEDURE [dbo].[UpdateOrderStatus]
                GO

                CREATE PROCEDURE [dbo].[UpdateOrderStatus]
                    @OrderId INT,
                    @NewStatus INT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    -- Update order status
                    UPDATE Orders
                    SET Status = @NewStatus
                    WHERE Id = @OrderId;
                    
                    -- If order is in preparation, update dish quantities
                    IF @NewStatus = 2 -- InPreparation
                    BEGIN
                        -- Create a temporary table to hold quantity updates
                        CREATE TABLE #TempQuantityUpdates (
                            DishId INT,
                            QuantityToDeduct DECIMAL(18,2)
                        );
                        
                        -- Calculate quantities for dishes directly in order
                        INSERT INTO #TempQuantityUpdates (DishId, QuantityToDeduct)
                        SELECT od.DishId, od.Quantity * d.PortionQuantity
                        FROM OrderDetails od
                        INNER JOIN Dishes d ON od.DishId = d.Id
                        WHERE od.OrderId = @OrderId AND od.DishId IS NOT NULL;
                        
                        -- Calculate quantities for dishes in menus in the order
                        INSERT INTO #TempQuantityUpdates (DishId, QuantityToDeduct)
                        SELECT md.DishId, od.Quantity * md.QuantityInMenu
                        FROM OrderDetails od
                        INNER JOIN Menus m ON od.MenuId = m.Id
                        INNER JOIN MenuDishes md ON m.Id = md.MenuId
                        WHERE od.OrderId = @OrderId AND od.MenuId IS NOT NULL;
                        
                        -- Aggregate the quantities by dish
                        CREATE TABLE #FinalQuantityUpdates (
                            DishId INT PRIMARY KEY,
                            TotalQuantityToDeduct DECIMAL(18,2)
                        );
                        
                        INSERT INTO #FinalQuantityUpdates (DishId, TotalQuantityToDeduct)
                        SELECT DishId, SUM(QuantityToDeduct)
                        FROM #TempQuantityUpdates
                        GROUP BY DishId;
                        
                        -- Update dish quantities
                        UPDATE d
                        SET TotalQuantity = d.TotalQuantity - fqu.TotalQuantityToDeduct
                        FROM Dishes d
                        INNER JOIN #FinalQuantityUpdates fqu ON d.Id = fqu.DishId;
                        
                        -- Clean up temporary tables
                        DROP TABLE #TempQuantityUpdates;
                        DROP TABLE #FinalQuantityUpdates;
                    END
                END
            ";
        }
    }
}
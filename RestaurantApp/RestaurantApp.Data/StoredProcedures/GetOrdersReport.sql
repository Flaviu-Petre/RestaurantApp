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
    AND o.Status <> 5 
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
    AND o.Status <> 5
    AND od.MenuId IS NOT NULL
    GROUP BY m.Name
    ORDER BY TotalQuantitySold DESC;
END
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
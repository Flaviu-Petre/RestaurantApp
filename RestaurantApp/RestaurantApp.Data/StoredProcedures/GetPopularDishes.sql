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
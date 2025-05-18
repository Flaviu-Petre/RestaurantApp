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
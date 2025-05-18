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
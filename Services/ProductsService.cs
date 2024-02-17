using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;
using Microsoft.CodeAnalysis;

namespace IOMSYS.Services
{
    public class ProductsService : IProductsService
    {
        private readonly DapperContext _dapperContext;

        public ProductsService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public async Task<IEnumerable<ProductsModel>> GetAllProductsAsync()
        {
            var sql = @"SELECT 
                            P.ProductId, 
                            P.ProductName, 
                            C.CategoryName, 
                            T.ProductTypeName, 
                            P.SellPrice, 
                            P.BuyPrice, 
                            P.MaxDiscount, 
                            P.DiscountsOn, 
                            P.Notes, 
                            P.CategoryId, 
                            P.ProductTypeId,
                            P.MinQuantity,
                            (COALESCE(SUM(pi.Quantity), 0) - COALESCE((
                                SELECT SUM(si.Quantity)
                                FROM SalesItems si
                                JOIN SalesInvoiceItems sii ON si.SalesItemId = sii.SalesItemId
                                JOIN SalesInvoices si2 ON sii.SalesInvoiceId = si2.SalesInvoiceId
                                WHERE si.ProductId = P.ProductId
                            ), 0)) AS TotalQuantity
                        FROM 
                            Products P
                        INNER JOIN 
                            Categories C ON C.CategoryId = P.CategoryId
                        INNER JOIN 
                            ProductTypes T ON T.ProductTypeId = P.ProductTypeId
                        LEFT JOIN 
                            PurchaseItems pi ON P.ProductId = pi.ProductId
                        LEFT JOIN 
                            SalesItems si ON P.ProductId = si.ProductId
                        GROUP BY 
                            P.ProductId, 
                            P.ProductName, 
                            C.CategoryName, 
                            T.ProductTypeName, 
                            P.SellPrice, 
                            P.BuyPrice, 
                            P.MaxDiscount, 
                            P.DiscountsOn, 
                            P.Notes, 
                            P.CategoryId, 
                            P.ProductTypeId,
                            P.MinQuantity";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductsModel>(sql).ConfigureAwait(false);
            }
        }
        public async Task<ProductsModel?> SelectProductByIdAsync(int productId)
        {
            var sql = @"SELECT P.ProductId, P.ProductName, C.CategoryName, P.ProductPhoto, T.ProductTypeName, P.SellPrice, P.BuyPrice, P.MaxDiscount, P.DiscountsOn, P.Notes, P.CategoryId, P.ProductTypeId,P.MinQuantity
                FROM Products P
                INNER JOIN Categories C ON C.CategoryId = P.CategoryId
                INNER JOIN ProductTypes T ON T.ProductTypeId = P.ProductTypeId
            WHERE ProductId = @ProductId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<ProductsModel>(sql, new { ProductId = productId }).ConfigureAwait(false);
            }
        }

        public async Task<int> InsertProductAsync(ProductsModel product)
        {
            var sql = @"INSERT INTO Products (ProductId, ProductName, CategoryId, ProductPhoto, ProductTypeId, SellPrice, BuyPrice, MaxDiscount, DiscountsOn, Notes,MinQuantity) 
                VALUES (@ProductId, @ProductName, @CategoryId, @ProductPhoto, @ProductTypeId, @SellPrice, @BuyPrice, @MaxDiscount, @DiscountsOn, @Notes,@MinQuantity);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    await db.ExecuteAsync(sql, new { product.ProductId, product.ProductName, product.CategoryId, product.ProductPhoto, product.ProductTypeId, product.SellPrice, product.BuyPrice, product.MaxDiscount, product.DiscountsOn, product.Notes,product.MinQuantity }).ConfigureAwait(false);
                    return product.ProductId;
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdateProductAsync(ProductsModel product)
        {
            var sql = @"UPDATE Products SET ProductName = @ProductName, CategoryId = @CategoryId, ProductPhoto = @ProductPhoto, ProductTypeId = @ProductTypeId, SellPrice = @SellPrice, BuyPrice = @BuyPrice, MaxDiscount = @MaxDiscount, DiscountsOn = @DiscountsOn, Notes = @Notes, MinQuantity = @MinQuantity WHERE ProductId = @ProductId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { product.ProductName, product.CategoryId, product.ProductPhoto, product.ProductTypeId, product.SellPrice, product.BuyPrice, product.MaxDiscount, product.DiscountsOn, product.Notes, product.MinQuantity, product.ProductId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> DeleteProductAsync(int productId)
        {
            var sql = @"DELETE FROM Products WHERE ProductId = @ProductId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { ProductId = productId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }
        public async Task<int> UpdateProductBuyandSellPriceAsync(int ProductId,decimal BuyPrice,decimal SellPrice)
        {
            var sql = @"UPDATE Products SET BuyPrice = @BuyPrice,SellPrice=@SellPrice WHERE ProductId = @ProductId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, new { BuyPrice, SellPrice, ProductId }).ConfigureAwait(false);
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<IEnumerable<ProductsModel>> GetAllProductsInWarehouseAsync()
        {
            var sql = @"
                SELECT 
                    P.ProductId, 
                    P.ProductName, 
                    C.CategoryName, 
                    T.ProductTypeName,
                    s.SizeName,
                    r.ColorName,
                    COALESCE(SUM(pi.Quantity), 0) AS PurchasedQuantity,
                    COALESCE(SUM(si.Quantity), 0) AS SoldQuantity,
                    COALESCE(SUM(pi.BuyPrice * pi.Quantity), 0) AS TotalBuyPrice,
                    (COALESCE(SUM(pi.Quantity), 0) - COALESCE((
                        SELECT SUM(si.Quantity)
                        FROM SalesItems si
                        JOIN SalesInvoiceItems sii ON si.SalesItemId = sii.SalesItemId
                        JOIN SalesInvoices si2 ON sii.SalesInvoiceId = si2.SalesInvoiceId
                        WHERE si.ProductId = P.ProductId AND si2.BranchId = @BranchId
                    ), 0)) AS TotalQuantity,
                    pi.BranchId 
                FROM 
                    Products P
                INNER JOIN 
                    Categories C ON C.CategoryId = P.CategoryId
                INNER JOIN 
                    ProductTypes T ON T.ProductTypeId = P.ProductTypeId
                LEFT JOIN 
                    PurchaseItems pi ON P.ProductId = pi.ProductId
                LEFT JOIN 
                    SalesItems si ON P.ProductId = si.ProductId AND pi.SizeId = si.SizeId AND pi.ColorId = si.ColorId
                LEFT JOIN 
                    Colors r ON r.ColorId = pi.ColorId
                LEFT JOIN 
                    Sizes s ON s.SizeId = pi.SizeId
                GROUP BY 
                    P.ProductId, 
                    P.ProductName, 
                    C.CategoryName, 
                    T.ProductTypeName,
                    s.SizeName,
                    r.ColorName,
                    pi.BranchId 
                HAVING 
                    (COALESCE(SUM(pi.Quantity), 0) - COALESCE(SUM(si.Quantity), 0)) > 0;";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductsModel>(sql).ConfigureAwait(false);
            }
        }
        public async Task<IEnumerable<ProductsModel>> GetAllProductsInWarehouseAsync(int BranchId)
        {
            var sql = @"
            SELECT 
                P.ProductId, 
                P.ProductName, 
                P.SellPrice, 
                P.BuyPrice, 
                C.CategoryName,
                T.ProductTypeName,
                s.SizeName,
                r.ColorName,
                COALESCE(SUM(si.Quantity), 0) AS TotalSoldQuantity,
                COALESCE(SUM(pi.BuyPrice * pi.Quantity), 0) AS TotalBuyPrice,                
                COALESCE(SUM(si.SellPrice * si.Quantity), 0) AS TotalSellPrice,
                (COALESCE(SUM(pi.Quantity), 0) - COALESCE(SUM(si.Quantity), 0)) AS TotalQuantity,
                pi.BranchId
            FROM 
                Products P
            INNER JOIN 
                Categories C ON C.CategoryId = P.CategoryId
            INNER JOIN 
                ProductTypes T ON T.ProductTypeId = P.ProductTypeId
            LEFT JOIN 
                PurchaseItems pi ON P.ProductId = pi.ProductId
            LEFT JOIN 
                SalesItems si ON P.ProductId = si.ProductId AND pi.SizeId = si.SizeId AND pi.ColorId = si.ColorId
            LEFT JOIN 
                Colors r ON r.ColorId = pi.ColorId
            LEFT JOIN 
                Sizes s ON s.SizeId = pi.SizeId
            WHERE 
                pi.BranchId = @BranchId
            GROUP BY 
                P.ProductId, 
                P.ProductName, 
                P.SellPrice, 
                P.BuyPrice, 
                C.CategoryName, 
                T.ProductTypeName,
                s.SizeName,
                r.ColorName,
                pi.BranchId 
            HAVING 
                (COALESCE(SUM(pi.Quantity), 0) - COALESCE(SUM(si.Quantity), 0)) > 0;";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductsModel>(sql, new { BranchId }).ConfigureAwait(false);
            }
        }
        public async Task<IEnumerable<ProductsModel>> GetMinQuantityProductsInWarehouseAsync(int BranchId)
        {
            var sql = @"
            SELECT 
                P.ProductId, 
                P.ProductName, 
                C.CategoryName, 
                T.ProductTypeName, 
                P.SellPrice, 
                P.BuyPrice, 
                P.MaxDiscount, 
                P.DiscountsOn, 
                P.Notes, 
                P.CategoryId, 
                P.ProductTypeId,
                P.MinQuantity,
                (COALESCE(SUM(pi.Quantity), 0) - COALESCE((
                    SELECT SUM(si.Quantity)
                    FROM SalesItems si
                    JOIN SalesInvoiceItems sii ON si.SalesItemId = sii.SalesItemId
                    JOIN SalesInvoices si2 ON sii.SalesInvoiceId = si2.SalesInvoiceId
                    WHERE si.ProductId = P.ProductId AND si2.BranchId = @BranchId
                ), 0)) AS AvailableQty
            FROM 
                Products P
            INNER JOIN 
                Categories C ON C.CategoryId = P.CategoryId
            INNER JOIN 
                ProductTypes T ON T.ProductTypeId = P.ProductTypeId
            LEFT JOIN 
                PurchaseItems pi ON P.ProductId = pi.ProductId AND pi.BranchId = @BranchId
            LEFT JOIN 
                SalesItems si ON si.ProductId = P.ProductId
            LEFT JOIN 
                SalesInvoiceItems sii ON si.SalesItemId = sii.SalesItemId
            LEFT JOIN 
                SalesInvoices inv ON sii.SalesInvoiceId = inv.SalesInvoiceId AND inv.BranchId = @BranchId
            GROUP BY 
                P.ProductId, 
                P.ProductName, 
                C.CategoryName, 
                T.ProductTypeName, 
                P.SellPrice, 
                P.BuyPrice, 
                P.MaxDiscount, 
                P.DiscountsOn, 
                P.Notes, 
                P.CategoryId, 
                P.ProductTypeId,
                P.MinQuantity
            HAVING 
                   (COALESCE(SUM(pi.Quantity), 0) - COALESCE((
                    SELECT SUM(si.Quantity)
                    FROM SalesItems si
                    JOIN SalesInvoiceItems sii ON si.SalesItemId = sii.SalesItemId
                    JOIN SalesInvoices si2 ON sii.SalesInvoiceId = si2.SalesInvoiceId
                    WHERE si.ProductId = P.ProductId AND si2.BranchId = @BranchId
                ), 0)) < P.MinQuantity;";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductsModel>(sql, new { BranchId }).ConfigureAwait(false);
            }
        }
        public async Task<IEnumerable<ProductsModel>> GetAvailableSizesAndColorsForProduct(int productId)
        {
            var sql = @"SELECT DISTINCT
                    S.SizeId,
                    S.SizeName,
                    Col.ColorId,
                    Col.ColorName
                FROM PurchaseItems PI
                LEFT JOIN Sizes S ON PI.SizeId = S.SizeId
                LEFT JOIN Colors Col ON PI.ColorId = Col.ColorId
                WHERE PI.ProductId = @ProductId
                AND PI.Quantity > 0;";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductsModel>(sql, new { ProductId = productId }).ConfigureAwait(false);
            }
        }
        public async Task<int> GetAvailableQuantity(int productId, int colorId, int sizeId, int branchId)
        {
            var sql = @"
                    WITH Purchased AS (
                        SELECT 
                            PI.ProductId, 
                            PI.SizeId, 
                            PI.ColorId, 
                            SUM(PI.Quantity) AS PurchasedQuantity
                        FROM PurchaseItems PI
                        JOIN PurchaseInvoiceItems PII ON PI.PurchaseItemId = PII.PurchaseItemId
                        JOIN PurchaseInvoices PInv ON PII.PurchaseInvoiceId = PInv.PurchaseInvoiceId
                        WHERE PInv.BranchId = @BranchId
                        GROUP BY PI.ProductId, PI.SizeId, PI.ColorId
                    ),
                    Sold AS (
                        SELECT 
                            SI.ProductId, 
                            SI.SizeId, 
                            SI.ColorId, 
                            SUM(SI.Quantity) AS SoldQuantity
                        FROM SalesItems SI
                        JOIN SalesInvoiceItems SII ON SI.SalesItemId = SII.SalesItemId
                        JOIN SalesInvoices SInv ON SII.SalesInvoiceId = SInv.SalesInvoiceId
                        WHERE SInv.BranchId = @BranchId
                        GROUP BY SI.ProductId, SI.SizeId, SI.ColorId
                    )
                    SELECT 
                        (ISNULL(P.PurchasedQuantity, 0) - ISNULL(S.SoldQuantity, 0)) AS AvailableQuantity
                    FROM Purchased P
                    LEFT JOIN Sold S ON P.ProductId = S.ProductId AND P.SizeId = S.SizeId AND P.ColorId = S.ColorId
                    WHERE P.ProductId = @ProductId AND P.SizeId = @SizeId AND P.ColorId = @ColorId;
                ";

            using (var connection = _dapperContext.CreateConnection())
            {
                var availableQuantity = await connection.QuerySingleOrDefaultAsync<int>(sql, new { ProductId = productId, ColorId = colorId, SizeId = sizeId, BranchId = branchId });
                return availableQuantity;
            }
        }

    }
}

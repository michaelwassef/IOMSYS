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
            var sql = @"SELECT P.ProductId, P.ProductName, C.CategoryName, P.ProductPhoto, T.ProductTypeName, P.SellPrice, P.BuyPrice, P.MaxDiscount, P.DiscountsOn, P.Notes, P.CategoryId, P.ProductTypeId,P.MinQuantity
                FROM Products P
                INNER JOIN Categories C ON C.CategoryId = P.CategoryId
                INNER JOIN ProductTypes T ON T.ProductTypeId = P.ProductTypeId";

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
        WITH Purchased AS (
            SELECT 
                PI.ProductId, 
                PI.SizeId, 
                PI.ColorId, 
                SUM(PI.Quantity) AS PurchasedQuantity
            FROM PurchaseItems PI
            GROUP BY PI.ProductId, PI.SizeId, PI.ColorId
        ),
        Sold AS (
            SELECT 
                SI.ProductId, 
                SI.SizeId, 
                SI.ColorId, 
                SUM(SI.Quantity) AS SoldQuantity
            FROM SalesItems SI
            GROUP BY SI.ProductId, SI.SizeId, SI.ColorId
        ),
        NetAvailable AS (
            SELECT 
                P.ProductId, 
                P.SizeId, 
                P.ColorId, 
                (ISNULL(P.PurchasedQuantity, 0) - ISNULL(S.SoldQuantity, 0)) AS AvailableQuantity
            FROM Purchased P
            LEFT JOIN Sold S ON P.ProductId = S.ProductId AND P.SizeId = S.SizeId AND P.ColorId = S.ColorId
        )
        SELECT 
            Prod.ProductId, 
            Prod.ProductName,
            Prod.CategoryId, 
            C.CategoryName,
            Prod.MinQuantity,
            Prod.SellPrice, 
            Prod.BuyPrice, 
            S.SizeId,
            S.SizeName,
            Col.ColorId,
            Col.ColorName,
            NA.AvailableQuantity AS TotalQuantity,
            Prod.Notes
        FROM Products Prod
        INNER JOIN Categories C ON C.CategoryId = Prod.CategoryId
        INNER JOIN ProductTypes T ON T.ProductTypeId = Prod.ProductTypeId
        INNER JOIN NetAvailable NA ON NA.ProductId = Prod.ProductId
        LEFT JOIN Sizes S ON S.SizeId = NA.SizeId
        LEFT JOIN Colors Col ON Col.ColorId = NA.ColorId
        WHERE NA.AvailableQuantity > 0
        GROUP BY 
            Prod.ProductId, 
            Prod.ProductName,
            Prod.CategoryId, 
            C.CategoryName,
            Prod.MinQuantity,
            S.SizeId,
            S.SizeName,
            Col.ColorId,
            Col.ColorName,    
            Prod.SellPrice,
            Prod.BuyPrice,
            NA.AvailableQuantity,
            Prod.Notes;";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductsModel>(sql).ConfigureAwait(false);
            }
        }
        public async Task<IEnumerable<ProductsModel>> GetAllProductsInWarehouseAsync(int BranchId)
        {
            var sql = @"
            WITH Purchased AS (
                SELECT 
                    PI.ProductId, 
                    PI.SizeId, 
                    PI.ColorId, 
                    SUM(PI.Quantity) AS PurchasedQuantity
                FROM PurchaseItems PI
                INNER JOIN PurchaseInvoiceItems PII ON PI.PurchaseItemId = PII.PurchaseItemId
                INNER JOIN PurchaseInvoices PInv ON PII.PurchaseInvoiceId = PInv.PurchaseInvoiceId
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
                INNER JOIN SalesInvoiceItems SII ON SI.SalesItemId = SII.SalesItemId
                INNER JOIN SalesInvoices SInv ON SII.SalesInvoiceId = SInv.SalesInvoiceId
                WHERE SInv.BranchId = @BranchId
                GROUP BY SI.ProductId, SI.SizeId, SI.ColorId
            ),
            NetAvailable AS (
                SELECT 
                    P.ProductId, 
                    P.SizeId, 
                    P.ColorId, 
                    (ISNULL(P.PurchasedQuantity, 0) - ISNULL(S.SoldQuantity, 0)) AS AvailableQuantity
                FROM Purchased P
                LEFT JOIN Sold S ON P.ProductId = S.ProductId AND P.SizeId = S.SizeId AND P.ColorId = S.ColorId
            ),
            TotalSellPrice AS (
                SELECT
                    SI.ProductId,
                    SI.SizeId,
                    SI.ColorId,
                    SUM(SI.Quantity * SI.SellPrice) AS TotalSellPrice,
                    SUM(SI.Quantity) AS TotalSoldQuantity
                FROM SalesItems SI
                INNER JOIN SalesInvoiceItems SII ON SI.SalesItemId = SII.SalesItemId
                INNER JOIN SalesInvoices SInv ON SII.SalesInvoiceId = SInv.SalesInvoiceId
                WHERE SInv.BranchId = @BranchId
                GROUP BY SI.ProductId, SI.SizeId, SI.ColorId
            ),
            TotalBuyPrice AS (
                SELECT
                    PI.ProductId,
                    PI.SizeId,
                    PI.ColorId,
                    SUM(PI.Quantity * PI.BuyPrice) AS TotalBuyPrice
                FROM PurchaseItems PI
                INNER JOIN PurchaseInvoiceItems PII ON PI.PurchaseItemId = PII.PurchaseItemId
                INNER JOIN PurchaseInvoices PInv ON PII.PurchaseInvoiceId = PInv.PurchaseInvoiceId
                WHERE PInv.BranchId = @BranchId
                GROUP BY PI.ProductId, PI.SizeId, PI.ColorId
            )
            SELECT 
                Prod.ProductId, 
                Prod.ProductName,
                Prod.CategoryId, 
                C.CategoryName,
                Prod.MinQuantity,
                Prod.SellPrice, 
                Prod.BuyPrice, 
                S.SizeId,
                S.SizeName,
                Col.ColorId,
                Col.ColorName,
                NA.AvailableQuantity AS TotalQuantity,
                Prod.Notes,
                TS.TotalSellPrice,
                TB.TotalBuyPrice,
                TS.TotalSoldQuantity
            FROM Products Prod
            INNER JOIN Categories C ON C.CategoryId = Prod.CategoryId
            INNER JOIN ProductTypes T ON T.ProductTypeId = Prod.ProductTypeId
            INNER JOIN NetAvailable NA ON NA.ProductId = Prod.ProductId
            LEFT JOIN Sizes S ON S.SizeId = NA.SizeId
            LEFT JOIN Colors Col ON Col.ColorId = NA.ColorId
            LEFT JOIN TotalSellPrice TS ON TS.ProductId = Prod.ProductId AND TS.SizeId = NA.SizeId AND TS.ColorId = NA.ColorId
            LEFT JOIN TotalBuyPrice TB ON TB.ProductId = Prod.ProductId AND TB.SizeId = NA.SizeId AND TB.ColorId = NA.ColorId
            GROUP BY 
                Prod.ProductId, 
                Prod.ProductName,
                Prod.CategoryId, 
                C.CategoryName,
                Prod.MinQuantity,
                S.SizeId,
                S.SizeName,
                Col.ColorId,
                Col.ColorName,    
                Prod.SellPrice,
                Prod.BuyPrice,
                NA.AvailableQuantity,
                Prod.Notes,
                TS.TotalSellPrice,
                TB.TotalBuyPrice,
                TS.TotalSoldQuantity;
            ";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductsModel>(sql, new { BranchId }).ConfigureAwait(false);
            }
        }
        public async Task<IEnumerable<ProductsModel>> GetMinQuantityProductsInWarehouseAsync(int BranchId)
        {
            var sql = @"
            SELECT 
                Prod.ProductId,
                Prod.ProductName,
                COALESCE(NA.AvailableQuantity, 0) AS AvailableQty,
                C.CategoryName,
                Prod.MinQuantity
            FROM Products Prod
            INNER JOIN Categories C ON C.CategoryId = Prod.CategoryId
            LEFT JOIN (
                SELECT 
                    PI.ProductId,
                    SUM(PI.Quantity) AS PurchasedQuantity
                FROM PurchaseItems PI
                INNER JOIN PurchaseInvoiceItems PII ON PI.PurchaseItemId = PII.PurchaseItemId
                INNER JOIN PurchaseInvoices PInv ON PII.PurchaseInvoiceId = PInv.PurchaseInvoiceId
                WHERE PInv.BranchId = @BranchId
                GROUP BY PI.ProductId
            ) PurchasedProducts ON PurchasedProducts.ProductId = Prod.ProductId
            LEFT JOIN (
                SELECT 
                    SI.ProductId,
                    SUM(SI.Quantity) AS SoldQuantity
                FROM SalesItems SI
                INNER JOIN SalesInvoiceItems SII ON SI.SalesItemId = SII.SalesItemId
                INNER JOIN SalesInvoices SInv ON SII.SalesInvoiceId = SInv.SalesInvoiceId
                WHERE SInv.BranchId = @BranchId
                GROUP BY SI.ProductId
            ) SoldProducts ON SoldProducts.ProductId = Prod.ProductId
            LEFT JOIN (
                SELECT 
                    PI.ProductId, 
                    SUM(PI.Quantity) AS PurchasedQuantity,
                    ISNULL(SUM(SI.Quantity), 0) AS SoldQuantity,
                    (SUM(PI.Quantity) - ISNULL(SUM(SI.Quantity), 0)) AS AvailableQuantity
                FROM PurchaseItems PI
                LEFT JOIN SalesItems SI ON PI.ProductId = SI.ProductId
                INNER JOIN PurchaseInvoiceItems PII ON PI.PurchaseItemId = PII.PurchaseItemId
                INNER JOIN PurchaseInvoices PInv ON PII.PurchaseInvoiceId = PInv.PurchaseInvoiceId
                WHERE PInv.BranchId = @BranchId
                GROUP BY PI.ProductId
            ) NA ON NA.ProductId = Prod.ProductId
            WHERE (PurchasedProducts.ProductId IS NULL OR PurchasedProducts.PurchasedQuantity < Prod.MinQuantity)
              AND (SoldProducts.ProductId IS NULL OR SoldProducts.SoldQuantity < Prod.MinQuantity);";

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

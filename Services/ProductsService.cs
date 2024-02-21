using Dapper;
using IOMSYS.Dapper;
using IOMSYS.IServices;
using IOMSYS.Models;

namespace IOMSYS.Services
{
    public class ProductsService : IProductsService
    {
        private readonly DapperContext _dapperContext;

        public ProductsService(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        //all products
        public async Task<IEnumerable<ProductsModel>> GetAllProductsAsync()
        {
            var sql = @"SELECT 
                            P.ProductId, 
                            P.ProductName, 
                            P.CategoryId, 
                            C.CategoryName, 
                            P.ProductTypeId,
                            T.ProductTypeName, 
                            P.SellPrice, 
                            P.BuyPrice, 
                            P.MaxDiscount, 
                            P.DiscountsOn, 
                            P.Notes, 
                            P.MinQuantity,
                            COALESCE(SUM(BI.AvailableQty), 0) AS TotalQuantity
                        FROM 
                            Products P
                        INNER JOIN 
                            Categories C ON C.CategoryId = P.CategoryId
                        INNER JOIN 
                            ProductTypes T ON T.ProductTypeId = P.ProductTypeId
                        LEFT JOIN 
                            BranchInventory BI ON P.ProductId = BI.ProductId
                        GROUP BY 
                            P.ProductId, 
                            P.ProductName, 
                            P.CategoryId, 
                            C.CategoryName, 
                            P.ProductTypeId,
                            T.ProductTypeName, 
                            P.SellPrice, 
                            P.BuyPrice, 
                            P.MaxDiscount, 
                            P.DiscountsOn, 
                            P.Notes, 
                            P.MinQuantity;
                        ";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductsModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<ProductsModel?> SelectProductByIdAsync(int productId)
        {
            var sql = @"SELECT P.ProductId, P.ProductName, C.CategoryName, P.ProductPhoto, T.ProductTypeName, P.SellPrice, P.BuyPrice, P.MaxDiscount, P.DiscountsOn, P.Notes, P.CategoryId, P.ProductTypeId, P.MinQuantity
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
            var sql = @"INSERT INTO Products (ProductId, ProductName, CategoryId, ProductPhoto, ProductTypeId, SellPrice, BuyPrice, MaxDiscount, DiscountsOn, Notes, MinQuantity) 
                VALUES (@ProductId, @ProductName, @CategoryId, @ProductPhoto, @ProductTypeId, @SellPrice, @BuyPrice, @MaxDiscount, @DiscountsOn, @Notes, @MinQuantity);";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    await db.ExecuteAsync(sql, new { product.ProductId, product.ProductName, product.CategoryId, product.ProductPhoto, product.ProductTypeId, product.SellPrice, product.BuyPrice, product.MaxDiscount, product.DiscountsOn, product.Notes, product.MinQuantity }).ConfigureAwait(false);
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

        public async Task<int> UpdateProductBuyandSellPriceAsync(int ProductId, decimal BuyPrice, decimal SellPrice)
        {
            var sql = @"UPDATE Products SET BuyPrice = @BuyPrice, SellPrice = @SellPrice WHERE ProductId = @ProductId";
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

        //it return ava product in sales page
        public async Task<IEnumerable<ProductsModel>> GetAllProductsInWarehouseAsync()
        {
            var sql = @"
               SELECT 
                    P.ProductId, 
                    P.ProductName, 
                    C.CategoryName, 
                    T.ProductTypeName,
                    SUM(BI.AvailableQty) AS TotalQuantity
                FROM 
                    Products P
                INNER JOIN 
                    Categories C ON C.CategoryId = P.CategoryId
                INNER JOIN 
                    ProductTypes T ON T.ProductTypeId = P.ProductTypeId
                INNER JOIN 
                    BranchInventory BI ON P.ProductId = BI.ProductId
                GROUP BY
                    P.ProductId, 
                    P.ProductName, 
                    C.CategoryName, 
                    T.ProductTypeName
                HAVING 
                    SUM(BI.AvailableQty) > 0;";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductsModel>(sql).ConfigureAwait(false);
            }
        }

        //المخزن بالفرع
        public async Task<IEnumerable<ProductsModel>> GetAllProductsInWarehouseAsync(int BranchId)
        {
            var sql = @"
           SELECT BI.ProductId
                  ,P.ProductName
                  ,BI.SizeId
                  ,S.SizeName
                  ,BI.ColorId
                  ,R.ColorName
                  ,C.CategoryName
                  ,T.ProductTypeName
                  ,BI.BranchId
                  ,AvailableQty 
                  ,COALESCE(si.TotalSoldQuantity, 0) AS TotalSoldQuantity
                  ,COALESCE(si.TotalSellPrice, 0) AS TotalSellPrice 
                  ,COALESCE(SUM(BI.AvailableQty), 0) AS TotalQuantity
            FROM IOMSYS.dbo.BranchInventory BI
            INNER JOIN
                     Products p ON BI.ProductId = p.ProductId
            INNER JOIN 
                     Categories C ON C.CategoryId = P.CategoryId
            INNER JOIN 
                     ProductTypes T ON T.ProductTypeId = P.ProductTypeId
            LEFT JOIN 
                     Colors r ON r.ColorId = BI.ColorId
            LEFT JOIN 
                     Sizes s ON s.SizeId = BI.SizeId
            LEFT JOIN 
                   (SELECT si.ProductId, si.ColorId, si.SizeId, si2.BranchId, SUM(si.Quantity) AS TotalSoldQuantity, SUM(si.SellPrice * si.Quantity) AS TotalSellPrice
                   FROM SalesItems si
                   JOIN SalesInvoiceItems sii ON si.SalesItemId = sii.SalesItemId
                   JOIN SalesInvoices si2 ON sii.SalesInvoiceId = si2.SalesInvoiceId
                   WHERE si2.BranchId = @BranchId
                   GROUP BY si.ProductId, si.ColorId, si.SizeId, si2.BranchId) si ON BI.ProductId = si.ProductId AND BI.ColorId = si.ColorId AND BI.SizeId = si.SizeId AND BI.BranchId = si.BranchId
            GROUP BY
                   BI.ProductId
                  ,P.ProductName
                  ,BI.SizeId
                  ,S.SizeName
                  ,BI.ColorId
                  ,R.ColorName
                  ,C.CategoryName
                  ,T.ProductTypeName
                  ,BI.BranchId
                  ,AvailableQty 
                  ,TotalSoldQuantity
                  ,TotalSellPrice
            HAVING BI.BranchId = @BranchId AND COALESCE(SUM(BI.AvailableQty), 0)> 0";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductsModel>(sql, new { BranchId }).ConfigureAwait(false);
            }
        }

        //النواقص
        public async Task<IEnumerable<ProductsModel>> GetMinQuantityProductsInWarehouseAsync(int BranchId)
        {
            var sql = @"
                      SELECT 
                          P.ProductId, 
                          P.ProductName, 
                          P.CategoryId, 
                          C.CategoryName, 
                          P.ProductTypeId,
                          T.ProductTypeName, 
                          P.SellPrice, 
                          P.BuyPrice, 
                          P.MaxDiscount, 
                          P.DiscountsOn, 
                          P.Notes, 
                          P.MinQuantity,
                          COALESCE(SUM(BI.AvailableQty), 0) AS TotalQuantity
                      FROM 
                          Products P
                      INNER JOIN 
                          Categories C ON C.CategoryId = P.CategoryId
                      INNER JOIN 
                          ProductTypes T ON T.ProductTypeId = P.ProductTypeId
                      LEFT JOIN 
                          BranchInventory BI ON P.ProductId = BI.ProductId AND BI.BranchId = @BranchId
                      GROUP BY 
                          P.ProductId, 
                          P.ProductName, 
                          P.CategoryId, 
                          C.CategoryName, 
                          P.ProductTypeId,
                          T.ProductTypeName, 
                          P.SellPrice, 
                          P.BuyPrice, 
                          P.MaxDiscount, 
                          P.DiscountsOn, 
                          P.Notes, 
                          P.MinQuantity
                        HAVING P.MinQuantity > COALESCE(SUM(BI.AvailableQty), 0)";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductsModel>(sql, new { BranchId }).ConfigureAwait(false);
            }
        }

        //المقاسات المتاحة
        public async Task<IEnumerable<ProductsModel>> GetAvailableSizesForProduct(int productId)
        {
            var sql = @"SELECT DISTINCT
                    S.SizeId,
                    S.SizeName
                FROM BranchInventory BI
                LEFT JOIN Sizes S ON BI.SizeId = S.SizeId
                WHERE BI.ProductId = @ProductId
                AND BI.AvailableQty > 0;";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductsModel>(sql, new { ProductId = productId }).ConfigureAwait(false);
            }
        }

        //الالوان المتاحة
        public async Task<IEnumerable<ProductsModel>> GetAvailableColorsForProduct(int productId)
        {
            var sql = @"SELECT DISTINCT
                    Col.ColorId,
                    Col.ColorName
                FROM BranchInventory BI
                LEFT JOIN Colors Col ON BI.ColorId = Col.ColorId
                WHERE BI.ProductId = @ProductId
                AND BI.AvailableQty > 0;";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductsModel>(sql, new { ProductId = productId }).ConfigureAwait(false);
            }
        }

        //الكمية المتاحة
        public async Task<int> GetAvailableQuantity(int productId, int colorId, int sizeId, int branchId)
        {
            var sql = @"
                    SELECT AvailableQty AS AvailableQuantity FROM BranchInventory bi 
                    WHERE bi.ProductId = @ProductId AND bi.SizeId = @SizeId AND bi.ColorId = @ColorId AND bi.BranchId = @BranchId;";

            using (var connection = _dapperContext.CreateConnection())
            {
                var availableQuantity = await connection.QuerySingleOrDefaultAsync<int>(sql, new { ProductId = productId, ColorId = colorId, SizeId = sizeId, BranchId = branchId });
                return availableQuantity;
            }
        }

    }
}

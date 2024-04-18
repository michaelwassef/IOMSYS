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
                            P.UnitId,
                            P.MaxDiscount, 
                            P.DiscountsOn, 
                            P.Notes, 
                            P.MinQuantity,
                            COALESCE(SUM(BI.AvailableQty), 0) AS TotalQuantity,
                            COALESCE(si.TotalSoldQuantity, 0) AS TotalSoldQuantity,
                            COALESCE(si.TotalSellPrice, 0) AS TotalSellPrice,
                            P.WholesalePrice,
                            P.FabricQuantity,
                            U.UnitName
                        FROM 
                            Products P
                        INNER JOIN 
                            Categories C ON C.CategoryId = P.CategoryId
                        INNER JOIN 
                            ProductTypes T ON T.ProductTypeId = P.ProductTypeId
                        INNER JOIN 
                            Units U ON U.UnitId = P.UnitId
                        LEFT JOIN 
                            BranchInventory BI ON P.ProductId = BI.ProductId
                        LEFT JOIN 
                           (SELECT 
                               si.ProductId, 
                               SUM(si.Quantity) AS TotalSoldQuantity, 
                               SUM(si.SellPrice * si.Quantity) AS TotalSellPrice
                            FROM SalesItems si
                            INNER JOIN SalesInvoiceItems sii ON si.SalesItemId = sii.SalesItemId
                            GROUP BY si.ProductId) si ON P.ProductId = si.ProductId
                        GROUP BY 
                            P.ProductId, 
                            P.ProductName, 
                            P.CategoryId, 
                            C.CategoryName, 
                            P.ProductTypeId,
                            T.ProductTypeName, 
                            P.SellPrice, 
                            P.BuyPrice, 
                            P.UnitId,
                            P.MaxDiscount, 
                            P.DiscountsOn, 
                            P.Notes, 
                            P.MinQuantity,
                            P.WholesalePrice,
                            P.FabricQuantity,
                            si.TotalSoldQuantity,
                            si.TotalSellPrice,
                            U.UnitName
                        ORDER BY P.ProductName;";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductsModel>(sql).ConfigureAwait(false);
            }
        }

        public async Task<ProductsModel?> SelectProductByIdAsync(int productId)
        {
            var sql = @"SELECT P.ProductId, P.ProductName, C.CategoryName, P.ProductPhoto, T.ProductTypeName, P.SellPrice, P.BuyPrice, P.MaxDiscount, 
                        P.DiscountsOn, P.Notes, P.CategoryId, P.ProductTypeId, P.MinQuantity, P.UnitId, U.UnitName, P.WholesalePrice, P.FabricQuantity
                FROM Products P
                INNER JOIN Categories C ON C.CategoryId = P.CategoryId
                INNER JOIN ProductTypes T ON T.ProductTypeId = P.ProductTypeId
                INNER JOIN Units U ON U.UnitId = P.UnitId
            WHERE ProductId = @ProductId";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QuerySingleOrDefaultAsync<ProductsModel>(sql, new { ProductId = productId }).ConfigureAwait(false);
            }
        }

        public async Task<int> InsertProductAsync(ProductsModel product)
        {
            var getIdSql = @"SELECT ISNULL(MAX(ProductId), 0) + 1 FROM Products;";
            var insertSql = @"INSERT INTO Products (ProductId, ProductName, CategoryId, ProductPhoto, ProductTypeId, SellPrice, BuyPrice, MaxDiscount, DiscountsOn, Notes, MinQuantity, UnitId, WholesalePrice, FabricQuantity) 
                      VALUES (@ProductId, @ProductName, @CategoryId, @ProductPhoto, @ProductTypeId, @SellPrice, @BuyPrice, @MaxDiscount, @DiscountsOn, @Notes, @MinQuantity, @UnitId, @WholesalePrice, @FabricQuantity);";

            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    var newProductId = await db.ExecuteScalarAsync<int>(getIdSql).ConfigureAwait(false);
                    product.ProductId = newProductId;
                    await db.ExecuteAsync(insertSql, product).ConfigureAwait(false);
                    return newProductId;
                }
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpdateProductAsync(ProductsModel product)
        {
            var sql = @"UPDATE Products SET ProductName = @ProductName, CategoryId = @CategoryId, ProductPhoto = @ProductPhoto, ProductTypeId = @ProductTypeId, 
                        SellPrice = @SellPrice, BuyPrice = @BuyPrice, MaxDiscount = @MaxDiscount, DiscountsOn = @DiscountsOn, Notes = @Notes, MinQuantity = @MinQuantity, 
                        UnitId = @UnitId, WholesalePrice = @WholesalePrice, FabricQuantity = @FabricQuantity WHERE ProductId = @ProductId";
            try
            {
                using (var db = _dapperContext.CreateConnection())
                {
                    return await db.ExecuteAsync(sql, product ).ConfigureAwait(false);
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
                    SUM(BI.AvailableQty) AS TotalQuantity,
                    P.UnitId
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
                    T.ProductTypeName,
                    P.UnitId
                HAVING 
                    SUM(BI.AvailableQty) > 0
                ORDER BY P.ProductName;";

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
                  ,P.UnitId
            FROM BranchInventory BI
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
                  ,P.UnitId
            HAVING BI.BranchId = @BranchId AND COALESCE(SUM(BI.AvailableQty), 0)> 0";

            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductsModel>(sql, new { BranchId }).ConfigureAwait(false);
            }
        }

        //تحركات المخزن
        public async Task<IEnumerable<ProductsModel>> WarehouseMovementsAsync(int BranchId, DateTime fromdate, DateTime todate)
        {
            var sql = @"WITH CombinedData AS (
                          SELECT
                                    P.ProductId,
                                    CASE 
                                        WHEN PI.Statues = 0 THEN 'مشتريات'                            
                                        WHEN PI.Statues = 1 THEN 'تصنيع'
                                        WHEN PI.Statues = 2 THEN 'توالف'
                                        WHEN PI.Statues = 3 THEN 'انتاج المصنع'                            
                                        WHEN PI.Statues = 4 THEN 'منقول من فرع'
                                        WHEN PI.Statues = 5 THEN 'منتجات منقوله بفاتورة'
                                        ELSE 'غير محدد'
                                    END AS RecordType,
                                    P.ProductName,
                                    S.SizeName,
                                    C.ColorName,
                                    CAT.CategoryName,
                                    PT.ProductTypeName,
                                    PI.Quantity,
                                    ui.UnitName,
                                    b.BranchName,
                                    PI.ModDate AS DateAdded,
                                    PI.Notes AS Notes,
                                    u.UserName
                                FROM PurchaseItems PI
                                LEFT JOIN PurchaseInvoiceItems PII ON PI.PurchaseItemId = PII.PurchaseItemId
                                INNER JOIN Products P ON PI.ProductId = P.ProductId
                                INNER JOIN Sizes S ON PI.SizeId = S.SizeId
                                INNER JOIN Colors C ON PI.ColorId = C.ColorId
                                INNER JOIN Categories CAT ON P.CategoryId = CAT.CategoryId
                                INNER JOIN Units ui ON P.UnitId = ui.UnitId
                                INNER JOIN ProductTypes PT ON P.ProductTypeId = PT.ProductTypeId
                                INNER JOIN Branches b ON PI.BranchId = b.BranchId
                                INNER JOIN Users u ON PI.ModUser = u.UserId
                                WHERE PI.BranchId = @BranchId

                                UNION ALL

                      SELECT
                          SI.ProductId,
                          'مبيعات' AS RecordType,
                          P.ProductName,
                          S.SizeName,
                          C.ColorName,
                          CAT.CategoryName,
                          PT.ProductTypeName,
                          -SI.Quantity AS Quantity,
                          ui.UnitName,
                          b.BranchName,
                          SI.ModDate AS DateAdded,
                          NULL AS Notes,
                          u.UserName
                      FROM SalesItems SI
                      INNER JOIN Products P ON SI.ProductId = P.ProductId
                      INNER JOIN Sizes S ON SI.SizeId = S.SizeId
                      INNER JOIN Colors C ON SI.ColorId = C.ColorId
                      INNER JOIN Categories CAT ON P.CategoryId = CAT.CategoryId
                      INNER JOIN Units ui ON P.UnitId = ui.UnitId
                      INNER JOIN ProductTypes PT ON P.ProductTypeId = PT.ProductTypeId
                      INNER JOIN Branches b ON SI.BranchId = b.BranchId
                      INNER JOIN Users u ON SI.ModUser = u.UserId
                      WHERE SI.BranchId = @BranchId
                  
                      UNION ALL
                  
                      SELECT
                          SI.ProductId,
                          'مرتجعات' AS RecordType,
                          P.ProductName,
                          S.SizeName,
                          C.ColorName,
                          CAT.CategoryName,
                          PT.ProductTypeName,
                          SI.Quantity AS Quantity,
                          ui.UnitName,
                          b.BranchName,
                          SI.ReturnDate AS DateAdded,
                          NULL AS Notes,
                          u.UserName
                      FROM SalesItems SI
                      INNER JOIN Products P ON SI.ProductId = P.ProductId
                      INNER JOIN Sizes S ON SI.SizeId = S.SizeId
                      INNER JOIN Colors C ON SI.ColorId = C.ColorId
                      INNER JOIN Categories CAT ON P.CategoryId = CAT.CategoryId
                      INNER JOIN Units ui ON P.UnitId = ui.UnitId
                      INNER JOIN ProductTypes PT ON P.ProductTypeId = PT.ProductTypeId
                      INNER JOIN Branches b ON SI.BranchId = b.BranchId
                      INNER JOIN Users u ON SI.ModUser = u.UserId
                      WHERE SI.BranchId = @BranchId AND SI.IsReturn = 1

                                UNION ALL

                                SELECT
                                    P.ProductId,
                                    CASE 
                                        WHEN IM.ToBranchId = @BranchId AND IM.FromBranchId IS NOT NULL AND IM.MakeInvoice = 1 THEN 'منتجات منقوله للفرع بفاتورة'
                                        WHEN IM.ToBranchId = @BranchId AND IM.FromBranchId IS NOT NULL AND IM.MakeInvoice = 0 THEN 'منتجات منقوله للفرع بدون فاتورة'
                                        WHEN IM.FromBranchId = @BranchId AND IM.MakeInvoice = 1 THEN 'منتجات منقوله من الفرع بفاتورة'
                                        WHEN IM.FromBranchId = @BranchId THEN 'منتجات منقوله من الفرع بدون فاتورة'
                                        WHEN IM.SalesInvoiceId IS NOT NULL THEN 'مبيعات'
                                        WHEN IM.PurchaseInvoiceId IS NOT NULL THEN 'مشتريات'
                                        WHEN IM.MakeInvoice = 1 THEN 'تصنيع'
                                        ELSE 'غير محدد'
                                    END AS RecordType,
                                    P.ProductName,
                                    S.SizeName,
                                    C.ColorName,
                                    CAT.CategoryName,
                                    PT.ProductTypeName,
                                    CASE 
                                        WHEN IM.ToBranchId = @BranchId THEN IM.Quantity
                                        WHEN IM.FromBranchId = @BranchId THEN -IM.Quantity
                                        ELSE 0
                                    END AS Quantity,
                                    ui.UnitName,
                                    CASE 
                                        WHEN IM.ToBranchId = @BranchId THEN (SELECT BranchName FROM Branches WHERE BranchId = IM.ToBranchId)
                                        WHEN IM.FromBranchId = @BranchId THEN (SELECT BranchName FROM Branches WHERE BranchId = IM.FromBranchId)
                                        ELSE 'Unknown'
                                    END AS BranchName,
                                    IM.MovementDate AS DateAdded,
                                    IM.Notes AS Notes,
                                    u.UserName
                                FROM InventoryMovements IM
                                INNER JOIN Products P ON IM.ProductId = P.ProductId
                                INNER JOIN Sizes S ON IM.SizeId = S.SizeId
                                INNER JOIN Colors C ON IM.ColorId = C.ColorId
                                INNER JOIN Categories CAT ON P.CategoryId = CAT.CategoryId
                                INNER JOIN ProductTypes PT ON P.ProductTypeId = PT.ProductTypeId
                                INNER JOIN Units ui ON P.UnitId = ui.UnitId
                                INNER JOIN Users u ON IM.ModUser = u.UserId
                                WHERE IM.IsApproved = 1
                                AND ((IM.ToBranchId = @BranchId AND IM.MakeInvoice = 0) OR IM.FromBranchId = @BranchId)
                    )
                    SELECT *
                    FROM CombinedData WHERE DateAdded >= @fromdate AND DateAdded <= @todate
                    ORDER BY DateAdded DESC;";
            using (var db = _dapperContext.CreateConnection())
            {
                return await db.QueryAsync<ProductsModel>(sql, new { BranchId, fromdate, todate }).ConfigureAwait(false);
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
                          P.UnitId,
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
                          P.UnitId,
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
        public async Task<decimal> GetAvailableQuantity(int productId, int colorId, int sizeId, int branchId)
        {
            var sql = @"
                    SELECT AvailableQty AS AvailableQuantity FROM BranchInventory bi 
                    WHERE bi.ProductId = @ProductId AND bi.SizeId = @SizeId AND bi.ColorId = @ColorId AND bi.BranchId = @BranchId;";

            using (var connection = _dapperContext.CreateConnection())
            {
                var availableQuantity = await connection.QuerySingleOrDefaultAsync<decimal>(sql, new { ProductId = productId, ColorId = colorId, SizeId = sizeId, BranchId = branchId });
                return availableQuantity;
            }
        }

        public async Task<string> GetWhereAvailableQuantityAsync(int productId, int colorId, int sizeId)
        {
            var sql = @"
                        SELECT B.BranchName, bi.AvailableQty AS AvailableQuantity 
                        FROM BranchInventory bi 
                        INNER JOIN Branches B ON bi.BranchId = B.BranchId
                        WHERE bi.ProductId = @ProductId AND bi.SizeId = @SizeId AND bi.ColorId = @ColorId AND bi.AvailableQty > 0;";

            using (var connection = _dapperContext.CreateConnection())
            {
                var results = await connection.QueryAsync(sql, new { ProductId = productId, ColorId = colorId, SizeId = sizeId });
                var availabilityInfo = results.Select(result => $"متوفر بفرع {result.BranchName}: {result.AvailableQuantity} ").ToList();
                var availabilityText = string.Join(Environment.NewLine, availabilityInfo);
                return availabilityText;
            }
        }

    }
}

﻿using IOMSYS.Models;

namespace IOMSYS.IServices
{
    public interface IPurchaseItemsService
    {
        Task<IEnumerable<PurchaseItemsModel>> GetAllPurchaseItemsAsync();
        Task<PurchaseItemsModel> GetPurchaseItemByIdAsync(int purchaseItemId);
        Task<int> InsertPurchaseItemAsync(PurchaseItemsModel purchaseItem);
        Task<int> UpdatePurchaseItemAsync(PurchaseItemsModel purchaseItem);
        Task<int> DeletePurchaseItemAsync(int purchaseItemId);
    }
}
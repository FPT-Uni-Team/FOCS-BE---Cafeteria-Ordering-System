
using FOCS.Common.Models.CartModels;

namespace FOCS.Common.Interfaces
{
    public interface ICartService
    {
        // <summary>
        /// Thêm hoặc cập nhật món trong cart (Redis) theo bàn và user/guest.
        /// </summary>
        Task AddOrUpdateItemAsync(Guid tableId, string actorId, CartItemRedisModel item, string storeId);

        /// <summary>
        /// Xoá 1 món trong cart
        /// </summary>
        Task RemoveItemAsync(Guid tableId, string actorId, string storeId, Guid menuItemId, Guid? variantId, int quantity);

        /// <summary>
        /// Lấy toàn bộ cart hiện tại
        /// </summary>
        Task<List<CartItemRedisModel>> GetCartAsync(Guid tableId, string storeId, string actorId);

        /// <summary>
        /// Xoá toàn bộ cart (sau khi checkout)
        /// </summary>
        Task ClearCartAsync(Guid tableId, string storeId, string actorId);

        /// <summary>
        /// Tạo Redis key dựa theo actor (user hoặc guest) + table
        /// </summary>
        string GetCartKey(Guid tableId, string storeId, string actorId);
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace OceanOfTheSea.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CartRepository(ApplicationDbContext db,IHttpContextAccessor httpContextAccessor ,UserManager<IdentityUser> userManager) 
        {
          _db = db;
          _userManager = userManager;
          _httpContextAccessor = httpContextAccessor;
        }
        public async Task<int> AddItem(int menuId, int qty)
        {
            string userId = GetUserId();
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("user is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _db.ShoppingCarts.Add(cart);
                }
                _db.SaveChanges();
                //cart detail section
                var cartItem = _db.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.MenuId == menuId);
                if (cartItem is not  null) 
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var menu = _db.Menus.Find(menuId);
                    cartItem = new CartDetail
                    {
                        MenuId = menuId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = menu.Price
                    };
                    _db.CartDetails.Add(cartItem);
                }
                _db.SaveChanges();
                transaction.Commit();
            }
            catch(Exception ex)
            {
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
          }
        public async Task<int> RemoveItem(int menuId)
        {
            //using var transaction = _db.Database.BeginTransaction();
            string userId = GetUserId();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("user is not logged-in");
                var cart = await GetCart(userId);
                if(cart is null)
                    throw new InvalidOperationException("Invalid cart");
                //cart detail section
                var cartItem = _db.CartDetails
                .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.MenuId == menuId);
                if (cartItem is null)
                    throw new InvalidOperationException("Not items in cart");
                else if (cartItem.Quantity == 1)
                    _db.CartDetails.Remove(cartItem);
                else
                    cartItem.Quantity = cartItem.Quantity - 1;
                _db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }
        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();
            if(userId == null)
                throw new InvalidOperationException("Invalid userid");
            var shoppingCart = await _db.ShoppingCarts
                                   .Include(a => a.CartDetails)
                                   .ThenInclude(a => a.Menu)
                                   .ThenInclude(a => a.Genre)
                                   .Where(a => a.UserId == userId).FirstOrDefaultAsync();
            return shoppingCart;
        }
        public async Task<ShoppingCart> GetCart(string userId)
        {
            var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
            return cart;
        
        }
        public async Task<int> GetCartItemCount(string userId = "")
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }
            var data = await (from cart in _db.ShoppingCarts
                              join cartDetail in _db.CartDetails
                              on cart.Id equals cartDetail.ShoppingCartId
                              where cart.UserId == userId
                              select new { cartDetail.Id }
                        ).ToListAsync();
            return data.Count;
        }
        public async Task<bool> DoCheckout(CheckoutModel model)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User is not logged-in");

                var cart = await GetCart(userId);
                if (cart is null)
                    throw new InvalidOperationException("Invalid cart");

                var cartDetail = await _db.CartDetails
                    .Where(a => a.ShoppingCartId == cart.Id)
                    .ToListAsync();
                if (cartDetail.Count == 0)
                    throw new InvalidOperationException("Cart is empty");

                var pendingRecord = await _db.OrderStatuses
                    .FirstOrDefaultAsync(s => s.Status == "Pending");
                if (pendingRecord is null)
                    throw new InvalidOperationException("Order status does not have Pending status");

                var order = new Order
                {
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow,
                    Name = model.Name,
                    Email = model.Email,
                    MobileNumber = model.MobileNumber,
                    PaymentMethod = model.PaymentMethod,
                    IsPaid = false,
                    OrderStatusId = pendingRecord.Id,
                };

                _db.orders.Add(order);
                _db.OrderDetails.AddRange(cartDetail.Select(item => new OrderDetail
                {
                    MenuId = item.MenuId,
                    OrderId = order.Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                }));

                _db.CartDetails.RemoveRange(cartDetail);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Checkout failed: {ex.Message}");
                return false;
            }
        }
        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            string userId = _userManager.GetUserId(principal);
            return userId; 
        }
    }
}
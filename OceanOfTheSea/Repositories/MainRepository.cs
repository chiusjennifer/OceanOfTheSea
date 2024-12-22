using Microsoft.EntityFrameworkCore;
using OceanOfTheSea.Data;
using OceanOfTheSea.Models;

namespace OceanOfTheSea.Repositories
{
    public class MainRepository : IMainRepository
    {
        private readonly ApplicationDbContext _db;

        public MainRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Genre>> Genres()
        {
            return await _db.Genres.ToListAsync();
        }
        public async Task<IEnumerable<Menu>> GetMenu(int genreId = 0)
        {
            var menus = await (from menu in _db.Menus
                               join genre in _db.Genres on menu.GenreId equals genre.Id
                               where genreId == 0 || menu.GenreId == genreId
                               select new Menu
                               {
                                   Id = menu.Id,
                                   DishName = menu.DishName,
                                   Price = menu.Price,
                                   GenreId = menu.GenreId
                               }).ToListAsync();
            return menus;
        }
    }

}

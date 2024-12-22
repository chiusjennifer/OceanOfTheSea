using OceanOfTheSea.Models;

namespace OceanOfTheSea
{
    public interface IMainRepository
    {
        Task<IEnumerable<Menu>> GetMenu(int genreId = 0);
        Task<IEnumerable<Genre>> Genres();
    }
}
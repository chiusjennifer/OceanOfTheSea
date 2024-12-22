using Microsoft.AspNetCore.Mvc;
using OceanOfTheSea.Models.DTOs;
using System.IO;
using static System.Reflection.Metadata.BlobBuilder;

namespace OceanOfTheSea.Controllers
{
    public class MenuController : Controller
    {
        private readonly IMainRepository _mainRepository;

        public MenuController(IMainRepository mainRepository)
        {
            _mainRepository = mainRepository;
        }

        public async Task<IActionResult> Index(int genreId = 0)
        {
            IEnumerable<Menu> menus = await _mainRepository.GetMenu(genreId);
            IEnumerable<Genre> genres = await _mainRepository.Genres();
            MenuDisplayModel menuModel = new MenuDisplayModel
            {
                Menus = menus,
                Genres = genres,
                GenreId = genreId
            };
            return View(menuModel);
        }
    }
}

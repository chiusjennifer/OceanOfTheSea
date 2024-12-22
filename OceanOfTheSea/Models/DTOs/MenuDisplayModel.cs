namespace OceanOfTheSea.Models.DTOs
{
    public class MenuDisplayModel
    {
        public IEnumerable<Menu> Menus { get; set; }
        public IEnumerable<Genre> Genres { get; set; }
        public int GenreId { get; set; } = 0;
    }
}

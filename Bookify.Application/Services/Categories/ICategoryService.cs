namespace Bookify.Application.Services
{
    public interface ICategoryService
    {
        public Category? GetById(int id);
        public IEnumerable<Category> GetAllWithNoTracking();
        public Category Add(Category category, string createdById);
        public Category? Update(int id, string lastUpdatedById);
        public bool AllowedItem(int id, string name);
        public Category? ToggleStatus(int id, string lastUpdatedById);
    }
}

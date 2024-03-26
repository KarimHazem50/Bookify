namespace Bookify.Application.Services
{
    internal class CategoryService : ICategoryService
    {
        private readonly IUnitOFWork _unitOFWork;

        public CategoryService(IUnitOFWork unitOFWork)
        {
            _unitOFWork = unitOFWork;
        }

        public Category? GetById(int id)
        {
            return _unitOFWork.Categories.GetById(id);
        }
        public IEnumerable<Category> GetAllWithNoTracking()
        {
            return _unitOFWork.Categories.GetAll(withNoTracking: false);
        }
        public Category Add(Category category, string createdById)
        {
            category.CreatedById = createdById;
            _unitOFWork.Categories.Add(category);
            _unitOFWork.Complete();
            return category;
        }
        public Category? Update(int id, string lastUpdatedById)
        {
            var category = GetById(id);

            if (category is null)
                return null;

            category.LastUpdatedOn = DateTime.Now;
            category.LastUpdatedById = lastUpdatedById;
            _unitOFWork.Complete();

            return category;
        }
        public bool AllowedItem(int id, string name)
        {
            var category = _unitOFWork.Categories.Find(c => c.Name == name);
            return category is null || category.Id.Equals(id);
        }
        public Category? ToggleStatus(int id, string lastUpdatedById)
        {
            var category = GetById(id);

            if (category is null)
                return null;

            category.IsDeleted = !category.IsDeleted;
            category.LastUpdatedOn = DateTime.Now;
            category.LastUpdatedById = lastUpdatedById;
            _unitOFWork.Complete();

            return category;
        }
    }
}

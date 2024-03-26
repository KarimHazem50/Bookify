namespace Bookify.Application.Services
{
    internal class AuthorService : IAuthorService
    {
        private readonly IUnitOFWork _unitOFWork;

        public AuthorService(IUnitOFWork unitOFWork)
        {
            _unitOFWork = unitOFWork;
        }

        public IEnumerable<Author> GetAll()
        {
            return _unitOFWork.Authors.GetAll(withNoTracking: false);
        }

        public Author? GetById(int id)
        {
            return _unitOFWork.Authors.GetById(id);
        }

        public Author Add(string name, string createdById)
        {
            var author = new Author
            {
                Name = name,
                CreatedById = createdById
            };

            _unitOFWork.Authors.Add(author);
            _unitOFWork.Complete();

            return author;
        }

        public Author? Update(int id, string name, string updatedById)
        {
            var author = _unitOFWork.Authors.GetById(id);

            if (author is null)
                return null;

            author.Name = name;
            author.LastUpdatedOn = DateTime.Now;
            author.LastUpdatedById = updatedById;
            _unitOFWork.Complete();

            return author;
        }

        public Author? ToggleStatus(int id, string updatedById)
        {
            var author = _unitOFWork.Authors.GetById(id);

            if (author is null)
                return null;

            author.IsDeleted = !author.IsDeleted;
            author.LastUpdatedOn = DateTime.Now;
            author.LastUpdatedById = updatedById;
            _unitOFWork.Complete();

            return author;
        }

        public bool AllowAuthor(int id, string name)
        {
            var author = _unitOFWork.Authors.Find(predicate: a => a.Name == name);
            return author is null || author.Id.Equals(id);
        }
    }
}

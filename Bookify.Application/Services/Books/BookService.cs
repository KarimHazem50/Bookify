using Bookify.Domain.Consts;
using System.Linq.Dynamic.Core;
namespace Bookify.Application.Services
{
    internal class BookService : IBookService
    {
        private readonly IUnitOFWork _unitOFWork;

        public BookService(IUnitOFWork unitOFWork)
        {
            _unitOFWork = unitOFWork;
        }

        public Book? GetById(int id)
        {
            return _unitOFWork.Books.GetById(id);
        }
        public Book? GetWithCategories(int id)
        {
            return _unitOFWork.Books.Find(predicate: b => b.Id == id, include: b => b.Include(x => x.Categories));
        }
        public Book? GetWithCategoriesAndCopies(int id)
        {
            return _unitOFWork.Books.Find(predicate: b => b.Id == id, include: b => b.Include(x => x.Categories).Include(b => b.BookCopies));
        }
        public IEnumerable<Author> GetAuthors()
        {
            return _unitOFWork.Authors.FindAll(predicate: a => !a.IsDeleted, orderBy: a => a.Name, orderByDirection: OrderBy.Ascending);
        }
        public IEnumerable<Category> GetCategories()
        {
            return _unitOFWork.Categories.FindAll(predicate: c => !c.IsDeleted, orderBy: c => c.Name, orderByDirection: OrderBy.Ascending);
        }
        public (IQueryable<Book> books, int count) GetFiltered(FiltrationDto dto)
        {
            var books = _unitOFWork.Books.GetBooks();

            if (!string.IsNullOrEmpty(dto.SearchValue))
                books = books.Where(b => b.Title.Contains(dto.SearchValue!) || b.Authors!.Name.Contains(dto.SearchValue!));

            books = books.OrderBy($"{dto.SortColumnName} {dto.SortColumnDirection}").Skip(dto.Skip).Take(dto.PageSize);

            int recordsTotal = _unitOFWork.Books.Count();

            return (books, count: recordsTotal);
        }
        public IQueryable<Book> GetDetails()
        {
            return _unitOFWork.Books.GetDetails();
        }
        public IEnumerable<BookDto> GetTopBooks(int numberOfBooks)
        {
            var query = _unitOFWork.RentalCopies.GetQueryable();

            return query.Include(c => c.BookCopy).ThenInclude(c => c!.Book).ThenInclude(b => b!.Authors)
                    .GroupBy(rc => new
                    {
                        rc.BookCopy!.BookId,
                        rc.BookCopy.Book!.Title,
                        rc.BookCopy.Book.ImageName,
                        AuthorName = rc.BookCopy.Book.Authors!.Name
                    })
                    .Select(b => new
                    {
                        b.Key.BookId,
                        b.Key.Title,
                        b.Key.ImageName,
                        b.Key.AuthorName,
                        Count = b.Count()
                    })
                    .OrderByDescending(b => b.Count)
                    .Take(numberOfBooks)
                    .Select(b => new BookDto(
                        b.BookId,
                        b.Title,
                        b.ImageName,
                        b.AuthorName
                    ))
                    .ToList();
        }
        public Book Add(Book book, IEnumerable<int> selectedCategories, string createdById)
        {
            foreach (var category in selectedCategories)
            {
                book.Categories.Add(new BookCategory { CategoryId = category });
            }

            book.CreatedById = createdById;
            _unitOFWork.Books.Add(book);
            _unitOFWork.Complete();

            return book;
        }
        public Book Update(Book book, IEnumerable<int> selectedCategories, string updatedById)
        {
            book.LastUpdatedOn = DateTime.Now;
            book.LastUpdatedById = updatedById;

            book.Categories.Clear();
            foreach (var category in selectedCategories)
            {
                book.Categories.Add(new BookCategory { CategoryId = category });
            }

            if (!book.IsAvailableForRental)
            {
                foreach (var copy in book.BookCopies)
                {
                    copy.IsAvailableForRental = false;
                }
            }

            _unitOFWork.Complete();

            return book;
        }
        public bool AllowedItem(string title, int id, int authorId)
        {
            var book = _unitOFWork.Books.Find(predicate: b => b.Title == title && b.AuthorId == authorId);
            return book is null || book.Id.Equals(id);
        }
        public Book? ToggleStatus(int id, string lastUpdatedById)
        {
            var book = GetById(id);

            if (book is null)
                return null;

            book.IsDeleted = !book.IsDeleted;
            book.LastUpdatedOn = DateTime.Now;
            book.LastUpdatedById = lastUpdatedById;

            _unitOFWork.Complete();
            return book;
        }
        public IEnumerable<Book> GetLastAddedBooks(int numberOfBooks)
        {
            return _unitOFWork.Books.FindAll(
                predicate: b => !b.IsDeleted,
                include: b => b.Include(x => x.Authors!),
                orderBy: b => b.Id,
                orderByDirection: OrderBy.Descending)
                .Take(numberOfBooks);
        }
        public IQueryable<Book> Search(string value)
        {
            return _unitOFWork.Books.GetQueryable().Include(b => b.Authors).Where(b => !b.IsDeleted && (b.Title.Contains(value) || b.Authors!.Name.Contains(value)));
        }
        public int GetActiveBooksCount()
        {
            return _unitOFWork.Books.Count(c => !c.IsDeleted);
        }
    }
}

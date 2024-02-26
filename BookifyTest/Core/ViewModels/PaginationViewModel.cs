namespace BookifyTest.Core.ViewModels
{
    public class PaginationViewModel
    {
        public int PageNumber { get;  set; }
        public int TotalPages { get;  set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => TotalPages > PageNumber;

        public int Start 
        { 
            get
            {
                var start = 1;
                var maxPages = (int)ReportsConfigurations.MaxShowNumbersPages;

                if (TotalPages > maxPages)
                    start = PageNumber;

                if(TotalPages - PageNumber < maxPages && (TotalPages - maxPages > 1))
                    start = TotalPages - maxPages;

                return start;
            } 
        }
        public int End
        {
            get
            {
                var end = TotalPages;
                var maxPages = (int)ReportsConfigurations.MaxShowNumbersPages;

                if (TotalPages > maxPages && (Start + maxPages - 1) < TotalPages)
                    end = Start + maxPages - 1;

                return end;
            }
        }
    }
}

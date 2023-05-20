using System.Collections;

namespace Bookify.Web.Helpers
{
    public class PagedList<T> : IReadOnlyList<T>
    {
        private readonly IList<T> _sublist;

        public PagedList(IEnumerable<T> items,int count, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count/ (double)pageSize);
            _sublist = items as IList<T> ?? new List<T>(items);
        }
        public int PageNumber { get; private set; }
        public int TotalPages { get; private set; }
        public bool IsFirstPage => PageNumber == 1;
        public bool IsLastPage => PageNumber == TotalPages;
        public int Count => _sublist.Count;
        public T this[int index] => _sublist[index];
        public IEnumerator<T> GetEnumerator() => _sublist.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _sublist.GetEnumerator();
    }
}

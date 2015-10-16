using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Library.Models.Custom
{
    public class BooksViewModel
    {
        public List<BookViewModel> Books { get; set; }
        public PaginatorViewModel Paginator { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Library.Models.Custom
{
    public class BookViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Author> Authors { get; set; }
        public int Quantity { get; set; }
        public IEnumerable<Journal> History { get; set; }
        public bool isAlreadyTaken { get; set; }
    } 
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Library.Models.Custom
{
    public class PaginatorViewModel
    {
        public int CurrentPage { get; private set; }
        public int ItemsPerPage { get; private set; }
        public int ItemsCount { get; private set; }
        public bool isFirstPage { get; private set; }
        public bool isLastPage { get; private set; }

        public PaginatorViewModel(int currentPage, int itemsPerPage, int itemsCount)
        {
            this.CurrentPage = currentPage;
            this.ItemsPerPage = itemsPerPage;
            this.ItemsCount = itemsCount;
            this.isFirstPage = this.CurrentPage == 1;
            this.isLastPage = this.ItemsCount <= this.CurrentPage * this.ItemsPerPage;
        }
    }
}
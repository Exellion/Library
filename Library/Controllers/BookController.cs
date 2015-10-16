using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using Library.Models;
using Library.Models.Custom;
using Microsoft.AspNet.Identity.Owin;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Controllers
{
    public class BookController : Controller
    {

        LibraryDataContext db = new LibraryDataContext();

        public ActionResult ShowBooks()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> TableBooks(int currentPage, int itemsPerPage, string sortBy, bool availableFilter, bool userBookFilter)
        {
            IEnumerable<Book> books = null;
            BooksViewModel result = null;
            try
            {
                if (availableFilter && userBookFilter)
                    throw new Exception("Filter error");
                if (availableFilter)
                    books = await Task.Run(() => db.Book.Where(b => b.Quantity > 0 && b.Journal.Where(j => j.UserName.Name == User.Identity.Name && j.DateIn == null).Count() == 0));
                if (userBookFilter)
                    books = await Task.Run(() => db.Book.Where(b => b.Journal.Where(j => j.UserName.Name == User.Identity.Name && j.DateIn == null).Count() > 0));
                books = SortBy(books ?? await Task.Run(() => db.Book), sortBy);
                int booksCount = books.Count();
                books = books.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage);
                List<BookViewModel> booksView = new List<BookViewModel>();
                foreach (var book in books)
                {
                    booksView.Add(new BookViewModel
                    {
                        Id = book.Id,
                        Name = book.Name,
                        Authors = book.AuthorToBook.Select(ab => ab.Author),
                        Quantity = book.Quantity
                    });
                }
                result = new BooksViewModel
                {
                    Books = booksView,
                    Paginator = new PaginatorViewModel(currentPage, itemsPerPage, booksCount)
                };
            }
            catch(Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, RouteData.Values["controller"].ToString(), RouteData.Values["action"].ToString()));
            }
            return PartialView("/Views/Book/_TableBooks.cshtml", result);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> ShowBookInfo(int bookId)
        {
            BookViewModel result = null;
            try
            {
                Book book = await Task.Run(() => db.Book.Where(b => b.Id == bookId).Single());
                result = new BookViewModel
                {
                    Id = book.Id,
                    Name = book.Name,
                    Authors = book.AuthorToBook.Select(ab => ab.Author),
                    Quantity = book.Quantity,
                    History = book.Journal,
                    isAlreadyTaken = book.Journal.Where(j => j.UserName.Name == User.Identity.Name && j.DateIn == null).Count() > 0
                };           
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, RouteData.Values["controller"].ToString(), RouteData.Values["action"].ToString()));
            }
            return View("/Views/Book/BookInfo.cshtml", result);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetBook(int bookId)
        {
            try
            {
                Book book = await Task.Run(() => db.Book.Where(b => b.Id == bookId).Single());
                UserName user = await Task.Run(() => db.UserName.Where(u => u.Name == User.Identity.Name).Single());
                if (book.Quantity > 0)
                    book.Quantity--;
                else
                    throw new Exception(string.Format("{0} is not available at this time", book.Name));
                await Task.Run(() => db.Journal.InsertOnSubmit(new Journal()
                {
                    BookId = bookId,
                    UserId = user.Id,
                    DateOut = DateTime.Now
                }));
                await Task.Run(() => db.SubmitChanges());
                Task.Run(() => SendMail(user, book));
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, RouteData.Values["controller"].ToString(), RouteData.Values["action"].ToString()));
            }
            return RedirectToAction("ShowBookInfo", new { bookId = bookId });
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> ReturnBook(int bookId)
        {
            try
            {
                Book book = await Task.Run(() => db.Book.Where(b => b.Id == bookId).Single());
                Journal item = book.Journal.Where(j => j.UserName.Name == User.Identity.Name && j.DateIn == null).Single();
                book.Quantity++;
                item.DateIn = DateTime.Now;
                await Task.Run(() => db.SubmitChanges());
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, RouteData.Values["controller"].ToString(), RouteData.Values["action"].ToString()));
            }
            return RedirectToAction("ShowBookInfo", new { bookId = bookId });
        }

        private IEnumerable<Book> SortBy(IEnumerable<Book> books, string sortBy)
        {
            switch(sortBy)
            {
                case "name":
                    return books.OrderBy(b => b.Name);
                case "author":
                    return books.OrderBy(b =>
                    {
                        string fullName = "";
                        foreach (var author in db.Author.Where(a => a.AuthorToBook.Select(ab => ab.BookId).Contains(b.Id)))
                            fullName += author.FirstName + author.LastName;
                        return fullName;
                    });
                case "default":
                    return books.OrderBy(b => b.Id);
                default:
                    return books;
            }
        }

        private void SendMail(UserName user, Book book)
        {
            MailAddress from = new MailAddress("library_app@mail.ua", "Library");
            MailAddress to = new MailAddress(user.Email);
            MailMessage msg = new MailMessage(from, to);
            msg.Subject = "You have a new book";
            msg.Body = string.Format("{0}, you are successfully taken a book called \"{1}\"", user.Name, book.Name);
            SmtpClient smtp = new SmtpClient();
            smtp.Send(msg);
        }
    }
}
using System;
using PHttp.Application;
using UrlShortener.DAL.Repositories;
using UrlShortener.Models;

namespace UrlShortener.Controllers
{
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            if (AuthenticatedUser != null)
            {
                return View("Home");
            }

            ViewBag.Title = "Login";


            return View();
        }


        [HttpPost]
        public ActionResult Index(UserModel user)
        {

            ViewBag.Title = "Login";
            var repo = new UserRepository();

            if (user == null)
            {
                return View(StatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(user.Username))
            {
                ViewBag.InvalidUser = true;
                ViewBag.InvalidUserMessage = "The username is required";
                return View(user);
            }

            if (string.IsNullOrEmpty(user.Password))
            {
                ViewBag.InvalidPassword = true;
                ViewBag.InvalidPasswordMessage = "The password is required";
                return View(user);
            }

            var result = repo.Authenticate(user.Username, user.Password);
            try
            {
                if (result == UserRepository.AuthenticationResult.Success)
                {
                    SetAuthenticatedUser(new AuthenticatedUser {Username = user.Username}, true);

                    return View("Home");
                }
                else
                {
                    if (result == UserRepository.AuthenticationResult.InvalidUsername)
                    {
                        ViewBag.InvalidUser = true;
                        ViewBag.InvalidUserMessage = "This user does not exist";
                    }
                    if (result == UserRepository.AuthenticationResult.InvalidPassword)
                    {
                        ViewBag.InvalidPassword = true;
                        ViewBag.InvalidPasswordMessage = "The password you entered is incorrect";
                    }
                    return View(user);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            return null;
        }
    }
}

using PHttp.Application;
using UrlShortener.DAL;
using UrlShortener.DAL.Models;
using UrlShortener.DAL.Repositories;
using UrlShortener.Models;

namespace UrlShortener.Controllers
{
    public class SignUpController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Sign Up";
            return View();
        }

        [HttpPost]
        public ActionResult Index(UserModel userModel)
        {
            ViewBag.Title = "Sign Up";
            var userRepository = new UserRepository();

            if (userModel == null)
            {
                return View(StatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(userModel.Username))
            {
                ViewBag.InvalidUser = true;
                ViewBag.InvalidUserMessage = "The username is required";
                return View(userModel);
            }
            if (string.IsNullOrEmpty(userModel.Password))
            {
                ViewBag.InvalidPassword = true;
                ViewBag.InvalidPasswordMessage = "The password is required";
                return View(userModel);
            }
            if (string.IsNullOrEmpty(userModel.PasswordConfirm))
            {
                ViewBag.InvalidPasswordConfirm = true;
                ViewBag.InvalidPasswordConfirmMessage = "The password confirmation is required";
                return View(userModel);
            }

            //Validate

            var isValid = true;
            //User exists
            var result = userRepository.GetByUserNameWithoutPasswordField(userModel.Username);
            if (result != null)
            {
                ViewBag.InvalidUser = true;
                ViewBag.InvalidUserMessage = "The username is already registered";
                isValid = false;
            }

            //password with more than 4 chars
            if (userModel.Password.Length < 5)
            {
                ViewBag.InvalidPassword = true;
                ViewBag.InvalidPasswordMessage = "The password must contains at least 5 characters";
                isValid = false;
            }

            //passwords match
            if (userModel.Password != userModel.PasswordConfirm)
            {
                ViewBag.InvalidPasswordConfirm = true;
                ViewBag.InvalidPasswordConfirmMessage = "The password confirmation doesn't match";
                isValid = false;
            }

            if (!isValid) return View(userModel);

            var user = new User();
            user.Password = userModel.Password;
            user.Username = userModel.Username;

            userRepository.Save(user);

            //Login
            SetAuthenticatedUser(new AuthenticatedUser { Username = user.Username }, true);
            return View("Home");

        }
    }
}

using PHttp.Application;

namespace UrlShortener.Controllers
{
    public class SignOutController : Controller
    {
        public ActionResult Index()
        {
            if (AuthenticatedUser != null)
            {
                RemoveAuthenticatedUser();
            }

            return View("Home");
        }
    }
}

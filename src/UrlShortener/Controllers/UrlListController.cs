using System;
using System.Collections.Generic;
using PHttp.Application;
using UrlShortener.DAL.Repositories;
using UrlShortener.Models;

namespace UrlShortener.Controllers
{
    public class UrlListController : Controller
    {
        public ActionResult Index()
        {
            if (AuthenticatedUser == null) return View("Home");
            ViewBag.Title = "Url List";

            //Check authenticated user
            var userRepository = new UserRepository();
            var authenticatedUser = userRepository.GetByUserNameWithoutPasswordField(AuthenticatedUser.Username);

            //Remove authenticated user information
            if (authenticatedUser == null) return View("SignOut");


            var urlRepository = new UrlRepository();

            var userUrls = urlRepository.GetNonDeletedByUserId(authenticatedUser.Id);

            var urlDisplayInformation = new List<UserUrlModel>();
            var siteUrl = "http://" + GetSiteUrl();
            var clickRepository = new ClickRepository();

            foreach (var userUrl in userUrls)
            {
                var userUrlModel = new UserUrlModel();

                userUrlModel.OriginalUrl = userUrl.OriginalUrl;
                userUrlModel.ShortenedUrl = siteUrl + userUrl.ShortenedSuffix;
                userUrlModel.ClickNumbers = clickRepository.GetNumberByUrlId(userUrl.Id);

                userUrlModel.CreationDateString = userUrl.CreationDate >= DateTime.Today.AddDays(-20)
                    ? (int)(DateTime.Today - userUrl.CreationDate).TotalDays + " days ago."
                    : userUrl.CreationDate.ToShortDateString();

                userUrlModel.Suffix = userUrl.ShortenedSuffix;

                urlDisplayInformation.Add(userUrlModel);
            }


            ViewBag.UrlDisplayInformation = urlDisplayInformation;


            return View();
        }

        public ActionResult Delete()
        {
            if (AuthenticatedUser == null) return View("Home");

            //Check authenticated user
            var userRepository = new UserRepository();
            var authenticatedUser = userRepository.GetByUserNameWithoutPasswordField(AuthenticatedUser.Username);

            //Remove authenticated user information
            if (authenticatedUser == null) return View("SignOut");

            string suffix = null;

            if (ControllerContext.OtherRouteValue.ContainsKey("id"))
            {
                suffix = ControllerContext.OtherRouteValue["id"];
            }

            if (suffix == null) return View("NotFound");

            var urlRepository = new UrlRepository();

            var url = urlRepository.GetNonDeletedByShortenedSuffix(suffix);

            if (url.UserId == authenticatedUser.Id)
            {
               urlRepository.Delete(url);
                return View("UrlList");
            }


            return View("Index");
        }
    }
}

using System;
using System.IO;
using System.Net;
using System.Resources;
using System.Text;
using Newtonsoft.Json;
using PHttp.Application;
using UrlShortener.DAL;
using UrlShortener.DAL.Repositories;
using UrlShortener.Models;

namespace UrlShortener.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Simplify your links";
            ViewBag.AuthenticatedUser = AuthenticatedUser != null;
            return View();
        }


        [HttpPost]
        public ActionResult Index(string originalUrl)
        {
            ViewBag.Title = "Simplify your links";
            var isAuthenticatedUser = AuthenticatedUser != null;
            ViewBag.AuthenticatedUser = isAuthenticatedUser;

            var recaptchaResponse = this.ControllerContext.HttpContext.Request.Form["g-recaptcha-response"];
            if (originalUrl != null)
                originalUrl = originalUrl.Trim();

            if (string.IsNullOrEmpty(originalUrl))
            {
                return View(new { EmptyUrl = true, EmptyUrlMessage = "The url field is empty." });
            }

            if (!isAuthenticatedUser)
            {
                var captchaSuccess = IsRecaptchaSuccess(recaptchaResponse);
                if (!captchaSuccess)
                {
                    return
                        View(
                            new
                            {
                                InvalidCaptcha = true,
                                InvalidCaptchaMessage = "ReCaptcha verification failed.",
                                OriginalUrl = originalUrl
                            });
                }
            }

            var urlRepository = new UrlRepository();


            //check if url exists
            var url = isAuthenticatedUser ? urlRepository.GetNonDeletedByOriginalUrlAndUsername(originalUrl, AuthenticatedUser.Username) :
                urlRepository.GetNonDeletedPublicByOriginalUrl(originalUrl);

            if (url != null)
            {
                return GetShortenedResult(url);
            }

            var newUrlSuffix = GetRandomShortenedSuffix(5);
            url = new url { OriginalUrl = originalUrl, ShortenedSuffix = newUrlSuffix };
            url.CreationDate = DateTime.Now;

            if (AuthenticatedUser != null)
                urlRepository.Save(url, AuthenticatedUser.Username);
            else
                urlRepository.Save(url);

            return GetShortenedResult(url);
        }

        private ActionResult GetShortenedResult(url url)
        {
            var siteUrl = "http://" + GetSiteUrl();

            ViewBag.ShortenedUrlMessage = "Your link has been shortened";
            ViewBag.Suffix = url.ShortenedSuffix;
            return View(new ShortenedUrlModel { ShortenedUrl = siteUrl + url.ShortenedSuffix });
        }


        //Those privete method should be in the BL.
        private string GetRandomShortenedSuffix(int size)
        {
            var validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

            var random = new Random();
            var result = new StringBuilder();

            for (var i = 0; i < size; i++)
            {
                var randomNumber = random.Next(validChars.Length - 1);
                result.Append(validChars[randomNumber]);
            }

            return result.ToString();
        }

        private static bool IsRecaptchaSuccess(string recaptchaResponse)
        {
            var postData = string.Format("&secret={0}&response={1}",
                  "6LcDaBwUAAAAAKJvghq9C_sHneWQhdROC2yt-DS3",
                  recaptchaResponse);

            var postDataAsBytes = Encoding.UTF8.GetBytes(postData);

            var request = WebRequest.Create("https://www.google.com/recaptcha/api/siteverify");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postDataAsBytes.Length;
            var dataStream = request.GetRequestStream();
            dataStream.Write(postDataAsBytes, 0, postDataAsBytes.Length);
            dataStream.Close();

            var response = request.GetResponse();

            using (dataStream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(dataStream))
                {
                    var responseFromServer = reader.ReadToEnd();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseFromServer);

                    return result.success == "true";
                }
            }
        }

    }
}

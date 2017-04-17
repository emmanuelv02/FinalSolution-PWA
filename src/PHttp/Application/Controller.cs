using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Reflection;
using PHttp.Helpers;

namespace PHttp.Application
{
    //TODO move to anothe file.
    public class ViewDataDictionary : DynamicObject
    {
        private Dictionary<string, object> _vievData;
        public Dictionary<string, object> ViewData
        {
            get
            {
                if (_vievData == null)
                {
                    _vievData = new Dictionary<string, object>();
                }

                return _vievData;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            ViewData[binder.Name] = value;
            return true;
        }


        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = ViewData.ContainsKey(binder.Name) ? ViewData[binder.Name] : "";
            return true;
        }
    }


    public abstract class Controller
    {
        private string _defaultCookieKey = "";
        public AuthenticatedUser AuthenticatedUser
        {
            get;
            private set;
        }

        private dynamic _viewBag;

        public ControllerContext ControllerContext { get; set; }



        protected internal ActionResult View()
        {
            return View(null, null, null);

        }

        protected internal ActionResult View(string viewName)
        {
            return View(viewName, null, null);
        }

        protected internal ActionResult View(object model)
        {
            return View(null, null, model);
        }

        protected internal ActionResult View(StatusCode statusCode)
        {
            return View(null, null, null, statusCode);
        }

        public dynamic ViewBag
        {
            get
            {
                if (_viewBag == null)
                {
                    _viewBag = new ViewDataDictionary();
                }
                return _viewBag;
            }

        }


        protected static string ReadViewPath(string currentDir, string relativeDir)
        {

            if (string.IsNullOrEmpty(relativeDir))
                throw new Exception("The dir could not be loaded");

            //var currentDir = Environment.CurrentDirectory;
            var path = currentDir.Substring(0, currentDir.IndexOf("bin"));
            path = Path.Combine(path, relativeDir);

            return path;
        }

        public enum StatusCode
        {
            NotFound = 404,
            Ok = 200,
            BadRequest = 400
        }

        protected void Redirect(string redirectionUrl)
        {
            ControllerContext.HttpContext.Response.Redirect(redirectionUrl);
        }

        protected internal virtual ViewResult View(string viewName, string masterName, object model, StatusCode statusCode = StatusCode.Ok)
        {
            /*     if (model != null)
                 {
                     ViewData.Model = model;
                 }

                 return new ViewResult
                 {
                     ViewName = viewName,
                     MasterName = masterName,
                     ViewData = ViewData,
                     TempData = TempData,
                     ViewEngineCollection = ViewEngineCollection
                 };*/

            var assemblyPath = Path.GetDirectoryName(Assembly.GetAssembly(GetType()).Location);

            ControllerContext.HttpContext.Response.StatusCode = (int)statusCode;

            if (!string.IsNullOrEmpty(viewName))
            {
                var redirectionUrl = GetSiteVirtualPath() + "/" + viewName;

                ControllerContext.HttpContext.Response.Redirect(redirectionUrl);
                return null;
            }



            var path = ReadViewPath(assemblyPath, string.Format("Views/{0}/{1}.hbs", ControllerContext.ControllerName, ControllerContext.InvokedActionName));

            ViewBag.Model = model;
            ViewBag.User = AuthenticatedUser;

            //TODO make this configurable
            var layoutPath = ReadViewPath(assemblyPath, "Views/layout.hbs");
            var result = new ViewResult(path, layoutPath, ViewBag);


            return result;
        }

        public virtual void Initialize(ControllerContext context)
        {
            ControllerContext = context;

            var cookieKey = GetSiteVirtualPath() + "user";
            _defaultCookieKey = cookieKey;

            var tokenCookie = GetAuthenticatedUserToken();
            if (!string.IsNullOrEmpty(tokenCookie))
            {
                var authResult = JwtManager.GetPrincipal(tokenCookie);
                if (authResult != null)
                {
                    AuthenticatedUser = new AuthenticatedUser { Username = authResult.Identity.Name };
                }
            }
        }

        /// <summary>
        /// Set the authenticated user to the controller.
        /// </summary>
        /// <param name="authenticatedUser">The authentcated user to be setted</param>
        /// <param name="saveToCookie">if you want to save the user information to the cookies</param>
        /// <returns>The token saved in the cookie</returns>
        protected string SetAuthenticatedUser(AuthenticatedUser authenticatedUser, bool saveToCookie)
        {
            if (saveToCookie)
            {
                var newToken = JwtManager.GenerateToken(authenticatedUser.Username);
                if (newToken != null)
                {
                    ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie(_defaultCookieKey, newToken));
                }
                return newToken;
            }

            AuthenticatedUser = authenticatedUser;
            return null;
        }

        protected string GetSiteVirtualPath()
        {
            return UrlHelper.GetSiteVirtualPath(ControllerContext.HttpContext.Request.Path);
        }

        protected string GetSiteUrl()
        {
            return ControllerContext.HttpContext.Request.Url.Authority + GetSiteVirtualPath() + "/";
        }

        protected string GetAuthenticatedUserToken()
        {
            return ControllerContext.HttpContext.Request.Cookies[_defaultCookieKey].Value;
        }

        protected void RemoveAuthenticatedUser()
        {
            var tokenCookie = GetAuthenticatedUserToken();
            if (!string.IsNullOrEmpty(tokenCookie))
            {
                ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie(_defaultCookieKey, ""));
            }

            AuthenticatedUser = null;
        }
    }
}

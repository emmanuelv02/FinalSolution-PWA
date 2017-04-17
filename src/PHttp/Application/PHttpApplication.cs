using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PHttp.Helpers;
using PHttp.Routing;

namespace PHttp.Application
{
    public abstract class PHttpApplication : IPHttpApplication
    {
        public string Name { get; set; }
        public RouteConfiguration RouteConfiguration { get; set; }
        private List<Type> _controllerTypes;
        public event PreApplicationStartMethod PreApplicationStart;
        public event ApplicationStartMethod ApplicationStart;

        protected PHttpApplication()
        {
            PreApplicationStart += LoadControllerTypes;
            RouteConfiguration = new RouteConfiguration();
            ;
        }

        public void OnPreApplicationStart(Type type, string method)
        {
            if (PreApplicationStart != null)
                PreApplicationStart(type, method);
        }

        public abstract void Start();

        private Type GetMethodAttribute(ControllerContext controllerContext)
        {
            if (controllerContext.HttpContext.Request.HttpMethod == "GET")
            {
                return typeof(HttpGet);
            }

            if (controllerContext.HttpContext.Request.HttpMethod == "POST")
            {
                return typeof(HttpPost);
            }

            if (controllerContext.HttpContext.Request.HttpMethod == "PUT")
            {
                return typeof(HttpPut);
            }

            if (controllerContext.HttpContext.Request.HttpMethod == "DELETE")
            {
                return typeof(HttpDelete);
            }

            return null;
        }

        private ActionResult CallControllerMethod(ControllerContext controllerContext, bool isHandlerCall = false)
        {
            while (true)
            {
                if (!_controllerTypes.Any()) return null;

                foreach (var controllerType in _controllerTypes)
                {
                    if (!controllerType.Name.Equals(controllerContext.ControllerName + "Controller",
                            StringComparison.OrdinalIgnoreCase)) continue;


                    var controllerInstance = (Controller)Activator.CreateInstance(controllerType);
                    controllerInstance.Initialize(controllerContext);
                    MethodInfo theMethod = null;
                    var methods = controllerType.GetMethods();

                    var httpMethodType = GetMethodAttribute(controllerContext);

                    if (httpMethodType != null)
                        foreach (var controllerMethod in methods)
                        {
                            if (controllerMethod.Name.Equals(controllerContext.InvokedActionName, StringComparison.OrdinalIgnoreCase) && (controllerMethod.IsDefined(httpMethodType) || (httpMethodType == typeof(HttpGet) && !controllerMethod.IsDefined(typeof(HttpMethodAttribute)))))
                            {
                                theMethod = controllerMethod;
                                break;
                            }
                        }

                    if (theMethod == null) continue;

                    var methodParameters = theMethod.GetParameters();

                    var parameters = new List<object>();

                    foreach (var param in methodParameters)
                    {
                        var type = param.ParameterType;

                        object parsedParameter = null;

                        if (type == typeof(string))
                        {
                            parsedParameter = controllerContext.HttpContext.Request.Form[param.Name];
                        }
                        else if (type == typeof(int))
                        {
                            int outParam;
                            int.TryParse(controllerContext.HttpContext.Request.Form[param.Name], out outParam);
                            parsedParameter = outParam;
                        }
                        else if (type == typeof(double))
                        {
                            double outParam;
                            double.TryParse(controllerContext.HttpContext.Request.Form[param.Name], out outParam);
                            parsedParameter = outParam;
                        }
                        else if (type == typeof(decimal))
                        {
                            decimal outParam;
                            decimal.TryParse(controllerContext.HttpContext.Request.Form[param.Name], out outParam);
                            parsedParameter = outParam;
                        }
                        else if (type == typeof(bool))
                        {
                            bool outParam;
                            bool.TryParse(controllerContext.HttpContext.Request.Form[param.Name], out outParam);
                            parsedParameter = outParam;
                        }
                        else
                        {
                            var modelInstance = Activator.CreateInstance(type);
                            var modelProperties = type.GetProperties();

                            foreach (var modelProperty in modelProperties)
                            {
                                modelProperty.SetValue(modelInstance, GetValue(modelProperty, controllerContext));
                            }

                            parsedParameter = modelInstance;
                        }

                        parameters.Add(parsedParameter);
                    }

                    var result = (ActionResult)theMethod.Invoke(controllerInstance, parameters.ToArray());
                    return result;
                }
                //Check for LostRequestsHandlerController

                if (!isHandlerCall)
                {
                    controllerContext = new ControllerContext(controllerContext.HttpContext, "NotFound", "Index", new Dictionary<string, string>());
                    isHandlerCall = true;
                    continue;
                }


                //TODO 404
                return null;
            }
        }

        private object GetValue(PropertyInfo param, ControllerContext controllerContext)
        {
            var type = param.PropertyType;
            object parsedParameter = null;

            if (type == typeof(string))
            {
                parsedParameter = controllerContext.HttpContext.Request.Form[param.Name];
            }
            else if (type == typeof(int))
            {
                int outParam;
                int.TryParse(controllerContext.HttpContext.Request.Form[param.Name], out outParam);
                parsedParameter = outParam;
            }
            else if (type == typeof(double))
            {
                double outParam;
                double.TryParse(controllerContext.HttpContext.Request.Form[param.Name], out outParam);
                parsedParameter = outParam;
            }
            else if (type == typeof(decimal))
            {
                decimal outParam;
                decimal.TryParse(controllerContext.HttpContext.Request.Form[param.Name], out outParam);
                parsedParameter = outParam;
            }
            else if (type == typeof(bool))
            {
                bool outParam;
                bool.TryParse(controllerContext.HttpContext.Request.Form[param.Name], out outParam);
                parsedParameter = outParam;
            }

            return parsedParameter;
        }

        public void ExecuteAction(HttpContext context)
        {
            var path = context.Request.Path;
            var sitePart = UrlHelper.GetSiteVirtualPath(path);


            if (!string.IsNullOrEmpty(sitePart))
            {

                ActionResult result = null;

                path = path.Remove(path.IndexOf(sitePart), sitePart.Length);

                if ((path.Contains("Content") || path.Contains("Scripts") || path.Contains("fonts")) && path.Contains("."))
                {

                    //TODO update to accept more types
                    //  if (requestedFileExt == "css" || requestedFileExt == "js")
                    {

                        var assemblyPath = Path.GetDirectoryName(Assembly.GetAssembly(GetType()).Location);

                        //TODO ignore case
                        var localFilePath = assemblyPath.Substring(0, assemblyPath.IndexOf("bin"));
                        if (path.Contains("Content"))
                            path = path.Substring(path.IndexOf("Content"));
                        else if (path.Contains("Script"))
                            path = path.Substring(path.IndexOf("Scripts"));
                        else if (path.Contains("fonts"))
                            path = path.Substring(path.IndexOf("fonts"));
                        localFilePath = Path.Combine(localFilePath, path);

                        result = new FileResult(localFilePath);
                        result.ExecuteResult(context);
                    }

                    return;
                }

                var routeInformation = RouteConfiguration.GetRouteInformation(path);
                var otherRouteValues = GetOtherRouteInformation(routeInformation);


                if (routeInformation.Count == 0)
                {
                    var defaultController = RouteConfiguration.DefaultRoute["controller"];
                    var defaultAction = RouteConfiguration.DefaultRoute["action"];
                    otherRouteValues = GetOtherRouteInformation(RouteConfiguration.DefaultRoute);

                    var controllerContext = new ControllerContext(context, defaultController, defaultAction, otherRouteValues);
                    result = CallControllerMethod(controllerContext);
                }
                else
                {
                    if (!routeInformation.ContainsKey("controller") || !routeInformation.ContainsKey("action"))
                    {
                        throw new Exception("The route configuration must contain a controller and an action");
                    }

                    var controllerName = routeInformation["controller"];
                    var actionName = routeInformation["action"];
                    if (string.IsNullOrEmpty(controllerName))
                    {
                        //TODO something
                    }
                    else if (string.IsNullOrEmpty(actionName))
                    {
                        //TODO go to default action
                        var controllerContext = new ControllerContext(context, controllerName, "Index", otherRouteValues);
                        result = CallControllerMethod(controllerContext);

                    }
                    else
                    {
                        var controllerContext = new ControllerContext(context, controllerName, actionName, otherRouteValues);
                        result = CallControllerMethod(controllerContext);
                    }
                }


                if (result != null)
                {
                    result.ExecuteResult(context);
                }
            }

        }

        private void LoadControllerTypes(Type type, string method)
        {
            _controllerTypes = Assembly.GetAssembly(GetType()).GetTypes().Where(x => typeof(Controller).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface).ToList();
        }

        private static Dictionary<string, string> GetOtherRouteInformation(Dictionary<string, string> routeInformation)
        {
            var otherRouteValues = new Dictionary<string, string>();
            foreach (var routeValue in routeInformation)
            {
                if (routeValue.Key != "controller" && routeValue.Key != "action")
                {
                    otherRouteValues.Add(routeValue.Key, routeValue.Value);
                }
            }
            return otherRouteValues;
        }
    }
}

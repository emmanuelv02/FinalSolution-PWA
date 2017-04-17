using System;

namespace PHttp.Application
{
    public class HttpPost : HttpMethodAttribute
    {
    }

    public class HttpPut : HttpMethodAttribute
    {
    }

    public class HttpDelete : HttpMethodAttribute
    {
    }


    public class HttpGet : HttpMethodAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class HttpMethodAttribute : Attribute
    {

    }
}

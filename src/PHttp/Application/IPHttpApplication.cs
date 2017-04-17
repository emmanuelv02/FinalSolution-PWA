using System;
using System.Reflection;
using System.Threading.Tasks;

namespace PHttp.Application
{
    public interface IPHttpApplication
    {
        string Name { get; set; }
        event PreApplicationStartMethod PreApplicationStart;
        event ApplicationStartMethod ApplicationStart;
        void Start();
        void ExecuteAction(HttpContext context);
    }

    public delegate void PreApplicationStartMethod(Type type, string method);
    public delegate void ApplicationStartMethod(Type type, string method);

}

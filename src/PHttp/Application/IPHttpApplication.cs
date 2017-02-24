using System;

namespace PHttp.Application
{
    public interface IPHttpApplication
    {
        string Name { get; set; }
        event PreApplicationStartMethod PreApplicationStart;
        event ApplicationStartMethod ApplicationStart;
        void Start();
        void ExecuteAction();
    }

    public delegate void PreApplicationStartMethod(Type type, string method);
    public delegate void ApplicationStartMethod(Type type, string method);
}

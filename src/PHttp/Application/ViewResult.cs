using System.IO;
using System.Threading.Tasks;
using HandlebarsDotNet;

namespace PHttp.Application
{
    public class ViewResult : ActionResult
    {
        private readonly string _viewPhysicalPath;
        private readonly string _viewLayoutPath;
        private readonly dynamic _viewBag;

        public ViewResult(string viewPhysicalPath, string viewLayoutPath, dynamic viewBag)
        {
            _viewPhysicalPath = viewPhysicalPath;
            _viewLayoutPath = viewLayoutPath;
            _viewBag = viewBag;

        }

        public override async void ExecuteResult(HttpContext context)
        {
            var exists = File.Exists(_viewPhysicalPath);

            if (exists)
            {
                var partialText = File.ReadAllText(_viewPhysicalPath);
                var layoutText = File.Exists(_viewLayoutPath) ? File.ReadAllText(_viewLayoutPath) : string.Empty;

                //Partial
                {
                    var partialTemplate = Handlebars.Compile(new StringReader(partialText));
                    Handlebars.RegisterTemplate("body", partialTemplate);
                }

                //Layout
                var template = Handlebars.Compile(layoutText);


                var data = _viewBag;


                //Execute
                var result = template(data);

                
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write(result);
                }
            }

            //TODO return some error
        }
    }
}

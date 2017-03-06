using System;
using System.Text;
using System.Web;

namespace PHttp
{
	public class HttpServerUtility
	{
		internal HttpServerUtility ()
		{
		}
		public string MachineName
		{
			get { return Environment.MachineName;}
		}

		public string HtmlEncode(string value)
		{
			return HttpUtility.HtmlEncode(value);
		}

		public string HtmlDecode(string value)
		{
			return HttpUtility.HtmlDecode(value);
		}

		public string UrlEncode(string text)
		{
			return Uri.EscapeDataString(text);
		}

		public string UrlDecode(string text)
		{
			return UrlDecode(text, Encoding.UTF8);
		}

		public string UrlDecode(string text, Encoding encoding)
		{
			if (encoding == null)
				throw new ArgumentNullException("encoding");

			return HttpUtility.UrlDecode(text, encoding);
		}
	}
}


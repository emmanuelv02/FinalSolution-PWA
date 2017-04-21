using System.Linq;
using UrlShortener.DAL.Models;

namespace UrlShortener.DAL.Repositories
{
    public class UserRepository
    {

        public User GetByUserNameWithoutPasswordField(string username)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Users.FirstOrDefault(x => x.Username == username);
                if (result != null) result.Password = null;
                return result;
            }
        }

        public void Save(User user)
        {
            using (var context = new UrlShortenerContext())
            {
                if (user != null)
                {
                    context.Users.Add(user);
                    context.SaveChanges();
                }
            }
        }

        public AuthenticationResult Authenticate(string username, string password)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Users.FirstOrDefault(x => x.Username == username);
                if (result == null) return AuthenticationResult.InvalidUsername;

                if (result.Password != password) return AuthenticationResult.InvalidPassword;

                return AuthenticationResult.Success;
            }
        }

        public enum AuthenticationResult
        {
            Success,
            InvalidUsername,
            InvalidPassword
        }
    }
}

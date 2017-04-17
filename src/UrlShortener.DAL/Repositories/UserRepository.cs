using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UrlShortener.DAL.Repositories
{
    public class UserRepository
    {

        public user GetByUserNameWithoutPasswordField(string username)
        {
            using (var context = new UrlShortenerDbContainer())
            {
                var result = context.users.FirstOrDefault(x => x.Username == username);
                if (result != null) result.Password = null;
                return result;
            }
        }

        public void Save(user user)
        {
            using (var context = new UrlShortenerDbContainer())
            {
                if (user != null)
                {
                    context.users.Add(user);
                    context.SaveChanges();
                }
            }
        }

        public AuthenticationResult Authenticate(string username, string password)
        {
            using (var context = new UrlShortenerDbContainer())
            {
                var result = context.users.FirstOrDefault(x => x.Username == username);
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

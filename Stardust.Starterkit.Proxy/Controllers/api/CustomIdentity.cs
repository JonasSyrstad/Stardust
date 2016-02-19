using System.Security.Principal;

namespace Stardust.Starterkit.Proxy.Controllers.api
{
    public class CustomIdentity : IIdentity
    {
        public CustomIdentity(string userName, string authenticationType)
        {
            Name = userName;
            AuthenticationType = authenticationType;
        }

        /// <summary>
        /// Gets the name of the current user.
        /// </summary>
        /// <returns>
        /// The name of the user on whose behalf the code is running.
        /// </returns>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the type of authentication used.
        /// </summary>
        /// <returns>
        /// The type of authentication used to identify the user.
        /// </returns>
        public string AuthenticationType { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether the user has been authenticated.
        /// </summary>
        /// <returns>
        /// true if the user was authenticated; otherwise, false.
        /// </returns>
        public bool IsAuthenticated
        {
            get
            {
                return true;
            }
        }
    }
}
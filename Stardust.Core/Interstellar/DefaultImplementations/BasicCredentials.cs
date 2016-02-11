using Stardust.Core.Security;

namespace Stardust.Interstellar.DefaultImplementations
{
    internal class BasicCredentials
    {
        public string Username { get; private set; }
        public EncryptionKeyContainer Password { get; private set; }

        public string DomainUserName { get; private set; }

        public string DomainName { get; private set; }

        public BasicCredentials(string username, EncryptionKeyContainer password)
        {
            Username = username;
            Password = password;
            if (username.Contains("\\"))
            {
                var parts = username.Split('\\');
                DomainName = parts[0];
                DomainUserName = parts[1];
            }
        }
    }
}

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Stardust.Core.Security
{
    /// <summary>
    /// A little something to make keys and passwords a bit more secure when in memory
    /// </summary>
    public class EncryptionKeyContainer:IDisposable
    {
        /// <summary>
        /// the secret as a secure string
        /// </summary>
        internal SecureString Secret { get; private set; }

        public EncryptionKeyContainer(string secret)
        {
            Secret = ConvertToSecureString(secret);
        }

        public SecureString GetSecureString()
        {
            return Secret;
        }

        internal string GetSecret()
        {
            return ConvertToUnSecureString(Secret);
        }

        private static SecureString ConvertToSecureString(string strPassword)
        {
            var secureStr = new SecureString();
            if (strPassword.Length <= 0) return secureStr;
            foreach (var c in strPassword.ToCharArray()) secureStr.AppendChar(c);
            return secureStr;
        }
        private string ConvertToUnSecureString(SecureString secstrPassword)
        {
            var unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secstrPassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if(disposing) GC.SuppressFinalize(this);
            Secret.Dispose();
        }
    }
}

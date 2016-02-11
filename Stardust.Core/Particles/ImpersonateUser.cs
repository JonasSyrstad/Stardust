//
// ImpersonateUser.cs
// This file is part of Stardust
//
// Author: Jonas Syrstad (jsyrstad2+StardustCore@gmail.com), http://no.linkedin.com/in/jonassyrstad/) 
// Copyright (c) 2014 Jonas Syrstad. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;

namespace Stardust.Particles
{
    /// <summary>
    /// Class used to impersonate a windows or domain user. 
    /// Comes in handy for file transfere within the domain.
    /// </summary>
    /// <remarks>If you use this class, be sure to demand that it
    ///  runs with FullTrust.</remarks>
    public class ImpersonateUser
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);
        private static IntPtr TokenHandle = new IntPtr(0);
        private static WindowsImpersonationContext ImpersonatedUser;
        private WindowsIdentity NewId;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(
            String lpszUsername,
            String lpszDomain,
            String lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);

        /// <summary>
        /// Impersonate wondows/domain user
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void Impersonate(string domainName, string userName, string password)
        {
            try
            {
                const int logon32ProviderDefault = 0;
                const int logon32LogonInteractive = 2;
                TokenHandle = IntPtr.Zero;
                var returnValue = LogonUser(
                                    userName,
                                    domainName,
                                    password,
                                    logon32LogonInteractive,
                                    logon32ProviderDefault,
                                    ref TokenHandle);

                if (false == returnValue)
                {
                    int ret = Marshal.GetLastWin32Error();
                    Console.WriteLine("LogonUser call failed with error code : " + ret);
                    throw new System.ComponentModel.Win32Exception(ret);
                }
                NewId = new WindowsIdentity(TokenHandle);
                ImpersonatedUser = NewId.Impersonate();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred. " + ex.Message);
            }
        }

        /// <summary>
        /// Stops impersonation
        /// </summary>
        public void Undo()
        {
            ImpersonatedUser.Undo();
            if (TokenHandle != IntPtr.Zero)
                CloseHandle(TokenHandle);
            NewId.Dispose();
        }
    }
}
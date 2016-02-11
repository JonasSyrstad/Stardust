//
// ApplicationPoolRecycle.cs
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
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Management;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Interstellar
{
    public static class ApplicationPoolRecycle
    {
        public static bool RecycleCurrentApplicationPool()
        {
            var appPoolRecycler = Resolver.Activate<IAppPoolRecycler>();
            if (appPoolRecycler.IsInstance())
            {
                return appPoolRecycler.TryRecycleCurrent();
                
            }
            try
            {
                if (IsApplicationRunningOnAppPool())
                {
                    var appPoolId = GetCurrentApplicationPoolId();
                    RecycleApplicationPool(appPoolId);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsApplicationRunningOnAppPool()
        {
            if (!AppDomain.CurrentDomain.FriendlyName.StartsWith("/LM/"))
                return false;
            if (!DirectoryEntry.Exists("IIS://Localhost/W3SVC/AppPools"))
                return false;
            return true;
        }

        private static string GetCurrentApplicationPoolId()
        {
            string virtualDirPath = AppDomain.CurrentDomain.FriendlyName;
            virtualDirPath = virtualDirPath.Substring(4);
            int index = virtualDirPath.Length + 1;
            index = virtualDirPath.LastIndexOf("-", index - 1, index - 1);
            index = virtualDirPath.LastIndexOf("-", index - 1, index - 1);
            virtualDirPath = "IIS://localhost/" + virtualDirPath.Remove(index);
            var virtualDirEntry = new DirectoryEntry(virtualDirPath);
            return virtualDirEntry.Properties["AppPoolId"].Value.ToString();
        }

        private static bool RecycleApplicationPool(string appPoolId)
        {
            var appPoolRecycler = Resolver.Activate<IAppPoolRecycler>();
            if (appPoolRecycler.IsInstance())
            {
                return appPoolRecycler.TryRecycle(appPoolId);
            }
            var appPoolPath = "IIS://localhost/W3SVC/AppPools/" + appPoolId;
            var appPoolEntry = new DirectoryEntry(appPoolPath);
            appPoolEntry.Invoke("Recycle");
            return true;
        }

        internal static bool RecycleAll()
        {
            var appPoolRecycler = Resolver.Activate<IAppPoolRecycler>();
            if (appPoolRecycler.IsInstance())
            {
                return appPoolRecycler.TryRecycleAll();
            }
            foreach (var listApplicationPool in ListApplicationPools(Utilities.Utilities.GetHostName()))
            {
                RecycleAppPool(Utilities.Utilities.GetHostName(), listApplicationPool);
            }
            return true;
        }

        static IEnumerable<string> ListApplicationPools(string hostname)
        {
            var classPath = @"\\" + hostname + @"\root\microsoftiisv2:IIsApplicationPoolSetting";
            var wmiObject = new ManagementClass(classPath);
            return (from ManagementObject child in wmiObject.GetInstances()
                    let SplitChar = "/".ToCharArray()
                    select child.ToString().Split(SplitChar)[child.ToString().Split(SplitChar).Length - 1].Replace(@"""", "")).ToArray();
        }

        public static void RecycleAppPool(string machine, string appPoolName)
        {
            var path = "IIS://" + machine + "/W3SVC/AppPools/" + appPoolName;
            var w3Svc = new DirectoryEntry(path);
            w3Svc.Invoke("Recycle", null);
        }
    }
}
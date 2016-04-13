//
// stardustcommandbase.cs
// This file is part of Stardust.Core.Automation
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

using System.Management.Automation;
using Stardust.Interstellar;
using Stardust.Nucleus;
using Stardust.Particles;
using Utilities = Stardust.Interstellar.Utilities.Utilities;

namespace Stardust.Core.Automation
{
    public abstract class StardustCommandBase : Cmdlet
    {
        protected sealed override void BeginProcessing()
        {
            WriteVerbose("Initializing Runtime");
            RuntimeInitializer();
        }

        protected virtual string CallingInstance
        {
            get { return "PowerShellCmd"; }
        }

        protected IRuntime Runtime;
        private string ConfigSet1;

        protected virtual string ServiceName { get { return "WebUi"; } }

        protected void RuntimeInitializer()
        {

            Runtime = RuntimeFactory.CreateRuntime();
            Runtime.SetEnvironment(Environment);
            Runtime.SetServiceName(this, ServiceName, "Initialize");
            Runtime.InitializeWithConfigSetName(ConfigSet);
        }

        [Parameter(Mandatory = false)]
        public string ConfigSet
        {
            get
            {
                if (ConfigSet1.IsNullOrEmpty())
                    ConfigSet1 = Utilities.GetConfigSetName();
                return ConfigSet1;
            }
            set
            {
                Utilities.SetConfigSetName(value);
                ConfigSet1 = value;
            }
        }

        [Parameter(Mandatory = false)]
        public string Environment
        {
            get
            {
                return Utilities.GetEnvironment();
            }
            set
            {
                Utilities.SetEnvironment(value);
            }
        }

        [Parameter(Mandatory = false)]
        public string UserName { get; set; }

        [Parameter(Mandatory = false)]
        public string Password { get; set; }

        protected sealed override void EndProcessing()
        {
            WriteVerbose("Logging Runtime lifetime");
            DoLogging(Runtime.CallStack);
            Runtime = null;
            ContainerFactory.Current.KillAllInstances();
        }

        protected abstract void DoLogging(CallStackItem callStack);

        protected IServiceContainer<T> GetServiceContainer<T>() where T : class
        {
            WriteVerbose(string.Format("Environment: {0}", Runtime.Environment));
            var container = Runtime.CreateServiceProxy<T>();
            WriteVerbose(string.Format("Service url: {0}", container.GetUrl()));
            container.Initialize(true);
            if (UserName.ContainsCharacters() && Password.ContainsCharacters())
                container.SetCredentials(UserName, Password);
            return container;
        }
    }
}

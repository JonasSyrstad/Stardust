//
// utilities.cs
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
using System.Configuration;
using System.ServiceModel;
using System.Web;
using Stardust.Interstellar.Messaging;
using Stardust.Nucleus;
using Stardust.Particles;

namespace Stardust.Interstellar.Utilities
{
    public static class Utilities
    {
        private const string ConfigSet = "configSet";
        private const string ConfigServiceUrl = "configServiceUrl";
        private const string Environment = "environment";

        public static TimeSpan GetExecutionTimeAsTimeSpan(this IResponseBase self)
        {
            return new TimeSpan(self.ResponseHeader.ExecutionTime);
        }

        public static string GetConfigSetName()
        {
            var set = ConfigurationManagerHelper.GetValueOnKey("stardust.configSet");
            if (set.ContainsCharacters()) return set;
            set = ConfigurationManagerHelper.GetValueOnKey(ConfigSet);
            ConfigurationManagerHelper.SetValueOnKey("stardust.configSet", set, true);
            return set;
        }

        public static string GetEnvironment()
        {
            var env = ConfigurationManagerHelper.GetValueOnKey("stardust.environment");
            if (env.ContainsCharacters()) return env;
            env= ConfigurationManagerHelper.GetValueOnKey(Environment);
            ConfigurationManagerHelper.SetValueOnKey("stardust.environment", env, true);
            return env;
        }

        public static void SetEnvironment(string environment)
        {
             ConfigurationManagerHelper.SetValueOnKey(Environment,environment,true);
             ConfigurationManagerHelper.SetValueOnKey("stardust.environment", environment, true);
        }

        public static string GetVersion()
        {
            return ConfigurationManagerHelper.GetValueOnKey("serviceVersion");
        }

        public static string GetConfigVersion()
        {
            return ConfigurationManagerHelper.GetValueOnKey("configVersion");
        }

        public static string GetServiceName()
        {
            var serviceName = ConfigurationManagerHelper.GetValueOnKey("stardust.serviceName");
            if (serviceName.ContainsCharacters()) return serviceName;
            serviceName = ConfigurationManagerHelper.GetValueOnKey("serviceName");
            ConfigurationManagerHelper.SetValueOnKey("stardust.serviceName", serviceName, true);
            return serviceName;
        }

        public static ServiceNameAttribute GetServiceNameFromAttribute<T>()
        {
            return GetServiceNameFromAttribute(typeof(T));
        }

        public static ServiceNameAttribute GetServiceNameFromAttribute(Type type)
        {
            return type.GetAttribute<ServiceNameAttribute>();
        }

        internal static string GetHostName()
        {
            return HttpContext.Current.Server.MachineName;
        }

        public static string GetConfigStorePath()
        {
            return ConfigurationManagerHelper.GetValueOnKey(ConfigServiceUrl);
        }

        public static string GetDefaultConnectionString()
        {
            return ConfigurationManager.ConnectionStrings[0].ConnectionString;
        }

        public static EndpointAddress FormatAddress(string uri, string serviceName)
        {
            return Resolver.Activate<IUrlFormater>().FormatUrl(uri, serviceName);
        }



        internal static string GetDisableCertificateValidation()
        {
            var value = ConfigurationManagerHelper.GetValueOnKey("disableCertificateValidation");
            return value.IsNullOrWhiteSpace() ? "false" : value;
        }

        public static void SetConfigSetName(string value)
        {
            ConfigurationManagerHelper.SetValueOnKey(ConfigSet, value, true);
        }


        public static bool IsDevelopementEnv()
        {
            return !IsNotDevelopementEnvironment();
        }

        public static bool IsNotDevelopementEnvironment()
        {
            if (GetEnvironment().IsNullOrWhiteSpace()) return true;
            return !GetEnvironment().Equals("dev", StringComparison.CurrentCultureIgnoreCase) &&
                   !GetEnvironment().Equals("development", StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsTestEnv()
        {
            if (GetEnvironment().IsNullOrWhiteSpace()) return true;
            return GetEnvironment().Equals("test", StringComparison.CurrentCultureIgnoreCase) ||
                   GetEnvironment().Equals("stag", StringComparison.CurrentCultureIgnoreCase) ||
                   GetEnvironment().Equals("staging", StringComparison.CurrentCultureIgnoreCase);
        }

        public static string GetConfigLocation()
        {
            return ConfigurationManagerHelper.GetValueOnKey("stardust.configLocation");
        }
    }
}

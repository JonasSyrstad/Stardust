using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BrightstarDB.EntityFramework;
using Stardust.Interstellar.ConfigurationReader;

namespace Stardust.Starterkit.Configuration.Repository
{
    [Entity("ConfigSet")]
    public interface IConfigSet
    {
        [Identifier("http://stardust.com/configuration/configsets/", KeyProperties = new[] { "Name", "System" }, KeySeparator = "-")]
        string Id { get; }

         [RegularExpression(Constants.KeyValidator, ErrorMessage = "special characters are not  allowed.")]
        string Name { get; set; }
          [RegularExpression(Constants.KeyValidator, ErrorMessage = "special characters are not  allowed.")]
        string System { get; set; }

        [InverseProperty("ConfigSet")]
        ICollection<IEnvironment> Environments { get; set; }

        [InverseProperty("ConfigSet")]
        ICollection<IServiceHostSettings> ServiceHosts { get; set; }
            
        [InverseProperty("ConfigSet")]
        ICollection<IServiceDescription> Services { get; set; }

        IConfigSet ParentConfigSet { get; set; }

        [InverseProperty("ParentConfigSet")]
        ICollection<IConfigSet> ChildConfigSets { get; set; }

        DateTime Created { get; set; }
        DateTime LastUpdate { get; set; }

        ConfigurationSet GetRawConfigData(string environment, out bool doSave);

        ICollection<IConfigUser> Administrators { get; set; }

        
        string Description { get; set; }

        /// <summary>
        /// a | separated list of layer names
        /// </summary>
        string LayerNames { get; set; }

        string ReaderKey { get; set; }

        void SetReaderKey(string key);

        string GetReaderKey();

        bool AllowAccessWithRootKey { get; set; }

        bool AllowAccessWithUserTokens { get; set; }

        string Version { get; set; }
    }
}
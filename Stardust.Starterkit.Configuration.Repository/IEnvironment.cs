using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BrightstarDB.EntityFramework;
using JetBrains.Annotations;
using Stardust.Interstellar.ConfigurationReader;

namespace Stardust.Starterkit.Configuration.Repository
{
    [Entity("Environment")]
    public interface IEnvironment
    {
        [Identifier("http://stardust.com/configuration/Environments/", KeyProperties = new[] { "ConfigSetNameId", "Name" }, KeySeparator = "-")]
        string Id { get; }
        [RegularExpression(Constants.KeyValidator, ErrorMessage = "special characters are not  allowed.")]
        string Name { get; set; }

        string ConfigSetNameId { get; set; }

        IConfigSet ConfigSet { get; set; }

        IEnvironment ParentEnvironment { get; set; }

        [InverseProperty("ParentEnvironment")]
        ICollection<IEnvironment> ChildEnvironments { get; set; }


        [InverseProperty("Environment")]
        ICollection<ISubstitutionParameter> SubstitutionParameters { get; set; }

        [InverseProperty("Environment")]
        ICollection<IEnvironmentParameter> EnvironmentParameters { get; set; }

        [RelativeOrAbsoluteIdentifier("relative")]
        [InverseProperty("Environment")]
        ICacheSettings CacheType { get; set; }

        EnvironmentConfig GetRawConfigData();
        IdentitySettings GetRawIdentityData();

        string Description { get; set; }

        string ReaderKey { get; set; }

        string ETag { get; set; }

        [DisplayName("Last updated")]
        DateTime LastPublish { get; set; }

        int Version { get; set; }

        void SetReaderKey(string key);

        string GetReaderKey();
    }

    [Entity("CacheSettings")]
    public interface ICacheSettings
    {
        string Id { get; }

        [Ignore]
        string CacheType { get; }

        string CacheImplementation { get; set; }

        IEnvironment Environment { get; set; }

        bool NotifyOnChange { get; set; }
        string CacheName { get; set; }

        [DisplayName("Machine names")]
        string MachineNames { get; set; }
        int Port { get; set; }
        bool Secure { get; set; }
        string PassPhrase { get; set; }

        string SecurityMode { get; set; }

        string ProtectionLevel { get; set; }

    }
}
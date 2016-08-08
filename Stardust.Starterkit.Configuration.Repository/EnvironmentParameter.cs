using System;
using System.Collections.Generic;
using Stardust.Interstellar.ConfigurationReader;
using Stardust.Particles;

namespace Stardust.Starterkit.Configuration.Repository
{
    public partial class EnvironmentParameter
    {
        public string Value
        {
            get
            {
                if (ItemValue == null)
                    return Parent == null ? "" : Parent.Value;
                return ItemValue;
            }
        }

        public bool IsRoot { get { return Parent == null; } }

        public bool IsInherited
        {
            get { return string.IsNullOrWhiteSpace(ItemValue) && !IsRoot; }
        }

        public ConfigParameter GetRawConfigData()
        {
            var binary = new byte[0];
            try
            {
                if (Value.IsBase64String() && IsSecureString)
                    binary = Convert.FromBase64String(Value);
            }
            catch
            {
            }
            return new ConfigParameter
            {
                Name = Name,
                Value = Value,
                BinaryValue = binary,
                ChildParameters = new List<ConfigParameter>(),
                Description = Description
            };
        }

        public void SetValue(string value)
        {
            if (!IsSecureString)
            {
                ItemValue = value;
                return;
            }
            var encrypted = value.Encrypt(KeyHelper.SharedSecret);
            ItemValue = encrypted;
            BinaryValue = encrypted.GetByteArray();
            if (ItemValue.Decrypt(KeyHelper.SharedSecret) != value) throw new StardustCoreException("Encryption validation failed!");
        }



        public override string ToString()
        {
            return Id;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Stardust.Interstellar.Endpoints;
using Stardust.Nucleus;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business
{
    public class UniqueIdGenerator : IDisposable
    {
        public static string CreateNewId()
        {
            return CreateNewId(10);
        }

        public static string CreateNewId(int numberOfCharacters)
        {
            return GetInstance().GetBase32UniqueId(numberOfCharacters);
        }

        private static readonly UniqueIdGenerator Instance = new UniqueIdGenerator();
        private static char[] _charMap = {
            '2', '3', '4', '5', '6', '7', '8', '9', 
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        public static UniqueIdGenerator GetInstance()
        {
            return Instance;
        }

        private readonly RNGCryptoServiceProvider _provider = new RNGCryptoServiceProvider();

        private UniqueIdGenerator()
        {
        }

        public void GetNext(byte[] bytes)
        {
            _provider.GetBytes(bytes);
        }

        public string GetBase32UniqueId(int numDigits)
        {
            return GetBase32UniqueId(new byte[0], numDigits);
        }

        public string GetBase32UniqueId(byte[] basis, int numDigits)
        {
            const int byteCount = 16;
            var randBytes = new byte[byteCount - basis.Length];
            GetNext(randBytes);
            var bytes = new byte[byteCount];
            Array.Copy(basis, 0, bytes, byteCount - basis.Length, basis.Length);
            Array.Copy(randBytes, 0, bytes, 0, randBytes.Length);

            var lo = (((ulong)BitConverter.ToUInt32(bytes, 8)) << 32) | BitConverter.ToUInt32(bytes, 12);
            var hi = (((ulong)BitConverter.ToUInt32(bytes, 0)) << 32) | BitConverter.ToUInt32(bytes, 4);
            const int mask = 0x1F;

            var chars = new char[26];
            var charIdx = 25;

            var work = lo;
            for (var i = 0; i < 26; i++)
            {
                if (i == 12)
                {
                    work = ((hi & 0x01) << 4) & lo;
                }
                else if (i == 13)
                {
                    work = hi >> 1;
                }
                var digit = (byte)(work & mask);
                chars[charIdx] = _charMap[digit];
                charIdx--;
                work = work >> 5;
            }

            var ret = new string(chars, 26 - numDigits, numDigits);
            return ret;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                GC.SuppressFinalize(this);
            if (_provider.IsInstance())
                _provider.Dispose();
        }

        ~UniqueIdGenerator()
        {
            Dispose(false);
        }
    }
    public static class ConfigReaderFactory
    {


        public static IConfigSetTask GetConfigSetTask()
        {
            return Resolver.Activate<IConfigSetTask>();
        }

        public static List<KeyValuePair<string, string>> GetAvailableBindingTypes(IEnumerable<string> addedBindings )
        {
            BindingFactory.Initialize();
            var dat = from b in Resolver.GetImplementationsOf<IBindingCreator>()
                where b.Key != "sts" && !addedBindings.Contains(b.Key)
                select b;
            return dat.ToList();
        }

        public static IUserFacade GetUserFacade()
        {
            return Resolver.Activate<IUserFacade>();
        }

        public static IConfigUser CurrentUser
        {
            get
            {
                return (IConfigUser) ContainerFactory.Current.Resolve(typeof(IConfigUser),Scope.Context, () => GetUserFacade().GetUser(ContainerFactory.CurrentPrincipal.Identity.Name.GetUsername()));
            }
        }

        private static string GetUsername(this string userName)
        {
            var splitted = userName.Split('\\');
            if (splitted.Length == 1) return userName;
            return splitted[1];
        }
    }
}

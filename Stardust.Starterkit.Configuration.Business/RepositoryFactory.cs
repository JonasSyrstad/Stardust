using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using BrightstarDB.Client;
using Stardust.Nucleus;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business
{
    public class RepositoryFactory : IRepositoryFactory
    {

        public ConfigurationContext GetRepository()
        {
            return (ConfigurationContext)ContainerFactory.Current.Resolve(typeof(ConfigurationContext), Scope.Context, CreateRepository);
        }

        private static ConfigurationContext CreateRepository()
        {
            var connectionString = GetConnectionString();
            var context = new ConfigurationContext(connectionString)
                              {
                                  FilterOptimizationEnabled = true
                              };
            return context;
        }

        public static string GetConnectionString()
        {
            string connectionString;
            var postfix = "Store";
            if (ConfigurationManagerHelper.GetValueOnKey("configStoreMigrationFile").ContainsCharacters())
            {
                if (File.Exists(ConfigurationManagerHelper.GetValueOnKey("configStoreMigrationFile")))
                {
                    postfix = "";
                }
                else
                {
                }
            }
            connectionString = ConfigurationManagerHelper.GetValueOnKey("configStore");
            if (connectionString.IsNullOrWhiteSpace())
            {
                connectionString = "Type=embedded;endpoint=http://localhost:8090/brightstar;StoresDirectory=C:\\Stardust\\Stores;StoreName=configWeb";
            }
            connectionString = connectionString + postfix;
            return connectionString;
        }

        public static FileInfo Backup(string file, string tempDir)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            ZipFile.CreateFromDirectory(tempDir, file, CompressionLevel.Fastest, true);
            ClearTempDir(tempDir);
            var finfo = new FileInfo(file);
            return finfo;
        }

        private static void ClearTempDir(string tempDir)
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }

        public static string CreateTempDir(string id, string path)
        {
            var tempDir = ConfigurationManagerHelper.GetValueOnKey("stardust.StoreLocation") + "\\bck_temp\\" + id;
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            Directory.CreateDirectory(tempDir);
            foreach (var enumerateFile in Directory.EnumerateFiles(path))
            {
                var finfo = new FileInfo(enumerateFile);
                finfo.CopyTo(tempDir + "\\" + finfo.Name, true);
            }
            return tempDir;
        }

        public static void Backup()
        {
            var id = GetConnectionString().Split(';').Last().Split('=').Last();
            var path = Path.Combine(ConfigurationManagerHelper.GetValueOnKey("stardust.StoreLocation"));
            var tempDir = CreateTempDir(id, path);
            var file = string.Format("{0}\\{1}_{2}.zip", ConfigurationManagerHelper.GetValueOnKey("stardust.StoreLocation"), id, DateTime.Now.ToString("yyMMdd"));
            Task.Run(() => Backup(file, tempDir));
        }
    }
}
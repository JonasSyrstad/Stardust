using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrightstarDB;
using BrightstarDB.Client;
using Stardust.Nucleus;
using Stardust.Particles;
using Stardust.Starterkit.Configuration.Repository;

namespace Stardust.Starterkit.Configuration.Business
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private static Dictionary<string, IJobInfo> jobs;

        private static string BackupFileExtension = ".bsbck";

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

        public static void Restore(string file)
        {
            if(Directory.Exists(new FileInfo(file).DirectoryName + "\\imp"))
                Directory.Delete(new FileInfo(file).DirectoryName + "\\imp",true);
            Directory.CreateDirectory(new FileInfo(file).DirectoryName + "\\imp");
            ZipFile.ExtractToDirectory(file, new FileInfo(file).DirectoryName+"\\imp");
            IJobInfo jobInf=null;
            var service = BrightstarService.GetClient(GetConnectionString());
            string storeName=null;
            List<string> files;
            files = Directory.EnumerateFiles(new FileInfo(file).DirectoryName + "\\imp").ToList();
            var dirs= Directory.EnumerateDirectories(new FileInfo(file).DirectoryName + "\\imp");
            foreach (var dir in dirs)
            {
                files.AddRange(Directory.EnumerateFiles(dir));
            }
            foreach (var enumerateFile in files)
            {
                if (enumerateFile.EndsWith(BackupFileExtension))
                {
                    storeName = new FileInfo(enumerateFile).Name.Replace(BackupFileExtension, "");

                    Task.Run(
                        () =>
                            {



                                if (!service.DoesStoreExist(storeName)) service.CreateStore(storeName);
                                jobInf = service.StartImport(storeName, enumerateFile);
                            });
                }
                else
                {
                    if (Directory.Exists(new FileInfo(file).DirectoryName + "\\imp"))
                        Directory.Delete(new FileInfo(file).DirectoryName + "\\imp", true);
                    return;
                }
                
            }
            var waitForIt = true;
            while (waitForIt)
            {
                Thread.Sleep(100);

                if(jobInf!=null);
                {
                    var j= service.GetJobInfo(storeName, jobInf.JobId);
                    waitForIt=  j.JobCompletedOk||j.JobCompletedWithErrors;
                }
                
            }
            if (Directory.Exists(new FileInfo(file).DirectoryName + "\\imp"))
                Directory.Delete(new FileInfo(file).DirectoryName + "\\imp", true);
        }

        public static FileInfo Backup(string file, string tempDir)
        {
            if (File.Exists(file))
                File.Delete(file);
            FileInfo fileInfo = null;
            var client = new RepositoryFactory().GetRepository();
            jobs = new Dictionary<string, IJobInfo>();
            StartBackup(ref fileInfo);
            var service = GetClient();
            var running = true;
            while (running)
            {
                running = !jobs.ToList().TrueForAll(
                    j1 =>
                        {
                            var j = service.GetJobInfo(j1.Key, j1.Value.JobId);
                            return j.JobCompletedOk || j.JobCompletedWithErrors;
                        }) || jobs.Count == 0;
                Thread.Sleep(100);
            }
            File.Copy(fileInfo.FullName, tempDir + fileInfo.Name);
            ZipFile.CreateFromDirectory(tempDir, file, CompressionLevel.Optimal, true);
            ClearTempDir(tempDir);
            return new FileInfo(file);
        }

        public static IBrightstarService GetClient()
        {
            return BrightstarService.GetClient(GetConnectionString());
        }

        private static void StartBackup(ref FileInfo fileInfo)
        {
            var service1 = BrightstarService.GetClient(GetConnectionString());
            foreach (var store in service1.ListStores())
            {

                if (GetConnectionString().Contains("StoreName=" + store))
                {
                    fileInfo = new FileInfo(FileName(store));
                }
            }
            Task.Run(() => GetValue());
            return;
        }


        private static void GetValue()
        {
            var service = BrightstarService.GetClient(GetConnectionString());
            foreach (var store in service.ListStores())
            {
                var job = service.StartExport(store, FileName(store));
                jobs.Add(store, job);
            }
        }

        private static string FileName(string store)
        {
            return ConfigurationManagerHelper.GetValueOnKey("stardust.StoreLocation") + "\\" + store + BackupFileExtension;
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
            var tempDir = ConfigurationManagerHelper.GetValueOnKey("stardust.StoreLocation") + "\\bck_temp\\";
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            Directory.CreateDirectory(tempDir);
            //foreach (var enumerateFile in Directory.EnumerateFiles(path))
            //{
            //    var finfo = new FileInfo(enumerateFile);
            //    finfo.CopyTo(tempDir + "\\" + finfo.Name, true);
            //}
            return tempDir;
        }

        public static void Backup()
        {
            Backup("", "");
            return;
            var id = GetConnectionString().Split(';').Last().Split('=').Last();
            var path = Path.Combine(ConfigurationManagerHelper.GetValueOnKey("stardust.StoreLocation"));
            var tempDir = CreateTempDir(id, path);
            var file = string.Format("{0}\\{1}_{2}.zip", ConfigurationManagerHelper.GetValueOnKey("stardust.StoreLocation"), id, DateTime.Now.ToString("yyMMdd"));
            Task.Run(() => Backup(file, tempDir));
        }
    }
}
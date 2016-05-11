using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Dispatcher;

namespace Stardust.Interstellar.Rest.ServiceWrapper
{
    public static class ServiceFactory
    {
        static ServiceBuilder Builder = new ServiceBuilder();

        private static List<Type> ServiceTypes=new List<Type>();

        public static Type CreateServiceImplementation<T>()
        {
            var type= Builder.CreateServiceImplementation<T>();
            ServiceTypes.Add(type);
            return type;
        }

        public static IEnumerable<Type> CreateServiceImplementationForAllInCotainingAssembly<T>()
        {
            var assembly = typeof(T).Assembly;
            return CreateServiceImplementations(assembly);
        }

        public static IEnumerable<Type> CreateServiceImplementations(Assembly assembly)
        {
            var types= assembly.GetTypes().Where(t => t.IsInterface).Select(item => Builder.CreateServiceImplementation(item));
            ServiceTypes.AddRange(types);
            return types;
        }

        public static void FinalizeRegistration()
        {

            try
            {
                Builder.Save();
            }
            catch (Exception)
            {
                // ignored
            }
            var parent = (IHttpControllerTypeResolver)GlobalConfiguration.Configuration.Services.GetService(typeof(IHttpControllerTypeResolver));
            CustomAssebliesResolver.SetParent(parent);
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerTypeResolver), new CustomAssebliesResolver());
        }

        public static Assembly GetAssembly()
        {
            return Builder.GetCustomAssembly();
        }

        public static IEnumerable<Type> GetTypes()
        {
            return ServiceTypes;
        }
    }

    public class CustomAssebliesResolver : IHttpControllerTypeResolver
    {

        internal static void SetParent(IHttpControllerTypeResolver parent)
        {
            ParentResolver = parent;
        }
        private static IHttpControllerTypeResolver ParentResolver;

        /// <summary> Returns a list of assemblies available for the application. </summary>
        /// <returns>A &lt;see cref="T:System.Collections.ObjectModel.Collection`1" /&gt; of assemblies.</returns>
        public ICollection<Assembly> GetAssemblies()
        {
            var baseAssemblies = new List<Assembly>(); //AppDomain.CurrentDomain.GetAssemblies().ToList();
            baseAssemblies.Add(ServiceFactory.GetAssembly());
            return baseAssemblies;

        }

        /// <summary> Returns a list of controllers available for the application. </summary>
        /// <returns>An &lt;see cref="T:System.Collections.Generic.ICollection`1" /&gt; of controllers.</returns>
        /// <param name="assembliesResolver">The resolver for failed assemblies.</param>
        public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            var controllers = ParentResolver.GetControllerTypes(assembliesResolver).ToList();
            controllers.AddRange(ServiceFactory.GetTypes());

            return controllers;
        }

    }
}


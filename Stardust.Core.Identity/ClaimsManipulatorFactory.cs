using System.Security.Claims;
using Stardust.Core.FactoryHelpers;

namespace Stardust.Core.Identity
{
    public static class ClaimsManipulatorFactory
    {
        static ClaimsManipulatorFactory()
        {
            BindAppender<DefaultClaimsAppender>();
            BindReader<DemoAttributeReader>();
            BindClaimsIdentity<TypedClaimsIdentity>();
        }
        public static IClaimsAppender GetClaimsAppender()
        {
            return Resolver.Activate<IClaimsAppender>();
        }

        public static IAttributeReader CreateAttributeReader()
        {
            return Resolver.Activate<IAttributeReader>();
        }

        public static void BindReader<T>() where T : IAttributeReader
        {
            Resolver.UnBind<IAttributeReader>()
                .AllAndBind()
                .To<T>();
        }

        public static void BindAppender<T>() where T : IClaimsAppender
        {
            Resolver.UnBind<IClaimsAppender>()
                .AllAndBind()
                .To<T>();
        }

        public static void BindClaimsIdentity<T>() where T : ITypedClaimsIdentity<TypedClaimsUser>
        {
            Resolver.UnBind<ClaimsIdentity>()
                .AllAndBind()
                .To<T>();
        }
    }
}
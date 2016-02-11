using System;
using Autofac;
using Autofac.Builder;
using Stardust.Nucleus;

namespace Stardust.Core.AutoFac
{
    public class AfBindContext<T> : IBindContext<T>, IScopeContext
    {
        private readonly ContainerBuilder Configuration;

        public AfBindContext(ContainerBuilder configuration)
        {
            Configuration = configuration;
        }

        public IScopeContext To(Type type, string identifier = "default")
        {
            AfSetting=Configuration.RegisterType(type).As<T>();
            AfSetting.Named<T>(identifier);
            return this;
        }

        public IScopeContext To(Type type, Enum identifier)
        {
            return To(type, identifier.ToString());
        }

        public IScopeContext To<TImplementation>() where TImplementation : T
        {
            To(typeof (TImplementation));
            return this;
        }

        public IScopeContext To<TImplementation>(string identifier) where TImplementation : T
        {
            To(typeof(TImplementation),identifier);
            return this;
        }

        public IScopeContext ToInstance(T instance, string identifier = "default")
        {
            To(instance.GetType(), identifier);
            return this;
        }

        public IScopeContext To<TImplementation>(Enum identifier) where TImplementation : T
        {
            To(typeof(TImplementation),identifier);
            return this;
        }

        public IScopeContext ToSelf()
        {
            return ToSelf("default");
        }

        public IScopeContext ToSelf(string identifier)
        {
            AfSetting = Configuration.RegisterType(typeof(T)).AsSelf().Named<T>(identifier);
            return this;
        }

        public IScopeContext ToSelf(Enum identifier)
        {
            return ToSelf(identifier.ToString());
        }

        public IScopeContext ToConstructor(Func<object> creator)
        {
            throw new NotImplementedException();
        }

        public IScopeContext ToConstructor(Func<object> creator, string identifier)
        {
            throw new NotImplementedException();
        }

        private  IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> AfSetting;

        
        public IScopeContext SetRequestResponseScope()
        {
            AfSetting.InstancePerMatchingLifetimeScope();
            ActivationScope = Scope.Context;
            return this;
        }

        public IScopeContext SetTransientScope()
        {
            ActivationScope = Scope.PerRequest;
            AfSetting.InstancePerDependency();
            return this;
        }

        public IScopeContext SetSessionScope()
        {
            ActivationScope = Scope.Session;
            AfSetting.InstancePerLifetimeScope();
            return this;
        }

        public IScopeContext SetThreadScope()
        {
            ActivationScope = Scope.Thread;
            AfSetting.InstancePerLifetimeScope();
            return this;
        }

        public IScopeContext SetSingletonScope()
        {
            AfSetting.SingleInstance();
            ActivationScope=Scope.Singleton;
            return this;
        }

        public Scope? ActivationScope { get; private set; }

        public bool AllowOverride { get;set;}

        public bool IsNull
        {
            get { return false; }
        }

        public string ImplementationKey { get; private set; }

        public void SetAllowOverride()
        {
            
        }

        public void DisableOverride()
        {
            
        }
    }
}
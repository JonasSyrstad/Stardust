using System;
using System.Data;
using Stardust.Nucleus.Internals;
using Stardust.Nucleus.ObjectActivator;
using Stardust.Particles;

namespace Stardust.Nucleus
{
    
    public static class ScopeContextActivation
    {
        internal static object Activate(this IScopeContext context)
        {
            return ((IScopeContextInternal)context).Activate(Scope.Context);
        }

        internal static T Activate<T>(this IScopeContextInternal context, Action<T> initializeMethod, Scope? scope = null)
        {
            if (context.IsNull() || context.IsNull) return default(T);
            var instance = (T)ContainerFactory.Current.Resolve(context.BoundType, GetActivationScope<T>(context, scope));
            if (instance.IsNull())
            {
                instance = ActivateAndInitialize(context, initializeMethod);
                ContainerFactory.Current.Bind(instance.GetType(), instance, GetActivationScope<T>(context, scope));
            }
            return instance;
        }

        internal static T Activate<T>(this IScopeContextInternal context, Scope scope)
        {
            return context.Activate<T>(null, GetActivationScope<T>(context, scope));
        }

        internal static object Activate(this IScopeContextInternal context, Scope scope)
        {
            if (context.IsNull() || context.IsNull) return null;
            if (context.AllowOverride || !context.ActivationScope.HasValue)
                return context.BoundType.Activate(scope);
            return context.BoundType.Activate(context.ActivationScope.Value);
        }

        internal static T Activate<T>(this IScopeContextInternal context, Scope scope, Action<T> initializeMethod)
        {
            return context.Activate( initializeMethod, scope);
        }


        private static T ActivateAndInitialize<T>(IScopeContextInternal context, Action<T> initializeMethod)
        {
            var instance = (T) (context.CreatorMethod.IsInstance() ? (T) context.CreatorMethod() : CreateInstance<T>(context));
            if (initializeMethod.IsInstance())
                initializeMethod(instance);
            return instance;
        }

        private static object CreateInstance<T>(IScopeContextInternal context)
        {
            if(context.BoundType.IsGenericTypeDefinition)
                context = CreateConcreteType<T>(context);
            return context.Activate();
        }

        private static IScopeContextInternal CreateConcreteType<T>(IScopeContextInternal context)
        {
            context =
                new ScopeContext(context.BoundType.MakeConcreteType(typeof (T).GetGenericArguments())).SetScope(
                    context.ActivationScope.GetValueOrDefault(ScopeContext.GetDefaultScope()));
            return context;
        }

        private static Scope GetActivationScope<T>(this IScopeContextInternal context)
        {
            return context.ActivationScope ?? Scope.PerRequest;
        }
        private static Scope GetActivationScope<T>(this IScopeContextInternal context, Scope? scope)
        {
            if ((context.AllowOverride || !context.ActivationScope.HasValue) && scope.HasValue)
                return scope.Value;
            return context.GetActivationScope<T>();
        }

        public static object Activate(this Type self, Scope scope)
        {
            if (self.IsNull()) throw new NoNullAllowedException("The type to create was not set.");
            var item = ContainerFactory.Current.Resolve(self, scope);
            if (item.IsNull())
            {
                item = ActivatorFactory.Activator.Activate(self);
                ContainerFactory.Current.Bind(self, item, scope);
            }
            return item;
        }

        internal static T Activate<T>(this Type self, Scope scope, Action<T> initializeMethod)
        {
            if (self.IsNull()) throw new NoNullAllowedException("The type to create was not set.");
            var item = (T)ContainerFactory.Current.Resolve(self, scope);
            if (item.IsNull())
            {
                item = ActivatorFactory.Activator.Activate<T>(self);
                if (initializeMethod.IsInstance()) initializeMethod(item);
                ContainerFactory.Current.Bind(self, item, scope);
            }
            return item;
        }

        internal static T Activate<T>(this IResolveContext<T> self)
        {
            if (self.IsNull()) throw new NoNullAllowedException("The type to create was not set.");
            if (self.TypeContext == null) return default (T);
            var context = ((IScopeContextInternal) self.TypeContext);
            return context.Activate<T>(context.GetActivationScope<T>(), self.Initializer);
        }
    }
}
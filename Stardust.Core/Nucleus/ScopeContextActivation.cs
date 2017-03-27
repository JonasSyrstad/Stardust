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
            if (context.BoundType.IsGenericTypeDefinition)
               context= CreateConcreteType<T>(context);
            var instance = (T)ContainerFactory.Current.Resolve(context.BoundType, GetActivationScope(context, scope));
           
            if (!instance.IsNull()) return instance;    
            instance = ActivateAndInitialize(context, initializeMethod);
            ContainerFactory.Current.Bind(instance.GetType(), instance, GetActivationScope(context, scope));
            return instance;
        }

        internal static T Activate<T>(this IScopeContextInternal context, Scope scope)
        {
            return context.Activate<T>(null, GetActivationScope(context, scope));
        }

        internal static object Activate(this IScopeContextInternal context, Scope scope)
        {
            if (context.IsNull() || context.IsNull) return null;

            var instance = ContainerFactory.Current.Resolve(context.BoundType, GetActivationScope(context, scope));

            if (!instance.IsNull()) return instance;
            instance = ActivateAndInitialize(context, null);
            ContainerFactory.Current.Bind(instance.GetType(), instance, GetActivationScope(context, scope));
            return instance;
        }
    

        internal static T Activate<T>(this IScopeContextInternal context, Scope scope, Action<T> initializeMethod)
        {
            return context.Activate(initializeMethod, scope);
        }


        private static T ActivateAndInitialize<T>(IScopeContextInternal context, Action<T> initializeMethod)
        {
            var instance = (T)(context.CreatorMethod.IsInstance() ? context.CreatorMethod() : CreateInstance(context));
            if (initializeMethod.IsInstance())
                initializeMethod(instance);
            return instance;
        }

        private static object ActivateAndInitialize(IScopeContextInternal context, Action<object> initializeMethod)
        {
            var instance = (context.CreatorMethod.IsInstance() ? context.CreatorMethod() : CreateInstance(context));
            if (initializeMethod.IsInstance())
                initializeMethod(instance);
            return instance;
        }


        //private static object CreateInstance<T>(IScopeContextInternal context)
        //{
        //    if (context.BoundType.IsGenericTypeDefinition)
        //        context = CreateConcreteType(context, context.BoundType);
        //    return context.Activate();
        //}

        private static object CreateInstance(IScopeContextInternal context)
        {
            if (context.BoundType.IsGenericTypeDefinition)
                context = CreateConcreteType(context,context.BoundType);
            return ActivatorFactory.Activator.Activate(context.BoundType);
        }

        private static IScopeContextInternal CreateConcreteType(IScopeContextInternal context,Type serviceType)
        {
            context =
                new ScopeContext(context.BoundType.MakeConcreteType(serviceType.GetGenericArguments())).SetScope(
                    context.ActivationScope.GetValueOrDefault(ScopeContext.GetDefaultScope()));
            return context;
        }

        private static IScopeContextInternal CreateConcreteType<T>(IScopeContextInternal context)
        {
            context =
                new ScopeContext(context.BoundType.MakeConcreteType(typeof(T).GetGenericArguments())).SetScope(
                    context.ActivationScope.GetValueOrDefault(ScopeContext.GetDefaultScope()));
            return context;
        }

        private static Scope GetActivationScope(this IScopeContextInternal context)
        {
            return context.ActivationScope ?? Scope.PerRequest;
        }
        private static Scope GetActivationScope(this IScopeContextInternal context, Scope? scope)
        {
            if ((context.AllowOverride || !context.ActivationScope.HasValue) && scope.HasValue)
                return scope.Value;
            return context.GetActivationScope();
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
            if (self.TypeContext == null) return default(T);
            var context = ((IScopeContextInternal)self.TypeContext);
            return context.Activate(context.GetActivationScope(), self.Initializer);
        }
    }
}
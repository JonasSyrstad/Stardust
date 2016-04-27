using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

namespace Stardust.Interstellar.Rest
{
    public static class ProxyFactory
    {
        public static Type CreateProxy<T>()
        {
            var builder = new ProxyBuilder<T>();
            return builder.Build();
        }

        public static T CreateInstance<T>(string baseUrl)
        {
            var t = CreateProxy<T>();
            var instance = Activator.CreateInstance(t, new NullAuthHandler(), new HeaderHandlerFactory(typeof(T)), TypeWrapper.Create<T>());


            var i = (RestWrapper)instance;
            i.SetBaseUrl(baseUrl);
            return (T)instance;
        }
    }
    public interface IHeaderHandlerFactory
    {
        IEnumerable<IHeaderHandler> GetHandlers();
    }
    public class HeaderHandlerFactory : IHeaderHandlerFactory
    {
        private readonly Type type;

        public HeaderHandlerFactory(Type type)
        {
            this.type = type;
        }

        public IEnumerable<IHeaderHandler> GetHandlers()
        {
            return new List<IHeaderHandler>();
        }
    }

    public class NullAuthHandler : IAuthenticationHandler
    {
        public void Apply(HttpWebRequest req)
        {

        }
    }

    internal class ProxyBuilder<T>
    {
        private AssemblyBuilder myAssemblyBuilder;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public Type Build()
        {
            var myCurrentDomain = AppDomain.CurrentDomain;
            var myAssemblyName = new AssemblyName();
            myAssemblyName.Name = Guid.NewGuid().ToString().Replace("-", "") + "_RestWrapper";
            myAssemblyBuilder = myCurrentDomain.DefineDynamicAssembly(myAssemblyName, AssemblyBuilderAccess.RunAndSave);
            var myModuleBuilder = myAssemblyBuilder.DefineDynamicModule("TempModule", "dyn.dll");
            var type = ReflectionTypeBuilder(myModuleBuilder, typeof(T).Name + "_dynimp");
            ctor(type);
            foreach (var methodInfo in typeof(T).GetMethods())
            {
                if (typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
                    BuildMethodAsync(type, methodInfo);
                else if (methodInfo.ReturnType != typeof(void))
                    BuildMethod(type, methodInfo);
                else
                    BuildVoidMethod(type, methodInfo);
            }

            var result = type.CreateType();
            myAssemblyBuilder.Save("dyn.dll");
            return result;
        }

        private MethodBuilder BuildMethod(TypeBuilder type, MethodInfo serviceAction)
        {
            const MethodAttributes methodAttributes = MethodAttributes.Public
                                                      | MethodAttributes.Virtual
                                                      | MethodAttributes.Final
                                                      | MethodAttributes.HideBySig
                                                      | MethodAttributes.NewSlot;
            var method = type.DefineMethod(serviceAction.Name, methodAttributes);
            // Preparing Reflection instances
            var method1 = typeof(RestWrapper).GetMethod(
                "GetParameters",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]{
                         typeof(string),
                         typeof(object[])
                     },
                null
                );
            MethodInfo method2 = typeof(RestWrapper).GetMethod(
                "Invoke",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]{
                         typeof(string),
                         typeof(ParameterWrapper[])
                     },
                null
                ).MakeGenericMethod(serviceAction.ReturnType);


            // Setting return type

            method.SetReturnType(serviceAction.ReturnType);
            // Adding parameters
            method.SetParameters(serviceAction.GetParameters().Select(p => p.ParameterType).ToArray());
            var i = 1;
            foreach (var parameterInfo in serviceAction.GetParameters())
            {
                var param = method.DefineParameter(i, ParameterAttributes.None, parameterInfo.Name);
                i++;
            }
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder par = gen.DeclareLocal(typeof(Object[]));
            LocalBuilder parameters = gen.DeclareLocal(typeof(ParameterWrapper[]));
            LocalBuilder result = gen.DeclareLocal(serviceAction.ReturnType);
            LocalBuilder str = gen.DeclareLocal(serviceAction.ReturnType);
            // Preparing labels
            Label label55 = gen.DefineLabel();
            // Writing body
            var ps = serviceAction.GetParameters();
            EmitInt32(gen, ps.Length);
            gen.Emit(OpCodes.Newarr, typeof(object));
            for (int j = 0; j < ps.Length; j++)
            {
                gen.Emit(OpCodes.Dup);
                EmitInt32(gen, j);
                EmitLdarg(gen, j + 1);

                var paramType = ps[j].ParameterType;
                if (paramType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, paramType);
                }
                gen.Emit(OpCodes.Stelem_Ref);
            }
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, serviceAction.Name);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, serviceAction.Name);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Call, method2);
            gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Ldloc_2);
            gen.Emit(OpCodes.Stloc_3);
            gen.Emit(OpCodes.Br_S, label55);
            gen.MarkLabel(label55);
            gen.Emit(OpCodes.Ldloc_3);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;

        }

        private MethodBuilder BuildVoidMethod(TypeBuilder type, MethodInfo serviceAction)
        {
            const MethodAttributes methodAttributes = MethodAttributes.Public
                                                      | MethodAttributes.Virtual
                                                      | MethodAttributes.Final
                                                      | MethodAttributes.HideBySig
                                                      | MethodAttributes.NewSlot;
            var method = type.DefineMethod(serviceAction.Name, methodAttributes);
            // Preparing Reflection instances
            var method1 = typeof(RestWrapper).GetMethod(
                "GetParameters",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]{
                         typeof(string),
                         typeof(object[])
                     },
                null
                );
            MethodInfo method2 = typeof(RestWrapper).GetMethod(
                "InvokeVoid",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]{
                         typeof(string),
                         typeof(ParameterWrapper[])
                     },
                null
                );

            // Adding parameters
            method.SetParameters(serviceAction.GetParameters().Select(p => p.ParameterType).ToArray());
            var i = 1;
            foreach (var parameterInfo in serviceAction.GetParameters())
            {
                var param = method.DefineParameter(i, ParameterAttributes.None, parameterInfo.Name);
                i++;
            }
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder par = gen.DeclareLocal(typeof(Object[]));
            LocalBuilder parameters = gen.DeclareLocal(typeof(ParameterWrapper[]));
            // Preparing labels
            Label label55 = gen.DefineLabel();
            // Writing body
            var ps = serviceAction.GetParameters();
            EmitInt32(gen, ps.Length);
            gen.Emit(OpCodes.Newarr, typeof(object));
            for (int j = 0; j < ps.Length; j++)
            {
                gen.Emit(OpCodes.Dup);
                EmitInt32(gen, j);
                EmitLdarg(gen, j + 1);
                var paramType = ps[j].ParameterType;
                if (paramType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, paramType);
                }
                gen.Emit(OpCodes.Stelem_Ref);
            }
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, serviceAction.Name);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, serviceAction.Name);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Call, method2);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;

        }

        private MethodBuilder BuildMethodAsync(TypeBuilder type, MethodInfo serviceAction)
        {
            const MethodAttributes methodAttributes = MethodAttributes.Public
                                                      | MethodAttributes.Virtual
                                                      | MethodAttributes.Final
                                                      | MethodAttributes.HideBySig
                                                      | MethodAttributes.NewSlot;
            var method = type.DefineMethod(serviceAction.Name, methodAttributes);
            // Preparing Reflection instances
            var method1 = typeof(RestWrapper).GetMethod(
                "GetParameters",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]{
                         typeof(string),
                         typeof(object[])
                     },
                null
                );
            
            var method2 = typeof(RestWrapper).GetMethod(serviceAction.ReturnType.GetGenericArguments().Length==0? "InvokeVoidAsync":"InvokeAsync", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(ParameterWrapper[]) }, null);
            if (serviceAction.ReturnType.GenericTypeArguments.Any())
                method2=method2.MakeGenericMethod(serviceAction.ReturnType.GenericTypeArguments);


            // Setting return type

            method.SetReturnType(serviceAction.ReturnType);
            // Adding parameters
            method.SetParameters(serviceAction.GetParameters().Select(p => p.ParameterType).ToArray());
            var i = 1;
            foreach (var parameterInfo in serviceAction.GetParameters())
            {
                var param = method.DefineParameter(i, ParameterAttributes.None, parameterInfo.Name);
                i++;
            }
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder par = gen.DeclareLocal(typeof(Object[]));
            LocalBuilder parameters = gen.DeclareLocal(typeof(ParameterWrapper[]));
            LocalBuilder result = gen.DeclareLocal(serviceAction.ReturnType);
            LocalBuilder str = gen.DeclareLocal(serviceAction.ReturnType);
            // Preparing labels
            Label label55 = gen.DefineLabel();
            // Writing body
            var ps = serviceAction.GetParameters();
            EmitInt32(gen, ps.Length);
            gen.Emit(OpCodes.Newarr, typeof(object));
            for (int j = 0; j < ps.Length; j++)
            {
                gen.Emit(OpCodes.Dup);
                EmitInt32(gen, j);
                EmitLdarg(gen, j + 1);
                var paramType = ps[j].ParameterType;
                if (paramType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, paramType);
                }
                gen.Emit(OpCodes.Stelem_Ref);

            }
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, serviceAction.Name);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, serviceAction.Name);
            gen.Emit(OpCodes.Ldloc_1);
            gen.Emit(OpCodes.Call, method2);
            gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Ldloc_2);
            gen.Emit(OpCodes.Stloc_3);
            gen.Emit(OpCodes.Br_S, label55);
            gen.MarkLabel(label55);
            gen.Emit(OpCodes.Ldloc_3);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        private static void EmitInt32(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1: il.Emit(OpCodes.Ldc_I4_M1); break;
                case 0: il.Emit(OpCodes.Ldc_I4_0); break;
                case 1: il.Emit(OpCodes.Ldc_I4_1); break;
                case 2: il.Emit(OpCodes.Ldc_I4_2); break;
                case 3: il.Emit(OpCodes.Ldc_I4_3); break;
                case 4: il.Emit(OpCodes.Ldc_I4_4); break;
                case 5: il.Emit(OpCodes.Ldc_I4_5); break;
                case 6: il.Emit(OpCodes.Ldc_I4_6); break;
                case 7: il.Emit(OpCodes.Ldc_I4_7); break;
                case 8: il.Emit(OpCodes.Ldc_I4_8); break;
                default:
                    if (value >= -128 && value <= 127)
                    {
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4, value);
                    }
                    break;
            }
        }

        private static void EmitLdarg(ILGenerator il, int value)
        {
            switch (value)
            {

                case 0: il.Emit(OpCodes.Ldarg_0); break;
                case 1: il.Emit(OpCodes.Ldarg_1); break;
                case 2: il.Emit(OpCodes.Ldarg_2); break;
                case 3: il.Emit(OpCodes.Ldarg_3); break;
                default:
                    if (value >= -128 && value <= 127)
                    {
                        il.Emit(OpCodes.Ldarg_S, (sbyte)value);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldarg, value);
                    }
                    break;
            }
        }

        public ConstructorBuilder ctor(TypeBuilder type)
        {
            const MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig;

            var method = type.DefineConstructor(methodAttributes, CallingConventions.Standard | CallingConventions.HasThis, new[] { typeof(IAuthenticationHandler), typeof(IHeaderHandlerFactory), typeof(TypeWrapper) });
            var authenticationHandler = method.DefineParameter(1, ParameterAttributes.None, "authenticationHandler");
            var headerHandlers = method.DefineParameter(2, ParameterAttributes.None, "headerHandlers");
            var interfaceType = method.DefineParameter(3, ParameterAttributes.None, "interfaceType");
            var ctor1 = typeof(RestWrapper).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(IAuthenticationHandler), typeof(IHeaderHandlerFactory), typeof(TypeWrapper) }, null);

            var gen = method.GetILGenerator();
            // Writing body
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldarg_2);
            gen.Emit(OpCodes.Ldarg_3);
            gen.Emit(OpCodes.Call, ctor1);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ret);

            // finished
            return method;
        }



        private TypeBuilder ReflectionTypeBuilder(ModuleBuilder module, string typeName)
        {
            try
            {
                var type = module.DefineType("TempModule." + typeName,
                    TypeAttributes.Public | TypeAttributes.Class,
                    typeof(RestWrapper),
                    new[] { typeof(T) }
                    );
                return type;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
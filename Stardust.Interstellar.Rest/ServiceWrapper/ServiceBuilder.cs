using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Web.Http;

namespace Stardust.Interstellar.Rest.ServiceWrapper
{
    class ServiceBuilder
    {
        private AssemblyBuilder myAssemblyBuilder;

        private ModuleBuilder myModuleBuilder;

        public ServiceBuilder()
        {
            var myCurrentDomain = AppDomain.CurrentDomain;
            var myAssemblyName = new AssemblyName();
            myAssemblyName.Name = Guid.NewGuid().ToString().Replace("-", "") + "_ServiceWrapper";
            myAssemblyBuilder = myCurrentDomain.DefineDynamicAssembly(myAssemblyName, AssemblyBuilderAccess.RunAndSave);
            myModuleBuilder = myAssemblyBuilder.DefineDynamicModule("TempModule", "service.dll");
        }

        public Type CreateServiceImplementation<T>()
        {
            return CreateServiceImplementation(typeof(T));
        }

        public Type CreateServiceImplementation(Type interfaceType)
        {
            try
            {
                var type = CreateServiceType(interfaceType);
                ctor(type, interfaceType);
                foreach (var methodInfo in interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
                    {
                        if (methodInfo.ReturnType.GetGenericArguments().Length == 0)
                        {
                            BuildAsyncVoidMethod(type, methodInfo);
                        }
                        else BuildAsyncMethod(type, methodInfo);
                    }
                    else
                    {
                        if (methodInfo.ReturnType == typeof(void)) BuildVoidMethod(type, methodInfo);
                        else BuildMethod(type, methodInfo);
                    }
                }
                return type.CreateType();
            }
            catch (Exception ex)
            {
                throw;
            }
        }



        public MethodBuilder BuildMethod(TypeBuilder type, MethodInfo implementationMethod)
        {
            // Declaring method builder
            // Method attributes
            const MethodAttributes methodAttributes = MethodAttributes.Public
                                                      | MethodAttributes.Virtual
                                                      | MethodAttributes.Final
                                                      | MethodAttributes.HideBySig
                                                      | MethodAttributes.NewSlot;
            var method = type.DefineMethod(implementationMethod.Name, methodAttributes);
            // Preparing Reflection instances
            #region MyRegion
            ConstructorInfo route = typeof(RouteAttribute).GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new Type[]{
                              typeof(String)
                              },
                    null
                    );
            ConstructorInfo httpGet = httpMethodAttribute(implementationMethod);
            ConstructorInfo uriAttrib = typeof(FromUriAttribute).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                          },
                null
                );
            ConstructorInfo bodyAttrib = typeof(FromBodyAttribute).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                          },
                null
                );
            MethodInfo gatherParams = typeof(Stardust.Interstellar.Rest.ServiceWrapper.ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod(
                "GatherParameters",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                              typeof(String),
                              typeof(Object[])
                          },
                null
                );
            var baseType = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType);
            var implementation = baseType.GetRuntimeFields().Single(f => f.Name == "implementation");
            MethodInfo getValue = typeof(ParameterWrapper).GetMethod(
                "get_value",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                          },
                null
                );
            MethodInfo createResponse = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod("CreateResponse",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
            if (implementationMethod.ReturnType == typeof(void))
                createResponse = createResponse.MakeGenericMethod(typeof(object));
            else
                createResponse = createResponse.MakeGenericMethod(implementationMethod.ReturnType);
            MethodInfo method9 = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod(
                "CreateErrorResponse",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                              typeof(Exception)
                          },
                null
                );
            #endregion

            // Setting return type
            method.SetReturnType(typeof(HttpResponseMessage));
            // Adding parameters
            var methodParams = new List<ParameterWrapper>();
            foreach (var parameterInfo in implementationMethod.GetParameters())
            {
                var @in = parameterInfo.GetCustomAttribute<InAttribute>(true);
                if (@in == null)
                {
                    var fromBody = parameterInfo.GetCustomAttribute<FromBodyAttribute>(true);
                    if (fromBody != null)
                        @in = new InAttribute(InclutionTypes.Body);
                    if (@in == null)
                    {
                        var fromUri = parameterInfo.GetCustomAttribute<FromUriAttribute>(true);
                        if (fromUri != null)
                            @in = new InAttribute(InclutionTypes.Path);
                    }
                }
                methodParams.Add(new ParameterWrapper { Name = parameterInfo.Name, Type = parameterInfo.ParameterType, In = @in?.InclutionType ?? InclutionTypes.Body });
            }
            var pTypes = methodParams.Select(p => p.Type).ToArray();
            method.SetParameters(pTypes.ToArray());
            // Parameter id
            int pid = 1;
            foreach (var parameterWrapper in methodParams)
            {

                try
                {
                    var p = method.DefineParameter(pid, ParameterAttributes.None, parameterWrapper.Name);
                    if (parameterWrapper.In == InclutionTypes.Path)
                        p.SetCustomAttribute(new CustomAttributeBuilder(uriAttrib, new Type[] { }));
                    else if (parameterWrapper.In == InclutionTypes.Body)
                        p.SetCustomAttribute(new CustomAttributeBuilder(bodyAttrib, new Type[] { }));
                    pid++;
                }
                catch (Exception)
                {

                    throw;
                }
            }
            // Adding custom attributes to method
            // [RouteAttribute]
            method.SetCustomAttribute(
                new CustomAttributeBuilder(
                    route,
                    new[]{
                             implementationMethod.GetCustomAttribute<RouteAttribute>().Template
                         }
                    )
                );
            // [HttpGetAttribute]
            method.SetCustomAttribute(
                new CustomAttributeBuilder(
                    httpGet,
                    new Type[] { }
                    )
                );
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder parameters = gen.DeclareLocal(typeof(Object[]));
            LocalBuilder serviceParameters = gen.DeclareLocal(typeof(ParameterWrapper[]));
            LocalBuilder result = gen.DeclareLocal(typeof(String));
            LocalBuilder message = gen.DeclareLocal(typeof(HttpResponseMessage));
            LocalBuilder ex = gen.DeclareLocal(typeof(Exception));
            // Preparing labels
            Label label97 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.BeginExceptionBlock();
            gen.Emit(OpCodes.Nop);
            var ps = pTypes;
            EmitHelpers.EmitInt32(gen, ps.Length);
            gen.Emit(OpCodes.Newarr, typeof(object));
            for (int j = 0; j < ps.Length; j++)
            {
                gen.Emit(OpCodes.Dup);
                EmitHelpers.EmitInt32(gen, j);
                EmitHelpers.EmitLdarg(gen, j + 1);
                var paramType = ps[j];
                if (paramType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, paramType);
                }
                gen.Emit(OpCodes.Stelem_Ref);

            }
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, implementationMethod.Name);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Call, gatherParams);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, implementation);
            int iii = 0;
            foreach (var parameterWrapper in methodParams)
            {
                gen.Emit(OpCodes.Ldloc_1);
                EmitHelpers.EmitInt32(gen, iii);//gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Ldelem_Ref);
                gen.Emit(OpCodes.Callvirt, getValue);
                if (parameterWrapper.Type.IsValueType)
                    gen.Emit(OpCodes.Unbox_Any, parameterWrapper.Type);
                else
                    gen.Emit(OpCodes.Castclass, parameterWrapper.Type);
                iii++;
            }
            gen.Emit(OpCodes.Callvirt, implementationMethod);
            gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4, 200);
            if (implementationMethod.ReturnType != typeof(void))
                gen.Emit(OpCodes.Ldloc_2);
            gen.Emit(OpCodes.Call, createResponse);
            gen.Emit(OpCodes.Stloc_3);
            gen.Emit(OpCodes.Leave_S, label97);
            gen.BeginCatchBlock(typeof(Exception));
            gen.Emit(OpCodes.Stloc_S, 4);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldloc_S, 4);
            gen.Emit(OpCodes.Call, method9);
            gen.Emit(OpCodes.Stloc_3);
            gen.Emit(OpCodes.Leave_S, label97);
            gen.EndExceptionBlock();
            gen.MarkLabel(label97);
            gen.Emit(OpCodes.Ldloc_3);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        public MethodBuilder BuildVoidMethod(TypeBuilder type, MethodInfo implementationMethod)
        {
            // Declaring method builder
            // Method attributes
            const MethodAttributes methodAttributes = MethodAttributes.Public
                                                      | MethodAttributes.Virtual
                                                      | MethodAttributes.Final
                                                      | MethodAttributes.HideBySig
                                                      | MethodAttributes.NewSlot;
            var method = type.DefineMethod(implementationMethod.Name, methodAttributes);
            // Preparing Reflection instances
            #region MyRegion
            ConstructorInfo route = typeof(RouteAttribute).GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new Type[]{
                              typeof(String)
                              },
                    null
                    );
            var httpGet = httpMethodAttribute(implementationMethod);
            ConstructorInfo uriAttrib = typeof(FromUriAttribute).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                          },
                null
                );
            ConstructorInfo bodyAttrib = typeof(FromBodyAttribute).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                          },
                null
                );
            MethodInfo gatherParams = typeof(Stardust.Interstellar.Rest.ServiceWrapper.ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod(
                "GatherParameters",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                              typeof(String),
                              typeof(Object[])
                          },
                null
                );
            var baseType = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType);
            var implementation = baseType.GetRuntimeFields().Single(f => f.Name == "implementation");
            MethodInfo getValue = typeof(ParameterWrapper).GetMethod(
                "get_value",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                          },
                null
                );
            MethodInfo createResponse = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod("CreateResponse", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (implementationMethod.ReturnType == typeof(void))
                createResponse = createResponse.MakeGenericMethod(typeof(object));
            else
                createResponse = createResponse.MakeGenericMethod(implementationMethod.ReturnType);
            MethodInfo method9 = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod(
                "CreateErrorResponse",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                              typeof(Exception)
                          },
                null
                );
            #endregion

            // Setting return type
            method.SetReturnType(typeof(HttpResponseMessage));
            // Adding parameters
            var methodParams = new List<ParameterWrapper>();
            foreach (var parameterInfo in implementationMethod.GetParameters())
            {
                var @in = parameterInfo.GetCustomAttribute<InAttribute>(true);
                if (@in == null)
                {
                    var fromBody = parameterInfo.GetCustomAttribute<FromBodyAttribute>(true);
                    if (fromBody != null)
                        @in = new InAttribute(InclutionTypes.Body);
                    if (@in == null)
                    {
                        var fromUri = parameterInfo.GetCustomAttribute<FromUriAttribute>(true);
                        if (fromUri != null)
                            @in = new InAttribute(InclutionTypes.Path);
                    }
                }
                methodParams.Add(new ParameterWrapper { Name = parameterInfo.Name, Type = parameterInfo.ParameterType, In = @in?.InclutionType ?? InclutionTypes.Body });
            }
            var pTypes = methodParams.Select(p => p.Type).ToArray();
            method.SetParameters(pTypes.ToArray());
            // Parameter id
            int pid = 1;
            foreach (var parameterWrapper in methodParams)
            {

                try
                {
                    var p = method.DefineParameter(pid, ParameterAttributes.None, parameterWrapper.Name);
                    if (parameterWrapper.In == InclutionTypes.Path)
                        p.SetCustomAttribute(new CustomAttributeBuilder(uriAttrib, new Type[] { }));
                    else if (parameterWrapper.In == InclutionTypes.Body)
                        p.SetCustomAttribute(new CustomAttributeBuilder(bodyAttrib, new Type[] { }));
                    pid++;
                }
                catch (Exception)
                {

                    throw;
                }
            }
            // Adding custom attributes to method
            // [RouteAttribute]
            method.SetCustomAttribute(
                new CustomAttributeBuilder(
                    route,
                    new[]{
                             implementationMethod.GetCustomAttribute<RouteAttribute>().Template
                         }
                    )
                );
            // [HttpGetAttribute]
            method.SetCustomAttribute(
                new CustomAttributeBuilder(
                    httpGet,
                    new Type[] { }
                    )
                );
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder parameters = gen.DeclareLocal(typeof(Object[]));
            LocalBuilder serviceParameters = gen.DeclareLocal(typeof(ParameterWrapper[]));
            LocalBuilder message = gen.DeclareLocal(typeof(HttpResponseMessage));
            LocalBuilder ex = gen.DeclareLocal(typeof(Exception));
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.BeginExceptionBlock();
            gen.Emit(OpCodes.Nop);
            var ps = pTypes;
            EmitHelpers.EmitInt32(gen, ps.Length);
            gen.Emit(OpCodes.Newarr, typeof(object));
            for (int j = 0; j < ps.Length; j++)
            {
                gen.Emit(OpCodes.Dup);
                EmitHelpers.EmitInt32(gen, j);
                EmitHelpers.EmitLdarg(gen, j + 1);
                var paramType = ps[j];
                if (paramType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, paramType);
                }
                gen.Emit(OpCodes.Stelem_Ref);

            }
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, implementationMethod.Name);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Call, gatherParams);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, implementation);
            int iii = 0;
            foreach (var parameterWrapper in methodParams)
            {
                gen.Emit(OpCodes.Ldloc_1);
                EmitHelpers.EmitInt32(gen, iii);//gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Ldelem_Ref);
                gen.Emit(OpCodes.Callvirt, getValue);
                if (parameterWrapper.Type.IsValueType)
                {
                    gen.Emit(OpCodes.Unbox_Any, parameterWrapper.Type);

                }
                else
                    gen.Emit(OpCodes.Castclass, parameterWrapper.Type);
                iii++;
            }
            gen.Emit(OpCodes.Callvirt, implementationMethod);
            if (implementationMethod.ReturnType != typeof(void))
                gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4, 200);
            if (implementationMethod.ReturnType != typeof(void))
                gen.Emit(OpCodes.Ldloc_2);
            else { gen.Emit(OpCodes.Ldnull); }

            gen.Emit(OpCodes.Call, createResponse);
            gen.Emit(OpCodes.Stloc_2);
            gen.BeginCatchBlock(typeof(Exception));
            gen.Emit(OpCodes.Stloc_3);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldloc_3);
            gen.Emit(OpCodes.Call, method9);
            gen.Emit(OpCodes.Stloc_2);
            gen.EndExceptionBlock();
            gen.Emit(OpCodes.Ldloc_2);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        private static ConstructorInfo httpMethodAttribute(MethodInfo implementationMethod)
        {
            var attribs = implementationMethod.GetCustomAttributes();
            if (attribs.Any(p => p is HttpGetAttribute))
                return typeof(HttpGetAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
            if (attribs.Any(p => p is HttpPostAttribute))
                return typeof(HttpPostAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
            if (attribs.Any(p => p is HttpPutAttribute))
                return typeof(HttpPutAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
            if (attribs.Any(p => p is HttpDeleteAttribute))
                return typeof(HttpDeleteAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);
            return typeof(HttpGetAttribute).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null);

        }

        public MethodBuilder BuildAsyncMethod(TypeBuilder type, MethodInfo implementationMethod)
        {
            // Declaring method builder
            // Method attributes
            const MethodAttributes methodAttributes = MethodAttributes.Public
                                                      | MethodAttributes.Virtual
                                                      | MethodAttributes.Final
                                                      | MethodAttributes.HideBySig
                                                      | MethodAttributes.NewSlot;
            var method = type.DefineMethod(implementationMethod.Name, methodAttributes);
            // Preparing Reflection instances
            #region MyRegion
            ConstructorInfo route = typeof(RouteAttribute).GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new Type[]{
                              typeof(String)
                              },
                    null
                    );
            ConstructorInfo httpGet = httpMethodAttribute(implementationMethod);
            ConstructorInfo uriAttrib = typeof(FromUriAttribute).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                          },
                null
                );
            ConstructorInfo bodyAttrib = typeof(FromBodyAttribute).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                          },
                null
                );
            MethodInfo gatherParams = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod(
                "GatherParameters",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                              typeof(string),
                              typeof(object[])
                          },
                null
                );
            var baseType = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType);
            var implementation = baseType.GetRuntimeFields().Single(f => f.Name == "implementation");
            MethodInfo getValue = typeof(ParameterWrapper).GetMethod(
                "get_value",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                          },
                null
                );
            MethodInfo createResponse = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod("CreateResponseAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            createResponse = createResponse.MakeGenericMethod(implementationMethod.ReturnType.GetGenericArguments());
            MethodInfo method9 = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod(
                "CreateErrorResponse",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                              typeof(Exception)
                          },
                null
                );
            MethodInfo fromResult = typeof(Task).GetMethod("FromResult").MakeGenericMethod(typeof(HttpResponseMessage));

            #endregion

            // Setting return type
            method.SetReturnType(typeof(Task<HttpResponseMessage>));
            // Adding parameters
            var methodParams = new List<ParameterWrapper>();
            foreach (var parameterInfo in implementationMethod.GetParameters())
            {
                var @in = parameterInfo.GetCustomAttribute<InAttribute>(true);
                if (@in == null)
                {
                    var fromBody = parameterInfo.GetCustomAttribute<FromBodyAttribute>(true);
                    if (fromBody != null)
                        @in = new InAttribute(InclutionTypes.Body);
                    if (@in == null)
                    {
                        var fromUri = parameterInfo.GetCustomAttribute<FromUriAttribute>(true);
                        if (fromUri != null)
                            @in = new InAttribute(InclutionTypes.Path);
                    }
                }
                methodParams.Add(new ParameterWrapper { Name = parameterInfo.Name, Type = parameterInfo.ParameterType, In = @in?.InclutionType ?? InclutionTypes.Body });
            }
            var pTypes = methodParams.Select(p => p.Type).ToArray();
            method.SetParameters(pTypes.ToArray());
            // Parameter id
            int pid = 1;
            foreach (var parameterWrapper in methodParams)
            {

                try
                {
                    var p = method.DefineParameter(pid, ParameterAttributes.None, parameterWrapper.Name);
                    if (parameterWrapper.In == InclutionTypes.Path)
                        p.SetCustomAttribute(new CustomAttributeBuilder(uriAttrib, new Type[] { }));
                    else if (parameterWrapper.In == InclutionTypes.Body)
                        p.SetCustomAttribute(new CustomAttributeBuilder(bodyAttrib, new Type[] { }));
                    pid++;
                }
                catch (Exception)
                {

                    throw;
                }
            }
            // Adding custom attributes to method
            // [RouteAttribute]
            method.SetCustomAttribute(
                new CustomAttributeBuilder(
                    route,
                    new[]{
                             implementationMethod.GetCustomAttribute<RouteAttribute>().Template
                         }
                    )
                );
            // [HttpGetAttribute]
            method.SetCustomAttribute(
                new CustomAttributeBuilder(
                    httpGet,
                    new Type[] { }
                    )
                );
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder parameters = gen.DeclareLocal(typeof(Object[]));
            LocalBuilder serviceParameters = gen.DeclareLocal(typeof(ParameterWrapper[]));
            LocalBuilder result = gen.DeclareLocal(typeof(Task<>).MakeGenericType(implementationMethod.ReturnType.GenericTypeArguments));
            LocalBuilder message = gen.DeclareLocal(typeof(Task<HttpResponseMessage>));
            LocalBuilder ex = gen.DeclareLocal(typeof(Exception));
            // Preparing labels
            Label label97 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.BeginExceptionBlock();
            gen.Emit(OpCodes.Nop);
            var ps = pTypes;
            EmitHelpers.EmitInt32(gen, ps.Length);
            gen.Emit(OpCodes.Newarr, typeof(object));
            for (int j = 0; j < ps.Length; j++)
            {
                gen.Emit(OpCodes.Dup);
                EmitHelpers.EmitInt32(gen, j);
                EmitHelpers.EmitLdarg(gen, j + 1);
                var paramType = ps[j];
                if (paramType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, paramType);
                }
                gen.Emit(OpCodes.Stelem_Ref);

            }
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, implementationMethod.Name);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Call, gatherParams);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, implementation);
            int iii = 0;
            foreach (var parameterWrapper in methodParams)
            {
                gen.Emit(OpCodes.Ldloc_1);
                EmitHelpers.EmitInt32(gen, iii);//gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Ldelem_Ref);
                gen.Emit(OpCodes.Callvirt, getValue);
                if (parameterWrapper.Type.IsValueType)
                    gen.Emit(OpCodes.Unbox_Any, parameterWrapper.Type);
                else
                    gen.Emit(OpCodes.Castclass, parameterWrapper.Type);
                iii++;
            }
            gen.Emit(OpCodes.Callvirt, implementationMethod);
            gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4, 200);
            if (implementationMethod.ReturnType != typeof(void))
                gen.Emit(OpCodes.Ldloc_2);
            gen.Emit(OpCodes.Call, createResponse);
            gen.Emit(OpCodes.Stloc_3);
            gen.Emit(OpCodes.Leave_S, label97);
            gen.BeginCatchBlock(typeof(Exception));
            gen.Emit(OpCodes.Stloc_S, 4);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldloc_S, 4);
            gen.Emit(OpCodes.Call, method9);
            gen.Emit(OpCodes.Call, fromResult);
            gen.Emit(OpCodes.Stloc_3);
            gen.Emit(OpCodes.Leave_S, label97);
            gen.EndExceptionBlock();
            gen.MarkLabel(label97);
            gen.Emit(OpCodes.Ldloc_3);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        public MethodBuilder BuildAsyncVoidMethod(TypeBuilder type, MethodInfo implementationMethod)
        {
            // Declaring method builder
            // Method attributes
            const MethodAttributes methodAttributes = MethodAttributes.Public
                                                      | MethodAttributes.Virtual
                                                      | MethodAttributes.Final
                                                      | MethodAttributes.HideBySig
                                                      | MethodAttributes.NewSlot;
            var method = type.DefineMethod(implementationMethod.Name, methodAttributes);
            // Preparing Reflection instances
            #region MyRegion
            ConstructorInfo route = typeof(RouteAttribute).GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new Type[]{
                              typeof(String)
                              },
                    null
                    );
            ConstructorInfo httpGet = httpMethodAttribute(implementationMethod);
            ConstructorInfo uriAttrib = typeof(FromUriAttribute).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                          },
                null
                );
            ConstructorInfo bodyAttrib = typeof(FromBodyAttribute).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                          },
                null
                );
            MethodInfo gatherParams = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType).GetMethod(
                "GatherParameters",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                              typeof(string),
                              typeof(object[])
                          },
                null
                );
            var baseType = typeof(ServiceWrapperBase<>).MakeGenericType(implementationMethod.DeclaringType);
            var implementation = baseType.GetRuntimeFields().Single(f => f.Name == "implementation");
            MethodInfo getValue = typeof(ParameterWrapper).GetMethod(
                "get_value",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                          },
                null
                );
            MethodInfo createResponse = baseType.GetMethod("CreateResponseVoidAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo method9 = baseType.GetMethod(
                "CreateErrorResponse",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[]{
                              typeof(Exception)
                          },
                null
                );
            MethodInfo fromResult = typeof(Task).GetMethod("FromResult").MakeGenericMethod(typeof(HttpResponseMessage));

            #endregion

            // Setting return type
            method.SetReturnType(typeof(Task<HttpResponseMessage>));
            // Adding parameters
            var methodParams = new List<ParameterWrapper>();
            foreach (var parameterInfo in implementationMethod.GetParameters())
            {
                var @in = parameterInfo.GetCustomAttribute<InAttribute>(true);
                if (@in == null)
                {
                    var fromBody = parameterInfo.GetCustomAttribute<FromBodyAttribute>(true);
                    if (fromBody != null)
                        @in = new InAttribute(InclutionTypes.Body);
                    if (@in == null)
                    {
                        var fromUri = parameterInfo.GetCustomAttribute<FromUriAttribute>(true);
                        if (fromUri != null)
                            @in = new InAttribute(InclutionTypes.Path);
                    }
                }
                methodParams.Add(new ParameterWrapper { Name = parameterInfo.Name, Type = parameterInfo.ParameterType, In = @in?.InclutionType ?? InclutionTypes.Body });
            }
            var pTypes = methodParams.Select(p => p.Type).ToArray();
            method.SetParameters(pTypes.ToArray());
            // Parameter id
            int pid = 1;
            foreach (var parameterWrapper in methodParams)
            {

                try
                {
                    var p = method.DefineParameter(pid, ParameterAttributes.None, parameterWrapper.Name);
                    if (parameterWrapper.In == InclutionTypes.Path)
                        p.SetCustomAttribute(new CustomAttributeBuilder(uriAttrib, new Type[] { }));
                    else if (parameterWrapper.In == InclutionTypes.Body)
                        p.SetCustomAttribute(new CustomAttributeBuilder(bodyAttrib, new Type[] { }));
                    pid++;
                }
                catch (Exception)
                {

                    throw;
                }
            }
            // Adding custom attributes to method
            // [RouteAttribute]
            method.SetCustomAttribute(
                new CustomAttributeBuilder(
                    route,
                    new[]{
                             implementationMethod.GetCustomAttribute<RouteAttribute>().Template
                         }
                    )
                );
            // [HttpGetAttribute]
            method.SetCustomAttribute(
                new CustomAttributeBuilder(
                    httpGet,
                    new Type[] { }
                    )
                );
            ILGenerator gen = method.GetILGenerator();
            // Preparing locals
            LocalBuilder parameters = gen.DeclareLocal(typeof(Object[]));
            LocalBuilder serviceParameters = gen.DeclareLocal(typeof(ParameterWrapper[]));
            LocalBuilder result = gen.DeclareLocal(typeof(Task));
            LocalBuilder message = gen.DeclareLocal(typeof(Task<HttpResponseMessage>));
            LocalBuilder ex = gen.DeclareLocal(typeof(Exception));
            // Preparing labels
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.BeginExceptionBlock();
            gen.Emit(OpCodes.Nop);
            var ps = pTypes;
            EmitHelpers.EmitInt32(gen, ps.Length);
            gen.Emit(OpCodes.Newarr, typeof(object));
            for (int j = 0; j < ps.Length; j++)
            {
                gen.Emit(OpCodes.Dup);
                EmitHelpers.EmitInt32(gen, j);
                EmitHelpers.EmitLdarg(gen, j + 1);
                var paramType = ps[j];
                if (paramType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, paramType);
                }
                gen.Emit(OpCodes.Stelem_Ref);

            }
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, implementationMethod.Name);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Call, gatherParams);
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, implementation);
            int iii = 0;
            foreach (var parameterWrapper in methodParams)
            {
                gen.Emit(OpCodes.Ldloc_1);
                EmitHelpers.EmitInt32(gen, iii);
                gen.Emit(OpCodes.Ldelem_Ref);
                gen.Emit(OpCodes.Callvirt, getValue);
                if (parameterWrapper.Type.IsValueType)
                    gen.Emit(OpCodes.Unbox_Any, parameterWrapper.Type);
                else
                    gen.Emit(OpCodes.Castclass, parameterWrapper.Type);
                iii++;
            }
            gen.Emit(OpCodes.Callvirt, implementationMethod);
            gen.Emit(OpCodes.Stloc_2);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4, 200);
            gen.Emit(OpCodes.Ldloc_2);
            gen.Emit(OpCodes.Call, createResponse);
            gen.Emit(OpCodes.Stloc_3);
            gen.BeginCatchBlock(typeof(Exception));
            gen.Emit(OpCodes.Stloc_S, 4);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldloc_S, 4);
            gen.Emit(OpCodes.Call, method9);
            gen.Emit(OpCodes.Call, fromResult);
            gen.Emit(OpCodes.Stloc_3);
            gen.EndExceptionBlock();
            gen.Emit(OpCodes.Ldloc_3);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        public ConstructorBuilder ctor(TypeBuilder type, Type interfaceType)
        {
            const MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig;

            var method = type.DefineConstructor(methodAttributes, CallingConventions.Standard | CallingConventions.HasThis, new[] { interfaceType });
            var implementation = method.DefineParameter(1, ParameterAttributes.None, "implementation");
            var ctor1 = typeof(ServiceWrapperBase<>).MakeGenericType(interfaceType).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { interfaceType }, null);

            var gen = method.GetILGenerator();
            // Writing body
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, ctor1);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ret);

            // finished
            return method;
        }

        private TypeBuilder CreateServiceType(Type interfaceType)
        {
            var routePrefix = interfaceType.GetCustomAttribute<IRoutePrefixAttribute>();
            var type = myModuleBuilder.DefineType("TempModule.Controllers." + interfaceType.Name.Remove(0, 1)+"Controller", TypeAttributes.Public | TypeAttributes.Class, typeof(ServiceWrapperBase<>).MakeGenericType(interfaceType));
            if (routePrefix!=null)  
            {
                var routePrefixCtor = typeof(RoutePrefixAttribute).GetConstructor(
                           BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                           null,
                           new Type[]{
                        typeof(String)
                               },
                           null
                           );

                type.SetCustomAttribute(new CustomAttributeBuilder(routePrefixCtor, new object[] { routePrefix.Prefix })); 
            }
            return type;
        }

        public void Save()
        {
            myAssemblyBuilder.Save("service.dll");
        }

        public Assembly GetCustomAssembly()
        {
            return myModuleBuilder.Assembly; 
        }
    }
}
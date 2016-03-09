using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Web.Http;

namespace Stardust.Rest.Client
{
    public class RestifyBuilder<T>
    {
        private const string InvokeName = "InvokeMember";

        private readonly Type baseType;

        private readonly Type sessionHandler;

        public RestifyBuilder(Type baseType, Type sessionHandler)
        {
            this.baseType = baseType;
            this.sessionHandler = sessionHandler;
        }

        private AssemblyBuilder myAssemblyBuilder;

        private MethodInfo method18;

        internal Type CreateRestProxy()
        {

            var myCurrentDomain = AppDomain.CurrentDomain;
            var myAssemblyName = new AssemblyName();
            myAssemblyName.Name = Guid.NewGuid().ToString().Replace("-", "") + "_RestWrapper";
            this.myAssemblyBuilder = myCurrentDomain.DefineDynamicAssembly(myAssemblyName, AssemblyBuilderAccess.Run);
            var myModuleBuilder = this.myAssemblyBuilder.DefineDynamicModule("TempModule");
            var type = this.ReflectionTypeBuilder(myModuleBuilder, typeof(T).Name + "_dynimp");
            var methodBuilder = this.ctor(type);
            foreach (var info in typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (info.ReturnType.IsAssignableFrom(typeof(Task)))//.Implements(typeof(Task)))
                {
                    BuildMethodAsyncExecute(type, info);
                }
                else
                    BuildMethodExecute(type, info);
            }

            var t = type.CreateType();
            return t;
        }



        #region SyncMethodCreation
        public MethodBuilder BuildMethodExecute(TypeBuilder type, MethodInfo methodInfo)
        {
            var httpMethod = GetHttpMethod(methodInfo);
            var method = this.CreateMethodSignature(type, methodInfo);
            var gen = method.GetILGenerator();
            var localBuilder = gen.DeclareLocal(methodInfo.ReturnType);
            var getResultType = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(RuntimeTypeHandle) }, null);
            var execute = this.baseType.GetMethod(InvokeName + httpMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(object), typeof(Type) }, null);
            var label26 = gen.DefineLabel();
            WriteExecuteBody(methodInfo, gen, getResultType, execute, label26);
            return method;
        }

        private static void WriteExecuteBody(MethodInfo methodInfo, ILGenerator gen, MethodInfo getResultType, MethodInfo execute, Label label26)
        {
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldtoken, methodInfo.ReturnType);
            gen.Emit(OpCodes.Call, getResultType);
            gen.Emit(OpCodes.Call, execute);
            gen.Emit(OpCodes.Castclass, methodInfo.ReturnType);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label26);
            gen.MarkLabel(label26);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
        }

        private MethodBuilder CreateMethodSignature(TypeBuilder type, MethodInfo methodInfo)
        {
            // Declaring method builder
            // Method attributes
            var methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final
                                   | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
            var method = type.DefineMethod(methodInfo.Name, methodAttributes);
            // Preparing Reflection instances

            // Setting return type
            method.SetReturnType(methodInfo.ReturnType);
            // Adding parameters
            method.SetParameters(methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());
            // Parameter request
            var request = method.DefineParameter(1, ParameterAttributes.None, "request");
            return method;
        }

        internal TypeBuilder ReflectionTypeBuilder(ModuleBuilder module, string typeName)
        {
            try
            {
                var type = module.DefineType("TempModule." + typeName,
                        TypeAttributes.Public,
                        this.baseType,
                        new[] { typeof(T) }
                        );
                return type;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        #endregion
        #region AsyncMethod
        public MethodBuilder BuildMethodAsyncExecute(TypeBuilder type, MethodInfo info)
        {
            var httpMethod = GetHttpMethod(info);
            var resultType = info.ReturnType.GenericTypeArguments[0];
            var methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
            var method = type.DefineMethod(info.Name, methodAttributes);
            // Preparing Reflection instances
            var method1 = baseType.GetMethod(
                "RunTyped" + httpMethod + "Async",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]{
            typeof(object)
            },
                null
                );

            method1 = method1.MakeGenericMethod(info.ReturnType.GenericTypeArguments);


            // Setting return type
            method.SetReturnType(info.ReturnType);
            // Adding parameters
            method.SetParameters(info.GetParameters().Select(p => p.ParameterType).ToArray());
            // Parameter request
            var request = method.DefineParameter(1, ParameterAttributes.None, info.GetParameters().First().Name);
            var gen = method.GetILGenerator();
            // Preparing locals
            var localBuilder = gen.DeclareLocal(info.ReturnType);
            // Preparing labels
            var label11 = gen.DefineLabel();
            // Writing body
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Br_S, label11);
            gen.MarkLabel(label11);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }

        private static string GetHttpMethod(MethodInfo info)
        {
            var httpMethod = "Get";
            var postAttrib = info.GetCustomAttribute<HttpPostAttribute>();
            if (postAttrib != null)
            {
                httpMethod = "Post";
            }
            var putAttrib = info.GetCustomAttribute<HttpPutAttribute>();
            if (putAttrib != null)
            {
                httpMethod = "Put";
            }
            var deleteAttrib = info.GetCustomAttribute<HttpDeleteAttribute>();
            if (deleteAttrib != null)
            {
                httpMethod = "Delete";
            }
            return httpMethod;
        }

        #endregion

        private ConstructorBuilder ctor(TypeBuilder type)
        {

            const MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig;

            var method = type.DefineConstructor(methodAttributes, CallingConventions.Any, new[] { sessionHandler });
            var ctor1 = baseType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { sessionHandler }, null);

            // Parameter session
            var session = method.DefineParameter(1, ParameterAttributes.None, "session");
            var gen = method.GetILGenerator();
            // Writing body
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Call, ctor1);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Nop);
            gen.Emit(OpCodes.Ret);
            // finished
            return method;
        }
    }
}

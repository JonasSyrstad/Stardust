using System;

namespace Stardust.Accretion
{
    public interface ICodeWriterContext
    {
        ICodeFileWriterContext Class(string name, Action<ICodeWriterContext> body);

        ICodeWriterContext InternalCtor();

        ICodeWriterContext Property<T>(string name, Action<ICodeWriterContext> get, Action<ICodeWriterContext> set);

        ICodeWriterContext Property(string type, string name, Action<ICodeWriterContext> get, Action<ICodeWriterContext> set);
        void Return(string code);

        void Var(string name, string value);

        ICodeWriterContext If(string clause, Action<ICodeWriterContext> body);

        ICodeWriterContext ElseIf(string clause, Action<ICodeWriterContext> body);

        void Else(Action<ICodeWriterContext> body);

        void Field(string type, string name, string initializer);

        void InitializedField(string type, string name);

        void AssignVariable(string variableName, string assignmentStatement);

        ICodeFileWriterContext StaticClass(string name, Action<ICodeWriterContext> body);

        void ExtMethod(string returnType, string name, string thisParam, Action<ICodeWriterContext> body);

        void Attribute(string attributeType);
    }
}
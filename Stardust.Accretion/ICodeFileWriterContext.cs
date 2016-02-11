using System;

namespace Stardust.Accretion
{
    public interface ICodeFileWriterContext
    {
        ICodeFileWriterContext Using(string @namespace);

        ICodeWriterContext DeclareNamespace(string @namespace, Action<ICodeWriterContext> body);

        ICodeFileWriterContext AddGeneratorHeader();
    }
}
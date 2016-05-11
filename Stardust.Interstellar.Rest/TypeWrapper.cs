using System;

namespace Stardust.Interstellar.Rest
{
    public class TypeWrapper
    {
        public Type Type { get; set; }

        public static TypeWrapper Create<T>()
        {
            return new TypeWrapper
                       {
                           Type = typeof(T)
                       };
        }
    }
}
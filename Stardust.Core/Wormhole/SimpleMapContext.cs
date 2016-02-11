//
// simplemapcontext.cs
// This file is part of Stardust
//
// Author: Jonas Syrstad (jsyrstad2+StardustCore@gmail.com), http://no.linkedin.com/in/jonassyrstad/) 
// Copyright (c) 2014 Jonas Syrstad. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Stardust.Wormhole.MapBuilder;

namespace Stardust.Wormhole
{
    internal sealed class SimpleMapContext<T> : ISimpleMapContext
    {
        private readonly T Source;

        public SimpleMapContext(T source)
        {
            Source = source;
        }

        public TOut To<TOut>()
        {
            return To<TOut>(MapFactory.CreateMapRule<T, TOut>());
        }

        public TOut To<TOut>(TOut outItem)
        {
            return To(outItem,MapFactory.CreateMapRule<T, TOut>());
        }

        public TOut To<TOut>(IMapContainer map)
        {
            return To(default(TOut), map);
        }

        public TOut To<TOut>(TOut target, IMapContainer map)
        {
            return MapContextHelper.Map(Source, map, target);
        }
    }
}
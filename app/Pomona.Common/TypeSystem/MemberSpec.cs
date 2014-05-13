// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Pomona.Common.TypeSystem
{
    public class Lazy<T>
    {
        [ThreadStatic]
        private static int recursiveCallCounter;

        private readonly LazyThreadSafetyMode lazyThreadSafetyMode;
        private Func<T> factory;
        private bool isInitialized;
        private T value;

        public Lazy(Func<T> factory, LazyThreadSafetyMode lazyThreadSafetyMode)
        {
            this.lazyThreadSafetyMode = lazyThreadSafetyMode;
            this.factory = factory ?? Expression.Lambda<Func<T>>(Expression.New(typeof (T))).Compile();
        }

        public T Value
        {
            get
            {
                if (!isInitialized)
                {
                    try
                    {
                        if (recursiveCallCounter++ > 500)
                            throw new InvalidOperationException("Seems like we're going to get a StackOverflowException here, lets fail early to avoid that.");
                        value = factory();
                    }
                    finally
                    {
                        recursiveCallCounter--;
                    }
                    Thread.MemoryBarrier();
                    isInitialized = true;
                }
                return value;
            }
        }
    }

    public abstract class MemberSpec
    {
        private readonly Lazy<ReadOnlyCollection<Attribute>> declaredAttributes;
        private readonly MemberInfo member;
        private readonly Lazy<string> name;
        private readonly ITypeResolver typeResolver;

        protected MemberSpec(ITypeResolver typeResolver, MemberInfo member)
        {
            if (typeResolver == null)
                throw new ArgumentNullException("typeResolver");
            this.typeResolver = typeResolver;
            this.member = member;
            declaredAttributes = CreateLazy(() => typeResolver.LoadDeclaredAttributes(this).ToList().AsReadOnly());
            name = CreateLazy(() => typeResolver.LoadName(this));
        }

        protected MemberInfo Member
        {
            get { return member; }
        }

        public UniqueMemberToken Token
        {
            get { return member.UniqueToken(); }
        }

        public ReadOnlyCollection<Attribute> DeclaredAttributes
        {
            get { return declaredAttributes.Value; }
        }


        public abstract IEnumerable<Attribute> InheritedAttributes { get; }

        public IEnumerable<Attribute> Attributes
        {
            get { return DeclaredAttributes.Concat(InheritedAttributes); }
        }

        public string Name
        {
            get { return name.Value; }
        }

        public ITypeResolver TypeResolver
        {
            get { return typeResolver; }
        }

        public bool TryGetAttribute<TAttribute>(out TAttribute attribute, bool inherit = true)
            where TAttribute : Attribute
        {
            attribute = (inherit ? Attributes : DeclaredAttributes).OfType<TAttribute>().FirstOrDefault();
            return attribute != null;
        }


        private IEnumerable<Attribute> GetAttributes(bool inherit)
        {
            return inherit ? Attributes : DeclaredAttributes;
        }


        public Maybe<TAttribute> FromAttribute<TAttribute>(bool inherit = true)
            where TAttribute : Attribute
        {
            return GetAttributes(inherit).OfType<TAttribute>().MaybeFirst();
        }

        protected bool Equals(MemberSpec other)
        {
            return Equals(member, other.member) && Equals(typeResolver, other.typeResolver);
        }

        public static bool operator ==(MemberSpec left, MemberSpec right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MemberSpec left, MemberSpec right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MemberSpec) obj);
        }

        protected static Lazy<T> CreateLazy<T>(Func<T> valueFactory)
        {
            var lazy = new Lazy<T>(valueFactory, LazyThreadSafetyMode.None);

            return lazy;
        }


        protected TypeSpec FromType(Type type)
        {
            return typeResolver.FromType(type);
        }


        protected internal virtual IEnumerable<Attribute> OnLoadDeclaredAttributes()
        {
            if (member == null)
                return Enumerable.Empty<Attribute>();

            return member.GetCustomAttributes(false).Cast<Attribute>();
        }


        protected internal virtual string OnLoadName()
        {
            if (member == null)
                throw new InvalidOperationException("Don't know how to load name for member with no wrapped member.");

            return member.Name;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((member != null ? member.GetHashCode() : 0)*397) ^
                       (typeResolver != null ? typeResolver.GetHashCode() : 0);
            }
        }
    }
}
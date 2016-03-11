#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Pomona.Common.TypeSystem
{
    public abstract class MemberSpec
    {
        private readonly Lazy<ReadOnlyCollection<Attribute>> declaredAttributes;
        private readonly MemberInfo member;
        private readonly Lazy<string> name;
        private readonly ITypeResolver typeResolver;


        protected MemberSpec(ITypeResolver typeResolver, MemberInfo member)
        {
            if (typeResolver == null)
                throw new ArgumentNullException(nameof(typeResolver));
            this.typeResolver = typeResolver;
            this.member = member;
            this.declaredAttributes = CreateLazy(() => typeResolver.LoadDeclaredAttributes(this).ToList().AsReadOnly());
            this.name = CreateLazy(() => typeResolver.LoadName(this));
        }


        public IEnumerable<Attribute> Attributes
        {
            get { return DeclaredAttributes.Concat(InheritedAttributes); }
        }

        public ReadOnlyCollection<Attribute> DeclaredAttributes
        {
            get { return this.declaredAttributes.Value; }
        }

        public abstract IEnumerable<Attribute> InheritedAttributes { get; }

        public MemberInfo Member
        {
            get { return this.member; }
        }

        public string Name
        {
            get { return this.name.Value; }
        }

        public UniqueMemberToken Token
        {
            get { return this.member.UniqueToken(); }
        }

        public ITypeResolver TypeResolver
        {
            get { return this.typeResolver; }
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((MemberSpec)obj);
        }


        public Maybe<TAttribute> FromAttribute<TAttribute>(bool inherit = true)
            where TAttribute : Attribute
        {
            return GetAttributes(inherit).OfType<TAttribute>().MaybeFirst();
        }


        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.member != null ? this.member.GetHashCode() : 0) * 397) ^
                       (this.typeResolver != null ? this.typeResolver.GetHashCode() : 0);
            }
        }


        public static bool operator ==(MemberSpec left, MemberSpec right)
        {
            return Equals(left, right);
        }


        public static bool operator !=(MemberSpec left, MemberSpec right)
        {
            return !Equals(left, right);
        }


        public bool TryGetAttribute<TAttribute>(out TAttribute attribute, bool inherit = true)
            where TAttribute : Attribute
        {
            attribute = (inherit ? Attributes : DeclaredAttributes).OfType<TAttribute>().FirstOrDefault();
            return attribute != null;
        }


        protected internal virtual IEnumerable<Attribute> OnLoadDeclaredAttributes()
        {
            if (this.member == null)
                return Enumerable.Empty<Attribute>();

            return this.member.GetCustomAttributes(false).Cast<Attribute>();
        }


        protected internal virtual string OnLoadName()
        {
            if (this.member == null)
                throw new InvalidOperationException("Don't know how to load name for member with no wrapped member.");

            return this.member.Name;
        }


        protected bool Equals(MemberSpec other)
        {
            return Equals(this.member, other.member) && Equals(this.typeResolver, other.typeResolver);
        }


        internal static Lazy<T> CreateLazy<T>(Func<T> valueFactory)
        {
            var lazy = new Lazy<T>(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication);

            return lazy;
        }


        private IEnumerable<Attribute> GetAttributes(bool inherit)
        {
            return inherit ? Attributes : DeclaredAttributes;
        }
    }
}
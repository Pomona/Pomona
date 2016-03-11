#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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
        private readonly Lazy<string> name;


        protected MemberSpec(ITypeResolver typeResolver, MemberInfo member)
        {
            if (typeResolver == null)
                throw new ArgumentNullException(nameof(typeResolver));
            TypeResolver = typeResolver;
            Member = member;
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

        public MemberInfo Member { get; }

        public string Name
        {
            get { return this.name.Value; }
        }

        public UniqueMemberToken Token
        {
            get { return Member.UniqueToken(); }
        }

        public ITypeResolver TypeResolver { get; }


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
                return ((Member != null ? Member.GetHashCode() : 0) * 397) ^
                       (TypeResolver != null ? TypeResolver.GetHashCode() : 0);
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
            if (Member == null)
                return Enumerable.Empty<Attribute>();

            return Member.GetCustomAttributes(false).Cast<Attribute>();
        }


        protected internal virtual string OnLoadName()
        {
            if (Member == null)
                throw new InvalidOperationException("Don't know how to load name for member with no wrapped member.");

            return Member.Name;
        }


        protected bool Equals(MemberSpec other)
        {
            return Equals(Member, other.Member) && Equals(TypeResolver, other.TypeResolver);
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
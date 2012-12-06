using System;
using System.Linq;
using System.Reflection;

namespace Pomona.Common
{
    public static class TypeExtensions
    {
        public static bool IsAnonymous(this Type type)
        {
            return type.Name.StartsWith("<>f__AnonymousType");
        }


        /// <summary>
        /// Here we check that a, which can be an open type with generic parameters
        /// is equivalent to b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsGenericallyEquivalentTo(this Type a, Type b)
        {
            if (a.IsGenericParameter)
            {
                if (b.IsGenericParameter)
                {
                    if (a.GenericParameterPosition != b.GenericParameterPosition)
                        return false;

                    if (a.GenericParameterAttributes != b.GenericParameterAttributes)
                    {
                        return false;
                    }

                    var aConstraints = a.GetGenericParameterConstraints();
                    var bConstraints = b.GetGenericParameterConstraints();

                    return aConstraints.Zip(bConstraints, IsGenericallyEquivalentTo).All(x => x);
                }
            }

            if (a.MetadataToken != b.MetadataToken)
            {
                return false;
            }

            if (a.IsGenericType)
            {
                if (!b.IsGenericType)
                    return false;
                return
                    a.GetGenericArguments()
                     .Zip(b.GetGenericArguments(), IsGenericallyEquivalentTo)
                     .All(x => x);
            }

            return true;
        }

        public static bool IsGenericInstanceOf(this Type type, params Type[] genericTypeDefinitions)
        {
            return genericTypeDefinitions.Any(x => x.MetadataToken == type.MetadataToken);
        }
    }
}
#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Reflection;

using Pomona.Common.TypeSystem;

namespace Pomona.RequestProcessing
{
    public class HandlerParameter
    {
        private readonly ParameterInfo parameterInfo;
        private TypeSpec typeSpec;


        public HandlerParameter(ParameterInfo parameterInfo, HandlerMethod method)
        {
            this.parameterInfo = parameterInfo;
            Method = method;
        }


        public bool IsResource
        {
            get { return TypeSpec is ResourceType; }
        }

        public bool IsTransformedType
        {
            get { return TypeSpec is StructuredType; }
        }

        public HandlerMethod Method { get; }

        public string Name
        {
            get { return this.parameterInfo.Name; }
        }

        public int Position
        {
            get { return this.parameterInfo.Position; }
        }

        public Type Type
        {
            get { return this.parameterInfo.ParameterType; }
        }

        public TypeSpec TypeSpec
        {
            get
            {
                Method.TypeMapper.TryGetTypeSpec(this.parameterInfo.ParameterType, out this.typeSpec);
                return this.typeSpec;
            }
        }
    }
}
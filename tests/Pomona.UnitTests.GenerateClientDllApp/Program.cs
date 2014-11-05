#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
using System.IO;

using Pomona.CodeGen;
using Pomona.Common.Internals;
using Pomona.Example;
using Pomona.Example.Models;
using Pomona.Example.SimpleExtraSite;
using Pomona.FluentMapping;

namespace Pomona.UnitTests.GenerateClientDllApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var typeMapper = new TypeMapper(new ModifiedCritterPomonaConfiguration());

            // Modify property Protected of class Critter to not be protected in client dll.
            // This is to test setting a protected property will throw exception on server.

            using (var file = new FileStream(@"..\..\..\..\lib\Critters.Client.dll", FileMode.OpenOrCreate))
            {
                ClientLibGenerator.WriteClientLibrary(typeMapper, file, embedPomonaClient : false);
            }
            using (var file = new FileStream(@"..\..\..\..\lib\Extra.Client.dll", FileMode.OpenOrCreate))
            {
                ClientLibGenerator.WriteClientLibrary(new TypeMapper(new SimplePomonaConfiguration()), file,
                                                      embedPomonaClient : false);
            }

            using (
                var file = new FileStream(
                    @"..\..\..\..\lib\IndependentCritters.dll",
                    FileMode.OpenOrCreate))
            {
                ClientLibGenerator.WriteClientLibrary(new TypeMapper(new IndependentClientDllConfiguration()),
                                                      file,
                                                      embedPomonaClient : true);
            }

            Console.WriteLine("Wrote client dll.");
        }

        #region Nested type: IndependentClientDllConfiguration

        private class IndependentClientDllConfiguration : ModifiedCritterPomonaConfiguration
        {
            public override ITypeMappingFilter TypeMappingFilter
            {
                get { return new IndependentClientDllTypeMappingFilter(SourceTypes); }
            }

            #region Nested type: IndependentClientDllTypeMappingFilter

            private class IndependentClientDllTypeMappingFilter : CritterTypeMappingFilter
            {
                public IndependentClientDllTypeMappingFilter(IEnumerable<Type> sourceTypes)
                    : base(sourceTypes)
                {
                }


                public override ClientMetadata ClientMetadata
                {
                    get { return base.ClientMetadata.With("IndependentCritters"); }
                }
            }

            #endregion
        }

        #endregion

        #region Nested type: ModifiedCritterPomonaConfiguration

        private class ModifiedCritterPomonaConfiguration : CritterPomonaConfiguration
        {
            private readonly ITypeMappingFilter typeMappingFilter;


            public ModifiedCritterPomonaConfiguration()
            {
                this.typeMappingFilter = new ModifiedTypeMappingFilter(SourceTypes);
            }


            public override IEnumerable<object> FluentRuleObjects
            {
                get { return base.FluentRuleObjects.Append(new ModifiedFluentRules()); }
            }

            public override ITypeMappingFilter TypeMappingFilter
            {
                get { return this.typeMappingFilter; }
            }
        }

        #endregion

        #region Nested type: ModifiedFluentRules

        internal class ModifiedFluentRules
        {
            public void Map(ITypeMappingConfigurator<UnpostableThingOnServer> map)
            {
                map.PostAllowed();
            }


            public void Map(ITypeMappingConfigurator<Critter> map)
            {
                map.Include(x => x.PublicAndReadOnlyThroughApi, o => o.ReadOnly());
            }
        }

        #endregion

        #region Nested type: ModifiedTypeMappingFilter

        private class ModifiedTypeMappingFilter : CritterTypeMappingFilter
        {
            public ModifiedTypeMappingFilter(IEnumerable<Type> sourceTypes)
                : base(sourceTypes)
            {
            }


            public override bool GetTypeIsAbstract(Type type)
            {
                if (type == typeof(AbstractOnServerAnimal))
                    return false;
                return base.GetTypeIsAbstract(type);
            }
        }

        #endregion
    }
}
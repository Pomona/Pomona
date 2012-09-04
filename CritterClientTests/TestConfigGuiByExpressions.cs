// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
using System.Linq.Expressions;
using CritterClient;

namespace CritterClientTests
{
    public interface IPropertyConfiguration
    {
        IPropertyConfiguration Description(string description);
        IPropertyConfiguration Editable();
        IPropertyConfiguration Header(string header);
        IPropertyConfiguration Name(string name);
    }

    public interface IListDefinition<T>
    {
        IListDefinition<T> Column(
            Expression<Func<T, object>> propSelector, Func<IPropertyConfiguration, IPropertyConfiguration> propConfig);
    }

    internal class TestConfigGuiByExpressions
    {
        public IListDefinition<T> DefineList<T>()
        {
            throw new NotImplementedException();
        }


        public IViewDefinition<T> DefineView<T>()
        {
            throw new NotImplementedException();
        }


        public void TestApiLook()
        {
            var critterList =
                DefineList<Critter>()
                    .Column(x => x.Id, x => x.Header("Identifier"))
                    .Column(x => x.Hat.HatType, x => x.Header("HatType").Editable());

            DefineView<Critter>()
                .SelectedUsing(critterList)
                .Label(x => x.CreatedOn)
                .Label(x => x.Hat.Id, x => x.Name("Hat id").Description("The #id of the hat"))
                .TextBox(x => x.Name, x => x.Editable())
                .TextBox(x => x.Hat.Style, x => x.Editable());
        }
    }

    internal interface IViewDefinition<T>
    {
        IViewDefinition<T> Label(
            Expression<Func<T, object>> propSelector, Func<IPropertyConfiguration, IPropertyConfiguration> propConfig);


        IViewDefinition<T> Label(
            Expression<Func<T, object>> propSelector);


        IViewDefinition<T> SelectedUsing(IListDefinition<T> listDefinition);


        IViewDefinition<T> TextBox(
            Expression<Func<T, object>> propSelector, Func<IPropertyConfiguration, IPropertyConfiguration> propConfig);
    }
}
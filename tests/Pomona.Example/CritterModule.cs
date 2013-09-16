﻿#region License

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

#endregion

using System;
using Microsoft.Practices.ServiceLocation;
using Nancy;
using Nancy.Validation;
using Pomona.Example.Models;

namespace Pomona.Example
{
    public class CritterModule : PomonaModule
    {
        public CritterModule(CritterDataSource dataSource, TypeMapper typeMapper, IServiceLocator serviceLocator)
            : base(dataSource, typeMapper, serviceLocator)
        {
        }


        public CritterDataSource CritterDataSource
        {
            get { return (CritterDataSource)DataSource; }
        }

        protected override PomonaError OnException(Exception exception)
        {
            if (exception is ModelValidationException)
            {
                return new PomonaError(HttpStatusCode.BadRequest, new ErrorStatus(exception.Message, 1337));
            }

            if (exception is ResourceValidationException)
            {
                var validationException = (ResourceValidationException)exception;
                return new PomonaError(HttpStatusCode.BadRequest,
                                       new ErrorStatus(validationException.Message, 0xdead,
                                                       validationException.MemberName));
            }

            return base.OnException(exception);
        }
    }
}
#region License

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

#endregion

using NUnit.Framework;

using Newtonsoft.Json.Linq;

namespace Pomona.UnitTests
{
    public static class JsonAssertExtensions
    {
        public static JToken AssertHasProperty(this JToken jtoken, string propertyName)
        {
            var jobject = jtoken as JObject;
            Assert.IsNotNull(
                jobject,
                "jtoken can't contain property " + propertyName + " because it's not an object (type is " +
                jtoken.GetType().Name + ")");

            JToken propValue;
            Assert.IsTrue(
                jobject.TryGetValue(propertyName, out propValue),
                string.Format("Object has no property named {0}. Contents:\r\n{1}", propertyName, jobject));
            return propValue;
        }


        public static JArray AssertHasPropertyWithArray(this JToken jtoken, string propertyName)
        {
            var jobject = jtoken as JObject;
            Assert.IsNotNull(
                jobject,
                "jtoken can't contain property " + propertyName + " because it's not an object (type is " +
                jtoken.GetType().Name + ")");

            var propToken = jobject.AssertHasProperty(propertyName);
            var jArray = propToken as JArray;
            Assert.IsNotNull(
                jArray,
                string.Format(
                    "JSON property {0} is not of type JArray. Contents:\r\n{1}",
                    propertyName,
                    jobject));
            return jArray;
        }


        public static JObject AssertHasPropertyWithObject(this JObject jobject, string propertyName)
        {
            var propValue = AssertHasProperty(jobject, propertyName);
            var propValueObject = propValue as JObject;
            Assert.IsNotNull(
                propValueObject,
                string.Format(
                    "JSON property {0} is not of type JObject. Contents:\r\n{1}",
                    propertyName,
                    jobject));
            return propValueObject;
        }


        public static string AssertHasPropertyWithString(this JToken jtoken, string propertyName)
        {
            var jobject = jtoken as JObject;
            Assert.IsNotNull(
                jobject,
                "jtoken can't contain property " + propertyName + " because it's not an object (type is " +
                jtoken.GetType().Name + ")");

            var propToken = jobject.AssertHasProperty(propertyName);
            var jsonValue = propToken as JValue;
            Assert.IsNotNull(
                jsonValue,
                string.Format(
                    "JSON property {0} is not of type JValue. Contents:\r\n{1}",
                    propertyName,
                    jobject));
            return (string)jsonValue.Value;
        }


        public static void AssertIsReference(this JToken jobject)
        {
            Assert.IsNotNullOrEmpty(jobject.AssertHasPropertyWithString("_ref"), "Uri reference null or empty");
        }
    }
}
#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace Pomona.TestHelpers
{
    public static class JsonAssertExtensions
    {
        public static void AssertDoesNotHaveProperty(this JObject jobject, string propertyName)
        {
            JToken ignoredValue;
            Assert.IsFalse(
                jobject.TryGetValue(propertyName, out ignoredValue),
                "JSON object has property with name " + propertyName + ", which was not expected.");
        }


        public static void AssertDoesNotHaveProperty(this JToken jtoken, string propertyName)
        {
            var jobject = jtoken as JObject;
            if (jobject == null)
                return;

            JToken propToken;
            Assert.IsFalse(
                jobject.TryGetValue(propertyName, out propToken),
                "Object shouldn't have contained property \"" + propertyName + "\":\r\n" + jobject);
        }


        public static JToken AssertHasProperty(this JToken jtoken, string propertyName)
        {
            return TryConvertToObjectAndGetProperty<JToken>(jtoken, propertyName);
        }


        public static JArray AssertHasPropertyWithArray(this JToken jtoken, string propertyName)
        {
            return TryConvertToObjectAndGetProperty<JArray>(jtoken, propertyName);
        }


        public static bool AssertHasPropertyWithBool(this JToken jtoken, string propertyName)
        {
            var jsonValue = TryConvertToObjectAndGetProperty<JValue>(jtoken, propertyName);
            return (bool)jsonValue.Value;
        }


        public static double AssertHasPropertyWithDouble(this JToken jtoken, string propertyName)
        {
            var jsonValue = TryConvertToObjectAndGetProperty<JValue>(jtoken, propertyName);
            return (double)jsonValue.Value;
        }


        public static long AssertHasPropertyWithInteger(this JToken jtoken, string propertyName)
        {
            var jsonValue = TryConvertToObjectAndGetProperty<JValue>(jtoken, propertyName);
            return (long)jsonValue.Value;
        }


        public static void AssertHasPropertyWithNull(this JToken jtoken, string propertyName)
        {
            var jsonValue = TryConvertToObjectAndGetProperty<JValue>(jtoken, propertyName);
            Assert.IsNull(jsonValue.Value);
        }


        public static JObject AssertHasPropertyWithObject(this JToken jtoken, string propertyName)
        {
            return TryConvertToObjectAndGetProperty<JObject>(jtoken, propertyName);
        }


        public static JObject AssertHasPropertyWithObject(this JObject jtoken, string propertyName)
        {
            return TryConvertToObjectAndGetProperty<JObject>(jtoken, propertyName);
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
                $"JSON property {propertyName} is not of type JValue. Contents:\r\n{jobject}");
            return (string)jsonValue.Value;
        }


        public static void AssertHasPropertyWithValue(this JToken jtoken, string propertyName, string value)
        {
            Assert.That(jtoken.AssertHasPropertyWithString(propertyName), Is.EqualTo(value));
        }


        public static void AssertHasPropertyWithValue(this JToken jtoken, string propertyName, int value)
        {
            Assert.That(jtoken.AssertHasPropertyWithInteger(propertyName), Is.EqualTo(value));
        }


        public static void AssertIsExpanded(this JToken jtoken)
        {
            var jobject = jtoken as JObject;
            Assert.IsNotNull(jobject, "object is not expanded (JObject).");

            JToken tmp;
            Assert.IsFalse(jobject.TryGetValue("_ref", out tmp), "Expanded object should not have _ref property!");
        }


        public static void AssertIsReference(this JToken jobject)
        {
            Assert.That(jobject.AssertHasPropertyWithString("_ref"), Is.Not.Null.Or.Empty, "Uri reference null or empty");
        }


        private static T TryConvertToObjectAndGetProperty<T>(JToken jtoken, string propertyName, out JObject jobject)
            where T : JToken
        {
            jobject = jtoken as JObject;
            Assert.IsNotNull(
                jobject,
                "jtoken can't contain property " + propertyName + " because it's not an object (type is " +
                jtoken.GetType().Name + ")");

            JToken propToken;
            Assert.IsTrue(
                jobject.TryGetValue(propertyName, out propToken),
                "Object does not contain property with name \"" + propertyName + "\":\r\n" + jobject);

            if (!(propToken is T))
                Assert.Fail("Expected that property " + propertyName + " had a value of JSON type " + typeof(T).Name);

            return (T)propToken;
        }


        private static T TryConvertToObjectAndGetProperty<T>(JToken jtoken, string propertyName)
            where T : JToken
        {
            JObject jobject;
            return TryConvertToObjectAndGetProperty<T>(jtoken, propertyName, out jobject);
        }
    }
}
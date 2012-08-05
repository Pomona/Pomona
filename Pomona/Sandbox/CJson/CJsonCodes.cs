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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Pomona.Sandbox.CJson
{
    public enum CJsonCodes
    {
        NewSegment = 0,
        StartArray = 1,
        StartObject = 2,
        Stop = 3,
        StringValue = 4,
        DoubleValue = 5,
        IntValue = 6,
        NullValue = 7,
        TrueValue = 8,
        FalseValue = 9,
        ReuseObject = 10,
        ModifyString = 11,
        ReferenceStartOffset = 16
    }

    /// <summary>
    /// Just more toying around with new packed JSON format.
    /// Will probably be useless and too complex, but it's fun allright!
    /// 
    /// This one is centered around the idea that there often are many
    /// objects in a JSON stream that have the same set of properties.
    /// 
    /// So we cache the property signatures on transmitter and receiver,
    /// and assign it an index, so we don't need to resend the property
    /// names every time.
    /// 
    /// A large list of critters, as stored in the CJsonEncoderTests
    /// seem to compress down to 22% of original size.
    /// 
    /// It would be most effective used with a stateful persistent stream
    /// of JSON objects. After a few hundreds bytes it could perform almost as
    /// effective as "raw" binary serialization of the structure.
    /// (oh course not for all colors of data).
    /// 
    /// The downside is that this needs to hold the entire state of the
    /// sent JSON object in memory until its completely scanned.
    /// 
    /// It might also be that the signature lookups cost too much CPU-wise.
    /// 
    /// The real test to as whether this kind of JSON packing is useful or
    /// not is seeing how it measure how it compares CPU-wise with gzip
    /// compression.
    /// 
    /// This is only a conceptual implementation of an encoder to see how it
    /// perform. I haven't yet written any decoder, so it might encode everything
    /// completely wrong.
    /// 
    /// It's all a test just for fun, I don't personally have any real use-case
    /// for this anyway. [KNS]
    /// </summary>
    public class CJson2Encoder
    {
        public const int SignatureCacheSize = 64;

        public const int PropertyNameCacheSize = 112;
        public const int ValueCacheSize = 112;
        public const int EqualStartingCharOptimizeLimit = 6;
        private string[] propertyNameCache = new string[PropertyNameCacheSize];
        private int propertyNameCacheIndex = 0;
        private Dictionary<string, int> propertyNameCacheToIndexDict = new Dictionary<string, int>();

        // Seems like compressing string stream by itself is useless
        private bool separateStringStreamEnabled = false;
        private CachedSignature[] signatureCache = new CachedSignature[SignatureCacheSize];
        private int signatureCacheIndex = 0;
        private Dictionary<string, CachedSignature> signatureKeyDict = new Dictionary<string, CachedSignature>();

        private List<byte> stringStream = new List<byte>();
        private object[] valueCache = new object[ValueCacheSize];
        private int valueCacheIndex = 0;
        private Dictionary<object, int> valueCacheToIndexDict = new Dictionary<object, int>();

        public Stream Stream { get; set; }

        public int TotalCachedPropertyNames { get; private set; }
        public int TotalCachedSignatures { get; private set; }

        private void AddPropertyNameToCache(string propname)
        {
            var pos = propertyNameCacheIndex;
            propertyNameCacheIndex = (propertyNameCacheIndex + 1)%PropertyNameCacheSize;

            var replacedEntry = propertyNameCache[pos];
            if (replacedEntry != null)
            {
                propertyNameCacheToIndexDict.Remove(replacedEntry);
            }

            propertyNameCache[pos] = propname;
            propertyNameCacheToIndexDict[propname] = pos;

            TotalCachedPropertyNames++;
        }

        private void AddValueToCache(object value)
        {
            var pos = valueCacheIndex;
            valueCacheIndex = (valueCacheIndex + 1)%ValueCacheSize;

            var replacedEntry = valueCache[pos];
            if (replacedEntry != null)
            {
                valueCacheToIndexDict.Remove(replacedEntry);
            }

            valueCache[pos] = value;
            valueCacheToIndexDict[value] = pos;
        }

        private void AddObjectToSignatureCache(CachedSignature newCacheEntry)
        {
            var pos = signatureCacheIndex;
            signatureCacheIndex = (signatureCacheIndex + 1)%SignatureCacheSize;

            var oldCacheEntry = signatureCache[pos];
            if (oldCacheEntry != null)
            {
                signatureKeyDict.Remove(oldCacheEntry.Key);
            }

            newCacheEntry.Index = pos;
            signatureCache[pos] = newCacheEntry;
            signatureKeyDict[newCacheEntry.Key] = newCacheEntry;

            TotalCachedSignatures++;
        }

        private int CountEqualStartingBytes(string a, string b)
        {
            var count = 0;
            var minlength = Math.Min(a.Length, b.Length);

            while (count < minlength && a[count] == b[count])
            {
                count++;
            }

            return count;
        }

        public void PackIt(JToken jtoken)
        {
            PackIt(jtoken, false);
            if (separateStringStreamEnabled)
            {
                OutputBytes(stringStream.ToArray());
            }
        }

        private void PackIt(JToken jtoken, bool jsonTypeIsImplicit)
        {
            var jobject = jtoken as JObject;
            var jarray = jtoken as JArray;
            var jvalue = jtoken as JValue;

            if (jobject != null)
            {
                var newSignature = new CachedSignature(jobject);
                CachedSignature cachedSignature;
                if (signatureKeyDict.TryGetValue(newSignature.Key, out cachedSignature))
                {
                    Console.WriteLine("Reusing signature " + newSignature.Key);
                    // OutputCode(CJsonCodes.ReuseObject);
                    OutputVarint((int) CJsonCodes.ReferenceStartOffset + (cachedSignature.Index*2) + 1);
                    var changedProperties = OutputChangeBitmapCode(cachedSignature.Template, jobject);
                    foreach (var jpropchange in changedProperties)
                    {
                        if (jpropchange.EncodeAsModifiedString)
                        {
                            var curStr = (string) jpropchange.NewProp.Value;
                            OutputCode(CJsonCodes.ModifyString);
                            Debug.Assert(curStr == (string) ((JValue) jpropchange.NewProp.Value).Value);
                            var stringEnd = curStr.Substring(jpropchange.EqualStartingCharCount);
                            OutputVarint(jpropchange.EqualStartingCharCount);
                            OutputString(stringEnd, true);
                        }
                        else
                        {
                            PackIt(jpropchange.NewProp.Value, jpropchange.TypeIsImplicit);
                        }
                    }

                    cachedSignature.Template = jobject;
                }
                else
                {
                    OutputCode(CJsonCodes.StartObject);

                    foreach (var jprop in jobject.Properties())
                    {
                        OutputPropertyName(jprop.Name);
                        PackIt(jprop.Value, false);
                    }

                    OutputCode(CJsonCodes.Stop);
                    AddObjectToSignatureCache(new CachedSignature(jobject));
                }
            }
            else if (jarray != null)
            {
                OutputCode(CJsonCodes.StartArray);
                foreach (var child in jarray.Children())
                {
                    PackIt(child, false);
                }
                OutputCode(CJsonCodes.Stop);
            }
            else if (jvalue != null)
            {
                if (jvalue.Type == JTokenType.String)
                {
                    OutputStringValue((string) jvalue.Value, jsonTypeIsImplicit);
                }
                else if (jvalue.Type == JTokenType.Integer)
                {
                    OutputIntegerValue((long) jvalue.Value, jsonTypeIsImplicit);
                }
                else if (jvalue.Type == JTokenType.Float)
                {
                    OutputDoubleValue((double) jvalue.Value, jsonTypeIsImplicit);
                }
                else if (jvalue.Type == JTokenType.Null)
                {
                    Debug.Assert(jsonTypeIsImplicit == false);
                    OutputCode(CJsonCodes.NullValue);
                }
                else if (jvalue.Type == JTokenType.Boolean)
                {
                    Debug.Assert(jsonTypeIsImplicit == false);
                    OutputCode((bool) jvalue.Value ? CJsonCodes.TrueValue : CJsonCodes.FalseValue);
                }
                else
                {
                    Debug.Assert(jsonTypeIsImplicit == false);
                    OutputStringValue(jvalue.Value.ToString(), false);
                    Console.WriteLine("Can't handle json type " + jvalue.Type);
                }
            }
        }

        private void OutputDoubleValue(double value, bool typeIsImplicit)
        {
            Debug.Assert(BitConverter.IsLittleEndian);
            var bytes = BitConverter.GetBytes(value);
            if (!typeIsImplicit)
                OutputCode(CJsonCodes.DoubleValue);
            OutputBytes(bytes);
        }

        private void OutputIntegerValue(long value, bool typeIsImplicit)
        {
            if (!typeIsImplicit)
                OutputCode(CJsonCodes.IntValue);
            // ZigZag encoded
            OutputVarint((value << 1) ^ (value >> 63));
        }

        private void OutputStringValue(string text, bool typeIsImplicit)
        {
            int index;
            if ((!typeIsImplicit) && valueCacheToIndexDict.TryGetValue(text, out index))
            {
                OutputVarint((int) CJsonCodes.ReferenceStartOffset + (index*2));
            }
            else
            {
                OutputString(text, typeIsImplicit);
                AddValueToCache(text);
            }
        }

        private void OutputString(string text, bool typeIsImplicit)
        {
            if (!typeIsImplicit)
                OutputCode(CJsonCodes.StringValue);

            OutputVarint(text.Length);

            var textBytes = Encoding.UTF8.GetBytes(text);
            if (separateStringStreamEnabled)
                stringStream.AddRange(textBytes);
            else
                OutputBytes(textBytes);
        }

        private void OutputByte(byte b)
        {
            Stream.WriteByte(b);
        }

        private void OutputBytes(byte[] bytes)
        {
            Stream.Write(bytes, 0, bytes.Length);
        }

        private void OutputVarint(long value)
        {
            var uval = (ulong) value;
            while (uval > 0x80)
            {
                OutputByte((byte) ((uval & 0x7f) | 0x80));
                uval = uval >> 7;
            }
            OutputByte((byte) (uval & 0x7f));
        }

        private void OutputCode(CJsonCodes code)
        {
            OutputVarint((int) code);
        }

        private IEnumerable<JPropChange> OutputChangeBitmapCode(JObject previous, JObject current)
        {
            var changedProperties = new List<JPropChange>();
            var changedMap = new List<bool>();

            foreach (
                var pair in
                    previous.Properties().Zip(current.Properties(),
                                              (prevProp, curProp) => new {PrevProp = prevProp, CurProp = curProp}))
            {
                if (IsEqualJValue(pair.CurProp.Value, pair.PrevProp.Value))
                {
                    changedMap.Add(false);
                    changedMap.Add(false);
                }
                else
                {
                    var typeIsImplicit = IsValidImplicitType(pair.CurProp.Value.Type) &&
                                         pair.CurProp.Value.Type == pair.PrevProp.Value.Type;
                    var startEqualCount = 0;

                    // Do not set type to implicit if string and we got 6 equal starting chars or more
                    if (typeIsImplicit && pair.CurProp.Value.Type == JTokenType.String)
                    {
                        var curStr = (string) (((JValue) pair.CurProp.Value).Value);
                        var oldStr = (string) (((JValue) pair.PrevProp.Value).Value);

                        startEqualCount = CountEqualStartingBytes(curStr, oldStr);

                        if (startEqualCount >= EqualStartingCharOptimizeLimit)
                        {
                            Console.WriteLine("\"{0}\" and \"{1}\" has {2} equal starting chars!", oldStr, curStr,
                                              startEqualCount);
                            typeIsImplicit = false;
                        }
                    }

                    changedProperties.Add(new JPropChange()
                                              {
                                                  NewProp = pair.CurProp,
                                                  OldProp = pair.PrevProp,
                                                  TypeIsImplicit = typeIsImplicit,
                                                  EqualStartingCharCount = startEqualCount
                                              });

                    changedMap.Add(true);
                    changedMap.Add(typeIsImplicit);
                }
            }

            Console.WriteLine("{0} out of {1} properties has changed", changedMap.Where(x => x).Count(),
                              changedMap.Count);

            foreach (var bitmaskPart in PackChangedBitmask(changedMap))
            {
                OutputVarint(bitmaskPart);
            }

            return changedProperties;
        }

        private bool IsValidImplicitType(JTokenType type)
        {
            return type == JTokenType.String || type == JTokenType.Integer || type == JTokenType.Float;
        }

        private bool IsEqualJValue(JToken a, JToken b)
        {
            var aval = a as JValue;
            if (aval == null)
                return false;
            var bval = b as JValue;
            if (bval == null)
                return false;

            return aval.Value.Equals(bval.Value);
        }

        private static IEnumerable<int> PackChangedBitmask(IEnumerable<bool> source)
        {
            var enumerator = source.GetEnumerator();

            var hasElements = true;

            while (hasElements)
            {
                var bitMask = 0;
                var bitIndex = 0;
                while (bitIndex < 7 && (hasElements = enumerator.MoveNext()))
                {
                    bitMask |= 1 << bitIndex;
                    bitIndex++;
                }
                if (bitIndex > 0)
                    yield return bitMask;
            }
        }

        private void OutputPropertyName(string name)
        {
            int index;
            if (propertyNameCacheToIndexDict.TryGetValue(name, out index))
            {
                OutputVarint((int) CJsonCodes.ReferenceStartOffset + index);
            }
            else
            {
                OutputString(name, false);
                AddPropertyNameToCache(name);
            }
        }

        #region Nested type: CachedSignature

        private class CachedSignature
        {
            public CachedSignature(JObject template)
            {
                Template = template;

                // For diagnostics purposes
                Index = -1;

                GenerateKey();
            }

            public string Key { get; private set; }
            public int Index { get; set; }
            public JObject Template { get; set; }

            private void GenerateKey()
            {
                var sb = new StringBuilder();
                foreach (var property in Template.Properties())
                {
                    sb.AppendFormat("\"{0}\",", property.Name);
                }

                Key = sb.ToString();
            }
        }

        #endregion

        #region Nested type: JPropChange

        private class JPropChange
        {
            public JProperty OldProp { get; set; }
            public JProperty NewProp { get; set; }
            public bool TypeIsImplicit { get; set; }

            public bool EncodeAsModifiedString
            {
                get { return EqualStartingCharCount >= EqualStartingCharOptimizeLimit; }
            }

            public int EqualStartingCharCount { get; set; }
        }

        #endregion
    }

    public class CJsonSymbolDict
    {
        private int symbolCounter;
        private Dictionary<object, int> symbolLookupTable = new Dictionary<object, int>();


        public CJsonSymbolDict()
        {
        }


        public bool WriteSymbol(object symbol, out int dictIndex)
        {
            if (!symbolLookupTable.TryGetValue(symbol, out dictIndex))
            {
                dictIndex = -1;
                symbolLookupTable[symbol] = symbolCounter++;

                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Just a small experiment in finding out what space benefit a compressed
    /// binary JSON format would have over normal JSON.
    /// 
    /// For now it seems its not too much, some examples compresses to 30-40% of size.
    /// However it would have an advantage for streaming data.
    /// </summary>
    public class CJsonEncoder
    {
        private EncoderState currentState = EncoderState.Start;
        private StringBuilder incomingString = new StringBuilder();
        private CJsonSymbolDict propertyNameSymbols = new CJsonSymbolDict();
        private int sizeCompressed = 0;

        private int sizeNotCompressed = 0;
        private Stack<EncoderState> stateStack = new Stack<EncoderState>();
        private CJsonSymbolDict valueSymbols = new CJsonSymbolDict();

        public int SizeCompressed
        {
            get { return sizeCompressed; }
        }

        public int SizeNotCompressed
        {
            get { return sizeNotCompressed; }
        }


        public void Parse(string str)
        {
            foreach (var c in str)
                Parse(c);
        }


        private bool IsAllowedFirstLetterOfUnescapedPropertyName(char c)
        {
            return c < 128 && (char.IsLetter(c) || c == '_');
        }


        private bool IsAllowedLetterOfUnescapedPropertyName(char c)
        {
            return IsAllowedFirstLetterOfUnescapedPropertyName(c) || char.IsNumber(c);
        }


        private bool IsAllowedUnescapedValueCharacter(char c)
        {
            return c < 128 && (char.IsLetterOrDigit(c) || c == '.' || c == '-');
        }


        private bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\r' || c == '\n' || c == '\t';
        }


        private void Output(CJsonCodes code)
        {
            sizeNotCompressed++;
            sizeCompressed++;
            Console.WriteLine("TODO: OUTPUT CODE " + code + " in state " + currentState);
        }


        private void OutputPropertyName(string propName)
        {
            int dictIndex;

            sizeNotCompressed += Encoding.UTF8.GetByteCount(propName) + 1;

            if (propertyNameSymbols.WriteSymbol(propName, out dictIndex))
            {
                sizeCompressed++;
                if (dictIndex > 120)
                    sizeCompressed++;
                Console.WriteLine("OUTPUT PROPNAME {0} LOOKUP INDEX {1}", propName, dictIndex);
            }
            else
            {
                sizeNotCompressed += Encoding.UTF8.GetByteCount(propName) + 1;
                sizeCompressed += Encoding.UTF8.GetByteCount(propName) + 2;
                Console.WriteLine("OUTPUT PROPNAME {0} (NEW IN TABLE)", propName);
            }
        }


        private void OutputStringValue(string toString)
        {
            int dictIndex;

            sizeNotCompressed += Encoding.UTF8.GetByteCount(toString) + 1;

            if (valueSymbols.WriteSymbol(toString, out dictIndex))
            {
                sizeCompressed++;
                if (dictIndex > 120)
                    sizeCompressed++;
                Console.WriteLine("OUTPUT VALUE {0} LOOKUP INDEX {1}", toString, dictIndex);
            }
            else
            {
                sizeNotCompressed += Encoding.UTF8.GetByteCount(toString) + 1;
                sizeCompressed += Encoding.UTF8.GetByteCount(toString) + 2;
                Console.WriteLine("OUTPUT VALUE {0} (NEW IN TABLE)", toString);
            }
        }


        private void OutputUnescapedValue(string toString)
        {
            Console.WriteLine("TODO: OUTPUT UNESCAPED VALUE {0}", toString);

            // NOT CORRECT, JUST TEMPORARY
            OutputStringValue(toString);
        }


        private void Parse(char c)
        {
            switch (currentState)
            {
                case EncoderState.WaitingPropertyColon:
                    if (IsWhiteSpace(c))
                        return;

                    if (c == ':')
                        currentState = EncoderState.WaitingPropertyValue;
                    else
                        ThrowUnexpectedCharacterException(c);
                    break;
                case EncoderState.InsideObject:
                    if (IsWhiteSpace(c))
                        return;

                    if (c == '}')
                    {
                        Output(CJsonCodes.Stop);
                        currentState = stateStack.Pop();
                        SetupWaitForNextValue();
                    }
                    else if (c == '"')
                    {
                        stateStack.Push(currentState);
                        incomingString.Clear();
                        currentState = EncoderState.InsideString;
                    }
                    else if (IsAllowedFirstLetterOfUnescapedPropertyName(c))
                    {
                        stateStack.Push(currentState);
                        incomingString.Clear();
                        currentState = EncoderState.InsideUnescapedPropName;

                        Parse(c);
                    }
                    else
                        ThrowUnexpectedCharacterException(c);
                    break;
                case EncoderState.WaitingComma:
                    if (IsWhiteSpace(c))
                        return;

                    if (c == ',')
                    {
                        sizeNotCompressed++;
                        currentState = stateStack.Pop();
                    }
                    else if (c == '}' || c == ']')
                    {
                        currentState = stateStack.Pop();

                        Parse(c);
                    }
                    else
                        ThrowUnexpectedCharacterException(c);

                    break;
                case EncoderState.Start:
                case EncoderState.InsideArray:

                case EncoderState.WaitingPropertyValue:
                    if (IsWhiteSpace(c))
                        return;

                    if (c == '{')
                    {
                        Output(CJsonCodes.StartObject);
                        stateStack.Push(currentState);
                        currentState = EncoderState.InsideObject;
                    }
                    else if (c == '[')
                    {
                        Output(CJsonCodes.StartArray);
                        stateStack.Push(currentState);
                        currentState = EncoderState.InsideArray;
                    }
                    else if (c == ']' && currentState == EncoderState.InsideArray)
                    {
                        Output(CJsonCodes.Stop);
                        currentState = stateStack.Pop();
                        SetupWaitForNextValue();
                    }
                    else if (c == '"')
                    {
                        stateStack.Push(currentState);
                        incomingString.Clear();
                        currentState = EncoderState.InsideString;
                    }
                    else if (IsAllowedUnescapedValueCharacter(c))
                    {
                        stateStack.Push(currentState);
                        incomingString.Clear();
                        currentState = EncoderState.InsideUnescapedValue;

                        Parse(c);
                    }
                    else
                        ThrowUnexpectedCharacterException(c);

                    break;

                case EncoderState.InsideUnescapedPropName:
                    if (IsAllowedUnescapedValueCharacter(c))
                        incomingString.Append(c);
                    else if (IsWhiteSpace(c) || c == ':')
                    {
                        currentState = EncoderState.WaitingPropertyColon;

                        OutputPropertyName(incomingString.ToString());

                        if (c == ':')
                            Parse(c);
                    }
                    else
                        ThrowUnexpectedCharacterException(c);
                    break;

                case EncoderState.InsideUnescapedValue:
                    if (IsAllowedUnescapedValueCharacter(c))
                        incomingString.Append(c);
                    else if (IsWhiteSpace(c) || c == '}' || c == ']' || c == ',')
                    {
                        currentState = stateStack.Pop();

                        OutputUnescapedValue(incomingString.ToString());

                        SetupWaitForNextValue();

                        Parse(c);
                    }
                    else
                        ThrowUnexpectedCharacterException(c);
                    break;

                case EncoderState.InsideString:
                    if (c == '\\')
                        currentState = EncoderState.InsideStringEscapeStart;
                    else if (c == '"')
                    {
                        currentState = stateStack.Pop();

                        if (currentState == EncoderState.InsideObject)
                        {
                            OutputPropertyName(incomingString.ToString());
                            stateStack.Push(currentState);
                            currentState = EncoderState.WaitingPropertyColon;
                        }
                        else if (currentState == EncoderState.InsideArray || currentState == EncoderState.Start
                                 || currentState == EncoderState.WaitingPropertyValue)
                        {
                            OutputStringValue(incomingString.ToString());

                            SetupWaitForNextValue();
                        }
                        else
                            throw new InvalidOperationException("Invalid encoding state here " + currentState);
                    }
                    else
                    {
                        if (char.IsControl(c))
                            ThrowUnexpectedCharacterException(c);

                        incomingString.Append(c);
                    }
                    break;

                default:
                    throw new InvalidOperationException("State " + currentState + " not handled.");
            }
        }


        private void SetupWaitForNextValue()
        {
            if (currentState == EncoderState.WaitingPropertyValue)
            {
                currentState = stateStack.Pop();
                if (currentState != EncoderState.InsideObject)
                {
                    throw new InvalidOperationException(
                        "Poped state " + currentState + ", expected state " + EncoderState.InsideObject);
                }
            }

            stateStack.Push(currentState);
            currentState = EncoderState.WaitingComma;
        }


        private void ThrowUnexpectedCharacterException(char c)
        {
            throw new NotImplementedException();
        }

        #region Nested type: EncoderState

        private enum EncoderState
        {
            Start,
            InsideArray,
            InsideObject,
            InsideUnescapedValue,
            InsideString,
            InsideStringEscapeStart,
            WaitingPropertyColon,
            WaitingPropertyValue,
            InsideUnescapedPropName,
            WaitingComma
        }

        #endregion
    }
}
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

using System;
using System.Collections.Generic;
using System.Text;

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
        ReferenceStartOffset = 16
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
            if (!this.symbolLookupTable.TryGetValue(symbol, out dictIndex))
            {
                dictIndex = -1;
                this.symbolLookupTable[symbol] = this.symbolCounter++;

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
            get { return this.sizeCompressed; }
        }

        public int SizeNotCompressed
        {
            get { return this.sizeNotCompressed; }
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
            this.sizeNotCompressed++;
            this.sizeCompressed++;
            Console.WriteLine("TODO: OUTPUT CODE " + code + " in state " + this.currentState);
        }


        private void OutputPropertyName(string propName)
        {
            int dictIndex;

            this.sizeNotCompressed += Encoding.UTF8.GetByteCount(propName) + 1;

            if (this.propertyNameSymbols.WriteSymbol(propName, out dictIndex))
            {
                this.sizeCompressed++;
                if (dictIndex > 120)
                    this.sizeCompressed++;
                Console.WriteLine("OUTPUT PROPNAME {0} LOOKUP INDEX {1}", propName, dictIndex);
            }
            else
            {
                this.sizeNotCompressed += Encoding.UTF8.GetByteCount(propName) + 1;
                this.sizeCompressed += Encoding.UTF8.GetByteCount(propName) + 2;
                Console.WriteLine("OUTPUT PROPNAME {0} (NEW IN TABLE)", propName);
            }
        }


        private void OutputStringValue(string toString)
        {
            int dictIndex;

            this.sizeNotCompressed += Encoding.UTF8.GetByteCount(toString) + 1;

            if (this.valueSymbols.WriteSymbol(toString, out dictIndex))
            {
                this.sizeCompressed++;
                if (dictIndex > 120)
                    this.sizeCompressed++;
                Console.WriteLine("OUTPUT VALUE {0} LOOKUP INDEX {1}", toString, dictIndex);
            }
            else
            {
                this.sizeNotCompressed += Encoding.UTF8.GetByteCount(toString) + 1;
                this.sizeCompressed += Encoding.UTF8.GetByteCount(toString) + 2;
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
            switch (this.currentState)
            {
                case EncoderState.WaitingPropertyColon:
                    if (IsWhiteSpace(c))
                        return;

                    if (c == ':')
                        this.currentState = EncoderState.WaitingPropertyValue;
                    else
                        ThrowUnexpectedCharacterException(c);
                    break;
                case EncoderState.InsideObject:
                    if (IsWhiteSpace(c))
                        return;

                    if (c == '}')
                    {
                        Output(CJsonCodes.Stop);
                        this.currentState = this.stateStack.Pop();
                        SetupWaitForNextValue();
                    }
                    else if (c == '"')
                    {
                        this.stateStack.Push(this.currentState);
                        this.incomingString.Clear();
                        this.currentState = EncoderState.InsideString;
                    }
                    else if (IsAllowedFirstLetterOfUnescapedPropertyName(c))
                    {
                        this.stateStack.Push(this.currentState);
                        this.incomingString.Clear();
                        this.currentState = EncoderState.InsideUnescapedPropName;

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
                        this.sizeNotCompressed++;
                        this.currentState = this.stateStack.Pop();
                    }
                    else if (c == '}' || c == ']')
                    {
                        this.currentState = this.stateStack.Pop();

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
                        this.stateStack.Push(this.currentState);
                        this.currentState = EncoderState.InsideObject;
                    }
                    else if (c == '[')
                    {
                        Output(CJsonCodes.StartArray);
                        this.stateStack.Push(this.currentState);
                        this.currentState = EncoderState.InsideArray;
                    }
                    else if (c == ']' && this.currentState == EncoderState.InsideArray)
                    {
                        Output(CJsonCodes.Stop);
                        this.currentState = this.stateStack.Pop();
                        SetupWaitForNextValue();
                    }
                    else if (c == '"')
                    {
                        this.stateStack.Push(this.currentState);
                        this.incomingString.Clear();
                        this.currentState = EncoderState.InsideString;
                    }
                    else if (IsAllowedUnescapedValueCharacter(c))
                    {
                        this.stateStack.Push(this.currentState);
                        this.incomingString.Clear();
                        this.currentState = EncoderState.InsideUnescapedValue;

                        Parse(c);
                    }
                    else
                        ThrowUnexpectedCharacterException(c);

                    break;

                case EncoderState.InsideUnescapedPropName:
                    if (IsAllowedUnescapedValueCharacter(c))
                        this.incomingString.Append(c);
                    else if (IsWhiteSpace(c) || c == ':')
                    {
                        this.currentState = EncoderState.WaitingPropertyColon;

                        OutputPropertyName(this.incomingString.ToString());

                        if (c == ':')
                            Parse(c);
                    }
                    else
                        ThrowUnexpectedCharacterException(c);
                    break;

                case EncoderState.InsideUnescapedValue:
                    if (IsAllowedUnescapedValueCharacter(c))
                        this.incomingString.Append(c);
                    else if (IsWhiteSpace(c) || c == '}' || c == ']' || c == ',')
                    {
                        this.currentState = this.stateStack.Pop();

                        OutputUnescapedValue(this.incomingString.ToString());

                        SetupWaitForNextValue();

                        Parse(c);
                    }
                    else
                        ThrowUnexpectedCharacterException(c);
                    break;

                case EncoderState.InsideString:
                    if (c == '\\')
                        this.currentState = EncoderState.InsideStringEscapeStart;
                    else if (c == '"')
                    {
                        this.currentState = this.stateStack.Pop();

                        if (this.currentState == EncoderState.InsideObject)
                        {
                            OutputPropertyName(this.incomingString.ToString());
                            this.stateStack.Push(this.currentState);
                            this.currentState = EncoderState.WaitingPropertyColon;
                        }
                        else if (this.currentState == EncoderState.InsideArray || this.currentState == EncoderState.Start
                                 || this.currentState == EncoderState.WaitingPropertyValue)
                        {
                            OutputStringValue(this.incomingString.ToString());

                            SetupWaitForNextValue();
                        }
                        else
                            throw new InvalidOperationException("Invalid encoding state here " + this.currentState);
                    }
                    else
                    {
                        if (char.IsControl(c))
                            ThrowUnexpectedCharacterException(c);

                        this.incomingString.Append(c);
                    }
                    break;

                default:
                    throw new InvalidOperationException("State " + this.currentState + " not handled.");
            }
        }


        private void SetupWaitForNextValue()
        {
            if (this.currentState == EncoderState.WaitingPropertyValue)
            {
                this.currentState = this.stateStack.Pop();
                if (this.currentState != EncoderState.InsideObject)
                {
                    throw new InvalidOperationException(
                        "Poped state " + this.currentState + ", expected state " + EncoderState.InsideObject);
                }
            }

            this.stateStack.Push(this.currentState);
            this.currentState = EncoderState.WaitingComma;
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
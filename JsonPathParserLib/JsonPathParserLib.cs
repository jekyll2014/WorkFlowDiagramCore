using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsonPathParserLib
{
    public class JsonPathParser
    {
        private string _jsonText;

        private List<ParsedProperty> _properties = new List<ParsedProperty>();

        private readonly char[] _escapeChars = new char[] { '\"', '\\', '/', 'b', 'f', 'n', 'r', 't', 'u' };
        private readonly char[] _allowedSpacerChars = new char[] { ' ', '\t', '\r', '\n' };
        private readonly char[] _endOfLineChars = new char[] { '\r', '\n' };
        private readonly char[] _keywordOrNumberChars = "-0123456789.truefalsnl".ToCharArray();
        private readonly string[] _keywords = { "true", "false", "null" };

        private bool _errorFound;
        private bool _searchMode;
        private string _searchPath;

        public bool TrimComplexValues { get; set; }
        public bool SaveComplexValues { get; set; }
        public char JsonPathDivider { get; set; } = '.';
        public string RootName { get; set; } = "root";
        public bool SearchStartOnly { get; set; }

        public IEnumerable<ParsedProperty> ParseJsonToPathList(string jsonText, out int endPosition, out bool errorFound)
        {
            _searchMode = false;
            _searchPath = string.Empty;
            var result = StartParser(jsonText, out endPosition, out errorFound);

            return result;
        }

        public IEnumerable<ParsedProperty> ParseJsonToPathList(string jsonText)
        {
            return StartParser(jsonText, out var _, out var _);
        }

        public ParsedProperty SearchJsonPath(string jsonText, string path)
        {
            _searchMode = true;
            _searchPath = path;
            var items = StartParser(jsonText, out var _, out var _).ToArray();

            if (!items.Any())
                return null;

            return items.FirstOrDefault(n => n.Path == path);
        }

        public static bool GetLinesNumber(string jsonText, int startPosition, int endPosition, out int startLine, out int endLine)
        {
            startLine = CountLinesFast(jsonText, 0, startPosition) + 1;
            endLine = startLine + CountLinesFast(jsonText, startPosition, endPosition);

            return true;
        }

        public static JsonValueType GetVariableType(string str)
        {
            var type = JsonValueType.Unknown;

            if (string.IsNullOrEmpty(str))
            {
                type = JsonValueType.Unknown;
            }
            else if (str.Length > 1 && str[0] == ('\"') && str[str.Length - 1] == ('\"'))
            {
                type = JsonValueType.String;
            }
            else if (str == "null")
            {
                type = JsonValueType.Null;
            }
            else if (str == "true" || str == "false")
            {
                type = JsonValueType.Boolean;
            }
            else if (IsNumeric(str))
            {
                if (str.Contains('.')) type = JsonValueType.Number;
                else type = JsonValueType.Integer;
            }

            return type;
        }

        public static string TrimObjectValue(string objectText)
        {
            return TrimBracketedValue(objectText, '{', '}');
        }

        public static string TrimArrayValue(string arrayText)
        {
            return TrimBracketedValue(arrayText, '[', ']');
        }

        public static string TrimBracketedValue(string text, char startChar, char endChar)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var startPosition = text.IndexOf(startChar);
            var endPosition = text.LastIndexOf(endChar);

            if (startPosition < 0 || endPosition <= 0 || endPosition <= startPosition)
            {
                return text;
            }

            if (endPosition - startPosition <= 1)
            {
                return string.Empty;
            }

            return text.Substring(startPosition + 1, endPosition - startPosition - 1).Trim();
        }

        // fool-proof
        public static int CountLines(string jsonText, int startIndex, int endIndex)
        {
            if (startIndex >= jsonText.Length)
                return -1;

            if (startIndex > endIndex)
            {
                var n = startIndex;
                startIndex = endIndex;
                endIndex = n;
            }

            if (endIndex >= jsonText.Length)
                endIndex = jsonText.Length;

            var endOfLineChars = new List<char>() { '\r', '\n' };
            endOfLineChars.AddRange(Environment.NewLine.ToCharArray());
            var linesCount = 0;
            char currentChar;
            char nextChar;
            for (; startIndex < endIndex; startIndex++)
            {
                currentChar = jsonText[startIndex];
                if (!endOfLineChars.Contains(currentChar))
                    continue;

                nextChar = jsonText[startIndex + 1];
                linesCount++;
                if (startIndex < endIndex - 1
                    && currentChar != nextChar
                    && endOfLineChars.Contains(nextChar))
                    startIndex++;
            }

            return linesCount;
        }

        public static int CountLinesFast(string jsonText, int startIndex, int endIndex)
        {
            var count = 0;
            while ((startIndex = jsonText.IndexOf('\n', startIndex)) != -1
                && startIndex < endIndex)
            {
                count++;
                startIndex++;
            }
            return count;
        }

        public IEnumerable<ParsedProperty> ConvertForTreeProcessing(IEnumerable<ParsedProperty> schemaProperties)
        {
            if (schemaProperties == null)
                return null;

            var result = new List<ParsedProperty>();

            foreach (var property in schemaProperties)
            {
                var tmpStr = new StringBuilder();
                tmpStr.Append(property.Path);
                var pos = tmpStr.ToString().IndexOf('[');
                while (pos >= 0)
                {
                    tmpStr.Insert(pos, JsonPathDivider);
                    pos = tmpStr.ToString().IndexOf('[', pos + 2);
                }

                var newProperty = new ParsedProperty(JsonPathDivider)
                {
                    Name = property.Name,
                    Path = tmpStr.ToString(),
                    JsonPropertyType = property.JsonPropertyType,
                    EndPosition = property.EndPosition,
                    StartPosition = property.StartPosition,
                    Value = property.Value,
                    ValueType = property.ValueType
                };

                result.Add(newProperty);
            }

            return result;
        }

        private IEnumerable<ParsedProperty> StartParser(string jsonText, out int endPosition, out bool errorFound)
        {
            _jsonText = jsonText;
            endPosition = 0;
            _errorFound = false;
            _properties = new List<ParsedProperty>();

            if (string.IsNullOrEmpty(jsonText))
            {
                errorFound = _errorFound;
                return _properties;
            }

            var currentPath = RootName;
            while (!_errorFound && endPosition < _jsonText.Length)
            {
                endPosition = FindStartOfNextToken(endPosition, out var foundObjectType);
                if (_errorFound || endPosition >= _jsonText.Length)
                    break;

                switch (foundObjectType)
                {
                    case JsonPropertyType.Property:
                        endPosition = GetPropertyName(endPosition, currentPath);
                        break;
                    case JsonPropertyType.Comment:
                        endPosition = GetComment(endPosition, currentPath);
                        break;
                    case JsonPropertyType.Object:
                        endPosition = GetObject(endPosition, currentPath);
                        break;
                    case JsonPropertyType.EndOfObject:
                        break;
                    case JsonPropertyType.Array:
                        endPosition = GetArray(endPosition, currentPath);
                        break;
                    case JsonPropertyType.EndOfArray:
                        break;
                    default:
                        _errorFound = true;
                        break;
                }

                endPosition++;
            }

            errorFound = _errorFound;
            return _properties;
        }

        private int FindStartOfNextToken(int pos, out JsonPropertyType foundObjectType)
        {
            foundObjectType = JsonPropertyType.Unknown;

            for (; pos < _jsonText.Length; pos++)
            {
                var currentChar = _jsonText[pos];
                switch (currentChar)
                {
                    case '/':
                        foundObjectType = JsonPropertyType.Comment;
                        return pos;
                    case '\"':
                        foundObjectType = JsonPropertyType.Property;
                        return pos;
                    case '{':
                        foundObjectType = JsonPropertyType.Object;
                        return pos;
                    case '}':
                        foundObjectType = JsonPropertyType.EndOfObject;
                        return pos;
                    case '[':
                        foundObjectType = JsonPropertyType.Array;
                        return pos;
                    case ']':
                        foundObjectType = JsonPropertyType.EndOfArray;
                        return pos;
                    default:
                        if (_keywordOrNumberChars.Contains(currentChar))
                        {
                            foundObjectType = JsonPropertyType.KeywordOrNumberProperty;
                            return pos;
                        }

                        var allowedChars = new[] { ' ', '\t', '\r', '\n', ',' };
                        if (!allowedChars.Contains(currentChar))
                        {
                            foundObjectType = JsonPropertyType.Error;
                            _errorFound = true;
                            return pos;
                        }

                        break;
                }
            }

            return pos;
        }

        private int GetComment(int pos, string currentPath)
        {
            if (_searchMode)
            {
                var lastItem = _properties?.LastOrDefault();
                if (lastItem?.Path == _searchPath)
                {
                    if (SearchStartOnly
                        || (lastItem?.JsonPropertyType != JsonPropertyType.Array
                        && lastItem?.JsonPropertyType != JsonPropertyType.Object))
                    {
                        _errorFound = true;
                        return pos;
                    }
                }
                else
                {
                    _properties?.Remove(_properties.LastOrDefault());
                }
            }

            var newElement = new ParsedProperty(JsonPathDivider)
            {
                JsonPropertyType = JsonPropertyType.Comment,
                StartPosition = pos,
                Path = currentPath,
                ValueType = JsonValueType.Unknown
            };
            _properties?.Add(newElement);

            pos++;

            if (pos >= _jsonText.Length)
            {
                _errorFound = true;
                return pos;
            }

            switch (_jsonText[pos])
            {
                //single line comment
                case '/':
                    {
                        pos++;
                        if (pos >= _jsonText.Length)
                        {
                            _errorFound = true;
                            return pos;
                        }

                        for (; pos < _jsonText.Length; pos++)
                        {
                            if (_endOfLineChars.Contains(_jsonText[pos])) //end of comment
                            {
                                pos--;
                                newElement.EndPosition = pos;
                                newElement.Value = _jsonText.Substring(newElement.StartPosition,
                                    newElement.EndPosition - newElement.StartPosition - 1);

                                return pos;
                            }
                        }

                        pos--;
                        newElement.EndPosition = pos;
                        newElement.Value = _jsonText.Substring(newElement.StartPosition);

                        return pos;
                    }
                //multi line comment
                case '*':
                    {
                        pos++;
                        if (pos >= _jsonText.Length)
                        {
                            _errorFound = true;
                            return pos;
                        }

                        for (; pos < _jsonText.Length; pos++)
                        {
                            if (_jsonText[pos] == '*') // possible end of comment
                            {
                                pos++;
                                if (pos >= _jsonText.Length)
                                {
                                    _errorFound = true;
                                    return pos;
                                }

                                if (_jsonText[pos] == '/')
                                {
                                    newElement.EndPosition = pos;
                                    newElement.Value = _jsonText.Substring(
                                        newElement.StartPosition,
                                        newElement.EndPosition - newElement.StartPosition + 1);

                                    return pos;
                                }

                                pos--;
                            }
                        }

                        break;
                    }
            }

            _errorFound = true;
            return pos;
        }

        private int GetPropertyName(int pos, string currentPath)
        {
            if (_searchMode)
            {
                var lastItem = _properties?.LastOrDefault();
                if (lastItem?.Path == _searchPath)
                {
                    if (SearchStartOnly
                        || (lastItem?.JsonPropertyType != JsonPropertyType.Array
                        && lastItem?.JsonPropertyType != JsonPropertyType.Object))
                    {
                        _errorFound = true;
                        return pos;
                    }
                }
                else
                {
                    _properties?.Remove(_properties.LastOrDefault());
                }
            }

            var newElement = new ParsedProperty(JsonPathDivider)
            {
                StartPosition = pos
            };
            _properties?.Add(newElement);

            pos++;
            for (; pos < _jsonText.Length; pos++) // searching for property name end
            {
                var currentChar = _jsonText[pos];

                if (currentChar == '\\') //skip escape chars
                {
                    pos++;
                    if (pos >= _jsonText.Length)
                    {
                        _errorFound = true;
                        return pos;
                    }

                    if (_escapeChars.Contains(_jsonText[pos]))
                    {
                        if (_jsonText[pos] == 'u') // if \u0000
                            pos += 4;
                    }
                    else
                    {
                        _errorFound = true;
                        return pos;
                    }
                }
                else if (currentChar == '\"') // end of property name found
                {
                    var newName = _jsonText.Substring(newElement.StartPosition, pos - newElement.StartPosition + 1);
                    pos++;

                    if (pos >= _jsonText.Length)
                    {
                        _errorFound = true;
                        return pos;
                    }

                    pos = GetPropertyDivider(pos, currentPath);

                    if (_errorFound)
                    {
                        return pos;
                    }

                    if (_jsonText[pos] == ',' || _jsonText[pos] == ']') // it's an array of values
                    {
                        pos--;
                        newElement.JsonPropertyType = JsonPropertyType.ArrayValue;
                        newElement.EndPosition = pos;
                        newElement.Path = currentPath;
                        newElement.ValueType = GetVariableType(newName);
                        newElement.Value = newElement.ValueType == JsonValueType.String ? newName.Trim('\"') : newName;
                        return pos;
                    }

                    newElement.Name = newName.Trim('\"');
                    pos++;
                    if (pos >= _jsonText.Length)
                    {
                        _errorFound = true;
                        return pos;
                    }

                    var valueStartPosition = pos;
                    pos = GetPropertyValue(pos, currentPath, ref valueStartPosition);
                    if (_errorFound)
                    {
                        return pos;
                    }

                    currentPath += JsonPathDivider + newElement.Name;
                    newElement.Path = currentPath;
                    switch (_jsonText[pos])
                    {
                        //it's an object
                        case '{':
                            newElement.JsonPropertyType = JsonPropertyType.Object;
                            newElement.EndPosition = pos = GetObject(pos, currentPath, false);
                            newElement.ValueType = JsonValueType.Object;

                            if (SaveComplexValues)
                            {
                                newElement.Value = _jsonText.Substring(newElement.StartPosition,
                                newElement.EndPosition - newElement.StartPosition + 1);

                                if (TrimComplexValues)
                                {
                                    newElement.Value = TrimObjectValue(newElement.Value);
                                }
                            }

                            return pos;
                        //it's an array
                        case '[':
                            newElement.JsonPropertyType = JsonPropertyType.Array;
                            newElement.EndPosition = pos = GetArray(pos, currentPath);
                            newElement.ValueType = JsonValueType.Array;

                            if (SaveComplexValues)
                            {
                                newElement.Value = _jsonText.Substring(newElement.StartPosition,
                                    newElement.EndPosition - newElement.StartPosition + 1);

                                if (TrimComplexValues)
                                {
                                    newElement.Value = TrimArrayValue(newElement.Value);
                                }
                            }

                            return pos;
                        // it's a property
                        default:
                            newElement.JsonPropertyType = JsonPropertyType.Property;
                            newElement.EndPosition = pos;
                            var newValue = _jsonText
                                .Substring(valueStartPosition, pos - valueStartPosition + 1)
                                .Trim();
                            newElement.ValueType = GetVariableType(newValue);
                            newElement.Value = newElement.ValueType == JsonValueType.String ? newValue.Trim('\"') : newValue;
                            return pos;
                    }
                }
                else if (_endOfLineChars.Contains(currentChar)) // check restricted chars
                {
                    _errorFound = true;
                    return pos;
                }
            }

            _errorFound = true;
            return pos;
        }

        private int GetPropertyDivider(int pos, string currentPath)
        {
            for (; pos < _jsonText.Length; pos++)
            {
                switch (_jsonText[pos])
                {
                    case ':':
                    case ']': // ????
                    case ',': // ????
                        return pos;
                    case '/':
                        pos = GetComment(pos, currentPath);
                        break;
                    default:
                        if (!_allowedSpacerChars.Contains(_jsonText[pos]))
                        {
                            _errorFound = true;
                            return pos;
                        }
                        break;
                }
            }

            _errorFound = true;
            return pos;
        }

        private int GetPropertyValue(int pos, string currentPath, ref int propertyStartPos)
        {
            for (; pos < _jsonText.Length; pos++)
            {
                switch (_jsonText[pos])
                {
                    // it's a start of array
                    case '[':
                    // or object
                    case '{':
                        return pos;
                    case '/':
                        //it's a comment
                        pos = GetComment(pos, currentPath);
                        propertyStartPos = pos + 1;
                        break;
                    //it's a start of value string 
                    case '\"':
                        {
                            pos++;

                            for (; pos < _jsonText.Length; pos++)
                            {
                                if (_jsonText[pos] == '\\') //skip escape chars
                                {
                                    pos++;
                                    if (pos >= _jsonText.Length)
                                    {
                                        _errorFound = true;
                                        return pos;
                                    }

                                    if (_escapeChars.Contains(_jsonText[pos]))
                                    {
                                        if (_jsonText[pos] == 'u') // if \u0000
                                            pos += 4;

                                        continue;
                                    }
                                    else
                                    {
                                        _errorFound = true;
                                        return pos;
                                    }
                                }
                                else if (_jsonText[pos] == '\"')
                                {
                                    return pos;
                                }
                                else if (_endOfLineChars.Contains(_jsonText[pos])) // check restricted chars
                                {
                                    _errorFound = true;
                                    return pos;
                                }
                            }

                            _errorFound = true;
                            return pos;
                        }
                    default:
                        if (!_allowedSpacerChars.Contains(_jsonText[pos])) // it's a literal property value
                        {
                            // ???? check this
                            // var endingChars = new[] { ',', ' ', '\t', '\r', '\n' };
                            var endingChars = new[] { ',', ']', '}', ' ', '\t', '\r', '\n', '/' };
                            for (; pos < _jsonText.Length; pos++)
                            {
                                // value end found
                                if (endingChars.Contains(_jsonText[pos]))
                                {
                                    pos--;
                                    return pos;
                                }

                                // non-allowed char found
                                if (!_keywordOrNumberChars.Contains(_jsonText[pos])) // check restricted chars
                                {
                                    _errorFound = true;
                                    return pos;
                                }
                            }
                        }
                        break;
                }
            }

            _errorFound = true;
            return pos;
        }

        private int GetArray(int pos, string currentPath)
        {
            pos++;
            var arrayIndex = 0;
            for (; pos < _jsonText.Length; pos++)
            {
                pos = FindStartOfNextToken(pos, out var foundObjectType);
                if (_errorFound)
                {
                    return pos;
                }

                switch (foundObjectType)
                {
                    case JsonPropertyType.Comment:
                        pos = GetComment(pos, currentPath + "[" + arrayIndex + "]");
                        arrayIndex++;
                        break;
                    case JsonPropertyType.Property:
                        pos = GetPropertyName(pos, currentPath + "[" + arrayIndex + "]");
                        arrayIndex++;
                        break;
                    case JsonPropertyType.Object:
                        pos = GetObject(pos, currentPath + "[" + arrayIndex + "]");
                        arrayIndex++;
                        break;
                    case JsonPropertyType.KeywordOrNumberProperty:
                        pos = GetKeywordOrNumber(pos, currentPath + "[" + arrayIndex + "]", true);
                        arrayIndex++;
                        break;
                    case JsonPropertyType.Array:
                        pos = GetArray(pos, currentPath);
                        break;
                    case JsonPropertyType.EndOfArray:
                        if (_searchMode && currentPath == _searchPath)
                        {
                            _errorFound = true;
                        }
                        return pos;
                    default:
                        _errorFound = true;
                        return pos;
                }

                if (_errorFound)
                {
                    return pos;
                }
            }

            _errorFound = true;
            return pos;
        }

        private int GetObject(int pos, string currentPath, bool save = true)
        {
            if (_searchMode)
            {
                var lastItem = _properties?.LastOrDefault();
                if (lastItem?.Path == _searchPath)
                {
                    if (SearchStartOnly
                        || (lastItem?.JsonPropertyType != JsonPropertyType.Array
                        && lastItem?.JsonPropertyType != JsonPropertyType.Object))
                    {
                        _errorFound = true;
                        return pos;
                    }
                }
                else
                {
                    _properties?.Remove(_properties.LastOrDefault());
                }
            }

            var newElement = new ParsedProperty(JsonPathDivider);

            if (save)
            {
                newElement.StartPosition = pos;
                newElement.JsonPropertyType = JsonPropertyType.Object;
                newElement.Path = currentPath;
                newElement.ValueType = JsonValueType.Object;
                _properties?.Add(newElement);
            }

            pos++;

            for (; pos < _jsonText.Length; pos++)
            {
                pos = FindStartOfNextToken(pos, out var foundObjectType);
                if (_errorFound)
                {
                    return pos;
                }

                switch (foundObjectType)
                {
                    case JsonPropertyType.Comment:
                        pos = GetComment(pos, currentPath);
                        break;
                    case JsonPropertyType.Property:
                        pos = GetPropertyName(pos, currentPath);
                        break;
                    case JsonPropertyType.Array:
                        pos = GetArray(pos, currentPath);
                        break;
                    case JsonPropertyType.Object:
                        pos = GetObject(pos, currentPath);
                        break;
                    case JsonPropertyType.EndOfObject:
                        if (save)
                        {
                            newElement.EndPosition = pos;
                            if (SaveComplexValues)
                            {
                                newElement.Value = _jsonText.Substring(newElement.StartPosition,
                                    newElement.EndPosition - newElement.StartPosition + 1);

                                if (TrimComplexValues)
                                {
                                    newElement.Value = TrimObjectValue(newElement.Value);
                                }
                            }

                            if (_searchMode && currentPath == _searchPath)
                            {
                                _errorFound = true;
                                return pos;
                            }
                        }

                        return pos;
                    default:
                        _errorFound = true;
                        return pos;
                }

                if (_errorFound)
                {
                    return pos;
                }
            }

            _errorFound = true;
            return pos;
        }

        private int GetKeywordOrNumber(int pos, string currentPath, bool isArray)
        {
            if (_searchMode)
            {
                var lastItem = _properties?.LastOrDefault();
                if (lastItem?.Path == _searchPath)
                {
                    if (SearchStartOnly
                        || (lastItem?.JsonPropertyType != JsonPropertyType.Array
                        && lastItem?.JsonPropertyType != JsonPropertyType.Object))
                    {
                        _errorFound = true;
                        return pos;
                    }
                }
                else
                {
                    _properties?.Remove(_properties.LastOrDefault());
                }
            }

            var newElement = new ParsedProperty(JsonPathDivider)
            {
                StartPosition = pos
            };
            _properties?.Add(newElement);

            var endingChars = new[] { ',', '}', ']', '\r', '\n', '/' };

            for (; pos < _jsonText.Length; pos++) // searching for token end
            {
                var currentChar = _jsonText[pos];
                // end of token found
                if (endingChars.Contains(currentChar))
                {
                    pos--;
                    var newValue = _jsonText
                        .Substring(newElement.StartPosition, pos - newElement.StartPosition + 1)
                        .Trim();

                    if (!_keywords.Contains(newValue) && !IsNumeric(newValue))
                    {
                        _errorFound = true;
                        return pos;
                    }

                    newElement.Value = newValue;
                    newElement.JsonPropertyType = isArray ? JsonPropertyType.ArrayValue : JsonPropertyType.Property;
                    newElement.EndPosition = pos;
                    newElement.Path = currentPath;
                    newElement.ValueType = GetVariableType(newValue);

                    return pos;
                }

                if (!_keywordOrNumberChars.Contains(currentChar)) // check restricted chars
                {
                    _errorFound = true;
                    return pos;
                }
            }

            _errorFound = true;
            return pos;
        }

        private static bool IsNumeric(string str)
        {
            return Double.TryParse(str, out double _);
        }
    }
}

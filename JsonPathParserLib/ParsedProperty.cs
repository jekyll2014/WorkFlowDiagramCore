namespace JsonPathParserLib
{
    public class ParsedProperty
    {
        public int StartPosition = -1;
        public int EndPosition = -1;
        public string Name = "";
        public string Value = "";
        public JsonPropertyType JsonPropertyType = JsonPropertyType.Unknown;
        public JsonValueType ValueType;
        public char PathDivider = '.';
        private string _path = "";
        public string Path
        {
            get => _path;
            set
            {
                _parentPath = null;
                _path = value;
            }
        }

        private string _parentPath;

        public ParsedProperty(char pathDivider)
        {
            PathDivider = pathDivider;
        }

        public string ParentPath
        {
            get
            {
                if (_parentPath == null)
                {
                    _parentPath = TrimPathEnd(Path, 1, PathDivider);
                }

                return _parentPath;
            }
        }

        public int RawLength
        {
            get
            {
                if (StartPosition == -1 || EndPosition == -1)
                    return -1;

                return EndPosition - StartPosition + 1;
            }
        }

        public int Depth
        {
            get
            {
                if (string.IsNullOrEmpty(_path))
                    return 0;

                return _path.Split(PathDivider).Length;
            }
        }

        private static string TrimPathEnd(string originalPath, int levels, char pathDivider)
        {
            for (; levels > 0; levels--)
            {
                var pos = originalPath.LastIndexOf(pathDivider);
                if (pos >= 0)
                {
                    originalPath = originalPath.Substring(0, pos);
                }
                else
                    break;
            }

            return originalPath;
        }

        public override string ToString()
        {
            return Path;
        }
    }
}

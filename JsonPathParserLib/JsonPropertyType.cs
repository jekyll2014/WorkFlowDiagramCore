namespace JsonPathParserLib
{
    public enum JsonPropertyType
    {
        Unknown,
        Comment,
        Property,
        KeywordOrNumberProperty,
        ArrayValue,
        Object,
        EndOfObject,
        Array,
        EndOfArray,
        Error
    }
}

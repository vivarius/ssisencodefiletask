namespace SSISEncodeFileTask100
{
    internal static class NamedStringMembers
    {
        public const string FILE_CONNECTOR = "FILE_CONNECTOR";
        public const string FileSourceFile = "FileSourceFile";
        public const string EncodingType = "EncodingType";
        public const string SourceType = "SourceType";
    }

    internal enum SourceFileType
    {
        FromFileConnector,
        FromFilePath
    }
}

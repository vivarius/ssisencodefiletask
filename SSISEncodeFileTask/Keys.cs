namespace SSISEncodeFileTask100
{
    internal static class Keys
    {
        public const string FILE_CONNECTOR = "FileConnector";
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

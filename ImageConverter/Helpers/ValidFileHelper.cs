namespace ImageConverter.Helpers
{
    public static class ValidFileHelper
    {
        public static List<string> fileTypesSupported;
        static ValidFileHelper()
        {
            fileTypesSupported = new List<string>()
            {
                "PNG",
                "TIF",
                "JPG",
                "ICO",
                "GIF"
            };
        }

        public static bool IsValid(string fileType)
        {
            return fileTypesSupported.Contains(fileType.ToUpper());
        }
    }
}

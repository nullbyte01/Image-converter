namespace Blazor.Converter.Client.Helpers
{
    public static class ValidFileHelper
    {
        public static List<string> fileTypesSupported;
        static ValidFileHelper()
        {
            fileTypesSupported = new List<string>()
            {
                "PNG",
                "JPEG",
                "TIF",
                "JPG",
                "SVG",
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

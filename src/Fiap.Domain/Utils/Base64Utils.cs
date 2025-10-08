namespace Fiap.Domain.Utils
{
    [ExcludeFromCodeCoverage]
    public static class Base64Utils
    {
        public static string DecodeBase64String(string input)
        {
            if (IsBase64String(input))
            {
                byte[] data = Convert.FromBase64String(input);
                return System.Text.Encoding.UTF8.GetString(data);
            }
            else
            {
                throw new ArgumentException("Input not valid base64 string");
            }
        }

        public static bool IsBase64String(string input)
        {
            if (string.IsNullOrEmpty(input) ||
                input.Length % 4 != 0 ||
                input.Contains(" ") ||
                input.Contains("\t") ||
                input.Contains("\r") ||
                input.Contains("\n"))
                return false;

            try
            {
                Convert.FromBase64String(input);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}

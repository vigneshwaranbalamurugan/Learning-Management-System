namespace LMSApi.BALLibrary.Utils
{
    public static class MaskingUtils
    {
        public static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return string.Empty;
            var parts = email.Split('@');
            if (parts.Length != 2) return email;

            var name = parts[0];
            var domain = parts[1];

            if (name.Length <= 2)
            {
                return name[0] + "*@" + domain;
            }

            return name[0] + new string('*', name.Length - 2) + name[^1] + "@" + domain;
        }
    }
}

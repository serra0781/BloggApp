namespace BlogApp.Helpers
{
    // Kullanıcı profil fotoğrafı yüklemesi olmadığı için (dosya depolama gerektirmez),
    // e-postadan deterministik olarak renkli baş harf avatarı üretir. Aynı kullanıcı
    // her zaman aynı rengi ve harfi alır.
    public static class AvatarHelper
    {
        private static readonly string[] Palette =
        {
            "C9184A", "FF4D6D", "7209B7", "B5179E", "F72585", "A4133C", "560BAD"
        };

        public static string GetColor(string? seed)
        {
            if (string.IsNullOrEmpty(seed))
            {
                return Palette[0];
            }

            var hash = 0;
            foreach (var c in seed)
            {
                hash = (hash * 31 + c) & int.MaxValue;
            }

            return Palette[hash % Palette.Length];
        }

        public static string GetInitial(string? seed)
        {
            if (string.IsNullOrEmpty(seed))
            {
                return "?";
            }

            return seed.Trim().Substring(0, 1).ToUpper();
        }
    }
}

using ICities;
using MoreBeautification.TranslationFramework;

namespace MoreBeautification
{
    public class MoreBeautification : IUserMod
    {
        public static Translation translation = new Translation();

        public string Name => "More Beautification";

        public string Description => translation.GetTranslation("MB_DESCRIPTION");
    }
}

using Core.Domain.Locale;
using Core.Domain.Settings;
using Core.Domain.Types;

namespace Core.Service.Game {
    public interface IYisoGameService : IYisoService {
        public YisoLocale.Locale GetCurrentLocale();
        public void SetCurrentLocale(YisoLocale.Locale locale);
        public YisoPlatforms GetPlatform();
        public bool IsDevelopMode();
        public bool IsSaveData();
        public bool IsMobile();
    }
}
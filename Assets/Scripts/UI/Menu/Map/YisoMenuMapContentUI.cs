using Core.Domain.Types;
using UI.Menu.Base;

namespace UI.Menu.Map {
    public class YisoMenuMapContentUI : YisoMenuBasePanelUI {
        public override void ClearPanel() { }
        public override YisoMenuTypes GetMenuType() => YisoMenuTypes.MAP;
    }
}
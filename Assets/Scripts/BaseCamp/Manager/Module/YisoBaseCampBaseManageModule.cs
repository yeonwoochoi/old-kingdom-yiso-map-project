namespace BaseCamp.Manager.Module {
    public abstract class YisoBaseCampBaseManageModule {
        protected readonly YisoBaseCampManager manager;

        protected YisoBaseCampBaseManageModule(YisoBaseCampManager manager) {
            this.manager = manager;
        }
    }
}
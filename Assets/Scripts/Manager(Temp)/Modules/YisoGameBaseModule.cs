namespace Manager.Modules {
    public abstract class YisoGameBaseModule {
        protected readonly GameManager manager;

        public virtual void OnEnabled() {
        }

        public virtual void OnDisabled() {
        }

        public virtual void OnDestroy() {
        }

        public virtual void OnUpdate() {
        }

        public YisoGameBaseModule(GameManager manager) {
            this.manager = manager;
        }
    }
}
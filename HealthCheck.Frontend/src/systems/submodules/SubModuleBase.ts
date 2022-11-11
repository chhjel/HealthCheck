export default abstract class SubModuleBase<TOptions> {
    protected _element: HTMLElement;
    protected _options: TOptions;
    protected abstract _defaultOptions: TOptions;

    public preInitializeModule(element: HTMLElement, optionsFromAttribute: Partial<TOptions>) {
        this._element = element;
        this._options = { ...this._defaultOptions, ...optionsFromAttribute };
    }

    abstract init(): void;
}

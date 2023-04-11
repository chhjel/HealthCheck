import SubModuleBase from '@systems/submodules/SubModuleBase';
import { registerGlobalComponents } from 'src/entry';
import { createApp } from 'vue';
import DataTableSubmoduleComponent from './DataTableSubmoduleComponent.vue';

export interface DataTableSubmoduleOptions {
    Headers: Array<string>;
    Rows: Array<Array<string>>;
}

export default class DataTableSubmodule extends SubModuleBase<DataTableSubmoduleOptions> {
    protected _defaultOptions: DataTableSubmoduleOptions = {
        Headers: [],
        Rows: []
    };

    init(): void {
        let props = { subModuleOptions: this._options };
        const app = createApp(DataTableSubmoduleComponent, props);
        registerGlobalComponents(app);
        app.mount(this._element);
    }
}

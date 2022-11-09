import SubModuleBase from '@systems/submodules/SubModuleBase';
import { registerGlobalComponents } from 'src/entry';
import { createApp } from 'vue';
import DataTableSubmoduleComponent from './DataTableSubmoduleComponent.vue';

interface DataTableSubmoduleOptions {
}

export default class DataTableSubmodule extends SubModuleBase<DataTableSubmoduleOptions> {
    protected _defaultOptions: DataTableSubmoduleOptions = {
    };

    init(): void {
        console.log(`DataTableSubmodule initialized`, this._options);
            
        let props = { subModuleOptions: this._options };
        const app = createApp(DataTableSubmoduleComponent, props);
        registerGlobalComponents(app);
        app.mount(this._element);
    }
}

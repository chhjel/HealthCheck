<!-- src/components/modules/Settings/SettingsPageComponent.vue -->
<template>
    <div class="settings-page">
        <content-component class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
            <v-container>
                <h1 class="mb-4">Settings</h1>

                <!-- LOAD PROGRESS -->
                <progress-linear-component 
                    v-if="loadStatus.inProgress"
                    indeterminate color="green"></progress-linear-component>

                <!-- DATA LOAD ERROR -->
                <alert-component :value="loadStatus.failed" v-if="loadStatus.failed" type="error">
                {{ loadStatus.errorMessage }}
                </alert-component>

                <div v-for="(group, gIndex) in settingGroups"
                    :key="`setting-group-${gIndex}`"
                    class="setting-group"
                    :class="{ 'without-header': group.name == null, 'with-header': group.name != null }">
                    
                    <div v-if="group.name != null" class="setting-group-header">
                        <h2 class="setting-group-header--name">{{ group.name }}</h2>
                        <p v-if="group.description != null" class="setting-group-header--desc">{{ group.description }}}</p>
                    </div>
                    
                    <backend-input-component
                        v-for="(setting, sIndex) in group.settings"
                        :key="`setting-item-${gIndex}-${sIndex}`"
                        class="setting-item"
                        v-model:value="setting.value"
                        :config="setting.definition"
                        :readonly="!HasAccessToChangeSettings"
                        />
                </div>

                <v-layout v-if="settingGroups.length > 0">
                    <v-flex xs6 sm2 class="mb-2">
                        <btn-component
                            v-if="HasAccessToChangeSettings"
                            @click="saveSettings()" 
                            class="primary"
                            :disabled="saveStatus.inProgress">{{ saveButtonText }}</btn-component>
                    </v-flex>

                    <!-- SAVE ERROR -->
                    <div v-if="saveStatus.failed" class="save-error">
                    {{ saveStatus.errorMessage }}
                    </div>
                </v-layout>

            </v-container>
        </v-flex>
        </v-layout>
        </v-container>
        </content-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import DateUtils from '@util/DateUtils';
import LinqUtils from '@util/LinqUtils';
import SettingsService from '@services/SettingsService';
import { FetchStatus,  } from '@services/abstractions/HCServiceBase';
import ModuleConfig from '@models/Common/ModuleConfig';
import ModuleOptions from '@models/Common/ModuleOptions';
import { HCBackendInputConfig } from "@generated/Models/Core/HCBackendInputConfig";
import BackendInputComponent from "@components/Common/Inputs/BackendInputs/BackendInputComponent.vue";
import { GetSettingsViewModel } from "@generated/Models/Core/GetSettingsViewModel";
import { SetSettingsViewModel } from "@generated/Models/Core/SetSettingsViewModel";
import { StoreUtil } from "@util/StoreUtil";

interface CustomSettingGroup
{
    name: string | null;
    settings: Array<CustomSetting>;
}
interface CustomSetting
{
    definition: HCBackendInputConfig;
    value: string;
}

@Options({
    components: {
        BackendInputComponent
    }
})
export default class SettingsPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;

    service: SettingsService = new SettingsService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    loadStatus: FetchStatus = new FetchStatus();
    saveStatus: FetchStatus = new FetchStatus();

    settingGroups: Array<CustomSettingGroup> = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.loadData();
    }

    created(): void {
    }

    beforeDestroy(): void {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }
    
    get saveButtonText(): string {
        return (this.saveStatus.inProgress) ? 'Saving..' : 'Save';
    }

    get HasAccessToChangeSettings(): boolean {
        return this.options.AccessOptions.indexOf("ChangeSettings") != -1;
    }
    
    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.service.GetSettings(this.loadStatus, { onSuccess: (data) => this.onDataRetrieved(data) });
    }

    onDataRetrieved(data: GetSettingsViewModel): void {
        this.settingGroups = LinqUtils.GroupByInto(data.Definitions,
            (def) => def.ExtraValues['GroupName'] || "",
            (key, defs) => {
                const group: CustomSettingGroup = {
                    name: (key === "") ? null : key,
                    settings: defs.map(d => {
                        const value = data.Values[d.Id];
                        const setting: CustomSetting = {
                            definition: d,
                            value: value
                        };
                        return setting;
                    })
                }
                return group;
            });
    }

    saveSettings(): void {
        const values: any = {};
        this.settingGroups.map(x => x.settings)
            .forEach(group => {
                group.forEach(set => {
                    values[set.definition.Id] = set.value;
                });
            });
        const payload: SetSettingsViewModel = {
            Values: values
        };
        this.service.SaveSettings(payload, this.saveStatus);
    }

    hasAccess(option: string): boolean {
        return this.options.AccessOptions.indexOf(option) != -1;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    
}
</script>

<style scoped lang="scss">
.settings-page {
    .setting-group {
        margin-bottom: 40px;
        margin-top: 15px;
        padding: 20px;
        border-radius: 25px;
        background-color: #fff;
        box-shadow: #d5d7d5 4px 4px 6px 0px;

        /* &.without-header {}
        &.with-header {} */

        .setting-group-header {
            padding-bottom: 10px;

            /* .setting-group-header--name {}
            .setting-group-header--desc {} */
        }

        .setting-item {
            padding-bottom: 10px;
        }
    }

    .save-error {
        color: red;
        display: flex;
        align-items: center;
        padding-bottom: 5px;
    }
}
</style>

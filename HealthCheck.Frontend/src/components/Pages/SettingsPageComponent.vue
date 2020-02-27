<!-- src/components/Pages/SettingsPageComponent.vue -->
<template>
    <div class="settings-page">
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
            <v-container>
                <h1 class="mb-4">Settings</h1>

                <!-- LOAD PROGRESS -->
                <v-progress-linear 
                    v-if="dataLoadInProgress"
                    indeterminate color="green"></v-progress-linear>

                <!-- DATA LOAD ERROR -->
                <v-alert :value="dataLoadFailed" v-if="dataLoadFailed" type="error">
                {{ dataFailedErrorMessage }}
                </v-alert>

                <div v-for="(group, gIndex) in settingGroups"
                    :key="`setting-group-${gIndex}`"
                    class="setting-group"
                    :class="{ 'without-header': group.name == null, 'with-header': group.name != null }">
                    
                    <div v-if="group.name != null" class="setting-group-header">
                        <h2 class="setting-group-header--name">{{ group.name }}</h2>
                        <p v-if="group.description != null" class="setting-group-header--desc">{{ group.description }}}</p>
                    </div>
                    
                    <setting-input-component v-for="(setting, sIndex) in group.settings"
                        :key="`setting-group-${gIndex}-${sIndex}`"
                        class="setting-item"
                        :setting="setting" />
                </div>

                <v-layout>
                    <v-flex xs6 sm2 class="mb-2">
                        <v-btn 
                            @click="saveSettings()" 
                            class="primary"
                            :disabled="isSaving">{{ saveButtonText }}</v-btn>
                    </v-flex>

                    <!-- SAVE ERROR -->
                    <div v-if="saveError != null" class="save-error">
                    {{ saveError }}
                    </div>
                </v-layout>

            </v-container>
        </v-flex>
        </v-layout>
        </v-container>
        </v-content>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../../models/Page/FrontEndOptionsViewModel';
import DateUtils from "../../util/DateUtils";
import LinqUtils from "../../util/LinqUtils";
import UrlUtils from "../../util/UrlUtils";
import SettingInputComponent from '../Settings/SettingInputComponent.vue';

export interface CustomSetting
{
    id: string;
    displayName: string;
    description: string | null;
    type: 'Boolean' | 'String' | 'Int32';
    value: any;
    validationError: string | null;
}

interface CustomSettingGroup
{
    name: string | null;
    settings: Array<CustomSetting>;
}

interface GetSettingsModel {
    Settings: Array<BackendSetting>;
}
interface BackendSetting
{
    Id: string;
    DisplayName: string;
    Description: string | null;
    Type: 'Boolean' | 'String' | 'Int32';
    Value: any;
    GroupName: string | null;
}

@Component({
    components: {
        SettingInputComponent
    }
})
export default class SettingsPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    dataLoadInProgress: boolean = false;
    dataLoadFailed: boolean = false;
    dataFailedErrorMessage: string = '';

    isSaving: boolean = false;
    saveError: string | null = null;

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
    get saveButtonText(): string {
        return (this.isSaving) ? 'Saving..' : 'Save';
    }
    
    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.dataLoadInProgress = true;
        this.dataLoadFailed = false;

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.GetSettingsEndpoint}${queryStringIfEnabled}`;
        fetch(url, {
            credentials: 'include',
            method: "GET",
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json',
            })
        })
        .then(response => response.json())
        .then((data: GetSettingsModel) => this.onDataRetrieved(data))
        .catch((e) => {
            this.dataLoadInProgress = false;
            this.dataLoadFailed = true;
            this.dataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
    }

    onDataRetrieved(data: GetSettingsModel): void {
        this.dataLoadInProgress = false;

        this.settingGroups = LinqUtils.GroupByInto(data.Settings, (item) => item.GroupName || "", (key, items) => {
            return {
                name: (key === "") ? null : key,
                settings: items.map(s => {
                    return {
                        id: s.Id,
                        displayName: s.DisplayName,
                        description: s.Description,
                        type: s.Type,
                        value: s.Value,
                        validationError: null,
                    };
                })
            }
        });
    }

    saveSettings(): void {
        this.isSaving = true;
        this.saveError = null;

        let settings = this.settingGroups
                .map(x => x.settings)
                .reduce((a: CustomSetting[], b: CustomSetting[]) => a.concat(b))
                .map((x: CustomSetting) => {
                    return {
                        Id: x.id,
                        Value: x.value
                    };
                });

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.SetSettingsEndpoint}${queryStringIfEnabled}`;
        let payload = {
            settings: settings
        };
        fetch(url, {
            credentials: 'include',
            method: "POST",
            body: JSON.stringify(payload),
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json',
            })
        })
        // .then(response => response.json())
        // .then((data: any) => this.onDataRetrieved(data))
        .then(response => this.isSaving = false)
        .catch((e) => {
            this.isSaving = false;
            this.saveError = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
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

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
                    v-if="loadStatus.inProgress"
                    indeterminate color="green"></v-progress-linear>

                <!-- DATA LOAD ERROR -->
                <v-alert :value="loadStatus.failed" v-if="loadStatus.failed" type="error">
                {{ loadStatus.errorMessage }}
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

                <v-layout v-if="settingGroups.length > 0">
                    <v-flex xs6 sm2 class="mb-2">
                        <v-btn 
                            @click="saveSettings()" 
                            class="primary"
                            :disabled="saveStatus.inProgress">{{ saveButtonText }}</v-btn>
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
import HCSettingsService, { GetSettingsModel, BackendSetting, CustomSetting, CustomSettingGroup }  from '../../services/HCSettingsService';
import { FetchStatus,  } from "../../services/abstractions/HCServiceBase";

@Component({
    components: {
        SettingInputComponent
    }
})
export default class SettingsPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    service: HCSettingsService = new HCSettingsService(this.options);
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
    get saveButtonText(): string {
        return (this.saveStatus.inProgress) ? 'Saving..' : 'Save';
    }
    
    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.service.GetSettings(this.loadStatus, (data) => this.onDataRetrieved(data));
    }

    onDataRetrieved(data: GetSettingsModel): void {
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
        this.service.SaveSettings(this.settingGroups, this.saveStatus);
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

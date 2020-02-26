<!-- src/components/Pages/SettingsPageComponent.vue -->
<template>
    <div class="settings-page">
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->

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
    name: string;
    description: string | null;
    type: 'Boolean' | 'String' | 'Int32';
    validationError: string | null;
}

interface CustomSettingGroup
{
    name: string | null;
    settings: Array<CustomSetting>;
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

    settingGroups: Array<CustomSettingGroup> = [
        {
            name: null,
            settings: [
                {
                    name: 'Some string',
                    description: 'A description goes here.',
                    type: 'String',
                    validationError: null
                },
                {
                    name: 'Checky check',
                    description: 'Checky check check.',
                    type: 'Boolean',
                    validationError: null
                }
            ]
        },
        {
            name: 'Data flows',
            settings: [
                {
                    name: 'A bool',
                    description: 'Something here.',
                    type: 'Boolean',
                    validationError: 'Some validation issue!'
                },
                {
                    name: 'A string',
                    description: null,
                    type: 'Int32',
                    validationError: null
                },
                {
                    name: 'A string',
                    description: null,
                    type: 'String',
                    validationError: null
                }
            ]
        }
    ];

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
    
    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.dataLoadInProgress = true;
        this.dataLoadFailed = false;

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.DiagramsDataEndpoint}${queryStringIfEnabled}`;
        fetch(url, {
            credentials: 'include',
            method: "GET",
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json',
            })
        })
        .then(response => response.json())
        .then((data: any) => this.onDataRetrieved(data))
        .catch((e) => {
            this.dataLoadInProgress = false;
            this.dataLoadFailed = true;
            this.dataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
    }

    onDataRetrieved(data: any): void {
        this.dataLoadInProgress = false;
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
}
</style>

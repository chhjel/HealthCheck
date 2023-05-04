<!-- src/components/modules/IPWhitelist/IPWhitelistPageComponent.vue -->
<template>
    <div class="ip-whitelist-config">
        <BlockComponent title="Blocked response">
            <div v-if="wlconfig">
                <InputHeaderComponent name="Response content" description="Supports html." class="mt-3" />
                <editor-component
                    class="editor"
                    :language="'html'"
                    v-model:value="wlconfig.DefaultResponse"
                    :read-only="isLoading"
                    ref="editor" />
                
                <CheckboxComponent 
                    v-model:value="wlconfig.UseDefaultResponseWrapper"
                    label="Use default response page wrapper"
                    title="If true, a default page html will be wrapped around the content above."
                    class="mb-3" />
                
                <text-field-component v-model:value="wlconfig.DefaultHttpStatusCode"
                    label="Status code"
                    type="number"
                    class="mb-3" />
            </div>
            
            <btn-component @disabled="isLoading"
                color="primary" class="mt-2"
                @click="saveConfig">Save</btn-component>
            <FeedbackComponent ref="saveConfigFeedback" />
        </BlockComponent>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { FetchStatus } from '@services/abstractions/TKServiceBase';
import { StoreUtil } from "@util/StoreUtil";
import IPWhitelistService from "@services/IPWhitelistService";
import IdUtils from "@util/IdUtils";
import ModuleConfig from "@models/Common/ModuleConfig";
import { TKIPWhitelistConfig } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistConfig";
import BlockComponent from "@components/Common/Basic/BlockComponent.vue";
import BtnComponent from "@components/Common/Basic/BtnComponent.vue";
import TextFieldComponent from "@components/Common/Basic/TextFieldComponent.vue";
import TextareaComponent from "@components/Common/Basic/TextareaComponent.vue";
import ValueUtils from "@util/ValueUtils";
import FeedbackComponent from "@components/Common/Basic/FeedbackComponent.vue";
import CheckboxComponent from "@components/Common/Basic/CheckboxComponent.vue";
import EditorComponent from "@components/Common/EditorComponent.vue";
import InputHeaderComponent from "@components/Common/Basic/InputHeaderComponent.vue";

@Options({
    components: {
        BlockComponent,
        BtnComponent,
        TextareaComponent,
        TextFieldComponent,
        EditorComponent,
        InputHeaderComponent,
        FeedbackComponent
    }
})
export default class IPWhitelistConfigComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: false, default: false })
    loading!: string | boolean;

    @Ref() readonly editor!: EditorComponent;
    @Ref() readonly saveConfigFeedback!: FeedbackComponent;

    // Service
    service: IPWhitelistService = new IPWhitelistService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    wlconfig: TKIPWhitelistConfig | null = null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.loadConfig();

        this.refreshEditorSize();
        this.$nextTick(() => this.refreshEditorSize());
        setTimeout(() => {
            this.refreshEditorSize();
        }, 100);
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadConfig(): void {
        this.service.GetConfig(this.dataLoadStatus, {
            onSuccess: (d) => this.wlconfig = d
        })
    }

    saveConfig(): void {
        this.saveConfigFeedback.show('Saving..');
        this.service.SaveConfig(this.wlconfig, this.dataLoadStatus, {
            onSuccess: d => {
                this.saveConfigFeedback.show('Saved');
            }
        });
    }

    refreshEditorSize(): void {
        if (this.editor)
        {
            this.editor.refreshSize();
        }
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get isLoading(): boolean {
        return this.dataLoadStatus.inProgress || ValueUtils.IsToggleTrue(this.loading);
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
.ip-whitelist-config {
    .editor {
        width: 100%;
        height: 400px;
    }
}
</style>
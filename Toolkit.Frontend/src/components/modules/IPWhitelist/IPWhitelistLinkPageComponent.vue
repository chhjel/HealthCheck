<!-- src/components/modules/IPWhitelist/IPWhitelistLinkPageComponent.vue -->
<template>
    <div class="ipwl-page">
        <div class="content-root">

            <p v-if="datax.note" class="note">{{ datax.note }}</p>
            
            <p>Add any number of addresses, one per line. Supports CIDR format.</p>
            <editor-component
                class="editor"
                :language="'text'"
                v-model:value="ipInput"
                :read-only="isLoading"
                ref="editor" />

            <btn-component color="primary" class="mt-3"
                @click.prevent="onAddIpClicked"
                :disabled="isLoading">
                <span style="white-space: normal;">Add IPs</span>
            </btn-component>
            
            <div>
                <FeedbackComponent ref="feedback" />
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import BlockComponent from '@components/Common/Basic/BlockComponent.vue';
import TKServiceBase, { FetchStatus } from "@services/abstractions/TKServiceBase";
import TextFieldComponent from "@components/Common/Basic/TextFieldComponent.vue";
import BtnComponent from "@components/Common/Basic/BtnComponent.vue";
import FeedbackComponent from "@components/Common/Basic/FeedbackComponent.vue";
import UrlUtils from "@util/UrlUtils";
import EditorComponent from "@components/Common/EditorComponent.vue";

interface ConfigFromWindow
{
    currentIp: string,
    ruleId: string,
    secret: string,
    note: string
}
interface AddIPResult {
    success: boolean;
    note: string;
}

@Options({
    components: {
        BlockComponent,
        TextFieldComponent,
        BtnComponent,
        FeedbackComponent,
        EditorComponent
    }
})
export default class IPWhitelistLinkPageComponent extends Vue {
    @Ref() readonly editor!: EditorComponent;
    @Ref() readonly feedback!: FeedbackComponent;

    loadStatus: FetchStatus = new FetchStatus();
    datax: ConfigFromWindow = {
        currentIp: '',
        ruleId: '',
        secret: '',
        note: ''
    };
    ipInput: string = '';

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.loadData();
        this.ipInput = this.datax.currentIp || '';
        
        this.refreshEditorSize();
        this.$nextTick(() => this.refreshEditorSize());
        setTimeout(() => {
            this.refreshEditorSize();
        }, 100);
    }

    created(): void {
    }

    beforeDestroy(): void {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get isLoading(): boolean { return false; }
    
    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.datax = (<any>window).__ipwl_data;
    }

    refreshEditorSize(): void {
        if (this.editor)
        {
            this.editor.refreshSize();
        }
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onAddIpClicked(): void {
        this.feedback.show('Adding..');

        if (this.loadStatus.inProgress) {
            return;
        }

        let url = UrlUtils.getRelativeToCurrent(`../IPWLLinkActivate/${this.datax.ruleId}_${this.datax.secret}`);
        let payload = {};
        const ips = this.ipInput.split('\n')
            .map(x => x.replace(/\r/g, '').trim())
            .filter(x => x.length > 0);
        if (ips.length == 0) {
            this.feedback.show('Enter some ips first');
            return;
        }

        let service = new TKServiceBase('', false);
        service.fetchExt<AddIPResult>(url, 'POST', payload, this.loadStatus,
            {
                onSuccess: (result) => {
                    if (result?.note) this.feedback.show(result.note, 4000);
                    else this.feedback.show('Something failed.');
                }
            },
            true,
            {
                'x-add-ip': ips.join('_')
            });
    }
}
</script>

<style scoped lang="scss">
.ipwl-page {
    height: 100%;
    text-align: center;
    padding: 5px 15px;
    margin: 0 auto;
    max-width: 1280px;
    width: calc(100% - 40px); // - padding (20+20)
    
    .content-root {
        max-width: 800px;
        margin: 0 auto;

        margin-top: 50px;
        border: 4px solid var(--color--primary-base);
        box-shadow: 0 0 16px 2px #919191;
    }

    .note {
        font-weight: 600;
    }

    .editor {
        width: 100%;
        height: 200px;
        text-align: left;
    }
}
</style>

<!-- src/components/modules/Messages/MessagesPageComponent.vue -->
<template>
    <div>
        <div>
            <!-- NAVIGATION DRAWER -->
            <Teleport to="#module-nav-menu">
                <filterable-list-component 
                    :items="menuItems"
                    :disabled="loadStatus.inProgress"
                    ref="filterableList"
                    :showFilter="false"
                    :groupByKey="`groupName`"
                    v-on:itemClicked="onMenuItemClicked"
                    @itemMiddleClicked="onMenuItemMiddleClicked"
                    />
            </Teleport>
            
            <div class="content-root">
                <!-- LOAD PROGRESS -->
                <progress-linear-component
                    v-if="loadStatus.inProgress"
                    indeterminate color="success"></progress-linear-component>

                <!-- DATA LOAD ERROR -->
                <alert-component :value="loadStatus.failed" v-if="loadStatus.failed" type="error">
                {{ loadStatus.errorMessage }}
                </alert-component>

                <div v-if="currentInbox != null">
                    <div style="display:flex">
                        <h1 class="mb-1">{{ currentInbox.Name }}</h1>
                            <btn-component color="error" flat
                                v-if="HasAccessToDeleteMessages"
                                :disabled="messageLoadStatus.inProgress"
                                @click="showDeleteInbox()">Delete all</btn-component>
                    </div>
                    <p v-if="currentInbox.Description != null && currentInbox.Description.length > 0">{{ currentInbox.Description }}</p>
                    <hr />

                    <paging-component
                        :count="totalMessageCount"
                        :pageSize="messagesPageSize"
                        v-model:value="messagesPageIndex"
                        :asIndex="true"
                        class="mb-2 mt-2"
                        />

                    <!-- MESSAGE LOAD PROGRESS -->
                    <progress-linear-component
                        v-if="messageLoadStatus.inProgress"
                        indeterminate color="success"></progress-linear-component>

                    <!-- MESSAGE DATA LOAD ERROR -->
                    <alert-component :value="messageLoadStatus.failed" v-if="messageLoadStatus.failed" type="error">
                    {{ messageLoadStatus.errorMessage }}
                    </alert-component>
                    
                    <div class="messages-list ml-1">
                        <div class="messages-list__item header">
                            <div class="messages-list__item__detail" style="display:none"></div>
                            <div class="messages-list__item__detail">From</div>
                            <div class="messages-list__item__detail">To</div>
                            <div class="messages-list__item__detail">Summary</div>
                            <div class="messages-list__item__detail">When</div>
                        </div>
                        <div v-for="(message, mindex) in messages"
                            :key="`message-${mindex}`"
                            class="messages-list__item"
                            @click="showMessage(message)">
                            <div class="messages-list__item__icon">
                                <icon-component :color="getMessageIconColor(message)" size="20px">{{ getMessageIcon(message) }}</icon-component>
                            </div>
                            <div class="messages-list__item__detail">
                                <b class="mobile-only">From: </b>
                                {{ message.From }}
                            </div>
                            <div class="messages-list__item__detail">
                                <b class="mobile-only">To: </b>
                                {{ message.To }}
                            </div>
                            <div class="messages-list__item__detail">
                                <b class="mobile-only">Summary: </b>
                                {{ message.Summary }}
                            </div>
                            <div class="messages-list__item__detail">
                                <b class="mobile-only">When: </b>
                                {{ formatDate(message.Timestamp) }}
                            </div>
                        </div>

                        <div v-if="messages.length == 0 && !messageLoadStatus.inProgress" class="mt-2">
                            No messages found.
                        </div>
                    </div>

                    <paging-component
                        :count="totalMessageCount"
                        :pageSize="messagesPageSize"
                        v-model:value="messagesPageIndex"
                        :asIndex="true"
                        class="mb-2 mt-2"
                        />
                </div>

            </div>
        </div>
            
        <dialog-component v-model:value="showMessageDialog" v-if="currentlyShownMessage != null" width="90vw" max-width="1200">
            <template #header>
                <div class="flex">
                    {{ currentlyShownMessage.Summary }}
                    <icon-component :color="getMessageIconColor(currentlyShownMessage)" class="ml-2">{{ getMessageIcon(currentlyShownMessage) }}</icon-component>
                </div>
            </template>

            <template #footer>
                <btn-component color="error"
                    v-if="HasAccessToDeleteMessages"
                    :disabled="messageLoadStatus.inProgress"
                    @click="showDeleteMessage(currentlyShownMessage)">Delete</btn-component>
                <btn-component color="secondary" @click="hideMessageDialog">Close</btn-component>
            </template>

            <div class="message">
                <div class="message__time"><b>When: </b>{{ formatDate(currentlyShownMessage.Timestamp, true) }}</div>
                <div class="message__from"><b>From: </b>{{ currentlyShownMessage.From }}</div>
                <div class="message__to"><b>To: </b>{{ currentlyShownMessage.To }}</div>
                <div class="message__summary"><b>Summary: </b>{{ currentlyShownMessage.Summary }}</div>
                <div v-if="currentlyShownMessage.Notes && currentlyShownMessage.Notes.length > 0">
                    <b>Notes:</b>
                    <ul>
                        <li class="message__note" v-for="(note, noteIndex) in currentlyShownMessage.Notes"
                            :key="`message-dialog-note-${noteIndex}`"
                            >{{ note }}</li>
                    </ul>
                </div>
                <div v-if="currentlyShownMessage.HasError" class="message__error-note">
                    Message contains an error, see below for details.
                </div>
                <block-component class="mt-2 mb-1">
                    <div class="message__body" v-if="currentlyShownMessage.BodyIsHtml && !showMessageBodyRaw">
                        <shadow-root>
                            <div v-html="currentlyShownMessage.Body"></div>
                        </shadow-root>
                    </div>
                    <editor-component
                        v-show="currentlyShownMessage.BodyIsHtml && showMessageBodyRaw"
                        class="editor"
                        :language="'xml'"
                        v-model:value="currentlyShownMessage.Body"
                        :read-only="true"
                        ref="editor"/>
                    <div class="message__body" v-if="!currentlyShownMessage.BodyIsHtml">{{ currentlyShownMessage.Body }}</div>
                    <div v-if="getAdditionalDetails(currentlyShownMessage).length > 0">
                        <code>{{ getAdditionalDetails(currentlyShownMessage) }}</code>
                    </div>
                </block-component>
                <a v-if="currentlyShownMessage.BodyIsHtml" @click="showMessageBodyRaw = !showMessageBodyRaw" class="ml-5 clickable">Toggle HTML</a>
                <block-component class="mt-2 mb-1" v-if="currentlyShownMessage.HasError" title="Error">
                    <code>{{ currentlyShownMessage.ErrorMessage }}</code>
                </block-component>
            </div>
        </dialog-component>
        
        <dialog-component v-model:value="deleteMessageDialogVisible" max-width="500">
            <template #header>Confirm deletion</template>
            <template #footer>
                <btn-component color="error" @click="deleteMessage()">Delete it</btn-component>
                <btn-component color="secondary" @click="deleteMessageDialogVisible = false">Cancel</btn-component>
            </template>
            <div>
                Are you sure you want to delete this message?
            </div>
        </dialog-component>
        
        <dialog-component v-model:value="deleteInboxDialogVisible" max-width="500">
            <template #header>Confirm deletion</template>
            <template #footer>
                <btn-component color="error" @click="deleteInbox()">Delete whole inbox</btn-component>
                <btn-component color="secondary" @click="deleteInboxDialogVisible = false">Cancel</btn-component>
            </template>
            <div>
                Are you sure you want to delete all messages in the inbox?
            </div>
        </dialog-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import DateUtils from '@util/DateUtils';
import LinqUtils from '@util/LinqUtils';
import KeyArray from '@util/models/KeyArray';
import KeyValuePair from '@models/Common/KeyValuePair';
// @ts-ignore
import IdUtils from '@util/IdUtils';
import BlockComponent from '@components/Common/Basic/BlockComponent.vue';
import { FetchStatus } from '@services/abstractions/HCServiceBase';
import MessagesService from '@services/MessagesService';
import ModuleOptions from '@models/Common/ModuleOptions';
import ModuleConfig from '@models/Common/ModuleConfig';
import { DataWithTotalCount, MessageItem, MessagesInboxMetadata } from '@models/modules/Messages/MessagesModels';
import { FilterableListItem } from '@components/Common/FilterableListComponent.vue.models';
import FilterableListComponent from '@components/Common/FilterableListComponent.vue';
import PagingComponent from '@components/Common/Basic/PagingComponent.vue';
import EditorComponent from '@components/Common/EditorComponent.vue';

import { ModuleFrontendOptions } from '@components/modules/EndpointControl/EndpointControlPageComponent.vue.models';
import { StoreUtil } from "@util/StoreUtil";
import StringUtils from "@util/StringUtils";
import UrlUtils from "@util/UrlUtils";
import { ModuleSpecificConfig } from "@components/HealthCheckPageComponent.vue.models";
@Options({
    components: {
        BlockComponent,
        PagingComponent,
        FilterableListComponent,
        EditorComponent
    }
})
export default class MessagesPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<ModuleFrontendOptions>;

    inboxes: Array<MessagesInboxMetadata> = [];
    currentInbox: MessagesInboxMetadata | null = null;
    messages: Array<MessageItem> = [];
    totalMessageCount: number = 0;
    messagesPageSize: number = 30;
    messagesPageIndex: number = 0;

    service: MessagesService = new MessagesService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);

    // UI STATE
    loadStatus: FetchStatus = new FetchStatus();
    messageLoadStatus: FetchStatus = new FetchStatus();
    showMessageDialog: boolean = false;
    showMessageBodyRaw: boolean = false;
    currentlyShownMessage: MessageItem | null = null;
    deleteMessageDialogVisible: boolean = false;
    currentlyDeletingMessage: MessageItem | null = null;
    deleteInboxDialogVisible: boolean = false;
    currentlyDeletingInbox: MessagesInboxMetadata | null = null;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        StoreUtil.store.commit('showMenuButton', true);
        this.loadInboxes();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }
    get HasAccessToDeleteMessages(): boolean {
        return this.options.AccessOptions.indexOf("DeleteMessages") != -1;
    }
    
    get menuItems(): Array<FilterableListItem>
    {
        return this.inboxes
            .map(x => {
                return {
                    title: x.Name,
                    subtitle: x.Description,
                    data: {
                        id: x.Id,
                        groupName: 'Inboxes'
                    }
                }
            });
    }

    get menu(): FilterableListComponent {
        return this.$refs.filterableList as FilterableListComponent;
    }

    ////////////////
    //  METHODS  //
    //////////////
    // public moduleSpecificConfig(): ModuleSpecificConfig {
    //     return {
    //         contentStyle: (s) => {
    //             // s['padding-left'] = '10px'
    //         }
    //     };
    // }
    
    updateUrl(): void {
        let routeParams: any = {};

        let id = '';
        if (this.currentInbox != null)
        {
            id = this.currentInbox.Id;
        }
        if (this.currentlyShownMessage != null)
        {
            id += "--" + this.currentlyShownMessage.Id;
        }

        routeParams['id'] = id;

        this.$router.push({ name: this.config.Id, params: routeParams })
    }

    updateSelectionFromUrl(): void {
        const idFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.id) || null;
        
        if (idFromHash) {
            let inboxId: string | null = null;
            let messageId: string | null = null;

            const splitIndex = idFromHash.indexOf('--');
            if (splitIndex == -1)
            {
                inboxId = idFromHash;
            }
            else
            {
                inboxId = idFromHash.substring(0, splitIndex);
                messageId = idFromHash.substring(splitIndex + 2);
            }

            this.setSelectedInbox(inboxId, false);
            
            if (messageId)
            {
                this.service.GetMessage(inboxId, messageId, null, {
                    onSuccess: (msg) => {
                        if (msg) {
                            msg.Timestamp = new Date(msg.Timestamp);
                            this.showMessage(msg, false);
                        }
                    }
                });
            }
        }
        else if(this.inboxes.length > 0)
        {
            this.setSelectedInbox(this.inboxes[0].Id);
        }
    }

    loadInboxes(): void {
        this.service.GetInboxMetadata(this.loadStatus, { onSuccess: (data) => this.onInboxMetadataRetrieved(data) });
    }

    onInboxMetadataRetrieved(data: Array<MessagesInboxMetadata>): void {
        this.inboxes = data;
        this.updateSelectionFromUrl();
    }

    refreshMessages(): void {
        if (this.currentInbox == null)
        {
            return;
        }
        
        this.service.GetLatestInboxMessages(
            this.currentInbox.Id,
            this.messagesPageSize, this.messagesPageIndex,
            this.messageLoadStatus, { onSuccess: (data) => this.onMessagesRetrieved(data) });
    }

    onMessagesRetrieved(data: DataWithTotalCount<Array<MessageItem>>): void {
        this.messages = data.Data.map(x => {
            x.Timestamp = new Date(x.Timestamp);
            return x;
        });
        this.totalMessageCount = data.TotalCount;
    }

    setSelectedInbox(inboxId: string | null, updateUrl: boolean = true): void {
        this.currentInbox = this.inboxes.find(x => x.Id == inboxId) ?? null;
        this.setSelectedMenuItem(inboxId);
        this.messagesPageIndex = 0;

        if (updateUrl)
        {
            this.updateUrl();
        }

        if (this.currentInbox != null)
        {
            this.refreshMessages();
        }
    }

    setSelectedMenuItem(inboxId: string | null): void {
        this.$nextTick(() => this.menu.setSelectedItemByFilter(x => {
            return x.data.id == inboxId;
        }));
    }

    formatDate(date: Date, both: boolean = false): string
    {
        const millisecondsAgo = (new Date().getTime() - date.getTime());
        let prefix = '';
        if (millisecondsAgo <= 1000 * 60 * 60 * 24)
        {
            const pretty = DateUtils.prettifyDurationString(millisecondsAgo, ' ago');
            if (!both)
            {
                return pretty;
            }
            prefix = `${pretty} at `;
        }
        return prefix + DateUtils.FormatDate(date, 'MMM d HH:mm:ss');
    }

    getMessageIcon(message: MessageItem): string {
        return (message != null && message.HasError)
            ? 'pest_control'
            : 'mark_email_read';
    }

    getMessageIconColor(message: MessageItem): string {
        return (message != null && message.HasError)
            ? '--color--error-lighten1'
            : '';
    }

    showMessage(message: MessageItem, updateUrl: boolean = true): void {
        if (this.currentlyShownMessage == message) {
            return;
        }

        this.currentlyShownMessage = message;
        this.showMessageDialog = true;
        this.showMessageBodyRaw = false;

        if (updateUrl)
        {
            this.updateUrl();
        }
    }

    hideMessageDialog(): void {
        this.showMessageDialog = false;
    }

    @Watch("showMessageDialog")
    onShowMessageDialogChanged(): void {
        if (!this.showMessageDialog) {
            this.currentlyShownMessage = null;
            this.updateUrl();
        }
    }

    getAdditionalDetails(message: MessageItem): string {
        const json = JSON.stringify(message.AdditionalDetails)
        if (json == "{}") {
            return '';
        }
        return json;
    }

    refreshEditorSize(): void {
        const editor: EditorComponent = <EditorComponent>this.$refs.editor;
        if (editor)
        {
            editor.refreshSize();
        }
    }

    showDeleteMessage(message: MessageItem): void {
        this.currentlyDeletingMessage = message;
        this.deleteMessageDialogVisible = true;
    }

    deleteMessage(): void {
        if (this.currentlyDeletingMessage != null && this.currentInbox != null)
        {
            this.service.DeleteMessage(this.currentInbox.Id, this.currentlyDeletingMessage.Id);

            const index = this.messages.findIndex(x => x.Id == this.currentlyDeletingMessage?.Id);
            this.messages.splice(index, 1);
        }
        this.currentlyDeletingMessage = null;
        this.deleteMessageDialogVisible = false;
        this.hideMessageDialog();
    }

    showDeleteInbox(): void {
        this.currentlyDeletingInbox = this.currentInbox;
        this.deleteInboxDialogVisible = true;
    }

    deleteInbox(): void {
        if (this.currentlyDeletingInbox != null)
        {
            this.service.DeleteInbox(this.currentlyDeletingInbox.Id);
            this.messages.splice(0, this.messages.length);
        }
        this.totalMessageCount = 0;
        this.messagesPageIndex = 0;
        this.currentlyDeletingInbox = null;
        this.deleteInboxDialogVisible = false;
        this.hideMessageDialog();
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onMenuItemClicked(item: FilterableListItem): void {
        const inboxId = item.data.id;
        this.setSelectedInbox(inboxId);
    }

    onMenuItemMiddleClicked(item: FilterableListItem): void {
        const inboxId = item?.data?.id;
        if (inboxId)
        {
            const idPart = UrlUtils.EncodeHashPart(inboxId);
            const route = `#/messages/${idPart}`;
            UrlUtils.openRouteInNewTab(route);
        }
    }

    @Watch("messagesPageIndex")
    onMessagesPageIndexChanged(): void
    {
        this.refreshMessages();
    }

    @Watch("showMessageBodyRaw")
    onShowMessageBodyRawChanged(): void {
        this.refreshEditorSize();
        this.$nextTick(() => this.refreshEditorSize());
        setTimeout(() => {
            this.refreshEditorSize();
        }, 100);
    }
}
</script>

<style scoped lang="scss">
.messages-list {
    &__item {
        position: relative;
        padding: 5px;
        display: flex;
        flex-wrap: nowrap;
        border-bottom: 1px solid #d2d2d2;
        transition: all 0.1s ease-in-out;
        
        &__icon {
            position: absolute;
            left: -20px;
            top: 4px;
        }

        &__detail {
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            width: 25%;
            padding-right: 10px;
        }
        &__detail:nth-child(2) { width: 20%; }
        &__detail:nth-child(3) { width: 20%; }
        &__detail:nth-child(4) { width: 40%; }
        &__detail:nth-child(5) { width: 20%; }

        @media (max-width: 960px) {
            flex-wrap: wrap;
            &__detail:nth-child(2),
            &__detail:nth-child(3),
            &__detail:nth-child(4),
            &__detail:nth-child(5) { width: 100%; }
        }

        &.header {
            font-weight: 600;
            @media (max-width: 960px) {
                display: none;
            }
        }

        &:not(.header) {
            cursor:pointer;
        }

        &:not(.header):hover {
            cursor:pointer;
            background-color: #d2d2d2;
        }
    }
}

.message {
    & > div {
        margin-bottom: 5px;
    }
    .editor {
        width: 100%;
        height: 300px;
    }
    &__error-note {
        color: var(--color--error-darken2);
        font-weight: 600;
    }
}

.message-dialog {
    &__title {
        margin-left: 10px;
    }
}

@media (min-width: 960px) {
    .mobile-only {
        display: none;
    }
}
@media (max-width: 960px) {
    .desktop-only {
        display: none;
    }
}
</style>
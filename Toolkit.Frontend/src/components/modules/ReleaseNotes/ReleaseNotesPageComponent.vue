<!-- src/components/modules/ReleaseNotes/ReleaseNotesPageComponent.vue -->
<template>
    <div>
        <div class="content-root">
            <!-- LOAD PROGRESS -->
            <progress-linear-component
                v-if="loadStatus.inProgress"
                indeterminate color="success"></progress-linear-component>

            <!-- DATA LOAD ERROR -->
            <alert-component :value="loadStatus.failed" v-if="loadStatus.failed" type="error">
            {{ loadStatus.errorMessage }}
            </alert-component>

            <div v-if="!datax && !loadStatus.inProgress">
                No release notes data was found.
            </div>
            <alert-component :value="datax && datax.ErrorMessage" v-if="datax && datax.ErrorMessage" type="error">
                {{ datax.ErrorMessage }}
            </alert-component>

            <div v-if="datax" class="release-notes">
                <h1 v-if="datax.Title" class="release-notes__title">{{ datax.Title }} (Version {{ datax.Version }})</h1>
                <p v-if="datax.Description" class="release-notes__description">{{ datax.Description }}</p>

                <div class="flex flex-wrap">
                    <chip-component v-if="datax.BuiltAt" class="release-notes__metadata mr-1 mt-1">Built at {{ formatDate(datax.BuiltAt) }}</chip-component>
                    <chip-component v-if="datax.BuiltCommitHash" class="release-notes__metadata mr-1 mt-1">Built hash: {{ datax.BuiltCommitHash }}</chip-component>
                    <chip-component v-if="datax.DeployedAt" class="release-notes__metadata mr-1 mt-1">Deployed at {{ formatDate(datax.DeployedAt) }}</chip-component>
                    <div class="spacer"></div>
                    <btn-component v-for="(link, lIndex) in topLinks"
                        :key="`toplink-${lIndex}-${id}`"
                        :href="link.Url">
                        {{ link.Title }}
                    </btn-component>
                </div>

                <div class="release-notes__changes" v-if="datax.Changes">
                    <h2 class="release-notes-changes__title">{{ changesTitle }}</h2>
                    <div v-for="(change, cindex) in datax.Changes"
                        :key="`change-${cindex}`"
                        class="release-notes-change"
                        :class="{ 'has-link': !!change.MainLink, 'has-pr': change.HasPullRequestLink, 'has-issue': change.HasIssueLink }"
                        :href="change.MainLink">
                        
                        <div class="release-notes-change__header">
                            <div>
                                <h3 class="release-notes-change__title">
                                    <icon-component class="release-notes-change__icon" v-if="change.Icon">{{ change.Icon }}</icon-component>
                                    {{ change.Title }}
                                </h3>
                                <div class="release-notes-change__description"
                                    v-if="change.Description">{{ change.Description }}</div>
                            </div>
                            <div>
                                <div class="release-notes-change__timestamp"
                                    v-if="change.Timestamp">{{ formatDate(change.Timestamp) }}</div>
                            </div>
                        </div>
                        
                        <div class="release-notes-change__links">
                            <div v-for="(link, lindex) in change.Links"
                                :key="`change-${cindex}-link-${lindex}`"
                                class="release-notes-change-link">
                                <icon-component v-if="link.Icon">{{ link.Icon }}</icon-component>
                                <a :href="link.Url">{{ link.Title }}</a>
                            </div>
                        </div>

                        <div class="release-notes-change__metadata">
                            <span v-if="change.CommitHash">Commit ({{ change.CommitHash }})</span>
                            <span v-if="change.AuthorName">by {{ change.AuthorName }}</span>
                        </div>
                    </div>
                </div>
            </div>

            <div v-if="HasAccessToDevDetails && !loadStatus.inProgress" class="mt-4">
                <btn-component flat @click.left="loadDataToggled()">{{ (forcedWithoutDevDetails) ? 'Show with dev details' : 'Show without dev details' }}</btn-component>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import DateUtils from '@util/DateUtils';
import BlockComponent from '@components/Common/Basic/BlockComponent.vue';
import { FetchStatus } from '@services/abstractions/TKServiceBase';
import ReleaseNotesService from '@services/ReleaseNotesService';
import ModuleOptions from '@models/Common/ModuleOptions';
import ModuleConfig from '@models/Common/ModuleConfig';
import { TKReleaseNotesViewModel } from "@generated/Models/Core/TKReleaseNotesViewModel";
import { TKReleaseNotesFrontendOptions } from "@generated/Models/Core/TKReleaseNotesFrontendOptions";

import { StoreUtil } from "@util/StoreUtil";
import { TKReleaseNoteLinkWithAccess } from "@generated/Models/Core/TKReleaseNoteLinkWithAccess";
import IdUtils from "@util/IdUtils";

@Options({
    components: {
        BlockComponent
    }
})
export default class ReleaseNotesPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<TKReleaseNotesFrontendOptions>;

    service: ReleaseNotesService = new ReleaseNotesService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    datax: TKReleaseNotesViewModel | null = null;
    forcedWithoutDevDetails: boolean = false;
    id: string = IdUtils.generateId();

    // UI STATE
    loadStatus: FetchStatus = new FetchStatus();

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.loadData();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }
    get HasAccessToDevDetails(): boolean {
        return this.options.AccessOptions.indexOf("DeveloperDetails") != -1;
    }
    get changesTitle(): string {
        if (!this.datax || !this.datax.Changes)
        {
            return 'Changes';
        }
        const changeCount = this.datax.Changes.length;
        const suffix = (changeCount === 1) ? '' : 's';
        return `${changeCount} change${suffix}`;
    }

    get topLinks(): TKReleaseNoteLinkWithAccess[] {
        return this.options?.Options?.TopLinks || [];
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.service.GetReleaseNotes(!this.forcedWithoutDevDetails && this.HasAccessToDevDetails, this.loadStatus, { onSuccess: (data) => this.onDataRetrieved(data) });
    }

    loadDataToggled(): void {
        this.forcedWithoutDevDetails = !this.forcedWithoutDevDetails;
        this.loadData();
    }

    onDataRetrieved(data: TKReleaseNotesViewModel | null): void {
        this.datax = data;
    }

    formatDate(date: Date): string {
        return DateUtils.FormatDate(date, "dddd dd. MMMM yyyy HH:mm:ss");
    }
}
</script>

<style scoped lang="scss">
.release-notes {
    &__title {
        
    }
    
    &__description {
        
    }

    &__changes {
        margin-top: 10px;
        margin-bottom: -10px;

        &__title {
            
        }
    }
}
.release-notes-change {
    margin: 10px 0;
    padding: 10px 10px;
    transition: all 0.1s ease-in-out;
    
    color: #666;
    /* border-radius: 5px; */
    border: 1px solid #ccc;
    border-left: 3px solid var(--color--success-base);
    text-decoration: none;

    &:not(.has-issue) {
        border-left: 3px solid #91a6e2;
    }

    /* &.has-link {
        &:hover {
            box-shadow: 1px 1px 5px 0px #5a5a5a54;
        }
    } */

    &__header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        flex-wrap: wrap;
    }

    &__title {
        display: flex;
        margin-bottom: 5px;
    }

    &__description {
        margin-bottom: 10px;
    }

    &__timestamp {
    }

    &__metadata {
    }

    &__links {
        display: flex;
        margin-top: 5px;
        margin-bottom: 5px;
    }
}
.release-notes-change-link {
    display: inline-flex;
    align-items: center;
    margin-right: 10px;
}
</style>
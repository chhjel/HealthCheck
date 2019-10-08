<!-- src/components/HealthCheckPageComponent.vue -->
<template>
    <div>
        <v-app light class="approot">
            <!-- TOOLBAR -->
            <v-toolbar clipped-left fixed app class="toolbar-main">
                <v-toolbar-side-icon
                    @click.stop="onSideMenuToggleButtonClicked"
                    v-if="showMenuButton"></v-toolbar-side-icon>
                <v-toolbar-title class="apptitle">
                    <a v-if="hasTitleLink" :href="titleLink">{{ options.ApplicationTitle }}</a>
                    <span v-else>{{ options.ApplicationTitle }}</span>
                </v-toolbar-title>
                <v-spacer></v-spacer>
                <v-toolbar-items v-if="showPagesMenu">
                    <v-btn flat
                        v-if="showPageMenu(PAGE_OVERVIEW)"
                        :href="`?page=${PAGE_OVERVIEW}`"
                        @click.left.prevent="setCurrentPage(PAGE_OVERVIEW)">Status</v-btn>
                    <v-btn flat 
                        v-if="showPageMenu(PAGE_TESTS)"
                        :href="`?page=${PAGE_TESTS}`"
                        @click.left.prevent="setCurrentPage(PAGE_TESTS)">Tests</v-btn>
                    <v-btn flat 
                        v-if="showPageMenu(PAGE_LOGS)"
                        :href="`?page=${PAGE_LOGS}`"
                        @click.left.prevent="setCurrentPage(PAGE_LOGS)">Logs</v-btn>
                    <v-btn flat 
                        v-if="showPageMenu(PAGE_AUDITLOG)"
                        :href="`?page=${PAGE_AUDITLOG}`"
                        @click.left.prevent="setCurrentPage(PAGE_AUDITLOG)">Audit log</v-btn>
                </v-toolbar-items>
            </v-toolbar>

            <!-- CONTENT -->
            <no-page-available-page-component
                v-if="shouldIncludePage(PAGE_NO_PAGES_AVAILABLE)"
                v-show="currentPage == PAGE_NO_PAGES_AVAILABLE"
                :options="options" />
            <overview-page-component
                v-if="shouldIncludePage(PAGE_OVERVIEW)"
                v-show="currentPage == PAGE_OVERVIEW"
                :options="options" />
            <test-suites-page-component 
                v-if="shouldIncludePage(PAGE_TESTS)"
                v-show="currentPage == PAGE_TESTS"
                ref="testsPage"
                :options="options" />
            <audit-log-page-component 
                v-if="shouldIncludePage(PAGE_AUDITLOG)"
                v-show="currentPage == PAGE_AUDITLOG"
                :options="options" />
            <log-viewer-page-component 
                v-if="shouldIncludePage(PAGE_LOGS)"
                v-show="currentPage == PAGE_LOGS"
                :options="options" />

            <!-- FOOTER -->
            <!-- <v-footer app fixed>
                <span>&copy; {{ new Date().getFullYear() }}</span>
            </v-footer> -->
        </v-app>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import NoPageAvailablePageComponent from './Pages/NoPageAvailablePageComponent.vue';
import TestSuitesPageComponent from './Pages/TestSuitesPageComponent.vue';
import OverviewPageComponent from './Pages/OverviewPageComponent.vue';
import AuditLogPageComponent from './Pages/AuditLogPageComponent.vue';
import LogViewerPageComponent from './Pages/LogViewerPageComponent.vue';
import FrontEndOptionsViewModel from '../models/Page/FrontEndOptionsViewModel';
import UrlUtils from '../util/UrlUtils';

@Component({
    components: {
        NoPageAvailablePageComponent,
        TestSuitesPageComponent,
        OverviewPageComponent,
        AuditLogPageComponent,
        LogViewerPageComponent
    }
})
export default class HealthCheckPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;
    
    showMenuButton: boolean = true;
    
    // Pages
    PAGE_OVERVIEW: string = "status";
    PAGE_TESTS: string = "tests";
    PAGE_LOGS: string = "logviewer";
    PAGE_AUDITLOG: string = "auditlog";
    PAGE_NO_PAGES_AVAILABLE: string = "no_page";
    currentPage: string = this.PAGE_TESTS;
    pagesWithMenu: string[] = [ this.PAGE_TESTS ];
    pagesShownAtLeastOnce: string[] = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.setInitialPage();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get showPagesMenu(): boolean {
        return this.options.Pages.length > 1;
    }

    get hasTitleLink(): boolean {
        let url = this.titleLink;
        return (url != null && url != undefined && url.trim().length > 0);
    }

    get titleLink(): string {
        return this.options.ApplicationTitleLink;
    }
    
    ////////////////
    //  METHODS  //
    //////////////
    setInitialPage(): void
    {
        // Attempt to get from query string first
        const pageFromHash = UrlUtils.GetHashPart(0);
        if (pageFromHash != null && this.options.Pages.indexOf(pageFromHash) != -1) {
            this.setCurrentPage(pageFromHash);
        } else if(this.options.Pages.length > 0) {
            this.setCurrentPage(this.options.Pages[0]);
        } else {
            this.setCurrentPage(this.currentPage);
        }
    }

    setCurrentPage(page: string) {
        // Only allow pages in option.Pages 
        if (this.options.Pages.indexOf(page) == -1) {
            page = (this.options.Pages.length > 0) ? this.options.Pages[0] : this.PAGE_NO_PAGES_AVAILABLE;
        }

        this.showMenuButton = this.pagesWithMenu.indexOf(page) != -1;
        this.currentPage = page;

        if (page !== this.PAGE_NO_PAGES_AVAILABLE) {
            if (page == this.PAGE_TESTS) {
                UrlUtils.SetHashPart(0, page);
                const testPage = (<TestSuitesPageComponent>this.$refs.testsPage);
                if (testPage != undefined) {
                    testPage.onPageShow();
                }
            } else {
                UrlUtils.SetHashParts([page]);
            }
        }

        if (this.pagesShownAtLeastOnce.indexOf(page) == -1) {
            this.pagesShownAtLeastOnce.push(page);
        }
    }

    showPageMenu(page: string): boolean {
        return this.options.Pages.indexOf(page) != -1;
    }

    shouldIncludePage(page: string): boolean {
        return this.pagesShownAtLeastOnce.indexOf(page) != -1;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onSideMenuToggleButtonClicked(): void {
        this.$emit("onSideMenuToggleButtonClicked");
    }
}
</script>

<style scoped>
.approot {
    background-color: #f4f4f4;
}
.apptitle a {
    color: inherit;
    text-decoration: inherit;
}
.toolbar-main {
    background-color: #fff;
    box-shadow: 0 2px 2px -1px rgba(0, 0, 0, 0.02), 0 3px 2px 0 rgba(0, 0, 0, 0.02), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
    z-index: 99;
}
.content-root {
    padding-right: 46px;
}
</style>
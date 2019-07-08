<!-- src/components/HealthCheckPageComponent.vue -->
<template>
    <div>
        <v-app light class="approot">
            <!-- TOOLBAR -->
            <v-toolbar clipped-left fixed app class="toolbar-main">
                <v-toolbar-side-icon
                    @click.stop="onSideMenuToggleButtonClicked"
                    v-if="showMenuButton"></v-toolbar-side-icon>
                <v-toolbar-title>{{ options.ApplicationTitle }}</v-toolbar-title>
                <v-spacer></v-spacer>
                <v-toolbar-items>
                    <v-btn flat @click="setCurrentPage(PAGE_OVERVIEW)">Overview</v-btn>
                    <v-btn flat @click="setCurrentPage(PAGE_TESTS)">Tests</v-btn>
                </v-toolbar-items>
            </v-toolbar>

            <!-- CONTENT -->
            <test-suites-page-component 
                v-if="shouldIncludePage(PAGE_TESTS)"
                v-show="currentPage == PAGE_TESTS"
                :options="options" />
            <overview-page-component
                v-if="shouldIncludePage(PAGE_OVERVIEW)"
                v-show="currentPage == PAGE_OVERVIEW"
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
import TestSuitesPageComponent from './Pages/TestSuitesPageComponent.vue';
import OverviewPageComponent from './Pages/OverviewPageComponent.vue';
import FrontEndOptionsViewModel from '../models/FrontEndOptionsViewModel';
import UrlUtils from '../util/UrlUtils';

@Component({
    components: {
        TestSuitesPageComponent,
        OverviewPageComponent
    }
})
export default class HealthCheckPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;
    
    showMenuButton: boolean = true;
    
    // Pages
    PAGE_OVERVIEW: string = "overview";
    PAGE_TESTS: string = "tests";
    currentPage: string = this.PAGE_OVERVIEW;
    pagesWithMenu: string[] = [ this.PAGE_TESTS ];
    validPages: string[] = [ this.PAGE_OVERVIEW, this.PAGE_TESTS ];
    pagesShownAtLeastOnce: string[] = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.setInitialCurrentPage();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    
    ////////////////
    //  METHODS  //
    //////////////
    setInitialCurrentPage(): void
    {
        // Attempt to get from query string first
        let pageFromQueryParam = UrlUtils.GetQueryStringParameter(this.urlParameterCurrentPage);
        if (pageFromQueryParam != null && this.validPages.indexOf(pageFromQueryParam) != -1) {
            this.setCurrentPage(pageFromQueryParam);
        } else {
            this.setCurrentPage(this.currentPage);
        }
    }

    urlParameterCurrentPage: string = "page";
    setCurrentPage(page: string) {
        this.showMenuButton = this.pagesWithMenu.indexOf(page) != -1;
        this.currentPage = page;
        UrlUtils.SetQueryStringParameter(this.urlParameterCurrentPage, page);

        if (this.pagesShownAtLeastOnce.indexOf(page) == -1) {
            this.pagesShownAtLeastOnce.push(page);
        }
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
.toolbar-main {
    background-color: #fff;
    box-shadow: 0 2px 2px -1px rgba(0, 0, 0, 0.02), 0 3px 2px 0 rgba(0, 0, 0, 0.02), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
    z-index: 99;
}
.content-root {
    padding-right: 46px;
}
</style>
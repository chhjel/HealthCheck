<!-- src/components/HealthCheckPageComponent.vue -->
<template>
    <div>
        <div v-if="!showIntegratedLogin">
            <!-- TOOLBAR -->
            <toolbar-component clipped-left>
                <btn-component icon
                    @click.stop="moduleNavMenuState = !moduleNavMenuState"
                    v-if="showModuleMenuButton">[Menu icon]</btn-component>
                <div class="apptitle">
                    <a v-if="hasTitleLink" :href="titleLink">{{ globalOptions.ApplicationTitle }}</a>
                    <span v-else>{{ globalOptions.ApplicationTitle }}</span>
                </div>
                <btn-component flat
                    v-for="(mconf, mindex) in this.moduleConfigsToShowInTopMenu"
                    :key="`module-menu-${mindex}`"
                    :href="getModuleLinkUrl(mconf)"
                    :class="{ 'active-tab': isModuleShowing(mconf) }"
                    @click.left.prevent="showModule(mconf)">{{ mconf.Name }}</btn-component>
                <btn-component flat 
                    v-if="showTokenKillswitch"
                    @click.left.prevent="tokenKillswitchDialogVisible = true">
                    <icon-component class="toolbar-icon mr-1">remove_circle</icon-component>
                    Token killswitch
                    </btn-component>
                <btn-component flat 
                    v-if="showIntegratedProfile"
                    @click.left.prevent="integratedProfileDialogVisible = true">
                    <icon-component class="toolbar-icon mr-1">person</icon-component>
                    Profile
                    </btn-component>
                <btn-component flat 
                    v-if="showLogoutLink"
                    @click.left.prevent="logoutRedirect">
                    <icon-component>logout</icon-component>
                    {{ logoutLinkTitle }}
                    </btn-component>
            </toolbar-component>

            <!-- CONTENT -->
            <invalid-module-configs-component
                v-if="invalidModuleConfigs.length > 0"
                :invalid-configs="invalidModuleConfigs" />

            <div class="module-root">
                <div id="module-nav-menu"
                    :class="{ 'open': isModuleNavOpen }"
                    ref="moduleNavMenu"></div>
                <div class="module-content" :class="{ 'has-menu': isModuleNavOpen }">
                    <router-view></router-view>
                </div>
            </div>

            <no-page-available-page-component
                v-if="noModuleAccess"
                v-show="noModuleAccess" />
            
            <health-check-profile-dialog-component v-if="showIntegratedProfile" v-model:value="integratedProfileDialogVisible" />
            <access-token-killswitch-dialog v-if="showTokenKillswitch" v-model:value="tokenKillswitchDialogVisible" />
        </div>
        
        <integrated-login-page-component v-if="showIntegratedLogin" />
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import NoPageAvailablePageComponent from '@components/NoPageAvailablePageComponent.vue';
import InvalidModuleConfigsComponent from '@components/InvalidModuleConfigsComponent.vue';
import IntegratedLoginPageComponent from '@components/modules/IntegratedLogin/IntegratedLoginPageComponent.vue';
import ModuleConfig from '@models/Common/ModuleConfig';
import BackendInputComponent from '@components/Common/Inputs/BackendInputs/BackendInputComponent.vue';
import HealthCheckProfileDialogComponent from '@components/profile/HealthCheckProfileDialogComponent.vue';
import AccessTokenKillswitchDialog from '@components/modules/AccessTokens/AccessTokenKillswitchDialog.vue';
import { HCFrontEndOptions } from "@generated/Models/WebUI/HCFrontEndOptions";
import { RouteLocationNormalized } from "vue-router";
import UrlUtils from "@util/UrlUtils";
import EventBus from "@util/EventBus";

@Options({
    components: {
        NoPageAvailablePageComponent,
        InvalidModuleConfigsComponent,
        IntegratedLoginPageComponent,
        BackendInputComponent,
        HealthCheckProfileDialogComponent,
        AccessTokenKillswitchDialog
    }
})
export default class HealthCheckPageComponent extends Vue {
    @Prop({ required: true })
    moduleConfig!: Array<ModuleConfig>;

    integratedProfileDialogVisible: boolean = false;
    tokenKillswitchDialogVisible: boolean = false;
    moduleNavMenuState: boolean = true;
    moduleNavMenu: Element | null = null;
    hackyTimer: number = 0;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.moduleNavMenu = (<Element>this.$refs.moduleNavMenu);
        await this.setInitialPage();
        this.bindRootEvents();
        this.$router.afterEach((t, f, err) => this.onRouteChanged(t, f));
        setInterval(() => this.hackyTimer++, 100);
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get showModuleMenuButton(): boolean {
        return this.hackyTimer > 0 && this.moduleNavMenu != null && this.moduleNavMenu.childNodes.length > 0;
    }

    get isModuleNavOpen(): boolean {
        return this.moduleNavMenuState && this.showModuleMenuButton;
    }

    get showTokenKillswitch(): boolean {
        return this.globalOptions.AllowAccessTokenKillswitch;
    }

    get showIntegratedProfile(): boolean {
        return this.globalOptions.IntegratedProfileConfig
            && !this.globalOptions.IntegratedProfileConfig.Hide;
    }

    get showIntegratedLogin(): boolean {
        return this.globalOptions.ShowIntegratedLogin;
    }

    get invalidModuleConfigs(): Array<ModuleConfig> {
        return this.moduleConfig.filter(x => !x.LoadedSuccessfully);
    }

    get validModuleConfigs(): Array<ModuleConfig> {
        return this.moduleConfig.filter(x => x.LoadedSuccessfully);
    }

    get moduleConfigsToShowInTopMenu(): Array<ModuleConfig> {
        const configs = this.validModuleConfigs;
        return configs.length > 1 ? configs : [];
    }

    get globalOptions(): HCFrontEndOptions {
        return this.$store.state.globalOptions;
    }

    get showLogoutLink(): boolean {
        return !!this.globalOptions.LogoutLinkUrl && this.globalOptions.LogoutLinkUrl.length > 0;
    }

    get logoutLinkTitle(): string {
        return this.globalOptions.LogoutLinkTitle;
    }

    get logoutLinkUrl(): string {
        return this.globalOptions.LogoutLinkUrl;
    }
        
    get noModuleAccess(): boolean {
        return this.moduleConfig.length == 0;
    }

    get showPagesMenu(): boolean {
        return this.validModuleConfigs.length > 1;
    }

    get hasTitleLink(): boolean {
        let url = this.titleLink;
        return (url != null && url != undefined && url.trim().length > 0);
    }

    get titleLink(): string {
        return this.globalOptions.ApplicationTitleLink;
    }
    
    ////////////////
    //  METHODS  //
    //////////////
    bindRootEvents(): void {
        document.addEventListener('keyup', this.onDocumentKeyDownOrDown);
        document.addEventListener('keydown', this.onDocumentKeyDownOrDown);
    }
    
    onDocumentKeyDownOrDown(e: KeyboardEvent): void {
        if (this.$store.state.input.ctrlIsHeldDown != e.ctrlKey)
        {
            this.$store.commit('setCtrlHeldDown', e.ctrlKey);
        }
        if (this.$store.state.input.altIsHeldDown != e.altKey)
        {
            this.$store.commit('setAltHeldDown', e.altKey);
        }
        if (this.$store.state.input.shiftIsHeldDown != e.shiftKey)
        {
            this.$store.commit('setShiftHeldDown', e.shiftKey);
        }
    }

    getModuleOptions(moduleId: string): any
    {
        var options = (<any>window).healthCheckModuleOptions;
        return options[moduleId];
    }

    showModule(module: ModuleConfig): void
    {
        if (this.isModuleShowing(module))
        {
            return;
        }

        this.lastAttemptedShownModule = module;
        if (!this.$store.state.ui.allowModuleSwitch)
        {
            this.$emit("onNotAllowedModuleSwitch");
            EventBus.notify("onNotAllowedModuleSwitch");
            return;
        }

        this.$store.commit('allowModuleSwitch', true);
        this.$store.commit('showMenuButton', false);
        this.$router.push(module.InitialRoute);
    }
    
    lastAttemptedShownModule: ModuleConfig | null = null;
    retryShowModule(): void {
        if (this.lastAttemptedShownModule != null)
        {
            this.showModule(this.lastAttemptedShownModule);
        }
    }

    isModuleShowing(module: ModuleConfig): boolean
    {
        return this.$route.matched.some(({ name }) => name === module.Id);
    }

    async setInitialPage()
    {
        // X:ToDo: set based on prioritized order if nothing active.
        let queryState = UrlUtils.GetQueryStringParameter('h');
        if (queryState && queryState != window.location.hash)
        {
            queryState = queryState.replace('#', '');
            this.$router.push(queryState);
        }
        
        await this.$router.isReady();

        let anyRouteActive = this.$route.matched.length > 0;
        if (!anyRouteActive && this.validModuleConfigs.length > 0)
        {
            this.showModule(this.validModuleConfigs[0]);
        }
    }

    logoutRedirect(): void {
        window.location.href = this.logoutLinkUrl;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onSideMenuToggleButtonClicked(): void {
        this.$store.commit('toggleMenuExpanded');
    }

    onRouteChanged(to: RouteLocationNormalized, from: RouteLocationNormalized): void {
        this.$nextTick(() => {
            this.$nextTick(() => {
                UrlUtils.updatePerstentQueryStringKey();
            })
        });
    }

    getModuleLinkUrl(mconf: ModuleConfig): string {
        let route = `#${mconf.InitialRoute}`;
        return UrlUtils.getOpenRouteInNewTabUrl(route);
    }
}
</script>

<style scoped lang="scss">
.active-tab {
    font-weight: 900;
}
.module-root {
    display: flex;
    flex-direction: row;
    flex-wrap: nowrap;

    #module-nav-menu {
        transition: 0.2s all;
        position: fixed;
        left: -300px;
        width: 300px;
        &:not(:empty) {
            &.open {
                left: 0;
            }
        }
    }

    .module-content {
        transition: 0.2s all;
        padding: 5px 20px;
        margin: 0 auto;
        max-width: 1280px;
        width: 100%;

        &.has-menu {
            padding-left: 300px;
        }
        @media (min-width: 960px) {
            // todo: menu on top instead of pushing content
        }
    }
}
</style>

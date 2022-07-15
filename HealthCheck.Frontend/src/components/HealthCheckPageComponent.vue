<!-- src/components/HealthCheckPageComponent.vue -->
<template>
    <div>
        <div v-if="!showIntegratedLogin">
            <!-- TOOLBAR -->
            <toolbar-component fixed class="box-shadow-small" :items="toolbarItems">
                <template #prefix>
                    <div class="toolbar-prefix">
                        <icon-component class="clickable toolbar-icon"
                            @click.stop="toggleNavMenu"
                            v-if="showModuleMenuButton">menu</icon-component>
                        <div class="toolbar-prefix_apptitle">
                            <a v-if="hasTitleLink" :href="titleLink">{{ globalOptions.ApplicationTitle }}</a>
                            <span v-else>{{ globalOptions.ApplicationTitle }}</span>
                        </div>
                    </div>
                </template>
            </toolbar-component>

            <invalid-module-configs-component
                v-if="invalidModuleConfigs.length > 0"
                :invalid-configs="invalidModuleConfigs" />

            <div class="module-root">
                <div id="module-nav-menu" class="toolbar-offset"
                    :class="{ 'open': isModuleNavOpen }"
                    ref="moduleNavMenu"></div>
                <div class="module-nav-menu__overlay"
                    :class="{ 'has-menu': isModuleNavOpen }"
                    @click.stop="hideNavMenu"></div>
                
                <div class="module-content" :class="moduleContentClasses" :style="moduleContentStyle">
                    <router-view ref="routerView"></router-view>
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
import { Vue, Prop, Watch } from "vue-property-decorator";
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
import EventBus, { CallbackUnregisterShortcut } from "@util/EventBus";
import { ModuleSpecificConfig } from "./HealthCheckPageComponent.vue.models";
import { ToolbarComponentMenuItem } from "./Common/Basic/ToolbarComponent.vue.models";

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
    theme: string = 'light';
    moduleSpecificConfig: ModuleSpecificConfig = {};

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        // // Don't open menu on page load for smaller devices
        // const isSmallMenuMode = window.matchMedia('(min-width: 961px)')
        // this.moduleNavMenuState = isSmallMenuMode.matches;

        this.theme = localStorage.getItem('theme') || 'light';
        this.onThemeChanged();

        this.moduleNavMenu = (<Element>this.$refs.moduleNavMenu);
        await this.setInitialPage();
        this.bindEventBusEvents();
        this.bindRootEvents();
        this.$router.afterEach((t, f, err) => this.onRouteChanged(t, f));
        setInterval(() => this.hackyTimer++, 100);

        this.onLoadOrRouteChanged();
    }

    beforeUnmount(): void {
        this.unbindEventBusEvents();
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

    get moduleContentClasses(): any {
        let classes: any =  {
            'has-menu': this.isModuleNavOpen,
            'full-width': this.moduleSpecificConfig?.fullWidth == true,
            'full-height': this.moduleSpecificConfig?.fullHeight == true,
            'dark': this.moduleSpecificConfig?.dark == true
        };
        return classes;
    }

    get moduleContentStyle(): any {
        let style: any =  {};
        if (this.moduleSpecificConfig?.contentStyle) {
            this.moduleSpecificConfig.contentStyle(style);
        }
        return style;
    }
    
    get toolbarItems(): Array<ToolbarComponentMenuItem> {
        let items: Array<ToolbarComponentMenuItem> = [];

        // Modules
        this.moduleConfigsToShowInTopMenu.forEach(mconf => {
            items.push({
                label: mconf.Name,
                active: this.isModuleShowing(mconf),
                data: mconf,
                onClick: () => this.showModule(mconf),
                href: this.getModuleLinkUrl(mconf)
            });
        });

        // Utils
        if (this.showTokenKillswitch) {
            items.push({
                label: 'Token killswitch',
                icon: 'remove_circle',
                active: false,
                data: null,
                onClick: () => this.tokenKillswitchDialogVisible = true,
            });
        }
        if (this.showIntegratedProfile) {
            items.push({
                label: 'Profile',
                icon: 'person',
                active: false,
                data: null,
                onClick: () => this.integratedProfileDialogVisible = true,
            });
        }
        if (this.showLogoutLink) {
            items.push({
                label: this.logoutLinkTitle,
                icon: 'logout',
                active: false,
                data: null,
                onClick: () => this.logoutRedirect(),
            });
        }
        return items;
    }

    ////////////////
    //  METHODS  //
    //////////////
    callbacks: Array<CallbackUnregisterShortcut> = [];
    bindEventBusEvents(): void {
        this.callbacks = [
            EventBus.on("FilterableList.itemClicked", this.onFilterableListItemClicked.bind(this))
        ];
    }
    unbindEventBusEvents(): void {
      this.callbacks.forEach(x => x.unregister());
    }

    onFilterableListItemClicked(): void {
        // Close sidemenu on item select if on mobile
        const isSmallMenuMode = window.matchMedia('(max-width: 960px)')
        console.log("isSmallMenuMode", isSmallMenuMode);
        if (isSmallMenuMode.matches && this.showModuleMenuButton) {
            this.moduleNavMenuState = false;
        }
    }

    bindRootEvents(): void {
        document.addEventListener('keyup', this.onDocumentKeyDownOrDown);
        document.addEventListener('keydown', this.onDocumentKeyDownOrDown);
        window.addEventListener('click', this.onWindowClick);
        window.addEventListener('scroll', this.onWindowScroll, true);
        window.addEventListener('resize', this.onWindowResize, true);
    }

    onWindowScroll(e: Event): void {
        EventBus.notify("onWindowScroll", e);
    }

    onWindowResize(e: UIEvent): void {
        EventBus.notify("onWindowResize", e);
    }

    onWindowClick(e: MouseEvent): void {
        EventBus.notify("onWindowClick", e);
    }
    
    onDocumentKeyDownOrDown(e: KeyboardEvent): void {
        if (e.key == 'Escape') {
            EventBus.notify("onEscapeClicked", e);
        }
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

    toggleThemes(): void {
        if (this.theme == 'light') this.theme = 'dark';
        else this.theme = 'light';
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

    hideNavMenu(): void {
        this.moduleNavMenuState = false;
    }
    toggleNavMenu(): void {
        this.moduleNavMenuState = !this.moduleNavMenuState;
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
                // Update querystring from hash
                UrlUtils.updatePerstentQueryStringKey();
            })

            this.onLoadOrRouteChanged();
        });
    }

    onLoadOrRouteChanged(): void {
        // Any of the page modules may have a method with the signature: public moduleSpecificConfig(): ModuleSpecificConfig
        const currentPageComponent = this.$route?.matched[0]?.instances?.default;
        const moduleSpecificConfigMethod = (<any>currentPageComponent)?.moduleSpecificConfig;
        let moduleSpecificConfig: ModuleSpecificConfig | null = null;
        if (moduleSpecificConfigMethod) {
            moduleSpecificConfig = moduleSpecificConfigMethod();
        }
        this.processModuleSpecificConfig(moduleSpecificConfig);
    }

    processModuleSpecificConfig(config: ModuleSpecificConfig | null): void {
        this.moduleSpecificConfig = config || {};
    }

    getModuleLinkUrl(mconf: ModuleConfig): string {
        let route = `#${mconf.InitialRoute}`;
        return UrlUtils.getOpenRouteInNewTabUrl(route);
    }

    @Watch('theme')
    onThemeChanged(): void {
        document.documentElement.setAttribute('theme', this.theme);
        localStorage.setItem('theme', this.theme);
    }
}
</script>

<style scoped lang="scss">
.module-root {
    display: flex;
    flex-direction: row;
    flex-wrap: nowrap;
	padding-top: 56px;
	@media (min-width: 960px) {
		padding-top: 64px;
	}

    #module-nav-menu {
        transition: 0.2s all;
        position: fixed;
        left: -300px;
        top: 0;
        width: 300px;
        box-shadow: 0 2px 2px -1px rgba(0, 0, 0, 0.02), 0 3px 2px 0 rgba(0, 0, 0, 0.02), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
        background-color: #292929;
        color: var(--color--text-light);
        overflow-y: auto;
        z-index: 10;
        height: calc(100% - 56px);
        box-shadow: 0 0 15px 10px #56595ab8;
        &:not(.open) {
            box-shadow: none;
        }
        &:not(:empty) {
            &.open {
                left: 0;
            }
        }
        @media (min-width: 960px) {
            box-shadow: none;
            height: calc(100% - 64px);
        }
    }
    .module-nav-menu__overlay {
        background-color: #303f4863;
        position: fixed;
        left: 0;
        top: 0;
        right: 0;
        bottom: 0;
        z-index: 9;
        &:not(.has-menu) {
            display: none;
        }
        @media (min-width: 961px) {
            display: none;
        }
    }

    .module-content {
        transition: 0.2s all;
        padding: 5px 20px;
        margin: 0 auto;
        max-width: 1280px;
        width: calc(100% - 40px); // - padding (20+20)

        &.dark {
            background: var(--color--background-dark);
        }
        &.has-menu {
            padding-left: 300px;
            width: calc(100% - 320px); // - padding (300+20)
        }
        &.full-width {
            max-width: calc(100% - 40px);
        }
        &.has-menu.full-width {
            max-width: calc(100% - 320px);
        }
        &.full-height {
            height: calc(100vh - 74px);
        }
        @media (max-width: 960px) {
            &.has-menu {
                padding-left: 20px;
                width: calc(100% - 40px); // - padding (20+20)
            }
            &.has-menu.full-width {
                max-width: calc(100% - 40px);
            }
            &.full-height {
                height: calc(100vh - 66px);
            }
        }
    }
}
</style>

<style scoped lang="scss">
.toolbar-prefix {
    display: flex;
    height: 100%;
    align-items: stretch;
    padding-left: 10px;
    &_apptitle {
        display: flex;
        align-items: center;
        font-size: 20px;
        font-weight: 500;
        letter-spacing: .02em;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        padding-left: 5px;
        padding-right: 10px;
        a {
            color: var(--color--text);
            text-decoration: inherit;
        }
    }
}
.toolbar-icon {
    align-self: center;
    border-radius: 50%;
    transition: 0.2s all;
    padding: 5px;
    &:hover {
        background-color: var(--color--accent-lighten1);
    }
}
</style>
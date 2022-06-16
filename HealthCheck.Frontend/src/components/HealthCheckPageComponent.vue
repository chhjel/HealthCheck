<!-- src/components/HealthCheckPageComponent.vue -->
<template>
    <div>
        <div v-if="!showIntegratedLogin">
            <!-- TOOLBAR -->
            <toolbar-component fixed class="box-shadow-small">
                <template #prefix>
                    <div class="toolbar-prefix">
                        <icon-component class="clickable toolbar-icon"
                            @click.stop="moduleNavMenuState = !moduleNavMenuState"
                            v-if="showModuleMenuButton">menu</icon-component>
                        <div class="toolbar-prefix_apptitle">
                            <a v-if="hasTitleLink" :href="titleLink">{{ globalOptions.ApplicationTitle }}</a>
                            <span v-else>{{ globalOptions.ApplicationTitle }}</span>
                        </div>
                    </div>
                </template>
                <btn-component flat
                    v-for="(mconf, mindex) in this.moduleConfigsToShowInTopMenu"
                    :key="`module-menu-${mindex}`"
                    :href="getModuleLinkUrl(mconf)"
                    :class="{ 'active-tab': isModuleShowing(mconf) }"
                    @click.left.prevent="showModule(mconf)">{{ mconf.Name }}</btn-component>
                <btn-component flat 
                    v-if="showTokenKillswitch"
                    @click.left.prevent="tokenKillswitchDialogVisible = true">
                    <icon-component class="mr-1">remove_circle</icon-component>
                    Token killswitch
                    </btn-component>
                <!-- <icon-component class="clickable toolbar-icon"
                    @click.stop="toggleThemes">dark_mode</icon-component> -->
                <btn-component flat 
                    v-if="showIntegratedProfile"
                    @click.left.prevent="integratedProfileDialogVisible = true">
                    <icon-component class="mr-1">person</icon-component>
                    Profile
                    </btn-component>
                <btn-component flat 
                    v-if="showLogoutLink"
                    @click.left.prevent="logoutRedirect">
                    <icon-component>logout</icon-component>
                    {{ logoutLinkTitle }}
                </btn-component>
            </toolbar-component>

            
            <invalid-module-configs-component
                v-if="invalidModuleConfigs.length > 0"
                :invalid-configs="invalidModuleConfigs" />

            <div class="module-root">
                <div id="module-nav-menu" class="toolbar-offset"
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
    theme: string = 'light';

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.theme = localStorage.getItem('theme') || 'light';
        this.onThemeChanged();

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
        window.addEventListener('click', this.onWindowClick);
    }

    onWindowClick(e: MouseEvent): void {
        EventBus.notify("onWindowClick", e);
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

    @Watch('theme')
    onThemeChanged(): void {
        document.documentElement.setAttribute('theme', this.theme);
        localStorage.setItem('theme', this.theme);
    }
}
</script>

<style scoped lang="scss">
.active-tab {
    font-weight: 900;
}
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
.module-root {
    display: flex;
    flex-direction: row;
    flex-wrap: nowrap;
	margin-top: 56px;
	@media (min-width: 960px) {
		margin-top: 64px;
	}

    #module-nav-menu {
        transition: 0.2s all;
        position: fixed;
        left: -300px;
        top: 0;
        width: 300px;
        height: 100%;
        box-shadow: 0 2px 2px -1px rgba(0, 0, 0, 0.02), 0 3px 2px 0 rgba(0, 0, 0, 0.02), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
        background-color: #292929;
        color: var(--color--text-light);
        overflow-y: auto;
        z-index: 10;
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

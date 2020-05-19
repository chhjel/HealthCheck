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
                    <a v-if="hasTitleLink" :href="titleLink">{{ globalOptions.ApplicationTitle }}</a>
                    <span v-else>{{ globalOptions.ApplicationTitle }}</span>
                </v-toolbar-title>
                <v-spacer></v-spacer>
                <v-toolbar-items v-if="showPagesMenu">
                    <v-btn flat
                        v-for="(mconf, mindex) in this.moduleConfig"
                        :key="`module-menu-${mindex}`"
                        :href="`#${mconf.InitialRoute}`"
                        :class="{ 'active-tab': isModuleShowing(mconf) }"
                        @click.left.prevent="showModule(mconf)">{{ mconf.Name }}</v-btn>
                </v-toolbar-items>
            </v-toolbar>

            <!-- CONTENT -->
            <router-view></router-view>

            <no-page-available-page-component
                v-if="noModuleAccess"
                v-show="noModuleAccess" />
        </v-app>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import NoPageAvailablePageComponent from './Pages/NoPageAvailablePageComponent.vue';
import FrontEndOptionsViewModel from '../models/Common/FrontEndOptionsViewModel';
import ModuleConfig from "../models/Common/ModuleConfig";

@Component({
    components: {
        NoPageAvailablePageComponent
    }
})
export default class HealthCheckPageComponent extends Vue {
    @Prop({ required: true })
    moduleConfig!: Array<ModuleConfig>;

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
    get globalOptions(): FrontEndOptionsViewModel {
        return this.$store.state.globalOptions;
    }
    
    get showMenuButton(): boolean {
        return this.$store.state.ui.menuButtonVisible;
    }
    
    get noModuleAccess(): boolean {
        return this.moduleConfig.length == 0;
    }

    get showPagesMenu(): boolean {
        return this.moduleConfig.length > 1;
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

        this.$store.commit('showMenuButton', false);
        this.$router.push(module.InitialRoute);
    }

    isModuleShowing(module: ModuleConfig): boolean
    {
        return this.$route.matched.some(({ name }) => name === module.Id);
    }

    setInitialPage(): void
    {
        // X:ToDo: set based on prioritized order if nothing active.
        
        let anyRouteActive = this.$route.matched.length > 0;
        if (!anyRouteActive && this.moduleConfig.length > 0)
        {
            this.showModule(this.moduleConfig[0]);
        }
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onSideMenuToggleButtonClicked(): void {
        this.$emit("onSideMenuToggleButtonClicked");
    }
}
</script>

<style scoped lang="scss">
.approot {
    background-color: #f4f4f4;
    /* background-color: #f7f6f4; */
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
.active-tab {
    font-weight: 900;
}
.application {
    font-family: 'Montserrat';
}
.v-toolbar__items {
    overflow-y: hidden;
    overflow-x: auto;
    overflow: overlay hidden;
    -ms-overflow-style: none;

    &::-webkit-scrollbar {
        display: none;
    }
}
</style>

<style lang="scss">
input[type=number] {
    -moz-appearance:textfield;
}
input[type=number]:hover,
input[type=number]:focus {
    -moz-appearance: number-input;
}
</style>
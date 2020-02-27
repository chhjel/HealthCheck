<!-- src/components/Settings/SettingInputComponent.vue -->
<template>
    <div>
        <div class="setting-header">
            <div class="setting-name">{{ setting.displayName }}</div>
            <v-icon small v-if="setting.description != null"
                color="gray" class="setting-help-icon"
                @click="toggleDescription">help</v-icon>
        </div>

        <div v-show="showDescription" class="setting-description" v-html="setting.description"></div>

        <component
            class="setting-input"
            :setting="setting"
            :is="getInputComponentNameFromType(setting.type)">
        </component>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import { CustomSetting } from "../Pages/SettingsPageComponent.vue";
// Input components
import UnknownSettingInputComponent from './InputTypes/UnknownSettingInputComponent.vue';
import SettingInputTypeBooleanComponent from './InputTypes/SettingInputTypeBooleanComponent.vue';
import SettingInputTypeInt32Component from './InputTypes/SettingInputTypeInt32Component.vue';
import SettingInputTypeStringComponent from './InputTypes/SettingInputTypeStringComponent.vue';

@Component({
    components: {
        // Parameter input components
        UnknownSettingInputComponent,
        SettingInputTypeBooleanComponent,
        SettingInputTypeInt32Component,
        SettingInputTypeStringComponent 
    }
})
export default class ParameterInputComponent extends Vue {
    @Prop({ required: true })
    setting!: CustomSetting;

    @Prop({ required: false })
    isListItem!: boolean;

    showDescription: boolean = false;

    mounted(): void {
    }

    getInputComponentNameFromType(typeName: string): string
    {
        typeName = typeName.replace('<', '').replace('>', '');
        let componentName = `SettingInputType${typeName}Component`;
        let componentExists = (this.$options!.components![componentName] != undefined);
        return componentExists
            ? componentName
            : "UnknownSettingInputComponent";
    }

    toggleDescription(): void {
        this.showDescription = !this.showDescription;
    }
}
</script>

<style scoped>
.setting-header {
    text-align: left;
}
.setting-name {
    display: inline-block;
    font-size: 16px;
    color: var(--v-secondary-base);
    font-weight: 600;
}
.setting-description {
    text-align: left;
    padding: 10px;
    border-radius: 10px;
    background-color: #ebf1fb;
}
.setting-help-icon {
    user-select: none;
    font-size: 20px !important;
}
.setting-help-icon:hover {
    color: #1976d2;
}
</style>

<style>
</style>
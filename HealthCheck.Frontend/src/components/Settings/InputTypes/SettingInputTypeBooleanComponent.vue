<!-- src/components/Settings/InputTypes/SettingInputTypeBooleanComponent.vue -->
<template>
    <div>
        <v-switch 
            v-model="value" 
            :label="label"
            v-on:change="onChanged"
            color="secondary"
            class="setting-checkbox pt-0 mt-2"
        ></v-switch>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import { CustomSetting } from "../../Pages/SettingsPageComponent.vue";

@Component({
    components: {
    }
})
export default class SettingInputTypeBooleanComponent extends Vue {
    @Prop({ required: true })
    setting!: CustomSetting;
    
    value: boolean = false;

    mounted(): void {
        this.value = (this.setting.value == true);
        this.onChanged();
    }

    get label(): string {
        return (this.setting.value == true) ? "Yes" : "No";
    }

    onChanged(): void {
        this.setting.value = this.value;
    }
}
</script>

<style>
.setting-checkbox label {
    color: #000 !important;
}
</style>

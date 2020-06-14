<!-- src/components/modules/Settings/InputTypes/SettingInputTypeBooleanComponent.vue -->
<template>
    <div>
        <v-switch 
            v-model="value" 
            :label="label"
            :disabled="disabled"
            v-on:change="onChanged"
            color="secondary"
            class="setting-checkbox pt-0 mt-2"
        ></v-switch>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import { CustomSetting }  from  '../../../../services/SettingsService';

@Component({
    components: {
    }
})
export default class SettingInputTypeBooleanComponent extends Vue {
    @Prop({ required: true })
    setting!: CustomSetting;

    @Prop({ required: false, default: false })
    disabled!: boolean;
    
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

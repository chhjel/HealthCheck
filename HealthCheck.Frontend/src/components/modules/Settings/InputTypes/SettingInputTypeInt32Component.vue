<!-- src/components/modules/Settings/InputTypes/SettingInputTypeInt32Component.vue -->
<template>
    <div>
        <v-text-field
            type="number"
            class="pt-0"
            v-model="setting.value"
            v-on:change="onValueChanged"
            :disabled="disabled"
            required />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import { CustomSetting }  from  '../../../../services/SettingsService';

@Component({
    components: {
    }
})
export default class SettingInputTypeInt32Component extends Vue {
    @Prop({ required: true })
    setting!: CustomSetting;

    @Prop({ required: false, default: false })
    disabled!: boolean;
    
    mounted(): void {
        if (this.setting.value == null || this.setting.value === '') {
            this.setting.value = 0;
        }
    }

    onValueChanged(): void {
        if (this.setting.value == null || this.setting.value === '') {
            this.setting.value = 0;
        }
        else {
            this.setting.value = Number(this.setting.value);
        }
    }
}
</script>

<style scoped>
</style>

<!-- src/components/modules/Settings/InputTypes/SettingInputTypeStringComponent.vue -->
<template>
    <div>
        <v-layout>
            <v-flex xs12>
                <v-text-field
                    class="pt-0"
                    v-model="setting.value"
                    v-on:change="onValueChanged"
                    :disabled="disabled"
                    required />
            </v-flex>
        </v-layout>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import { CustomSetting }  from  '../../../../services/SettingsService';

@Component({
    components: {
    }
})
export default class SettingInputTypeStringComponent extends Vue {
    @Prop({ required: true })
    setting!: CustomSetting;

    @Prop({ required: false })
    isListItem!: boolean;

    @Prop({ required: false, default: false })
    disabled!: boolean;
    
    mounted(): void {
        if (this.setting.value == null) {
            this.setting.value = "";
        }
    }

    onValueChanged(): void {
        if (this.setting.value == null) {
            this.setting.value = '0';
        }
    }
}
</script>

<style scoped>
</style>

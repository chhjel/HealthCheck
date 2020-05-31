<!-- src/components/modules/Settings/InputTypes/UnknownSettingInputComponent.vue -->
<template>
    <div style="color: red;">
        Unknown setting type '{{setting.type}}'. No input component created for this type yet.
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import { CustomSetting }  from  '../../../../services/SettingsService';

@Component({
    components: {
    }
})
export default class UnknownSettingInputComponent extends Vue {
    @Prop({ required: true })
    setting!: CustomSetting;

    mounted(): void {
    }
}
</script>

<style scoped>
</style>

<!-- src/components/Common/Inputs/BackendInputs/BackendInputComponent.vue -->
<template>
    <div>
        <div class="parameter-header" v-if="showInputHeader">
            <div class="parameter-name">{{ displayName }}</div>
            <v-icon small v-if="hasDescription"
                color="gray" class="parameter-help-icon"
                @click="toggleDescription">help</v-icon>
            <v-icon v-if="showActionIcon"
                color="gray" class="parameter-action-icon"
                @click="onActionIconClicked">{{ actionIcon }}</v-icon>
        </div>

        <div v-show="showDescription" class="parameter-description" v-html="displayDescription"></div>

        <component
            :key="`${id}-input`"
            class="parameter-input"
            :is="inputComponentName"
            v-model="localValue"
            :type="resolvedType"
            :name="displayName"
            :config="config"
            :listType="genericListInputType"
            :isListItem="isListItem"
            :readonly="readonly"
            :isCustomReferenceType="isCustomReferenceType"
            @isAnyJson="notifyIsAnyJson()"
            v-on:disableInputHeader="disableInputHeader">
        </component>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import { HCBackendInputConfig } from 'generated/Models/Core/HCBackendInputConfig';
// Parameter input components
import UnknownBackendInputComponent from './UnknownBackendInputComponent.vue';
import ParameterInputTypeInt32Component from './Types/ParameterInputTypeInt32Component.vue';
import ParameterInputTypeInt64Component from './Types/ParameterInputTypeInt64Component.vue';
import ParameterInputTypeDecimalComponent from './Types/ParameterInputTypeDecimalComponent.vue';
import ParameterInputTypeSingleComponent from './Types/ParameterInputTypeSingleComponent.vue';
import ParameterInputTypeDoubleComponent from './Types/ParameterInputTypeDoubleComponent.vue';
import ParameterInputTypeStringComponent from './Types/ParameterInputTypeStringComponent.vue';
import ParameterInputTypeBooleanComponent from './Types/ParameterInputTypeBooleanComponent.vue';
import ParameterInputTypeDateTimeComponent from './Types/ParameterInputTypeDateTimeComponent.vue';
import ParameterInputTypeDateTimeOffsetComponent from './Types/ParameterInputTypeDateTimeOffsetComponent.vue';
import ParameterInputTypeEnumComponent from './Types/ParameterInputTypeEnumComponent.vue';
import ParameterInputTypeFlaggedEnumComponent from './Types/ParameterInputTypeFlaggedEnumComponent.vue';
import ParameterInputTypeGenericListComponent from './Types/ParameterInputTypeGenericListComponent.vue';
import ParameterInputTypeHttpPostedFileBaseComponent from './Types/ParameterInputTypeHttpPostedFileBaseComponent.vue';
import ParameterInputPickReferenceComponent from './Types/ParameterInputPickReferenceComponent.vue';
import ParameterInputTypeGuidComponent from "./Types/ParameterInputTypeGuidComponent.vue";
import ParameterInputAnyJsonComponent from "./Types/ParameterInputAnyJsonComponent.vue";
import { TestModuleOptions } from "components/modules/TestSuite/TestSuitesPageComponent.vue";
import IdUtils from "../../../../util/IdUtils";

@Component({
    components: {
      // Parameter input components
      UnknownBackendInputComponent,
      ParameterInputTypeInt32Component,
      ParameterInputTypeInt64Component,
      ParameterInputTypeDecimalComponent,
      ParameterInputTypeSingleComponent,
      ParameterInputTypeDoubleComponent,
      ParameterInputTypeStringComponent,
      ParameterInputTypeBooleanComponent,
      ParameterInputTypeDateTimeComponent,
      ParameterInputTypeDateTimeOffsetComponent,
      ParameterInputTypeEnumComponent,
      ParameterInputTypeFlaggedEnumComponent,
      ParameterInputTypeGenericListComponent,
      ParameterInputTypeHttpPostedFileBaseComponent,
      ParameterInputPickReferenceComponent,
      ParameterInputTypeGuidComponent,
      ParameterInputAnyJsonComponent
    }
})
export default class BackendInputComponent extends Vue {
    @Prop({ required: true })
    config!: HCBackendInputConfig;

    @Prop({ required: true })
    value!: string;

    @Prop({ required: false, default: '' })
    forceType!: string;
    
    @Prop({ required: false, default: '' })
    forceName!: string;
    
    @Prop({ required: false, default: '' })
    forceDescription!: string;

    @Prop({ required: false, default: false })
    isListItem!: boolean;

    // @Prop({ required: false, default: true })
    // allowJsonInput!: boolean;

    @Prop({ required: false, default: false })
    readonly!: boolean;

    @Prop({ required: false, default: false })
    isCustomReferenceType!: boolean;
    
    @Prop({ required: false, default: '' })
    actionIcon!: string;

    showInputHeader: boolean = true;
    showDescription: boolean = false;
    localValue: string = "";
    id: string = IdUtils.generateId();

    mounted(): void {
        this.updateLocalValue();
        this.emitLocalValue();

        if (this.isListItem)
        {
            this.disableInputHeader();
        }
    }

    disableInputHeader(): void {
        this.showInputHeader = false;
    }

    toggleDescription(): void {
        this.showDescription = !this.showDescription;
    }

    notifyIsAnyJson(): void {
        this.$emit('isAnyJson');
    }
    
    ////////////////
    //  GETTERS  //
    //////////////
    get testsOptions(): TestModuleOptions {
        return this.$store.state.tests.options;
    }

    get allowJsonInput(): boolean {
        return this.testsOptions.AllowAnyParameterType;
    }

    get showActionIcon(): boolean {
        return !!this.actionIcon && this.actionIcon.length > 0;
    }

    get displayName(): string {
        if (this.forceName && this.forceName.length > 0)
        {
            return this.forceName;
        }
        else if (this.config.Name && this.config.Name.length > 0)
        {
            return this.config.Name;
        }
        return this.forceName;
    }

    get resolvedType(): string {
        if (this.forceType && this.forceType.length > 0)
        {
            return this.forceType;
        }
        else if (this.config.Type && this.config.Type.length > 0)
        {
            return this.config.Type;
        }
        return this.forceType;
    }

    get displayDescription(): string {
        if (this.forceDescription && this.forceDescription.length > 0)
        {
            return this.forceDescription;
        }
        else if (this.config.Description && this.config.Description.length > 0)
        {
            return this.config.Description;
        }
        return this.forceDescription;
    }

    get hasDescription(): boolean {
        return this.displayDescription != null && this.displayDescription.length > 0;
    }
    
    get inputComponentName(): string
    {
        let typeName = this.resolvedType;
        let genericType = this.genericListInputType;
        // const isList = genericType != null;
        // if (this.isCustomReferenceType && isList)
        // {
        //     return 'ParameterInputTypeGenericListComponent';
        // }
        if (genericType != null) {
            return "ParameterInputTypeGenericListComponent";
        }
        else if (this.isCustomReferenceType)
        {
            return 'ParameterInputPickReferenceComponent';
        }

        typeName = typeName.replace('<', '').replace('>', '');
        let componentName = `ParameterInputType${typeName}Component`;
        let componentExists = (this.$options!.components![componentName] != undefined);

        if (componentExists)
        {
            return componentName;
        }
        else if (this.allowJsonInput)
        {
            this.notifyIsAnyJson();
            return "ParameterInputAnyJsonComponent";
        }
        else {
            return "UnknownBackendInputComponent";
        }
    }

    get genericListInputType(): string | null
    {
        let genericListPattern = /^List<(\w+)>$/;
        if (genericListPattern.test(this.resolvedType)) {
            let match = this.resolvedType.match(genericListPattern);
            let itemType: string | null = (match == null) ? null : match[1];
            return itemType;
        }
        return null;
    }

    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
        this.localValue = this.value;
    }

    @Watch('localValue')
    emitLocalValue(): void
    {
        this.$emit('input', this.localValue);
    }

    onActionIconClicked(): void {
        this.$emit('actionIconClicked');
    }
}
</script>

<style scoped>
.parameter-header {
    text-align: left;
    display: flex;
    align-items: center;
}
.parameter-name {
    display: inline-block;
    font-size: 16px;
    color: var(--v-secondary-base);
    font-weight: 600;
}
.parameter-description {
    text-align: left;
    padding: 10px;
    border-radius: 10px;
    background-color: #ebf1fb;
}
.parameter-help-icon {
    user-select: none;
    font-size: 20px !important;
    margin-left: 4px;
}
.parameter-help-icon:hover {
    color: #1976d2;
}
.parameter-action-icon {
    user-select: none;
    font-size: 24px !important;
    margin-left: 4px;
}
.parameter-action-icon:hover {
    color: #1976d2;
}
</style>

<style>
.parameter-input input {
    font-size: 18px;
    color: #000 !important;
}
</style>
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
            :parameterDetailContext="parameterDetailContext"
            :referenceValueFactoryConfig="referenceValueFactoryConfig"
            @isAnyJson="notifyIsAnyJson()"
            v-on:disableInputHeader="disableInputHeader"
            ref="inputComp">
        </component>

        <div class="parameter-footer" v-if="hasFeedback">
            <div class="parameter-feedback">{{ feedback }}</div>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { HCBackendInputConfig } from '@generated/Models/Core/HCBackendInputConfig';
// Parameter input components
import UnknownBackendInputComponent from '@components/Common/Inputs/BackendInputs/UnknownBackendInputComponent.vue';
import ParameterInputTypeInt32Component from '@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeInt32Component.vue';
import ParameterInputTypeInt64Component from '@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeInt64Component.vue';
import ParameterInputTypeDecimalComponent from '@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeDecimalComponent.vue';
import ParameterInputTypeSingleComponent from '@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeSingleComponent.vue';
import ParameterInputTypeDoubleComponent from '@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeDoubleComponent.vue';
import ParameterInputTypeStringComponent from '@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeStringComponent.vue';
import ParameterInputTypeBooleanComponent from '@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeBooleanComponent.vue';
import ParameterInputTypeDateTimeComponent from '@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeDateTimeComponent.vue';
import ParameterInputTypeDateTimeOffsetComponent from '@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeDateTimeOffsetComponent.vue';
import ParameterInputTypeEnumComponent from '@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeEnumComponent.vue';
import ParameterInputTypeFlaggedEnumComponent from '@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeFlaggedEnumComponent.vue';
import ParameterInputTypeGenericListComponent from '@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeGenericListComponent.vue';
import ParameterInputTypeTimeSpanComponent from '@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeTimeSpanComponent.vue';
import ParameterInputTypeHttpPostedFileBaseComponent from '@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeHttpPostedFileBaseComponent.vue';
import ParameterInputPickReferenceComponent from '@components/Common/Inputs/BackendInputs/Types/ParameterInputPickReferenceComponent.vue';
import ParameterInputTypeGuidComponent from '@components/Common/Inputs/BackendInputs/Types/ParameterInputTypeGuidComponent.vue';
import ParameterInputAnyJsonComponent from '@components/Common/Inputs/BackendInputs/Types/ParameterInputAnyJsonComponent.vue';
import { TestModuleOptions } from "@components/modules/TestSuite/TestSuitesPageComponent.vue";
import IdUtils from '@util/IdUtils';
import { ReferenceValueFactoryConfigViewModel } from "@generated/Models/Core/ReferenceValueFactoryConfigViewModel";

@Options({
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
      ParameterInputTypeTimeSpanComponent,
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

    @Prop({ required: false, default: '' })
    parameterDetailContext!: string;

    @Prop({ required: false, default: null })
    referenceValueFactoryConfig!: ReferenceValueFactoryConfigViewModel | null;

    @Prop({ required: false, default: null })
    feedback!: string | null;

    showInputHeader: boolean = true;
    showDescription: boolean = false;
    localValue: string = "";
    id: string = IdUtils.generateId();

    mounted(): void {
        this.updateLocalValue();
        if (this.config && this.config.DefaultValue)
        {
            let def = this.config.DefaultValue;
            // Any of the subcomponents may have a method with the signature: public formatDefaultValue(val: string): string | null 
            const formatterMethod = (<any>this.inputComponent).formatDefaultValue;
            if (formatterMethod)
            {
                def = formatterMethod(def);
            }
            if (def != null)
            {
                this.localValue = def;
            }
        }
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
    get inputComponent(): Vue {
        return this.$refs.inputComp as Vue;
    }
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

    get hasFeedback(): boolean {
        return this.feedback != null && this.feedback.length > 0;
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
.parameter-feedback {
    font-size: small;
    font-weight: 600;
    color: var(--v-error-darken1);
}
</style>

<style lang="scss">
.parameter-input input {
    font-size: 18px;
    color: #000 !important;
}
.parameter-input {
    .v-text-field__slot {
        max-width: 100%;
    }
}
</style>
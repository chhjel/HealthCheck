<!-- src/components/Common/Inputs/BackendInputs/BackendInputComponent.vue -->
<template>
    <div>
        <div class="parameter-header" v-if="showInputHeader">
            <div class="parameter-name">{{ name }}</div>
            <v-icon small v-if="hasDescription"
                color="gray" class="parameter-help-icon"
                @click="toggleDescription">help</v-icon>
        </div>

        <div v-show="showDescription" class="parameter-description" v-html="description"></div>

        <component
            class="parameter-input"
            v-model="localValue"
            :type="type"
            :name="name"
            :config="config"
            :listType="genericListInputType"
            :isListItem="isListItem"
            :is="inputComponentName"
            v-on:disableInputHeader="disableInputHeader">
        </component>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import BackendInputConfig from './BackendInputConfig';
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
    type!: string;
    
    @Prop({ required: true })
    name!: string;

    @Prop({ required: true })
    value!: string;

    @Prop({ required: true })
    config!: BackendInputConfig;
    
    @Prop({ required: false, default: '' })
    description!: string;

    @Prop({ required: false, default: false })
    isListItem!: boolean;

    @Prop({ required: false, default: true })
    allowJsonInput!: boolean;

    @Prop({ required: false, default: false })
    isCustomReferenceType!: boolean;

    showInputHeader: boolean = true;
    showDescription: boolean = false;
    localValue: string = "";

    mounted(): void {
        this.updateLocalValue();
    }

    disableInputHeader(): void {
        this.showInputHeader = false;
    }

    toggleDescription(): void {
        this.showDescription = !this.showDescription;
    }
    
    ////////////////
    //  GETTERS  //
    //////////////
    get hasDescription(): boolean {
        return this.description != null && this.description.length > 0;
    }
    
    get inputComponentName(): string
    {
        if (this.isCustomReferenceType)
        {
            return 'ParameterInputPickReferenceComponent';
        }

        let typeName = this.type;
        let genericType = this.genericListInputType;
        if (genericType != null) {
            return "ParameterInputTypeGenericListComponent";
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
            return "ParameterInputAnyJsonComponent";
        }
        else {
            return "UnknownBackendInputComponent";
        }
    }

    get genericListInputType(): string | null
    {
        let genericListPattern = /^List<(\w+)>$/;
        if (genericListPattern.test(this.type)) {
            let match = this.type.match(genericListPattern);
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
}
</script>

<style scoped>
.parameter-header {
    text-align: left;
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
}
.parameter-help-icon:hover {
    color: #1976d2;
}
</style>

<style>
.parameter-input input {
    font-size: 18px;
    color: #000 !important;
}
</style>
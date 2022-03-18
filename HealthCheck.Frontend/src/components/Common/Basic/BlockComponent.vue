<!-- src/components/Common/Basic/BlockComponent.vue -->
<template>
    <div class="block-component">
        <!-- HEADER -->
        <div class="block-component--header" :class="{'no-details': headerOnly}">

            <div class="block-component--header-status-label subheading font-weight-bold"
                :class="statusClass"
                v-if="status.length > 0">{{status}}</div>
            
            <h4 class="block-component--header-title" v-if="title.length > 0">
            {{ title }}
            </h4>
            
            <div class="block-component--header-details" v-if="details.length > 0">{{ details }}</div>
            
            <btn-component 
                class="ma-0 pl-1 pr-3 block-component--header-button"
                v-if="buttonText.length > 0"
                ripple color="primary" 
                @click.stop.prevent="onHeaderButtonClicked"
                :disabled="buttonDisabled">
            <icon-component color="white" large v-if="buttonIcon.length > 0">{{ buttonIcon }}</icon-component>
            {{ buttonText }}
            </btn-component>
        </div>
        
        <!-- CONTENT -->
        <div v-if="!headerOnly"
            class="block-component--content"
            :class="{'no-header': title.length == 0}"
            >
            <slot/>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";

@Options({
    components: {}
})
export default class BlockComponent extends Vue
{
    @Prop({ required: false, default: '' })
    title!: string;
    
    @Prop({ required: false, default: '' })
    details!: string;
    
    @Prop({ required: false, default: false })
    headerOnly!: boolean;
    
    @Prop({ required: false, default: '' })
    buttonText!: string;
    
    @Prop({ required: false, default: '' })
    buttonIcon!: string;

    @Prop({ required: false, default: false })
    buttonDisabled!: boolean;
    
    @Prop({ required: false, default: '' })
    status!: string;
    
    @Prop({ required: false, default: 'success' })
    statusType!: 'success' | 'warning' | 'error';

    mounted(): void {
    }

    get statusClass(): string {
        return `label-${this.statusType}`; 
    }

    onHeaderButtonClicked(): void {
        this.$emit('header-button-clicked');
    }
}
</script>

<style scoped lang="scss">
.block-component {
    border-radius: 25px;
    background-color: #fff;
    box-shadow: #d5d7d5 4px 4px 6px 0px;
    padding: 24px;
    @media (max-width: 960px) {
        padding-left: 0;
        padding-right: 0;
        padding-top: 12px;
        padding-bottom: 12px;
    }

    .block-component--header {
        display: flex;
        padding-left: 24px;
        border-radius: 0 25px 0 0;
        @media (max-width: 960px) {
            padding-left: 12px;
        }

        &.no-details {
           border-radius: 0 25px 0 25px;
        }

        .block-component--header-title {
            flex-grow: 1;
            font-size: 22px;
            margin-top: 10px;
        }
        .block-component--header-details {
            padding-right: 10px;
            color: hsla(273, 40%, 80%, 1);
            display: flex;
            align-items: center;
        }

        .block-component--header-button {
            font-size: 20px;
            min-width: 120px;
            min-height: 53px;
            border-radius: 25px;
            text-transform: inherit;
        }

        .block-component--header-status-label {
            color: #fff;
            background-color: var(--v-success-base);
            height: 33px;
            padding: 8px;
            margin-right: 8px;
            padding-top: 5px;
            align-self: center;

            &.label-success {
                color: #317711;
                background-color: #c7e6c8;
            }
            &.label-warning {
                color: #df6d03;
                background-color: #f3d5b2;
            }
            &.label-error {
                color: #c20404;
                background-color: #eeb2b2;
            }
        }
    }

    .block-component--content {
        padding: 0px 48px 24px 24px;
        @media (max-width: 960px) {
            padding: 0px 12px 24px 12px;
        }

        &.no-header {
            padding-bottom: 0;
        }
    }
}
</style>
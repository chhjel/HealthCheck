<!-- src/components/Common/LoadingScreenComponent.vue -->
<template>
    <div class="loading-screen" 
        v-if="showLoadingScreen"
        v-bind:class="{ done: loadingIsDone }">
        <center class="spinner">
            <div class="loader" id="loader"></div>
            <div class="loader" id="loader2"></div>
            <div class="loader" id="loader3"></div>
            <div class="loader" id="loader4"></div>
            <span id="text">{{ text }}</span>
        </center>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
// or import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';
// if shipping only a subset of the features & languages is desired
import * as monaco from 'monaco-editor'
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { ICodeMark } from '@models/modules/DynamicCodeExecution/Models';


@Options({
    components: {
    }
})
export default class LoadingScreenComponent extends Vue {
    @Prop({ required: false, default: "LOADING" })
    text!: string;

    showLoadingScreen: boolean = true;
    loadingIsDone: boolean = false;
    
    public hide(): void {
        this.loadingIsDone = true;
        setTimeout(() => {
            this.showLoadingScreen = false;
        }, 1000);
    }
}
</script>

<style scoped lang="scss">
@keyframes fadeout {
    from { opacity: 1; }
    to   { opacity: 0; }
}
.loading-screen {
    position: absolute;
    left: 0;
    top: 64px;
    right: 0;
    bottom: 0;
    background-color: #1e1e1e;
    z-index: 999;
    overflow: hidden;
    &.done {
        animation: fadeout 1s;
    }
    /// Spinner
    .spinner {
        margin-top: 15%;
    }
    .loader{
        margin-bottom: 6px;
        border:3px solid #d6336c;
        width:200px;
        height:200px;
        border-radius:50%; 
        border-left-color: transparent;
    border-right-color: transparent;
        animation:rotate 2s cubic-bezier(0.26, 1.36, 0.74,-0.29) infinite;
    }
    #loader2{
        border:3px solid #3bc9db;
        width:220px;
        height:220px;
        position:relative;
        top:-216px;
        border-left-color: transparent;
    border-right-color: transparent;
        animation:rotate2 2s cubic-bezier(0.26, 1.36, 0.74,-0.29) infinite;
    }
    #loader3{
        border:3px solid #d6336c;
        width:240px;
        height:240px;
        position:relative;
        top:-452px;
        border-left-color: transparent;
    border-right-color: transparent;
        animation:rotate 2s cubic-bezier(0.26, 1.36, 0.74,-0.29) infinite;
    }
    #loader4{
        border:3px solid #3bc9db;
        width:260px;
        height:260px;
        position:relative;
        top:-708px;
        border-left-color: transparent;
    border-right-color: transparent;
        animation:rotate2 2s cubic-bezier(0.26, 1.36, 0.74,-0.29) infinite;
    }
    @keyframes rotate{
        0%{transform:rotateZ(-360deg)}
        100%{transform:rotateZ(0deg)}
    }
    @keyframes rotate2{
        0%{transform:rotateZ(360deg)}
        100%{transform:rotateZ(0deg)}
    }
    #text{
        color:white;
        font-family:Arial;
        font-size:20px;
        position:relative;
        top:-857px;
    }
}
</style>

<!-- src/components/Common/LoadingScreenComponent.vue -->
<template>
    <div class="loading-screen" 
        v-if="showLoadingScreen"
        v-bind:class="{ done: loadingIsDone }">
        <div class="spinner">
            <div class="loader loader-1"></div>
            <div class="loader loader-2"></div>
            <div class="loader loader-3"></div>
            <div class="loader loader-4"></div>
            <div class="loader-text">{{ text }}</div>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";

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
    from {
        opacity: 1;
    }
    to   {
        opacity: 0;
    }
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
    display: flex;
    justify-content: center;
    text-align: center;

    &.done {
        animation: fadeout 1s;
    }
    /// Spinner
    .spinner {
        margin-top: 15%;
        position: relative;
        display: flex;
        justify-content: center;
        align-items: center;
        height: 200px;
    }
    .loader-text {
        position: absolute;
        color: white;
        font-family: Arial;
        font-size: 20px;
        white-space: nowrap;
    }
    .loader {
        position: absolute;
        border: 3px solid #d6336c;
        width: 200px;
        height: 200px;
        border-radius: 50%; 
        border-left-color: transparent;
        border-right-color: transparent;
        animation: rotate 2s cubic-bezier(0.26, 1.36, 0.74,-0.29) infinite;
        box-sizing: border-box;
    }
    .loader-2 {
        border: 3px solid #3bc9db;
        width: 220px;
        height: 220px;
        border-left-color: transparent;
        border-right-color: transparent;
        animation: rotate2 2s cubic-bezier(0.26, 1.36, 0.74,-0.29) infinite;
    }
    .loader-3 {
        border: 3px solid #d6336c;
        width: 240px;
        height: 240px;
        border-left-color: transparent;
        border-right-color: transparent;
        animation: rotate 2s cubic-bezier(0.26, 1.36, 0.74,-0.29) infinite;
    }
    .loader-4 {
        border: 3px solid #3bc9db;
        width: 260px;
        height: 260px;
        border-left-color: transparent;
        border-right-color: transparent;
        animation: rotate2 2s cubic-bezier(0.26, 1.36, 0.74,-0.29) infinite;
    }
    @keyframes rotate{
        0%{transform: rotateZ(-360deg)}
        100%{transform: rotateZ(0deg)}
    }
    @keyframes rotate2{
        0%{transform: rotateZ(360deg)}
        100%{transform: rotateZ(0deg)}
    }
}
</style>

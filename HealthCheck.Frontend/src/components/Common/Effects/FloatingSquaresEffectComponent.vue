<!-- src/components/Common/Effects/FloatingSquaresEffectComponent.vue -->
<template>
    <div class="floating-squares-effect-component">
        <ul class="bg-bubbles">
            <li></li>
            <li></li>
            <li></li>
            <li></li>
            <li></li>
            <li></li>
            <li></li>
            <li></li>
            <li></li>
            <li></li>
        </ul>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";

@Component({
	components: {}
})
export default class FloatingSquaresEffectComponent extends Vue {}
</script>

<style scoped lang="scss">
.floating-squares-effect-component {
    width: 100%;
    height: 100%;
    position: fixed;
    background-color: #fff;

	.bg-bubbles {
		position: absolute;
		top: 0;
		left: 0;
		width: 100%;
		height: 100%;
		z-index: 1;
	}
	.bg-bubbles li {
		position: absolute;
		list-style: none;
		display: block;
		width: 40px;
		height: 40px;
		background-color: var(--v-primary-lighten5);
		bottom: -160px;
		-webkit-animation: square 70s infinite;
		animation: square 70s infinite;
		-webkit-transition-timing-function: linear;
		transition-timing-function: linear;
	}
	.bg-bubbles li:nth-child(1) {
		left: 10%;
	}
	.bg-bubbles li:nth-child(2) {
		left: 20%;
		width: 80px;
		height: 80px;
		-webkit-animation-delay: 2s;
		animation-delay: 2s;
		-webkit-animation-duration: 57s;
		animation-duration: 57s;
	}
	.bg-bubbles li:nth-child(3) {
		left: 25%;
		-webkit-animation-delay: 4s;
		animation-delay: 4s;
	}
	.bg-bubbles li:nth-child(4) {
		left: 40%;
		width: 60px;
		height: 60px;
		-webkit-animation-duration: 62s;
		animation-duration: 62s;
		background-color: var(--v-primary-lighten4);
	}
	.bg-bubbles li:nth-child(5) {
		left: 70%;
	}
	.bg-bubbles li:nth-child(6) {
		left: 80%;
		width: 120px;
		height: 120px;
		-webkit-animation-delay: 3s;
		animation-delay: 3s;
		background-color: var(--v-primary-lighten3);
	}
	.bg-bubbles li:nth-child(7) {
		left: 32%;
		width: 160px;
		height: 160px;
		-webkit-animation-delay: 7s;
		animation-delay: 7s;
	}
	.bg-bubbles li:nth-child(8) {
		left: 55%;
		width: 20px;
		height: 20px;
		-webkit-animation-delay: 15s;
		animation-delay: 15s;
		-webkit-animation-duration: 88s;
		animation-duration: 88s;
	}
	.bg-bubbles li:nth-child(9) {
		left: 25%;
		width: 10px;
		height: 10px;
		-webkit-animation-delay: 2s;
		animation-delay: 2s;
		-webkit-animation-duration: 90s;
		animation-duration: 90s;
		background-color: var(--v-primary-lighten4);
	}
	.bg-bubbles li:nth-child(10) {
		left: 90%;
		width: 160px;
		height: 160px;
		-webkit-animation-delay: 28s;
		animation-delay: 28s;
	}
	@-webkit-keyframes square {
		0% {
			-webkit-transform: translateY(0);
			transform: translateY(0);
		}
		100% {
			-webkit-transform: translateY(-1700px) rotate(600deg);
			transform: translateY(-1700px) rotate(600deg);
		}
	}
	@keyframes square {
		0% {
			-webkit-transform: translateY(0);
			transform: translateY(0);
		}
		100% {
			-webkit-transform: translateY(-1700px) rotate(600deg);
			transform: translateY(-1700px) rotate(600deg);
		}
	}
}
</style>

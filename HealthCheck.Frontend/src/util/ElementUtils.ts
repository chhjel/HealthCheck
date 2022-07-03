export default class ElementUtils
{
	static isScrolledIntoView(el: HTMLElement, allowPartiallyVisible: boolean = true): boolean {
		var rect = el.getBoundingClientRect();
		var elemTop = rect.top;
		var elemBottom = rect.bottom;

		if (allowPartiallyVisible) {
			// Partially visible elements return true:
			return elemTop < window.innerHeight && elemBottom >= 0;
		}
		else{ 
			// Only completely visible elements return true:
			return (elemTop >= 0) && (elemBottom <= window.innerHeight);
		}
	}
}

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

    static isChildOf(child: Node, parent: Node): boolean
    {
      let c: any = child;
      while ((c = c?.parentNode) && c !== parent);
      return !!c; 
    }

	static calcDropdownPosition(input: HTMLElement, dropdown: HTMLElement, yOffset: number = 0): { top: string, left: string } {
        // Calculate some values
        const bodyOffsetLeft = window.scrollX + document.body.getBoundingClientRect().left;
        const bodyOffsetTop = window.scrollY + document.body.getBoundingClientRect().top;
        const inputWrapperHeight = input.clientHeight;
        const inputWrapperRect = input.getBoundingClientRect();
        const inputWrapperTop = inputWrapperRect.top + window.scrollY - bodyOffsetTop;

        // Find top/left of dropdown
        let left: number = inputWrapperRect.left + window.scrollX - bodyOffsetLeft;

        // Check if dropdown should be above input
        let top: number = 0;
        const dropdownHeight = dropdown.clientHeight + 2 /* 2=border */;
        const dropdownBottomY = inputWrapperRect.top + inputWrapperHeight + dropdownHeight;
        let yExtraOffset = 4;
		if (yOffset) {
			yExtraOffset -= yOffset;
		}
        if (dropdownBottomY >= window.innerHeight) {
            top = inputWrapperTop - dropdownHeight + yExtraOffset;
        }
        else {
            top = inputWrapperTop + inputWrapperHeight - yExtraOffset;
        }

        // Return calculated values
		return {
			top: top + 'px',
			left: left + 'px'
		};
	}
}

export default class CssUtils
{
	static readonly PredefColorVariableNames: Array<string> = [
		'info', 'warning', 'error', 'success', 'primary', 'secondary', 'accent'
	];

	static translateColor(color: string): string
	{
		if (color == null || color.length == 0)
		{
			return null;
		}
		else if (CssUtils.isColorVariable(color))
		{
			return `var(${color})`;
		}
		else if (CssUtils.isColorVariableName(color))
		{
			return `var(--color--${color}-base)`;
		}
		return color;
	}

	static isColorVariableOrName(color: string): boolean {
		return CssUtils.isColorVariable(color)
			|| CssUtils.isColorVariableName(color);
	}

	static isColorVariable(color: string): boolean {
		if (color == null || color.length == 0) return false;
		else return color.includes('-');
	}

	static isColorVariableName(color: string): boolean {
		if (color == null || color.length == 0) return false;
		else return CssUtils.PredefColorVariableNames.includes(color);
	}
	
	static setColorClassIfPredefined(color: string, classes: any): void {
        if (CssUtils.isColorVariableName(color))
        {
            classes[color] = true;
        }
    }

	static setColorStyleIfNotPredefined(color: string, style: any): void {
        if (!CssUtils.isColorVariableName(color))
        {
            style['color'] = CssUtils.translateColor(color);
        }
    }
}

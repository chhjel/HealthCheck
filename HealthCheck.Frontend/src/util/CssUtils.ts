export default class CssUtils
{
	static translateColor(color: string): string
	{
		if (color == null || color.length == 0)
		{
			return null;
		}
		else if (color == 'info'
			|| color == 'warning'
			|| color == 'error'
			|| color == 'success')
		{
			return `var(--color--${color}-base)`;
		}
		return color;
	}
}

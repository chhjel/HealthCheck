export default class CssUtils
{
	static translateColor(color: string): string
	{
		if (color == null || color.length == 0)
		{
			return null;
		}
		else if (color.includes('-'))
		{
			return `var(${color})`;
		}
		else if (color == 'info'
			|| color == 'warning'
			|| color == 'error'
			|| color == 'success'
			|| color == 'primary'
			|| color == 'secondary'
			|| color == 'accent')
		{
			return `var(--color--${color}-base)`;
		}
		return color;
	}
}

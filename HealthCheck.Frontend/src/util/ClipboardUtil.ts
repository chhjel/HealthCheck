export default class ClipboardUtil
{
	static putDataOnClipboard(tempTextArea: HTMLTextAreaElement): string | null
	{
		let copySourceElement = tempTextArea;
		copySourceElement.setAttribute("style", "display:inherit;");
		copySourceElement.select();

		let error: string | null = null;
		try {
			let successful = document.execCommand("copy");
			if (!successful) {
				error = "Oops, unable to copy :(";
			}
		} catch (err) {
			error = "Oops, unable to copy :(";
		}

		copySourceElement.setAttribute("style", "display:none;");

		const selection = window.getSelection();
		if (selection != null) {
			selection.removeAllRanges();
		}
		return null;
	}
}

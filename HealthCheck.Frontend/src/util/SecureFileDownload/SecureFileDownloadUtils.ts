import { SecureFileDownloadDefinition } from "../../models/modules/SecureFileDownload/Models";

export default class SecureFileDownloadUtils
{
    static getAbsoluteDownloadUrl(urlSegmentText: string): string {
        let path = window.location.pathname;
        if (path == '/') path = '';
        else if (path.endsWith('/')) path = path.substr(0, path.length - 1);
        return `${window.location.origin.trim()}${path}/download/${urlSegmentText}`;
    }

    static postProcessDownload(x: SecureFileDownloadDefinition): void {
        if (x.LastDownloadedAt != null)
        {
            x.LastDownloadedAt = new Date(x.LastDownloadedAt);
        }
        if (x.LastModifiedAt != null)
        {
            x.LastModifiedAt = new Date(x.LastModifiedAt);
        }
        if (x.CreatedAt != null)
        {
            x.CreatedAt = new Date(x.CreatedAt);
        }
        if (x.ExpiresAt != null)
        {
            x.ExpiresAt = new Date(x.ExpiresAt);
        }
    }
}

import { SecureFileDownloadDefinition } from "../../models/modules/SecureFileDownload/Models";

export default class SecureFileDownloadUtils
{
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

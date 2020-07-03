export interface SecureFileDownloadDefinition {
    Id: string;
    CreatedAt: Date;
    CreatedByUsername: string;
    CreatedByUserId: string;
    LastModifiedAt: Date;
    LastModifiedByUsername: string | null;
    LastModifiedByUserId: string | null;
    FileName: string;
    StorageId: string;
    FileId: string;
    UrlSegmentText: string;
    Password: string | null;
    DownloadCount: number;
    LastDownloadedAt: Date | null;
    DownloadCountLimit: number | null;
    ExpiresAt: Date | null;
    IsExpired: boolean;
    Note: string | null;
}

export interface SecureFileDownloadsViewModel {
    Definitions: Array<SecureFileDownloadDefinition>;
}

export interface SecureFileDownloadSaveViewModel {
    Success: boolean;
    ErrorMessage: string | null;
    Definition: SecureFileDownloadDefinition;
}

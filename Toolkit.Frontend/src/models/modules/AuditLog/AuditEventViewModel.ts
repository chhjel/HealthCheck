import KeyValuePair from "../../Common/KeyValuePair";

export default interface AuditEventViewModel {
    Id: string;
    Timestamp: Date;
    Area: string;
    Action: string;
    Subject: string;
    Details: Array<KeyValuePair<string, string>>;
    Blobs: Array<KeyValuePair<string, string>>;
    UserId: string;
    UserName: string;
    UserAccessRoles: Array<string>;    
}

import { AuditEventArea } from "./AuditEventArea";

export default interface AuditEventViewModel {
    Timestamp: Date;
    Area: AuditEventArea;
    AreaCode: number;
    Title: string;
    Subject: string;
    Details: Array<any>;
    UserId: string;
    UserName: string;
    UserAccessRoles: Array<string>;    
}

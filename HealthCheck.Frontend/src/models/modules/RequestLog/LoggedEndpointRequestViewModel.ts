export default interface LoggedEndpointRequestViewModel
{
    Timestamp: Date;
    Version: string;
    StatusCode: string;
    ErrorDetails: string;
    Url: string;
    SourceIP: string;
}
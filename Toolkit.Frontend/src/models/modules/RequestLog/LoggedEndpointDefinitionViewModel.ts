import LoggedEndpointRequestViewModel from "./LoggedEndpointRequestViewModel";
import { EntryState } from "./EntryState";

export default interface LoggedEndpointDefinitionViewModel
{
    Id: string;
    EndpointId: string;
    Name: string;
    Description: string;
    Group: string;
    ControllerType: string;
    Controller: string;
    FullControllerName: string;
    Action: string;
    HttpVerb: string;
    Url: string;
    Calls: Array<LoggedEndpointRequestViewModel>;
    Errors: Array<LoggedEndpointRequestViewModel>;
    
    // Frontend only
    State: EntryState
}
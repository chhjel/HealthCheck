import LoggedActionCallEntryViewModel from "./LoggedActionCallEntryViewModel";
import { EntryState } from "./EntryState";

export default interface LoggedActionEntryViewModel
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
    Calls: Array<LoggedActionCallEntryViewModel>;
    Errors: Array<LoggedActionCallEntryViewModel>;
    
    // Frontend only
    State: EntryState
}
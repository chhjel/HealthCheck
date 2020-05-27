export default interface ModuleConfig {
    Id: string;
    Name: string;
    ComponentName: string;
    InitialRoute: string;
    RoutePath: string;
    LoadedSuccessfully: string;
    LoadErrors: string;
    LoadErrorStacktrace: string;
}
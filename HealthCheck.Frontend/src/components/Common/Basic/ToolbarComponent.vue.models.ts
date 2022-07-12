export interface ToolbarComponentMenuItem {
    label: string;
    active: boolean;
    href?: string;
    icon?: string;
    data: any;
    onClick?: (item: ToolbarComponentMenuItem) => void;
}

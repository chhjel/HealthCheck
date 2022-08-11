export interface ToolbarComponentMenuItem {
    label?: string;
    active?: boolean;
    href?: string;
    icon?: string;
    isSpacer?: boolean;
    data?: any;
    onClick?: (item: ToolbarComponentMenuItem) => void;
}

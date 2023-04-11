namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models;

/// <summary></summary>
public class TKDataRepeaterItemMergeConflictResult
{
    /// <summary>
    /// Action to perform on the old item.
    /// <para>Defaults to <see cref="OldItemActionType.Update"/></para>
    /// </summary>
    public OldItemActionType OldItemAction { get; set; } = OldItemActionType.Update;

    /// <summary>
    /// Action to perform on the new item.
    /// <para>Defaults to <see cref="NewItemActionType.Ignore"/></para>
    /// </summary>
    public NewItemActionType NewItemAction { get; set; }

    /// <summary>
    /// Action type to perform on the old item.
    /// </summary>
    public enum OldItemActionType
    {
        /// <summary>
        /// Update the item.
        /// </summary>
        Update,

        /// <summary>
        /// Remove the item.
        /// </summary>
        Delete
    }

    /// <summary>
    /// Action type to perform on the new item.
    /// </summary>
    public enum NewItemActionType
    {
        /// <summary>
        /// Keep the item and insert it.
        /// </summary>
        Insert,

        /// <summary>
        /// Don't store the item.
        /// </summary>
        Ignore
    }
}

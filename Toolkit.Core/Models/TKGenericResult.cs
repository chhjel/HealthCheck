using System;

namespace QoDL.Toolkit.Core.Models;

/// <summary></summary>
public class TKGenericResult
{
    /// <summary></summary>
    public bool Success { get; set; }

    /// <summary></summary>
    public bool HasError => !Success;

    /// <summary></summary>
    public string Error { get; set; }

    /// <summary></summary>
    public static TKGenericResult CreateSuccess() => new() { Success = true };

    /// <summary></summary>
    public static TKGenericResult CreateError(string error) => new() { Error = error };

    /// <summary></summary>
    public static TKGenericResult CreateError(Exception ex) => new() { Error = ex?.ToString() };
}

/// <summary></summary>
public class TKGenericResult<TData> : TKGenericResult
{
    /// <summary></summary>
    public TData Data { get; set; }

    /// <summary></summary>
    public static TKGenericResult<TData> CreateSuccess(TData data) => new() { Success = true, Data = data };

    /// <summary></summary>
    public static new TKGenericResult<TData> CreateError(string error) => new() { Error = error };

    /// <summary></summary>
    public static new TKGenericResult<TData> CreateError(Exception ex) => new() { Error = ex?.ToString() };
}

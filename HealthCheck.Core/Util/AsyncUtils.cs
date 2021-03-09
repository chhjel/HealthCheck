using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Async code related utilities.
    /// </summary>
    public static class AsyncUtils
    {
        #region Run async code sync
        // https://github.com/aspnet/AspNetIdentity/blob/master/src/Microsoft.AspNet.Identity.Core/AsyncHelper.cs
        private static readonly TaskFactory _myTaskFactory = new TaskFactory(CancellationToken.None,
            TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// Run async code synchronous with result.
        /// </summary>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;
            return _myTaskFactory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Run async code synchronous without any result.
        /// </summary>
        public static void RunSync(Func<Task> func)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;
            _myTaskFactory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }).Unwrap().GetAwaiter().GetResult();
        }
        #endregion

        #region Reflection
        /// <summary>
        /// Execute any async task to get the result.
        /// </summary>
        public static async Task<T> InvokeAsync<T>(MethodInfo method, object obj, params object[] parameters)
        {
            var result = await InvokeAsync(method, obj, parameters);
            return result is T value ? value : default;
        }

        /// <summary>
        /// Execute any async task to get the result.
        /// </summary>
        public static async Task<object> InvokeAsync(MethodInfo method, object obj, params object[] parameters)
        {
            var task = (Task)method.Invoke(obj, parameters);
            await task.ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty.GetValue(task);
        }
        /// <summary>
        /// Execute any async task synchronous to get the result.
        /// </summary>
        public static T InvokeAsyncSync<T>(MethodInfo method, object obj, params object[] parameters)
            => RunSync(() => InvokeAsync<T>(method, obj, parameters));

        /// <summary>
        /// Execute any async task synchronous to get the result.
        /// </summary>
        public static object InvokeAsyncSync(MethodInfo method, object obj, params object[] parameters)
            => RunSync(() => InvokeAsync(method, obj, parameters));
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hawkmoth.Webjobs
{
    /// <summary>
    /// defines the execution container for jobs
    /// </summary>
    public interface IJobHost : IDisposable
    {


        /// <summary>
        /// Calls a job method
        /// </summary>
        /// <param name="method">The method to call</param>
        void Call(MethodInfo method);

        /// <summary>
        /// Calls a job method
        /// </summary>
        /// <param name="method">The method to call</param>
        /// <param name="arguments">An object with public properties representing argument names and values to bind to parameters in the job method.</param>
        void Call(MethodInfo method, object arguments);

        /// <summary>
        /// Calls a job method
        /// </summary>
        /// <param name="method">The job method to call</param>
        /// <param name="arguments"> The argument names and values to bind to parameters in the job method</param>
        void Call(MethodInfo method, IDictionary<string, object> arguments);


        /// <summary>
        /// Calls a job method asyncronously
        /// </summary>
        /// <param name="method">The job method to call</param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> that will call the job method.</returns>
        Task CallAsync(MethodInfo method, CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Calls a job method asyncronously
        /// </summary>
        /// <param name="method">The job method to call</param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests.</param>
        /// <param name="arguments">An object with public properties representing argument names and values to bind to parameters in the job method.</param>
        /// <returns>A <see cref="Task"/> that will call the job method.</returns>
        Task CallAsync(MethodInfo method, object arguments, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Calls a job method asyncronously
        /// </summary>
        /// <param name="method">The job method to call</param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests.</param>
        /// <param name="arguments"> The argument names and values to bind to parameters in the job method</param>
        /// <returns>A <see cref="Task"/> that will call the job method.</returns>
        Task CallAsync(MethodInfo method, IDictionary<string, object> arguments, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Runs the host and blocks the current thread while the host remains running.
        /// </summary>
        void RunAndBlock();

        /// <summary>
        /// Starts the host
        /// </summary>
        void Start();

        /// <summary>
        /// Starts the host asyncronously
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A System.Threading.Tasks.Task that will start the host.</returns>
        Task StartAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Stops the host
        /// </summary>
        void Stop();

        /// <summary>
        /// Stops the host asyncronously
        /// </summary>
        /// <returns>A System.Threading.Tasks.Task that will stop the host.</returns>
        Task StopAsync();

    }
}

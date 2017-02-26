using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Hawkmoth.Webjobs
{
    public class AzureJobHost : IJobHost
    {
        private JobHost _jobHost;

        /// <summary>
        /// defult ctor
        /// </summary>
        public AzureJobHost()
        {
            _jobHost = new JobHost();
        }

        /// <summary>
        /// ctor initialises the Azure Job Host with <see cref="JobHostConfiguration"/>
        /// </summary>
        /// <param name="config"><see cref="JobHostConfiguration"/></param>
        public AzureJobHost(JobHostConfiguration config)
        {
            _jobHost = new JobHost(config);
        }

        /// <summary>
        /// <see cref="IJobHost.Call(MethodInfo)"/>
        /// </summary>
        public void Call(MethodInfo method)
        {
            _jobHost.Call(method);
        }

        /// <summary>
        /// <see cref="IJobHost.Call(MethodInfo, IDictionary{string, object})"/>
        /// </summary>
        public void Call(MethodInfo method, IDictionary<string, object> arguments)
        {
            _jobHost.Call(method, arguments);
        }

        /// <summary>
        /// <see cref="IJobHost.Call(MethodInfo, object)"/>
        /// </summary>
        public void Call(MethodInfo method, object arguments)
        {
            _jobHost.Call(method, arguments);
        }

        /// <summary>
        /// <see cref="IJobHost.CallAsync(MethodInfo, CancellationToken)"/>
        /// </summary>
        /// <returns></returns>
        public async Task CallAsync(MethodInfo method, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _jobHost.CallAsync(method, cancellationToken);
        }

        /// <summary>
        /// <see cref="IJobHost.CallAsync(MethodInfo, IDictionary{string, object}, CancellationToken)"/>
        /// </summary>
        /// <returns></returns>
        public async Task CallAsync(MethodInfo method, IDictionary<string, object> arguments, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _jobHost.CallAsync(method, arguments, cancellationToken);
        }

        /// <summary>
        /// <see cref="IJobHost.CallAsync(MethodInfo, object, CancellationToken)"/>
        /// </summary>
        public async Task CallAsync(MethodInfo method, object arguments, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _jobHost.CallAsync(method, arguments, cancellationToken);
        }

        /// <summary>
        /// <see cref="IJobHost.RunAndBlock"/>
        /// </summary>
        public void RunAndBlock()
        {
            _jobHost.RunAndBlock();
        }

        /// <summary>
        /// <see cref="IJobHost.Start"/>
        /// </summary>
        public void Start()
        {
            _jobHost.Start();
        }

        /// <summary>
        /// <see cref="IJobHost.StartAsync(CancellationToken)"/>
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _jobHost.StartAsync(cancellationToken);
        }

        /// <summary>
        /// <see cref="IJobHost.Stop"/>
        /// </summary>
        public void Stop()
        {
            _jobHost.Stop();
        }

        /// <summary>
        /// <see cref="IJobHost.StopAsync"/>
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            await _jobHost.StopAsync();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_jobHost != null)
                {
                    _jobHost.Dispose();
                    _jobHost = null;
                }
            }
        }
    }
}

﻿#if NET452
namespace Unosquare.Swan
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.ServiceProcess;

    partial class Extensions
    {
        
        /// <summary>
        /// Runs a service in console mode.
        /// </summary>
        /// <param name="serviceToRun">The service to run.</param>
        public static void RunInConsoleMode(this ServiceBase serviceToRun)
        {
            RunInConsoleMode(new ServiceBase[] { serviceToRun });
        }

        /// <summary>
        /// Runs a set of services in console mode.
        /// </summary>
        /// <param name="servicesToRun">The services to run.</param>
        public static void RunInConsoleMode(this ServiceBase[] servicesToRun)
        {
            const string OnStartMethodName = "OnStart";
            const string OnStopMethodName = "OnStop";

            var onStartMethod = typeof(ServiceBase).GetMethod(OnStartMethodName,
                    BindingFlags.Instance | BindingFlags.NonPublic);
            var onStopMethod = typeof(ServiceBase).GetMethod(OnStopMethodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            var serviceThreads = new List<Thread>();

            "Starting services . . .".Info(Runtime.EntryAssemblyName.Name);

            foreach (var service in servicesToRun)
            {
                var thread = new Thread(() =>
                {
                    onStartMethod.Invoke(service, new object[] { new string[] { } });
                    $"Started service '{service.GetType().Name}'".Info(service.GetType());
                });

                serviceThreads.Add(thread);
                thread.Start();
            }

            "Press any key to stop all services.".Info(Runtime.EntryAssemblyName.Name);
            Terminal.ReadKey(true, true);
            "Stopping services . . .".Info(Runtime.EntryAssemblyName.Name);

            foreach (var service in servicesToRun)
            {
                onStopMethod.Invoke(service, null);
                $"Stopped service '{service.GetType().Name}'".Info(service.GetType());
            }

            foreach (var thread in serviceThreads)
                thread.Join();

            "Stopped all services.".Info(Runtime.EntryAssemblyName.Name);
        }
    }
}
#endif
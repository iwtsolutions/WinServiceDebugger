using System;
using System.ServiceProcess;
using log4net;

namespace WinServiceDebugger
{
    public abstract class DebugServiceBase : ServiceBase, IDebuggableService
    {
        protected ILog logger;

        public DebugServiceBase()
            : this("Service") { }

        public DebugServiceBase(string logName)
        {
            logger = LogManager.GetLogger(logName);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        public abstract void StartProcess();

        public abstract void StopProcess();

        protected override void OnStart(string[] args)
        {
            try
            {
                StartProcess();
            }
            catch (Exception ex)
            {
                logger.Fatal("OnStart exception", ex);
            }
        }

        protected override void OnStop()
        {
            try
            {
                StopProcess();
            }
            catch (Exception ex)
            {
                logger.Fatal("OnStop exception", ex);
            }
        } 

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;

            if (ex != null)
                logger.Fatal("Unhandled exception", ex);

            Stop();
        }
    }
}
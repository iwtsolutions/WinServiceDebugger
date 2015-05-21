
namespace WinServiceDebugger
{
    public class ServiceDebugger
    {
        private readonly DebugServiceBase _debuggableService;

        public ServiceDebugger(DebugServiceBase service)
        {
            _debuggableService = service;
        }

        public void Debug()
        {
            // Start this process as an application, not a service
            _debuggableService.StartProcess();

            // MemoryAppender is read from the DebugForm
            var memAppender = new log4net.Appender.MemoryAppender
            {
                Name = "MemoryAppender",
                Layout = new log4net.Layout.PatternLayout("%level%newline%message%newline%exception"),
            };

            log4net.Config.BasicConfigurator.Configure(memAppender);

            var debugDialog = new DebugForm();
            debugDialog.FormClosed += debugDialog_FormClosed;
            debugDialog.ShowDialog();
        }

        public void Start()
        {
            Start(null);
        }

        public void Start(string[] args)
        {
            if (args != null)
            {
                foreach (string arg in args)
                {
                    if (arg.Trim().ToLower() == "--debug")
                    {
                        Debug();
                        return;
                    }
                }
            }

            // Just use/implement a log4net configuration file where your EXE is located, called log.config
            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(path + "\\log.config"));
            System.ServiceProcess.ServiceBase.Run(_debuggableService);
        }

        private void debugDialog_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            _debuggableService.StopProcess();
        }
    }
}
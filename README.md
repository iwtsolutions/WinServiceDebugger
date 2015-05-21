Windows Service Debugger
====================

This project is intended to provide an easy way to debug windows services using windows forms and log4net.

## Requirements

- Log4net

##Usage/Example

To run the windows service in debug mode, do one of the following:

- In your service, set the build configuration to Debug and make sure the DEBUG constant is set in Properties > Build.
- Run the exe of the service with '--debug' as an argument. E.g. SampleService.exe --debug

#### Entry Point (Program.cs)
	namespace SampleService
	{
		static class Program
		{
			static void Main(string[] args)
			{
    #if DEBUG
                new WinServiceDebugger.ServiceDebugger(new Service1()).Debug();
    #else
                new WinServiceDebugger.ServiceDebugger(new Service1()).Start(args);
    #endif
			}
		}
	}	

#### Service File (Service1.cs)
	namespace SampleService
	{
		public partial class Service1 : WinServiceDebugger.DebugServiceBase
		{
			private Processor _process; 

			public override void StartProcess()
			{
				_process = new Processor();
				new System.Threading.Thread(_process.Run).Start();
			}

			public override void StopProcess()
			{
				_process.Stop();
			}
		}

		public class Processor
		{
			private bool _continue = false;

			public void Run()
			{
				_continue = true;

				while (_continue)
				{
					System.Diagnostics.Debug.Print("I'm a test service.");
					System.Threading.Thread.Sleep(5000);
				}
			}

			public void Stop()
			{
				_continue = false;
			}
		}
	}
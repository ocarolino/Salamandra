using Salamandra.Engine.Services.Logger;
using Serilog;
using Serilog.Context;

namespace Salamandra.Engine.Services
{
    // Based on: https://github.com/thecodrr/BreadPlayer/blob/add082d5d3ed3da472791f9a5040920a6084a937/BreadPlayer.Common/BLogger.cs (2022-06-08)
    public class LogManager : IDisposable
    {
        private CaptureFilePathHook? captureFilePathHook;

        public ILogger? Logger { get; set; }
        public string OutputFolder { get; set; }
        public string DefaultFilename { get; set; }
        public string? CurrentFilename
        {
            get
            {
                if (this.Logger != null)
                    return this.captureFilePathHook?.Path;

                return null;
            }
        }
        public bool DailyInterval { get; set; }

        public LogManager(string outputFolder, string defaultFilename, bool dailyInterval)
        {
            this.OutputFolder = outputFolder;
            this.DefaultFilename = defaultFilename;
            this.DailyInterval = dailyInterval;
        }

        public void InitializeLog()
        {
            if (this.captureFilePathHook == null)
                this.captureFilePathHook = new CaptureFilePathHook();

            var outputTemplate = "{Timestamp:HH:mm:ss}\t{Level:u3}\t{ActionContext,-15}\t\t{Message:lj}{NewLine}{Exception}";
            var path = Path.Combine(this.OutputFolder, this.DefaultFilename + ".txt");

            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.File(path,
                    outputTemplate: outputTemplate,
                    rollingInterval: this.DailyInterval ? RollingInterval.Day : RollingInterval.Infinite,
                    hooks: captureFilePathHook,
                    shared: false)
                .CreateLogger();

            this.Logger = logger;
        }

        public void Information(string message, string context)
        {
            using (LogContext.PushProperty("ActionContext", context))
                this.Logger?.Information(message);
        }

        public void Error(string message, string context, Exception? exception = null)
        {
            using (LogContext.PushProperty("ActionContext", context))
                this.Logger?.Error(exception, message);
        }

        public void Fatal(string message, string context, Exception? exception = null)
        {
            using (LogContext.PushProperty("ActionContext", context))
                this.Logger?.Fatal(message, exception);
        }

        public void Dispose()
        {
            if (this.Logger != null)
                (this.Logger as IDisposable)!.Dispose();
        }
    }
}

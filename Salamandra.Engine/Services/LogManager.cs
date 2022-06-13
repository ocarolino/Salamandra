using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Salamandra.Engine.Extensions;
using Serilog;
using Serilog.Context;
using Serilog.Core;

namespace Salamandra.Engine.Services
{
    // Based on: https://github.com/thecodrr/BreadPlayer/blob/add082d5d3ed3da472791f9a5040920a6084a937/BreadPlayer.Common/BLogger.cs (2022-06-08)
    public class LogManager : IDisposable
    {
        public ILogger? Logger { get; set; }
        public string OutputFolder { get; set; }

        public LogManager(string outputFolder)
        {
            this.OutputFolder = outputFolder;
        }

        public void InitializeLog()
        {
            var outputTemplate = "{Timestamp:HH:mm:ss}\t{Level:u3}\t{ActionContext,-15}\t\t{Message:lj}{NewLine}{Exception}";
            var path = Path.Combine(this.OutputFolder, "Salamandra Player Log - .txt");

            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.File(path,
                    outputTemplate: outputTemplate,
                    rollingInterval: RollingInterval.Day,
                    shared: true)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Salamandra.Engine.Extensions;
using Serilog;
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
            var outputTemplate = "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";
            var path = Path.Combine(this.OutputFolder, "PlayerSalamandra-.txt");

            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(path,
                    outputTemplate: outputTemplate,
                    rollingInterval: RollingInterval.Day,
                    shared: true)
                .CreateLogger();

            this.Logger = logger;
        }

        public void Information(string message, params object[] propertyValues)
        {
            this.Logger?.Information(message, propertyValues);
        }

        public void Error(string message, Exception? exception = null, params object[] propertyValues)
        {
            this.Logger?.Error(exception, message, propertyValues);
        }

        public void Dispose()
        {
            if (this.Logger != null)
                (this.Logger as IDisposable)!.Dispose();
        }
    }
}

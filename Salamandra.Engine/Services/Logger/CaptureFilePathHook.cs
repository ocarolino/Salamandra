using Serilog.Sinks.File;
using System.Text;

namespace Salamandra.Engine.Services.Logger
{
    internal class CaptureFilePathHook : FileLifecycleHooks
    {
        public string? Path { get; private set; }

        public override Stream OnFileOpened(string path, Stream _, Encoding __)
        {
            Path = path;
            return base.OnFileOpened(path, _, __);
        }
    }
}

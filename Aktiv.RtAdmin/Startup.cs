using Microsoft.Extensions.DependencyInjection;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Serilog;
using System;
using System.IO;
using System.Text;
using RutokenPkcs11Interop;

namespace Aktiv.RtAdmin
{
    public static class Startup
    {
#if DEBUG
        private static readonly string pkcs11NativeLibraryName = Settings.RutokenEcpDllDefaultPath;
#else
        private const string pkcs11NativeLibraryName = "pkcs11NativeLib.dll";
#endif
        public static IServiceProvider Configure(string logFilePath)
        {
            ConfigureLogger(logFilePath);
            return RegisterServices();
        }

        private static void ConfigureLogger(string logFilePath)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel
                .Debug();

            // TODO: проверка на существование пути
            if (string.IsNullOrWhiteSpace(logFilePath))
            {
                loggerConfiguration.WriteTo.Console();
            }
            else
            {
                loggerConfiguration.WriteTo.File(logFilePath);
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }

        private static IServiceProvider RegisterServices()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return new ServiceCollection()
                .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true))
                .AddSingleton(s => new Pkcs11(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), pkcs11NativeLibraryName), AppType.MultiThreaded))
                .AddSingleton<PinsStore>()
                .AddSingleton<VolumeOwnersStore>()
                .AddTransient<RutokenCore>()
                .AddTransient<TokenParams>()
                .AddTransient<CommandHandlerBuilder>()
                .BuildServiceProvider();
        }
    }
}

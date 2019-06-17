using Microsoft.Extensions.DependencyInjection;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Serilog;
using System;
using System.Text;

namespace Aktiv.RtAdmin
{
    public static class Startup
    {
        private const string pkcs11NativeLibraryName = "pkcs11NativeLib.so";

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
                .AddSingleton(s => new Pkcs11(pkcs11NativeLibraryName, AppType.MultiThreaded))
                .AddSingleton<PinsStore>()
                .AddTransient<RutokenCore>()
                .AddTransient<TokenParams>()
                .AddTransient<CommandHandlerBuilder>()
                .BuildServiceProvider();
        }
    }
}

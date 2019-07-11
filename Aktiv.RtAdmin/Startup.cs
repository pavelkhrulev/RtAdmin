using Microsoft.Extensions.DependencyInjection;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Serilog;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using RutokenPkcs11Interop;

namespace Aktiv.RtAdmin
{
    public static class Startup
    {
        public static IServiceProvider Configure(string logFilePath, 
            string nativeLibraryPath)
        {
            ConfigureLogger(logFilePath);
            return RegisterServices(nativeLibraryPath);
        }

        private static void ConfigureLogger(string logFilePath)
        {
            var loggerConfiguration = new LoggerConfiguration().MinimumLevel
                                                               .Debug();

            // TODO: проверка на существование пути
            if (string.IsNullOrWhiteSpace(logFilePath))
            {
                loggerConfiguration.WriteTo.Console(outputTemplate: "{Message:lj}{NewLine}{Exception}");
            }
            else
            {
                loggerConfiguration.WriteTo.File(logFilePath);
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }

        private static IServiceProvider RegisterServices(string nativeLibraryPath)
        {
            // TODO: check file existance
            var nativeLibraryPathIsUse = !string.IsNullOrWhiteSpace(nativeLibraryPath)
                ? nativeLibraryPath
                : Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    GetNativeLibraryName());

            return new ServiceCollection()
                .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true))
                .AddSingleton(s => new Pkcs11(nativeLibraryPathIsUse, AppType.MultiThreaded))
                .AddSingleton<PinsStorage>()
                .AddSingleton<ConfigLinesStorage>()
                .AddSingleton<VolumeOwnersStore>()
                .AddTransient<TokenSlot>()
                .AddScoped<RuntimeTokenParams>()
                .AddScoped<LogMessageBuilder>()
                .AddTransient<CommandHandlerBuilder>()
                .BuildServiceProvider();
        }

        private static string GetNativeLibraryName()
        {
#if DEBUG
            return Settings.RutokenEcpDllDefaultPath;
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "rtpkcs11ecp.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "librtpkcs11ecp.so";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "librtpkcs11ecp.dylib";
            }

            throw new InvalidOperationException("Incorrect OS version");
#endif
        }
    }
}

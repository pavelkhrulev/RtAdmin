using Microsoft.Extensions.DependencyInjection;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Serilog;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Aktiv.RtAdmin
{
    public static class Startup
    {
#if DEBUG
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
                .AddSingleton(s => new Pkcs11(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), GetNativeLibraryName()), AppType.MultiThreaded))
                .AddSingleton<PinsStore>()
                .AddSingleton<VolumeOwnersStore>()
                .AddTransient<RutokenCore>()
                .AddTransient<TokenParams>()
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
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "librtpkcs11ecp.dylib";
            }

            throw new InvalidOperationException("Incorrect OS version");
#endif
        }
    }
}

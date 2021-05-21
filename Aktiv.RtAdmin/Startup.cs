using Microsoft.Extensions.DependencyInjection;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Serilog;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Aktiv.RtAdmin.Properties;
using Net.RutokenPkcs11Interop;
using Net.RutokenPkcs11Interop.HighLevelAPI;

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

            if (string.IsNullOrWhiteSpace(logFilePath))
            {
                loggerConfiguration.WriteTo.Console(outputTemplate: DefaultValues.LogTemplate);
            }
            else
            {
                loggerConfiguration.WriteTo.File(logFilePath, outputTemplate: DefaultValues.LogTemplate);
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }

        private static IServiceProvider RegisterServices(string nativeLibraryPath)
        {
            var nativeLibraryPathIsUse = !string.IsNullOrWhiteSpace(nativeLibraryPath) &&
                                         File.Exists(nativeLibraryPath)
                ? nativeLibraryPath
                : Path.Combine(
                    System.AppContext.BaseDirectory,
                    GetNativeLibraryName());

            RutokenPkcs11InteropFactories factory;
            try
            {
                factory = new RutokenPkcs11InteropFactories();
                using (_ = factory.RutokenPkcs11LibraryFactory.LoadPkcs11Library(factory, nativeLibraryPathIsUse, AppType.MultiThreaded));
            }
            catch (UnmanagedException)
            {
                Console.WriteLine(Resources.NativeLibraryFileInvalid);
                throw new AppMustBeClosedException(-1);
            }
            catch (LibraryArchitectureException)
            {
                Console.WriteLine(Resources.NativeLibraryFileInvalid);
                throw new AppMustBeClosedException(-1);
            }

            return new ServiceCollection()
                .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true))
                .AddSingleton<IRutokenPkcs11Library>(s => factory.RutokenPkcs11LibraryFactory.LoadRutokenPkcs11Library(factory, nativeLibraryPathIsUse, AppType.MultiThreaded))
                .AddSingleton<PinsStorage>()
                .AddSingleton<ConfigLinesStorage>()
                .AddSingleton<VolumeOwnersStore>()
                .AddSingleton<VolumeAttributesStore>()
                .AddTransient<TokenSlot>()
                .AddScoped<RuntimeTokenParams>()
                .AddScoped<LogMessageBuilder>()
                .AddTransient<CommandHandlerBuilder>()
                .BuildServiceProvider();
            
        }

        private static string GetNativeLibraryName()
        {
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
                return "rtpkcs11ecp.framework/rtpkcs11ecp";
            }

            throw new InvalidOperationException(Resources.IncorrectOsVersion);
        }
    }
}

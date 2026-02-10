using System;
using System.Configuration;
using System.Runtime.InteropServices;

namespace PowerMode
{
    /// <summary>
    /// This program allows for setting the Windows "power mode" or "power slider" value from the command line.
    /// </summary>
    class SetPowerMode
    {
        /// <summary>
        /// Execution starts here.
        /// </summary>
        /// <param name="args">Command line parameters.</param>
        /// <returns>Error status; 0 = success, non-zero = failure.</returns>
        static int Main(string[] args)
        {
            try
            {
                // Read from App.config.
                ReadConfig();

                if (args.Length == 0)
                {
                    // Report the current power modes.
                    uint result = PowerGetEffectiveOverlayScheme(out Guid effectiveMode);
                    if (result != 0)
                    {
                        return (int)result;
                    }

                    Console.WriteLine("Effective: {0} ({1})", FormatPowerMode(effectiveMode), effectiveMode);

                    if (PowerGetUserConfiguredACPowerMode(out Guid acMode) == 0)
                    {
                        Console.WriteLine("AC:        {0} ({1})", FormatPowerMode(acMode), acMode);
                    }

                    if (PowerGetUserConfiguredDCPowerMode(out Guid dcMode) == 0)
                    {
                        Console.WriteLine("DC:        {0} ({1})", FormatPowerMode(dcMode), dcMode);
                    }
                }
                else if (args.Length == 1)
                {
                    // Attempt to set the power mode (both AC and DC).
                    string parameter = args[0].ToLower();

                    if (parameter == "/?" || parameter == "-?")
                    {
                        Usage();
                        return 1;
                    }

                    Guid powerMode = ParsePowerMode(parameter);
                    uint result = PowerSetActiveOverlayScheme(powerMode);

                    if (result == 0)
                    {
                        Console.WriteLine("Set power mode to {0}.", powerMode);
                    }
                    else
                    {
                        Console.Error.WriteLine("Failed to set power mode.\n");
                        Usage();
                    }

                    return (int)result;
                }
                else
                {
                    // Parse /ac and /dc arguments.
                    Guid? acMode = null;
                    Guid? dcMode = null;

                    for (int i = 0; i < args.Length; i++)
                    {
                        string flag = args[i].ToLower();

                        if ((flag == "/ac" || flag == "-ac" || flag == "--ac") && i + 1 < args.Length)
                        {
                            acMode = ParsePowerMode(args[++i].ToLower());
                        }
                        else if ((flag == "/dc" || flag == "-dc" || flag == "--dc") && i + 1 < args.Length)
                        {
                            dcMode = ParsePowerMode(args[++i].ToLower());
                        }
                        else
                        {
                            Console.Error.WriteLine("Unrecognized argument: {0}\n", args[i]);
                            Usage();
                            return 1;
                        }
                    }

                    if (acMode == null && dcMode == null)
                    {
                        Usage();
                        return 1;
                    }

                    if (acMode != null)
                    {
                        Guid acGuid = acMode.Value;
                        uint result = PowerSetUserConfiguredACPowerMode(ref acGuid);

                        if (result == 0)
                        {
                            Console.WriteLine("Set AC power mode to {0}.", acGuid);
                        }
                        else
                        {
                            Console.Error.WriteLine("Failed to set AC power mode (error {0}).\n", result);
                            return (int)result;
                        }
                    }

                    if (dcMode != null)
                    {
                        Guid dcGuid = dcMode.Value;
                        uint result = PowerSetUserConfiguredDCPowerMode(ref dcGuid);

                        if (result == 0)
                        {
                            Console.WriteLine("Set DC power mode to {0}.", dcGuid);
                        }
                        else
                        {
                            Console.Error.WriteLine("Failed to set DC power mode (error {0}).\n", result);
                            return (int)result;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                // Print error information to the console.
                Console.Error.WriteLine("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
                Console.WriteLine();
                Usage();
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Format a power mode GUID as a friendly name, if known.
        /// </summary>
        private static string FormatPowerMode(Guid mode)
        {
            if (mode == PowerMode.BestPowerEfficiency) return "Best power efficiency";
            if (mode == PowerMode.Balanced) return "Balanced";
            if (mode == PowerMode.BestPerformance) return "Best performance";
            return "Unknown";
        }

        /// <summary>
        /// Parse a power mode string into a GUID. Accepts named modes or a raw GUID string.
        /// </summary>
        /// <param name="parameter">The power mode name or GUID string (should be lowercase).</param>
        /// <returns>The corresponding GUID.</returns>
        private static Guid ParsePowerMode(string parameter)
        {
            if (parameter == "bestpowerefficiency")
            {
                return PowerMode.BestPowerEfficiency;
            }
            else if (parameter == "balanced")
            {
                return PowerMode.Balanced;
            }
            else if (parameter == "bestperformance")
            {
                return PowerMode.BestPerformance;
            }
            else
            {
                try
                {
                    return new Guid(parameter);
                }
                catch (Exception)
                {
                    throw new ArgumentException(string.Format("Failed to parse power mode: {0}", parameter));
                }
            }
        }

        /// <summary>
        /// Print a usage message to the console.
        /// </summary>
        private static void Usage()
        {
            Console.WriteLine(
                    "PowerMode (GPLv3); used to set the active power mode on Windows 10, version 1709 or later\n" +
                    "https://github.com/AaronKelley/PowerMode\n" +
                    "\n" +
                    "  PowerMode                              Report the current power mode\n" +
                    "  PowerMode <mode>                       Set the power mode (both AC and DC)\n" +
                    "  PowerMode /ac <mode>                   Set the AC (plugged in) power mode\n" +
                    "  PowerMode /dc <mode>                   Set the DC (battery) power mode\n" +
                    "  PowerMode /ac <mode> /dc <mode>        Set AC and DC power modes independently\n" +
                    "\n" +
                    "  Modes: BestPowerEfficiency, Balanced, BestPerformance, or a GUID"
                );
        }

        private static void ReadConfig()
        {
            if (ConfigurationManager.AppSettings["BestPowerEfficiencyGuid"] != null)
            {
                PowerMode.BestPowerEfficiency = new Guid(ConfigurationManager.AppSettings["BestPowerEfficiencyGuid"]);
            }
            if (ConfigurationManager.AppSettings["BalancedGuid"] != null)
            {
                PowerMode.Balanced = new Guid(ConfigurationManager.AppSettings["BalancedGuid"]);
            }
            if (ConfigurationManager.AppSettings["BestPerformanceGuid"] != null)
            {
                PowerMode.BestPerformance = new Guid(ConfigurationManager.AppSettings["BestPerformanceGuid"]);
            }
        }

        /// <summary>
        /// Contains GUID constants for the different power modes.
        /// </summary>
        /// <seealso cref="https://docs.microsoft.com/en-us/windows-hardware/customize/desktop/customize-power-slider"/>
        private static class PowerMode
        {
            /// <summary>
            /// Best Power Efficiency mode.
            /// </summary>
            public static Guid BestPowerEfficiency = new Guid("961cc777-2547-4f9d-8174-7d86181b8a7a");

            /// <summary>
            /// Balanced mode.
            /// </summary>
            public static Guid Balanced = new Guid("00000000-0000-0000-0000-000000000000");

            /// <summary>
            /// Best Performance mode.
            /// </summary>
            public static Guid BestPerformance = new Guid("ded574b5-45a0-4f42-8737-46345c09c238");
        }

        /// <summary>
        /// Retrieves the active overlay power scheme and returns a GUID that identifies the scheme.
        /// </summary>
        /// <param name="EffectiveOverlayPolicyGuid">A pointer to a GUID structure.</param>
        /// <returns>Returns zero if the call was successful, and a nonzero value if the call failed.</returns>
        [DllImportAttribute("powrprof.dll", EntryPoint = "PowerGetEffectiveOverlayScheme")]
        private static extern uint PowerGetEffectiveOverlayScheme(out Guid EffectiveOverlayPolicyGuid);

        /// <summary>
        /// Sets the active power overlay power scheme.
        /// </summary>
        /// <param name="OverlaySchemeGuid">The identifier of the overlay power scheme.</param>
        /// <returns>Returns zero if the call was successful, and a nonzero value if the call failed.</returns>
        [DllImportAttribute("powrprof.dll", EntryPoint = "PowerSetActiveOverlayScheme")]
        private static extern uint PowerSetActiveOverlayScheme(Guid OverlaySchemeGuid);

        /// <summary>
        /// Retrieves the user configured power mode for when the device is in an AC (plugged in) power state.
        /// </summary>
        [DllImportAttribute("powrprof.dll", EntryPoint = "PowerGetUserConfiguredACPowerMode")]
        private static extern uint PowerGetUserConfiguredACPowerMode(out Guid PowerModeGuid);

        /// <summary>
        /// Retrieves the user configured power mode for when the device is in a DC (battery) power state.
        /// </summary>
        [DllImportAttribute("powrprof.dll", EntryPoint = "PowerGetUserConfiguredDCPowerMode")]
        private static extern uint PowerGetUserConfiguredDCPowerMode(out Guid PowerModeGuid);

        /// <summary>
        /// Updates the user configured power mode for when the device is in an AC (plugged in) power state.
        /// </summary>
        /// <param name="PowerModeGuid">A GUID that specifies the user configured power mode.</param>
        /// <returns>Returns zero if the call was successful, and a nonzero value if the call failed.</returns>
        /// <seealso cref="https://learn.microsoft.com/en-us/windows/win32/api/powrprof/nf-powrprof-powersetuserconfiguredacpowermode"/>
        [DllImportAttribute("powrprof.dll", EntryPoint = "PowerSetUserConfiguredACPowerMode")]
        private static extern uint PowerSetUserConfiguredACPowerMode(ref Guid PowerModeGuid);

        /// <summary>
        /// Updates the user configured power mode for when the device is in a DC (battery) power state.
        /// </summary>
        /// <param name="PowerModeGuid">A GUID that specifies the user configured power mode.</param>
        /// <returns>Returns zero if the call was successful, and a nonzero value if the call failed.</returns>
        /// <seealso cref="https://learn.microsoft.com/en-us/windows/win32/api/powrprof/nf-powrprof-powersetuserconfigureddcpowermode"/>
        [DllImportAttribute("powrprof.dll", EntryPoint = "PowerSetUserConfiguredDCPowerMode")]
        private static extern uint PowerSetUserConfiguredDCPowerMode(ref Guid PowerModeGuid);
    }
}

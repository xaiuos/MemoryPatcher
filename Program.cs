
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;


//=========================================================
//var hastegas = "hastega"
//module.ModuleName?.ToLower() == "clr.dll"
//addressBaseHexEndWith  = "B57D" ; 
//searchPattern = { 0x48, 0x89, 0x07 };
//patchPattern  = { 0x90, 0x90, 0x90 };



namespace MemoryPatcher
{
    class Program
    {
        // Win32 API constants IDK what is should do but it must have 
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_VM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PAGE_EXECUTE_READWRITE = 0x40;
        const int MEM_COMMIT = 0x1000;
        const int MEM_RESERVE = 0x2000;

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(int dwDesiDarkMagentaAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        static extern int GetLastError();



        //  admin check
        static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Console.Title = "Xaiuos Hastega Injector";
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("_____________________________________");
                Console.WriteLine("\n      Xaiuos Hastega Injector        ");
                Console.WriteLine("_____________________________________");
                Console.ResetColor();
                Console.WriteLine("\nProcessing...");

                // Check if running as admin
                if (!IsAdministrator())
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine("\n ERROR: not runing as admin");
                    Console.ResetColor();
                    Console.WriteLine("\n exit in 60 seconds...");
                    Thread.Sleep(60000);
                    return;
                }
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n administrator on");
                Console.ResetColor();




                // now get all processes and search for target
                Process[] processes = Process.GetProcesses();
                List<Process> dotNetProcesses = new List<Process>();
                Process? targetProcess = null;

                // finding hastega process  
                Console.WriteLine("\n searching for hastega");
                var hastegas = processes.Where(p => p.ProcessName.ToLower().Contains("hastega")).ToList();

                if (hastegas.Count > 0)
                {
                    targetProcess = hastegas[0];
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n found {targetProcess.ProcessName} PID: {targetProcess.Id} ");
                    Console.ResetColor();
                }
                else
                {

                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine("\n can't find hastega process");
                    Console.ResetColor();
                    Console.WriteLine("\n exit in 60 seconds...");
                    Thread.Sleep(60000);
                    return;














                    // Console.WriteLine("\n No 'hastega' process found. method 2 finding.NET processes");
                    // // searchingu sing  method .NET
                    // foreach (Process process in processes)
                    // {
                    //     try
                    //     {
                    //         // search with title 
                    //         if (!string.IsNullOrEmpty(process.MainWindowTitle))
                    //         {
                    //             Console.WriteLine($"\nChecking process: {process.ProcessName} (PID: {process.Id})");
                    //             bool has_net = false;

                    //             try
                    //             {
                    //                 foreach (ProcessModule module in process.Modules)
                    //                 {
                    //                     if (module.ModuleName?.ToLower().Contains("mscorelib") == true ||
                    //                         module.ModuleName?.ToLower().Contains("clr.dll") == true ||
                    //                         module.ModuleName?.ToLower().Contains("coreclr") == true)
                    //                     {
                    //                         has_net = true;
                    //                         Console.WriteLine($"\n- Found .NET module: {module.ModuleName}");
                    //                         break;
                    //                     }
                    //                 }
                    //             }
                    //             catch (Exception ex)
                    //             {
                    //                 Console.WriteLine($"\n- Error checking modules: {ex.Message}");
                    //             }

                    //             if (has_net)
                    //             {
                    //                 dotNetProcesses.Add(process);
                    //             }
                    //         }
                    //     }
                    //     catch (Exception ex)
                    //     {
                    //         Console.WriteLine($"\n Error accessing process {process.Id}: {ex.Message}");
                    //     }
                    // }

                    // if (dotNetProcesses.Count == 0)
                    // {
                    //     Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    //     Console.WriteLine("\nERROR: No .NET processes found. Make sure the target application is running.");
                    //     Console.ResetColor();
                    //     Console.WriteLine("\nexit in 30 seconds...");
                    //     Thread.Sleep(30000);
                    //     return;
                    // }

                    // // autamticly select first
                    // targetProcess = dotNetProcesses[0];
                    // Console.WriteLine($"\nAutomatically selecting process: {targetProcess.ProcessName} (PID: {targetProcess.Id})");
                }












                // next clr.dll mudeler search
                Console.WriteLine("\n searching for clr.dll module ");
                ProcessModule? targetModule = null;
                try
                {
                    foreach (ProcessModule module in targetProcess.Modules)
                    {
                        // Console.WriteLine($"\nChecking module: {module.ModuleName}");
                        if (module.ModuleName?.ToLower() == "clr.dll")
                        {
                            targetModule = module;
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"\n found : {module.ModuleName}");
                            Console.ResetColor();
                            break;
                        }
                    }

                    // ANOTHER General way to search for clr moduler 
                    // if (targetModule == null)
                    // {
                    //     foreach (ProcessModule module in targetProcess.Modules)
                    //     {
                    //         if (module.ModuleName?.ToLower().Contains("clr") == true)
                    //         {
                    //             targetModule = module;
                    //             Console.WriteLine($"\n  FOUND MODULE: {module.ModuleName}");
                    //             break;
                    //         }
                    //     }
                    // }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine($"\n ERROR: couldn't find modules  {ex.Message}");
                    Console.ResetColor();
                    Console.WriteLine("\n exit in 60 seconds...");
                    Thread.Sleep(60000);
                    return;
                }

                if (targetModule == null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine("\nERROR: Could not find module.");
                    Console.ResetColor();
                    Console.WriteLine("\nexit in 60 seconds...");
                    Thread.Sleep(60000);
                    return;
                }









                // finding address and patfh it 
                // Console.ForegroundColor = ConsoleColor.Cyan;
                // Console.WriteLine($"\n found base address : 0x{targetModule.BaseAddress.ToInt64():X}");
                // Console.ResetColor();


                // memory patch
                IntPtr processHandle = OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE, false, targetProcess.Id);
                if (processHandle == IntPtr.Zero)
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine($"\nERROR: can't  open process. Error code: {GetLastError()}");
                    Console.ResetColor();
                    Console.WriteLine("\nexit in 60 seconds...");
                    Thread.Sleep(60000);
                    return;
                }

                // the pattern 
                byte[] searchPattern = new byte[] { 0x48, 0x89, 0x07 }; // hex search
                byte[] patchPattern = new byte[] { 0x90, 0x90, 0x90 }; // NOP as many as  i want 

                //  start and end of the address range
                long startAddress = targetModule.BaseAddress.ToInt64();
                long endAddress = startAddress + targetModule.ModuleMemorySize;


                // idk if i should show it or not 
                // Console.WriteLine($"\n finding address between memory range: 0x{startAddress:X} - 0x{endAddress:X}");

                //  buffer to read memory in chunks
                int chunkSize = 4096;
                byte[] buffer = new byte[chunkSize];

                List<long> foundAddresses = new List<long>();


                // counter idk if if really need it 
                long totalBytesScanned = 0;
                long totalChunks = 0;
                long successfulReads = 0;

                for (long currentAddress = startAddress; currentAddress < endAddress; currentAddress += chunkSize)
                {
                    totalChunks++;
                    int bytesRead = 0;
                    if (!ReadProcessMemory(processHandle, (IntPtr)currentAddress, buffer, chunkSize, out bytesRead))
                    {
                        continue; // skip if read fail 
                    }

                    successfulReads++;
                    totalBytesScanned += bytesRead;

                    //  searching for the target adress
                    for (int i = 0; i < bytesRead - searchPattern.Length; i++)
                    {
                        bool match = true;
                        for (int j = 0; j < searchPattern.Length; j++)
                        {
                            if (buffer[i + j] != searchPattern[j])
                            {
                                match = false;
                                break;
                            }
                        }

                        if (match)
                        {
                            long foundAddress = currentAddress + i;

                            // Check if address ends with 15BD (in hex)
                            string addressBaseHexEndWith = foundAddress.ToString("X");
                            if (addressBaseHexEndWith.EndsWith("B57D"))
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine($"\n found match address: 0x{foundAddress:X}");
                                Console.ResetColor();
                                foundAddresses.Add(foundAddress);
                            }
                        }
                    }

                    // Progress  meh just remove it not taking time to find it to use this shit

                    // if (totalChunks % 1000 == 0)
                    // {
                    //      Console.WriteLine($"\nProgress: Scanned {totalBytesScanned / 1024 / 1024} MB ");
                    // }




                    //summery
                }
                Console.WriteLine($"\n finish scaning address");
                //maybe i delete this 
                Console.WriteLine($"\n Total scanned: {totalBytesScanned / 1024 / 1024} MB");
                Console.WriteLine($" \n Total chunks attempted: {totalChunks}, successfully read: {successfulReads}");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"\n Found {foundAddresses.Count}  matches.");
                Console.ResetColor();



                if (foundAddresses.Count > 0)
                {
                    //  patch all found addresses
                    Console.WriteLine("\n patching addresses...");
                    int successCount = 0;
                    foreach (long address in foundAddresses)
                    {
                        // allow writing
                        uint oldProtection;
                        if (!VirtualProtectEx(processHandle, (IntPtr)address, (UIntPtr)patchPattern.Length, PAGE_EXECUTE_READWRITE, out oldProtection))
                        {
                            Console.WriteLine($"\ncan't allowed writing 0x{address:X}. Error code: {GetLastError()}");
                            continue;
                        }



                        // Write the NOPs 
                        int bytesWritten;
                        if (WriteProcessMemory(processHandle, (IntPtr)address, patchPattern, patchPattern.Length, out bytesWritten))
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"\n Successfully patched address: 0x{address:X}");
                            Console.ResetColor();
                            successCount++;
                        }
                        else
                        {
                            Console.WriteLine($"\n Failed to patch memory at 0x{address:X}. Error code: {GetLastError()}");
                        }

                        // protect writing
                        uint temp;
                        VirtualProtectEx(processHandle, (IntPtr)address, (UIntPtr)patchPattern.Length, oldProtection, out temp);
                    }

                    Console.WriteLine($"\n patched {successCount} from {foundAddresses.Count} addresses.");

                    if (successCount > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("\n_____________________________________________________");
                        Console.WriteLine("\n           THE TIMER SHOULD'T WORK NOW               ");
                        Console.WriteLine("_____________________________________________________");
                        Console.ResetColor();
                        CloseHandle(processHandle);
                        Console.WriteLine("\n  UwU bye.");
                        Console.WriteLine("\nclose in 3 seconds...");

                        Thread.Sleep(3000);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        Console.WriteLine("\nFailed to patch any addresses ");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine("\n No adress matched found .");
                    Console.ResetColor();
                    Console.WriteLine("\nexit in 60 seconds...");
                    Thread.Sleep(60000);
                }

                // CloseHandle(processHandle);
                // Console.WriteLine("\n Finish.");
                // Console.WriteLine("\nclose in 5 seconds...");

                // Thread.Sleep(5000);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine($"\nCRITICAL ERROR: {ex.Message}");
                Console.WriteLine($"\nStack trace: {ex.StackTrace}");
                Console.ResetColor();
                Console.WriteLine("\nexit in 60 seconds...");
                Thread.Sleep(60000);
            }
        }
    }
}

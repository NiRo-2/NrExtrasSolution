using System.Diagnostics;
using System.Text;

namespace NrExtras.ExecuteTasks
{
    public static class ProcessHelper
    {
        /// <summary>
        /// Kill process by name
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="writeInfoToConsole">true by default</param>
        /// <returns>true on success, false otherwise</returns>
        public static bool KillProcessByName(string processName, bool writeInfoToConsole = true)
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit(); // Optional: waits until it's fully killed
                    if (writeInfoToConsole)
                        Console.WriteLine($"Killed process {process.ProcessName} (PID: {process.Id})");
                }
                catch (Exception ex)
                {
                    if (writeInfoToConsole)
                        Console.WriteLine($"Failed to kill process {process.ProcessName}: {ex.Message}");

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Kill process by ID
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="writeInfoToConsole">true by default</param>
        /// <returns></returns>
        public static bool KillProcessById(int processId, bool writeInfoToConsole = true)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                process.Kill();
                process.WaitForExit(); // Wait until it's fully killed

                if (writeInfoToConsole)
                    Console.WriteLine($"Killed process {process.ProcessName} (PID: {process.Id})");

                return true;
            }
            catch (Exception ex)
            {
                if (writeInfoToConsole)
                    Console.WriteLine($"Failed to kill process with PID {processId}: {ex.Message}");

                return false;
            }
        }
    }

    public static class ExecuteTasks
    {
        /// <summary>
        /// Run python script with args and return its result, with retries if timeout occurs.
        /// </summary>
        /// <param name="script">script path</param>
        /// <param name="args">args</param>
        /// <param name="timeoutSeconds">timeout in seconds (default: 30 seconds)</param>
        /// <param name="maxRetries">Maximum number of retry attempts (default: 3)</param>
        /// <returns>script output</returns>
        public static string RunPythonScript(string script, string args = "", int timeoutSeconds = 30, int maxRetries = 3)
        {
            int attempt = 0;
            while (attempt < maxRetries)
            {
                try
                {
                    attempt++;

                    // Check if the script exists
                    if (!File.Exists(script)) throw new Exception("Script not found");

                    // Configure the process to run the Python script
                    ProcessStartInfo start = new ProcessStartInfo
                    {
                        FileName = "python", // or "python3" depending on your environment
                        Arguments = $"{script} {args}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (Process process = Process.Start(start))
                    {
                        // Read the output from the script
                        StringBuilder output = new StringBuilder();
                        StringBuilder error = new StringBuilder();

                        process.OutputDataReceived += (sender, e) => output.AppendLine(e.Data);
                        process.ErrorDataReceived += (sender, e) => error.AppendLine(e.Data);

                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        // Wait for the process to exit or timeout
                        bool exited = process.WaitForExit(timeoutSeconds * 1000); // Timeout in milliseconds

                        if (!exited)
                        {
                            // If the process hasn't exited in time, kill it
                            process.Kill();
                            throw new TimeoutException($"The Python script timed out (attempt {attempt}/{maxRetries}).");
                        }

                        // Check the process exit code
                        if (process.ExitCode != 0)
                        {
                            output.AppendLine("Error occurred:");
                            output.AppendLine(error.ToString());
                        }

                        return output.ToString();
                    }
                }
                catch (TimeoutException timeoutEx)
                {
                    // Log the timeout exception and retry if needed
                    Console.WriteLine(timeoutEx.Message);
                    if (attempt >= maxRetries)
                    {
                        return $"Error: The script exceeded the timeout limit after {maxRetries} attempts.";
                    }
                    Console.WriteLine($"Retrying attempt {attempt} of {maxRetries}...");
                }
                catch (Exception ex)
                {
                    // Handle other exceptions and return the error message
                    return $"Error: {ex.Message}";
                }
            }

            return "Error: Max retry attempts reached.";
        }

        /// <summary>
        /// Execute command and read output
        /// </summary>
        /// <param name="command">command to execute</param>
        /// <returns>console string</returns>
        public static string ExecuteCommand_AndReadOutput(object command)
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run, and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows, and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);
                // The following commands are needed to redirect the standard output. 
                //This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();

                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();

                // return the command output.
                return result;
            }
            catch (Exception objException)
            {
                // Log the exception
                return "ExecuteCommandSync failed" + objException.Message;
            }
        }

        /// <summary>
        /// Executes a shell command synchronously.
        /// </summary>
        /// <param name="command">string command</param>
        /// <returns>string, as output of the command.</returns>
        public static void ExecuteCommandSync(object command)
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run, and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows, and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);
                // The following commands are needed to redirect the standard output. 
                //This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();

                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();

                // Display the command output.
                Console.WriteLine(result);
            }
            catch (Exception objException)
            {
                // Log the exception
                Console.WriteLine("ExecuteCommandSync failed" + objException.Message);
            }
        }

        /// <summary>
        /// Execute the command Asynchronously.
        /// </summary>
        /// <param name="command">string command.</param>
        public static void ExecuteCommandAsync(string command)
        {
            try
            {
                if (string.IsNullOrEmpty(command)) throw new ArgumentNullException(nameof(command), "Command cannot be null or empty.");

                //Asynchronously start the Thread to process the Execute command request.
                Thread objThread = new Thread(new ParameterizedThreadStart(ExecuteCommandSync));
                //Make the thread as background thread.
                objThread.IsBackground = true;
                //Set the Priority of the thread.
                objThread.Priority = ThreadPriority.AboveNormal;
                //Start the thread.
                objThread.Start(command);
            }
            catch (ThreadStartException ex)
            {
                // Log the exception
                Console.WriteLine("ExecuteCommandAsync failed ThreadStartException: " + ex.Message);
            }
            catch (ThreadAbortException ex)
            {
                // Log the exception
                Console.WriteLine("ExecuteCommandAsync failed ThreadAbortException: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine("ExecuteCommandAsync failed: " + ex.Message);
            }
        }
    }
}
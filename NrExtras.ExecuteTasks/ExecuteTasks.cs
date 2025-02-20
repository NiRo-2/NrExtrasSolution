using System.Diagnostics;
using System.Text;

namespace NrExtras.ExecuteTasks
{
    public static class ExecuteTasks
    {
        /// <summary>
        /// Run python script with args and return it's result
        /// </summary>
        /// <param name="script">script path</param>
        /// <param name="args">args</param>
        /// <returns>script output</returns>
        public static string RunPythonScript(string script, string args="")
        {
            try
            {
                // Check if the script exists
                if (File.Exists(script) == false) throw new Exception("Script not found");

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

                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        output.AppendLine("Error occurred:");
                        output.AppendLine(error.ToString());
                    }

                    return output.ToString();
                }
            }
            catch
            {
                throw;
            }
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
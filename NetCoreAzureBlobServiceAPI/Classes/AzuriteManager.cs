using System.Diagnostics;
namespace NetCoreAzureBlobServiceAPI.Classes
{
    public class AzuriteManager
    {
        private Process azuriteProcess;

        public void StartAzurite()
        {
            azuriteProcess = new Process
            {
                StartInfo =
            {
                FileName = "azurite.cmd", // or the path to your Azurite executable
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
            };

            azuriteProcess.Start();
        }

        public void StopAzurite()
        {
            if (azuriteProcess != null && !azuriteProcess.HasExited)
            {
                azuriteProcess.Kill();
                azuriteProcess.WaitForExit();
                azuriteProcess.Dispose();
            }
        }
    }
}
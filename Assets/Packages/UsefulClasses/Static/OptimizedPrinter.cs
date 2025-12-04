using System;
using System.IO;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace UsefulClasses
{
    public static class OptimizedPrinter
    {
        public static bool enableLogWrite = false;
        public static string errString = "[ERR]";
        private static readonly string logFilePath = Path.Combine(Application.persistentDataPath, "log.txt");
        private const long maxFileSizeBytes = 1024 * 1024 * 3; // 3 MB
        private static long currentFileSize = -1;

        public static void Print(string msg,[CallerFilePath] string filePath = "",[CallerMemberName] string memberName = "" )
        {
            Debug.Log(msg);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            Debug.Log($"[{fileName}.{memberName}] {msg}");
            if (enableLogWrite==false) return;
            AppendToLogTxt(msg);
        }
        public static void PrintErr(string msg,[CallerFilePath] string filePath = "",[CallerMemberName] string memberName = "" )
        {
            Debug.LogError(errString+msg);
            if (enableLogWrite==false) return;
            AppendToLogTxt(errString+msg);
        }
        

        private static void AppendToLogTxt( string msg)
        {
            
            if (currentFileSize == -1)
            {
                if (File.Exists(logFilePath))
                {
                    using (var stream = File.OpenRead(logFilePath))
                    {
                        currentFileSize = stream.Length;
                    }
                }
                else
                {
                    currentFileSize = 0;
                }
            }

            string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}{Environment.NewLine}";
            int estimatedSize = System.Text.Encoding.UTF8.GetByteCount(logLine);

            if (currentFileSize + estimatedSize > maxFileSizeBytes)
            {
                enableLogWrite = false;
                return;
            }

            File.AppendAllText(logFilePath, logLine);
            currentFileSize += estimatedSize;
        }
        
        public static void AddHardwareInfo()
        {
            // Create file and write device info
            using (StreamWriter writer = File.CreateText(logFilePath))
            {
                writer.WriteLine("=== Device Info ===");
                writer.WriteLine("Model: " + SystemInfo.deviceModel);
                writer.WriteLine("Name: " + SystemInfo.deviceName);
                writer.WriteLine("Type: " + SystemInfo.deviceType);
                writer.WriteLine("OS: " + SystemInfo.operatingSystem);
                writer.WriteLine("CPU: " + SystemInfo.processorType);
                writer.WriteLine("CPU Cores: " + SystemInfo.processorCount);
                writer.WriteLine("RAM MB: " + SystemInfo.systemMemorySize);
                writer.WriteLine("GPU: " + SystemInfo.graphicsDeviceName);
                writer.WriteLine("GPU RAM MB: " + SystemInfo.graphicsMemorySize);
                writer.WriteLine("Resolution: " + Screen.width + "x" + Screen.height);
                writer.WriteLine("Platform: " + Application.platform);
                writer.WriteLine("Language: " + Application.systemLanguage);
                writer.WriteLine("App Version: " + Application.version);
                writer.WriteLine("====================\n");
            }
        }
    }
}

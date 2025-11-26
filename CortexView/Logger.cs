using System;
using System.IO;

public static class Logger
{
    public static void Log(string message)
    {
        string filePath = "error.log";
        File.AppendAllText(filePath, $"{DateTime.Now:u} {message}\n");
    }
}

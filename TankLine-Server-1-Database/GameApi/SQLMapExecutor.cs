using System;
using System.Diagnostics;

public class SQLMapExecutor
{
    public static void ExecuteSQLMap()
    {
        var sqlMapPath = "/home/ubuntu/TankLine/TankLine-Server-1-Database/GameApi/sqlmap/sqlmap.py";  // path to sqlmap.py
        var targetUrl = "https://localhost/api/auth/login"; // URL à tester pour les injections SQL
        
        var startInfo = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"\"{sqlMapPath}/sqlmap.py\" -u \"{targetUrl}\" --batch",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(startInfo))
        {
            using (var reader = process.StandardOutput)
            {
                string output = reader.ReadToEnd();
                Console.WriteLine(output);
            }
        }
    }
}



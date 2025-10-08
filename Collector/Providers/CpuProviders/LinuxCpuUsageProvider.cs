using System;
using System.IO;
using System.Threading;
using MonitoringSystem.Collector.Interfaces;

namespace MonitoringSystem.Collector.Providers.CpuProviders;

public class LinuxCpuUsageProvider : ICpuUsageProvider
{
    public double GetCpuUsage()
    {
        try
        {
            var lines = File.ReadAllLines("/proc/stat");
            var cpuLine = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);

            ulong user = ulong.Parse(cpuLine[1]);
            ulong nice = ulong.Parse(cpuLine[2]);
            ulong system = ulong.Parse(cpuLine[3]);
            ulong idle = ulong.Parse(cpuLine[4]);
            ulong total1 = user + nice + system + idle;

            Thread.Sleep(500);

            lines = File.ReadAllLines("/proc/stat");
            cpuLine = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);

            ulong user2 = ulong.Parse(cpuLine[1]);
            ulong nice2 = ulong.Parse(cpuLine[2]);
            ulong system2 = ulong.Parse(cpuLine[3]);
            ulong idle2 = ulong.Parse(cpuLine[4]);
            ulong total2 = user2 + nice2 + system2 + idle2;
            ulong totalDelta = total2 - total1;
            ulong idleDelta = idle2 - idle;
            double cpuUsage = 100.0 * (totalDelta - idleDelta) / totalDelta;
            
            return Math.Round(cpuUsage, 2);
        }
        catch (Exception ex)
        {
            throw new CpuUsageException("Failed to read Linux CPU usage.", ex);
        }
    }
}
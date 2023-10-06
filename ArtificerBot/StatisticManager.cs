using System.Text;
using Artificer.Utility;

namespace Artificer.Bot;

public class StatisticManager
{
    private readonly string reportPath;
    
    public StatisticManager(string path)
    {
        Debug.Log("Initializing an Statistic Subsystem Instance...", Debug.Sender.Statistic, Debug.MessageStatus.INFO);

        reportPath = path;
        
        //new Thread(StartKeepingStatistics).Start();
        
        Debug.Log($"The Statistic Subsystem instance initialized successfully.", Debug.Sender.Statistic, Debug.MessageStatus.INFO);
    }

    private async void StartKeepingStatistics(object? o)
    {
        try
        {
            var start = DateTime.Now;
            while (true)
            {
                var span = DateTime.Now - start;
            
                if (span.Days == 1)
                {
                    await AddStatistic();
                
                    start = DateTime.Now;

                    ActivityReport();
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }
        catch (Exception e)
        {
            Debug.Log($"[StartKeepingStatistics] {e.Message} {e.StackTrace}", Debug.Sender.Statistic, Debug.MessageStatus.FAIL);
        }
    }

    public async Task AddStatistic()
    {
        var dau = await Kernel.Database.GetDailyActiveUsers(DateTime.Parse("30.03.2023"));
        var wau = await Kernel.Database.GetWeeklyActiveUsers(DateTime.Parse("30.03.2023"));
        var mau = await Kernel.Database.GetMonthlyActiveUsers(DateTime.Parse("30.03.2023"));
        var mnu = await Kernel.Database.GetMonthlyNewUsers(DateTime.Parse("30.03.2023"));
                
        var utilisation = (double) await Kernel.Database.GetConversUser() / 
            await Kernel.Database.GetTrialUser() * 100;
                
        var frequency = (double) await Kernel.Database.GetTotalCountActionsPerDay(DateTime.Parse("30.03.2023")) /
                        await Kernel.Database.GetTotalDailyActiveUsers(DateTime.Parse("30.03.2023"));
                
        var churnRate = (double)(await Kernel.Database.GetMonthlyLeaveUsers(DateTime.Parse("30.03.2023")) - 
                                 await Kernel.Database.GetMonthlyReturnUsers(DateTime.Parse("30.03.2023"))) / mnu * 100;

        var arpu = await Kernel.Database.GetMonthlyProfit(DateTime.Parse("30.03.2023")) / mau;
        var arppu = await Kernel.Database.GetMonthlyProfit(DateTime.Parse("30.03.2023")) / 
                    await Kernel.Database.GetPayingUsersPerMonth(DateTime.Parse("30.03.2023"));
                
        await Kernel.Database.AddRecordToStatistics(dau, wau, mau, mnu, utilisation, frequency, churnRate, arpu, arppu);
    }

    public void UpdateReports()
    {
        ActivityReport();
    }

    private void ActivityReport()
    {
        var statList = Kernel.Database.GetMountlyStatistics();

        if (statList.Count <= 0) return;
        using var writer = new StreamWriter(Path.Combine(reportPath, "activity.csv"), Encoding.UTF8, 
            new FileStreamOptions {Access = FileAccess.Write, Mode = FileMode.Create});
        writer.WriteLine($"dau;wau;mau;datetime");
        foreach (var item in statList)
        {
            writer.WriteLine($"{item.DailyActiveUsers};{item.WeeklyActiveUsers};{item.MonthlyActiveUsers};{item.Date}");
        }
    }
}
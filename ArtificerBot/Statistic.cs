namespace Artificer.Bot;

public class Statistic
{
    public Statistic(int dau, int wau, int mau, int mnu, double utilisation, 
        double frequency, double churnRate, double arpu, double arppu, DateTime date)
    {
        DailyActiveUsers = dau;
        WeeklyActiveUsers = wau;
        MonthlyActiveUsers = mau;
        MonthlyNewUsers = mnu;
        Utilisation = utilisation;
        Frequency = frequency;
        ChurnRate = churnRate;
        AverageRevenuePerUser = arpu;
        AverageRevenuePerPayingUser = arppu;
        Date = date;
    }
    
    public int DailyActiveUsers { get; } // (Ежедневные Активные Пользователи) — количество уникальных пользователей, которые зашли в приложение в течение суток.
    
    public int WeeklyActiveUsers { get; } // (Еженедельные Активные Пользователи) — количество уникальных пользователей, которые зашли в приложение в течение недели.
    
    public int MonthlyActiveUsers { get; } // Monthly Active Users (Ежемесячные Активные Пользователи) — количество уникальных пользователей, которые зашли в приложение в течение месяца.
    
    public int MonthlyNewUsers { get; } // MNU — количество новых пользователей за месяц. Тут интересно и то, сколько новых людей пришло в продукт, и то, сколько денег они вам принесли, их вклад в прибыль и обороты.

    public double Utilisation { get; } // Сколько человек начали использовать ваш продукт после успешно пройденного онбординга, покупки или оформления подписки.
    
    public double Frequency { get; } // Frequency или частота использования. Среднее количество активных действий в продукте.
    
    public double ChurnRate { get; } // Churn rate или уровень оттока пользователей. Если больше 5-7% — это плохой знак.
    
    public double AverageRevenuePerUser { get; } // Average Revenue Per User (Средний Счет На Пользователя) — средних доход с пользователя. Рассчитывается по формуле: Выручка приложения / Количество всех пользователей, посетивших приложение за период полученной выручки.
   
    public double AverageRevenuePerPayingUser { get; } // verage Revenue Per Paying User (Средний Счет На Платящего Пользователя) — средний доход с одного платящего пользователя. Рассчитывается по формуле: Выручка приложения / Количество пользователей, совершивших платеж.
    
    public DateTime Date { get; }
}
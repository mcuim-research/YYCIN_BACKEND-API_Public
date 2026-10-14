using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using test3.Jobs;
using test3.Models;

namespace test3.Jobs
{
    public class NotificationAlarmJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    public async Task Execute(IJobExecutionContext context)
    {
        // 呼叫 Firebase API 發送通知
        var reminders = GetTimeForTakingMedicine();
        if(reminders == null){
            return;
        }
        foreach (var reminder in reminders)
        {

            var title = "您的服藥時間到了。";
            var body = "請服用您的藥物！";
            //斷尾求生
            // try{
            //     using (var scope = _serviceProvider.CreateScope())
            //     {
            //         var repository = scope.ServiceProvider.GetRequiredService<SmartBoxMedicationRepo>();
            //         var re = repository.GetSmartBoxMedication(reminder.SmartBoxMedication.SmartBoxMedicationID);
            //     }
            // }
            // var token = reminder.AccountFirebase.Token;
            // var notificationHelper = new NotificationHelper();
            // notificationHelper.sendNotification(token, title, body);
        }

        
    }
    public List<Reminder> GetTimeForTakingMedicine()
    {
        List<Reminder> re;
        try{
            using (var scope = _serviceProvider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<ReminderRepo>();
                re = repository.GetRemindersThisMinute();
                return re;
            }
        }catch(Exception e){
            Console.WriteLine(e);
            return null;
        }
        

    }
}
}

public class SchedulerService : IHostedService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private IScheduler _scheduler;

    public SchedulerService(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _scheduler = await _schedulerFactory.GetScheduler();
        var job = JobBuilder.Create<NotificationAlarmJob>().Build();
        var trigger = TriggerBuilder.Create()
            .WithCronSchedule("0 * * * * ?") // 每天8點執行
            .Build();

        await _scheduler.ScheduleJob(job, trigger, cancellationToken);
        await _scheduler.Start(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _scheduler?.Shutdown(cancellationToken);
    }
}
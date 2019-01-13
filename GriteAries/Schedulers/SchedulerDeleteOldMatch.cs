using System.Collections.Async;
using System.Threading.Tasks;
using System;
using Quartz;
using Quartz.Impl;
using GriteAries.Models;

namespace GriteAries.Schedulers
{
    public class SchedulerDeleteOldMatch
    {
        public static async void Start()
        {
            DateTime date = DateTime.Now.AddMinutes(5);
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<RunDeleteMatches>().Build();

            ITrigger trigger = TriggerBuilder.Create()  // создаем триггер
                .WithIdentity("deletesRun", "group4")     // идентифицируем триггер с именем и группой
                .StartAt(date)                           // запуск сразу после начала выполнения
                .WithSimpleSchedule(x => x            // настраиваем выполнение действия
                    .WithIntervalInSeconds(5)          // через 1 минуту
                    .RepeatForever())                   // бесконечное повторение
                .Build();                               // создаем триггер

            await scheduler.ScheduleJob(job, trigger);        // начинаем выполнение работы
        }
    }

    public class RunDeleteMatches : IJob
    {
        Job _job = new Job();
        public async Task Execute(IJobExecutionContext context)
        {
            await _job.DeleteOldMatches();
        }
    }
}
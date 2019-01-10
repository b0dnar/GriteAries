using System;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace GriteAries.Schedulers
{
    public class SchedulerEquilsData
    {
        public static async void Start()
        {
            DateTime date = DateTime.Now.AddSeconds(100); 
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<RunEquilsData>().Build();

            ITrigger trigger = TriggerBuilder.Create()  // создаем триггер
                .WithIdentity("equilsRun", "group3")     // идентифицируем триггер с именем и группой
                .StartAt(date)                           // запуск сразу после начала выполнения
                .WithSimpleSchedule(x => x            // настраиваем выполнение действия
                    .WithIntervalInSeconds(5)          // через 1 минуту
                    .RepeatForever())                   // бесконечное повторение
                .Build();                               // создаем триггер

            await scheduler.ScheduleJob(job, trigger);        // начинаем выполнение работы
        }
    }

    public class RunEquilsData : IJob
    {
        Job _job = new Job();
        public async Task Execute(IJobExecutionContext context)
        {
            await _job.EquilsData();
        }
    }
}
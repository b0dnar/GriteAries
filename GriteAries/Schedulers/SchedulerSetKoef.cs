using System.Collections.Async;
using System.Threading.Tasks;
using System;
using Quartz;
using Quartz.Impl;
using GriteAries.Models;

namespace GriteAries.Schedulers
{
    public class SchedulerSetKoef
    {
        public static async void Start()
        {
            DateTime date = DateTime.Now.AddSeconds(30);
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<RunSetKoef>().Build();

            ITrigger trigger = TriggerBuilder.Create()  // создаем триггер
                .WithIdentity("setKoefRun", "group2")     // идентифицируем триггер с именем и группой
                .StartAt(date)                            // запуск сразу после начала выполнения
                .WithSimpleSchedule(x => x            // настраиваем выполнение действия
                    .WithIntervalInSeconds(7)          // через 1 минуту
                    .RepeatForever())                   // бесконечное повторение
                .Build();                               // создаем триггер

            await scheduler.ScheduleJob(job, trigger);        // начинаем выполнение работы
        }
    }

    public class RunSetKoef : IJob
    {
        Job _job = new Job();
        public async Task Execute(IJobExecutionContext context)
        {
            const int maxThread = 10;
            var allFootball = Container.GetUsedDatas(TypeSport.Football);

            if(allFootball.Count == 0)
            {
                return;
            }

            await allFootball.ParallelForEachAsync(async x =>
            {
               await _job.SetKoef(x);
            }, maxThread);
        }
    }
}




    

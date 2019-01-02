using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace GriteAries.Schedulers
{
    public class ScheduleStartData 
    {
        public static async void Start()
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<RunStartData>().Build();

            ITrigger trigger = TriggerBuilder.Create()  // создаем триггер
                .WithIdentity("startRun", "group1")     // идентифицируем триггер с именем и группой
                .StartNow()                            // запуск сразу после начала выполнения
                .WithSimpleSchedule(x => x            // настраиваем выполнение действия
                    .WithIntervalInMinutes(5)          // через 1 минуту
                    .RepeatForever())                   // бесконечное повторение
                .Build();                               // создаем триггер

            await scheduler.ScheduleJob(job, trigger);        // начинаем выполнение работы
        }
    }

    public class RunStartData : IJob
    {
        Job _job = new Job();
        public async Task Execute(IJobExecutionContext context)
        {
            await _job.RunFootball();
        }
    }
}
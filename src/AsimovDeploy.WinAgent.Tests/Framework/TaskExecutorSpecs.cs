using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsimovDeploy.WinAgent.Framework.Common;
using NUnit.Framework;
using Shouldly;

namespace AsimovDeploy.WinAgent.Tests.Framework
{
    [TestFixture]
    public class TaskExecutorSpecs
    {
        [Test]
        public async Task executes_one_task()
        {
            TaskExecutor e = new TaskExecutor();
            e.Start();

            var testTask = new TestTask();
            await e.AddTask(testTask);
            
            testTask.Executed.ShouldBe(true);
        }

        [Test]
        public void blocks_when_max_concurrent_tasks_is_reached()
        {
            TaskExecutor e = new TaskExecutor();
            
            for (int i = 0; i < TaskExecutor.MaxConcurrentTasks; i++)
            {
                e.AddTask(new TestTask());
            }

            var extraTask = Task.Run(() =>
            {
                var taskThatWontBeAddedWhileTaskQueueIsFull = new TestTask();
                e.AddTask(taskThatWontBeAddedWhileTaskQueueIsFull);
            });

            extraTask.Wait(TimeSpan.FromMilliseconds(500));
            extraTask.IsCompleted.ShouldBe(false);
            
            //Start processing task queue
            e.Start();
            
            extraTask.Wait(TimeSpan.FromMilliseconds(500));
            extraTask.IsCompleted.ShouldBe(true);


        }
    }

    public class TestTask    : AsimovTask
    {
        public bool Executed;
        protected override void Execute()
        {
            Executed = true;
        }
    }
}

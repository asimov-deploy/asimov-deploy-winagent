/*******************************************************************************
* Copyright (C) 2012 eBay Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*   http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
******************************************************************************/

using System;
using AsimovDeploy.WinAgent.Framework.Models;
using StructureMap;
using log4net;

namespace AsimovDeploy.WinAgent.Framework.Common
{
    public abstract class AsimovTask
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        protected ILog Log;

        protected AsimovTask()
        {
            Log = LogManager.GetLogger(GetType());
        }

        protected abstract void Execute();

        protected virtual string InfoString()
        {
            return "";
        }

        protected virtual string GetTaskName()
        {
            return GetType().Name;
        }

        public event Action<Exception> Completed;

        public void ExecuteTask()
        {
            try
            {
                Log.Info($"Executing {GetTaskName()} - {InfoString()}");
                Execute();
                RaiseExecuted(null);
            }
            catch (Exception ex)
            {
                Log.Error("Task failed", ex);
                RaiseExecuted(ex);
            }
        }

        public void RaiseExecuted(Exception exception) => Completed?.Invoke(exception);

        private IAsimovConfig _config;
        protected virtual IAsimovConfig Config => _config ?? (_config = ObjectFactory.GetInstance<IAsimovConfig>());

        protected virtual void AddTask(AsimovTask task)
        {
            ObjectFactory.GetInstance<ITaskExecutor>().AddTask(task);
        }
    }
}
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Deployment;
using AsimovDeploy.WinAgent.Framework.Deployment.Steps;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Framework.Models.Units;
using StructureMap;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace AsimovDeploy.WinAgent.Framework.Tasks
{
    public class DeployTask : AsimovTask
    {
        private readonly DeployUnit _deployUnit;
        private readonly AsimovVersion _version;
        private readonly ParameterValues _parameterValues;
	    private readonly AsimovUser _user;

	    private IList<Func<IDeployStep>> _steps = new List<Func<IDeployStep>>();

        public DeployTask(DeployUnit deployUnit, AsimovVersion version, ParameterValues parameterValues, AsimovUser user)
        {
            _deployUnit = deployUnit;
            _version = version;
            _parameterValues = parameterValues;
	        _user = user;

	        AddDeployStep<CleanTempFolder>();
            AddDeployStep<CopyPackageToTempFolder>();
        }

        public void AddDeployStep<T>() where T : IDeployStep
        {
            _steps.Add(() => ObjectFactory.GetInstance(typeof(T)) as IDeployStep);
        }
        
        public void AddDeployStep(IDeployStep step) 
        {
            _steps.Add(() => step);
        }

        protected virtual DeployContext CreateDeployContext()
        {
            return new DeployContext()
                {
                    DeployUnit = _deployUnit,
                    Log = Log,
                    NewVersion = _version,
                    ParameterValues = _parameterValues
                };
        }

        protected override void Execute()
        {
            if (PasswordIsIncorrect())
            {
                Log.Error("Invalid deploy password, aborting deployment");
                return;
            }


            InDeployContext(context =>
            {
                context.DeployUnit.StartingDeploy(context.NewVersion, context.LogFileName, _user, _parameterValues);

                Log.InfoFormat("Starting deployment of {0}, Version: {1}, {2} {3}", _deployUnit.Name, _version.Number, _parameterValues.GetLogString(), GetCurrentUserInfo());

                foreach (var stepFunc in _steps)
                {
                    var step = stepFunc();
                    Log.InfoFormat("Executing deploy step: {0}", step.GetType().Name);
                    step.Execute(context);
                }

                context.DeployUnit.DeployCompleted();

                Log.Info("Deployment completed");
            });
        }

	    private string GetCurrentUserInfo()
	    {
			if (_user.UserName != null)
			{
				return string.Format("- Triggered by {0}", _user.UserName);	
			}
		    return "";
	    }

	    private bool PasswordIsIncorrect()
        {
            if (!_deployUnit.HasDeployParameters)
                return false;

            foreach (var parameter in _deployUnit.DeployParameters)
            {
                var passwordParameter = parameter as PasswordActionParameter;
                if (passwordParameter == null) continue;

                var suppliedPassword = _parameterValues.GetValue(passwordParameter.Name);
                return suppliedPassword != passwordParameter.Password;
            }

            return false;
        }

        private void InDeployContext(Action<DeployContext> action)
        {
            var context = CreateDeployContext();

            FileAppender fileAppender;
            var logger = CreateLogger(context, out fileAppender);

            try
            {
                action(context);
            }
            catch (Exception ex)
            {
                context.DeployUnit.DeployFailed();
                context.Log.Error("DeployFailed", ex);
            }
            finally
            {
                logger.RemoveAppender(fileAppender);
                fileAppender.Close();
            }

        }

        private Logger CreateLogger(DeployContext context, out FileAppender fileAppender)
        {
            var logger = (Logger) Log.Logger;

            fileAppender = new FileAppender();
	        fileAppender.Encoding = Encoding.UTF8;

            // update file property of appender
            context.LogFileName = string.Format("deploy-{0:yyyy-MM-dd_HH_mm_ss}.log", DateTime.Now);
            fileAppender.File = Path.Combine(context.DeployUnit.DataDirectory, "Logs", context.LogFileName);
            // add the layout
            var patternLayout = new PatternLayout("%date{HH:mm:ss} [%-5level]  %m%n");
            fileAppender.Layout = patternLayout;
            // add the filter for the log source
            // activate the options
            fileAppender.ActivateOptions();

            logger.AddAppender(fileAppender);
            return logger;
        }
    }
}
using AsimovDeploy.WinAgent.Framework.Common;
using AsimovDeploy.WinAgent.Framework.Events;
using AsimovDeploy.WinAgent.Framework.LoadBalancers;
using AsimovDeploy.WinAgent.Framework.Models;
using AsimovDeploy.WinAgent.Framework.Tasks;
using AsimovDeploy.WinAgent.Web.Commands;
using AsimovDeploy.WinAgent.Web.Contracts;
using NUnit.Framework;
using Shouldly;

namespace AsimovDeploy.WinAgent.Tests.Tasks
{
	[TestFixture]
	public class ChangeLoadBalancerStateTaskSpecs
	{
		private FakeNotifier _fakeNotifier;
		private TestChangeLoadBalancerStateTask _changeLoadBalancerStateTask;
		private FakeLoadBalancerService _loadBalancerService;

		[SetUp]
		public void SetUp()
		{
			_fakeNotifier = new FakeNotifier();
			_loadBalancerService = new FakeLoadBalancerService();
			_changeLoadBalancerStateTask = new TestChangeLoadBalancerStateTask(
				new ChangeLoadBalancerStateCommand { action = "disable" },
				_loadBalancerService,
				_fakeNotifier);
		}

		[Test]
		public void should_send_notification_when_state_is_disabled()
		{
			_loadBalancerService.StartState = false;

			_changeLoadBalancerStateTask.ExecuteDisableAction();

			_fakeNotifier.WasNotified.ShouldBe(true);
		}

		[Test]
		public void should_timeout_after_timeout_period_and_not_send_notification()
		{
			_loadBalancerService.StartState = true;
			_changeLoadBalancerStateTask.SecondsToWaitBeforeTimeout = 1;

			_changeLoadBalancerStateTask.ExecuteDisableAction();

			_fakeNotifier.WasNotified.ShouldBe(false);
		} 
		
		[Test]
		public void should_send_notification_when_load_balancer_has_finished_disabling()
		{
			_loadBalancerService.StartState = true;
			_loadBalancerService.NumberOfSecondsThatActionTakesToComplete = 1;
			_changeLoadBalancerStateTask.SecondsToWaitBeforeTimeout = 10;

			_changeLoadBalancerStateTask.ExecuteDisableAction();

			_fakeNotifier.WasNotified.ShouldBe(true);
		} 
	}

	public class TestChangeLoadBalancerStateTask : ChangeLoadBalancerStateTask
	{
		private readonly IAsimovConfig _config;

		public TestChangeLoadBalancerStateTask(ChangeLoadBalancerStateCommand command, ILoadBalancerService loadBalancerService, INotifier nodeFront) : base(command, loadBalancerService, nodeFront)
		{
			_config = new AsimovConfig();
		}

		public void ExecuteDisableAction()
		{
			Execute();
		}

		public int SecondsToWaitBeforeTimeout
		{
			set { Config.LoadBalancerTimeout = value; }
		}

		protected override IAsimovConfig Config
		{
			get { return _config; }
		}
	}
	public class FakeNotifier : INotifier
	{
		public bool WasNotified = false;
		public void Notify(AsimovEvent data)
		{
			WasNotified = true;
		}
	}

	public class FakeLoadBalancerService : ILoadBalancerService
	{
		public bool StartState;
		public int NumberOfSecondsThatActionTakesToComplete = 11;
		private int secondsPassed = 0;

		public bool UseLoadBalancer { get; set; }
		public LoadBalancerStateDTO GetCurrentState()
		{
			if (secondsPassed == NumberOfSecondsThatActionTakesToComplete)
				return new LoadBalancerStateDTO { enabled = false };
			secondsPassed++;

			return new LoadBalancerStateDTO { enabled = StartState };
		}

		public void EnableServer()
		{
		}

		public void DisableServer()
		{
		}
	}
}
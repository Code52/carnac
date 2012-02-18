using System;
using Analects.SettingsService;
using Carnac.Logic;
using Carnac.Models;
using Carnac.Utilities;
using Carnac.ViewModels;
using NSubstitute;
using Xunit;

namespace Carnac.Tests.ViewModels
{
    public class ShellViewModelTests
    {
        readonly ISettingsService settingsService = Substitute.For<ISettingsService>();
        readonly IScreenManager screenManager = Substitute.For<IScreenManager>();
        readonly ITimerFactory timerFactory = Substitute.For<ITimerFactory>();
        private ShellViewModel viewModel;

        public ShellViewModelTests()
        {
            viewModel = new ShellViewModel(settingsService, screenManager, timerFactory);
        }

        [Fact]
        public void Constructor_Always_StartsTimer()
        {
            timerFactory.Received().Start(1000, Arg.Any<Action>());
        }

        [Fact]
        public void Constructor_Always_FetchesScreens()
        {
            screenManager.Received().GetScreens();
        }

        [Fact]
        public void Constructor_Always_ChecksForSettings()
        {
            settingsService.Received().Get<Settings>("PopupSettings");
        }

    }
}

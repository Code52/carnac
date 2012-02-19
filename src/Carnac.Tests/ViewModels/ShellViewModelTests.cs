using Analects.SettingsService;
using Caliburn.Micro;
using Carnac.Logic;
using Carnac.Models;
using Carnac.Utilities;
using Carnac.ViewModels;
using NSubstitute;
using Xunit;
using Action = System.Action;

namespace Carnac.Tests.ViewModels
{
    public class ShellViewModelTests
    {
        readonly ISettingsService settingsService = Substitute.For<ISettingsService>();
        readonly IScreenManager screenManager = Substitute.For<IScreenManager>();
        readonly ITimerFactory timerFactory = Substitute.For<ITimerFactory>();
        readonly IWindowManager windowManager = Substitute.For<IWindowManager>();
        private ShellViewModel viewModel;

        public ShellViewModelTests()
        {
            viewModel = new ShellViewModel(settingsService, screenManager, timerFactory, windowManager);
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


        [Fact]
        public void Constructor_WhenSettingsExists_PopulatesValue()
        {
            var settings = new Settings();
            settingsService.Get<Settings>("PopupSettings").Returns(settings);
            viewModel = new ShellViewModel(settingsService, screenManager, timerFactory, windowManager);

            Assert.Equal(settings, viewModel.Settings);
        }

        [Fact]
        public void Constructor_WhenSettingsDoNotExists_PopulatesNewValue()
        {
            Settings settings = null;
            settingsService.Get<Settings>("PopupSettings").Returns(settings);
            viewModel = new ShellViewModel(settingsService, screenManager, timerFactory, windowManager);

            Assert.NotNull(viewModel.Settings);
        }

    }
}

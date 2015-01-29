using Caliburn.Micro;
using Carnac.Logic;
using Carnac.Logic.Models;
using Carnac.Utilities;
using Carnac.ViewModels;
using NSubstitute;
using SettingsProviderNet;
using Xunit;
using Action = System.Action;

namespace Carnac.Tests.ViewModels
{
    public class ShellViewModelFacts
    {
        public class when_creating_the_new_viewmodel : SpecificationFor<ShellViewModel>
        {
            private readonly ISettingsProvider settingsService = Substitute.For<ISettingsProvider>();
            private readonly IScreenManager screenManager = Substitute.For<IScreenManager>();
            private readonly ITimerFactory timerFactory = Substitute.For<ITimerFactory>();
            private readonly IWindowManager windowManager = Substitute.For<IWindowManager>();
            private readonly IMessageProvider messageProvider = Substitute.For<IMessageProvider>();

            public override ShellViewModel Given()
            {
                return new ShellViewModel(settingsService, screenManager, timerFactory, windowManager, messageProvider);
            }

            public override void When()
            {
                // do nothing
            }

            [Fact]
            public void TimerFactory_Always_StartsATimer()
            {
                timerFactory.Received().Start(1000, Arg.Any<Action>());
            }

            [Fact]
            public void ScreenManager_Always_Fetches_CurrentScreens()
            {
                screenManager.Received().GetScreens();
            }

            [Fact]
            public void SettingsService_Always_Fetches_ExistingSettings()
            {
                settingsService.Received().GetSettings<PopupSettings>();
            }

            [Fact]
            public void Constructor_Always_ShowsKeyShowViewModel()
            {
                windowManager.ReceivedWithAnyArgs().ShowWindow(null);
            }
        }

        public class when_the_settings_file_is_defined : SpecificationFor<ShellViewModel>
        {
            private readonly ISettingsProvider settingsService = Substitute.For<ISettingsProvider>();
            private readonly IScreenManager screenManager = Substitute.For<IScreenManager>();
            private readonly ITimerFactory timerFactory = Substitute.For<ITimerFactory>();
            private readonly IWindowManager windowManager = Substitute.For<IWindowManager>();
            private readonly IMessageProvider messageProvider = Substitute.For<IMessageProvider>();
            private readonly PopupSettings popupSettings = new PopupSettings();

            public override ShellViewModel Given()
            {
                settingsService.GetSettings<PopupSettings>().Returns(popupSettings);
                return new ShellViewModel(settingsService, screenManager, timerFactory, windowManager, messageProvider);
            }

            public override void When()
            {
                // do nothing
            }

            [Fact]
            public void the_settings_file_is_the_existing_instance()
            {
                Assert.Equal(popupSettings, Subject.Settings);
            }
        }

        public class when_the_settings_file_is_not_defined : SpecificationFor<ShellViewModel>
        {
            private readonly ISettingsProvider settingsService = Substitute.For<ISettingsProvider>();
            private readonly IScreenManager screenManager = Substitute.For<IScreenManager>();
            private readonly ITimerFactory timerFactory = Substitute.For<ITimerFactory>();
            private readonly IWindowManager windowManager = Substitute.For<IWindowManager>();
            private readonly IMessageProvider messageProvider = Substitute.For<IMessageProvider>();
            private readonly PopupSettings popupSettings = Substitute.For<PopupSettings>();

            public override ShellViewModel Given()
            {
                settingsService.GetSettings<PopupSettings>().Returns(popupSettings);
                return new ShellViewModel(settingsService, screenManager, timerFactory, windowManager, messageProvider);
            }

            public override void When()
            {
                // do nothing
            }

            [Fact]
            public void the_settings_file_is_the_existing_instance()
            {
                Assert.NotNull(Subject.Settings);
            }
        }
    }
}

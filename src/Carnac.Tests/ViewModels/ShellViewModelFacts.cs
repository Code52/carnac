using Carnac.Logic;
using Carnac.Logic.Models;
using Carnac.ViewModels;
using NSubstitute;
using SettingsProviderNet;
using Xunit;

namespace Carnac.Tests.ViewModels
{
    public class ShellViewModelFacts
    {
        public class when_creating_the_new_viewmodel : SpecificationFor<ShellViewModel>
        {
            private readonly ISettingsProvider settingsService = Substitute.For<ISettingsProvider>();
            private readonly IScreenManager screenManager = Substitute.For<IScreenManager>();

            public override ShellViewModel Given()
            {
                return new ShellViewModel(settingsService, screenManager);
            }

            public override void When()
            {
                // do nothing
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
        }

        public class when_the_settings_file_is_defined : SpecificationFor<ShellViewModel>
        {
            private readonly ISettingsProvider settingsService = Substitute.For<ISettingsProvider>();
            private readonly IScreenManager screenManager = Substitute.For<IScreenManager>();
            private readonly PopupSettings popupSettings = new PopupSettings();

            public override ShellViewModel Given()
            {
                settingsService.GetSettings<PopupSettings>().Returns(popupSettings);
                return new ShellViewModel(settingsService, screenManager);
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
            private readonly PopupSettings popupSettings = Substitute.For<PopupSettings>();

            public override ShellViewModel Given()
            {
                settingsService.GetSettings<PopupSettings>().Returns(popupSettings);
                return new ShellViewModel(settingsService, screenManager);
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

## This is a fork

This repository is a fork from https://github.com/Code52/carnac

It also includes the fix for RegEx from https://github.com/DNIStream :

- [Resolved Code52#232 Fixed issue with RegEx process nmae filtering not…](https://github.com/DNIStream/carnac/commit/47e3333d7708e6620c66564b3ec81fbd0e2503c0)

The Repository adds support for filtering on sub-processes. For instance, when 'vim' is launched from cmd.exe or powershell.exe, the keyboard focus is processed by the shell (cmd.exe) rathern than vim.exe, so filtering doesn't work.

The "fix" is to treat shells named "cmd" and "powershell" in a special. That is, when one of these process receive the keyboard focus, their child-processes are also validated.

The code determines what is a shell using the RegEx "cmd|powershell", which can be modified in the C:\ProgramData\Carnac\PopupSettings.settings, although there's no UI for it.

There's no builds for this repository. You have to build it locally. Clone it and run:

    powershell .\build.ps1

Update: I created a release for testing: https://github.com/kasajian/carnac/releases

## Carnac the Magnificent Keyboard Utility

[![Join the chat at https://gitter.im/Code52/carnac](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Code52/carnac?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

A keyboard logging and presentation utility for presentations, screencasts, and to help you become a better keyboard user.

### Build Status

[![Build status](https://ci.appveyor.com/api/projects/status/qorhqwc2favf18r4?svg=true)](https://ci.appveyor.com/project/shiftkey/carnac)

### Installation

You can install the latest version of Carnac via [Chocolatey](https://chocolatey.org/):

```ps
cinst carnac
```

Alternatively, you can grab the latest zip file from [here](https://github.com/Code52/carnac/releases/latest), unpack it and run `Setup.exe`.

**Note:** Carnac requires .NET 4.5.2 to work - you can install that from [here](https://www.microsoft.com/en-au/download/details.aspx?id=42643) if you don't have it already.

### Updating

We use `Squirrel.Windows` to update your `carnac` application.

The application will check for updates in the background, if a new version has been released, it will automatically install the new version and once you restart `carnac` you will be up-to-date.

### Usage

**Enabling silent mode**

If you want to stop `Carnac` from recording certain key strokes, you can enter _silent mode_ by pressing `Ctrl+Alt+P`. To exit _silent mode_ you simply press `Ctrl+Alt+P` again.

### Contributing

**Getting started with Git and GitHub**

- [Setting up Git for Windows and connecting to GitHub](http://help.github.com/win-set-up-git/)
- [Forking a GitHub repository](http://help.github.com/fork-a-repo/)
- [The simple guide to GIT guide](http://rogerdudler.github.com/git-guide/)
- [Open an issue](https://github.com/Code52/carnac/issues) if you encounter a bug or have a suggestion for improvements/features

Once you're familiar with Git and GitHub, clone the repository and run the `.\build.cmd` script to compile the code and run all the unit tests. You can use this script to test your changes quickly.

### Resources

This blog series covers a series of refactorings which have recently happened in Carnac to make better use of Rx.
If you are learning Rx and want to be shown through Carnac's codebase then this blog series may help you.

[Part 1 - Refactoring the InterceptKeys class ](http://jake.ginnivan.net/blog/carnac-improvements/part-1/)  
[Part 2 - Refactoring the MessageProvider class](http://jake.ginnivan.net/blog/carnac-improvements/part-2/)  
[Part 3 - Introducing the MessageController class](http://jake.ginnivan.net/blog/carnac-improvements/part-3/)

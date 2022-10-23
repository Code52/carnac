## Carnac the Magnificent Keyboard Utility

[![Join the chat at https://gitter.im/Code52/carnac](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Code52/carnac?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

A keyboard logging and presentation utility for presentations, screencasts, and to help you become a better keyboard user.

### Build Status
[![Build status](https://ci.appveyor.com/api/projects/status/qorhqwc2favf18r4?svg=true)](https://ci.appveyor.com/project/shiftkey/carnac)

### Installation

You can install the latest version of Carnac via [Chocolatey](https://chocolatey.org/):

```ps
choco install carnac
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

 * [Setting up Git for Windows and connecting to GitHub](http://help.github.com/win-set-up-git/)
 * [Forking a GitHub repository](http://help.github.com/fork-a-repo/)
 * [The simple guide to GIT guide](http://rogerdudler.github.com/git-guide/)
 * [Open an issue](https://github.com/Code52/carnac/issues) if you encounter a bug or have a suggestion for improvements/features

Once you're familiar with Git and GitHub, clone the repository and run the ```.\build.cmd``` script to compile the code and run all the unit tests. You can use this script to test your changes quickly.

### Resources
This blog series covers a series of refactorings which have recently happened in Carnac to make better use of Rx.
If you are learning Rx and want to be shown through Carnac's codebase then this blog series may help you.

[Part 1 - Refactoring the InterceptKeys class ](http://jake.ginnivan.net/blog/carnac-improvements/part-1/)  
[Part 2 - Refactoring the MessageProvider class](http://jake.ginnivan.net/blog/carnac-improvements/part-2/)  
[Part 3 - Introducing the MessageController class](http://jake.ginnivan.net/blog/carnac-improvements/part-3/)  


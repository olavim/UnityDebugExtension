# Unity Debug Extension

A Visual Studio extension for debugging Unity games.

## Features

Adds `Run Command and Attach Unity Debugger` command under the `Debug` menu.

The command is quite configurable. Choose your behaviour in `Tools -> Options -> UnityDebugExtension`.

You can:
- Configure where the debugger connects (IP-address and port).
- Configure an optional executable to run. The debugger will be attached while this script runs (with a configurable delay). This enables you to, for example, run your game and attach the debugger with a single click.
- Configure an optional setup executable to run before the main executable is ran and the debugger is attached. This enabled you to, for example, run a build script before the main executable is ran and debugger is attached.

## Dependencies

- [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/)
- [Visual Studio Tools for Unity](https://learn.microsoft.com/en-us/visualstudio/gamedev/unity/get-started/visual-studio-tools-for-unity)
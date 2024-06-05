## GameLibsMaker - What Does It Do?
GameLibsMaker is specifically tailored for Unity Engine games using the BepInEx framework. Its core function is to extract the assemblies used by a Unity game, then apply a "publicize" and "strip" process to these assemblies.

The Publicize process transforms all members and types to be public, which allows the mods makers unrestricted access for view and alteration.

The Strip process removes the method bodies from the compiled code. This leaves just the metadata and APIs but rids of the implementation details, ensuring an efficient size and the fact that no copyrighted code is distributed.

These treated assemblies are then moved into an output directory for easy access.

Along with the assemblies, a `GameLibs.props` file is generated. This file maintains references to all imported assemblies, serving as a convenient link to the game's libraries.

One of the main benefits is that this `GameLibs.props` file can be easily imported into a BepInEx plugin .csproj file. This gives the plugin project instant and easy access to the game's assemblies, making modding even simpler and more efficient.

> BepInEx is only required for games build with il2Cpp

## GameLibsMaker - Configuration Guide

There are three ways to configure the GameLibsMaker program: using command-line arguments, creating a configuration file, and interacting via the console.

### Command Line Arguments
You can run the program by passing it two command-line arguments:
1. The game directory
2. The output directory

Here is an example of how to run the program with command-line arguments:
```shell
GameLibsMaker.exe "C:\path\to\game\directory" "C:\path\to\output\directory"
```

### Configuration File
If no arguments are passed to the command line, the program will check for the existence of a configuration file in the current directory. The configuration file should be named `.GameLibsMaker`.

This file should contain the game directory path on the first line and the output directory path on the second line. For example:
```text
C:\path\to\game\directory
C:\path\to\output\directory
```
If a valid configuration file is found, the program will use it to get the game directory and output directory paths.

### Console Interaction
If no command-line arguments are provided and no configuration file is found, the program will ask you to enter the game directory and output directory via console interaction. Please follow the on-screen instructions to enter the appropriate paths.

For any questions or problems you may encounter while configuring GameLibsMaker, don't hesitate to ask for help in issues.
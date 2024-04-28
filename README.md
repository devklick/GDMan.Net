<h1 align="center">
    GDMan
</h1>

<p align="center">
    A cross-platform CLI tool for managing versions of the Godot game engine.
</p>
<br/>
<br/>
<br/>
<br/>

> [!WARNING]
> This project is WIP

## Motivation

While working with Godot on Linux, I find it a chore having to download new versions,
extract them, rename files and/or create a symbolic link, and having to update
desktop launchers & config files that point to a previous version.
I wanted a way to simply run a command and have all this happen behind the scenes.
And that's exactly what GDMan does.

## Installing GDMan

- Download the [latest release](https://github.com/devklick/GDMan/releases/latest)
  for your operating system and extract it to a folder of your choice, e.g. `~/.local/share/gdman/app/`
- Update your path, adding your new folder to it. In the above example, we'd add `~/.local/share/gdman/app/` to the PATH.

That's it for now. But after we use GDMan to install a version of Godot, we need to then also:

- Update your path, adding

## Basic CLI

```
./GDMan.Cli --help

Command line application for managing versions of Godot

Usage: gdman [command] [command-options]

Commands:
  install | i - Installs the specified version of Godot
  list | l - Lists the versions of Godot currently installed on the system
  current | c - Prints the currently-active version of Godot
```

## Install Command

```
./GDMan.Cli install --help

Supported arguments:
  --latest, -l
    Whether or not the latest version should be fetched. If used in conjunction with the Version argument and multiple matching versions are found, the latest of the matches will be used.
    Type: Boolean Flag (Present (True) or Omitted (False))
    Default: False

  --version, -v
    The version to use, e.g. 1.2.3. Any valid semver range is supported
    Type: String (Valid sematic version range)

  --platform, -p
    The platform or operating system to find a version for
    Type: Enum
      Windows, win, w
      Linux, lin, l
      MacOS, mac, osx, m
    Default: Linux

  --architecture, -a
    The system architecture to find a version for
    Type: Enum
      Arm32
      Arm64
      X86
      X64
    Default: X64

  --flavour, -f
    The "flavour" (for lack of a better name) of version to use
    Type: Enum
      Standard: The standard version of Godot which uses GDScript
      Mono: The version of Godot required to use .Net C#
    Default: Standard

  --directory, -d
    The directory where the downloaded version should be installed
    Type: String (Anything)
    Default: /home/user/.local/share/gdman/versions

  --verbose, -vl
    Whether or not extensive information should be logged
    Type: Boolean Flag (Present (True) or Omitted (False))
    Default: False

  --help, -h
    Prints the help information to the console
    Type: Boolean Flag (Present (True) or Omitted (False))
    Default: False
```

## List Command

```
./GDMan.Cli list --help

Supported arguments:
  --verbose, -vl
    Whether or not extensive information should be logged
    Type: Boolean Flag (Present (True) or Omitted (False))
    Default: False

  --help, -h
    Prints the help information to the console
    Type: Boolean Flag (Present (True) or Omitted (False))
    Default: False
```

## Current Command

```
./GDMan.Cli current --help

Supported arguments:
  --verbose, -vl
    Whether or not extensive information should be logged
    Type: Boolean Flag (Present (True) or Omitted (False))
    Default: False

  --help, -h
    Prints the help information to the console
    Type: Boolean Flag (Present (True) or Omitted (False))
    Default: False
```

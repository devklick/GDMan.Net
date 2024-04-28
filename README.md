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

And that's exactly what GDMan does!

## Installing GDMan

GDMan is cross-platform and runs in an entirely self-contained folder.
That means that basically all you have to is download it and run it.

- Download the [latest release](https://github.com/devklick/GDMan/releases/latest)
  for your operating system and extract it to a folder of your choice, e.g. `~/gdman`.
- Update your path, adding your new folder to it. In the above example, we'd add `~/gdman` to the PATH.

This will allow you to invoke two commands from your terminal:

- `gdman` - for managing versions of Godot
- `godot` - to run the currently-active version of Godot
  <br/>
  **Note that this will not exist until you first install a version of Godot via GDMan**

## Installing versions of Godot

To install a version of Godot, you run the `install` command.

### Installing latest version

In most cases, you probably want to install the latest version of Godot, so you
would pass in the `--latest` flag.

```
gdman install --latest
```

> [!NOTE]
> The shorthard option for `--latest` is `-l`

### Installing an exact version of Godot

If you need more precision to install an exact version of godot, you can pass
in the `--version` flag with any valid semver value.

```
gdman install --version 4.2.2
```

> The shorthard option for `--version` is `-v`

### Installing mono version

If you need to install the Mono version of Godot, you can do so by passing in the `--flavour` flag option with a value of `mono`.

```
gdman install --latest --flavour mono
```

Alternatively, you can set the `GDMAN_TARGET_FLAVOUR` environment variable to `mono`.
Doing so means that Mono will be the default flavour whenever the install command runs,
and to override this with the non-mono version you would override it with the `standard` option.

```
gdman install --latest --flavour standard
```

> The shorthard option for `--flavour` is `-f`

### Installing the correct version for your platform

In most cases, GDMan will be able to determine what platform you are running on (Windows, Linux or macOS),
however you can also specify the platform when running the `install` command.

```
gdman install --latest --platform mac
```

You can override the default behaviour by setting the `GDMAN_TARGET_PLATFORM` environment variable
with the desired platform. Then, whenever you run the `install` command, the value from the
environment variable will be used, you wont have to specify the `--platform` arg, and
the app will not attempt to detect the platform you are running on.

> The shorthard option for `--platform` is `-p`

### Installing the correct version for your system architecture

In most cases, GDMan will be able to determine what architecture your system uses (e.g x86, x64 etc).
However, you can specify the `--architecture` option to specify exactly which archiecture to use.

```
gdman install --latest --architecture x86
```

You can override the default behaviour by setting the `GDMAN_TARGET_ARCHITECTURE` environment variable
with the desired architecture. Then, whenever you run the `install` command, the value from the
environment variable will be used, you wont have to specify the `--architecture` arg, and
the app will not attempt to detect the system architecture you are running on.

> The shorthard option for `--architecture` is `-a`

## List all installed versions

To view all of the versions of Godot that are installed on your system, run the `list` command.

```
gdman list
```

> This will only list version installed by GDMan

## List active version

To see the version of Godot that is currently active, run the `current` command

```
gdman current
```

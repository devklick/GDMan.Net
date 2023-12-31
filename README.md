# GDMan

Temp notes




```
GDMan (v1.2.3)

Command line application for managing versions of Godot

Usage: gdman [command] [command-options]

Commands:
  install - Installs the specified version of Godot
  use     - Sets an already installed version of Godot as the current active version

Run 'gdman [command] --help' for more information on a command.






GDMan Install

Usage: gdman install [command-options]

Options:
  --help | -h
    Shows this help information

  --latest | -l
    Whether or not the latest version should be fetched. If used in conjunction with the Version argument and multiple matching versions are found, the latest of the matches will be used.
    Type: Boolean Flag (Present (True) or Omitted (False))
    Default: False

  --version | -v
    The version to use, e.g. 1.2.3. Any valid semver range is supported
    Type: String (Valid sematic version range)

  --platform | -p
    The platform or operating system to find a version for
    Type: Enum
      Windows, win, w
      Linux, lin, l
      MacOS, mac, osx, m
    Default: Windows

  --architecture | -a
    The system architecture to find a version for
    Type: Enum
      Arm32
      Arm64
      X86
      X64
    Default: X64

  --flavour | -f
    The "flavour" (for lack of a better name) of version to use
    Type: Enum
      Standard: The standard version of Godot which uses GDScript
      Mono: The version of Godot required to use .Net C#
    Default: Standard

  --directory | -d
    The directory where the downloaded version should be installed
    Type: String (Anything)
    Default: C:\users\user\Local Settings\Application Data\.gdman\versions
```



App Flow...

	1. Accept request to download/install a version
	2. If exact version requested, check if that version is installed
		? Yes, Update symlink to point to this version - done
		: No, proceed to download and install
	3. Find matching version via github
		? Found, proceed to download/installation
		: Not found, report error to user
	4. Is the version found already installed?
		? Yes, Update symlink to point to this version - done
		: No, proceed to download and install
	5. Download to versions folder
	6. Extract to versions folder & delete zip
	7. Update symlink to point to new version


Structure:

    <local-app-data>/.gdman/
        bin/
            godot <- symlink to a specific version
            gdman <- symlink to this apps executable
        versions/ <- The godot versions that have been downloaded
            godot_1.2.3-stable/
            godot_3.4.5-alpha/
        GDMan/
            ...contents of this app
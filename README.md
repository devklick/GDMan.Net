# GDMan

Temp notes

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
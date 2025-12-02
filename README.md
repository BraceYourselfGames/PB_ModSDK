![t0_splash.png](/ModWindowImages/t0_splash.png)

## Overview

Phantom Brigade Mod SDK is a mod development suite created in collaboration with our valued community. Based on the tools we used to develop the game, it is a free Unity project that includes:
- Mod project manager: simplifies mod metadata setup, manages mod source files and helps you manage different mod tools. Facilitates export of completed mods to the local user folder, to a distributable archive or to Steam Workshop.
- Config editors: powerful inspectors putting every database underpinning the game at your fingertips. Modify everything from game balance to enemy squad compositions and from mission logic to combat abilities. Take advantage of data validation, reactive inspectors, value dropdowns and specialized editors for complex data types like curves to create what would take hours to set up through manual text editing in minutes. Preview kitbashed weapons, put together bosses with complex behaviors, create new status effects and more.
- Level editor: enables you to create new combat arenas for new and existing missions. Our voxel-based level system and an array of specialized editing tools allow you to quickly create intricate architecture and complex natural terrains.
- Asset builder: enables you to include custom 3D assets into your mods, unlocking modding of weapons and armor with custom models.

While Phantom Brigade supported mods for a while and is underpinned by easily editable YAML files, modding was by no means an easy process. It is time consuming to define complex data through a text editor, easy to make mistakes and hard to visualize what you're building. A small balance tweak can be reasonably straightforward, but building an entire scenario or a missile guidance program is anything but. With the release of this SDK, we hope to change that.

We can't wait to see what you create!

## Installation

Quick setup overview:
- Download & install [Unity Hub](https://unity.com/download)
- Open the download page for [Unity 2020.3.49f1 LTS](https://unity.com/releases/editor/whats-new/2020.3.49f1). Click **Install** on top of the page to begin installation through Unity Hub. If this doesn't work, try downloading a Windows installer through links at the bottom of that page. It is very important to download the exact version of Unity Editor to avoid problems in the SDK.
- Install git and make sure it is in your `PATH`. If you're using a GUI client, you might not have standalone git.exe installed or might not have `PATH` set up. [Download Git here](https://www.git-scm.com/download).
- Download the project from this repository. We strongly recommend downloading (cloning) through Git to facilitate easy updating.
- In Unity Hub, navigate to the **Projects** tab and choose **Open**, selecting the folder with the project. Wait for the project to import.


> Pick a drive with at least 10Gb of free space and select a location with a short path, such as **C:/Work/Unity Projects/PB_ModSDK**. The core repository is a fairly small download, but an optional asset package totals around 4Gb. Full import of the project with the optional asset package installed might create up to 5Gb of temporary files, hence our recommendation to reserve up to 10Gb.

> If you're not familiar with Git and are unsure how to download the SDK project from GitHub, try installing the [GitHub Desktop](https://desktop.github.com/) client and [following this tutorial](https://docs.github.com/en/desktop/adding-and-cloning-repositories/cloning-a-repository-from-github-to-github-desktop). You can find the list alternative Windows GUI clients [here](https://www.git-scm.com/download/gui/windows).

## Getting started

Once the project is open, click **PB Mod SDK/Getting Started** in the top menu of the Unity Editor window to get additional guidance. The Getting Started window is the central point of the SDK: it can help you check the status of different components, navigate to important links and scenes and guide you through creating your own mods.

![desc_getting_started.png](/ModWindowImages/repo/desc_getting_started.png)

Click the Tutorials button at the bottom of the Getting Started window to learn more information. We strongly recommend using the window to check how to:
- Open the correct scene
- Find the mod project manager and create a simple config mod
- Install the optional asset package to unlock item previews & level editing
- Export your mods through the Steam Workshop
- And more!

## Optional assets

We separated some assets into an optional download to keep the repository lightweight and easy to update. These include 3D models of mech items, levels, textures and some other art assets. These files are not required for most of the tools included in the SDK, but we recommend downloading and installing the optional asset package to unlock every feature. This includes ability to edit levels and ability to display items in the 3D viewport.

Follow the instructions in the Getting Started window to install this optional asset package. To save time, we recommend [downloading the file](https://cdn.braceyourselfgames.com/PB/PB_ModSDK_AssetPackage_V20A.unitypackage) in the background while you set up the main project and follow the initial tutorials.

## Documentation

Outside of this repository and the embedded tutorials, we recommend checking a few additional pages. General info:
- [Modding intro](https://wiki.braceyourselfgames.com/en/PhantomBrigade/Modding)
- [Detailed modding overview](https://wiki.braceyourselfgames.com/en/PhantomBrigade/Modding/ModSystem)
- [Modding guidelines](https://wiki.braceyourselfgames.com/en/PhantomBrigade/Modding/ModGuidelines)
- [SDK overview mirror](https://wiki.braceyourselfgames.com/en/PhantomBrigade/Modding/ModSDK)

Change logs:
- [Mod system changelog](https://wiki.braceyourselfgames.com/en/PhantomBrigade/Modding/ModSystemChanges)
- [Game changelog](https://braceyourselfgames.com/phantom-brigade/updates/)

Tutorials:
- [Creating mech armor](https://wiki.braceyourselfgames.com/en/PhantomBrigade/Modding/official-mech-armor-modding)
- [Creating weapons](https://wiki.braceyourselfgames.com/en/PhantomBrigade/Modding/official-custom-weapon-assets)

If you get stuck or experience a bug, please don't hesitate to ask questions in the #phantom-modding channel of [the official Discord server](https://discord.com/invite/braceyourselfgames).

## Dependencies

- [Odin Inspector](https://odininspector.com): Used for all custom inspectors and drawers. This dependency is licensed for redistribution only as a part of the Phantom Brigade modding SDK and can not be used in other Unity projects. A standalone version of this library can be acquired [here](https://odininspector.com/pricing).
- YAML serialization used in this project depends on heavily modified old release of YAML.NET. The original project is available [here](https://github.com/aaubry/YamlDotNet). We do not recommend upgrading this dependency.
- [Steamworks.NET]([https://steamworks.github.io/](https://github.com/rlabrecque/Steamworks.NET)): Used for Steam Workshop uploads. Important: This is an external package downloaded through Git and your project might not import if you do not have Git installed.

## Contributions

If you're interested in helping improve this project, please check out [contribution guidelines](CONTRIBUTING.md) for more details on the process! Whether it's a bug report, a feature request, a suggestion about the documentation or a pull request, we're looking forward to hearing from you!

## License

This project is licensed under Commons Clause + MIT license. See the [license file](LICENSE.md) for details.

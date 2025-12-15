![t0_splash.png](/ModWindowImages/t0_splash.png)

## Overview

Phantom Brigade Mod SDK is a mod development suite created in collaboration with our valued community. Based on the tools we used to develop the game, it is a free Unity project that includes everything you need to develop mods of any complexity.

![t0_ui_sample.png](/ModWindowImages/t0_ui_sample.png)

While Phantom Brigade is underpinned by easily editable YAML files and supports modding without the SDK, it can be challenging to mod many aspects of the game without developer tools. Defining complex data through a text editor can be time consuming and error prone, and it can be hard to visualize what you're building. A small balance tweak can be reasonably straightforward, but making an entire scenario or a missile guidance program is anything but. We hope that releasing our internal tools through this SDK will help everyone interested in tinkering with the game. Here are just some of the tools included with the SDK:

- Mod project manager: simplifies mod metadata setup, source file management and export to local folders or Steam Workshop.
- Data editors: powerful tools putting every database underpinning the game at your fingertips. Modify everything from game balance to enemy squad compositions and from mission logic to combat abilities. Take advantage of data validation, reactive inspectors, value dropdowns and specialized editors for complex data types like curves to create what would take hours to set up through manual text editing in minutes. Preview kitbashed weapons, put together bosses with complex behaviors, create new landscapes and status effects and more.
- Level editor: enables you to create new combat arenas for new and existing missions. Our voxel-based level system and an array of specialized editing tools allow you to quickly create intricate architecture and complex natural terrains.

## Getting started

Please refer to the wiki for detailed installation instructions: [Installing the SDK](../../wiki/Installing-the-SDK). Check the [Mod SDK Wiki](https://github.com/BraceYourselfGames/PB_ModSDK/wiki) for additional information about the modding system, SDK setup and a variety of tutorials. At a minimum, consider checking the following articles:
- [Installing the optional assets](../../wiki/Installing-the-optional-assets)
- [Mod projects](../../wiki/Mod-projects)
- [Mod system overview](../../wiki/Mod-system-overview)

Once the SDK is set up and the Unity Project is open, click **PB Mod SDK/Getting Started** in the top menu of the Unity Editor window to get additional guidance. The Getting Started window is the central point of the SDK: it can help you check the status of different components, navigate to important links and scenes and guide you through creating your own mods.

![desc_getting_started.png](/ModWindowImages/repo/desc_getting_started.png)

If you get stuck or experience a bug, please don't hesitate to ask questions in the `#phantom-modding` channel of [the official Discord server](https://discord.com/invite/braceyourselfgames).

## Dependencies

- The current SDK release is intended to be used with Phantom Brigade 2.1.
- [Odin Inspector](https://odininspector.com): Used for all custom inspectors and drawers. This dependency is licensed for redistribution only as a part of the Phantom Brigade modding SDK and can not be used in other Unity projects. A standalone version of this library can be acquired [here](https://odininspector.com/pricing).
- YAML serialization used in this project depends on heavily modified old release of YAML.NET. The original project is available [here](https://github.com/aaubry/YamlDotNet). We do not recommend upgrading this dependency.
- [Steamworks.NET]([https://steamworks.github.io/](https://github.com/rlabrecque/Steamworks.NET)): Used for Steam Workshop uploads. Important: This is an external package downloaded through Git and your project might not import if you do not have Git installed.

## Contributions

If you're interested in helping improve this project, please check out [contribution guidelines](CONTRIBUTING.md) for more details on the process! Whether it's a bug report, a feature request, a suggestion about the documentation or a pull request, we're looking forward to hearing from you!

## License

This project is licensed under Commons Clause + MIT license. See the [license file](LICENSE.md) for details.

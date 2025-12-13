![t0_splash.png](/ModWindowImages/t0_splash.png)

## Overview

Phantom Brigade Mod SDK is a mod development suite created in collaboration with our valued community. Based on the tools we used to develop the game, it is a free Unity project that includes:

- Mod project manager: simplifies mod metadata setup, manages mod source files and helps you manage different mod tools. Facilitates export of completed mods to the local user folder, to a distributable archive or to Steam Workshop.
- Config editors: powerful inspectors putting every database underpinning the game at your fingertips. Modify everything from game balance to enemy squad compositions and from mission logic to combat abilities. Take advantage of data validation, reactive inspectors, value dropdowns and specialized editors for complex data types like curves to create what would take hours to set up through manual text editing in minutes. Preview kitbashed weapons, put together bosses with complex behaviors, create new status effects and more.
- Level editor: enables you to create new combat arenas for new and existing missions. Our voxel-based level system and an array of specialized editing tools allow you to quickly create intricate architecture and complex natural terrains.
- Asset builder: enables you to include custom 3D assets into your mods, unlocking modding of weapons and armor with custom models.

While Phantom Brigade is underpinned by easily editable YAML files and supports modding without the SDK, it can be challenging to mod many aspects of the game without developer tools. Defining complex data through a text editor can be time consuming and error prone, and it can be hard to visualize what you're building. A small balance tweak can be reasonably straightforward, but making an entire scenario or a missile guidance program is anything but. We hope that releasing our internal tools through this SDK will help everyone interested in tinkering with the game.

If you get stuck or experience a bug, please don't hesitate to ask questions in the #phantom-modding channel of the official Discord server. We can't wait to see what you create!

## Getting started

Please refer to this wiki page for detailed installation instructions: [Installing the SDK](../../wiki/Installing-the-SDK)

Once the SDK is set up and the Unity Project is open, click **PB Mod SDK/Getting Started** in the top menu of the Unity Editor window to get additional guidance. The Getting Started window is the central point of the SDK: it can help you check the status of different components, navigate to important links and scenes and guide you through creating your own mods.

![desc_getting_started.png](/ModWindowImages/repo/desc_getting_started.png)

Click the Tutorials button at the bottom of the Getting Started window to learn more information. We strongly recommend using the window to check how to:
- Open the correct scene
- Find the mod project manager and create a simple config mod
- Install the optional asset package to unlock item previews & level editing
- Export your mods through the Steam Workshop
- And more!

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

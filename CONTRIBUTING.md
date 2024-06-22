# Contributing to the SDK

We're glad to hear you're interested in helping the project! Before we talk about the different ways you could do that, let's cover the basics:

1. **When in doubt, open an issue** - For almost every kind of contribution, the first step is opening an issue. Even if you think you already know what the solution is, writing down a description of the problem you're trying to solve will help everyone get context when they review your pull request. If it's truly a trivial change (e.g. spelling error), you can skip this step -- but as the subject says, when in doubt, [open an issue](https://github.com/BraceYourselfGames/PB_ModSDK/issues/new/choose).
2. **Only submit your own work**  (or work you have sufficient rights to submit) - Please make sure that any code or documentation you submit is your work or you have the rights to submit. We respect the intellectual property rights of others, and as part of contributing, we'll ask you to sign your contribution with a "Developer Certificate of Origin" (DCO) that states you have the rights to submit this work and you understand we'll use your contribution. There's more information about this topic in the [DCO section](#developer-certificate-of-origin).

# Ways to Contribute

## Bug Reports

To help us understand what's going on with a potential bug, we first want to make sure you're working from the latest version. Please make sure you're testing against the latest version. Verify that you're on the latest commit of the main branch.

Once you've confirmed that the bug still exists in the latest version, you'll want to check the bug is not something we already know about. A good way to figure this out is to search for your bug on the [open issues GitHub page](https://github.com/BraceYourselfGames/PB_ModSDK/issues).

If you've upgraded to the latest version and you can't find it in our open issues list, then you'll need to tell us how to reproduce it. Please provide as much information as you can: your mod configuration, steps you made since session startup etc.

## Feature Requests

If you have an idea for improving the SDK, we'd love to hear about it. We track feature requests using GitHub, so please feel free to open an issue which describes the feature you would like to see, why you need it, and how it should work. After opening an issue, the fastest way to see your change made is to open a pull request following the requested changes you detailed in your issue. You can learn more about opening a pull request in the [contributing code section](#contributing-code).

## Documentation Changes

The project relies on two kinds of documentation.

1. **General documentation & tutorials** - Located on the [Brace Yourself Games wiki](https://wiki.braceyourselfgames.com/en/PhantomBrigade/Modding/). We invite you to contribute to the wiki if you'd like to create a traditional long-form tutorial or documentation page with embedded media and rich formatting.
2. **Interactive tutorials** - Accessed through the SDK project, under the Tutorials section of the Getting Started window. These tutorials have fewer formatting options, but support conditional logic and interactive options, making them a good fit for tutorials asking users to perform steps such as complex data input or navigation to specific objects. The interactive tutorials are data driven through another database and can be contributed through pull requests.

## Contributing Code

As with other types of contributions, the first step is to [open an issue](https://github.com/BraceYourselfGames/PB_ModSDK/issues/new/choose). Opening an issue before you make changes makes sure that someone else isn't already working on that particular problem. It also lets us all work together to find the right approach before you spend a bunch of time on a PR.

When opening a pull request, follow the template and make sure you complete all the required steps from the template:
- Test the SDK with the changes in place: confirm there are no regressions.*
- Check that your contribution is made under the terms of the MIT license.
- Check that your commit description includes a line confirming agreement to the Developer Certificate of Origin.

When testing for regressions, pay attention to the following:
- Confirm that standard Editor compilation succeeds without errors or new warnings. Press Ctrl+R if you're unsure if the Editor has been recompiled after your last change.
- If you have modified any expressions or functions used by Odin Inspector attributes, make sure to test the inspectors. Select a mod with config editing support, enter the config editing mode and try to make use of the affected properties and methods. Some errors might not become apparrent until you do so: for example, `ValueDropdown` functions only evaluate once you click a dropdown, and drawing expressions only evaluate when a connected property is first seen by a user.
- Export a mod containing Asset Bundles and confirm there were no compilation errors. Exporting Asset Bundles requires the project to be recompiled in play mode (to make generated assets compatible with the game), which can trigger compilation errors invisible in Editor mode.
  - The most common kind of an error relates to usage of Unity Editor APIs or Editor-only properties in code not wrapped with the Editor preprocessor directive.
  - This includes Unity Editor APIs, any Editor specific namespaces of Odin Inspector and most of the properties under `DataContainerModData` and `DataManagerMod`
  - To fix this issue, wrap the offending code in a `#if UNITY_EDITOR` directive. This will omit it from play mode compilation.
  - Always make sure that conditional compilation directives don't cause new compile errors: for example, a bool-returning method with the wrapped body must still return something in Play mode and should contain an `#else` block.
  - Make sure Odin Inspector arguments do not rely on Editor-only constants. If you use `ShowIf (nameof(myProperty))` instead of `ShowIf ("myProperty")`, make sure `myProperty` is not located in a UNITY_EDITOR block or use the string argument.
 
List of namespaces that must never be used without the `#if UNITY_EDITOR` directive:
```
Sirenix.OdinInspector.Editor;
Sirenix.Utilities.Editor;
Unity.EditorCoroutines.Editor;
UnityEditor;
```

## Developer Certificate of Origin

The Phantom Brigade Mod SDK is a source available project. You are free to use, redistribute or modify/fork this Software for any purpose (subject to license conditions of third party dependencies). However, we'd like to prohibit commercial use such as selling of any product or service substantially derived from this Software. To this end, we add the [“Commons Clause”](https://commonsclause.com/) license condition to the base MIT license.

We respect intellectual property rights of others and we want to make sure all incoming contributions are correctly attributed and licensed. A Developer Certificate of Origin (DCO) is a lightweight mechanism to do that.

The DCO is a declaration attached to every contribution made by every developer. In the commit message of the contribution, the developer simply adds a `Signed-off-by` statement and thereby agrees to the DCO, which you can find below or at [DeveloperCertificate.org](http://developercertificate.org/).

```
Developer's Certificate of Origin 1.1

By making a contribution to this project, I certify that:

(a) The contribution was created in whole or in part by me and I
    have the right to submit it under the open source license
    indicated in the file; or

(b) The contribution is based upon previous work that, to the
    best of my knowledge, is covered under an appropriate open
    source license and I have the right under that license to
    submit that work with modifications, whether created in whole
    or in part by me, under the same open source license (unless
    I am permitted to submit under a different license), as
    Indicated in the file; or

(c) The contribution was provided directly to me by some other
    person who certified (a), (b) or (c) and I have not modified
    it.

(d) I understand and agree that this project and the contribution
    are public and that a record of the contribution (including
    all personal information I submit with it, including my
    sign-off) is maintained indefinitely and may be redistributed
    consistent with this project or the open source license(s)
    involved.
```

We require that every contribution to the Phantom Brigade Mod SDK is signed with a Developer Certificate of Origin. Each commit must include a DCO which looks like this:

```
Signed-off-by: Jane Smith <jane.smith@email.com>
```

You may type this line on your own when writing your commit messages. However, if your user.name and user.email are set in your git configs, you can use `-s` or `--signoff` to add the `Signed-off-by` line to the end of the commit message. 

We also recommend verifying your commits via GPG or SSH signing to protect your identity. Please refer to [this documentation page](https://docs.github.com/en/authentication/managing-commit-signature-verification) for reference.

# Build and Upload - Build a Unity project and upload it to different platforms

Builds the project with a Unity build profile and uploads the result from a staged copy, so folders that must not be shipped stay out of the upload.

The version of an upload is the `PlayerSettings.bundleVersion` the build was made with. It is read by a build processor (`BuildVersionCapture`, order 1000), after the processors with the default order have run, so a tool that sets the version for the build only and restores it afterwards (e.g. `SeweralIdeas.ProjectVersion`) is respected.

## Setup

1. Make a build profile for each platform (**File > Build Profiles**), or skip it to build the active target with the scenes of the build settings.
2. **Assets > Create > SeweralIdeas > Upload Profile**, one per platform and destination (web to itch, Windows to itch, Windows to Steam, ...).
3. In the profile, pick the build profile and an uploader, and fill in its fields.
4. **Tools > Build and Upload...**, pick the profile.

Per-machine settings (the path to butler or steamcmd, the Steam account) are not stored in the profile asset but in EditorPrefs; they are shown under "This machine" in the profile's inspector. The window only picks the profile and runs it.

## Output

| Folder | Content |
|--------|---------|
| `Builds/<profile>/Build` | what the build makes |
| `Builds/<profile>/Upload` | the copy that is uploaded, without the excluded names |
| `Builds/<profile>/Steam` | the generated build script of the Steam uploader |

Add `/Builds/` to the project's `.gitignore`.

## Archiving builds

Tick **Archive build** in the window and pick a folder: after every successful build, a copy of `Build` is stored there as `<profile>_<version>_<yyyy-MM-dd_HH-mm-ss>`. It is made from the build, not from the upload copy, so it keeps the debug symbols that the upload leaves out. Only the profile's **Exclude Always** names are left out of it. The option and the folder are kept per machine.

## Excluding files from the upload

Two lists on the profile, of names of files and folders (`*` is any run of characters, case is ignored):

- **Exclude Always**: never copied, to neither the upload nor the archive. Defaults: `*_BurstDebugInformation_DoNotShip`, `*_BackUpThisFolder_ButDontShipItWithYourGame`.
- **Exclude From Upload**: left out of the upload only, kept in the archive. For the debug symbols, e.g. `*.pdb`. Empty by default.

## Uploaders

- **itch.io**: needs [butler](https://itch.io/docs/butler/), logged in once with `butler login` (or `BUTLER_API_KEY`). Target is `user/game`, the channel (e.g. `windows-test`) is created by the first push.
- **Steam**: needs steamcmd (Steamworks SDK ContentBuilder). App id, depot id and an optional branch to set live. The first login asks for the password and the Steam Guard code in the console window.

A new destination is a subclass of `BuildUploader`; it shows up in the profile's dropdown.

# Changelog

All notable changes to this project will be documented in this file.

## [0.1.0] - 28.10.2026

### Added
- Initial release
- `UploadProfile` asset: a Unity build profile (or the active build target), the files to leave out of the upload, and an uploader
- Window at **Tools > Build and Upload...**: Build and Upload, Build Only, Upload Last Build, Upload Folder. The profile is edited in the inspector, where the machine settings of its uploader are too
- Archive build (window option): a copy of each build, with the debug symbols the upload leaves out, in a folder of your choice, named by profile, version and time
- Staging: the upload is made from a copy of the build without the excluded names. The profile has **Exclude Always** (Unity's `*_DoNotShip` and backup folders, left out of the upload and the archive) and **Exclude From Upload** (debug symbols, kept in the archive)
- `ItchUploader` (butler) and `SteamUploader` (steamcmd, with a generated build script), both run in a console window of their own
- The version of the build is read in the build process (`BuildVersionCapture`), after the processors that might set it.
### Archival notice
How come this repository never had a README...

This repository will be archived for preservation. I consider the game mostly complete. There were some weird decisions made, like using 3 tilemaps for a game that should only use 2, wonky physics or portals being one-sided.
Working on this game also turned a tool I made for game jams into a complete mess. I also tried porting the game in Rust, only to be met by engine limitations and university work that halted development.
I still want to work on this game again someday, but I will be starting again from scratch, in C or Rust with a completely custom engine.

# Upfall

A tiny precision platformer about switching between dark and light worlds. Made in one week using [Brocco](https://github.com/Raoul1808/Brocco) for the first edition of the 1-BIT game jam.

## Building

Pre-requisites:
- .NET 7.0 SDK or later

Steps:
1. Clone this repo and its submodules with either:
  - `git clone --recursive <url>`
  - `git clone <url>`, then cd in the repo's directory and run `git submodule update --init --recursive`
2. Build the game (`dotnet build` or the Build button in your favourite IDE)
3. Copy the contents of the `Brocco/deps/<your target architecture here>` in the target build directory
4. Run the game

OR

1. Clone (see above)
2. Run one of the scripts `compile-release-<target os>.sh` with bash (on Windows, use `Git Bash` or `MSYS2`)

## License

This project is licensed under the [MIT License](LICENSE)

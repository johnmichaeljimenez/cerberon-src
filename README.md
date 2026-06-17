# Cerberon Engine

[![Build and Push to Itch.io](https://github.com/johnmichaeljimenez/cerberon-src/actions/workflows/build.yml/badge.svg)](https://github.com/johnmichaeljimenez/cerberon-src/actions/workflows/build.yml)

- Lightweight, personal and pragmatic 2D Game Engine written in C# using Raylib-cs
- Currently work in progress

## Getting Started

1. Ensure you have the [.NET 9.0 SDK](https://dotnet.microsoft.com/download) installed.
2. Clone the repository:
   ```bash
   git clone https://github.com/johnmichaeljimenez/cerberon-src.git
   cd cerberon-src
   git submodule update --init --recursive
   ```
3. Restore packages and build:
   ```bash
   dotnet restore
   dotnet build
   ```
4. Run the engine:
   ```bash
   dotnet run
   ```

All assets are automatically copied to the output directory on build.

## Game demo
Download the game on itch.io: [rateater93.itch.io/vasodilator](https://rateater93.itch.io/vasodilator)
- The game is continuously updated by this repo via GitHub workflow.

## AI Disclosure
This project is AI-assisted. I used my custom LLM workflow engine, Flowbench ([https://github.com/johnmichaeljimenez/flowbench](https://github.com/johnmichaeljimenez/flowbench)), to assist with generating mock data, code sanity checks (first-pass), and populating placeholder helper functions.
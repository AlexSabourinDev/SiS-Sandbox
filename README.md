# Siiiiii-Sandbox

## Overview
Siiiiii - Sandbox is a studio-wide Unity project that is intended to be used as a tool to collaborate and experiment on various technical strategies.

## Guidelines
- Attempt to document your changes. This will allow new people to jump in and get an understanding of the systems that have been put in place.
- Share. Share your brain smarts with others, this project is an attempt to increase collaboration and overall studio knowledge.

## Proposed Features
### Networking
- How to improve usability?
- Do we want to check out the new Unity multiplayer package?
- Potential Resources:
- [Gaffer On Games](https://gafferongames.com/)
- Nathan Hanlan

### Rendering
- Materials and Lighting. Can we take a look at PBR?
- Potential resource:
- [Filament PBR](https://google.github.io/filament/Filament.md.html)
- [Aura](https://assetstore.unity.com/packages/tools/particles-effects/aura-volumetric-lighting-111664)
- Alex Sabouringtorulethemall
- Thomas Hill

### Tools
- Don't know yet.
- Jared MoocDoonoold

### 3D Character control
- What dis?

### Object collection
- What dis?

## Features
- Travis Martin

### SubScene Streaming
- An incomplete streaming feature is included into the project. It works simply by referencing streaming "cells" that are defined by subscenes. Loading and unloading these subscenes is already supported by Unity's subscene workflow, as a result, the system is simply a driver that determines which subscenes are to be loaded and unloaded.
- It is in an incomplete state because it requires using their ECS infrastructure which makes it difficult to interop with the rest of Unity's infrastructure. Maybe in the future when ECS is more fully fledged. Maybe something else could be introduced in the future instead. For now, this feature is being shelved as is.
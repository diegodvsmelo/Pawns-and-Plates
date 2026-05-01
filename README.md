# Displate (Working name)

Displate (Working name) is a **tactical restaurant management game** in development with Unity, centered around **dispatching staff to time-sensitive tasks** across a growing restaurant.

Instead of relying on passive “preparation timers” as the main interaction, the game turns restaurant flow into an active decision-making system: tasks appear on the map, the player evaluates requirements, assigns the best available employees, manages stamina and recovery, and expands the restaurant while keeping the operation under control.

## Features

* Tactical staff dispatch system
* Employee stats, stamina, and progression
* Time-sensitive restaurant tasks
* Operational vs customer-facing task balance
* Reputation-based restaurant growth
* Hiring and team-building systems
* Daily management loop with end-of-day reports
* Expandable restaurant structure through new stations and furniture
* Camera drag and zoom controls for map navigation

## Gameplay Overview

The core gameplay loop is built around six steps:

1. **A task appears**
   Orders or operational issues spawn on restaurant stations.

2. **The player evaluates the task**
   Each task has requirements, timing pressure, and a calculated success chance.

3. **The player assigns a team**
   Employees are selected manually based on stats, availability, and stamina.

4. **The task enters execution**
   Assigned staff become occupied until the task is resolved.

5. **The player collects the result**
   Rewards, penalties, stamina cost, and any buffs or debuffs are applied when the result is opened.

6. **The restaurant grows**
   Money and reputation are used to improve the operation, unlock new options, and push toward the final evaluation.

## Core Systems

### Employees

Each employee has core attributes:

* **Cooking**
* **Service**
* **Operational**
* **Agility**
* **Stamina**

Employees are intended to feel valuable rather than disposable. Some may also come with unique traits that make them especially useful in specific situations.

### Tasks

Tasks are the main gameplay unit. They can represent:

* customer orders
* urgent requests
* infrastructure problems
* internal operational demands

Tasks move through clear states:

* available
* in progress
* waiting for result collection

### Stamina and Rest

Stamina limits how much work an employee can perform before needing recovery.

The player can manually send employees to rest, and employees who reach zero stamina are forced into full recovery automatically.

### Progression

Progression is tied to:

* reputation
* restaurant prestige tiers
* new stations and furniture
* staff hiring
* better team composition

The long-term goal is to build enough prestige to attract a critic and succeed in the final evaluation day.

## Current Development Status

The project currently includes:

* task spawning on the restaurant map
* team slot assignment
* success chance calculation
* employee XP and leveling
* manual stat allocation
* stamina system
* daily cycle and financial report
* manager-based project structure
* observer/event-based architecture foundations
* draggable orthographic camera with zoom

## Built With

* **Unity**
* **C#**
* **Unity Input System**
* **ScriptableObjects** for data-driven configuration

## Project Goals

The project is being developed with a focus on:

* a realistic solo-dev scope
* modular architecture
* readable UI and game state clarity
* tactical decision-making over fast reflexes
* a short but satisfying progression arc

## Planned Scope for 1.0

The current 1.0 target includes:

* complete daily gameplay loop
* reputation and prestige progression
* restaurant expansion through purchasable stations
* hiring new employees
* basic buffs and debuffs
* initial trait system
* final critic/endgame objective

More advanced systems such as emergent traits, complex random events, free layout placement, and decoration-focused customization are considered possible future expansions rather than current priorities.

## Roadmap

* [x] Core task flow
* [x] Employee stats and progression
* [x] Stamina management
* [x] Daily cycle foundation
* [x] Camera navigation
* [ ] Restaurant expansion system
* [ ] Hiring flow
* [ ] Buff/debuff implementation
* [ ] Visual Assets Creation and Implementation
* [ ] Trait system integration
* [ ] Final critic progression
* [ ] Save/load
* [ ] Game Feel

## Screenshots

Screenshots and gameplay previews will be added as development progresses.

## Installation

This repository contains a Unity project.

To open it:

1. Clone the repository
2. Open the project with the appropriate Unity version
3. Let Unity import dependencies
4. Run the main scene from the editor

## Status

**Work in progress.**

## Author

Developed by **Diego Davis**.

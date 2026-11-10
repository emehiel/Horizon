![Horizon Simulation Framework banner](assets/hsf-banner.png)

<p align="center">
  <img src="assets/hsf-logo.png" alt="HSF logo" width="180">
</p>

# Horizon Simulation Framework (HSF)

HSF is a simulation framework for exploring feasible task schedules in complex systems. It combines dynamic programming with discrete-event simulation and a user-defined system model. The model evaluates proposed tasks, checks system constraints, and tracks changes in system state over time.

## Why HSF

Many system-level requirements describe how a system must perform in use. Those requirements can be difficult to verify before the full system is operational. HSF lets a user model system behavior and evaluate candidate schedules against that model.

The framework separates schedule generation from system behavior. The scheduler proposes tasks and candidate schedules. A modular system model determines whether each task can be performed and how it changes the system state.

## How it works

At a high level, HSF:

1. Generates candidate schedules by proposing tasks within their available access windows.
2. Evaluates each proposed task against the system model and its constraints.
3. Retains feasible schedules and scores them against user-defined objectives or requirements.

Core concepts include:

- **Task:** an action for an asset to perform at a target.
- **Event:** a task together with the system state at a simulation time step.
- **Schedule:** a time-ordered collection of events.
- **Subsystem:** a modular model element that defines system capabilities and state changes.
- **Constraint:** a condition that a subsystem or system state must satisfy.
- **Dependency:** a defined interface through which subsystems exchange information.

## Visualization with Cesium ion

A Cesium ion-based visualization effort is underway as an important companion to the simulation framework. It is intended to make mission scenarios and simulation results easier to explore in an interactive geospatial view. The visualization project is in progress; add its current capabilities, data flow, and relationship to HSF runs here as those details are established.

## Example: Aeolus mission

The project presentation demonstrates HSF with an Earth-observing satellite scenario named Aeolus. The example models a three-revolution mission and reports target-imaging and asset-state outputs. The presentation's v3.0 target-imaging chart reports **307 hits** for that example; treat this as an illustrative result, not a general performance benchmark.

Example outputs shown include target hits, sensor on/off state, solar-panel power, battery depth of discharge, data rate, buffer fill ratio, and image pixel count.

## Example applications

The project materials identify examples involving:

- Thermal soaring
- CubeSats
- Sounding rockets
- UAV swarm search and rescue
- A 6U space telescope constellation
- Lunar Gateway trajectory analysis

## Getting started

> **Draft note:** Confirm the current supported language, dependencies, installation steps, and a runnable example before publishing this section.

1. Install the project dependencies: **[add instructions]**
2. Run the example: **[add command or notebook link]**
3. Review the generated schedules and simulation outputs: **[add output location or viewer instructions]**

## Documentation

- [Academic documentation](https://github.com/emehiel/Horizon/wiki/Academic-Documentation)
- **Cesium ion visualization documentation:** [add repository or project link]

## Project status

**Status:** **[confirm current development status and supported release]**

The material used to prepare this draft describes the framework and example scenarios but does not specify a current release, installation workflow, license, or contribution process. Add those project details before treating this README as final.

## Contributing

Contribution guidelines: **[add link or instructions]**

## License

License: **[add license name and link]**

## Citation

If you use HSF in academic work, cite: **[add preferred citation]**

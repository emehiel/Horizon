# Horizon Simulation Framework
## Introduction
The Horizon Simulation Framework is a modeling and simulation tool which takesmodels,  targets  and  constraints  and  outputs  schedules  for  the  modeled  system  to capture targets within constraints.  It is a powerful systems engineering and mission planning tool because it can be used in initial planning to asses feasibility of a mission concept, in mission design and verification to find leverage points and bottlenecks and iterate designs, and in implementation to determine the final mission schedule that the system will exicute. 

To perform all these functions, Horizon only needs three relatively simple input arguments, a target deck,  a model comprised of subsystems,  and simulation parameters,  in  the  form  of  xml  documents. If a desired subsystem, environment of equation of motion does not exist in the current Horizon modeling library, a user may include their own custom model file written in python (or C#). It  returns  schedules,  which  is  are lists  of  successfully  completed  tasks  with  start  and  end  times. The schedules represent efficient ways that the proposed system can achieve the desired goals of the mission.  Because Horizon is split into two independent components, modeling tools and a timedriven simulation algorithm, the Horizon can simulate any model in any domain, without increases in complexity. 

Check out our [wiki](https://github.com/emehiel/Horizon/wiki) for more information about Horizon and to see [examples](https://github.com/emehiel/Horizon/wiki/Example:-Aeolus) of past mission simulations. For those looking to dive into the details about how to create your own models and simulation, check out our User Guide, which describes the building blocks of a model
## Installation Instructions
To use Horizon Simulation Framework, simply download the repository and execute the ???????

For contributers who want to add new features, it will be useful to download [Visual Studio Community Edition](https://visualstudio.microsoft.com/downloads/).  
## Contribute
Both users and developers can contribute to Horizon.  First project ideas and example missions are organized in the wiki for new users. Users contribute greatly to the project by adding new models and instances of missions to inspire future users.  New developers can get ideas for thier first contribution under the [Projects tab](https://github.com/emehiel/Horizon/projects).  Prospective contributers can find inspiration for simple new features and regressed features to reintroduce. 

Please read the contribution guidelines and Code of Conduct if you'd like to contribute to this project.
## Contribution Guidelines
If you are thinking about contributing, please read our [contribution guidelines](https://github.com/emehiel/Horizon/wiki/Contribution-Guidelines) for where to find issues and requested features, standards for code quality, and the recommended steps for a successful contribution.
## Code of Conduct
[Code of Conduct for Contributers](Horizon/wiki/CODE_OF_CONDUCT.md)

Credit: GitHub
## License
[![The MIT License](https://img.shields.io/badge/License-MIT-green)](https://mit-license.org/)
## Credits
Cory O'Connor

[Dr. Eric Mehiel](https://github.com/emehiel)

[Morgan Yost](https://github.com/moyodancer)

Jack Balfour
## Get in touch
Questions can be addressed to Eric Mehiel at emehiel@calpoly.edu.  Please read (or at least ctrl+F) the Wiki, contribution guidelines, and User Guide before emailing.  

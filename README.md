<div align="center">
    <a href="#"><img src="Documentation/Images/Logo%20-%20with%20text%20180x180.png" alt="C4 InterFlow - Logo"></a>
</div>
<div align="center">
    <strong>C4InterFlow</strong> = <strong>C4</strong> Model + <strong>Inter</strong>faces + <strong>Flow</strong>s
</div>


# Vision 💡
Transform the landscape of **Application Architecture** by bridging the **gap** between **Architecture Model** and **Code**.

# Mission 🎯
 Provide a **comprehensive** Application Architecture **framework** for IT Professionals to
 1. Effortlessly express Application **Architecture as Code** (AaC)
 2. Use AaC to build **Business Processes as Code** (BPaC)
 3. Automatically **generate** Application and Business Architecture **diagrams** and other **documentation** from AaC and BPaC
 4. Intuitively **analyse** Application Architecture by issuing simple **queries** against AaC
 
# Overview

**C4InterFlow** is a comprehensive **Application Architecture** focused **framework** designed for anyone in an organisation, who wants to either **contribute** to (or just to **explore**) the Application Architecture **documentation**.  

## Problem statements ❗

Documenting Application Architecture with diagram drawing tools like Visio, Miro, LucidCharts etc., while popular and often effective, poses several key (genetically unsolved) problems that stem from the inherent limitations of these tools and the complex nature of software architecture. Here are some of these problems

- **Complexity Management**: Modern Application Architecture can become very complex, with many layers, services, components and technologies. Diagrams can get too crowded and hard to understand, making it **hard to clearly express the Architecture**.

- **Dynamic and Evolving Architectures**: Architecture structures change over time. Manually updating diagrams is slow and error-prone, causing **documentation** to become **inaccurate** and  **outdated**.

- **Architecture Model - Code gap**: There is a mismatch between diagramming and coding tools. Changes in code don't automatically update diagrams, causing **inconsistencies** between **Architecture representation** and the **actual code**.

- **Standardisation and Consistency**: Different diagramming tools and different diagram authors use different symbols and styles, leading to **inconsistent documentation**. This can **confuse** and **slow down communication among team members**.

- **Duplication**: **Different teams** drawing Architecture diagrams at **different times** will inevitably create **different views** of the same Architecture **structures** and their **relationships** using slightly **different terminology**. This leads to **duplications** and **confusions**.

- **Collaboration and Version Control**: Many diagramming tools have limited features for teamwork and tracking changes over time, making it **hard to handle updates**, especially with **large or remote teams**.

- **Interoperability and Exporting**: Sharing diagrams between different tools or platforms can lose details, as there's no common standard for diagram formats. This makes it **harder to share and collaborate** on architectural designs.

- **Architecture Analysis**: Diagramming tools often provide **static** representations that lack interactive capabilities for in-depth analysis. This limitation makes it **difficult to understand** how changes in one component might affect others, leading to **inefficient planning** and potential oversights in managing complex systems.

- **Business Process Modelling**: Diagramming tools require users to create **new static diagrams** each time they want to model **new interactions** between different structures, their behaviours, and actors, such as **business processes**. This method leads to significant **duplication** of structures and behaviours across diagrams, making it **cumbersome** to **update**, **maintain**, and **understand** the holistic view of the system.

## Solutions 💡
  
The table below maps Problems to Possible Solutions and C4InterFlow Capabilities.

| ❗ Problem | 💡 Proposed solution | 🌟 C4Interflow Capability |
|---------|-------------------|------------------------|
| **Complexity Management** |  **Modular Architecture Definitions**: define **architecture in code**, so that it can be modularised into smaller, manageable components, making complex systems easier to understand and manage. | Definition of **Architecture as Code** in **C#**, **YAML** or **JSON** at any level of modularity e.g. Software System, Container, Component, Interface etc. |
| **Complexity Management** | **Adaptive Visualization**: allow the user to **adjust the number of structures and their relationships** (boxes and lines) they see, and at what **level of detail**, when they visualise Application Architecture. Manage complexity by displaying only relevant information based on the user's **context**, **focus**, or specific **task at hand**. | Generation of Architecture diagrams of different **Types** (e.g. C4, C4 Static and Sequence), of different **Scopes** (e.g. all Software Systems, Software System, Container, Interface(s), Business Process etc.) and at different **Levels of Details** (e.g. Context, Container and Component) |
| **Dynamic and Evolving Architectures** | **Continuous Integration/Continuous Deployment (CI/CD) for Architecture**: implement of CI/CD pipelines for architecture code, managing evolving architecture through automated integration, and deployment processes. | Using **Command Line Interface** (CLI) in CI/CD pipelines for automating AaC **generation**, AaC **querying** and **diagrams** and other documentation  **generation**|
| **Architecture Model - Code gap** | **Code-driven Architecture and Diagrams generation**: generate architecture diagrams directly from metadata in the codebase, using code analysis and other techniques. | Automatic generation of Architecture as Code in **C#**, **YAML** or **JSON**  from **.NET (C#) source code**. Extensible C4InterFlow architecture allows generation of Architecture as Code from codebases in **other languages** as well as from **Infrastructure as Code** (IaC) |
| **Standardization and Consistency** | **Architecture Domain-Specific Languages (DSLs)**: create standardized DSLs for defining architectures, ensuring **consistency** in how architectures are described and understood across tools and teams. | Use C4InterFlow **Architecture as Code DSL**, inspired by C4 Model and ArchiMate, to express architecture **Structures** and **Behaviours** in **C#**, **YAML** or **JSON**   |
| **Standardization and Consistency** | **Adopt Standards for Diagramming**: work towards a **widely adopted standard(s)** for software architecture diagramming that includes symbols, notation, and abstraction levels, similar to UML. | Automatic generation of **C4 Model** and UML **Sequence** diagrams using a single **Visualisation Engine** that guaranties **consistency** in visual architecture representations |
| **Duplication** | **Architecture Model driven visualisation**: allows for **generation** of architecture diagrams for different view points from a single **Architecture Model** | Create multiple architecture views by **querying** Architecture as Code to find **relevant** Structures and Behaviours (Interfaces) and generating diagrams using **query results** |
| **Collaboration and Version Control**  | **Architecture Catalog**: use it as a collaborative platform where teams can contribute, share, and discuss architectural structures and their relationships. | **Excel Architecture Catalogue** template with **validation** and **CSV export** Macro. Automatic **generation** of Architecture as Code in **C#**, **YAML** or **JSON**  form **CSV Architecture Catalogue** |
| **Collaboration and Version Control**  | **Version-Controlled Architecture Repositories**: store architecture code in version-controlled repositories, enabling collaborative editing, branching, and merging, similar to how source code is managed. | Storing **Architecture as Code** in **C#**, **YAML** or **JSON**  in **version-controlled** repositories e.g. Git |
| **Interoperability and Exporting** | **Exportable and Importable Code and Diagram Formats**: Ensure architecture code and diagrams are written/published in formats that are easily exportable and importable across different tools and platforms, facilitating interoperability. | **Architecture as Code** can be written/generated in **C#**, **YAML** or **JSON** . **Diagrams** can be generated in a variety of formats including **Plant UML** (.puml), **SVG**, **PNG** and **Markdown** (.md files with embedded PNG diagrams). With a bit of additional **custom automation**, these diagrams can be published to other platforms e.g. **GitHub Pages**, **Confluence** etc. |
| **Architecture Analysis** | **Query Language for Architecture**: Develop (or adopt existing) **query** language specifically for **Architecture as Code** or **Architecture Catalogue**. Users could use this language to ask complex questions about **dependencies**, **impact of changes**, and **architecture dynamics**, with the tools parsing the architecture to provide results. | Advanced **querying** of Architecture as Code, with support for **JSON Path**-like syntax, to uncover dependencies or to perform any other AaC analysis. **Custom diagram generation** based on query results |
| **Business Process Modelling** | **Business Process Domain-Specific Languages (DSLs)**: Develop a **Business Process modelling language** that allows for **dynamic linking** between different architectural **structures** and their **behaviour**. This would enable users to define a **single instance** of a structure or behaviours that can be **referenced** across multiple Business Processes, ensuring **consistency** and **reducing duplication**. | Describe **Business Process as Code** in **C#**, **YAML** or **JSON**   through **Activities** and **Flows** utilising existing **Interfaces**. Add and Edit **Business Processes** via the **Excel Architecture Catalogue**. C4InterFlow can **automatically generate** Business Process as Code in **C#**, **YAML** or **JSON**  from Excel/CSV Architecture Catalogue |

## Capabilities map

<a href="#"><img src="Documentation/Images/C4InterFlow%20-%20overview.gif" alt="C4 InterFlow - Overview"></a>

## Toolchain Tracks

<a href="#"><img src="Documentation/Images/C4InterFlow - toolchain tracks.png" alt="C4 InterFlow - Overview"></a>

- **Track 1**
  - User populates Architecture/Business Processes Catalogue
    - Excel is currently the only supported format
  - User executes Excel Macro to Write (export) Architecture/Business Processes Catalogue into CSV files
  - CLI `execute-aac-strategy` Command is executed via CI/CD or in any other manner
    - This generates Architecture as Code (AaC) and Business Processes as Code (BPaC) from Architecture/Business Processes Catalogue in CSV format
- **Track 2**
  - User writes Source Code for Software System(s)
  - CLI `execute-aac-strategy` Command is executed via CI/CD or in any other manner
    - This generates Architecture as Code (AaC) from the Source Code
      - C# Source Code is supported out-of-the-box
      - Extensible C4InterFlow architecture allows generation of Architecture as Code from codebases in **other languages** as well as from **Infrastructure as Code** (IaC)
- **Track 3**
  - User writes Architecture as Code (AaC) and Business Processes as Code (BPaC) either in **C#**, **YAML** or **JSON** 

- **Common steps for Tracks 1, 2 and 3**
  - CLI `draw-diagrams` Command is executed via CI/CD or in any other manner
    - This generates Diagrams as Code in PlantUML (.puml) format
    - This can also generate SVG, PNG and Markdown (.md) formats for the same diagrams

## Example use cases
**Software Engineers** and **Architects** contribute by populating an **Architecture Catalogue** and by generating (or writing manually) **Architecture as Code** (AaC).  

**Product Owners**, **QA** and any other team members, on the other hand, explore the Application Architecture by **browsing** various **automatically generated diagrams** (e.g. C4, Sequence etc.) as well as by **querying** Application Architecture (e.g. to find dependencies).  

**Architects** and **Product Owners** contribute by adding Business Processes to the **Architecture Catalogue** or by writing **Business Processes as Code** manually.

**Operations** team members explore the Business Processes by **browsing** various **automatically generated diagrams** 

# Getting Started

- Choose an **approach** for expressing Architecture as Code (AaC) i.e. **C#**, **YAML** or **JSON** 
- Choose the [C4InterFlow toolchain Track](#toolchain-tracks) you'd like use to eventually arrive to the Application Architecture Diagrams
- Check out the [Samples](https://github.com/SlavaVedernikov/C4InterFlow/wiki/Samples)
- Explore the [CLI capabilities](https://github.com/SlavaVedernikov/C4InterFlow/wiki/Command-Line-Interface-(CLI)) available **out-of-the-box** with C4InterFlow
- **Learn** more on the [wiki](https://github.com/SlavaVedernikov/C4InterFlow/wiki)
- Have a **question** or any other **enquiries**, [let's talk](https://www.c4interflow.com/letstalk).
- Want to **work together** on **your** implementation of **Architecture as Code**, [let's talk](https://www.c4interflow.com/letstalk).
- Want to **discuss the best place to start**, [Book a FREE 30 minutes consultation](https://scheduler.zoom.us/c4interflow/getting-started-consultation)

# Feedback

If you've tried using C4InterFlow, we would love to hear from you.  
What were your expectations? What were you trying to achieve? Did it work for you? What was missing?  
Leave us [your feedback](https://github.com/SlavaVedernikov/C4InterFlow/discussions/73).  We'll learn from it and try our best to make C4InterFlow better for you.

# System Requirements

- C4InterFlow uses PlantUML .jar (embedded within the project), which requires **Java** to be installed on your machine
  - Check if Java is already installed: `java -version`. The minimum version needed is **Java 8**.

# Known issues

- Git "Filename too long" message on Windows
  - Use `git config --global core.longpaths true` to allow long paths
  - See [Paths too long on Windows](https://github.com/SlavaVedernikov/C4InterFlow/issues/107) issue

# Support

- For help or to report issues, please raise [GitHub issues](https://github.com/SlavaVedernikov/C4InterFlow/issues).

# Contributions

- Adhere to .NET coding standards.
- Work on approved issues only.
- Submit contributions via Pull Requests for review.

# Acknowledgements

- Inspired by the [C4 Model](https://c4model.com/), [C4-PlantUML project](https://github.com/plantuml-stdlib/C4-PlantUML), and [c4sharp project](https://github.com/your-github-repo/c4sharp).

---
**NOTE**: This document is subject to change. Users are encouraged to check back regularly for updates.



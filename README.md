![C4 InterFlow - Logo](Documentation/Images/Logo%20-%20with%20text%20180x180.png)

# What is C4InterFlow?

**C4InterFlow** is a comprehensive **Application Architecture** focused **toolchain** designed for anyone in an organisation, who wants to either **contribute** to (or just to **explore**) the Application Architecture **documentation**. For example, Software Engineers and Architects can contribute by populating an **Architecture Catalogue** and by writing (or generating) **Architecture as Code** (AaC). Product Owners, QA and any other team members, on the other hand, may want to explore the Application Architecture by **browsing** various **automatically generated diagrams** (e.g. C4, Sequence etc.) as well as by **querying** Application Architecture (e.g. to find dependencies). Architects and Product Owners can also contribute by adding Business Processes to the **Architecture Catalogue** or by writing **Business Processes as Code**.

# Capabilities
C4InterFlow offers versatile capabilities including:
- Manual creation of **Architecture as Code** in **C#** or **YAML**
- Automatic generation of Architecture as Code in C# or YAML from **.NET (C#) source code** or from **Excel/CSV Architecture Catalogue**
  - **NOTE**: Automatic generation of Architecture as Code is also possible from the source code in **other languages** as well as from **Infrastructure as Code** (IaC) 
- Generation of **C4 Model** and **Sequence** diagrams of various **scopes** (e.g. all Software Systems, Software System, Container, Interface(s), Business Process etc.) and at different **levels of details** (e.g. Context, Container and Component)
  - Diagrams can be generated in a variety of **formats** including **Plant UML** (.puml), **SVG**, **PNG** and **Markdown** (.md files with embedded PNG diagrams)
  - With a bit of additional custom automation, these diagrams can be published to other platforms e.g. **GitHub Pages**, **Confluence** etc.
- Advanced **querying** of Architecture as Code, with support for **JSON Path-like** syntax, to uncover dependencies or to perform any other AaC analysis
  - **Custom** diagram generation based on **query results**
- **Business Process** description through **Activities** and **Interfaces** utilisation
  - Manual writing of **Business Process as Code** in **C#** or **YAML**
  - Adding and editing Business Processes via the **Excel/CSV Architecture Catalogue**
    - **Automatic generation** of **Business Process as Code** in C# or YAML from **Excel/CSV Architecture Catalogue**
- **Command Line Interface** (CLI) used for automating AaC **generation**, AaC **querying** and **diagrams generation**

# System Requirements

- Java PlantUML .jar (embedded within the project).

# Getting Started

- Utilize the provided CLI capabilities within any C# Command Line App.
- Choose a strategy for Architecture as Code generation.
- Utilize CLI commands and options to generate or manually write Architecture as Code and to start drawing diagrams.

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



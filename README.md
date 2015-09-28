# DevArch - pre-alpha
## Architechture for anti-architechts

DevArch provides an alternative method for creating architecture diagrams.

Following the footsteps of reverse-architechting, DevArch aims to provide one button press diagram generation.

However, instead of allowing the developer to later modify the generated diagrams to their liking (which can be extremely demanding), DevArch relies on smart filtering algorithms that allows the user to customize *how* the diagram is generated.

These customizations, or rather, *diagram definitions*, can then be checked in together with the sourcecode. 

This means that anyone with the sourcecode are able to regenerate diagrams that are tailored for comunicating the original intent.

####DevArch makes architechting more formal, more explicit, faster and easier.

### Current architechture, as generated by DevArch:
![Alt text](/Current arch.PNG?raw=true "Optional Title")

Generated using the settings:
 - Pluralize name patterns (unused)
 - Pluralize base class patterns (LayerControl and ArchControl where merged into 'UserControls')
 - Depth: 3
 - Treat chained dependencies as linnear ones ('Clients' depend on both Presentation and Analysis, but the analysis dependency is viewed as an indirectional result of being dependant on Presentation)
 - Skip single child containers (the default namespaces are not displayed inside the projects)
 - Remove tests (the whole test project is skipped)

###To try it out
Set the project "Standalone" to current startup project and press F5 to view a diagram of the current architechture.

###Todo:
* **Filters to add:**
* Reference count filter
* Lines of code count filter
* Exceptions filter
* Max number of classes, instead of specifying depth
* **Patterns to find:**
* Facade
* Singleton
* Vertical layers
* **Enable dependency order setting **
* **Enable generation of class diagrams**

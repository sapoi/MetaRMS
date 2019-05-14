# MetaRMS
This repository contains source files for the MetaRMS bachelor thesis.
This thesis was written by Hana Nekvindova at the Faculty of Mathematics and Physics at the Charles University in 2019.

### Abstract
Data are an essential element of the present world. The problem of storing data concerns everybody, from a large company with information about their clients to individual users with their shopping lists. Options vary between a simple Excel sheet and expensive custom solution. A general software solution to cover these cases is needed. However, the requirements on the structure of the data differ for every use-case. 
This thesis aims to solve this problem by creating an application generating software. The software generates a custom application when provided with the description of the data structure. For that, we define the format of the description of the data structure and analyze various approaches to the implementation of the application generating software. 
Our solution contains an ASP.NET Core server application and an example web client application communicating over the public JSON API. The server accepts the description and creates an application accordingly. The solution also contains a library, that is used by the example web client and is reusable by other client front-ends.


### Contents of the repository
```
├── .vscode - configuration for VisualStudio Code
├── Core - Core project
├── Shared library - SharedLibrary project
├── MetaRMS.sln - solution file
├── thesis.pdf - text of the thesis
└── LICENSE.txt - licensing information
```
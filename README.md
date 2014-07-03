EasyBuild
=========
[![Build Status](https://travis-ci.org/giacomelli/EasyBuild.png?branch=master)](https://travis-ci.org/giacomelli/EasyBuild)

An easy-to-use collection of MS Build tasks to help improve your build process.


--------

Tasks
===
 - **StartWebProjectTask**: when you need to start some one of your web projects during the build process (usefull to generate some client library for your web apis).
 
--------

Usage
===
Build the EasyBuild, put his assembly and dependencies assemblies in a folder on your solution, let me say "references\EasyBuild"

StartWebProjectTask
---
Create a file called StartMyWebProject.targets on your msbuilds file folder (if you don't have one, create one "msbuilds" on your solution root dir):
 
```xml
<Project ToolsVersion="4.0" DefaultTargets="BeforeBuild" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <UsingTask TaskName="StartWebProjectTask" 
        AssemblyFile="..\references\EasyBuild\EasyBuild.dll"/>
    
    	<Target Name="BeforeBuild">
    		<Message text="Starting MyWebProject..." />
    		<StartWebProjectTask projectFolderName="MyWebProject" port="8181" />
    		<Message text="MyWebProject started." />
	</Target>
</Project>

```

Edit the target project file that you want use the task, and add the following line bellow the "Microsoft.CSharp.targets" one:

```xml
<Import Project="..\msbuilds\StartMyWebProject.targets" />

```

Build your project and the web project (MyWebProject) should be started before the target project build.

--------

FAQ
-------- 
Having troubles? 
 - Ask on [Stack Overflow](http://stackoverflow.com/search?q=EasyBuild)

Roadmap
-------- 
 - Add others tasks.
 
--------

How to improve it?
======

- Create a fork of [EasyBuild](https://github.com/giacomelli/EasyBuild/fork). 
- Follow our [develoment guidelines](https://github.com/giacomelli/EasyBuild/wiki/Development-Guidelines).
- Did you change it? [Submit a pull request](https://github.com/giacomelli/EasyBuild/pull/new/master).


License
======

Licensed under the The MIT License (MIT).
In others words, you can use this library for developement any kind of software: open source, commercial, proprietary and alien.


Change Log
======
 - 0.1.0 First version.

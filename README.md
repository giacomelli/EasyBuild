#EasyBuild
[![Build Status](https://travis-ci.org/giacomelli/EasyBuild.png?branch=master)](https://travis-ci.org/giacomelli/EasyBuild)

An easy-to-use collection of MS Build tasks to help improve your build process.


--------

##Tasks
===
 - **StartWebProjectTask**: when you need to start some one of your web projects during the build process (useful to generate some client library for your web apis).

 - **Resources2JSTask**: serialize resource files with localized labels to JS files.
 
--------

##Usage
Follow the steps below to put EasyBuild on your solution. 
On Step 3 you can choose just the tasks you want to use.

###Step 1
Build the EasyBuild, put his assembly and dependencies assemblies in a folder on your solution, let me say "references\EasyBuild"

###Step 2
If you don't jhave a folder to your msbuild files, just create one "msbuilds" folder on your solution root folder.

###Step 3
StartWebProjectTask
---
Create a file called StartMyWebProject.targets on your msbuilds folder:
 
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

Resources2JSTask
---
Create a file called .Resources2JSTask.targets on your msbuilds file folder (if you don't have one, create one "msbuilds" on your solution root dir):
 
```xml
<Project ToolsVersion="4.0" DefaultTargets="BeforeBuild" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <UsingTask TaskName="Resources2JSTask" 
        AssemblyFile="..\references\EasyBuild\EasyBuild.dll"/>
    
    	<Target Name="AfterBuild">
    		<Message text="Starting Resources2JSTask..." />
    		
    		<Resources2JSTask 
    			assemblyFileName="..\<YOUR PROJECT WITH RESOURCE FILE>\bin\$(configuration)\<YOUR PROJECT WITH RESOURCE FILE>.dll" 
    			serializationFolder="..\<YOUR WEB PROJECT>Scripts\Framework\Globalization"
    			serializationFilename="<YOUR DESIRED FILE NAME>"
    			cultureCodes="pt,es,en"
    			defaultCultureCode="en" />

    		<Message text="Resources2JSTask done." />
	</Target>
</Project>

```

###Step 4
Edit the target project file that you want use the task, and add the following line bellow the "Microsoft.CSharp.targets" one:

```xml
<Import Project="..\msbuilds\<your msbuild.targets file>" />

```

###Step 5
Build your target project to see the task in action.

FAQ
-------- 
Having troubles? 
 - Ask on [Stack Overflow](http://stackoverflow.com/search?q=EasyBuild)

Roadmap
-------- 
 - Add others tasks.
 - Publish a NuGet package.
 
--------

How to improve it?
======

- Create a fork of [EasyBuild](https://github.com/giacomelli/EasyBuild/fork). 
- Did you change it? [Submit a pull request](https://github.com/giacomelli/EasyBuild/pull/new/master).


License
======

Licensed under the The MIT License (MIT).
In others words, you can use this library for developement any kind of software: open source, commercial, proprietary and alien.


Change Log
======
 - 0.1.0 First version.

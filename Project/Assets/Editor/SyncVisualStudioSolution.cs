using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;

//yeah yeah, hack slash code. at least it works ;)
//
//have fun, Lucas (lucas@lucasmeijer.com)

public class SyncVisualStudioSolution
{
    static bool stopwining = false;

    [MenuItem("Tools/CreateVisualStudioSolution")]
    public static void CreateIt()
    {
        string vs_root = Directory.GetCurrentDirectory();
       
        //write solution file out to disk.
        StreamWriter sw = new StreamWriter(Path.Combine(vs_root,GetProjectName() + ".sln"));
        sw.Write(GetSolutionText());
        sw.Close();

        //write first part of our project file.
        sw = new StreamWriter(Path.Combine(vs_root,GetProjectName() + ".csproj"));
        sw.Write(GetProjectFileHead());

        //add a line for each .cs file found.
        DirectoryInfo di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(),"Assets"));
        FileInfo[] fis = di.GetFiles("*.cs",SearchOption.AllDirectories);
        foreach (FileInfo fi in fis)
        {
            string relative = fi.FullName.Substring(di.FullName.Length+1);
            relative = relative.Replace("/", "\\");
            sw.WriteLine("     <Compile Include=\"Assets\\"+relative+"\" />");
        }

        //add a line for each shader found.
        fis = di.GetFiles("*.shader", SearchOption.AllDirectories);  //make recursive
        foreach (FileInfo fi in fis)
        {
            string relative = fi.FullName.Substring(di.FullName.Length + 1);
            relative = relative.Replace("/", "\\");
            sw.WriteLine("     <None Include=\"Assets\\" + relative + "\" />");
        }
    
        //and write the tail of our projectfile.
        sw.Write(GetProjectFileTail());
        sw.Close();

        //why is System.IO.FileInfo always struggling with me?
        FileInfo dll = new FileInfo(GetAssemblyPath(typeof (GameObject)));
        string assemblyfolder = dll.Directory.ToString();

        FileInfo unityengine = new FileInfo(assemblyfolder+"\\UnityEngine.xml");
        FileInfo unityeditor = new FileInfo(assemblyfolder + "\\UnityEditor.xml");

        if ((!unityengine.Exists || !unityeditor.Exists) && !stopwining)
        {
            EditorUtility.DisplayDialog("Documentation missing.",
                                        "For inline documentation in visual studio, download this archive:\nhttp://www.unifycommunity.com\n/wiki/images/d/d2/Visual_Studio_docs_2.5.zip and unpack it into " +
                                        unityengine.Directory.FullName,"OK","OK");
            stopwining = true;
        }
    }


    static string GetProjectName()
    {
        DirectoryInfo d = new DirectoryInfo(Directory.GetCurrentDirectory());
        string projectname = d.Name;
        return projectname;
    }
    static string MyHash(string input)
    {
        byte[] bs = MD5.Create().ComputeHash(Encoding.Default.GetBytes(input));
        StringBuilder sb = new StringBuilder();
        foreach (byte b in bs)
            sb.Append(b.ToString("x2"));
        string s = sb.ToString();

        s = s.Substring(0, 8) + "-" + s.Substring(8, 4) + "-" + s.Substring(12, 4) + "-" + s.Substring(16, 4) + "-" + s.Substring(20, 12);
        return s.ToUpper();
    }
    static string GetProjectGUID()
    {
        return MyHash(GetProjectName() + "salt");
    }
	static string GetAssemblyPath(Type t)
	{
		return Assembly.GetAssembly(t).Location.Replace("/","\\");
	}
	
    static string GetSolutionGUID()
    {
        return MyHash(GetProjectName());
    }


    static string GetSolutionText()
    {
        string t = @"Microsoft Visual Studio Solution File, Format Version 10.00

# Visual Studio 2008

Project(~{" + GetSolutionGUID() + @"}~) = ~" + GetProjectName() + @"~, ~" + GetProjectName() + @".csproj~, ~{" + GetProjectGUID() + @"}~
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{" + GetProjectGUID() + @"}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{" + GetProjectGUID() + @"}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{" + GetProjectGUID() + @"}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{" + GetProjectGUID() + @"}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal
";
        return t.Replace("~", "\"");
    }




    static string GetProjectFileHead()
    {
        string t = @"<?xml version=~1.0~ encoding=~utf-8~?>
<Project ToolsVersion=~3.5~ DefaultTargets=~Build~ xmlns=~http://schemas.microsoft.com/developer/msbuild/2003~>
  <PropertyGroup>
    <Configuration Condition=~ '$(Configuration)' == '' ~>Debug</Configuration>
    <Platform Condition=~ '$(Platform)' == '' ~>AnyCPU</Platform>
    <ProductVersion>9.0.21022</ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectGuid>{" + GetProjectGUID() + @"}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>Irrelevant</RootNamespace>
    <AssemblyName>Irrelvant</AssemblyName>
    <TargetFrameworkVersion>v3.5</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
  </PropertyGroup>
  <PropertyGroup Condition=~ '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ~>
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>Temp\bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition=~ '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ~>
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>Temp\bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include=~System~ />
    <Reference Include=~System.Core~ />
    <Reference Include=~UnityEngine~>
      <HintPath>"+ GetAssemblyPath(typeof(GameObject)) + @"</HintPath>
    </Reference>
    <Reference Include=~UnityEditor~>
      <HintPath>" + GetAssemblyPath(typeof(EditorWindow)) + @"</HintPath>
    </Reference>
  </ItemGroup>
  <ItemGroup>
";
        return t.Replace("~", "\"");
    }

    static string GetProjectFileTail()
    {
        string t = @"  </ItemGroup>
  <Import Project=~$(MSBuildToolsPath)\Microsoft.CSharp.targets~ />
  <!-- To modify your build process, add your task inside one of the targets below and uncomment it. 
       Other similar extension points exist, see Microsoft.Common.targets.
  <Target Name=~BeforeBuild~>
  </Target>
  <Target Name=~AfterBuild~>
  </Target>
  -->
</Project>
    ";
        return t.Replace("~", "\"");
    }
}
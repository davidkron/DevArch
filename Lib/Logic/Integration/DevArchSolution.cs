﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Logic.Building;
using Logic.Common;
using Logic.SemanticTree;
using Microsoft.Build.Construction;
using Microsoft.CodeAnalysis.MSBuild;
using Project = EnvDTE.Project;
using Solution = Microsoft.CodeAnalysis.Solution;

namespace Logic.Integration
{
    public class VisualStudio
    {
        public readonly DevArchSolution Solution;
        private readonly DTE2 _automationObject;

        public OutputWindowPane DevArchOutputWindow()
        {
            if (_automationObject == null)
                return null;

            var outputWindow = _automationObject.ToolWindows.OutputWindow;
            outputWindow.Parent.Activate();
            OutputWindowPane devArchOutput;
            try
            {
                devArchOutput = outputWindow.OutputWindowPanes.Item("DevArch");
            }
            catch (Exception)
            {
                devArchOutput = null;
            }
            if (devArchOutput == null)
                devArchOutput = outputWindow.OutputWindowPanes.Add("DevArch");
            devArchOutput.Activate();
            return devArchOutput;
        }

        public VisualStudio(DTE environment)
        {
            _automationObject = (DTE2) environment;
            Solution = new DevArchSolution(environment);
        }
        public VisualStudio(DTE environment,Solution RoslynSolution)
        {
            _automationObject = (DTE2)environment;
            Solution = new DevArchSolution(environment,RoslynSolution);
        }
    }

    public class DevArchSolution
    {
        public readonly Solution RoslynSolution;
        //private Projects DteProjects;
        public string _fullName;
        public string Name;
        public string Directory;
        public readonly SolutionNode SolutionTree;
        /*public DevArchSolution(_DTE dte)
        {
            var build = MSBuildWorkspace.Create();
            var dteSolution = KeepTrying.ToGet(() => dte.Solution);
            _fullName = KeepTrying.ToGet(() => dteSolution.FullName);
            if (string.IsNullOrEmpty(_fullName))
                throw new Exception("Unable to find opened solution");
            var sol = build.OpenSolutionAsync(_fullName);
            RoslynSolution = KeepTrying.ToGet(() => sol.Result);

            DteProjects = KeepTrying.ToGet(() =>dteSolution.Projects);
            Name = Path.GetFileName(_fullName);
            if (_fullName == null)
                throw new NoSolutionOpenException();
        }*/

        public DevArchSolution(_DTE dte, Solution currentSolution = null)
        {
            SolutionTree = GetDteProjects(dte);
            if (currentSolution != null)
            {
                RoslynSolution = currentSolution;
            }
            else
            {
                var build = MSBuildWorkspace.Create();
                var sol = build.OpenSolutionAsync(_fullName);
                sol.Wait();
                RoslynSolution = KeepTrying.ToGet(() => sol.Result);
                if (!RoslynSolution.Projects.Any())
                    throw new NoCsharpProjectsFoundException();
            }
        }

        public DevArchSolution(string path)
        {
            _fullName = path;
            SolutionTree = new SolutionNode("-");
            SolutionTree.AddChilds(GetProjectTree(_fullName));
            //
            var build = MSBuildWorkspace.Create();
            var sol = build.OpenSolutionAsync(_fullName);
            sol.Wait();
            RoslynSolution = KeepTrying.ToGet(() => sol.Result);
            if (!RoslynSolution.Projects.Any())
                throw new NoCsharpProjectsFoundException();
        }
        

        public IEnumerable<Node> GetProjectTree(string path)
        {
            var solFile = SolutionFile.Parse(path);
            
            var projects = solFile.ProjectsInOrder.ToList();
            var archProjects = projects.Where(x => x.RelativePath.EndsWith(".archproj")).ToList();
            projects.RemoveRange(archProjects);
            ArchProjects = archProjects.Select(x => new ArchProject(x)).ToList();

            var all = solFile.ProjectsInOrder.SelectList(x => new ProjectNode(x));
            var nodes = new List<Node>();
            foreach (var project in solFile.ProjectsInOrder)
            {
                var currNode = all.FirstOrDefault(p => p.ProjectId == new Guid(project.ProjectGuid));
                if (currNode == null)
                    continue;
                if (project.ParentProjectGuid != null)
                {
                    var parentId = new Guid(project.ParentProjectGuid);
                    var parent = all.First(f => f.ProjectId == parentId);
                    parent.AddChild(currNode);
                }
                else
                {
                    nodes.Add(currNode);
                }
            }
            return nodes;
        }

        private SolutionNode GetDteProjects(_DTE dte)
        {
            var dteSolution = dte.Solution;
            _fullName = KeepTrying.ToGet(() => dteSolution.FullName);
            if (string.IsNullOrEmpty(_fullName))
                throw new Exception("Unable to find opened solution");
            var DteProjects = KeepTrying.ToGet(() => dteSolution.Projects);
            Name = Path.GetFileName(_fullName);
            Directory = Path.GetDirectoryName(_fullName) + "\\";
            var sln = new SolutionNode("-");
            sln.AddChilds(ProjectTreeBuilder.AddSolutionFoldersToTree(DteProjects));
            return sln;
        }

        public List<ArchProject> ArchProjects;
    }

    public static class ProjectExtensions
    {
        private static readonly Func<ProjectItem, bool> IsFolder = x => x.ProjectItems.Count != 0;

        public static IEnumerable<ProjectItem> GetAllProjectItems(this Project project)
        {
            var areFolder = project.ProjectItems.Cast<ProjectItem>().ToLookup(IsFolder);
            var items = areFolder[false].ToList();
            var folders = areFolder[true].ToList();
            return items.Concat(folders.SelectMany(GetAllProjectItems));
        }

        private static IEnumerable<ProjectItem> GetAllProjectItems(ProjectItem folder)
        {
            var areFolder = folder.ProjectItems.Cast<ProjectItem>().ToLookup(IsFolder);
            var items = areFolder[false];
            var folders = areFolder[true];
            return items.Concat(folders.SelectMany(GetAllProjectItems));
        }
    }


    static class KeepTrying
    {
        static int _totalRetries = 0;
        const int Maxtries = 3;

        public static T ToGet<T>(Func<T> function) where T : class
        {
            T result = null;
            var failed = true;
            var tries = 0;
            while (failed && result == null)
            {
                tries++;
                try
                {
                    result = function();
                    failed = false;
                }
                catch (Exception e)
                {
                    if (tries < Maxtries) continue;

                    if (_totalRetries > Maxtries)
                    {
                        throw e;
                    }

                    _totalRetries++;
                    tries = 0;
                }
            }
            return result;
        }
    }
}
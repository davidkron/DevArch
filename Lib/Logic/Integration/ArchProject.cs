using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;

namespace Logic.Integration
{
    public class ArchProject
    {
        private readonly Project _project;

        public class DiagramDefinitionFile
        {
            private readonly string _path;
            private readonly string _name;

            public DiagramDefinitionFile(ProjectItem item)
            {
                _path = item.FileNames[0];
                _name = item.Name;
            }

            private string Content => File.ReadAllText(_path);

            public DiagramDefinitionParseResult Parse(string directory)
            {
                try
                {
                    var definition = DiagramDefinitionParser.ParseDiagramDefinition(_name, Content);
                    //Insert directory before output path
                    definition.Output = new OutputSettings(directory + definition.Output.Path,definition.Output.Size);
                    return new DiagramDefinitionParseResult(definition);
                }
                catch (Exception e)
                {
                    return new DiagramDefinitionParseResult(new Exception(_name + "- " + e.Message));
                }
            }
        }

        public ArchProject(Project project)
        {
            _project = project;
        }

        public IEnumerable<DiagramDefinitionFile> GetDiagramDefinitionFiles()
        {
            var projectItems = _project.GetAllProjectItems();
            var definitionItems = projectItems.Where(d => d.Name.EndsWith(".diagramdefinition"));
            return definitionItems.Select(x => new DiagramDefinitionFile(x));
        }
    }
}
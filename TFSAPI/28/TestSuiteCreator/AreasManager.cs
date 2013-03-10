// Written by Shai Raiten: http://blogs.microsoft.co.il/blogs/shair/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TestSuiteCreator
{
public class AreasManager
{
    private Project Project;

    public AreasManager(Project project)
    {
        this.Project = project;
    }

    public List<string> GetProjectAreas()
    {
        List<string> Areas = new List<string>();

        WriteNodes(this.Project.AreaRootNodes, Areas);

        return Areas;
    }

    private void WriteNodes(NodeCollection nodes, List<string> Areas)
    {
        foreach (Node node in nodes)
        {
            Areas.Add(node.Path.Substring(node.Path.IndexOf(@"\")));

            if (node.ChildNodes.Count > 0)
            {
                WriteNodes(node.ChildNodes, Areas);
            }
        }
    }
}
}

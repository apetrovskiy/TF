// Written by Shai Raiten: http://blogs.microsoft.co.il/blogs/shair/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TestSuiteCreator
{
public class TestSuiteHelper
{
    private ITestManagementTeamProject _testproject;
    private Project _project;

    public TestSuiteHelper(ITestManagementTeamProject TestManagementTeamProject, Project project)
    {
        this._testproject = TestManagementTeamProject;
        this._project = project;
    }

    public void AddTests(IStaticTestSuite suite, string area)
    {
        IEnumerable<ITestCase> testcases = _testproject.TestCases.Query(string.Format("Select * from [WorkItems] where [System.AreaPath] = \"{0}\\{1}\"", _project.Name, area));

        foreach (ITestCase testcase in testcases)
        {
            suite.Entries.Add(testcase);
        }
    }

    public IStaticTestSuite FindSuite(ITestSuiteEntryCollection collection, string title)
    {
        foreach (ITestSuiteEntry entry in collection)
        {
            IStaticTestSuite suite = entry.TestSuite as IStaticTestSuite;

            if (suite != null)
            {
                if (suite.Title == title)
                    return suite;
                else if (suite.Entries.Count > 0)
                    FindSuite(suite.Entries, title);
            }
        }
        return _testproject.TestSuites.CreateStatic();
    }
}
}

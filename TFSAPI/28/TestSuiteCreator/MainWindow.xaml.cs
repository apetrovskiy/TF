// Written by Shai Raiten: http://blogs.microsoft.co.il/blogs/shair/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.Server;
using System.ComponentModel;

namespace TestSuiteCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TfsTeamProjectCollection _tfs;
        private WorkItemStore _store;
        private ITestManagementTeamProject _testproject;
        private Project _project;
        private ITestPlan _plan;

        private delegate void Execute();

        BackgroundWorker bg_get_areas;
        BackgroundWorker bg_create_test_suites;

        private TestSuiteHelper TestHelper;

        public MainWindow()
        {
            InitializeComponent();

            bg_get_areas = new BackgroundWorker();
            bg_get_areas.DoWork += new DoWorkEventHandler(bg_get_areas_DoWork);
            bg_get_areas.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_get_areas_RunWorkerCompleted);


            bg_create_test_suites = new BackgroundWorker();
            bg_create_test_suites.DoWork += new DoWorkEventHandler(bg_create_test_suites_DoWork);
            bg_create_test_suites.WorkerReportsProgress = true;
            bg_create_test_suites.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_create_test_suites_RunWorkerCompleted);
            bg_create_test_suites.ProgressChanged += new ProgressChangedEventHandler(bg_create_test_suites_ProgressChanged);

        }

        void bg_create_test_suites_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;

            lbl_progress.Content = string.Format("Items Complete: {0} of {1}", e.ProgressPercentage, progressBar1.Maximum);
        }

        void bg_create_test_suites_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            GetTestPlans();
            this.tabControl1.IsEnabled = true;
        }

        void bg_create_test_suites_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> areas = e.Argument as List<string>;

            Execute ex = delegate()
            {
                progressBar1.Value = 0;
                progressBar1.Maximum = areas.Count - 1;
            };
            this.Dispatcher.Invoke(ex, null);

            int count = areas.Count;
            for (int i = 0; i < count; i++)
            {
                CreateTestSuite(areas[i]);
                bg_create_test_suites.ReportProgress(i);
            }
        }
        /// <summary>
        /// Get's a full path of Area, split it and for each part create Test Suite and apply the Test Cases beneath it.
        /// </summary>
        /// <param name="full_area">Area Path, for example - CMMI\First Area\Sub Area\Content Area</param>
        void CreateTestSuite(string full_area)
        {
            try
            {
                string[] areas = full_area.Split('\\');
                string full_path = string.Empty;
                IStaticTestSuite suite = null;
                string current_area = string.Empty;

                for (int i = 0; i < areas.Length; i++)
                {
                    if (!string.IsNullOrEmpty(areas[i]))
                    {
                        string area = areas[i].RemoveBadChars();
                        current_area += area;

                        //The first item, find it and assigned to suite object.
                        if (i == 1)
                        {
                            ITestSuiteEntryCollection collection = _plan.RootSuite.Entries;
                            suite = TestHelper.FindSuite(collection, area);
                            if (suite.Id == 0)
                            {
                                suite.Title = area;
                                TestHelper.AddTests(suite, current_area);
                                _plan.RootSuite.Entries.Add(suite);
                            }
                        }
                        else
                        {
                            ITestSuiteEntryCollection collection = suite.Entries;
                            //* collection - Perform search only under the suite.Entries  - Duplicate items allowed. 
                            IStaticTestSuite subSuite = TestHelper.FindSuite(collection, area);

                            if (subSuite.Id == 0)
                            {//Cannot find Test Suite
                                subSuite.Title = area;
                                suite.Entries.Add(subSuite);
                                //After creating the Test Suite - Add the related TestCases based on the Area Path.
                                TestHelper.AddTests(subSuite, current_area);
                            }

                            suite = subSuite;
                        }
                        current_area += "\\";
                        _plan.Save();
                    }
                }
                _plan.Save();
            }
            catch (TestSuiteInvalidOperationException testex)
            {
                if (!testex.Message.Contains("Duplicate suite name detected"))
                    throw new ArgumentNullException(testex.Message);
            }
        }

        void bg_get_areas_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<string> Areas = e.Result as List<string>;
            list_areas.ItemsSource = Areas;
            this.tabControl1.IsEnabled = true;
        }

        void bg_get_areas_DoWork(object sender, DoWorkEventArgs e)
        {
            AreasManager areas = new AreasManager(this._project);
            e.Result = areas.GetProjectAreas();
        }

        private void btn_connect_tfs_Click(object sender, RoutedEventArgs e)
        {
            TeamProjectPicker tpp = new TeamProjectPicker(TeamProjectPickerMode.SingleProject, false);
            tpp.ShowDialog();

            if (tpp.SelectedTeamProjectCollection != null)
            {
                this._tfs = tpp.SelectedTeamProjectCollection;
                this._store = (WorkItemStore)_tfs.GetService(typeof(WorkItemStore));
                this._project = _store.Projects[tpp.SelectedProjects[0].Name];
                ITestManagementService test_service = (ITestManagementService)_tfs.GetService(typeof(ITestManagementService));
                this._testproject = test_service.GetTeamProject(_project);

                TestHelper = new TestSuiteHelper(_testproject, _project);
                this.tp_ts_creator.IsEnabled = true;

                bg_get_areas.RunWorkerAsync();
                GetTestPlans();
            }
        }

        void GetTestPlans()
        {
            ITestPlanCollection plans = _testproject.TestPlans.Query("Select * From TestPlan");
            this.list_plans.ItemsSource = plans;
        }

        private void btn_create_testsuites_Click(object sender, RoutedEventArgs e)
        {
            if (!bg_create_test_suites.IsBusy && list_plans.SelectedItem != null && list_areas.SelectedItems.Count > 0)
            {
                List<string> areas = new List<string>();
                foreach (string a in list_areas.SelectedItems)
                {
                    areas.Add(a);
                }

                this.tabControl1.IsEnabled = false;
                this._plan = list_plans.SelectedItem as ITestPlan;
                bg_create_test_suites.RunWorkerAsync(areas);
            }
            else
            {
                MessageBox.Show("Please make sure to select Test Plan and at least one area to create", "Missing Value", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btn_remove_plan_Click(object sender, RoutedEventArgs e)
        {
            if (list_plans.SelectedItem != null)
            {
                ITestPlan plan = list_plans.SelectedItem as ITestPlan;
                plan.Delete(DeleteAction.ForceDeletion);
                GetTestPlans();
            }
            else
                MessageBox.Show("Please select Test Plan", "Missing Value", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btn_add_plan_Click_1(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txt_test_plan.Text))
            {
                ITestPlan plan = _testproject.TestPlans.Create();
                plan.Name = txt_test_plan.Text;
                plan.Save();

                txt_test_plan.Text = string.Empty;

                GetTestPlans();
                this.list_plans.SelectedItem = plan;
            }
            else
                MessageBox.Show("Please enter 'Test Plan Name'", "Missing Value", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

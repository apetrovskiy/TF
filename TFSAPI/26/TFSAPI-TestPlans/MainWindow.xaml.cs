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
using Microsoft.TeamFoundation.TestManagement.Client;
using System.ComponentModel;

namespace TFSAPI_TestPlans
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TfsTeamProjectCollection _tfs;
        private ITestManagementTeamProject _testproject;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_connect_Click(object sender, RoutedEventArgs e)
        {
            TeamProjectPicker tpp = new TeamProjectPicker(TeamProjectPickerMode.SingleProject, false);
            tpp.ShowDialog();

            if (tpp.SelectedTeamProjectCollection != null)
            {
                this._tfs = tpp.SelectedTeamProjectCollection;
                ITestManagementService test_service = (ITestManagementService)_tfs.GetService(typeof(ITestManagementService));
                this._testproject = test_service.GetTeamProject(tpp.SelectedProjects[0].Name);

                GetTestPlans();

                btn_add.IsEnabled = true;
                btn_remove_plan.IsEnabled = true;
            }
        }

        void GetTestPlans()
        {
            ITestPlanCollection plans = _testproject.TestPlans.Query("Select * From TestPlan");
            this.list_plans.ItemsSource = plans;

            //_testproject.TestPlans.Find("int - If you know the id...");
        }

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            ITestPlan plan = _testproject.TestPlans.Create();
            plan.Name = txt_plan_name.Text;
            //plan.AreaPath, plan.Description,plan.EndDate,plan.StartDate,plan.State
            plan.Save();

            txt_plan_name.Text = string.Empty;

            GetTestPlans();
            this.list_plans.SelectedItem = plan;
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
                MessageBox.Show("Please select plan");
        }
    }
}

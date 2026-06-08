using finanzen_sql.Tables;
using finanzen_sql.utils;
using MySqlConnector;
using System.Windows;
using System.Windows.Controls;


namespace finanzen_sql.Builds;

/// <summary>
/// Interaction logic for OverviewPanelBuild.xaml
/// </summary>
public partial class OverviewPanelBuild : UserControl
{
    private DatabaseUtils _dbUtils = new();
    public User? user { get; set; }
    public OverviewPanelBuild()
    {
        InitializeComponent();
    }


}

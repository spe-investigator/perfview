using System.Windows;
using Dir = System.IO.Directory;

namespace PerfView {
    public partial class MainWindow {
        const string connect = "Connect Azure";
        const string disconnect = "Azure Disconnected";
        
        private void ConnectAzure_Click(object sender, RoutedEventArgs e) {
            AzureConnect.Content = AzureConnect.Content == connect ? disconnect : connect; 
            
            if (ConnectAzure()) {
                Title = "Azure Connected";
                var children = m_CurrentDirectory.Children;
                TreeView.ItemsSource = children;
            } else {
                Title = "Azure Disconnected";
            }
        }

        private void AddAzure_Click(object sender, RoutedEventArgs e) {
            AddAzureConnection();
        }

        public bool ConnectAzure() {
            var isConnected = AzureConnect.Content != connect;

            if (isConnected) {
                m_CurrentDirectory = null;
                return false;
            }

            var configurationPath = Dir.GetCurrentDirectory();
            m_CurrentDirectory = new PerfViewAzureConfiguration(configurationPath);
            return true;
        }

        public void AddAzureConnection() {
            if (!(m_CurrentDirectory is PerfViewAzureConfiguration)) {
                StatusBar.Log("Connect to Azure, first.");
                return;
            }

            //var azureUrl = new InputBox().ShowDialog();
            //var azureSecret = new InputBox().ShowDialog();
        }
    }

    internal class InputBox {
        public InputBox() {
        }

        public string ShowDialog() {
            return string.Empty;
        }
    }
}

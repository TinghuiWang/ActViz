using ActViz.Helpers;
using ActViz.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Storage;

namespace ActViz.ViewModels
{
    public class DatabaseConnectionViewModel : ObservableObject
    {
        private Logger appLog;
        private CasasDatabaseService casasDBService;
        private readonly string dbCredentialKey = "ActViz_DB_Credentials";
        private readonly string sshCredentialKey = "ActViz_SSH_Credentials";
        private PasswordVault vault;

        public DatabaseConnectionViewModel(CasasDatabaseService casasDBService) : base()
        {
            this.casasDBService = casasDBService;
            appLog = Logger.Instance;
            vault = new PasswordVault();
            LoadFromLocalSettings();
        }

        private bool _isCredentialsSaved;
        public bool IsCredentialSaved
        {
            get { return _isCredentialsSaved; }
            set { SetProperty(ref _isCredentialsSaved, value); }
        }

        private string _dbServer;
        public string DbServer
        {
            get { return _dbServer; }
            set { SetProperty(ref _dbServer, value); }
        }

        private string _dbPort;
        public string DbPort
        {
            get { return _dbPort; }
            set { SetProperty(ref _dbPort, value); }
        }

        private string _dbUsername;
        public string DbUsername
        {
            get { return _dbUsername; }
            set { SetProperty(ref _dbUsername, value); }
        }

        private string _dbPassword;
        public string DbPassword
        {
            get { return _dbPassword; }
            set { SetProperty(ref _dbPassword, value); }
        }

        private bool _isSshEnabled;
        public bool IsSshEnabled
        {
            get { return _isSshEnabled; }
            set { SetProperty(ref _isSshEnabled, value); }
        }

        private string _sshServer;
        public string SshServer
        {
            get { return _sshServer; }
            set { SetProperty(ref _sshServer, value); }
        }

        private string _sshUsername;
        public string SshUsername
        {
            get { return _sshUsername; }
            set { SetProperty(ref _sshUsername, value); }
        }

        private string _sshPassword;
        public string SshPassword
        {
            get { return _sshPassword; }
            set { SetProperty(ref _sshPassword, value); }
        }

        private T RetrieveFromSettings<T>(string key, T defaultValue, ApplicationDataContainer localSettings = null)
        {
            if (localSettings == null) localSettings = ApplicationData.Current.LocalSettings;
            try
            {
                return (T)localSettings.Values[key];
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        private void LoadFromLocalSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            DbServer = RetrieveFromSettings("DbServer", "", localSettings);
            DbPort = RetrieveFromSettings("DbPort", "", localSettings);
            DbUsername = RetrieveFromSettings("DbUsername", "", localSettings);
            IsCredentialSaved = RetrieveFromSettings("IsCredentialSaved", false, localSettings);
            IsSshEnabled = RetrieveFromSettings("IsSshEnabled", false, localSettings);
            SshServer = RetrieveFromSettings("SshServer", "", localSettings);
            SshUsername = RetrieveFromSettings("SshUsername", "", localSettings);
            if (IsCredentialSaved)
            {
                PasswordCredential credential = vault.Retrieve(dbCredentialKey, DbUsername);
                DbPassword = (credential == null) ? "" : credential.Password;
                if (IsSshEnabled)
                {
                    credential = vault.Retrieve(sshCredentialKey, SshUsername);
                    SshPassword = (credential == null) ? "" : credential.Password;
                }
            }
        }

        public void SaveToLocalSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["DbServer"] = DbServer;
            localSettings.Values["DbPort"] = DbPort;
            localSettings.Values["DbUsername"] = DbUsername;
            localSettings.Values["IsSshEnabled"] = IsSshEnabled;
            if (IsSshEnabled)
            {
                localSettings.Values["SshServer"] = SshServer;
                localSettings.Values["SshUsername"] = SshUsername;
            }
            localSettings.Values["IsCredentialSaved"] = IsCredentialSaved;
            if (IsCredentialSaved)
            {
                vault.Add(new PasswordCredential(dbCredentialKey, DbUsername, DbPassword));
                if (IsSshEnabled)
                {
                    vault.Add(new PasswordCredential(sshCredentialKey, SshUsername, SshPassword));
                }
            }
            else
            {
                IReadOnlyList<PasswordCredential> credentialList;
                credentialList = vault.FindAllByResource(dbCredentialKey);
                foreach (PasswordCredential credential in credentialList) vault.Remove(credential);
                credentialList = vault.FindAllByResource(sshCredentialKey);
                foreach (PasswordCredential credential in credentialList) vault.Remove(credential);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// Returns true if connection is succeeded. Returns false otherwise.
        /// </returns>
        public bool TryDBConnect()
        {
            appLog.Info(this.GetType().ToString(), "Connect To Database Asynchronously.");
            appLog.Debug(this.GetType().ToString(), this.ToString());
            casasDBService.IsSshEnabled = IsSshEnabled;
            casasDBService.SshServer = SshServer;
            casasDBService.SshPort = 22;
            casasDBService.SshUsername = SshUsername;
            casasDBService.SshPassword = SshPassword;
            casasDBService.DbServer = DbServer;
            casasDBService.DbPort = int.Parse(DbPort, System.Globalization.NumberStyles.Integer);
            casasDBService.DbUsername = DbUsername;
            casasDBService.DbPassword = DbPassword;
            try
            {
                casasDBService.Start();
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public void CloseConnection()
        {
            appLog.Info(this.GetType().ToString(), "Closing Connections to Server...");
            casasDBService.Stop();
        }

        public override string ToString()
        {
            return string.Format("DB Server: {0} \nDB Port: {1}\nDB Username: {2}\nSSH Enabled: {3}\nSSH Server: {4}\nSSH Username: {5}\n",
                DbServer, DbPort, DbUsername, IsSshEnabled.ToString(), SshServer, SshUsername
                );
        }
    }
}

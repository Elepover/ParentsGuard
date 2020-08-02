namespace ParentsGuard
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.serviceProcessInstallerBlocking = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstallerBlocking = new System.ServiceProcess.ServiceInstaller();
            this.eventLogInstallerPrngBlocking = new System.Diagnostics.EventLogInstaller();
            this.eventLogInstallerPrng = new System.Diagnostics.EventLogInstaller();
            // 
            // serviceProcessInstallerBlocking
            // 
            this.serviceProcessInstallerBlocking.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.serviceProcessInstallerBlocking.Password = null;
            this.serviceProcessInstallerBlocking.Username = null;
            // 
            // serviceInstallerBlocking
            // 
            this.serviceInstallerBlocking.Description = "Service for blocking installation, execution and downloads of blocked executables" +
    ".";
            this.serviceInstallerBlocking.DisplayName = "Parents\' Guard Blocking Service";
            this.serviceInstallerBlocking.ServiceName = "prng-blocking";
            this.serviceInstallerBlocking.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // eventLogInstallerPrngBlocking
            // 
            this.eventLogInstallerPrngBlocking.CategoryCount = 0;
            this.eventLogInstallerPrngBlocking.CategoryResourceFile = null;
            this.eventLogInstallerPrngBlocking.Log = "Application";
            this.eventLogInstallerPrngBlocking.MessageResourceFile = null;
            this.eventLogInstallerPrngBlocking.ParameterResourceFile = null;
            this.eventLogInstallerPrngBlocking.Source = "prng-blocking";
            // 
            // eventLogInstallerPrng
            // 
            this.eventLogInstallerPrng.CategoryCount = 0;
            this.eventLogInstallerPrng.CategoryResourceFile = null;
            this.eventLogInstallerPrng.Log = "Application";
            this.eventLogInstallerPrng.MessageResourceFile = null;
            this.eventLogInstallerPrng.ParameterResourceFile = null;
            this.eventLogInstallerPrng.Source = "prng";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstallerBlocking,
            this.serviceInstallerBlocking,
            this.eventLogInstallerPrngBlocking,
            this.eventLogInstallerPrng});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstallerBlocking;
        private System.ServiceProcess.ServiceInstaller serviceInstallerBlocking;
        private System.Diagnostics.EventLogInstaller eventLogInstallerPrngBlocking;
        private System.Diagnostics.EventLogInstaller eventLogInstallerPrng;
    }
}
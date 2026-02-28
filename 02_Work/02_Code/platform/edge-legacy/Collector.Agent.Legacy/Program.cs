using System;
using System.Threading;
using System.Windows.Forms;
using Collector.Agent.Legacy.Forms;
using Collector.Agent.Legacy.Services;
using NLog;

namespace Collector.Agent.Legacy
{
    internal static class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static Mutex _mutex;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 单实例检测
            const string mutexName = "Collector.Agent.Legacy.SingleInstance";
            bool createdNew;
            _mutex = new Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("采集器程序已在运行中！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // 配置全局异常处理
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // 初始化语言管理器
                InitializeLanguage();

                // 初始化 DI 容器（需要 LocalSettings 中的网关地址等）
                InitializeServiceLocator();

                Logger.Info("========== 采集器程序启动 ==========");

                // 显示登录窗口
                using (var loginForm = new LoginForm())
                {
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        // 登录成功，显示主窗口
                        Application.Run(new MainForm());
                    }
                }

                Logger.Info("========== 采集器程序退出 ==========");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "程序启动失败");
                MessageBox.Show($"程序启动失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ServiceLocator.Dispose();
                _mutex?.ReleaseMutex();
                _mutex?.Dispose();
                LogManager.Shutdown();
            }
        }

        /// <summary>
        /// 初始化 DI 服务容器
        /// </summary>
        private static void InitializeServiceLocator()
        {
            try
            {
                var settings = AppService.Instance.LoadSettings() ?? new LocalSettings();
                ServiceLocator.Initialize(settings);
                Logger.Info("DI 容器初始化完成");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "DI 容器初始化失败");
                // 使用默认设置重试
                ServiceLocator.Initialize(new LocalSettings());
            }
        }

        /// <summary>
        /// 初始化语言设置
        /// </summary>
        private static void InitializeLanguage()
        {
            try
            {
                var settings = AppService.Instance.LoadSettings();
                var languageCode = settings?.Language ?? "";
                var setting = LanguageManager.ParseSetting(languageCode);
                LanguageManager.Initialize(setting);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "加载语言设置失败，使用默认语言");
                LanguageManager.Initialize(LanguageManager.LanguageSetting.System);
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Logger.Error(e.Exception, "UI线程未处理异常");
            MessageBox.Show($"发生错误：{e.Exception.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Logger.Fatal(ex, "应用程序域未处理异常");
            if (e.IsTerminating)
            {
                MessageBox.Show($"发生严重错误，程序即将退出：{ex?.Message}", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

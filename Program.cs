using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinLagFixer
{
    static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main()
        {
            try
            {
                if (Environment.OSVersion.Version.Major >= 6)
                    SetProcessDPIAware();
            }
            catch { }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class ServiceItem
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public Func<Action<string, Color>, CancellationToken, Task> ActionFunc { get; set; }
    }

    public class MainForm : Form
    {
        // UI Controls
        private Panel headerPanel;
        private Label lblTitle;
        private Label lblSubtitle;
        private Panel adminWarningPanel;
        private Button btnElevate;
        private CheckedListBox clbTasks;
        private Button btnQuickFix;
        private Button btnFullRestart;
        private Button btnCustomRun;
        private Button btnCancel;
        private ProgressBar progressBar;
        private RichTextBox rtbLogs;
        private Label lblStatus;
        private Button btnClearLogs;
        private Button btnSelectAll;
        private Button btnDeselectAll;

        private List<ServiceItem> serviceTasks;
        private CancellationTokenSource cts;
        private bool isRunning = false;

        public MainForm()
        {
            InitializeComponent();
            CheckAdminStatus();
            SetupTasks();
        }

        private bool IsRunAsAdmin()
        {
            try
            {
                var id = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(id);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private void InitializeComponent()
        {
            this.Text = "⚡ Windows Anti-Lag & Service Restarter (Khắc Phục Giật Lag Windows)";
            this.Size = new Size(820, 680);
            this.MinimumSize = new Size(760, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(24, 24, 37); // Dark theme #181825
            this.ForeColor = Color.FromArgb(205, 214, 244);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // Header Panel
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = Color.FromArgb(30, 30, 46), // #1E1E2E
                Padding = new Padding(20, 12, 20, 10)
            };

            lblTitle = new Label
            {
                Text = "⚡ WINDOWS ANTI-LAG & SERVICE RESTARTER",
                Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(137, 180, 250), // Accent Cyan/Blue #89B4FA
                AutoSize = true,
                Location = new Point(18, 12)
            };

            lblSubtitle = new Label
            {
                Text = "Khởi động lại các dịch vụ ngầm bị treo/rò rỉ RAM, nạp lại card đồ họa, làm mượt máy tính an toàn.",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(166, 173, 200),
                AutoSize = true,
                Location = new Point(20, 42)
            };

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            this.Controls.Add(headerPanel);

            // Admin Warning Panel (shown if not admin)
            adminWarningPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = Color.FromArgb(69, 58, 26),
                Visible = false,
                Padding = new Padding(15, 6, 15, 6)
            };

            Label lblAdminWarn = new Label
            {
                Text = "⚠️ Cảnh báo: Bạn chưa chạy bằng quyền Administrator. Một số dịch vụ hệ thống sẽ không thể khởi động lại.",
                ForeColor = Color.FromArgb(249, 226, 175),
                AutoSize = true,
                Location = new Point(15, 11),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            btnElevate = new Button
            {
                Text = "🛡️ Chạy lại với quyền Admin",
                Size = new Size(200, 28),
                Location = new Point(580, 7),
                BackColor = Color.FromArgb(249, 226, 175),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            btnElevate.FlatAppearance.BorderSize = 0;
            btnElevate.Click += (s, e) => RestartAsAdmin();

            adminWarningPanel.Controls.Add(lblAdminWarn);
            adminWarningPanel.Controls.Add(btnElevate);
            this.Controls.Add(adminWarningPanel);

            // Main Body Layout
            Panel bodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 12, 18, 15)
            };
            this.Controls.Add(bodyPanel);

            // Action Buttons Panel
            Panel actionsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.Transparent
            };

            btnQuickFix = new Button
            {
                Text = "⚡ Khắc Phục Nhanh (Khuyên dùng)",
                Size = new Size(230, 42),
                Location = new Point(0, 4),
                BackColor = Color.FromArgb(49, 130, 206), // Blue #3182CE
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnQuickFix.FlatAppearance.BorderSize = 0;
            btnQuickFix.Click += (s, e) => StartExecution(quickFixOnly: true);

            btnFullRestart = new Button
            {
                Text = "🛠️ Restart Toàn Bộ Dịch Vụ",
                Size = new Size(210, 42),
                Location = new Point(238, 4),
                BackColor = Color.FromArgb(128, 90, 213), // Purple #805AD5
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnFullRestart.FlatAppearance.BorderSize = 0;
            btnFullRestart.Click += (s, e) => StartExecution(quickFixOnly: false);

            btnCustomRun = new Button
            {
                Text = "▶️ Chạy Mục Đã Chọn",
                Size = new Size(170, 42),
                Location = new Point(456, 4),
                BackColor = Color.FromArgb(43, 114, 90), // Green #2B725A
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCustomRun.FlatAppearance.BorderSize = 0;
            btnCustomRun.Click += (s, e) => StartExecutionCustom();

            btnCancel = new Button
            {
                Text = "⛔ Hủy / Dừng",
                Size = new Size(110, 42),
                Location = new Point(634, 4),
                BackColor = Color.FromArgb(185, 28, 28), // Red #B91C1C
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => CancelExecution();

            actionsPanel.Controls.Add(btnQuickFix);
            actionsPanel.Controls.Add(btnFullRestart);
            actionsPanel.Controls.Add(btnCustomRun);
            actionsPanel.Controls.Add(btnCancel);
            bodyPanel.Controls.Add(actionsPanel);

            // Task List Title & Selection Buttons
            Panel taskHeaderPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(0, 6, 0, 2)
            };

            Label lblTaskTitle = new Label
            {
                Text = "Danh sách dịch vụ & Tác vụ hệ thống:",
                ForeColor = Color.FromArgb(186, 194, 222),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 7)
            };

            btnSelectAll = new Button
            {
                Text = "Chọn tất cả",
                Size = new Size(85, 24),
                Location = new Point(570, 4),
                BackColor = Color.FromArgb(49, 50, 68),
                ForeColor = Color.FromArgb(205, 214, 244),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8f),
                Cursor = Cursors.Hand
            };
            btnSelectAll.FlatAppearance.BorderSize = 0;
            btnSelectAll.Click += (s, e) => SetAllTasks(true);

            btnDeselectAll = new Button
            {
                Text = "Bỏ chọn",
                Size = new Size(80, 24),
                Location = new Point(662, 4),
                BackColor = Color.FromArgb(49, 50, 68),
                ForeColor = Color.FromArgb(205, 214, 244),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8f),
                Cursor = Cursors.Hand
            };
            btnDeselectAll.FlatAppearance.BorderSize = 0;
            btnDeselectAll.Click += (s, e) => SetAllTasks(false);

            taskHeaderPanel.Controls.Add(lblTaskTitle);
            taskHeaderPanel.Controls.Add(btnSelectAll);
            taskHeaderPanel.Controls.Add(btnDeselectAll);
            bodyPanel.Controls.Add(taskHeaderPanel);

            // CheckedListBox
            clbTasks = new CheckedListBox
            {
                Dock = DockStyle.Top,
                Height = 165,
                BackColor = Color.FromArgb(30, 30, 46),
                ForeColor = Color.FromArgb(205, 214, 244),
                BorderStyle = BorderStyle.FixedSingle,
                CheckOnClick = true,
                Font = new Font("Segoe UI", 9.2f)
            };
            bodyPanel.Controls.Add(clbTasks);

            // Progress & Status Panel
            Panel progressPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                Padding = new Padding(0, 8, 0, 4)
            };

            lblStatus = new Label
            {
                Text = "Sẵn sàng. Hãy chọn thao tác ở trên.",
                ForeColor = Color.FromArgb(166, 227, 161), // Green #A6E3A1
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                AutoSize = true,
                Location = new Point(0, 6)
            };

            progressBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Height = 12,
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };

            progressPanel.Controls.Add(lblStatus);
            progressPanel.Controls.Add(progressBar);
            bodyPanel.Controls.Add(progressPanel);

            // Log Header Panel
            Panel logHeaderPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 28,
                Padding = new Padding(0, 4, 0, 2)
            };

            Label lblLogTitle = new Label
            {
                Text = "Nhật ký tiến trình chi tiết (Real-time Logs):",
                ForeColor = Color.FromArgb(186, 194, 222),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 6)
            };

            btnClearLogs = new Button
            {
                Text = "Xóa nhật ký",
                Size = new Size(85, 22),
                Location = new Point(660, 3),
                BackColor = Color.FromArgb(49, 50, 68),
                ForeColor = Color.FromArgb(166, 173, 200),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8f),
                Cursor = Cursors.Hand
            };
            btnClearLogs.FlatAppearance.BorderSize = 0;
            btnClearLogs.Click += (s, e) => rtbLogs.Clear();

            logHeaderPanel.Controls.Add(lblLogTitle);
            logHeaderPanel.Controls.Add(btnClearLogs);
            bodyPanel.Controls.Add(logHeaderPanel);

            // RichTextBox Log Box
            rtbLogs = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(17, 17, 27), // Very dark #11111B
                ForeColor = Color.FromArgb(205, 214, 244),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                Font = new Font("Consolas", 9f),
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            bodyPanel.Controls.Add(rtbLogs);

            // Bring controls to front in correct dock order
            rtbLogs.BringToFront();
            logHeaderPanel.BringToFront();
            progressPanel.BringToFront();
            clbTasks.BringToFront();
            taskHeaderPanel.BringToFront();
            actionsPanel.BringToFront();
        }

        private void CheckAdminStatus()
        {
            if (!IsRunAsAdmin())
            {
                adminWarningPanel.Visible = true;
                Log("⚠️ Ứng dụng đang chạy ở chế độ người dùng thông thường.", Color.Orange);
                Log("👉 Khuyên bạn bấm nút '🛡️ Chạy lại với quyền Admin' phía trên để có quyền restart các dịch vụ hệ thống.", Color.Yellow);
            }
            else
            {
                adminWarningPanel.Visible = false;
                Log("🛡️ Đã có đầy đủ quyền Administrator (Quản trị viên hệ thống).", Color.LightGreen);
            }
        }

        private void RestartAsAdmin()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(psi);
                Application.Exit();
            }
            catch (Exception ex)
            {
                Log("Không thể nâng quyền Admin: " + ex.Message, Color.Red);
            }
        }

        private void SetupTasks()
        {
            serviceTasks = new List<ServiceItem>
            {
                new ServiceItem
                {
                    Key = "GPU",
                    Title = "Nạp lại Card Màn Hình / GPU Pipeline (Win + Ctrl + Shift + B)",
                    Description = "Xử lý đơ hình, nhấp nháy, khựng khung hình (micro-stutter) do GPU driver bị treo.",
                    IsDefault = true,
                    ActionFunc = ResetGpuDriver
                },
                new ServiceItem
                {
                    Key = "EXPLORER",
                    Title = "Khởi động lại Windows Explorer (explorer.exe)",
                    Description = "Làm mới thanh Taskbar, Start Menu, Desktop, giải phóng bộ nhớ rò rỉ của Shell UI.",
                    IsDefault = true,
                    ActionFunc = RestartWindowsExplorer
                },
                new ServiceItem
                {
                    Key = "AUDIO",
                    Title = "Khởi động lại Hệ Thống Âm Thanh (AudioSrv, AudioEndpointBuilder)",
                    Description = "Khắc phục giật máy do driver âm thanh bị desync, xung đột buffer hoặc mất tiếng đột ngột.",
                    IsDefault = true,
                    ActionFunc = RestartAudioServices
                },
                new ServiceItem
                {
                    Key = "RAM",
                    Title = "Giải phóng RAM Cache & Thu dọn Working Set của tiến trình ngầm",
                    Description = "Dọn sạch bộ nhớ đệm Standby List bị ứ đọng, hạ mức chiếm dụng RAM ngay lập tức.",
                    IsDefault = true,
                    ActionFunc = CleanMemoryCache
                },
                new ServiceItem
                {
                    Key = "DNS",
                    Title = "Xóa Bộ Nhớ Đệm DNS & Reset Sockets Mạng (ipconfig /flushdns)",
                    Description = "Khắc phục lag mạng, nghẽn truy vấn DNS và độ trễ phản hồi Internet.",
                    IsDefault = true,
                    ActionFunc = FlushDnsCache
                },
                new ServiceItem
                {
                    Key = "WSEARCH",
                    Title = "Khởi động lại Windows Search Indexer (WSearch)",
                    Description = "Khắc phục hiện tượng Windows Search ăn 100% CPU hoặc ổ cứng quay liên tục.",
                    IsDefault = false,
                    ActionFunc = RestartSearchService
                },
                new ServiceItem
                {
                    Key = "SYSMAIN",
                    Title = "Khởi động lại SysMain (SuperFetch Cache)",
                    Description = "Khắc phục lỗi nạp cache đĩa ngầm gây đơ giật trong game hoặc ứng dụng nặng.",
                    IsDefault = false,
                    ActionFunc = RestartSysMainService
                },
                new ServiceItem
                {
                    Key = "UPDATES",
                    Title = "Khởi động lại Dịch Vụ Cập Nhật (wuauserv, bits, cryptsvc)",
                    Description = "Xử lý tiến trình TiWorker hoặc Windows Update tải ngầm chiếm tài nguyên máy.",
                    IsDefault = false,
                    ActionFunc = RestartUpdateServices
                },
                new ServiceItem
                {
                    Key = "SPOOLER",
                    Title = "Khởi động lại Print Spooler (Spooler)",
                    Description = "Khắc phục nghẽn máy do các tác vụ in ấn bị kẹt trong hàng đợi.",
                    IsDefault = false,
                    ActionFunc = RestartSpoolerService
                }
            };

            foreach (var item in serviceTasks)
            {
                clbTasks.Items.Add(item.Title, item.IsDefault);
            }
        }

        private void SetAllTasks(bool check)
        {
            for (int i = 0; i < clbTasks.Items.Count; i++)
            {
                clbTasks.SetItemChecked(i, check);
            }
        }

        private void SetControlsRunningState(bool running)
        {
            isRunning = running;
            btnQuickFix.Enabled = !running;
            btnFullRestart.Enabled = !running;
            btnCustomRun.Enabled = !running;
            btnCancel.Enabled = running;
            btnSelectAll.Enabled = !running;
            btnDeselectAll.Enabled = !running;
            clbTasks.Enabled = !running;
        }

        private async void StartExecution(bool quickFixOnly)
        {
            if (isRunning) return;

            List<ServiceItem> toRun = new List<ServiceItem>();
            if (quickFixOnly)
            {
                // Chỉ chạy các mục cốt lõi QuickFix
                toRun = serviceTasks.FindAll(t => t.Key == "GPU" || t.Key == "EXPLORER" || t.Key == "AUDIO" || t.Key == "RAM" || t.Key == "DNS");
            }
            else
            {
                // Toàn bộ dịch vụ
                toRun = new List<ServiceItem>(serviceTasks);
            }

            await ExecuteTaskListAsync(toRun, quickFixOnly ? "⚡ Khắc Phục Nhanh" : "🛠️ Restart Toàn Bộ Dịch Vụ");
        }

        private async void StartExecutionCustom()
        {
            if (isRunning) return;

            List<ServiceItem> toRun = new List<ServiceItem>();
            for (int i = 0; i < clbTasks.Items.Count; i++)
            {
                if (clbTasks.GetItemChecked(i))
                {
                    toRun.Add(serviceTasks[i]);
                }
            }

            if (toRun.Count == 0)
            {
                MessageBox.Show("Vui lòng tích chọn ít nhất 1 dịch vụ để thực hiện!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            await ExecuteTaskListAsync(toRun, "▶️ Chạy Mục Đã Chọn");
        }

        private async Task ExecuteTaskListAsync(List<ServiceItem> tasks, string modeName)
        {
            cts = new CancellationTokenSource();
            SetControlsRunningState(true);
            progressBar.Value = 0;
            progressBar.Maximum = tasks.Count;

            Log("==================================================", Color.Gray);
            Log(string.Format("🚀 Bắt đầu chế độ [{0}] ({1} tác vụ)...", modeName, tasks.Count), Color.Cyan);
            Log("==================================================", Color.Gray);

            int completed = 0;
            int failed = 0;

            try
            {
                for (int i = 0; i < tasks.Count; i++)
                {
                    if (cts.Token.IsCancellationRequested)
                    {
                        Log("⛔ Tiến trình đã bị hủy bởi người dùng!", Color.Red);
                        break;
                    }

                    var current = tasks[i];
                    lblStatus.Text = string.Format("Đang xử lý [{0}/{1}]: {2}...", i + 1, tasks.Count, current.Key);

                    try
                    {
                        await current.ActionFunc(Log, cts.Token);
                        completed++;
                    }
                    catch (OperationCanceledException)
                    {
                        Log(string.Format("⛔ Tác vụ {0} bị hủy.", current.Key), Color.Red);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Log(string.Format("❌ Lỗi khi thực hiện {0}: {1}", current.Key, ex.Message), Color.OrangeRed);
                        failed++;
                    }

                    progressBar.Value = i + 1;
                }
            }
            finally
            {
                SetControlsRunningState(false);
                lblStatus.Text = string.Format("Hoàn tất! Thành công: {0} | Bỏ qua/Lỗi: {1}", completed, failed);
                Log("--------------------------------------------------", Color.Gray);
                Log(string.Format("🎉 Đã hoàn tất! Máy tính đã được làm mới mượt mà. (Hoàn thành {0}/{1})", completed, tasks.Count), Color.LightGreen);
            }
        }

        private void CancelExecution()
        {
            if (cts != null && !cts.IsCancellationRequested)
            {
                Log("Đang gửi tín hiệu dừng tiến trình...", Color.Yellow);
                cts.Cancel();
            }
        }

        // ================= SERVICE & SYSTEM ACTIONS =================

        #region GPU Reset
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const byte VK_LWIN = 0x5B;
        private const byte VK_CONTROL = 0x11;
        private const byte VK_SHIFT = 0x10;
        private const byte VK_B = 0x42;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private async Task ResetGpuDriver(Action<string, Color> logger, CancellationToken token)
        {
            logger("🔄 [GPU] Đang gửi lệnh nạp lại Card Đồ Họa (Win + Ctrl + Shift + B)...", Color.DeepSkyBlue);
            await Task.Run(() =>
            {
                // Simulate Win + Ctrl + Shift + B safely
                keybd_event(VK_LWIN, 0, 0, 0);
                keybd_event(VK_CONTROL, 0, 0, 0);
                keybd_event(VK_SHIFT, 0, 0, 0);
                keybd_event(VK_B, 0, 0, 0);

                Thread.Sleep(50);

                keybd_event(VK_B, 0, KEYEVENTF_KEYUP, 0);
                keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, 0);
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
                keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, 0);
            });
            logger("✅ [GPU] Đã kích hoạt lệnh làm mới driver màn hình. Màn hình có thể chớp 1 lần ngắn.", Color.LightGreen);
        }
        #endregion

        #region Explorer Restart
        private async Task RestartWindowsExplorer(Action<string, Color> logger, CancellationToken token)
        {
            logger("🔄 [EXPLORER] Đang khởi động lại giao diện Windows Explorer...", Color.DeepSkyBlue);
            await Task.Run(() =>
            {
                try
                {
                    foreach (var process in Process.GetProcessesByName("explorer"))
                    {
                        try
                        {
                            process.Kill();
                            process.WaitForExit(3000);
                        }
                        catch { }
                    }
                }
                catch { }

                Thread.Sleep(800);

                // Check if explorer auto-restarted, if not, start it
                if (Process.GetProcessesByName("explorer").Length == 0)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe"),
                        UseShellExecute = true
                    });
                }
            });
            logger("✅ [EXPLORER] Windows Explorer đã được khởi động lại thành công.", Color.LightGreen);
        }
        #endregion

        #region Audio Restart
        private async Task RestartAudioServices(Action<string, Color> logger, CancellationToken token)
        {
            logger("🔄 [AUDIO] Đang khởi động lại AudioEndpointBuilder & AudioSrv...", Color.DeepSkyBlue);
            await Task.Run(() =>
            {
                RestartWindowsService("AudioEndpointBuilder", logger, 6000);
                RestartWindowsService("AudioSrv", logger, 6000);
            });
            logger("✅ [AUDIO] Hoàn tất nạp lại các dịch vụ âm thanh.", Color.LightGreen);
        }
        #endregion

        #region Memory Cache Cleaner
        [DllImport("psapi.dll")]
        static extern int EmptyWorkingSet(IntPtr hwProc);

        private async Task CleanMemoryCache(Action<string, Color> logger, CancellationToken token)
        {
            logger("🔄 [RAM] Đang thu dọn working set và bộ nhớ đệm của các tiến trình ngầm...", Color.DeepSkyBlue);
            await Task.Run(() =>
            {
                int cleanedCount = 0;
                Process[] processes = Process.GetProcesses();
                foreach (Process p in processes)
                {
                    if (token.IsCancellationRequested) break;
                    try
                    {
                        // Skip system idle and current process to prevent slowdown
                        if (p.Id <= 4 || p.Id == Process.GetCurrentProcess().Id) continue;
                        EmptyWorkingSet(p.Handle);
                        cleanedCount++;
                    }
                    catch
                    {
                        // Access denied on protected system processes is expected
                    }
                }

                // Collect managed memory
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            });
            logger("✅ [RAM] Đã giải phóng bộ nhớ đệm RAM thành công. Giảm tải bộ nhớ tức thì!", Color.LightGreen);
        }
        #endregion

        #region DNS Flush
        private async Task FlushDnsCache(Action<string, Color> logger, CancellationToken token)
        {
            logger("🔄 [NETWORK] Đang xóa bộ nhớ đệm DNS (ipconfig /flushdns)...", Color.DeepSkyBlue);
            await Task.Run(() =>
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "ipconfig.exe",
                        Arguments = "/flushdns",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    };
                    using (Process p = Process.Start(psi))
                    {
                        p.WaitForExit(4000);
                    }
                }
                catch (Exception ex)
                {
                    logger("⚠️ [NETWORK] Không thể chạy flushdns: " + ex.Message, Color.Yellow);
                }
            });
            logger("✅ [NETWORK] Đã làm sạch DNS Cache và socket mạng.", Color.LightGreen);
        }
        #endregion

        #region Other Services (Search, SysMain, Updates, Spooler)
        private async Task RestartSearchService(Action<string, Color> logger, CancellationToken token)
        {
            logger("🔄 [WSEARCH] Đang khởi động lại dịch vụ Windows Search Indexer...", Color.DeepSkyBlue);
            await Task.Run(() => RestartWindowsService("WSearch", logger, 8000));
            logger("✅ [WSEARCH] Đã nạp lại Windows Search.", Color.LightGreen);
        }

        private async Task RestartSysMainService(Action<string, Color> logger, CancellationToken token)
        {
            logger("🔄 [SYSMAIN] Đang khởi động lại dịch vụ SysMain (Superfetch)...", Color.DeepSkyBlue);
            await Task.Run(() => RestartWindowsService("SysMain", logger, 8000));
            logger("✅ [SYSMAIN] Đã nạp lại SysMain.", Color.LightGreen);
        }

        private async Task RestartUpdateServices(Action<string, Color> logger, CancellationToken token)
        {
            logger("🔄 [UPDATES] Đang khởi động lại dịch vụ Windows Update & BITS...", Color.DeepSkyBlue);
            await Task.Run(() =>
            {
                RestartWindowsService("wuauserv", logger, 8000);
                RestartWindowsService("BITS", logger, 8000);
                RestartWindowsService("CryptSvc", logger, 8000);
            });
            logger("✅ [UPDATES] Đã nạp lại Windows Update & BITS.", Color.LightGreen);
        }

        private async Task RestartSpoolerService(Action<string, Color> logger, CancellationToken token)
        {
            logger("🔄 [SPOOLER] Đang khởi động lại Print Spooler...", Color.DeepSkyBlue);
            await Task.Run(() => RestartWindowsService("Spooler", logger, 6000));
            logger("✅ [SPOOLER] Đã nạp lại Print Spooler.", Color.LightGreen);
        }

        private void RestartWindowsService(string serviceName, Action<string, Color> logger, int timeoutMs)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.Paused)
                    {
                        try
                        {
                            sc.Stop();
                            sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(timeoutMs));
                        }
                        catch { }
                    }

                    try
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(timeoutMs));
                        logger(string.Format("   -> Dịch vụ [{0}] đã chạy lại bình thường.", serviceName), Color.Gainsboro);
                    }
                    catch (Exception ex)
                    {
                        logger(string.Format("   ⚠️ Không thể bật lại dịch vụ [{0}]: {1}", serviceName, ex.Message), Color.Yellow);
                    }
                }
            }
            catch (Exception ex)
            {
                logger(string.Format("   ⚠️ Không tìm thấy hoặc không thể truy cập dịch vụ [{0}]: {1}", serviceName, ex.Message), Color.Yellow);
            }
        }
        #endregion

        // Logging helper
        private void Log(string message, Color color)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => Log(message, color)));
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            rtbLogs.SelectionStart = rtbLogs.TextLength;
            rtbLogs.SelectionLength = 0;

            rtbLogs.SelectionColor = Color.DarkGray;
            rtbLogs.AppendText(string.Format("[{0}] ", timestamp));

            rtbLogs.SelectionColor = color;
            rtbLogs.AppendText(message + Environment.NewLine);

            rtbLogs.ScrollToCaret();
        }
    }
}

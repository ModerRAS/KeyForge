using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeyForge.Infrastructure.Services;
using KeyForge.Core.Interfaces;
using KeyForge.Core.Models;
using KeyEventArgs = KeyForge.Core.Interfaces.KeyEventArgs;
using KeyAction = KeyForge.Core.Models.KeyAction;
using Timer = System.Windows.Forms.Timer;

namespace KeyForge.UI
{
    /// <summary>
    /// 改进的主窗体 - 使用新的服务架构
    /// 原本实现：直接使用KeyForgeApp，职责混乱
    /// 改进实现：使用服务依赖注入，清晰的职责分离
    /// </summary>
    public partial class MainFormImproved : Form
    {
        private KeyInputService _keyInputService;
        private ScriptService _scriptService;
        private Timer _updateTimer;
        private DateTime _recordingStartTime;

        // UI控件
        private Button _recordButton;
        private Button _stopRecordButton;
        private Button _playButton;
        private Button _stopButton;
        private Button _saveButton;
        private Button _loadButton;
        private Button _clearButton;
        private ListBox _actionsListBox;
        private TextBox _logTextBox;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;
        private Label _recordStatusLabel;
        private Label _playStatusLabel;
        private Label _statsLabel;

        public MainFormImproved()
        {
            InitializeComponent();
            InitializeServices();
            InitializeEventHandlers();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // 窗体设置
            this.Text = "KeyForge - 改进版本";
            this.Size = new Size(900, 700);
            this.MinimumSize = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.Icon = SystemIcons.Application;

            // 创建主界面
            CreateMainInterface();
            
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void InitializeServices()
        {
            _keyInputService = new KeyInputService();
            _scriptService = new ScriptService();
            _updateTimer = new Timer();
            _updateTimer.Interval = 100;
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void InitializeEventHandlers()
        {
            _keyInputService.KeyRecorded += OnKeyRecorded;
            _keyInputService.StatusChanged += OnStatusChanged;
            _scriptService.ActionRecorded += OnActionRecorded;
            _scriptService.ActionPlayed += OnActionPlayed;
            _scriptService.StatusChanged += OnStatusChanged;
            _scriptService.PlaybackCompleted += OnPlaybackCompleted;
            _scriptService.PlaybackStopped += OnPlaybackStopped;
        }

        private void CreateMainInterface()
        {
            // 创建主面板
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            this.Controls.Add(mainPanel);

            // 创建控制面板
            var controlPanel = CreateControlPanel();
            mainPanel.Controls.Add(controlPanel);

            // 创建动作列表面板
            var actionsPanel = CreateActionsPanel();
            mainPanel.Controls.Add(actionsPanel);

            // 创建日志面板
            var logPanel = CreateLogPanel();
            mainPanel.Controls.Add(logPanel);

            // 创建状态栏
            CreateStatusBar();
        }

        private Panel CreateControlPanel()
        {
            var panel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(860, 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle
            };

            // 录制控制
            var recordGroupBox = new GroupBox
            {
                Text = "录制控制",
                Location = new Point(10, 10),
                Size = new Size(240, 100),
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold)
            };
            panel.Controls.Add(recordGroupBox);

            _recordButton = new Button
            {
                Text = "开始录制",
                Location = new Point(10, 20),
                Size = new Size(80, 30),
                BackColor = Color.Green,
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold)
            };
            _recordButton.Click += RecordButton_Click;
            recordGroupBox.Controls.Add(_recordButton);

            _stopRecordButton = new Button
            {
                Text = "停止录制",
                Location = new Point(100, 20),
                Size = new Size(80, 30),
                BackColor = Color.Red,
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold),
                Enabled = false
            };
            _stopRecordButton.Click += StopRecordButton_Click;
            recordGroupBox.Controls.Add(_stopRecordButton);

            _recordStatusLabel = new Label
            {
                Text = "状态: 未录制",
                Location = new Point(10, 60),
                Size = new Size(200, 20),
                Font = new Font("Microsoft YaHei UI", 8F)
            };
            recordGroupBox.Controls.Add(_recordStatusLabel);

            // 播放控制
            var playGroupBox = new GroupBox
            {
                Text = "播放控制",
                Location = new Point(260, 10),
                Size = new Size(240, 100),
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold)
            };
            panel.Controls.Add(playGroupBox);

            _playButton = new Button
            {
                Text = "播放",
                Location = new Point(10, 20),
                Size = new Size(80, 30),
                BackColor = Color.Blue,
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold)
            };
            _playButton.Click += PlayButton_Click;
            playGroupBox.Controls.Add(_playButton);

            _stopButton = new Button
            {
                Text = "停止",
                Location = new Point(100, 20),
                Size = new Size(80, 30),
                BackColor = Color.Red,
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold),
                Enabled = false
            };
            _stopButton.Click += StopButton_Click;
            playGroupBox.Controls.Add(_stopButton);

            _playStatusLabel = new Label
            {
                Text = "状态: 未播放",
                Location = new Point(10, 60),
                Size = new Size(200, 20),
                Font = new Font("Microsoft YaHei UI", 8F)
            };
            playGroupBox.Controls.Add(_playStatusLabel);

            // 文件操作
            var fileGroupBox = new GroupBox
            {
                Text = "文件操作",
                Location = new Point(510, 10),
                Size = new Size(240, 100),
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold)
            };
            panel.Controls.Add(fileGroupBox);

            _saveButton = new Button
            {
                Text = "保存脚本",
                Location = new Point(10, 20),
                Size = new Size(80, 30),
                Font = new Font("Microsoft YaHei UI", 8F)
            };
            _saveButton.Click += SaveButton_Click;
            fileGroupBox.Controls.Add(_saveButton);

            _loadButton = new Button
            {
                Text = "加载脚本",
                Location = new Point(100, 20),
                Size = new Size(80, 30),
                Font = new Font("Microsoft YaHei UI", 8F)
            };
            _loadButton.Click += LoadButton_Click;
            fileGroupBox.Controls.Add(_loadButton);

            _clearButton = new Button
            {
                Text = "清空",
                Location = new Point(10, 60),
                Size = new Size(80, 30),
                Font = new Font("Microsoft YaHei UI", 8F)
            };
            _clearButton.Click += ClearButton_Click;
            fileGroupBox.Controls.Add(_clearButton);

            return panel;
        }

        private Panel CreateActionsPanel()
        {
            var panel = new Panel
            {
                Location = new Point(10, 140),
                Size = new Size(860, 200),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle
            };

            var titleLabel = new Label
            {
                Text = "按键动作列表",
                Location = new Point(10, 10),
                Size = new Size(840, 20),
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold)
            };
            panel.Controls.Add(titleLabel);

            _actionsListBox = new ListBox
            {
                Location = new Point(10, 40),
                Size = new Size(840, 150),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Font = new Font("Consolas", 8F),
                ScrollAlwaysVisible = true
            };
            panel.Controls.Add(_actionsListBox);

            _statsLabel = new Label
            {
                Text = "统计: 0 个动作",
                Location = new Point(10, 170),
                Size = new Size(840, 20),
                Font = new Font("Microsoft YaHei UI", 8F),
                ForeColor = Color.Gray
            };
            panel.Controls.Add(_statsLabel);

            return panel;
        }

        private Panel CreateLogPanel()
        {
            var panel = new Panel
            {
                Location = new Point(10, 350),
                Size = new Size(860, 280),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BorderStyle = BorderStyle.FixedSingle
            };

            var titleLabel = new Label
            {
                Text = "执行日志",
                Location = new Point(10, 10),
                Size = new Size(840, 20),
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold)
            };
            panel.Controls.Add(titleLabel);

            _logTextBox = new TextBox
            {
                Location = new Point(10, 40),
                Size = new Size(840, 230),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 8F),
                BackColor = Color.Black,
                ForeColor = Color.Lime
            };
            panel.Controls.Add(_logTextBox);

            return panel;
        }

        private void CreateStatusBar()
        {
            _statusStrip = new StatusStrip
            {
                Dock = DockStyle.Bottom,
                GripStyle = ToolStripGripStyle.Hidden
            };

            _statusLabel = new ToolStripStatusLabel
            {
                Text = "就绪",
                Spring = true
            };
            _statusStrip.Items.Add(_statusLabel);

            var versionLabel = new ToolStripStatusLabel
            {
                Text = "v2.0.0",
                Alignment = ToolStripItemAlignment.Right
            };
            _statusStrip.Items.Add(versionLabel);

            this.Controls.Add(_statusStrip);
        }

        #region 事件处理方法

        private void OnKeyRecorded(object sender, KeyEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<object, KeyEventArgs>(OnKeyRecorded), sender, e);
                return;
            }

            var action = new KeyAction(e.KeyCode, e.Key.ToString(), e.IsKeyDown);
            _scriptService.AddAction(action);
        }

        private void OnActionRecorded(object sender, KeyAction action)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<object, KeyAction>(OnActionRecorded), sender, action);
                return;
            }

            _actionsListBox.Items.Add(action.ToString());
            _actionsListBox.SelectedIndex = _actionsListBox.Items.Count - 1;
            UpdateStats();
            LogMessage($"录制: {action.KeyName} {(action.IsKeyDown ? "按下" : "释放")}");
        }

        private void OnActionPlayed(object sender, KeyAction action)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<object, KeyAction>(OnActionPlayed), sender, action);
                return;
            }

            LogMessage($"播放: {action.KeyName} {(action.IsKeyDown ? "按下" : "释放")}");
            
            // 执行按键模拟
            _keyInputService.SimulateKey((System.Windows.Forms.Keys)action.KeyCode, action.IsKeyDown);
        }

        private void OnStatusChanged(object sender, string status)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<object, string>(OnStatusChanged), sender, status);
                return;
            }

            UpdateStatus(status);
        }

        private void OnPlaybackCompleted(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnPlaybackCompleted(sender, e)));
                return;
            }

            UpdateUIState();
            LogMessage("脚本播放完成");
        }

        private void OnPlaybackStopped(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnPlaybackStopped(sender, e)));
                return;
            }

            UpdateUIState();
            LogMessage("脚本播放已停止");
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            UpdateUIState();
        }

        private void UpdateUIState()
        {
            var isRecording = _keyInputService.IsRecording;
            var isPlaying = _scriptService.IsPlaying;

            _recordButton.Enabled = !isRecording && !isPlaying;
            _stopRecordButton.Enabled = isRecording;
            _playButton.Enabled = !isRecording && !isPlaying && _actionsListBox.Items.Count > 0;
            _stopButton.Enabled = isPlaying;
            _saveButton.Enabled = !isRecording && !isPlaying && _actionsListBox.Items.Count > 0;
            _loadButton.Enabled = !isRecording && !isPlaying;
            _clearButton.Enabled = !isRecording && !isPlaying && _actionsListBox.Items.Count > 0;

            _recordStatusLabel.Text = isRecording ? "状态: 录制中..." : "状态: 未录制";
            _playStatusLabel.Text = isPlaying ? "状态: 播放中..." : "状态: 未播放";
        }

        private void UpdateStats()
        {
            var stats = _scriptService.GetStats();
            _statsLabel.Text = $"统计: {stats.TotalActions} 个动作 | 按下: {stats.KeyDownActions} | 释放: {stats.KeyUpActions} | 时长: {stats.Duration / 1000.0:F1}秒";
        }

        private void UpdateStatus(string message)
        {
            _statusLabel.Text = message;
        }

        private void LogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] {message}{Environment.NewLine}";
            _logTextBox.AppendText(logEntry);
            _logTextBox.ScrollToCaret();
        }

        private async void RecordButton_Click(object sender, EventArgs e)
        {
            try
            {
                _scriptService.StartRecording();
                _keyInputService.StartRecording();
                _recordingStartTime = DateTime.Now;
                LogMessage("开始录制按键...");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"开始录制失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StopRecordButton_Click(object sender, EventArgs e)
        {
            try
            {
                _keyInputService.StopRecording();
                _scriptService.StopRecording();
                LogMessage("录制已停止");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"停止录制失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void PlayButton_Click(object sender, EventArgs e)
        {
            try
            {
                await _scriptService.PlayAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"播放失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            try
            {
                _scriptService.StopPlayback();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"停止播放失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "脚本文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                Title = "保存脚本",
                FileName = $"keyforge_script_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _scriptService.SaveToFile(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "脚本文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                Title = "加载脚本"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _scriptService.LoadFromFile(openFileDialog.FileName);
                    RefreshActionsList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清空所有按键动作吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _scriptService.Clear();
                _actionsListBox.Items.Clear();
                UpdateStats();
                LogMessage("脚本已清空");
            }
        }

        private void RefreshActionsList()
        {
            _actionsListBox.Items.Clear();
            var actions = _scriptService.Actions;
            foreach (var action in actions)
            {
                _actionsListBox.Items.Add(action.ToString());
            }
            UpdateStats();
        }

        #endregion

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                _keyInputService?.StopRecording();
                _scriptService?.StopPlayback();
                _updateTimer?.Stop();
                _updateTimer?.Dispose();
                _keyInputService?.Dispose();
                _scriptService?.Dispose();
                LogMessage("应用程序正在关闭...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"关闭时发生错误: {ex.Message}");
            }

            base.OnFormClosing(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // 快捷键处理
            switch (keyData)
            {
                case Keys.F6:
                    if (_keyInputService.IsRecording)
                        StopRecordButton_Click(null, null);
                    else
                        RecordButton_Click(null, null);
                    return true;
                    
                case Keys.F7:
                    if (_actionsListBox.Items.Count > 0)
                        PlayButton_Click(null, null);
                    return true;
                    
                case Keys.F8:
                    StopButton_Click(null, null);
                    return true;
                    
                case Keys.Control | Keys.S:
                    if (_actionsListBox.Items.Count > 0)
                        SaveButton_Click(null, null);
                    return true;
                    
                case Keys.Control | Keys.O:
                    LoadButton_Click(null, null);
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
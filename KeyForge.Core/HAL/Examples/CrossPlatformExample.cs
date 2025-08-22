using System;
using System.Drawing;
using System.Threading.Tasks;
using KeyForge.HAL.Abstractions;
using KeyForge.HAL.Core;

// 使用类型别名解决命名空间冲突
using HALKeyEventArgs = KeyForge.HAL.Abstractions.KeyEventArgs;
using HALMouseEventArgs = KeyForge.HAL.Abstractions.MouseEventArgs;

namespace KeyForge.CrossPlatform.Examples
{
    /// <summary>
    /// 跨平台架构使用示例
    /// 展示如何使用KeyForge的硬件抽象层进行跨平台操作
    /// </summary>
    public class CrossPlatformExample
    {
        private IHardwareAbstractionLayer _hal;
        
        /// <summary>
        /// 初始化跨平台HAL
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                Console.WriteLine("正在初始化跨平台硬件抽象层...");
                
                // 自动检测平台并创建HAL实例
                _hal = HALFactory.CreateHAL();
                
                // 初始化HAL
                await _hal.InitializeAsync();
                
                Console.WriteLine($"成功初始化HAL - 平台: {_hal.PlatformInfo.Name} {_hal.PlatformInfo.Version}");
                Console.WriteLine($"架构: {_hal.PlatformInfo.Architecture}");
                Console.WriteLine($"初始化状态: {_hal.IsInitialized}");
                
                // 注册事件处理
                RegisterEvents();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 注册事件处理
        /// </summary>
        private void RegisterEvents()
        {
            // 平台事件
            _hal.PlatformChanged += OnPlatformChanged;
            _hal.HardwareStateChanged += OnHardwareStateChanged;
            
            // 键盘事件
            _hal.Keyboard.KeyPressed += OnKeyPressed;
            _hal.Keyboard.KeyReleased += OnKeyReleased;
            
            // 鼠标事件
            _hal.Mouse.MouseMoved += OnMouseMoved;
            _hal.Mouse.MouseButtonClicked += OnMouseButtonClicked;
            
            // 热键事件
            _hal.GlobalHotkeys.HotkeyPressed += OnHotkeyPressed;
            
            // 屏幕事件
            _hal.Screen.ScreenChanged += OnScreenChanged;
            
            // 窗口事件
            _hal.Window.WindowActivated += OnWindowActivated;
        }
        
        /// <summary>
        /// 键盘操作示例
        /// </summary>
        public async Task DemonstrateKeyboardOperations()
        {
            Console.WriteLine("\n=== 键盘操作示例 ===");
            
            try
            {
                // 按下和释放按键
                Console.WriteLine("按下Ctrl+A...");
                await _hal.Keyboard.PressKeyAsync(KeyCode.A, KeyModifiers.Control);
                await Task.Delay(100);
                await _hal.Keyboard.ReleaseKeyAsync(KeyCode.A, KeyModifiers.Control);
                
                // 输入文本
                Console.WriteLine("输入文本 'Hello World'...");
                await _hal.Keyboard.TypeTextAsync("Hello World");
                
                // 组合键操作
                Console.WriteLine("按下Ctrl+Shift+Esc...");
                await _hal.Keyboard.PressKeyAsync(KeyCode.Escape, KeyModifiers.Control | KeyModifiers.Shift);
                await Task.Delay(100);
                await _hal.Keyboard.ReleaseKeyAsync(KeyCode.Escape, KeyModifiers.Control | KeyModifiers.Shift);
                
                Console.WriteLine("键盘操作演示完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"键盘操作失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 鼠标操作示例
        /// </summary>
        public async Task DemonstrateMouseOperations()
        {
            Console.WriteLine("\n=== 鼠标操作示例 ===");
            
            try
            {
                // 获取当前位置
                var currentPosition = await _hal.Mouse.GetPositionAsync();
                Console.WriteLine($"当前鼠标位置: ({currentPosition.X}, {currentPosition.Y})");
                
                // 移动鼠标
                Console.WriteLine("移动鼠标到(100, 100)...");
                await _hal.Mouse.MoveToAsync(100, 100);
                await Task.Delay(500);
                
                // 平滑移动
                Console.WriteLine("平滑移动到(500, 300)...");
                await _hal.Mouse.MoveSmoothAsync(500, 300, 1000);
                await Task.Delay(500);
                
                // 点击操作
                Console.WriteLine("左键点击...");
                await _hal.Mouse.ClickAsync(MouseButton.Left);
                await Task.Delay(500);
                
                // 双击
                Console.WriteLine("双击左键...");
                await _hal.Mouse.DoubleClickAsync(MouseButton.Left);
                await Task.Delay(500);
                
                // 右键点击
                Console.WriteLine("右键点击...");
                await _hal.Mouse.ClickAsync(MouseButton.Right);
                await Task.Delay(500);
                
                // 拖拽操作
                Console.WriteLine("拖拽操作: (200, 200) -> (600, 400)...");
                await _hal.Mouse.DragDropAsync(200, 200, 600, 400, MouseButton.Left);
                
                // 滚轮操作
                Console.WriteLine("向上滚动...");
                await _hal.Mouse.ScrollAsync(3);
                await Task.Delay(500);
                
                Console.WriteLine("向下滚动...");
                await _hal.Mouse.ScrollAsync(-3);
                
                // 恢复原始位置
                await _hal.Mouse.MoveToAsync(currentPosition.X, currentPosition.Y);
                
                Console.WriteLine("鼠标操作演示完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"鼠标操作失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 屏幕操作示例
        /// </summary>
        public async Task DemonstrateScreenOperations()
        {
            Console.WriteLine("\n=== 屏幕操作示例 ===");
            
            try
            {
                // 获取屏幕信息
                var screens = await _hal.Screen.GetScreensAsync();
                Console.WriteLine($"发现 {screens.Length} 个屏幕:");
                foreach (var screen in screens)
                {
                    Console.WriteLine($"  {screen.Name}: {screen.Bounds} {(screen.IsPrimary ? "[主屏幕]" : "")}");
                }
                
                // 获取主屏幕
                var primaryScreen = await _hal.Screen.GetPrimaryScreenAsync();
                Console.WriteLine($"主屏幕: {primaryScreen.Bounds}");
                
                // 获取DPI信息
                var dpiInfo = await _hal.Screen.GetDpiInfoAsync();
                Console.WriteLine($"DPI信息: {dpiInfo}");
                
                // 截屏
                Console.WriteLine("截取屏幕...");
                var screenshot = await _hal.Screen.CaptureScreenAsync();
                Console.WriteLine($"截屏成功，尺寸: {screenshot.Width}x{screenshot.Height}");
                
                // 获取像素颜色
                var pixelColor = await _hal.Screen.GetPixelColorAsync(0, 0);
                Console.WriteLine($"(0, 0)处像素颜色: {pixelColor}");
                
                // 获取显示设置
                var displaySettings = await _hal.Screen.GetDisplaySettingsAsync();
                Console.WriteLine($"显示设置: {displaySettings}");
                
                Console.WriteLine("屏幕操作演示完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"屏幕操作失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 热键操作示例
        /// </summary>
        public async Task DemonstrateHotkeyOperations()
        {
            Console.WriteLine("\n=== 热键操作示例 ===");
            
            try
            {
                // 注册热键
                var hotkey = new HotkeyCombination
                {
                    Key = KeyCode.F1,
                    Modifiers = KeyModifiers.Control | KeyModifiers.Alt,
                    DisplayName = "Ctrl+Alt+F1"
                };
                
                Console.WriteLine($"注册热键: {hotkey.DisplayName}...");
                bool registered = await _hal.GlobalHotkeys.RegisterHotkeyAsync("demo_hotkey", hotkey, "演示热键");
                
                if (registered)
                {
                    Console.WriteLine("热键注册成功，请按下 Ctrl+Alt+F1 测试");
                    Console.WriteLine("等待5秒以测试热键...");
                    await Task.Delay(5000);
                    
                    // 注销热键
                    Console.WriteLine("注销热键...");
                    await _hal.GlobalHotkeys.UnregisterHotkeyAsync("demo_hotkey");
                }
                
                // 获取热键能力
                var capabilities = await _hal.GlobalHotkeys.GetCapabilitiesAsync();
                Console.WriteLine($"热键能力: 最大注册数={capabilities.MaxRegisteredHotkeys}, 支持系统热键={capabilities.SupportsSystemHotkeys}");
                
                Console.WriteLine("热键操作演示完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"热键操作失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 窗口操作示例
        /// </summary>
        public async Task DemonstrateWindowOperations()
        {
            Console.WriteLine("\n=== 窗口操作示例 ===");
            
            try
            {
                // 获取前台窗口
                var foregroundWindow = await _hal.Window.GetForegroundWindowAsync();
                Console.WriteLine($"前台窗口句柄: {foregroundWindow}");
                
                if (foregroundWindow != IntPtr.Zero)
                {
                    // 获取窗口信息
                    var windowInfo = await _hal.Window.GetWindowInfoAsync(foregroundWindow);
                    Console.WriteLine($"窗口信息: {windowInfo}");
                    
                    // 获取窗口边界
                    var bounds = await _hal.Window.GetWindowBoundsAsync(foregroundWindow);
                    Console.WriteLine($"窗口边界: {bounds}");
                    
                    // 获取窗口状态
                    var state = await _hal.Window.GetWindowStateAsync(foregroundWindow);
                    Console.WriteLine($"窗口状态: {state}");
                }
                
                // 查找窗口
                Console.WriteLine("查找记事本窗口...");
                var notepadWindow = await _hal.Window.FindWindowAsync("Notepad");
                if (notepadWindow != IntPtr.Zero)
                {
                    Console.WriteLine($"找到记事本窗口: {notepadWindow}");
                    
                    // 激活窗口
                    Console.WriteLine("激活窗口...");
                    await _hal.Window.ActivateWindowAsync(notepadWindow);
                    await Task.Delay(1000);
                    
                    // 移动窗口
                    Console.WriteLine("移动窗口到(100, 100)...");
                    await _hal.Window.MoveWindowAsync(notepadWindow, 100, 100);
                    await Task.Delay(500);
                    
                    // 调整窗口大小
                    Console.WriteLine("调整窗口大小到800x600...");
                    await _hal.Window.ResizeWindowAsync(notepadWindow, 800, 600);
                    await Task.Delay(500);
                }
                
                Console.WriteLine("窗口操作演示完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"窗口操作失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 图像识别示例
        /// </summary>
        public async Task DemonstrateImageRecognition()
        {
            Console.WriteLine("\n=== 图像识别示例 ===");
            
            try
            {
                // 创建测试图像
                using (var template = new Bitmap(100, 100))
                using (var graphics = Graphics.FromImage(template))
                {
                    graphics.Clear(Color.Red);
                    graphics.DrawString("Test", new Font("Arial", 12), Brushes.White, 10, 10);
                    
                    // 查找图像
                    Console.WriteLine("在屏幕上查找测试图像...");
                    var result = await _hal.ImageRecognition.FindImageAsync(template);
                    Console.WriteLine($"查找结果: {result}");
                    
                    // 保存模板
                    Console.WriteLine("保存模板...");
                    await _hal.ImageRecognition.SaveTemplateAsync("test_template", template);
                    
                    // 加载模板
                    Console.WriteLine("加载模板...");
                    var loadedTemplate = await _hal.ImageRecognition.LoadTemplateAsync("test_template");
                    Console.WriteLine($"加载的模板尺寸: {loadedTemplate.Width}x{loadedTemplate.Height}");
                    
                    // 获取所有模板名称
                    var templateNames = await _hal.ImageRecognition.GetTemplateNamesAsync();
                    Console.WriteLine($"已保存的模板: {string.Join(", ", templateNames)}");
                    
                    // 删除模板
                    Console.WriteLine("删除模板...");
                    await _hal.ImageRecognition.DeleteTemplateAsync("test_template");
                }
                
                Console.WriteLine("图像识别演示完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"图像识别失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 运行完整演示
        /// </summary>
        public async Task RunFullDemo()
        {
            try
            {
                await InitializeAsync();
                
                Console.WriteLine("\n开始跨平台硬件抽象层演示");
                Console.WriteLine("=================================");
                
                await DemonstrateKeyboardOperations();
                await DemonstrateMouseOperations();
                await DemonstrateScreenOperations();
                await DemonstrateHotkeyOperations();
                await DemonstrateWindowOperations();
                await DemonstrateImageRecognition();
                
                Console.WriteLine("\n=================================");
                Console.WriteLine("演示完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"演示过程中发生错误: {ex.Message}");
            }
            finally
            {
                await Cleanup();
            }
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        public async Task Cleanup()
        {
            if (_hal != null)
            {
                try
                {
                    await _hal.ShutdownAsync();
                    (_hal as IDisposable)?.Dispose();
                    _hal = null;
                    Console.WriteLine("HAL已清理");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"清理HAL时发生错误: {ex.Message}");
                }
            }
        }
        
        // 事件处理方法
        private void OnPlatformChanged(object sender, PlatformEventArgs e)
        {
            Console.WriteLine($"平台事件: {e.EventType} - {e.PlatformInfo.Name}");
        }
        
        private void OnHardwareStateChanged(object sender, HardwareEventArgs e)
        {
            Console.WriteLine($"硬件状态变更: {e.Operation} - {e.Success}");
        }
        
        private void OnKeyPressed(object sender, HALKeyEventArgs e)
        {
            Console.WriteLine($"按键按下: {e.Key} + {e.Modifiers}");
        }
        
        private void OnKeyReleased(object sender, HALKeyEventArgs e)
        {
            Console.WriteLine($"按键释放: {e.Key} + {e.Modifiers}");
        }
        
        private void OnMouseMoved(object sender, HALMouseEventArgs e)
        {
            Console.WriteLine($"鼠标移动: ({e.Position.X}, {e.Position.Y})");
        }
        
        private void OnMouseButtonClicked(object sender, HALMouseEventArgs e)
        {
            Console.WriteLine($"鼠标点击: {e.Button} 在 ({e.Position.X}, {e.Position.Y})");
        }
        
        private void OnHotkeyPressed(object sender, HotkeyEventArgs e)
        {
            Console.WriteLine($"热键按下: {e.Id} - {e.Hotkey.DisplayName}");
        }
        
        private void OnScreenChanged(object sender, ScreenEventArgs e)
        {
            Console.WriteLine($"屏幕变更: {e.EventType}");
        }
        
        private void OnWindowActivated(object sender, WindowEventArgs e)
        {
            Console.WriteLine($"窗口激活: {e.WindowInfo.Title}");
        }
    }
}
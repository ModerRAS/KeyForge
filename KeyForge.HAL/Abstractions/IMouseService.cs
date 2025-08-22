namespace KeyForge.HAL.Abstractions;

/// <summary>
/// 鼠标服务接口 - 提供跨平台鼠标输入功能
/// </summary>
public interface IMouseService
{
    /// <summary>
    /// 移动鼠标到指定位置
    /// </summary>
    /// <param name="x">X坐标</param>
    /// <param name="y">Y坐标</param>
    /// <returns>是否成功</returns>
    Task<bool> MoveToAsync(int x, int y);

    /// <summary>
    /// 相对移动鼠标
    /// </summary>
    /// <param name="deltaX">X轴偏移</param>
    /// <param name="deltaY">Y轴偏移</param>
    /// <returns>是否成功</returns>
    Task<bool> MoveByAsync(int deltaX, int deltaY);

    /// <summary>
    /// 模拟鼠标左键按下
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> LeftButtonDownAsync();

    /// <summary>
    /// 模拟鼠标左键释放
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> LeftButtonUpAsync();

    /// <summary>
    /// 模拟鼠标左键点击
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> LeftClickAsync();

    /// <summary>
    /// 模拟鼠标右键点击
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> RightClickAsync();

    /// <summary>
    /// 模拟鼠标中键点击
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> MiddleClickAsync();

    /// <summary>
    /// 模拟鼠标双击
    /// </summary>
    /// <returns>是否成功</returns>
    Task<bool> DoubleClickAsync();

    /// <summary>
    /// 模拟鼠标滚轮滚动
    /// </summary>
    /// <param name="delta">滚动量</param>
    /// <returns>是否成功</returns>
    Task<bool> ScrollAsync(int delta);

    /// <summary>
    /// 获取当前鼠标位置
    /// </summary>
    /// <returns>鼠标位置</returns>
    Point GetPosition();

    /// <summary>
    /// 获取鼠标按钮状态
    /// </summary>
    /// <param name="button">鼠标按钮</param>
    /// <returns>按钮状态</returns>
    MouseButtonState GetButtonState(MouseButton button);

    /// <summary>
    /// 鼠标事件
    /// </summary>
    event EventHandler<MouseEventArgs>? MouseEvent;
}
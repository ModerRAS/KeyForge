@KeyForge
@ScriptPlayback
Feature: 脚本回放功能
  作为KeyForge用户
  我想要回放录制的脚本
  以便自动化重复性任务

  @Playback
  @Basic
  Scenario: 正常速度回放
    Given 用户有一个已录制的脚本
    And 脚本包含5个键盘和鼠标操作
    And 脚本状态是"活跃"
    When 用户点击"播放"按钮
    Then 系统应该按原始速度回放脚本
    And 所有操作应该按正确顺序执行
    And 每个操作的延迟应该准确
    And 回放完成后应该停止

  @Playback
  @SpeedControl
  Scenario: 调整回放速度
    Given 用户正在回放包含延迟的脚本
    When 用户将回放速度设置为2.0x
    Then 系统应该以双倍速度回放脚本
    And 延迟时间应该减半
    When 用户将回放速度设置为0.5x
    Then 系统应该以半速回放脚本
    And 延迟时间应该加倍

  @Playback
  @Looping
  Scenario: 循环回放功能
    Given 用户有一个包含3个操作的脚本
    When 用户启用循环回放并设置循环次数为3
    Then 系统应该在脚本结束后重新开始
    And 总共应该执行9个操作
    And 循环次数应该符合用户设置
    When 用户启用无限循环
    Then 系统应该持续回放直到用户停止

  @Playback
  @Control
  Scenario: 回放过程中的暂停和恢复
    Given 系统正在回放包含10个操作的脚本
    And 当前正在执行第3个操作
    When 用户点击"暂停"按钮
    Then 系统应该暂停回放
    And 当前状态应该保存
    When 用户点击"继续"按钮
    Then 系统应该从暂停位置继续回放
    And 剩余操作应该按顺序执行

  @Playback
  @Control
  Scenario: 回放过程中的停止
    Given 系统正在回放脚本
    And 当前正在执行第5个操作
    When 用户点击"停止"按钮
    Then 系统应该立即停止回放
    And 回放位置应该重置
    And 脚本状态应该保持不变

  @Playback
  @Accuracy
  Scenario: 回放精度验证
    Given 用户有一个包含精确坐标的脚本
    When 系统回放该脚本
    Then 鼠标移动精度误差应该小于2像素
    And 时间精度误差应该小于100ms
    And 按键操作应该完全准确

  @Playback
  @ErrorHandling
  Scenario: 回放错误处理
    Given 用户有一个脚本
    And 脚本中包含无效的操作
    When 系统回放到无效操作
    Then 系统应该记录错误
    And 应该跳过无效操作继续执行
    And 应该向用户报告错误信息

  @Playback
  @Performance
  Scenario: 大型脚本回放性能
    Given 用户有一个包含1000个操作的大型脚本
    When 系统回放该脚本
    Then 回放过程应该流畅
    And 内存使用应该保持稳定
    And 系统响应时间应该小于50ms

  @Playback
  @Concurrent
  Scenario: 多脚本并发回放
    Given 用户有3个不同的脚本
    When 用户同时启动所有脚本的回放
    Then 所有脚本应该同时执行
    And 脚本之间不应该相互干扰
    And 系统性能下降应该小于20%
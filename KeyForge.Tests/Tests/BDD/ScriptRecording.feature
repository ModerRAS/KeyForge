@KeyForge
@ScriptRecording
Feature: 按键录制功能
  作为KeyForge用户
  我想要录制键盘和鼠标操作
  以便创建可重复使用的自动化脚本

  @Recording
  @Keyboard
  Scenario: 录制单键操作
    Given 用户打开了KeyForge应用程序
    And 用户点击了"开始录制"按钮
    When 用户按下键盘上的"A"键
    Then 系统应该记录按键事件
    And 录制状态应该显示为"正在录制"
    And 记录的事件类型应该是"KeyDown"

  @Recording
  @Keyboard
  Scenario: 录制组合键操作
    Given 用户正在录制脚本
    When 用户同时按下Ctrl+C组合键
    Then 系统应该记录组合键事件
    And 事件应该包含正确的按键组合
    And 事件应该包含时间戳信息

  @Recording
  @Mouse
  Scenario: 录制鼠标点击操作
    Given 用户正在录制脚本
    When 用户在坐标(100,200)处点击鼠标左键
    Then 系统应该记录鼠标点击事件
    And 事件应该包含正确的坐标和按钮信息
    And 事件类型应该是"MouseDown"后跟"MouseUp"

  @Recording
  @Control
  Scenario: 暂停和继续录制
    Given 用户正在录制脚本
    And 已经录制了3个操作
    When 用户点击"暂停录制"按钮
    Then 系统应该暂停录制
    And 暂停期间的操作不应该被记录
    When 用户点击"继续录制"按钮
    Then 系统应该继续录制
    And 继续录制的操作应该被正确记录

  @Recording
  @Validation
  Scenario: 停止录制并生成脚本
    Given 用户正在录制脚本
    And 已经录制了多个操作
    When 用户点击"停止录制"按钮
    Then 系统应该停止录制
    And 应该生成包含所有操作的脚本
    And 脚本应该有正确的开始和结束时间
    And 脚本状态应该是"草稿"

  @Recording
  @Performance
  Scenario: 录制性能要求
    Given 用户正在录制脚本
    When 用户快速连续按下10个键
    Then 系统应该记录所有按键事件
    And 录制过程中CPU占用率应该小于20%
    And 内存使用增长应该在合理范围内

  @Recording
  @ErrorHandling
  Scenario: 录制过程中的错误处理
    Given 用户正在录制脚本
    When 系统遇到录制错误
    Then 系统应该记录错误日志
    And 应该向用户显示错误提示
    And 已录制的部分不应该丢失
    And 用户可以选择保存或丢弃录制的部分
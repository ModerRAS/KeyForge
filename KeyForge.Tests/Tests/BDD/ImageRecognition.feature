@KeyForge
@ImageRecognition
Feature: 图像识别功能
  作为KeyForge用户
  我想要使用图像识别功能
  以便基于视觉元素做出决策

  @TemplateCreation
  @Basic
  Scenario: 创建图像模板
    Given 用户想要创建图像模板
    And 用户选择了屏幕区域(100,100,200,200)
    And 用户设置了匹配阈值为0.8
    And 用户输入了模板名称"登录按钮"
    When 用户点击"保存模板"按钮
    Then 系统应该保存模板信息
    And 模板应该包含图像数据和匹配参数
    And 模板应该被标记为活跃状态

  @TemplateMatching
  @Basic
  Scenario: 模板匹配识别成功
    Given 用户有一个名为"登录按钮"的图像模板
    And 匹配阈值设置为0.8
    And 屏幕上存在与模板匹配的区域
    When 系统执行图像识别
    Then 系统应该返回匹配成功结果
    And 匹配度应该大于等于0.8
    And 应该返回匹配位置的坐标
    And 识别响应时间应该小于100ms

  @TemplateMatching
  @Failure
  Scenario: 模板匹配识别失败
    Given 用户有一个图像模板
    And 屏幕上没有匹配的区域
    When 系统执行图像识别
    Then 系统应该返回匹配失败结果
    And 匹配度应该小于阈值
    And 应该提供失败原因说明
    And 不应该返回任何坐标

  @MultiTarget
  @Basic
  Scenario: 多目标识别
    Given 用户有一个图像模板
    And 屏幕上有3个相似的区域
    And 系统配置为查找所有匹配
    When 系统执行识别
    Then 系统应该返回所有匹配结果
    And 结果应该按匹配度从高到低排序
    And 每个结果都应该包含坐标和匹配度

  @RecognitionAccuracy
  @Performance
  Scenario: 识别准确率测试
    Given 用户有100个测试图像
    And 包含50个匹配和50个不匹配的情况
    When 系统批量执行识别
    Then 识别准确率应该大于等于95%
    And 误报率应该小于5%
    And 漏报率应该小于5%

  @Performance
  @Speed
  Scenario: 识别性能要求
    Given 系统需要识别多个模板
    When 并发执行10个模板识别
    Then 每个识别的响应时间都应该小于100ms
    And 系统CPU占用率应该小于50%
    And 内存使用应该保持稳定

  @Robustness
  @Lighting
  Scenario: 光照变化适应性
    Given 用户有一个图像模板
    And 屏幕亮度发生了变化
    When 系统执行识别
    Then 系统应该仍然能够识别目标
    And 匹配度下降不应该超过20%
    And 识别时间不应该显著增加

  @Robustness
  @Occlusion
  Scenario: 部分遮挡处理
    Given 用户有一个图像模板
    And 目标区域被部分遮挡
    When 系统执行识别
    Then 系统应该能够识别剩余部分
    And 应该降低匹配度要求
    And 应该报告遮挡情况

  @Robustness
  @Resolution
  Scenario: 分辨率变化适应性
    Given 用户有一个图像模板
    And 屏幕分辨率发生了变化
    When 系统执行识别
    Then 系统应该能够适应新分辨率
    And 应该使用缩放算法
    And 识别准确率不应该显著下降

  @Advanced
  @OCR
  Scenario: OCR文字识别
    Given 用户想要识别屏幕上的文字
    And 用户选择了包含文字的区域
    When 系统执行OCR识别
    Then 系统应该返回识别的文字内容
    And 应该提供文字的置信度评分
    And 应该返回文字的位置信息

  @Advanced
  @Color
  Scenario: 颜色识别功能
    Given 用户想要识别特定颜色
    And 用户设置了颜色范围
    When 系统执行颜色识别
    Then 系统应该返回匹配的颜色区域
    And 应该提供颜色匹配度
    And 应该支持多种颜色空间

  @Preprocessing
  @Basic
  Scenario: 图像预处理功能
    Given 用户有一个低对比度的图像模板
    When 系统在识别前应用预处理
    Then 系统应该应用灰度化处理
    And 应该应用对比度增强
    And 应该应用噪声过滤
    And 预处理后的识别率应该提高

  @Caching
  @Performance
  Scenario: 识别结果缓存
    Given 用户频繁识别相同的模板
    When 系统启用结果缓存
    Then 系统应该缓存识别结果
    And 缓存命中时应该立即返回结果
    And 缓存过期时间应该可配置
    And 内存使用应该在合理范围内
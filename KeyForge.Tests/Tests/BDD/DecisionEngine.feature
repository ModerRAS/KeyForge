@KeyForge
@DecisionEngine
Feature: 决策引擎功能
  作为KeyForge用户
  我想要创建智能决策逻辑
  以便根据不同条件执行不同操作

  @ConditionEvaluation
  @Basic
  Scenario: 基本条件判断
    Given 用户创建了一个决策规则
    And 规则名称为"检查登录状态"
    And 条件表达式为"imageFound == true"
    And 动作为"点击登录按钮"
    When 系统评估条件且imageFound为true
    Then 系统应该执行相应的动作
    And 评估时间应该小于10ms
    And 应该记录决策结果

  @ConditionEvaluation
  @Complex
  Scenario: 复杂逻辑表达式
    Given 用户创建了一个决策规则
    And 条件表达式为"(imageFound == true) && (confidence > 0.8) && (x > 100)"
    When 系统评估条件
    Then 系统应该正确解析复杂表达式
    And 应该按运算符优先级评估
    And 短路求值应该正确工作

  @ConditionEvaluation
  @Operators
  Scenario: 支持各种比较运算符
    Given 用户创建了一个决策规则
    And 条件表达式为"score >= 85"
    When 系统评估条件且score为90
    Then 条件应该评估为true
    When 系统评估条件且score为80
    Then 条件应该评估为false
    And 所有比较运算符(==, !=, >, <, >=, <=)都应该正常工作

  @ConditionEvaluation
  @Logical
  Scenario: 逻辑运算符支持
    Given 用户创建了一个决策规则
    And 条件表达式为"(condition1 || condition2) && condition3"
    When 系统评估条件
    Then 系统应该正确处理OR运算符
    And 应该正确处理AND运算符
    And 应该正确处理NOT运算符
    And 运算符优先级应该正确

  @Variables
  @Basic
  Scenario: 变量定义和使用
    Given 用户在决策规则中定义了变量
    And 变量名为"loginStatus"且值为"false"
    And 条件表达式为"loginStatus == false"
    When 系统评估条件
    Then 系统应该使用变量值进行评估
    And 变量作用域应该正确管理
    And 变量类型应该正确推断

  @Variables
  @Scope
  Scenario: 变量作用域管理
    Given 用户有嵌套的决策逻辑
    And 在内层作用域定义了变量"temp"
    When 系统评估条件
    Then 内层变量不应该影响外层
    And 变量生命周期应该正确管理
    And 内存使用应该没有泄漏

  @Rules
  @Priority
  Scenario: 规则优先级处理
    Given 用户有3个决策规则
    And 规则优先级分别为1、2、3
    And 所有规则条件都满足
    When 系统执行决策
    Then 系统应该执行优先级为1的规则
    And 其他规则不应该被执行
    And 应该记录选择的原因

  @Rules
  @Conflict
  Scenario: 规则冲突处理
    Given 用户有多个规则条件相同
    And 规则有不同的优先级
    When 系统执行决策
    Then 系统应该选择优先级最高的规则
    And 应该报告冲突情况
    And 用户应该收到警告

  @StateMachine
  @Basic
  Scenario: 状态转换
    Given 用户有一个状态机
    And 当前状态为"等待登录"
    And 存在到"已登录"的转换
    And 转换条件为"loginSuccess == true"
    When 系统评估条件且loginSuccess为true
    Then 系统应该转换到"已登录"状态
    And 应该触发状态转换事件
    And 应该更新状态历史记录

  @StateMachine
  @Complex
  Scenario: 复杂状态机逻辑
    Given 用户有一个包含5个状态的状态机
    And 状态之间有多个转换路径
    And 转换条件互斥
    When 系统在"初始状态"执行
    Then 系统应该根据条件选择正确的转换
    And 状态转换应该是确定性的
    And 不应该出现状态混乱

  @DecisionLogic
  @IfElse
  Scenario: If-Else条件分支
    Given 用户创建了If-Else决策逻辑
    And 条件为"score > 80"
    And If分支动作为"执行A级操作"
    And Else分支动作为"执行B级操作"
    When 系统评估条件且score为90
    Then 系统应该执行A级操作
    When 系统评估条件且score为70
    Then 系统应该执行B级操作

  @DecisionLogic
  @Switch
  Scenario: Switch-Case多分支
    Given 用户创建了Switch-Case决策逻辑
    And 条件变量为"userType"
    And Case值为"Admin"、"User"、"Guest"
    When 系统评估条件且userType为"Admin"
    Then 系统应该执行Admin分支
    And 其他分支不应该被执行

  @DecisionLogic
  @Loops
  Scenario: 循环结构支持
    Given 用户创建了While循环
    And 条件为"count < 10"
    And 循环体为"执行操作并增加count"
    When 系统执行循环
    Then 系统应该执行循环体10次
    And 循环变量应该正确更新
    And 应该防止无限循环

  @Debugging
  @Breakpoints
  Scenario: 逻辑断点调试
    Given 用户在决策逻辑中设置了断点
    当 系统执行到断点位置
    Then 系统应该暂停执行
    And 应该显示当前变量状态
    And 用户应该能够单步执行

  @Performance
  @Evaluation
  Scenario: 决策评估性能
    Given 用户有100个决策规则
    当 系统批量评估所有规则
    Then 评估时间应该小于100ms
    And 内存使用应该保持稳定
    And CPU占用率应该合理

  @ErrorHandling
  @InvalidExpression
  Scenario: 无效表达式处理
    Given 用户创建了语法错误的条件表达式
    当 系统尝试评估表达式
    Then 系统应该检测到语法错误
    And 应该提供错误位置信息
    And 应该建议修正方案
    And 系统应该保持稳定

  @ErrorHandling
  @Runtime
  Scenario: 运行时错误处理
    Given 用户使用了未定义的变量
    当 系统评估条件
    Then 系统应该捕获运行时错误
    And 应该提供友好的错误信息
    And 应该记录错误堆栈
    And 不应该影响其他规则的执行